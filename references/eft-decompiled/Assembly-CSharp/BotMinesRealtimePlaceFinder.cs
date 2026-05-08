using System;
using Comfort.Common;
using EFT;
using EFT.SynchronizableObjects;
using UnityEngine;
using UnityEngine.AI;

public class BotMinesRealtimePlaceFinder : GClass429
{
	[NonSerialized]
	public float NextPossibleFind;

	[NonSerialized]
	public Vector3 LastPlaced;

	[NonSerialized]
	public const float SDIST_TOO_CLOSE_TO_PREV = 4f;

	[NonSerialized]
	public const float SDIST__BY_POINT_ON_WAY = 9f;

	public BotMinesRealtimePlaceFinder(BotOwner owner)
		: base(owner)
	{
	}

	public void TryFindAndPlant()
	{
		if (NextPossibleFind > Time.time)
		{
			return;
		}
		NextPossibleFind = Time.time + 1f;
		Vector3 vector = BotOwner_0.Mover.PrevCorner();
		Vector3 vector2 = (vector + BotOwner_0.Position) * 0.5f;
		if ((vector2 - LastPlaced).sqrMagnitude < 4f || (vector - BotOwner_0.Position).sqrMagnitude < 9f)
		{
			return;
		}
		bool flag = true;
		foreach (Player allAlivePlayers in Singleton<GameWorld>.Instance.AllAlivePlayersList)
		{
			if (!allAlivePlayers.IsAI && !((vector2 - allAlivePlayers.Position).sqrMagnitude >= 81f))
			{
				flag = false;
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		Vector3 n = GClass855.NormalizeFastSelf(vector2 - BotOwner_0.Position);
		Vector3 vector3 = 0.5f * GClass855.Rotate90(n, GClass855.SideTurn.left);
		Vector3 vector4 = 0.5f * GClass855.Rotate90(n, GClass855.SideTurn.right);
		Vector3 sourcePosition = vector2 + vector4;
		Vector3 sourcePosition2 = vector2 + vector3;
		if (!NavMesh.SamplePosition(sourcePosition, out var hit, 0.3f, -1))
		{
			return;
		}
		Vector3 position = hit.position;
		if (NavMesh.SamplePosition(sourcePosition2, out hit, 0.3f, -1))
		{
			LastPlaced = vector2;
			Vector3 position2 = hit.position;
			if (TripwireSynchronizableObject.ValidateTripwirePosition(position, position, position2, isAI: true, withLogs: false, useLayerWithoutPlayer: true))
			{
				BotOwner_0.MinesData.PlantHereNow(position, position2);
			}
		}
	}
}
