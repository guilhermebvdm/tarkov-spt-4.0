using UnityEngine;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

public struct MuscleHit
{
	public int muscleIndex;

	public float unPin;

	public Vector3 force;

	public Vector3 position;

	public MuscleHit(int muscleIndex, float unPin, Vector3 force, Vector3 position)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		this.muscleIndex = muscleIndex;
		this.unPin = unPin;
		this.force = force;
		this.position = position;
	}
}
