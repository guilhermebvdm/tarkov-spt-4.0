using UnityEngine;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

public interface ICollisionEventListener
{
	void OnCollisionEnterEvent(Collision collision, CollisionEventBroadcaster broadcaster);

	void OnCollisionStayEvent(Collision collision, CollisionEventBroadcaster broadcaster);

	void OnCollisionExitEvent(Collision collision, CollisionEventBroadcaster broadcaster);
}
