using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using FastAnimatorSystem;
using UnityEngine;

public class FastAnimatorProcessorClass : IAnimator, IAnimatorNotificator
{
	public class GClass1342 : GInterface134
	{
		[NonSerialized]
		public FastAnimatorControllerClass FastAnimatorControllerClass;

		[NonSerialized]
		public Dictionary<int, GClass1312> Dictionary_0;

		[NonSerialized]
		public Dictionary<int, string> Dictionary_1;

		[NonSerialized]
		public Dictionary<string, GClass1312> Dictionary_2;

		[NonSerialized]
		public Dictionary<int, SpeedAnimParamClass> Dictionary_3 = new Dictionary<int, SpeedAnimParamClass>(70);

		[NonSerialized]
		public GClass713 Gclass713_0;

		[NonSerialized]
		public Dictionary<string, SpeedAnimParamClass> Dictionary_4 = new Dictionary<string, SpeedAnimParamClass>(30);

		[NonSerialized]
		public List<GClass1312> List_0;

		public GClass1342(FastAnimatorControllerClass animatorController, GClass713 logger)
		{
			FastAnimatorControllerClass = animatorController;
			Gclass713_0 = logger;
			List_0 = animatorController.AllStates;
			Dictionary_0 = new Dictionary<int, GClass1312>(List_0.Count);
			Dictionary_2 = new Dictionary<string, GClass1312>(List_0.Count);
			foreach (GClass1312 item in List_0)
			{
				Dictionary_0.Add(item.Hash, item);
				Dictionary_2.Add(item.Name, item);
			}
			Dictionary_1 = new Dictionary<int, string>(animatorController.LayersCount);
			for (int i = 0; i < animatorController.LayersCount; i++)
			{
				Dictionary_1.Add(i, animatorController.GetLayerStateMachine(i).Name);
			}
		}

		public bool GetState(int stateInfoFullPathHash, out GClass1312 state)
		{
			return Dictionary_0.TryGetValue(stateInfoFullPathHash, out state);
		}

		public bool GetState(string stateName, int layer, out GClass1312 state)
		{
			if (Dictionary_2.TryGetValue(stateName, out state))
			{
				return true;
			}
			string key = GetLayerName(layer) + "." + stateName;
			if (Dictionary_2.TryGetValue(key, out state))
			{
				return true;
			}
			return false;
		}

		public string GetLayerName(int layer)
		{
			if (!Dictionary_1.TryGetValue(layer, out var value))
			{
				return string.Empty;
			}
			return value;
		}

		public void SetParameter(string paramaterName, bool value)
		{
			method_0(paramaterName)?.SetValue(value);
		}

		public void SetParameter(int paramaterHash, bool value)
		{
			method_1(paramaterHash)?.SetValue(value);
		}

		public void SetParameter(string paramaterName, int value)
		{
			method_0(paramaterName)?.SetValue(value);
		}

		public void SetParameter(int valueNameHash, int value)
		{
			method_1(valueNameHash)?.SetValue(value);
		}

		public void SetParameter(int valueNameHash, float value)
		{
			method_1(valueNameHash)?.SetValue(value);
		}

		public void SetParameter(string valueNameHash, float value)
		{
			method_0(valueNameHash)?.SetValue(value);
		}

		public void AddDependencyToParameter(int parameterHash, CurrentStateAbstractClass animatorState)
		{
			method_1(parameterHash)?.AddValueListener(animatorState);
		}

		public void RemoveDependencyFromParameter(int parameterHash, CurrentStateAbstractClass animatorState)
		{
			method_1(parameterHash)?.RemoveValueListener(animatorState);
		}

		public SpeedAnimParamClass method_0(string paramaterName)
		{
			if (Dictionary_4.TryGetValue(paramaterName, out var value))
			{
				return value;
			}
			SpeedAnimParamClass speedAnimParamClass = FastAnimatorControllerClass.FindParameter(paramaterName);
			if (speedAnimParamClass != null)
			{
				Dictionary_4.Add(speedAnimParamClass.Name, speedAnimParamClass);
				Dictionary_3.Add(speedAnimParamClass.NameHash, speedAnimParamClass);
			}
			else
			{
				Dictionary_4.Add(paramaterName, null);
			}
			return speedAnimParamClass;
		}

		public SpeedAnimParamClass method_1(int parameterHash)
		{
			if (Dictionary_3.TryGetValue(parameterHash, out var value))
			{
				return value;
			}
			value = FastAnimatorControllerClass.FindParameter(parameterHash);
			if (value != null)
			{
				Dictionary_4.Add(value.Name, value);
				Dictionary_3.Add(value.NameHash, value);
			}
			else
			{
				Dictionary_3.Add(parameterHash, null);
			}
			return value;
		}

		public float GetParameter(int nameHash)
		{
			SpeedAnimParamClass speedAnimParamClass = method_1(nameHash);
			if (speedAnimParamClass == null)
			{
				Debug.LogErrorFormat("Can't get parameter from hash:{0}", nameHash);
				return 0f;
			}
			return speedAnimParamClass.GetFloatValue();
		}

