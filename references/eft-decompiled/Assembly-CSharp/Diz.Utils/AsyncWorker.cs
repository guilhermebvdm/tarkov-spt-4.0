using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Diz.Utils;

public class AsyncWorker : MonoBehaviour
{
	[CompilerGenerated]
	public class Class1008<T>
	{
		public Func<T> function;

		public TaskCompletionSource<T> completionSource;

		public Action method_0()
		{
			try
			{
				T result = function();
				return delegate
				{
					completionSource.SetResult(result);
				};
			}
			catch (Exception ex)
			{
				Exception ex2 = ex;
				Exception e = ex2;
				return delegate
				{
					completionSource.SetException(e);
				};
			}
		}
	}

	[CompilerGenerated]
	public class Class1009<T>
	{
		public T result;

		public Class1008<T> class1008_0;

		public void method_0()
		{
			class1008_0.completionSource.SetResult(result);
		}
	}

	[CompilerGenerated]
	public class Class1010<T>
	{
		public Exception e;

		public Class1008<T> class1008_0;

		public void method_0()
		{
			class1008_0.completionSource.SetException(e);
		}
	}

	[CompilerGenerated]
	public class Class1011
	{
		public Action function;

		public TaskCompletionSource<object> completionSource;

		public Action action_0;

		public Action method_0()
		{
			try
			{
				function();
				return delegate
				{
					completionSource.SetResult(true);
				};
			}
			catch (Exception ex)
			{
				Exception ex2 = ex;
				Exception e = ex2;
				return delegate
				{
					completionSource.SetException(e);
				};
			}
		}

		public void method_1()
		{
			completionSource.SetResult(true);
		}
	}

	[CompilerGenerated]
	public class Class1012
	{
		public Exception e;

		public Class1011 class1011_0;

		public void method_0()
		{
			class1011_0.completionSource.SetException(e);
		}
	}

	[CompilerGenerated]
	private static readonly GClass1516 gclass1516_0 = new GClass1516();

	public static GClass1516 GClass1516_0
	{
		[CompilerGenerated]
		get
		{
			return gclass1516_0;
		}
	}

	public void Start()
	{
		GClass1516_0.CreateThread();
	}

	public void Update()
	{
		GClass1516_0.CheckForFinishedTasks();
	}

	public void FixedUpdate()
	{
		GClass1516_0.CheckForFinishedTasks();
	}

	public void OnDestroy()
	{
		GClass1516_0.Kill();
	}

	public static void RunInMainTread(Action action)
	{
		GClass1516_0.RunInMainTread(action);
	}

	public static bool IsStopped()
	{
		if (GClass1516_0 != null)
		{
			return GClass1516_0.ShouldStop;
		}
		return true;
	}

	public static Task RunOnBackgroundThread(Action function)
	{
		TaskCompletionSource<object> completionSource = new TaskCompletionSource<object>();
		if (IsStopped())
		{
			function();
			completionSource.SetResult(true);
			return completionSource.Task;
		}
		GClass1516_0.AddTask(delegate
		{
			try
			{
				function();
				return delegate
				{
					completionSource.SetResult(true);
				};
			}
			catch (Exception ex)
			{
				Exception ex2 = ex;
				Exception e = ex2;
				return delegate
				{
					completionSource.SetException(e);
				};
			}
		});
		return completionSource.Task;
	}

	public static Task<TResult> RunOnBackgroundThread<TResult>(Func<TResult> function)
	{
		TaskCompletionSource<TResult> completionSource = new TaskCompletionSource<TResult>();
		if (IsStopped())
		{
			TResult result = function();
			completionSource.SetResult(result);
			return completionSource.Task;
		}
		GClass1516_0.AddTask(delegate
		{
			try
			{
				TResult result2 = function();
				return delegate
				{
					completionSource.SetResult(result2);
				};
			}
			catch (Exception ex)
			{
				Exception ex2 = ex;
				Exception e = ex2;
				return delegate
				{
					completionSource.SetException(e);
				};
			}
		});
		return completionSource.Task;
	}

	public static bool CheckIsMainThread()
	{
		return Thread.CurrentThread.ManagedThreadId == GClass1516_0.MainThreadId;
	}
}
