using System;
using System.Runtime.InteropServices;
using UnityEngine.LowLevel;

namespace CustomPlayerLoopSystem;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct StartOfFrame
{
	public static event Action OnUpdate;

	public static PlayerLoopSystem GetNewSystem()
	{
		return new PlayerLoopSystem
		{
			type = typeof(StartOfFrame),
			updateDelegate = UpdateFunction
		};
	}

	public static void UpdateFunction()
	{
		StartOfFrame.OnUpdate?.Invoke();
	}
}
