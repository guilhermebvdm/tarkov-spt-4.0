using System;
using System.Collections.Generic;   // ref: PA-02-01 — HashSet do BranchFailLog
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.Animations;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;   // ref: AUD-01-03 — Mathf nos branches de recuo movidos

namespace CustomClasses.Client;

/// <summary>
///     ref: AUD-01-03 — branches de <c>FirearmController.SetAnimatorAndProceduralValues</c>, movidos SEM
///     alteração de fórmula dos três patches que consolidaram em <see cref="FirearmSyncPatch"/>.
///     <para>
///     ⚠️ <b>Nenhum branch grava o <c>__state</c></b> (PA-03-02) — quem captura é o Prefix, incondicionalmente,
///     antes de qualquer um deles rodar.
///     </para>
/// </summary>
internal static class ReloadBranches
{
    /// <summary>
    ///     085 — 🔧 Adrenaline (Fuzileiro): recarga mais rápida na janela. ref: origem ReloadSpeedPatch.
    ///     Escala <c>BuffInfo.ReloadSpeed ÷ t</c> ANTES do push → arma + corpo em lockstep.
    /// </summary>
    /// <remarks>⚠️ Recebe o <c>FirearmController</c>, não o BuffInfo: o tipo dele é OFUSCADO
    /// (<c>GClass2250</c>) e nomeá-lo numa assinatura violaria o AP-09 — esses números mudam entre builds do
    /// EFT. O código original também nunca o nomeava (usava <c>var</c>).</remarks>
    internal static void Adrenaline(Player.FirearmController fc)
    {
        if (PerksConfig.AdrenalineEnabled?.Value != true || !AdrenalineState.IsActive
            || !SkillMultipliers.IsLocalClass(EClassId.Rifleman))
        {
            return;
        }

        var buff = fc.BuffInfo;
        if (buff == null)
        {
            return;
        }

        var t = PerksConfig.AdrenalineReloadTime?.Value ?? 1f;
        if (t > 0f && t < 1f)
        {
            buff.ReloadSpeed /= t;   // tempo 0.7 → speed ÷0.7 ≈ ×1.43
        }
    }

    /// <summary>
    ///     084 — 🔫 Recarga Rápida de Escopeta (Tanque). ref: origem ShotgunReloadPatch.
    ///     ⚠️ <c>WeapClass=="shotgun"</c> é OBRIGATÓRIO: o <c>SupportsInternalReload</c> sozinho pega
    ///     Mosin/SKS/revólver/M32. Saiga (ExternalMagazine) e bicano (OnlyBarrel) ficam de fora corretamente.
    ///     <para>Mutuamente exclusivo com <see cref="Adrenaline"/> — Tanque e Fuzileiro são classes distintas,
    ///     então o Prefix nunca escala o campo duas vezes.</para>
    /// </summary>
    /// <remarks>⚠️ Ver a nota de AP-09 em <see cref="Adrenaline"/> — o tipo do BuffInfo é ofuscado.</remarks>
    internal static void Shotgun(Player.FirearmController fc)
    {
        if (PerksConfig.ShotgunReloadEnabled?.Value != true || !SkillMultipliers.IsLocalClass(EClassId.Tank))
        {
            return;
        }

        var buff = fc.BuffInfo;
        if (buff == null)
        {
            return;
        }

        var weapon = fc.Item;
        if (weapon == null || weapon.WeapClass != "shotgun" || !weapon.SupportsInternalReload)
        {
            return;   // só escopeta de TUBO
        }

        var t = PerksConfig.ShotgunReloadTime?.Value ?? 1f;
        if (t > 0f && t < 1f)
        {
            buff.ReloadSpeed /= t;   // 0.6 = 40% mais rápido
        }
    }

