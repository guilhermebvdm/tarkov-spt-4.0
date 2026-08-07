using UnityEngine;
using Object = UnityEngine.Object;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

public class AnimationBlocker : MonoBehaviour
{
	private void LateUpdate()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localPosition = Vector3.zero;
		((Component)this).transform.localRotation = Quaternion.identity;
	}
}
