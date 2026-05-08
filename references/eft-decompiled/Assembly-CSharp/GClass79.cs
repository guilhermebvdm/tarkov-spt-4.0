using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass79 : BaseLogicLayerSimpleAbstractClass
{
	[CompilerGenerated]
	public class Class215
	{
		public GClass79 gclass79_0;

		public int maxFarDist;

		public bool method_0(GroupPoint x)
		{
			if (x.CorePointInGame == gclass79_0.BotOwner_0.StartCorePoint)
			{
				return (gclass79_0.BotOwner_0.Position - x.Position).sqrMagnitude < (float)maxFarDist;
			}
			return false;
		}
	}

	[NonSerialized]
	public const int Int_1 = 2;

	[NonSerialized]
	public List<GroupPoint> List_0 = new List<GroupPoint>();

	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	public float Float_3 = -10f;

	[NonSerialized]
	public float Float_4 = 10000f;

	[NonSerialized]
	public float Float_5 = 900f;

	[NonSerialized]
	public float Float_6;

	[NonSerialized]
	public float Float_7;

	[NonSerialized]
	public float Float_8;

	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public bool Bool_5;

	[NonSerialized]
	public ECoverPointSpecial EcoverPointSpecial_0;

	[NonSerialized]
	public bool Bool_6;

	[NonSerialized]
	public bool Bool_7;

	public bool Boolean_0
	{
		get
		{
			if (!BotOwner_0.Medecine.FirstAid.Have2Do)
			{
				return BotOwner_0.Medecine.SurgicalKit.HaveWork;
			}
			return true;
		}
	}

	public GClass79(BotOwner bot, int priority, ECoverPatrolControls controls, float minDist, float maxDist, ECoverPointSpecial specialsToEvade)
		: base(bot, priority)
	{
		EcoverPointSpecial_0 = specialsToEvade;
		Bool_7 = EcoverPointSpecial_0 > (ECoverPointSpecial)0;
		Float_5 = minDist * minDist;
		Float_4 = maxDist * maxDist;
		switch (controls)
		{
		case ECoverPatrolControls.byY:
			Bool_4 = true;
			break;
		case ECoverPatrolControls.distances:
			Bool_5 = true;
			break;
		case ECoverPatrolControls.both:
			Bool_4 = true;
			Bool_5 = true;
			break;
		}
		method_13();
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (CustomNavigationPoint_0 == null || (CustomNavigationPoint_0 != null && CustomNavigationPoint_0.IsSpotted))
		{
			method_14();
		}
		if (Boolean_0)
		{
			if (BotOwner_0.Memory.IsInCover)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "first aid");
			}
			if (method_12(20f))
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "goforheal");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "heal now");
		}
		if ((float)BotOwner_0.WeaponManager.Reload.BulletCount / (float)BotOwner_0.WeaponManager.Reload.MaxBulletCount < 0.6f && Float_7 < Time.time)
		{
			Float_7 = Time.time + 30f;
			BotOwner_0.WeaponManager.Reload.TryReload();
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			if ((BotOwner_0.Memory.CurCustomCoverPoint.Position - BotOwner_0.Position).sqrMagnitude > 2f)
			{
				if (method_12(20f))
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "run2cov");
				}
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToCoverPoint, "notClose");
			}
			Float_6 = Time.time;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "wait");
		}
		if (method_12(20f))
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "run2cov");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToCoverPoint, "toCov");
	}

	public override bool ShallUseNow()
	{
		return true;
	}

	public override string Name()
	{
		return "PtrlBirdEye";
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		if (CustomNavigationPoint_0 != null)
		{
			return CustomNavigationPoint_0;
		}
		return base.FindPoint(data, p, checkCurrent);
	}

	public override AICoreActionEndStruct EndHeal()
	{
		return base.EndHeal();
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (Float_8 > Time.time && !BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		Float_8 = Time.time + 2f;
		if (Boolean_0)
		{
			return new AICoreActionEndStruct("meds");
		}
		if (method_18(25f) && !method_16(BotOwner_0.Position, Float_5, Float_4))
		{
			method_14();
			return new AICoreActionEndStruct("notGood");
		}
		if (method_18(90f))
		{
			method_14();
			return new AICoreActionEndStruct("timeHasCome");
		}
		if (!BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		if (CustomNavigationPoint_0 != null && CustomNavigationPoint_0.IsSpotted)
		{
			CustomNavigationPoint_0 = null;
			return new AICoreActionEndStruct("IsSpotted");
		}
		return AICoreActionEndStruct_1;
	}

	public void method_13()
	{
		int maxFarDist = 2500;
		if (Bool_4)
		{
			maxFarDist = 6400;
		}
		List<GroupPoint> list = BotOwner_0.BotsController.CoversData.Points.Where((GroupPoint x) => x.CorePointInGame == BotOwner_0.StartCorePoint && (BotOwner_0.Position - x.Position).sqrMagnitude < (float)maxFarDist).ToList();
		int num = 20;
		if (Bool_4)
		{
			list.Sort(method_17);
			for (int num2 = 0; num2 < num && num2 < list.Count; num2++)
			{
				List_0.Add(list[num2]);
			}
		}
		else
		{
			List_0 = GClass856.RandomElement(list, num).ToList();
		}
	}

	public void method_14()
	{
		if (Time.time - Float_3 < 1f)
		{
			return;
		}
		CustomNavigationPoint_0?.Spotted(1f);
		CustomNavigationPoint customNavigationPoint = null;
		if (List_0.Count > 0)
		{
			Vector3 position = GClass856.RandomElement(List_0).Position;
			customNavigationPoint = BotOwner_0.Covers.GetClosestPoint(position, (GroupPoint x) => !x.IsSpotted && x.IsFreeById(BotOwner_0.Id) && method_16(x.Position, Float_5, Float_4) && method_15(x));
			if (customNavigationPoint == null)
			{
				customNavigationPoint = BotOwner_0.Covers.GetClosestPoint(position, (GroupPoint x) => !x.IsSpotted && x.IsFreeById(BotOwner_0.Id));
			}
		}
		if (customNavigationPoint == null)
		{
			customNavigationPoint = BotOwner_0.Covers.GetClosestPoint(BotOwner_0.Position, (GroupPoint x) => !x.IsSpotted && x.IsFreeById(BotOwner_0.Id));
		}
		CustomNavigationPoint_0 = customNavigationPoint;
		Float_3 = Time.time;
		BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(CustomNavigationPoint_0);
	}

	public bool method_15(GroupPoint point)
	{
		if (!Bool_7)
		{
			return true;
		}
		if (point.Special == (ECoverPointSpecial)0)
		{
			return true;
		}
		return (EcoverPointSpecial_0 & point.Special) != EcoverPointSpecial_0;
	}

	public bool method_16(Vector3 point, float minDist, float maxDist)
	{
		if (Bool_6)
		{
			return true;
		}
		if (!Bool_5)
		{
			return true;
		}
		int num = 0;
		while (true)
		{
			if (num < BotOwner_0.BotsGroup.MembersCount)
			{
				BotOwner botOwner = BotOwner_0.BotsGroup.Member(num);
				if (!(botOwner == BotOwner_0))
				{
					float sqrMagnitude = ((botOwner.Mover.TargetPoint ?? botOwner.Position) - point).sqrMagnitude;
					if (!(sqrMagnitude <= maxDist))
					{
						return false;
					}
					if (!(sqrMagnitude >= minDist))
					{
						break;
					}
				}
				num++;
				continue;
			}
			return true;
		}
		return false;
	}

	public int method_17(GroupPoint pointA, GroupPoint pointB)
	{
		try
		{
			if (pointA == pointB)
			{
				return 0;
			}
			if (pointA.Position.y < pointB.Position.y)
			{
				return 1;
			}
			return -1;
		}
		catch (Exception value)
		{
			Console.WriteLine(value);
			throw;
		}
	}

	public bool method_18(float period)
	{
		return Time.time - Float_6 > period;
	}

	[CompilerGenerated]
	public bool method_19(GroupPoint x)
	{
		if (!x.IsSpotted && x.IsFreeById(BotOwner_0.Id) && method_16(x.Position, Float_5, Float_4))
		{
			return method_15(x);
		}
		return false;
	}

	[CompilerGenerated]
	public bool method_20(GroupPoint x)
	{
		if (!x.IsSpotted)
		{
			return x.IsFreeById(BotOwner_0.Id);
		}
		return false;
	}

	[CompilerGenerated]
	public bool method_21(GroupPoint x)
	{
		if (!x.IsSpotted)
		{
			return x.IsFreeById(BotOwner_0.Id);
		}
		return false;
	}
}
