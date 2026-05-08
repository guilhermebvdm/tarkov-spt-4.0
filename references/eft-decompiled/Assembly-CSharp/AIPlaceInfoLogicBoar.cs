using System.Collections.Generic;
using EFT;
using UnityEngine;

public class AIPlaceInfoLogicBoar : AIPlaceInfoLogic
{
	public bool PermanentVision;

	public List<Transform> PossiblePositions = new List<Transform>();

	private Vector3? nullable_0;

	public Vector3 Position
	{
		get
		{
			if (!nullable_0.HasValue)
			{
				return base.transform.position;
			}
			return nullable_0.Value;
		}
	}

	public override void Init(AIPlaceInfo aiPlaceInfo, BotsController botsController)
	{
		if (PossiblePositions != null && PossiblePositions.Count > 0)
		{
			Transform transform = GClass856.RandomElement(PossiblePositions);
			nullable_0 = transform.position;
		}
		else
		{
			nullable_0 = base.transform.position;
		}
		base.Init(aiPlaceInfo, botsController);
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.DrawSphere(base.transform.position, 0.1f);
		Gizmos.DrawWireSphere(base.transform.position, 0.2f);
	}
}
