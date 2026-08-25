using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Comfort.Common;
using EFT;
using EFT.AssetsManager;
using EFT.Game.Spawning;
using SPT.Common.Http;
using Newtonsoft.Json;
using TRLDynamicSpawn.Models;
using TRLDynamicSpawn.Helpers;

namespace TRLDynamicSpawn.Components
{
    public class BotDespawnManager : MonoBehaviour
    {
        private static BotDespawnManager _instance;
        private Coroutine _despawnRoutine;
        private TRLConfig _serverConfig;
        
        private string _currentLocation;
        private static readonly Dictionary<string, float> _teleportCooldowns = new Dictionary<string, float>();

        // Histórico de pontos de teleporte e alternância de BotZone
        private static Queue<Vector3> _lastTeleportPositions = new Queue<Vector3>();
        private static int _maxHistoryTeleportPoint = 6;
        private static BotZone _lastTeleportZone;

        // Cache estático de reflexão para CR-01-01 (BotsGroup.Members)
        private static PropertyInfo _botsGroupMembersProp;
        private static FieldInfo _botsGroupMembersField;
        private static bool _botsGroupMembersReflectionDone = false;

        public static void Enable()
        {
            if (_instance != null) return;
            var go = new GameObject("TRL_BotDespawnManager");
            _instance = go.AddComponent<BotDespawnManager>();
            DontDestroyOnLoad(go);
        }

        private void Start()
        {
            // ref: AUD-01-02 — the loop is no longer born with the process; it is born with the raid
            // (RaidLifecycle.OnRaidStart → StartLoop) and stopped by the raid-end hooks (StopLoop).
        }

        /// <summary>Idempotent: one DespawnLoop per raid, never two. ref: AUD-01-02</summary>
        public static void StartLoop()
        {
            if (_instance == null) return;
            if (_instance._despawnRoutine != null) return;
            _teleportCooldowns.Clear();                 // NR-4: previously done in Start() once per process
            _instance._currentLocation = null;
            _instance._despawnRoutine = _instance.StartCoroutine(_instance.DespawnLoop());
        }

        /// <summary>Stops the raid loop. No-op when not running. ref: AUD-01-02</summary>
        public static void StopLoop()
        {
            if (_instance == null || _instance._despawnRoutine == null) return;
            _instance.StopCoroutine(_instance._despawnRoutine);
            _instance._despawnRoutine = null;
        }

