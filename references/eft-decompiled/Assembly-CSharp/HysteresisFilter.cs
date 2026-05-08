using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public class HysteresisFilter : MonoBehaviour
{
	public AnimationCurve Forward;

	public AnimationCurve Backward;

	[CompilerGenerated]
	private Action<bool, bool> action_0;

	private Action<float> action_1;

	private bool bool_0;

	private float float_0;

	private float float_1;

	private float float_2;

	[CompilerGenerated]
	private Action<float> action_2;

	public bool Direction => bool_0;

	public event Action<bool, bool> OnEnd
	{
		[CompilerGenerated]
		add
		{
			Action<bool, bool> action = action_0;
			Action<bool, bool> action2;
			do
			{
				action2 = action;
				Action<bool, bool> value2 = (Action<bool, bool>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<bool, bool> action = action_0;
			Action<bool, bool> action2;
			do
			{
				action2 = action;
				Action<bool, bool> value2 = (Action<bool, bool>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<float> OnValueChanged
	{
		[CompilerGenerated]
		add
		{
			Action<float> action = action_2;
			Action<float> action2;
			do
			{
				action2 = action;
				Action<float> value2 = (Action<float>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<float> action = action_2;
			Action<float> action2;
			do
			{
				action2 = action;
				Action<float> value2 = (Action<float>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public void Init(Action<float> action)
	{
		action_1 = action;
	}

	public void OnEnable()
	{
		action_0?.Invoke(arg1: true, bool_0);
	}

	public void OnDisable()
	{
		action_0?.Invoke(arg1: false, bool_0);
	}

	public void Deinit()
	{
		if (!(this == null))
		{
			action_1 = null;
			base.enabled = false;
		}
	}

	public void Set(bool isOn, bool initial)
	{
		bool_0 = isOn;
		if (initial)
		{
			float_0 = (isOn ? GClass842.GetDuration(Forward) : 0f);
			float_2 = (isOn ? Forward.Evaluate(float_0) : Backward.Evaluate(float_0));
			action_1(float_2);
			action_2?.Invoke(float_2);
			base.enabled = false;
		}
		else
		{
			float_1 = (isOn ? GClass842.GetDuration(Forward) : 0f);
			if (!base.enabled && method_0())
			{
				base.enabled = true;
			}
		}
	}

	public bool method_0()
	{
		return !Mathf.Approximately(float_0, float_1);
	}

	public void Update()
	{
		if (method_0())
		{
			float_0 = Mathf.MoveTowards(float_0, float_1, Time.deltaTime);
			float num = (bool_0 ? Forward.Evaluate(float_0) : Backward.Evaluate(float_0));
			if ((bool_0 && float_2 < num) || (!bool_0 && float_2 > num))
			{
				float_2 = num;
				action_1(num);
				action_2?.Invoke(num);
			}
		}
		else
		{
			base.enabled = false;
		}
	}
}
