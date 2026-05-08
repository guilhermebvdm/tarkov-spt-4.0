using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[HelpURL("http://www.root-motion.com/finalikdox/html/page11.html")]
[AddComponentMenu("Scripts/RootMotion.FinalIK/Grounder/Grounder Biped")]
public class GrounderBipedIK : Grounder
{
	[Tooltip("The BipedIK componet.")]
	public BipedIK ik;

	[Tooltip("The amount of spine bending towards upward slopes.")]
	public float spineBend = 7f;

	[Tooltip("The interpolation speed of spine bending.")]
	public float spineSpeed = 3f;

	private Transform[] transform_0 = new Transform[2];

	private Quaternion[] quaternion_0 = new Quaternion[2];

	private Vector3 vector3_0;

	private Vector3 vector3_1;

	private Vector3 vector3_2;

	private float float_0;

	[ContextMenu("User Manual")]
	public override void OpenUserManual()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/page11.html");
	}

	[ContextMenu("Scrpt Reference")]
	public override void OpenScriptReference()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_grounder_biped_i_k.html");
	}

	public override void ResetPosition()
	{
		solver.Reset();
		vector3_2 = Vector3.zero;
	}

	public bool method_2()
	{
		if (ik == null)
		{
			return false;
		}
		if (!ik.solvers.leftFoot.initiated)
		{
			return false;
		}
		if (!ik.solvers.rightFoot.initiated)
		{
			return false;
		}
		return true;
	}

	public void Update()
	{
		weight = Mathf.Clamp(weight, 0f, 1f);
		if (!(weight <= 0f) && !initiated && method_2())
		{
			method_3();
		}
	}

	public void method_3()
	{
		transform_0 = new Transform[2];
		quaternion_0 = new Quaternion[2];
		transform_0[0] = ik.references.leftFoot;
		transform_0[1] = ik.references.rightFoot;
		quaternion_0[0] = Quaternion.identity;
		quaternion_0[1] = Quaternion.identity;
		IKSolverFABRIK spine = ik.solvers.spine;
		spine.OnPreUpdate = (IKSolver.GDelegate47)Delegate.Combine(spine.OnPreUpdate, new IKSolver.GDelegate47(method_4));
		IKSolverLimb rightFoot = ik.solvers.rightFoot;
		rightFoot.OnPostUpdate = (IKSolver.GDelegate47)Delegate.Combine(rightFoot.OnPostUpdate, new IKSolver.GDelegate47(method_6));
		vector3_0 = ik.references.pelvis.localPosition;
		solver.Initiate(ik.references.root, transform_0);
		initiated = true;
	}

	public void OnDisable()
	{
		if (initiated)
		{
			ik.solvers.leftFoot.IKPositionWeight = 0f;
			ik.solvers.rightFoot.IKPositionWeight = 0f;
		}
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
		if (OnPreGrounder != null)
		{
			OnPreGrounder();
		}
		if (ik.references.pelvis.localPosition != vector3_1)
		{
			vector3_0 = ik.references.pelvis.localPosition;
		}
		else
		{
			ik.references.pelvis.localPosition = vector3_0;
		}
		solver.Update();
		ik.references.pelvis.position += solver.pelvis.IKOffset * weight;
		method_5(ik.solvers.leftFoot, 0);
		method_5(ik.solvers.rightFoot, 1);
		if (spineBend != 0f && ik.references.spine.Length != 0)
		{
			spineSpeed = Mathf.Clamp(spineSpeed, 0f, spineSpeed);
			Vector3 vector = GetSpineOffsetTarget() * weight;
			vector3_2 = Vector3.Lerp(vector3_2, vector * spineBend, Time.deltaTime * spineSpeed);
			Quaternion rotation = ik.references.leftUpperArm.rotation;
			Quaternion rotation2 = ik.references.rightUpperArm.rotation;
			Vector3 up = solver.up;
			Quaternion quaternion = Quaternion.FromToRotation(up, up + vector3_2);
			ik.references.spine[0].rotation = quaternion * ik.references.spine[0].rotation;
			ik.references.leftUpperArm.rotation = rotation;
			ik.references.rightUpperArm.rotation = rotation2;
		}
		if (OnPostGrounder != null)
		{
			OnPostGrounder();
		}
	}

	public void method_5(IKSolverLimb limb, int index)
	{
		quaternion_0[index] = transform_0[index].rotation;
		limb.IKPosition = solver.legs[index].IKPosition;
		limb.IKPositionWeight = weight;
	}

	public void method_6()
	{
		if (!(weight <= 0f) && base.enabled)
		{
			for (int i = 0; i < transform_0.Length; i++)
			{
				transform_0[i].rotation = Quaternion.Slerp(Quaternion.identity, solver.legs[i].rotationOffset, weight) * quaternion_0[i];
			}
			vector3_1 = ik.references.pelvis.localPosition;
		}
	}

	public void OnDestroy()
	{
		if (initiated && ik != null)
		{
			IKSolverFABRIK spine = ik.solvers.spine;
			spine.OnPreUpdate = (IKSolver.GDelegate47)Delegate.Remove(spine.OnPreUpdate, new IKSolver.GDelegate47(method_4));
			IKSolverLimb rightFoot = ik.solvers.rightFoot;
			rightFoot.OnPostUpdate = (IKSolver.GDelegate47)Delegate.Remove(rightFoot.OnPostUpdate, new IKSolver.GDelegate47(method_6));
		}
	}
}
