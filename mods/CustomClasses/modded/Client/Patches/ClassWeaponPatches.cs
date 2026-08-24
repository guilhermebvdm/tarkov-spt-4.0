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
                && (SkillMultipliers.IsLocalClass(EClassId.CombatMedic) || SkillMultipliers.IsLocalClass(EClassId.Scavenger)))
            {
                str *= PerksConfig.ShakyHandsRecoil?.Value ?? 1f;
            }

            // 🔧 Adrenaline (Fuzileiro): −30% de recuo durante a janela.
            if (PerksConfig.AdrenalineEnabled?.Value == true && AdrenalineState.IsActive
                && SkillMultipliers.IsLocalClass(EClassId.Rifleman))
            {
                str *= PerksConfig.AdrenalineRecoil?.Value ?? 1f;
            }

            // 🔧 Bunker (Tanque): −15% de recuo com arma pesada (LMG/HMG/GL/underbarrel) na mão.
            if (PerksConfig.BunkerEnabled?.Value == true && SkillMultipliers.IsLocalClass(EClassId.Tank)
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
///     085 — 🔧 Adrenaline (Fuzileiro) — recarga mais rápida durante a janela de combate. <b>CORRIGIDO (era um
///     NO-OP)</b>: o alvo antigo <c>GetWeaponReloadAnimationSpeed</c> é CÓDIGO MORTO no EFT 0.16.9 (nada o chama; o
///     reload speed virou push-based) → o Postfix nunca disparava e o perk não tinha efeito. Migrado para o MESMO
///     funil do <see cref="ShotgunReloadPatch"/> (084): Prefix ESCALA <c>BuffInfo.ReloadSpeed ÷ t</c> antes do push
///     (arma+corpo em lockstep), Postfix RESTAURA. Ref: [[reference_eft_reload_speed_getter_dead]].
///     <para>
///     ⚠️ <b>Abrir/fechar a janela NÃO gera sync sozinho</b> (os gatilhos de re-sync são saque/init/skill-level/
///     mastering/dano-de-braço), então o valor congelaria. O <see cref="AdrenalineState"/> força
///     <c>FirearmController.SetAnimatorAndProceduralValues()</c> nas transições da janela (watcher) → este Prefix
///     re-avalia <c>IsActive</c> e aplica/restaura. Gate: MainPlayer local (075) + Rifleman + janela ativa. Vale p/
///     QUALQUER arma. Coexiste com o 084 no mesmo método sem conflito de <c>__state</c>: são classes MUTUAMENTE
///     EXCLUSIVAS (Tank vs Rifleman), então nunca escalam juntos.
///     </para>
/// </summary>
internal class ReloadSpeedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player.FirearmController), "SetAnimatorAndProceduralValues");
    }

    [PatchPrefix]
    private static void Prefix(Player.FirearmController __instance, out float __state)
    {
        __state = float.NaN;
        try
        {
            if (PerksConfig.AdrenalineEnabled?.Value != true || !AdrenalineState.IsActive
                || !SkillMultipliers.IsLocalClass(EClassId.Rifleman))
            {
                return;
            }

            if (!ReferenceEquals(__instance, Singleton<GameWorld>.Instance?.MainPlayer?.HandsController))
            {
                return;   // só a arma do player local (075)
            }

            var buff = __instance.BuffInfo;
            if (buff == null)
            {
                return;
            }

            var t = PerksConfig.AdrenalineReloadTime?.Value ?? 1f;
            if (t > 0f && t < 1f)
            {
                __state = buff.ReloadSpeed;
                buff.ReloadSpeed /= t;   // arma+corpo recebem ×(1/t) em lockstep (tempo 0.7 → speed ÷0.7 ≈ ×1.43)
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (085) adrenaline reload (pre) falhou: {ex.Message}");
        }
    }

    [PatchPostfix]
    private static void Postfix(Player.FirearmController __instance, float __state)
    {
        try
        {
            if (!float.IsNaN(__state) && __instance.BuffInfo != null)
            {
                __instance.BuffInfo.ReloadSpeed = __state;
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (085) adrenaline reload (post) falhou: {ex.Message}");
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
                && SkillMultipliers.IsLocalClass(EClassId.Rifleman))
            {
                var t = PerksConfig.AdrenalineAdsTime?.Value ?? 1f;
                if (t > 0f && t < 1f)
                {
                    ____aimingSpeed /= t;   // tempo ×0.8 → aimingSpeed ÷0.8 (mira mais rápido)
                }
            }

            // 🔧 Sharpshooter (Caçador): ADS mais rápido (sempre).
            if (PerksConfig.SharpshooterEnabled?.Value == true && SkillMultipliers.IsLocalClass(EClassId.Hunter))
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

            if (SkillMultipliers.IsLocalClass(EClassId.Tank) && HeavyWeapon.IsHeavy(__instance.Item))
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
                && (SkillMultipliers.IsLocalClass(EClassId.Stealth) || SkillMultipliers.IsLocalClass(EClassId.CombatMedic)))
            {
                factor = PerksConfig.RattledAimPunch?.Value ?? 1f;
            }
            else if (PerksConfig.CoolUnderFireEnabled?.Value == true && SkillMultipliers.IsLocalClass(EClassId.Rifleman))
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
///     080 — 🔫 <b>Saque Rápido</b> (Caçador + Fuzileiro + Furtivo): SACAR a arma do slot <b>HOLSTER</b> mais rápido.
///     <b>CORRIGIDO 2× (087, report in-game: "acelerou a SAÍDA da pistola, não o saque")</b>. Diagnóstico do
///     decompile: na troca de arma há DOIS controles independentes —
///     <list type="bullet">
///     <item><b>DRAW-IN</b> (sacar/trazer à mão): o <c>Animator.speed</c> GLOBAL, via o arg <c>animationSpeed</c> de
///     <c>FirearmController.Spawn</c> (Player.cs:13495 → estado "SPAWN" via GClass2055). No vanilla é sempre <c>1f</c>
///     — o saque NUNCA acelera por skill.</item>
///     <item><b>PUT-AWAY</b> (guardar): o float <c>SpeedDraw</c> (= <c>SwapSpeed</c>), via
///     <c>SetAnimatorAndProceduralValues</c>. A skill de arma só acelera ISTO.</item>
///     </list>
///     As tentativas anteriores (getter <c>GetWeaponDrawSpeedMultiplier</c> = só quickdraw-fast; escalar
///     <c>SwapSpeed</c> = só put-away) mexeram no controle ERRADO. O ponto certo do saque é o <c>animationSpeed</c>
///     do <c>Spawn</c>. Prova BSG: <c>GClass2949</c> (spawn) seta só <c>Animator.speed</c>; <c>GClass2944</c>
///     (put-away) seta <c>SpeedDraw</c>.
///     <para>
///     ⚠️ <c>Animator.speed</c> é GLOBAL e NÃO é resetado ao fim do draw-in (no vanilla nunca incomoda porque Spawn
///     sempre passa 1f). Se só escalássemos, a pistola dispararia/recarregaria/idle acelerada até a próxima troca.
///     Por isso o <see cref="HolsterDrawResetPatch"/> restaura <c>speed = 1f</c> assim que o saque termina (no 1º
///     <c>SetAnimatorAndProceduralValues</c> pós-Spawn = <c>GClass2055.WeaponAppeared</c>). Gate: MainPlayer local
///     (075) + classe + a arma que ENTRA vem do Holster.
///     </para>
/// </summary>
internal class HolsterDrawSpeedPatch : ModulePatch
{
    /// <summary>087: marca que o draw-in foi acelerado (Animator.speed global) → o <see cref="HolsterDrawResetPatch"/>
    /// precisa restaurar 1f no fim do saque. Estático porque o boost (Spawn) e o reset (SetAnimator…) são métodos diferentes.</summary>
    internal static bool BoostedDraw;

    protected override MethodBase GetTargetMethod()
    {
        // Spawn(float animationSpeed, Action callback) — desambigua pelos tipos (é override de AbstractHandsController).
        return AccessTools.Method(typeof(Player.FirearmController), "Spawn", new[] { typeof(float), typeof(Action) });
    }

    [PatchPrefix]
    private static void Prefix(Player.FirearmController __instance, ref float animationSpeed)
    {
        try
        {
            if (PerksConfig.QuickDrawEnabled?.Value != true)
            {
                return;
            }

            if (!(SkillMultipliers.IsLocalClass(EClassId.Hunter)
                  || SkillMultipliers.IsLocalClass(EClassId.Rifleman)
                  || SkillMultipliers.IsLocalClass(EClassId.Stealth)))
            {
                return;
            }

            var mainPlayer = Singleton<GameWorld>.Instance?.MainPlayer;
            if (mainPlayer == null || !ReferenceEquals(__instance, mainPlayer.HandsController))
            {
                return;   // só a arma do player local (075) — SpawnController seta HandsController ANTES de Spawn
            }

            // A arma que está ENTRANDO (sendo sacada) vem do slot HOLSTER? CurrentAddress é o accessor SEGURO.
            var holster = mainPlayer.Inventory?.Equipment?.GetSlot(EquipmentSlot.Holster);
            var container = __instance.Item?.CurrentAddress?.Container;
            if (holster == null || container == null || !ReferenceEquals(container, holster))
            {
                return;
            }

            var t = PerksConfig.QuickDrawDrawInTime?.Value ?? 1f;   // fase 3 — TEMPO de sacar (draw-in)
            if (t > 0f && t < 1f)
            {
                animationSpeed /= t;    // draw-in mais rápido (Animator.speed global do estado SPAWN; 0.8 → ×1.25)
                BoostedDraw = true;     // marca p/ o reset restaurar 1f no fim do saque
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (080) quick draw (pre) falhou: {ex.Message}");
        }
    }
}

/// <summary>
///     087 — parceiro do <see cref="HolsterDrawSpeedPatch"/>: restaura o <c>Animator.speed</c> GLOBAL para <c>1f</c>
///     assim que o saque acelerado termina. O <c>Spawn</c> deixa o speed global elevado e NÃO o reseta; o 1º
///     <c>SetAnimatorAndProceduralValues</c> após o Spawn roda em <c>GClass2055.WeaponAppeared</c> (fim do estado
///     SPAWN / draw-in) — momento certo p/ zerar. Só age se o draw-in foi de fato acelerado (flag <c>BoostedDraw</c>)
///     e no MainPlayer local. Sem isso, a pistola operaria acelerada (tiro/reload/idle) até a próxima troca. Espelha
///     o vanilla observado (GClass2944 reseta <c>SetAnimationSpeed(1f)</c> no put-away).
/// </summary>
internal class HolsterDrawResetPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player.FirearmController), "SetAnimatorAndProceduralValues");
    }

    [PatchPostfix]
    private static void Postfix(Player.FirearmController __instance)
    {
        try
        {
            if (!HolsterDrawSpeedPatch.BoostedDraw)
            {
                return;
            }

            if (!ReferenceEquals(__instance, Singleton<GameWorld>.Instance?.MainPlayer?.HandsController))
            {
                return;   // só o player local
            }

            __instance.FirearmsAnimator?.SetAnimationSpeed(1f);   // restaura o speed global pós draw-in (getter público)
            HolsterDrawSpeedPatch.BoostedDraw = false;
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (080) quick draw (reset) falhou: {ex.Message}");
        }
    }
}

