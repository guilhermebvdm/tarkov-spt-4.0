using System;
using System.Reflection;
using EFT;
using EFT.HealthSystem;      // IEffect
using EFT.InventoryLogic;    // HealthEffectsComponent, EDamageEffectType, Item, MedsItemClass
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CustomClasses.Client;

/// <summary>
///     Item 072 — 🩺 estado compartilhado dos perks de tempo do <b>Médico de Combate</b>
///     (<see cref="MedsOperationScopePatch"/> arma · <see cref="MedUseTimePatch"/> encurta o EFEITO ·
///     <see cref="MedAnimSpeedPatch"/> acelera a ANIMAÇÃO na mesma proporção).
///     <para>
///     <b>Por que existe um "escopo armado" em vez de patchar o tempo direto.</b> O tempo de uso sai de
///     <c>HealthEffectsComponent.UseTimeFor(bodyPart)</c> — um componente <b>DO ITEM</b>, que não sabe QUEM está
///     usando. Um Postfix ali, gateado só em "a classe local é Médico", encurtaria também a cura de um <b>peer
///     Fika</b> processada no seu cliente (o `ObservedMedsController` do peer chama o mesmo caminho). O escopo é
///     armado no <c>method_5</c> da operação, que TEM o <c>_player</c> — então o perk só vale quando quem está
///     realmente usando o item é o SEU player.
///     </para>
/// </summary>
internal static class MedicTiming
{
    /// <summary>Fator de tempo da operação em curso (1 = sem efeito). Válido só entre o prefix e o postfix do method_5.</summary>
    internal static float PendingFactor = 1f;

    /// <summary>True enquanto o <c>method_5</c> do SEU player (Médico) está em execução.</summary>
    internal static bool Armed;

    /// <summary>
    ///     Cirurgia vs. curativo — o discriminador é do ITEM, não do body part: o kit de cirurgia (CMS/Surv12)
    ///     declara <c>DestroyedPart</c> nos efeitos. É o mesmo teste que o EFT usa no <c>DoMedEffect</c>
    ///     (<c>AffectsAny(EDamageEffectType.DestroyedPart)</c>) e no <c>NoMove</c> do MedEffect.
    /// </summary>
    internal static bool IsSurgery(Item? item)
    {
        return item is MedsItemClass meds
               && meds.HealthEffectsComponent != null
               && meds.HealthEffectsComponent.AffectsAny(EDamageEffectType.DestroyedPart);
    }

    /// <summary>Fator do item em uso: 🔧 Swift Surgeon (cirurgia) ou 🔧 Rapid Care (demais meds). 1 = perk off.</summary>
    internal static float FactorFor(Item? item)
    {
        if (IsSurgery(item))
        {
            return PerksConfig.SwiftSurgeonEnabled?.Value == true
                ? (PerksConfig.SwiftSurgeonTime?.Value ?? 0.5f)
                : 1f;
        }

        return PerksConfig.RapidCareEnabled?.Value == true
            ? (PerksConfig.RapidCareUseTime?.Value ?? 0.7f)
            : 1f;
    }

    internal static void Disarm()
    {
        Armed = false;
        PendingFactor = 1f;
    }
}

