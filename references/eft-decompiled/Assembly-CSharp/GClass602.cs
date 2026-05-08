using System;
using EFT;
using UnityEngine;

public class GClass602
{
	public EnemyInfo EnemyByRequester;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public Vector3 Vector3_1;

	public GClass602(EnemyInfo enemyByRequester)
	{
		EnemyByRequester = enemyByRequester;
	}

	public GClass602(Vector3 point)
	{
		Vector3_0 = point;
	}

	public bool IsTarget(IPlayer person)
	{
		if (EnemyByRequester == null)
		{
			return false;
		}
		return EnemyByRequester.Person == person;
	}

	public Vector3 ShootPos()
	{
		if (EnemyByRequester != null)
		{
			return EnemyByRequester.EnemyLastPosition + Vector3.up * 0.8f;
		}
		return Vector3_0 + Vector3.up * 0.8f;
	}
}
