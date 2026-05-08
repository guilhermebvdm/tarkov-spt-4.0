using System;
using UnityEngine;

public readonly struct GStruct111
{
	public readonly Vector3 ObservationPoint;

	public readonly Bounds[] VisibilityCells;

	[NonSerialized]
	public global::VisibilityIndicesClass<ushort> Gclass1222_0;

	public ushort[] VisibilityIndices => Gclass1222_0.Buffer;

	public int NumIndices => Gclass1222_0.Count;

	public bool IsEmpty
	{
		get
		{
			if (Gclass1222_0 != null)
			{
				return Gclass1222_0.Count == 0;
			}
			return true;
		}
	}

	public bool TestAABB(Bounds bounds)
	{
		if (IsEmpty)
		{
			return false;
		}
		int numIndices = NumIndices;
		int num = 0;
		while (true)
		{
			if (num < numIndices)
			{
				if (bounds.Intersects(VisibilityCells[VisibilityIndices[num]]))
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

	public GStruct111(Vector3 observationPoint, Bounds[] visibilityCells, global::VisibilityIndicesClass<ushort> visibilityIndices)
	{
		ObservationPoint = observationPoint;
		VisibilityCells = visibilityCells;
		Gclass1222_0 = visibilityIndices;
	}
}