    /// <summary>
    ///     087 — restaura o <c>Animator.speed</c> GLOBAL para 1f no fim do saque acelerado.
    ///     ref: origem HolsterDrawResetPatch. O <c>Spawn</c> deixa o speed elevado e NÃO o reseta; o 1º
    ///     <c>SetAnimatorAndProceduralValues</c> pós-Spawn roda no <c>GClass2055.WeaponAppeared</c> (fim do
    ///     estado SPAWN / draw-in) — momento certo de zerar. Sem isto a pistola operaria acelerada
    ///     (tiro/reload/idle) até a próxima troca.
    /// </summary>
    internal static void ResetHolsterDraw(Player.FirearmController fc)
    {
        if (!HolsterDrawSpeedPatch.BoostedDraw)
        {
            return;
        }

        fc.FirearmsAnimator?.SetAnimationSpeed(1f);   // getter público
        HolsterDrawSpeedPatch.BoostedDraw = false;
    }
}

/// <summary>
///     ref: AUD-01-03 — patch consolidado de <c>FirearmController.SetAnimatorAndProceduralValues</c>
///     (3 patches → 1 par Prefix/Postfix): <c>ReloadSpeedPatch</c> (085, Adrenalina) +
///     <c>ShotgunReloadPatch</c> (084, escopeta) + <c>HolsterDrawResetPatch</c> (087, reset do saque).
///     <para>
///     ⚠️ PA-02-06: nenhum dos três tinha <c>[HarmonyPriority]</c> — consolidar não move fronteira de ordem.
///     </para>
///     <para>
///     ⚠️ <b>Ganho de correção, não só de custo:</b> antes havia DOIS pares Prefix/Postfix independentes
///     escalando e restaurando o MESMO campo <c>BuffInfo.ReloadSpeed</c>, cada um com o seu <c>__state</c>.
///     Funcionava só porque Tanque e Fuzileiro são mutuamente exclusivos. Agora é um <c>__state</c> só.
///     </para>
/// </summary>
internal class FirearmSyncPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player.FirearmController), "SetAnimatorAndProceduralValues");
    }

    /// <summary>
    ///     ref: CR-01-01 — o <c>__state</c> é um PAR, não só o valor.
    ///     <para>
    ///     <c>Original</c> é capturado <b>incondicionalmente antes de qualquer branch</b> (exigência do
    ///     PA-03-02: o try/catch por branch contém a exceção mas NÃO desfaz uma escrita parcial).
    ///     <c>MayHaveScaled</c> resolve o outro lado: sem ele o Postfix restauraria <b>sempre</b>, e um Prefix
    ///     de OUTRO MOD que escrevesse <c>ReloadSpeed</c> depois do nosso seria sobrescrito — uma fronteira de
    ///     composição alterada como efeito colateral da consolidação, exatamente o que o PA-01-01 ensinou a
    ///     não fazer. O flag é setado <b>antes</b> de chamar os branches (não por eles), então um branch que
    ///     lance no meio de uma mutação continua coberto.
    ///     </para>
    /// </summary>
    internal readonly struct SyncState
    {
        internal readonly float Original;
        internal readonly bool MayHaveScaled;

        internal SyncState(float original, bool mayHaveScaled)
        {
            Original = original;
            MayHaveScaled = mayHaveScaled;
        }
    }

    /// <summary>
    ///     Algum branch PODE agir nesta invocação? Teste barato (config + classe), sem tocar no estado.
    ///     Espelha a condição de entrada de <see cref="ReloadBranches.Adrenaline"/> e
    ///     <see cref="ReloadBranches.Shotgun"/> — as duas são mutuamente exclusivas (Fuzileiro × Tanque).
    /// </summary>
    private static bool AnyReloadBranchMayAct()
    {
        var adrenaline = PerksConfig.AdrenalineEnabled?.Value == true
                         && AdrenalineState.IsActive
                         && SkillMultipliers.IsLocalClass(EClassId.Rifleman);

        var shotgun = PerksConfig.ShotgunReloadEnabled?.Value == true
                      && SkillMultipliers.IsLocalClass(EClassId.Tank);

        return adrenaline || shotgun;
    }

    [PatchPrefix]
    private static void Prefix(Player.FirearmController __instance, out SyncState __state)
    {
        __state = default;   // Original=0, MayHaveScaled=false → o Postfix não restaura

        var buff = __instance.BuffInfo;   // = gclass2250_0 (pode ser null antes do 1º sync de skill)
        if (buff == null)
        {
            return;
        }

        if (!ReferenceEquals(__instance, Singleton<GameWorld>.Instance?.MainPlayer?.HandsController))
        {
            return;   // só a arma do player local (075) — GATE ÚNICO, era resolvido 2×
        }

        // ⚠️ PA-03-02 — captura INCONDICIONAL e ANTES de qualquer branch. O try/catch por branch (PA-02-01)
        // CONTÉM a exceção mas NÃO desfaz a escrita: um branch que lance depois de mutar ReloadSpeed e antes
        // de gravar o original deixaria o campo escalado PELA RAID INTEIRA (recarga permanentemente
        // acelerada), com um único erro no log. Só esta ordem garante que o Postfix tenha o valor original.
        //
        // ⚠️ CR-01-01 — o flag é decidido AQUI, antes dos branches: se nenhum deles pode agir, o Postfix não
        // toca no campo e uma escrita de terceiro sobrevive.
        var mayScale = AnyReloadBranchMayAct();
        __state = new SyncState(buff.ReloadSpeed, mayScale);

        if (!mayScale)
        {
            return;   // nada a fazer nesta invocação — nem escalar, nem restaurar
        }

        // ref: PA-02-01 — isolamento por branch.
        try { ReloadBranches.Adrenaline(__instance); } catch (Exception ex) { BranchFailLog.Once("sync/adrenaline", ex); }
        try { ReloadBranches.Shotgun(__instance); } catch (Exception ex) { BranchFailLog.Once("sync/shotgun", ex); }
    }

    [PatchPostfix]
    private static void Postfix(Player.FirearmController __instance, SyncState __state)
    {
        // Restaura o campo: não acumula entre syncs nem vaza p/ outros consumidores do mesmo GClass2250
        // (FixSpeed/AimMovementSpeed etc.).
        // ref: CR-01-01 — SÓ quando algum branch podia ter escalado. O alvo apenas LÊ ReloadSpeed
        // (Player.cs:12634-12664; quem escreve é o SyncWithCharacterSkills, ANTES da chamada), então
        // restaurar à toa não quebrava o vanilla — mas clobberaria a escrita de um Prefix de outro mod.
        try
        {
            if (__state.MayHaveScaled && __instance.BuffInfo != null)
            {
                __instance.BuffInfo.ReloadSpeed = __state.Original;
            }
        }
        catch (Exception ex) { BranchFailLog.Once("sync/restore", ex); }

        try { ReloadBranches.ResetHolsterDraw(__instance); } catch (Exception ex) { BranchFailLog.Once("sync/holster-reset", ex); }
    }
}

