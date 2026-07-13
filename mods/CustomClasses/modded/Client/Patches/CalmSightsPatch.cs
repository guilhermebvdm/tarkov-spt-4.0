using System;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.Animations;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CustomClasses.Client;

/// <summary>
///     Item 072 — 🎯 <b>Calm Sights / Mira Serena</b> (Caçador): reduz o SWAY da arma (×0.7 por padrão).
///     <para>
///     Alvo: <c>ProceduralWeaponAnimation.UpdateSwayFactors()</c>, que RECALCULA o vetor do zero
///     (<c>MotionReact.SwayFactors = new Vector3(...) * IntensityByAiming</c>) a partir de peso ergonômico,
///     sobrepeso e IsAiming. Como é uma ATRIBUIÇÃO (nunca <c>*=</c>), um Postfix que multiplica sempre incide
///     sobre a base limpa — <b>não acumula entre chamadas</b> (era o risco central: um perk que fosse composto a
///     cada frame zeraria o sway em segundos). Confirmado no decompile da DLL real: é a ÚNICA escrita em
///     <c>SwayFactors</c> no assembly inteiro.
///     </para>
///     <para>
///     ⚠️ <b>ESCOPO REAL DO EFEITO</b> (registrar, o card não pode mentir): <c>SwayFactors</c> alimenta apenas os
///     <i>mouse processors</i> — o sway induzido por mira/movimento. O sway de <b>respiração</b> é outro effector
///     (<c>Breath.Intensity</c>) e <b>NÃO</b> é afetado por este perk. O Caçador já tem Iron Lungs para a respiração.
///     </para>
///     <para>
///     ⚠️ É <b>event-driven</b> (mirar, trocar de pose/arma, mudar de peso, trocar scope) — não roda por frame.
///     Logo, ligar/desligar o perk no F12 no meio da raid só reflete no PRÓXIMO evento. Aceito (é config de
///     validação, não de gameplay). Gate obrigatório: <c>MotionReact</c> é POR PLAYER, então sem o gate o perk
///     valeria para bots e peers Fika.
///     </para>
/// </summary>
internal class CalmSightsPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        // public void UpdateSwayFactors() — não-virtual, sem params (decompile da DLL real).
        return AccessTools.Method(typeof(ProceduralWeaponAnimation), nameof(ProceduralWeaponAnimation.UpdateSwayFactors));
    }

    [PatchPostfix]
    private static void Postfix(ProceduralWeaponAnimation __instance)
    {
        try
        {
            if (PerksConfig.CalmSightsEnabled?.Value != true)
            {
                return;
            }

            // ⚠️ CR-F5 — ORDEM DOS GATES IMPORTA. Este Postfix roda para CADA ProceduralWeaponAnimation da cena
            // (bots, peers Fika), e o IsLocalClass() cai num EnsureLoaded() que, com cache frio, faz um GET HTTP
            // SÍNCRONO. Identidade primeiro (ReferenceEquals, barato); só o PWA do SEU player chega na classe.
            var mainPwa = Singleton<GameWorld>.Instance?.MainPlayer?.ProceduralWeaponAnimation;
            if (mainPwa == null || !ReferenceEquals(__instance, mainPwa))
            {
                return;
            }

            if (!SkillMultipliers.IsLocalClass("Hunter"))
            {
                return;
            }

            var motion = __instance.MotionReact;   // campo público (PWA), inicializado no Initialize
            if (motion == null)
            {
                return;
            }

            motion.SwayFactors *= PerksConfig.CalmSightsSway?.Value ?? 0.7f;
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (072) calm sights falhou: {ex.Message}");
        }
    }
}
