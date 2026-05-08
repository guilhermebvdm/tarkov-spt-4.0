using System;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using UnityEngine.LowLevel;

namespace CustomPlayerLoopSystem;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct FrameCounter
{
	[NonSerialized]
	public static int CurrentFrame_1;

	public static int CurrentFrame => CurrentFrame_1;

	public static PlayerLoopSystem GetNewSystem()
	{
		return new PlayerLoopSystem
		{
			type = typeof(FrameCounter),
			updateDelegate = UpdateFunction
		};
	}

	public static void UpdateFunction()
	{
		Interlocked.Exchange(ref CurrentFrame_1, Time.frameCount);
	}
}
