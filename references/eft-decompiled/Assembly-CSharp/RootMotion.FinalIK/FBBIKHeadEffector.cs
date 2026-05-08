using System;
using UnityEngine;

namespace RootMotion.FinalIK;

public class FBBIKHeadEffector : MonoBehaviour
{
	[Serializable]
	public class BendBone
	{
		[Tooltip("Assign spine and/or neck bones.")]
		public Transform transform;

		[Tooltip("The weight of rotating this bone.")]
		[Range(0f, 1f)]
		public float weight = 0.5f;

		[NonSerialized]
		public Quaternion DefaultLocalRotation = Quaternion.identity;

		public BendBone()
		{
		}

		public BendBone(Transform transform, float weight)
		{
			this.transform = transform;
			this.weight = weight;
		}

		public void StoreDefaultLocalState()
		{
			DefaultLocalRotation = transform.localRotation;
		}

		public void FixTransforms()
		{
			transform.localRotation = DefaultLocalRotation;
		}
	}

	[Tooltip("Reference to the FBBIK component.")]
	public FullBodyBipedIK ik;

	[GAttribute20("Position")]
	[Tooltip("Master weight for positioning the head.")]
	[Range(0f, 1f)]
	public float positionWeight = 1f;

	[Tooltip("The weight of moving the body along with the head")]
	[Range(0f, 1f)]
	public float bodyWeight = 0.8f;

	[Tooltip("The weight of moving the thighs along with the head")]
	[Range(0f, 1f)]
	public float thighWeight = 0.8f;

	[GAttribute20("Rotation")]
	[Tooltip("The weight of rotating the head bone after solving")]
	[Range(0f, 1f)]
	public float rotationWeight;

	[Tooltip("Clamping the rotation of the body")]
	[Range(0f, 1f)]
	public float bodyClampWeight = 0.5f;

	[Tooltip("Clamping the rotation of the head")]
	[Range(0f, 1f)]
	public float headClampWeight = 0.5f;

	[Tooltip("The master weight of bending/twisting the spine to the rotation of the head effector. This is similar to CCD, but uses the rotation of the head effector not the position.")]
	[Range(0f, 1f)]
	public float bendWeight = 1f;

	[Tooltip("The bones to use for bending.")]
	public BendBone[] bendBones = new BendBone[0];

	[GAttribute20("CCD")]
	[Tooltip("Optional. The master weight of the CCD (Cyclic Coordinate Descent) IK effect that bends the spine towards the head effector before FBBIK solves.")]
	[Range(0f, 1f)]
	public float CCDWeight = 1f;

	[Tooltip("The weight of rolling the bones in towards the target")]
	[Range(0f, 1f)]
	public float roll;

	[Tooltip("Smoothing the CCD effect.")]
	[Range(0f, 1000f)]
	public float damper = 500f;

	[Tooltip("Bones to use for the CCD pass. Assign spine and/or neck bones.")]
	public Transform[] CCDBones = new Transform[0];

	[GAttribute20("Stretching")]
	[Tooltip("Stretching the spine/neck to help reach the target. This is useful for making sure the head stays locked relative to the VR headset. NB! Stretching is done after FBBIK has solved so if you have the hand effectors pinned and spine bones included in the 'Stretch Bones', the hands might become offset from their target positions.")]
	[Range(0f, 1f)]
	public float postStretchWeight = 1f;

	[Tooltip("Stretch magnitude limit.")]
	public float maxStretch = 0.1f;

	[Tooltip("If > 0, dampers the stretching effect.")]
	public float stretchDamper;

	[Tooltip("If true, will fix head position to this Transform no matter what. Good for making sure the head will not budge away from the VR headset")]
	public bool fixHead;

	[Tooltip("Bones to use for stretching. The more bones you add, the less noticable the effect.")]
	public Transform[] stretchBones = new Transform[0];

	[GAttribute20("Chest Direction")]
	public Vector3 chestDirection = Vector3.forward;

	[Range(0f, 1f)]
	public float chestDirectionWeight = 1f;

	public Transform[] chestBones;

	public IKSolver.GDelegate47 OnPostHeadEffectorFK;

	private Vector3 vector3_0;

	private Vector3 vector3_1;

	private Vector3 vector3_2;

	private Vector3 vector3_3;

	private Vector3 vector3_4;

	private Vector3 vector3_5;

	private Vector3 vector3_6;

	private float float_0;

	private float float_1;

	private float float_2;

	private Quaternion quaternion_0;

	private Quaternion quaternion_1;

