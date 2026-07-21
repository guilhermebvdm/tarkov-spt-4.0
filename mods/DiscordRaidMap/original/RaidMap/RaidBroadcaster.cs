using System;
using System.Threading.Tasks;
using DiscordRaidMap.Discord;
using UnityEngine;

namespace DiscordRaidMap.RaidMap
{
    internal class RaidBroadcaster
    {
        private readonly RaidStateCollector _collector;
        private readonly DiscordWebhookClient _discord;
        private readonly object _rendererLock = new();

        private Renderer _renderer;
        private float _updateIntervalSeconds;

        private float _nextUpdateTime;
        private volatile bool _started;
        private volatile bool _uploading;

        public RaidBroadcaster(RaidStateCollector collector, Renderer renderer, DiscordWebhookClient discord, int updateIntervalSeconds)
        {
            _collector = collector;
            _renderer = renderer;
            _discord = discord;
            SetUpdateInterval(updateIntervalSeconds);
        }

        public void Start()
        {
            _started = true;
            _nextUpdateTime = 0f;
        }

        public void Update()
        {
            if (!_started || _uploading || Time.unscaledTime < _nextUpdateTime)
            {
                return;
            }

            _nextUpdateTime = Time.unscaledTime + _updateIntervalSeconds;

            try
            {
                var snapshot = _collector.CollectSnapshot();
                if (snapshot == null)
                {
                    return;
                }

                _uploading = true;
                _ = RenderAndUploadAsync(snapshot);
            }
            catch (Exception ex)
            {
                _uploading = false;
                Plugin.Log.LogError($"Discord map update failed: {ex}");
            }
        }

        private async Task RenderAndUploadAsync(RaidSnapshot snapshot)
        {
            try
            {
                var png = await Task.Run(() =>
                {
                    lock (_rendererLock)
                    {
                        return _renderer.Render(snapshot);
                    }
                });

                await _discord.UpsertMessageAsync(png);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning($"Discord map upload failed: {ex.Message}");
            }
            finally
            {
                _uploading = false;
                if (!_started)
                {
                    lock (_rendererLock)
                    {
                        _renderer.Dispose();
                    }

                    await _discord.DeleteMessageAsync();
                }
            }
        }

        public void SetUpdateInterval(int updateIntervalSeconds)
        {
            _updateIntervalSeconds = updateIntervalSeconds;
            _nextUpdateTime = Time.unscaledTime + _updateIntervalSeconds;
        }

        public void ReplaceRenderer(Renderer renderer)
        {
            lock (_rendererLock)
            {
                _renderer.Dispose();
                _renderer = renderer;
            }
        }

        public void Stop()
        {
            _started = false;
            _collector.Dispose();
            if (!_uploading)
            {
                lock (_rendererLock)
                {
                    _renderer.Dispose();
                }

                _ = _discord.DeleteMessageAsync();
            }
        }
    }
}
