using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class GClass1006
{
	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public NativeArray<Vector3> NativeArray_0;

	public GClass1006(float radius, float subdivisions, int maxParticles, float maxLength)
	{
		Int_0 = maxParticles;
		Float_0 = maxLength;
		NativeArray_0 = smethod_0(radius, subdivisions);
	}

	public void Dispose()
	{
		NativeArray_0.Dispose();
	}

	public void RayCast(List<Vector3> output, Vector3 origin)
	{
		NativeArray<RaycastHit> hitResults = new NativeArray<RaycastHit>(NativeArray_0.Length, Allocator.TempJob);
		NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(NativeArray_0.Length, Allocator.TempJob);
		for (int i = 0; i < NativeArray_0.Length; i++)
		{
			commands[i] = new RaycastCommand(origin, NativeArray_0[i], QueryParameters.Default, Float_0);
		}
		RaycastCommand.ScheduleBatch(commands, hitResults, 25, 1).Complete();
		output.Clear();
		method_0(in hitResults, output, 1f, origin, UnityEngine.Random.onUnitSphere);
		method_0(in hitResults, output, 0.8f, origin, UnityEngine.Random.onUnitSphere);
		method_0(in hitResults, output, 0.6f, origin, UnityEngine.Random.onUnitSphere);
		hitResults.Dispose();
		commands.Dispose();
	}

	public void method_0(in NativeArray<RaycastHit> hitResults, List<Vector3> output, float radius, Vector3 origin, Vector3 dirOffset)
	{
		for (int i = 0; i < hitResults.Length; i++)
		{
			if (output.Count >= Int_0)
			{
				break;
			}
			RaycastHit raycastHit = hitResults[i];
			Vector3 vector = Quaternion.Euler(dirOffset) * NativeArray_0[i];
			Vector3 b = origin + vector * Float_0;
			if ((bool)raycastHit.collider)
			{
				b = raycastHit.point;
			}
			Vector3 item = Vector3.Lerp(origin, b, radius);
			output.Add(item);
		}
	}

	public static NativeArray<Vector3> smethod_0(float r, float n)
	{
		List<Vector3> list = new List<Vector3>();
		float num = MathF.PI * 4f * r * r / n;
		float num2 = Mathf.Sqrt(num);
		float num3 = Mathf.Round(MathF.PI / num2);
		float num4 = MathF.PI / num3;
		float num5 = num / num4;
		for (int i = 0; (float)i < num3 - 1f; i++)
		{
			float f = MathF.PI * ((float)i + 0.5f) / num3;
			float num6 = Mathf.Round(MathF.PI * 2f * Mathf.Sin(f) / num5);
			for (int j = 0; (float)j <= num6 - 1f; j++)
			{
				float f2 = MathF.PI * 2f * (float)j / num6;
				list.Add(new Vector3
				{
					x = r * (Mathf.Sin(f) * Mathf.Cos(f2)),
					y = r * (Mathf.Sin(f) * Mathf.Sin(f2)),
					z = r * Mathf.Cos(f)
				}.normalized);
			}
		}
		return new NativeArray<Vector3>(list.ToArray(), Allocator.Persistent);
	}
}
