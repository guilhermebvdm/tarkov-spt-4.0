using System;
using EFT;
using UnityEngine;

public class GClass2094 : GInterface211
{
	[NonSerialized]
	public const float Float_0 = 36f;

	[NonSerialized]
	public Transform Transform_0;

	[NonSerialized]
	public Quaternion Quaternion_0;

	public bool IsValid => Transform_0 != null;

	public GClass2094(Player player, Quaternion? intermediateRotation = null)
	{
		Transform_0 = player.CameraContainer.transform;
		if (IsValid)
		{
			Quaternion_0 = intermediateRotation ?? Transform_0.rotation;
		}
	}

	public Quaternion GetRotation()
	{
		GClass855.DecomposeSwingTwist(Transform_0.rotation, Vector3.up, out var _, out var twist);
		Quaternion_0 = Quaternion.Slerp(Quaternion_0, twist, Time.deltaTime * 36f);
		return Quaternion_0;
	}
}
