using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using TRLDynamicSpawn.Helpers;

namespace TRLDynamicSpawn.Components
{
    public class TrackedCorpse
    {
        public Player Player { get; set; }
        public float DeathTime { get; set; }
        public float LifetimeSeconds { get; set; }
        public WildSpawnType Role { get; set; }
        public string ProfileId { get; set; }
        public bool IsConverted { get; set; } = false;
        public bool IsDespawned { get; set; } = false;
        public float LastRetryTime { get; set; } = 0f;
        public GameObject SpawnedBackpackVisual { get; set; }
    }

    public class CorpseCleanupManager : MonoBehaviour
    {
        private const string MBSS_BACKPACK_TEMPLATE = "544a5cde4bdc2d39388b456b"; // Flyye MBSS backpack (UCP)

        private static CorpseCleanupManager _instance;
        private Coroutine _corpseRoutine;
        private static readonly Dictionary<string, TrackedCorpse> _trackedCorpses = new Dictionary<string, TrackedCorpse>();
        private static readonly List<Renderer> _tempRenderers = new List<Renderer>(256);
        private static readonly List<Player> _aliveHumansBuffer = new List<Player>(16);
        private static GameObject _cachedMbssPrefab;

        public static void Enable()
        {
            if (_instance != null) return;
            var go = new GameObject("TRL_CorpseCleanupManager");
            _instance = go.AddComponent<CorpseCleanupManager>();
            DontDestroyOnLoad(go);
        }

        public static void StartLoop()
        {
            if (_instance == null) return;
            if (_instance._corpseRoutine != null) return;

            // Apenas o Host ou Solo gerencia a limpeza e os timers de corpos
            if (!FikaHelper.IsHostOrSolo()) return;

            ClearStaticState();
            _instance._corpseRoutine = _instance.StartCoroutine(_instance.CorpseLoop());
            Plugin.LogSource?.LogInfo("[TRL-DynamicSpawn] Corpse Cleanup Loop started on Host.");
        }

        public static void StopLoop()
        {
            if (_instance == null || _instance._corpseRoutine == null) return;
            _instance.StopCoroutine(_instance._corpseRoutine);
            _instance._corpseRoutine = null;
            ClearStaticState();
        }

        public static void ClearStaticState()
        {
            foreach (var kvp in _trackedCorpses)
            {
                if (kvp.Value?.SpawnedBackpackVisual != null)
                {
                    UnityEngine.Object.Destroy(kvp.Value.SpawnedBackpackVisual);
                }
            }
            _trackedCorpses.Clear();
            _tempRenderers.Clear();
            _aliveHumansBuffer.Clear();
            _cachedMbssPrefab = null;
        }

