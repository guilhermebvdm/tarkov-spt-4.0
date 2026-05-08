using System;
using System.Collections.Generic;
using System.Diagnostics;
using EFT;
using UnityEngine;

public class BotUnityEditorRunChecker(BotOwner owner) : GClass429(owner)
{
	[NonSerialized]
	public List<GStruct24> List = new List<GStruct24>();

	public void Activate()
	{
	}

	public void ManualLateUpdate()
	{
	}

	public void method_0(bool obj)
	{
		if (!GClass4062.enabled)
		{
			StackTrace callstack = new StackTrace();
			List.Add(new GStruct24
			{
				callstack = callstack,
				val = obj,
				FrameIndex = Time.frameCount
			});
		}
	}

	public void Dispose()
	{
	}
}
