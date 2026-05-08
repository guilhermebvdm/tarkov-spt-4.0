using System;
using System.Collections.Generic;
using UnityEngine;

public class GClass1166
{
	public readonly List<Vector3> candidateEdgePoints = new List<Vector3>();

	public readonly List<Tuple<Vector3, Vector3, bool>> validationLines = new List<Tuple<Vector3, Vector3, bool>>();

	public readonly List<Tuple<Vector3, Vector3, bool>> searchLines = new List<Tuple<Vector3, Vector3, bool>>();

	public Vector3 sourceHitPoint;

	public Vector3 listenerHitPoint;

	public Vector3 bestEdgePoint;

	public bool needsEdgeSearch;

	public bool foundPath;
}
