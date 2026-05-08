using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Diz.Jobs;

public class JobScheduler : MonoBehaviour
{
	public class GClass1619
	{
		public long WorkCpuLoad;

		public float WorkCpuLoadPercent;

		public GClass1621 AvgWorkCpuLoad = new GClass1621();

		public float MaxWorkCpuLoadPercent;

		public override string ToString()
		{
			return $"CurrentWorkCpuLoad: {WorkCpuLoad}\nCurrentWorkCpuLoadPercent: {WorkCpuLoadPercent}\nAvgWorkCpuLoad ({AvgWorkCpuLoad.Count}): {AvgWorkCpuLoad.Average}\nMaxWorkCpuLoadPercent: {MaxWorkCpuLoadPercent}";
		}
	}

	public static int FreeTimeWindowFrame;

	public static float MaxAvgFrameLoad;

	private static GClass1622 gclass1622_0 = new GClass1622(100);

	public static readonly GClass1619 WorkingStats = new GClass1619();

	private static bool bool_0;

	private const int int_0 = 1024;

	private static List<Struct263> list_0 = new List<Struct263>(1024);

	private static List<Struct263> list_1 = new List<Struct263>(1024);

	private static List<Struct263> list_2 = new List<Struct263>(1024);

	private static List<Struct263> list_3 = new List<Struct263>(1024);

	private static List<Struct263> list_4 = new List<Struct263>();

	private Class1031 class1031_0;

	private static Class1032 class1032_0 = new Class1032();

	internal static readonly Class1029 class1029_0 = new Class1029();

	private static readonly Class1028 class1028_0 = new Class1028();

	internal static Class1030 class1030_0 = new Class1030();

	private static readonly WaitForEndOfFrame waitForEndOfFrame_0 = new WaitForEndOfFrame();

	private readonly Stopwatch stopwatch_0 = new Stopwatch();

	private readonly Stopwatch stopwatch_1 = new Stopwatch();

	private bool bool_1;

	public float DefaultForceModeMultiplier = 3f;

	private float float_0 = 3f;

	[CompilerGenerated]
	private long long_0 = 160000L;

	[CompilerGenerated]
	private byte byte_0 = 6;

	private int int_1;

	private int int_2;

	public static int AvgTimeBufferSize
	{
		get
		{
			return gclass1622_0?.BufferSize ?? 0;
		}
		set
		{
			gclass1622_0 = new GClass1622(value);
		}
	}

	public long FrameTicks
	{
		[CompilerGenerated]
		get
		{
			return long_0;
		}
		[CompilerGenerated]
		set
		{
			long_0 = value;
		}
	}

	public long LoopTicks => FrameTicks / 2L;

	public byte SlowFrames
	{
		[CompilerGenerated]
		get
		{
			return byte_0;
		}
		[CompilerGenerated]
		set
		{
			byte_0 = value;
		}
	}

	public bool Boolean_0
	{
		get
		{
			if (stopwatch_0.ElapsedTicks >= FrameTicks && int_1 <= SlowFrames)
			{
				return bool_1;
			}
			return true;
		}
	}

	public static bool Boolean_1
	{
		get
		{
			if (list_0.Count <= 0 && list_2.Count <= 0)
			{
				return list_4.Count > 0;
			}
			return true;
		}
	}

	public bool Boolean_2 => (float)stopwatch_1.ElapsedTicks <= (float)LoopTicks * (bool_1 ? float_0 : 1f);

	public static int QueueLength => list_0.Count + list_2.Count;

	public bool IsForceModeEnabled => bool_1;

	public void Init(bool enableContinuationProfiler)
	{
		class1030_0.Enabled = enableContinuationProfiler;
		class1031_0 = new Class1031((Struct263 continuation) => Boolean_0 && !continuation.wasExecuted);
	}

	public void SetForceMode(bool enable, float forceModeMultiplier = -1f)
	{
		if (forceModeMultiplier < 0f)
		{
			forceModeMultiplier = DefaultForceModeMultiplier;
		}
		UnityEngine.Debug.Log(string.Format("{0} {1}, {2}={3}", "SetForceMode", enable, "forceModeMultiplier", forceModeMultiplier));
		bool_1 = enable;
		float_0 = forceModeMultiplier;
	}

