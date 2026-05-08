using System;
using System.Collections.Generic;
using AnimationEventSystem;
using EFT;
using JetBrains.Annotations;

public abstract class ObjectInHandsAnimator
{
	public enum PlayerState
	{
		None,
		Idle,
		Jump,
		Sprint,
		Prone
	}

	[NonSerialized]
	public bool InventoryLocked;

	public const int LACTION_DROP_BACKPACK_INDEX = 300;

	public const int LACTION_PICKUP_INDEX = 1;

	[NonSerialized]
	public const string LACTIONS_LAYER_NAME = "LActions";

	[NonSerialized]
	public Func<IAnimator> AnimatorGetter;

	[NonSerialized]
	public int AnimatorLayersCount;

	[NonSerialized]
	public List<IActorEvents> EventsConsumers = new List<IActorEvents>(2);

	[NonSerialized]
	public HashSet<int> ExistedParameters = new HashSet<int>();

	public int LACTIONS_LAYER_INDEX = -1;

	public IAnimator Animator => AnimatorGetter();

	public bool IsInInteraction => WeaponAnimationSpeedControllerClass.GetBoolUseLeftHand(Animator);

	public bool IsInInventory => WeaponAnimationSpeedControllerClass.GetBoolInventory(Animator);

	public bool IsInventoryLocked => InventoryLocked;

	public virtual Action FireEventAction => null;

	public void AnimatorEventHandler(int functionNameHash, AnimationEventParameter parameter)
	{
		GClass756.AnimatorEventHandler(EventsConsumers, functionNameHash, parameter);
	}

	public void Gesture(EInteraction index)
	{
		WeaponAnimationSpeedControllerClass.TriggerGesture(Animator);
		WeaponAnimationSpeedControllerClass.SetUseLeftHand(Animator, useLeftHand: true);
		WeaponAnimationSpeedControllerClass.SetLActionIndex(Animator, 0);
		WeaponAnimationSpeedControllerClass.SetGestureIndex(Animator, (int)(index - 1));
	}

	public void ShowCompass(bool show)
	{
		if (show)
		{
			WeaponAnimationSpeedControllerClass.SetUseLeftHand(Animator, useLeftHand: true);
		}
		WeaponAnimationSpeedControllerClass.SetLActionIndex(Animator, show ? 404 : 0);
	}

	public void ShowRadioTransmitter(bool show)
	{
		if (show)
		{
			WeaponAnimationSpeedControllerClass.SetUseLeftHand(Animator, useLeftHand: true);
		}
		WeaponAnimationSpeedControllerClass.SetLActionIndex(Animator, show ? 404 : 0);
	}

	public void AddEventsConsumer([NotNull] IActorEvents eventsConsumer)
	{
		EventsConsumers.Add(eventsConsumer);
	}

	public void RemoveEventsConsumer([NotNull] IActorEvents eventsConsumer)
	{
		EventsConsumers.Remove(eventsConsumer);
	}

	public virtual void SetAnimatorGetter(Func<IAnimator> getter)
	{
		using (GClass4062.BeginSampleWithToken("ObjectInHandsAnimator:77.SetAnimatorGetter", "SetAnimatorGetter"))
		{
			AnimatorGetter = getter;
			AnimatorLayersCount = Animator.layerCount;
			ExistedParameters.Clear();
			for (int i = 0; i < Animator.parameterCount; i++)
			{
				AnimatorParameterInfo parameter = Animator.GetParameter(i);
				ExistedParameters.Add(parameter.nameHash);
			}
		}
	}

	public bool HasParameter(int parameterHash)
	{
		return ExistedParameters.Contains(parameterHash);
	}

	public void SetPlayerState(PlayerState state)
	{
		if (HasParameter(WeaponAnimationSpeedControllerClass.INT_PLAYERSTATE))
		{
			WeaponAnimationSpeedControllerClass.SetPlayerState(Animator, (int)state);
		}
	}

	public void SetPatrol(bool b)
	{
		WeaponAnimationSpeedControllerClass.SetPatrol(Animator, b);
	}

	public void SkipTime(float t)
	{
		Animator.Update(t);
	}

	public bool CurrentStateNameIs(int layer, string sname)
	{
		return Animator.GetCurrentAnimatorStateInfo(layer).IsName(sname);
	}

	public void SetLayerWeight(int layerid, int p2)
	{
		Animator.SetLayerWeight(layerid, p2);
	}

	public void SetLayerWeight(int layerId, float weight)
	{
		Animator.SetLayerWeight(layerId, weight);
	}

	public void SetLayerWeight(string layerName, int p2)
	{
		Animator.SetLayerWeight(Animator.GetLayerIndex(layerName), p2);
	}

	public void ResetLeftHand()
	{
		WeaponAnimationSpeedControllerClass.SetUseLeftHand(Animator, useLeftHand: false);
	}

	public void SetInventory(bool open)
	{
		if (!InventoryLocked)
		{
			WeaponAnimationSpeedControllerClass.SetInventory(Animator, open);
			ResetLeftHand();
		}
	}

	public void LockInventory()
	{
		InventoryLocked = true;
	}

	public void UnlockInventory()
	{
		InventoryLocked = false;
	}

	public void SetActiveParam(bool active, bool resetLeftHand = true)
	{
		WeaponAnimationSpeedControllerClass.SetActive(Animator, active);
		if (resetLeftHand)
		{
			ResetLeftHand();
		}
	}

	public void SetFastHide(bool fastHide)
	{
		WeaponAnimationSpeedControllerClass.SetFastHide(Animator, fastHide);
	}

	public void SetQuickFire(bool quickFire)
	{
		WeaponAnimationSpeedControllerClass.SetQuickFire(Animator, quickFire);
	}

	public void SetPointOfViewOnSpawn(EPointOfView pointOfView)
	{
		WeaponAnimationSpeedControllerClass.SetThirdPerson(Animator, (pointOfView == EPointOfView.ThirdPerson) ? 1 : 0);
	}

	public void SetAnimationSpeed(float speed)
	{
		Animator.speed = speed;
	}

	public void SetLActionIndex(int actionIndex)
	{
		WeaponAnimationSpeedControllerClass.SetLActionIndex(Animator, actionIndex);
	}

	public void SetDeflected(bool b)
	{
		WeaponAnimationSpeedControllerClass.SetDeflected(Animator, b ? 1f : 0f);
	}

	public void SetMeleeSpeed(float speed)
	{
		WeaponAnimationSpeedControllerClass.SetSwingSpeed(Animator, speed);
	}

	public float GetAnimatorParameter(int hash)
	{
		return Animator.GetFloat(hash);
	}

	public float GetLeftHandProgress()
	{
		return Animator.GetFloat(WeaponAnimationSpeedControllerClass.FLOAT_LEFTHANDPROGRESS);
	}

	public float GetLayerWeight(int layerIndex)
	{
		if (layerIndex >= 0 && layerIndex < AnimatorLayersCount)
		{
			return Animator.GetLayerWeight(layerIndex);
		}
		return 0f;
	}

	public ObjectInHandsAnimator()
	{
	}
}
