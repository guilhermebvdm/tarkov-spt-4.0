using UnityEngine;

namespace VisceralCombat.Ragdolls.Classes.RootMotion;

public class TriggerEventBroadcaster : MonoBehaviour
{
	public GameObject target;

	private void OnTriggerEnter(Collider collider)
	{
		if ((Object)(object)target != (Object)null)
		{
			target.SendMessage("OnTriggerEnter", (object)collider, (SendMessageOptions)1);
		}
	}

	private void OnTriggerStay(Collider collider)
	{
		if ((Object)(object)target != (Object)null)
		{
			target.SendMessage("OnTriggerStay", (object)collider, (SendMessageOptions)1);
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		if ((Object)(object)target != (Object)null)
		{
			target.SendMessage("OnTriggerExit", (object)collider, (SendMessageOptions)1);
		}
	}
}