	public void SetTargetFrameRate(int frameRate)
	{
		if (frameRate > 0)
		{
			int num = 1000 / frameRate;
			FrameTicks = TimeSpan.FromMilliseconds(num).Ticks;
			UnityEngine.Debug.Log("FrameTicks:" + FrameTicks + " frameRate:" + frameRate);
		}
	}

	public IEnumerator Start()
	{
		stopwatch_0.Start();
		while (true)
		{
			yield return waitForEndOfFrame_0;
			stopwatch_0.Restart();
		}
	}

	public void LateUpdate()
	{
		long elapsedTicks = stopwatch_0.ElapsedTicks;
		gclass1622_0.AddValue(elapsedTicks);
		method_0();
		stopwatch_0.Restart();
	}

	public void method_0()
	{
		if (!Boolean_1)
		{
			return;
		}
		stopwatch_1.Restart();
		int_2 = QueueLength;
		int_1++;
		while (Boolean_0 && Boolean_1)
		{
			method_1(class1031_0);
			if (int_1 > SlowFrames)
			{
				int_1 = SlowFrames / 2;
			}
			if (!Boolean_2)
			{
				break;
			}
		}
		if (int_2 <= 0)
		{
			int_1 = 0;
		}
	}

	public void method_1(Interface5 executeCondition)
	{
		if (bool_0)
		{
			UnityEngine.Debug.LogError("Trying to call ManualUpdate from the inside of ManualUpdate");
			return;
		}
		bool_0 = true;
		method_2(ref list_2, ref list_3, list_3, executeCondition);
		method_2(ref list_0, ref list_1, list_2, executeCondition);
		bool_0 = false;
	}

	public void method_2(ref List<Struct263> continuations, ref List<Struct263> continuationsDoubleBuffer, List<Struct263> delayedContinuations, Interface5 executeCondition)
	{
		if (continuations.Count == 0)
		{
			return;
		}
		list_4 = continuations;
		continuations = continuationsDoubleBuffer;
		continuationsDoubleBuffer = list_4;
		for (int i = 0; i < list_4.Count; i++)
		{
			Struct263 continuation = list_4[i];
			if (executeCondition.CanExecute(continuation) && Boolean_2)
			{
				int_2++;
				executeCondition.Execute(ref continuation);
				list_4[i] = continuation;
			}
			else if (!continuation.wasExecuted)
			{
				delayedContinuations.Add(continuation);
			}
		}
		list_4.Clear();
	}

	public static bool ForceExecuteContinuations(JobYieldClass jobYield)
	{
		class1032_0.Init(jobYield);
		while (jobYield.IsInProgress && Boolean_1)
		{
			class1032_0.HasExecution = false;
			smethod_0(list_4);
			smethod_0(list_2);
			smethod_0(list_0);
			if (!class1032_0.HasExecution)
			{
				UnityEngine.Debug.LogErrorFormat("No continuations found for {0}", jobYield.Name);
				return false;
			}
		}
		return true;
	}

	public static void smethod_0(List<Struct263> continuations)
	{
		for (int i = 0; i < continuations.Count; i++)
		{
			Struct263 continuation = continuations[i];
			if (class1032_0.CanExecute(continuation))
			{
				class1032_0.Execute(ref continuation);
				continuations[i] = continuation;
			}
		}
	}

	public static void smethod_1(Action action, JobYieldClass jobYield)
	{
		list_0.Add(new Struct263(action, jobYield));
	}

	public static GInterface150 Yield(EJobPriority priority = EJobPriority.General, JobYieldClass jobYield = null)
	{
		switch (priority)
		{
		default:
			throw new NotImplementedException();
		case EJobPriority.Immediate:
			return class1028_0.Init(jobYield, runSynchronously: true);
		case EJobPriority.Low:
		case EJobPriority.General:
			return class1028_0.Init(jobYield, runSynchronously: false);
		}
	}

	public static string GetPerformanceReport()
	{
		return $"Stats: {WorkingStats}\n\nContinuationProfiler: {class1030_0.GetReport()}";
	}

	[CompilerGenerated]
	public bool method_3(Struct263 continuation)
	{
		if (Boolean_0)
		{
			return !continuation.wasExecuted;
		}
		return false;
	}
}
