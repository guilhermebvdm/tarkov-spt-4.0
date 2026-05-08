using System.Collections.Generic;
using JsonType;

public class LoginResponseClass
{
	public bool queued;

	public double banTime;

	public string hash;

	public string lang;

	public string aid;

	public string token;

	public string taxonomy;

	public string activeProfileId;

	public string nickname;

	public double utc_time;

	public GClass1392 backend;

	public long totalInGame;

	public bool reportAvailable;

	public Dictionary<EGame, bool> purchasedGames;

	public bool isGameSynced;
}
