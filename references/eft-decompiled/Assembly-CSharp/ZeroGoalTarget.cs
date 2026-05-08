using System;
using UnityEngine;

public class ZeroGoalTarget
{
	[field: NonSerialized]
	public float CreatedTime { get; set; }

	[field: NonSerialized]
	public float ExpireTime { get; set; }

	public ZeroGoalTarget()
	{
		CreatedTime = Time.time;
		ExpireTime = Time.time + 30f;
	}

	public bool IsExpire()
	{
		return Time.time > ExpireTime;
	}
}