/// <summary>
///     Item 072 — abre/fecha o ESCOPO da operação de meds do SEU player (ver <see cref="MedicTiming"/>).
///     <para>
///     Alvo: <c>Player.MedsController.ObservedMedsControllerClass.method_5()</c> (Player.cs:19542). Dentro dele, na
///     ordem: <c>DoMedEffect(...)</c> (Player.cs:19553 → consome <c>UseTimeFor</c> = o EFEITO) e depois
///     <c>SetUseTimeMultiplier(...)</c> (Player.cs:19568 = a ANIMAÇÃO). É essa ordem que garante que os dois lados
///     leiam o MESMO fator e não dessincronizem — o risco central que adiou este item no 050.
///     </para>
///     <para>
///     ⚠️ <b>Convivência com o TRL-ImmersiveCombatMedicine</b>, que também tem um Prefix aqui e pode retornar
///     <c>false</c> (pulando o original): neste caso o original não roda, nada é encurtado, e o nosso Postfix
///     apenas desarma. Degrada para no-op — nunca para estado sujo.
///     </para>
/// </summary>
internal class MedsOperationScopePatch : ModulePatch
{
    // Reflection cacheada (csharp-mod-best-practices §3): `_player` é privado no MedsController.
    private static readonly FieldInfo? PlayerField = AccessTools.Field(typeof(Player.MedsController), "_player");

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player.MedsController.ObservedMedsControllerClass), "method_5");
    }

    [PatchPrefix]
    [HarmonyPriority(Priority.First)]   // abre o escopo ANTES de qualquer outro prefix (inclusive o do TRL)
    private static void Prefix(Player.MedsController.ObservedMedsControllerClass __instance)
    {
        try
        {
            MedicTiming.Disarm();   // estado limpo a cada operação (defesa contra um postfix perdido)

            if (!SkillMultipliers.IsLocalClass("Combat Medic"))
            {
                return;
            }

            var controller = __instance?.MedsController_0;
            if (controller == null || PlayerField?.GetValue(controller) is not Player player || !player.IsYourPlayer)
            {
                return;   // ⚠️ bots e peers Fika passam por aqui — é exatamente o vazamento que este gate evita
            }

            var factor = MedicTiming.FactorFor(controller.Item);
            if (factor >= 1f)
            {
                return;   // perk desligado no F12 → caminho vanilla intacto
            }

            MedicTiming.Armed = true;
            MedicTiming.PendingFactor = factor;
        }
        catch (Exception ex)
        {
            MedicTiming.Disarm();
            Plugin.Log?.LogError($"[CustomClasses] (072) med scope (prefix) falhou: {ex.Message}");
        }
    }

    [PatchPostfix]
    private static void Postfix() => MedicTiming.Disarm();
}

/// <summary>
///     Item 072 — 🔧 <b>Rapid Care</b> (cura/estabilização ×0.7) e 🔧 <b>Swift Surgeon</b> (cirurgia ×0.5): o EFEITO.
///     <para>
///     <c>HealthEffectsComponent.UseTimeFor(EBodyPart)</c> é o tempo-base que o <c>DoMedEffect</c> consome
///     (decompile da DLL real: <c>float num = medsItemClass.HealthEffectsComponent.UseTimeFor(value);</c>). Encurtar
///     aqui encurta a operação de verdade — a operação termina quando o EFEITO morre, não quando a animação acaba.
///     </para>
///     <para>
///     ⚠️ Só age dentro do escopo armado (<see cref="MedsOperationScopePatch"/>): este componente é do ITEM e não
///     sabe quem o usa. Sem o escopo, o perk do Médico local encurtaria a cura de um peer.
///     </para>
///     <para>
///     ℹ️ O vanilla ainda aplica, DEPOIS disto, <c>num /= (1 + SurgerySpeed)</c> — mas só no ramo de cirurgia. Os
///     dois se compõem por multiplicação; a skill do jogador segue valendo por cima do perk.
///     </para>
/// </summary>
internal class MedUseTimePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        // public float UseTimeFor(EBodyPart bodyPart) — EFT.InventoryLogic.HealthEffectsComponent
        return AccessTools.Method(typeof(HealthEffectsComponent), nameof(HealthEffectsComponent.UseTimeFor));
    }

    [PatchPostfix]
    private static void Postfix(ref float __result)
    {
        try
        {
            if (!MedicTiming.Armed)
            {
                return;
            }

            __result *= MedicTiming.PendingFactor;
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (072) med use time falhou: {ex.Message}");
        }
    }
}

