using UnityEngine;
using Object = UnityEngine.Object;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

public class JointBreakBroadcaster : MonoBehaviour
{
	[HideInInspector]
	public PuppetMaster puppetMaster;

	[HideInInspector]
	public int muscleIndex;

	private void OnJointBreak()
	{
		if (((Behaviour)this).enabled)
		{
			puppetMaster.RemoveMuscleRecursive(puppetMaster.muscles[muscleIndex].joint, attachTarget: true, blockTargetAnimation: true, MuscleRemoveMode.Numb);
		}
	}
}
