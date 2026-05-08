using System.Collections.Generic;
using EFT;
using UnityEngine;

public class AIPlaceInfoTagillaAmbush : AIPlaceInfo
{
	public enum EPointSearchType
	{
		Random = 1,
		Nearest
	}

	[SerializeField]
	private readonly List<Transform> _ambushPoints = new List<Transform>();

	[SerializeField]
	private readonly EPointSearchType _pointSearchType = EPointSearchType.Nearest;

	[SerializeField]
	private readonly float _maxAmbushDistance = 100f;

	public override void OnEnterPlace(Player player)
	{
		method_0(player, state: true);
		base.OnEnterPlace(player);
	}

	public override void OnLeavePlace(Player player)
	{
		method_0(player, state: false);
		base.OnLeavePlace(player);
	}

	public void method_0(Player player, bool state)
	{
		foreach (BotOwner botOwner in BotsController.FindBotControllerEditorOnly().Bots.BotOwners)
		{
			_ = botOwner;
		}
	}

	public Transform method_1(BotOwner owner)
	{
		EPointSearchType pointSearchType = _pointSearchType;
		if (pointSearchType != EPointSearchType.Random && pointSearchType == EPointSearchType.Nearest)
		{
			Transform transform = _ambushPoints[0];
			float num = GClass856.SqrDistance(owner.Position, transform.position);
			for (int i = 1; i < _ambushPoints.Count; i++)
			{
				float num2 = GClass856.SqrDistance(owner.Position, _ambushPoints[i].position);
				if (num2 < num)
				{
					num = num2;
					transform = _ambushPoints[i];
				}
			}
			return transform;
		}
		return _ambushPoints[GClass856.RandomInclude(0, _ambushPoints.Count - 1)];
	}

	public void method_2(BotOwner ambushOwner, bool state)
	{
		if (ambushOwner.Memory.GoalEnemy == null)
		{
			method_1(ambushOwner);
		}
	}

	public void OnDrawGizmos()
	{
		if (!DebugBotData.Instance.Gizmos.DrawTagillaAmbushPlace)
		{
			return;
		}
		Gizmos.color = Color.magenta;
		Gizmos.DrawSphere(base.transform.position, 0.6f);
		foreach (Transform ambushPoint in _ambushPoints)
		{
			if (ambushPoint != null)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawSphere(ambushPoint.position, 0.3f);
				Gizmos.DrawLine(base.transform.position, ambushPoint.transform.position);
			}
		}
	}
}
