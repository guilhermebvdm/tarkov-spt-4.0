using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Diz.Utils;
using EFT;
using Newtonsoft.Json;

public class VOIPBanDataClass
{
	public const string EXHIBITION_BLOCKED_MESSAGE = "Exhibition/BlockedMessage";

	[CompilerGenerated]
	private Action action_0;

	public readonly EBanType Type;

	public readonly long ExpirationTime;

	[NonSerialized]
	public CancellationTokenSource CancellationTokenSource_0 = new CancellationTokenSource();

	[NonSerialized]
	public Task Task_0;

	[JsonIgnore]
	public bool Expired => TimeLeft.TotalSeconds <= 0.0;

	[JsonIgnore]
	public DateTime BanUntil => EFTDateTimeClass.UniversalDateTimeFromUnixTime(ExpirationTime);

	[JsonIgnore]
	public TimeSpan TimeLeft => TimeSpan.FromSeconds((double)ExpirationTime - EFTDateTimeClass.UtcNowUnix);

	public event Action OnBanExpired
	{
		[CompilerGenerated]
		add
		{
			Action action = action_0;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_0;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public VOIPBanDataClass(GClass2222 descriptor)
		: this(descriptor.Type, descriptor.ExpirationTime)
	{
	}

	public VOIPBanDataClass(EBanType type, long expirationTime)
	{
		Type = type;
		ExpirationTime = expirationTime;
		method_1();
	}

	public VOIPBanDataClass()
	{
	}

	[OnDeserialized]
	public void method_0(StreamingContext context)
	{
		method_1();
	}

	public void method_1()
	{
		lock (this)
		{
			if (Task_0 == null)
			{
				Task_0 = method_2();
				Task_0.HandleExceptions();
			}
		}
	}

	public async Task method_2()
	{
		try
		{
			await Task.Delay(TimeLeft, CancellationTokenSource_0.Token);
		}
		catch (Exception)
		{
			return;
		}
		if (!CancellationTokenSource_0.IsCancellationRequested)
		{
			AsyncWorker.RunInMainTread(delegate
			{
				action_0?.Invoke();
			});
		}
	}

	public void Dispose()
	{
		action_0 = null;
		CancellationTokenSource_0.Cancel();
	}

	[CompilerGenerated]
	public void method_3()
	{
		action_0?.Invoke();
	}
}
