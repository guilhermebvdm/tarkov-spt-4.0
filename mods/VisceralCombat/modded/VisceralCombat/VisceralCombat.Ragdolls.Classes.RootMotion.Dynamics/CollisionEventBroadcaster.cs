using UnityEngine;
using Object = UnityEngine.Object;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

public class CollisionEventBroadcaster : MonoBehaviour
{
	public ICollisionEventListener listener;

	public MuscleLite muscle;

	private void OnCollisionEnter(Collision collision)
	{
		if (listener != null)
		{
			listener.OnCollisionEnterEvent(collision, this);
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if (listener != null)
		{
			listener.OnCollisionStayEvent(collision, this);
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		if (listener != null)
		{
			listener.OnCollisionExitEvent(collision, this);
		}
	}
}