		public bool HasParameter(int parameterHash)
		{
			if (!Dictionary_3.ContainsKey(parameterHash))
			{
				return FastAnimatorControllerClass.FindParameter(parameterHash) != null;
			}
			return true;
		}

		public string GetParameterName(int nameHash)
		{
			return method_1(nameHash).Name;
		}
	}

	[NonSerialized]
	public static GClass713 Gclass713_0;

	public readonly FastAnimatorControllerClass FastAnimatorController;

	public GClass1343 FastControllerInfo;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public GClass1342 Gclass1342_0;

	[NonSerialized]
	public Dictionary<int, GClass1448> Dictionary_0;

	[NonSerialized]
	public Transform Transform_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public GInterface123 Ginterface123_0;

	[NonSerialized]
	public int Int_0 = int.MinValue;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public AnimatorUpdateMode AnimatorUpdateMode_0;

	[NonSerialized]
	[CompilerGenerated]
	public AnimatorCullingMode AnimatorCullingMode_0;

	[NonSerialized]
	public float Float_1 = 1f;

	[NonSerialized]
	public Avatar Avatar_0;

	[CompilerGenerated]
	private Action<int, bool> action_0;

	[CompilerGenerated]
	private Action<int, int> action_1;

	[CompilerGenerated]
	private Action<int, float> action_2;

	[CompilerGenerated]
	private Action<int, bool> action_3;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_2;

	public int Int32_0
	{
		get
		{
			if (Int_0 == int.MinValue)
			{
				Int_0 = GetLayerIndex("TagillaSyncLayerForRegularOperations");
			}
			return Int_0;
		}
	}

	public string name => FastAnimatorController.Name;

	public bool enabled
	{
		get
		{
			return Bool_1;
		}
		set
		{
			Bool_1 = value;
		}
	}

	public RuntimeAnimatorController runtimeAnimatorController
	{
		get
		{
			Reset();
			Gclass713_0.LogNotUsed("Get::runtimeAnimatorController", this);
			return null;
		}
		set
		{
			Gclass713_0.LogNotUsed("Set::runtimeAnimatorController", this);
		}
	}

	public AnimatorUpdateMode updateMode
	{
		get
		{
			return AnimatorUpdateMode_0;
		}
		set
		{
			AnimatorUpdateMode_0 = value;
			Gclass713_0.LogNotUsed("Set::_animatorUpdateMode", this);
		}
	}

	public AnimatorCullingMode cullingMode
	{
		[CompilerGenerated]
		get
		{
			return AnimatorCullingMode_0;
		}
		[CompilerGenerated]
		set
		{
			AnimatorCullingMode_0 = value;
		}
	}

	public int layerCount => FastAnimatorController.LayersCount;

	public float speed
	{
		get
		{
			return Float_1;
		}
		set
		{
			Gclass713_0.LogNotUsed("Set::_speed", this);
			Float_1 = value;
		}
	}

	public int parameterCount => FastAnimatorController.ParametersCount;

	public Vector3 deltaPosition
	{
		get
		{
			int layerIndex = ((Int32_0 >= 0 && GetLayerWeight(Int32_0) > 0f && !GetCurrentAnimatorStateInfo(Int32_0).IsName("TagillaOverride.SledgeHammer_Idle")) ? Int32_0 : 0);
			GClass1344 stateInfo = FastControllerInfo.GetStateInfo(layerIndex);
			Vector3 vector = method_2(stateInfo.CurrentState, stateInfo.CurrentStateNormalizedTime);
			if (IsInTransition(layerIndex) && stateInfo.Transition.Duration >= Mathf.Epsilon)
			{
				Vector3 vector2 = method_2(stateInfo.DestinationState, stateInfo.DestinationStateNormalizedTime);
				float num = stateInfo.TransitionAbsTime / stateInfo.TransitionAbsDuration;
				return Time.timeScale * (Transform_0.rotation * (vector * (1f - num) + vector2 * num));
			}
			return Time.timeScale * (Transform_0.rotation * vector);
		}
	}

	public Quaternion deltaRotation
	{
		get
		{
			int layerIndex = ((Int32_0 >= 0 && GetLayerWeight(Int32_0) > 0f && !GetCurrentAnimatorStateInfo(Int32_0).IsName("TagillaOverride.SledgeHammer_Idle")) ? Int32_0 : 0);
			GClass1344 stateInfo = FastControllerInfo.GetStateInfo(layerIndex);
			if (stateInfo.CurrentState == null)
			{
				return Quaternion.identity;
			}
			Quaternion result = method_3(stateInfo.CurrentState);
			if (IsInTransition(layerIndex) && stateInfo.Transition.Duration >= Mathf.Epsilon)
			{
				Quaternion quaternion = method_3(stateInfo.DestinationState);
				float num = stateInfo.TransitionAbsTime / stateInfo.TransitionAbsDuration;
				float num2 = 1f - num;
				return new Quaternion(result.x * num2 + quaternion.x * num, result.y * num2 + quaternion.y * num, result.z * num2 + quaternion.z * num, result.w * num2 + quaternion.w * num);
			}
			return result;
		}
	}

