using System;
using System.Reflection;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace ombarella;

public class Patch_VisionSpeed : ModulePatch
{
	private static readonly Type[] VisibilitySpeedParameters311 = new Type[6]
	{
		typeof(BotDifficultySettingsClass),
		typeof(IAIData),
		typeof(float),
		typeof(Vector3),
		typeof(float),
		typeof(float)
	};

	private static readonly Type[] VisibilitySpeedParameters400 = new Type[7]
	{
		typeof(BotDifficultySettingsClass),
		typeof(IAIData),
		typeof(float),
		typeof(Vector3),
		typeof(float),
		typeof(float),
		typeof(float)
	};

	protected override MethodBase GetTargetMethod()
	{
		MethodInfo methodInfo = AccessTools.Method(typeof(EnemyInfo), "method_9", VisibilitySpeedParameters400, (Type[])null) ?? AccessTools.Method(typeof(EnemyInfo), "method_7", VisibilitySpeedParameters311, (Type[])null) ?? AccessTools.Method(typeof(EnemyInfo), "method_9", VisibilitySpeedParameters311, (Type[])null) ?? FindVisibilitySpeedMethod();
		if (methodInfo == null)
		{
			throw new MissingMethodException("Unable to find EnemyInfo visibility-speed multiplier.");
		}
		if (Utils.Logger != null)
		{
			Utils.Logger.LogInfo((object)$"Ombarella vision speed patch target: EnemyInfo.{methodInfo.Name}({methodInfo.GetParameters().Length} params)");
		}
		return methodInfo;
	}

	[PatchPostfix]
	public static void PatchPostfix(ref float __result, EnemyInfo __instance)
	{
		if (!((Object)(object)Plugin.Instance == (Object)null) && Plugin.Instance.CanApplyBotVisibilityPatch() && __instance != null)
		{
			float num = Plugin.Instance.FinalLightMeter;
			if (__instance.HaveNightVision())
			{
				num = Mathf.Lerp(num, 1f, 0.7f);
			}
			__result *= num;
		}
	}

	private static MethodInfo FindVisibilitySpeedMethod()
	{
		MethodInfo[] methods = typeof(EnemyInfo).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (MethodInfo methodInfo in methods)
		{
			if (!(methodInfo.ReturnType != typeof(float)))
			{
				ParameterInfo[] parameters = methodInfo.GetParameters();
				if (ParametersMatch(parameters, VisibilitySpeedParameters311) || ParametersMatch(parameters, VisibilitySpeedParameters400))
				{
					return methodInfo;
				}
			}
		}
		return null;
	}

	private static bool ParametersMatch(ParameterInfo[] parameters, Type[] expectedTypes)
	{
		if (parameters.Length != expectedTypes.Length)
		{
			return false;
		}
		for (int i = 0; i < parameters.Length; i++)
		{
			if (parameters[i].ParameterType != expectedTypes[i])
			{
				return false;
			}
		}
		return true;
	}
}
