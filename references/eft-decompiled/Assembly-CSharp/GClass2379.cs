using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;

public abstract class GClass2379
{
	[CompilerGenerated]
	public class Class1815
	{
		public Tween self;

		public TaskCompletionSource<Tween> source;

		public void method_0()
		{
			// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
			Tween tween = self;
			tween.onComplete = (TweenCallback)Delegate.Remove(tween.onComplete, new TweenCallback(method_0));
			source.SetResult(self);
		}
	}

	[CompilerGenerated]
	public class Class1816
	{
		public Tween tween;

		public TaskCompletionSource<bool> taskCompletionSource;

		public void method_0(object obj)
		{
			tween.Kill();
		}

		public void method_1()
		{
			taskCompletionSource.TrySetResult(result: true);
		}

		public void method_2()
		{
			taskCompletionSource.TrySetCanceled();
		}
	}

	public static TaskAwaiter<Tween> GetAwaiter(this Tween self)
	{
		Class1815 CS_0024_003C_003E8__locals8 = new Class1815();
		CS_0024_003C_003E8__locals8.self = self;
		CS_0024_003C_003E8__locals8.source = new TaskCompletionSource<Tween>();
		Tween tween = CS_0024_003C_003E8__locals8.self;
		tween.onComplete = (TweenCallback)Delegate.Combine(tween.onComplete, (TweenCallback)delegate
		{
			// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
			Tween tween2 = CS_0024_003C_003E8__locals8.self;
			tween2.onComplete = (TweenCallback)Delegate.Remove(tween2.onComplete, new TweenCallback(CS_0024_003C_003E8__locals8.method_0));
			CS_0024_003C_003E8__locals8.source.SetResult(CS_0024_003C_003E8__locals8.self);
		});
		return CS_0024_003C_003E8__locals8.source.Task.GetAwaiter();
	}

	public static Task AsTask(this Tween tween, CancellationToken cancellationToken)
	{
		TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
		cancellationToken.Register(delegate
		{
			tween.Kill();
		}, taskCompletionSource);
		tween.OnComplete(delegate
		{
			taskCompletionSource.TrySetResult(result: true);
		});
		tween.OnKill(delegate
		{
			taskCompletionSource.TrySetCanceled();
		});
		return taskCompletionSource.Task;
	}
}
