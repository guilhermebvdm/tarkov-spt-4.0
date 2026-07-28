using UnityEngine;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

public class FixFootColliders : MonoBehaviour
{
	public Transform root;

	public Transform leftFoot;

	public Transform rightFoot;

	[ContextMenu("Fix")]
	public void Fix()
	{
		Collider val = BipedRagdollCreator.FixFootCollider(leftFoot, root);
		Collider val2 = BipedRagdollCreator.FixFootCollider(rightFoot, root);
	}
}
