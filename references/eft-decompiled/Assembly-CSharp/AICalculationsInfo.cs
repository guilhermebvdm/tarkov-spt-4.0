using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class AICalculationsInfo : MonoBehaviour
{
	[CompilerGenerated]
	public class Class292
	{
		public EAICalcSystem type;

		public bool method_0(BaseAICalcSystemInfo x)
		{
			return x.Name == type;
		}
	}

	public List<BaseAICalcSystemInfo> Infos = new List<BaseAICalcSystemInfo>();

	public void Save(BaseAICalcSystemInfo system)
	{
		foreach (BaseAICalcSystemInfo info in Infos)
		{
			if (info.Name == system.Name)
			{
				Infos.Remove(info);
				break;
			}
		}
		Infos.Add(system);
	}

	public static AICalculationsInfo CreateOrFind()
	{
		AICalculationsInfo aICalculationsInfo = Object.FindObjectOfType<AICalculationsInfo>();
		if (aICalculationsInfo != null)
		{
			if (aICalculationsInfo.transform.parent != null)
			{
				Debug.LogError($"Error AICalculationsInfo creates:{aICalculationsInfo.transform.parent.gameObject}");
			}
			return aICalculationsInfo;
		}
		if (aICalculationsInfo == null)
		{
			aICalculationsInfo = new GameObject("AICalculationsInfo").AddComponent<AICalculationsInfo>();
			BotZone botZone = Object.FindObjectOfType<BotZone>();
			aICalculationsInfo.transform.SetParent(botZone.transform);
			aICalculationsInfo.transform.SetParent(null);
		}
		return aICalculationsInfo;
	}

	public static void UnSuccess(EAICalcSystem type)
	{
		BaseAICalcSystemInfo baseAICalcSystemInfo = CreateOrFind().Infos.FirstOrDefault((BaseAICalcSystemInfo x) => x.Name == type);
		baseAICalcSystemInfo.Success = false;
	}

	public bool HaveErrors()
	{
		foreach (BaseAICalcSystemInfo info in Infos)
		{
			if (!info.Success)
			{
				return true;
			}
		}
		return false;
	}
}
