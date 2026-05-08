using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GClass397
{
	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public GameObject GameObject_0;

	[NonSerialized]
	public Transform Transform_0;

	[NonSerialized]
	[CompilerGenerated]
	public static GClass397 Gclass397_0 = new GClass397();

	public static GClass397 Instance
	{
		[CompilerGenerated]
		get
		{
			return Gclass397_0;
		}
	}

	public GClass397()
	{
		GameObject_0 = GClass861.Load("NavMeshObstacleObj") as GameObject;
		Transform_0 = new GameObject("NavMesObs_Container").transform;
	}

	public void SetObstacle(Vector3 pos)
	{
	}
}
