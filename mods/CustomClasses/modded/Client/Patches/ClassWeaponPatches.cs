using System;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.Animations;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CustomClasses.Client;

/// <summary>
///     Item 050.2 — recuo da arma por classe.
///     Prefix em <c>ProceduralWeaponAnimation.Shoot(str)</c> — <c>str</c> escala linearmente a força do recuo do tiro
///     (mãos, rotação de mãos e rotação de câmera). Gating: só o PWA do MainPlayer local. F12 no apply-time.
///     Cobre agora: 🔻 Shaky Hands (Médico recuo ×1.25). Adrenaline recuo ×0.7 entra aqui com a state-machine.
/// </summary>
internal class ShootRecoilPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(ProceduralWeaponAnimation), nameof(ProceduralWeaponAnimation.Shoot));
    }

    [PatchPrefix]
    private static void Prefix(ProceduralWeaponAnimation __instance, ref float str)
    {
        try
        {
            var p = Singleton<GameWorld>.Instance?.MainPlayer;
            if (p == null || !ReferenceEquals(__instance, p.ProceduralWeaponAnimation))
            {
                return;   // só a arma do player local
            }

            var str0 = str;   // (052) baseline p/ o diagnóstico

            // 🔻 Shaky Hands (Médico): +25% de recuo (mãos trêmulas).
            if (PerksConfig.ShakyHandsEnabled?.Value == true && SkillMultipliers.IsLocalClass("Combat Medic"))
            {
                str *= PerksConfig.ShakyHandsRecoil?.Value ?? 1f;
            }

            // 🔧 Adrenaline (Fuzileiro): −30% de recuo durante a janela.
            if (PerksConfig.AdrenalineEnabled?.Value == true && AdrenalineState.IsActive
                && SkillMultipliers.IsLocalClass("Rifleman"))
            {
                str *= PerksConfig.AdrenalineRecoil?.Value ?? 1f;
            }

            // 🔧 Bunker (Tanque): −15% de recuo com arma pesada (LMG/HMG/GL/underbarrel) na mão.
            if (PerksConfig.BunkerEnabled?.Value == true && SkillMultipliers.IsLocalClass("Tank")
                && HeavyWeapon.InHand(p))
            {
                str *= PerksConfig.BunkerHeavyRecoil?.Value ?? 1f;
            }

            if (PerkDiag.Enabled)
            {
                PerkDiag.RecoilBefore = str0;
                PerkDiag.RecoilAfter = str;
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] recoil falhou: {ex.Message}");
        }
    }
}

/// <summary>
///     🔧 Adrenaline (Fuzileiro) — recarga mais rápida durante a janela.
///     Postfix em <c>FirearmController.GetWeaponReloadAnimationSpeed</c> (a speed do animator de reload):
///     tempo ×0.8 ⇒ speed ÷0.8 (mais rápido). Gate: a arma atual do MainPlayer.
/// </summary>
internal class ReloadSpeedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player.FirearmController), nameof(Player.FirearmController.GetWeaponReloadAnimationSpeed));
    }

    [PatchPostfix]
    private static void Postfix(Player.FirearmController __instance, ref float __result)
    {
        try
        {
            if (PerksConfig.AdrenalineEnabled?.Value != true || !AdrenalineState.IsActive
                || !SkillMultipliers.IsLocalClass("Rifleman"))
            {
                return;
            }

            if (!ReferenceEquals(__instance, Singleton<GameWorld>.Instance?.MainPlayer?.HandsController))
            {
                return;   // só a arma do player local
            }

            var t = PerksConfig.AdrenalineReloadTime?.Value ?? 1f;
            if (t > 0f && t < 1f)
            {
                __result /= t;   // tempo ×0.8 → speed ÷0.8 (recarrega mais rápido)
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] adrenaline reload falhou: {ex.Message}");
        }
    }
}

/// <summary>
///     🔧 Adrenaline (Fuzileiro) — ADS mais rápido durante a janela.
///     Postfix em <c>ProceduralWeaponAnimation.UpdateWeaponVariables</c> escrevendo o campo privado
///     <c>_aimingSpeed</c> (mult. de todas as lerps de mira): tempo ×0.8 ⇒ aimingSpeed ÷0.8 (mira mais rápido).
/// </summary>
internal class AdsSpeedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(ProceduralWeaponAnimation), nameof(ProceduralWeaponAnimation.UpdateWeaponVariables));
    }

    [PatchPostfix]
    private static void Postfix(ProceduralWeaponAnimation __instance, ref float ____aimingSpeed)
    {
        try
        {
            if (!ReferenceEquals(__instance, Singleton<GameWorld>.Instance?.MainPlayer?.ProceduralWeaponAnimation))
            {
                return;   // só a arma do player local
            }

            // 🔧 Adrenaline (Fuzileiro): ADS mais rápido durante a janela.
            if (PerksConfig.AdrenalineEnabled?.Value == true && AdrenalineState.IsActive
                && SkillMultipliers.IsLocalClass("Rifleman"))
            {
                var t = PerksConfig.AdrenalineAdsTime?.Value ?? 1f;
                if (t > 0f && t < 1f)
                {
                    ____aimingSpeed /= t;   // tempo ×0.8 → aimingSpeed ÷0.8 (mira mais rápido)
                }
            }

            // 🔧 Sharpshooter (Caçador): ADS mais rápido (sempre).
            if (PerksConfig.SharpshooterEnabled?.Value == true && SkillMultipliers.IsLocalClass("Hunter"))
            {
                var t = PerksConfig.SharpshooterAdsTime?.Value ?? 1f;
                if (t > 0f && t < 1f)
                {
                    ____aimingSpeed /= t;
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] ADS speed falhou: {ex.Message}");
        }
    }
}

