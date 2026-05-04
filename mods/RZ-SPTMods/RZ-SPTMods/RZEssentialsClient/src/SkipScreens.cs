// RemzDNB - 2026

using System.Reflection;
using EFT;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using EFT.UI.Matchmaker;
using EFT.UI.SessionEnd;
using SPT.Reflection.Patching;

namespace RZEssentialsClient;

public static class SkipScreensPatches
{
    public static void Enable()
    {
        new SkipSideSelectionScreenPatch().Enable();
        new SkipInsuranceScreenPatch().Enable();
        new SkipRaidSettingsScreenPatch().Enable();
        new SkipExperienceScreenPatch().Enable();
    }

    public class SkipSideSelectionScreenPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MatchMakerSideSelectionScreen).GetMethod("Show", BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { typeof(ISession), typeof(RaidSettings), typeof(IHealthController), typeof(InventoryController) },
                null);
        }

        [PatchPostfix]
        public static void Postfix(MatchMakerSideSelectionScreen __instance)
        {
            if (!ClientConfig.Instance.SkipSideSelectionScreen) 
                return;

            var current = CurrentScreenSingletonClass.Instance.CurrentBaseScreenController;
            if (current != null)
                current.Disabled = true;

            typeof(MatchMakerSideSelectionScreen)
                .GetMethod("method_18", BindingFlags.Public | BindingFlags.Instance)
                ?.Invoke(__instance, null);
        }
    }

    public class SkipInsuranceScreenPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MatchmakerInsuranceScreen).GetMethod("Show", BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { typeof(MatchmakerInsuranceScreen.GClass3913) },
                null);
        }

        [PatchPostfix]
        public static void Postfix(MatchmakerInsuranceScreen __instance)
        {
            if (!ClientConfig.Instance.SkipInsuranceScreen) 
                return;

            var current = CurrentScreenSingletonClass.Instance.CurrentBaseScreenController;
            if (current != null)
                current.Disabled = true;

            typeof(MatchmakerInsuranceScreen)
                .GetMethod("method_9", BindingFlags.Public | BindingFlags.Instance)
                ?.Invoke(__instance, null);
        }
    }

    public class SkipRaidSettingsScreenPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(MatchmakerOfflineRaidScreen).GetMethod("Show", BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { typeof(MatchmakerOfflineRaidScreen.CreateRaidSettingsForProfileClass) },
                null);
        }

        [PatchPostfix]
        public static void Postfix(MatchmakerOfflineRaidScreen __instance)
        {
            if (!ClientConfig.Instance.SkipRaidSettingsScreen) 
                return;

            var current = CurrentScreenSingletonClass.Instance.CurrentBaseScreenController;
            if (current != null)
                current.Disabled = true;

            typeof(MatchmakerOfflineRaidScreen)
                .GetMethod("method_5", BindingFlags.Public | BindingFlags.Instance)
                ?.Invoke(__instance, null);
        }
    }

    public class SkipExperienceScreenPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(SessionResultExperienceCount).GetMethod("Show", BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { typeof(SessionResultExperienceCount.GClass3909) },
                null);
        }

        [PatchPostfix]
        public static void Postfix(SessionResultExperienceCount __instance)
        {
            if (!ClientConfig.Instance.SkipExperienceScreen) 
                return;

            typeof(SessionResultExperienceCount)
                .GetMethod("method_3", BindingFlags.Public | BindingFlags.Instance)
                ?.Invoke(__instance, null);
        }
    }
}