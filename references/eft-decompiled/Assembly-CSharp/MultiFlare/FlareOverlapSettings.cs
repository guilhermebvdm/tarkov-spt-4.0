using System;
using UnityEngine;

namespace MultiFlare;

[Serializable]
public class FlareOverlapSettings : BatchSettings
{
	[SerializeField]
	public int _maxNeighborCount;

	[SerializeField]
	public float _searchRange;

	[SerializeField]
	public float _maxScaleMultiplier;

	public int MaxNeighborCount => _maxNeighborCount;

	public float SearchRange => _searchRange;

	public float MaxScaleMultiplier => _maxScaleMultiplier;
}
