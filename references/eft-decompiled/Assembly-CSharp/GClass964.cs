using System;
using System.Collections.Generic;
using UnityEngine;

public class GClass964 : GClass962<GClass971>
{
	public struct GStruct66
	{
		public Texture2D Texture;

		public float MaxSize;

		public Vector3 Position;
	}

	[NonSerialized]
	public Bounds Bounds_0;

	public void Dispose()
	{
		if (Gclass962_1 != null)
		{
			(Gclass962_1 as GClass964).Dispose();
		}
		if (Gclass962_2 != null)
		{
			(Gclass962_2 as GClass964).Dispose();
		}
		if (Gclass962_3 != null)
		{
			(Gclass962_3 as GClass964).Dispose();
		}
		if (Gclass962_4 != null)
		{
			(Gclass962_4 as GClass964).Dispose();
		}
	}

	public GClass964(GClass964 parent, Rect bounds)
		: base((GClass962<GClass971>)parent, bounds)
	{
	}

	public override GClass962<GClass971> CreateQuadrant(GClass962<GClass971> parent, Rect bounds)
	{
		return new GClass964(parent as GClass964, bounds);
	}

	public (float, float) GetMaxPixelsSizeFactor(GClass2380 frustum, ref float maxHPixelsFactor, ref float maxVPixelsFactor, ref float closestDist)
	{
		if (frustum.GetBoxIntersection(ref Bounds_0) == GClass2380.IntersectionType.Disjoint)
		{
			return (0f, 0f);
		}
		if (Gclass962_1 != null)
		{
			(Gclass962_1 as GClass964).GetMaxPixelsSizeFactor(frustum, ref maxHPixelsFactor, ref maxVPixelsFactor, ref closestDist);
		}
		if (Gclass962_2 != null)
		{
			(Gclass962_2 as GClass964).GetMaxPixelsSizeFactor(frustum, ref maxHPixelsFactor, ref maxVPixelsFactor, ref closestDist);
		}
		if (Gclass962_3 != null)
		{
			(Gclass962_3 as GClass964).GetMaxPixelsSizeFactor(frustum, ref maxHPixelsFactor, ref maxVPixelsFactor, ref closestDist);
		}
		if (Gclass962_4 != null)
		{
			(Gclass962_4 as GClass964).GetMaxPixelsSizeFactor(frustum, ref maxHPixelsFactor, ref maxVPixelsFactor, ref closestDist);
		}
		foreach (GClass971 data in GetDataList())
		{
			Vector3 vector = frustum.Position - data.pos;
			float num = 1f;
			if ((double)vector.sqrMagnitude > 0.001)
			{
				num = vector.magnitude;
			}
			closestDist = Mathf.Min(closestDist, num);
			float num2 = data.uvStartEnd.z - data.uvStartEnd.x;
			float num3 = data.uvStartEnd.w - data.uvStartEnd.y;
			maxHPixelsFactor = Mathf.Max(maxHPixelsFactor, data.maxSize / num) / num2;
			maxVPixelsFactor = Mathf.Max(maxVPixelsFactor, data.maxSize / num) / num3;
		}
		return (maxHPixelsFactor, maxVPixelsFactor);
	}

	public Bounds ComputeBounds()
	{
		Bounds bounds = default(Bounds);
		bool flag = false;
		if (Gclass962_1 != null)
		{
			bounds = (Gclass962_1 as GClass964).ComputeBounds();
			flag = true;
		}
		if (Gclass962_2 != null)
		{
			Bounds bounds2 = (Gclass962_2 as GClass964).ComputeBounds();
			if (flag)
			{
				bounds.Encapsulate(bounds2);
			}
			else
			{
				bounds = bounds2;
				flag = true;
			}
		}
		if (Gclass962_3 != null)
		{
			Bounds bounds3 = (Gclass962_3 as GClass964).ComputeBounds();
			if (flag)
			{
				bounds.Encapsulate(bounds3);
			}
			else
			{
				bounds = bounds3;
				flag = true;
			}
		}
		if (Gclass962_4 != null)
		{
			Bounds bounds4 = (Gclass962_4 as GClass964).ComputeBounds();
			if (flag)
			{
				bounds.Encapsulate(bounds4);
			}
			else
			{
				bounds = bounds4;
				flag = true;
			}
		}
		List<GClass971> dataList = GetDataList();
		if (dataList != null)
		{
			foreach (GClass971 item in dataList)
			{
				Bounds bounds5 = new Bounds(item.pos, new Vector3(item.maxSize, item.maxSize, item.maxSize));
				if (flag)
				{
					bounds.Encapsulate(bounds5);
					continue;
				}
				bounds = bounds5;
				flag = true;
			}
		}
		Bounds_0 = bounds;
		if (!flag)
		{
			throw new InvalidOperationException("There is a texture quadrant node without any data!");
		}
		return bounds;
	}

	public (float, float, float) GetMaxMipFactor(GClass2380 frustum)
	{
		if (frustum.GetBoxIntersection(ref Bounds_0) == GClass2380.IntersectionType.Disjoint)
		{
			return (0f, 0f, 0f);
		}
		float maxHPixelsFactor = 0f;
		float maxVPixelsFactor = 0f;
		float closestDist = float.MaxValue;
		if (Gclass962_1 != null)
		{
			(Gclass962_1 as GClass964).GetMaxPixelsSizeFactor(frustum, ref maxHPixelsFactor, ref maxVPixelsFactor, ref closestDist);
		}
		if (Gclass962_2 != null)
		{
			(Gclass962_2 as GClass964).GetMaxPixelsSizeFactor(frustum, ref maxHPixelsFactor, ref maxVPixelsFactor, ref closestDist);
		}
		if (Gclass962_3 != null)
		{
			(Gclass962_3 as GClass964).GetMaxPixelsSizeFactor(frustum, ref maxHPixelsFactor, ref maxVPixelsFactor, ref closestDist);
		}
		if (Gclass962_4 != null)
		{
			(Gclass962_4 as GClass964).GetMaxPixelsSizeFactor(frustum, ref maxHPixelsFactor, ref maxVPixelsFactor, ref closestDist);
		}
		if (List_0 != null)
		{
			foreach (GClass961<GClass971> item in List_0)
			{
				Vector3 vector = frustum.Position - item.Node.pos;
				float num = 1f;
				if ((double)vector.sqrMagnitude > 0.001)
				{
					num = vector.magnitude;
				}
				closestDist = Mathf.Min(closestDist, num);
				float num2 = item.Node.uvStartEnd.z - item.Node.uvStartEnd.x;
				float num3 = item.Node.uvStartEnd.w - item.Node.uvStartEnd.y;
				maxHPixelsFactor = Mathf.Max(maxHPixelsFactor, item.Node.maxSize / num / num2);
				maxVPixelsFactor = Mathf.Max(maxVPixelsFactor, item.Node.maxSize / num / num3);
			}
		}
		return (maxHPixelsFactor, maxVPixelsFactor, closestDist);
	}
}
