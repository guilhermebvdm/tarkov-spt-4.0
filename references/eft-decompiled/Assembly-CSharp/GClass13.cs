using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using CustomPlayerLoopSystem;
using UnityEngine.Profiling;

public class GClass13 : IDisposable
{
	[NonSerialized]
	public static GClass13 Gclass13_0;

	[NonSerialized]
	public Queue<string> Queue_0 = new Queue<string>();

	[NonSerialized]
	public WaitCallback WaitCallback_0;

	[NonSerialized]
	public CustomSampler CustomSampler_0;

	[NonSerialized]
	public Func<int, string> Func_0;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public int Int_2;

	[NonSerialized]
	public int Int_3;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public bool Bool_1;

	[CompilerGenerated]
	private Action action_0;

	[CompilerGenerated]
	private Action action_1;

	public const string UpdateSamplerName = "EmbeddedProfiler.Update";

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_2;

	public int CurrentFrameCounter => Int_2;

	public static GClass13 Instance => Gclass13_0 ?? (Gclass13_0 = new GClass13());

	public bool Started
	{
		[CompilerGenerated]
		get
		{
			return Bool_2;
		}
		[CompilerGenerated]
		set
		{
			Bool_2 = value;
		}
	}

	public bool Paused
	{
		get
		{
			return Bool_0;
		}
		set
		{
			if (Started && Bool_0 != value)
			{
				Bool_0 = value;
				Profiler.enabled = !value;
			}
		}
	}

	public event Action OnUpdate
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

	public event Action BeforeNewFileCreated
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

	public GClass13()
	{
		WaitCallback_0 = smethod_0;
		CustomSampler_0 = CustomSampler.Create("EmbeddedProfiler.Update");
		StartOfFrame.OnUpdate += Update;
	}

	public void Dispose()
	{
		StopProfiling();
		StartOfFrame.OnUpdate -= Update;
	}

	public void StartProfiling(Func<int, string> fileNameGetter, int framesPerFile, int maxFilesCount, bool paused, params ProfilerArea[] profilerAreas)
	{
		StopProfiling();
		Func_0 = fileNameGetter;
		Int_1 = maxFilesCount;
		Int_0 = framesPerFile;
		Int_2 = 0;
		Int_3 = 0;
		Queue_0.Clear();
		EnableAreas(profilerAreas);
		method_0();
		Started = true;
		Bool_0 = paused;
		Profiler.enabled = !paused;
	}

	public void StopProfiling()
	{
		if (Started)
		{
			Profiler.enableBinaryLog = false;
			Profiler.enabled = false;
			Profiler.logFile = "";
			Started = false;
			Bool_0 = false;
		}
	}

	public static void EnableAreas(params ProfilerArea[] profilerAreas)
	{
		for (int i = 0; i < Profiler.areaCount; i++)
		{
		}
		if (profilerAreas != null && profilerAreas.Length != 0)
		{
			for (int j = 0; j < profilerAreas.Length; j++)
			{
			}
		}
	}

	public void method_0()
	{
		Profiler.logFile = method_1();
		Profiler.enableBinaryLog = true;
	}

	public void Update()
	{
		if (!Started || Bool_0)
		{
			return;
		}
		action_0?.Invoke();
		if (Int_2 >= Int_0 - 1)
		{
			Profiler.enableBinaryLog = false;
			action_1?.Invoke();
			method_0();
			if (Queue_0.Count > Int_1)
			{
				string state = Queue_0.Dequeue();
				if (!Bool_1)
				{
					ThreadPool.QueueUserWorkItem(WaitCallback_0, state);
				}
				else
				{
					Bool_1 = false;
				}
			}
			Int_2 = 0;
		}
		else
		{
			Int_2++;
		}
	}

	public void KeepCurrentFile()
	{
		Bool_1 = true;
	}

	public static void smethod_0(object state)
	{
		if (state is string path && File.Exists(path))
		{
			File.Delete(path);
		}
	}

	public void PruneAllFiles()
	{
		while (Queue_0.Count > 0)
		{
			smethod_0(Queue_0.Dequeue());
		}
	}

	[CompilerGenerated]
	public string method_1()
	{
		string text = Func_0(Int_3);
		if (!Queue_0.Contains(text))
		{
			Queue_0.Enqueue(text);
		}
		Int_3++;
		return text;
	}
}
