using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace TRLFixes.Patches
{
    /// <summary>
    /// Corrige a colisão de chaves e o erro "CRITICAL ERROR DICTIONARY" em ObservedPlayer.RefreshSlotViews.
    /// No Fika nativo, ao reconstruir os slots da arma observada (ex.: após re-sincronização de inventário),
    /// os slots eram inseridos em um Dictionary indexado por slot.FullId. Armas com múltiplos slots de mesmo
    /// identificador (ex.: múltiplos adaptadores/trilhos mod_tactical) causavam colisão de chaves e disparavam
    /// o log alarmista de erro crítico.
    /// 
    /// Este patch substitui a indexação vulnerável por uma lista de pares chave-valor segura, preservando
    /// a vinculação correta dos ContainerBones para todos os slots sem emitir erros falsos.
    /// </summary>
    public class FikaRefreshSlotViewsSafetyPatch
    {
        private static Type _observedPlayerType;
        private static Type _observedSlotViewHandlerType;
        private static ConstructorInfo _handlerConstructor;
        private static FieldInfo _handlersListField;

        public void Enable()
        {
            try
            {
                var harmony = new Harmony("com.trl.fixes.fikarefreshslotviews");
                _observedPlayerType = AccessTools.TypeByName("Fika.Core.Main.Players.ObservedPlayer");

                if (_observedPlayerType == null)
                {
                    Plugin.Log?.LogInfo("TRL-Fixes: ObservedPlayer não detectado. FikaRefreshSlotViewsSafetyPatch não será ativado.");
                    return;
                }

                _observedSlotViewHandlerType = AccessTools.Inner(_observedPlayerType, "ObservedSlotViewHandler");
                if (_observedSlotViewHandlerType != null)
                {
                    _handlerConstructor = _observedSlotViewHandlerType.GetConstructor(new[] { typeof(Slot), _observedPlayerType, typeof(EquipmentSlot) });
                }

                _handlersListField = AccessTools.Field(_observedPlayerType, "_observedSlotViewHandlers");

                var targetMethod = AccessTools.Method(_observedPlayerType, "RefreshSlotViews");
                if (targetMethod != null)
                {
                    var prefixMethod = AccessTools.Method(typeof(FikaRefreshSlotViewsSafetyPatch), nameof(Prefix));
                    harmony.Patch(targetMethod, prefix: new HarmonyMethod(prefixMethod));
                    Plugin.Log?.LogInfo("TRL-Fixes: FikaRefreshSlotViewsSafetyPatch aplicado com sucesso em ObservedPlayer.RefreshSlotViews!");
                }
                else
                {
                    Plugin.Log?.LogWarning("TRL-Fixes: ObservedPlayer.RefreshSlotViews não encontrado!");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"TRL-Fixes: Erro ao aplicar FikaRefreshSlotViewsSafetyPatch: {ex}");
            }
        }

        public static bool Prefix(Player __instance)
        {
            try
            {
                if (__instance == null || __instance.PlayerBody == null || __instance.Inventory == null || __instance.Inventory.Equipment == null)
                {
                    return true;
                }

                // 1. Atualiza ObservedSlotViewHandlers para os slots do corpo do jogador
                IList handlersList = null;
                if (_handlersListField != null)
                {
                    handlersList = _handlersListField.GetValue(__instance) as IList;
                }

                if (handlersList != null && _handlerConstructor != null)
                {
                    for (int i = 0; i < handlersList.Count; i++)
                    {
                        if (handlersList[i] is IDisposable disposable)
                        {
                            disposable.Dispose();
                        }
                    }
                    handlersList.Clear();

                    foreach (var equipmentSlot in PlayerBody.SlotNames)
                    {
                        var slot = __instance.Inventory.Equipment.GetSlot(equipmentSlot);
                        if (slot != null)
                        {
                            var handler = _handlerConstructor.Invoke(new object[] { slot, __instance, equipmentSlot });
                            if (handler != null)
                            {
                                handlersList.Add(handler);
                            }
                        }
                    }

                    if (__instance.PlayerBody.HaveHolster && __instance.PlayerBody.SlotViews.ContainsKey(EquipmentSlot.Holster))
                    {
                        var holsterSlot = __instance.Inventory.Equipment.GetSlot(EquipmentSlot.Holster);
                        if (holsterSlot != null)
                        {
                            var handler = _handlerConstructor.Invoke(new object[] { holsterSlot, __instance, EquipmentSlot.Holster });
                            if (handler != null)
                            {
                                handlersList.Add(handler);
                            }
                        }
                    }
                }

                // 2. Atualiza ContainerBones da arma nas mãos sem colisão de dicionário
                if (__instance.HandsController is Player.FirearmController controller && controller.CCV != null && controller.Item != null)
                {
                    if (__instance.Inventory.Equipment.TryFindItem(controller.Item.Id, out var item) && item is Weapon newWeapon)
                    {
                        var newSlots = newWeapon.AllSlots;
                        if (newSlots != null && controller.CCV.ContainerBones != null)
                        {
                            // Usa lista de KeyValuePair para suportar múltiplos slots táticos de mesmo FullId sem lançar exceções ou erros de colisão
                            var currentViews = new List<KeyValuePair<string, GClass768.GClass769>>();

                            foreach (var kvp in controller.CCV.ContainerBones)
                            {
                                if (kvp.Key is Slot slot && slot.ContainedItem != null && kvp.Value != null)
                                {
                                    currentViews.Add(new KeyValuePair<string, GClass768.GClass769>(slot.FullId, kvp.Value));
                                }
                            }

                            if (controller.Weapon != null && controller.Weapon.AllSlots != null)
                            {
                                controller.CCV.RemoveBones(controller.Weapon.AllSlots);
                            }

                            foreach (IContainer container in newSlots)
                            {
                                if (container is Slot slot)
                                {
                                    if (slot.ContainedItem == null)
                                    {
                                        if (controller.CCV.GameObject != null)
                                        {
                                            var transform = TransformHelperClass.FindTransformRecursive(controller.CCV.GameObject.transform, slot.ID, true);
                                            if (transform != null)
                                            {
                                                controller.CCV.AddBone(slot, transform);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < currentViews.Count; i++)
                                        {
                                            if (currentViews[i].Key == slot.FullId)
                                            {
                                                controller.CCV.ContainerBones[slot] = currentViews[i].Value;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return false; // Execução segura concluída; pula o método nativo com bug de dicionário do Fika
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogWarning($"TRL-Fixes: Exceção no Prefix de FikaRefreshSlotViewsSafetyPatch: {ex.Message}");
                return true; // Fallback caso ocorra alguma inconsistência de estado
            }
        }
    }
}