/// <summary>
///     Item 072 — o outro lado da moeda: a ANIMAÇÃO. Sem isto, o item curaria em 0.7× do tempo mas a mão continuaria
///     no gesto completo — a operação terminaria com a animação cortada no meio (a dessincronia que o 050 temia).
///     <para>
///     <c>FirearmsAnimator.SetUseTimeMultiplier(float speed)</c> é a VELOCIDADE da animação (param float do Animator).
///     Efeito ×0.7 ⇒ animação precisa correr 1/0.7 ≈ 1.43× mais rápido. Como o vanilla chama isto DEPOIS do
///     <c>DoMedEffect</c> dentro do mesmo <c>method_5</c>, o fator armado ainda está de pé — os dois lados batem.
///     </para>
/// </summary>
internal class MedAnimSpeedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(FirearmsAnimator), nameof(FirearmsAnimator.SetUseTimeMultiplier));
    }

    [PatchPrefix]
    private static void Prefix(ref float speed)
    {
        try
        {
            if (!MedicTiming.Armed || MedicTiming.PendingFactor <= 0f)
            {
                return;
            }

            speed /= MedicTiming.PendingFactor;   // ×0.7 no efeito → ÷0.7 na animação
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (072) med anim speed falhou: {ex.Message}");
        }
    }
}

/// <summary>
///     Item 072 — 🔧 <b>Mobile Surgery / Cirurgia em Movimento</b> (Médico): operar ANDANDO.
///     <para>
///     O 050 deu este perk como "não localizável no estático". <b>É localizável</b> — o que faltava era o decompile
///     (o namespace <c>EFT.HealthSystem</c> está VAZIO no dump versionado do repo). A cadeia real:
///     <c>DoMedEffect</c> → <c>MedEffect.Started</c> → <c>Player.OnHealthEffectAdded</c> (Player.cs:28939), que faz:
///     <code>
///     MovementContext.SetPhysicalCondition(EPhysicalCondition.UsingMeds,   true);   // qualquer med
///     if (medEffect.NoMove)
///         MovementContext.SetPhysicalCondition(EPhysicalCondition.HealingLegs, true);   // ← ENRAÍZA
///     </code>
///     e <c>HealingLegs</c> é o único motivo de <c>MovementContext.CanWalk == false</c> (fora de colisão física),
///     que por sua vez faz o <c>IdleStateClass.Move()</c> DESCARTAR o input de andar.
///     </para>
///     <para>
///     O perk desliga <b>só</b> <c>HealingLegs</c> e <b>preserva</b> <c>UsingMeds</c> → o Médico passa a ANDAR durante
///     a cirurgia, mas segue sem correr/pular/deitar. É literalmente "a cirurgia passa a se comportar como curativo".
///     Seguro: <c>HealingLegs</c> tem só 2 escritores no assembly e NÃO é re-setado por frame, então o Postfix não é
///     desfeito no frame seguinte.
///     </para>
///     <para>
///     ⚠️ Gate por <c>DestroyedPart</c> (= kit de cirurgia) DE PROPÓSITO: <c>NoMove</c> também é true ao usar tala em
///     perna JÁ FRATURADA. Esse lock é vanilla e não é "cirurgia" — fica intacto.
///     </para>
///     <para>
///     ⚠️ <b>Pendente de validação in-game:</b> o código diz que nada mais trava o jogador, mas um lock de animação
///     (Mecanim, camada full-body) não é inspecionável no C#. Teste: iniciar cirurgia e apertar W.
///     </para>
/// </summary>
internal class MobileSurgeryPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.OnHealthEffectAdded));
    }

    [PatchPostfix]
    private static void Postfix(Player __instance, IEffect effect)
    {
        try
        {
            if (PerksConfig.MobileSurgeryEnabled?.Value != true
                || __instance == null || !__instance.IsYourPlayer
                || !SkillMultipliers.IsLocalClass("Combat Medic"))
            {
                return;
            }

            // O efeito de med expõe `MedItem` (interface obfuscada — resolvemos pelo TIPO CONCRETO, que sobrevive
            // à renumeração de GInterfaceNNNN entre builds do EFT).
            if (effect == null || AccessTools.Property(effect.GetType(), "MedItem")?.GetValue(effect) is not Item item)
            {
                return;
            }

            if (!MedicTiming.IsSurgery(item))
            {
                return;   // tala em perna fraturada também enraíza — comportamento vanilla, preservado
            }

            __instance.MovementContext?.SetPhysicalCondition(EPhysicalCondition.HealingLegs, val: false);
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (072) mobile surgery falhou: {ex.Message}");
        }
    }
}
