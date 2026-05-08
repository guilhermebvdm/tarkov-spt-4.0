using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine.LowLevel;

public abstract class GClass1469
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate uint Delegate5();

	[CompilerGenerated]
	public class Class932
	{
		public int level;

		public Type type;

		public Delegate5 systemDelegate;

		public void method_0()
		{
			smethod_1(level, type.Name);
			if (systemDelegate != null)
			{
				systemDelegate();
			}
		}
	}

	[NonSerialized]
	public static int Int_0 = 0;

	[NonSerialized]
	public static GClass23 Gclass23_0 = new GClass23();

	[Conditional("DEVELOPMENT_BUILD")]
	public static void smethod_0()
	{
		PlayerLoopSystem defaultPlayerLoop = PlayerLoop.GetDefaultPlayerLoop();
		defaultPlayerLoop = smethod_2(defaultPlayerLoop, 0, defaultPlayerLoop.updateFunction);
		PlayerLoop.SetPlayerLoop(defaultPlayerLoop);
	}

	public static void smethod_1(int level, string name)
	{
		for (int i = level; i <= Int_0; i++)
		{
			Gclass23_0.EndSample();
		}
		Gclass23_0.BeginSample(name);
		Int_0 = level;
	}

	public static PlayerLoopSystem smethod_2(PlayerLoopSystem system, int level, IntPtr nullFnc)
	{
		Delegate5 systemDelegate = null;
		if (system.updateFunction.ToInt64() != 0L)
		{
			IntPtr ptr = Marshal.ReadIntPtr(system.updateFunction);
			if (ptr.ToInt64() != 0L)
			{
				systemDelegate = (Delegate5)Marshal.GetDelegateForFunctionPointer(ptr, typeof(Delegate5));
			}
		}
		Type type = system.type;
		system.updateDelegate = delegate
		{
			smethod_1(level, type.Name);
			if (systemDelegate != null)
			{
				systemDelegate();
			}
		};
		system.updateFunction = nullFnc;
		if (system.subSystemList == null)
		{
			return system;
		}
		for (int num = 0; num < system.subSystemList.Length; num++)
		{
			system.subSystemList[num] = smethod_2(system.subSystemList[num], level + 1, nullFnc);
		}
		return system;
	}
}
