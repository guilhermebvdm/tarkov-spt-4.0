using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BotZoneEntranceInfo : MonoBehaviour
{
	[FormerlySerializedAs("EntraceList")]
	public List<BotZoneEntrance> EntranceList = new List<BotZoneEntrance>();

	private Dictionary<int, List<BotZoneEntrance>> dictionary_0 = new Dictionary<int, List<BotZoneEntrance>>();

	public void Init()
	{
		foreach (BotZoneEntrance entrance in EntranceList)
		{
			if (dictionary_0.TryGetValue(entrance.ConnectedAreaId, out var value))
			{
				value.Add(entrance);
				continue;
			}
			value = new List<BotZoneEntrance>();
			dictionary_0.Add(entrance.ConnectedAreaId, value);
		}
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		foreach (BotZoneEntrance entrance in EntranceList)
		{
			entrance.DrawGizmosSelected();
		}
	}

	public BotZoneEntrance GetClosest(Vector3 pos, int areaId)
	{
		if (dictionary_0.TryGetValue(areaId, out var value))
		{
			BotZoneEntrance result = null;
			float num = float.MaxValue;
			{
				foreach (BotZoneEntrance item in value)
				{
					float sqrMagnitude = (item.CenterPoint - pos).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						num = sqrMagnitude;
						result = item;
					}
				}
				return result;
			}
		}
		return null;
	}
}