/// <summary>ref: AUD-01-03 · PA-01-01 — estado de UMA invocação de Shoot, gravado pelo
/// <see cref="ShootCapturePatch"/> e lido pelo <see cref="ShootApplyPatch"/>. Main thread (Shoot roda no
/// update do player) — sem concorrência. Substitui o antigo <c>RecoilFloorCapturePatch.StrBefore</c>, com o
/// mesmo papel: dois patches Harmony distintos NÃO compartilham <c>__state</c>, então o estático fica.</summary>
internal static class ShootRecoilState
{
    /// <summary>`str` original desta invocação (NaN = não é a arma do player local → o apply ignora).</summary>
    internal static float StrBefore = float.NaN;
}

/// <summary>
///     ref: AUD-01-03 — branches de recuo, movidos SEM alteração de fórmula dos três patches que
///     consolidaram em <see cref="ShootApplyPatch"/>. O gate saiu daqui (resolvido uma vez no patch).
/// </summary>
internal static class RecoilBranches
{
    /// <summary>058 · Perna 2 — recuo × (1 − rec/nível × Level). ref: origem WeaponMasteryRecoilPatch.</summary>
    internal static void ApplyMastery(Player p, ref float str)
    {
        if (PerksConfig.WeaponMasteryEnabled?.Value != true)
        {
            return;
        }

        var skill = WeaponMastery.SkillForHeld(p.Skills, (p.HandsController as Player.FirearmController)?.Item);
        var lvl = skill?.Level ?? 0;
        var rec = PerksConfig.MasteryRecoilPerLevel?.Value ?? 0f;
        if (lvl > 0 && rec > 0f)
        {
            str *= Mathf.Max(0.5f, 1f - rec * lvl);   // clamp: nunca corta mais que 50% via maestria
        }
    }

