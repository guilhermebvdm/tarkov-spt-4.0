using System.Collections.Generic;
using EFT;
using UnityEngine;

namespace ombarella;

public static class CameraRig
{
	public static Camera _lightCam;

	public static void Initialize(Camera camera)
	{
		_lightCam = camera;
	}

	public static void RepositionCamera(List<Player> targetList)
	{
		TryRepositionCamera(Utils.GetMainPlayer(), targetList);
	}

	public static bool TryRepositionCamera(Player player, List<Player> observerList)
	{
		Vector3 focusPoint;
		return TryRepositionCamera(player, observerList, out focusPoint);
	}

	public static bool TryRepositionCamera(Player player, List<Player> observerList, out Vector3 focusPoint)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		focusPoint = Vector3.zero;
		if (!Utils.IsLightMeterUsablePlayer(player) || observerList == null || observerList.Count == 0 || (Object)(object)_lightCam == (Object)null)
		{
			return false;
		}
		float num = float.MaxValue;
		Player val = null;
		foreach (Player observer in observerList)
		{
			if (!((Object)(object)observer == (Object)(object)player) && Utils.IsLightMeterUsablePlayer(observer))
			{
				float num2 = Vector3.Distance(observer.Position, player.Position);
				if (!float.IsNaN(num2) && !float.IsInfinity(num2) && num2 < num)
				{
					num = num2;
					val = observer;
				}
			}
		}
		if ((Object)(object)val == (Object)null)
		{
			return false;
		}
		focusPoint = GetPlayerFocusPoint(player);
		Vector3 val2 = focusPoint - val.PlayerBones.Head.position;
		if (((Vector3)(ref val2)).sqrMagnitude <= 0.0001f)
		{
			return false;
		}
		Vector3 val3 = focusPoint - val2;
		((Component)_lightCam).gameObject.transform.position = val3;
		((Component)_lightCam).gameObject.transform.rotation = Quaternion.LookRotation(focusPoint - val3);
		return true;
	}

	public static bool TryRepositionCameraOnOrbit(Player player, float radius, float heightOffset, float angleDegrees, out Vector3 focusPoint)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		focusPoint = Vector3.zero;
		if (!Utils.IsLightMeterUsablePlayer(player) || (Object)(object)_lightCam == (Object)null)
		{
			return false;
		}
		focusPoint = GetPlayerFocusPoint(player);
		if (!Utils.IsFinite(focusPoint))
		{
			return false;
		}
		radius = Mathf.Max(0.1f, radius);
		Vector3 val = Quaternion.Euler(0f, angleDegrees, 0f) * (Vector3.forward * radius);
		Vector3 val2 = focusPoint + val + Vector3.up * heightOffset;
		Vector3 val3 = focusPoint - val2;
		if (((Vector3)(ref val3)).sqrMagnitude <= 0.0001f)
		{
			return false;
		}
		((Component)_lightCam).gameObject.transform.position = val2;
		((Component)_lightCam).gameObject.transform.rotation = Quaternion.LookRotation(val3);
		return true;
	}

	public static Vector3 GetPlayerFocusPoint(Player player)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		return player.PlayerBones.Ribcage.position + Vector3.up * Plugin.CameraFocusHeightOffset.Value;
	}
}
