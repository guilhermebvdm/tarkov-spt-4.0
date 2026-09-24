using System.Net.WebSockets;
using System.Text;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Ws;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers.Ws;
using SPTarkov.Server.Core.Servers.Ws.Message;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using CompoundingPerf.Telemetry;

namespace CompoundingPerf.Features;

/// <summary>S13 control surface — one toggle for both notifier-path overrides below.</summary>
public static class CalmNotifier
{
    public static volatile bool IsEnabled;

    public static void Apply(CalmNotifierOptions options, ISptLogger<CompoundingPerfMod> logger)
    {
        IsEnabled = options.Enabled;
        if (options.Enabled)
        {
            logger.Success("[CompoundingPerf/S13] calm notifier ACTIVE — websocket sends moved outside the global lock, long-polls release their threads");
        }
        else
        {
            logger.Info("[CompoundingPerf/S13] calm notifier disabled in config");
        }
    }
}

/// <summary>
/// S13a — Subclass of <see cref="SptWebSocketConnectionHandler"/> via DI
/// <see cref="Injectable.TypeOverride"/>. Four source-verified defects in the vanilla
/// send path:
/// <list type="number">
///   <item><c>SendMessageToAll</c> holds the global <c>_socketsLock</c> across every
///     network write — one slow client stalls every connect/disconnect/IsConnected
///     call on the server.</item>
///   <item><c>GetSessionWebSocket</c> returns a LAZY LINQ iterator built over the
///     sockets dictionary; the lock is released before enumeration, so the actual
///     dictionary reads race with concurrent connects/disconnects.</item>
///   <item>The payload is re-serialized once per socket (and per session in
///     broadcasts) — vanilla's own comment acknowledges this.</item>
///   <item><c>sendTask.Wait()</c> blocks the calling thread per send (kept deliberately
///     — WebSocket requires one in-flight send per socket, and the callers are
///     low-frequency notification pushes; what matters is that the wait now happens
///     OUTSIDE the lock).</item>
/// </list>
/// <para>Both <c>_sockets</c> and <c>_socketsLock</c> are <c>protected</c>, so the
/// override snapshots open sockets <i>under</i> the lock, serializes the payload once,
/// and performs all network I/O after the lock is released.</para>
/// </summary>
// Lifetime must match vanilla SptWebSocketConnectionHandler
// ([Injectable(InjectionType.Singleton)]) — a scoped override would hand MVC-scoped
// consumers their own handler with an EMPTY socket list. See CoalescingSaveServer.
[Injectable(InjectionType.Singleton, TypeOverride = typeof(SptWebSocketConnectionHandler), TypePriority = 100)]
public class CalmWebSocketHandler(
    ISptLogger<SptWebSocketConnectionHandler>  logger,
    ServerLocalisationService                  serverLocalisationService,
    JsonUtil                                   jsonUtil,
    ProfileHelper                              profileHelper,
    IEnumerable<ISptWebSocketMessageHandler>   messageHandlers)
    : SptWebSocketConnectionHandler(logger, serverLocalisationService, jsonUtil, profileHelper, messageHandlers)
{
    private readonly ISptLogger<SptWebSocketConnectionHandler> _logger = logger;
    private readonly ServerLocalisationService _localisation = serverLocalisationService;
    private readonly JsonUtil _jsonUtil = jsonUtil;

    public override void SendMessageToAll(WsNotificationEvent output)
    {
        if (!CalmNotifier.IsEnabled)
        {
            base.SendMessageToAll(output);
            return;
        }

        // Serialize once for the whole broadcast.
        var payload = Encoding.UTF8.GetBytes(_jsonUtil.Serialize(output, output.GetType()));

        // Snapshot open sockets under the lock; do ALL network I/O outside it.
        List<(MongoId session, WebSocket socket)> targets = [];
        lock (_socketsLock)
        {
            foreach (var (sessionId, sockets) in _sockets)
            {
                foreach (var ws in sockets.Values)
                {
                    if (ws.State == WebSocketState.Open)
                    {
                        targets.Add((sessionId, ws));
                    }
                }
            }
        }

        TelemetryHub.Increment("s13.ws.broadcasts");
        foreach (var (sessionId, socket) in targets)
        {
            SendPayload(sessionId, socket, payload);
        }
    }

    public override void SendMessage(MongoId sessionID, WsNotificationEvent output)
    {
        if (!CalmNotifier.IsEnabled)
        {
            base.SendMessage(sessionID, output);
            return;
        }

        var payload = Encoding.UTF8.GetBytes(_jsonUtil.Serialize(output, output.GetType()));

        // Materialize the open sockets INSIDE the lock — fixes vanilla's lazy-iterator
        // use-after-unlock race.
        WebSocket[] targets;
        lock (_socketsLock)
        {
            targets = _sockets.GetValueOrDefault(sessionID)?.Values.Where(s => s.State == WebSocketState.Open).ToArray() ?? [];
        }

        TelemetryHub.Increment("s13.ws.sends");
        foreach (var socket in targets)
        {
            SendPayload(sessionID, socket, payload);
        }
    }

    private void SendPayload(MongoId sessionId, WebSocket socket, byte[] payload)
    {
        try
        {
            // One in-flight send per socket is a WebSocket protocol requirement; the
            // synchronous wait preserves vanilla's per-socket ordering — but now runs
            // outside the global lock, so it can't stall the rest of the server.
            socket.SendAsync(payload, WebSocketMessageType.Text, true, CancellationToken.None).Wait();
        }
        catch (Exception err)
        {
            _logger.Error(_localisation.GetText("websocket-message_send_failed_with_error", err.Message), err);
        }
    }
}

/// <summary>
/// S13b — Subclass of <see cref="NotifierController"/> via DI
/// <see cref="Injectable.TypeOverride"/>. Vanilla's <c>NotifyAsync</c> long-poll runs
/// <c>Task.Factory.StartNew</c> + <c>Thread.Sleep(300)</c> in a loop for up to 15
/// seconds — pinning one thread-pool thread per connected client, near-permanently
/// (the EFT client re-polls immediately after each response). With N FIKA players
/// that's N pinned threads doing nothing.
///
/// <para>The override keeps the exact same observable behavior — 300ms poll interval,
/// 15s timeout, same default-notification fallback — but uses <c>async</c>/<c>await
/// Task.Delay</c>, which releases the thread between polls.</para>
/// </summary>
[Injectable(TypeOverride = typeof(NotifierController), TypePriority = 100)]
public class CalmNotifierController(
    HttpServerHelper    httpServerHelper,
    NotifierHelper      notifierHelper,
    NotificationService notificationService)
    : NotifierController(httpServerHelper, notifierHelper, notificationService)
{
    private readonly NotifierHelper _notifierHelper = notifierHelper;
    private readonly NotificationService _notificationService = notificationService;

    public override async Task<List<WsNotificationEvent>> NotifyAsync(MongoId sessionId)
    {
        if (!CalmNotifier.IsEnabled)
        {
            return await base.NotifyAsync(sessionId);
        }

        TelemetryHub.Increment("s13.notifier.polls");

        // Mirror of vanilla: PollInterval=300, Timeout=15000 (both protected consts on base).
        var counter = 0;
        while (counter < Timeout)
        {
            if (_notificationService.Has(sessionId))
            {
                var messages = _notificationService.Get(sessionId);
                _notificationService.UpdateMessageOnQueue(sessionId, []);
                return messages;
            }

            counter += PollInterval;
            await Task.Delay(PollInterval); // releases the thread — vanilla pins it with Thread.Sleep
        }

        return [_notifierHelper.GetDefaultNotification()];
    }
}
