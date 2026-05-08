using System;
using EFT.InventoryLogic;
using UnityEngine;

public class AIGreanageThrowData
{
	public bool CanThrow;

	public float Ang;

	public float Force;

	public Vector3 Direction;

	public Vector3 Target;

	public Vector3 Position;

	public bool ThrowComplete;

	public GameObject HitsObject;

	public ThrowWeapType? GrenadeType;

	[NonSerialized]
	public bool AlwaysGood;

	[NonSerialized]
	public float CreatedTime;

	public AIGreanageThrowData()
	{
		CanThrow = false;
	}

	public AIGreanageThrowData(GameObject hitsObject)
	{
		HitsObject = hitsObject;
		CanThrow = false;
	}

	public AIGreanageThrowData(float ang, float force, Vector3 dir, Vector3 from, Vector3 trg, bool alwaysGood, ThrowWeapType? grenadeType = null)
	{
		GrenadeType = grenadeType;
		AlwaysGood = alwaysGood;
		Target = trg;
		Position = from;
		Ang = ang;
		Force = force;
		Direction = GClass855.RotateVectorOnAngToZ(dir, Ang);
		CanThrow = true;
		CreatedTime = Time.time;
	}

	public bool IsUpToDate()
	{
		if (!AlwaysGood)
		{
			return Time.time - CreatedTime < 1f;
		}
		return true;
	}
}
