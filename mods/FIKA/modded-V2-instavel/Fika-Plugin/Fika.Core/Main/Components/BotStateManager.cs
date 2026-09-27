using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using EFT;
using Fika.Core.Main.GameMode;
using Fika.Core.Main.Players;
using Fika.Core.Main.Utils;
using Fika.Core.Networking;
using Fika.Core.Networking.LiteNetLib;
using Fika.Core.Networking.Packets.Player;
using Fika.Core.Networking.Snapshotting;
using LiteNetLib;
using UnityEngine;
using static Fika.Core.Networking.NetworkUtils;

namespace Fika.Core.Main.Components;

public sealed class BotStateManager : MonoBehaviour
{
    public struct BotSnapshotItem
    {
        public PlayerStateData State;
        public bool InCombat;
    }

    public struct PeerSnapshotTarget
    {
        public NetPeer Peer;
        public Vector3 Position;
        public bool HasPosition;
    }

    public sealed class StateSnapshotBuffer
    {
        public double RemoteTime;
        public int BotCount;
        public readonly BotSnapshotItem[] Bots = new BotSnapshotItem[128];
        public int PeerCount;
        public readonly PeerSnapshotTarget[] Peers = new PeerSnapshotTarget[32];
    }

    private List<FikaBot> _bots;
    private HostGameController _controller;
    private BotsController _botsController;
    private FikaServer _server;

    private float _updateCount;
    private float _updatesPerTick;
    private readonly byte _stateSize = PlayerStateData.PacketSize;

    // Double Buffering & Threading
    private StateSnapshotBuffer _frontBuffer = new();
    private StateSnapshotBuffer _backBuffer = new();
    private readonly object _swapLock = new();

    private Thread _workerThread;
    private readonly AutoResetEvent _workSignal = new(false);
    private volatile bool _workerRunning;
    private uint _tickCounter;

    private readonly List<NetPeer> _connectedPeersCache = new(16);
    private NetDataWriter _workerWriter;
    private NetDataWriter _mainWriter;
    private readonly System.Collections.Concurrent.ConcurrentDictionary<Type, long> _lastErrorLogTicks = new();
    private const long ERROR_LOG_INTERVAL_MS = 1000;

    private void Start()
    {
        FikaBot.OnPlayerDeath += OnPlayerDeath;
        FikaBot.OnPlayerDestroyed += OnPlayerDeath;
    }

    private void OnPlayerDeath(FikaPlayer player)
    {
        if (player is FikaBot bot)
        {
            RemoveBot(bot);
        }
    }

    public void AddBot(FikaBot bot)
    {
        if (_bots.Contains(bot))
        {
            return;
        }

        _bots.Add(bot);
    }

    public bool RemoveBot(FikaBot bot)
    {
        return _bots.Remove(bot);
    }

    public static BotStateManager Create(AbstractGame game, FikaServer server, HostGameController hostGameController)
    {
        var component = game.gameObject.AddComponent<BotStateManager>();
        component._controller = hostGameController;
        component._updateCount = 0f;
        component._updatesPerTick = 1f / server.SendRate;
        component._bots = [];
        component._server = server;
        component._workerWriter = new NetDataWriter(true, 1024);
        component._mainWriter = new NetDataWriter(true, 1024);

        // Inicialização do Background Worker para descarregar o I/O de rede da Main Thread
        component._workerRunning = true;
        component._workerThread = new Thread(component.WorkerLoop)
        {
            IsBackground = true,
            Name = "Fika-Network-State-Worker",
            Priority = System.Threading.ThreadPriority.AboveNormal
        };
        component._workerThread.Start();

        return component;
    }

    private void Update()
    {
        _controller.Update?.Invoke();
        _botsController?.method_0();

        _updateCount += Time.unscaledDeltaTime;
        if (_updateCount >= _updatesPerTick)
        {
            SendBatchStates();
            _updateCount -= _updatesPerTick;
        }
    }

