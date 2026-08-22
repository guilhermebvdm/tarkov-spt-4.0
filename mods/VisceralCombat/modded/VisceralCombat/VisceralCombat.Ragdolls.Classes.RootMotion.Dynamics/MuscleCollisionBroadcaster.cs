using UnityEngine;
using Object = UnityEngine.Object;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

[AddComponentMenu("Scripts/RootMotion.Dynamics/PuppetMaster/Muscle Collision Broadcaster")]
public class MuscleCollisionBroadcaster : MonoBehaviour
{
	[HideInInspector]
	public PuppetMaster puppetMaster;

	[HideInInspector]
	public int muscleIndex;

	private const string onMuscleHit = "OnMuscleHit";

	private const string onMuscleCollision = "OnMuscleCollision";

	private const string onMuscleCollisionExit = "OnMuscleCollisionExit";

	private MuscleCollisionBroadcaster otherBroadcaster;

	public void Hit(float unPin, Vector3 force, Vector3 position)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (((Behaviour)this).enabled)
		{
			BehaviourBase[] behaviours = puppetMaster.behaviours;
			foreach (BehaviourBase behaviourBase in behaviours)
			{
				behaviourBase.OnMuscleHit(new MuscleHit(muscleIndex, unPin, force, position));
			}
		}
	}

	private bool IsSelf(Collider c)
	{
		return ((Component)c).transform.IsChildOf(((Component)puppetMaster).transform);
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (((Behaviour)this).enabled && !((Object)(object)puppetMaster == (Object)null) && !IsSelf(collision.collider) && !puppetMaster.muscles[muscleIndex].state.isDisconnected)
		{
			BehaviourBase[] behaviours = puppetMaster.behaviours;
			foreach (BehaviourBase behaviourBase in behaviours)
			{
				behaviourBase.OnMuscleCollision(new MuscleCollision(muscleIndex, collision));
			}
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if (((Behaviour)this).enabled && !((Object)(object)puppetMaster == (Object)null) && (!((Object)(object)Singleton<PuppetMasterSettings>.instance != (Object)null) || Singleton<PuppetMasterSettings>.instance.collisionStayMessages) && !IsSelf(collision.collider) && !puppetMaster.muscles[muscleIndex].state.isDisconnected)
		{
			BehaviourBase[] behaviours = puppetMaster.behaviours;
			foreach (BehaviourBase behaviourBase in behaviours)
			{
				behaviourBase.OnMuscleCollision(new MuscleCollision(muscleIndex, collision, isStay: true));
			}
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		if (((Behaviour)this).enabled && !((Object)(object)puppetMaster == (Object)null) && (!((Object)(object)Singleton<PuppetMasterSettings>.instance != (Object)null) || Singleton<PuppetMasterSettings>.instance.collisionExitMessages) && !IsSelf(collision.collider) && !puppetMaster.muscles[muscleIndex].state.isDisconnected)
		{
			BehaviourBase[] behaviours = puppetMaster.behaviours;
			foreach (BehaviourBase behaviourBase in behaviours)
			{
				behaviourBase.OnMuscleCollisionExit(new MuscleCollision(muscleIndex, collision));
			}
		}
	}
}
