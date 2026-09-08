using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace UIFixes;

public static class ReloadInPlacePatches
{
    private static bool IsReloading = false;
    private static MagazineItemClass FoundMagazine = null;
    private static ItemAddress FoundAddress = null;

    public static void Enable()
    {
        // These patch ItemUiContext.ReloadWeapon, which is called from the context menu Reload
        new ReloadInPlacePatch().Enable();
        new ReloadInPlaceFindMagPatch().Enable();
        new ReloadInPlaceFindSpotPatch().Enable();
        new AlwaysSwapPatch().Enable();

        // This patches the firearmsController code when you hit R in raid with an external magazine class
        new SwapIfNoSpacePatch().Enable();
        //new InsertMagDebugPatch().Enable();
    }

    public class ReloadInPlacePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ItemUiContext), nameof(ItemUiContext.ReloadWeapon));
        }

        [PatchPrefix]
        public static void Prefix()
        {
            IsReloading = Settings.SwapMags.Value;
        }

        [PatchPostfix]
        public static void Postfix()
        {
            IsReloading = false;
            FoundMagazine = null;
            FoundAddress = null;
        }
    }

    public class ReloadInPlaceFindMagPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ItemUiContext), nameof(ItemUiContext.method_18));
        }

        [PatchPostfix]
        public static void Postfix(MagazineItemClass __result)
        {
            if (__result != null && IsReloading)
            {
                FoundMagazine = __result;
                FoundAddress = FoundMagazine.Parent;
            }
        }
    }

    public class ReloadInPlaceFindSpotPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            Type type = typeof(ItemUiContext).GetNestedTypes().Single(t => t.GetField("currentMagazine") != null); // ItemUiContext.Class2775
            return AccessTools.Method(type, "method_0");
        }

        [PatchPrefix]
        public static void Prefix(StashGridClass grid, ref GStruct154<RemoveOperation> __state)
        {
            if (!Settings.SwapMags.Value)
            {
                return;
            }

            if (grid.Contains(FoundMagazine))
            {
                __state = InteractionsHandlerClass.Remove(FoundMagazine, grid.ParentItem.Owner as TraderControllerClass, false);
            }
        }

        [PatchPostfix]
        public static void Postfix(GStruct154<RemoveOperation> __state)
        {
            if (!Settings.SwapMags.Value || __state.Value == null)
            {
                return;
            }

            if (__state.Succeeded)
            {
                __state.Value.RollBack();
            }
        }
    }

    public class AlwaysSwapPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            Type type = typeof(ItemUiContext).GetNestedTypes().Single(t => t.GetField("func_3") != null); // ItemUiContext.Class2755
            return AccessTools.Method(type, "method_5");
        }

        [PatchPostfix]
        public static void Postfix(GridItemAddress g, ref int __result)
        {
            if (!Settings.AlwaysSwapMags.Value)
            {
                return;
            }

            if (!g.Equals(FoundAddress))
            {
                // Addresses that aren't the found address get massive value increase so found address is sorted first
                __result += 1000;
            }
        }
    }

    public class SwapIfNoSpacePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            if (Plugin.FikaPresent())
            {
                Type type = Type.GetType("Fika.Core.Main.ClientClasses.HandsControllers.FikaClientFirearmController, Fika.Core");
                return AccessTools.Method(type, "ReloadMag");
            }

            return AccessTools.Method(typeof(Player.FirearmController), nameof(Player.FirearmController.ReloadMag));
        }

        // By default this method will do a series of removes and adds, but not swap, to reload.
        // If there's no space in the rig/pockets, find the spot that the new magazine is currently occupying
        // and assign it to itemAddress so the native/Fika reload puts the current magazine there.
        [PatchPrefix]
        public static bool Prefix(Player.FirearmController __instance, MagazineItemClass magazine, ref ItemAddress itemAddress, Callback callback, Player ____player)
        {
            // TEMP DIAG (item 001) — remove after root cause confirmed in-game
            Plugin.Instance.Logger.LogWarning($"[UIFixes-Diag] SwapIfNoSpacePatch.Prefix ENTER — SwapMags={Settings.SwapMags.Value} AlwaysSwapMags={Settings.AlwaysSwapMags.Value} itemAddress={(itemAddress == null ? "null" : itemAddress.GetType().Name)} isAI={____player?.IsAI}");

            // If itemAddress isn't null, it already found a place for the current mag, so let it run (unless always swap is enabled)
            if (!Settings.SwapMags.Value || (itemAddress != null && !Settings.AlwaysSwapMags.Value))
            {
                Plugin.Instance.Logger.LogWarning("[UIFixes-Diag] bail: SwapMags off, or itemAddress already set by vanilla and AlwaysSwapMags off");
                return true;
            }

            if (____player.IsAI)
            {
                Plugin.Instance.Logger.LogWarning("[UIFixes-Diag] bail: player IsAI");
                return true;
            }

            if (__instance.Blindfire)
            {
                Plugin.Instance.Logger.LogWarning("[UIFixes-Diag] bail: Blindfire");
                return false;
            }

            // Weapon doesn't currently have a magazine, let the default run (will load one)
            MagazineItemClass currentMagazine = __instance.Weapon.GetCurrentMagazine();
            if (currentMagazine == null)
            {
                Plugin.Instance.Logger.LogWarning("[UIFixes-Diag] bail: weapon has no current magazine");
                return true;
            }

            InventoryController controller = __instance.Weapon.Owner as InventoryController;
            if (controller == null)
            {
                Plugin.Instance.Logger.LogWarning($"[UIFixes-Diag] bail: Weapon.Owner is not InventoryController (actual={__instance.Weapon.Owner?.GetType().Name ?? "null"})");
                return true;
            }

            ItemAddress magAddress = magazine.Parent;
            Plugin.Instance.Logger.LogWarning($"[UIFixes-Diag] magAddress type={magAddress?.GetType().Name ?? "null"} currentMagazine={currentMagazine.TemplateId}");

            // Null address means it couldn't find a spot. Try to remove magazine (temporarily) and find where the old mag fits
            var operation = InteractionsHandlerClass.Remove(magazine, controller, false);
            if (operation.Failed)
            {
                Plugin.Instance.Logger.LogWarning($"[UIFixes-Diag] bail: temp Remove(magazine) failed — {operation.Error?.GetType().Name}");
                return true;
            }

            ItemAddress candidateAddress = controller.Inventory.Equipment.GetPrioritizedGridsForUnloadedObject(false)
                .Select(grid => grid.FindLocationForItem(currentMagazine))
                .Where(address => address != null)
                .OrderByDescending(address => Settings.AlwaysSwapMags.Value && address.Equals(magAddress)) // Prioritize swapping slot if desired
                .ThenBy(address => address.Grid.GridWidth * address.Grid.GridHeight)
                .FirstOrDefault();

            Plugin.Instance.Logger.LogWarning($"[UIFixes-Diag] primary search candidateAddress={(candidateAddress == null ? "null" : candidateAddress.GetType().Name)}");

            // ref: CR-01-01 — GetPrioritizedGridsForUnloadedObject(false) never considers the backpack, so if
            // the vest/pockets are full and the new magazine came from the backpack, the spot it just vacated
            // is invisible to the search above. Fallback: only runs when the search above already failed, so
            // it never changes the existing priority/behavior.
            if (candidateAddress == null && magAddress is GridItemAddress gridMagAddress)
            {
                candidateAddress = gridMagAddress.Grid.FindLocationForItem(currentMagazine);
                Plugin.Instance.Logger.LogWarning($"[UIFixes-Diag] fallback (vacated-slot) candidateAddress={(candidateAddress == null ? "null" : candidateAddress.GetType().Name)}");
            }
            else if (candidateAddress == null)
            {
                Plugin.Instance.Logger.LogWarning($"[UIFixes-Diag] fallback SKIPPED — magAddress is not GridItemAddress (actual={magAddress?.GetType().Name ?? "null"})");
            }

            // Put the magazine back
            operation.Value.RollBack();

            if (candidateAddress == null)
            {
                Plugin.Instance.Logger.LogWarning("[UIFixes-Diag] EXIT — no candidate anywhere, letting native run (will drop mag on ground)");
                // Nowhere to put the old magazine at all. Let native run (will drop it on the ground).
                return true;
            }

            // ref: same-slot swap discovery — when candidateAddress is exactly the spot the NEW magazine
            // currently occupies (the only possibility once rig/pockets are 100% full), the native
            // ReloadMag pipeline (GClass2006.Run) can never place the old magazine there: it moves the
            // old magazine into vestTargetAddress BEFORE moving the new magazine out of that same spot,
            // so the Move always collides with itself (GridSpaceTakenError) and the whole reload silently
            // aborts. A true atomic Swap (both items exchange positions in one operation) is required
            // instead — same mechanism the drag-and-drop weapon-apply swap already uses successfully
            // under Fika (SwapPatches.cs WeaponApplyPatch, relying on FIKA's ObservedInventoryController
            // self-referential-magazine-swap grace window to avoid GClass1561 on Headless).
            if (candidateAddress.Equals(magAddress))
            {
                Plugin.Instance.Logger.LogWarning("[UIFixes-Diag] same-slot case detected — using atomic Swap instead of native Move+Move");

                // ref: native Player.FirearmController.ReloadMag (and FikaClientFirearmController.ReloadMag)
                // both call RemoveLeftHandItem(3f) + ForceStopInteractions() as the very first thing they do,
                // before touching any item — that's the left-hand-releases-the-grip animation/pacing every
                // reload plays. Since we return false below and skip that method entirely, we have to
                // replicate this ourselves (same as the original upstream Swap-based Prefix did) or the swap
                // happens instantly with no animation at all.
                ____player.RemoveLeftHandItem(3f);
                ____player.MovementContext.PlayerAnimator.AnimatedInteractions.ForceStopInteractions();
                if (____player.MovementContext.PlayerAnimator.AnimatedInteractions.IsInteractionPlaying)
                {
                    Plugin.Instance.Logger.LogWarning("[UIFixes-Diag] bail: interaction already playing after ForceStopInteractions");
                    return false;
                }

                if (!__instance.CanStartReload())
                {
                    Plugin.Instance.Logger.LogWarning("[UIFixes-Diag] bail: CanStartReload false");
                    callback?.Fail("Cant StartReload");
                    return false;
                }

                ItemAddress weaponMagSlotAddress = __instance.Weapon.GetMagazineSlot().CreateItemAddress();
                var swapResult = InteractionsHandlerClass.Swap(magazine, weaponMagSlotAddress, currentMagazine, candidateAddress, controller, true);

                Plugin.Instance.Logger.LogWarning($"[UIFixes-Diag] Swap result: Succeeded={swapResult.Succeeded} Error={swapResult.Error?.GetType().Name}");

                controller.TryRunNetworkTransaction(swapResult, callback);

                // Skip the native ReloadMag entirely for this case — we already performed and networked the swap.
                return false;
            }

            // Found a genuinely different free spot (not occupied by the new magazine) — safe for the
            // native Move+Move sequence, since the new magazine's slot isn't the target.
            itemAddress = candidateAddress;

            Plugin.Instance.Logger.LogWarning($"[UIFixes-Diag] EXIT — different free spot found, letting native run with itemAddress={itemAddress.GetType().Name}");

            // Return true so the official Player.FirearmController / FikaClientFirearmController.ReloadMag runs!
            // This allows the full hands animation to play, Fika's ReloadMagPacket to be sent to the Headless/Host,
            // and completely prevents WaitingForCallback deadlocks and blinking magazines.
            return true;
        }
    }

    // Dumps the animator parameters, lol. Desparate times and all that
    public class InsertMagDebugPatch : ModulePatch
    {
        private static FieldInfo ParameterListField;

        protected override MethodBase GetTargetMethod()
        {
            ParameterListField = AccessTools.Field(typeof(AnimatorWrapper), "animatorControllerParameter_0");
            return AccessTools.DeclaredMethod(typeof(FirearmInsertedMagState), nameof(FirearmInsertedMagState.Start));
        }

        [PatchPrefix]
        public static void Prefix(FirearmsAnimator ___firearmsAnimator_0)
        {
            StringBuilder sb = new();
            if (___firearmsAnimator_0.Animator is AnimatorWrapper animator)
            {
                for (int i = 0; i < animator.parameterCount; i++)
                {
                    AnimatorParameterInfo paramInfo = animator.GetParameter(i);
                    string name = animator.GetParameterName(paramInfo.nameHash);
                    string value = paramInfo.type switch
                    {
                        AnimatorControllerParameterType.Bool => animator.GetBool(paramInfo.nameHash).ToString(),
                        AnimatorControllerParameterType.Float => animator.GetFloat(paramInfo.nameHash).ToString(),
                        AnimatorControllerParameterType.Int => animator.GetInteger(paramInfo.nameHash).ToString(),
                        _ => "Unknown",
                    };

                    sb.AppendLine($"{name} ({paramInfo.type}) = {value}");
                }

                Plugin.Instance.Logger.LogInfo(sb.ToString());
            }
        }
    }
}