	private Quaternion[] quaternion_2 = new Quaternion[0];

	private Vector3 vector3_7;

	private Quaternion quaternion_3;

	private Vector3[] vector3_8 = new Vector3[0];

	private Quaternion[] quaternion_4 = new Quaternion[0];

	private Vector3[] vector3_9 = new Vector3[0];

	private Quaternion[] quaternion_5 = new Quaternion[0];

	private int int_0;

	private int int_1;

	private int int_2;

	private int int_3;

	public void Awake()
	{
		IKSolverFullBodyBiped solver = ik.solver;
		solver.OnPreRead = (IKSolver.GDelegate47)Delegate.Combine(solver.OnPreRead, new IKSolver.GDelegate47(method_2));
		IKSolverFullBodyBiped solver2 = ik.solver;
		solver2.OnPreIteration = (IKSolver.GDelegate48)Delegate.Combine(solver2.OnPreIteration, new IKSolver.GDelegate48(method_5));
		IKSolverFullBodyBiped solver3 = ik.solver;
		solver3.OnPostUpdate = (IKSolver.GDelegate47)Delegate.Combine(solver3.OnPostUpdate, new IKSolver.GDelegate47(method_6));
		IKSolverFullBodyBiped solver4 = ik.solver;
		solver4.OnStoreDefaultLocalState = (IKSolver.GDelegate47)Delegate.Combine(solver4.OnStoreDefaultLocalState, new IKSolver.GDelegate47(method_0));
		IKSolverFullBodyBiped solver5 = ik.solver;
		solver5.OnFixTransforms = (IKSolver.GDelegate47)Delegate.Combine(solver5.OnFixTransforms, new IKSolver.GDelegate47(method_1));
		quaternion_1 = Quaternion.Inverse(ik.references.root.rotation) * ik.references.head.rotation;
	}

	public void method_0()
	{
		BendBone[] array = bendBones;
		for (int i = 0; i < array.Length; i++)
		{
			array[i]?.StoreDefaultLocalState();
		}
		quaternion_2 = new Quaternion[CCDBones.Length];
		for (int j = 0; j < CCDBones.Length; j++)
		{
			if (CCDBones[j] != null)
			{
				quaternion_2[j] = CCDBones[j].localRotation;
			}
		}
		vector3_7 = ik.references.head.localPosition;
		quaternion_3 = ik.references.head.localRotation;
		vector3_8 = new Vector3[stretchBones.Length];
		quaternion_4 = new Quaternion[stretchBones.Length];
		for (int k = 0; k < stretchBones.Length; k++)
		{
			if (stretchBones[k] != null)
			{
				vector3_8[k] = stretchBones[k].localPosition;
				quaternion_4[k] = stretchBones[k].localRotation;
			}
		}
		vector3_9 = new Vector3[chestBones.Length];
		quaternion_5 = new Quaternion[chestBones.Length];
		for (int l = 0; l < chestBones.Length; l++)
		{
			if (chestBones[l] != null)
			{
				vector3_9[l] = chestBones[l].localPosition;
				quaternion_5[l] = chestBones[l].localRotation;
			}
		}
		int_0 = bendBones.Length;
		int_1 = CCDBones.Length;
		int_2 = stretchBones.Length;
		int_3 = chestBones.Length;
	}

	public void method_1()
	{
		if (!ik.enabled)
		{
			return;
		}
		BendBone[] array = bendBones;
		for (int i = 0; i < array.Length; i++)
		{
			array[i]?.FixTransforms();
		}
		for (int j = 0; j < CCDBones.Length; j++)
		{
			if (CCDBones[j] != null)
			{
				CCDBones[j].localRotation = quaternion_2[j];
			}
		}
		ik.references.head.localPosition = vector3_7;
		ik.references.head.localRotation = quaternion_3;
		for (int k = 0; k < stretchBones.Length; k++)
		{
			if (stretchBones[k] != null)
			{
				stretchBones[k].localPosition = vector3_8[k];
				stretchBones[k].localRotation = quaternion_4[k];
			}
		}
		for (int l = 0; l < chestBones.Length; l++)
		{
			if (chestBones[l] != null)
			{
				chestBones[l].localPosition = vector3_9[l];
				chestBones[l].localRotation = quaternion_5[l];
			}
		}
	}

