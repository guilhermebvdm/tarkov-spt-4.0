using Newtonsoft.Json;

public class GClass1706
{
	public class GClass1707
	{
		public bool Enabled;

		public double RecordTriggerValue;

		public int MaxRecords;
	}

	public class GClass1708
	{
		public int MinFramerateLimit = -1;

		public int MaxFramerateLobbyLimit = -1;

		public int MaxFramerateGameLimit = -1;
	}

	public class GClass1709
	{
		public int LossThreshold;

		public int RttThreshold;
	}

	public const int DEFAULT_SEND_RATE_LIMIT = 100;

	public const int DEFAULT_KEEP_ALIVE_LIMIT = 60;

	public const int DEFAULT_GROUP_STATUS_LIMIT = 20;

	public const int DEFAULT_GROUP_STATUS_BUTTON_UPDATE_LIMIT = 7;

	public const int DEFAULT_PING_SERVER_RESULT_SEND_INTERVAL = 20;

	public const int DEFAULT_PING_SERVERS_INTERVAL = 5;

	public bool TurnOffLogging;

	public int ClientSendRateLimit = 100;

	public int KeepAliveInterval = 60;

	public int GroupStatusInterval = 20;

	public int GroupStatusButtonInterval = 7;

	public int PingServerResultSendInterval = 20;

	public int PingServersInterval = 5;

	public int FirstCycleDelaySeconds = 20;

	public int SecondCycleDelaySeconds = 40;

	public int NextCycleDelaySeconds = 60;

	public int AdditionalRandomDelaySeconds = 10;

	public int DefaultRetriesCount = 5;

	public int CriticalRetriesCount = 10;

	public bool Mark502and504AsNonImportant;

	public bool DisablePrewarmsSoundsForObservers;

	public GClass1708 FramerateLimit;

	public GClass1709 NetworkStateView;

	public MemoryControllerClass.MemoryManagementSettings MemoryManagementSettings = MemoryControllerClass.MemoryManagementSettings.Default;

	public bool NVidiaHighlights;

	public GClass1707 ReleaseProfiler = new GClass1707();

	public bool WebDiagnosticsEnabled;

	public float WeaponOverlapDistanceCulling = -1f;

	public float AFKTimeoutSeconds = 600f;

	public string[] RequestsMadeThroughLobby;

	public float LobbyKeepAliveInterval;

	public float[] RequestConfirmationTimeouts;

	public bool ShouldEstablishLobbyConnection;

	[JsonProperty("AudioSettings")]
	public GClass1096 AudioSettings = new GClass1096();
}
