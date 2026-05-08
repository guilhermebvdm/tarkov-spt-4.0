using Comfort.Common;
using EFT;
using UnityEngine;

public abstract class GClass1891
{
	public static float PastTime
	{
		get
		{
			AbstractGame instance = Singleton<AbstractGame>.Instance;
			if (!(instance == null))
			{
				return instance.PastTime;
			}
			return Time.timeSinceLevelLoad;
		}
	}
}
