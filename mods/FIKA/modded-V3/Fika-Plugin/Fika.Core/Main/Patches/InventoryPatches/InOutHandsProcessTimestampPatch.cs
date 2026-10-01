// © 2026 Lacyway All Rights Reserved

using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace Fika.Core.Main.Patches.InventoryPatches;

/// <summary>
/// Correlaciona cada janela "entrando/saindo das mãos" (GEventArgs17/Begin-Succeed) com o item
/// que efetivamente a abriu — a própria arma (saque/guarda) ou um item aninhado nela (ex.: um
/// carregador, num reload/swap 1-para-1). Consumido por ObservedInventoryController.CheckItemAction
/// (ver ref: PA-01-02) para reconhecer, dentro de uma janela curta, que a colisão detectada é a
/// própria troca de magazine em andamento — nunca um saque de arma real ou outra operação
/// concorrente genuína.
///
/// Dois pontos de captura, porque ObservedInventoryController.InProcess é sobrescrito e chama
/// RaiseInOutProcessEvents diretamente, sem passar por Player.TrySetInHands (confirmado em
/// ObservedInventoryController.cs:260-296 durante o /code-mod — a spec técnica original previa um
/// segundo Harmony patch em TrySetInHands que nunca dispararia para o caminho observado/Headless):
///   1. Lado "remover" (OutProcess, não sobrescrito por ObservedInventoryController): Harmony
///      Prefix em Player.TryRemoveFromHands captura o item removido.
///   2. Lado "inserir" (InProcess, sobrescrito): chamada direta a SetPendingMovedItem a partir de
///      ObservedInventoryController.HandleInProcess, sem necessidade de Harmony.
/// </summary>
public static class InOutHandsProcessTimestampPatch
{
    private sealed class Entry
    {
        public Item MovedItem;
        public float Timestamp;
    }

    // ref: Assembly-CSharp/TraderControllerClass.cs:1887 — RaiseInOutProcessEvents(GEventArgs17 args)
    private static readonly ConditionalWeakTable<TraderControllerClass, Dictionary<Item, Entry>> _state = new();

    // Campo "ambiente": item efetivamente movido (a arma, num saque/guarda; um item aninhado —
    // ex. carregador — num reload/swap). Escrito pelo Prefix de TryRemoveFromHands ou diretamente
    // por ObservedInventoryController.HandleInProcess; consumido e limpo pelo Postfix de
    // RaiseInOutProcessEvents assim que um Begin é levantado. Main-thread only (fluxo de
    // input/rede do EFT), sem concorrência a proteger.
    private static Item _pendingMovedItem;

    /// <summary>Chamado diretamente por ObservedInventoryController.HandleInProcess (sem Harmony — código do próprio mod).</summary>
    public static void SetPendingMovedItem(Item item)
    {
        _pendingMovedItem = item;
    }

    /// <summary>Se há um Begin pendente para essa arma, diz há quanto tempo e qual item o abriu.</summary>
    public static bool TryGetPendingBegin(TraderControllerClass controller, Item weapon, out Item movedItem, out float elapsedSeconds)
    {
        movedItem = null;
        elapsedSeconds = 0f;
        if (weapon == null || !_state.TryGetValue(controller, out var perWeapon) || !perWeapon.TryGetValue(weapon, out var entry))
        {
            return false;
        }

        movedItem = entry.MovedItem;
        elapsedSeconds = Time.time - entry.Timestamp;
        return true;
    }

    public class CaptureMovedItemOnRemove : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // ref: Assembly-CSharp/EFT/Player.cs:32223
            return AccessTools.Method(typeof(Player), nameof(Player.TryRemoveFromHands));
        }

        [PatchPrefix]
        public static void Prefix(Item item)
        {
            _pendingMovedItem = item;
        }

        [PatchPostfix]
        public static void Postfix()
        {
            // Limpeza defensiva: cobre os ramos de TryRemoveFromHands que retornam sem nunca
            // levantar um Begin (ex.: HandsController == null, CanExecute == false) — nesses
            // casos o Postfix de RaiseInOutProcessEvents nunca roda para consumir o valor.
            _pendingMovedItem = null;
        }
    }

    public class RecordBeginSucceed : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // ref: Assembly-CSharp/TraderControllerClass.cs:1887
            return AccessTools.Method(typeof(TraderControllerClass), nameof(TraderControllerClass.RaiseInOutProcessEvents));
        }

        [PatchPostfix]
        public static void Postfix(TraderControllerClass __instance, GEventArgs17 args)
        {
            if (args?.Item == null)
            {
                return;
            }

            if (args.Status == CommandStatus.Begin)
            {
                var movedItem = _pendingMovedItem;
                _pendingMovedItem = null;

                var perWeapon = _state.GetOrCreateValue(__instance);
                perWeapon[args.Item] = new Entry { MovedItem = movedItem, Timestamp = Time.time };
            }
            else if (args.Status == CommandStatus.Succeed)
            {
                if (_state.TryGetValue(__instance, out var perWeapon))
                {
                    perWeapon.Remove(args.Item);
                }
            }
        }
    }
}
