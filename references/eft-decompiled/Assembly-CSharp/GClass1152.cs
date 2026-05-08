using System;
using Comfort.Common;
using EFT;
using EFT.EnvironmentEffect;
using EFT.Interactive;
using UnityEngine;
using UnityEngine.Audio;

public abstract class GClass1152
{
	[NonSerialized]
	public static RaycastHit RaycastHit_0;

	[NonSerialized]
	public static float Float_0;

	[NonSerialized]
	public static float Float_1;

	public static Player Player_0 => MonoBehaviourSingleton<BetterAudio>.Instance.ListenerPlayer;

	public static bool OcclusionTest(Vector3 origin, Vector3 destination, float distance)
	{
		return smethod_0(origin, destination, distance, MonoBehaviourSingleton<BetterAudio>.Instance.OcclusionMask);
	}

	public static bool smethod_0(Vector3 origin, Vector3 destination, float distance, int mask)
	{
		return GClass943.GetNearestHit(origin, destination, out RaycastHit_0, distance, mask);
	}

	public static void CalculateGunshotsOcclusion(Vector3 gunPosition, out float volume, ref float lowerFrequency)
	{
		volume = 1f;
		if (CameraClass.Instance.Camera == null)
		{
			return;
		}
		Vector3 position = CameraClass.Instance.Camera.transform.position;
		GStruct437 relation = BetterPropagationVolume.GetRelation(MonoBehaviourSingleton<BetterAudio>.Instance.ListenerPlayer, null, position, gunPosition);
		if (relation.HasSelfFlag)
		{
			return;
		}
		float verticalDot;
		bool flag = GClass1162.IsNeedVerticalOcclusion(gunPosition, position, out verticalDot);
		if (relation.HasStairsFlag)
		{
			if (flag)
			{
				lowerFrequency = 1600f;
			}
			return;
		}
		if (relation.HasVerticalFlag || relation.HasNone)
		{
			verticalDot = GClass1162.CalculateVerticalDot(gunPosition, position);
			if (flag)
			{
				float num = ((verticalDot > 0f) ? 700f : 450f);
				lowerFrequency = num;
				return;
			}
		}
		if (relation.HasConnectedFlag)
		{
			volume = (relation.UseManualAudibilitySettings ? relation.Audibility : volume);
			lowerFrequency = 1600f;
		}
		else if (relation.HasIsolatedFlag)
		{
			volume = 0f;
			lowerFrequency = 0f;
		}
	}

	public static void VolumeDependentOcclusion(Vector3 soundPosition, ref float propagationEffect, ref float lowerFrequency, ref float volume, float maxDistance = 50f)
	{
		if (CameraClass.Instance.Camera == null || Player_0 == null)
		{
			return;
		}
		Vector3 position = CameraClass.Instance.Camera.transform.position;
		float num = CameraClass.Instance.Distance(soundPosition);
		bool isSourcesInDifferentVolume = MonoBehaviourSingleton<BetterAudio>.Instance.IsSourcesInDifferentVolume(position, soundPosition);
		bool num2 = Singleton<GClass1125>.Instance.IsPlayerAndSourceInSameVolume(soundPosition);
		BetterPropagationVolume playerCurrentPropagationVolume = Singleton<GClass1125>.Instance.GetPlayerCurrentPropagationVolume();
		if (num2 && playerCurrentPropagationVolume != null)
		{
			lowerFrequency = 1600f;
			propagationEffect = Mathf.Clamp(propagationEffect, 0f, playerCurrentPropagationVolume.SelfOcclusionIntensity);
			return;
		}
		GStruct437 relationByPosition = BetterPropagationVolume.GetRelationByPosition(position, soundPosition);
		Float_1 = relationByPosition.Audibility;
		if (relationByPosition.HasSelfFlag && !relationByPosition.HasConnectedFlag && !relationByPosition.HasStairsFlag && !relationByPosition.HasVerticalFlag && playerCurrentPropagationVolume != null)
		{
			lowerFrequency = 1600f;
			propagationEffect = Mathf.Clamp(propagationEffect, 0f, playerCurrentPropagationVolume.SelfOcclusionIntensity);
			smethod_2(relationByPosition, ref lowerFrequency, position, soundPosition, ref propagationEffect, ref volume, num, isSourcesInDifferentVolume);
		}
		else
		{
			smethod_1(relationByPosition, ref lowerFrequency, ref propagationEffect, ref volume, isSourcesInDifferentVolume, num, maxDistance);
			smethod_2(relationByPosition, ref lowerFrequency, position, soundPosition, ref propagationEffect, ref volume, num, isSourcesInDifferentVolume);
		}
	}

