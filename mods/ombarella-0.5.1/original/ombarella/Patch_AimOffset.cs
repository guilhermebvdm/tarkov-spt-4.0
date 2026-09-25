using System;
using System.Reflection;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace ombarella;

public class Patch_AimOffset : ModulePatch
{
	private static readonly FieldInfo AimOffsetField = AccessTools.Field(typeof(BotAimingClass), "float_13") ?? AccessTools.Field(typeof(BotAimingClass), "Float_13");

	private static readonly FieldInfo AimDirectionField = AccessTools.Field(typeof(BotAimingClass), "vector3_4") ?? AccessTools.Field(typeof(BotAimingClass), "Vector3_4");

	protected override MethodBase GetTargetMethod()
	{
		return AccessTools.Method(typeof(BotAimingClass), "method_13", (Type[])null, (Type[])null);
	}

	[PatchPostfix]
	public static void PatchPostfix(BotAimingClass __instance)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)Plugin.Instance == (Object)null) && Plugin.Instance.CanApplyBotVisibilityPatch() && __instance != null && !(AimOffsetField == null) && !(AimDirectionField == null))
		{
			float num = (float)AimOffsetField.GetValue(__instance);
			float num2 = (1f - Plugin.Instance.FinalLightMeter) * 100f;
			num2 *= Plugin.AimNerf.Value;
			num *= num2;
			Vector3 val = (Vector3)AimDirectionField.GetValue(__instance);
			Vector3 endTargetPoint = __instance.RealTargetPoint + val * num;
			__instance.EndTargetPoint = endTargetPoint;
		}
	}
}
