using System;
using UnityEngine;

public class GClass833
{
	[Flags]
	public enum CheckType
	{
		Distance = 1,
		Frustrum = 2
	}

	[SerializeField]
	public struct GStruct51
	{
		public CheckType check;

		public float sqrDistance;
	}

	[NonSerialized]
	public Transform Transform_0;

	[NonSerialized]
	public Renderer[] Renderer_0;

	[NonSerialized]
	public Camera Camera_0;

	[NonSerialized]
	public GStruct51 Gstruct51_0;

	public bool IsInFrustrumView
	{
		get
		{
			if ((Gstruct51_0.check & CheckType.Frustrum) == 0)
			{
				return true;
			}
			int num = 0;
			while (true)
			{
				if (num < Renderer_0.Length)
				{
					Renderer renderer = Renderer_0[num];
					if (renderer.enabled && GClass860.IsVisibleFrom(renderer, Camera_0))
					{
						break;
					}
					num++;
					continue;
				}
				return false;
			}
			return true;
		}
	}

	public bool IsInDistance
	{
		get
		{
			if ((Gstruct51_0.check & CheckType.Distance) == 0)
			{
				return true;
			}
			return Vector3.SqrMagnitude(Transform_0.position - Camera_0.transform.position) <= Gstruct51_0.sqrDistance;
		}
	}

	public bool IsVisible
	{
		get
		{
			if (IsInDistance)
			{
				return IsInFrustrumView;
			}
			return false;
		}
	}

	public GClass833(Camera camera, GameObject gameObject, GStruct51 settings)
	{
		Camera_0 = camera;
		Gstruct51_0 = settings;
		Transform_0 = gameObject.transform;
		if ((Gstruct51_0.check & CheckType.Frustrum) != 0)
		{
			Renderer_0 = gameObject.GetComponentsInChildren<Renderer>();
		}
	}
}
