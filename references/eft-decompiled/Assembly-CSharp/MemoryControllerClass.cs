using System;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

public abstract class MemoryControllerClass
{
	[Serializable]
	public struct MemoryManagementSettings
	{
		public static readonly MemoryManagementSettings Default = new MemoryManagementSettings
		{
			HeapPreAllocationEnabled = false,
			HeapPreAllocationMB = 200,
			OverrideRamCleanerSettings = false,
			RamCleanerEnabled = true,
			GigabytesRequiredToDisableGCDuringRaid = 12,
			AggressiveGC = true
		};

		public bool HeapPreAllocationEnabled;

		public int HeapPreAllocationMB;

		public bool OverrideRamCleanerSettings;

		public bool RamCleanerEnabled;

		public int GigabytesRequiredToDisableGCDuringRaid;

		public bool AggressiveGC;
	}

	[Serializable]
	[CompilerGenerated]
	public class Class491
	{
		public static readonly Class491 class491_0 = new Class491();

		public void method_0(string s)
		{
			UnityEngine.Debug.Log(s);
		}
	}

	[NonSerialized]
	public const float Float_0 = 600f;

	[NonSerialized]
	public static float Float_1;

	[NonSerialized]
	[CompilerGenerated]
	public static MemoryManagementSettings MemoryManagementSettings_0;

	public static Action<string> Logger = delegate(string s)
	{
		UnityEngine.Debug.Log(s);
	};

	public static float Single_0 => (float)SystemInfo.systemMemorySize / 1024f;

	public static MemoryManagementSettings Settings
	{
		[CompilerGenerated]
		get
		{
			return MemoryManagementSettings_0;
		}
		[CompilerGenerated]
		set
		{
			MemoryManagementSettings_0 = value;
		}
	}

	public static bool GCEnabled
	{
		get
		{
			return GarbageCollector.GCMode == GarbageCollector.Mode.Enabled;
		}
		set
		{
			if (value != GCEnabled)
			{
				if (!value && Single_0 < (float)Settings.GigabytesRequiredToDisableGCDuringRaid)
				{
					UnityEngine.Debug.LogWarningFormat("Can't disable GC due to low memory size {0}GB, requested {1}GB", Single_0, Settings.GigabytesRequiredToDisableGCDuringRaid);
				}
				else
				{
					GarbageCollector.Mode mode = (value ? GarbageCollector.Mode.Enabled : GarbageCollector.Mode.Disabled);
					Logger?.Invoke($"GC mode switched to {mode}");
					GarbageCollector.GCMode = mode;
				}
			}
		}
	}

	public static void Init(MemoryManagementSettings settings)
	{
		Settings = settings;
	}

	public static void EmptyWorkingSet()
	{
		UnityEngine.Debug.Log("EmptyWorkingSet");
		EmptyWorkingSet_1(Process.GetCurrentProcess().Handle);
	}

	public static float Collect(bool force = false)
	{
		return Collect(2, GCCollectionMode.Optimized, isBlocking: true, compacting: false, force);
	}

	public static float Collect(int generation, GCCollectionMode gcMode, bool isBlocking, bool compacting, bool force)
	{
		float num = 0f;
		if (!force && Time.time < Float_1 + 600f)
		{
			return num;
		}
		bool gCEnabled = GCEnabled;
		if (force || gCEnabled)
		{
			float totalAllocatedMemoryGB = GetTotalAllocatedMemoryGB();
			Logger?.Invoke(string.Format("{0} {1}", "totalMemoryBeforeCleanUp", totalAllocatedMemoryGB));
			GCEnabled = true;
			Logger?.Invoke("GC::Collect");
			GC.Collect();
			if (Settings.AggressiveGC)
			{
				Logger?.Invoke("GC::Collect Aggressive");
				GC.WaitForPendingFinalizers();
				GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
				GC.Collect(generation, gcMode, isBlocking, compacting);
			}
			num = totalAllocatedMemoryGB - GetTotalAllocatedMemoryGB();
			Logger?.Invoke(string.Format("{0} {1}", "collectedMemoryGB", num));
			GCEnabled = gCEnabled;
		}
		Float_1 = Time.time;
		return num;
	}

	public static async Task CollectAsync()
	{
		float totalAllocatedMemoryGB = GetTotalAllocatedMemoryGB();
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		bool flag = true;
		while (flag && !(Time.realtimeSinceStartup - realtimeSinceStartup >= 5f))
		{
			float realtimeSinceStartup2 = Time.realtimeSinceStartup;
			flag = GarbageCollector.CollectIncremental(50000000uL);
			Logger?.Invoke($"GC::CollectAsync iteration: {Time.realtimeSinceStartup - realtimeSinceStartup2}s {flag}");
			await Task.Yield();
		}
		Float_1 = Time.time;
		float num = totalAllocatedMemoryGB - GetTotalAllocatedMemoryGB();
		Logger?.Invoke($"GC::CollectAsync time {Time.realtimeSinceStartup - realtimeSinceStartup}s {totalAllocatedMemoryGB} - collected {num}");
	}

	public static float GetTotalAllocatedMemoryGB()
	{
		return (float)GC.GetTotalMemory(forceFullCollection: true) / 1024f / 1024f;
	}

	public static void RunHeapPreAllocation()
	{
		if (!Settings.HeapPreAllocationEnabled)
		{
			Logger?.Invoke("Heap pre-allocation - disabled");
			return;
		}
		Stopwatch stopwatch = Stopwatch.StartNew();
		int num = Math.Max(0, Settings.HeapPreAllocationMB);
		Logger?.Invoke($"Heap pre-allocation - {num}MBs");
		if (num > 0)
		{
			object[] array = new object[1024 * num];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new byte[1024];
			}
			array = null;
			stopwatch.Stop();
			Logger?.Invoke($"Heap pre-allocation for {num} mBs took {stopwatch.ElapsedMilliseconds}");
		}
	}

	[DllImport("psapi.dll", EntryPoint = "EmptyWorkingSet")]
	public static extern bool EmptyWorkingSet_1(IntPtr hProcess);
}
