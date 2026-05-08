using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass360 : GClass359
{
	[Serializable]
	[CompilerGenerated]
	public class Class237
	{
		public static readonly Class237 class237_0 = new Class237();

		public static Func<bool> func_0;

		public bool method_0()
		{
			return true;
		}
	}

	[NonSerialized]
	public const int Int_0 = 1;

	[NonSerialized]
	public const int Int_1 = 2;

	[NonSerialized]
	public const int Int_2 = 3;

	[NonSerialized]
	public const int Int_3 = 5;

	[NonSerialized]
	public const int Int_4 = 6;

	[NonSerialized]
	public const int Int_5 = 7;

	[NonSerialized]
	public const int Int_6 = 8;

	[NonSerialized]
	public const int Int_7 = 9;

	[NonSerialized]
	public const int Int_8 = 10;

	[NonSerialized]
	public const int Int_9 = 11;

	[NonSerialized]
	public const int Int_10 = 12;

	[NonSerialized]
	public const float Float_0 = 30f;

	[NonSerialized]
	public GClass138 Gclass138_0;

	[NonSerialized]
	public GClass153 Gclass153_0;

	[NonSerialized]
	public GClass154 Gclass154_0;

	[NonSerialized]
	public GClass155 Gclass155_0;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public GClass448 Gclass448_0;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public Class103 Class103_0;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public CultistEventsClass CultistEventsClass;

	public override BotOwner Bot => Owner;

	public GClass360(BotOwner owner)
		: base(owner)
	{
		GClass48 layer = new GClass48(owner, 200);
		method_0(5, layer, activeOnStart: true);
		GClass118 layer2 = new GClass118(owner, 198);
		method_0(12, layer2, activeOnStart: true);
		GClass166 layer3 = new GClass166(owner, 120);
		method_0(11, layer3, activeOnStart: true);
		GClass62 layer4 = new GClass62(Owner, 100);
		method_0(10, layer4, activeOnStart: false);
		Gclass154_0 = new GClass154(owner, 90);
		method_0(6, Gclass154_0, activeOnStart: false);
		Gclass155_0 = new GClass155(owner, 80, this);
		method_0(8, Gclass155_0, activeOnStart: false);
		Gclass153_0 = new GClass153(owner, 70, this);
		method_0(1, Gclass153_0, activeOnStart: true);
		GClass101 layer5 = new GClass101(owner, 47, () => true);
		method_0(3, layer5, activeOnStart: true);
		GClass132 layer6 = new GClass132(owner, 13);
		method_0(9, layer6, activeOnStart: true);
		Class103_0 = new Class103(owner, 12);
		method_0(7, Class103_0, activeOnStart: true);
		Gclass138_0 = new GClass138(owner, 11, canUseGreenPoints: true);
		method_0(2, Gclass138_0, activeOnStart: true);
		Owner.GetPlayer.ActiveHealthController.DoPainKiller();
		Owner.GetPlayer.ActiveHealthController.DoPermanentHealthBoost(1f);
		Owner.Tactic.SetTactic(BotsGroup.BotCurrentTactic.Attack);
		Subscribe();
		CultistEventsClass = new CultistEventsClass(Owner);
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, 120, 55, -1);
	}

	public override void SetCorePosition(Vector3 pos)
	{
		Gclass138_0.SetCorePosition(pos);
		Gclass153_0.SetCorePosition(pos);
		Gclass155_0.SetCorePosition(pos);
	}

	public override bool IsCurShootAndRun()
	{
		return method_2(Gclass154_0);
	}

	public void ActivateMelee()
	{
		method_1(1);
		method_3(8);
		method_3(6);
	}

	public void Subscribe()
	{
		Owner.Memory.OnGoalEnemyChanged += method_10;
		Owner.Memory.OnBulletNear += method_16;
		Owner.GetPlayer.GetPlayer.BeingHitAction += method_14;
		Owner.Memory.OnPeaceChange += method_13;
		Owner.Memory.OnSpottedByHit += method_11;
		Owner.ShootData.OnTriggerPressed += method_12;
	}

	public override void SetBoss(GClass448 sectantPriest)
	{
		Gclass448_0 = sectantPriest;
		Gclass448_0.Owner.GetPlayer.HealthController.DiedEvent += method_7;
		Gclass448_0.Owner.LeaveData.OnLeave += method_18;
		Gclass153_0.SetBoss(sectantPriest);
		Gclass155_0.SetBoss(sectantPriest);
	}

	public override void SetAttackWithDelay(float delay)
	{
		if (method_17(method_8))
		{
			Owner.BotTalk.Say(EPhraseTrigger.NeedHelp);
			Gclass155_0.SetDelay(delay);
		}
	}

	public override void ManualUpdate()
	{
		if (Bool_0 && Float_2 < Time.time)
		{
			Bool_0 = false;
			SetAttackWithDelay(0f);
		}
		if (Float_1 < Time.time)
		{
			Float_1 = Time.time + 2f;
			method_6();
		}
		if (IsCurShootAndRun() && Time.time - Float_4 > 30f && TryChangeToMelee())
		{
			Gclass153_0.SetRavangeMode();
		}
		CultistEventsClass.ManualUpdate();
		base.ManualUpdate();
	}

	public override bool TryChangeToMelee()
	{
		return method_17(ActivateMelee);
	}

	public bool CanRunToEnemyWithShortPath()
	{
		return Gclass153_0.CanRunToEnemyWithShortPath();
	}

	public void ReturnToSupportShoot(float secPeriod)
	{
		Bool_0 = true;
		Float_2 = Time.time + secPeriod;
	}

	public override string ShortName()
	{
		return "SectantWarrior";
	}

	public void method_6()
	{
		Bool_1 = Owner.AIData.PlaceInfo != null && Owner.AIData.PlaceInfo.IsInside;
		Gclass153_0.SetInside(Bool_1);
		Gclass155_0.SetInside(Bool_1);
		Gclass138_0.SetInside(Bool_1);
	}

	public void method_7(EDamageType obj)
	{
		SetAttackWithDelay(0f);
		method_1(10);
	}

	public void method_8()
	{
		method_3(1);
		method_1(8);
		method_3(6);
	}

	public void method_9()
	{
		method_3(1);
		method_3(8);
		method_1(6);
	}

	public void method_10(BotOwner obj)
	{
		Gclass448_0?.FollowerTalkCheck();
	}

	public void method_11(Vector3? obj)
	{
		if (Gclass448_0 != null)
		{
			method_15();
			Gclass448_0.FollowerUnderAttack(Owner);
		}
	}

	public void method_12()
	{
		Gclass448_0.TriggerPressedBy(Owner);
	}

	public void method_13(bool isPeace)
	{
		if (isPeace)
		{
			TryChangeToMelee();
		}
	}

	public void method_14(DamageInfoStruct arg1, EBodyPart arg2, float arg3)
	{
	}

	public void method_15()
	{
		if (!IsCurShootAndRun())
		{
			Float_4 = Time.time;
			method_17(method_9);
		}
	}

	public void method_16(BotOwner bot, IPlayer source)
	{
		if (Owner.Memory.IsInCover)
		{
			Gclass448_0?.FollowerUnderAttack(Owner);
			method_15();
		}
	}

	public bool method_17(Action changeAction)
	{
		if (Float_3 < Time.time)
		{
			Float_3 = Time.time + 15f;
			changeAction();
			return true;
		}
		return false;
	}

	public void method_18(BotOwner obj)
	{
		method_1(10);
	}

	public override void Dispose()
	{
		try
		{
			if (Gclass448_0 != null && Gclass448_0.Owner != null)
			{
				Gclass448_0.Owner.GetPlayer.HealthController.DiedEvent -= method_7;
				if (Gclass448_0.Owner.LeaveData != null)
				{
					Gclass448_0.Owner.LeaveData.OnLeave -= method_18;
				}
			}
		}
		catch (Exception)
		{
		}
		try
		{
			Owner.Memory.OnPeaceChange -= method_13;
			Owner.GetPlayer.GetPlayer.BeingHitAction -= method_14;
			Owner.Memory.OnBulletNear -= method_16;
			Owner.Memory.OnSpottedByHit -= method_11;
			Owner.ShootData.OnTriggerPressed -= method_12;
			Owner.Memory.OnGoalEnemyChanged -= method_10;
		}
		catch (Exception)
		{
		}
		try
		{
			base.Dispose();
		}
		catch (Exception)
		{
		}
	}
}
