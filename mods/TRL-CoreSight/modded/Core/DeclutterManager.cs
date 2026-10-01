using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Comfort.Common;
using EFT;
using EFT.AssetsManager;
using EFT.Ballistics;
using EFT.Interactive;
using Koenigz.PerfectCulling.EFT;
using TRLCoreSight.Configuration;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TRLCoreSight.Core
{
    public enum EClutterType
    {
        Garbage,
        Heaps,
        Cartridges,
        FakeFood,
        Decals,
        Puddles,
        Shards
    }

    public class TrackedClutterItem
    {
        public GameObject GameObject;
        public EClutterType Type;
        public Renderer[] Renderers;
        public bool OriginalActive;
        public bool[] OriginalRendererStates;
        public bool IsHidden;
    }

    public class DeclutterManager : MonoBehaviour
    {
        public static DeclutterManager Instance { get; private set; }

        private readonly List<TrackedClutterItem> _trackedItems = new List<TrackedClutterItem>();
        private Coroutine _scanCoroutine;
        private bool _isScanning = false;
        private string _currentLocationId = string.Empty;

        private void Awake()
        {
            Instance = this;
            ModConfig.OnDeclutterSettingChanged += OnSettingChanged;
        }

        private void OnDestroy()
        {
            ModConfig.OnDeclutterSettingChanged -= OnSettingChanged;
            Cleanup();
            if (Instance == this) Instance = null;
        }

        public void OnRaidStarted()
        {
            if (IsInHideout())
            {
                Plugin.LogSource?.LogInfo("[TRL-CoreSight][Declutter] Hideout detectado (bunker_2). Declutter ignorado.");
                return;
            }

            _currentLocationId = ResolveCurrentLocationId();

            if (!IsMapEnabled(_currentLocationId))
            {
                Plugin.LogSource?.LogInfo($"[TRL-CoreSight][Declutter] Mapa '{_currentLocationId}' desativado nas configurações. Pulando varredura.");
                return;
            }

            if (!ModConfig.EnableDeclutter.Value)
            {
                Plugin.LogSource?.LogInfo("[TRL-CoreSight][Declutter] Declutter desativado globalmente.");
                return;
            }

            StartScan();
        }

        public void OnRaidFinished()
        {
            Cleanup();
        }

        public void StartScan()
        {
            if (_isScanning) return;

            if (_scanCoroutine != null)
            {
                StopCoroutine(_scanCoroutine);
                _scanCoroutine = null;
            }

            _scanCoroutine = StartCoroutine(TimeSlicedScanRoutine());
        }

        private void OnSettingChanged()
        {
            if (_isScanning) return;

            // Se ativou no F12 durante a raid e ainda não foi escaneado, inicia a varredura ao vivo!
            if (_trackedItems.Count == 0)
            {
                if (string.IsNullOrEmpty(_currentLocationId))
                    _currentLocationId = ResolveCurrentLocationId();

                if (ModConfig.EnableDeclutter.Value && IsMapEnabled(_currentLocationId))
                {
                    StartScan();
                    return;
                }
            }

            bool isLocationEnabled = IsMapEnabled(_currentLocationId);
            bool globalEnabled = ModConfig.EnableDeclutter.Value;

            if (!isLocationEnabled || !globalEnabled)
            {
                RestoreAll();
                return;
            }

            ApplyDeclutterStates();
        }

        private void ApplyDeclutterStates()
        {
            if (!ModConfig.EnableDeclutter.Value || !IsMapEnabled(_currentLocationId))
            {
                RestoreAll();
                return;
            }

            EDeclutterMode mode = ModConfig.DeclutterMode.Value;

            foreach (var item in _trackedItems)
            {
                if (item.GameObject == null) continue;

                bool shouldHide = ShouldHideCategory(item.Type);

                if (shouldHide)
                {
                    if (mode == EDeclutterMode.RendererOnly)
                    {
                        // Se o GameObject estava desligado (modo anterior FullGameObject), reativa-o
                        if (!item.GameObject.activeSelf && item.OriginalActive)
                            item.GameObject.SetActive(true);

                        if (item.Renderers != null)
                        {
                            for (int i = 0; i < item.Renderers.Length; i++)
                            {
                                if (item.Renderers[i] != null)
                                    item.Renderers[i].enabled = false;
                            }
                        }
                    }
                    else // FullGameObject
                    {
                        // Restaura renderers antes de desligar o GameObject para evitar inconsistência
                        if (item.Renderers != null && item.OriginalRendererStates != null)
                        {
                            for (int i = 0; i < item.Renderers.Length; i++)
                            {
                                if (item.Renderers[i] != null && i < item.OriginalRendererStates.Length)
                                    item.Renderers[i].enabled = item.OriginalRendererStates[i];
                            }
                        }

                        if (item.GameObject.activeSelf)
                            item.GameObject.SetActive(false);
                    }
                    item.IsHidden = true;
                }
                else
                {
                    RestoreItem(item);
                }
            }
        }

        private void RestoreAll()
        {
            foreach (var item in _trackedItems)
            {
                if (item.GameObject == null) continue;
                RestoreItem(item);
            }
        }

        private void RestoreItem(TrackedClutterItem item)
        {
            if (item.GameObject == null) return;

            // Restaura GameObject se foi desligado
            if (!item.GameObject.activeSelf && item.OriginalActive)
            {
                item.GameObject.SetActive(true);
            }

            // Restaura Renderers se foram desligados
            if (item.Renderers != null && item.OriginalRendererStates != null)
            {
                for (int i = 0; i < item.Renderers.Length; i++)
                {
                    if (item.Renderers[i] != null && i < item.OriginalRendererStates.Length)
                    {
                        item.Renderers[i].enabled = item.OriginalRendererStates[i];
                    }
                }
            }

            // Se pertencer a um LODGroup, forçar recálculo para renderização instantânea (in live)
            LODGroup lod = item.GameObject.GetComponentInParent<LODGroup>();
            if (lod != null)
            {
                lod.RecalculateBounds();
            }

            item.IsHidden = false;
        }

        public void Cleanup()
        {
            if (_scanCoroutine != null)
            {
                StopCoroutine(_scanCoroutine);
                _scanCoroutine = null;
            }

            _isScanning = false;
            _trackedItems.Clear();
            _currentLocationId = string.Empty;
        }

        private IEnumerator TimeSlicedScanRoutine()
        {
            _isScanning = true;
            _trackedItems.Clear();

            int sceneCount = SceneManager.sceneCount;
            int totalProcessed = 0;
            int totalFound = 0;
            const int batchSizePerFrame = 250;
            int processedInBatch = 0;

            Queue<Transform> traversalQueue = new Queue<Transform>(2048);

            for (int i = 0; i < sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded || scene.name == "bunker_2") continue;

                GameObject[] rootObjects = scene.GetRootGameObjects();
                foreach (var root in rootObjects)
                {
                    if (root != null)
                        traversalQueue.Enqueue(root.transform);
                }
            }

            while (traversalQueue.Count > 0)
            {
                Transform current = traversalQueue.Dequeue();
                if (current == null) continue;

                totalProcessed++;
                processedInBatch++;

                // Enfileira filhos para busca hierárquica
                for (int c = 0; c < current.childCount; c++)
                {
                    Transform child = current.GetChild(c);
                    if (child != null)
                        traversalQueue.Enqueue(child);
                }

                GameObject obj = current.gameObject;

                if (EvaluateGameObject(obj, out EClutterType category))
                {
                    Renderer[] allRenderers = obj.GetComponentsInChildren<Renderer>(true);
                    List<Renderer> validRenderers = new List<Renderer>();
                    for (int r = 0; r < allRenderers.Length; r++)
                    {
                        var ren = allRenderers[r];
                        if (ren == null) continue;
                        string renName = ren.gameObject.name.ToLowerInvariant();
                        bool isRenForbidden = false;
                        foreach (var forbidden in DontDisableWords)
                        {
                            if (renName.Contains(forbidden))
                            {
                                isRenForbidden = true;
                                break;
                            }
                        }
                        if (!isRenForbidden)
                        {
                            validRenderers.Add(ren);
                        }
                    }

                    if (validRenderers.Count == 0) continue;
                    Renderer[] renderers = validRenderers.ToArray();
                    bool[] origRendererStates = renderers.Select(r => r != null && r.enabled).ToArray();

                    TrackedClutterItem item = new TrackedClutterItem
                    {
                        GameObject = obj,
                        Type = category,
                        Renderers = renderers,
                        OriginalActive = obj.activeSelf,
                        OriginalRendererStates = origRendererStates,
                        IsHidden = false
                    };

                    _trackedItems.Add(item);
                    totalFound++;
                }

                if (processedInBatch >= batchSizePerFrame)
                {
                    processedInBatch = 0;
                    yield return null; // Pausa para permitir que a Unity processe os quadros
                }
            }

            _isScanning = false;
            _scanCoroutine = null;

            Plugin.LogSource?.LogInfo($"[TRL-CoreSight][Declutter] Varredura concluída. Objetos avaliados: {totalProcessed}. Detritos cosméticos identificados: {totalFound}.");

            // Aplica os estados imediatamente após o fim da varredura
            ApplyDeclutterStates();
        }

        private bool EvaluateGameObject(GameObject obj, out EClutterType category)
        {
            category = EClutterType.Garbage;
            if (obj == null) return false;

            string nameLower = obj.name.ToLowerInvariant();

            // 1. Verificação de Palavras-Chave Proibidas no próprio objeto
            foreach (var forbidden in DontDisableWords)
            {
                if (nameLower.Contains(forbidden)) return false;
            }

            // 1.1 Blindagem de Hierarquia Ascendente: se qualquer PAI for móvel, estante, mesa ou estrutura, protege!
            Transform parent = obj.transform.parent;
            while (parent != null)
            {
                string parentName = parent.name.ToLowerInvariant();
                foreach (var forbidden in DontDisableWords)
                {
                    if (parentName.Contains(forbidden)) return false;
                }
                parent = parent.parent;
            }

            // 2. Classificação de Categoria
            if (!TryClassifyCategory(nameLower, out category)) return false;

            // 3. Verificação de Componentes Críticos do Jogo (Blindagem EFT 0.16.9)
            if (IsProtectedTarkovComponent(obj)) return false;

            // 4. Verificação de Filhos Críticos ou com Nomes Proibidos
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                Transform child = obj.transform.GetChild(i);
                if (child != null)
                {
                    string childName = child.name.ToLowerInvariant();
                    foreach (var forbidden in DontDisableWords)
                    {
                        if (childName.Contains(forbidden)) return false;
                    }
                    if (IsProtectedTarkovComponent(child.gameObject)) return false;
                }
            }

            // 5. Verificação de Colisores Sólidos de Mobília / Superfície Física
            Collider[] colliders = obj.GetComponentsInChildren<Collider>(true);
            if (colliders != null && colliders.Length > 0)
            {
                for (int c = 0; c < colliders.Length; c++)
                {
                    var col = colliders[c];
                    if (col != null && !col.isTrigger && col.GetType().Name != "WheelCollider")
                    {
                        // Se tem colisor físico sólido considerável, é móvel ou obstáculo sólido
                        if (col.bounds.size.magnitude > 0.35f)
                        {
                            return false;
                        }
                    }
                }
            }

            // 6. Verificação de Altura Vertical e Volume 3D de Móveis
            bool isDecalOrParticle = obj.GetComponent<StaticDeferredDecal>() != null || obj.GetComponent("ParticleSystem") != null;
            if (!isDecalOrParticle)
            {
                Bounds combined = GetCombinedBounds(obj);
                if (combined.size.y > ModConfig.DeclutterScaleLimit.Value)
                    return false; // Muito alto, pode servir de cobertura física sólida

                // Se tiver volume tridimensional significativo em todas as dimensões (ex: mesa ou escrivaninha)
                if (combined.size.x > 0.5f && combined.size.z > 0.5f && combined.size.y > 0.35f)
                    return false; // É um móvel ou estrutura volumétrica, não é detrito cosmético de chão
            }

            return true;
        }

        private static Bounds GetCombinedBounds(GameObject obj)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(false);
            if (renderers == null || renderers.Length == 0) return new Bounds(obj.transform.position, Vector3.zero);

            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                    b.Encapsulate(renderers[i].bounds);
            }
            return b;
        }

        private static bool IsProtectedTarkovComponent(GameObject go)
        {
            if (go == null) return true;

            // Loot & Itens
            if (go.GetComponent<LootableContainer>() != null) return true;
            if (go.GetComponent<LootableContainersGroup>() != null) return true;
            if (go.GetComponent<ObservedLootItem>() != null) return true;
            if (go.GetComponent<LootItem>() != null) return true;
            if (go.GetComponent<WeaponModPoolObject>() != null) return true;

            // Entidades & Jogadores
            if (go.GetComponent<LocalPlayer>() != null) return true;
            if (go.GetComponent<Player>() != null) return true;
            if (go.GetComponent<BotOwner>() != null) return true;
            if (go.GetComponent<BotSpawner>() != null) return true;

            // Sistemas de Culling e Oclusão BSG
            if (go.GetComponent<CullingObject>() != null) return true;
            if (go.GetComponent<CullingLightObject>() != null) return true;
            if (go.GetComponent<CullingGroup>() != null) return true;
            if (go.GetComponent<DisablerCullingObject>() != null) return true;
            if (go.GetComponent<ObservedCullingManager>() != null) return true;
            if (go.GetComponent<PerfectCullingCrossSceneGroup>() != null) return true;
            if (go.GetComponent<ScreenDistanceSwitcher>() != null) return true;
            if (go.GetComponent<BakedLodContent>() != null) return true;
            if (go.GetComponent<GuidComponent>() != null) return true;
            if (go.GetComponent<OcclusionPortal>() != null) return true;
            if (go.GetComponent<MultisceneSharedOccluder>() != null) return true;

            // Física & Balística
            if (go.GetComponent<WindowBreaker>() != null) return true;
            if (go.GetComponent<BallisticCollider>() != null) return true;

            // Sistemas Atualizados EFT 0.16.x
            if (go.GetComponent("TripwireVisual") != null) return true;
            if (go.GetComponent("TripwireSynchronizableObject") != null) return true;
            if (go.GetComponent("TripwireProceduralMesh") != null) return true;
            if (go.GetComponent("TripwireInteractionTrigger") != null) return true;
            if (go.GetComponent("SynchronizableObject") != null) return true;
            if (go.GetComponent<ExfiltrationPoint>() != null) return true;
            if (go.GetComponent<StationaryWeapon>() != null) return true;
            if (go.GetComponent<PlaceItemTrigger>() != null) return true;
            if (go.GetComponent<BorderZone>() != null) return true;
            if (go.GetComponent("TrapSyncable") != null) return true;

            return false;
        }

        private static bool TryClassifyCategory(string name, out EClutterType category)
        {
            // Cartuchos gastos
            foreach (var key in CartridgeKeywords)
            {
                if (name.Contains(key))
                {
                    category = EClutterType.Cartridges;
                    return true;
                }
            }

            // Comida falsa
            foreach (var key in FakeFoodKeywords)
            {
                if (name.Contains(key))
                {
                    category = EClutterType.FakeFood;
                    return true;
                }
            }

            // Decalques
            foreach (var key in DecalKeywords)
            {
                if (name.Contains(key))
                {
                    category = EClutterType.Decals;
                    return true;
                }
            }

            // Poças
            foreach (var key in PuddleKeywords)
            {
                if (name.Contains(key))
                {
                    category = EClutterType.Puddles;
                    return true;
                }
            }

            // Cacos de vidro
            foreach (var key in ShardKeywords)
            {
                if (name.Contains(key))
                {
                    category = EClutterType.Shards;
                    return true;
                }
            }

            // Montes de entulho
            foreach (var key in HeapKeywords)
            {
                if (name.Contains(key))
                {
                    category = EClutterType.Heaps;
                    return true;
                }
            }

            // Lixo e papéis em geral
            foreach (var key in GarbageKeywords)
            {
                if (name.Contains(key))
                {
                    category = EClutterType.Garbage;
                    return true;
                }
            }

            category = EClutterType.Garbage;
            return false;
        }

        private static bool ShouldHideCategory(EClutterType type)
        {
            switch (type)
            {
                case EClutterType.Garbage: return ModConfig.DeclutterGarbage.Value;
                case EClutterType.Heaps: return ModConfig.DeclutterHeaps.Value;
                case EClutterType.Cartridges: return ModConfig.DeclutterCartridges.Value;
                case EClutterType.FakeFood: return ModConfig.DeclutterFakeFood.Value;
                case EClutterType.Decals: return ModConfig.DeclutterDecals.Value;
                case EClutterType.Puddles: return ModConfig.DeclutterPuddles.Value;
                case EClutterType.Shards: return ModConfig.DeclutterShards.Value;
                default: return false;
            }
        }

        private static bool IsMapEnabled(string locationId)
        {
            if (string.IsNullOrEmpty(locationId)) return false;

            switch (locationId.ToLowerInvariant())
            {
                case "factory4_day":
                case "factory4_night":
                    return ModConfig.DeclutterFactory.Value;
                case "bigmap":
                    return ModConfig.DeclutterCustoms.Value;
                case "woods":
                    return ModConfig.DeclutterWoods.Value;
                case "shoreline":
                    return ModConfig.DeclutterShoreline.Value;
                case "interchange":
                    return ModConfig.DeclutterInterchange.Value;
                case "rezervbase":
                    return ModConfig.DeclutterReserve.Value;
                case "lighthouse":
                    return ModConfig.DeclutterLighthouse.Value;
                case "tarkovstreets":
                    return ModConfig.DeclutterStreets.Value;
                case "sandbox":
                case "sandbox_high":
                    return ModConfig.DeclutterGroundZero.Value;
                case "laboratory":
                    return ModConfig.DeclutterTheLab.Value;
                case "labyrinth":
                    return ModConfig.DeclutterTheLabyrinth.Value;
                default:
                    return false;
            }
        }

        private static string ResolveCurrentLocationId()
        {
            try
            {
                var session = Singleton<ClientApplication<ISession>>.Instance as TarkovApplication;
                if (session != null && session.CurrentRaidSettings != null && !string.IsNullOrEmpty(session.CurrentRaidSettings.LocationId))
                {
                    return session.CurrentRaidSettings.LocationId;
                }
            }
            catch { }

            try
            {
                var mainPlayer = Singleton<GameWorld>.Instance?.MainPlayer;
                if (mainPlayer != null && !string.IsNullOrEmpty(mainPlayer.Location))
                {
                    return mainPlayer.Location;
                }
            }
            catch { }

            return string.Empty;
        }

        private static bool IsInHideout()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).name == "bunker_2")
                    return true;
            }
            return false;
        }

        // --- Dicionários Curados ---
        private static readonly string[] DontDisableWords = new string[]
        {
            // Entidades e Componentes do Jogo
            "item_", "weapon_", "barter_", "mod_", "audio", "container", "trigger",
            "culling", "collider", "colider", "group", "manager", "scene", "player",
            "portal", "bakelod", "door", "shadow", "mine", "tripwire", "extract",
            "exfil", "stationary", "quest", "marker", "btr",

            // Mobília, Mesas, Armários e Superfícies (NUNCA DESATIVAR)
            "table", "desk", "shelf", "bookshelf", "wardrobe", "cabinet", "chair",
            "bed", "furniture", "couch", "sofa", "bench", "stand", "counter", "rack",
            "office", "cupboard", "dresser", "nightstand",

            // Livros, Documentos e Materiais de Prateleira (NUNCA DESATIVAR)
            "book", "books", "folder", "folders", "magazine", "magazines", "binder",
            "document", "notebook",

            // Estruturas e Arquitetura
            "wall", "floor", "ceiling", "window", "stairs", "pillar", "beam", "roof",
            "room", "building", "facade", "interior", "wood_board"
        };

        private static readonly string[] GarbageKeywords = new string[]
        {
            "turniket_", "tray_", "electronic_box", "styrofoam_", "polyethylene_set",
            "penyok_", "trashbag_", "kaska", "boot_", "garbage_", "garbage_stone",
            "garbage_paper", "cable", "drawing_", "paper_", "_paper", "paper1", "paper2",
            "paper3", "paper4", "paper5", "paper6", "paper7", "paper8", "paper9",
            "pan1", "pan2", "pan3", "pan4", "pan5", "pan6", "pan7", "pan8", "pan9",
            "poster1", "poster2", "poster3", "poster4", "poster5", "poster6", "poster7",
            "poster8", "poster9", "_junk", "junk_", "_trash", "trash_", "cardboard_",
            "_cardboard", "sticks", "cloth_", "pants_", "shirt_", "dishes_", "cutlery_",
            "fuel_tube", "city_garbage_", "city_road_garbage", "reserve_garbage_",
            "reserve_road_garbage", "garbage_parking_", "goshan_garbage", "package_garbage",
            "leaves_"
        };

        private static readonly string[] HeapKeywords = new string[]
        {
            "trash_pile_", "_trash_pile", "crushed_concrete", "crushed_concreate",
            "baked_garbage", "garbage_constructor", "_scrap", "scrap_", "heap_", "_heap",
            "_pile", "pile_", "_rubble", "rubble_", "scatter_", "_scatter",
            "scattered_", "_scattered", "brick_pile",
            "poletelen01", "poletelen02", "poletelen03", "poletelen04", "poletelen05",
            "poletelen06", "poletelen07", "poletelen08", "poletelen09",
            "vetky1", "vetky2", "vetky1_", "vetky2_", "vetky3_", "vetky4_", "vetky5_", "vetky6_",
            "tile_broken_"
        };

        private static readonly string[] CartridgeKeywords = new string[]
        {
            "shotshell_", "shells_", "_shotshell", "_shells", "rifleshell_", "_rifleshell", "rifle_shells_"
        };

        private static readonly string[] FakeFoodKeywords = new string[]
        {
            "canned", "canned_", "can_", "juice_", "carton_", "_creased", "bottle", "bottle_",
            "crackers_", "oat_flakes", "chocolate_", "biscuits", "package_", "cigarette_",
            "medkit_", "_cup", "plasticcup_"
        };

        private static readonly string[] DecalKeywords = new string[]
        {
            "goshan_decal", "ground_decal", "decalgraffiti", "blood_", "_blood", "sand_decal",
            "decal_dirt", "decal_drip", "decal_", "decals_"
        };

        private static readonly string[] PuddleKeywords = new string[]
        {
            "puddle", "puddles_", "_puddles", "puddle group"
        };

        private static readonly string[] ShardKeywords = new string[]
        {
            "_glass", "brokenglass_", "glass_crush", "plite_crush", "lesa_crush", "shards_", "_shards"
        };
    }
}
