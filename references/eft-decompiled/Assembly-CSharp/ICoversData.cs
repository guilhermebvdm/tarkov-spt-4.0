using System;
using JetBrains.Annotations;
using UnityEngine;

public interface ICoversData
{
	int ConnectionGroupId { get; }

	CustomNavigationPoint LastCustomNavigationPoint { get; }

	CustomNavigationPoint FindClosestPoint(Vector3 dataCenterPos, bool noRestrictions = false, [CanBeNull] Func<GroupPoint, bool> goodFunc = null);

	CustomNavigationPoint GetFreeClosePoint(Vector3 pos, float minSDistToEnemy);

	GroupPoint GetClosest(Vector3 center, int connectionGroupId);

	GroupPoint GetClosestByClosestsVoxeles(Vector3 center);
}
