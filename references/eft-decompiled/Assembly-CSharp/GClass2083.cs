using Comfort.Common;
using EFT;
using EFT.Ballistics;
using EFT.NetworkPackets;

public abstract class GClass2083
{
	public static void AddHit(this ref GStruct380 knifePacket, Player.GStruct182 hit, BallisticCollider ballisticCollider, ShotInfoClass playerHit, Player.EKickType hitType)
	{
		GStruct379 item = new GStruct379
		{
			IsStab = (hitType == Player.EKickType.Stab),
			HitType = EHitType.Default
		};
		if (ballisticCollider == null)
		{
			item.HittedId = -1;
		}
		else if (ballisticCollider is BodyPartCollider bodyPartCollider)
		{
			if (bodyPartCollider is ArmorPlateCollider armorPlateCollider)
			{
				item.ArmorPlateCollider = armorPlateCollider.ArmorPlateColliderType;
			}
			item.EColliderType = bodyPartCollider.BodyPartColliderType;
			item.EBodyPart = bodyPartCollider.BodyPartType;
			item.HittedId = bodyPartCollider.playerBridge.iPlayer.Id;
			item.ServerFrame = (ulong)Singleton<AbstractGame>.Instance.LastServerFrameId;
			item.TimeStamp = Singleton<AbstractGame>.Instance.LastServerTimeStamp;
		}
		else
		{
			switch (ballisticCollider.HitType)
			{
			default:
				item.HittedId = -1;
				break;
			case EHitType.Lamp:
				item.HitType = EHitType.Lamp;
				item.HittedId = ballisticCollider.NetId;
				break;
			case EHitType.Window:
				item.HitType = EHitType.Window;
				item.HittedId = ballisticCollider.NetId;
				break;
			case EHitType.Btr:
				item.HitType = EHitType.Btr;
				item.HittedId = ballisticCollider.NetId;
				break;
			case EHitType.Tripwire:
				item.HitType = EHitType.Tripwire;
				item.HittedId = ballisticCollider.NetId;
				break;
			case EHitType.Event:
				item.HitType = EHitType.Event;
				item.HittedId = ballisticCollider.NetId;
				break;
			}
		}
		item.HitPosition = hit.point;
		knifePacket.HitsForApproval.Add(item);
	}
}
