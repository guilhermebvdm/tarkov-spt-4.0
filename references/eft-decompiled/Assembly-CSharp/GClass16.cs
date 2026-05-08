using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Profiling;

public class GClass16 : IDisposable
{
	public struct Struct5
	{
		public string path;

		public float time;

		public int profilerFrame;

		public int gameFrame;
	}

	public class Class97
	{
		[NonSerialized]
		public Recorder Recorder_0;

		[NonSerialized]
		public bool Bool_0;

		public Class97(Recorder recorder, bool divideBySamplesCount = false)
		{
			Recorder_0 = recorder;
			Bool_0 = divideBySamplesCount;
		}

		public long method_0()
		{
			long num = Recorder_0.elapsedNanoseconds;
			if (Bool_0 && Recorder_0.sampleBlockCount > 0)
			{
				num /= Recorder_0.sampleBlockCount;
			}
			return num;
		}
	}

	[NonSerialized]
	public Class97[] Class97_0;

	[NonSerialized]
	public Class97[] Class97_1;

	[NonSerialized]
	public Recorder Recorder_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public string String_0;

	[NonSerialized]
	public string String_1;

	[NonSerialized]
	public WaitCallback WaitCallback_0;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public Struct5 Struct5_0;

	[NonSerialized]
	public string String_2;

	public GClass16(string workingDirectory, IReadOnlyList<GStruct2> addRecorders, IReadOnlyList<GStruct2> subtractRecorders, float triggerTime, int framesPerFile, bool paused, params ProfilerArea[] profilerAreas)
	{
		String_0 = workingDirectory;
		Float_0 = triggerTime;
		Recorder_0 = Recorder.Get("EmbeddedProfiler.Update");
		Class97_0 = smethod_0(addRecorders);
		Class97_1 = smethod_0(subtractRecorders);
		if (!Directory.Exists(String_0))
		{
			Directory.CreateDirectory(String_0);
		}
		String_1 = Path.GetFileName(String_0);
		WaitCallback_0 = method_3;
		GClass13.Instance.OnUpdate += method_1;
		GClass13.Instance.BeforeNewFileCreated += method_2;
		GClass13.Instance.StartProfiling(method_0, framesPerFile, 1, paused, profilerAreas);
	}

	public void Dispose()
	{
		GClass13.Instance.OnUpdate -= method_1;
		GClass13.Instance.BeforeNewFileCreated -= method_2;
		GClass13.Instance.StopProfiling();
		GClass13.Instance.PruneAllFiles();
	}

	public static Class97[] smethod_0(IReadOnlyList<GStruct2> configs)
	{
		Class97[] array = new Class97[configs.Count];
		for (int i = 0; i < configs.Count; i++)
		{
			GStruct2 gStruct = configs[i];
			Recorder recorder = Recorder.Get(gStruct.SamplerName);
			array[i] = new Class97(recorder, gStruct.DivideBySamplesCount);
		}
		return array;
	}

	public string method_0(int fileNumber)
	{
		String_2 = $"{String_0}/{String_1}__{Int_0}";
		return String_2;
	}

	public static long smethod_1(Class97[] recorders)
	{
		long num = 0L;
		foreach (Class97 @class in recorders)
		{
			num += @class.method_0();
		}
		return num;
	}

	public void method_1()
	{
		long num = smethod_1(Class97_0);
		long num2 = smethod_1(Class97_1);
		float num3 = (float)(num - num2 - Recorder_0.elapsedNanoseconds) / 1000000f;
		if (!(num3 < Float_0) && num3 > Struct5_0.time)
		{
			Struct5_0.time = num3;
			Struct5_0.profilerFrame = GClass13.Instance.CurrentFrameCounter;
			Struct5_0.gameFrame = Time.frameCount;
		}
	}

	public void method_2()
	{
		if (!(Struct5_0.time <= 0f))
		{
			GClass13.Instance.KeepCurrentFile();
			Struct5_0.path = String_2;
			ThreadPool.QueueUserWorkItem(WaitCallback_0, Struct5_0);
			Int_0++;
			Struct5_0 = default(Struct5);
		}
	}

	public void Pause()
	{
		GClass13.Instance.Paused = true;
	}

	public void Resume()
	{
		GClass13.Instance.Paused = false;
	}

	public void method_3(object state)
	{
		Struct5 @struct = (Struct5)state;
		string text = @struct.path + ".raw";
		if (File.Exists(text))
		{
			File.Move(text, $"{Path.GetDirectoryName(text)}/{Path.GetFileNameWithoutExtension(text)}__gf{@struct.gameFrame}__pf{@struct.profilerFrame}__{@struct.time:###.##}ms{Path.GetExtension(text)}");
		}
	}
}
