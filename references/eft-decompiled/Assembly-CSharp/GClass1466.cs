using RootMotion.FinalIK;
using UnityEngine;

public class GClass1466 : IKSolverTrigonometric
{
	public override void SetBendGoalPosition(Vector3 goalPosition, float weight)
	{
		if (base.initiated)
		{
			Vector3 vector = Vector3.Cross(goalPosition - bone1.transform.position, IKPosition - bone1.transform.position);
			if (vector != Vector3.zero)
			{
				bendNormal = vector;
			}
		}
	}

	public override void OnUpdate()
	{
		if (target != null)
		{
			IKPosition = target.position;
			IKRotation = target.rotation;
		}
		OnUpdateVirtual();
		if (!DirectHierarchy)
		{
			bone1.Initiate(bone2.transform.position, bendNormal);
			bone2.Initiate(bone3.transform.position, bendNormal);
		}
		bone1.sqrMag = (bone2.transform.position - bone1.transform.position).sqrMagnitude;
		bone2.sqrMag = (bone3.transform.position - bone2.transform.position).sqrMagnitude;
		if (bendNormal == Vector3.zero && !GClass1465.logged)
		{
			LogWarning("IKSolverTrigonometric Bend Normal is Vector3.zero.");
		}
		WeightIKPosition = IKPosition;
		Vector3 vector = bendNormal;
		Vector3 vector2 = GetBendDirection(WeightIKPosition, vector);
		if (vector2 == Vector3.zero)
		{
			vector2 = bone2.transform.position - bone1.transform.position;
		}
		bone1.transform.rotation = bone1.GetRotation(vector2, vector);
		bone2.transform.rotation = bone2.GetRotation(WeightIKPosition - bone2.transform.position, bone2.GetBendNormalFromCurrentRotation());
		bone3.transform.rotation = IKRotation;
		OnPostSolveVirtual();
	}
}
