using System;
using System.Reflection;
using EFT;          // SkillClass
using EFT.UI;       // SkillIcon
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;      // Color
using UnityEngine.UI;   // Image

namespace CustomClasses.Client;

/// <summary>
///     (010) UI — borda colorida no ícone da skill: verde = buff, vermelho = debuff.
///     Postfix em SkillIcon.Show(SkillClass, IHealthController, Action&lt;bool,PointerEventData&gt;).
///     ref: Assembly-CSharp.dll → EFT.UI.SkillIcon { Image _border; bool skillClass.IsEliteLevel }.
///     PA-01-01: Elite tem precedência (mantém o laranja vanilla); o buff/debuff é sinalizado
///     pela seta + tooltip ao lado do nome (SkillPanelPatch).
///     PA-01-03: sem fator → reseta a borda (branco; laranja se Elite) p/ não vazar cor entre células recicladas.
/// </summary>
internal class SkillIconBorderPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(SkillIcon), nameof(SkillIcon.Show));   // overload único
    }

    [PatchPostfix]
    private static void Postfix(SkillClass skill, Image ____border)
    {
        if (!Plugin.ShowOnUi || skill is null || ____border is null)
        {
            return;
        }

        try
        {
            SkillMultipliers.EnsureLoaded();
            // Pula skills bloqueadas/"beta" (Locked = sem buffs) — não colore borda de skill que não dá p/ usar.
            var got = SkillMultipliers.TryGet(skill.Id, out var f);
            var has = got && !skill.Locked && MultiplierFormat.IsActive(f);

            if (has && !skill.IsEliteLevel)
            {
                ____border.color = MultiplierFormat.BorderColor(f);   // verde/vermelho
            }
            else
            {
                // Elite (com ou sem fator) → laranja vanilla; demais sem fator → branco.
                // Repõe a cor vanilla p/ não herdar a borda colorida de uma skill anterior na célula reciclada.
                ____border.color = skill.IsEliteLevel ? MultiplierFormat.EliteBorder : Color.white;
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] skill icon border falhou: {ex.Message}");
        }
    }
}
