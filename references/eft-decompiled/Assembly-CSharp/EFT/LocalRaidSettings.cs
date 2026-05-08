using System;
using JsonType;
using Newtonsoft.Json;

namespace EFT;

[Serializable]
public class LocalRaidSettings
{
	public string serverId;

	public string location;

	public EDateTime timeVariant;

	public ELocalMode mode;

	public ESideType playerSide;

	public ELocationTransition transitionType;

	public RaidTransitionInfoClass transition;

	[JsonIgnore]
	public LocationSettingsClass.Location selectedLocation;

	public override string ToString()
	{
		return $"[mode-{mode}] side-{playerSide} id-{serverId} location-{location} time-{timeVariant} {transition}";
	}
}