	public static void smethod_1(GStruct437 connection, ref float lowerFrequency, ref float occlusionEffect, ref float volume, bool isSourcesInDifferentVolume, float originalDistance, float maxDistance)
	{
		if (isSourcesInDifferentVolume)
		{
			float num = Mathf.InverseLerp(maxDistance, 0f, originalDistance);
			if (connection.HasIsolatedFlag && !connection.HasConnectedFlag && !connection.HasStairsFlag && !connection.HasVerticalFlag)
			{
				volume = 0f;
				lowerFrequency = 0f;
				occlusionEffect = 1f;
			}
			else if (connection.HasConnectedFlag && !connection.HasVerticalFlag)
			{
				lowerFrequency = 1600f;
				occlusionEffect = (connection.UseManualAudibilitySettings ? Mathf.InverseLerp(1.1f, 0f, Float_1 * num) : occlusionEffect);
				volume = (connection.UseManualAudibilitySettings ? Float_1 : volume);
			}
			else if (connection.HasNone)
			{
				lowerFrequency = 0f;
			}
		}
	}

	public static void smethod_2(GStruct437 connection, ref float lowerFrequency, Vector3 listenerPosition, Vector3 soundPosition, ref float occlusionEffect, ref float volume, float distance, bool isSourcesInDifferentVolume)
	{
		Vector3 vector = soundPosition - listenerPosition;
		float y = vector.normalized.y;
		float num = Mathf.Abs(soundPosition.y - listenerPosition.y);
		float time = Mathf.InverseLerp(20f, 0f, num * distance);
		float num2 = EFTHardSettings.Instance.StairsSoundOcclusionCurve.Evaluate(time) * EFTHardSettings.Instance.VerticalSoundFactor;
		if (connection.HasStairsFlag)
		{
			if (!(vector.y < EFTHardSettings.Instance.ABOVE_OR_BELOW_STAIRS.x))
			{
				_ = EFTHardSettings.Instance.ABOVE_OR_BELOW_STAIRS.y;
			}
			occlusionEffect = (connection.UseManualAudibilitySettings ? Mathf.InverseLerp(1.1f, 0f, Float_1 * num2) : occlusionEffect);
			volume = (connection.UseManualAudibilitySettings ? Float_1 : volume);
			lowerFrequency = 1600f;
			return;
		}
		if (vector.y < EFTHardSettings.Instance.ABOVE_OR_BELOW.x || vector.y > EFTHardSettings.Instance.ABOVE_OR_BELOW.y || y < EFTHardSettings.Instance.VERTICAL_DOT_RANGE.x || y > EFTHardSettings.Instance.VERTICAL_DOT_RANGE.y)
		{
			float num3 = ((y > 0f) ? 700f : 450f);
			if (connection.HasVerticalFlag || (connection.HasConnectedFlag && isSourcesInDifferentVolume))
			{
				occlusionEffect = (connection.UseManualAudibilitySettings ? Mathf.InverseLerp(1.1f, 0f, Float_1 * num2) : occlusionEffect);
				volume = (connection.UseManualAudibilitySettings ? Float_1 : volume);
			}
			lowerFrequency = num3;
		}
		if (!isSourcesInDifferentVolume)
		{
			lowerFrequency = 5000f;
		}
	}

	public static AudioMixerGroup VolumeDependentOcclusion(Vector3 soundPosition, ref float volume, float distance)
	{
		return VolumeDependentOcclusion(soundPosition, ref volume, distance, null, EnvironmentType.Indoor, EFTHardSettings.Instance.BaseMask);
	}

	public static AudioMixerGroup VolumeDependentOcclusion(Vector3 soundPosition, ref float volume, float distance, Player sourcePlayer, EnvironmentType sourceEnvironment)
	{
		return VolumeDependentOcclusion(soundPosition, ref volume, distance, sourcePlayer, sourceEnvironment, EFTHardSettings.Instance.BaseMask);
	}