/// <summary>
///     088 — 🔫 <b>Saque Rápido, fase 1</b> (put-away): acelera também o GUARDAR da arma que SAI quando a arma que
///     ENTRA vem do Holster, para a TROCA INTEIRA acelerar (não só o draw-in). Feedback in-game: com só a fase 3
///     acelerada, o começo da troca (guardar a primária + transição) ficava lento e destoava.
///     <para>
///     Prefix em <c>FirearmController.Drop(animationSpeed, callback, fastDrop, nextControllerItem)</c> (Player.cs:13506):
///     <c>SetAnimationSpeed(animationSpeed)</c> é o <c>Animator.speed</c> GLOBAL do controller que SAI → escalar
///     acelera o put-away inteiro. A <b>fase 2 (transição) encurta de brinde</b>: o callback do <c>Drop</c> dispara no
///     evento de animação <c>OnWeapOut</c> do put-away, então acelerá-lo antecipa todo o encadeamento
///     create→spawn (não há timer/WaitSeconds a remover). <b>SEM reset</b>: o controller que sai é destruído logo
///     depois (<c>DestroyController</c>) — ao contrário da fase 3. ⚠️ CR-088: o prefab da arma vai pro POOL (não é
///     literalmente destruído), então o <c>Animator.speed</c> boostado sobrevive no pool; a garantia de "sem vazamento"
///     é que TODO <c>Spawn</c> reescreve o speed incondicionalmente (Player.cs:13497) → o próximo saque zera o resíduo.
///     Se um dia o <c>Spawn</c> parar de chamar <c>SetAnimationSpeed</c>, essa premissa quebra.
///     </para>
///     <para>⚠️ Gate INVERTIDO vs. a fase 3: aqui <c>__instance.Item</c> é a arma que SAI (primária); quem está no
///     Holster é o <c>nextControllerItem</c> (a arma que ENTRA). Gate: MainPlayer local (075, ainda é o HandsController
///     atual no Drop) + classe + <c>nextControllerItem</c> vem do Holster. Usa <c>QuickDrawPutAwayTime</c> (config próprio da fase 1).</para>
/// </summary>
internal class HolsterPutAwaySpeedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        // Drop(float animationSpeed, Action callback, bool fastDrop, Item nextControllerItem)
        return AccessTools.Method(typeof(Player.FirearmController), "Drop",
            new[] { typeof(float), typeof(Action), typeof(bool), typeof(Item) });
    }

    [PatchPrefix]
    private static void Prefix(Player.FirearmController __instance, ref float animationSpeed, Item nextControllerItem)
    {
        try
        {
            if (PerksConfig.QuickDrawEnabled?.Value != true)
            {
                return;
            }

            if (!(SkillMultipliers.IsLocalClass(EClassId.Hunter)
                  || SkillMultipliers.IsLocalClass(EClassId.Rifleman)
                  || SkillMultipliers.IsLocalClass(EClassId.Stealth)))
            {
                return;
            }

            var mainPlayer = Singleton<GameWorld>.Instance?.MainPlayer;
            if (mainPlayer == null || !ReferenceEquals(__instance, mainPlayer.HandsController))
            {
                return;   // só o player local (075) — no Drop, o HandsController ainda é a arma que sai
            }

            // A arma que ENTRA (nextControllerItem, não __instance.Item!) vem do slot HOLSTER?
            var holster = mainPlayer.Inventory?.Equipment?.GetSlot(EquipmentSlot.Holster);
            var container = (nextControllerItem as Weapon)?.CurrentAddress?.Container;
            if (holster == null || container == null || !ReferenceEquals(container, holster))
            {
                return;
            }

            var t = PerksConfig.QuickDrawPutAwayTime?.Value ?? 1f;   // fase 1 — TEMPO de guardar (put-away)
            if (t > 0f && t < 1f)
            {
                animationSpeed /= t;   // put-away mais rápido (sem reset — controller destruído logo após)
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (088) quick draw put-away falhou: {ex.Message}");
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
            if (PerksConfig.ShotgunReloadEnabled?.Value != true || !SkillMultipliers.IsLocalClass(EClassId.Tank))
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
