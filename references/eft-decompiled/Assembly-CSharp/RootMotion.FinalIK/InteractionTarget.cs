using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[HelpURL("https://www.youtube.com/watch?v=r5jiZnsDH3M")]
[AddComponentMenu("Scripts/RootMotion.FinalIK/Interaction System/Interaction Target")]
public class InteractionTarget : MonoBehaviour
{
	[Serializable]
	public class Multiplier
	{
		[Tooltip("The curve type (InteractionObject.WeightCurve.Type).")]
		public InteractionObject.WeightCurve.Type curve;

		[Tooltip("Multiplier of the curve's value.")]
		public float multiplier;
	}

	[Tooltip("The type of the FBBIK effector.")]
	public FullBodyBipedEffector effectorType;

	[Tooltip("InteractionObject weight curve multipliers for this effector target.")]
	public Multiplier[] multipliers;

	[Tooltip("The interaction speed multiplier for this effector. This can be used to make interactions faster/slower for specific effectors.")]
	public float interactionSpeedMlp = 1f;

	[Tooltip("The pivot to twist/swing this interaction target about. For symmetric objects that can be interacted with from a certain angular range.")]
	public Transform pivot;

	[Tooltip("The axis of twisting the interaction target (blue line).")]
	public Vector3 twistAxis = Vector3.up;

	[Tooltip("The weight of twisting the interaction target towards the effector bone in the start of the interaction.")]
	public float twistWeight = 1f;

	[Tooltip("The weight of swinging the interaction target towards the effector bone in the start of the interaction. Swing is defined as a 3-DOF rotation around any axis, while twist is only around the twist axis.")]
	public float swingWeight;

	[Tooltip("If true, will twist/swing around the pivot only once at the start of the interaction. If false, will continue rotating throuout the whole interaction.")]
	public bool rotateOnce = true;

	private Quaternion quaternion_0;

	private Transform transform_0;

	[ContextMenu("TUTORIAL VIDEO (PART 1: BASICS)")]
	public void method_0()
	{
		Application.OpenURL("https://www.youtube.com/watch?v=r5jiZnsDH3M");
	}

	[ContextMenu("TUTORIAL VIDEO (PART 2: PICKING UP...)")]
	public void method_1()
	{
		Application.OpenURL("https://www.youtube.com/watch?v=eP9-zycoHLk");
	}

	[ContextMenu("TUTORIAL VIDEO (PART 3: ANIMATION)")]
	public void method_2()
	{
		Application.OpenURL("https://www.youtube.com/watch?v=sQfB2RcT1T4&index=14&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6");
	}

	[ContextMenu("TUTORIAL VIDEO (PART 4: TRIGGERS)")]
	public void method_3()
	{
		Application.OpenURL("https://www.youtube.com/watch?v=-TDZpNjt2mk&index=15&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6");
	}

	[ContextMenu("Support Group")]
	public void method_4()
	{
		Application.OpenURL("https://groups.google.com/forum/#!forum/final-ik");
	}

	[ContextMenu("Asset Store Thread")]
	public void method_5()
	{
		Application.OpenURL("http://forum.unity3d.com/threads/final-ik-full-body-ik-aim-look-at-fabrik-ccd-ik-1-0-released.222685/");
	}

	public float GetValue(InteractionObject.WeightCurve.Type curveType)
	{
		int num = 0;
		while (true)
		{
			if (num < multipliers.Length)
			{
				if (multipliers[num].curve == curveType)
				{
					break;
				}
				num++;
				continue;
			}
			return 1f;
		}
		return multipliers[num].multiplier;
	}

	public void ResetRotation()
	{
		if (pivot != null)
		{
			pivot.localRotation = quaternion_0;
		}
	}

	public void RotateTo(Vector3 position)
	{
		if (!(pivot == null))
		{
			if (pivot != transform_0)
			{
				quaternion_0 = pivot.localRotation;
				transform_0 = pivot;
			}
			pivot.localRotation = quaternion_0;
			if (twistWeight > 0f)
			{
				Vector3 tangent = base.transform.position - pivot.position;
				Vector3 vector = pivot.rotation * twistAxis;
				Vector3 normal = vector;
				Vector3.OrthoNormalize(ref normal, ref tangent);
				normal = vector;
				Vector3 tangent2 = position - pivot.position;
				Vector3.OrthoNormalize(ref normal, ref tangent2);
				Quaternion b = GClass1463.FromToAroundAxis(tangent, tangent2, vector);
				pivot.rotation = Quaternion.Lerp(Quaternion.identity, b, twistWeight) * pivot.rotation;
			}
			if (swingWeight > 0f)
			{
				Quaternion b2 = Quaternion.FromToRotation(base.transform.position - pivot.position, position - pivot.position);
				pivot.rotation = Quaternion.Lerp(Quaternion.identity, b2, swingWeight) * pivot.rotation;
			}
		}
	}

	[ContextMenu("User Manual")]
	public void method_6()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/page10.html");
	}

	[ContextMenu("Scrpt Reference")]
	public void method_7()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_interaction_target.html");
	}
}