/// <summary>
///     🔧 Bunker (Tanque) — +15% de ergonomia com arma pesada (LMG/HMG/lança-granadas/underbarrel) na mão.
///     Postfix no getter <c>FirearmController.TotalErgonomics</c> (funil real de ergo da arma, lido por
///     recoil/handling/sway). Gate: a arma atual do MainPlayer + classe Tank + arma pesada.
/// </summary>
internal class HeavyWeaponErgoPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(Player.FirearmController), nameof(Player.FirearmController.TotalErgonomics));
    }

    [PatchPostfix]
    private static void Postfix(Player.FirearmController __instance, ref float __result)
    {
        try
        {
            if (PerksConfig.BunkerEnabled?.Value != true)
            {
                return;
            }

            if (!ReferenceEquals(__instance, Singleton<GameWorld>.Instance?.MainPlayer?.HandsController))
            {
                return;   // só a arma do player local
            }

            if (SkillMultipliers.IsLocalClass("Tank") && HeavyWeapon.IsHeavy(__instance.Item))
            {
                __result *= PerksConfig.BunkerHeavyErgo?.Value ?? 1f;
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] bunker ergo falhou: {ex.Message}");
        }
    }
}

/// <summary>Detecção de arma pesada (LMG/HMG = weapClass "machinegun"; lança-granadas; underbarrel acoplado).</summary>
internal static class HeavyWeapon
{
    internal static bool IsHeavy(Weapon? w)
    {
        if (w == null)
        {
            return false;
        }

        // weapClass: "machinegun" = LMG+HMG · "grenadeLauncher" = lança-granadas standalone.
        // (underbarrel acoplado tipo GP-25 = follow-up — Weapon não expõe um flag simples no client.)
        var wc = w.WeapClass;
        return wc == "machinegun" || wc == "grenadeLauncher";
    }

    internal static bool InHand(Player? p)
    {
        return IsHeavy((p?.HandsController as Player.FirearmController)?.Item);
    }
}

/// <summary>
///     Tranco de câmera/mãos ao LEVAR dano. Prefix em <c>ForceEffector.AddForce(strength, hands, camera)</c>
///     (overload sem Vector3 = jolt de hit), gateado ao <c>ForceReact</c> do PWA do MainPlayer (única origem
///     desse efeito no cliente além do hit de faca). Dois branches independentes:
///     🔻 Rattled (Furtivo) ×1.5 (mais tranco) · 🔧 Cool Under Fire (Fuzileiro) ×0.5 (firme sob fogo, menos flinch).
///     NB: re-escopo do Cool Under Fire — o EFT 0.16.9 não tem efeito de SUPRESSÃO/near-miss no cliente; o perk
///     passou a atenuar o flinch de hit (decisão do usuário, 2026-06-23).
/// </summary>
internal class AimPunchPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(
            typeof(ForceEffector), nameof(ForceEffector.AddForce),
            new[] { typeof(float), typeof(float), typeof(float) });
    }

    [PatchPrefix]
    private static void Prefix(ForceEffector __instance, ref float strength)
    {
        try
        {
            var fr = Singleton<GameWorld>.Instance?.MainPlayer?.ProceduralWeaponAnimation?.ForceReact;
            if (fr == null || !ReferenceEquals(__instance, fr))
            {
                return;   // só o tranco do player local
            }

            // (review fix 2026-06-24) só aplica se houve dano de COMBATE recente (ApplyDamageInfo). Dano de QUEDA
            // não passa por ApplyDamageInfo → timestamp velho → não dispara. Janela curta = mesmo frame do hit.
            if (UnityEngine.Time.time - LocalHitTypePatch.LastCombatHitTime > 0.15f)
            {
                return;
            }

            // 🔻 Rattled (Furtivo): +50% no tranco de câmera ao levar dano.
            if (PerksConfig.RattledEnabled?.Value == true && SkillMultipliers.IsLocalClass("Stealth"))
            {
                strength *= PerksConfig.RattledAimPunch?.Value ?? 1f;
            }

            // 🔧 Cool Under Fire (Fuzileiro): −50% no tranco ao levar dano (firme sob fogo).
            if (PerksConfig.CoolUnderFireEnabled?.Value == true && SkillMultipliers.IsLocalClass("Rifleman"))
            {
                strength *= PerksConfig.CoolUnderFireFlinch?.Value ?? 1f;
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] aim-punch falhou: {ex.Message}");
        }
    }
}

/// <summary>
///     (review fix 2026-06-24) Captura o tipo do último dano recebido pelo player LOCAL, pra o
///     <c>AimPunchPatch</c> (Rattled/Cool Under Fire) NÃO disparar em dano de QUEDA. Prefix em
///     <c>Player.ApplyDamageInfo</c> — roda antes do <c>EffectsController</c>→<c>ForceEffector.AddForce</c>.
/// </summary>
internal class LocalHitTypePatch : ModulePatch
{
    // Marca o instante do último dano de COMBATE (que passa por Player.ApplyDamageInfo). Dano de QUEDA NÃO passa
    // por aqui — vai por ActiveHealthController.ApplyDamage (review 2026-06-24) → o timestamp fica velho → o
    // aim-punch de queda é barrado por RECÊNCIA no AimPunchPatch (o AddForce de combate é síncrono ao ApplyDamageInfo).
    internal static float LastCombatHitTime = -999f;

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.ApplyDamageInfo));
    }

    [PatchPrefix]
    private static void Prefix(Player __instance)
    {
        if (ReferenceEquals(__instance, Singleton<GameWorld>.Instance?.MainPlayer))
        {
            LastCombatHitTime = UnityEngine.Time.time;
        }
    }
}