	public void method_2()
	{
		if (base.enabled && ik.enabled && base.gameObject.activeInHierarchy && ik.solver.iterations != 0)
		{
			if (int_0 != bendBones.Length || int_1 != CCDBones.Length || int_2 != stretchBones.Length || int_3 != chestBones.Length)
			{
				method_0();
			}
			method_7();
			method_3();
			method_4();
			vector3_0 = base.transform.position - ik.references.head.position;
			float_0 = Vector3.Distance(ik.references.leftUpperArm.position, ik.references.rightUpperArm.position);
			float_1 = Vector3.Distance(ik.references.head.position, ik.references.leftUpperArm.position);
			float_2 = Vector3.Distance(ik.references.head.position, ik.references.rightUpperArm.position);
			vector3_1 = ik.solver.rootNode.position - ik.references.head.position;
			vector3_3 = ik.references.leftThigh.position - ik.references.head.position;
			vector3_4 = ik.references.rightThigh.position - ik.references.head.position;
			vector3_5 = ik.references.leftUpperArm.position + vector3_0 * bodyWeight;
			vector3_6 = ik.references.rightUpperArm.position + vector3_0 * bodyWeight;
			quaternion_0 = Quaternion.LookRotation(ik.references.head.position - ik.references.leftUpperArm.position, ik.references.rightUpperArm.position - ik.references.leftUpperArm.position);
			if (OnPostHeadEffectorFK != null)
			{
				OnPostHeadEffectorFK();
			}
		}
	}

	public void method_3()
	{
		float num = bendWeight * ik.solver.IKPositionWeight;
		if (num <= 0f || bendBones.Length == 0)
		{
			return;
		}
		Quaternion rotation = base.transform.rotation * Quaternion.Inverse(ik.references.root.rotation * quaternion_1);
		rotation = GClass1463.ClampRotation(rotation, bodyClampWeight, 2);
		float num2 = 1f / (float)bendBones.Length;
		for (int i = 0; i < bendBones.Length; i++)
		{
			if (bendBones[i].transform != null)
			{
				bendBones[i].transform.rotation = Quaternion.Lerp(Quaternion.identity, rotation, num2 * bendBones[i].weight * num) * bendBones[i].transform.rotation;
			}
		}
	}

	public void method_4()
	{
		float num = CCDWeight * ik.solver.IKPositionWeight;
		if (!(num <= 0f))
		{
			for (int num2 = CCDBones.Length - 1; num2 > -1; num2--)
			{
				Quaternion quaternion = Quaternion.FromToRotation(ik.references.head.position - CCDBones[num2].position, base.transform.position - CCDBones[num2].position) * CCDBones[num2].rotation;
				float num3 = Mathf.Lerp((CCDBones.Length - num2) / CCDBones.Length, 1f, roll);
				float num4 = Quaternion.Angle(Quaternion.identity, quaternion);
				num4 = Mathf.Lerp(0f, num4, (damper - num4) / damper);
				CCDBones[num2].rotation = Quaternion.RotateTowards(CCDBones[num2].rotation, quaternion, num4 * num * num3);
			}
		}
	}

	public void method_5(int iteration)
	{
		if (base.enabled && ik.enabled && base.gameObject.activeInHierarchy && ik.solver.iterations != 0)
		{
			vector3_5 = base.transform.position + (vector3_5 - base.transform.position).normalized * float_1;
			vector3_6 = base.transform.position + (vector3_6 - base.transform.position).normalized * float_2;
			method_10(ref vector3_5, ref vector3_6, float_0);
			method_9(ik.solver.leftShoulderEffector, vector3_5, positionWeight * ik.solver.IKPositionWeight, ik.solver.leftShoulderEffector.positionOffset);
			method_9(ik.solver.rightShoulderEffector, vector3_6, positionWeight * ik.solver.IKPositionWeight, ik.solver.rightShoulderEffector.positionOffset);
			Quaternion to = Quaternion.LookRotation(base.transform.position - vector3_5, vector3_6 - vector3_5);
			Quaternion quaternion = GClass1463.FromToRotation(quaternion_0, to);
			Vector3 vector = quaternion * vector3_1;
			method_9(ik.solver.bodyEffector, base.transform.position + vector, positionWeight * ik.solver.IKPositionWeight, ik.solver.bodyEffector.positionOffset - ik.solver.pullBodyOffset);
			Quaternion quaternion2 = Quaternion.Lerp(Quaternion.identity, quaternion, thighWeight);
			Vector3 vector2 = quaternion2 * vector3_3;
			Vector3 vector3 = quaternion2 * vector3_4;
			method_9(ik.solver.leftThighEffector, base.transform.position + vector2, positionWeight * ik.solver.IKPositionWeight, ik.solver.bodyEffector.positionOffset - ik.solver.pullBodyOffset + ik.solver.leftThighEffector.positionOffset);
			method_9(ik.solver.rightThighEffector, base.transform.position + vector3, positionWeight * ik.solver.IKPositionWeight, ik.solver.bodyEffector.positionOffset - ik.solver.pullBodyOffset + ik.solver.rightThighEffector.positionOffset);
		}
	}

