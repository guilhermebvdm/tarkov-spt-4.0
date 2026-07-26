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

            // 🔻 Falta de habilidade / Unskilled (Médico + Saqueador — 079): +25% de recuo por falta de perícia.
            if (PerksConfig.ShakyHandsEnabled?.Value == true
                && (SkillMultipliers.IsLocalClass("Combat Medic") || SkillMultipliers.IsLocalClass("Scavenger")))
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
    private static void Prefix(ForceEffector __instance, ref float hands, ref float camera)
    {
        try
        {
            var fr = Singleton<GameWorld>.Instance?.MainPlayer?.ProceduralWeaponAnimation?.ForceReact;
            if (fr == null || !ReferenceEquals(__instance, fr))
            {
                return;   // só o tranco do player local (075: gate de instância — ForceReact do MainPlayer)
            }

            // (review fix 2026-06-24) só aplica se houve dano de COMBATE recente (ApplyDamageInfo). Dano de QUEDA
            // não passa por ApplyDamageInfo → timestamp velho → não dispara. Janela curta = mesmo frame do hit.
            if (UnityEngine.Time.time - LocalHitTypePatch.LastCombatHitTime > 0.15f)
            {
                return;
            }

            // 074/F6 (2026-07-18, auditoria de eficácia): o multiplicador vai em HANDS/CAMERA, NÃO no STRENGTH.
            // A aceleração do tranco = direction × camera × WiggleMagnitude × Clamp01(strength) (ForceEffector.
            // AddForce): o Clamp01 SÓ morde o strength, então o ×1.5 do Rattled nele SATURAVA em hits fortes
            // (parcialmente inerte). hands/camera (0.05–1.3 por body-part, EffectsController:1465-1481) NÃO são
            // clampados → escalá-los entrega o ±% CHEIO em todo hit (e o Cool Under Fire passa a reduzir o flinch
            // até em hits enormes, que antes já saturavam). Rattled/Cool Under Fire são classes mutuamente exclusivas.
            var factor = 1f;
            // 🔻 Rattled / Abalado (Furtivo + Médico — 079): +50% tranco ao levar dano. Mesmo lever/valor p/ as 2 classes.
            if (PerksConfig.RattledEnabled?.Value == true
                && (SkillMultipliers.IsLocalClass("Stealth") || SkillMultipliers.IsLocalClass("Combat Medic")))
            {
                factor = PerksConfig.RattledAimPunch?.Value ?? 1f;
            }
            else if (PerksConfig.CoolUnderFireEnabled?.Value == true && SkillMultipliers.IsLocalClass("Rifleman"))
            {
                factor = PerksConfig.CoolUnderFireFlinch?.Value ?? 1f;      // 🔧 Fuzileiro: −50% tranco
            }

            hands *= factor;
            camera *= factor;
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] aim-punch falhou: {ex.Message}");
        }
    }
}

