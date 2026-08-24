using System;
using System.Reflection;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CustomClasses.Client;

/// <summary>
///     Item 061 — 🎒 Quick Hands (Saqueador): revista DOIS contêineres ao mesmo tempo, desde o início.
///     <para>
///     NÃO é mecânica nova: é o bônus <b>ELITE vanilla</b> da skill Search (nível 51 — buff
///     <c>EBuffId.SearchDouble</c>, <c>BuffType = EBuffType.Elite</c>) ANTECIPADO para a classe
///     (anotação do usuário, 2026-07-05).
///     </para>
///     <para>
///     Lever: o getter <c>SkillManager.IsSearchDouble</c> (= <c>SearchDouble.Value</c> — ref:
///     SkillManager.cs:1843), lido pelo search controller do player em
///     <c>CanStartNewSearchOperation()</c> (ref: GClass2235.cs:82):
///     <code>
///     if (Profile_0.SkillsInfo == null || !Profile_0.SkillsInfo.IsSearchDouble) return count &lt; 1;
///     return count &lt; 2;
///     </code>
///     Forçando o getter a <c>true</c> reusamos TODA a lógica vanilla (inclusive o teto de 2 operações) —
///     nada do fluxo de busca é reimplementado. O SPT server não tem lógica de <c>SearchDouble</c> (só o
///     locale do buff), então não há re-validação server-side: o efeito é puramente client.
///     </para>
///     <para>
///     <b>No-op no elite natural:</b> se a Search já chegou a Elite, <c>__result</c> já é <c>true</c> →
///     saímos cedo, sem "dobrar" (o teto vanilla é um <c>bool</c>, não um contador — nunca vira 3).
///     </para>
///     <para>
///     <b>Coop-safe / bots:</b> efeito local, e há proteção DUPLA. (1) Gate igual ao
///     <see cref="PackMulePatch"/> (mesmo alvo <c>SkillManager</c>): na raid compara <c>__instance</c> com
///     <c>MainPlayer.Skills</c> — barra peers Fika (<c>ObservedPlayer</c>) e qualquer SkillManager alheio;
///     <c>MainPlayer</c> null (loading/headless) cai no <c>ReferenceEquals(x, null) == false</c> → sem buff.
///     (2) A IA nem chega aqui: <c>AISearchControllerClass</c> sobrescreve <c>CanStartNewSearchOperation()</c>
///     para <c>false</c> e passa <c>Profile_0 = null</c> — o branch que lê <c>IsSearchDouble</c> nunca roda p/ bot.
///     </para>
///     <para>
///     <b>Fora da raid</b> (<c>GameWorld</c> null): o gate cai para "só pela classe" — defensivo, seguindo o
///     <see cref="PackMulePatch"/>. Não está confirmado que o fluxo de busca do menu/stash instancie este
///     controller (o ctor exige um <c>Player.PlayerInventoryController</c>, e não há <c>Player</c> no menu);
///     se não instanciar, o patch é simplesmente INERTE lá — inofensivo, pois fora da raid só existe o perfil local.
///     </para>
/// </summary>
internal class QuickHandsPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(SkillManager), nameof(SkillManager.IsSearchDouble));
    }

    [PatchPostfix]
    private static void Postfix(SkillManager __instance, ref bool __result)
    {
        try
        {
            // já é Elite (no-op — não dobra) ou perk desligado no F12.
            if (__result || PerksConfig.QuickHandsEnabled?.Value != true)
            {
                return;
            }

            // Na raid: só o SkillManager do MainPlayer local (não bufar bots).
            // Fora da raid (stash/menu — GameWorld null, sem bots): gateia só pela classe.
            var gw = Singleton<GameWorld>.Instance;
            if (gw != null && !ReferenceEquals(__instance, gw.MainPlayer?.Skills))
            {
                return;
            }

            if (SkillMultipliers.IsLocalClass(EClassId.Scavenger))
            {
                __result = true;   // habilita o teto de 2 buscas simultâneas (mesmo do elite vanilla)
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] Quick Hands falhou: {ex.Message}");
        }
    }
}
