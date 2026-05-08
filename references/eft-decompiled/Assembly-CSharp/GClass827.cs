using System;
using System.Runtime.InteropServices;
using UnityEngine;

public abstract class GClass827
{
	[StructLayout(LayoutKind.Sequential, Size = 72)]
	public struct GStruct50
	{
		public uint cb;

		public uint PageFaultCount;

		public ulong PeakWorkingSetSize;

		public ulong WorkingSetSize;

		public ulong QuotaPeakPagedPoolUsage;

		public ulong QuotaPagedPoolUsage;

		public ulong QuotaPeakNonPagedPoolUsage;

		public ulong QuotaNonPagedPoolUsage;

		public ulong PagefileUsage;

		public ulong PeakPagefileUsage;

		public override string ToString()
		{
			return $"PageFaultCount={PageFaultCount}, PeakWorkingSetSize={PeakWorkingSetSize}, WorkingSetSize={WorkingSetSize}, QuotaPeakPagedPoolUsage={QuotaPeakPagedPoolUsage}, QuotaPagedPoolUsage={QuotaPagedPoolUsage}, QuotaPeakNonPagedPoolUsage={QuotaPeakNonPagedPoolUsage}, QuotaNonPagedPoolUsage={QuotaNonPagedPoolUsage}, PagefileUsage={PagefileUsage}, PeakPagefileUsage={PeakPagefileUsage}";
		}
	}

	[DllImport("kernel32.dll")]
	public static extern IntPtr GetCurrentProcess();

	[DllImport("psapi.dll", SetLastError = true)]
	public static extern bool GetProcessMemoryInfo(IntPtr hProcess, out GStruct50 counters, uint size);

	public static GStruct50 smethod_0()
	{
		IntPtr currentProcess = GetCurrentProcess();
		GStruct50 counters = default(GStruct50);
		counters.cb = (uint)Marshal.SizeOf(typeof(GStruct50));
		if (!GetProcessMemoryInfo(currentProcess, out counters, counters.cb))
		{
			throw new Exception("GetProcessMemoryInfo returned false. Error Code is " + Marshal.GetLastWin32Error());
		}
		return counters;
	}

	public static GStruct50 GetCounters()
	{
		GStruct50 result = default(GStruct50);
		try
		{
			result = smethod_0();
			return result;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		return result;
	}
}
