using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

public class GClass969
{
	[NonSerialized]
	[CompilerGenerated]
	public CommandBuffer CommandBuffer_0;

	[NonSerialized]
	[CompilerGenerated]
	public Bounds Bounds_0;

	[NonSerialized]
	public Material Material_0;

	[NonSerialized]
	public uint[] Uint_0 = new uint[5];

	[NonSerialized]
	public ComputeBuffer ComputeBuffer_0;

	[NonSerialized]
	public ComputeBuffer ComputeBuffer_1;

	[NonSerialized]
	public ComputeBuffer ComputeBuffer_2;

	[NonSerialized]
	public ComputeBuffer ComputeBuffer_3;

	[NonSerialized]
	public ComputeBuffer ComputeBuffer_4;

	[NonSerialized]
	public ComputeBuffer ComputeBuffer_5;

	[NonSerialized]
	public bool Bool_0 = true;

	[NonSerialized]
	public HashSet<Camera> HashSet_0 = new HashSet<Camera>();

	[NonSerialized]
	public static int Int_0 = Shader.PropertyToID("localToWorldMatrixBuffer");

	[NonSerialized]
	public static int Int_1 = Shader.PropertyToID("worldToLocalMatrixBuffer");

	[NonSerialized]
	public static int Int_2 = Shader.PropertyToID("uvStartEndBuffer");

	[NonSerialized]
	public static int Int_3 = Shader.PropertyToID("normalEdgeClipAndFadeoffBuffer");

	[NonSerialized]
	public static int Int_4 = Shader.PropertyToID("tintBuffer");

	public ComputeBuffer ArgsBuffer => ComputeBuffer_0;

	public Material Material => Material_0;

	public CommandBuffer CommandBuffer
	{
		[CompilerGenerated]
		get
		{
			return CommandBuffer_0;
		}
		[CompilerGenerated]
		set
		{
			CommandBuffer_0 = value;
		}
	}

	public Bounds Bounds
	{
		[CompilerGenerated]
		get
		{
			return Bounds_0;
		}
		[CompilerGenerated]
		set
		{
			Bounds_0 = value;
		}
	}

	public void InitBuffers(Material material, int decalsCount, Mesh mesh)
	{
		Material_0 = material;
		if (decalsCount != 0)
		{
			method_0(decalsCount, mesh);
			method_1(ref ComputeBuffer_1, 64, decalsCount);
			method_1(ref ComputeBuffer_2, 64, decalsCount);
			method_1(ref ComputeBuffer_3, 16, decalsCount);
			method_1(ref ComputeBuffer_4, 16, decalsCount);
		}
	}

	public void AddCommandBufferToCamera(Camera camera)
	{
		camera.AddCommandBuffer(CameraEvent.BeforeReflections, CommandBuffer);
		HashSet_0.Add(camera);
	}

	public void EnableCommandBuffer(bool enable)
	{
		if (enable == Bool_0)
		{
			return;
		}
		foreach (Camera item in HashSet_0)
		{
			if (enable)
			{
				item.AddCommandBuffer(CameraEvent.BeforeReflections, CommandBuffer);
			}
			else
			{
				item.RemoveCommandBuffer(CameraEvent.BeforeReflections, CommandBuffer);
			}
		}
		Bool_0 = enable;
	}

	public bool IsCommandBufferEnabled()
	{
		return Bool_0;
	}

	public void method_0(int decalsCount, Mesh mesh)
	{
		Uint_0[0] = mesh.GetIndexCount(0);
		Uint_0[1] = (uint)decalsCount;
		Uint_0[2] = mesh.GetIndexStart(0);
		Uint_0[3] = mesh.GetBaseVertex(0);
		if (ComputeBuffer_0 == null)
		{
			ComputeBuffer_0 = new ComputeBuffer(1, Uint_0.Length * 4, ComputeBufferType.DrawIndirect);
		}
		ComputeBuffer_0.SetData(Uint_0);
	}