    private void SendBatchStates()
    {
        if (_server == null)
        {
            return;
        }

        // Se não houver clientes remotos conectados, não há necessidade de serializar estados de rede
        _connectedPeersCache.Clear();
        _server.GetConnectedPeers(_connectedPeersCache);
        if (_connectedPeersCache.Count == 0)
        {
            return;
        }

        var buffer = _frontBuffer;
        buffer.RemoteTime = NetworkTimeSync.NetworkTime;

        // 1. Extração ultra-rápida de structs blittable dos bots vivos na Main Thread (< 0.15ms)
        var botCount = 0;
        var totalBots = _bots.Count;
        for (var i = totalBots - 1; i >= 0 && botCount < buffer.Bots.Length; i--)
        {
            var bot = _bots[i];
            if (bot != null && bot.HealthController != null && bot.HealthController.IsAlive && bot.BotPacketSender != null)
            {
                if (bot.BotPacketSender.TryGetState(out var state))
                {
                    ref var item = ref buffer.Bots[botCount++];
                    item.State = state;
                    // Detecta se o bot está ativamente em combate (promoção prioritária de AoI)
                    item.InCombat = bot.AIData?.BotOwner?.Memory?.GoalEnemy != null;
                }
            }
        }
        buffer.BotCount = botCount;

        // 2. Extração de alvos de peers e suas posições no mapa
        var peerCount = 0;
        for (var i = 0; i < _connectedPeersCache.Count && peerCount < buffer.Peers.Length; i++)
        {
            var peer = _connectedPeersCache[i];
            ref var target = ref buffer.Peers[peerCount++];
            target.Peer = peer;
            if (_server.TryGetPlayerByPeer(peer, out var player) && player != null && player.HealthController.IsAlive)
            {
                target.Position = player.Position;
                target.HasPosition = true;
            }
            else
            {
                target.Position = Vector3.zero;
                target.HasPosition = false;
            }
        }
        buffer.PeerCount = peerCount;

        // 3. Despacho: Multithread (Worker) ou Síncrono (Fallback)
        var settings = FikaPlugin.Instance?.Settings;
        var useThreading = settings == null || settings.EnableNetworkThreading.Value;

        if (useThreading && _workerRunning)
        {
            lock (_swapLock)
            {
                var temp = _frontBuffer;
                _frontBuffer = _backBuffer;
                _backBuffer = temp;
            }
            _workSignal.Set();
        }
        else
        {
            ProcessAndSendSnapshot(buffer, _mainWriter);
        }
    }

    private void WorkerLoop()
    {
        while (_workerRunning)
        {
            _workSignal.WaitOne();
            if (!_workerRunning)
            {
                break;
            }

            StateSnapshotBuffer toProcess;
            lock (_swapLock)
            {
                toProcess = _backBuffer;
            }

            try
            {
                ProcessAndSendSnapshot(toProcess, _workerWriter);
            }
            catch (Exception ex)
            {
                if (!_workerRunning)
                {
                    break;
                }

                var exType = ex.GetType();
                var now = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
                if (!_lastErrorLogTicks.TryGetValue(exType, out var lastTick) || (now - lastTick) >= ERROR_LOG_INTERVAL_MS)
                {
                    _lastErrorLogTicks[exType] = now;
                    FikaGlobals.LogError($"[BotStateManager] Erro ao processar pacotes no worker de rede ({exType.Name}): {ex.Message}");
                }
            }
        }
    }

