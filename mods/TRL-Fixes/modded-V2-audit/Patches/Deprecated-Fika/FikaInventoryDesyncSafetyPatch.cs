using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace TRLFixes.Patches
{
    /// <summary>
    /// Corrige e previne desyncs de grid, itens invisíveis/fantasmas e rejeições de inventário
    /// ("is taken by another item" / GClass1543) no modo cooperativo do FIKA e SPT.
    /// 
    /// O patch atua em 2 camadas de segurança robustas:
    /// 1. QuickMoveSlotReservation: Reserva temporariamente slots durante rajadas de Ctrl+Click rápido,
    ///    impedindo que dois cliques consecutivos calculem e disputem exatamente as mesmas coordenadas (x, y) no servidor.
    /// 2. InventoryRejectionAutoRecovery: Ao receber uma rejeição do servidor indicando que o slot está ocupado,
    ///    despacha na Main Thread a reconstrução geométrica do grid e o disparo de RaiseRefreshEvent no contêiner pai,
    ///    fazendo o item real reaparecer instantaneamente na tela do jogador sem exigir que ele jogue a mochila no chão.
    /// </summary>
    public class FikaInventoryDesyncSafetyPatch
    {
        private static Type _clientOpHandlerType;
        private static FieldInfo _operationField;
        private static FieldInfo _serverStatusStatusField;
        private static FieldInfo _serverStatusErrorField;

        // Estrutura leve e estática para reservas de QuickMove (CR-01-01 & CR-01-06)
        private struct SlotReservation
        {
            public string GridId;
            public string ContainerId;
            public int X;
            public int Y;
            public int Width;
            public int Height;
            public float ExpireTime;
        }

        private static readonly List<SlotReservation> _activeReservations = new List<SlotReservation>(16);
        private static readonly object _reservationLock = new object();
        private const float ReservationDurationSeconds = 1.5f;

        // Componente MonoBehaviour para despachar ações na Main Thread de forma 100% segura
        private static MainThreadDispatcher _dispatcher;

        public void Enable()
        {
            try
            {
                var harmony = new Harmony("com.trl.fixes.fikainventorydesync");

                // 1. Inicializa o MainThreadDispatcher
                InitDispatcher();

                // 2. Patch em StashGridClass.FindFreeSpace para Reserva Preemptiva em QuickMove
                var findFreeSpaceMethod = AccessTools.Method(typeof(StashGridClass), nameof(StashGridClass.FindFreeSpace));
                if (findFreeSpaceMethod != null)
                {
                    var postfixFindFreeSpace = AccessTools.Method(typeof(FikaInventoryDesyncSafetyPatch), nameof(Postfix_FindFreeSpace));
                    harmony.Patch(findFreeSpaceMethod, postfix: new HarmonyMethod(postfixFindFreeSpace));
                    Plugin.Log?.LogInfo("TRL-Fixes: QuickMoveSlotReservationPatch aplicado com sucesso em StashGridClass.FindFreeSpace.");
                }

                // 3. Patch em Fika ClientInventoryOperationHandler para Auto-Recuperação Visual em Rejeição
                _clientOpHandlerType = AccessTools.TypeByName("Fika.Core.Main.ClientClasses.ClientInventoryOperationHandler");
                if (_clientOpHandlerType != null)
                {
                    _operationField = AccessTools.Field(_clientOpHandlerType, "Operation");
                    
                    var receiveStatusMethod = AccessTools.Method(_clientOpHandlerType, "ReceiveStatusFromServer");
                    if (receiveStatusMethod != null)
                    {
                        var postfixReceiveStatus = AccessTools.Method(typeof(FikaInventoryDesyncSafetyPatch), nameof(Postfix_ReceiveStatusFromServer));
                        harmony.Patch(receiveStatusMethod, postfix: new HarmonyMethod(postfixReceiveStatus));
                        Plugin.Log?.LogInfo("TRL-Fixes: InventoryRejectionAutoRecoveryPatch aplicado com sucesso em ClientInventoryOperationHandler.ReceiveStatusFromServer.");
                    }
                }
                else
                {
                    Plugin.Log?.LogInfo("TRL-Fixes: ClientInventoryOperationHandler do FIKA não detectado (modo singleplayer ou host puro).");
                }

                Plugin.Log?.LogInfo("TRL-Fixes: FikaInventoryDesyncSafetyPatch completamente ativado!");
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"TRL-Fixes: Erro ao aplicar FikaInventoryDesyncSafetyPatch: {ex}");
            }
        }

        private static void InitDispatcher()
        {
            if (_dispatcher == null)
            {
                var go = new GameObject("TRLFixes_InventoryDispatcher");
                GameObject.DontDestroyOnLoad(go);
                _dispatcher = go.AddComponent<MainThreadDispatcher>();
            }
        }

        #region 1. QuickMove Slot Reservation (Prevenção de Colisão Concorrente)

        public static void Postfix_FindFreeSpace(StashGridClass __instance, Item item, ref LocationInGrid __result)
        {
            if (__instance == null || item == null || __result == null)
            {
                return;
            }

            try
            {
                lock (_reservationLock)
                {
                    float now = Time.time;
                    ClearExpiredReservations(now);

                    string gridId = __instance.ID ?? string.Empty;
                    string containerId = __instance.ParentItem?.Id ?? string.Empty;

                    var itemSize = item.CalculateRotatedSize(__result.r);

                    // Verifica se o slot calculado colide com alguma reserva ativa
                    bool hasCollision = false;
                    for (int i = 0; i < _activeReservations.Count; i++)
                    {
                        var res = _activeReservations[i];
                        if (res.GridId == gridId && res.ContainerId == containerId)
                        {
                            if (IsRectOverlapping(__result.x, __result.y, itemSize.X, itemSize.Y, res.X, res.Y, res.Width, res.Height))
                            {
                                hasCollision = true;
                                break;
                            }
                        }
                    }

                    if (hasCollision)
                    {
                        // Procura uma posição alternativa que não colida com as reservas
                        var alternativeLocation = FindAlternativeFreeSpace(__instance, item, itemSize);
                        if (alternativeLocation != null)
                        {
                            __result = alternativeLocation;
                        }
                    }

                    // Registra a reserva do slot atual
                    _activeReservations.Add(new SlotReservation
                    {
                        GridId = gridId,
                        ContainerId = containerId,
                        X = __result.x,
                        Y = __result.y,
                        Width = itemSize.X,
                        Height = itemSize.Y,
                        ExpireTime = now + ReservationDurationSeconds
                    });
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogWarning($"TRL-Fixes: Exceção leve no QuickMoveSlotReservation: {ex.Message}");
            }
        }

        private static void ClearExpiredReservations(float now)
        {
            for (int i = _activeReservations.Count - 1; i >= 0; i--)
            {
                if (now > _activeReservations[i].ExpireTime)
                {
                    _activeReservations.RemoveAt(i);
                }
            }
        }

        private static bool IsRectOverlapping(int x1, int y1, int w1, int h1, int x2, int y2, int w2, int h2)
        {
            return x1 < x2 + w2 && x1 + w1 > x2 && y1 < y2 + h2 && y1 + h1 > y2;
        }

        private static LocationInGrid FindAlternativeFreeSpace(StashGridClass grid, Item item, XYCellSizeStruct itemSize)
        {
            // Varre o grid buscando coordenadas onde não haja colisão nem no grid nem nas reservas
            for (int y = 0; y <= grid.GridHeight - itemSize.Y; y++)
            {
                for (int x = 0; x <= grid.GridWidth - itemSize.X; x++)
                {
                    var loc = new LocationInGrid(x, y, ItemRotation.Horizontal);
                    if (grid.method_1(itemSize, loc))
                    {
                        bool collidesWithReservation = false;
                        string gridId = grid.ID ?? string.Empty;
                        string containerId = grid.ParentItem?.Id ?? string.Empty;

                        for (int i = 0; i < _activeReservations.Count; i++)
                        {
                            var res = _activeReservations[i];
                            if (res.GridId == gridId && res.ContainerId == containerId)
                            {
                                if (IsRectOverlapping(x, y, itemSize.X, itemSize.Y, res.X, res.Y, res.Width, res.Height))
                                {
                                    collidesWithReservation = true;
                                    break;
                                }
                            }
                        }

                        if (!collidesWithReservation)
                        {
                            return loc;
                        }
                    }
                }
            }
            return null;
        }

        #endregion

        #region 2. Auto-Recuperação Visual em Rejeição de Servidor (Ghost Item Recovery)

        public static void Postfix_ReceiveStatusFromServer(object __instance, object serverStatus)
        {
            if (__instance == null || serverStatus == null)
            {
                return;
            }

            try
            {
                // Extrai status e erro do ServerOperationStatus via Reflection
                if (_serverStatusStatusField == null || _serverStatusErrorField == null)
                {
                    var statusType = serverStatus.GetType();
                    _serverStatusStatusField = AccessTools.Field(statusType, "Status");
                    _serverStatusErrorField = AccessTools.Field(statusType, "Error");
                }

                if (_serverStatusStatusField != null && _serverStatusErrorField != null)
                {
                    var statusVal = _serverStatusStatusField.GetValue(serverStatus)?.ToString();
                    var errorVal = _serverStatusErrorField.GetValue(serverStatus)?.ToString() ?? string.Empty;

                    // Se a operação falhou e o motivo for colisão de slot ou item fantasma
                    if (statusVal == "Failed" || statusVal == "2")
                    {
                        if (errorVal.IndexOf("is taken by another item", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            errorVal.IndexOf("GClass1543", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            errorVal.IndexOf("SlotTakenError", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            var operation = _operationField?.GetValue(__instance) as BaseInventoryOperationClass;
                            if (operation != null)
                            {
                                TriggerContainerVisualRefresh(operation);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogWarning($"TRL-Fixes: Exceção no InventoryRejectionAutoRecovery: {ex.Message}");
            }
        }

        private static void TriggerContainerVisualRefresh(BaseInventoryOperationClass operation)
        {
            try
            {
                // Se for uma operação de movimento (MoveOperationClass), recupera o contêiner de destino
                var toProperty = AccessTools.Property(operation.GetType(), "To");
                var toAddress = toProperty?.GetValue(operation) as ItemAddress;

                CompoundItem targetContainer = null;
                StashGridClass targetGrid = null;

                if (toAddress is GClass3393 gridAddress)
                {
                    targetGrid = gridAddress.Grid as StashGridClass;
                    targetContainer = targetGrid?.ParentItem;
                }
                else if (toAddress?.Container is StashGridClass containerGrid)
                {
                    targetGrid = containerGrid;
                    targetContainer = containerGrid.ParentItem;
                }

                if (_dispatcher != null)
                {
                    _dispatcher.Enqueue(() =>
                    {
                        try
                        {
                            if (targetContainer != null)
                            {
                                targetContainer.RaiseRefreshEvent();
                                Plugin.Log?.LogInfo($"TRL-Fixes: Auto-recuperação visual disparada com sucesso para o contêiner: {targetContainer.Id} ({targetContainer.ShortName})");
                            }

                            if (targetGrid != null)
                            {
                                targetGrid.RaiseResizeEvent();
                            }
                        }
                        catch (Exception ex)
                        {
                            Plugin.Log?.LogWarning($"TRL-Fixes: Falha ao executar refresh visual na MainThread: {ex.Message}");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogWarning($"TRL-Fixes: Erro ao despachar TriggerContainerVisualRefresh: {ex.Message}");
            }
        }

        #endregion

        #region 3. MainThread Dispatcher Component

        private class MainThreadDispatcher : MonoBehaviour
        {
            private readonly Queue<Action> _executionQueue = new Queue<Action>(32);
            private readonly object _queueLock = new object();
            private const int MaxQueueSize = 100;

            public void Enqueue(Action action)
            {
                if (action == null) return;
                lock (_queueLock)
                {
                    // Proteção defensiva de limite de fila contra vazamento de memória (CR-02-02)
                    if (_executionQueue.Count >= MaxQueueSize)
                    {
                        _executionQueue.Clear();
                    }
                    _executionQueue.Enqueue(action);
                }
            }

            private void Update()
            {
                lock (_queueLock)
                {
                    while (_executionQueue.Count > 0)
                    {
                        try
                        {
                            _executionQueue.Dequeue()?.Invoke();
                        }
                        catch (Exception ex)
                        {
                            Plugin.Log?.LogError($"TRL-Fixes Dispatcher Error: {ex}");
                        }
                    }
                }
            }
        }

        #endregion
    }
}
