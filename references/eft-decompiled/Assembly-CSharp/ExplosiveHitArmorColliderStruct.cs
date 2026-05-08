public struct ExplosiveHitArmorColliderStruct
{
	public EBodyPartColliderType BodyPartColliderType;

	public EArmorPlateCollider ArmorPlateCollider;

	public override int GetHashCode()
	{
		return (391 + BodyPartColliderType.GetHashCode()) * 23 + ArmorPlateCollider.GetHashCode();
	}

	public ExplosiveHitArmorColliderStruct(EBodyPartColliderType bodyPartColliderType, EArmorPlateCollider armorPlateCollider)
	{
		BodyPartColliderType = bodyPartColliderType;
		ArmorPlateCollider = armorPlateCollider;
	}
}