    /// <summary>050.2 — Shaky Hands · Adrenaline · Bunker. ref: origem ShootRecoilPatch.</summary>
    internal static void ApplyPerks(Player p, ref float str)
    {
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
    }

    /// <summary>
    ///     B15 (balance 2026-07-11) — <b>PISO COMBINADO de recuo</b>. ref: origem RecoilFloorApplyPatch
    ///     (o arquivo <c>RecoilFloorPatch.cs</c> foi removido — PA-03-07 — e este XMLdoc é o histórico dele).
    ///     <para>
    ///     Os multiplicadores de recuo empilham por <b>PRODUTO</b> sobre o mesmo <c>ref float str</c>:
    ///     maestria da arma (058) × perks (050: Shaky Hands / Adrenalina / Bunker). A maestria tem piso
    ///     PRÓPRIO (0.5 — inalcançável no cap de nível 51), mas o PRODUTO não tinha piso nenhum (Anexo C do
    ///     balance board): Tanque + LMG + maestria 51 ≈ <b>×0.68</b>; Fuzileiro na janela de Adrenalina +
    ///     maestria ≈ <b>×0.56</b>. Com o piso 0.60, essencialmente só a janela de Adrenalina morde.
    ///     </para>
    ///     <para>
    ///     ⚠️ O clamp é OPCIONAL (toggle do F12) mas a escrita do diagnóstico NÃO — ela vive fora deste
    ///     método, no fim do <see cref="ShootApplyPatch"/>, para o overlay 052 sempre refletir o valor real
    ///     mesmo com o piso desligado (code-review 2026-07-11, 2ª rodada).
    ///     </para>
    /// </summary>
    internal static void ApplyFloor(float str0, ref float str)
    {
        if (PerksConfig.RecoilFloorEnabled?.Value != true)
        {
            return;
        }

        var floor = PerksConfig.RecoilFloor?.Value ?? 0.6f;
        var min = str0 * floor;
        if (str < min)
        {
            str = min;   // o produto (maestria × perks) tentou passar do piso → clampa
        }
    }
}

/// <summary>
///     ref: AUD-01-03 · PA-01-01 — <b>FRONTEIRA DE ENTRADA</b> do <c>PWA.Shoot</c>.
///     <para>
///     <c>Priority.First</c>: captura o <c>str</c> ANTES de qualquer multiplicador, inclusive os de
///     <b>OUTROS MODS</b> (o usuário roda RealRecoil). Não muta nada — só observa.
///     </para>
///     <para>
///     ⚠️ É por isto que a consolidação é 4 → <b>2</b> e não 4 → 1: <c>Priority.First</c>/<c>Last</c> ordenam
///     contra prefixos de terceiros, não só contra os nossos. Num patch único de prioridade <c>Normal</c>, o
///     "original" capturado já viria multiplicado por um mod de prioridade mais alta e o piso B15 clamparia
///     ANTES dos multiplicadores externos — em silêncio, e o overlay 052 não pegaria (ele só mede a nossa cadeia).
///     </para>
/// </summary>
internal class ShootCapturePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(ProceduralWeaponAnimation), nameof(ProceduralWeaponAnimation.Shoot));
    }

    [HarmonyPriority(Priority.First)]
    [PatchPrefix]
    private static void Prefix(ProceduralWeaponAnimation __instance, ref float str)
    {
        try
        {
            var p = Singleton<GameWorld>.Instance?.MainPlayer;
            ShootRecoilState.StrBefore = p != null && ReferenceEquals(__instance, p.ProceduralWeaponAnimation)
                ? str
                : float.NaN;   // arma de bot/remoto → o apply não faz nada

            // PERF-INSTR AUD-01-03 — temporary, remove after validation
            if (PerkDiag.Enabled)
            {
                PerfCount.ShootCalls++;
                PerfCount.ShootGates++;
            }
        }
        catch (Exception ex)
        {
            ShootRecoilState.StrBefore = float.NaN;
            Plugin.Log?.LogError($"[CustomClasses] recoil capture falhou: {ex.Message}");
        }
    }
}

