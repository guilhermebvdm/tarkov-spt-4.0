using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[HelpURL("http://www.root-motion.com/finalikdox/html/page11.html")]
[AddComponentMenu("Scripts/RootMotion.FinalIK/Grounder/Grounder Quadruped")]
public class GrounderQuadruped : Grounder
{
	public struct GStruct147(IKSolver solver, Transform transform)
	{
		public IKSolver solver = solver;

		public Transform transform = transform;

		public Quaternion rotation = transform.rotation;

		public Grounding.Leg leg = null;
	}

	[Tooltip("The Grounding solver for the forelegs.")]
	public Grounding forelegSolver = new Grounding();

	[Tooltip("The weight of rotating the character root to the ground angle (range: 0 - 1).")]
	[Range(0f, 1f)]
	public float rootRotationWeight = 0.5f;

	[Tooltip("The maximum angle of rotating the quadruped downwards (going downhill, range: -90 - 0).")]
	[Range(-90f, 0f)]
	public float minRootRotation = -25f;

	[Tooltip("The maximum angle of rotating the quadruped upwards (going uphill, range: 0 - 90).")]
	[Range(0f, 90f)]
	public float maxRootRotation = 45f;

	[Tooltip("The speed of interpolating the character root rotation (range: 0 - inf).")]
	public float rootRotationSpeed = 5f;

	[Tooltip("The maximum IK offset for the legs (range: 0 - inf).")]
	public float maxLegOffset = 0.5f;

	[Tooltip("The maximum IK offset for the forelegs (range: 0 - inf).")]
	public float maxForeLegOffset = 0.5f;

	[Tooltip("The weight of maintaining the head's rotation as it was before solving the Grounding (range: 0 - 1).")]
	[Range(0f, 1f)]
	public float maintainHeadRotationWeight = 0.5f;

	[Tooltip("The root Transform of the character, with the rigidbody and the collider.")]
	public Transform characterRoot;

	[Tooltip("The pelvis transform. Common ancestor of both legs and the spine.")]
	public Transform pelvis;

	[Tooltip("The last bone in the spine that is the common parent for both forelegs.")]
	public Transform lastSpineBone;

	[Tooltip("The head (optional, if you intend to maintain it's rotation).")]
	public Transform head;

	public IK[] legs;

	public IK[] forelegs;

	[HideInInspector]
	public Vector3 gravity = Vector3.down;

	private GStruct147[] gstruct147_0 = new GStruct147[0];

	private Vector3 vector3_0;

	private Quaternion quaternion_0;

	private Quaternion quaternion_1;

	private Vector3 vector3_1;

	private Quaternion quaternion_2;

	private Quaternion quaternion_3;

	private int int_0;

	private bool bool_0;

	private float float_0;

	private Transform transform_0;

	private Quaternion quaternion_4;

	private float float_1;

