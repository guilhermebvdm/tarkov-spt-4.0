using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public class GClass1516
{
	[NonSerialized]
	public volatile bool Bool_0;

	[NonSerialized]
	public Thread Thread_0;

	[NonSerialized]
	public Thread Thread_1;

	[NonSerialized]
	public EventWaitHandle EventWaitHandle_0 = new EventWaitHandle(initialState: false, EventResetMode.AutoReset);

	[NonSerialized]
	public Queue<Func<Action>> Queue_0 = new Queue<Func<Action>>();

	[NonSerialized]
	public Queue<Action> Queue_1 = new Queue<Action>();

	[NonSerialized]
	[CompilerGenerated]
	public int Int_0;

	public bool ShouldStop => Bool_0;

	public int MainThreadId
	{
		[CompilerGenerated]
		get
		{
			return Int_0;
		}
	}

	public GClass1516()
	{
		Int_0 = Thread.CurrentThread.ManagedThreadId;
	}

	public void AddTask(Func<Action> task)
	{
		if (Thread.CurrentThread.ManagedThreadId == MainThreadId)
		{
			lock (Queue_0)
			{
				Queue_0.Enqueue(task);
			}
			lock (EventWaitHandle_0)
			{
				EventWaitHandle_0.Set();
				return;
			}
		}
		method_0(task);
	}

	public void CreateThread()
	{
		Bool_0 = false;
		Thread_0 = new Thread(method_1);
		Thread_0.Name = "AWorker Thread 1";
		Thread_0.Start();
		Thread_1 = new Thread(method_1);
		Thread_1.Name = "AWorker Thread 2";
		Thread_1.Start();
	}

	public void RunInMainTread(Action action)
	{
		if (Thread.CurrentThread.ManagedThreadId == MainThreadId)
		{
			action();
			return;
		}
		lock (Queue_1)
		{
			Queue_1.Enqueue(action);
		}
	}

	public void CheckForFinishedTasks()
	{
		while (true)
		{
			Action action;
			lock (Queue_1)
			{
				if (Queue_1.Count <= 0)
				{
					break;
				}
				action = Queue_1.Dequeue();
			}
			action();
		}
	}

	public void Kill()
	{
		if (!Bool_0)
		{
			Bool_0 = true;
			Thread_0.Abort();
			Thread_0 = null;
			Thread_1.Abort();
			Thread_1 = null;
		}
	}

	public void method_0(Func<Action> task)
	{
		Action action = null;
		try
		{
			action = task();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		if (action == null)
		{
			Debug.LogError("Async operation without completion callback");
			return;
		}
		lock (Queue_1)
		{
			Queue_1.Enqueue(action);
		}
	}

	public async void method_1()
	{
		while (!Bool_0)
		{
			Func<Action> func = null;
			bool flag = false;
			lock (Queue_0)
			{
				if (Queue_0.Count > 0)
				{
					func = Queue_0.Dequeue();
					flag = Queue_0.Count > 0;
				}
			}
			if (func != null)
			{
				if (flag)
				{
					lock (EventWaitHandle_0)
					{
						EventWaitHandle_0.Set();
					}
				}
				method_0(func);
				if (Bool_0)
				{
					break;
				}
			}
			else
			{
				try
				{
					EventWaitHandle_0.WaitOne();
				}
				catch (ThreadInterruptedException)
				{
				}
				catch (ThreadAbortException)
				{
					break;
				}
			}
		}
	}
}
