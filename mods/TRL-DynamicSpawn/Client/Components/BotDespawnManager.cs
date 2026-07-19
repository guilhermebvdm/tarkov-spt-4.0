using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Comfort.Common;
using EFT;
using EFT.AssetsManager;
using UnityEngine;
using SPT.Common.Http;
using Newtonsoft.Json;
using TRLDynamicSpawn.Models;

namespace TRLDynamicSpawn.Components
{
    public class BotDespawnManager : MonoBehaviour
    {
        private static BotDespawnManager _instance;
        private Coroutine _despawnRoutine;
        private TRLConfig _serverConfig;
        
        // This keeps track of our current map and if it has despawn enabled.
        private string _currentLocation;

        public static void Enable()
        {
            if (_instance != null) return;
            var go = new GameObject("TRL_BotDespawnManager");
            _instance = go.AddComponent<BotDespawnManager>();
            DontDestroyOnLoad(go);
        }

        private void Start()
        {
            _despawnRoutine = StartCoroutine(DespawnLoop());
        }

        private IEnumerator DespawnLoop()
        {
            // Fetch Config
            string json = RequestHandler.GetJson("/trldynamicspawn/getConfig");
            if (!string.IsNullOrEmpty(json))
            {
                _serverConfig = JsonConvert.DeserializeObject<TRLConfig>(json);
            }

            while (true)
            {
                // Config can be null if not loaded yet
                if (_serverConfig == null || !TRLDynamicSpawn.Helpers.Settings.masterDespawnToggle.Value)
                {
                    yield return new WaitForSeconds(5f);
                    continue;
                }

                float interval = 5f;
                if (!string.IsNullOrEmpty(_currentLocation) && _serverConfig.MapConfigs.TryGetValue(_currentLocation, out var currentMapSettings))
                {
                    if (currentMapSettings.EnableDespawn)
                    {
                        interval = currentMapSettings.DespawnInterval;
                    }
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

                    // Get current location
                    _currentLocation = gameWorld.MainPlayer?.Location;
                    if (string.IsNullOrEmpty(_currentLocation))
                        continue;
                        
                    _currentLocation = _currentLocation.ToLower();
                    
                    if (!_serverConfig.MapConfigs.TryGetValue(_currentLocation, out var mapSettings))
                    {
                        continue;
                    }

                    // Check if despawn is enabled for this map
                    if (!mapSettings.EnableDespawn)
                        continue;

                    // For Fika compatibility, we only run this on the Host or Solo
                    if (!IsHostOrSolo())
                        continue;

                    // Get all alive human players to check distance against
                    var alivePlayers = new List<Player>();
                    for (int i = 0; i < gameWorld.AllAlivePlayersList.Count; i++)
                    {
                        var p = gameWorld.AllAlivePlayersList[i];
                        if (p != null && (p.IsYourPlayer || !p.IsAI))
                        {
                            alivePlayers.Add(p);
                        }
                    }
                    
                    if (alivePlayers.Count == 0 && gameWorld.MainPlayer != null)
                    {
                        alivePlayers.Add(gameWorld.MainPlayer);
                    }
                    
                    if (alivePlayers.Count == 0)
                        continue;

                    // Attempt despawning bots
                    float despawnDist = mapSettings.DespawnDistance;
                    bool despawnPmcs = mapSettings.DespawnPMCs;

                    // Iterate backwards or use array to avoid collection modified
                    var allBots = botsController.Bots.BotOwners.ToArray();

                    foreach (var bot in allBots)
                    {
                        if (bot == null || bot.GetPlayer == null || bot.HealthController == null || !bot.HealthController.IsAlive)
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
                            StartCoroutine(AttemptToDespawnBotCoroutine(botsController, bot));
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
    }
}
