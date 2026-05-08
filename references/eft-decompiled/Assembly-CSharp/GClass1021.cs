using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public abstract class GClass1021
{
	public enum PerfRating
	{
		FPS4,
		FPS16,
		FPS24,
		FPS36,
		FPS60,
		FPS100,
		FPS200
	}

	[Serializable]
	[CompilerGenerated]
	public class Class713
	{
		public static readonly Class713 class713_0 = new Class713();

		public void cctor_003Eb__6_0()
		{
		}
	}

	[CompilerGenerated]
	private static Action action_0;

	[NonSerialized]
	public static Dictionary<string, string> Dictionary_0;

	[NonSerialized]
	public static float Float_0;

	[NonSerialized]
	public static float Float_1;

	[NonSerialized]
	[CompilerGenerated]
	public static PerfRating PerfRating_0;

	[NonSerialized]
	public static float[] Float_2;

	[NonSerialized]
	public static float[] Float_3;

	public static PerfRating PerformanceRating
	{
		[CompilerGenerated]
		get
		{
			return PerfRating_0;
		}
		[CompilerGenerated]
		set
		{
			PerfRating_0 = value;
		}
	}

	public static event Action OnFPSChange
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

	public static void AddLog(string n, object t)
	{
		Dictionary_0.Add(n, t.ToString());
	}

	public static Dictionary<string, string> GetSystemInfo()
	{
		AddLog("deviceModel", SystemInfo.deviceModel);
		AddLog("deviceType", SystemInfo.deviceType);
		AddLog("graphicsDeviceName", SystemInfo.graphicsDeviceName);
		AddLog("graphicsDeviceVendor", SystemInfo.graphicsDeviceVendor);
		AddLog("graphicsDeviceVersion", SystemInfo.graphicsDeviceVersion);
		AddLog("graphicsMemorySize", SystemInfo.graphicsMemorySize);
		AddLog("graphicsShaderLevel", SystemInfo.graphicsShaderLevel);
		AddLog("maxTextureSize", SystemInfo.maxTextureSize);
		AddLog("processorCount", SystemInfo.processorCount);
		AddLog("processorType", SystemInfo.processorType);
		AddLog("supportedRenderTargetCount", SystemInfo.supportedRenderTargetCount);
		AddLog("supports3DTextures", SystemInfo.supports3DTextures);
		AddLog("supportsImageEffects", SystemInfo.supportsImageEffects);
		AddLog("supportsShadows", SystemInfo.supportsShadows);
		AddLog("systemMemorySize", SystemInfo.systemMemorySize);
		return Dictionary_0;
	}

	static GClass1021()
	{
		action_0 = delegate
		{
		};
		Dictionary_0 = new Dictionary<string, string>();
		Float_0 = float.MinValue;
		Float_2 = new float[7] { -9999f, 7f, 18f, 28f, 40f, 70f, 130f };
		Float_3 = new float[7] { 12f, 22f, 32f, 50f, 90f, 170f, 999999f };
		Debug.LogError("!!!!!!!!!!!ALLLARM!!!");
		Update();
	}

	public static void Update()
	{
		if (Time.time < Float_0)
		{
			return;
		}
		Float_0 = Time.time + 0.25f;
		Float_1 = Mathf.Lerp(Float_1, 1f / Time.deltaTime, 1f / 30f);
		int num = (int)PerformanceRating;
		int num2 = num;
		float num3 = Float_2[num];
		float num4 = Float_3[num];
		if (Float_1 < num3)
		{
			num--;
		}
		else if (Float_1 > num4)
		{
			num++;
		}
		if (num2 != num)
		{
			if (num > 0 && num < 7)
			{
				PerformanceRating = (PerfRating)num;
			}
			action_0();
		}
	}
}