	[ContextMenu("User Manual")]
	public override void OpenUserManual()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/page11.html");
	}

	[ContextMenu("Scrpt Reference")]
	public override void OpenScriptReference()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_grounder_quadruped.html");
	}

	public override void ResetPosition()
	{
		solver.Reset();
		forelegSolver.Reset();
	}

	public bool method_2()
	{
		if (pelvis == null)
		{
			return false;
		}
		if (lastSpineBone == null)
		{
			return false;
		}
		if (legs.Length == 0)
		{
			return false;
		}
		if (forelegs.Length == 0)
		{
			return false;
		}
		if (characterRoot == null)
		{
			return false;
		}
		if (!method_3(legs))
		{
			return false;
		}
		if (!method_3(forelegs))
		{
			return false;
		}
		return true;
	}

	public bool method_3(IK[] ikComponents)
	{
		int num = 0;
		while (true)
		{
			if (num < ikComponents.Length)
			{
				IK iK = ikComponents[num];
				if (!(iK == null))
				{
					if (!(iK is FullBodyBipedIK))
					{
						if (!(iK is FABRIKRoot))
						{
							if (iK is AimIK)
							{
								break;
							}
							num++;
							continue;
						}
						LogWarning("GrounderIK does not support FABRIKRoot, use CCDIK, FABRIK, LimbIK or TrigonometricIK instead.");
						return false;
					}
					LogWarning("GrounderIK does not support FullBodyBipedIK, use CCDIK, FABRIK, LimbIK or TrigonometricIK instead. If you want to use FullBodyBipedIK, use the GrounderFBBIK component.");
					return false;
				}
				return false;
			}
			return true;
		}
		LogWarning("GrounderIK does not support AimIK, use CCDIK, FABRIK, LimbIK or TrigonometricIK instead.");
		return false;
	}

	public void OnDisable()
	{
		if (!initiated)
		{
			return;
		}
		for (int i = 0; i < gstruct147_0.Length; i++)
		{
			if (gstruct147_0[i].solver != null)
			{
				gstruct147_0[i].solver.IKPositionWeight = 0f;
			}
		}
	}

	public void Update()
	{
		weight = Mathf.Clamp(weight, 0f, 1f);
		if (!(weight <= 0f))
		{
			bool_0 = false;
			if (!initiated && method_2())
			{
				method_4();
			}
		}
	}

	public void method_4()
	{
		gstruct147_0 = new GStruct147[legs.Length + forelegs.Length];
		Transform[] array = method_5(legs, ref gstruct147_0, 0);
		Transform[] array2 = method_5(forelegs, ref gstruct147_0, legs.Length);
		vector3_0 = pelvis.localPosition;
		quaternion_0 = pelvis.localRotation;
		if (head != null)
		{
			quaternion_1 = head.localRotation;
		}
		transform_0 = new GameObject().transform;
		transform_0.parent = base.transform;
		transform_0.name = "Forefeet Root";
		solver.Initiate(base.transform, array);
		forelegSolver.Initiate(transform_0, array2);
		for (int i = 0; i < array.Length; i++)
		{
			gstruct147_0[i].leg = solver.legs[i];
		}
		for (int j = 0; j < array2.Length; j++)
		{
			gstruct147_0[j + legs.Length].leg = forelegSolver.legs[j];
		}
		initiated = true;
	}

	public Transform[] method_5(IK[] ikComponents, ref GStruct147[] f, int indexOffset)
	{
		Transform[] array = new Transform[ikComponents.Length];
		for (int i = 0; i < ikComponents.Length; i++)
		{
			IKSolver.Point[] points = ikComponents[i].GetIKSolver().GetPoints();
			f[i + indexOffset] = new GStruct147(ikComponents[i].GetIKSolver(), points[^1].transform);
			array[i] = f[i + indexOffset].transform;
			IKSolver iKSolver = f[i + indexOffset].solver;
			iKSolver.OnPreUpdate = (IKSolver.GDelegate47)Delegate.Combine(iKSolver.OnPreUpdate, new IKSolver.GDelegate47(method_7));
			IKSolver iKSolver2 = f[i + indexOffset].solver;
			iKSolver2.OnPostUpdate = (IKSolver.GDelegate47)Delegate.Combine(iKSolver2.OnPostUpdate, new IKSolver.GDelegate47(method_10));
		}
		return array;
	}

	public void LateUpdate()
	{
		if (!(weight <= 0f))
		{
			rootRotationWeight = Mathf.Clamp(rootRotationWeight, 0f, 1f);
			minRootRotation = Mathf.Clamp(minRootRotation, -90f, maxRootRotation);
			maxRootRotation = Mathf.Clamp(maxRootRotation, minRootRotation, 90f);
			rootRotationSpeed = Mathf.Clamp(rootRotationSpeed, 0f, rootRotationSpeed);
			maxLegOffset = Mathf.Clamp(maxLegOffset, 0f, maxLegOffset);
			maxForeLegOffset = Mathf.Clamp(maxForeLegOffset, 0f, maxForeLegOffset);
			maintainHeadRotationWeight = Mathf.Clamp(maintainHeadRotationWeight, 0f, 1f);
			method_6();
		}
	}

	public void method_6()
	{
		if (!(rootRotationWeight <= 0f) && !(rootRotationSpeed <= 0f))
		{
			solver.rotateSolver = true;
			forelegSolver.rotateSolver = true;
			Vector3 tangent = characterRoot.forward;
			Vector3 normal = -gravity;
			Vector3.OrthoNormalize(ref normal, ref tangent);
			Quaternion quaternion = Quaternion.LookRotation(tangent, -gravity);
			Vector3 vector = forelegSolver.rootHit.point - solver.rootHit.point;
			Vector3 vector2 = Quaternion.Inverse(quaternion) * vector;
			float num = Mathf.Atan2(vector2.y, vector2.z) * 57.29578f;
			num = Mathf.Clamp(num * rootRotationWeight, minRootRotation, maxRootRotation);
			float_0 = Mathf.Lerp(float_0, num, Time.deltaTime * rootRotationSpeed);
			characterRoot.rotation = Quaternion.Slerp(characterRoot.rotation, Quaternion.AngleAxis(0f - float_0, characterRoot.right) * quaternion, weight);
		}
	}

	public void method_7()
	{
		if (!base.enabled)
		{
			return;
		}
		if (weight <= 0f)
		{
			if (float_1 <= 0f)
			{
				return;
			}
			OnDisable();
		}
		float_1 = weight;
		if (bool_0)
		{
			return;
		}
		if (OnPreGrounder != null)
		{
			OnPreGrounder();
		}
		if (pelvis.localPosition != vector3_1)
		{
			vector3_0 = pelvis.localPosition;
		}
		else
		{
			pelvis.localPosition = vector3_0;
		}
		if (pelvis.localRotation != quaternion_2)
		{
			quaternion_0 = pelvis.localRotation;
		}
		else
		{
			pelvis.localRotation = quaternion_0;
		}
		if (head != null)
		{
			if (head.localRotation != quaternion_3)
			{
				quaternion_1 = head.localRotation;
			}
			else
			{
				head.localRotation = quaternion_1;
			}
		}
		for (int i = 0; i < gstruct147_0.Length; i++)
		{
			gstruct147_0[i].rotation = gstruct147_0[i].transform.rotation;
		}
		if (head != null)
		{
			quaternion_4 = head.rotation;
		}
		method_8();
		solver.Update();
		forelegSolver.Update();
		pelvis.position += solver.pelvis.IKOffset * weight;
		Vector3 fromDirection = lastSpineBone.position - pelvis.position;
		Vector3 toDirection = lastSpineBone.position + forelegSolver.root.up * Mathf.Clamp(forelegSolver.pelvis.heightOffset, float.NegativeInfinity, 0f) - solver.root.up * solver.pelvis.heightOffset - pelvis.position;
		Quaternion b = Quaternion.FromToRotation(fromDirection, toDirection);
		pelvis.rotation = Quaternion.Slerp(Quaternion.identity, b, weight) * pelvis.rotation;
		for (int j = 0; j < gstruct147_0.Length; j++)
		{
			method_9(gstruct147_0[j], (j < 2) ? maxLegOffset : maxForeLegOffset);
		}
		bool_0 = true;
		int_0 = 0;
		if (OnPostGrounder != null)
		{
			OnPostGrounder();
		}
	}

	public void method_8()
	{
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < forelegSolver.legs.Length; i++)
		{
			zero += forelegSolver.legs[i].transform.position;
		}
		zero /= (float)forelegs.Length;
		Vector3 vector = zero - base.transform.position;
		Vector3 normal = base.transform.up;
		Vector3 tangent = vector;
		Vector3.OrthoNormalize(ref normal, ref tangent);
		transform_0.position = base.transform.position + tangent.normalized * vector.magnitude;
	}

	public void method_9(GStruct147 foot, float maxOffset)
	{
		Vector3 vector = foot.leg.IKPosition - foot.transform.position;
		foot.solver.IKPosition = foot.transform.position + Vector3.ClampMagnitude(vector, maxOffset);
		foot.solver.IKPositionWeight = weight;
	}

	public void method_10()
	{
		if (weight <= 0f || !base.enabled)
		{
			return;
		}
		int_0++;
		if (int_0 >= gstruct147_0.Length)
		{
			for (int i = 0; i < gstruct147_0.Length; i++)
			{
				gstruct147_0[i].transform.rotation = Quaternion.Slerp(Quaternion.identity, gstruct147_0[i].leg.rotationOffset, weight) * gstruct147_0[i].rotation;
			}
			if (head != null)
			{
				head.rotation = Quaternion.Lerp(head.rotation, quaternion_4, maintainHeadRotationWeight * weight);
			}
			vector3_1 = pelvis.localPosition;
			quaternion_2 = pelvis.localRotation;
			if (head != null)
			{
				quaternion_3 = head.localRotation;
			}
		}
	}

	public void OnDestroy()
	{
		if (initiated)
		{
			method_11(legs);
			method_11(forelegs);
		}
	}

	public void method_11(IK[] ikComponents)
	{
		foreach (IK iK in ikComponents)
		{
			if (iK != null)
			{
				IKSolver iKSolver = iK.GetIKSolver();
				iKSolver.OnPreUpdate = (IKSolver.GDelegate47)Delegate.Remove(iKSolver.OnPreUpdate, new IKSolver.GDelegate47(method_7));
				IKSolver iKSolver2 = iK.GetIKSolver();
				iKSolver2.OnPostUpdate = (IKSolver.GDelegate47)Delegate.Remove(iKSolver2.OnPostUpdate, new IKSolver.GDelegate47(method_10));
			}
		}
	}
}
