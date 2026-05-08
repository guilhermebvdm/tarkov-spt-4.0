using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GClass1001
{
	public readonly Dictionary<Camera, CommandBuffer> Cameras = new Dictionary<Camera, CommandBuffer>();

	public readonly Dictionary<Camera, SSAA> SSAAs = new Dictionary<Camera, SSAA>();

	public readonly string BufferName;

	public readonly CameraEvent CameraEvent;

	public void method_0(Camera camera, CommandBuffer cmdBuf)
	{
		SSAA component = camera.GetComponent<SSAA>();
		Cameras.Add(camera, cmdBuf);
		SSAAs.Add(camera, component);
	}

	public GClass1001(string bufferName, CameraEvent cameraEvent)
	{
		BufferName = bufferName;
		CameraEvent = cameraEvent;
	}

	public CommandBuffer OnRenderObject()
	{
		Camera current = Camera.current;
		if (!current)
		{
			return null;
		}
		if (!Cameras.TryGetValue(current, out var value))
		{
			value = FindOrCreate(current, CameraEvent, BufferName);
			method_0(current, value);
		}
		return value;
	}

	public CommandBuffer GetForCamera(Camera camera)
	{
		return FindOrCreate(camera, CameraEvent, BufferName);
	}

	public bool OnRenderObject(out CommandBuffer buffer)
	{
		buffer = null;
		Camera current = Camera.current;
		if (!current)
		{
			return false;
		}
		if (Cameras.TryGetValue(current, out buffer))
		{
			return false;
		}
		buffer = FindOrCreate(current, CameraEvent, BufferName);
		method_0(current, buffer);
		return true;
	}

	public bool UpdateOnRenderObject(out CommandBuffer buffer)
	{
		buffer = null;
		Camera current = Camera.current;
		if (!current)
		{
			return false;
		}
		if (Cameras.TryGetValue(current, out buffer))
		{
			Cameras.Remove(current);
			SSAAs.Remove(current);
		}
		buffer = FindOrCreate(current, CameraEvent, BufferName);
		method_0(current, buffer);
		return true;
	}

	public bool UpdateOnPreCullRender(out CommandBuffer buffer)
	{
		buffer = null;
		Camera current = Camera.current;
		if (!current)
		{
			return false;
		}
		if (Cameras.TryGetValue(current, out buffer))
		{
			return true;
		}
		buffer = FindOrCreate(current, CameraEvent, BufferName);
		method_0(current, buffer);
		return true;
	}

	public SSAA GetSSAAComponent()
	{
		Camera current = Camera.current;
		if (!current)
		{
			return null;
		}
		SSAA value = null;
		if (SSAAs.TryGetValue(current, out value))
		{
			return value;
		}
		return null;
	}

	public static CommandBuffer smethod_0(Camera camera, CameraEvent cameraEvent, string name)
	{
		CommandBuffer[] commandBuffers = camera.GetCommandBuffers(cameraEvent);
		int num = 0;
		while (true)
		{
			if (num < commandBuffers.Length)
			{
				if (commandBuffers[num].name == name)
				{
					break;
				}
				num++;
				continue;
			}
			return null;
		}
		return commandBuffers[num];
	}

	public static CommandBuffer FindOrCreate(Camera camera, CameraEvent cameraEvent, string name)
	{
		CommandBuffer commandBuffer = smethod_0(camera, cameraEvent, name);
		if (commandBuffer == null)
		{
			commandBuffer = new CommandBuffer
			{
				name = name
			};
			camera.AddCommandBuffer(cameraEvent, commandBuffer);
		}
		return commandBuffer;
	}

	public void DestroyBuffers()
	{
		foreach (KeyValuePair<Camera, CommandBuffer> camera in Cameras)
		{
			if ((bool)camera.Key)
			{
				camera.Key.RemoveCommandBuffer(CameraEvent, camera.Value);
			}
		}
		Cameras.Clear();
		SSAAs.Clear();
	}
}
