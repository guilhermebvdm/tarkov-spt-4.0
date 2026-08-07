using UnityEngine;
using Object = UnityEngine.Object;

namespace VisceralCombat.Ragdolls.Classes.RootMotion;

public class TQ
{
	public Vector3 t;

	public Quaternion q;

	public TQ()
	{
	}

	public TQ(Vector3 translation, Quaternion rotation)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		t = translation;
		q = rotation;
	}
}
