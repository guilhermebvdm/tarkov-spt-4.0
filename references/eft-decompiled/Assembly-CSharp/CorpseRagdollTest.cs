using System.Collections.Generic;
using EFT.AssetsManager;
using RootMotion.FinalIK;
using UnityEngine;

public class CorpseRagdollTest : MonoBehaviour
{
	public RagdollClass Ragdoll;

	public void Init()
	{
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = true;
		}
		IK[] componentsInChildren2 = GetComponentsInChildren<IK>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].enabled = false;
		}
		int layer = LayerMask.NameToLayer("Deadbody");
		TransformHelperClass.SetLayersRecursively(base.gameObject, layer);
	}

	public void Drop(Vector3 velocity, float maxDepenetrationVelocity, CollisionDetectionMode collisionDetectionMode, bool keepRigidbody, bool putToSleep)
	{
		CharacterJointSpawner[] componentsInChildren = base.gameObject.GetComponentsInChildren<CharacterJointSpawner>();
		RigidbodySpawner[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<RigidbodySpawner>();
		List<PlayerRigidbodySleepHierarchy> rigidbodySleepHierarchy = PlayerPoolObject.CreatePlayerRigidbodySleepHierarchy(componentsInChildren2);
		Ragdoll = new RagdollClass(componentsInChildren2, componentsInChildren, rigidbodySleepHierarchy, velocity, maxDepenetrationVelocity, collisionDetectionMode, this, method_0, null, null, method_1, keepRigidbody, putToSleep);
	}

	public bool method_0(bool sleeping, float timePass)
	{
		if (!sleeping)
		{
			return timePass >= 15f;
		}
		return true;
	}

	public void method_1()
	{
		base.gameObject.name += " (still)";
	}
}
