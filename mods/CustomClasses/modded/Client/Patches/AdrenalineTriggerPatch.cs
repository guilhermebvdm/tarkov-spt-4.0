using System;
using System.Reflection;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CustomClasses.Client;

/// <summary>
///     🔧 Adrenaline (Fuzileiro) — gatilho. Postfix em <c>Player.ApplyDamageInfo</c>:
///     causar dano (atacante = local) OU receber dano (vítima = local) abre/renova a janela da Adrenaline.
///     Os efeitos (recuo/recarga/ADS) leem <c>AdrenalineState.IsActive</c> nos respectivos patches.
/// </summary>
internal class AdrenalineTriggerPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.ApplyDamageInfo));
    }

    [PatchPostfix]
    private static void Postfix(Player __instance, DamageInfoStruct damageInfo)
    {
        try
        {
            if (PerksConfig.AdrenalineEnabled?.Value != true || !SkillMultipliers.IsLocalClass("Rifleman"))
            {
                return;
            }

            var mp = Singleton<GameWorld>.Instance?.MainPlayer;
            if (mp == null)
            {
                return;
            }

            // Receber dano (vítima = local) OU causar dano (atacante = local).
            // damageInfo.Player é IPlayerOwner (wrapper) → comparar pelo ProfileId do iPlayer.
            var dealt = damageInfo.Player?.iPlayer != null
                        && damageInfo.Player.iPlayer.ProfileId == mp.ProfileId;
            if (ReferenceEquals(__instance, mp) || dealt)
            {
                AdrenalineState.Trigger();
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] adrenaline trigger falhou: {ex.Message}");
        }
    }
}
