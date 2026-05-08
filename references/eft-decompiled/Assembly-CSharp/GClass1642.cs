using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Diz.Binding;
using JetBrains.Annotations;

public abstract class GClass1642<T> : IBindable<T>
{
	[CompilerGenerated]
	public class Class1043
	{
		public GClass1642<T> gclass1642_0;

		public Action<T> handler;

		public void method_0()
		{
			gclass1642_0.Unbind(handler);
		}
	}

	[CompilerGenerated]
	public class Class1044
	{
		public GClass1642<T> gclass1642_0;

		public IHandler handler;

		public void method_0()
		{
			gclass1642_0.method_2();
			gclass1642_0.List_1.Remove(handler);
		}
	}

	[NonSerialized]
	public T Gparam_0;

	[NonSerialized]
	public IEqualityComparer<T> IequalityComparer_0;

	[NonSerialized]
	public List<Action<T>> List_0_1;

	[NonSerialized]
	public List<IHandler> List_1_1;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public Queue<Action<T>> Queue_0 = new Queue<Action<T>>();

	[CanBeNull]
	public IReadOnlyList<Action<T>> Handlers => List_0_1;

	public bool HasHandlers
	{
		get
		{
			if (List_0_1 != null)
			{
				return List_0_1.Count > 0;
			}
			return false;
		}
	}

	public bool HasCheckChanges
	{
		get
		{
			if (List_1_1 != null)
			{
				return List_1_1.Count > 0;
			}
			return false;
		}
	}

	public List<Action<T>> List_0 => List_0_1 ?? (List_0_1 = new List<Action<T>>(16));

	public List<IHandler> List_1 => List_1_1 ?? (List_1_1 = new List<IHandler>(16));

	public virtual T Value
	{
		get
		{
			return Gparam_0;
		}
		set
		{
		}
	}

	public GClass1642(IEqualityComparer<T> equalityComparer = null)
	{
		IequalityComparer_0 = equalityComparer;
	}

	public virtual Action Bind(Action<T> handler)
	{
		Action result = Subscribe(handler);
		handler(Gparam_0);
		return result;
	}

	public Action Subscribe(Action<T> handler)
	{
		method_2();
		List_0.Add(handler);
		return delegate
		{
			Unbind(handler);
		};
	}

	public virtual void Unbind(Action<T> handler)
	{
		if (Bool_0)
		{
			Queue_0.Enqueue(handler);
		}
		else
		{
			List_0_1?.Remove(handler);
		}
	}

	public virtual Action BindWithoutValue(IHandler handler)
	{
		method_2();
		List_1.Add(handler);
		handler.CheckChanges();
		return delegate
		{
			method_2();
			List_1.Remove(handler);
		};
	}

	public void method_0(T newState)
	{
		if (method_1(newState))
		{
			return;
		}
		Bool_0 = true;
		Gparam_0 = newState;
		if (HasHandlers)
		{
			foreach (Action<T> item in List_0_1)
			{
				item(Gparam_0);
			}
		}
		if (HasCheckChanges)
		{
			foreach (IHandler item2 in List_1_1)
			{
				item2.CheckChanges();
			}
		}
		Bool_0 = false;
		while (Queue_0.Count > 0)
		{
			Unbind(Queue_0.Dequeue());
		}
	}

	public bool method_1(T newState)
	{
		return (IequalityComparer_0 ?? EqualityComparer<T>.Default).Equals(Gparam_0, newState);
	}

	public void method_2()
	{
		if (Bool_0)
		{
			throw new Exception("Current operation is not supported while updating value!");
		}
	}

	public static implicit operator T(GClass1642<T> setting)
	{
		return setting.Value;
	}
}
