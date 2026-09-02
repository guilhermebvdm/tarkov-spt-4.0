using System.Reflection;
using HarmonyLib;
using SAIN.Components;
using SAIN.Preset.GlobalSettings;
using SPT.Reflection.Patching;

namespace SAIN.Patches.Shoot.Grenades;

public class SetGrenadePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotGrenadeController), nameof(BotGrenadeController.method_3));
    }

    [PatchPostfix]
    public static void Patch(ThrowWeapItemClass potentialGrenade, BotGrenadeController __instance)
    {
        if (potentialGrenade == null)
        {
            return;
        }
        // ref: AUD-05-03 - Acesso defensivo padronizado via SAINEnableClass
        if (!SAINEnableClass.GetSAIN(__instance.BotOwner_0?.ProfileId, out var botComponent))
        {
            return;
        }
        //__instance.Mass = potentialGrenade.Weight;
        botComponent.Grenade.MyGrenade = potentialGrenade;
    }
}

public class ResetGrenadePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotGrenadeController), nameof(BotGrenadeController.method_2));
    }

    [PatchPostfix]
    public static void Patch(BotGrenadeController __instance)
    {
        // ref: AUD-05-03 - Acesso defensivo padronizado via SAINEnableClass
        if (!SAINEnableClass.GetSAIN(__instance.BotOwner_0?.ProfileId, out var botComponent))
        {
            return;
        }
        botComponent.Grenade.MyGrenade = __instance.Grenade;
    }
}

public class DisableGrenadesPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(BotGrenadeController), nameof(BotGrenadeController.HaveGrenade));
    }

    [PatchPostfix]
    public static void Patch(BotGrenadeController __instance, ref bool __result)
    {
        if (!__result)
        {
            return;
        }

        var settings = GlobalSettingsClass.Instance.General;
        if (!settings.BotsUseGrenades)
        {
            __result = false;
            return;
        }

        // ref: AUD-17-02 - Null-safety em BotOwner_0
        if (SAINEnableClass.GetSAIN(__instance.BotOwner_0?.ProfileId, out BotComponent bot))
        {
            if (!bot.Info.FileSettings.Core.CanGrenade)
            {
                __result = false;
                return;
            }

            var goalEnemy = bot.EnemyController.GoalEnemy;
            if (goalEnemy == null)
            {
                __result = false;
                return;
            }

            if (!settings.BotVsBotGrenade && goalEnemy.IsAI)
            {
                __result = false;
            }
        }
    }
}
