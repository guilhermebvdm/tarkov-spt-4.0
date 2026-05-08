using System;
using System.Collections.Generic;
using UnityEngine;

namespace Diz.Skinning;

public class PluggableBone : MonoBehaviour
{
	[SerializeField]
	public string BonePath;

	public void Plug(Skeleton skeleton, Vector3 localPosition, Vector3 localRotation)
	{
		Dictionary<string, Transform> bones = skeleton.Bones;
		if (!bones.ContainsKey(BonePath))
		{
			throw new Exception("no bone with name " + BonePath + " found in " + skeleton);
		}
		base.transform.SetParent(bones[BonePath], worldPositionStays: false);
		base.transform.localPosition = localPosition;
		base.transform.localRotation = Quaternion.Euler(localRotation);
	}

	public void Unplug()
	{
		base.transform.SetParent(null, worldPositionStays: false);
	}
}
