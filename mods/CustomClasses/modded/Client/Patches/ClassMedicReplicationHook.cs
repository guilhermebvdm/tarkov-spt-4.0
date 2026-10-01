using System;
using System.Reflection;
using EFT;
using EFT.InventoryLogic; // Item
using HarmonyLib;

namespace CustomClasses.Client;

/// <summary>
///     Item 090 — assina o hook genérico do FIKA (<c>ObservedMedsSpeedHook.ExtraSpeedMultiplier</c>, item
///     FIKA/005) pra replicar, nos clientes que OBSERVAM um Médico de Combate (auto-cura OU cura de aliado via
///     TRL-ImmersiveCombatMedicine), o mesmo fator de velocidade que os itens 072/077 já aplicam no cliente do
///     PRÓPRIO operador. Sem protocolo novo: a classe do operador já é resolvida pelo mapa nickname→classe
///     existente (<see cref="ClassIdentities.ClassIdOf"/>, item 057), e o VALOR do perk vem do F12 de quem
///     roda isto — mesma premissa "config idêntica p/ todos" já usada pelos itens 065/066.
/// </summary>
internal static class ClassMedicReplicationHook
{
    // Resolvido só o TIPO por reflection (não membros privados) — evita referência de build ao Fika.Core.dll,
    // então este arquivo compila e funciona igual em SP (Fika ausente: Resolve() vira no-op, fail-open).
    private static readonly Type? HookType =
        AccessTools.TypeByName("Fika.Core.Main.ObservedClasses.HandsControllers.ObservedMedsSpeedHook");

    private static readonly FieldInfo? ExtraSpeedMultiplierField =
        HookType != null ? AccessTools.Field(HookType, "ExtraSpeedMultiplier") : null;

    /// <summary>Chamado 1x no Awake do Plugin (dentro de try/catch, molde do ExecutionSpeedCapPatch).
    /// Depende de <c>[BepInDependency("com.fika.core", SoftDependency)]</c> em Plugin.cs (PA-01-01) —
    /// sem isso, a ordem de carregamento entre CustomClasses e Fika não é garantida, e este método pode
    /// nunca resolver o hook mesmo com o Fika instalado.</summary>
    internal static void Register()
    {
        if (ExtraSpeedMultiplierField == null)
        {
            // PA-01-02: aviso (não erro) — cenário esperado em SP (Fika ausente) OU um Fika/fork sem o
            // hook do item 005. Diagnosticável sem poluir o log com "erro" quando é só ausência normal.
            Plugin.Log?.LogWarning("[CustomClasses] (090) ObservedMedsSpeedHook não encontrado — Fika ausente ou fork sem o hook do item FIKA/005 (sem efeito, fail-open).");
            return;
        }

        // ref: CR-01-01 — o hook é last-write-wins (item FIKA/005); avisar se alguém já assinou antes de nós,
        // pra não silenciar um conflito com outro mod que também use ObservedMedsSpeedHook.
        if (ExtraSpeedMultiplierField.GetValue(null) != null)
        {
            Plugin.Log?.LogWarning("[CustomClasses] (090) sobrescrevendo um ExtraSpeedMultiplier já assinado por outro mod (last-write-wins) — comportamento pode não ser o esperado.");
        }

        Func<Player, Item, float> del = ResolveFactor;
        ExtraSpeedMultiplierField.SetValue(null, del);
        Plugin.Log?.LogInfo("[CustomClasses] (090) hook de velocidade de cura (Fika) assinado com sucesso.");   // PA-01-02
    }

    /// <summary>Assinatura EXATA exigida pelo hook (Fika.Core): Func&lt;Player, Item, float&gt;.
    /// Retorna 1f (sem efeito) se o dono não for Médico de Combate ou o perk estiver off.</summary>
    private static float ResolveFactor(Player fikaPlayer, Item item)
    {
        try
        {
            // ref: ClassIdentities.cs:143-151 — já barra IsAI (bot) e resolve local-vs-peer sozinho.
            if (ClassIdentities.ClassIdOf(fikaPlayer) != EClassId.CombatMedic)
            {
                return 1f;
            }

            var isSurgery = MedicTiming.IsSurgery(item);      // ref: ClassMedicPatches.cs:86-91
            var factor = MedicTiming.FactorFor(isSurgery);    // ref: ClassMedicPatches.cs:114-131 (F12 local)
            return factor > 0f ? 1f / factor : 1f;            // extra = inverso do fator (0.7 no efeito ⇒ ÷0.7 na anim)
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (090) ResolveFactor falhou: {ex.Message}");
            return 1f;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════════════════════
    // FALLBACK (Opção A, NÃO implementado) — fórmula de referência caso um dia seja preciso rodar
    // contra um Fika VANILLA/não-forkado (sem o hook do item FIKA/005). Reflection direta contra a
    // classe privada aninhada `ObservedMedsController.ObservedMedsOperation`:
    //
    //   var outerType = AccessTools.TypeByName(
    //       "Fika.Core.Main.ObservedClasses.HandsControllers.ObservedMedsController");
    //   var innerType = AccessTools.Inner(outerType, "ObservedMedsOperation");
    //   var observedControllerField = AccessTools.Field(innerType, "_observedMedsController"); // ObservedMedsController.cs:129
    //   var fikaPlayerField = AccessTools.Field(outerType, "_fikaPlayer");                      // ObservedMedsController.cs:16
    //
    //   // 2 Harmony Postfix (ModulePatch), targets resolvidos via innerType:
    //   //   AccessTools.Method(innerType, "ObservedStart", new[] { typeof(Action) })
    //   //   AccessTools.Method(innerType, "HealthController_EffectRemovedEvent", new[] { typeof(IEffect) })
    //   // Cada Postfix: ler _observedMedsController (do __instance) → _fikaPlayer (do controller) →
    //   // ClassIdOf/IsSurgery/FactorFor (igual acima) → (controller as Player.AbstractHandsController)
    //   //   ?.FirearmsAnimator?.SetUseTimeMultiplier(valor). No 2º patch, reconstruir a fórmula nativa
    //   // do Fika ANTES de aplicar o fator: `(1f + fikaPlayer.Skills.SurgerySpeed.Value/100f) / factor`
    //   // (ObservedMedsController.cs:182-183 — não há getter pro multiplicador já aplicado).
    //
    // Ver histórico da spec técnica deste item (090-...-02-spec-tech.md) pela versão completa desses
    // 2 stubs, escrita antes da decisão do usuário de usar o hook do FIKA/005 em vez de reflection.
    // ═══════════════════════════════════════════════════════════════════════════════════════════
}
