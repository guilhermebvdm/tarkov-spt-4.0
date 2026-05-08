using System;
using System.Runtime.InteropServices;
using UnityEngine.LowLevel;

namespace CustomPlayerLoopSystem;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct EndOfFixedUpdate
{
	public static event Action OnUpdate;

	public static PlayerLoopSystem GetNewSystem()
	{
		return new PlayerLoopSystem
		{
			type = typeof(EndOfFixedUpdate),
			updateDelegate = UpdateFunction
		};
	}

	public static void UpdateFunction()
	{
		EndOfFixedUpdate.OnUpdate?.Invoke();
	}
}
