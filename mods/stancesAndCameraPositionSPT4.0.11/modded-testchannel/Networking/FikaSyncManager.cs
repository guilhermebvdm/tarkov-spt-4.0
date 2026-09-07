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

        /// <summary>
        /// Item 020 (F4): o mecanismo foi extraído para `ThrottledLog` (agora também serve o laço
        /// principal). Assinatura mantida pelos call-sites de rede; comportamento idêntico.
        /// </summary>
        internal static void LogErrorThrottled(string context, Exception ex) => ThrottledLog.Error(context, ex);

        public static void Initialize(ManualLogSource logger)
        {
            if (_initialized) return;
            _logger = logger;

            try
            {
                // Gatilho secundário: o registro real é dirigido por EnsurePacketsRegistered.
                FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerCreatedEvent>(OnNetworkManagerCreated);
                _logger.LogInfo("[TRL-StancesAndMobility] Fika integration initialized.");
                _initialized = true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[TRL-StancesAndMobility] Failed to initialize Fika integration: {ex}");
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
                // Sync de estado de câmara: enviado pelo convidado, aplicado pelo host.
                currentManager.RegisterPacket<ChamberStateSyncPacket>(OnChamberStateSyncPacketReceived);

                _lastRegisteredNetworkManager = currentManager;
                _logger?.LogInfo($"[TRL-StancesAndMobility] Registered StanceSyncPacketV2 (+legacy+chamber) on new IFikaNetworkManager instance ({currentManager.GetType().Name}).");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[TRL-StancesAndMobility] Error registering stance packets: {ex}");
            }
        }

        public const uint TRLS_MAGIC_HEADER = 0x534C5254; // "TRLS" (0x54 0x52 0x4C 0x53) - Assinatura do Canal 3 TRL Stances

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
                // ref: Roadmap 1.1 — Canal 3 Isolado de Dados TRL (ReliableUnordered para não travar a fila do Channel 0 de inventário)
                _lastRegisteredNetworkManager.SendData(ref packet, Fika.Core.Networking.LiteNetLib.DeliveryMethod.ReliableUnordered, true);
            }
            catch (Exception ex)
            {
                LogErrorThrottled("Error sending StanceSyncPacketV2", ex);
            }
        }

        /// <summary>
        /// Enviado pelo convidado FIKA quando a câmara de sua arma muda de estado via ação manual
        /// (RechamberRound ou bolt action manual). Só envia se for cliente FIKA — o host é
        /// autoritátivo e não precisa notificar ninguém.
        /// </summary>
        public static void SendChamberState(string profileId, string weaponId, bool chamberFilled)
        {
            EnsurePacketsRegistered();
            if (_lastRegisteredNetworkManager == null) return;

            try
            {
                // Só envia se for cliente (convidado) — host gerencia o próprio inventário
                if (!Fika.Core.Main.Utils.FikaBackendUtils.IsClient) return;
            }
            catch { return; } // FIKA não disponível — sessão solo

            var packet = new ChamberStateSyncPacket
            {
                ProfileId = profileId,
                WeaponId = weaponId,
                ChamberFilled = chamberFilled
            };

            try
            {
                // ReliableOrdered: garantia de entrega e ordem — perda do pacote causaria
                // inventário permanentemente inconsistente no host.
                _lastRegisteredNetworkManager.SendData(ref packet, Fika.Core.Networking.LiteNetLib.DeliveryMethod.ReliableOrdered, true);
                _logger?.LogInfo($"[TRL-StancesAndMobility] ChamberStateSyncPacket enviado: weapon={weaponId}, filled={chamberFilled}");
            }
            catch (Exception ex)
            {
                LogErrorThrottled("SendChamberState", ex);
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

        /// <summary>
        /// Recebido pelo Host: aplica PopTo (mag→chamber) para o jogador remoto identificado por
        /// ProfileId, tornando o estado de inventário autoritátivo. Executado na thread principal
        /// via PollEvents() no Update() do FikaServer.
        /// </summary>
        private static void OnChamberStateSyncPacketReceived(ChamberStateSyncPacket packet)
        {
            try
            {
                if (!Singleton<EFT.GameWorld>.Instantiated) return;
                if (_lastRegisteredNetworkManager == null) return;
                if (string.IsNullOrEmpty(packet.ProfileId) || string.IsNullOrEmpty(packet.WeaponId)) return;

                // Apenas o host processa — convidados ignoram (evita ecos)
                try { if (!Fika.Core.Main.Utils.FikaBackendUtils.IsServer) return; }
                catch { return; }

                // Localizar jogador remoto: ObservedPlayers primeiro, fallback AllAlivePlayersList
                var gameWorld = Singleton<EFT.GameWorld>.Instance;
                Player targetPlayer = _lastRegisteredNetworkManager.ObservedPlayers?
                    .FirstOrDefault(p => p != null && p.ProfileId == packet.ProfileId)
                    ?? gameWorld?.AllAlivePlayersList?
                        .FirstOrDefault(p => p != null && p.ProfileId == packet.ProfileId);

                if (targetPlayer == null)
                {
                    _logger?.LogWarning($"[TRL-StancesAndMobility] ChamberSync: jogador {packet.ProfileId} não encontrado.");
                    return;
                }

                var fc = targetPlayer.HandsController as EFT.Player.FirearmController;
                if (fc == null || fc.Weapon == null) return;

                // Verifica que a arma é a correta (evita aplicar em troca rápida de arma)
                if (fc.Weapon.Id != packet.WeaponId) return;

                if (packet.ChamberFilled && fc.Weapon.ChamberAmmoCount == 0)
                {
                    var mag = fc.Weapon.GetCurrentMagazine();
                    if (mag != null && mag.Count > 0)
                    {
                        var result = mag.Cartridges.PopTo(
                            targetPlayer.InventoryController,
                            fc.Item.Chambers[0].CreateItemAddress());

                        if (result.Value != null)
                            _logger?.LogInfo($"[TRL-StancesAndMobility] Host aplicou câmara para {targetPlayer.Profile?.Nickname ?? packet.ProfileId}.");
                        else
                            _logger?.LogWarning($"[TRL-StancesAndMobility] Host falhou ao aplicar câmara para {targetPlayer.Profile?.Nickname ?? packet.ProfileId}: {result.Error}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogErrorThrottled("OnChamberStateSyncPacketReceived", ex);
            }
        }
    }
}