/// <summary>
///     ref: AUD-01-03 · PA-01-01 — <b>FRONTEIRA DE SAÍDA</b> do <c>PWA.Shoot</c>.
///     <para>
///     <c>Priority.Last</c>: roda depois de TODOS os multiplicadores, nossos e de terceiros. Funde três
///     patches num só (maestria 058 + perks 050 + piso B15), com a ordem interna <b>escrita em sequência</b>
///     em vez de emergir da coordenação de três <c>[HarmonyPriority]</c> + um estático compartilhado:
///     (1) maestria → (2) perks → (3) piso → (4) diagnóstico.
///     Era: <c>First</c> (capture) → <c>High</c> (maestria) → <c>Normal</c> (perks) → <c>Last</c> (apply).
///     </para>
/// </summary>
internal class ShootApplyPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(ProceduralWeaponAnimation), nameof(ProceduralWeaponAnimation.Shoot));
    }

    [HarmonyPriority(Priority.Last)]
    [PatchPrefix]
    private static void Prefix(ProceduralWeaponAnimation __instance, ref float str)
    {
        var str0 = ShootRecoilState.StrBefore;
        if (float.IsNaN(str0))
        {
            return;   // não era a arma do player local nesta invocação
        }

        var p = Singleton<GameWorld>.Instance?.MainPlayer;
        if (p == null)
        {
            return;   // GATE ÚNICO (era resolvido 3× aqui dentro) — o ganho do AUD-01-03
        }

        // PERF-INSTR AUD-01-03 — temporary, remove after validation
        if (PerkDiag.Enabled)
        {
            PerfCount.ShootGates++;
        }

        // ⚠️ ref: PA-02-01 — try/catch POR BRANCH, nunca um externo único. Consolidar o GATE não pode
        // consolidar a FALHA: `ApplyMastery` toca p.Skills e skill.Level, que ficam nulos numa troca de arma;
        // num catch externo, ela lançando pularia o PISO B15 e o tiro sairia SEM CLAMP NENHUM.
        try { RecoilBranches.ApplyMastery(p, ref str); } catch (Exception ex) { BranchFailLog.Once("recoil/mastery", ex); }
        try { RecoilBranches.ApplyPerks(p, ref str); } catch (Exception ex) { BranchFailLog.Once("recoil/perks", ex); }
        try { RecoilBranches.ApplyFloor(str0, ref str); } catch (Exception ex) { BranchFailLog.Once("recoil/floor", ex); }

        // (4) baseline = str ORIGINAL. Fora do gate do piso de propósito: com o piso DESLIGADO o overlay
        // ainda tem de mostrar o valor real, senão volta a mentir (code-review 2026-07-11, 2ª rodada).
        if (PerkDiag.Enabled)
        {
            PerkDiag.RecoilBefore = str0;
            PerkDiag.RecoilAfter = str;
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
///     ref: PA-02-01 — log de falha de branch com dedupe.
///     <para>
///     Consolidar patches (AUD-01-03) consolida o GATE, mas <b>não pode consolidar a FALHA</b>: hoje cada
///     patch é uma unidade Harmony independente com <c>try/catch</c> próprio, então um que lança não impede
///     os outros de rodar. Os patches consolidados preservam isso com <c>try/catch</c> POR BRANCH — e este
///     helper existe porque esses branches rodam em hot path (por tiro, por dano): o padrão atual de
///     <c>LogError</c> a cada ocorrência inunda o console quando algo quebra numa rajada. Uma linha por
///     branch por sessão basta para diagnosticar.
///     </para>
/// </summary>
internal static class BranchFailLog
{
    private static readonly HashSet<string> Seen = new(StringComparer.Ordinal);

    internal static void Once(string branch, Exception ex)
    {
        if (!Seen.Add(branch))
        {
            return;
        }

        Plugin.Log?.LogError($"[CustomClasses] branch '{branch}' falhou (log 1× por sessão): {ex.Message}");
    }
}

/// <summary>
///     ref: AUD-01-03 — branches de ergonomia, extraídos dos patches que consolidaram em
///     <see cref="TotalErgoPatch"/>. Fórmulas movidas 1:1; o gate saiu daqui (é resolvido uma vez no patch).
/// </summary>
internal static class ErgoBranches
{
    /// <summary>🔧 Bunker (Tanque) — +15% de ergo com arma pesada. ref: origem HeavyWeaponErgoPatch.</summary>
    internal static void Bunker(Player p, Player.FirearmController fc, ref float result)
    {
        if (PerksConfig.BunkerEnabled?.Value != true)
        {
            return;
        }

        if (SkillMultipliers.IsLocalClass(EClassId.Tank) && HeavyWeapon.IsHeavy(fc.Item))
        {
            result *= PerksConfig.BunkerHeavyErgo?.Value ?? 1f;
        }
    }

    /// <summary>058 — ergo × (1 + ergo/nível × Level) da maestria da arma. ref: origem WeaponMasteryErgoPatch.</summary>
    internal static void Mastery(Player p, Player.FirearmController fc, ref float result)
    {
        if (PerksConfig.WeaponMasteryEnabled?.Value != true)
        {
            return;
        }

        var skill = WeaponMastery.SkillForHeld(p.Skills, fc.Item);
        var lvl = skill?.Level ?? 0;
        var ergo = PerksConfig.MasteryErgoPerLevel?.Value ?? 0f;
        if (lvl > 0 && ergo > 0f)
        {
            result *= 1f + ergo * lvl;
        }
    }
}

/// <summary>
///     ref: AUD-01-03 — patch ÚNICO no getter <c>FirearmController.TotalErgonomics</c> (funil real de ergo da
///     arma, lido por recoil/handling/sway). Substitui <c>HeavyWeaponErgoPatch</c> (050.4b, Bunker) +
///     <c>WeaponMasteryErgoPatch</c> (058, maestria), que resolviam o MESMO gate duas vezes por leitura.
///     <para>
///     Ordem irrelevante entre os dois branches (ambos multiplicam — comuta), mas escrita mesmo assim.
///     ⚠️ PA-02-06: nenhum dos dois patches originais tinha <c>[HarmonyPriority]</c>, então consolidá-los não
///     move nenhuma fronteira contra mods externos (ao contrário do <c>PWA.Shoot</c> — ver PA-01-01).
///     </para>
/// </summary>
internal class TotalErgoPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(Player.FirearmController), nameof(Player.FirearmController.TotalErgonomics));
    }

    [PatchPostfix]
    private static void Postfix(Player.FirearmController __instance, ref float __result)
    {
        // GATE ÚNICO (era resolvido 2×).
        var p = Singleton<GameWorld>.Instance?.MainPlayer;
        if (p == null || !ReferenceEquals(__instance, p.HandsController))
        {
            return;   // só a arma do player local
        }

        // PERF-INSTR AUD-01-03 — temporary, remove after validation
        if (PerkDiag.Enabled)
        {
            PerfCount.ErgoGates++;
        }

        // ref: PA-02-01 — isolamento POR BRANCH (não um try/catch externo).
        try { ErgoBranches.Bunker(p, __instance, ref __result); } catch (Exception ex) { BranchFailLog.Once("ergo/bunker", ex); }
        try { ErgoBranches.Mastery(p, __instance, ref __result); } catch (Exception ex) { BranchFailLog.Once("ergo/mastery", ex); }
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
            if (UnityEngine.Time.time - LocalHitState.LastCombatHitTime > 0.15f)
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
///     ref: AUD-01-03 — o que sobrou do <c>LocalHitTypePatch</c>: só o carimbo. O Prefix em
///     <c>Player.ApplyDamageInfo</c> virou <c>DamageBranches.StampCombatHit</c>, chamado PRIMEIRO pelo
///     <c>ClassDamagePatch</c> consolidado.
///     <para>
///     Marca o instante do último dano de COMBATE (que passa por <c>ApplyDamageInfo</c>). Dano de QUEDA NÃO
///     passa por lá — vai por <c>ActiveHealthController.ApplyDamage</c> (review 2026-06-24) → o timestamp
///     fica velho → o aim-punch de queda é barrado por RECÊNCIA no <see cref="AimPunchPatch"/>.
///     </para>
/// </summary>
internal static class LocalHitState
{
    internal static float LastCombatHitTime = -999f;
}


