using UnityEngine;

public class MirrorTransformChanges : MonoBehaviour
{
	public Transform RoleModel;

	public EFTPhysicsClass.ColliderSync[] colliderSyncInPhysics;

	public void LateUpdate()
	{
		if (!(RoleModel != null) || !RoleModel.hasChanged)
		{
			return;
		}
		base.transform.SetPositionAndRotation(RoleModel.position, RoleModel.rotation);
		base.transform.localScale = RoleModel.localScale;
		if (colliderSyncInPhysics != null && colliderSyncInPhysics.Length != 0)
		{
			EFTPhysicsClass.ColliderSync[] array = colliderSyncInPhysics;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Sync();
			}
		}
		RoleModel.hasChanged = false;
	}
}
