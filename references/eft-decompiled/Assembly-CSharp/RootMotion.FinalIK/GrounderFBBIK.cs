using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[HelpURL("https://www.youtube.com/watch?v=9MiZiaJorws&index=6&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6")]
[AddComponentMenu("Scripts/RootMotion.FinalIK/Grounder/Grounder Full Body Biped")]
public class GrounderFBBIK : Grounder
{
	[Serializable]
	public class SpineEffector
	{
		[Tooltip("The type of the effector.")]
		public FullBodyBipedEffector effectorType;

		[Tooltip("The weight of horizontal bend offset towards the slope.")]
		public float horizontalWeight = 1f;

		[Tooltip("The vertical bend offset weight.")]
		public float verticalWeight;
	}

	[Tooltip("Reference to the FBBIK componet.")]
	public FullBodyBipedIK ik;

	[Tooltip("The amount of spine bending towards upward slopes.")]
	public float spineBend = 2f;

	[Tooltip("The interpolation speed of spine bending.")]
	public float spineSpeed = 3f;

	public SpineEffector[] spine = new SpineEffector[0];

	private Transform[] transform_0 = new Transform[2];

	private Vector3 vector3_0;

	private bool bool_0;

	[ContextMenu("TUTORIAL VIDEO")]
	public void method_2()
	{
		Application.OpenURL("https://www.youtube.com/watch?v=9MiZiaJorws&index=6&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6");
	}

	[ContextMenu("User Manual")]
	public override void OpenUserManual()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/page11.html");
	}

	[ContextMenu("Scrpt Reference")]
	public override void OpenScriptReference()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_grounder_f_b_b_i_k.html");
	}

	public override void ResetPosition()
	{
		solver.Reset();
		vector3_0 = Vector3.zero;
	}

	public bool method_3()
	{
		if (ik == null)
		{
			return false;
		}
		if (!ik.solver.initiated)
		{
			return false;
		}
		return true;
	}

	public void Update()
	{
	}

	public void FixedUpdate()
	{
	}

	public void LateUpdate()
	{
	}

	public void Initiate()
	{
		ik.solver.leftLegMapping.maintainRotationWeight = 1f;
		ik.solver.rightLegMapping.maintainRotationWeight = 1f;
		transform_0 = new Transform[2];
		transform_0[0] = ik.solver.leftFootEffector.bone;
		transform_0[1] = ik.solver.rightFootEffector.bone;
		IKSolverFullBodyBiped iKSolverFullBodyBiped = ik.solver;
		iKSolverFullBodyBiped.OnPreUpdate = (IKSolver.GDelegate47)Delegate.Combine(iKSolverFullBodyBiped.OnPreUpdate, new IKSolver.GDelegate47(method_4));
		solver.Initiate(ik.references.root, transform_0);
		initiated = true;
	}

	public void method_4()
	{
		if (weight <= 0f)
		{
			return;
		}
		if (OnPreGrounder != null)
		{
			OnPreGrounder();
		}
		solver.Update();
		ik.references.pelvis.position += solver.pelvis.IKOffset * weight;
		method_5(ik.solver.leftFootEffector, solver.legs[0]);
		method_5(ik.solver.rightFootEffector, solver.legs[1]);
		if (spineBend != 0f)
		{
			spineSpeed = Mathf.Clamp(spineSpeed, 0f, spineSpeed);
			Vector3 vector = GetSpineOffsetTarget() * weight;
			vector3_0 = Vector3.Lerp(vector3_0, vector * spineBend, Time.deltaTime * spineSpeed);
			Vector3 vector2 = ik.references.root.up * vector3_0.magnitude;
			for (int i = 0; i < spine.Length; i++)
			{
				ik.solver.GetEffector(spine[i].effectorType).positionOffset += vector3_0 * spine[i].horizontalWeight + vector2 * spine[i].verticalWeight;
			}
		}
		if (OnPostGrounder != null)
		{
			OnPostGrounder();
		}
	}

	public void method_5(IKEffector effector, Grounding.Leg leg)
	{
		effector.positionOffset += (leg.IKPosition - effector.bone.position) * weight;
		effector.bone.rotation = Quaternion.Slerp(Quaternion.identity, leg.rotationOffset, weight) * effector.bone.rotation;
	}

	public void OnDrawGizmosSelected()
	{
		if (ik == null)
		{
			ik = GetComponent<FullBodyBipedIK>();
		}
		if (ik == null)
		{
			ik = GetComponentInParent<FullBodyBipedIK>();
		}
		if (ik == null)
		{
			ik = GetComponentInChildren<FullBodyBipedIK>();
		}
	}

	public void OnDestroy()
	{
		if (initiated && ik != null)
		{
			IKSolverFullBodyBiped iKSolverFullBodyBiped = ik.solver;
			iKSolverFullBodyBiped.OnPreUpdate = (IKSolver.GDelegate47)Delegate.Remove(iKSolverFullBodyBiped.OnPreUpdate, new IKSolver.GDelegate47(method_4));
		}
	}
}
