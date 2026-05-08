using System;
using EFT;
using UnityEngine;

public class GClass569 : GClass563
{
	[NonSerialized]
	public const float Float_2 = 4f;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public Vector3 Vector3_1;

	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	public GClass513 Gclass513_0;

	[NonSerialized]
	public float Float_4;

	public GClass569(BotOwner owner, int index, GClass513 bossStayAtPlace)
		: base(owner, index)
	{
		Gclass513_0 = bossStayAtPlace;
	}

	public override bool SetTarget(GStruct25 trg, Vector3? lookSide)
	{
		bool num = (Vector3_1 - trg.Position).sqrMagnitude > 1f;
		Vector3_1 = trg.Position;
		if (!num && BotOwner_0.Memory.IsInCover)
		{
			return true;
		}
		return method_2(checkCur: false);
	}

	public override void OnDrawSelectedGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawLine(Vector3_0, BotOwner_0.Position + Vector3.up);
		Gizmos.DrawLine(Vector3_1, BotOwner_0.Position + Vector3.up);
	}

	public override void Update()
	{
		if (CustomNavigationPoint_0 == null)
		{
			return;
		}
		if (Float_4 < Time.time)
		{
			Float_4 = Time.time + 10f;
			method_2(checkCur: true);
		}
		Vector3 firePosition = CustomNavigationPoint_0.FirePosition;
		firePosition.y = CustomNavigationPoint_0.Position.y;
		float dist;
		bool num = GClass510.IsCome(BotOwner_0, firePosition, extraTarget: false, out dist);
		bool val = !num && dist > 4f;
		BotOwner_0.Sprint(val);
		if (num)
		{
			BotOwner_0.LookData.SetLookPointByHearing(CustomNavigationPoint_0);
			BotOwner_0.SetPose(0.1f);
			BotOwner_0.StopMove();
		}
		else
		{
			BotObserveDataClass.SetVector(BotOwner_0.Mover.NormDirCurPoint);
			if (Float_0 < Time.time)
			{
				Float_0 = Time.time + 1.5f;
				BotOwner_0.GoToPoint(firePosition);
			}
		}
		BotObserveDataClass.Update();
	}

	public bool method_2(bool checkCur)
	{
		Vector3_0 = Gclass513_0.GetLookToPoint();
		if (BotOwner_0.Memory.GoalEnemy == null || BotOwner_0.Memory.IsPeace)
		{
			CoverSearchData data = new CoverSearchData(Vector3_1, BotOwner_0.CoverSearchInfo, CoverShootType.shoot, 4900f, 0f, CoverSearchType.distToToCenter, new ShootPointClass(Vector3_0), null, null, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(BotOwner_0.Settings.FileSettings.Cover.MIN_DEFENCE_LEVEL));
			CustomNavigationPoint coverPointMain = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(data, checkCur);
			BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(coverPointMain);
			CustomNavigationPoint_0 = coverPointMain;
		}
		return false;
	}
}
