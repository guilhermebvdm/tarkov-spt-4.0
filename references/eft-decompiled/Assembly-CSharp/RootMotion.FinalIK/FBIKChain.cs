using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class FBIKChain
{
	[Serializable]
	public class ChildConstraint
	{
		public float pushElasticity;

		public float pullElasticity;

		[SerializeField]
		public Transform bone1;

		[SerializeField]
		public Transform bone2;

		[NonSerialized]
		public float CrossFade;

		[NonSerialized]
		public float InverseCrossFade;

		[NonSerialized]
		public int Chain1Index;

		[NonSerialized]
		public int Chain2Index;

		[field: NonSerialized]
		public float nominalDistance { get; set; }

		[field: NonSerialized]
		public bool isRigid { get; set; }

		public ChildConstraint(Transform bone1, Transform bone2, float pushElasticity = 0f, float pullElasticity = 0f)
		{
			this.bone1 = bone1;
			this.bone2 = bone2;
			this.pushElasticity = pushElasticity;
			this.pullElasticity = pullElasticity;
		}

		public void Initiate(IKSolverFullBody solver)
		{
			Chain1Index = solver.GetChainIndex(bone1);
			Chain2Index = solver.GetChainIndex(bone2);
			OnPreSolve(solver);
		}

		public void OnPreSolve(IKSolverFullBody solver)
		{
			nominalDistance = Vector3.Distance(solver.chain[Chain1Index].nodes[0].transform.position, solver.chain[Chain2Index].nodes[0].transform.position);
			isRigid = pushElasticity <= 0f && pullElasticity <= 0f;
			if (isRigid)
			{
				float num = solver.chain[Chain1Index].pull - solver.chain[Chain2Index].pull;
				CrossFade = 1f - (0.5f + num * 0.5f);
			}
			else
			{
				CrossFade = 0.5f;
			}
			InverseCrossFade = 1f - CrossFade;
		}

		public void Solve(IKSolverFullBody solver)
		{
			if (pushElasticity >= 1f && pullElasticity >= 1f)
			{
				return;
			}
			Vector3 vector = solver.chain[Chain2Index].nodes[0].solverPosition - solver.chain[Chain1Index].nodes[0].solverPosition;
			float magnitude = vector.magnitude;
			if (magnitude != nominalDistance && magnitude != 0f)
			{
				float num = 1f;
				if (!isRigid)
				{
					float num2 = ((magnitude > nominalDistance) ? pullElasticity : pushElasticity);
					num = 1f - num2;
				}
				num *= 1f - nominalDistance / magnitude;
				Vector3 vector2 = vector * num;
				solver.chain[Chain1Index].nodes[0].solverPosition += vector2 * CrossFade;
				solver.chain[Chain2Index].nodes[0].solverPosition -= vector2 * InverseCrossFade;
			}
		}
	}

	[Serializable]
	public enum Smoothing
	{
		None,
		Exponential,
		Cubic
	}

	[Range(0f, 1f)]
	public float pin;

	[Range(0f, 1f)]
	public float pull = 1f;

	[Range(0f, 1f)]
	public float push;

	[Range(-1f, 1f)]
	public float pushParent;

	[Range(0f, 1f)]
	public float reach = 0.1f;

	public Smoothing reachSmoothing = Smoothing.Exponential;

	public Smoothing pushSmoothing = Smoothing.Exponential;

	public IKSolver.Node[] nodes = new IKSolver.Node[0];

	public int[] children = new int[0];

	public ChildConstraint[] childConstraints = new ChildConstraint[0];

	public IKConstraintBend bendConstraint = new IKConstraintBend();

	[NonSerialized]
	public float RootLength;

	[NonSerialized]
	public bool Initiated;

	[NonSerialized]
	public float Length;

	[NonSerialized]
	public float Distance;

	[NonSerialized]
	public IKSolver.Point p;

	[NonSerialized]
	public float ReachForce;

	[NonSerialized]
	public float PullParentSum;

	[NonSerialized]
	public float[] CrossFades;

	[NonSerialized]
	public float SqrMag1;

	[NonSerialized]
	public float SqrMag2;

	[NonSerialized]
	public float SqrMagDif;

	[NonSerialized]
	public const float MaxLimbLength = 0.99999f;

	public FBIKChain()
	{
	}

	public FBIKChain(float pin, float pull, params Transform[] nodeTransforms)
	{
		this.pin = pin;
		this.pull = pull;
		SetNodes(nodeTransforms);
		children = new int[0];
	}

	public void SetNodes(params Transform[] boneTransforms)
	{
		nodes = new IKSolver.Node[boneTransforms.Length];
		for (int i = 0; i < boneTransforms.Length; i++)
		{
			nodes[i] = new IKSolver.Node(boneTransforms[i]);
		}
	}

	public int GetNodeIndex(Transform boneTransform)
	{
		int num = 0;
		while (true)
		{
			if (num < nodes.Length)
			{
				if (nodes[num].transform == boneTransform)
				{
					break;
				}
				num++;
				continue;
			}
			return -1;
		}
		return num;
	}

	public bool IsValid(ref string message)
	{
		if (nodes.Length == 0)
		{
			message = "FBIK chain contains no nodes.";
			return false;
		}
		IKSolver.Node[] array = nodes;
		int num = 0;
		while (true)
		{
			if (num < array.Length)
			{
				if (array[num].transform == null)
				{
					break;
				}
				num++;
				continue;
			}
			return true;
		}
		message = "Node transform is null in FBIK chain.";
		return false;
	}

	public void Initiate(IKSolverFullBody solver)
	{
		Initiated = false;
		IKSolver.Node[] array = nodes;
		foreach (IKSolver.Node obj in array)
		{
			obj.solverPosition = obj.transform.position;
		}
		method_0(solver);
		ChildConstraint[] array2 = childConstraints;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].Initiate(solver);
		}
		if (nodes.Length == 3)
		{
			bendConstraint.SetBones(nodes[0].transform, nodes[1].transform, nodes[2].transform);
			bendConstraint.Initiate(solver);
		}
		CrossFades = new float[children.Length];
		Initiated = true;
	}

	public void ReadPose(IKSolverFullBody solver, bool fullBody)
	{
		if (!Initiated)
		{
			return;
		}
		for (int i = 0; i < nodes.Length; i++)
		{
			nodes[i].solverPosition = nodes[i].transform.position + nodes[i].offset;
		}
		method_0(solver);
		if (!fullBody)
		{
			return;
		}
		for (int j = 0; j < childConstraints.Length; j++)
		{
			childConstraints[j].OnPreSolve(solver);
		}
		if (children.Length != 0)
		{
			float num = nodes[nodes.Length - 1].effectorPositionWeight;
			for (int k = 0; k < children.Length; k++)
			{
				num += solver.chain[children[k]].nodes[0].effectorPositionWeight * solver.chain[children[k]].pull;
			}
			num = Mathf.Clamp(num, 1f, float.PositiveInfinity);
			for (int l = 0; l < children.Length; l++)
			{
				CrossFades[l] = solver.chain[children[l]].nodes[0].effectorPositionWeight * solver.chain[children[l]].pull / num;
			}
		}
		PullParentSum = 0f;
		for (int m = 0; m < children.Length; m++)
		{
			PullParentSum += solver.chain[children[m]].pull;
		}
		PullParentSum = Mathf.Clamp(PullParentSum, 1f, float.PositiveInfinity);
		if (nodes.Length == 3)
		{
			ReachForce = reach * Mathf.Clamp(nodes[2].effectorPositionWeight, 0f, 1f);
		}
		else
		{
			ReachForce = 0f;
		}
		if (push > 0f && nodes.Length > 1)
		{
			Distance = Vector3.Distance(nodes[0].transform.position, nodes[nodes.Length - 1].transform.position);
		}
	}

	public void method_0(IKSolverFullBody solver)
	{
		Length = 0f;
		int num = 0;
		while (true)
		{
			if (num < nodes.Length - 1)
			{
				nodes[num].length = Vector3.Distance(nodes[num].transform.position, nodes[num + 1].transform.position);
				Length += nodes[num].length;
				if (nodes[num].length == 0f)
				{
					break;
				}
				num++;
				continue;
			}
			for (int i = 0; i < children.Length; i++)
			{
				solver.chain[children[i]].RootLength = (solver.chain[children[i]].nodes[0].transform.position - nodes[nodes.Length - 1].transform.position).magnitude;
				if (solver.chain[children[i]].RootLength == 0f)
				{
					return;
				}
			}
			if (nodes.Length == 3)
			{
				SqrMag1 = nodes[0].length * nodes[0].length;
				SqrMag2 = nodes[1].length * nodes[1].length;
				SqrMagDif = SqrMag1 - SqrMag2;
			}
			return;
		}
		GClass1465.Log("Bone " + nodes[num].transform.name + " - " + nodes[num + 1].transform.name + " length is zero, can not solve.", nodes[num].transform);
	}

	public void Reach(IKSolverFullBody solver)
	{
		if (!Initiated)
		{
			return;
		}
		for (int i = 0; i < children.Length; i++)
		{
			solver.chain[children[i]].Reach(solver);
		}
		if (ReachForce <= 0f)
		{
			return;
		}
		Vector3 vector = nodes[2].solverPosition - nodes[0].solverPosition;
		if (!(vector == Vector3.zero))
		{
			float magnitude = vector.magnitude;
			Vector3 vector2 = vector / magnitude * Length;
			float num = Mathf.Clamp(magnitude / Length, 1f - ReachForce, 1f + ReachForce) - 1f;
			num = Mathf.Clamp(num + ReachForce, -1f, 1f);
			switch (reachSmoothing)
			{
			case Smoothing.Cubic:
				num *= num * num;
				break;
			case Smoothing.Exponential:
				num *= num;
				break;
			}
			Vector3 vector3 = vector2 * Mathf.Clamp(num, 0f, magnitude);
			nodes[0].solverPosition += vector3 * (1f - nodes[0].effectorPositionWeight);
			nodes[2].solverPosition += vector3;
		}
	}

	public Vector3 Push(IKSolverFullBody solver)
	{
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < children.Length; i++)
		{
			zero += solver.chain[children[i]].Push(solver) * solver.chain[children[i]].pushParent;
		}
		nodes[nodes.Length - 1].solverPosition += zero;
		if (nodes.Length < 2)
		{
			return Vector3.zero;
		}
		if (push <= 0f)
		{
			return Vector3.zero;
		}
		Vector3 vector = nodes[2].solverPosition - nodes[0].solverPosition;
		float magnitude = vector.magnitude;
		if (magnitude == 0f)
		{
			return Vector3.zero;
		}
		float num = 1f - magnitude / Distance;
		if (num <= 0f)
		{
			return Vector3.zero;
		}
		switch (pushSmoothing)
		{
		case Smoothing.Cubic:
			num *= num * num;
			break;
		case Smoothing.Exponential:
			num *= num;
			break;
		}
		Vector3 vector2 = -vector * num * push;
		nodes[0].solverPosition += vector2;
		return vector2;
	}

	public void SolveTrigonometric(IKSolverFullBody solver, bool calculateBendDirection = false)
	{
		if (!Initiated)
		{
			return;
		}
		for (int i = 0; i < children.Length; i++)
		{
			solver.chain[children[i]].SolveTrigonometric(solver, calculateBendDirection);
		}
		if (nodes.Length == 3)
		{
			Vector3 vector = nodes[2].solverPosition - nodes[0].solverPosition;
			float magnitude = vector.magnitude;
			if (magnitude != 0f)
			{
				float num = Mathf.Clamp(magnitude, 0f, Length * 0.99999f);
				Vector3 direction = vector / magnitude * num;
				Vector3 bendDirection = ((!calculateBendDirection || !bendConstraint.initiated) ? (nodes[1].solverPosition - nodes[0].solverPosition) : bendConstraint.GetDir(solver));
				Vector3 dirToBendPoint = GetDirToBendPoint(direction, bendDirection, num);
				nodes[1].solverPosition = nodes[0].solverPosition + dirToBendPoint;
			}
		}
	}

	public void Stage1(IKSolverFullBody solver)
	{
		for (int i = 0; i < children.Length; i++)
		{
			solver.chain[children[i]].Stage1(solver);
		}
		if (children.Length == 0)
		{
			ForwardReach(nodes[nodes.Length - 1].solverPosition);
			return;
		}
		Vector3 solverPosition = nodes[nodes.Length - 1].solverPosition;
		method_2(solver);
		for (int j = 0; j < children.Length; j++)
		{
			Vector3 vector = solver.chain[children[j]].nodes[0].solverPosition;
			if (solver.chain[children[j]].RootLength > 0f)
			{
				vector = method_1(nodes[nodes.Length - 1].solverPosition, solver.chain[children[j]].nodes[0].solverPosition, solver.chain[children[j]].RootLength);
			}
			if (PullParentSum > 0f)
			{
				solverPosition += (vector - nodes[nodes.Length - 1].solverPosition) * (solver.chain[children[j]].pull / PullParentSum);
			}
		}
		ForwardReach(Vector3.Lerp(solverPosition, nodes[nodes.Length - 1].solverPosition, pin));
	}

	public void Stage2(IKSolverFullBody solver, Vector3 position)
	{
		method_4(position);
		int num = Mathf.Clamp(solver.iterations, 2, 4);
		if (childConstraints.Length != 0)
		{
			for (int i = 0; i < num; i++)
			{
				SolveConstraintSystems(solver);
			}
		}
		for (int j = 0; j < children.Length; j++)
		{
			solver.chain[children[j]].Stage2(solver, nodes[nodes.Length - 1].solverPosition);
		}
	}

	public void SolveConstraintSystems(IKSolverFullBody solver)
	{
		method_2(solver);
		for (int i = 0; i < children.Length; i++)
		{
			method_3(nodes[nodes.Length - 1], solver.chain[children[i]].nodes[0], CrossFades[i], solver.chain[children[i]].RootLength);
		}
	}

	public Vector3 method_1(Vector3 pos1, Vector3 pos2, float length)
	{
		return pos2 + (pos1 - pos2).normalized * length;
	}

	public Vector3 GetDirToBendPoint(Vector3 direction, Vector3 bendDirection, float directionMagnitude)
	{
		float num = (directionMagnitude * directionMagnitude + SqrMagDif) / 2f / directionMagnitude;
		float y = (float)Math.Sqrt(Mathf.Clamp(SqrMag1 - num * num, 0f, float.PositiveInfinity));
		if (direction == Vector3.zero)
		{
			return Vector3.zero;
		}
		return Quaternion.LookRotation(direction, bendDirection) * new Vector3(0f, y, num);
	}

	public void method_2(IKSolverFullBody solver)
	{
		for (int i = 0; i < childConstraints.Length; i++)
		{
			childConstraints[i].Solve(solver);
		}
	}

	public void method_3(IKSolver.Node node1, IKSolver.Node node2, float crossFade, float distance)
	{
		Vector3 vector = node2.solverPosition - node1.solverPosition;
		float magnitude = vector.magnitude;
		if (distance != magnitude && magnitude != 0f)
		{
			Vector3 vector2 = vector * (1f - distance / magnitude);
			node1.solverPosition += vector2 * crossFade;
			node2.solverPosition -= vector2 * (1f - crossFade);
		}
	}

	public void ForwardReach(Vector3 position)
	{
		nodes[nodes.Length - 1].solverPosition = position;
		for (int num = nodes.Length - 2; num > -1; num--)
		{
			nodes[num].solverPosition = method_1(nodes[num].solverPosition, nodes[num + 1].solverPosition, nodes[num].length);
		}
	}

	public void method_4(Vector3 position)
	{
		if (RootLength > 0f)
		{
			position = method_1(nodes[0].solverPosition, position, RootLength);
		}
		nodes[0].solverPosition = position;
		for (int i = 1; i < nodes.Length; i++)
		{
			nodes[i].solverPosition = method_1(nodes[i].solverPosition, nodes[i - 1].solverPosition, nodes[i - 1].length);
		}
	}
}
