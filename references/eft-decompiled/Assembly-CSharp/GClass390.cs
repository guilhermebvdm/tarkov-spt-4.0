using EFT;
using Newtonsoft.Json;
using UnityEngine;

public class GClass390 : ISerializer<NavGraphFindDebug>
{
	[JsonProperty("centerPosition")]
	public Vector3 _centerPosition;

	[JsonProperty("startPoint")]
	public int _startPoint;

	[JsonProperty("shootType")]
	public CoverShootType _shootType;

	[JsonProperty("botId")]
	public int _botId;

	[JsonProperty("steps")]
	public NavGraphFindDebugStepInfo[] _steps;

	[JsonProperty("endInfo")]
	public string _endInfo;

	[JsonProperty("zoneName")]
	public string _zoneName;

	[JsonProperty("ResultWayIds")]
	public int[] ResultWayIds;

	public NavGraphFindDebug Deserialize()
	{
		NavGraphFindDebug navGraphFindDebug = new NavGraphFindDebug(_botId, _zoneName, _shootType);
		navGraphFindDebug.StartPoint = _startPoint;
		navGraphFindDebug.CenterPosition = _centerPosition;
		NavGraphFindDebugStepInfo[] steps = _steps;
		foreach (NavGraphFindDebugStepInfo item in steps)
		{
			navGraphFindDebug.StepsInfo.Add(item);
		}
		navGraphFindDebug.EndInfo = _endInfo;
		navGraphFindDebug.ZoneName = _zoneName;
		navGraphFindDebug.ResultWayIds = ResultWayIds;
		return navGraphFindDebug;
	}

	public object Serialize(NavGraphFindDebug t)
	{
		_centerPosition = t.CenterPosition;
		_startPoint = t.StartPoint;
		_zoneName = t.ZoneName;
		_endInfo = t.EndInfo;
		_botId = t.BotId;
		_shootType = t.ShootType;
		if (t.ResultWay != null)
		{
			ResultWayIds = new int[t.ResultWay.Length];
			for (int i = 0; i < t.ResultWay.Length; i++)
			{
				ResultWayIds[i] = t.ResultWay[i].Id;
			}
		}
		_steps = t.StepsInfo.ToArray();
		return this;
	}
}
