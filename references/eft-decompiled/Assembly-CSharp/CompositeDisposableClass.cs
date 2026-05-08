using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Diz.Binding;
using UnityEngine.Events;

public class CompositeDisposableClass : IDisposable
{
	[CompilerGenerated]
	public class Class486
	{
		public UnityEvent unityEvent;

		public UnityAction handler;

		public void method_0()
		{
			unityEvent.RemoveListener(handler);
		}
	}

	[CompilerGenerated]
	public class Class487<T>
	{
		public UnityEvent<T> unityEvent;

		public UnityAction<T> handler;

		public void method_0()
		{
			unityEvent.RemoveListener(handler);
		}
	}

	[NonSerialized]
	public Action Action_0;

	[NonSerialized]
	public List<Action> List_0;

	[NonSerialized]
	public CancellationTokenSource CancellationTokenSource_0;

	public CancellationToken CancellationToken
	{
		get
		{
			if (CancellationTokenSource_0 == null)
			{
				CancellationTokenSource_0 = new CancellationTokenSource();
				AddDisposable(method_0);
			}
			return CancellationTokenSource_0.Token;
		}
	}

	public CompositeDisposableClass()
	{
		List_0 = null;
	}

	public CompositeDisposableClass(int capacity)
	{
		List_0 = new List<Action>(capacity);
	}

	public void AddDisposable(Action destroy)
	{
		if (List_0 == null && Action_0 == null)
		{
			Action_0 = destroy;
			return;
		}
		List_0 = List_0 ?? new List<Action>(8);
		if (Action_0 != null)
		{
			List_0.Add(Action_0);
			Action_0 = null;
		}
		List_0.Add(destroy);
	}

	public T AddDisposable<T>(T disposable) where T : class, IDisposable
	{
		AddDisposable(disposable.Dispose);
		return disposable;
	}

	public void DisposeReference<T>(ref T disposable) where T : class, IDisposable
	{
		if (disposable != null)
		{
			Action action = disposable.Dispose;
			if (List_0 != null)
			{
				List_0.Remove(action);
			}
			else if (Action_0 == action)
			{
				Action_0 = null;
			}
			action();
			disposable = null;
		}
	}

	public void SubscribeEvent(UnityEvent unityEvent, UnityAction handler)
	{
		unityEvent.AddListener(handler);
		AddDisposable(delegate
		{
			unityEvent.RemoveListener(handler);
		});
	}

	public void SubscribeEvent<T>(UnityEvent<T> unityEvent, UnityAction<T> handler)
	{
		unityEvent.AddListener(handler);
		AddDisposable(delegate
		{
			unityEvent.RemoveListener(handler);
		});
	}

	public void BindEvent(UnityEvent unityEvent, UnityAction handler)
	{
		SubscribeEvent(unityEvent, handler);
		handler();
	}

	public void BindEvent<T>(UnityEvent<T> unityEvent, UnityAction<T> handler, T value)
	{
		SubscribeEvent(unityEvent, handler);
		handler(value);
	}

	public void SubscribeEvent(IBindableEvent @event, Action handler)
	{
		AddDisposable(@event.Subscribe(handler));
	}

	public void SubscribeEvent<T>(GInterface152<T> @event, Action<T> handler)
	{
		AddDisposable(@event.Subscribe(handler));
	}

	public void BindEvent(IBindableEvent @event, Action handler)
	{
		AddDisposable(@event.Bind(handler));
	}

	public void BindEvent<T>(GInterface152<T> @event, Action<T> handler, T defaultValue)
	{
		AddDisposable(@event.Bind(handler, defaultValue));
	}

	public void BindState<T>(IBindable<T> state, Action<T> handler)
	{
		AddDisposable(state.Bind(handler));
	}

	public void SubscribeState<T>(IBindable<T> state, Action<T> handler)
	{
		AddDisposable(state.Subscribe(handler));
	}

	public void Dispose()
	{
		Action_0?.Invoke();
		Action_0 = null;
		if (List_0 == null || List_0.Count < 1)
		{
			return;
		}
		foreach (Action item in List_0)
		{
			item();
		}
		List_0.Clear();
	}

	public void method_0()
	{
		if (CancellationTokenSource_0 != null)
		{
			CancellationTokenSource_0.Cancel(throwOnFirstException: false);
			CancellationTokenSource_0.Dispose();
			CancellationTokenSource_0 = null;
		}
	}
}
