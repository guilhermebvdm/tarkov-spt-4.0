namespace SPT.Launcher.Models.Launcher
{
    public class ServerBandwidthStatus
    {
        public bool InRaid { get; set; }
        public int ActiveRaids { get; set; }
        public int MaxDownloadRateBytesSec { get; set; }
        public double MaxDownloadRateMBps { get; set; }
        public string Mode { get; set; } = "Turbo";
    }
}
