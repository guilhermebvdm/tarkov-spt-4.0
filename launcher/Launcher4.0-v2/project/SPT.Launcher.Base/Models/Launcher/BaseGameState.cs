using System;
using Newtonsoft.Json;

namespace SPT.Launcher.Models.Launcher
{
    public class BaseGameState
    {
        [JsonProperty("status")]
        public string Status { get; set; } = "NotInstalled";

        [JsonProperty("completed")]
        public bool Completed { get; set; } = false;

        [JsonProperty("torrentHash")]
        public string TorrentHash { get; set; } = "";

        [JsonProperty("progressPercentage")]
        public double ProgressPercentage { get; set; } = 0.0;

        [JsonProperty("totalBytes")]
        public long TotalBytes { get; set; } = 0;

        [JsonProperty("downloadedBytes")]
        public long DownloadedBytes { get; set; } = 0;

        [JsonProperty("installedVersion")]
        public string InstalledVersion { get; set; } = "";

        [JsonProperty("lastUpdated")]
        public DateTime? LastUpdated { get; set; }
    }
}
