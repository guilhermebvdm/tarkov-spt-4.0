using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[HelpURL("http://www.root-motion.com/finalikdox/html/page11.html")]
[AddComponentMenu("Scripts/RootMotion.FinalIK/Grounder/Grounder IK")]
public class GrounderIK : Grounder
{
	public IK[] legs;

	[Tooltip("The pelvis transform. Common ancestor of all the legs.")]
	public Transform pelvis;

	[Tooltip("The root Transform of the character, with the rigidbody and the collider.")]
	public Transform characterRoot;

	[Tooltip("The weight of rotating the character root to the ground normal (range: 0 - 1).")]
	[Range(0f, 1f)]
	public float rootRotationWeight;

	[Tooltip("The speed of rotating the character root to the ground normal (range: 0 - inf).")]
	public float rootRotationSpeed = 5f;

	[Tooltip("The maximum angle of root rotation (range: 0 - 90).")]
	public float maxRootRotationAngle = 45f;

	private Transform[] transform_0 = new Transform[0];

	private Quaternion[] quaternion_0 = new Quaternion[0];

	private Vector3 vector3_0;

	private Vector3 vector3_1;

	private int int_0;

	private bool bool_0;

	private float float_0;

	[ContextMenu("User Manual")]
	public override void OpenUserManual()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/page11.html");
	}

	[ContextMenu("Scrpt Reference")]
	public override void OpenScriptReference()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_grounder_i_k.html");
	}

	public override void ResetPosition()
	{
		solver.Reset();
	}

	public bool method_2()
	{
		if (pelvis == null)
		{
			return false;
		}
		if (legs.Length == 0)
		{
			return false;
		}
		IK[] array = legs;
		int num = 0;
		while (true)
		{
			if (num < array.Length)
			{
				IK iK = array[num];
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
		for (int i = 0; i < legs.Length; i++)
		{
			if (legs[i] != null)
			{
				legs[i].GetIKSolver().IKPositionWeight = 0f;
			}
		}
	}

	public void Update()
	{
		weight = Mathf.Clamp(weight, 0f, 1f);
		if (weight <= 0f)
		{
			return;
		}
		bool_0 = false;
		if (initiated)
		{
			rootRotationWeight = Mathf.Clamp(rootRotationWeight, 0f, 1f);
			rootRotationSpeed = Mathf.Clamp(rootRotationSpeed, 0f, rootRotationSpeed);
			if (characterRoot != null && rootRotationSpeed > 0f && rootRotationWeight > 0f)
			{
				Vector3 vector = solver.GetLegsPlaneNormal();
				if (rootRotationWeight < 1f)
				{
					vector = Vector3.Slerp(Vector3.up, vector, rootRotationWeight);
				}
				Quaternion b = Quaternion.RotateTowards(Quaternion.FromToRotation(base.transform.up, Vector3.up) * characterRoot.rotation, Quaternion.FromToRotation(base.transform.up, vector) * characterRoot.rotation, maxRootRotationAngle);
				characterRoot.rotation = Quaternion.Lerp(characterRoot.rotation, b, Time.deltaTime * rootRotationSpeed);
			}
		}
		else if (method_2())
		{
			method_3();
		}
	}

	public void method_3()
	{
		transform_0 = new Transform[legs.Length];
		quaternion_0 = new Quaternion[legs.Length];
		for (int i = 0; i < transform_0.Length; i++)
		{
			quaternion_0[i] = Quaternion.identity;
		}
		for (int j = 0; j < legs.Length; j++)
		{
			IKSolver.Point[] points = legs[j].GetIKSolver().GetPoints();
			transform_0[j] = points[^1].transform;
			IKSolver iKSolver = legs[j].GetIKSolver();
			iKSolver.OnPreUpdate = (IKSolver.GDelegate47)Delegate.Combine(iKSolver.OnPreUpdate, new IKSolver.GDelegate47(method_4));
			IKSolver iKSolver2 = legs[j].GetIKSolver();
			iKSolver2.OnPostUpdate = (IKSolver.GDelegate47)Delegate.Combine(iKSolver2.OnPostUpdate, new IKSolver.GDelegate47(method_6));
		}
		vector3_0 = pelvis.localPosition;
		solver.Initiate(base.transform, transform_0);
		initiated = true;
	}

	public void method_4()
	{
		if (!base.enabled)
		{
			return;
		}
		if (weight <= 0f)
		{
			if (float_0 <= 0f)
			{
				return;
			}
			OnDisable();
		}
		float_0 = weight;
		if (!bool_0)
		{
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
			solver.Update();
			for (int i = 0; i < legs.Length; i++)
			{
				method_5(i);
			}
			pelvis.position += solver.pelvis.IKOffset * weight;
			bool_0 = true;
			int_0 = 0;
			if (OnPostGrounder != null)
			{
				OnPostGrounder();
			}
		}
	}

	public void method_5(int index)
	{
		quaternion_0[index] = transform_0[index].rotation;
		legs[index].GetIKSolver().IKPosition = solver.legs[index].IKPosition;
		legs[index].GetIKSolver().IKPositionWeight = weight;
	}

	public void method_6()
	{
		if (weight <= 0f || !base.enabled)
		{
			return;
		}
		int_0++;
		if (int_0 >= transform_0.Length)
		{
			for (int i = 0; i < transform_0.Length; i++)
			{
				transform_0[i].rotation = Quaternion.Slerp(Quaternion.identity, solver.legs[i].rotationOffset, weight) * quaternion_0[i];
			}
			vector3_1 = pelvis.localPosition;
		}
	}

	public void OnDestroy()
	{
		if (!initiated)
		{
			return;
		}
		IK[] array = legs;
		foreach (IK iK in array)
		{
			if (iK != null)
			{
				IKSolver iKSolver = iK.GetIKSolver();
				iKSolver.OnPreUpdate = (IKSolver.GDelegate47)Delegate.Remove(iKSolver.OnPreUpdate, new IKSolver.GDelegate47(method_4));
				IKSolver iKSolver2 = iK.GetIKSolver();
				iKSolver2.OnPostUpdate = (IKSolver.GDelegate47)Delegate.Remove(iKSolver2.OnPostUpdate, new IKSolver.GDelegate47(method_6));
			}
		}
	}
}
