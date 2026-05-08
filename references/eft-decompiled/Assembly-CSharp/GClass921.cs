using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Systems.Effects;
using Comfort.Common;
using EFT;
using EFT.AssetsManager;
using EFT.HealthSystem;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.NextObservedPlayer;
using UnityEngine;

public class GClass921 : IHealthController, GInterface381
{
	public ETagStatus HealthStatus;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0 = true;

	[NonSerialized]
	public ObservedPlayerView ObservedPlayerView_0;

	[NonSerialized]
	public ObservedCorpse ObservedCorpse_0;

	[NonSerialized]
	public Player.GClass2004 Gclass2004_0;

	[NonSerialized]
	public EPhysicalCondition EphysicalCondition_0;

	[NonSerialized]
	public HashSet<string> HashSet_0 = new HashSet<string>();

	[NonSerialized]
	public bool Bool_1;

	[CompilerGenerated]
	private Action<EPhysicalCondition, EPhysicalCondition> action_0;

	[CompilerGenerated]
	private Action<IEffect> action_1;

	[CompilerGenerated]
	private Action<IEffect> action_2;

	[CompilerGenerated]
	private Action<IEffect> action_3;

	[CompilerGenerated]
	private Action<IEffect> action_4;

	[CompilerGenerated]
	private Action<IEffect> action_5;

	[CompilerGenerated]
	private Action<IEffect> action_6;

	[CompilerGenerated]
	private Action<EBodyPart, float, DamageInfoStruct> action_7;

	[CompilerGenerated]
	private Action<EBodyPart, float, DamageInfoStruct> action_8;

	[CompilerGenerated]
	private Action<float> action_9;

	[CompilerGenerated]
	private Action<float> action_10;

	[CompilerGenerated]
	private Action<float> action_11;

	[CompilerGenerated]
	private Action<EBodyPart, EDamageType> action_12;

	[CompilerGenerated]
	private Action<EBodyPart, ValueStruct> action_13;

	[CompilerGenerated]
	private Action<EDamageType> action_14;

	[CompilerGenerated]
	private Action<IEffect> action_15;

	[CompilerGenerated]
	private Action<Vector3, float, float> action_16;

	[CompilerGenerated]
	private Action<IPlayerBuff> action_17;

	[CompilerGenerated]
	private Action<IPlayerBuff> action_18;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_0;

	[NonSerialized]
	[CompilerGenerated]
	public ValueStruct ValueStruct_0;

	[NonSerialized]
	[CompilerGenerated]
	public ValueStruct ValueStruct_1;

	[NonSerialized]
	[CompilerGenerated]
	public ValueStruct ValueStruct_2;

	[NonSerialized]
	[CompilerGenerated]
	public ValueStruct ValueStruct_3;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_1;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_2;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_3;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_4;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_5;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_6;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_0;

	[NonSerialized]
	[CompilerGenerated]
	public Player Player_0;

	[NonSerialized]
	[CompilerGenerated]
	public HealthEffects HealthEffects_0;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_7;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_8;

