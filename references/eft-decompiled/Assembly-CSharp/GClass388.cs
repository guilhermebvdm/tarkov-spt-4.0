using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

public class GClass388
{
	[NonSerialized]
	public Dictionary<int, GClass387> Dictionary_0 = new Dictionary<int, GClass387>();

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public GClass387 Gclass387_0;

	[NonSerialized]
	public string String_0;

	[NonSerialized]
	[CompilerGenerated]
	public Vector3 Vector3_0;

	public Vector3 StartPos
	{
		[CompilerGenerated]
		get
		{
			return Vector3_0;
		}
	}

	public GClass388(IBot bot, Vector3 startPos)
	{
		if (method_0())
		{
			Vector3_0 = startPos;
			String_0 = $"ID:{bot.Id} role:{bot.Type.ToString()}  startPos:{startPos}";
			StepRecursiveState();
		}
	}

	public void StepRecursiveState()
	{
		if (method_0())
		{
			GClass387 gClass = new GClass387();
			Dictionary_0.Add(Int_0, gClass);
			Int_0++;
			Gclass387_0 = gClass;
		}
	}

	public void LeaveRecursive(string coverFound)
	{
		if (method_0())
		{
			Gclass387_0.info = Gclass387_0.info + " Leave:" + coverFound;
		}
	}

	public void AddKeyInfo(string info, Func<Tuple<float, float>> getRad)
	{
		if (method_0())
		{
			Tuple<float, float> tuple = getRad();
			float item = tuple.Item1;
			float item2 = tuple.Item2;
			Gclass387_0.info = $"{info} dist:{Mathf.Sqrt(item)}-{Mathf.Sqrt(item2)}";
		}
	}

	public void AddGroupsAtStep(HashSet<CustomNavigationPoint> nearGroups, int level)
	{
		if (!method_0())
		{
			return;
		}
		foreach (CustomNavigationPoint nearGroup in nearGroups)
		{
			Gclass387_0.Set.Add(nearGroup.Id);
		}
	}

	public void TryPrint(CustomNavigationPoint point, PointsArrayType arrayType, Vector3? pointToBeClose, CoverSearchData data, bool logAsError)
	{
		if (!method_0())
		{
			return;
		}
		if (point != null)
		{
			float num = Mathf.Sqrt(data.Bot.Settings.Cover.MIN_TO_ENEMY_TO_BE_NOT_SAFE_SQRT);
			StringBuilder stringBuilder = new StringBuilder("\nDistance to care:");
			foreach (Vector3 carePosition in data.CarePositions)
			{
				float magnitude = (carePosition - data.CenterPos).magnitude;
				stringBuilder.Append($" {magnitude} pos:{carePosition}");
			}
			stringBuilder.Append($" minDist:{num}");
			stringBuilder.Append($" UseLineCastToCover:{data.UseLineCastToCover}");
			stringBuilder.Append($" UseAngCastToCover:{data.UseAngCastToCover}");
			string text = (data.EnvironmentId.HasValue ? data.EnvironmentId.Value.ToString() : "no id");
			stringBuilder.Append(" EnvId:" + text);
			if (data.Shoot2Point != null)
			{
				stringBuilder.Append(" Shoot2Point:" + data.Shoot2Point.Point.ToString() + " ");
			}
			stringBuilder.Append(" shootType:" + data.shootType.ToString() + " ");
			string text2 = stringBuilder.ToString();
			String_0 = string.Format("{0} result:{1}  arrayType:{2}  botpos:{3} pointToBeClose:{4}  {5}", String_0, point.Id, arrayType.ToString(), data.Bot.Position, pointToBeClose.HasValue ? pointToBeClose.Value.ToString() : "null", text2);
		}
		StringBuilder stringBuilder2 = new StringBuilder(String_0);
		foreach (KeyValuePair<int, GClass387> item in Dictionary_0)
		{
			stringBuilder2.Append($"\nKey: {item.Key}   Reason:{item.Value.info}[");
			foreach (int item2 in item.Value.Set)
			{
				stringBuilder2.Append($",{item2}");
			}
			stringBuilder2.Append(",]");
		}
		string text3 = stringBuilder2.ToString();
		string message = "Ids:" + text3;
		if (logAsError)
		{
			Debug.LogError(message);
		}
	}

	public bool method_0()
	{
		return GClass398.Instance.IsTraceEnable();
	}
}
