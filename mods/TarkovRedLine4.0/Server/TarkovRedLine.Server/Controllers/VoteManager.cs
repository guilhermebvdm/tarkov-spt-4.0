using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace TarkovRedLine.Server.Controllers;

[ApiController]
[Route("redline")]
public class VoteManagerController : ControllerBase
{
    private const long VOTE_TIME = 180000;    // 3 minutos
    private const long VETO_TIME = 30000;     // 30 segundos (tempo encurtado no veto)
    private const long COOLDOWN_TIME = 600000; // 10 minutos

    private static bool _inProgress = false;
    private static bool _isVetoed = false;
    private static bool _triggerRestart = false;
    private static long _endTime = 0;
    private static long _originalEndTime = 0;
    private static long _cooldownUntil = 0;
    private static int _yesVotes = 0;
    private static int _noVotes = 0;
    private static Dictionary<string, bool> _votedIPs = new();
    
    private static readonly object _lock = new();

    public class VoteRequest
    {
        public bool vote { get; set; }
        public string clientId { get; set; }
    }

    public class CancelRequest
    {
        public string clientId { get; set; }
    }

    [HttpGet("vote/status")]
    public IActionResult GetStatus()
    {
        lock (_lock)
        {
            CheckVoteLogic();

            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long timeLeft = _inProgress ? Math.Max(0, _endTime - now) : 0;
            long cooldownLeft = Math.Max(0, _cooldownUntil - now);

            return Ok(new {
                inProgress = _inProgress,
                isVetoed = _isVetoed,
                yesVotes = _yesVotes,
                noVotes = _noVotes,
                timeLeft = timeLeft,
                cooldownLeft = cooldownLeft,
                triggerRestart = _triggerRestart
            });
        }
    }

    [HttpPost("vote/cast")]
    public IActionResult CastVote([FromBody] VoteRequest request)
    {
        lock (_lock)
        {
            CheckVoteLogic();
            
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var voteValue = request?.vote ?? false;
            var voterId = !string.IsNullOrEmpty(request?.clientId) 
                ? request.clientId 
                : HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            if (!_inProgress)
            {
                if (now < _cooldownUntil)
                {
                    return StatusCode(403);
                }

                _inProgress = true;
                _isVetoed = false;
                _yesVotes = 0;
                _noVotes = 0;
                _endTime = now + VOTE_TIME;
                _originalEndTime = _endTime;
                _votedIPs.Clear();
            }

            if (_votedIPs.ContainsKey(voterId))
            {
                return Ok(new { status = "already_voted" });
            }

            _votedIPs[voterId] = voteValue;

            if (voteValue == false)
            {
                _noVotes++;
                if (!_isVetoed)
                {
                    _isVetoed = true;
                    _endTime = now + VETO_TIME;
                }
            }
            else
            {
                _yesVotes++;
            }

            return Ok(new { status = "ok" });
        }
    }

    [HttpPost("vote/cancel")]
    public IActionResult CancelVote([FromBody] CancelRequest request)
    {
        lock (_lock)
        {
            CheckVoteLogic();

            var voterId = !string.IsNullOrEmpty(request?.clientId) 
                ? request.clientId 
                : HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            if (_inProgress && _votedIPs.TryGetValue(voterId, out bool previousVote))
            {
                if (previousVote == true)
                {
                    if (_yesVotes > 0) _yesVotes--;
                }
                else
                {
                    if (_noVotes > 0) _noVotes--;
                    if (_noVotes == 0 && _isVetoed)
                    {
                        _isVetoed = false;
                        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        if (_originalEndTime > now)
                        {
                            _endTime = _originalEndTime;
                        }
                    }
                }
                _votedIPs.Remove(voterId);
            }

            return Ok(new { status = "cancelled", currentVotes = _yesVotes });
        }
    }

    [HttpPost("vote/terminate")]
    public IActionResult TerminateVote()
    {
        lock (_lock)
        {
            if (_inProgress)
            {
                _inProgress = false;
                _endTime = 0;
                _yesVotes = 0;
                _noVotes = 0;
                _votedIPs.Clear();
            }

            return Ok(new { status = "terminated" });
        }
    }
    
    [HttpPost("headless/ack-restart")]
    public IActionResult AckRestart()
    {
        lock (_lock)
        {
            _triggerRestart = false;
            return Ok(new { status = "ok" });
        }
    }

    private void CheckVoteLogic()
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Fallback de segurança: limpa a flag 60s após ser ativada caso o ACK do Headless nunca chegue
        if (_triggerRestart && now > _endTime + 60000)
        {
            _triggerRestart = false;
        }

        if (!_inProgress) return;

        if (now >= _endTime)
        {
            _inProgress = false;
            _cooldownUntil = now + COOLDOWN_TIME;
            _votedIPs.Clear();

            if (_isVetoed)
            {
                // Encerrado por VETO
            }
            else if (_yesVotes == 0)
            {
                // Encerrado: 0 votos
            }
            else
            {
                // APROVADO! Enviando comando de SHUTDOWN para o plugin Headless...
                _triggerRestart = true;
            }
        }
    }
}
