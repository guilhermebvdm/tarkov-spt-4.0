using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public class OnRenderObjectConnector : MonoBehaviour
{
	[CompilerGenerated]
	private static Action<OnRenderObjectConnector> action_0;

	[CompilerGenerated]
	private static Action<OnRenderObjectConnector> action_1;

	public static event Action<OnRenderObjectConnector> OnRenderObjectConnectorAdd
	{
		[CompilerGenerated]
		add
		{
			Action<OnRenderObjectConnector> action = action_0;
			Action<OnRenderObjectConnector> action2;
			do
			{
				action2 = action;
				Action<OnRenderObjectConnector> value2 = (Action<OnRenderObjectConnector>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<OnRenderObjectConnector> action = action_0;
			Action<OnRenderObjectConnector> action2;
			do
			{
				action2 = action;
				Action<OnRenderObjectConnector> value2 = (Action<OnRenderObjectConnector>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public static event Action<OnRenderObjectConnector> OnRenderObjectConnectorRemove
	{
		[CompilerGenerated]
		add
		{
			Action<OnRenderObjectConnector> action = action_1;
			Action<OnRenderObjectConnector> action2;
			do
			{
				action2 = action;
				Action<OnRenderObjectConnector> value2 = (Action<OnRenderObjectConnector>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<OnRenderObjectConnector> action = action_1;
			Action<OnRenderObjectConnector> action2;
			do
			{
				action2 = action;
				Action<OnRenderObjectConnector> value2 = (Action<OnRenderObjectConnector>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public virtual void Start()
	{
		action_0?.Invoke(this);
	}

	public virtual void OnDestroy()
	{
		action_1?.Invoke(this);
	}

	public virtual void ManualOnRenderObject(Camera currentCamera)
	{
	}
}
