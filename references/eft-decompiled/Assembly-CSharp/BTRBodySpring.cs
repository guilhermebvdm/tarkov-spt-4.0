using System;
using UnityEngine;

[Serializable]
public class BTRBodySpring
{
	public float ReturnSpeed;

	public float Damping;

	public float Intensity;

	public bool UseOffset;

	[HideInInspector]
	public Vector3 Offset;

	[HideInInspector]
	public Vector3 Velocity;

	[HideInInspector]
	public Vector3 Current;

	[HideInInspector]
	public Vector3 ForceAccumulatorLimitless;

	[HideInInspector]
	public bool NeedUpdate;

	[HideInInspector]
	public bool StableOn;

	public virtual void Update(float deltaTime)
	{
		if (NeedUpdate)
		{
			Velocity += ForceAccumulatorLimitless * Intensity;
			ForceAccumulatorLimitless = Vector3.zero;
			NeedUpdate = false;
		}
		float num = ReturnSpeed * deltaTime;
		if (UseOffset)
		{
			Velocity -= (Current - Offset) * num;
		}
		else
		{
			Velocity -= Current * num;
		}
		Velocity *= Damping;
		Current += Velocity;
	}

	public virtual void AddAccelerationLimitless(int comp, float val)
	{
		ForceAccumulatorLimitless[comp] += val;
		NeedUpdate = true;
	}

	public virtual Vector3 GetValue(bool useOffset)
	{
		if (useOffset)
		{
			return Offset + Current;
		}
		return Current;
	}
}
