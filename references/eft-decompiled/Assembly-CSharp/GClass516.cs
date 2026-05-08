using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public class GClass516(BotSuppressStationary suppressStationary, BotOwner owner) : AbstractSuppressStationary(suppressStationary, owner)
{
	[NonSerialized]
	public List<Vector3> List_0 = new List<Vector3>();

	[NonSerialized]
	public float Float_0 = 16f;

	[NonSerialized]
	public int Int_0 = 10;

	public override bool Usable => true;

	public override bool CanStartSuppressAt(Vector3 pos)
	{
		if (!IsAtSector(pos))
		{
			return false;
		}
		foreach (Vector3 item in List_0)
		{
			if (!((item - pos).sqrMagnitude >= Float_0))
			{
				return false;
			}
		}
		return true;
	}

	public override bool CanStartSupressEnemy(EnemyInfo enemy)
	{
		if (enemy != null)
		{
			return Time.time - enemy.TimeLastSeen > BotOwner_0.Settings.FileSettings.Shoot.LAST_SEEN_TIME_TO_START_SUPPRESS_STATIONARY_AGS;
		}
		return true;
	}

	public override void StopExternal(Vector3 badPos)
	{
		Stop();
		if (List_0.Count > Int_0)
		{
			List_0.RemoveAt(0);
		}
		List_0.Add(badPos);
	}
}