        /// <summary>
        /// Registra a morte de um bot para iniciar a contagem individual de vida útil do cadáver.
        /// </summary>
        public static void RegisterDeadBot(Player deadPlayer)
        {
            if (deadPlayer == null || deadPlayer.IsYourPlayer || !deadPlayer.IsAI) return;
            if (!FikaHelper.IsHostOrSolo()) return;

            try
            {
                string profileId = DynamicSpawnManager.SanitizeMongoId(deadPlayer.Profile.Id);
                if (_trackedCorpses.ContainsKey(profileId)) return;

                float lifetime = Mathf.Max(30f, Settings.corpseLifetimeMinutes.Value * 60f);
                var role = deadPlayer.Profile.Info.Settings.Role;

                var tracked = new TrackedCorpse
                {
                    Player = deadPlayer,
                    DeathTime = Time.time,
                    LifetimeSeconds = lifetime,
                    Role = role,
                    ProfileId = profileId
                };

                _trackedCorpses[profileId] = tracked;

                if (Settings.enableDebugLogs.Value)
                {
                    Plugin.LogSource?.LogInfo($"[TRL-DynamicSpawn] Corpse Cleanup: Registered dead bot '{deadPlayer.Profile.Nickname}' ({role}). Lifetime: {lifetime:F0}s.");
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogWarning($"[TRL-DynamicSpawn] Failed to register dead bot for corpse cleanup: {ex.Message}");
            }
        }

        private IEnumerator CorpseLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(5f);

                if (!Singleton<GameWorld>.Instantiated)
                {
                    _corpseRoutine = null;
                    yield break;
                }

                if (!Settings.enableCorpseCleanup.Value)
                    continue;

                if (!FikaHelper.IsHostOrSolo())
                    continue;

                var gameWorld = Singleton<GameWorld>.Instance;
                if (gameWorld == null) continue;

                // Obter todos os jogadores humanos vivos da raid (Host + Convidados Fika) sem alocação
                _aliveHumansBuffer.Clear();
                for (int i = 0; i < gameWorld.AllAlivePlayersList.Count; i++)
                {
                    var p = gameWorld.AllAlivePlayersList[i];
                    if (p != null && (p.IsYourPlayer || !p.IsAI))
                    {
                        if (DynamicSpawnManager.IsHeadlessPlayer(p)) continue;
                        _aliveHumansBuffer.Add(p);
                    }
                }

                if (_aliveHumansBuffer.Count == 0 && gameWorld.MainPlayer != null && !DynamicSpawnManager.IsHeadlessPlayer(gameWorld.MainPlayer))
                {
                    _aliveHumansBuffer.Add(gameWorld.MainPlayer);
                }

                if (_aliveHumansBuffer.Count == 0) continue;

                float currentTime = Time.time;
                var keys = _trackedCorpses.Keys.ToList();
                string mode = Settings.corpseCleanupMode.Value;
                bool protectBosses = Settings.protectBossCorpses.Value;

                foreach (var key in keys)
                {
                    if (!_trackedCorpses.TryGetValue(key, out var tracked) || tracked == null)
                        continue;

                    if (tracked.IsDespawned)
                    {
                        _trackedCorpses.Remove(key);
                        continue;
                    }

                    if (tracked.Player == null || tracked.Player.gameObject == null)
                    {
                        if (tracked.SpawnedBackpackVisual != null)
                        {
                            UnityEngine.Object.Destroy(tracked.SpawnedBackpackVisual);
                        }
                        _trackedCorpses.Remove(key);
                        continue;
                    }

                    // Se for boss protegido, não aplicamos conversão nem despawn
                    if (protectBosses && IsBossOrSpecial(tracked.Role))
                        continue;

                    // Verifica se o tempo de vida individual já expirou
                    if (currentTime < tracked.DeathTime + tracked.LifetimeSeconds)
                        continue;

                    // Se já foi convertido em mochila e estamos no modo Backpack Convert, nada mais a fazer
                    if (mode == "Backpack Convert" && tracked.IsConverted)
                        continue;

                    // Teste de segurança (Proximidade + LoS sob demanda)
                    if (!IsCorpseSafeToProcess(tracked.Player, _aliveHumansBuffer))
                    {
                        tracked.LastRetryTime = currentTime;
                        continue; // Sinal Vermelho 🔴: adia a ação
                    }

                    // Sinal Verde 🟢: executa a ação conforme o modo selecionado
                    try
                    {
                        if (mode == "Backpack Convert")
                        {
                            ConvertToBackpack(tracked);
                            tracked.IsConverted = true;
                        }
                        else
                        {
                            DestroyCorpse(tracked);
                            tracked.IsDespawned = true;
                            _trackedCorpses.Remove(key);
                        }
                    }
                    catch (Exception ex)
                    {
                        Plugin.LogSource?.LogError($"[TRL-DynamicSpawn] Error processing corpse cleanup for {tracked.ProfileId}: {ex.Message}");
                    }
                }
            }
        }

        private bool IsCorpseSafeToProcess(Player corpse, List<Player> aliveHumans)
        {
            if (corpse == null || corpse.gameObject == null) return false;

            Vector3 corpsePos = corpse.Position;
            float minSafeDist = Settings.corpseMinSafeDistance.Value;
            bool checkLoS = Settings.corpseCheckLoS.Value;

            foreach (var human in aliveHumans)
            {
                if (human == null || human.gameObject == null) continue;

                // 1. Trava de Proximidade Coletiva (Host + Convidados)
                float dist = Vector3.Distance(human.Position, corpsePos);
                if (dist < minSafeDist)
                {
                    return false; // Muito perto
                }

                // 2. Trava de Linha de Visão (LoS sob demanda)
                if (checkLoS)
                {
                    Vector3 directionToCorpse = (corpsePos - human.Position).normalized;
                    float dot = Vector3.Dot(human.LookDirection, directionToCorpse);

                    // Se o cadáver está dentro do cone de visão frontal (~120 graus)
                    if (dot > 0.35f)
                    {
                        Vector3 headPos = human.MainParts.ContainsKey(BodyPartType.head)
                            ? human.MainParts[BodyPartType.head].Position
                            : human.Position + Vector3.up * 1.5f;

                        Vector3 targetPos = corpse.MainParts.ContainsKey(BodyPartType.head)
                            ? corpse.MainParts[BodyPartType.head].Position
                            : corpsePos + Vector3.up * 0.3f;

                        // Se não há paredes ou terreno bloqueando a visão, o jogador está vendo o corpo
                        if (!Physics.Linecast(headPos, targetPos, LayerMaskClass.HighPolyWithTerrainMask))
                        {
                            return false; // Visível
                        }
                    }
                }
            }

            return true;
        }

        private void ConvertToBackpack(TrackedCorpse tracked)
        {
            var corpsePlayer = tracked.Player;
            if (corpsePlayer == null || corpsePlayer.gameObject == null) return;

            // 1. Congelar física do Ragdoll (CPU = 0% de física contínua)
            var rbs = corpsePlayer.GetComponentsInChildren<Rigidbody>();
            foreach (var rb in rbs)
            {
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.isKinematic = true;
                    rb.detectCollisions = false;
                }
            }

            var joints = corpsePlayer.GetComponentsInChildren<Joint>();
            foreach (var j in joints)
            {
                if (j != null) j.enableCollision = false;
            }

