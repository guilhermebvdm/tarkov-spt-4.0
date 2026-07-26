using System;
using System.Linq;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using UnityEngine;

namespace CameraRotationMod.Networking
{
    public static class FikaSyncManager
    {
        private static ManualLogSource _logger;
        private static bool _initialized = false;

        /// <summary>
        /// Rastreamento por REFERÊNCIA de instância (não por flag bool): o FIKA destrói e recria o
        /// IFikaNetworkManager em cada transição menu → lobby → raid, e a nova instância tem o
        /// NetPacketProcessor vazio. Comparar a referência é o que detecta essa troca.
        /// </summary>
        private static IFikaNetworkManager _lastRegisteredNetworkManager;

        // Throttle de log: com peer de versão divergente a falha vira por-pacote. Sem limite, o
        // console do BepInEx receberia um stack trace por pacote, causando hitching e escondendo
        // o erro real.
        private const int ErrorLogIntervalMs = 5000;
        private static int _lastErrorLogTick;
        private static int _suppressedErrors;

        /// <summary>
        /// Tipos de exceção que já renderam stack trace completo. A chave é o TIPO (e não um
        /// booleano global): uma falha diferente que apareça raids depois ainda precisa do trace,
        /// caso contrário o throttle esconderia justamente o problema novo.
        /// </summary>
        private static readonly System.Collections.Generic.HashSet<Type> _tracedExceptionTypes
            = new System.Collections.Generic.HashSet<Type>();

        /// <summary>
        /// Loga a exceção completa na primeira ocorrência de cada TIPO e, depois, no máximo uma vez
        /// a cada 5 s com a contagem do que foi suprimido.
        /// </summary>
        internal static void LogErrorThrottled(string context, Exception ex)
        {
            var now = Environment.TickCount;

            if (ex != null && _tracedExceptionTypes.Add(ex.GetType()))
            {
                _lastErrorLogTick = now;
                _logger?.LogError($"[CameraRotationMod] {context}: {ex}");
                return;
            }

            // Subtração de ints trata o wrap-around de TickCount corretamente.
            if (now - _lastErrorLogTick < ErrorLogIntervalMs)
            {
                _suppressedErrors++;
                return;
            }

            _lastErrorLogTick = now;
            var suppressed = _suppressedErrors;
            _suppressedErrors = 0;
            _logger?.LogError($"[CameraRotationMod] {context}: {ex?.Message}"
                + (suppressed > 0 ? $" (+{suppressed} falhas suprimidas nos últimos {ErrorLogIntervalMs / 1000}s)" : string.Empty));
        }

        public static void Initialize(ManualLogSource logger)
        {
            if (_initialized) return;
            _logger = logger;

            try
            {
                // Gatilho secundário: o registro real é dirigido por EnsurePacketsRegistered.
                FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerCreatedEvent>(OnNetworkManagerCreated);
                _logger.LogInfo("[CameraRotationMod] Fika integration initialized.");
                _initialized = true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[CameraRotationMod] Failed to initialize Fika integration: {ex}");
            }
        }

        private static void OnNetworkManagerCreated(FikaNetworkManagerCreatedEvent ev)
        {
            try
            {
                EnsurePacketsRegistered();
            }
            catch (Exception ex)
            {
                LogErrorThrottled("OnNetworkManagerCreated", ex);
            }
        }

        /// <summary>
        /// Garante que os pacotes estejam registrados na instância ATIVA do IFikaNetworkManager.
        /// Chamado no Update do plugin e antes de qualquer envio.
        /// </summary>
        public static void EnsurePacketsRegistered()
        {
            if (!Singleton<IFikaNetworkManager>.Instantiated)
            {
                _lastRegisteredNetworkManager = null;
                return;
            }

            var currentManager = Singleton<IFikaNetworkManager>.Instance;
            if (currentManager == null) return;
            if (_lastRegisteredNetworkManager == currentManager) return;

            try
            {
                currentManager.RegisterPacket<StanceSyncPacketV2>(OnStanceSyncPacketReceived);
                // Formato legado (≤2.10.0): registrado só para RECEPÇÃO, para continuar entendendo
                // peers ainda não atualizados. Nunca é enviado.
                currentManager.RegisterPacket<StanceSyncPacket>(OnStanceSyncPacketReceivedLegacy);

                _lastRegisteredNetworkManager = currentManager;
                _logger?.LogInfo($"[CameraRotationMod] Registered StanceSyncPacketV2 (+legacy) on new IFikaNetworkManager instance ({currentManager.GetType().Name}).");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[CameraRotationMod] Error registering stance packets: {ex}");
            }
        }

        public static void SendStance(int stance, bool isAiming)
        {
            EnsurePacketsRegistered();
            if (_lastRegisteredNetworkManager == null) return;

            Player player = Singleton<EFT.GameWorld>.Instantiated && Singleton<EFT.GameWorld>.Instance.MainPlayer != null
                ? Singleton<EFT.GameWorld>.Instance.MainPlayer
                : null;

            if (player == null) return;

            var packet = new StanceSyncPacketV2
            {
                ProfileId = player.ProfileId,
                Stance = stance,
                IsAiming = isAiming
            };

            try
            {
                _lastRegisteredNetworkManager.SendData(ref packet, Fika.Core.Networking.LiteNetLib.DeliveryMethod.ReliableOrdered, true);
            }
            catch (Exception ex)
            {
                LogErrorThrottled("Error sending StanceSyncPacketV2", ex);
            }
        }

        private static void OnStanceSyncPacketReceived(StanceSyncPacketV2 packet)
        {
            ApplyStance(packet.ProfileId, packet.Stance, packet.IsAiming, nameof(OnStanceSyncPacketReceived));
        }

        /// <summary>Recepção do formato legado (peer ≤ 2.10.0). Mesmo processamento do V2.</summary>
        private static void OnStanceSyncPacketReceivedLegacy(StanceSyncPacket packet)
        {
            ApplyStance(packet.ProfileId, packet.Stance, packet.IsAiming, nameof(OnStanceSyncPacketReceivedLegacy));
        }

        /// <summary>
        /// Airbag de recepção: o corpo inteiro é protegido. Uma exceção que escape daqui sobe pelo
        /// ReadAllPackets do LiteNetLib e descarta o restante do lote de pacotes daquele frame —
        /// inclusive os dos outros mods e os de movimento do FIKA.
        /// </summary>
        private static void ApplyStance(string profileId, int stance, bool isAiming, string context)
        {
            try
            {
                // Fora de raid não há ObservedPlayer para animar, e AddComponent num GameObject de
                // menu vazaria. O gate é feito aqui, e nunca com UnregisterPacket (desregistrar
                // corromperia a tabela de handlers compartilhada do FIKA).
                if (!Singleton<EFT.GameWorld>.Instantiated) return;
                if (_lastRegisteredNetworkManager == null) return;
                if (string.IsNullOrEmpty(profileId)) return;

                var observedPlayer = _lastRegisteredNetworkManager.ObservedPlayers?
                    .FirstOrDefault(p => p != null && p.ProfileId == profileId);
                if (observedPlayer == null) return;

                var animator = observedPlayer.gameObject.GetComponent<ObservedStanceAnimator>();
                if (animator == null)
                {
                    animator = observedPlayer.gameObject.AddComponent<ObservedStanceAnimator>();
                    animator.Init(observedPlayer);
                }

                animator.SetStance(stance, isAiming);
            }
            catch (Exception ex)
            {
                LogErrorThrottled($"Error processing stance packet ({context})", ex);
            }
        }
    }
}
