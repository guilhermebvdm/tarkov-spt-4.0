using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using UnityEngine.Profiling;

public abstract class GClass14
{
	[Flags]
	public enum EPauseReason : byte
	{
		None = 0,
		Inventory = 1,
		Snapshot = 2,
		AltTab = 4,
		All = 7
	}

	public enum DetectType
	{
		Loop,
		FixedUpdate,
		SingleAVGFixedUpdate,
		LoopWithoutFixedUpdate,
		LoopWithSingleAVGFixedUpdateOnly
	}

	[NonSerialized]
	[CompilerGenerated]
	public static EPauseReason EpauseReason_0;

	[NonSerialized]
	public static GClass16 Gclass16_0;

	[NonSerialized]
	public static Rigidbody2D Rigidbody2D_0;

	[NonSerialized]
	public static Dictionary<string, long> Dictionary_0 = new Dictionary<string, long>();

	public static EPauseReason PauseReason
	{
		[CompilerGenerated]
		get
		{
			return EpauseReason_0;
		}
		[CompilerGenerated]
		set
		{
			EpauseReason_0 = value;
		}
	}

	public static bool Paused => PauseReason != EPauseReason.None;

	public static ApplicationConfigClass.ProfilingSettings ProfilingSettings_0 => BackendConfigAbstractClass.Config.Profiling;

	[Conditional("DEBUG")]
	public static void StartSpikeAnalyzer(float triggerTime)
	{
		List<GStruct2> list = new List<GStruct2>();
		List<GStruct2> list2 = new List<GStruct2>();
		GStruct2 item = new GStruct2("PlayerLoop");
		List<GStruct2> list3 = new List<GStruct2>
		{
			new GStruct2("FixedUpdate.ScriptRunBehaviourFixedUpdate"),
			new GStruct2("FixedUpdate.ScriptRunDelayedFixedFrameRate"),
			new GStruct2("FixedUpdate.DirectorFixedUpdate"),
			new GStruct2("FixedUpdate.DirectorFixedUpdatePostPhysics"),
			new GStruct2("PreLateUpdate.UNetUpdate")
		};
		List<GStruct2> list4 = new List<GStruct2>();
		foreach (GStruct2 item2 in list3)
		{
			list4.Add(new GStruct2(item2.SamplerName, divideBySamplesCount: true));
		}
		switch (ProfilingSettings_0.DetectType)
		{
		default:
			throw new ArgumentOutOfRangeException("DetectType");
		case DetectType.Loop:
			list.Add(item);
			break;
		case DetectType.FixedUpdate:
			list.AddRange(list3);
			break;
		case DetectType.SingleAVGFixedUpdate:
			list.AddRange(list4);
			break;
		case DetectType.LoopWithoutFixedUpdate:
			list.Add(item);
			list2.AddRange(list3);
			break;
		case DetectType.LoopWithSingleAVGFixedUpdateOnly:
			list.Add(item);
			list2.AddRange(list3);
			list.AddRange(list4);
			break;
		}
		string workingDirectory = (string.IsNullOrEmpty(ProfilingSettings_0.WorkingDirectory) ? (Application.dataPath + "/SpikeAnalyzer/" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff")) : ProfilingSettings_0.WorkingDirectory);
		Gclass16_0 = new GClass16(workingDirectory, list, list2, triggerTime, ProfilingSettings_0.FramesCountPerFile, Paused, ProfilingSettings_0.ProfilingAreas);
	}

	[Conditional("DEBUG")]
	public static void StopSpikeAnalyzer(bool resetPauseState = true)
	{
		if (resetPauseState)
		{
			PauseReason = EPauseReason.None;
		}
		Interlocked.Exchange(ref Gclass16_0, null)?.Dispose();
	}

	[Conditional("DEBUG")]
	public static void TryStartSpikeAnalyzer()
	{
		_ = ProfilingSettings_0.EnableInGameSpikeAnalyzer;
	}

	[Conditional("DEBUG")]
	public static void TryStopSpikeAnalyzer(bool resetPauseState = true)
	{
		_ = ProfilingSettings_0.EnableInGameSpikeAnalyzer;
	}

	[Conditional("DEBUG")]
	public static void TryPauseSpikeAnalyzer(EPauseReason pauseReason)
	{
		if ((ProfilingSettings_0.PauseReason & pauseReason) == 0)
		{
			return;
		}
		EPauseReason ePauseReason = PauseReason | pauseReason;
		if (ePauseReason != PauseReason)
		{
			bool num = PauseReason == EPauseReason.None;
			PauseReason = ePauseReason;
			if (num)
			{
				Gclass16_0?.Pause();
			}
		}
	}

	[Conditional("DEBUG")]
	public static void TryResumeSpikeAnalyzer(EPauseReason pauseReason)
	{
		if ((ProfilingSettings_0.PauseReason & pauseReason) == 0)
		{
			return;
		}
		EPauseReason ePauseReason = (EPauseReason)((uint)PauseReason & (uint)(byte)(~(int)pauseReason));
		if (ePauseReason != PauseReason)
		{
			PauseReason = ePauseReason;
			if (PauseReason == EPauseReason.None)
			{
				Gclass16_0?.Resume();
			}
		}
	}

	[Conditional("DEBUG")]
	public static void InitMarks()
	{
		GameObject gameObject = new GameObject("ProfilerHelper_mark (debug only)");
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		Rigidbody2D_0 = gameObject.AddComponent<Rigidbody2D>();
		Rigidbody2D_0.bodyType = RigidbodyType2D.Kinematic;
		Physics2D.simulationMode = SimulationMode2D.Script;
		Physics2D.autoSyncTransforms = false;
	}

	[Conditional("DEBUG")]
	public static void MakeMark()
	{
		Rigidbody2D_0.bodyType = ((Rigidbody2D_0.bodyType != RigidbodyType2D.Kinematic) ? RigidbodyType2D.Kinematic : RigidbodyType2D.Static);
	}

	public static void PrintMemoryUsage()
	{
		smethod_0("Profiler.GetTotalReservedMemory", Profiler.GetTotalReservedMemoryLong());
		smethod_0("Profiler.GetTotalAllocatedMemory", Profiler.GetTotalAllocatedMemoryLong());
		smethod_0("Profiler.GetTotalUnusedReservedMemory", Profiler.GetTotalUnusedReservedMemoryLong());
		smethod_0("Profiler.GetMonoHeapSize", Profiler.GetMonoHeapSizeLong());
		smethod_0("Profiler.GetMonoUsedSize", Profiler.GetMonoUsedSizeLong());
		smethod_0("GC.GetTotalMemory", (uint)GC.GetTotalMemory(forceFullCollection: true));
	}

	public static void smethod_0(string description, long newValue)
	{
		Dictionary_0.TryGetValue(description, out var value);
		long num = newValue - value;
		string text;
		string text2;
		if (num > 0L)
		{
			text = "+";
			text2 = "red";
		}
		else
		{
			text = "-";
			text2 = "blue";
		}
		UnityEngine.Debug.LogFormat("{0}: {1} <color={2}>{3}{4}</color>", description, GClass856.SplitNumber(newValue, "."), text2, text, GClass856.SplitNumber(Math.Abs(num), "."));
		Dictionary_0[description] = newValue;
	}
}
