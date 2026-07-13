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
    /// <summary>
    ///     CR-F3 — profundidade, NÃO um bool. O <c>method_5</c> pode ser re-entrado (fila de body parts via
    ///     <c>method_8</c>; o Band_Aid o re-invoca por reflexão). Com um bool, o Postfix do nível INTERNO desarmaria
    ///     enquanto o externo ainda não chamou <c>SetUseTimeMultiplier</c> → efeito encurtado + animação em
    ///     velocidade vanilla: exatamente a dessincronia que este design existe para impedir. Com contador, é
    ///     estruturalmente impossível.
    /// </summary>
    private static int _depth;

    /// <summary>Fator de tempo da operação em curso (1 = sem efeito). Válido só dentro do escopo armado.</summary>
    internal static float PendingFactor = 1f;

    /// <summary>CR-F4 — a operação em curso é cirurgia? (o vanilla só divide o EFEITO por (1+SurgerySpeed) na cirurgia)</summary>
    internal static bool PendingIsSurgery;

    /// <summary>CR-F4 — <c>Skills.SurgerySpeed.Value</c> do dono da operação, capturado no arm.</summary>
    internal static float PendingSurgerySpeed;

    /// <summary>True enquanto o <c>method_5</c> do SEU player (Médico) está em execução.</summary>
    internal static bool Armed => _depth > 0;

    internal static void Arm(float factor, bool isSurgery, float surgerySpeed)
    {
        if (_depth == 0)
        {
            PendingFactor = factor;
            PendingIsSurgery = isSurgery;
            PendingSurgerySpeed = surgerySpeed;
        }

        _depth++;
    }

    /// <summary>Fecha UM nível do escopo. Chamado pelo <c>[PatchFinalizer]</c> — que roda mesmo se o original LANÇAR
    ///     (CR-F2: o Postfix não roda numa exceção, e o <c>DoMedEffect</c> lança <c>ArgumentException</c> de verdade).</summary>
    internal static void Release()
    {
        if (_depth > 0)
        {
            _depth--;
        }

        if (_depth == 0)
        {
            Reset();
        }
    }

    private static void Reset()
    {
        PendingFactor = 1f;
        PendingIsSurgery = false;
        PendingSurgerySpeed = 0f;
    }

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

    /// <summary>
    ///     CR-F6 — trava de segurança: se QUALQUER um dos 3 patches de tempo falhar ao aplicar, o perk inteiro é
    ///     desligado. Meio-perk (efeito curto + animação vanilla, ou o inverso) é pior que perk nenhum.
    /// </summary>
    private static bool _disabled;

    internal static void ForceDisable()
    {
        _disabled = true;
        _depth = 0;
        Reset();
    }

    /// <summary>Fator do item em uso: 🔧 Swift Surgeon (cirurgia) ou 🔧 Rapid Care (demais meds). 1 = perk off.</summary>
    internal static float FactorFor(Item? item)
    {
        if (_disabled)
        {
            return 1f;
        }

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

    /// <summary>
    ///     <b>CR-F1 — soft-reference ao TRL-ImmersiveCombatMedicine (Band_Aid).</b> Aquele mod REDIRECIONA a cura para
    ///     um ALIADO: o prefix dele chama <c>DoMedEffect</c> no HealthController do PACIENTE, de dentro do nosso
    ///     escopo armado. Sem este guard, o perk do Médico encurtaria o efeito aplicado <b>no corpo do outro</b> — e
    ///     pior, o Band_Aid espera um tempo FIXO (<c>UseTime + 2f</c>, tabela própria) sem ler o efeito encurtado, e
    ///     para paciente REMOTO nem existe MedEffect → a animação correria 1.43× contra uma espera fixa e o gesto
    ///     terminaria visivelmente antes da hora.
    ///     <para>
    ///     Resolvido por REFLEXÃO (nunca por referência dura): o CustomClasses não pode depender do outro mod.
    ///     Ausente → <c>false</c> → comportamento normal.
    ///     </para>
    /// </summary>
    private static readonly FieldInfo? BandAidRedirectFlag =
        AccessTools.Field(AccessTools.TypeByName("Band_Aid.MedicHealPatch"), "IsRedirectingHeal");

    internal static bool BandAidIsRedirecting()
    {
        try
        {
            return BandAidRedirectFlag?.GetValue(null) is true;
        }
        catch
        {
            return false;   // mod ausente / assinatura mudou → segue o fluxo normal
        }
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
    [HarmonyPriority(Priority.First)]   // abre o escopo ANTES de qualquer outro prefix (inclusive o do Band_Aid)
    private static void Prefix(Player.MedsController.ObservedMedsControllerClass __instance, out bool __state)
    {
        __state = false;   // "eu armei?" — é o que o Finalizer usa para fechar exatamente os níveis que abri

        try
        {
            if (!SkillMultipliers.IsLocalClass("Combat Medic"))
            {
                return;
            }

            // CR-F1: o Band_Aid redireciona a cura para um ALIADO de dentro deste escopo. O perk é SEU, não do
            // corpo do outro — e o tempo de espera dele é fixo, então encurtar só quebraria a animação.
            if (MedicTiming.BandAidIsRedirecting())
            {
                return;
            }

            var controller = __instance?.MedsController_0;
            if (controller == null || PlayerField?.GetValue(controller) is not Player player || !player.IsYourPlayer)
            {
                return;   // ⚠️ bots e peers Fika passam por aqui — é exatamente o vazamento que este gate evita
            }

            var item = controller.Item;
            var factor = MedicTiming.FactorFor(item);
            if (factor >= 1f)
            {
                return;   // perk desligado no F12 → caminho vanilla intacto
            }

            // CR-F4: capturamos a skill Surgery do dono p/ corrigir a animação da cirurgia (ver MedAnimSpeedPatch).
            var surgerySpeed = player.Skills?.SurgerySpeed?.Value ?? 0f;
            MedicTiming.Arm(factor, MedicTiming.IsSurgery(item), surgerySpeed);
            __state = true;
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (072) med scope (prefix) falhou: {ex.Message}");
        }
    }

    /// <summary>
    ///     CR-F2 — <b>Finalizer</b>, não Postfix. O HarmonyX emite os postfixes no fim do corpo: se o ORIGINAL lançar,
    ///     eles NÃO rodam. E o <c>DoMedEffect</c> lança de verdade (<c>ArgumentException("Item is neither Meds nor
    ///     Food.")</c>). Com um Postfix, o escopo ficaria armado até a próxima operação de qualquer um — e o perk
    ///     vazaria para o <c>UseTimeFor</c> seguinte, inclusive o de um peer. O Finalizer roda mesmo com exceção.
    /// </summary>
    [PatchFinalizer]
    private static void Finalizer(bool __state)
    {
        if (__state)
        {
            MedicTiming.Release();
        }
    }
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
///     <para>
///     <b>CR-F4 — corrigimos, DENTRO do escopo do perk, um bug de fábrica da BSG.</b> Na cirurgia o vanilla divide o
///     EFEITO por <c>(1 + SurgerySpeed)</c> (AHC:4300) mas multiplica a ANIMAÇÃO por <c>(1 + SurgerySpeed/100)</c>
///     (Player.cs:19562+19568) — <b>um fator 100 de discrepância</b>. Com a skill Surgery treinada, a operação já
///     termina no vanilla com parte da animação por tocar; o Swift Surgeon só herdaria o defeito e o deixaria mais
///     visível. Aqui trocamos o termo bugado (<c>S/100</c>) pelo termo real (<c>S</c>) — <b>só</b> quando o perk está
///     armado, então o comportamento vanilla de quem não é Médico fica intocado.
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

            // CR-F4: o `speed` que chega é `1 + S/100 (+0.2 se HP quase cheio)`. O `+0.2` já casa com o efeito
            // (o `num2` do DoMedEffect); o que NÃO casa é a skill. Substituímos S/100 por S — e só na cirurgia,
            // que é o único ramo em que o efeito de fato divide por (1 + S).
            if (MedicTiming.PendingIsSurgery)
            {
                var s = MedicTiming.PendingSurgerySpeed;
                speed += s - (s / 100f);
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
