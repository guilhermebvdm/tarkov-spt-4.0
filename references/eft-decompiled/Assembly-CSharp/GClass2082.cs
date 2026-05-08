using System.Collections.Generic;
using Comfort.Common;
using EFT;
using EFT.Ballistics;
using EFT.NetworkPackets;
using UnityEngine;

public abstract class GClass2082
{
	public static GStruct385[] smethod_0(EftBulletClass shot)
	{
		List<GStruct385> list = new List<GStruct385>();
		do
		{
			bool hitObstacleAtTheEnd = shot.HittedBallisticCollider != null && !(shot.HittedBallisticCollider is BodyPartCollider);
			GStruct385 item = new GStruct385
			{
				StartVelocity = shot.StartVelocity,
				StartPosition = shot.StartPosition,
				BulletMassGram = shot.BulletMassGram,
				BulletDiameterMilimeters = shot.BulletDiameterMilimeters,
				BallisticCoefficient = shot.BallisticCoefficient,
				Duration = shot.TimeSinceShot,
				HitObstacleAtTheEnd = hitObstacleAtTheEnd
			};
			list.Add(item);
		}
		while ((shot = shot.Parent) != null && list.Count <= 64);
		if (list.Count > 64)
		{
			Debug.LogError("too many shotTrajectoryParts");
		}
		GStruct385[] array = new GStruct385[list.Count];
		for (int i = 0; i < list.Count; i++)
		{
			array[list.Count - i - 1] = list[i];
		}
		return array;
	}

	public static GStruct444 GetShotInfo(this EftBulletClass shot)
	{
		BallisticCollider hittedBallisticCollider = shot.HittedBallisticCollider;
		BodyPartCollider bodyPartCollider = hittedBallisticCollider as BodyPartCollider;
		ArmorPlateCollider armorPlateCollider = hittedBallisticCollider as ArmorPlateCollider;
		EHitType hitType = EHitType.Default;
		int num = -1;
		if (bodyPartCollider == null && hittedBallisticCollider != null)
		{
			switch (hittedBallisticCollider.HitType)
			{
			case EHitType.Lamp:
				hitType = EHitType.Lamp;
				num = hittedBallisticCollider.NetId;
				break;
			case EHitType.Window:
				hitType = EHitType.Window;
				num = hittedBallisticCollider.NetId;
				break;
			case EHitType.Btr:
				hitType = EHitType.Btr;
				num = hittedBallisticCollider.NetId;
				break;
			case EHitType.Tripwire:
				hitType = EHitType.Tripwire;
				num = hittedBallisticCollider.NetId;
				break;
			case EHitType.Event:
				hitType = EHitType.Event;
				num = hittedBallisticCollider.NetId;
				break;
			}
		}
		GStruct444 result = new GStruct444
		{
			AmmoId = ((shot.Ammo != null) ? shot.Ammo.Id.GetHashCode() : 0),
			StaminaBurnRate = ((shot.Ammo is AmmoItemClass ammoItemClass) ? ammoItemClass.StaminaBurnRate : 0f),
			FragmentId = (byte)shot.FragmentIndex,
			HitType = hitType,
			HittedId = ((bodyPartCollider == null || bodyPartCollider.Player == null) ? num : bodyPartCollider.Player.Id),
			BodyPart = ((bodyPartCollider == null) ? EBodyPart.Common : bodyPartCollider.BodyPartType),
			BodyPartColliderType = ((bodyPartCollider != null) ? bodyPartCollider.BodyPartColliderType : EBodyPartColliderType.None),
			ArmorPlateCollider = ((armorPlateCollider != null) ? armorPlateCollider.ArmorPlateColliderType : ((EArmorPlateCollider)0)),
			HitPosition = shot.HitPoint,
			Damage = (shot.AvoidAdditionalDamage ? 0f : shot.Damage),
			PenetrationPower = shot.PenetrationPower,
			ArmorDamage = shot.ArmorDamage,
			ShotTrajectoryParts = smethod_0(shot),
			HitCos = shot.HitCosDirectionToNormal,
			Seed = shot.RandomSeed,
			FireIndex = shot.FireIndex
		};
		MongoID? hitArmorItemID = shot.HitArmorItemID;
		result.NullableHitArmorID = (hitArmorItemID.HasValue ? ((string)hitArmorItemID.GetValueOrDefault()) : null);
		result.ServerFrame = (ulong)Singleton<AbstractGame>.Instance.LastServerFrameId;
		result.TimeStamp = ((bodyPartCollider == null || bodyPartCollider.playerBridge == null) ? Singleton<AbstractGame>.Instance.LastServerTimeStamp : bodyPartCollider.playerBridge.WorldTime);
		return result;
	}
}