	public Avatar avatar
	{
		get
		{
			Gclass713_0.LogNotUsed("GET:_avatar", this);
			return Avatar_0;
		}
		set
		{
			Avatar_0 = value;
			Gclass713_0.LogNotUsed("SET:_avatar", this);
		}
	}

	public bool keepAnimatorControllerStateOnDisable
	{
		[CompilerGenerated]
		get
		{
			return Bool_2;
		}
		[CompilerGenerated]
		set
		{
			Bool_2 = value;
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

	public FastAnimatorProcessorClass(FastAnimatorControllerClass fastAnimatorController, Dictionary<int, GClass1448> computedNodes, Transform rootBone, GInterface123 playableAnimator)
	{
		if (Gclass713_0 == null)
		{
			Gclass713_0 = new GClass713(LoggerMode.Add);
		}
		Dictionary_0 = computedNodes;
		Transform_0 = rootBone;
		Ginterface123_0 = playableAnimator;
		FastAnimatorController = fastAnimatorController;
		Reset();
		if (computedNodes == null)
		{
			return;
		}
		foreach (GClass1312 allState in FastAnimatorController.AllStates)
		{
			if (allState is GClass1320)
			{
				GClass1320 gClass = allState as GClass1320;
				if (Dictionary_0.ContainsKey(allState.Hash))
				{
					gClass.InitParametersCacheAndRootMotionTable(Gclass1342_0, Dictionary_0[allState.Hash]);
				}
			}
		}
	}

	public bool Reset()
	{
		FastControllerInfo = new GClass1343(FastAnimatorController);
		Gclass1342_0 = new GClass1342(FastAnimatorController, Gclass713_0);
		Bool_0 = false;
		FastAnimatorController.ResetCache();
		method_0();
		return true;
	}

	public GInterface134 GetParametersCache()
	{
		return Gclass1342_0;
	}

	public void method_0()
	{
		foreach (SpeedAnimParamClass item in FastAnimatorController.List_0)
		{
			if (!item.IsControlledByAnimationCurve)
			{
				switch (item.ValueType)
				{
				case ThresholdClass.EAnimatorValueType.Int:
					item.SetValue(item.DefaultIntValue);
					break;
				case ThresholdClass.EAnimatorValueType.Float:
					item.SetValue(item.DefaultFloatValue);
					break;
				case ThresholdClass.EAnimatorValueType.Boolean:
					item.SetValue(item.DefaultBoolValue);
					break;
				case ThresholdClass.EAnimatorValueType.Trigger:
					item.SetValue(item.DefaultBoolValue);
					break;
				case ThresholdClass.EAnimatorValueType.Undefined:
					throw new NotImplementedException("parameter.ValueType == AnimatorValue.EAnimatorValueType.Undefined");
				}
			}
		}
	}

	public AnimatorParameterInfo GetParameter(int parameterIndex)
	{
		return method_1(FastAnimatorController.GetParameter(parameterIndex));
	}

	public AnimatorParameterInfo method_1(SpeedAnimParamClass parameter)
	{
		return new AnimatorParameterInfo(parameter.NameHash, smethod_0(parameter.ValueType));
	}

	public static AnimatorControllerParameterType smethod_0(ThresholdClass.EAnimatorValueType parameterType)
	{
		return parameterType switch
		{
			ThresholdClass.EAnimatorValueType.Int => AnimatorControllerParameterType.Int, 
			ThresholdClass.EAnimatorValueType.Float => AnimatorControllerParameterType.Float, 
			ThresholdClass.EAnimatorValueType.Boolean => AnimatorControllerParameterType.Bool, 
			ThresholdClass.EAnimatorValueType.Trigger => AnimatorControllerParameterType.Trigger, 
			_ => throw new NotImplementedException(), 
		};
	}

	public int StringToHash(string parameterName)
	{
		return Animator.StringToHash(parameterName);
	}

	public bool IsParameterControlledByCurve(int parameterNameHash)
	{
		SpeedAnimParamClass speedAnimParamClass = FastAnimatorController.FindParameter(parameterNameHash);
		if (speedAnimParamClass == null)
		{
			Debug.LogErrorFormat("Parameter is null for hash:{0}", parameterNameHash);
			return false;
		}
		return speedAnimParamClass.IsControlledByAnimationCurve;
	}

	public string GetParameterName(int hash)
	{
		return Gclass1342_0.GetParameterName(hash);
	}

	public string GetStateName(in AnimatorStateInfoWrapper stateInfo)
	{
		if (Gclass1342_0.GetState(stateInfo.fullPathHash, out var state))
		{
			return state.Name;
		}
		Gclass713_0.LogError("state {0} not found", stateInfo.fullPathHash);
		return "";
	}

	public float GetStateAbsTime(in AnimatorStateInfoWrapper stateInfo, int layerIndex)
	{
		GClass1344 stateInfo2 = FastControllerInfo.GetStateInfo(layerIndex);
		if (stateInfo2.CurrentState.Hash == stateInfo.fullPathHash)
		{
			return stateInfo2.CurrentStateAbsoluteTime;
		}
		if (!IsInTransition(0))
		{
			return 0f;
		}
		if (stateInfo2.DestinationState.Hash != stateInfo.fullPathHash)
		{
			return 0f;
		}
		return stateInfo2.DestinationStateAbsoluteTime;
	}

	public bool IsInTransition(int layerIndex)
	{
		return FastControllerInfo.GetStateInfo(layerIndex).Transition != null;
	}

	public void GetTransitionInfo(int layerIndex, out string tranName, out float tranDuration, out float tranTimeNormalized)
	{
		if (!IsInTransition(layerIndex))
		{
			tranName = "no transition";
			tranDuration = -1f;
			tranTimeNormalized = 0f;
		}
		else
		{
			GClass1344 stateInfo = FastControllerInfo.GetStateInfo(layerIndex);
			tranName = stateInfo.Transition.Name;
			tranDuration = stateInfo.TransitionAbsDuration;
			tranTimeNormalized = stateInfo.TransitionAbsTime / tranDuration;
		}
	}

	public float GetTransitionNormalizedTime(int layerIndex)
	{
		if (!IsInTransition(layerIndex))
		{
			return 0f;
		}
		GClass1344 stateInfo = FastControllerInfo.GetStateInfo(layerIndex);
		return stateInfo.TransitionAbsTime / stateInfo.TransitionAbsDuration;
	}

	public AnimatorStateInfoWrapper GetNextAnimatorStateInfo(int layerIndex)
	{
		GClass1344 stateInfo = FastControllerInfo.GetStateInfo(layerIndex);
		return smethod_1(stateInfo.DestinationState, stateInfo.DestinationStateNormalizedTime);
	}

	public AnimatorStateInfoWrapper GetCurrentAnimatorStateInfo(int layerIndex)
	{
		GClass1344 stateInfo = FastControllerInfo.GetStateInfo(layerIndex);
		return smethod_1(stateInfo.CurrentState, stateInfo.CurrentStateNormalizedTime);
	}

	public Vector3 method_2(CurrentStateAbstractClass animState, float stateNormalizedTime)
	{
		if (animState == null)
		{
			return Vector3.zero;
		}
		int hash = animState.Hash;
		GClass1448 gClass = (Dictionary_0.ContainsKey(hash) ? Dictionary_0[hash] : null);
		if (gClass == null)
		{
			return Vector3.zero;
		}
		Vector3 vector = gClass.ComputeRootPosition(Float_0, stateNormalizedTime, Gclass1342_0);
		float num = 1f;
		if (animState.SpeedAnimParamClass != null)
		{
			num = Mathf.Abs(animState.SpeedAnimParamClass.GetFloatValue());
		}
		return vector * Mathf.Abs(animState.Float_0) * num;
	}

	public Quaternion method_3(CurrentStateAbstractClass animState)
	{
		int hash = animState.Hash;
		return (Dictionary_0.ContainsKey(hash) ? Dictionary_0[hash] : null).ComputeDeltaRotation(Float_0, Gclass1342_0);
	}

	public static AnimatorStateInfoWrapper smethod_1(CurrentStateAbstractClass state, float normalizedTime)
	{
		if (state == null)
		{
			return new AnimatorStateInfoWrapper(-1, normalizedTime, loop: false, -1, -1, 0f, 0f);
		}
		int hash = state.Hash;
		float num = state.SpeedAnimParamClass?.GetFloatValue() ?? state.Float_0;
		return new AnimatorStateInfoWrapper(hash, normalizedTime, state.Loop, hash, hash, state.MotionDuration, num);
	}

	public T[] GetBehaviours<T>() where T : IStateBehaviour
	{
		return FastAnimatorController.GetBehaviors<T>();
	}

	public void SetLayerWeight(int layerIndex, float weight)
	{
		if (layerIndex >= 0 && layerIndex < layerCount)
		{
			FastAnimatorController.List_1[layerIndex].LayerWeight = weight;
			if (Ginterface123_0 != null)
			{
				Ginterface123_0.SetLayerWeight(layerIndex, weight);
			}
		}
		else
		{
			Gclass713_0.LogErrorOnSetLayerWeight(layerIndex, weight, this);
		}
	}

	public float GetLayerWeight(int layerIndex)
	{
		if (layerIndex >= 0 && layerIndex < layerCount)
		{
			return FastAnimatorController.List_1[layerIndex].LayerWeight;
		}
		Gclass713_0.LogErrorOnGetLayerWeight(layerIndex, this);
		return -1f;
	}

	public int GetLayerIndex(string layerName)
	{
		int num = -1;
		for (int i = 0; i < FastAnimatorController.List_1.Count; i++)
		{
			if (FastAnimatorController.List_1[i].Name.Equals(layerName))
			{
				num = i;
				break;
			}
		}
		if (num < 0 || num > FastAnimatorController.List_1.Count)
		{
			Gclass713_0.LogLayerGetByNameWarning(layerName, this);
		}
		return num;
	}

	public string GetLayerName(int layerIndex)
	{
		if (layerIndex >= 0 && layerIndex < FastAnimatorController.List_1.Count)
		{
			return FastAnimatorController.List_1[layerIndex].Name;
		}
		Gclass713_0.LogGetLayerNameWarning(layerIndex, this);
		return string.Empty;
	}

	public float GetCurrentTransitionStartOffset(int layerIndex)
	{
		if (!IsInTransition(layerIndex))
		{
			return 0f;
		}
		return FastControllerInfo.GetStateInfo(layerIndex).Transition.NormalizedDestinationStateOffset;
	}

	public float GetFloat(int parameterNameHash)
	{
		return FastAnimatorController.FindParameter(parameterNameHash)?.GetFloatValue() ?? 0f;
	}

	public float GetFloat(string parameterName)
	{
		return FastAnimatorController.FindParameter(parameterName)?.GetFloatValue() ?? 0f;
	}

	public int GetInteger(int parameterNameHash)
	{
		return FastAnimatorController.FindParameter(parameterNameHash)?.GetIntValue() ?? 0;
	}

	public int GetInteger(string parameterName)
	{
		return FastAnimatorController.FindParameter(parameterName)?.GetIntValue() ?? 0;
	}

	public bool GetBool(int parameterNameHash)
	{
		return FastAnimatorController.FindParameter(parameterNameHash)?.GetBoolValue() ?? false;
	}

	public bool GetBool(string parameterName)
	{
		return FastAnimatorController.FindParameter(parameterName)?.GetBoolValue() ?? false;
	}

	public void SetBool(string valueName, bool value)
	{
		Gclass1342_0.SetParameter(valueName, value);
	}

	public void SetBool(int valueNameHash, bool value)
	{
		Gclass1342_0.SetParameter(valueNameHash, value);
		action_0?.Invoke(valueNameHash, value);
	}

	public void SetInteger(string valueName, int value)
	{
		Gclass1342_0.SetParameter(valueName, value);
	}

	public void SetInteger(int valueNameHash, int value)
	{
		Gclass1342_0.SetParameter(valueNameHash, value);
		action_1?.Invoke(valueNameHash, value);
	}

	public void SetFloat(int valueNameHash, float value)
	{
		Gclass1342_0.SetParameter(valueNameHash, value);
		action_2?.Invoke(valueNameHash, value);
	}

	public void SetFloat(string valueName, float value)
	{
		Gclass1342_0.SetParameter(valueName, value);
	}

	public void SetTrigger(int valueNameHash)
	{
		Gclass1342_0.SetParameter(valueNameHash, value: true);
		action_3?.Invoke(valueNameHash, arg2: true);
	}

	public void SetTrigger(string triggerName)
	{
		Gclass1342_0.SetParameter(triggerName, value: true);
	}

	public void ResetTrigger(int triggerNameHash)
	{
		Gclass1342_0.SetParameter(triggerNameHash, value: false);
		action_3?.Invoke(triggerNameHash, arg2: false);
	}

	public void ResetTrigger(string triggerName)
	{
		Gclass1342_0.SetParameter(triggerName, value: false);
	}

	public bool HasParameter(int parameterHash)
	{
		return Gclass1342_0.HasParameter(parameterHash);
	}

	public void Update(float deltaTime)
	{
		Float_0 = deltaTime;
		for (int i = 0; i < FastAnimatorController.LayersCount; i++)
		{
			method_4(deltaTime, i, FastAnimatorController, FastControllerInfo);
		}
	}

	public void Play(string stateName, int layer, float normalizedTime)
	{
		if (Gclass1342_0.GetState(stateName, layer, out var state))
		{
			GClass1344 stateInfo = FastControllerInfo.GetStateInfo(layer);
			stateInfo.ForcePlayState = state;
			stateInfo.ForcePlayStateNT = normalizedTime;
		}
	}

	public void Play(int stateNameHash, int layer, float normalizedTime)
	{
		if (Gclass1342_0.GetState(stateNameHash, out var state))
		{
			GClass1344 stateInfo = FastControllerInfo.GetStateInfo(layer);
			stateInfo.ForcePlayState = state;
			stateInfo.ForcePlayStateNT = normalizedTime;
		}
	}

	public void RebindBones()
	{
	}

	public void method_4(float deltaTime, int layerIndex, FastAnimatorControllerClass fastAnimatorController, GClass1343 fastControllerInfo)
	{
		GClass1344 stateInfo = fastControllerInfo.GetStateInfo(layerIndex);
		if (stateInfo.CurrentState != null)
		{
			method_5(stateInfo, deltaTime, layerIndex);
		}
	}

	public void method_5(GClass1344 layerInfo, float deltaTime, int layerIndex, int recursionLevel = 0)
	{
		if (layerInfo.ForcePlayState != null)
		{
			layerInfo.CurrentStateAbsoluteTime += deltaTime;
			AnimatorStateInfoWrapper layerInfo2 = smethod_1(layerInfo.CurrentState, layerInfo.CurrentStateNormalizedTime);
			layerInfo.CurrentState.OnStateExit(this, in layerInfo2, layerIndex);
			layerInfo.CurrentState.OnStateMove(this, in layerInfo2, layerIndex);
			if (layerInfo.Transition != null)
			{
				AnimatorStateInfoWrapper layerInfo3 = smethod_1(layerInfo.DestinationState, layerInfo.DestinationStateAbsoluteTime);
				layerInfo.DestinationState.OnStateExit(this, in layerInfo3, layerIndex);
				layerInfo.DestinationState.OnStateMove(this, in layerInfo3, layerIndex);
				layerInfo.Transition.MarkDependentParameterDirty();
				layerInfo.Transition = null;
				layerInfo.TransitionAbsTime = 0f;
				layerInfo.DestinationStateAbsoluteTime = 0f;
			}
			layerInfo.CurrentState = layerInfo.ForcePlayState as CurrentStateAbstractClass;
			layerInfo.CurrentStateAbsoluteTime = layerInfo.CurrentState.MotionDuration * layerInfo.ForcePlayStateNT;
			layerInfo2 = smethod_1(layerInfo.CurrentState, layerInfo.CurrentStateNormalizedTime);
			layerInfo.CurrentState.OnStateEnter(this, in layerInfo2, layerIndex);
			layerInfo.CurrentState.OnStateMove(this, in layerInfo2, layerIndex);
			layerInfo.ForcePlayState = null;
			return;
		}
		bool flag = true;
		if (layerInfo.Transition != null)
		{
			TransitionClass transition = layerInfo.Transition;
			if (transition.InterruptionSource != ETransitionInterruptionSource.None)
			{
				CurrentStateAbstractClass currentState = layerInfo.CurrentState;
				CurrentStateAbstractClass destinationState = layerInfo.DestinationState;
				TransitionClass transitionClass = method_7(currentState, destinationState, layerInfo.CurrentStateAbsoluteTime, layerInfo.DestinationStateAbsoluteTime, deltaTime, transition);
				if (transitionClass != null)
				{
					layerInfo.CurrentStateAbsoluteTime += deltaTime;
					layerInfo.DestinationStateAbsoluteTime += deltaTime;
					layerInfo.DestinationState.OnStateExit(this, smethod_1(layerInfo.DestinationState, layerInfo.DestinationStateNormalizedTime), layerIndex);
					layerInfo.Transition = transitionClass;
					layerInfo.CurrentState.MarkTransitionDirty();
					layerInfo.TransitionAbsTime = 0f;
					float num = (transitionClass.HasExitTime ? deltaTime : 0f);
					layerInfo.DestinationStateAbsoluteTime = num + transitionClass.NormalizedDestinationStateOffset * transitionClass.DestinationState.MotionDuration;
					AnimatorStateInfoWrapper layerInfo4 = smethod_1(layerInfo.CurrentState, layerInfo.CurrentStateNormalizedTime);
					AnimatorStateInfoWrapper layerInfo5 = smethod_1(layerInfo.DestinationState, layerInfo.DestinationStateNormalizedTime);
					if (Bool_0)
					{
						layerInfo.CurrentState.OnStateUpdate(this, in layerInfo4, layerIndex);
					}
					else
					{
						layerInfo.CurrentState.OnStateEnter(this, in layerInfo4, layerIndex);
					}
					layerInfo.DestinationState.OnStateEnter(this, in layerInfo5, layerIndex);
					layerInfo.CurrentState.OnStateMove(this, in layerInfo4, layerIndex);
					layerInfo.DestinationState.OnStateMove(this, in layerInfo5, layerIndex);
					Bool_0 = true;
					return;
				}
			}
			if (layerInfo.TransitionAbsTime + deltaTime < layerInfo.TransitionAbsDuration)
			{
				layerInfo.CurrentStateAbsoluteTime += deltaTime;
				layerInfo.DestinationStateAbsoluteTime += deltaTime;
				layerInfo.TransitionAbsTime += deltaTime;
				AnimatorStateInfoWrapper layerInfo6 = smethod_1(layerInfo.CurrentState, layerInfo.CurrentStateNormalizedTime);
				AnimatorStateInfoWrapper layerInfo7 = smethod_1(layerInfo.DestinationState, layerInfo.DestinationStateNormalizedTime);
				layerInfo.CurrentState.OnStateUpdate(this, in layerInfo6, layerIndex);
				layerInfo.CurrentState.OnStateMove(this, in layerInfo6, layerIndex);
				layerInfo.DestinationState.OnStateUpdate(this, in layerInfo7, layerIndex);
				layerInfo.DestinationState.OnStateMove(this, in layerInfo7, layerIndex);
				Bool_0 = true;
				return;
			}
			float num2 = transition.NormalizedDestinationStateOffset * transition.DestinationState.MotionDuration + transition.Duration * transition.SourceState.MotionDuration - layerInfo.DestinationStateAbsoluteTime;
			layerInfo.CurrentStateAbsoluteTime += num2;
			layerInfo.DestinationStateAbsoluteTime += num2;
			CurrentStateAbstractClass currentState2 = layerInfo.CurrentState;
			float currentStateNormalizedTime = layerInfo.CurrentStateNormalizedTime;
			CurrentStateAbstractClass destinationState2 = layerInfo.DestinationState;
			float destinationStateNormalizedTime = layerInfo.DestinationStateNormalizedTime;
			layerInfo.CurrentState = layerInfo.DestinationState;
			layerInfo.CurrentStateAbsoluteTime = layerInfo.DestinationStateAbsoluteTime;
			layerInfo.Transition.MarkDependentParameterDirty();
			layerInfo.Transition = null;
			layerInfo.CurrentState.MarkTransitionDirty();
			layerInfo.TransitionAbsTime = 0f;
			layerInfo.DestinationStateAbsoluteTime = 0f;
			float num3 = deltaTime - num2;
			layerInfo.CurrentStateAbsoluteTime += num3;
			flag = false;
			AnimatorStateInfoWrapper layerInfo8 = smethod_1(currentState2, currentStateNormalizedTime);
			AnimatorStateInfoWrapper layerInfo9 = smethod_1(destinationState2, destinationStateNormalizedTime);
			currentState2.OnStateExit(this, in layerInfo8, layerIndex);
			if (Bool_0)
			{
				destinationState2.OnStateUpdate(this, in layerInfo9, layerIndex);
			}
			else
			{
				destinationState2.OnStateEnter(this, in layerInfo9, layerIndex);
			}
			currentState2.OnStateMove(this, in layerInfo8, layerIndex);
			destinationState2.OnStateMove(this, in layerInfo9, layerIndex);
			Bool_0 = true;
		}
		CurrentStateAbstractClass currentState3 = layerInfo.CurrentState;
		float currentStateAbsoluteTime = layerInfo.CurrentStateAbsoluteTime;
		float num4 = currentStateAbsoluteTime + deltaTime;
		TransitionClass transitionClass2 = method_8(currentState3, currentStateAbsoluteTime, num4);
		if (transitionClass2 == null && flag)
		{
			layerInfo.CurrentStateAbsoluteTime = num4;
			AnimatorStateInfoWrapper layerInfo10 = smethod_1(layerInfo.CurrentState, layerInfo.CurrentStateNormalizedTime);
			if (Bool_0)
			{
				layerInfo.CurrentState.OnStateUpdate(this, in layerInfo10, layerIndex);
				layerInfo.CurrentState.OnStateMove(this, in layerInfo10, layerIndex);
			}
			else
			{
				layerInfo.CurrentState.OnStateEnter(this, in layerInfo10, layerIndex);
				layerInfo.CurrentState.OnStateMove(this, in layerInfo10, layerIndex);
				Bool_0 = true;
			}
		}
		else
		{
			if (transitionClass2 == null)
			{
				return;
			}
			Gclass713_0.LogTriggInfo(currentState3, layerInfo.CurrentStateNormalizedTime, currentStateAbsoluteTime, num4);
			Gclass713_0.LogTriggeredTransition(transitionClass2);
			method_6(transitionClass2);
			if (transitionClass2.Duration > 0f)
			{
				float num5 = deltaTime;
				if (num4 > transitionClass2.SourceState.MotionDuration)
				{
					num4 = (transitionClass2.SourceState.Loop ? (num4 % transitionClass2.SourceState.MotionDuration) : transitionClass2.SourceState.MotionDuration);
				}
				if (num5 > transitionClass2.Duration * transitionClass2.SourceState.MotionDuration && transitionClass2.HasExitTime)
				{
					CurrentStateAbstractClass destinationState3 = transitionClass2.DestinationState;
					CurrentStateAbstractClass currentState4 = layerInfo.CurrentState;
					float currentStateNormalizedTime2 = layerInfo.CurrentStateNormalizedTime;
					layerInfo.CurrentState = destinationState3;
					float num6 = transitionClass2.NormalizedDestinationStateOffset * transitionClass2.DestinationState.MotionDuration;
					layerInfo.CurrentStateAbsoluteTime = num6 + num5;
					float currentStateNormalizedTime3 = layerInfo.CurrentStateNormalizedTime;
					AnimatorStateInfoWrapper layerInfo11 = smethod_1(currentState4, currentStateNormalizedTime2);
					AnimatorStateInfoWrapper layerInfo12 = smethod_1(destinationState3, currentStateNormalizedTime3);
					currentState4.OnStateExit(this, in layerInfo11, layerIndex);
					destinationState3.OnStateEnter(this, in layerInfo12, layerIndex);
					currentState4.OnStateMove(this, in layerInfo11, layerIndex);
					destinationState3.OnStateMove(this, in layerInfo12, layerIndex);
					Bool_0 = true;
					return;
				}
				if (!transitionClass2.HasExitTime)
				{
					num5 = 0f;
				}
				layerInfo.Transition = transitionClass2;
				layerInfo.CurrentState.MarkTransitionDirty();
				layerInfo.TransitionAbsTime = 0f;
				layerInfo.CurrentStateAbsoluteTime = num4;
				layerInfo.DestinationStateAbsoluteTime = num5 + transitionClass2.NormalizedDestinationStateOffset * layerInfo.DestinationState.MotionDuration;
				AnimatorStateInfoWrapper layerInfo13 = smethod_1(layerInfo.CurrentState, layerInfo.CurrentStateNormalizedTime);
				AnimatorStateInfoWrapper layerInfo14 = smethod_1(layerInfo.DestinationState, layerInfo.DestinationStateNormalizedTime);
				if (Bool_0)
				{
					layerInfo.CurrentState.OnStateUpdate(this, in layerInfo13, layerIndex);
				}
				else
				{
					layerInfo.CurrentState.OnStateEnter(this, in layerInfo13, layerIndex);
				}
				layerInfo.DestinationState.OnStateEnter(this, in layerInfo14, layerIndex);
				layerInfo.CurrentState.OnStateMove(this, in layerInfo13, layerIndex);
				layerInfo.DestinationState.OnStateMove(this, in layerInfo14, layerIndex);
				Bool_0 = true;
			}
			else
			{
				float num7 = 0f;
				float num8 = currentStateAbsoluteTime;
				if (num7 > transitionClass2.SourceState.MotionDuration)
				{
					num7 = (transitionClass2.SourceState.Loop ? (num7 % transitionClass2.SourceState.MotionDuration) : transitionClass2.SourceState.MotionDuration);
				}
				if (transitionClass2.HasExitTime)
				{
					num8 = transitionClass2.SourceState.MotionDuration * transitionClass2.ExitTimeNormalized;
					num7 = num4 - num8;
				}
				float prevstatDt = num4 - currentStateAbsoluteTime;
				method_9(layerInfo, num7, recursionLevel, transitionClass2, layerIndex, prevstatDt);
			}
		}
	}

	public void method_6(TransitionClass triggeredTransition)
	{
		triggeredTransition.ResetTriggeredTriggers();
	}

	public TransitionClass method_7(CurrentStateAbstractClass current, CurrentStateAbstractClass next, float currentStateAbsTime, float nextStateAbsTime, float dt, TransitionClass transition)
	{
		return transition.InterruptionSource switch
		{
			ETransitionInterruptionSource.None => null, 
			ETransitionInterruptionSource.Source => current.GetInterruptedTriggeredTransition(current, next, currentStateAbsTime, nextStateAbsTime, dt, transition), 
			ETransitionInterruptionSource.Destination => next.GetInterruptedTriggeredTransition(current, next, currentStateAbsTime, nextStateAbsTime, dt, transition), 
			ETransitionInterruptionSource.SourceThenDestination => current.GetInterruptedTriggeredTransition(current, next, currentStateAbsTime, nextStateAbsTime, dt, transition) ?? next.GetInterruptedTriggeredTransition(current, next, currentStateAbsTime, nextStateAbsTime, dt, transition), 
			ETransitionInterruptionSource.DestinationThenSource => next.GetInterruptedTriggeredTransition(current, next, currentStateAbsTime, nextStateAbsTime, dt, transition) ?? current.GetInterruptedTriggeredTransition(current, next, currentStateAbsTime, nextStateAbsTime, dt, transition), 
			_ => throw new NotImplementedException(), 
		};
	}

	public TransitionClass method_8(CurrentStateAbstractClass state, float absoluteStartFrameTime, float absoluteEndFrameTime)
	{
		return state.GetTriggeredTransition(absoluteStartFrameTime, absoluteEndFrameTime);
	}

	public void method_9(GClass1344 layerInfo, float destStateDT, int recursionLevel, TransitionClass triggeredTransition, int layerIndex, float prevstatDt)
	{
		CurrentStateAbstractClass destinationState = triggeredTransition.DestinationState;
		CurrentStateAbstractClass currentState = layerInfo.CurrentState;
		Ginterface123_0?.RaiseImmediateTransitionHappened(layerIndex);
		layerInfo.CurrentStateAbsoluteTime += prevstatDt;
		float currentStateNormalizedTime = layerInfo.CurrentStateNormalizedTime;
		float num = triggeredTransition.NormalizedDestinationStateOffset * triggeredTransition.DestinationState.MotionDuration + destStateDT;
		float normalizedTime = num / triggeredTransition.DestinationState.MotionDuration;
		AnimatorStateInfoWrapper layerInfo2 = smethod_1(currentState, currentStateNormalizedTime);
		AnimatorStateInfoWrapper layerInfo3 = smethod_1(destinationState, normalizedTime);
		currentState.OnStateExit(this, in layerInfo2, layerIndex);
		destinationState.OnStateEnter(this, in layerInfo3, layerIndex);
		layerInfo.CurrentState = destinationState;
		layerInfo.CurrentStateAbsoluteTime = num;
		currentState.OnStateMove(this, in layerInfo2, layerIndex);
		destinationState.OnStateMove(this, in layerInfo3, layerIndex);
		Bool_0 = true;
	}

	public override string ToString()
	{
		return $"{GetType()}({name})";
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
