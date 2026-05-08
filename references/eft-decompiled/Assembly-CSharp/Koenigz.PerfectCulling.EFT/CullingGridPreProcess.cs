using System;
using UnityEngine;

namespace Koenigz.PerfectCulling.EFT;

public class CullingGridPreProcess : PerfectCullingCrossSceneGroupPreProcess
{
	[SerializeField]
	private PerfectCullingAdaptiveGrid _grid;

	[SerializeField]
	[HideInInspector]
	private Bounds[] _visibilityCells = Array.Empty<Bounds>();

	[SerializeField]
	private GuidReference[] _cullingGridContent;

	public Bounds[] VisibilityCells => _visibilityCells;

	public GuidReference[] CullingGridContent => _cullingGridContent;

	public override int GetBakeHash()
	{
		if (_grid == null)
		{
			return -1;
		}
		return GClass1228.HashStringInt32(_grid.GridHash) ^ _visibilityCells.Length;
	}
}