	public void method_1(ref ComputeBuffer computeBuffer, int stride, int decalsCount)
	{
		if (computeBuffer != null)
		{
			computeBuffer.Release();
		}
		computeBuffer = new ComputeBuffer(decalsCount, stride);
	}

	public void UpdateBuffers(List<StaticDeferredDecal> decals, Mesh mesh, Bounds bounds)
	{
		if (decals.Count != 0)
		{
			method_2(decals.Count, mesh);
			method_8();
			method_3(decals);
			method_4(decals);
			method_5(decals);
			method_6(decals);
			method_7(decals);
			if (CommandBuffer != null)
			{
				CommandBuffer.Dispose();
			}
			CommandBuffer = new CommandBuffer();
			CommandBuffer.name = "StaticDeferredDecalInstanceCB";
			Bounds = bounds;
		}
	}

	public void method_2(int decalsCount, Mesh mesh)
	{
		if (ComputeBuffer_0 == null)
		{
			method_0(decalsCount, mesh);
			return;
		}
		Uint_0[1] = (uint)decalsCount;
		ComputeBuffer_0.SetData(Uint_0);
	}

	public void method_3(List<StaticDeferredDecal> decals)
	{
		ComputeBuffer_1 = new ComputeBuffer(decals.Count, 64);
		Matrix4x4[] array = new Matrix4x4[decals.Count];
		for (int i = 0; i < decals.Count; i++)
		{
			array[i] = decals[i].transform.localToWorldMatrix;
		}
		ComputeBuffer_1.SetData(array);
		Material_0.SetBuffer(Int_0, ComputeBuffer_1);
	}

	public void method_4(List<StaticDeferredDecal> decals)
	{
		ComputeBuffer_2 = new ComputeBuffer(decals.Count, 64);
		Matrix4x4[] array = new Matrix4x4[decals.Count];
		for (int i = 0; i < decals.Count; i++)
		{
			array[i] = decals[i].transform.worldToLocalMatrix;
		}
		ComputeBuffer_2.SetData(array);
		Material_0.SetBuffer(Int_1, ComputeBuffer_2);
	}

	public void method_5(List<StaticDeferredDecal> decals)
	{
		ComputeBuffer_3 = new ComputeBuffer(decals.Count, 16);
		Vector4[] array = new Vector4[decals.Count];
		for (int i = 0; i < decals.Count; i++)
		{
			array[i] = decals[i].GetUVStartEnd();
		}
		ComputeBuffer_3.SetData(array);
		Material_0.SetBuffer(Int_2, ComputeBuffer_3);
	}

	public void method_6(List<StaticDeferredDecal> decals)
	{
		ComputeBuffer_4 = new ComputeBuffer(decals.Count, 16);
		Vector4[] array = new Vector4[decals.Count];
		for (int i = 0; i < decals.Count; i++)
		{
			array[i] = decals[i].GetNormalEdgeClipAndFadeoff();
		}
		ComputeBuffer_4.SetData(array);
		Material_0.SetBuffer(Int_3, ComputeBuffer_4);
	}

	public void method_7(List<StaticDeferredDecal> decals)
	{
		ComputeBuffer_5 = new ComputeBuffer(decals.Count, 16);
		Vector4[] array = new Vector4[decals.Count];
		for (int i = 0; i < decals.Count; i++)
		{
			array[i] = decals[i].Tint;
		}
		ComputeBuffer_5.SetData(array);
		Material_0.SetBuffer(Int_4, ComputeBuffer_5);
	}

	public void DestroyBuffers()
	{
		method_9(ref ComputeBuffer_0);
		method_8();
	}

	public void method_8()
	{
		method_9(ref ComputeBuffer_1);
		method_9(ref ComputeBuffer_2);
		method_9(ref ComputeBuffer_3);
		method_9(ref ComputeBuffer_4);
		method_9(ref ComputeBuffer_5);
	}

	public void method_9(ref ComputeBuffer computeBuffer)
	{
		if (computeBuffer != null)
		{
			computeBuffer.Release();
		}
		computeBuffer = null;
	}
}
