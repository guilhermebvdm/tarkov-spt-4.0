using System;
using UnityEngine;

public class CallEnemyTargetInfo
{
	[NonSerialized]
	public Vector3? Target;

	[NonSerialized]
	public int EnvId_1 = -1;

	public bool HasValue => Target.HasValue;

	public Vector3 Value => Target.Value;

	public int EnvId => EnvId_1;

	public CallEnemyTargetInfo(Vector3 trg, int envId)
	{
		Target = trg;
		EnvId_1 = envId;
	}

	public CallEnemyTargetInfo()
	{
		Target = null;
		EnvId_1 = -1;
	}
}
