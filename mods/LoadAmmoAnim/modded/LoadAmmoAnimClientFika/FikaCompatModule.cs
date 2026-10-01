using BepInEx.Logging;
using Comfort.Common;
using EFT;
using Fika.Core.Main.Players;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using Fika.Core.Networking.LiteNetLib;
using Manimal.LoadAmmoAnim;
using Manimal.LoadAmmoAnim.Fika.Packets;
using Manimal.LoadAmmoAnim.Patches;
using System;

namespace Manimal.LoadAmmoAnim.Fika
{
    // sideloaded entry point. main plugin's Awake calls Enable() via reflection
    // when Fika is detected. registers packet handlers, subscribes to driver
    // events, and broadcasts/receives anim sessions across the Fika network.
    public static class FikaCompatModule
    {
        // separate logger so Fika-related lines are easy to grep in BepInEx.log.
        private static ManualLogSource _log;

        // captured at Enable / OnNetworkManagerCreated. used both to send and to register receivers.
        private static IFikaNetworkManager _netManager;

        public static void Enable()
        {
            _log = BepInEx.Logging.Logger.CreateLogSource("LoadAmmoAnim.Fika");

            // Registro canônico no frame zero: assina a criação de qualquer instância do IFikaNetworkManager
            FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerCreatedEvent>(OnNetworkManagerCreated);

            if (Singleton<IFikaNetworkManager>.Instantiated && Singleton<IFikaNetworkManager>.Instance != null)
            {
                _netManager = Singleton<IFikaNetworkManager>.Instance;
                RegisterReceivers(_netManager);
                _log.LogInfo("[LoadAmmoAnim.Fika] IFikaNetworkManager pre-existente registrado.");
            }
            else
            {
                _log.LogInfo("[LoadAmmoAnim.Fika] Aguardando FikaNetworkManagerCreatedEvent para registro imediato no frame zero.");
            }

            // subscribe to driver events for the local-player session lifecycle.
            // every event filters on IsYourPlayer so we only ever broadcast for the
            // local player. observed-player session changes are driven by inbound
            // packets, never re-broadcast.
            LoadAmmoAnimEvents.AnimStarted += OnAnimStarted;
            LoadAmmoAnimEvents.AnimStopped += OnAnimStopped;
            LoadAmmoAnimEvents.MagSwapped  += OnMagSwapped;

            _log.LogInfo("[LoadAmmoAnim.Fika] enabled.");
        }

        private static void OnNetworkManagerCreated(FikaNetworkManagerCreatedEvent ev)
        {
            _netManager = ev.Manager;
            if (_netManager != null)
            {
                RegisterReceivers(_netManager);
                _log?.LogInfo("[LoadAmmoAnim.Fika] Pacotes registrados no frame zero do IFikaNetworkManager criado.");
            }
        }

        private static void RegisterReceivers(IFikaNetworkManager net)
        {
            net.RegisterPacket<LoadAmmoBundleStartPacket>(OnStartPacket);
            net.RegisterPacket<LoadAmmoBundleStopPacket>(OnStopPacket);
            net.RegisterPacket<LoadAmmoBundleSwapMeshPacket>(OnSwapMeshPacket);
        }

        // ---- send side ----

        private static IFikaNetworkManager EnsureNetManager()
        {
            if (_netManager != null) return _netManager;
            if (Singleton<IFikaNetworkManager>.Instantiated)
            {
                _netManager = Singleton<IFikaNetworkManager>.Instance;
                if (_netManager != null) RegisterReceivers(_netManager);
            }
            return _netManager;
        }

        private static FikaPlayer AsLocalFikaPlayer(Player p)
        {
            // only the local player should ever broadcast. observed players have
            // IsYourPlayer=false and exist on every machine, so re-broadcasting
            // their state would feedback-loop.
            if (p is FikaPlayer fp && fp.IsYourPlayer) return fp;
            return null;
        }

        private static void OnAnimStarted(Player player, string magTpl, string ammoTpl, float speed)
        {
            var fp = AsLocalFikaPlayer(player);
            if (fp == null) return;
            var net = EnsureNetManager();
            if (net == null) { _log?.LogWarning("[LoadAmmoAnim.Fika] cant send Start: no network manager."); return; }

            var pkt = new LoadAmmoBundleStartPacket
            {
                NetId = fp.NetId,
                MagTemplateId = magTpl ?? string.Empty,
                AmmoTemplateId = ammoTpl ?? string.Empty,
                LoadOneAmmoSpeed = speed
            };
            net.SendData(ref pkt, DeliveryMethod.ReliableOrdered, broadcast: true);
        }

