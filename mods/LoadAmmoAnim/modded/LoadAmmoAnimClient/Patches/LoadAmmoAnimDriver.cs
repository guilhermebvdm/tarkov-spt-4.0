using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using Manimal.LoadAmmoAnim.CustomEFTData;
using SPT.Reflection.Patching;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace Manimal.LoadAmmoAnim.Patches
{
    // event seam used by the Fika compat assembly. the main client raises these
    // for local-player session lifecycle changes, the Fika layer subscribes and
    // turns them into network packets. fire-and-forget — handlers must not throw.
    internal static class LoadAmmoAnimEvents
    {
        public static event Action<Player, string, string, float> AnimStarted;
        public static event Action<Player, bool> AnimStopped;
        public static event Action<Player, string> MagSwapped;

        internal static void RaiseStarted(Player p, string magTpl, string ammoTpl, float speed)
        {
            try { AnimStarted?.Invoke(p, magTpl, ammoTpl, speed); }
            catch (Exception ex) { Plugin.LogSource?.LogError($"[LoadAmmoAnim] AnimStarted handler threw: {ex.Message}"); }
        }

        internal static void RaiseStopped(Player p, bool playPutAway)
        {
            try { AnimStopped?.Invoke(p, playPutAway); }
            catch (Exception ex) { Plugin.LogSource?.LogError($"[LoadAmmoAnim] AnimStopped handler threw: {ex.Message}"); }
        }

        internal static void RaiseMagSwapped(Player p, string magTpl)
        {
            try { MagSwapped?.Invoke(p, magTpl); }
            catch (Exception ex) { Plugin.LogSource?.LogError($"[LoadAmmoAnim] MagSwapped handler threw: {ex.Message}"); }
        }
    }

    // per-player session bag. one of these per Player that has ever started a load.
    // keeps everything that used to be on the static LoadAmmoAnimState class so
    // multi-player setups (Fika) can run independent sessions side by side.
    internal sealed class PlayerSession
    {
        // tracks how many Class1204 sessions are alive for this player. the visible
        // anim only starts on the 0 to 1 edge.
        public int LoadingCount;

        public bool IsOurAnimation;
        public LoadAmmoBundleController ActiveController;
        public Coroutine LoopCoroutine;

        // seconds per round at the player's current mag drills level. re-read from
        // Class1204.Float_0 every session so skill ups take effect right away.
        public float LoadOneAmmoSpeed = 1f;

        // armed at the start of each session, consumed by Class1204DrawDelayPatch on
        // the first bullet only. that way we only stretch the initial delay, not every one.
        public bool DrawPhasePending;

        // template id of the mag we are loading.
        public string CurrentMagTemplateId;

        // the live mag item. AnimLoop checks this every frame to know when its full
        // and we should stop.
        public MagazineItemClass CurrentMag;

        // template id of the source ammo being loaded.
        public string CurrentAmmoTemplateId;

        // the live source ammo stack.
        public AmmoItemClass CurrentAmmo;

        public bool IsLoading => LoadingCount > 0;
    }

    // registry of per-player sessions. patches that have a Player handle look up
    // through here. patches without one (rare) fall back to MainPlayer.
    internal static class LoadAmmoAnimState
    {
        private static readonly Dictionary<Player, PlayerSession> _sessions =
            new Dictionary<Player, PlayerSession>();

        public static PlayerSession Get(Player player)
        {
            if (player == null) return null;
            if (!_sessions.TryGetValue(player, out var s))
                _sessions[player] = s = new PlayerSession();
            return s;
        }

        public static PlayerSession TryGet(Player player)
        {
            if (player == null) return null;
            _sessions.TryGetValue(player, out var s);
            return s;
        }

        public static void Clear(Player player)
        {
            if (player == null) return;
            _sessions.Remove(player);
        }

        public static void ClearAllSessions()
        {
            _sessions.Clear();
        }

        // global probe for patches without a player handle (eg. CLA's static methods).
        public static bool AnyIsOurAnimation()
        {
            foreach (var s in _sessions.Values)
                if (s.IsOurAnimation) return true;
            return false;
        }

        public static void OnLoadingStarted(Player player)
        {
            if (player == null) return;
            var s = Get(player);
            // only start a new anim on the 0 to 1 edge. anything else is just an extra
            // session piling on top of one we are already animating for.
            if (s.LoadingCount++ == 0 && !s.IsOurAnimation)
            {
                s.DrawPhasePending = true;
                player.StartCoroutine(LoadAmmoAnimDriver.StartNextFrame(player));
                // notify Fika compat layer so it can broadcast.
                LoadAmmoAnimEvents.RaiseStarted(player, s.CurrentMagTemplateId, s.CurrentAmmoTemplateId, s.LoadOneAmmoSpeed);
            }
        }

        public static void OnLoadingEnded(Player player)
        {
            var s = TryGet(player);
            if (s == null) return;
            if (--s.LoadingCount < 0) s.LoadingCount = 0;
        }

        // hard reset for LoadingCount, used when the stall logic decides things have gone off the rails.
        internal static void ForceResetLoading(Player player)
        {
            var s = TryGet(player);
            if (s != null) s.LoadingCount = 0;
        }
    }

    // creates the virtual bundle item, runs the controller swap, drives the per-frame
    // loop that watches mag count and triggers teardown.
    internal static class LoadAmmoAnimDriver
    {
        // matches the template id in hidden_anim_STANAGS.jsonc. factory.CreateItem
        // produces LoadAmmoBundleItem via WTTClientCommonLib's [CustomParent].
        private const string BundleItemTplId = "69d69c70ed183ba9c882f7f7";

        // length of the bundle's draw clip. Class1204DrawDelayPatch pads the
        // first-bullet wait by this, scaled to the actual playback speed.
        internal const float DrawClipSeconds = 0.867f;

        // ceiling on the put-away wait. clip is roughly 1s.
        private const float PutAwayMaxWaitSeconds = 1.5f;

        // CLA can dip LoadingCount to 0 for a tick between bullets, so we wait this
        // long before deciding the session is actually over.
        private const float ClaStopTimeoutSeconds = 0.3f;

        // process-wide flag, the asset cache is shared across players.
        private static bool _bundleWarmed;

        public static IEnumerator StartNextFrame(Player player)
        {
            // wait one frame so Class1204.Start can finish its synchronous setup.
            // proceeding inline races the inventory state.
            yield return null;
            if (player == null) yield break;

            var session = LoadAmmoAnimState.TryGet(player);
            if (session == null || !session.IsLoading) yield break;

            var bundleItem = CreateBundleItem();
            if (bundleItem == null)
            {
                Plugin.LogSource?.LogWarning("[LoadAmmoAnim] could not obtain bundle item, skipping animation.");
                yield break;
            }

            // raid-start warm-up hasnt finished yet, so retain the bundle inline here.
            // the first reload of the raid sometimes lands on this path. every reload
            // after is a no-op since _bundleWarmed sticks around.
            if (!_bundleWarmed)
            {
                var usePrefab = bundleItem.UsePrefab;
                if (usePrefab != null)
                {
                    var poolManager = Singleton<PoolManagerClass>.Instance;
                    if (poolManager?.EasyAssets != null)
                    {
                        var retainTask = GClass1857.RetainSeparateTask(
                            poolManager.EasyAssets, new[] { usePrefab.path });
                        while (!retainTask.IsCompleted) yield return null;

                        if (!retainTask.IsFaulted && retainTask.Result?.LoadingJob != null)
                        {
                            var loadTask = retainTask.Result.LoadingJob;
                            while (!loadTask.IsCompleted) yield return null;
                        }
                        _bundleWarmed = true;
                    }
                }
            }

            TryStart(player, bundleItem);
        }

        private static void TryStart(Player player, LoadAmmoBundleItem bundleItem)
        {
            var session = LoadAmmoAnimState.Get(player);
            session.IsOurAnimation = true;

            try { player.StopBlindFire(); } catch { }
            try { player.RemoveLeftHandItem(); } catch { }

            // hand-stitched controller swap. Player.Proceed<T> uses PoolManagerClass.CreateItem
            // as its factory, which reads the item's Prefab field. our bundle is on UsePrefab,
            // so Proceed loads PortableRangeFinder's default mesh and smethod_4 NREs trying
            // to find the WeaponPrefab component.
            //
            // Drop fires the current weapon's put-away. when it finishes, our callback
            // does destroy + create + spawn back-to-back so PWA never sees stale transforms.
            player.DropCurrentController(
                () => CreateAndSpawnBundleController(player, bundleItem),
                fastDrop: false,
                nextControllerItem: bundleItem);
        }

        // post-drop callback. CreateItemUsablePrefab is the factory delegate that reads
        // UsePrefab where our bundle path lives.
        private static void CreateAndSpawnBundleController(Player player, LoadAmmoBundleItem bundleItem)
        {
            var session = LoadAmmoAnimState.Get(player);
            try
            {
                if (player.HandsController != null)
                {
                    try { player.DestroyController(); }
                    catch (Exception ex)
                    {
                        Plugin.LogSource?.LogError(
                            $"[LoadAmmoAnim] DestroyController in spawn callback: {ex.Message}");
                    }
                }

                var controller = Player.ItemHandsController.smethod_1<LoadAmmoBundleController>(
                    player,
                    bundleItem,
                    new Player.ItemHandsController.Delegate8(
                        Singleton<PoolManagerClass>.Instance.CreateItemUsablePrefab));

                if (controller == null)
                {
                    Plugin.LogSource?.LogError("[LoadAmmoAnim] smethod_1 returned null controller.");
                    session.IsOurAnimation = false;
                    return;
                }

                Player.UsableItemController.smethod_8<LoadAmmoBundleController>(controller, player);

                // SpawnController calls controller.Spawn(1f, ...) which forces animator
                // speed to 1f and registers the equip-event callback. by the time that
                // callback resolves the draw clip is already done, so we set animSpeed
                // synchronously after SpawnController returns. that way it applies to
                // the entire draw, not just the post-draw loop.
                player.SpawnController(controller, () => { });

                session.ActiveController = controller;

                // tie playback to the player's current mag-drills speed.
                var firearmsAnim = controller.FirearmsAnimator;
                if (firearmsAnim != null && session.LoadOneAmmoSpeed > 0f)
                    firearmsAnim.SetAnimationSpeed(1f / session.LoadOneAmmoSpeed);

                // Attach dynamic native magazine and ammo prefabs
                controller.AttachDynamicItems(
                    player,
                    session.CurrentMag,
                    session.CurrentMagTemplateId,
                    session.CurrentAmmo,
                    session.CurrentAmmoTemplateId);

                session.LoopCoroutine = player.StartCoroutine(AnimLoop(player, controller));
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError(
                    $"[LoadAmmoAnim] CreateAndSpawnBundleController threw: {ex.GetType().Name}: {ex.Message}");
                session.IsOurAnimation = false;
            }
        }

        // builds a throwaway bundle item that only lives in memory. it never enters
        // any inventory and never gets consumed.
        private static LoadAmmoBundleItem CreateBundleItem()
        {
            try
            {
                var factory = Singleton<ItemFactoryClass>.Instance;
                if (factory == null) return null;
                return factory.CreateItem(
                    MongoID.Generate(false).ToString(),
                    BundleItemTplId,
                    null) as LoadAmmoBundleItem;
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogWarning(
                    $"[LoadAmmoAnim] could not create bundle item: {ex.Message}");
                return null;
            }
        }

        // preloads the bundle at raid start so the first reload doesnt hitch waiting
        // for it to load.
        public static IEnumerator WarmBundleAsync()
        {
            _bundleWarmed = false;

            string bundlePath = null;
            try
            {
                var factory = Singleton<ItemFactoryClass>.Instance;
                if (factory != null)
                {
                    var tmp = factory.CreateItem(
                        MongoID.Generate(false).ToString(),
                        BundleItemTplId, null) as LoadAmmoBundleItem;
                    bundlePath = tmp?.UsePrefab?.path;
                }
            }
            catch { }

            if (string.IsNullOrEmpty(bundlePath)) yield break;

            var poolManager = Singleton<PoolManagerClass>.Instance;
            if (poolManager?.EasyAssets == null) yield break;

            var retainTask = GClass1857.RetainSeparateTask(
                poolManager.EasyAssets, new[] { bundlePath });
            while (!retainTask.IsCompleted) yield return null;

            if (!retainTask.IsFaulted && retainTask.Result?.LoadingJob != null)
            {
                var loadTask = retainTask.Result.LoadingJob;
                while (!loadTask.IsCompleted) yield return null;
            }

            _bundleWarmed = true;
        }

        // immediate teardown. used when bailing without animating.
        public static void StopAnimationInstantly(Player player)
        {
            var session = LoadAmmoAnimState.TryGet(player);
            if (session?.ActiveController != null)
            {
                session.ActiveController.ReleaseDynamicItems();
            }

            if (player?.HandsController is LoadAmmoBundleController)
            {
                try { player.DestroyController(); }
                catch (Exception ex)
                {
                    Plugin.LogSource?.LogError(
                        $"[LoadAmmoAnim] DestroyController failed: {ex.GetType().Name}: {ex.Message}");
                }
            }
        }

        // terminal teardown. play put-away with the mesh visible, then once the mag
        // is off-screen disable it, destroy the controller, equip the last weapon.
        // DestroyController + TrySetLastEquippedWeapon back-to-back hits Process.Execute's
        // null-hands fast-path so the swap is synchronous and PWA rebinds cleanly.
        internal static IEnumerator PlayPutawayThenRestore(
            Player player,
            LoadAmmoBundleController controller)
        {
            controller?.PlayPutAway();

            float deadline = Time.unscaledTime + PutAwayMaxWaitSeconds;
            while (Time.unscaledTime < deadline)
            {
                if (controller == null) break;
                if (controller.IsPutAwayNearlyDone()) break;
                yield return null;
            }

            controller?.ReleaseDynamicItems();

            if (player == null) yield break;

            if (player.HandsController is LoadAmmoBundleController)
            {
                try { player.DestroyController(); }
                catch (Exception ex)
                {
                    Plugin.LogSource?.LogError(
                        $"[LoadAmmoAnim] DestroyController failed: {ex.GetType().Name}: {ex.Message}");
                }
            }

            if (!(player.HandsController is Player.FirearmController))
                player.TrySetLastEquippedWeapon(true, null);
        }

        // entry point for the Fika compat layer to start the anim on a remote
        // (observed) player when a Start packet arrives. doesnt fire AnimStarted —
        // only the local Class1204 path does, to avoid re-broadcast loops.
        internal static void StartBundleAnim(Player player, string magTemplateId, string ammoTemplateId, float loadOneAmmoSpeed)
        {
            if (player == null) return;
            var session = LoadAmmoAnimState.Get(player);
            // already running for this player (eg. duplicate packet) — ignore.
            if (session.IsOurAnimation) return;

            session.LoadOneAmmoSpeed = loadOneAmmoSpeed > 0f ? loadOneAmmoSpeed : 1f;
            session.CurrentMagTemplateId = magTemplateId;
            session.CurrentMag = null;          // observed: we dont have the source's mag item
            session.CurrentAmmoTemplateId = ammoTemplateId;
            session.CurrentAmmo = null;
            session.LoadingCount = 1;           // synthetic, kept up by Stop packet
            session.DrawPhasePending = false;   // Class1204DrawDelayPatch is local-only

            player.StartCoroutine(StartNextFrame(player));
        }

        // entry point for the Fika compat layer to end an observed player's anim
        // when a Stop packet arrives. playPutAway=true gives the put-away clip,
        // false destroys instantly. doesnt fire AnimStopped to avoid re-broadcast.
        internal static void StopBundleAnim(Player player, bool playPutAway)
        {
            var session = LoadAmmoAnimState.TryGet(player);
            if (session == null) return;

            var controller = session.ActiveController;
            session.LoadingCount = 0;
            session.IsOurAnimation = false;

            if (playPutAway && controller != null && player != null)
                player.StartCoroutine(PlayPutawayThenRestore(player, controller));
            else
                StopAnimationInstantly(player);
        }

        // entry point for the Fika compat layer to swap the visible mag mesh on
        // an observed player when a SwapMesh packet arrives. mirrors the inline
        // chained-mag block inside AnimLoop. doesnt fire MagSwapped.
        internal static void SwapMagMesh(Player player, string newMagTemplateId)
        {
            var session = LoadAmmoAnimState.TryGet(player);
            if (session == null || !session.IsOurAnimation) return;
            session.CurrentMagTemplateId = newMagTemplateId;
            var controller = session.ActiveController;
            if (controller != null && player != null)
                player.StartCoroutine(SwapMagMeshCoroutine(player, controller));
        }

        private static IEnumerator SwapMagMeshCoroutine(Player player, LoadAmmoBundleController controller)
        {
            controller.PlayPutAway();

            float deadline = Time.unscaledTime + PutAwayMaxWaitSeconds;
            while (Time.unscaledTime < deadline)
            {
                if (controller == null) break;
                if (controller.IsPutAwayNearlyDone()) break;
                yield return null;
            }

            if (controller == null) yield break;

            var session = LoadAmmoAnimState.TryGet(player);
            if (session != null)
            {
                controller.AttachDynamicItems(
                    player,
                    session.CurrentMag,
                    session.CurrentMagTemplateId,
                    session.CurrentAmmo,
                    session.CurrentAmmoTemplateId);
            }

            controller.PlayDraw();
        }

        private static IEnumerator AnimLoop(Player player, LoadAmmoBundleController controller)
        {
            var session = LoadAmmoAnimState.Get(player);
            bool claInstalled = ContinuousLoadAmmoCompat.IsInstalled;
            float idleSince = -1f;

            // snapshot the mag this session was started for. once the next mag's
            // Class1204.Start fires, CurrentMag gets overwritten, so we cant trust
            // it inside the loop body.
            MagazineItemClass sessionMag = session.CurrentMag;

            while (session.IsOurAnimation)
            {
                yield return null;

                if (!session.IsOurAnimation || controller == null)
                    break;

                if (!session.IsLoading)
                {
                    if (!claInstalled)
                    {
                        // always run put-away + destroy + re-equip, regardless of
                        // inventory state. instant-teardown leaves PWA stale.
                        session.IsOurAnimation = false;
                        LoadAmmoAnimEvents.RaiseStopped(player, true);
                        if (player != null)
                            player.StartCoroutine(PlayPutawayThenRestore(player, controller));
                        break;
                    }

                    if (idleSince < 0f) idleSince = Time.realtimeSinceStartup;
                    if (Time.realtimeSinceStartup - idleSince >= ClaStopTimeoutSeconds)
                    {
                        session.IsOurAnimation = false;
                        LoadAmmoAnimEvents.RaiseStopped(player, true);
                        if (player != null)
                            player.StartCoroutine(PlayPutawayThenRestore(player, controller));
                        break;
                    }

                    continue;
                }

                idleSince = -1f;

                if (sessionMag != null && sessionMag.Count >= sessionMag.MaxCount)
                {
                    bool nextMagAlreadyLoading = session.CurrentMag != null
                                                 && session.CurrentMag != sessionMag;

                    // chained mag. play put-away, swap mesh while off-screen, play
                    // draw, all on the same controller. keeps the controller alive
                    // across the swap so the next mag flows in seamlessly.
                    if (nextMagAlreadyLoading)
                    {
                        controller.PlayPutAway();

                        float deadline = Time.unscaledTime + PutAwayMaxWaitSeconds;
                        while (Time.unscaledTime < deadline)
                        {
                            if (controller == null) break;
                            if (controller.IsPutAwayNearlyDone()) break;
                            yield return null;
                        }

                        if (controller == null) break;

                        sessionMag = session.CurrentMag;
                        controller.AttachDynamicItems(
                            player,
                            sessionMag,
                            session.CurrentMagTemplateId,
                            session.CurrentAmmo,
                            session.CurrentAmmoTemplateId);

                        LoadAmmoAnimEvents.RaiseMagSwapped(player, session.CurrentMagTemplateId);

                        controller.PlayDraw();
                        continue;
                    }

                    // terminal. put-away + destroy + re-equip.
                    session.IsOurAnimation = false;
                    LoadAmmoAnimState.ForceResetLoading(player);
                    LoadAmmoAnimEvents.RaiseStopped(player, true);
                    if (player != null)
                        player.StartCoroutine(PlayPutawayThenRestore(player, controller));

                    break;
                }
            }

            session.LoopCoroutine = null;
            session.ActiveController = null;
        }
    }

    // catches Class1204.Start, so we can grab the mag's per-bullet speed and template
    // id before the loading session actually starts.
    public class LoadAmmoAnimDetectPatch : ModulePatch
    {

        protected override MethodBase GetTargetMethod()
        {
            var class1204 = AccessTools.Inner(typeof(Player.PlayerInventoryController), "Class1204");
            return AccessTools.Method(class1204, "Start");
        }

        [PatchPrefix]
        private static void Prefix(object __instance)
        {
            var player = Singleton<GameWorld>.Instance?.MainPlayer;
            if (player == null) return;
            var session = LoadAmmoAnimState.Get(player);

            session.CurrentMagTemplateId = null;
            session.CurrentMag = null;
            session.CurrentAmmoTemplateId = null;
            session.CurrentAmmo = null;

            var traverse = Traverse.Create(__instance);
            float speed = traverse.Field<float>("Float_0").Value;
            if (speed > 0f) session.LoadOneAmmoSpeed = speed;

            var mag = traverse.Field<MagazineItemClass>("MagazineItemClass").Value;
            if (mag != null)
            {
                session.CurrentMag = mag;
                session.CurrentMagTemplateId = mag.TemplateId;
            }

            var ammo = traverse.Field<AmmoItemClass>("AmmoItemClass").Value;
            if (ammo != null)
            {
                session.CurrentAmmo = ammo;
                session.CurrentAmmoTemplateId = ammo.TemplateId;
            }

            // Apply loading speed multiplier to BOTH our session and the actual Class1204 instance.
            // LoadOneAmmoSpeed is seconds-per-bullet: dividing by multiplier makes it faster (mult > 1),
            // multiplying makes it slower (mult < 1). Updating Float_0 and Float_1 inside Class1204
            // synchronizes the actual inventory insertion task loop with the 3D visual animation.
            float loadMult = Plugin.LoadingSpeedMultiplier?.Value ?? 1f;
            if (loadMult > 0f && !Mathf.Approximately(loadMult, 1f))
            {
                session.LoadOneAmmoSpeed /= loadMult;
                traverse.Field<float>("Float_0").Value = session.LoadOneAmmoSpeed;
                traverse.Field<float>("Float_1").Value = session.LoadOneAmmoSpeed;
            }

            LoadAmmoAnimState.OnLoadingStarted(player);
        }

        [PatchPostfix]
        private static void Postfix(Task<IResult> __result)
        {
            var player = Singleton<GameWorld>.Instance?.MainPlayer;
            if (player == null) return;
            // Class1204.Start returns a Task that resolves when the session ends.
            // hook the continuation so LoadingCount decrements no matter how it ends.
            __result?.ContinueWith(_ => LoadAmmoAnimState.OnLoadingEnded(player));
        }
    }

    // method_5 on Class1204 is the per-bullet wait. on the first bullet of a session
    // we extend it by the draw clip duration so the bundle's draw anim has time to
    // finish before a round actually loads. every bullet after gets the normal wait.
    public class Class1204DrawDelayPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var class1204 = AccessTools.Inner(typeof(Player.PlayerInventoryController), "Class1204");
            return AccessTools.Method(class1204, "method_5");
        }

        [PatchPrefix]
        public static bool Prefix(object __instance, ref Task __result)
        {
            var player = Singleton<GameWorld>.Instance?.MainPlayer;
            if (player == null) return true;
            var session = LoadAmmoAnimState.TryGet(player);
            if (session == null) return true;

            if (session.DrawPhasePending)
            {
                session.DrawPhasePending = false;

                // animator runs at 1 / LoadOneAmmoSpeed, so the actual draw-clip duration
                // is DrawClipSeconds * LoadOneAmmoSpeed.
                int normalMs = Mathf.CeilToInt(session.LoadOneAmmoSpeed * 1000f);
                int drawMs   = Mathf.RoundToInt(
                    LoadAmmoAnimDriver.DrawClipSeconds * session.LoadOneAmmoSpeed * 1000f);

                __result = Task.Delay(drawMs + normalMs);
                return false;
            }

            // Subsequent bullets: ensure delay is accurately timed
            float float1 = Traverse.Create(__instance).Field<float>("Float_1").Value;
            int delayMs = Mathf.Max(1, Mathf.CeilToInt(float1 * 1000f));
            __result = Task.Delay(delayMs);
            return false;
        }
    }

    // method_4 on Class1207 is the per-bullet wait for the UNLOAD operation:
    // return Task.Delay(Mathf.CeilToInt(Float_1 * 1000f))  [Player.cs:611]
    // patch divides the delay by UnloadingSpeedMultiplier to speed up / slow down emptying.
    public class Class1207UnloadDelayPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var class1207 = AccessTools.Inner(typeof(Player.PlayerInventoryController), "Class1207");
            return AccessTools.Method(class1207, "method_4");
        }

        [PatchPrefix]
        public static bool Prefix(object __instance, ref Task __result)
        {
            float mult = Plugin.UnloadingSpeedMultiplier?.Value ?? 1f;
            if (mult <= 0f || Mathf.Approximately(mult, 1f)) return true; // passthrough

            float float1 = Traverse.Create(__instance).Field<float>("Float_1").Value;
            int delayMs = Mathf.Max(1, Mathf.CeilToInt(float1 * 1000f / mult));
            __result = Task.Delay(delayMs);
            return false;
        }
    }

    // preload the bundle as soon as the raid starts so the first reload in-raid
    // doesnt hitch waiting for it to load, and clean up any stale sessions.
    public class RaidStartBundleWarmPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(GameWorld).GetMethod("OnGameStarted");

        [PatchPostfix]
        public static void Postfix()
        {
            LoadAmmoAnimState.ClearAllSessions();
            var player = Singleton<GameWorld>.Instance?.MainPlayer;
            if (player != null)
                player.StartCoroutine(LoadAmmoAnimDriver.WarmBundleAsync());
        }
    }

    // every PWA method that runs from LateUpdate eventually derefs WeaponRootAnim
    // with no null check (ApplyPosition, ApplyComplexRotation, AvoidObstacles,
    // LerpCamera, FOV coroutines). guarding ProcessEffectors at the top of the
    // call chain catches almost all of them. costs one frame of skipped weapon-
    // position processing during a controller swap.
    public class ProcessEffectorsNullGuardPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            AccessTools.Method(typeof(EFT.Animations.ProceduralWeaponAnimation), "ProcessEffectors");

        [PatchPrefix]
        public static bool Prefix(EFT.Animations.ProceduralWeaponAnimation __instance)
        {
            if (__instance?.HandsContainer?.WeaponRootAnim == null) return false;
            return true;
        }
    }

    // Player.VisualPass directly reads WeaponRootAnim.localPosition / localRotation
    // (Player.cs lines 1447-1448) AFTER calling ProcessEffectors. patching VisualPass
    // bails before either dereference.
    public class VisualPassNullGuardPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            AccessTools.Method(typeof(Player), "VisualPass");

        [PatchPrefix]
        public static bool Prefix(Player __instance)
        {
            if (__instance?.ProceduralWeaponAnimation?.HandsContainer?.WeaponRootAnim == null)
                return false;
            return true;
        }
    }
}
