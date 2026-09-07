using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;

namespace TRLFixes.Patches
{
    /// <summary>
    /// Corrige a falha de validação no FikaServer ao receber um ProceedRequestPacket com ProceedType == EmptyHands.
    /// No Fika nativo, o servidor tenta obrigatoriamente buscar o item pelo packet.ItemId via TryFindItemForProceedPacket,
    /// mas para mãos vazias (EmptyHands) o ItemId é vazio/default (000000000000000000000000).
    /// Isso fazia o servidor rejeitar a requisição com "Could not find item with id", gerando o erro no cliente:
    /// "[Error : Fika.Core] [HandleCallbackResponse]: Could not execute callback with id XX on the server".
    /// 
    /// Este patch intercepta OnProceedRequestPacketReceived no Prefix: se o tipo for EmptyHands,
    /// valida apenas a existência do jogador e responde com sucesso imediatamente, evitando o erro e a dessincronização.
    /// </summary>
    public class FikaProceedEmptyHandsSafetyPatch
    {
        private const byte EmptyHandsProceedType = 0;
        private const int ReliableOrderedDeliveryMethod = 2;

        private static Type _fikaServerType;
        private static Type _proceedRequestPacketType;
        private static Type _proceedResponsePacketType;
        private static Type _deliveryMethodEnum;
        private static object _cachedDeliveryMethodVal;
        private static MethodInfo _sendDataToPeerMethod;
        private static PropertyInfo _coopHandlerProperty;
        private static PropertyInfo _playersProperty;
        private static FieldInfo _netIdField;
        private static FieldInfo _callbackIdField;
        private static FieldInfo _proceedTypeField;
        private static FieldInfo _responseCallbackIdField;
        private static FieldInfo _responseErrorField;

        public void Enable()
        {
            try
            {
                var harmony = new Harmony("com.trl.fixes.fikaproceedemptyhands");
                _fikaServerType = AccessTools.TypeByName("Fika.Core.Networking.FikaServer");
                _proceedRequestPacketType = AccessTools.TypeByName("Fika.Core.Networking.Packets.FirearmController.ProceedRequestPacket");
                _proceedResponsePacketType = AccessTools.TypeByName("Fika.Core.Networking.Packets.FirearmController.ProceedResponsePacket");
                _deliveryMethodEnum = AccessTools.TypeByName("LiteNetLib.DeliveryMethod");
                var netPeerType = AccessTools.TypeByName("LiteNetLib.NetPeer");

                if (_fikaServerType == null || _proceedRequestPacketType == null || _proceedResponsePacketType == null || netPeerType == null || _deliveryMethodEnum == null)
                {
                    Plugin.Log?.LogInfo("TRL-Fixes: FIKA Networking não detectado. FikaProceedEmptyHandsSafetyPatch não será ativado.");
                    return;
                }

                // Cache do valor enum ReliableOrdered para zero-alloc no runtime (AUD-01-06)
                _cachedDeliveryMethodVal = Enum.ToObject(_deliveryMethodEnum, ReliableOrderedDeliveryMethod);

                _netIdField = AccessTools.Field(_proceedRequestPacketType, "NetId");
                _callbackIdField = AccessTools.Field(_proceedRequestPacketType, "CallbackId");
                _proceedTypeField = AccessTools.Field(_proceedRequestPacketType, "ProceedType");

                _responseCallbackIdField = AccessTools.Field(_proceedResponsePacketType, "CallbackId");
                _responseErrorField = AccessTools.Field(_proceedResponsePacketType, "Error");

                _coopHandlerProperty = AccessTools.Property(_fikaServerType, "CoopHandler");
                var coopHandlerType = AccessTools.TypeByName("Fika.Core.Coop.CoopHandler");
                if (coopHandlerType != null)
                {
                    _playersProperty = AccessTools.Property(coopHandlerType, "Players");
                }

                // Localiza SendDataToPeer<T>(ref T packet, DeliveryMethod deliveryMethod, NetPeer peer)
                var sendMethods = _fikaServerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.Name == "SendDataToPeer" && m.IsGenericMethodDefinition);

                foreach (var m in sendMethods)
                {
                    var pars = m.GetParameters();
                    if (pars.Length == 3 && pars[0].ParameterType.IsByRef && pars[2].ParameterType == netPeerType)
                    {
                        _sendDataToPeerMethod = m.MakeGenericMethod(_proceedResponsePacketType);
                        break;
                    }
                }

                var targetMethod = AccessTools.Method(_fikaServerType, "OnProceedRequestPacketReceived", new[] { _proceedRequestPacketType, netPeerType });
                if (targetMethod != null)
                {
                    var prefixMethod = AccessTools.Method(typeof(FikaProceedEmptyHandsSafetyPatch), nameof(Prefix));
                    harmony.Patch(targetMethod, prefix: new HarmonyMethod(prefixMethod));
                    Plugin.Log?.LogInfo("TRL-Fixes: FikaProceedEmptyHandsSafetyPatch aplicado com sucesso em FikaServer.OnProceedRequestPacketReceived!");
                }
                else
                {
                    Plugin.Log?.LogWarning("TRL-Fixes: FikaServer.OnProceedRequestPacketReceived não encontrado!");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"TRL-Fixes: Erro ao aplicar FikaProceedEmptyHandsSafetyPatch: {ex}");
            }
        }

        public static bool Prefix(object __instance, object packet, object peer)
        {
            try
            {
                if (packet == null || _proceedTypeField == null) return true;

                // EProceedType.EmptyHands tem valor numérico 0
                var proceedTypeVal = (byte)Convert.ChangeType(_proceedTypeField.GetValue(packet), typeof(byte));
                if (proceedTypeVal != EmptyHandsProceedType) // Se NÃO for EmptyHands, segue o fluxo normal do Fika
                {
                    return true;
                }

                int netId = (int)_netIdField.GetValue(packet);
                uint callbackId = (uint)_callbackIdField.GetValue(packet);

                // Cria instância de ProceedResponsePacket
                object response = Activator.CreateInstance(_proceedResponsePacketType);
                _responseCallbackIdField.SetValue(response, callbackId);

                // Verifica se o player existe em CoopHandler.Players
                bool playerFound = false;
                if (_coopHandlerProperty != null && _playersProperty != null)
                {
                    var coopHandler = _coopHandlerProperty.GetValue(__instance);
                    if (coopHandler != null)
                    {
                        var playersDict = _playersProperty.GetValue(coopHandler) as System.Collections.IDictionary;
                        if (playersDict != null && playersDict.Contains(netId))
                        {
                            playerFound = true;
                        }
                    }
                }
                else
                {
                    playerFound = true; // Fallback se não conseguir inspecionar CoopHandler
                }

                if (!playerFound)
                {
                    _responseErrorField.SetValue(response, $"Could not find player with id {netId}");
                }
                else
                {
                    _responseErrorField.SetValue(response, null); // Sucesso! Sem erro
                }

                // Envia ProceedResponsePacket via SendDataToPeer(ref response, DeliveryMethod.ReliableOrdered, peer) usando enum em cache
                if (_sendDataToPeerMethod != null && _cachedDeliveryMethodVal != null)
                {
                    object[] invokeArgs = new object[] { response, _cachedDeliveryMethodVal, peer };
                    _sendDataToPeerMethod.Invoke(__instance, invokeArgs);
                }

                return false; // Interceptado com sucesso, pula o método original do FikaServer
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogWarning($"TRL-Fixes: Exceção no Prefix de FikaProceedEmptyHandsSafetyPatch: {ex.Message}");
                return true; // Em caso de falha, deixa o original rodar
            }
        }
    }
}
