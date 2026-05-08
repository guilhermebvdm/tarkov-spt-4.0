using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[HelpURL("https://www.youtube.com/watch?v=r5jiZnsDH3M")]
[AddComponentMenu("Scripts/RootMotion.FinalIK/Interaction System/Interaction Object")]
public class InteractionObject : MonoBehaviour
{
	[Serializable]
	public class InteractionEvent
	{
		[Tooltip("The time of the event since interaction start.")]
		public float time;

		[Tooltip("If true, the interaction will be paused on this event. The interaction can be resumed by InteractionSystem.ResumeInteraction() or InteractionSystem.ResumeAll;")]
		public bool pause;

		[Tooltip("If true, the object will be parented to the effector bone on this event. Note that picking up like this can be done by only a single effector at a time. If you wish to pick up an object with both hands, see the Interaction PickUp2Handed demo scene.")]
		public bool pickUp;

		[Tooltip("The animations called on this event.")]
		public AnimatorEvent[] animations;

		[Tooltip("The messages sent on this event using GameObject.SendMessage().")]
		public Message[] messages;

		public void Activate(Transform t)
		{
			AnimatorEvent[] array = animations;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Activate(pickUp);
			}
			Message[] array2 = messages;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].Send(t);
			}
		}
	}

	[Serializable]
	public class Message
	{
		[Tooltip("The name of the function called.")]
		public string function;

		[Tooltip("The recipient game object.")]
		public GameObject recipient;

		[NonSerialized]
		public const string Empty = "";

		public void Send(Transform t)
		{
			if (!(recipient == null) && !(function == string.Empty) && !(function == ""))
			{
				recipient.SendMessage(function, t, SendMessageOptions.RequireReceiver);
			}
		}
	}

	[Serializable]
	public class AnimatorEvent
	{
		[Tooltip("The Animator component that will receive the AnimatorEvents.")]
		public Animator animator;

		[Tooltip("The Animation component that will receive the AnimatorEvents (Legacy).")]
		public Animation animation;

		[Tooltip("The name of the animation state.")]
		public string animationState;

		[Tooltip("The crossfading time.")]
		public float crossfadeTime = 0.3f;

		[Tooltip("The layer of the animation state (if using Legacy, the animation state will be forced to this layer).")]
		public int layer;

		[Tooltip("Should the animation always start from 0 normalized time?")]
		public bool resetNormalizedTime;

		[NonSerialized]
		public const string Empty = "";

		public void Activate(bool pickUp)
		{
			if (animator != null)
			{
				if (pickUp)
				{
					animator.applyRootMotion = false;
				}
				method_0(animator);
			}
			if (animation != null)
			{
				method_1(animation);
			}
		}

		public void method_0(Animator animator)
		{
			if (!(animationState == ""))
			{
				if (resetNormalizedTime)
				{
					animator.CrossFade(animationState, crossfadeTime, layer, 0f);
				}
				else
				{
					animator.CrossFade(animationState, crossfadeTime, layer);
				}
			}
		}

		public void method_1(Animation animation)
		{
			if (!(animationState == ""))
			{
				if (resetNormalizedTime)
				{
					animation[animationState].normalizedTime = 0f;
				}
				animation[animationState].layer = layer;
				animation.CrossFade(animationState, crossfadeTime);
			}
		}
	}

	[Serializable]
	public class WeightCurve
	{
		[Serializable]
		public enum Type
		{
			PositionWeight,
			RotationWeight,
			PositionOffsetX,
			PositionOffsetY,
			PositionOffsetZ,
			Pull,
			Reach,
			RotateBoneWeight,
			Push,
			PushParent,
			PoserWeight
		}

		[Tooltip("The type of the curve (InteractionObject.WeightCurve.Type).")]
		public Type type;

		[Tooltip("The weight curve.")]
		public AnimationCurve curve;

		public float GetValue(float timer)
		{
			return curve.Evaluate(timer);
		}
	}

	[Serializable]
	public class Multiplier
	{
		[Tooltip("The curve type to multiply.")]
		public WeightCurve.Type curve;

		[Tooltip("The multiplier of the curve's value.")]
		public float multiplier = 1f;

		[Tooltip("The resulting value will be applied to this channel.")]
		public WeightCurve.Type result;

		public float GetValue(WeightCurve weightCurve, float timer)
		{
			return weightCurve.GetValue(timer) * multiplier;
		}
	}

	[Tooltip("If the Interaction System has a 'Look At' LookAtIK component assigned, will use it to make the character look at the specified Transform. If unassigned, will look at this GameObject.")]
	public Transform otherLookAtTarget;

	[Tooltip("The root Transform of the InteractionTargets. If null, will use this GameObject. GetComponentsInChildren<InteractionTarget>() will be used at initiation to find all InteractionTargets associated with this InteractionObject.")]
	public Transform otherTargetsRoot;

	[Tooltip("If assigned, all PositionOffset channels will be applied in the rotation space of this Transform. If not, they will be in the rotation space of the character.")]
	public Transform positionOffsetSpace;

	public WeightCurve[] weightCurves;

	public Multiplier[] multipliers;

	public InteractionEvent[] events;

	private InteractionTarget[] targets = new InteractionTarget[0];

	public float length { get; set; }

	public InteractionSystem lastUsedInteractionSystem { get; set; }

	public Transform lookAtTarget
	{
		get
		{
			if (otherLookAtTarget != null)
			{
				return otherLookAtTarget;
			}
			return base.transform;
		}
	}

	public Transform targetsRoot
	{
		get
		{
			if (otherTargetsRoot != null)
			{
				return otherTargetsRoot;
			}
			return base.transform;
		}
	}

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

	public void Initiate()
	{
		for (int i = 0; i < weightCurves.Length; i++)
		{
			if (weightCurves[i].curve.length > 0)
			{
				float time = weightCurves[i].curve.keys[weightCurves[i].curve.length - 1].time;
				length = Mathf.Clamp(length, time, length);
			}
		}
		for (int j = 0; j < events.Length; j++)
		{
			length = Mathf.Clamp(length, events[j].time, length);
		}
		targets = targetsRoot.GetComponentsInChildren<InteractionTarget>();
	}

	public InteractionTarget GetTarget(FullBodyBipedEffector effectorType, InteractionSystem interactionSystem)
	{
		InteractionTarget[] array;
		int num;
		if (!(interactionSystem.tag == string.Empty) && !(interactionSystem.tag == ""))
		{
			array = targets;
			num = 0;
			InteractionTarget interactionTarget;
			while (true)
			{
				if (num < array.Length)
				{
					interactionTarget = array[num];
					if (interactionTarget.effectorType == effectorType && interactionTarget.tag == interactionSystem.tag)
					{
						break;
					}
					num++;
					continue;
				}
				return null;
			}
			return interactionTarget;
		}
		array = targets;
		num = 0;
		InteractionTarget interactionTarget2;
		while (true)
		{
			if (num < array.Length)
			{
				interactionTarget2 = array[num];
				if (interactionTarget2.effectorType == effectorType)
				{
					break;
				}
				num++;
				continue;
			}
			return null;
		}
		return interactionTarget2;
	}

	public bool CurveUsed(WeightCurve.Type type)
	{
		WeightCurve[] array = weightCurves;
		int num = 0;
		while (true)
		{
			if (num < array.Length)
			{
				if (array[num].type == type)
				{
					break;
				}
				num++;
				continue;
			}
			Multiplier[] array2 = multipliers;
			num = 0;
			while (true)
			{
				if (num < array2.Length)
				{
					if (array2[num].result == type)
					{
						break;
					}
					num++;
					continue;
				}
				return false;
			}
			return true;
		}
		return true;
	}

	public InteractionTarget[] GetTargets()
	{
		return targets;
	}

	public Transform GetTarget(FullBodyBipedEffector effectorType, string tag)
	{
		if (!(tag == string.Empty) && !(tag == ""))
		{
			int num = 0;
			while (true)
			{
				if (num < targets.Length)
				{
					if (targets[num].effectorType == effectorType && targets[num].tag == tag)
					{
						break;
					}
					num++;
					continue;
				}
				return base.transform;
			}
			return targets[num].transform;
		}
		return method_7(effectorType);
	}

	public void OnStartInteraction(InteractionSystem interactionSystem)
	{
		lastUsedInteractionSystem = interactionSystem;
	}

	public void Apply(IKSolverFullBodyBiped solver, FullBodyBipedEffector effector, InteractionTarget target, float timer, float weight)
	{
		for (int i = 0; i < weightCurves.Length; i++)
		{
			float num = ((target == null) ? 1f : target.GetValue(weightCurves[i].type));
			method_6(solver, effector, weightCurves[i].type, weightCurves[i].GetValue(timer), weight * num);
		}
		for (int j = 0; j < multipliers.Length; j++)
		{
			if (multipliers[j].curve == multipliers[j].result && !GClass1465.logged)
			{
				GClass1465.Log("InteractionObject Multiplier 'Curve' " + multipliers[j].curve.ToString() + "and 'Result' are the same.", base.transform);
			}
			int num2 = method_8(multipliers[j].curve);
			if (num2 != -1)
			{
				float num3 = ((target == null) ? 1f : target.GetValue(multipliers[j].result));
				method_6(solver, effector, multipliers[j].result, multipliers[j].GetValue(weightCurves[num2], timer), weight * num3);
			}
			else if (!GClass1465.logged)
			{
				GClass1465.Log("InteractionObject Multiplier curve " + multipliers[j].curve.ToString() + "does not exist.", base.transform);
			}
		}
	}

	public float GetValue(WeightCurve.Type weightCurveType, InteractionTarget target, float timer)
	{
		int num = method_8(weightCurveType);
		if (num != -1)
		{
			float num2 = ((target == null) ? 1f : target.GetValue(weightCurveType));
			return weightCurves[num].GetValue(timer) * num2;
		}
		int num3 = 0;
		int num4;
		while (true)
		{
			if (num3 < multipliers.Length)
			{
				if (multipliers[num3].result == weightCurveType)
				{
					num4 = method_8(multipliers[num3].curve);
					if (num4 != -1)
					{
						break;
					}
				}
				num3++;
				continue;
			}
			return 0f;
		}
		float num5 = ((target == null) ? 1f : target.GetValue(multipliers[num3].result));
		return multipliers[num3].GetValue(weightCurves[num4], timer) * num5;
	}

	public void Awake()
	{
		Initiate();
	}

	public void method_6(IKSolverFullBodyBiped solver, FullBodyBipedEffector effector, WeightCurve.Type type, float value, float weight)
	{
		switch (type)
		{
		case WeightCurve.Type.PositionWeight:
			solver.GetEffector(effector).positionWeight = Mathf.Lerp(solver.GetEffector(effector).positionWeight, value, weight);
			break;
		case WeightCurve.Type.RotationWeight:
			solver.GetEffector(effector).rotationWeight = Mathf.Lerp(solver.GetEffector(effector).rotationWeight, value, weight);
			break;
		case WeightCurve.Type.PositionOffsetX:
			solver.GetEffector(effector).position += ((positionOffsetSpace != null) ? positionOffsetSpace.rotation : solver.GetRoot().rotation) * Vector3.right * value * weight;
			break;
		case WeightCurve.Type.PositionOffsetY:
			solver.GetEffector(effector).position += ((positionOffsetSpace != null) ? positionOffsetSpace.rotation : solver.GetRoot().rotation) * Vector3.up * value * weight;
			break;
		case WeightCurve.Type.PositionOffsetZ:
			solver.GetEffector(effector).position += ((positionOffsetSpace != null) ? positionOffsetSpace.rotation : solver.GetRoot().rotation) * Vector3.forward * value * weight;
			break;
		case WeightCurve.Type.Pull:
			solver.GetChain(effector).pull = Mathf.Lerp(solver.GetChain(effector).pull, value, weight);
			break;
		case WeightCurve.Type.Reach:
			solver.GetChain(effector).reach = Mathf.Lerp(solver.GetChain(effector).reach, value, weight);
			break;
		case WeightCurve.Type.RotateBoneWeight:
			break;
		case WeightCurve.Type.Push:
			solver.GetChain(effector).push = Mathf.Lerp(solver.GetChain(effector).push, value, weight);
			break;
		case WeightCurve.Type.PushParent:
			solver.GetChain(effector).pushParent = Mathf.Lerp(solver.GetChain(effector).pushParent, value, weight);
			break;
		}
	}

	public Transform method_7(FullBodyBipedEffector effectorType)
	{
		int num = 0;
		while (true)
		{
			if (num < targets.Length)
			{
				if (targets[num].effectorType == effectorType)
				{
					break;
				}
				num++;
				continue;
			}
			return base.transform;
		}
		return targets[num].transform;
	}

	public int method_8(WeightCurve.Type weightCurveType)
	{
		int num = 0;
		while (true)
		{
			if (num < weightCurves.Length)
			{
				if (weightCurves[num].type == weightCurveType)
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

	public int method_9(WeightCurve.Type weightCurveType)
	{
		int num = 0;
		while (true)
		{
			if (num < multipliers.Length)
			{
				if (multipliers[num].result == weightCurveType)
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

	[ContextMenu("User Manual")]
	public void method_10()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/page10.html");
	}

	[ContextMenu("Scrpt Reference")]
	public void method_11()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_interaction_object.html");
	}
}
