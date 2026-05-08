using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public class GClass1446 : IAnimator, IAnimatorNotificator
{
	[NonSerialized]
	public static GClass713 Gclass713_0;

	[NonSerialized]
	public Animator Animator_0;

	[NonSerialized]
	public string String_0;

	[NonSerialized]
	public AnimatorControllerParameter[] AnimatorControllerParameter_0;

	[NonSerialized]
	public HashSet<int> HashSet_0 = new HashSet<int>();

	[CompilerGenerated]
	private Action<int, bool> action_0;

	[CompilerGenerated]
	private Action<int, int> action_1;

	[CompilerGenerated]
	private Action<int, float> action_2;

	[CompilerGenerated]
	private Action<int, bool> action_3;

	public Animator Animator => Animator_0;

	public string name => String_0;

	public RuntimeAnimatorController runtimeAnimatorController
	{
		get
		{
			return Animator_0.runtimeAnimatorController;
		}
		set
		{
			Animator_0.runtimeAnimatorController = value;
			method_0();
		}
	}

	public bool enabled
	{
		get
		{
			return Animator_0.enabled;
		}
		set
		{
			Animator_0.enabled = value;
		}
	}

	public AnimatorUpdateMode updateMode
	{
		get
		{
			return Animator_0.updateMode;
		}
		set
		{
			Animator_0.updateMode = value;
		}
	}

	public AnimatorCullingMode cullingMode
	{
		get
		{
			return Animator_0.cullingMode;
		}
		set
		{
			Animator_0.cullingMode = value;
		}
	}

	public int parameterCount => Animator_0.parameterCount;

	public Vector3 deltaPosition => Animator_0.deltaPosition;

	public Quaternion deltaRotation => Animator_0.deltaRotation;

	public Avatar avatar
	{
		get
		{
			return Animator_0.avatar;
		}
		set
		{
			Animator_0.avatar = value;
		}
	}

	public int layerCount => Animator_0.layerCount;

	public float speed
	{
		get
		{
			return Animator_0.speed;
		}
		set
		{
			Animator_0.speed = value;
		}
	}

	public bool keepAnimatorControllerStateOnDisable
	{
		get
		{
			return Animator_0.keepAnimatorStateOnDisable;
		}
		set
		{
			Animator_0.keepAnimatorStateOnDisable = value;
		}
	}

	public event Action<int, bool> OnBoolValueChanged
	{
		[CompilerGenerated]
		add
		{
			Action<int, bool> action = action_0;
			Action<int, bool> action2;
			do
			{
				action2 = action;
				Action<int, bool> value2 = (Action<int, bool>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<int, bool> action = action_0;
			Action<int, bool> action2;
			do
			{
				action2 = action;
				Action<int, bool> value2 = (Action<int, bool>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<int, int> OnIntegerValueChanged
	{
		[CompilerGenerated]
		add
		{
			Action<int, int> action = action_1;
			Action<int, int> action2;
			do
			{
				action2 = action;
				Action<int, int> value2 = (Action<int, int>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<int, int> action = action_1;
			Action<int, int> action2;
			do
			{
				action2 = action;
				Action<int, int> value2 = (Action<int, int>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<int, float> OnFloatValueChanged
	{
		[CompilerGenerated]
		add
		{
			Action<int, float> action = action_2;
			Action<int, float> action2;
			do
			{
				action2 = action;
				Action<int, float> value2 = (Action<int, float>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<int, float> action = action_2;
			Action<int, float> action2;
			do
			{
				action2 = action;
				Action<int, float> value2 = (Action<int, float>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<int, bool> OnTriggerChanged
	{
		[CompilerGenerated]
		add
		{
			Action<int, bool> action = action_3;
			Action<int, bool> action2;
			do
			{
				action2 = action;
				Action<int, bool> value2 = (Action<int, bool>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<int, bool> action = action_3;
			Action<int, bool> action2;
			do
			{
				action2 = action;
				Action<int, bool> value2 = (Action<int, bool>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public static GClass1446 Create()
	{
		if (Gclass713_0 == null)
		{
			Gclass713_0 = new GClass713(LoggerMode.Add);
		}
		return new GClass1446();
	}

	public static GClass1446 Create(Animator animator)
	{
		GClass1446 gClass = Create();
		gClass.SetAnimator(animator);
		return gClass;
	}

	public void SetAnimator(Animator animator)
	{
		if ((object)animator != Animator_0)
		{
			String_0 = animator.name;
			Animator_0 = animator;
			Animator_0.logWarnings = false;
			method_0();
		}
	}

	public float GetLayerWeight(int layerIndex)
	{
		if (layerIndex < 0 || layerIndex >= layerCount)
		{
			Gclass713_0.LogErrorOnGetLayerWeight(layerIndex, this);
		}
		return Animator_0.GetLayerWeight(layerIndex);
	}

	public float GetCurrentTransitionStartOffset(int layerIndex)
	{
		throw new NotImplementedException();
	}

	public float GetTransitionNormalizedTime(int layerIndex)
	{
		throw new NotImplementedException();
	}

	public float GetFloat(int conditionCachedNameHash)
	{
		return Animator_0.GetFloat(conditionCachedNameHash);
	}

	public int GetInteger(int conditionCachedNameHash)
	{
		return Animator_0.GetInteger(conditionCachedNameHash);
	}

	public bool GetBool(int conditionCachedNameHash)
	{
		return Animator_0.GetBool(conditionCachedNameHash);
	}

	public float GetFloat(string parameterName)
	{
		return Animator_0.GetFloat(parameterName);
	}

	public int GetInteger(string parameterName)
	{
		return Animator_0.GetInteger(parameterName);
	}

	public bool GetBool(string parameterName)
	{
		return Animator_0.GetBool(parameterName);
	}

	public int StringToHash(string parameterName)
	{
		return Animator.StringToHash(parameterName);
	}

	public string GetParameterName(int hash)
	{
		AnimatorControllerParameter[] parameters = Animator_0.parameters;
		int num = 0;
		AnimatorControllerParameter animatorControllerParameter;
		while (true)
		{
			if (num < parameters.Length)
			{
				animatorControllerParameter = parameters[num];
				if (animatorControllerParameter.nameHash == hash)
				{
					break;
				}
				num++;
				continue;
			}
			return string.Empty;
		}
		return animatorControllerParameter.name;
	}

	public string GetStateName(in AnimatorStateInfoWrapper stateInfo)
	{
		return GClass3598.GetStateName(Animator_0, stateInfo.fullPathHash);
	}

	public bool IsInTransition(int layerIndex)
	{
		return Animator_0.IsInTransition(layerIndex);
	}

	public AnimatorStateInfoWrapper GetNextAnimatorStateInfo(int layerIndex)
	{
		return CreateAnimatorStateInfoWrapper(Animator_0.GetNextAnimatorStateInfo(layerIndex));
	}

	public static AnimatorStateInfoWrapper CreateAnimatorStateInfoWrapper(AnimatorStateInfo stateInfo)
	{
		return new AnimatorStateInfoWrapper(stateInfo.fullPathHash, stateInfo.normalizedTime, stateInfo.loop, stateInfo.shortNameHash, stateInfo.fullPathHash, stateInfo.length, stateInfo.speed);
	}

	public virtual AnimatorStateInfoWrapper GetCurrentAnimatorStateInfo(int layerIndex)
	{
		return CreateAnimatorStateInfoWrapper(Animator_0.GetCurrentAnimatorStateInfo(layerIndex));
	}

	public T[] GetBehaviours<T>() where T : IStateBehaviour
	{
		return Animator_0.GetBehaviours<StateMachineBehaviour>().OfType<T>().ToArray();
	}

	public void method_0()
	{
		if (Animator_0 != null && Animator_0.runtimeAnimatorController != null && Animator_0.gameObject.activeInHierarchy)
		{
			AnimatorControllerParameter_0 = Animator_0.parameters;
			HashSet_0.Clear();
			AnimatorControllerParameter[] animatorControllerParameter_ = AnimatorControllerParameter_0;
			foreach (AnimatorControllerParameter animatorControllerParameter in animatorControllerParameter_)
			{
				HashSet_0.Add(animatorControllerParameter.nameHash);
			}
		}
	}

	public void SetBool(string valueName, bool value)
	{
		Animator_0.SetBool(valueName, value);
	}

	public void SetLayerWeight(int layerIndex, float weight)
	{
		if (layerIndex >= 0 && layerIndex < Animator_0.layerCount)
		{
			Animator_0.SetLayerWeight(layerIndex, weight);
		}
		else
		{
			Gclass713_0.LogErrorOnSetLayerWeight(layerIndex, weight, this);
		}
	}

	public void SetInteger(int valueNameHash, int value)
	{
		Animator_0.SetInteger(valueNameHash, value);
		action_1?.Invoke(valueNameHash, value);
	}

	public void SetBool(int valueNameHash, bool value)
	{
		Animator_0.SetBool(valueNameHash, value);
		action_0?.Invoke(valueNameHash, value);
	}

	public void SetFloat(int valueNameHash, float value)
	{
		Animator_0.SetFloat(valueNameHash, value);
		action_2?.Invoke(valueNameHash, value);
	}

	public void SetTrigger(int triggerNameHash)
	{
		Animator_0.SetTrigger(triggerNameHash);
		action_3?.Invoke(triggerNameHash, arg2: true);
	}

	public void ResetTrigger(int triggerNameHash)
	{
		Animator_0.ResetTrigger(triggerNameHash);
		action_3?.Invoke(triggerNameHash, arg2: false);
	}

	public void SetInteger(string valueName, int value)
	{
		Animator_0.SetInteger(valueName, value);
	}

	public void SetFloat(string floatValueHash, float value)
	{
		Animator_0.SetFloat(floatValueHash, value);
	}

	public void SetTrigger(string triggerName)
	{
		Animator_0.SetTrigger(triggerName);
	}

	public void ResetTrigger(string triggerName)
	{
		Animator_0.ResetTrigger(triggerName);
	}

	public bool HasParameter(int parameterHash)
	{
		return HashSet_0.Contains(parameterHash);
	}

	public virtual void Update(float dt)
	{
		if (Animator_0 == null)
		{
			Debug.LogError(Time.frameCount + " Animator is destroyed but still receives updates: " + String_0);
		}
		else
		{
			Animator_0.Update(dt);
		}
	}

	public void Play(string stateName)
	{
		Animator_0.Play(stateName);
	}

	public int GetLayerIndex(string layerName)
	{
		int layerIndex = Animator_0.GetLayerIndex(layerName);
		if (layerIndex < 0 || layerIndex > Animator_0.layerCount)
		{
			Gclass713_0.LogLayerGetByNameWarning(layerName, this);
		}
		return layerIndex;
	}

	public string GetLayerName(int layerIndex)
	{
		if (layerIndex < 0 || layerIndex > Animator_0.layerCount)
		{
			Gclass713_0.LogGetLayerNameWarning(layerIndex, this);
		}
		return Animator_0.GetLayerName(layerIndex);
	}

	public bool IsParameterControlledByCurve(int parameterNameHash)
	{
		return Animator_0.IsParameterControlledByCurve(parameterNameHash);
	}

	public void Play(int stateNameHash, int layer, float normalizedTime)
	{
		Animator_0.Play(stateNameHash, layer, normalizedTime);
	}

	public void RebindBones()
	{
		GClass3599.RebindAnimatorWithStateRcovering(Animator_0);
	}

	public void Play(string stateName, int layer, float normalizedTime)
	{
		Animator_0.Play(stateName, layer, normalizedTime);
	}

	public AnimatorParameterInfo GetParameter(int parameterIndex)
	{
		if (AnimatorControllerParameter_0 == null)
		{
			method_0();
		}
		AnimatorControllerParameter animatorControllerParameter = AnimatorControllerParameter_0[parameterIndex];
		return new AnimatorParameterInfo(animatorControllerParameter.nameHash, animatorControllerParameter.type);
	}

	public bool Reset()
	{
		return method_1();
	}

	public bool method_1()
	{
		if (!Animator_0.gameObject.activeInHierarchy)
		{
			return false;
		}
		AnimatorControllerParameter[] parameters = Animator_0.parameters;
		foreach (AnimatorControllerParameter animatorControllerParameter in parameters)
		{
			int nameHash = animatorControllerParameter.nameHash;
			if (!Animator_0.IsParameterControlledByCurve(nameHash))
			{
				switch (animatorControllerParameter.type)
				{
				case AnimatorControllerParameterType.Trigger:
					Animator_0.SetBool(nameHash, animatorControllerParameter.defaultBool);
					break;
				case AnimatorControllerParameterType.Float:
					Animator_0.SetFloat(nameHash, animatorControllerParameter.defaultFloat);
					break;
				case AnimatorControllerParameterType.Int:
					Animator_0.SetInteger(nameHash, animatorControllerParameter.defaultInt);
					break;
				case AnimatorControllerParameterType.Bool:
					Animator_0.SetBool(nameHash, animatorControllerParameter.defaultBool);
					break;
				}
			}
		}
		return true;
	}

	public string GetStateName_1(in AnimatorStateInfoWrapper stateInfo)
	{
		return GetStateName(in stateInfo);
	}

	string IAnimator.GetStateName(in AnimatorStateInfoWrapper stateInfo)
	{
		//ILSpy generated this explicit interface implementation from .override directive in GetStateName_1
		return this.GetStateName_1(in stateInfo);
	}
}