/// <summary>
///     080 — 🔫 <b>Saque Rápido</b> (Caçador + Fuzileiro + Furtivo): sacar a arma do slot <b>HOLSTER</b> mais rápido.
///     Postfix em <c>Player.FirearmController.GetWeaponDrawSpeedMultiplier</c> (Player.cs:12591) — o análogo do
///     <c>GetWeaponReloadAnimationSpeed</c> p/ o SAQUE (retorna a VELOCIDADE do parâmetro <c>draw</c> do animator;
///     maior = mais rápido). Gate: só o HandsController do MainPlayer local + classe + a arma sacada vem do slot
///     Holster (padrão canônico do EFT, Player.cs:12637). <c>__result /= tempo</c> (tempo 0.8 → speed ×1.25).
/// </summary>
internal class HolsterDrawSpeedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player.FirearmController), nameof(Player.FirearmController.GetWeaponDrawSpeedMultiplier));
    }

    [PatchPostfix]
    private static void Postfix(Player.FirearmController __instance, Weapon weapon, ref float __result)
    {
        try
        {
            if (PerksConfig.QuickDrawEnabled?.Value != true)
            {
                return;
            }

            var mainPlayer = Singleton<GameWorld>.Instance?.MainPlayer;
            if (mainPlayer == null || !ReferenceEquals(__instance, mainPlayer.HandsController))
            {
                return;   // só a arma do player local
            }

            if (!(SkillMultipliers.IsLocalClass("Hunter")
                  || SkillMultipliers.IsLocalClass("Rifleman")
                  || SkillMultipliers.IsLocalClass("Stealth")))
            {
                return;
            }

            // Só quando a arma sacada vem do slot HOLSTER (padrão canônico do EFT — Player.cs:12637).
            // CurrentAddress é o accessor SEGURO (o getter .Parent pode lançar).
            var holster = mainPlayer.Inventory?.Equipment?.GetSlot(EquipmentSlot.Holster);
            var container = weapon?.CurrentAddress?.Container;
            if (holster == null || container == null || !ReferenceEquals(container, holster))
            {
                return;
            }

            var t = PerksConfig.QuickDrawTime?.Value ?? 1f;   // TEMPO de saque (0.8 = 20% mais rápido)
            if (t > 0f && t < 1f)
            {
                __result /= t;   // speed maior = saque mais rápido
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (080) quick draw falhou: {ex.Message}");
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

/// <summary>
///     084 — 🔫 <b>Recarga Rápida Escopeta</b> (Tanque): acelera a recarga de escopetas de TUBO (shell-a-shell). A
///     mecânica elite "2 cartuchos por vez" (Mag Drills) NÃO existe no EFT 0.16.9 — o fallback do épico é reduzir o
///     TEMPO. Como a recarga tubular é 100% dirigida por eventos de animação (cada shell = 1 keyframe), acelerar a
///     animação de reload acelera a cadência shell-a-shell na mesma proporção.
///     <para>
///     Alvo: <c>FirearmController.SetAnimatorAndProceduralValues()</c> — o funil REAL que empurra o reload speed
///     (lê o CAMPO <c>BuffInfo.ReloadSpeed</c> direto e o repassa a DOIS animators em lockstep: o da ARMA
///     (<c>FirearmsAnimator</c>) e o do CORPO (<c>MovementContext.PlayerAnimator</c>). ⚠️ O getter
///     <c>GetWeaponReloadAnimationSpeed()</c> é CÓDIGO MORTO no 0.16.9 (nada o chama), então o molde do
///     <see cref="ReloadSpeedPatch"/> (Postfix no getter) não serve — este ponto é o funil de fato.
///     </para>
///     <para>
///     <b>Estratégia (code-review CR-084):</b> em vez de re-setar SÓ o animator da arma num Postfix (o que
///     dessincronizaria mãos×corpo, pois o base atualiza os dois), o <b>Prefix ESCALA o campo</b>
///     <c>BuffInfo.ReloadSpeed ÷ t</c> ANTES do método rodar → os DOIS animators recebem o valor já acelerado, em
///     lockstep, sem tocar no draw/swap (a branch de quickdraw-fast preserva o próprio <c>draw</c>). O <b>Postfix
///     RESTAURA</b> o valor original (via <c>__state</c>) → não acumula entre syncs nem vaza para outros consumidores
///     do campo. Persiste enquanto a escopeta estiver em mãos (o método roda no saque/sync/início de reload).
///     </para>
///     <para>Gate: MainPlayer local (075) + Tank + <c>WeapClass=="shotgun"</c> + <c>Weapon.SupportsInternalReload</c>.
///     ⚠️ O <c>SupportsInternalReload</c> sozinho pega bolt-action (Mosin), SKS, revólveres e a M32 (todos
///     <c>InternalMagazine</c>) — o <c>WeapClass=="shotgun"</c> restringe às 8 escopetas de tubo (MR-133/153, M870,
///     KS-23M, 590A1, MP-155, MTs-255, Benelli M3). Saiga (<c>ExternalMagazine</c>) e bicano (<c>OnlyBarrel</c>) ficam
///     de fora corretamente.</para>
/// </summary>
internal class ShotgunReloadPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player.FirearmController), "SetAnimatorAndProceduralValues");
    }

    // __state = NaN → "não escalei" (Postfix não restaura). Senão = o ReloadSpeed original a restaurar.
    [PatchPrefix]
    private static void Prefix(Player.FirearmController __instance, out float __state)
    {
        __state = float.NaN;
        try
        {
            if (PerksConfig.ShotgunReloadEnabled?.Value != true || !SkillMultipliers.IsLocalClass("Tank"))
            {
                return;
            }

            if (!ReferenceEquals(__instance, Singleton<GameWorld>.Instance?.MainPlayer?.HandsController))
            {
                return;   // só a arma do player local (075)
            }

            var weapon = __instance.Item;
            // WeapClass=="shotgun" é OBRIGATÓRIO: SupportsInternalReload sozinho pega Mosin/SKS/revólver/M32.
            if (weapon == null || weapon.WeapClass != "shotgun" || !weapon.SupportsInternalReload)
            {
                return;   // só escopeta de TUBO; Saiga (ExternalMagazine) e bicano (OnlyBarrel) ficam de fora
            }

            var buff = __instance.BuffInfo;   // = gclass2250_0 (pode ser null antes do 1º sync de skill)
            if (buff == null)
            {
                return;
            }

            var t = PerksConfig.ShotgunReloadTime?.Value ?? 1f;   // TEMPO de recarga (0.6 = 40% mais rápido)
            if (t > 0f && t < 1f)
            {
                __state = buff.ReloadSpeed;         // salva o original p/ o Postfix restaurar
                buff.ReloadSpeed /= t;              // escala ANTES do push → arma + corpo recebem ×(1/t) em lockstep
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (084) shotgun reload (pre) falhou: {ex.Message}");
        }
    }

    [PatchPostfix]
    private static void Postfix(Player.FirearmController __instance, float __state)
    {
        try
        {
            // restaura o campo (não acumula a cada sync; não vaza p/ FixSpeed/AimMovementSpeed etc. que leem o mesmo GClass2250)
            if (!float.IsNaN(__state) && __instance.BuffInfo != null)
            {
                __instance.BuffInfo.ReloadSpeed = __state;
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (084) shotgun reload (post) falhou: {ex.Message}");
        }
    }
}
