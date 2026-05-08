using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace ChartAndGraph;

public abstract class ChartItemEffect : MonoBehaviour
{
	internal int int_0;

	private CharItemEffectController charItemEffectController_0;

	[CompilerGenerated]
	private Action<ChartItemEffect> action_0;

	public CharItemEffectController Controller
	{
		get
		{
			if (charItemEffectController_0 == null)
			{
				charItemEffectController_0 = GetComponent<CharItemEffectController>();
				if (charItemEffectController_0 == null)
				{
					charItemEffectController_0 = base.gameObject.AddComponent<CharItemEffectController>();
				}
			}
			return charItemEffectController_0;
		}
	}

	public abstract Vector3 Vector3_0 { get; }

	public abstract Quaternion Quaternion_0 { get; }

	public abstract Vector3 Vector3_1 { get; }

	public event Action<ChartItemEffect> Deactivate
	{
		[CompilerGenerated]
		add
		{
			Action<ChartItemEffect> action = action_0;
			Action<ChartItemEffect> action2;
			do
			{
				action2 = action;
				Action<ChartItemEffect> value2 = (Action<ChartItemEffect>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<ChartItemEffect> action = action_0;
			Action<ChartItemEffect> action2;
			do
			{
				action2 = action;
				Action<ChartItemEffect> value2 = (Action<ChartItemEffect>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public void RaiseDeactivated()
	{
		if (action_0 != null)
		{
			action_0(this);
		}
	}

	public void method_0()
	{
		CharItemEffectController controller = Controller;
		if (controller != null)
		{
			controller.Register(this);
		}
	}

	public void method_1()
	{
		CharItemEffectController controller = Controller;
		if (controller != null)
		{
			controller.Unregister(this);
		}
	}

	public virtual void OnDisable()
	{
		method_1();
	}

	public virtual void OnEnable()
	{
		method_0();
	}

	public virtual void Start()
	{
		method_0();
	}

	public virtual void Destroy()
	{
		method_1();
	}

	public abstract void TriggerIn(bool deactivateOnEnd);

	public abstract void TriggerOut(bool deactivateOnEnd);

	public ChartItemEffect()
	{
	}
}