	public bool IsAlive
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
		[CompilerGenerated]
		set
		{
			Bool_0 = value;
		}
	}

	public ObservedCorpse PlayerCorpse => ObservedCorpse_0;

	public float FallSafeHeight
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

	public ValueStruct Energy
	{
		[CompilerGenerated]
		get
		{
			return ValueStruct_0;
		}
	}

	public ValueStruct Hydration
	{
		[CompilerGenerated]
		get
		{
			return ValueStruct_1;
		}
	}

	public ValueStruct Temperature
	{
		[CompilerGenerated]
		get
		{
			return ValueStruct_2;
		}
	}

	public ValueStruct Poison
	{
		[CompilerGenerated]
		get
		{
			return ValueStruct_3;
		}
	}

	public float HealthRate
	{
		[CompilerGenerated]
		get
		{
			return Float_1;
		}
	}

	public float EnergyRate
	{
		[CompilerGenerated]
		get
		{
			return Float_2;
		}
	}

	public float HydrationRate
	{
		[CompilerGenerated]
		get
		{
			return Float_3;
		}
	}

	public float TemperatureRate
	{
		[CompilerGenerated]
		get
		{
			return Float_4;
		}
	}

	public float DamageCoeff
	{
		[CompilerGenerated]
		get
		{
			return Float_5;
		}
	}

	public float StaminaCoeff
	{
		[CompilerGenerated]
		get
		{
			return Float_6;
		}
	}

	public int UpdateTime
	{
		[CompilerGenerated]
		get
		{
			return Int_0;
		}
	}

	public Player Player
	{
		[CompilerGenerated]
		get
		{
			return Player_0;
		}
	}

	public HealthEffects BodyPartEffects
	{
		[CompilerGenerated]
		get
		{
			return HealthEffects_0;
		}
	}

	public float CarryingWeightAbsoluteModifier
	{
		[CompilerGenerated]
		get
		{
			return Float_7;
		}
	}

	public float CarryingWeightRelativeModifier
	{
		[CompilerGenerated]
		get
		{
			return Float_8;
		}
	}

	public event Action<EPhysicalCondition, EPhysicalCondition> OnPhysicalConditionChanged
	{
		[CompilerGenerated]
		add
		{
			Action<EPhysicalCondition, EPhysicalCondition> action = action_0;
			Action<EPhysicalCondition, EPhysicalCondition> action2;
			do
			{
				action2 = action;
				Action<EPhysicalCondition, EPhysicalCondition> value2 = (Action<EPhysicalCondition, EPhysicalCondition>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<EPhysicalCondition, EPhysicalCondition> action = action_0;
			Action<EPhysicalCondition, EPhysicalCondition> action2;
			do
			{
				action2 = action;
				Action<EPhysicalCondition, EPhysicalCondition> value2 = (Action<EPhysicalCondition, EPhysicalCondition>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<IEffect> EffectAddedEvent
	{
		[CompilerGenerated]
		add
		{
			Action<IEffect> action = action_1;
			Action<IEffect> action2;
			do
			{
				action2 = action;
				Action<IEffect> value2 = (Action<IEffect>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<IEffect> action = action_1;
			Action<IEffect> action2;
			do
			{
				action2 = action;
				Action<IEffect> value2 = (Action<IEffect>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<IEffect> EffectStartedEvent
	{
		[CompilerGenerated]
		add
		{
			Action<IEffect> action = action_2;
			Action<IEffect> action2;
			do
			{
				action2 = action;
				Action<IEffect> value2 = (Action<IEffect>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<IEffect> action = action_2;
			Action<IEffect> action2;
			do
			{
				action2 = action;
				Action<IEffect> value2 = (Action<IEffect>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<IEffect> EffectUpdatedEvent
	{
		[CompilerGenerated]
		add
		{
			Action<IEffect> action = action_3;
			Action<IEffect> action2;
			do
			{
				action2 = action;
				Action<IEffect> value2 = (Action<IEffect>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<IEffect> action = action_3;
			Action<IEffect> action2;
			do
			{
				action2 = action;
				Action<IEffect> value2 = (Action<IEffect>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<IEffect> EffectResidualEvent
	{
		[CompilerGenerated]
		add
		{
			Action<IEffect> action = action_4;
			Action<IEffect> action2;
			do
			{
				action2 = action;
				Action<IEffect> value2 = (Action<IEffect>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_4, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<IEffect> action = action_4;
			Action<IEffect> action2;
			do
			{
				action2 = action;
				Action<IEffect> value2 = (Action<IEffect>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_4, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<IEffect> EffectRemovedEvent
	{
		[CompilerGenerated]
		add
		{
			Action<IEffect> action = action_5;
			Action<IEffect> action2;
			do
			{
				action2 = action;
				Action<IEffect> value2 = (Action<IEffect>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_5, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<IEffect> action = action_5;
			Action<IEffect> action2;
			do
			{
				action2 = action;
				Action<IEffect> value2 = (Action<IEffect>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_5, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<IEffect> EffectStatusUpdateEvent
	{
		[CompilerGenerated]
		add
		{
			Action<IEffect> action = action_6;
			Action<IEffect> action2;
			do
			{
				action2 = action;
				Action<IEffect> value2 = (Action<IEffect>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_6, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<IEffect> action = action_6;
			Action<IEffect> action2;
			do
			{
				action2 = action;
				Action<IEffect> value2 = (Action<IEffect>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_6, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<EBodyPart, float, DamageInfoStruct> ApplyDamageEvent
	{
		[CompilerGenerated]
		add
		{
			Action<EBodyPart, float, DamageInfoStruct> action = action_7;
			Action<EBodyPart, float, DamageInfoStruct> action2;
			do
			{
				action2 = action;
				Action<EBodyPart, float, DamageInfoStruct> value2 = (Action<EBodyPart, float, DamageInfoStruct>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_7, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<EBodyPart, float, DamageInfoStruct> action = action_7;
			Action<EBodyPart, float, DamageInfoStruct> action2;
			do
			{
				action2 = action;
				Action<EBodyPart, float, DamageInfoStruct> value2 = (Action<EBodyPart, float, DamageInfoStruct>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_7, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<EBodyPart, float, DamageInfoStruct> HealthChangedEvent
	{
		[CompilerGenerated]
		add
		{
			Action<EBodyPart, float, DamageInfoStruct> action = action_8;
			Action<EBodyPart, float, DamageInfoStruct> action2;
			do
			{
				action2 = action;
				Action<EBodyPart, float, DamageInfoStruct> value2 = (Action<EBodyPart, float, DamageInfoStruct>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_8, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<EBodyPart, float, DamageInfoStruct> action = action_8;
			Action<EBodyPart, float, DamageInfoStruct> action2;
			do
			{
				action2 = action;
				Action<EBodyPart, float, DamageInfoStruct> value2 = (Action<EBodyPart, float, DamageInfoStruct>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_8, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<float> EnergyChangedEvent
	{
		[CompilerGenerated]
		add
		{
			Action<float> action = action_9;
			Action<float> action2;
			do
			{
				action2 = action;
				Action<float> value2 = (Action<float>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_9, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<float> action = action_9;
			Action<float> action2;
			do
			{
				action2 = action;
				Action<float> value2 = (Action<float>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_9, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<float> HydrationChangedEvent
	{
		[CompilerGenerated]
		add
		{
			Action<float> action = action_10;
			Action<float> action2;
			do
			{
				action2 = action;
				Action<float> value2 = (Action<float>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_10, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<float> action = action_10;
			Action<float> action2;
			do
			{
				action2 = action;
				Action<float> value2 = (Action<float>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_10, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<float> TemperatureChangedEvent
	{
		[CompilerGenerated]
		add
		{
			Action<float> action = action_11;
			Action<float> action2;
			do
			{
				action2 = action;
				Action<float> value2 = (Action<float>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_11, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<float> action = action_11;
			Action<float> action2;
			do
			{
				action2 = action;
				Action<float> value2 = (Action<float>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_11, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<EBodyPart, EDamageType> BodyPartDestroyedEvent
	{
		[CompilerGenerated]
		add
		{
			Action<EBodyPart, EDamageType> action = action_12;
			Action<EBodyPart, EDamageType> action2;
			do
			{
				action2 = action;
				Action<EBodyPart, EDamageType> value2 = (Action<EBodyPart, EDamageType>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_12, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<EBodyPart, EDamageType> action = action_12;
			Action<EBodyPart, EDamageType> action2;
			do
			{
				action2 = action;
				Action<EBodyPart, EDamageType> value2 = (Action<EBodyPart, EDamageType>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_12, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<EBodyPart, ValueStruct> BodyPartRestoredEvent
	{
		[CompilerGenerated]
		add
		{
			Action<EBodyPart, ValueStruct> action = action_13;
			Action<EBodyPart, ValueStruct> action2;
			do
			{
				action2 = action;
				Action<EBodyPart, ValueStruct> value2 = (Action<EBodyPart, ValueStruct>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_13, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<EBodyPart, ValueStruct> action = action_13;
			Action<EBodyPart, ValueStruct> action2;
			do
			{
				action2 = action;
				Action<EBodyPart, ValueStruct> value2 = (Action<EBodyPart, ValueStruct>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_13, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<EDamageType> DiedEvent
	{
		[CompilerGenerated]
		add
		{
			Action<EDamageType> action = action_14;
			Action<EDamageType> action2;
			do
			{
				action2 = action;
				Action<EDamageType> value2 = (Action<EDamageType>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_14, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<EDamageType> action = action_14;
			Action<EDamageType> action2;
			do
			{
				action2 = action;
				Action<EDamageType> value2 = (Action<EDamageType>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_14, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<IEffect> HealerDoneEvent
	{
		[CompilerGenerated]
		add
		{
			Action<IEffect> action = action_15;
			Action<IEffect> action2;
			do
			{
				action2 = action;
				Action<IEffect> value2 = (Action<IEffect>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_15, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<IEffect> action = action_15;
			Action<IEffect> action2;
			do
			{
				action2 = action;
				Action<IEffect> value2 = (Action<IEffect>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_15, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<Vector3, float, float> BurnEyesEvent
	{
		[CompilerGenerated]
		add
		{
			Action<Vector3, float, float> action = action_16;
			Action<Vector3, float, float> action2;
			do
			{
				action2 = action;
				Action<Vector3, float, float> value2 = (Action<Vector3, float, float>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_16, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<Vector3, float, float> action = action_16;
			Action<Vector3, float, float> action2;
			do
			{
				action2 = action;
				Action<Vector3, float, float> value2 = (Action<Vector3, float, float>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_16, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<IPlayerBuff> StimulatorBuffEvent
	{
		[CompilerGenerated]
		add
		{
			Action<IPlayerBuff> action = action_17;
			Action<IPlayerBuff> action2;
			do
			{
				action2 = action;
				Action<IPlayerBuff> value2 = (Action<IPlayerBuff>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_17, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<IPlayerBuff> action = action_17;
			Action<IPlayerBuff> action2;
			do
			{
				action2 = action;
				Action<IPlayerBuff> value2 = (Action<IPlayerBuff>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_17, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<IPlayerBuff> StimulatorBuffActivationEvent
	{
		[CompilerGenerated]
		add
		{
			Action<IPlayerBuff> action = action_18;
			Action<IPlayerBuff> action2;
			do
			{
				action2 = action;
				Action<IPlayerBuff> value2 = (Action<IPlayerBuff>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_18, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<IPlayerBuff> action = action_18;
			Action<IPlayerBuff> action2;
			do
			{
				action2 = action;
				Action<IPlayerBuff> value2 = (Action<IPlayerBuff>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_18, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public GClass921(ObservedPlayerView player)
	{
		ObservedPlayerView_0 = player;
	}

	public bool PhysicalConditionContainsAny(EPhysicalCondition c)
	{
		return (EphysicalCondition_0 & c) != 0;
	}

	public bool PhysicalConditionIs(EPhysicalCondition c)
	{
		return (EphysicalCondition_0 & c) == c;
	}

	public void method_0(EPhysicalCondition c)
	{
		EphysicalCondition_0 = c;
	}

	public bool HaveMedEffectWithID(string ID)
	{
		return HashSet_0.Contains(ID);
	}

	public void Apply(GStruct342 model)
	{
		method_0(model.PhysicalCondition);
	}

	public void Execute(ICommandMessage command)
	{
		switch (command.Type)
		{
		case CommandMessageType.EquipChanged:
			method_1(command as GClass2803);
			break;
		case CommandMessageType.EffectOnPlayer:
			method_4(command as GClass2810);
			break;
		case CommandMessageType.Temperature:
			method_3(command as GClass2843);
			break;
		case CommandMessageType.HealthStatus:
			method_5(command as GClass2816);
			break;
		case CommandMessageType.MedEffectStatus:
			method_2(command as GClass2822);
			break;
		}
	}

	public void method_1(GClass2803 command)
	{
		if (command.Item == null && !(ObservedCorpse_0 == null) && ObservedCorpse_0.ItemInHands != null && ObservedCorpse_0.ItemInHands.Value != null && command.ItemId == (MongoID)ObservedCorpse_0.ItemInHands.Value.Id)
		{
			ObservedCorpse_0.ClearWeaponInHands();
		}
	}

	public void method_2(GClass2822 medEffectStatusCommandMessage)
	{
		if (medEffectStatusCommandMessage.EffectAdded)
		{
			HashSet_0.Add(medEffectStatusCommandMessage.EffectID);
		}
		else
		{
			HashSet_0.Remove(medEffectStatusCommandMessage.EffectID);
		}
	}

	public void method_3(GClass2843 temperatureCommandMessage)
	{
		ObservedPlayerView_0.PlayerBody.SetTemperatureForBody(temperatureCommandMessage.Temperature);
	}

	public void method_4(GClass2810 effectOnPlayerStatusCommandMessage)
	{
		if (effectOnPlayerStatusCommandMessage.ChangedEffectType == GClass2810.EHealthEffectType.HeavyBleeding && Singleton<Effects>.Instantiated)
		{
			if (effectOnPlayerStatusCommandMessage.IsActive)
			{
				Singleton<Effects>.Instance.EffectsCommutator.StartBleedingForPlayer(ObservedPlayerView_0);
			}
			else
			{
				Singleton<Effects>.Instance.EffectsCommutator.StopBleedingForPlayer(ObservedPlayerView_0);
			}
		}
	}

	public void method_5(GClass2816 healthStatusCommandMessage)
	{
		if (!Bool_1)
		{
			IsAlive = healthStatusCommandMessage.isAlive;
		}
		HealthStatus = healthStatusCommandMessage.HealthStatus;
	}

	public void ApplyDeath(GClass2806 deathPacket)
	{
		if (Singleton<Effects>.Instantiated)
		{
			Singleton<Effects>.Instance.EffectsCommutator.StopBleedingForPlayer(ObservedPlayerView_0);
		}
		Bool_1 = true;
		IsAlive = false;
		ObservedPlayerView_0.BodyAnimator.enabled = false;
		Collider[] colliders = ObservedPlayerView_0.Colliders;
		for (int i = 0; i < colliders.Length; i++)
		{
			colliders[i].enabled = true;
		}
		ObservedPlayerView_0.ApplyDeath();
		if (Singleton<GameWorld>.Instantiated)
		{
			Singleton<GameWorld>.Instance.UnregisterPlayer(ObservedPlayerView_0);
		}
		bool flag = !ObservedPlayerView_0.ObservedPlayerController.Culling.IsVisible;
		ObservedCorpse_0 = method_6(deathPacket.CorpseImpulse.OverallVelocity, flag);
		ObservedCorpse_0.IsZombieCorpse = ObservedPlayerView_0.UsedSimplifiedSkeleton;
		ObservedCorpse_0.SetSpecificSettings(ObservedPlayerView_0.PlayerBones.RightPalm);
		Singleton<GameWorld>.Instance.ObservedPlayersCorpses.Add(ObservedPlayerView_0.ObservedPlayerController.EquipmentViewController.Equipment.Id.GetHashCode(), ObservedCorpse_0);
		if (!flag)
		{
			method_10(deathPacket.CorpseImpulse.BodyPartColliderType, deathPacket.CorpseImpulse.Direction, deathPacket.CorpseImpulse.Point, deathPacket.CorpseImpulse.Force);
		}
		if (ObservedPlayerView_0.BundleAnimationBones.StationaryWeapon != null)
		{
			ObservedPlayerView_0.BundleAnimationBones.ApplyDeath();
			ObservedPlayerView_0.ObservedPlayerController.HandsController.ResetAnimatorOnDespawnHands();
			ObservedPlayerView_0.ObservedPlayerController.HandsController.DestroyHandsController();
			method_9();
		}
		else
		{
			ObservedCorpse_0.SetItemInHandsLootedCallback(method_9);
			method_7(ObservedPlayerView_0.ObservedPlayerController.HandsController.ItemInHands, ObservedPlayerView_0.ObservedPlayerController.HandsController.CurrentWeaponPrefab.gameObject, deathPacket.CorpseImpulse.OverallVelocity);
		}
		if (flag)
		{
			ObservedCorpse_0.ForceStill();
		}
	}

	public ObservedCorpse method_6(Vector3 overallVelocity, bool simulateCorpse)
	{
		GClass768 containerCollectionView = null;
		GameObject gameObject = ObservedPlayerView_0.gameObject;
		string profileId = ObservedPlayerView_0.ProfileId;
		InventoryEquipment equipment = ObservedPlayerView_0.ObservedPlayerController.EquipmentViewController.Equipment;
		GClass2197 bodyCustomization = ObservedPlayerView_0.PlayerBody.BodyCustomization;
		GameWorld instance = Singleton<GameWorld>.Instance;
		EPlayerSide side = ObservedPlayerView_0.Side;
		bool foreStillCorpse = simulateCorpse;
		return Corpse.CreateCorpse<ObservedCorpse>(gameObject, profileId, equipment, bodyCustomization, reinitBody: false, instance, side, overallVelocity, ObservedPlayerView_0.PlayerBones.Pelvis.Original, ObservedPlayerView_0.ObservedPlayerController.EquipmentViewController.ItemInHands, foreStillCorpse, containerCollectionView);
	}

	public void method_7(Item item, GameObject prefab, Vector3 velocity)
	{
		int deadbodyLayer = LayerMaskClass.DeadbodyLayer;
		int shellsLayer = LayerMaskClass.ShellsLayer;
		AssetPoolObject[] componentsInChildren = prefab.GetComponentsInChildren<AssetPoolObject>(includeInactive: true);
		Collider collider = null;
		AssetPoolObject[] array = componentsInChildren;
		foreach (AssetPoolObject assetPoolObject in array)
		{
			foreach (Collider collider2 in assetPoolObject.Colliders)
			{
				if (!collider2.isTrigger && collider2.gameObject.layer != shellsLayer)
				{
					assetPoolObject.StoreCollider(collider2);
					collider2.enabled = true;
					collider2.gameObject.layer = deadbodyLayer;
					if (collider == null || collider.bounds.extents.sqrMagnitude < collider2.bounds.extents.sqrMagnitude)
					{
						collider = collider2;
					}
				}
			}
		}
		Gclass2004_0 = new Player.GClass2004
		{
			Transform = prefab.transform
		};
		Rigidbody rigidbody = prefab.AddComponent<Rigidbody>();
		LootItem component = prefab.GetComponent<LootItem>();
		if (component != null)
		{
			component.SetItemAndRigidbody(item, rigidbody);
		}
		Gclass2004_0.Shift = ((rigidbody != null) ? rigidbody.centerOfMass : Vector3.zero);
		if ((bool)collider)
		{
			Gclass2004_0.Transportee = collider.gameObject.AddComponent<CommonTransportee>();
			Gclass2004_0.Transportee.ParentTransform = prefab.transform;
		}
		foreach (Transform item2 in prefab.transform)
		{
			item2.localPosition -= Gclass2004_0.Shift;
		}
		TransformLinks componentInChildren = prefab.GetComponentInChildren<TransformLinks>();
		bool flag;
		if (!(flag = item is PistolItemClass || item is ThrowWeapItemClass || item.GetItemComponent<KnifeComponent>() != null))
		{
			ObservedPlayerView_0.ObservedPlayerController.HandsController.BundleAnimationBones.HandPosers[1].Lerp2Target(EFTHardSettings.Instance.RIGHT_HAND_QTS, 5f);
		}
		RigidbodySpawner obj = (flag ? ObservedPlayerView_0.PlayerBones.Forearms[1].GetComponent<RigidbodySpawner>() : ObservedPlayerView_0.GetComponentInChildren<RigidbodySpawner>());
		ObservedCorpse_0.Ragdoll.AttachWeapon(rigidbody, ObservedPlayerView_0.gameObject, ObservedPlayerView_0.PlayerBones, componentInChildren, flag, velocity);
		obj.RemoveEvent += method_8;
	}

	public void method_8(RigidbodySpawner spawner)
	{
		if (spawner != null)
		{
			spawner.RemoveEvent -= method_8;
		}
		Gclass2004_0?.RemovePhysics();
	}

	public void method_9()
	{
		if (Gclass2004_0 != null)
		{
			Gclass2004_0.Destroy();
			Gclass2004_0 = null;
		}
		ObservedPlayerView_0.ObservedPlayerController.HandsController.ResetAnimatorOnDespawnHands();
		ObservedPlayerView_0.ObservedPlayerController.HandsController.DestroyHandsController();
		if (!ObservedPlayerView_0.UsedSimplifiedSkeleton)
		{
			ObservedPlayerView_0.ObservedPlayerController.HandsController.BundleAnimationBones.HandPosers[1].Lerp2Target(EFTHardSettings.Instance.RIGHT_HAND_QTS, 5f);
		}
	}

	public void method_10(EBodyPartColliderType bodyPartColliderType, Vector3 direction, Vector3 point, float force)
	{
		if (bodyPartColliderType != EBodyPartColliderType.None && ObservedPlayerView_0.PlayerBones.BodyPartCollidersDictionary.TryGetValue(bodyPartColliderType, out var value))
		{
			ObservedCorpse_0.Ragdoll.ApplyImpulse(value.Collider, direction, point, force);
		}
	}

	public ValueStruct GetBodyPartHealth(EBodyPart bodyPart, bool rounded = false)
	{
		throw new NotImplementedException();
	}

	public bool IsBodyPartBroken(EBodyPart bodyPart)
	{
		throw new NotImplementedException();
	}

	public bool IsBodyPartDestroyed(EBodyPart bodyPart)
	{
		throw new NotImplementedException();
	}

	public void GetBodyPartsInCriticalCondition(float threshold, out int all, out int vital)
	{
		throw new NotImplementedException();
	}

	public void SetEncumbered(bool encumbered)
	{
	}

	public void SetOverEncumbered(bool encumbered)
	{
	}

	public void AddFatigue()
	{
	}

	public void AddImmunityNotificationEffect()
	{
	}

	public TEffect FindExistingEffect<TEffect>(EBodyPart bodyPart = EBodyPart.Common) where TEffect : IEffect
	{
		throw new NotImplementedException();
	}

	public TEffect FindActiveEffect<TEffect>(EBodyPart bodyPart = EBodyPart.Common) where TEffect : IEffect
	{
		throw new NotImplementedException();
	}

	public IEnumerable<TEffect> FindActiveEffects<TEffect>(EBodyPart bodyPart = EBodyPart.Common) where TEffect : IEffect
	{
		throw new NotImplementedException();
	}

	public IEnumerable<IEffect> GetAllActiveEffects(EBodyPart bodyPart = EBodyPart.Common)
	{
		throw new NotImplementedException();
	}

	public IEnumerable<IEffect> GetAllEffects(EBodyPart bodyPart = EBodyPart.Common)
	{
		throw new NotImplementedException();
	}

	public IEnumerable<IEffect> GetAllResidualEffects(EBodyPart bodyPart = EBodyPart.Common)
	{
		throw new NotImplementedException();
	}

	public GStruct382<EBodyPart> BodyPartsPriority(Item item, bool continuousHealEnabled)
	{
		throw new NotImplementedException();
	}

	public bool IsItemForHealing(Item item)
	{
		throw new NotImplementedException();
	}

	public IResult HasPartsToApply(Item item)
	{
		throw new NotImplementedException();
	}

	public bool CanApplyItem(Item item, EBodyPart bodyPart)
	{
		throw new InvalidOperationException();
	}

	public bool ApplyItem(Item item, GStruct382<EBodyPart> bodyPart, float? amount = null)
	{
		throw new InvalidOperationException();
	}

	public bool ApplyItem(Item item, EBodyPart bodyPart, float? amount = null)
	{
		throw new NotImplementedException();
	}

	public void CancelApplyingItem()
	{
	}

	public void ManualUpdate(float deltaTime)
	{
	}

	public void PropagateAllEffects()
	{
	}

	public string[] ActiveBuffsNames()
	{
		throw new NotImplementedException();
	}

	public void PauseAllEffects()
	{
	}

	public void UnpauseAllEffects()
	{
	}

	public void method_11(IEffect iHealthEffect)
	{
		action_1?.Invoke(iHealthEffect);
	}

	public void method_12(IEffect iHealthEffect)
	{
		action_2?.Invoke(iHealthEffect);
	}

	public void method_13(IEffect iHealthEffect)
	{
		action_3?.Invoke(iHealthEffect);
	}

	public void method_14(IEffect iHealthEffect)
	{
		action_4?.Invoke(iHealthEffect);
	}

	public void method_15(IEffect iHealthEffect)
	{
		action_5?.Invoke(iHealthEffect);
	}

	public void method_16(IEffect iHealthEffect)
	{
		action_6?.Invoke(iHealthEffect);
	}

	public void method_17(EBodyPart eBodyPart, float damage, DamageInfoStruct damageInfo)
	{
		action_7?.Invoke(eBodyPart, damage, damageInfo);
	}

	public void method_18(EBodyPart eBodyPart, float damage, DamageInfoStruct damageInfo)
	{
		action_8?.Invoke(eBodyPart, damage, damageInfo);
	}

	public void method_19(float delta)
	{
		action_9?.Invoke(delta);
	}

	public void method_20(float delta)
	{
		action_10?.Invoke(delta);
	}

	public void method_21(float delta)
	{
		action_11?.Invoke(delta);
	}

	public void method_22(EBodyPart eBodyPart, EDamageType eDamageType)
	{
		action_12?.Invoke(eBodyPart, eDamageType);
	}

	public void method_23(EBodyPart eBodyPart, ValueStruct value)
	{
		action_13?.Invoke(eBodyPart, value);
	}

	public void method_24(EDamageType eDamageType)
	{
		action_14?.Invoke(eDamageType);
	}

	public void method_25(IEffect iHealthEffect)
	{
		action_15?.Invoke(iHealthEffect);
	}

	public void method_26(Vector3 position, float str, float time)
	{
		action_16?.Invoke(position, str, time);
	}

	public void method_27(IPlayerBuff stimulatorBuff)
	{
		action_17?.Invoke(stimulatorBuff);
	}

	public void method_28(IPlayerBuff stimulatorBuff)
	{
		action_18?.Invoke(stimulatorBuff);
	}

	public void DisableMetabolism()
	{
	}
}
