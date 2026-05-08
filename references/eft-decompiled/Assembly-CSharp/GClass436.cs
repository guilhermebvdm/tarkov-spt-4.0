using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Comfort.Common;
using EFT;
using UnityEngine;

public class GClass436 : ABossLogic
{
	public const string ALARM_EVENT_END = "autoId_00000_ALARM_KIBA_CONSOLE";

	public const string ALARM_EVENT_START = "autoId_00000_ALARM_KIBA";

	[NonSerialized]
	public const string String_0 = "AlarmBounds";

	[NonSerialized]
	public const string String_1 = "KILLA_PATROL_ALT";

	[NonSerialized]
	public BotLastBlindEffectModifierClass BotLastBlindEffectModifierClass;

	[NonSerialized]
	public int Int_0 = 1;

	[NonSerialized]
	public bool Bool_0;

	[CompilerGenerated]
	private Action<bool> action_0;

	[NonSerialized]
	[CompilerGenerated]
	public AIPlaceInfo AiplaceInfo_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_1;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_2;

	public AIPlaceInfo AlarmBounds
	{
		[CompilerGenerated]
		get
		{
			return AiplaceInfo_0;
		}
		[CompilerGenerated]
		set
		{
			AiplaceInfo_0 = value;
		}
	}

	public bool HaveSecurityInfo
	{
		[CompilerGenerated]
		get
		{
			return Bool_1;
		}
		[CompilerGenerated]
		set
		{
			Bool_1 = value;
		}
	}

	public bool AlarmActivated
	{
		[CompilerGenerated]
		get
		{
			return Bool_2;
		}
		[CompilerGenerated]
		set
		{
			Bool_2 = value;
		}
	}

	public event Action<bool> OnAlarmChange
	{
		[CompilerGenerated]
		add
		{
			Action<bool> action = action_0;
			Action<bool> action2;
			do
			{
				action2 = action;
				Action<bool> value2 = (Action<bool>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<bool> action = action_0;
			Action<bool> action2;
			do
			{
				action2 = action;
				Action<bool> value2 = (Action<bool>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public GClass436(BotOwner owner, BotBoss bossLogic)
		: base(owner, bossLogic)
	{
		BotOwner_0.Memory.OnBulletNear += method_2;
		BotOwner_0.Memory.OnPeaceChange += method_0;
	}

	public override void SetPatrolMode()
	{
		GClass559 pointChooser = new GClass559(BotOwner_0, BotOwner_0.SpawnProfileData, "KILLA_PATROL_ALT");
		BotOwner_0.PatrollingData.SetMode(PatrolMode.bossRoundProtect, pointChooser);
	}

	public override void Activate()
	{
		BotOwner_0.GetPlayer.ActiveHealthController.DoPainKiller();
		AIPlaceInfo alarmBounds = BotOwner_0.BotsGroup.BotGame.BotsController.CoversData.AIPlaceInfoHolder.FindPlace("AlarmBounds");
		AlarmBounds = alarmBounds;
		HaveSecurityInfo = AlarmBounds != null;
		BotEventHandler instance = Singleton<BotEventHandler>.Instance;
		if (instance != null)
		{
			instance.OnInteractObject += method_3;
		}
	}

	public override void BossLogicUpdate()
	{
	}

	public void method_0(bool isPeace)
	{
		if (BotOwner_0.Memory.IsPeace)
		{
			Int_0 = 0;
			method_1();
		}
	}

	public void method_1()
	{
		if (BotLastBlindEffectModifierClass != null && BotLastBlindEffectModifierClass.IsApplyed)
		{
			BotOwner_0.Settings.Current.Dismiss(BotLastBlindEffectModifierClass);
		}
	}

	public void method_2(BotOwner bot, IPlayer source)
	{
		method_1();
		Int_0++;
		Int_0 = Mathf.Clamp(Int_0, 0, 6);
		BotLastBlindEffectModifierClass = new BotLastBlindEffectModifierClass();
		BotLastBlindEffectModifierClass.TriggerDownDelay = BotOwner_0.Settings.FileSettings.Boss.KILLA_TRIGGER_DOWN_DELAY * (float)Int_0;
		BotLastBlindEffectModifierClass.WaitInCoverCoef = BotOwner_0.Settings.FileSettings.Boss.KILLA_WAIT_IN_COVER_COEF * (float)Int_0;
		BotOwner_0.Settings.Current.Apply(BotLastBlindEffectModifierClass);
	}

	public void method_3(string doorId, Vector3 position)
	{
		if (doorId == "autoId_00000_ALARM_KIBA" && !Bool_0)
		{
			Bool_0 = true;
			AlarmActivated = true;
			action_0?.Invoke(AlarmActivated);
		}
		else if ("autoId_00000_ALARM_KIBA_CONSOLE" == doorId && Bool_0)
		{
			AlarmActivated = false;
			action_0?.Invoke(AlarmActivated);
		}
	}

	public override void Dispose()
	{
		if (BotOwner_0.Memory != null)
		{
			BotOwner_0.Memory.OnBulletNear -= method_2;
			BotOwner_0.Memory.OnPeaceChange -= method_0;
		}
	}
}
