using System;
using System.Collections.Generic;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class InteractionEffector
{
	[NonSerialized]
	public Poser Poser;

	[NonSerialized]
	public IKEffector Effector;

	[NonSerialized]
	public float Timer;

	[NonSerialized]
	public float Length;

	[NonSerialized]
	public float Weight;

	[NonSerialized]
	public float FadeInSpeed;

	[NonSerialized]
	public float DefaultPositionWeight;

	[NonSerialized]
	public float DefaultRotationWeight;

	[NonSerialized]
	public float DefaultPull;

	[NonSerialized]
	public float DefaultReach;

	[NonSerialized]
	public float DefaultPush;

	[NonSerialized]
	public float DefaultPushParent;

	[NonSerialized]
	public float ResetTimer;

	[NonSerialized]
	public bool PositionWeightUsed;

	[NonSerialized]
	public bool RotationWeightUsed;

	[NonSerialized]
	public bool PullUsed;

	[NonSerialized]
	public bool ReachUsed;

	[NonSerialized]
	public bool PushUsed;

	[NonSerialized]
	public bool PushParentUsed;

	[NonSerialized]
	public bool PickedUp;

	[NonSerialized]
	public bool Defaults;

	[NonSerialized]
	public bool PickUpOnPostFBBIK;

	[NonSerialized]
	public Vector3 PickUpPosition;

	[NonSerialized]
	public Vector3 PausePositionRelative;

	[NonSerialized]
	public Quaternion PickUpRotation;

	[NonSerialized]
	public Quaternion PauseRotationRelative;

	[NonSerialized]
	public InteractionTarget InteractionTarget;

	[NonSerialized]
	public Transform Target;

	[NonSerialized]
	public List<bool> Triggered = new List<bool>();

	[NonSerialized]
	public InteractionSystem InteractionSystem;

	[NonSerialized]
	public bool Started;

	[field: NonSerialized]
	public FullBodyBipedEffector effectorType { get; set; }

	[field: NonSerialized]
	public bool isPaused { get; set; }

	[field: NonSerialized]
	public InteractionObject interactionObject { get; set; }

	public bool inInteraction => interactionObject != null;

	public float progress
	{
		get
		{
			if (!inInteraction)
			{
				return 0f;
			}
			if (Length == 0f)
			{
				return 0f;
			}
			return Timer / Length;
		}
	}

	public InteractionEffector(FullBodyBipedEffector effectorType)
	{
		this.effectorType = effectorType;
	}

	public void Initiate(InteractionSystem interactionSystem)
	{
		InteractionSystem = interactionSystem;
		if (Effector == null)
		{
			Effector = interactionSystem.ik.solver.GetEffector(effectorType);
			Poser = Effector.bone.GetComponent<Poser>();
		}
		method_0();
	}

	public void method_0()
	{
		DefaultPositionWeight = InteractionSystem.ik.solver.GetEffector(effectorType).positionWeight;
		DefaultRotationWeight = InteractionSystem.ik.solver.GetEffector(effectorType).rotationWeight;
		DefaultPull = InteractionSystem.ik.solver.GetChain(effectorType).pull;
		DefaultReach = InteractionSystem.ik.solver.GetChain(effectorType).reach;
		DefaultPush = InteractionSystem.ik.solver.GetChain(effectorType).push;
		DefaultPushParent = InteractionSystem.ik.solver.GetChain(effectorType).pushParent;
	}

	public bool ResetToDefaults(float speed)
	{
		if (inInteraction)
		{
			return false;
		}
		if (isPaused)
		{
			return false;
		}
		if (Defaults)
		{
			return false;
		}
		ResetTimer = Mathf.Clamp(ResetTimer -= Time.deltaTime * speed, 0f, 1f);
		if (Effector.isEndEffector)
		{
			if (PullUsed)
			{
				InteractionSystem.ik.solver.GetChain(effectorType).pull = Mathf.Lerp(DefaultPull, InteractionSystem.ik.solver.GetChain(effectorType).pull, ResetTimer);
			}
			if (ReachUsed)
			{
				InteractionSystem.ik.solver.GetChain(effectorType).reach = Mathf.Lerp(DefaultReach, InteractionSystem.ik.solver.GetChain(effectorType).reach, ResetTimer);
			}
			if (PushUsed)
			{
				InteractionSystem.ik.solver.GetChain(effectorType).push = Mathf.Lerp(DefaultPush, InteractionSystem.ik.solver.GetChain(effectorType).push, ResetTimer);
			}
			if (PushParentUsed)
			{
				InteractionSystem.ik.solver.GetChain(effectorType).pushParent = Mathf.Lerp(DefaultPushParent, InteractionSystem.ik.solver.GetChain(effectorType).pushParent, ResetTimer);
			}
		}
		if (PositionWeightUsed)
		{
			Effector.positionWeight = Mathf.Lerp(DefaultPositionWeight, Effector.positionWeight, ResetTimer);
		}
		if (RotationWeightUsed)
		{
			Effector.rotationWeight = Mathf.Lerp(DefaultRotationWeight, Effector.rotationWeight, ResetTimer);
		}
		if (ResetTimer <= 0f)
		{
			PullUsed = false;
			ReachUsed = false;
			PushUsed = false;
			PushParentUsed = false;
			PositionWeightUsed = false;
			RotationWeightUsed = false;
			Defaults = true;
		}
		return true;
	}

	public bool Pause()
	{
		if (!inInteraction)
		{
			return false;
		}
		isPaused = true;
		PausePositionRelative = Target.InverseTransformPoint(Effector.position);
		PauseRotationRelative = Quaternion.Inverse(Target.rotation) * Effector.rotation;
		if (InteractionSystem.OnInteractionPause != null)
		{
			InteractionSystem.OnInteractionPause(effectorType, interactionObject);
		}
		return true;
	}

	public bool Resume()
	{
		if (!inInteraction)
		{
			return false;
		}
		isPaused = false;
		if (InteractionSystem.OnInteractionResume != null)
		{
			InteractionSystem.OnInteractionResume(effectorType, interactionObject);
		}
		return true;
	}

	public bool Start(InteractionObject interactionObject, string tag, float fadeInTime, bool interrupt)
	{
		if (!inInteraction)
		{
			Effector.position = Effector.bone.position;
			Effector.rotation = Effector.bone.rotation;
		}
		else if (!interrupt)
		{
			return false;
		}
		Target = interactionObject.GetTarget(effectorType, tag);
		if (Target == null)
		{
			return false;
		}
		InteractionTarget = Target.GetComponent<InteractionTarget>();
		this.interactionObject = interactionObject;
		if (InteractionSystem.OnInteractionStart != null)
		{
			InteractionSystem.OnInteractionStart(effectorType, interactionObject);
		}
		interactionObject.OnStartInteraction(InteractionSystem);
		Triggered.Clear();
		for (int i = 0; i < interactionObject.events.Length; i++)
		{
			Triggered.Add(item: false);
		}
		if (Poser != null)
		{
			if (Poser.poseRoot == null)
			{
				Poser.weight = 0f;
			}
			if (InteractionTarget != null)
			{
				Poser.poseRoot = Target.transform;
			}
			else
			{
				Poser.poseRoot = null;
			}
			Poser.AutoMapping();
		}
		PositionWeightUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.PositionWeight);
		RotationWeightUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.RotationWeight);
		PullUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.Pull);
		ReachUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.Reach);
		PushUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.Push);
		PushParentUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.PushParent);
		method_0();
		Timer = 0f;
		Weight = 0f;
		FadeInSpeed = ((fadeInTime > 0f) ? (1f / fadeInTime) : 1000f);
		Length = interactionObject.length;
		isPaused = false;
		PickedUp = false;
		PickUpPosition = Vector3.zero;
		PickUpRotation = Quaternion.identity;
		if (InteractionTarget != null)
		{
			InteractionTarget.RotateTo(Effector.bone.position);
		}
		Started = true;
		return true;
	}

	public void Update(Transform root, float speed)
	{
		if (!inInteraction)
		{
			if (Started)
			{
				isPaused = false;
				PickedUp = false;
				Defaults = false;
				ResetTimer = 1f;
				Started = false;
			}
			return;
		}
		if (InteractionTarget != null && !InteractionTarget.rotateOnce)
		{
			InteractionTarget.RotateTo(Effector.bone.position);
		}
		if (isPaused)
		{
			Effector.position = Target.TransformPoint(PausePositionRelative);
			Effector.rotation = Target.rotation * PauseRotationRelative;
			interactionObject.Apply(InteractionSystem.ik.solver, effectorType, InteractionTarget, Timer, Weight);
			return;
		}
		Timer += Time.deltaTime * speed * ((InteractionTarget != null) ? InteractionTarget.interactionSpeedMlp : 1f);
		Weight = Mathf.Clamp(Weight + Time.deltaTime * FadeInSpeed * speed, 0f, 1f);
		bool pickUp = false;
		bool pause = false;
		method_1(checkTime: true, out pickUp, out pause);
		Vector3 b = (PickedUp ? PickUpPosition : Target.position);
		Quaternion b2 = (PickedUp ? PickUpRotation : Target.rotation);
		Effector.position = Vector3.Lerp(Effector.bone.position, b, Weight);
		Effector.rotation = Quaternion.Lerp(Effector.bone.rotation, b2, Weight);
		interactionObject.Apply(InteractionSystem.ik.solver, effectorType, InteractionTarget, Timer, Weight);
		if (pickUp)
		{
			method_2(root);
		}
		if (pause)
		{
			Pause();
		}
		float value = interactionObject.GetValue(InteractionObject.WeightCurve.Type.PoserWeight, InteractionTarget, Timer);
		if (Poser != null)
		{
			Poser.weight = Mathf.Lerp(Poser.weight, value, Weight);
		}
		else if (value > 0f)
		{
			GClass1465.Log("InteractionObject " + interactionObject.name + " has a curve/multipler for Poser Weight, but the bone of effector " + effectorType.ToString() + " has no HandPoser/GenericPoser attached.", Effector.bone);
		}
		if (Timer >= Length)
		{
			Stop();
		}
	}

	public void method_1(bool checkTime, out bool pickUp, out bool pause)
	{
		pickUp = false;
		pause = false;
		for (int i = 0; i < Triggered.Count; i++)
		{
			if (Triggered[i] || (checkTime && !(interactionObject.events[i].time < Timer)))
			{
				continue;
			}
			interactionObject.events[i].Activate(Effector.bone);
			if (interactionObject.events[i].pickUp)
			{
				if (Timer >= interactionObject.events[i].time)
				{
					Timer = interactionObject.events[i].time;
				}
				pickUp = true;
			}
			if (interactionObject.events[i].pause)
			{
				if (Timer >= interactionObject.events[i].time)
				{
					Timer = interactionObject.events[i].time;
				}
				pause = true;
			}
			if (InteractionSystem.OnInteractionEvent != null)
			{
				InteractionSystem.OnInteractionEvent(effectorType, interactionObject, interactionObject.events[i]);
			}
			Triggered[i] = true;
		}
	}

	public void method_2(Transform root)
	{
		PickUpPosition = Effector.position;
		PickUpRotation = Effector.rotation;
		PickUpOnPostFBBIK = true;
		PickedUp = true;
		Rigidbody component = interactionObject.targetsRoot.GetComponent<Rigidbody>();
		if (component != null)
		{
			if (!component.isKinematic)
			{
				component.isKinematic = true;
			}
			if (root.GetComponent<Collider>() != null)
			{
				Collider[] componentsInChildren = interactionObject.targetsRoot.GetComponentsInChildren<Collider>();
				foreach (Collider collider in componentsInChildren)
				{
					if (!collider.isTrigger)
					{
						Physics.IgnoreCollision(root.GetComponent<Collider>(), collider);
					}
				}
			}
		}
		if (InteractionSystem.OnInteractionPickUp != null)
		{
			InteractionSystem.OnInteractionPickUp(effectorType, interactionObject);
		}
	}

	public bool Stop()
	{
		if (!inInteraction)
		{
			return false;
		}
		bool pickUp = false;
		bool pause = false;
		method_1(checkTime: false, out pickUp, out pause);
		if (InteractionSystem.OnInteractionStop != null)
		{
			InteractionSystem.OnInteractionStop(effectorType, interactionObject);
		}
		if (InteractionTarget != null)
		{
			InteractionTarget.ResetRotation();
		}
		interactionObject = null;
		Weight = 0f;
		Timer = 0f;
		isPaused = false;
		Target = null;
		Defaults = false;
		ResetTimer = 1f;
		if (Poser != null && !PickedUp)
		{
			Poser.weight = 0f;
		}
		PickedUp = false;
		Started = false;
		return true;
	}

	public void OnPostFBBIK()
	{
		if (inInteraction)
		{
			float num = interactionObject.GetValue(InteractionObject.WeightCurve.Type.RotateBoneWeight, InteractionTarget, Timer) * Weight;
			if (num > 0f)
			{
				Quaternion b = (PickedUp ? PickUpRotation : Effector.rotation);
				Quaternion quaternion = Quaternion.Slerp(Effector.bone.rotation, b, num * num);
				Effector.bone.localRotation = Quaternion.Inverse(Effector.bone.parent.rotation) * quaternion;
			}
			if (PickUpOnPostFBBIK)
			{
				Vector3 position = Effector.bone.position;
				Effector.bone.position = PickUpPosition;
				interactionObject.targetsRoot.parent = Effector.bone;
				Effector.bone.position = position;
				PickUpOnPostFBBIK = false;
			}
		}
	}
}
