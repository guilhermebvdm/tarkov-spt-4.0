// © 2026 Lacyway All Rights Reserved

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using EFT;
using EFT.InventoryLogic;
using Fika.Core.Main.Utils;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace Fika.Core.Main.Patches.InventoryPatches;

/// <summary>
/// Correlaciona o Begin/Succeed de GEventArgs9 ("BeginSetInHands", Class1311) e GEventArgs10
/// ("BeginRemoveFromHands", Class1312) com um timestamp, por (controller, item). Consumido por
/// ObservedInventoryController.CheckItemAction para tolerar, dentro de uma janela curta, uma
/// colisão genérica (item == geventArgs2.Item) contra um evento de bookkeeping recente do MESMO
/// jogador no MESMO item — nunca concorrência real (List_0 já é escopado por jogador, item 004
/// §1.2) nem qualquer outro tipo de evento (GEventArgs2/3/7/8/17, que continuam bloqueando como
/// hoje). Mecanismo INDEPENDENTE de InOutHandsProcessTimestampPatch (item 003) — GEventArgs9/10
/// não têm o conceito de "item efetivamente movido" que GEventArgs17 tem (o item já É a própria
/// chave); ver spec técnica do item 006 §1.1.
///
/// Causa raiz que este patch endereça: FirearmController.Drop (Player.cs:13506-13524) abre um
/// GEventArgs10 Begin ANTES da animação de esconder a arma começar e só confirma quando ela
/// termina — usado por SetEmptyHands (Player.cs:31704), chamado pelo SPT-ContinuousLoadAmmo entre
/// cada carregador. O cliente local avança pro próximo carregador com base no SEU PRÓPRIO timing;
/// o Headless roda sua própria cópia dessa mesma animação/confirmação, sujeita a latência de rede
/// — se o próximo pedido chega no Headless antes do Confirm() anterior, a arma ainda tem um
/// GEventArgs10 pendente e o pedido é rejeitado (GClass1561).
/// </summary>
public static class HandsBookkeepingTimestampPatch
{
    // ref: Assembly-CSharp/TraderControllerClass.cs:1822 — method_19(GEventArgs1 args), hub
    // confirmado de Add/Remove de List_0 para TODOS os tipos de evento (RaiseInOutProcessEvents e
    // RaiseEvent(GEventArgs13) delegam pra ele — TraderControllerClass.cs:1887 e :1969-1971).
    private static readonly ConditionalWeakTable<TraderControllerClass, Dictionary<Item, float>> _state = new();

    /// <summary>Se há um evento de bookkeeping (GEventArgs9/10) pendente pra esse item, há quanto tempo.</summary>
    public static bool TryGetPendingTimestamp(TraderControllerClass controller, Item item, out float elapsedSeconds)
    {
        elapsedSeconds = 0f;
        if (item == null || !_state.TryGetValue(controller, out var perItem) || !perItem.TryGetValue(item, out var timestamp))
        {
            return false;
        }

        elapsedSeconds = Time.time - timestamp;
        return true;
    }

    public class RecordHandsBookkeepingEvent : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // ref: Assembly-CSharp/TraderControllerClass.cs:1822 — method_19(GEventArgs1 args).
            // TODO confirmar (AP-09): nome obfuscado sem wrapper nomeado equivalente ao
            // RaiseInOutProcessEvents pra GEventArgs9/10 (ver spec técnica do item 006 §2).
            // Validação de assinatura abaixo evita patchear silenciosamente o método errado se
            // renumerado — mas retornar null AQUI lança exceção em Enable(); o registro deste
            // patch em FikaPlugin.cs precisa estar protegido por try/catch (ver comentário lá).
            var candidates = AccessTools.GetDeclaredMethods(typeof(TraderControllerClass))
                .Where(m => m.Name == "method_19"
                    && m.GetParameters().Length == 1
                    && m.GetParameters()[0].ParameterType == typeof(GEventArgs1)
                    && m.ReturnType == typeof(void))
                .ToList();

            if (candidates.Count != 1)
            {
                FikaGlobals.LogError(
                    $"HandsBookkeepingTimestampPatch: esperava exatamente 1 candidato pra " +
                    $"TraderControllerClass.method_19(GEventArgs1), achou {candidates.Count}. " +
                    "Patch NÃO habilitado — Fix 1b do item 006 fica inerte até isso ser corrigido.");
                return null;
            }

            return candidates[0];
        }

        [PatchPostfix]
        public static void Postfix(TraderControllerClass __instance, GEventArgs1 args)
        {
            if (args is not (GEventArgs9 or GEventArgs10) || args.Item == null)
            {
                return;
            }

            if (args.Status == CommandStatus.Begin)
            {
                _state.GetOrCreateValue(__instance)[args.Item] = Time.time;
            }
            else if (_state.TryGetValue(__instance, out var perItem))
            {
                perItem.Remove(args.Item);
            }
        }
    }
}
