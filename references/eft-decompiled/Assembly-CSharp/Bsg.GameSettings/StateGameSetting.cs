using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Bsg.GameSettings;

[Serializable]
public class StateGameSetting<T> : GameSetting<T>
{
	[NonSerialized]
	public global::BindableStateClass<T> InternalState = new global::BindableStateClass<T>();

	[NonSerialized]
	public T DefaultValue;

	public override T GetValue()
	{
		return InternalState.Value;
	}

	public override async Task SetValue(T value)
	{
		global::BindableStateClass<T> internalState = InternalState;
		internalState.Value = await PreProcessValue(value);
	}

	public StateGameSetting(string key, T defaultValue, Func<T, Task<T>> asyncPreProcessor)
		: base(key, asyncPreProcessor)
	{
		method_0(defaultValue);
	}

	public StateGameSetting(string key, T defaultValue, Func<T, T> preProcessor)
		: base(key, preProcessor)
	{
		method_0(defaultValue);
	}

	public void method_0(T defaultValue)
	{
		DefaultValue = defaultValue;
		ResetToDefault();
	}

	public override bool HasSameValue(GameSetting<T> other)
	{
		return EqualityComparer<T>.Default.Equals(base.Value, other.Value);
	}

	public override void TakeValueFrom(GameSetting<T> other)
	{
		base.Value = other.Value;
	}

	public override Action Bind(Action<T> handler)
	{
		return InternalState.Bind(handler);
	}

	public override Action Subscribe(Action<T> handler)
	{
		return InternalState.Subscribe(handler);
	}

	public override Action BindWithoutValue(IHandler handler)
	{
		return InternalState.BindWithoutValue(handler);
	}

	public sealed override void ResetToDefault()
	{
		InternalState.Value = DefaultValue;
	}

	public override void ForceApply()
	{
		if (InternalState.Handlers == null)
		{
			return;
		}
		foreach (Action<T> handler in InternalState.Handlers)
		{
			try
			{
				handler(base.Value);
			}
			catch (Exception ex)
			{
				Debug.LogError("Error happened while applying " + base.Key + ": " + ex);
			}
		}
	}
}
