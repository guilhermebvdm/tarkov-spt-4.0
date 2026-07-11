using System;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.Animations;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CustomClasses.Client;

/// <summary>
///     B15 (balance 2026-07-11) — <b>PISO COMBINADO de recuo</b>.
///     <para>
///     Os multiplicadores de recuo empilham por <b>PRODUTO</b> sobre o mesmo <c>ref float str</c> de
///     <c>ProceduralWeaponAnimation.Shoot</c>: maestria da arma (058, <see cref="WeaponMasteryRecoilPatch"/>,
///     <c>Priority.High</c>) × perks (050, <see cref="ShootRecoilPatch"/>: Shaky Hands / Adrenalina / Bunker).
///     A maestria tem piso PRÓPRIO (0.5 — inalcançável no cap de nível 51), mas o PRODUTO não tinha piso
///     nenhum (Anexo C do board): Tanque + LMG + maestria 51 ≈ <b>×0.68</b>; Fuzileiro na janela de
///     Adrenalina + maestria ≈ <b>×0.56</b>. Com o piso 0.60, essencialmente só a janela de Adrenalina morde.
///     </para>
///     <para>
///     Implementação: dois Prefixes no MESMO alvo, CERCANDO a cadeia (não dá para clampar de dentro de um
///     dos patches — nenhum deles conhece o <c>str</c> verdadeiramente original, já que a maestria roda antes).
///     <see cref="RecoilFloorCapturePatch"/> (<c>Priority.First</c>) guarda o <c>str</c> ORIGINAL;
///     <see cref="RecoilFloorApplyPatch"/> (<c>Priority.Last</c>) roda depois de TODOS os multiplicadores e
///     garante <c>str &gt;= original × piso</c>. Só a arma do player local (gate nos dois; se a chamada não
///     for do player, o capture marca <c>NaN</c> e o apply ignora).
///     </para>
/// </summary>
internal class RecoilFloorCapturePatch : ModulePatch
{
    /// <summary>`str` original desta invocação de Shoot (NaN = não é a arma do player local → apply ignora).
    /// Main thread (Shoot roda no update do player) — sem concorrência.</summary>
    internal static float StrBefore = float.NaN;

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(ProceduralWeaponAnimation), nameof(ProceduralWeaponAnimation.Shoot));
    }

    [HarmonyPriority(Priority.First)]   // ANTES de qualquer multiplicador (maestria = High, perks = Normal)
    [PatchPrefix]
    private static void Prefix(ProceduralWeaponAnimation __instance, ref float str)
    {
        try
        {
            var p = Singleton<GameWorld>.Instance?.MainPlayer;
            StrBefore = p != null && ReferenceEquals(__instance, p.ProceduralWeaponAnimation)
                ? str
                : float.NaN;   // arma de bot/remoto → o apply não faz nada
        }
        catch (Exception ex)
        {
            StrBefore = float.NaN;
            Plugin.Log?.LogError($"[CustomClasses] recoil floor (capture) falhou: {ex.Message}");
        }
    }
}

/// <summary>B15 — aplica o piso DEPOIS de todos os multiplicadores. Ver <see cref="RecoilFloorCapturePatch"/>.</summary>
internal class RecoilFloorApplyPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(ProceduralWeaponAnimation), nameof(ProceduralWeaponAnimation.Shoot));
    }

    [HarmonyPriority(Priority.Last)]   // DEPOIS de maestria (High) e perks (Normal)
    [PatchPrefix]
    private static void Prefix(ProceduralWeaponAnimation __instance, ref float str)
    {
        try
        {
            if (PerksConfig.RecoilFloorEnabled?.Value != true)
            {
                return;
            }

            var before = RecoilFloorCapturePatch.StrBefore;
            if (float.IsNaN(before))
            {
                return;   // não era a arma do player local nesta invocação
            }

            var floor = PerksConfig.RecoilFloor?.Value ?? 0.6f;
            var min = before * floor;
            if (str < min)
            {
                str = min;   // o produto (maestria × perks) tentou passar do piso → clampa
            }

            // (code-review 2026-07-11) O ShootRecoilPatch grava `PerkDiag.RecoilAfter` na prioridade Normal —
            // ou seja, ANTES deste clamp. Sem isto, o overlay (052) mostraria o recuo PRÉ-piso e mentiria
            // justamente no caso que o B15 existe p/ corrigir (ex.: janela de Adrenalina). Reescrevemos aqui,
            // no fim da cadeia, usando o `before` REAL (o `RecoilBefore` do outro patch já vem multiplicado
            // pela maestria — este é o original de verdade).
            if (PerkDiag.Enabled)
            {
                PerkDiag.RecoilBefore = before;
                PerkDiag.RecoilAfter = str;
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] recoil floor (apply) falhou: {ex.Message}");
        }
    }
}
