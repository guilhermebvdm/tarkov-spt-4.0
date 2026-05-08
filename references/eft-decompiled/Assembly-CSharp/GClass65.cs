using System;
using System.Runtime.CompilerServices;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public abstract class GClass65 : GClass62
{
	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_1;

	[NonSerialized]
	public Vector3 Vector3_1;

	[NonSerialized]
	public float Float_7;

	[NonSerialized]
	[CompilerGenerated]
	public GClass435 Gclass435_0;

	public GClass435 GClass435_0
	{
		[CompilerGenerated]
		get
		{
			return Gclass435_0;
		}
		[CompilerGenerated]
		set
		{
			Gclass435_0 = value;
		}
	}

	public GClass65([NotNull] BotOwner owner, int priority)
		: base(owner, priority)
	{
	}

	public void SetBoss(GClass435 gluhar)
	{
		GClass435_0 = gluhar;
		if (GClass435_0.HaveTrainInfo)
		{
			Vector3 vector = GClass435_0.TrainBounds.Collider.center + GClass435_0.TrainBounds.transform.position;
			Vector3 vector2 = vector - GClass435_0.TrainBounds.Collider.size * 0.3f;
			Vector3 vector3 = vector + GClass435_0.TrainBounds.Collider.size * 0.3f;
			float x = GClass856.Random(vector2.x, vector3.x);
			float z = GClass856.Random(vector2.z, vector3.z);
			Vector3_1 = new Vector3(x, vector.y, z);
		}
	}
}
