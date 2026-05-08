using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

public class GClass1025
{
	[NonSerialized]
	public const CameraEvent CameraEvent_0 = CameraEvent.AfterForwardAlpha;

	[NonSerialized]
	[CompilerGenerated]
	public CommandBuffer CommandBuffer_0_1;

	[NonSerialized]
	[CompilerGenerated]
	public Camera Camera_0;

	public CommandBuffer CommandBuffer_0
	{
		[CompilerGenerated]
		get
		{
			return CommandBuffer_0_1;
		}
	}

	public Camera Camera
	{
		[CompilerGenerated]
		get
		{
			return Camera_0;
		}
	}

	public GClass1025(Camera camera)
	{
		Camera_0 = camera;
		CommandBuffer_0_1 = new CommandBuffer();
		CommandBuffer_0.name = "FlareRenderer-" + Camera.name;
		Camera.AddCommandBuffer(CameraEvent.AfterForwardAlpha, CommandBuffer_0);
	}

	public void ApplyRenderCommands(GClass1027 batch)
	{
		batch.ApplyRenderCommands(CommandBuffer_0);
	}

	public void Dispose()
	{
		if ((bool)Camera)
		{
			Camera.RemoveCommandBuffer(CameraEvent.AfterForwardAlpha, CommandBuffer_0);
		}
		CommandBuffer_0.Release();
	}

	public override int GetHashCode()
	{
		return Camera.GetHashCode();
	}

	public virtual void StartRendering()
	{
		CommandBuffer_0.Clear();
	}
}