    private void ProcessAndSendSnapshot(StateSnapshotBuffer snapshot, NetDataWriter writer)
    {
        if (snapshot == null || snapshot.PeerCount == 0 || snapshot.BotCount == 0 || _server == null || !_workerRunning)
        {
            return;
        }

        var settings = FikaPlugin.Instance?.Settings;
        var enableAoI = settings == null || settings.EnableAoICulling.Value;
        var nearDist = settings?.AoINearDistance?.Value ?? 250.0f;
        var midDist = settings?.AoIMidDistance?.Value ?? 500.0f;
        var nearSqr = nearDist * nearDist;
        var midSqr = midDist * midDist;

        var currentTick = unchecked(_tickCounter++);
        var remoteTime = snapshot.RemoteTime;
        var maxMtu = _server.MaxMTU;

        for (var p = 0; p < snapshot.PeerCount; p++)
        {
            ref readonly var target = ref snapshot.Peers[p];
            var peer = target.Peer;
            if (peer == null || peer.ConnectionState != ConnectionState.Connected)
            {
                continue;
            }

            writer.Reset();
            var headerWritten = false;
            var writtenCount = 0;

            for (var b = 0; b < snapshot.BotCount; b++)
            {
                ref readonly var bot = ref snapshot.Bots[b];

                // Filtro de Área de Interesse (AoI)
                if (enableAoI && target.HasPosition)
                {
                    var sqrDist = (bot.State.Position - target.Position).sqrMagnitude;

                    if (sqrDist <= nearSqr || bot.InCombat)
                    {
                        // Zona Tática (< 250m) ou Bot em Combate: envio em taxa cheia (20/30 Hz)
                    }
                    else if (sqrDist <= midSqr)
                    {
                        // Zona Periférica (250m - 500m): intercalado a cada 4 ticks (~5 Hz) com balanceamento
                        if ((currentTick + (uint)b) % 4 != 0)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        // Zona Morta (> 500m): heartbeat lento a cada 20 ticks (~1 Hz) com balanceamento
                        if ((currentTick + (uint)b) % 20 != 0)
                        {
                            continue;
                        }
                    }
                }

                if (!headerWritten)
                {
                    writer.PutEnum(EPacketType.PlayerState);
                    writer.Put(remoteTime);
                    headerWritten = true;
                }

                // Verifica MTU para fatiar pacotes se necessário
                if ((writer.Length + _stateSize) > maxMtu)
                {
                    try
                    {
                        if (writtenCount > 0 && _server != null && peer.ConnectionState == ConnectionState.Connected)
                        {
                            _server.SendStatesToPeer(peer, writer);
                        }
                    }
                    catch (Exception ex)
                    {
                        FikaGlobals.LogWarning($"[BotStateManager] Falha ao enviar fatia de estados para peer: {ex.Message}");
                        break;
                    }

                    writer.Reset();
                    writer.PutEnum(EPacketType.PlayerState);
                    writer.Put(remoteTime);
                    writtenCount = 0;
                }

                writer.PutUnmanaged(bot.State);
                writtenCount++;
            }

            try
            {
                if (writtenCount > 0 && _server != null && peer.ConnectionState == ConnectionState.Connected)
                {
                    _server.SendStatesToPeer(peer, writer);
                }
            }
            catch (Exception ex)
            {
                FikaGlobals.LogWarning($"[BotStateManager] Falha ao enviar estados para peer: {ex.Message}");
            }
        }
    }

    private void OnDestroy()
    {
        FikaBot.OnPlayerDeath -= OnPlayerDeath;
        FikaBot.OnPlayerDestroyed -= OnPlayerDeath;

        // Encerramento limpo da background thread com timeout seguro de 1000ms (RT-01)
        _workerRunning = false;
        try
        {
            _workSignal?.Set();
            if (_workerThread != null && _workerThread.IsAlive)
            {
                _workerThread.Join(1000);
            }
            _workSignal?.Dispose();
        }
        catch (Exception ex)
        {
            FikaGlobals.LogWarning($"[BotStateManager] Aviso ao encerrar worker thread: {ex.Message}");
        }

        _bots?.Clear();
        _botsController = null;
        _controller = null;
        _server = null;
        _workerWriter = null;
        _mainWriter = null;
    }

    public void AssignBotsController(BotsController botsController)
    {
        _botsController = botsController;
    }

    public void UnassignBotsController()
    {
        _botsController = null;
    }
}
