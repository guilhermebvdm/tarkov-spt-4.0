using System;
using System.Collections.Generic;
using EFT;
using Newtonsoft.Json;

public class ProfileStatusClass
{
	[NonSerialized]
	public static Dictionary<EProfileStatus, HashSet<EProfileStatus>> Dictionary_0 = new Dictionary<EProfileStatus, HashSet<EProfileStatus>>
	{
		{
			EProfileStatus.Busy,
			new HashSet<EProfileStatus> { EProfileStatus.MatchWait }
		},
		{
			EProfileStatus.Leaving,
			new HashSet<EProfileStatus>
			{
				EProfileStatus.Busy,
				EProfileStatus.MatchWait
			}
		}
	};

	[JsonProperty("profileid")]
	public string profileid;

	[JsonProperty("profileToken")]
	public string profileToken;

	[JsonProperty("status")]
	public EProfileStatus status;

	[JsonProperty("raidMode")]
	public ERaidMode raidMode;

	[JsonProperty("ip")]
	public string ip;

	[JsonProperty("port")]
	public int port;

	[JsonProperty("location")]
	public string location;

	[JsonProperty("sid")]
	public string sid;

	[JsonProperty("mode")]
	public string gameMode;

	[JsonProperty("shortId")]
	public string shortId;

	public bool IsAppropriateStatusChange(EProfileStatus newValue)
	{
		if (Dictionary_0.TryGetValue(status, out var value))
		{
			return !value.Contains(newValue);
		}
		return true;
	}

	public override string ToString()
	{
		return $"Profileid: {profileid}, Status: {status}, RaidMode: {raidMode}, Ip: {ip}, Port: {port}, Location: {location}, Sid: {sid}, GameMode: {gameMode}, shortId: {shortId}";
	}
}
