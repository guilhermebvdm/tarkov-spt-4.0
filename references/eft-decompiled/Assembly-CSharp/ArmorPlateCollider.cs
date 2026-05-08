public class ArmorPlateCollider : BodyPartCollider
{
	public EArmorPlateCollider ArmorPlateColliderType;

	public override void InitColliderSettings()
	{
	}

	public override ShotInfoClass ApplyHit(DamageInfoStruct damageInfo, ShotIdStruct shotID)
	{
		if (playerBridge != null && damageInfo.IsForwardHit)
		{
			return playerBridge.ApplyShot(damageInfo, BodyPartType, BodyPartColliderType, ArmorPlateColliderType, shotID);
		}
		return null;
	}
}