        private static void OnAnimStopped(Player player, bool playPutAway)
        {
            var fp = AsLocalFikaPlayer(player);
            if (fp == null) return;
            var net = EnsureNetManager();
            if (net == null) { _log?.LogWarning("[LoadAmmoAnim.Fika] cant send Stop: no network manager."); return; }

            var pkt = new LoadAmmoBundleStopPacket
            {
                NetId = fp.NetId,
                PlayPutAway = playPutAway
            };
            net.SendData(ref pkt, DeliveryMethod.ReliableOrdered, broadcast: true);
        }

        private static void OnMagSwapped(Player player, string magTpl)
        {
            var fp = AsLocalFikaPlayer(player);
            if (fp == null) return;
            var net = EnsureNetManager();
            if (net == null) { _log?.LogWarning("[LoadAmmoAnim.Fika] cant send SwapMesh: no network manager."); return; }

            var pkt = new LoadAmmoBundleSwapMeshPacket
            {
                NetId = fp.NetId,
                NewMagTemplateId = magTpl ?? string.Empty
            };
            net.SendData(ref pkt, DeliveryMethod.ReliableOrdered, broadcast: true);
        }

        // ---- receive side ----

        // resolves the Player whose FikaPlayer.NetId matches. ignores the local
        // player so an echoed broadcast doesnt double-trigger our own anim.
        private static Player ResolvePlayerByNetId(int netId)
        {
            var gw = Singleton<GameWorld>.Instance;
            if (gw == null) return null;

            // GameWorld.AllAlivePlayersList is a List<Player> with the live combat
            // roster — includes both local and observed.
            var list = gw.AllAlivePlayersList;
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var p = list[i];
                    if (p is FikaPlayer fp && fp.NetId == netId && !fp.IsYourPlayer)
                        return p;
                }
            }

            // fallback: AllPlayersEverExisted is IEnumerable<Player> covering
            // everyone whos joined this raid, alive or not.
            var all = gw.AllPlayersEverExisted;
            if (all != null)
            {
                foreach (var p in all)
                {
                    if (p is FikaPlayer fp && fp.NetId == netId && !fp.IsYourPlayer)
                        return p;
                }
            }

            return null;
        }

        private static void OnStartPacket(LoadAmmoBundleStartPacket pkt)
        {
            try
            {
                var player = ResolvePlayerByNetId(pkt.NetId);
                if (player == null)
                {
                    _log?.LogDebug($"[LoadAmmoAnim.Fika] Start: no player for NetId {pkt.NetId} (likely not joined yet).");
                    return;
                }
                LoadAmmoAnimDriver.StartBundleAnim(player, pkt.MagTemplateId, pkt.AmmoTemplateId, pkt.LoadOneAmmoSpeed);
            }
            catch (Exception ex)
            {
                _log?.LogError($"[LoadAmmoAnim.Fika] OnStartPacket threw: {ex}");
            }
        }

        private static void OnStopPacket(LoadAmmoBundleStopPacket pkt)
        {
            try
            {
                var player = ResolvePlayerByNetId(pkt.NetId);
                if (player == null)
                {
                    _log?.LogDebug($"[LoadAmmoAnim.Fika] Stop: no player for NetId {pkt.NetId}.");
                    return;
                }
                LoadAmmoAnimDriver.StopBundleAnim(player, pkt.PlayPutAway);
            }
            catch (Exception ex)
            {
                _log?.LogError($"[LoadAmmoAnim.Fika] OnStopPacket threw: {ex}");
            }
        }

        private static void OnSwapMeshPacket(LoadAmmoBundleSwapMeshPacket pkt)
        {
            try
            {
                var player = ResolvePlayerByNetId(pkt.NetId);
                if (player == null)
                {
                    _log?.LogDebug($"[LoadAmmoAnim.Fika] SwapMesh: no player for NetId {pkt.NetId}.");
                    return;
                }
                LoadAmmoAnimDriver.SwapMagMesh(player, pkt.NewMagTemplateId);
            }
            catch (Exception ex)
            {
                _log?.LogError($"[LoadAmmoAnim.Fika] OnSwapMeshPacket threw: {ex}");
            }
        }
    }
}
