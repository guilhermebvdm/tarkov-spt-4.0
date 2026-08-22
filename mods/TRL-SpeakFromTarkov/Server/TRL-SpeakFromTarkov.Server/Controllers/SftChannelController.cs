using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SPTarkov.Server.Core.Servers.Ws;
using SPTarkov.Server.Core.Models.Eft.Ws;

namespace TRL_SpeakFromTarkov.Server.Controllers
{
    public record SftWsChannelEvent : WsNotificationEvent
    {
        public SftWsChannelEvent()
        {
            EventType = (NotificationEventType)999;
        }
    }

    public class SftMenuChannelDto
    {
        public int channelId { get; set; }
        public string channelName { get; set; } = string.Empty;
        public string hostProfileId { get; set; } = string.Empty;
        public string hostNickname { get; set; } = string.Empty;
        public long lastSeen { get; set; }
        public List<string> members { get; set; } = new List<string>();
        public List<string> bannedProfileIds { get; set; } = new List<string>();
    }

    public class SftChannelActionDto
    {
        public int channelId { get; set; }
        public string channelName { get; set; } = string.Empty;
        public string hostProfileId { get; set; } = string.Empty;
        public string hostNickname { get; set; } = string.Empty;
        public int action { get; set; } // 0=Announce/Heartbeat, 1=Close, 2=Join, 3=Leave, 4=Kick, 5=Ban
        public string targetProfileId { get; set; } = string.Empty;
    }

    [ApiController]
    [Route("sft/channels")]
    public class SftChannelController(SptWebSocketConnectionHandler webSocketHandler) : ControllerBase
    {
        private static readonly ConcurrentDictionary<int, SftMenuChannelDto> _channels = new ConcurrentDictionary<int, SftMenuChannelDto>();

        private static void CleanupStaleChannels()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            foreach (var kvp in _channels.ToArray())
            {
                if (now - kvp.Value.lastSeen > 30000) // Expirar salas sem heartbeat > 30s
                {
                    _channels.TryRemove(kvp.Key, out _);
                }
            }
        }

        private void NotifyClientsWebSocket()
        {
            try
            {
                webSocketHandler?.SendMessageToAll(new SftWsChannelEvent());
            }
            catch { }
        }

        [HttpGet("list")]
        public IActionResult GetChannelList()
        {
            CleanupStaleChannels();
            return Ok(_channels.Values.ToList());
        }

        [HttpPost("announce")]
        public async Task<IActionResult> AnnounceChannel([FromBody] SftChannelActionDto? request)
        {
            if (request == null || request.channelId <= 0)
            {
                try
                {
                    string rawJson;
                    var requestIsCompressed = !Request.Headers.TryGetValue("requestcompressed", out var compressHeader) || compressHeader != "0";
                    if (requestIsCompressed)
                    {
                        using var deflateStream = new ZLibStream(Request.Body, CompressionMode.Decompress);
                        using var reader = new StreamReader(deflateStream, Encoding.UTF8);
                        rawJson = await reader.ReadToEndAsync();
                    }
                    else
                    {
                        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
                        rawJson = await reader.ReadToEndAsync();
                    }

                    if (!string.IsNullOrEmpty(rawJson))
                    {
                        request = JsonSerializer.Deserialize<SftChannelActionDto>(rawJson);
                    }
                }
                catch { }
            }

            if (request == null || request.channelId <= 0) return BadRequest();

            if (request.action == 1) // Close
            {
                _channels.TryRemove(request.channelId, out _);
            }
            else
            {
                var ch = _channels.GetOrAdd(request.channelId, id => new SftMenuChannelDto
                {
                    channelId = id,
                    channelName = request.channelName,
                    hostProfileId = request.hostProfileId,
                    hostNickname = request.hostNickname,
                    lastSeen = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    members = new List<string>(),
                    bannedProfileIds = new List<string>()
                });

                ch.channelName = !string.IsNullOrEmpty(request.channelName) ? request.channelName : ch.channelName;
                ch.hostProfileId = !string.IsNullOrEmpty(request.hostProfileId) ? request.hostProfileId : ch.hostProfileId;
                ch.hostNickname = !string.IsNullOrEmpty(request.hostNickname) ? request.hostNickname : ch.hostNickname;
                ch.lastSeen = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                if (request.action == 2 && !string.IsNullOrEmpty(request.hostProfileId)) // Join
                {
                    if (!ch.members.Contains(request.hostProfileId)) ch.members.Add(request.hostProfileId);
                }
                else if (request.action == 3 && !string.IsNullOrEmpty(request.hostProfileId)) // Leave
                {
                    ch.members.Remove(request.hostProfileId);
                }
                else if (request.action == 4 && !string.IsNullOrEmpty(request.targetProfileId)) // Kick
                {
                    ch.members.Remove(request.targetProfileId);
                }
                else if (request.action == 5 && !string.IsNullOrEmpty(request.targetProfileId)) // Ban
                {
                    ch.members.Remove(request.targetProfileId);
                    if (!ch.bannedProfileIds.Contains(request.targetProfileId)) ch.bannedProfileIds.Add(request.targetProfileId);
                }
            }

            CleanupStaleChannels();
            NotifyClientsWebSocket();
            return Ok(new { status = "OK" });
        }
    }
}
