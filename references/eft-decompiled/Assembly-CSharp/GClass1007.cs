using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class GClass1007
{
	[NonSerialized]
	public static int Int_0 = Shader.PropertyToID("_ControlPoints");

	[NonSerialized]
	public static int Int_1 = Shader.PropertyToID("_ControlPointsCount");

	[NonSerialized]
	public GraphicsBuffer GraphicsBuffer_0;

	[NonSerialized]
	public ParticleSystem ParticleSystem_0;

	[NonSerialized]
	public ParticleSystemRenderer ParticleSystemRenderer_0;

	[NonSerialized]
	public int Int_2;

	public GClass1007(ParticleSystem system)
	{
		ParticleSystem_0 = UnityEngine.Object.Instantiate(system);
		Int_2 = ParticleSystem_0.main.maxParticles;
		GraphicsBuffer_0 = new GraphicsBuffer(GraphicsBuffer.Target.Structured, Int_2, Marshal.SizeOf<Vector3>());
		GraphicsBuffer_0.name = "_ControlPoints";
		ParticleSystemRenderer_0 = ParticleSystem_0.GetComponent<ParticleSystemRenderer>();
		ParticleSystemRenderer_0.bounds = new Bounds(Vector3.zero, Vector3.one * 999999f);
	}

	public void Dispose()
	{
		GraphicsBuffer_0.Dispose();
		UnityEngine.Object.Destroy(ParticleSystem_0.gameObject);
	}

	public void Play(List<Vector3> points)
	{
		int num = Mathf.Min(points.Count, Int_2);
		Material sharedMaterial = ParticleSystemRenderer_0.sharedMaterial;
		GraphicsBuffer_0.SetData(points);
		sharedMaterial.SetBuffer(Int_0, GraphicsBuffer_0);
		sharedMaterial.SetInt(Int_1, num);
		ParticleSystem_0.Emit(num);
	}
}