	public static AudioMixerGroup VolumeDependentOcclusion(Vector3 soundPosition, ref float volume, float distance, IPlayer sourcePlayer, EnvironmentType sourceEnvironment, int mask)
	{
		BetterAudio instance = MonoBehaviourSingleton<BetterAudio>.Instance;
		if (CameraClass.Instance.Camera == null)
		{
			return instance.SimpleOccludedMixerGroup;
		}
		Vector3 position = CameraClass.Instance.Camera.transform.position;
		float distance2 = ((distance > 0f) ? distance : CameraClass.Instance.Distance(soundPosition));
		if (!smethod_0(soundPosition, position, distance2, mask))
		{
			return instance.VeryStandartMixerGroup;
		}
		if ((!(EnvironmentManager.Instance != null) || EnvironmentManager.Instance.Environment == EnvironmentType.Outdoor) && sourceEnvironment == EnvironmentType.Outdoor)
		{
			return instance.SimpleOccludedMixerGroup;
		}
		GStruct437 relation = BetterPropagationVolume.GetRelation(Player_0, sourcePlayer, position, soundPosition);
		if (relation.HasSelfFlag)
		{
			return instance.SimpleOccludedMixerGroup;
		}
		if (relation.HasIsolatedFlag && !relation.HasConnectedFlag && !relation.HasVerticalFlag)
		{
			volume = 0f;
			return instance.MutedGroup;
		}
		Vector3 vector = soundPosition - position;
		float y = vector.normalized.y;
		Vector3 point = RaycastHit_0.point;
		Vector3 vector2 = point;
		if (relation.HasStairsFlag)
		{
			if (!(vector.y < EFTHardSettings.Instance.ABOVE_OR_BELOW_STAIRS.x) && vector.y <= EFTHardSettings.Instance.ABOVE_OR_BELOW_STAIRS.y)
			{
				return instance.VeryStandartMixerGroup;
			}
			if (smethod_0(position, soundPosition, distance2, mask))
			{
				vector2 = RaycastHit_0.point;
			}
			float a = volume * EFTHardSettings.Instance.VOLUME_BY_FLOOR_THICKNESS.Evaluate((y > 0f) ? Mathf.Abs(point.y - vector2.y) : (0f - Mathf.Abs(point.y - vector2.y)));
			volume = Mathf.Lerp(a, volume, relation.Audibility);
			return instance.SimpleOccludedMixerGroup;
		}
		if (!(vector.y < EFTHardSettings.Instance.ABOVE_OR_BELOW.x) && !(vector.y > EFTHardSettings.Instance.ABOVE_OR_BELOW.y) && !(y < EFTHardSettings.Instance.VERTICAL_DOT_RANGE.x) && y <= EFTHardSettings.Instance.VERTICAL_DOT_RANGE.y)
		{
			return instance.SimpleOccludedMixerGroup;
		}
		if (smethod_0(position, soundPosition, distance2, mask))
		{
			vector2 = RaycastHit_0.point;
		}
		float num = volume * EFTHardSettings.Instance.VOLUME_BY_FLOOR_THICKNESS.Evaluate((y > 0f) ? Mathf.Abs(point.y - vector2.y) : (0f - Mathf.Abs(point.y - vector2.y)));
		if (relation.HasVerticalFlag)
		{
			volume = Mathf.Lerp(num, volume, relation.Audibility);
			if (!(y > 0f))
			{
				return instance.LowerOccluded;
			}
			return instance.UpperOccluded;
		}
		if (relation.HasConnectedFlag)
		{
			volume = Mathf.Lerp(num, volume, relation.Audibility);
			return instance.SimpleOccludedMixerGroup;
		}
		volume = num;
		if (!(y > 0f))
		{
			return instance.LowerOccluded;
		}
		return instance.UpperOccluded;
	}

	public static AudioMixerGroup GetGunshotsOcclusionGroup(Player player, out float volume)
	{
		volume = 1f;
		BetterAudio instance = MonoBehaviourSingleton<BetterAudio>.Instance;
		MonoBehaviourSingleton<BetterAudio>.Instance.Master.SetFloat("GunsLowPassFilter", 1200f);
		if (!(CameraClass.Instance.Camera == null) && player.PointOfView == EPointOfView.ThirdPerson)
		{
			Vector3 position = CameraClass.Instance.Camera.transform.position;
			Vector3 position2 = player.PlayerBones.WeaponRoot.position;
			float distance = CameraClass.Instance.Distance(position2);
			if (!smethod_0(position2, position, distance, EFTHardSettings.Instance.GunshotMask))
			{
				return instance.GunshotMixerGroup;
			}
			EnvironmentType environment = player.Environment;
			if (EnvironmentManager.Instance.Environment == EnvironmentType.Outdoor && environment == EnvironmentType.Outdoor)
			{
				return instance.GunshotOccludedMixerGroup;
			}
			GStruct437 relation = BetterPropagationVolume.GetRelation(Player_0, player, position, position2);
			if (relation.HasSelfFlag)
			{
				return instance.GunshotOccludedMixerGroup;
			}
			if (relation.HasStairsFlag)
			{
				if (!GClass1162.IsNeedVerticalOcclusion(position2, position, out var _))
				{
					return instance.GunshotMixerGroup;
				}
				return instance.GunshotOccludedMixerGroup;
			}
			if ((relation.HasVerticalFlag || relation.HasNone) && GClass1162.IsNeedVerticalOcclusion(position2, position, out var verticalDot2))
			{
				if (!(verticalDot2 > 0f))
				{
					return instance.LowerOccluded;
				}
				return instance.UpperOccluded;
			}
			if (relation.HasConnectedFlag)
			{
				if (relation.HasIsolatedFlag)
				{
					volume = relation.Audibility;
				}
				return instance.GunshotOccludedMixerGroup;
			}
			if (relation.HasIsolatedFlag)
			{
				volume = 0f;
				return instance.MutedGroup;
			}
			return instance.GunshotOccludedMixerGroup;
		}
		return instance.GunshotMixerGroup;
	}
}
