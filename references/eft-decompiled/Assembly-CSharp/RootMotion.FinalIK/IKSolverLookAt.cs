using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class IKSolverLookAt : IKSolver
{
	[Serializable]
	public class LookAtBone : Bone
	{
		public Vector3 forward => transform.rotation * axis;

		public LookAtBone()
		{
		}

		public LookAtBone(Transform transform)
		{
			base.transform = transform;
		}

		public void Initiate(Transform root)
		{
			if (!(transform == null))
			{
				axis = Quaternion.Inverse(transform.rotation) * root.forward;
			}
		}

		public void LookAt(Vector3 direction, float weight)
		{
			Quaternion quaternion = Quaternion.FromToRotation(forward, direction);
			Quaternion rotation = transform.rotation;
			transform.rotation = Quaternion.Lerp(rotation, quaternion * rotation, weight);
		}
	}

	public Transform target;

	public LookAtBone[] spine = new LookAtBone[0];

	public LookAtBone head = new LookAtBone();

	public LookAtBone[] eyes = new LookAtBone[0];

	[Range(0f, 1f)]
	public float bodyWeight = 0.5f;

	[Range(0f, 1f)]
	public float headWeight = 0.5f;

	[Range(0f, 1f)]
	public float eyesWeight = 1f;

	[Range(0f, 1f)]
	public float clampWeight = 0.5f;

	[Range(0f, 1f)]
	public float clampWeightHead = 0.5f;

	[Range(0f, 1f)]
	public float clampWeightEyes = 0.5f;

	[Range(0f, 2f)]
	public int clampSmoothing = 2;

	public AnimationCurve spineWeightCurve = new AnimationCurve(new Keyframe(0f, 0.3f), new Keyframe(1f, 1f));

	[NonSerialized]
	public Vector3[] SpineForwards = new Vector3[0];

	[NonSerialized]
	public Vector3[] HeadForwards = new Vector3[1];

	[NonSerialized]
	public Vector3[] EyeForward = new Vector3[1];

	public bool Boolean_0
	{
		get
		{
			if (spine == null)
			{
				return false;
			}
			if (spine.Length == 0)
			{
				return true;
			}
			int num = 0;
			while (true)
			{
				if (num < spine.Length)
				{
					if (spine[num] == null || spine[num].transform == null)
					{
						break;
					}
					num++;
					continue;
				}
				return true;
			}
			return false;
		}
	}

	public bool Boolean_1 => spine.Length == 0;

	public bool Boolean_2
	{
		get
		{
			if (head == null)
			{
				return false;
			}
			return true;
		}
	}

	public bool Boolean_3 => head.transform == null;

	public bool Boolean_4
	{
		get
		{
			if (eyes == null)
			{
				return false;
			}
			if (eyes.Length == 0)
			{
				return true;
			}
			int num = 0;
			while (true)
			{
				if (num < eyes.Length)
				{
					if (eyes[num] == null || eyes[num].transform == null)
					{
						break;
					}
					num++;
					continue;
				}
				return true;
			}
			return false;
		}
	}

	public bool Boolean_5 => eyes.Length == 0;

	public void SetLookAtWeight(float weight)
	{
		IKPositionWeight = Mathf.Clamp(weight, 0f, 1f);
	}

	public void SetLookAtWeight(float weight, float bodyWeight)
	{
		IKPositionWeight = Mathf.Clamp(weight, 0f, 1f);
		this.bodyWeight = Mathf.Clamp(bodyWeight, 0f, 1f);
	}

	public void SetLookAtWeight(float weight, float bodyWeight, float headWeight)
	{
		IKPositionWeight = Mathf.Clamp(weight, 0f, 1f);
		this.bodyWeight = Mathf.Clamp(bodyWeight, 0f, 1f);
		this.headWeight = Mathf.Clamp(headWeight, 0f, 1f);
	}

	public void SetLookAtWeight(float weight, float bodyWeight, float headWeight, float eyesWeight)
	{
		IKPositionWeight = Mathf.Clamp(weight, 0f, 1f);
		this.bodyWeight = Mathf.Clamp(bodyWeight, 0f, 1f);
		this.headWeight = Mathf.Clamp(headWeight, 0f, 1f);
		this.eyesWeight = Mathf.Clamp(eyesWeight, 0f, 1f);
	}

	public void SetLookAtWeight(float weight, float bodyWeight, float headWeight, float eyesWeight, float clampWeight)
	{
		IKPositionWeight = Mathf.Clamp(weight, 0f, 1f);
		this.bodyWeight = Mathf.Clamp(bodyWeight, 0f, 1f);
		this.headWeight = Mathf.Clamp(headWeight, 0f, 1f);
		this.eyesWeight = Mathf.Clamp(eyesWeight, 0f, 1f);
		this.clampWeight = Mathf.Clamp(clampWeight, 0f, 1f);
		clampWeightHead = this.clampWeight;
		clampWeightEyes = this.clampWeight;
	}

	public void SetLookAtWeight(float weight, float bodyWeight = 0f, float headWeight = 1f, float eyesWeight = 0.5f, float clampWeight = 0.5f, float clampWeightHead = 0.5f, float clampWeightEyes = 0.3f)
	{
		IKPositionWeight = Mathf.Clamp(weight, 0f, 1f);
		this.bodyWeight = Mathf.Clamp(bodyWeight, 0f, 1f);
		this.headWeight = Mathf.Clamp(headWeight, 0f, 1f);
		this.eyesWeight = Mathf.Clamp(eyesWeight, 0f, 1f);
		this.clampWeight = Mathf.Clamp(clampWeight, 0f, 1f);
		this.clampWeightHead = Mathf.Clamp(clampWeightHead, 0f, 1f);
		this.clampWeightEyes = Mathf.Clamp(clampWeightEyes, 0f, 1f);
	}

	public override void StoreDefaultLocalState()
	{
		for (int i = 0; i < spine.Length; i++)
		{
			spine[i].StoreDefaultLocalState();
		}
		for (int j = 0; j < eyes.Length; j++)
		{
			eyes[j].StoreDefaultLocalState();
		}
		if (head != null && head.transform != null)
		{
			head.StoreDefaultLocalState();
		}
	}

	public override void FixTransforms()
	{
		if (!(IKPositionWeight <= 0f))
		{
			for (int i = 0; i < spine.Length; i++)
			{
				spine[i].FixTransform();
			}
			for (int j = 0; j < eyes.Length; j++)
			{
				eyes[j].FixTransform();
			}
			if (head != null && head.transform != null)
			{
				head.FixTransform();
			}
		}
	}

	public override bool IsValid(ref string message)
	{
		if (!Boolean_0)
		{
			message = "IKSolverLookAt spine setup is invalid. Can't initiate solver.";
			return false;
		}
		if (!Boolean_2)
		{
			message = "IKSolverLookAt head transform is null. Can't initiate solver.";
			return false;
		}
		if (!Boolean_4)
		{
			message = "IKSolverLookAt eyes setup is invalid. Can't initiate solver.";
			return false;
		}
		if (Boolean_1 && Boolean_3 && Boolean_5)
		{
			message = "IKSolverLookAt eyes setup is invalid. Can't initiate solver.";
			return false;
		}
		Bone[] bones = spine;
		Transform transform = IKSolver.ContainsDuplicateBone(bones);
		if (transform != null)
		{
			message = transform.name + " is represented multiple times in a single IK chain. Can't initiate solver.";
			return false;
		}
		bones = eyes;
		Transform transform2 = IKSolver.ContainsDuplicateBone(bones);
		if (transform2 != null)
		{
			message = transform2.name + " is represented multiple times in a single IK chain. Can't initiate solver.";
			return false;
		}
		return true;
	}

	public override Point[] GetPoints()
	{
		Point[] array = new Point[spine.Length + eyes.Length + ((head.transform != null) ? 1 : 0)];
		for (int i = 0; i < spine.Length; i++)
		{
			array[i] = spine[i];
		}
		int num = 0;
		for (int j = spine.Length; j < array.Length; j++)
		{
			array[j] = eyes[num];
			num++;
		}
		if (head.transform != null)
		{
			array[^1] = head;
		}
		return array;
	}

	public override Point GetPoint(Transform transform)
	{
		LookAtBone[] array = spine;
		int num = 0;
		LookAtBone lookAtBone;
		while (true)
		{
			if (num < array.Length)
			{
				lookAtBone = array[num];
				if (lookAtBone.transform == transform)
				{
					break;
				}
				num++;
				continue;
			}
			array = eyes;
			num = 0;
			LookAtBone lookAtBone2;
			while (true)
			{
				if (num < array.Length)
				{
					lookAtBone2 = array[num];
					if (lookAtBone2.transform == transform)
					{
						break;
					}
					num++;
					continue;
				}
				if (head.transform == transform)
				{
					return head;
				}
				return null;
			}
			return lookAtBone2;
		}
		return lookAtBone;
	}

	public bool SetChain(Transform[] spine, Transform head, Transform[] eyes, Transform root)
	{
		method_4(spine, ref this.spine);
		this.head = new LookAtBone(head);
		method_4(eyes, ref this.eyes);
		Initiate(root);
		return base.initiated;
	}

	public override void OnInitiate()
	{
		if (FirstInitiation || !Application.isPlaying)
		{
			if (spine.Length != 0)
			{
				IKPosition = spine[spine.Length - 1].transform.position + root.forward * 3f;
			}
			else if (head.transform != null)
			{
				IKPosition = head.transform.position + root.forward * 3f;
			}
			else if (eyes.Length != 0 && eyes[0].transform != null)
			{
				IKPosition = eyes[0].transform.position + root.forward * 3f;
			}
		}
		LookAtBone[] array = spine;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Initiate(root);
		}
		if (head != null)
		{
			head.Initiate(root);
		}
		array = eyes;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Initiate(root);
		}
		if (SpineForwards == null || SpineForwards.Length != spine.Length)
		{
			SpineForwards = new Vector3[spine.Length];
		}
		if (HeadForwards == null)
		{
			HeadForwards = new Vector3[1];
		}
		if (EyeForward == null)
		{
			EyeForward = new Vector3[1];
		}
	}

	public override void OnUpdate()
	{
		if (!(IKPositionWeight <= 0f))
		{
			IKPositionWeight = Mathf.Clamp(IKPositionWeight, 0f, 1f);
			if (target != null)
			{
				IKPosition = target.position;
			}
			method_0();
			method_1();
			method_2();
		}
	}

	public void method_0()
	{
		if (!(bodyWeight <= 0f) && !Boolean_1)
		{
			Vector3 normalized = (IKPosition - spine[spine.Length - 1].transform.position).normalized;
			method_3(ref SpineForwards, spine[0].forward, normalized, spine.Length, clampWeight);
			for (int i = 0; i < spine.Length; i++)
			{
				spine[i].LookAt(SpineForwards[i], bodyWeight * IKPositionWeight);
			}
		}
	}

	public void method_1()
	{
		if (!(headWeight <= 0f) && !Boolean_3)
		{
			Vector3 vector = ((spine.Length == 0 || !(spine[spine.Length - 1].transform != null)) ? head.forward : spine[spine.Length - 1].forward);
			Vector3 normalized = Vector3.Lerp(vector, (IKPosition - head.transform.position).normalized, headWeight * IKPositionWeight).normalized;
			method_3(ref HeadForwards, vector, normalized, 1, clampWeightHead);
			head.LookAt(HeadForwards[0], headWeight * IKPositionWeight);
		}
	}

	public void method_2()
	{
		if (!(eyesWeight <= 0f) && !Boolean_5)
		{
			for (int i = 0; i < eyes.Length; i++)
			{
				Vector3 baseForward = ((head.transform != null) ? head.forward : eyes[i].forward);
				method_3(ref EyeForward, baseForward, (IKPosition - eyes[i].transform.position).normalized, 1, clampWeightEyes);
				eyes[i].LookAt(EyeForward[0], eyesWeight * IKPositionWeight);
			}
		}
	}

	public Vector3[] method_3(ref Vector3[] forwards, Vector3 baseForward, Vector3 targetForward, int bones, float clamp)
	{
		if (!(clamp >= 1f) && IKPositionWeight > 0f)
		{
			float num = Vector3.Angle(baseForward, targetForward);
			float num2 = 1f - num / 180f;
			float num3 = ((clamp > 0f) ? Mathf.Clamp(1f - (clamp - num2) / (1f - num2), 0f, 1f) : 1f);
			float num4 = ((clamp > 0f) ? Mathf.Clamp(num2 / clamp, 0f, 1f) : 1f);
			for (int i = 0; i < clampSmoothing; i++)
			{
				num4 = Mathf.Sin(num4 * MathF.PI * 0.5f);
			}
			if (forwards.Length == 1)
			{
				forwards[0] = Vector3.Slerp(baseForward, targetForward, num4 * num3);
			}
			else
			{
				float num5 = 1f / (float)(forwards.Length - 1);
				for (int j = 0; j < forwards.Length; j++)
				{
					forwards[j] = Vector3.Slerp(baseForward, targetForward, spineWeightCurve.Evaluate(num5 * (float)j) * num4 * num3);
				}
			}
			return forwards;
		}
		for (int k = 0; k < forwards.Length; k++)
		{
			forwards[k] = baseForward;
		}
		return forwards;
	}

	public void method_4(Transform[] array, ref LookAtBone[] bones)
	{
		if (array == null)
		{
			bones = new LookAtBone[0];
			return;
		}
		if (bones.Length != array.Length)
		{
			bones = new LookAtBone[array.Length];
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (bones[i] == null)
			{
				bones[i] = new LookAtBone(array[i]);
			}
		}
	}
}