            // 2. Ocultar 100% das malhas do corpo humano, roupas e armas (GPU = 0 Draw Calls para o bot morto)
            _tempRenderers.Clear();
            if (corpsePlayer.PlayerBody != null)
            {
                corpsePlayer.PlayerBody.GetRenderersNonAlloc(_tempRenderers);
                foreach (var r in _tempRenderers)
                {
                    if (r != null)
                    {
                        r.forceRenderingOff = true;
                    }
                }
            }

            // Ocultar armas e acessórios anexados
            var allRenderers = corpsePlayer.GetComponentsInChildren<Renderer>();
            foreach (var r in allRenderers)
            {
                if (r != null)
                {
                    r.forceRenderingOff = true;
                }
            }

            // 3. Instanciar o modelo 3D visual padrão da mochila Flyye MBSS (UCP)
            try
            {
                Vector3 backpackPos = corpsePlayer.Position + Vector3.up * 0.12f;
                if (corpsePlayer.PlayerBones?.Spine3?.Original != null)
                {
                    backpackPos = corpsePlayer.PlayerBones.Spine3.Original.position;
                }

                GameObject mbssPrefab = GetMbssBackpackPrefab();
                if (mbssPrefab != null)
                {
                    GameObject backpackVisual = UnityEngine.Object.Instantiate(mbssPrefab, backpackPos, Quaternion.identity);
                    backpackVisual.transform.SetParent(corpsePlayer.gameObject.transform, true);

                    // Garantir que os renderers da mochila instanciada estejam visíveis
                    var mbssRenderers = backpackVisual.GetComponentsInChildren<Renderer>();
                    foreach (var r in mbssRenderers)
                    {
                        if (r != null)
                        {
                            r.forceRenderingOff = false;
                            r.enabled = true;
                        }
                    }

                    // Remover rigidbodies/colliders extras do prefab visual para não colidir com o bot
                    var visualRbs = backpackVisual.GetComponentsInChildren<Rigidbody>();
                    foreach (var vrb in visualRbs)
                    {
                        if (vrb != null) UnityEngine.Object.Destroy(vrb);
                    }

                    tracked.SpawnedBackpackVisual = backpackVisual;
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogWarning($"[TRL-DynamicSpawn] Could not attach visual MBSS backpack: {ex.Message}");
            }

            Plugin.LogSource?.LogInfo($"[TRL-DynamicSpawn] Corpse Cleanup: Converted '{corpsePlayer.Profile.Nickname}' to standard Flyye MBSS Backpack (Physics Frozen + 0 Draw Calls). Loot remains interactive.");
        }

        private GameObject GetMbssBackpackPrefab()
        {
            if (_cachedMbssPrefab != null) return _cachedMbssPrefab;

            try
            {
                if (Singleton<ItemFactoryClass>.Instantiated)
                {
                    var item = Singleton<ItemFactoryClass>.Instance.CreateItem(MongoID.Generate(), MBSS_BACKPACK_TEMPLATE, null);
                    if (item?.Template?.Prefab != null)
                    {
                        if (Singleton<IEasyAssets>.Instantiated)
                        {
                            _cachedMbssPrefab = Singleton<IEasyAssets>.Instance.GetAsset<GameObject>(item.Template.Prefab);
                        }

                        if (_cachedMbssPrefab == null && Singleton<PoolManagerClass>.Instantiated)
                        {
                            _cachedMbssPrefab = Singleton<PoolManagerClass>.Instance.CreateCleanLootPrefab(item, EFT.CameraControl.ECameraType.Default);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogWarning($"[TRL-DynamicSpawn] Failed to load MBSS backpack prefab: {ex.Message}");
            }

            return _cachedMbssPrefab;
        }

        private void DestroyCorpse(TrackedCorpse tracked)
        {
            var corpsePlayer = tracked.Player;
            if (corpsePlayer == null || corpsePlayer.gameObject == null) return;

            string nickname = corpsePlayer.Profile?.Nickname ?? "Unknown";

            if (tracked.SpawnedBackpackVisual != null)
            {
                UnityEngine.Object.Destroy(tracked.SpawnedBackpackVisual);
            }

            if (Singleton<IBotGame>.Instantiated && Singleton<IBotGame>.Instance?.BotsController != null)
            {
                Singleton<IBotGame>.Instance.BotsController.DestroyInfo(corpsePlayer);
            }

            corpsePlayer.Dispose();
            UnityEngine.Object.Destroy(corpsePlayer.gameObject);

            Plugin.LogSource?.LogInfo($"[TRL-DynamicSpawn] Corpse Cleanup: Full Despawn executed for '{nickname}'.");
        }

        private static bool IsBossOrSpecial(WildSpawnType role)
        {
            // Normal Scavs (Assault, CursedAssault, Marksman/Sniper) e PMCs (Usec/Bear) NÃO são bosses
            if (role == WildSpawnType.assault || role == WildSpawnType.cursedAssault ||
                role == WildSpawnType.marksman ||
                role == WildSpawnType.pmcUSEC || role == WildSpawnType.pmcBEAR)
            {
                return false;
            }
            return true;
        }
    }
}
