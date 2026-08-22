using System.Collections;
using System.Collections.Generic;
using EFT;
using EFT.Ballistics;
using UnityEngine;
using VisceralCombat.Dismemberment.Patches;
using VisceralCombat.Ragdolls.Patches;

namespace VisceralCombat.Combined.Classes;

/// <summary>
/// Centralized shot processor that replaces per-patch, redundant coroutines.
/// Listens for completed shots and dispatches impulse, dismemberment, and blood fx sequentially.
/// </summary>
public static class VisceralShotProcessor
{
	private static readonly HashSet<EftBulletClass> ActiveShots = new HashSet<EftBulletClass>();

	public static void RegisterShot(EftBulletClass shot)
	{
		if (shot == null) return;

		// Deduplicate if multiple shoot hooks trigger for the same bullet instance
		if (!ActiveShots.Add(shot)) return;

		if (StaticManager.Instance != null)
		{
			((MonoBehaviour)StaticManager.Instance).StartCoroutine(WatchShotCoroutine(shot));
		}
	}

	private static IEnumerator WatchShotCoroutine(EftBulletClass shot)
	{
		if (shot == null) yield break;

		float timeout = 3.0f;
		while (!shot.IsShotFinished && timeout > 0f)
		{
			timeout -= Time.deltaTime;
			yield return null;
		}

		ActiveShots.Remove(shot);

		if (shot != null && shot.IsShotFinished && shot.HitCollider != null)
		{
			// 1. Process physical impulse & Wake on Hit
			BodiesImpulsePatch.ProcessImpulse(shot);

			// 2. Process post-mortem or living leg dismemberment
			LimbKillPatch.ProcessLimbKill(shot);

			// 3. Process blood splatter, arterial spurt & sparks
			BleedPatch.ProcessWatchShot(shot);
		}
	}

	public static void ClearShots()
	{
		ActiveShots.Clear();
	}
}
