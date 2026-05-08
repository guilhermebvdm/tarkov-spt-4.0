using System;
using System.Runtime.CompilerServices;
using System.Threading;
using EFT;
using UnityEngine;

public class GClass413
{
	[CompilerGenerated]
	public class Class155
	{
		public IPlayer person;

		public BotsGroup botGroup;

		public void method_0()
		{
			botGroup.AddEnemy(person, EBotEnemyCause.pairLogic);
		}
	}

	public float _nextUpdateTime;

	[NonSerialized]
	public bool Bool_0;

	[CompilerGenerated]
	private Action<GClass413> action_0;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_0;

	[NonSerialized]
	[CompilerGenerated]
	public IPlayer Iplayer_0;

	[NonSerialized]
	[CompilerGenerated]
	public IPlayer Iplayer_1;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_1;

	[NonSerialized]
	[CompilerGenerated]
	public Vector3 Vector3_0;

	[NonSerialized]
	[CompilerGenerated]
	public EBotPairActionType EbotPairActionType_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_1;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_2;

	public float EndTime
	{
		[CompilerGenerated]
		get
		{
			return Float_0;
		}
		[CompilerGenerated]
		set
		{
			Float_0 = value;
		}
	}

	public IPlayer Person1
	{
		[CompilerGenerated]
		get
		{
			return Iplayer_0;
		}
	}

	public IPlayer Person2
	{
		[CompilerGenerated]
		get
		{
			return Iplayer_1;
		}
	}

	public float Dist
	{
		[CompilerGenerated]
		get
		{
			return Float_1;
		}
		[CompilerGenerated]
		set
		{
			Float_1 = value;
		}
	}

	public Vector3 Dir
	{
		[CompilerGenerated]
		get
		{
			return Vector3_0;
		}
		[CompilerGenerated]
		set
		{
			Vector3_0 = value;
		}
	}

	public EBotPairActionType ActionType
	{
		[CompilerGenerated]
		get
		{
			return EbotPairActionType_0;
		}
		[CompilerGenerated]
		set
		{
			EbotPairActionType_0 = value;
		}
	}

	public bool CanInteract
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

	public bool ShallStop
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

	public event Action<GClass413> OnPairDataStops
	{
		[CompilerGenerated]
		add
		{
			Action<GClass413> action = action_0;
			Action<GClass413> action2;
			do
			{
				action2 = action;
				Action<GClass413> value2 = (Action<GClass413>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<GClass413> action = action_0;
			Action<GClass413> action2;
			do
			{
				action2 = action;
				Action<GClass413> value2 = (Action<GClass413>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public GClass413(IPlayer person1, IPlayer person2)
	{
		Iplayer_0 = person1;
		Iplayer_1 = person2;
		method_3();
		Bool_0 = (Person1.IsAI && !Person2.IsAI) || (!Person1.IsAI && Person2.IsAI);
		CanInteract = true;
		if (Person1.IsAI && Person2.IsAI)
		{
			BotOwner botOwner = Person1.AIData.BotOwner;
			BotOwner botOwner2 = Person2.AIData.BotOwner;
			method_0(botOwner, botOwner2);
			method_0(botOwner2, botOwner);
		}
	}

	public void ManualUpdate()
	{
		if (Bool_0)
		{
			method_1();
		}
		method_3();
		if (EndTime < Time.time && ActionType != EBotPairActionType.none)
		{
			_nextUpdateTime = Time.time + LocalBotSettingsProviderClass.Core.PERIOD_NEXT_ACTION_SEC;
			ActionType = EBotPairActionType.none;
			action_0?.Invoke(this);
		}
	}

	public IPlayer GetAnother(IPlayer gamePerson)
	{
		if (Person1 != gamePerson)
		{
			return Person1;
		}
		return Person2;
	}

	public void SetAction(EBotPairActionType clap, float length, bool shallStop)
	{
		ShallStop = shallStop;
		ActionType = clap;
		EndTime = Time.time + length;
		_nextUpdateTime = Time.time + LocalBotSettingsProviderClass.Core.PERIOD_NEXT_ACTION_SEC;
	}

	public bool CanDoSign()
	{
		return _nextUpdateTime < Time.time;
	}

	public void StopPosibleInteractions()
	{
		CanInteract = false;
	}

	public bool AllAlive()
	{
		if (Person1.HealthController.IsAlive)
		{
			return Person2.HealthController.IsAlive;
		}
		return false;
	}

	public void method_0(BotOwner bot1, BotOwner bot2)
	{
		if (bot1.Boss.IamBoss && bot1.Boss.Followers.Contains(bot2))
		{
			CanInteract = false;
		}
	}

	public void method_1()
	{
		if (Dist > LocalBotSettingsProviderClass.Core.DIST_TO_BECOME_ENEMY)
		{
			return;
		}
		Vector3 vector = GClass855.NormalizeFastSelf(Dir);
		if (GClass855.IsAngLessNormalized(Person1.LookDirection, -vector, 0.1736482f) && GClass855.IsAngLessNormalized(Person2.LookDirection, vector, 0.1736482f))
		{
			if (Person1.IsAI)
			{
				method_2(Person1.AIData.BotOwner, Person2);
			}
			else if (Person2.IsAI)
			{
				method_2(Person2.AIData.BotOwner, Person1);
			}
		}
	}

	public void method_2(BotOwner bot, IPlayer person)
	{
		if (bot.Memory.IsPeace && !bot.BotRequestController.HaveActivatedRequests())
		{
			GClass641.IBotTimer botTimer = StaticManager.Instance.TimerManager.MakeTimer(TimeSpan.FromSeconds(LocalBotSettingsProviderClass.Core.DELAY_BEFORE_ENEMY));
			bot.Gesture.TryGestus(EInteraction.GetOffGesture, withAimingDelay: false);
			BotsGroup botGroup = bot.BotsGroup;
			botTimer.OnTimer += delegate
			{
				botGroup.AddEnemy(person, EBotEnemyCause.pairLogic);
			};
		}
	}

	public void method_3()
	{
		Dir = Person1.Position - Person2.Position;
		Dist = Dir.magnitude;
	}
}
