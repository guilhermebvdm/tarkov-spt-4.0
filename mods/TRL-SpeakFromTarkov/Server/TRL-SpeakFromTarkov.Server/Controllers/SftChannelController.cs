using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        public async Task<IActionResult> AnnounceChannel([FromBody] SftChannelActionDto? data)
        {
            if (data == null)
            {
                try
                {
                    using var reader = new System.IO.StreamReader(Request.Body);
                    string rawJson = await reader.ReadToEndAsync();
                    if (!string.IsNullOrEmpty(rawJson))
                    {
                        data = System.Text.Json.JsonSerializer.Deserialize<SftChannelActionDto>(rawJson);
                    }
                }
                catch { }
            }

            if (data == null || data.channelId <= 0) return BadRequest();

            if (data.action == 1) // Close
            {
                _channels.TryRemove(data.channelId, out _);
            }
            else
            {
                var ch = _channels.GetOrAdd(data.channelId, id => new SftMenuChannelDto
                {
                    channelId = id,
                    channelName = data.channelName,
                    hostProfileId = data.hostProfileId,
                    hostNickname = data.hostNickname,
                    lastSeen = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    members = new List<string>(),
                    bannedProfileIds = new List<string>()
                });

                ch.channelName = !string.IsNullOrEmpty(data.channelName) ? data.channelName : ch.channelName;
                ch.hostProfileId = !string.IsNullOrEmpty(data.hostProfileId) ? data.hostProfileId : ch.hostProfileId;
                ch.hostNickname = !string.IsNullOrEmpty(data.hostNickname) ? data.hostNickname : ch.hostNickname;
                ch.lastSeen = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                if (data.action == 2 && !string.IsNullOrEmpty(data.hostProfileId)) // Join
                {
                    if (!ch.members.Contains(data.hostProfileId)) ch.members.Add(data.hostProfileId);
                }
                else if (data.action == 3 && !string.IsNullOrEmpty(data.hostProfileId)) // Leave
                {
                    ch.members.Remove(data.hostProfileId);
                }
                else if (data.action == 4 && !string.IsNullOrEmpty(data.targetProfileId)) // Kick
                {
                    ch.members.Remove(data.targetProfileId);
                }
                else if (data.action == 5 && !string.IsNullOrEmpty(data.targetProfileId)) // Ban
                {
                    ch.members.Remove(data.targetProfileId);
                    if (!ch.bannedProfileIds.Contains(data.targetProfileId)) ch.bannedProfileIds.Add(data.targetProfileId);
                }
            }

            CleanupStaleChannels();
            NotifyClientsWebSocket();
            return Ok(new { status = "OK" });
        }
    }
}