        private IEnumerator DespawnLoop()
        {
            while (true)
            {
                // Safety net: if the raid ended without the hook (should not happen), exit without touching HTTP.
                if (!Singleton<GameWorld>.Instantiated)
                {
                    _despawnRoutine = null;
                    yield break;
                }

                _serverConfig = ServerConfigProvider.Config;   // raid-scoped cache hit (no HTTP) — ref: AUD-01-01

                // Config can be null if not loaded yet
                if (_serverConfig == null || !TRLDynamicSpawn.Helpers.Settings.masterDespawnToggle.Value)
                {
                    yield return new WaitForSeconds(5f);
                    continue;
                }

                // Resolve a localização atual do jogador no início do ciclo
                string loc = null;
                if (Singleton<GameWorld>.Instantiated && Singleton<GameWorld>.Instance?.MainPlayer != null)
                {
                    loc = Singleton<GameWorld>.Instance.MainPlayer.Location?.ToLower();
                }

                if (!string.IsNullOrEmpty(loc) && _currentLocation != loc)
                {
                    _currentLocation = loc;
                    _teleportCooldowns.Clear();
                }

                // Obter intervalo do mapa (Check Interval em segundos) configurado no servidor
                float interval = 30f;
                var currentMapSettings = MapNameHelper.GetMapSettings(_serverConfig, _currentLocation);
                if (currentMapSettings != null && currentMapSettings.EnableDespawn && currentMapSettings.DespawnInterval > 0)
                {
                    interval = currentMapSettings.DespawnInterval;
                }

                if (interval < 5f) interval = 5f;

                yield return new WaitForSeconds(interval);

                try
                {
                    // Ensure GameWorld exists
                    if (!Singleton<GameWorld>.Instantiated)
                        continue;

                    var gameWorld = Singleton<GameWorld>.Instance;
                    if (gameWorld == null)
                        continue;

                    // Ensure BotsController exists
                    if (!Singleton<IBotGame>.Instantiated)
                        continue;

                    var botGame = Singleton<IBotGame>.Instance;
                    var botsController = botGame?.BotsController;
                    if (botsController == null)
                        continue;

                    var mapSettings = MapNameHelper.GetMapSettings(_serverConfig, _currentLocation);
                    if (mapSettings == null || !mapSettings.EnableDespawn)
                    {
                        continue;
                    }

                    // For Fika compatibility, we only run this on the Host or Solo
                    if (!FikaHelper.IsHostOrSolo())
                        continue;


                    // Se a otimização de bolha de spawn estiver desativada no cliente (F12), não fazemos o teletransporte/despawn
                    if (!TRLDynamicSpawn.Helpers.Settings.enableSpawnBubble.Value)
                        continue;

                    // Get all alive human players to check distance against
                    var alivePlayers = new List<Player>();
                    for (int i = 0; i < gameWorld.AllAlivePlayersList.Count; i++)
                    {
                        var p = gameWorld.AllAlivePlayersList[i];
                        if (p != null && (p.IsYourPlayer || !p.IsAI))
                        {
                            if (DynamicSpawnManager.IsHeadlessPlayer(p)) continue;
                            alivePlayers.Add(p);
                        }
                    }

                    if (alivePlayers.Count == 0 && gameWorld.MainPlayer != null && !DynamicSpawnManager.IsHeadlessPlayer(gameWorld.MainPlayer))
                    {
                        alivePlayers.Add(gameWorld.MainPlayer);
                    }

                    if (alivePlayers.Count == 0)
                        continue;

                    // Distância da Bolha de Spawn para teletransporte:
                    // Bots com distância < despawnDist (dentro da bolha/zona segura) NUNCA são teleportados.
                    // Apenas bots com distância >= despawnDist (fora da bolha) são teleportados.
                    float despawnDist = (float)mapSettings.SpawnBubbleDistance;
                    if (despawnDist <= 0) despawnDist = 300f;
                    bool despawnPmcs = mapSettings.DespawnPMCs;

                    if (TRLDynamicSpawn.Helpers.Settings.enableDebugLogs.Value)
                    {
                        Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Teleport Scan for '{_currentLocation}': Interval={interval}s, Bubble Radius={despawnDist}m, PMC Despawn={despawnPmcs}.");
                    }

                    // Iterate backwards or use array to avoid collection modified
                    var allBots = botsController.Bots.BotOwners.ToArray();
                    var processedInLoop = new HashSet<string>();

                    foreach (var bot in allBots)
                    {
                        if (bot == null || bot.GetPlayer == null || bot.HealthController == null || !bot.HealthController.IsAlive)
                            continue;

                        string botId = DynamicSpawnManager.SanitizeMongoId(bot.Profile.Id);
                        if (processedInLoop.Contains(botId))
                            continue;

                        // Don't despawn special bots (bosses, followers, snipers, etc)
                        if (IsSpecialBot(bot))
                            continue;

                        // If it's a PMC and PMC despawn is disabled, skip
                        bool isPmc = bot.Profile.Side == EPlayerSide.Bear || bot.Profile.Side == EPlayerSide.Usec;
                        if (isPmc && !despawnPmcs)
                            continue;

                        // Check distance and Line of Sight against ALL human players
                        bool canDespawn = true;
                        foreach (var human in alivePlayers)
                        {
                            if (human == null) continue;
                            float dist = Vector3.Distance(bot.Position, human.Position);
                            
                            // Se o bot estiver dentro da bolha (< despawnDist), ele NUNCA é teleportado
                            if (dist < despawnDist)
                            {
                                canDespawn = false;
                                break;
                            }

                            if (IsBotVisibleToPlayer(human, bot))
                            {
                                canDespawn = false;
                                break;
                            }
                        }

                        if (canDespawn)
                        {
                            var teleportedGroup = AttemptToTeleportGroup(bot);
                            foreach (var member in teleportedGroup)
                            {
                                if (member != null && member.Profile != null)
                                {
                                    processedInLoop.Add(DynamicSpawnManager.SanitizeMongoId(member.Profile.Id));
                                }
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Plugin.LogSource.LogError($"[TRL-DynamicSpawn] Error in DespawnLoop: {ex.Message}");
                }
            }
        }

        private bool IsBotVisibleToPlayer(Player player, BotOwner bot)
        {
            if (player == null || bot == null || bot.GetPlayer == null) return false;
            
            Vector3 botPos = bot.Position;
            Vector3 playerPos = player.Position;
            
            if (!TRLDynamicSpawn.Helpers.Settings.enableLoSCulling.Value)
                return false;

            Vector3 directionToBot = (botPos - playerPos).normalized;
            float dot = Vector3.Dot(player.LookDirection, directionToBot);
            
            if (dot > 0.5f)
            {
                Vector3 headPos = player.MainParts.ContainsKey(BodyPartType.head) ? player.MainParts[BodyPartType.head].Position : playerPos + Vector3.up * 1.5f;
                Vector3 botTargetPos = bot.MainParts.ContainsKey(BodyPartType.head) ? bot.MainParts[BodyPartType.head].Position : botPos + Vector3.up * 1f;
                
                if (!Physics.Linecast(headPos, botTargetPos, LayerMaskClass.HighPolyWithTerrainMask))
                {
                    return true;
                }
            }
            
            return false;
        }

        private bool IsHostOrSolo()
        {
            try
            {
                // Verifica dinamicamente usando reflexão o estado da Fika sem forçar dependência dura de carregamento
                var type = System.Type.GetType("Fika.Core.Main.Utils.FikaBackendUtils, Fika.Core");
                if (type != null)
                {
                    var prop = type.GetProperty("IsServer", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (prop != null)
                    {
                        return (bool)prop.GetValue(null);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Plugin.LogSource.LogWarning($"[TRL-DynamicSpawn] Failed to query Fika status: {ex.Message}");
            }
            return true; // Fallback para Solo clássico
        }

        private bool IsSpecialBot(BotOwner bot)
        {
            var role = bot.Profile.Info.Settings.Role;
            // Only allow normal Scavs (Assault) and PMCs (Usec/Bear) to despawn.
            // pmcBot = 39, exUsec = 40 (Rogue), arenaFighter = 41, etc. Just check enums.
            if (role == WildSpawnType.assault || role == WildSpawnType.cursedAssault || role == WildSpawnType.pmcBot || role == (WildSpawnType)41 || role == (WildSpawnType)42 || role == WildSpawnType.pmcUSEC || role == WildSpawnType.pmcBEAR)
            {
                return false;
            }
            return true;
        }

        private void InvokeOnPlayerDead(Player player)
        {
            if (player == null) return;
            try
            {
                var field = HarmonyLib.AccessTools.Field(typeof(Player), "OnPlayerDead");
                if (field != null)
                {
                    var evt = field.GetValue(player) as System.MulticastDelegate;
                    if (evt != null)
                    {
                        foreach (var handler in evt.GetInvocationList())
                        {
                            handler.Method.Invoke(handler.Target, new object[] { player, player, default(DamageInfoStruct), EBodyPart.Chest });
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Plugin.LogSource.LogWarning($"[TRL-DynamicSpawn] Failed to invoke OnPlayerDead via reflection: {ex.Message}");
            }
        }

        private IEnumerator AttemptToDespawnBotCoroutine(BotsController botsController, BotOwner botToDespawn)
        {
            // Wait until the end of the frame to avoid breaking other mods (like Orbit/Fika) that might be iterating over live agents in Update()
            yield return new WaitForEndOfFrame();

            try
            {
                var gameWorld = Singleton<GameWorld>.Instance;
                if (gameWorld == null) yield break;

                var botPlayer = botToDespawn.GetPlayer;

                // 1. Notify EFT (and SAIN) that the bot has died. SAIN intercepts this and runs its own cleanup (StopAllCoroutines, etc).
                botsController.BotDied(botToDespawn);

                // 1.5 Notify ALL external mods (ORBIT, SAIN, FIKA) that this player is effectively "dead" so they remove it from their live tracking lists.
                InvokeOnPlayerDead(botPlayer);

                // 2. Now manually force-destroy the SAIN component if it still exists.
                // Since SAIN calls Object.Destroy(this) which is delayed, returning to the pool immediately would cancel it or cause it to linger.
                if (botToDespawn.gameObject != null)
                {
                    var components = botToDespawn.gameObject.GetComponents<MonoBehaviour>();
                    foreach (var comp in components)
                    {
                        if (comp != null && comp.GetType().Name == "BotComponent" && comp.GetType().Namespace == "SAIN.Components")
                        {
                            bool logEnabled = UnityEngine.Debug.unityLogger.logEnabled;
                            try
                            {
                                UnityEngine.Debug.unityLogger.logEnabled = false;
                                UnityEngine.Object.DestroyImmediate(comp);
                            }
                            catch { }
                            finally
                            {
                                UnityEngine.Debug.unityLogger.logEnabled = logEnabled;
                            }
                        }
                    }
                }

                // 3. Dispose of the EFT BotOwner and Player correctly.
                botToDespawn.Dispose();
                
                try 
                {
                    botPlayer.Dispose();
                }
                catch { }



                botsController.DestroyInfo(botPlayer);

                // 4. Return to pool safely.
                AssetPoolObject.ReturnToPool(botToDespawn.gameObject, true);

                Plugin.LogSource.LogInfo($"[TRL] Despawned bot {botPlayer.Profile.Nickname} ({botPlayer.Profile.Info.Settings.Role}) due to distance.");

                if (TRLDynamicSpawn.Helpers.Settings.replaceDespawnedBots.Value && DynamicSpawnManager.Instance != null)
                {
                    DynamicSpawnManager.Instance.RequestReplacementBot(botPlayer.Profile.Side, botPlayer.Profile.Info.Settings.Role, botToDespawn.Profile.Info.Settings.BotDifficulty);
                }
            }
            catch (System.Exception ex)
            {
                Plugin.LogSource.LogError($"[TRL] Error attempting to despawn bot: {ex.Message}");
            }
        }

        private static void WipeMemoryResidue(BotMemoryClass memory)
        {
            if (memory == null) return;
            try
            {
                var field = typeof(BotMemoryClass).GetField("LastEnemy", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                           ?? typeof(BotMemoryClass).GetField("_lastEnemy", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                field?.SetValue(memory, null);
            }
            catch { }
        }

        private static void ForceBackToPatrol(BotOwner bot)
        {
            if (bot.PatrollingData == null) return;
            try
            {
                bot.PatrollingData.Unpause();
                var comeToPatrolMethod = bot.PatrollingData.GetType().GetMethod("ComeToPatrol", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (comeToPatrolMethod != null)
                {
                    var parameters = comeToPatrolMethod.GetParameters();
                    if (parameters.Length == 2) comeToPatrolMethod.Invoke(bot.PatrollingData, new object[] { true, true });
                    else if (parameters.Length == 1) comeToPatrolMethod.Invoke(bot.PatrollingData, new object[] { true });
                    else comeToPatrolMethod.Invoke(bot.PatrollingData, null);
                }
            }
            catch { }
        }

        private ISpawnPoint GetValidTeleportPoint(BotOwner bot, string mapName, double minTeleportDist, double maxTeleportDist, out BotZone targetZone)
        {
            targetZone = null;
            var allZones = LocationScene.GetAllObjects<BotZone>();
            if (allZones == null || allZones.Count() == 0) return null;

            var gameWorld = Singleton<GameWorld>.Instance;
            var playersList = gameWorld.AllAlivePlayersList;
            var players = new List<Player>();

            if (playersList != null)
            {
                foreach (var p in playersList)
                {
                    if (p == null || p.Profile == null) continue;
                    if (p.IsAI && !p.IsYourPlayer) continue;
                    if (DynamicSpawnManager.IsHeadlessPlayer(p)) continue;
                    players.Add(p);
                }
            }

            bool enableLos = TRLDynamicSpawn.Helpers.Settings.enableLoSCulling.Value;
            float losDist = TRLDynamicSpawn.Helpers.Settings.losCullingDistance.Value;
            float heightLimit = (mapName == "factory4_day" || mapName == "factory4_night" || mapName == "sandbox" || mapName == "sandbox_high") ? 5.0f : 15.0f;

            List<ISpawnPoint> strictPoints = new List<ISpawnPoint>();
            List<ISpawnPoint> fallbackStrictPoints = new List<ISpawnPoint>();
            List<ISpawnPoint> noBubblePoints = new List<ISpawnPoint>();
            List<ISpawnPoint> fallbackNoBubblePoints = new List<ISpawnPoint>();

            Dictionary<ISpawnPoint, BotZone> pointToZoneMap = new Dictionary<ISpawnPoint, BotZone>();

            WildSpawnType botRole = bot.Profile?.Info?.Settings?.Role ?? WildSpawnType.assault;
            bool isSniperBot = SpawnPointHelper.IsSniperRole(botRole);

            // Prioriza BotZones diferentes da última zona teleportada (_lastTeleportZone)
            var availableZones = allZones.Where(z => z != null && z.SpawnPoints != null && z.SpawnPoints.Length > 0 && (isSniperBot ? SpawnPointHelper.IsSniperZone(z) : !SpawnPointHelper.IsSniperZone(z))).ToList();
            var nonRecentZones = availableZones.Where(z => z != _lastTeleportZone).ToList();
            if (nonRecentZones.Count > 0)
            {
                availableZones = nonRecentZones;
            }

            foreach (var z in availableZones)
            {
                foreach (var sp in z.SpawnPoints)
                {
                    if (sp == null) continue;

                    bool isSniperPoint = SpawnPointHelper.IsSniperSpawnPoint(sp, z);
                    if (!isSniperBot && isSniperPoint) continue;
                    if (isSniperBot && !isSniperPoint) continue;

                    bool tooCloseToRecent = false;
                    foreach (var recentPos in _lastTeleportPositions)
                    {
                        if (Vector3.Distance(sp.Position, recentPos) < 50f)
                        {
                            tooCloseToRecent = true;
                            break;
                        }
                    }

                    bool insideBubble = false;
                    bool outsideSafe = true;
                    bool hasLoS = false;

                    if (players.Count > 0)
                    {
                        // 1. Check Bubble (Max dist)
                        foreach (var p in players)
                        {
                            if (Vector3.Distance(p.Position, sp.Position) <= maxTeleportDist)
                            {
                                insideBubble = true;
                                break;
                            }
                        }

                        // 2. Check Safe dist (Min dist) - INVIOLÁVEL (100m)
                        foreach (var p in players)
                        {
                            float dx = p.Position.x - sp.Position.x;
                            float dz = p.Position.z - sp.Position.z;
                            float dh = (float)System.Math.Sqrt(dx * dx + dz * dz);
                            float dv = System.Math.Abs(p.Position.y - sp.Position.y);
                            float limitW = System.Math.Max((float)minTeleportDist, 5f);
                            float limitH = System.Math.Max(heightLimit, 3f);

                            if ((dh / limitW) + (dv / limitH) <= 1.0f)
                            {
                                outsideSafe = false;
                                break;
                            }
                        }

                        // OBRIGATÓRIO: Se o ponto estiver dentro da SafeZone (100m), descarta sumariamente!
                        if (!outsideSafe) continue;

                        // 3. Check LoS - INVIOLÁVEL
                        if (enableLos)
                        {
                            foreach (var p in players)
                            {
                                float d = Vector3.Distance(p.Position, sp.Position);
                                if (d <= losDist)
                                {
                                    bool isVis = false;
                                    if (p.IsYourPlayer && Camera.main != null)
                                    {
                                        Vector3 screenPoint = Camera.main.WorldToViewportPoint(sp.Position + Vector3.up * 1f);
                                        if (screenPoint.z > 0 && screenPoint.x >= 0 && screenPoint.x <= 1 && screenPoint.y >= 0 && screenPoint.y <= 1) isVis = true;
                                    }
                                    else
                                    {
                                        Vector3 dir = (sp.Position - p.Position).normalized;
                                        if (Vector3.Dot(p.LookDirection, dir) > 0.5f) isVis = true;
                                    }

                                    if (isVis)
                                    {
                                        Vector3 headPos = p.MainParts.ContainsKey(BodyPartType.head) ? p.MainParts[BodyPartType.head].Position : p.Position + Vector3.up * 1.5f;
                                        if (!Physics.Linecast(headPos, sp.Position + Vector3.up * 1f, LayerMaskClass.HighPolyWithTerrainMask))
                                        {
                                            hasLoS = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        // OBRIGATÓRIO: Se o ponto tiver visão direta (LoS), descarta sumariamente!
                        if (hasLoS) continue;
                    }

                    // Categorizar pontos que passaram 100% nas travas da SafeZone e LoS
                    if (insideBubble)
                    {
                        if (tooCloseToRecent) fallbackStrictPoints.Add(sp);
                        else strictPoints.Add(sp);
                    }
                    else
                    {
                        if (tooCloseToRecent) fallbackNoBubblePoints.Add(sp);
                        else noBubblePoints.Add(sp);
                    }

                    pointToZoneMap[sp] = z;
                }
            }

            // SELEÇÃO EM CASCATA COM MASTER FALLBACK EM ÚLTIMA INSTÂNCIA:
            List<ISpawnPoint> chosenList = null;
            if (strictPoints.Count > 0) chosenList = strictPoints;
            else if (fallbackStrictPoints.Count > 0) chosenList = fallbackStrictPoints;
            // MASTER FALLBACK: Somente acionado se NENHUM ponto na bolha funcionar!
            else if (noBubblePoints.Count > 0)
            {
                Plugin.LogSource.LogWarning("[TRL-DynamicSpawn] MASTER FALLBACK LEVEL 1 (Teleport): Selecting point outside bubble.");
                chosenList = noBubblePoints;
            }
            else if (fallbackNoBubblePoints.Count > 0)
            {
                Plugin.LogSource.LogWarning("[TRL-DynamicSpawn] MASTER FALLBACK LEVEL 2 (Teleport): Selecting point outside bubble with history.");
                chosenList = fallbackNoBubblePoints;
            }

            if (chosenList != null && chosenList.Count > 0)
            {
                // Bias Direcional 100% Frontal
                ISpawnPoint selectedPoint = null;
                if (players.Count > 0 && players[0] != null)
                {
                    var mainPlayer = players[0];
                    Vector3 playerMoveDir = (mainPlayer.MovementContext != null && mainPlayer.MovementContext.Velocity.sqrMagnitude > 0.1f)
                                           ? mainPlayer.MovementContext.Velocity.normalized
                                           : mainPlayer.LookDirection;

                    var forwardPoints = chosenList.Where(sp =>
                    {
                        Vector3 dirToPoint = (sp.Position - mainPlayer.Position).normalized;
                        return Vector3.Dot(playerMoveDir, dirToPoint) > 0f;
                    }).ToList();

                    if (forwardPoints.Count > 0)
                    {
                        selectedPoint = forwardPoints[UnityEngine.Random.Range(0, forwardPoints.Count)];
                    }
                }

                if (selectedPoint == null)
                {
                    selectedPoint = chosenList[UnityEngine.Random.Range(0, chosenList.Count)];
                }

                targetZone = pointToZoneMap[selectedPoint];
                _lastTeleportZone = targetZone;

                _lastTeleportPositions.Enqueue(selectedPoint.Position);
                if (_lastTeleportPositions.Count > _maxHistoryTeleportPoint)
                {
                    _lastTeleportPositions.Dequeue();
                }

                return selectedPoint;
            }

            return null;
        }

        private static void SafeResetBotForTeleport(BotOwner bot)
        {
            if (bot == null || bot.GetPlayer == null || bot.HealthController == null || !bot.HealthController.IsAlive)
                return;

            // 1. Limpeza cirúrgica do alvo ativo de combate (GoalEnemy e tiro/mira)
            try
            {
                if (bot.Memory != null)
                {
                    bot.Memory.GoalEnemy = null;
                    WipeMemoryResidue(bot.Memory);
                    bot.Memory.LastTimeHit = -1000f;
                }
                bot.ShootData?.EndShoot();
                if (bot.AimingManager?.CurrentAiming != null)
                {
                    bot.AimingManager.CurrentAiming.LoseTarget();
                }
            }
            catch { }

            // 2. Parar navegação física antiga no NavMesh
            try
            {
                bot.Mover?.Stop();
            }
            catch { }

            // 3. Forçar a reconvocação de patrulha nativa na nova BotZone
            try
            {
                ForceBackToPatrol(bot);
            }
            catch { }
        }

        private List<BotOwner> GetGroupMembers(BotOwner bot)
        {
            var members = new List<BotOwner>();
            if (bot == null || bot.GetPlayer == null || bot.HealthController == null || !bot.HealthController.IsAlive)
                return members;

            members.Add(bot);

            // 1. Tentar obter membros através do BotsGroup (com cache estático de reflexão CR-01-01)
            if (bot.BotsGroup != null)
            {
                try
                {
                    if (!_botsGroupMembersReflectionDone)
                    {
                        var bgType = bot.BotsGroup.GetType();
                        _botsGroupMembersProp = bgType.GetProperty("Members", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        _botsGroupMembersField = bgType.GetField("Members", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        _botsGroupMembersReflectionDone = true;
                    }

                    System.Collections.IEnumerable groupList = null;
                    if (_botsGroupMembersProp != null) groupList = _botsGroupMembersProp.GetValue(bot.BotsGroup) as System.Collections.IEnumerable;
                    else if (_botsGroupMembersField != null) groupList = _botsGroupMembersField.GetValue(bot.BotsGroup) as System.Collections.IEnumerable;

                    if (groupList != null)
                    {
                        foreach (var item in groupList)
                        {
                            if (item is BotOwner memberOwner && memberOwner != null && memberOwner.GetPlayer != null)
                            {
                                if (memberOwner.HealthController != null && memberOwner.HealthController.IsAlive && !members.Contains(memberOwner))
                                {
                                    members.Add(memberOwner);
                                }
                            }
                        }
                    }
                }
                catch { }
            }

            // 2. Tentar obter seguidores se este bot for um Boss/Líder
            if (bot.Boss != null && bot.Boss.Followers != null)
            {
                foreach (var follower in bot.Boss.Followers)
                {
                    if (follower != null && follower.GetPlayer != null && follower.HealthController != null && follower.HealthController.IsAlive && !members.Contains(follower))
                    {
                        members.Add(follower);
                    }
                }
            }

            // 3. Tentar obter o líder e seus outros seguidores se este bot for um seguidor
            if (bot.BotFollower != null && bot.BotFollower.HaveBoss && bot.BotFollower.BossToFollow != null)
            {
                var bossOwner = bot.BotFollower.BossToFollow as BotOwner;
                if (bossOwner != null && bossOwner.GetPlayer != null && bossOwner.HealthController != null && bossOwner.HealthController.IsAlive && !members.Contains(bossOwner))
                {
                    members.Add(bossOwner);
                    if (bossOwner.Boss != null && bossOwner.Boss.Followers != null)
                    {
                        foreach (var f in bossOwner.Boss.Followers)
                        {
                            if (f != null && f.GetPlayer != null && f.HealthController != null && f.HealthController.IsAlive && !members.Contains(f))
                            {
                                members.Add(f);
                            }
                        }
                    }
                }
            }

            return members;
        }

        private List<BotOwner> AttemptToTeleportGroup(BotOwner bot)
        {
            var teleportedList = new List<BotOwner>();
            if (bot == null || bot.Profile == null || bot.GetPlayer == null) return teleportedList;

            try
            {
                var gameWorld = Singleton<GameWorld>.Instance;
                if (gameWorld == null || DynamicSpawnManager.Instance == null) return teleportedList;

                var groupMembers = GetGroupMembers(bot);
                if (groupMembers.Count == 0) return teleportedList;

                // Checa se algum membro do grupo ainda está em cooldown de 30s
                foreach (var member in groupMembers)
                {
                    string memberId = DynamicSpawnManager.SanitizeMongoId(member.Profile.Id);
                    if (_teleportCooldowns.TryGetValue(memberId, out float lastTime))
                    {
                        if (Time.time - lastTime < 30f)
                        {
                            return teleportedList;
                        }
                    }
                }

                string mapName = gameWorld.MainPlayer?.Location?.ToLower() ?? "";
                var role = bot.Profile.Info.Settings.Role;

                double minTeleportDist = 100.0;
                double maxTeleportDist = 300.0;

                var mapSettings = MapNameHelper.GetMapSettings(DynamicSpawnManager.Instance?.ServerConfig, mapName);
                if (mapSettings != null)
                {
                    minTeleportDist = mapSettings.TeleportMinDistance;
                    maxTeleportDist = mapSettings.SpawnBubbleDistance;
                }

                BotZone selectedZone = null;
                ISpawnPoint spawnPoint = GetValidTeleportPoint(bot, mapName, minTeleportDist, maxTeleportDist, out selectedZone);

                if (spawnPoint == null || selectedZone == null)
                {
                    Plugin.LogSource.LogWarning($"[TRL] Group teleport failed: could not find a valid target spawn point for group {bot.GetPlayer.Profile.Nickname} ({groupMembers.Count} bots) within Bubble({maxTeleportDist}m) / Safe({minTeleportDist}m).");
                    return teleportedList;
                }

                Plugin.LogSource.LogInfo($"[TRL] Teleporting GROUP (Size: {groupMembers.Count}, Role: {role}) from {bot.Position} to {selectedZone.NameZone} ({spawnPoint.Position})...");

                for (int i = 0; i < groupMembers.Count; i++)
                {
                    var m = groupMembers[i];
                    if (m == null || m.GetPlayer == null || m.HealthController == null || !m.HealthController.IsAlive)
                        continue;

                    // Aplica pequenos desvios radiais de 1.5m para os seguidores não nascerem colados/clipando
                    Vector3 offset = (i == 0) ? Vector3.zero : new Vector3(UnityEngine.Random.Range(-1.5f, 1.5f), 0f, UnityEngine.Random.Range(-1.5f, 1.5f));
                    Vector3 targetPos = spawnPoint.Position + offset;

                    // Teleporta fisicamente o bot para a nova posição no NavMesh
                    m.GetPlayer.Teleport(targetPos, true);

                    // Reset de segurança defensivo da mente/combate do bot
                    SafeResetBotForTeleport(m);

                    string mId = DynamicSpawnManager.SanitizeMongoId(m.Profile.Id);
                    _teleportCooldowns[mId] = Time.time;

                    teleportedList.Add(m);
                }
            }
            catch (System.Exception ex)
            {
                Plugin.LogSource.LogError($"[TRL] Error during group teleport: {ex.Message}\n{ex.StackTrace}");
            }
            return teleportedList;
        }
    }
}
