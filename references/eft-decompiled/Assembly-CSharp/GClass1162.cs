using System;
using Audio.SpatialSystem;
using UnityEngine;

public abstract class GClass1162
{
	public const float LOWER_OCCLUSION_FREQUENCY = 450f;

	public const float UPPER_OCCLUSION_FREQUENCY = 700f;

	public const float WALL_OCCLUSION_FREQUENCY = 1600f;

	public const float FULL_OCCLUSION_FREQUENCY = 0f;

	public const float DEFAULT_OCCLUSION_FREQUENCY = 5000f;

	[NonSerialized]
	public const float Float_0 = 0.55f;

	public static int HighPolyOcclusionMask => (1 << LayerMask.NameToLayer("HighPolyCollider")) | (1 << LayerMask.NameToLayer("Terrain")) | (1 << LayerMask.NameToLayer("TransparentCollider"));

	public static int LowPolyOcclusionMask => (1 << LayerMask.NameToLayer("LowPolyCollider")) | (1 << LayerMask.NameToLayer("Terrain")) | (1 << LayerMask.NameToLayer("TransparentCollider"));

	public static bool IsOcclusionTestValid(float effect)
	{
		return effect > 0.55f;
	}

	public static bool IsNeedVerticalOcclusion(Vector3 soundPosition, Vector3 listenerPosition, out float verticalDot)
	{
		float y = (soundPosition - listenerPosition).y;
		verticalDot = CalculateVerticalDot(soundPosition, listenerPosition);
		EFTHardSettings instance = EFTHardSettings.Instance;
		if (!(y < instance.ABOVE_OR_BELOW.x) && !(y > instance.ABOVE_OR_BELOW.y) && !(verticalDot < instance.VERTICAL_DOT_RANGE.x))
		{
			return verticalDot > instance.VERTICAL_DOT_RANGE.y;
		}
		return true;
	}

	public static float CalculateVerticalDot(Vector3 soundPosition, Vector3 listenerPosition)
	{
		return (soundPosition - listenerPosition).normalized.y;
	}

	public static bool SimpleOcclusionTest(Vector3 soundPos, Vector3 listenerPos)
	{
		RaycastHit hitInfo;
		return GClass943.GetNearestHit(soundPos, listenerPos, out hitInfo, HighPolyOcclusionMask);
	}

	public static bool SimpleOcclusionTest(Vector3 soundPos, Vector3 listenerPos, int mask)
	{
		RaycastHit hitInfo;
		return GClass943.GetNearestHit(soundPos, listenerPos, out hitInfo, mask);
	}

	public static bool SimpleOcclusionTest(Vector3 soundPos, Vector3 listenerPos, float distance, int mask)
	{
		RaycastHit hitInfo;
		return GClass943.GetNearestHit(soundPos, listenerPos, out hitInfo, distance, mask);
	}

	public static float GetVerticalFactor(Vector3 soundPosition, Vector3 listenerPosition)
	{
		float num = Mathf.Abs(soundPosition.y - listenerPosition.y);
		if (num <= 1.8f)
		{
			return 1f;
		}
		EFTHardSettings instance = EFTHardSettings.Instance;
		float time = Mathf.InverseLerp(20f, 0f, num);
		return instance.StairsSoundOcclusionCurve.Evaluate(time) * instance.VerticalSoundFactor;
	}

	public static float GetVerticalMult(Vector3 soundPosition, Vector3 listenerPosition)
	{
		float num = Mathf.Abs(listenerPosition.y - soundPosition.y);
		Vector3 normalized = (soundPosition - listenerPosition).normalized;
		float num2 = Vector3.Angle((listenerPosition - soundPosition).normalized, normalized);
		float num3 = 1f / (num + 1f);
		float num4 = Mathf.Cos(num2 * (MathF.PI / 180f));
		return Mathf.Abs((float)Math.Round(num3 * num4, 2, MidpointRounding.ToEven));
	}

	public static float CalculateHorizontalDiffraction(Vector3 listenerPos, Vector3 targetPos)
	{
		Vector3 vector = listenerPos - targetPos;
		float num = Mathf.Atan2(vector.z, vector.x) * 57.29578f;
		num = ((!(vector.x < 0f)) ? (num + 180f) : (num - 180f));
		if (num < -180f)
		{
			num += 360f;
		}
		else if (num > 180f)
		{
			num -= 360f;
		}
		return Mathf.Abs(Mathf.Cos(num * (MathF.PI / 180f)));
	}

	public static float CalculateVerticalDiffraction(Vector3 listenerPos, Vector3 targetPos)
	{
		Vector3 vector = listenerPos - targetPos;
		float num = Mathf.Atan2(vector.y, vector.z) * 57.29578f;
		num = ((!(vector.z < 0f)) ? (num + 180f) : (num - 180f));
		if (num < -180f)
		{
			num += 360f;
		}
		else if (num > 180f)
		{
			num -= 360f;
		}
		return 1f - Mathf.Cos(num * (MathF.PI / 180f));
	}

	public static float CalculateTotalDiffraction(Vector3 listenerPos, Vector3 targetPos)
	{
		float num = CalculateHorizontalDiffraction(listenerPos, targetPos);
		float num2 = CalculateVerticalDiffraction(listenerPos, targetPos);
		return (num + num2) / 2f;
	}

	public static float GetPortalArea(BaseSpatialAudioPortal portal)
	{
		Vector3 localScale = portal.transform.localScale;
		float x = localScale.x;
		float y = localScale.y;
		return x * y;
	}
}
