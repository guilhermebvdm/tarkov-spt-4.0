// © 2026 Lacyway All Rights Reserved

using System;
using BepInEx.Logging;
using EFT;
using EFT.InventoryLogic;

namespace Fika.Core.Main.ObservedClasses.HandsControllers;

/// <summary>
///     Item 005 — ponto de extensão GENÉRICO (sem conhecimento de nenhum mod específico) pra ajustar a
///     velocidade da animação de uma operação de meds REPLICADA (o cliente de quem OBSERVA um peer usando um
///     item médico). O Fika, sozinho, só ajusta essa velocidade pela skill Cirurgia nativa
///     (<see cref="ObservedMedsController"/>); um mod que queira refletir, pros observadores, um perk de
///     classe/velocidade próprio atribui <see cref="ExtraSpeedMultiplier"/>.
///     <para>
///     Contrato: <c>null</c> (ninguém assinou) ou uma exceção/valor inválido do delegate ⇒ multiplicador extra
///     = 1 (sem efeito, comportamento idêntico a antes deste item). O Fika NUNCA lança por causa de um
///     assinante malcomportado — ver <see cref="ResolveExtra"/>.
///     </para>
///     <para>
///     <b>Limitação consciente:</b> um único delegate (last-write-wins), não um evento multicast. Se um 2º mod
///     precisar do mesmo hook simultaneamente, ele precisa compor com o valor anterior por conta própria antes
///     de reatribuir — não há suporte nativo a múltiplos assinantes independentes.
///     </para>
///     <para>
///     <b>Ciclo de vida:</b> atribuir <see cref="ExtraSpeedMultiplier"/> UMA ÚNICA VEZ, no <c>Awake()</c> do
///     mod consumidor (ou assim que ele detectar que o Fika está carregado). O campo é <c>static</c> e vale
///     pra sessão inteira do processo — não há hook de raid-start/raid-end aqui, e não é necessário (nem
///     esperado) reatribuir por raid.
///     </para>
/// </summary>
public static class ObservedMedsSpeedHook
{
    /// <summary>
    ///     Recebe o <see cref="Player"/> (peer observado) executando a operação e o <see cref="Item"/> em uso;
    ///     retorna o multiplicador EXTRA a compor sobre o que o Fika já calcula (1 = sem efeito). Atribuir
    ///     <c>null</c> remove o hook. Hoje sempre chamado com argumentos não-nulos (ver <see cref="ResolveExtra"/>),
    ///     mas consumidores devem tratar como robustez, não garantia contratual permanente.
    ///     <para>
    ///     ref: CR-01-02 — chamado de forma SÍNCRONA, na main thread, de dentro de um callback de gameplay (por
    ///     parte do corpo curada). Mantenha a implementação rápida e sem I/O/alocação pesada — um delegate lento
    ///     aqui causa hitch perceptível a cada parte curada, de QUALQUER peer observado.
    ///     </para>
    /// </summary>
    public static Func<Player, Item, float> ExtraSpeedMultiplier;

    private static ManualLogSource Log => FikaPlugin.Instance?.FikaLogger;

    /// <summary>Invocação protegida — usada pelos 2 call-sites de <see cref="ObservedMedsController"/>.
    /// Nunca lança; sempre devolve um multiplicador finito e positivo.</summary>
    internal static float ResolveExtra(Player player, Item item)
    {
        // Guard defensivo — hoje os 2 call-sites sempre passam player/item não-nulos, mas não é um contrato
        // garantido (um 3º call-site futuro, ou mudança de ordem no Fika, poderia mudar isso). Silencioso
        // (sem log): não é uma falha, é o hook simplesmente não se aplicando a um estado sem sentido.
        if (player == null || item == null)
        {
            return 1f;
        }

        var hook = ExtraSpeedMultiplier;
        if (hook == null)
        {
            return 1f;
        }

        try
        {
            var value = hook(player, item);
            return float.IsFinite(value) && value > 0f ? value : 1f;
        }
        catch (Exception ex)
        {
            Log?.LogError($"[ObservedMedsSpeedHook] assinante lançou exceção — ignorando (extra=1): {ex}");
            return 1f;
        }
    }
}
