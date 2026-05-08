public abstract class GClass780
{
	public static bool IsHeadCollider(this EBodyPartColliderType colliderType)
	{
		if (colliderType != EBodyPartColliderType.Ears && colliderType != EBodyPartColliderType.Eyes && colliderType != EBodyPartColliderType.Jaw && colliderType != EBodyPartColliderType.BackHead && colliderType != EBodyPartColliderType.HeadCommon && colliderType != EBodyPartColliderType.NeckBack && colliderType != EBodyPartColliderType.NeckFront)
		{
			return colliderType == EBodyPartColliderType.ParietalHead;
		}
		return true;
	}
}
