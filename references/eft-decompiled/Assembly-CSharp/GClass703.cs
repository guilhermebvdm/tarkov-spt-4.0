using System;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass703 : GInterface26
{
	[Serializable]
	[CompilerGenerated]
	public class Class364
	{
		public static readonly Class364 class364_0 = new Class364();

		public static Func<Transform, Quaternion> func_0;

		public static Func<Transform, Quaternion> func_1;

		public Quaternion method_0(Transform x)
		{
			return x.localRotation;
		}

		public Quaternion method_1(Transform x)
		{
			return x.localRotation;
		}
	}

	public Quaternion[][] Fingers = new Quaternion[2][];

	[NonSerialized]
	public Transform Transform_0;

	[NonSerialized]
	public Transform Transform_1;

	[NonSerialized]
	public float Float_0;

	public Player.ValueBlender Blender;

	public Quaternion this[int index]
	{
		get
		{
			if (Float_0 <= 0f)
			{
				return Fingers[0][index];
			}
			if (Float_0 >= 1f)
			{
				return Fingers[1][index];
			}
			return Quaternion.Lerp(Fingers[0][index], Fingers[1][index], Float_0);
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public bool IsAlternative => true;

	public bool IsCached => true;

	public Vector3 Position
	{
		get
		{
			if (Float_0 <= 0f)
			{
				return Transform_0.position;
			}
			if (Float_0 >= 1f)
			{
				return Transform_1.position;
			}
			return Vector3.Lerp(Transform_0.position, Transform_1.position, Float_0);
		}
	}

	public Quaternion Rotation
	{
		get
		{
			if (Float_0 <= 0f)
			{
				return Transform_0.rotation;
			}
			if (Float_0 >= 1f)
			{
				return Transform_1.rotation;
			}
			return Quaternion.Lerp(Transform_0.rotation, Transform_1.rotation, Float_0);
		}
	}

	public void Update()
	{
		Float_0 = Blender.Value;
	}

	public GClass703(GripPose pose1, GripPose pose2, float blendSpeed, float blendDefault)
	{
		Transform_0 = pose1.transform;
		Transform_1 = pose2.transform;
		Fingers[0] = (pose1.IsCached ? pose1.Fingers : pose1.FingerTransforms.Select((Transform x) => x.localRotation).ToArray());
		Fingers[1] = (pose2.IsCached ? pose2.Fingers : pose2.FingerTransforms.Select((Transform x) => x.localRotation).ToArray());
		Blender = new Player.ValueBlender
		{
			Target = blendDefault,
			Speed = blendSpeed,
			Value = blendDefault
		};
	}
}
