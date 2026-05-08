using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Audio.SpatialSystem;
using UnityEngine;

public abstract class GClass1130 : GClass1128
{
	public readonly struct SoundRay(Vector3 startPoint, Vector3 endPoint)
	{
		[NonSerialized]
		[CompilerGenerated]
		public Vector3 Vector3_0 = startPoint;

		[NonSerialized]
		[CompilerGenerated]
		public Vector3 Vector3_1 = endPoint;

		[NonSerialized]
		[CompilerGenerated]
		public float Float_0 = Vector3.Distance(startPoint, endPoint);

		public Vector3 StartPoint
		{
			[CompilerGenerated]
			get
			{
				return Vector3_0;
			}
		}

		public Vector3 EndPoint
		{
			[CompilerGenerated]
			get
			{
				return Vector3_1;
			}
		}

		public float Distance
		{
			[CompilerGenerated]
			get
			{
				return Float_0;
			}
		}
	}

	[NonSerialized]
	public const int Int_0 = 11;

	[NonSerialized]
	public const int Int_1 = 5;

	[NonSerialized]
	public HashSet<GClass901> HashSet_0 = new HashSet<GClass901>();

	public static int Int32_0 => GClass1162.HighPolyOcclusionMask;

	public GClass1130(SpatialAudioSystem audioSystem)
		: base(audioSystem)
	{
	}

	public static Vector3 smethod_0(Vector3 soundPos, Vector3 listenerPos, float offset, bool positive)
	{
		float num = Vector3.Distance(new Vector3(soundPos.x, 0f, soundPos.z), new Vector3(listenerPos.x, 0f, listenerPos.z));
		float num2 = offset / num;
		float x;
		float z;
		if (positive)
		{
			x = soundPos.x + num2 * (soundPos.z - listenerPos.z);
			z = soundPos.z - num2 * (soundPos.x - listenerPos.x);
		}
		else
		{
			x = soundPos.x - num2 * (soundPos.z - listenerPos.z);
			z = soundPos.z + num2 * (soundPos.x - listenerPos.x);
		}
		return new Vector3(x, soundPos.y, z);
	}

	public void method_0(float duration = 0.5f)
	{
	}
}
