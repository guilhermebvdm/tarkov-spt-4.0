using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

public class GClass2314 : IDisposable
{
	public class GClass2315
	{
		public readonly GClass4073 Line;

		public readonly Task Task;

		public readonly string Text;

		public GClass2315(GClass4073 line, Task task)
		{
			Line = line;
			Task = task;
			Text = GClass2348.Localized(line.Key);
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class1533
	{
		public static readonly Class1533 class1533_0 = new Class1533();

		public static Func<GStruct284, GClass4073> func_0;

		public GClass4073 method_0(GStruct284 cc)
		{
			return new GClass4073
			{
				Key = cc.Text,
				Start = (float)cc.StartTime,
				End = (float)cc.EndTime
			};
		}
	}

	[CompilerGenerated]
	public class Class1534
	{
		public double startTime;

		public double seconds;

		public bool method_0()
		{
			return AudioSettings.dspTime - startTime >= seconds;
		}
	}

	[CompilerGenerated]
	private Action<GClass2315> action_0;

	[CompilerGenerated]
	private Action action_1;

	[NonSerialized]
	public string String_0;

	[NonSerialized]
	public IEnumerable<GClass4073> Ienumerable_0;

	[NonSerialized]
	public double Double_0;

	[NonSerialized]
	public CompositeDisposableClass CompositeDisposableClass = new CompositeDisposableClass();

	[NonSerialized]
	public CompositeDisposableClass CompositeDisposableClass_1 = new CompositeDisposableClass();

	[NonSerialized]
	public List<Task> List_0 = new List<Task>();

	[NonSerialized]
	public List<GClass2315> List_1 = new List<GClass2315>();

	[NonSerialized]
	[CompilerGenerated]
	public double Double_1;

	public double Duration
	{
		[CompilerGenerated]
		get
		{
			return Double_1;
		}
		[CompilerGenerated]
		set
		{
			Double_1 = value;
		}
	}

	public IEnumerable<GClass2315> CurrentLines => List_1;

	public event Action<GClass2315> OnNewCCLine
	{
		[CompilerGenerated]
		add
		{
			Action<GClass2315> action = action_0;
			Action<GClass2315> action2;
			do
			{
				action2 = action;
				Action<GClass2315> value2 = (Action<GClass2315>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<GClass2315> action = action_0;
			Action<GClass2315> action2;
			do
			{
				action2 = action;
				Action<GClass2315> value2 = (Action<GClass2315>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action OnSubtitlesEnded
	{
		[CompilerGenerated]
		add
		{
			Action action = action_1;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_1;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public GClass2314(IEnumerable<GClass4073> subtitlesData)
	{
		CompositeDisposableClass.Dispose();
		Double_0 = AudioSettings.dspTime;
		method_0(subtitlesData);
		CompositeDisposableClass.AddDisposable(LocaleManagerClass.LocaleManagerClass.AddLocaleUpdateListener(delegate
		{
			method_0(Ienumerable_0);
		}));
		CompositeDisposableClass.AddDisposable(method_3);
	}

	public void method_0(IEnumerable<GClass4073> subtitlesData)
	{
		method_3();
		Ienumerable_0 = subtitlesData;
		Start().HandleExceptions();
	}

	public async Task Start()
	{
		foreach (GClass4073 item in Ienumerable_0)
		{
			List_0.Add(method_1(item));
		}
		Task task = Task.WhenAll(List_0);
		List_0.Clear();
		CancellationToken cancellationToken = CompositeDisposableClass_1.CancellationToken;
		await task;
		if (!cancellationToken.IsCancellationRequested)
		{
			action_1?.Invoke();
			Dispose();
		}
	}

	public async Task method_1(GClass4073 line)
	{
		double num = AudioSettings.dspTime - Double_0;
		if (num >= (double)line.End)
		{
			return;
		}
		float num2 = (float)((double)line.Start - num);
		float num3 = line.End - line.Start;
		Duration = Math.Max(Duration, line.End);
		CancellationToken cancellationToken = CompositeDisposableClass_1.CancellationToken;
		if (!(num2 > 0f))
		{
			num3 += num2;
		}
		else
		{
			await method_2(num2);
			if (cancellationToken.IsCancellationRequested)
			{
				return;
			}
		}
		Task task = method_2(num3);
		GClass2315 gClass = new GClass2315(line, task);
		List_1.Add(gClass);
		action_0?.Invoke(gClass);
		await task;
		List_1.Remove(gClass);
	}

	public Task method_2(double seconds)
	{
		double startTime = AudioSettings.dspTime;
		return TasksExtensions.WaitUntil(() => AudioSettings.dspTime - startTime >= seconds, CompositeDisposableClass_1.CancellationToken);
	}

	public void method_3()
	{
		Duration = 0.0;
		List_1.Clear();
		CompositeDisposableClass_1.Dispose();
	}

	public void Dispose()
	{
		CompositeDisposableClass.Dispose();
	}

	public void method_4()
	{
		string text = GClass2348.Localized(String_0);
		try
		{
			GStruct284[] source = JsonConvert.DeserializeObject<GStruct284[]>(text);
			Ienumerable_0 = source.Select((GStruct284 cc) => new GClass4073
			{
				Key = cc.Text,
				Start = (float)cc.StartTime,
				End = (float)cc.EndTime
			});
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			Debug.LogError(text);
			string text2 = JsonConvert.ToString(String_0).TrimStart('"').TrimEnd('"');
			Ienumerable_0 = new GClass4073[1]
			{
				new GClass4073
				{
					Key = "Corrupted subtitles: \"" + text2 + "\".",
					Start = 0f,
					End = 5f
				}
			};
		}
	}

	[CompilerGenerated]
	public void method_5()
	{
		method_0(Ienumerable_0);
	}
}
