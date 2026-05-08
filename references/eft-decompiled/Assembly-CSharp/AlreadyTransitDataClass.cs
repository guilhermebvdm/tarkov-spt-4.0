using System.Collections.Generic;
using EFT;
using JsonType;

public class AlreadyTransitDataClass
{
	public string hash;

	public int playersCount;

	public string ip;

	public string location;

	public Dictionary<string, ProfileKey> profiles;

	public string transitionRaidId;

	public ERaidMode raidMode;

	public ESideType side;

	public EDateTime dayTime;

	public override string ToString()
	{
		return $"hash: {hash}, players: {playersCount}, location: {location}, raidId: {transitionRaidId}, ip: {ip}";
	}
}
