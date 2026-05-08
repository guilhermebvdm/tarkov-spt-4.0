using System;
using UnityEngine;

namespace Koenigz.PerfectCulling;

public class PerfectCullingExcludeVolume : MonoBehaviour, GClass879.GInterface33
{
	private static readonly Bounds bounds_0 = new Bounds(Vector3.zero, Vector3.one);

	[SerializeField]
	public Vector3 volumeSize = Vector3.one;

	[SerializeField]
	public PerfectCullingBakingBehaviour[] restrictToBehaviours = Array.Empty<PerfectCullingBakingBehaviour>();

	public Bounds volumeExcludeBounds
	{
		get
		{
			return new Bounds(base.transform.position, volumeSize);
		}
		set
		{
			base.transform.position = value.center;
			volumeSize = new Vector3(Mathf.Max(1f, value.size.x), Mathf.Max(1f, value.size.y), Mathf.Max(1f, value.size.z));
		}
	}

	public Vector3 HandleSized
	{
		get
		{
			return volumeExcludeBounds.size;
		}
		set
		{
			volumeExcludeBounds = new Bounds(base.transform.position, value);
		}
	}

	public bool IsPositionActive(PerfectCullingBakingBehaviour bakingBehaviour, Vector3 pos)
	{
		if (restrictToBehaviours.Length != 0 && Array.IndexOf(restrictToBehaviours, bakingBehaviour) < 0)
		{
			return false;
		}
		Matrix4x4 inverse = Matrix4x4.TRS(base.transform.position, base.transform.rotation, volumeExcludeBounds.size).inverse;
		return bounds_0.Contains(inverse.MultiplyPoint3x4(pos));
	}
}
