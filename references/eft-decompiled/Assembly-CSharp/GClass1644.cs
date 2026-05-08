using System;
using System.Collections.Generic;

public abstract class GClass1644<T> : GClass1642<T>
{
	[NonSerialized]
	public Action[] Action_0;

	public void Disconnect()
	{
		for (int i = 0; i < Action_0.Length; i++)
		{
			Action_0[i]();
		}
		Action_0 = null;
	}

	public void method_3(params Action[] subscriptions)
	{
		Action_0 = subscriptions;
	}

	public override Action Bind(Action<T> handler)
	{
		if (!base.HasHandlers)
		{
			Connect();
		}
		return base.Bind(handler);
	}

	public override Action BindWithoutValue(IHandler handler)
	{
		if (!base.HasHandlers)
		{
			Connect();
		}
		return base.BindWithoutValue(handler);
	}

	public override void Unbind(Action<T> handler)
	{
		base.Unbind(handler);
		if (!base.HasHandlers)
		{
			Disconnect();
		}
	}

	public abstract void Connect();

	public GClass1644()
		: base((IEqualityComparer<T>)null)
	{
	}
}
