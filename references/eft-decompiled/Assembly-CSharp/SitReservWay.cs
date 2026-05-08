using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using EFT.Interactive;
using UnityEngine;
using UnityEngine.AI;

public class SitReservWay : AReserveWayAction
{
	public Transform lookToPoint;

	public bool ShallLoot = true;

	public string LootableContainerId;

	private bool bool_0;

	private bool bool_1;

	private bool bool_2;

	private bool bool_3;

	private float float_0;

	[CompilerGenerated]
	private LootableContainer lootableContainer_0;

	public LootableContainer Container
	{
		[CompilerGenerated]
		get
		{
			return lootableContainer_0;
		}
		[CompilerGenerated]
		set
		{
			lootableContainer_0 = value;
		}
	}

	public override Vector3 GoTo => base.transform.position;

	public override Vector3 LookShootTo => lookToPoint.position;

	public override ReserveWayResult ManualUpdate(BotOwner bot)
	{
		if (bool_2 && float_0 < Time.time)
		{
			bool_2 = false;
		}
		if (bot.PatrollingData.Status == PatrolStatus.stay)
		{
			_cuResult = ReserveWayResult.stay;
		}
		else
		{
			_cuResult = ReserveWayResult.move;
		}
		return _cuResult;
	}

	public override void SetLeaveUser(BotOwner bot)
	{
		method_0(bot);
	}

	public override void RefreshData()
	{
		CheckWayFromParent("Walk reserv way", base.transform.position);
		if (NavMesh.SamplePosition(base.transform.position, out var hit, 1f, -1))
		{
			base.transform.position = hit.position;
		}
		else
		{
			Debug.LogError("SitReservWay point no nav mesh:" + base.gameObject.name);
		}
		if (lookToPoint == null)
		{
			Debug.LogError("SitReservWay point have wrong look direction " + base.gameObject.name);
		}
		CheckPoint(base.transform.position, "Sit reserv way ");
	}

	public override void RefreshBot()
	{
		_cuResult = ReserveWayResult.move;
	}

	public override float TimeToUse(BotOwner owner)
	{
		return owner.Settings.FileSettings.Patrol.RESERVE_LOOT_TIME_STAY;
	}

	public override void ComeTo(BotOwner bot)
	{
		method_1();
		if (ShallLoot)
		{
			_cuResult = ReserveWayResult.looting;
			if (bool_1 && Container.DoorState == EDoorState.Shut)
			{
				float_0 = Time.time + 0.2f;
				bool_2 = true;
			}
		}
		else
		{
			_cuResult = ReserveWayResult.stay;
		}
	}

	public override void AutoFix()
	{
		base.transform.localPosition = Vector3.zero;
		if (NavMesh.SamplePosition(base.transform.position, out var hit, 1f, -1))
		{
			base.transform.position = hit.position;
		}
	}

	public void SetLootId(string lootableContainerId)
	{
		LootableContainerId = lootableContainerId;
	}

	public override void DrawGizmos()
	{
		if (ShallLoot)
		{
			Gizmos.color = new Color(0.5f, 0.1f, 0.7f, 0.9f);
		}
		else
		{
			Gizmos.color = new Color(0.5f, 0.1f, 0.4f, 0.9f);
		}
		Gizmos.DrawCube(lookToPoint.position, new Vector3(1f, 4f, 1f) * 0.2f);
		Gizmos.DrawCube(lookToPoint.position, new Vector3(1f, 1f, 4f) * 0.2f);
		Vector3 vector = base.transform.position + Vector3.up * 1.6f;
		Gizmos.DrawCube(vector, new Vector3(1f, 1f, 1f) * 0.3f);
		Gizmos.DrawCube(base.transform.position, new Vector3(1f, 1f, 1f) * 0.4f);
		Gizmos.DrawLine(vector, lookToPoint.position);
	}

	public void method_0(BotOwner bot)
	{
		bool_2 = false;
	}

	public void method_1()
	{
		if (bool_0)
		{
			return;
		}
		bool_0 = false;
		IEnumerable<LootableContainer> allObjects = LocationScene.GetAllObjects<LootableContainer>();
		if (LootableContainerId.Length > 0)
		{
			Container = allObjects.FirstOrDefault((LootableContainer x) => x.Id == LootableContainerId);
			bool_1 = Container != null;
			if (!bool_1 && !bool_3)
			{
				bool_3 = true;
			}
		}
	}

	[CompilerGenerated]
	public bool method_2(LootableContainer x)
	{
		return x.Id == LootableContainerId;
	}
}