	public void method_6()
	{
		if (base.enabled && ik.enabled && base.gameObject.activeInHierarchy)
		{
			method_8();
			Quaternion rotation = GClass1463.FromToRotation(ik.references.head.rotation, base.transform.rotation);
			rotation = GClass1463.ClampRotation(rotation, headClampWeight, 2);
			ik.references.head.rotation = Quaternion.Lerp(Quaternion.identity, rotation, rotationWeight * ik.solver.IKPositionWeight) * ik.references.head.rotation;
		}
	}

	public void method_7()
	{
		float num = chestDirectionWeight * ik.solver.IKPositionWeight;
		if (num <= 0f)
		{
			return;
		}
		bool changed = false;
		chestDirection = GClass1464.ClampDirection(chestDirection, ik.references.root.forward, 0.45f, 2, out changed);
		if (!(chestDirection == Vector3.zero))
		{
			Quaternion b = Quaternion.FromToRotation(ik.references.root.forward, chestDirection);
			b = Quaternion.Lerp(Quaternion.identity, b, num * (1f / (float)chestBones.Length));
			Transform[] array = chestBones;
			foreach (Transform transform in array)
			{
				transform.rotation = b * transform.rotation;
			}
		}
	}

	public void method_8()
	{
		float num = postStretchWeight * ik.solver.IKPositionWeight;
		if (num > 0f)
		{
			Vector3 vector = Vector3.ClampMagnitude(base.transform.position - ik.references.head.position, maxStretch);
			vector *= num;
			stretchDamper = Mathf.Max(stretchDamper, 0f);
			if (stretchDamper > 0f)
			{
				vector /= (1f + vector.magnitude) * (1f + stretchDamper);
			}
			for (int i = 0; i < stretchBones.Length; i++)
			{
				if (stretchBones[i] != null)
				{
					stretchBones[i].position += vector / stretchBones.Length;
				}
			}
		}
		if (fixHead && ik.solver.IKPositionWeight > 0f)
		{
			ik.references.head.position = base.transform.position;
		}
	}

	public void method_9(IKEffector effector, Vector3 position, float weight, Vector3 offset)
	{
		effector.GetNode(ik.solver).solverPosition = Vector3.Lerp(effector.GetNode(ik.solver).solverPosition, position + offset, weight);
	}

	public void method_10(ref Vector3 pos1, ref Vector3 pos2, float nominalDistance)
	{
		Vector3 vector = pos2 - pos1;
		float magnitude = vector.magnitude;
		if (magnitude != nominalDistance && magnitude != 0f)
		{
			float num = 1f;
			num *= 1f - nominalDistance / magnitude;
			Vector3 vector2 = vector * num * 0.5f;
			pos1 += vector2;
			pos2 -= vector2;
		}
	}

	public void OnDestroy()
	{
		if (ik != null)
		{
			IKSolverFullBodyBiped solver = ik.solver;
			solver.OnPreRead = (IKSolver.GDelegate47)Delegate.Remove(solver.OnPreRead, new IKSolver.GDelegate47(method_2));
			IKSolverFullBodyBiped solver2 = ik.solver;
			solver2.OnPreIteration = (IKSolver.GDelegate48)Delegate.Remove(solver2.OnPreIteration, new IKSolver.GDelegate48(method_5));
			IKSolverFullBodyBiped solver3 = ik.solver;
			solver3.OnPostUpdate = (IKSolver.GDelegate47)Delegate.Remove(solver3.OnPostUpdate, new IKSolver.GDelegate47(method_6));
			IKSolverFullBodyBiped solver4 = ik.solver;
			solver4.OnStoreDefaultLocalState = (IKSolver.GDelegate47)Delegate.Remove(solver4.OnStoreDefaultLocalState, new IKSolver.GDelegate47(method_0));
			IKSolverFullBodyBiped solver5 = ik.solver;
			solver5.OnFixTransforms = (IKSolver.GDelegate47)Delegate.Remove(solver5.OnFixTransforms, new IKSolver.GDelegate47(method_1));
		}
	}
}
