using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Comfort.Common;
using EFT.InventoryLogic;
using JetBrains.Annotations;
using UnityEngine;

namespace EFT;

public class SkillManager : IInventoryProfileSkillInfo
{
	public class GClass2250
	{
		public float DeltaErgonomics;

		public float ReloadSpeed = 1f;

		public Vector2 RecoilSupression;

		public float FixSpeed = 1f;

		public float SwapSpeed = 1f;

		public float AimSpeed;

		public bool StiffDraw;

		public float AimMovementSpeed;

		public bool Maxed;

		public float DoubleActionRecoilReduce;

		public float MountingBonusErgo;

		public float BipodBonusErgo;
	}

	public enum EBuffType
	{
		Simple,
		Elite,
		Switching,
		Plebian
	}

	public struct GStruct279
	{
		public float Noise;

		public float Overweight;

		public float Fatigue;
	}

	public class GClass2251
	{
		public float Distance;

		public EBodyPart BodyPart;

		public bool HoldBreath;
	}

	public abstract class SkillBuffAbstractClass
	{
		public EBuffId Id;

		public EBuffType BuffType;

		public bool HidenForPlayers;

		public Action<int> BaseRuleFunc;

		public Action EliteRuleFunc;

		public virtual object ValueObj => null;

		public SkillBuffAbstractClass()
		{
		}
	}

	public class SkillBuffClass : SkillBuffAbstractClass
	{
		[CompilerGenerated]
		public class Class1425
		{
			public SkillBuffClass SkillBuffClass;

			public float factor;

			public void method_0(int level)
			{
				SkillBuffClass.Value = (float)level * factor;
			}
		}

		[CompilerGenerated]
		public class Class1426
		{
			public SkillBuffClass SkillBuffClass;

			public float factor;

			public void method_0(int level)
			{
				SkillBuffClass.Value = (float)level * factor;
			}
		}

		[CompilerGenerated]
		public class Class1427
		{
			public SkillBuffClass SkillBuffClass;

			public Func<int, float> rule;

			public void method_0(int level)
			{
				SkillBuffClass.Value = rule(level);
			}
		}

		[CompilerGenerated]
		public class Class1428
		{
			public SkillBuffClass SkillBuffClass;

			public float absolute;

			public void method_0()
			{
				SkillBuffClass.Value = absolute;
			}
		}

		public float Value;

		public override object ValueObj => Value;

		public static implicit operator float(SkillBuffClass buff)
		{
			return buff.Value;
		}

		public virtual SkillBuffClass Default(float val)
		{
			Value = val;
			return this;
		}

		public SkillBuffClass PerLevel(float factor)
		{
			BaseRuleFunc = delegate(int level)
			{
				Value = (float)level * factor;
			};
			return this;
		}

		public SkillBuffClass Max(float factor)
		{
			factor /= 50f;
			BaseRuleFunc = delegate(int level)
			{
				Value = (float)level * factor;
			};
			return this;
		}

		public SkillBuffClass Custom(Func<int, float> rule)
		{
			BaseRuleFunc = delegate(int level)
			{
				Value = rule(level);
			};
			return this;
		}

		public SkillBuffClass Elite(float absolute)
		{
			EliteRuleFunc = delegate
			{
				Value = absolute;
			};
			return this;
		}

		public virtual void Apply(ref float val)
		{
			throw new NotImplementedException("FloatBuff shall be applied manually");
		}

		public virtual float Apply(float val)
		{
			throw new NotImplementedException("FloatBuff shall be applied manually");
		}
	}

	public class GClass2254 : SkillBuffClass
	{
		[CompilerGenerated]
		private Action<bool> action_0;

		public event Action<bool> OnResult
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

		public bool Roll()
		{
			if (Value <= 0f)
			{
				return false;
			}
			bool flag = UnityEngine.Random.Range(0f, 1f) <= Value;
			action_0?.Invoke(flag);
			return flag;
		}

		public bool Roll(float precomputed)
		{
			if (Value <= 0f)
			{
				return false;
			}
			bool flag = precomputed <= Value;
			action_0?.Invoke(flag);
			return flag;
		}
	}

	public class GClass2255 : SkillBuffClass
	{
		public override void Apply(ref float val)
		{
			val *= 1f + Value;
		}

		public override float Apply(float val)
		{
			return val * (1f + Value);
		}
	}

	public class GClass2256 : SkillBuffClass
	{
		public override void Apply(ref float val)
		{
			val *= 1f - Value;
		}

		public override float Apply(float val)
		{
			return val * (1f - Value);
		}
	}

	public class GClass2257 : SkillBuffAbstractClass
	{
		public bool Value;

		public override object ValueObj => Value;

		public GClass2257()
		{
			BaseRuleFunc = delegate
			{
				Value = false;
			};
			EliteRuleFunc = delegate
			{
				Value = true;
			};
		}

		public static implicit operator bool(GClass2257 buff)
		{
			return buff.Value;
		}

		[CompilerGenerated]
		public void method_0(int level)
		{
			Value = false;
		}

		[CompilerGenerated]
		public void method_1()
		{
			Value = true;
		}
	}

	[Obsolete]
	public class GClass2258<T> : SkillBuffAbstractClass
	{
		[CompilerGenerated]
		public class Class1429
		{
			public GClass2258<T> gclass2258_0;

			public Func<int, T> rule;

			public T method_0(int level)
			{
				return gclass2258_0.Value = rule(level);
			}
		}

		[CompilerGenerated]
		public class Class1430
		{
			public GClass2258<T> gclass2258_0;

			public Func<T> rule;

			public T method_0()
			{
				return gclass2258_0.Value = rule();
			}
		}

		public T Value;

		public new Func<int, T> BaseRuleFunc;

		public new Func<T> EliteRuleFunc;

		public override object ValueObj => Value;

		public static implicit operator T(GClass2258<T> buff)
		{
			return buff.Value;
		}

		public GClass2258<T> BaseRule(Func<int, T> rule)
		{
			BaseRuleFunc = (int level) => Value = rule(level);
			return this;
		}

		public GClass2258<T> EliteRule(Func<T> rule)
		{
			EliteRuleFunc = () => Value = rule();
			return this;
		}
	}

	public class SkillActionClass
	{
		[CompilerGenerated]
		public class Class1431
		{
			public string log;

			public void method_0(SkillActionClass _, float f)
			{
				Debug.Log(string.Format(log, f));
			}
		}

		[CompilerGenerated]
		private Action<SkillActionClass, float> action_0;

		public float FactorValue = 1f;

		public bool SimpleCalculation;

		[NonSerialized]
		public float Float_0;

		public event Action<SkillActionClass, float> ExternalEvent
		{
			[CompilerGenerated]
			add
			{
				Action<SkillActionClass, float> action = action_0;
				Action<SkillActionClass, float> action2;
				do
				{
					action2 = action;
					Action<SkillActionClass, float> value2 = (Action<SkillActionClass, float>)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange(ref action_0, value2, action2);
				}
				while ((object)action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action<SkillActionClass, float> action = action_0;
				Action<SkillActionClass, float> action2;
				do
				{
					action2 = action;
					Action<SkillActionClass, float> value2 = (Action<SkillActionClass, float>)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange(ref action_0, value2, action2);
				}
				while ((object)action != action2);
			}
		}

		public virtual void Complete(float val = 1f)
		{
			if (Float_0 > 0f)
			{
				method_0(Time.time - Float_0);
				Float_0 = 0f;
			}
			else
			{
				method_0(val);
			}
		}

		public SkillActionClass Log(string log)
		{
			ExternalEvent += delegate(SkillActionClass _, float f)
			{
				Debug.Log(string.Format(log, f));
			};
			return this;
		}

		public virtual void Begin()
		{
			Float_0 = Time.time;
		}

		public void method_0(float val)
		{
			action_0?.Invoke(this, val * FactorValue);
		}

		public SkillActionClass Factor(float fac, bool nonWeaponSkill = true)
		{
			FactorValue = (nonWeaponSkill ? (fac * Singleton<BackendConfigSettingsClass>.Instance.SkillsSettings.SkillProgressRate) : (fac * Singleton<BackendConfigSettingsClass>.Instance.SkillsSettings.WeaponSkillProgressRate));
			return this;
		}
	}

	public class GClass2260<T> : SkillActionClass
	{
		[CompilerGenerated]
		public class Class1432
		{
			public Func<T, bool> condition;

			public GClass2260<T> gclass2260_0;

			public void method_0(float val, T filter1)
			{
				if (condition(filter1))
				{
					gclass2260_0.method_0(val);
				}
			}
		}

		[NonSerialized]
		[CompilerGenerated]
		public Action<float, T> Action_1;

		public event Action<float, T> InnerEvent
		{
			[CompilerGenerated]
			add
			{
				Action<float, T> action = Action_1;
				Action<float, T> action2;
				do
				{
					action2 = action;
					Action<float, T> value2 = (Action<float, T>)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange(ref Action_1, value2, action2);
				}
				while ((object)action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action<float, T> action = Action_1;
				Action<float, T> action2;
				do
				{
					action2 = action;
					Action<float, T> value2 = (Action<float, T>)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange(ref Action_1, value2, action2);
				}
				while ((object)action != action2);
			}
		}

		public GClass2260()
		{
		}

		public GClass2260(GClass2260<T> action, Func<T, bool> condition)
		{
			GClass2260<T> gclass2260_0 = this;
			action.InnerEvent += delegate(float val, T filter1)
			{
				if (condition(filter1))
				{
					gclass2260_0.method_0(val);
				}
			};
		}

		public void Complete(T filter, float val = 1f)
		{
			Action_1?.Invoke((Float_0 > 0f) ? (Time.time - Float_0) : (val * FactorValue), filter);
		}

		public SkillActionClass Where(Func<T, bool> condition)
		{
			return new GClass2260<T>(this, condition);
		}
	}

	public class GClass2261<T> : SkillActionClass
	{
		[CompilerGenerated]
		public class Class1433
		{
			public Func<T, float> condition;

			public GClass2261<T> gclass2261_0;

			public void method_0(float val, T filter1)
			{
				float num = condition(filter1);
				gclass2261_0.FactorValue = Singleton<BackendConfigSettingsClass>.Instance.SkillsSettings.SkillProgressRate;
				if (num > 0f)
				{
					gclass2261_0.method_0(val * num);
				}
			}
		}

		[NonSerialized]
		[CompilerGenerated]
		public Action<float, T> Action_1;

		public event Action<float, T> InnerEvent
		{
			[CompilerGenerated]
			add
			{
				Action<float, T> action = Action_1;
				Action<float, T> action2;
				do
				{
					action2 = action;
					Action<float, T> value2 = (Action<float, T>)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange(ref Action_1, value2, action2);
				}
				while ((object)action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action<float, T> action = Action_1;
				Action<float, T> action2;
				do
				{
					action2 = action;
					Action<float, T> value2 = (Action<float, T>)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange(ref Action_1, value2, action2);
				}
				while ((object)action != action2);
			}
		}

		public GClass2261()
		{
			Factor(1f);
		}

		public GClass2261(GClass2261<T> action, Func<T, float> condition)
		{
			GClass2261<T> gclass2261_0 = this;
			action.InnerEvent += delegate(float val, T filter1)
			{
				float num = condition(filter1);
				gclass2261_0.FactorValue = Singleton<BackendConfigSettingsClass>.Instance.SkillsSettings.SkillProgressRate;
				if (num > 0f)
				{
					gclass2261_0.method_0(val * num);
				}
			};
		}

		public void Complete(T filter, float val = 1f)
		{
			Action_1?.Invoke((Float_0 > 0f) ? (Time.time - Float_0) : val, filter);
		}

		public SkillActionClass Factor(Func<T, float> condition)
		{
			return new GClass2261<T>(this, condition);
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class1434
	{
		public static readonly Class1434 class1434_0 = new Class1434();

		public static Func<int, float> func_0;

		public static Func<SkillClass, bool> func_1;

		public static Func<SkillClass, bool> func_2;

		public static Func<bool, bool> func_3;

		public static Func<bool, bool> func_4;

		public static Func<SkillClass, bool> func_5;

		public static Func<SkillClass, bool> func_6;

		public static Func<SkillClass, bool> func_7;

		public static Func<IEffect, bool> func_8;

		public static Func<float, bool> func_9;

		public static Func<float, bool> func_10;

		public static Func<IEffect, bool> func_11;

		public static Func<IEffect, bool> func_12;

		public static Func<int, float> func_13;

		public static Func<SkillClass, bool> func_14;

		public static Func<bool, bool> func_15;

		public static Func<GClass2251, bool> func_16;

		public static Func<int, float> func_17;

		public float method_0(int _)
		{
			return 1f;
		}

		public bool method_1(SkillClass skill)
		{
			return skill.Id == ESkillId.Lockpicking;
		}

		public bool method_2(SkillClass skill)
		{
			return skill.Id == ESkillId.Crafting;
		}

		public bool method_3(bool x)
		{
			return x;
		}

		public bool method_4(bool x)
		{
			return !x;
		}

		public bool method_5(SkillClass skill)
		{
			return skill.Id == ESkillId.Intellect;
		}

		public bool method_6(SkillClass skill)
		{
			return skill.Id == ESkillId.Attention;
		}

		public bool method_7(SkillClass skill)
		{
			return skill.Id == ESkillId.Perception;
		}

		public bool method_8(IEffect effect)
		{
			return effect is GInterface341;
		}

		public bool method_9(float x)
		{
			return x > 0f;
		}

		public bool method_10(float x)
		{
			return x > 0f;
		}

		public bool method_11(IEffect effect)
		{
			return effect is GInterface357;
		}

		public bool method_12(IEffect effect)
		{
			return effect is GInterface346;
		}

		public float method_13(int i)
		{
			return 1f + (float)i * 0.01f;
		}

		public bool method_14(SkillClass x)
		{
			if (x.Id != ESkillId.FieldMedicine)
			{
				return x.Id == ESkillId.FirstAid;
			}
			return true;
		}

		public bool method_15(bool x)
		{
			return x;
		}

		public bool method_16(GClass2251 p)
		{
			if (p.Distance >= 70f && p.HoldBreath)
			{
				return p.BodyPart == EBodyPart.Head;
			}
			return false;
		}

		public float method_17(int i)
		{
			return Mathf.Clamp((float)i / 10f, 0f, 2f);
		}
	}

	[CompilerGenerated]
	public class Class1435
	{
		public SkillsDescriptorClass.GClass2226 mastering;

		public bool method_0(BackendConfigSettingsClass.GClass1734 x)
		{
			return x.Id == mastering.Id;
		}
	}

	[CompilerGenerated]
	public class Class1436
	{
		public SkillClass skill;

		public SkillManager skillManager_0;

		public void method_0()
		{
			skillManager_0.OnSkillLevelChanged?.Invoke(skill);
		}

		public void method_1()
		{
			skillManager_0.OnSkillExperienceChanged?.Invoke(skill);
		}
	}

	[CompilerGenerated]
	public class Class1437
	{
		public string weaponTemplateId;

		public bool method_0(BackendConfigSettingsClass.GClass1734 x)
		{
			return x.Id == weaponTemplateId;
		}
	}

	[CompilerGenerated]
	public class Class1438
	{
		public BackendConfigSettingsClass.GClass1734 group;

		public SkillManager skillManager_0;

		public MasterSkillClass newMastering;

		public bool method_0(Item x)
		{
			if (x != null)
			{
				return MasteringGroup(x.TemplateId) == group;
			}
			return false;
		}

		public void method_1()
		{
			skillManager_0.WeaponMastered?.Invoke(newMastering);
		}

		public void method_2()
		{
			skillManager_0.OnMasteringExperienceChanged?.Invoke(newMastering);
		}
	}

	[CompilerGenerated]
	public class Class1439
	{
		public string templateId;

		public bool method_0(KeyValuePair<string, MasterSkillClass> x)
		{
			return x.Value.MasteringGroup.Templates.Contains(templateId);
		}
	}

	[CompilerGenerated]
	public class Class1440
	{
		public SkillClass newSkillInfo;

		public bool method_0(SkillClass x)
		{
			return x.Id == newSkillInfo.Id;
		}
	}

	[CompilerGenerated]
	public class Class1441
	{
		public SkillManager skillManager_0;

		public ESkillId[] skillsRelatedToHealth;

		public float method_0(GStruct279 movement)
		{
			if (!(movement.Overweight > 0f))
			{
				return skillManager_0.Settings.Endurance.SprintAction * (1f + skillManager_0.Settings.Endurance.GainPerFatigueStack * movement.Fatigue);
			}
			return 0f;
		}

		public float method_1(GStruct279 movement)
		{
			if (!(movement.Overweight > 0f))
			{
				return skillManager_0.Settings.Endurance.MovementAction * (1f + skillManager_0.Settings.Endurance.GainPerFatigueStack * movement.Fatigue);
			}
			return 0f;
		}

		public float method_2(GStruct279 movement)
		{
			if (!(movement.Overweight > 0f))
			{
				return 0f;
			}
			return Mathf.Lerp(skillManager_0.Settings.Strength.SprintActionMin, skillManager_0.Settings.Strength.SprintActionMax, movement.Overweight);
		}

		public float method_3(GStruct279 movement)
		{
			if (!(movement.Overweight > 0f))
			{
				return 0f;
			}
			return Mathf.Lerp(skillManager_0.Settings.Strength.MovementActionMin, skillManager_0.Settings.Strength.MovementActionMax, movement.Overweight);
		}

		public float method_4(GStruct279 movement)
		{
			if (!(movement.Overweight > 0f))
			{
				return 0f;
			}
			return Mathf.Lerp(skillManager_0.Settings.Strength.PushUpMin, skillManager_0.Settings.Strength.PushUpMax, movement.Overweight);
		}

		public bool method_5(SkillClass skill)
		{
			return Array.IndexOf(skillsRelatedToHealth, skill.Id) >= 0;
		}
	}

	[CompilerGenerated]
	public class Class1442
	{
		public BackendConfigSettingsClass.GClass1754.GClass1755 pointsRate;

		public SkillManager skillManager_0;

		public float method_0((float, bool) tuple)
		{
			if (!tuple.Item2)
			{
				return pointsRate.PointsPerResource;
			}
			return pointsRate.PointsPerResource + skillManager_0.Settings.HideoutManagement.SkillPointsRate[EAreaType.SolarPower].PointsPerResource;
		}
	}

	public readonly Dictionary<Type, Dictionary<EBuffId, SkillBuffClass>> WeaponBuffs = new Dictionary<Type, Dictionary<EBuffId, SkillBuffClass>>();

	public readonly Dictionary<Type, Dictionary<EBuffId, GClass2257>> WeaponBoolenBuffs = new Dictionary<Type, Dictionary<EBuffId, GClass2257>>();

	public readonly SkillBuffClass EnduranceBuffEnduranceInc = new SkillBuffClass
	{
		Id = EBuffId.EnduranceBuffEnduranceInc,
		BuffType = EBuffType.Switching
	};

	public readonly SkillBuffClass EnduranceHands = new SkillBuffClass
	{
		Id = EBuffId.EnduranceHands,
		BuffType = EBuffType.Elite
	};

	public readonly SkillBuffClass EnduranceBuffJumpCostRed = new SkillBuffClass
	{
		Id = EBuffId.EnduranceBuffJumpCostRed
	};

	public readonly SkillBuffClass EnduranceBuffBreathTimeInc = new SkillBuffClass
	{
		Id = EBuffId.EnduranceBuffBreathTimeInc
	};

	public readonly SkillBuffClass EnduranceBuffRestoration = new SkillBuffClass
	{
		Id = EBuffId.EnduranceBuffRestorationTimeRed,
		BuffType = EBuffType.Plebian
	};

	public readonly SkillBuffClass EnduranceBreathElite = new SkillBuffClass
	{
		Id = EBuffId.EnduranceBreathElite,
		BuffType = EBuffType.Elite
	};

	public readonly SkillBuffClass StrengthBuffLiftWeightInc = new SkillBuffClass
	{
		Id = EBuffId.StrengthBuffLiftWeightInc
	};

	public readonly SkillBuffClass StrengthBuffSprintSpeedInc = new SkillBuffClass
	{
		Id = EBuffId.StrengthBuffSprintSpeedInc
	};

	public readonly SkillBuffClass StrengthBuffJumpHeightInc = new SkillBuffClass
	{
		Id = EBuffId.StrengthBuffJumpHeightInc
	};

	public readonly SkillBuffClass StrengthBuffAimFatigue = new SkillBuffClass
	{
		Id = EBuffId.StrengthBuffAim
	};

	public readonly SkillBuffClass StrengthBuffThrowDistanceInc = new SkillBuffClass
	{
		Id = EBuffId.StrengthBuffThrowDistanceInc
	};

	public readonly SkillBuffClass StrengthBuffMeleePowerInc = new SkillBuffClass
	{
		Id = EBuffId.StrengthBuffMeleePowerInc
	};

	public readonly GClass2257 StrengthBuffElite = new GClass2257
	{
		Id = EBuffId.StrengthBuffElite,
		BuffType = EBuffType.Elite
	};

	public readonly SkillBuffClass StrengthBuffMeleeCrits = new SkillBuffClass
	{
		Id = EBuffId.StrengthBuffMeleeCrits,
		BuffType = EBuffType.Elite
	};

	public readonly SkillBuffClass VitalityBuffBleedChanceRed = new SkillBuffClass
	{
		Id = EBuffId.VitalityBuffBleedChanceRed
	};

	public readonly SkillBuffClass VitalityBuffSurviobilityInc = new SkillBuffClass
	{
		Id = EBuffId.VitalityBuffSurviobilityInc
	};

	public readonly GClass2257 VitalityBuffRegeneration = new GClass2257
	{
		Id = EBuffId.VitalityBuffRegeneration,
		BuffType = EBuffType.Elite
	};

	public readonly GClass2257 VitalityBuffBleedStop = new GClass2257
	{
		Id = EBuffId.VitalityBuffBleedStop,
		BuffType = EBuffType.Elite
	};

	public readonly SkillBuffClass HealthBreakChanceRed = new SkillBuffClass
	{
		Id = EBuffId.HealthBreakChanceRed
	};

	public readonly SkillBuffClass HealthOfflineRegenerationInc = new SkillBuffClass
	{
		Id = EBuffId.HealthOfflineRegenerationInc
	};

	public readonly SkillBuffClass HealthEnergy = new SkillBuffClass
	{
		Id = EBuffId.HealthEnergy
	};

	public readonly SkillBuffClass HealthHydration = new SkillBuffClass
	{
		Id = EBuffId.HealthHydration
	};

	public readonly GClass2257 HealthEliteAbsorbDamage = new GClass2257
	{
		Id = EBuffId.HealthEliteAbsorbDamage,
		BuffType = EBuffType.Elite
	};

	public readonly SkillBuffClass HealthElitePosion = new SkillBuffClass
	{
		Id = EBuffId.HealthElitePosion,
		BuffType = EBuffType.Elite
	};

	public readonly SkillBuffClass StressPain = new SkillBuffClass
	{
		Id = EBuffId.StressPainChance
	};

	public readonly SkillBuffClass StressTremor = new SkillBuffClass
	{
		Id = EBuffId.StressTremor
	};

	public readonly GClass2257 StressBerserk = new GClass2257
	{
		Id = EBuffId.StressBerserk,
		BuffType = EBuffType.Elite
	};

	public readonly GClass2255 MetabolismRatioPlus = new GClass2255
	{
		Id = EBuffId.MetabolismRatioPlus
	};

	public readonly SkillBuffClass MetabolismPoisonTime = new SkillBuffClass
	{
		Id = EBuffId.MetabolismPoisonTime
	};

	public readonly GClass2256 MetabolismMiscDebuffTime = new GClass2256
	{
		Id = EBuffId.MetabolismMiscDebuffTime
	};

	public readonly GClass2257 MetabolismEliteBuffNoDyhydration = new GClass2257
	{
		Id = EBuffId.MetabolismEliteBuffNoDyhydration,
		BuffType = EBuffType.Elite
	};

	public readonly SkillBuffClass PerceptionHearing = new SkillBuffClass
	{
		Id = EBuffId.PerceptionHearing
	};

	public readonly SkillBuffClass PerceptionLootDot = new SkillBuffClass
	{
		Id = EBuffId.PerceptionLootDot
	};

	public readonly GClass2257 PerceptionEliteNoIdea = new GClass2257
	{
		Id = EBuffId.PerceptionmEliteNoIdea,
		BuffType = EBuffType.Elite
	};

	public readonly SkillBuffClass IntellectLearningSpeed = new SkillBuffClass
	{
		Id = EBuffId.IntellectLearningSpeed
	};

	public readonly SkillBuffClass IntellectWeaponMaintance = new SkillBuffClass
	{
		Id = EBuffId.IntellectWeaponMaintance
	};

	public readonly GClass2257 IntellectEliteNaturalLearner = new GClass2257
	{
		Id = EBuffId.IntellectEliteNaturalLearner,
		BuffType = EBuffType.Elite
	};

	public readonly GClass2257 IntellectEliteAmmoCounter = new GClass2257
	{
		Id = EBuffId.IntellectEliteAmmoCounter,
		BuffType = EBuffType.Elite
	};

	public readonly GClass2257 IntellectEliteContainerScope = new GClass2257
	{
		Id = EBuffId.IntellectEliteContainerScope,
		BuffType = EBuffType.Elite
	};

	public readonly SkillBuffClass IntellectRepairPointsCostReduction = new SkillBuffClass
	{
		Id = EBuffId.IntellectRepairPointsCostReduction
	};

	public readonly SkillBuffClass AttentionLootSpeed = new SkillBuffClass
	{
		Id = EBuffId.AttentionLootSpeed
	};

	public readonly SkillBuffClass AttentionExamine = new SkillBuffClass
	{
		Id = EBuffId.AttentionRareLoot
	};

	public readonly SkillBuffClass AttentionEliteLuckySearch = new SkillBuffClass
	{
		Id = EBuffId.AttentionEliteLuckySearch,
		BuffType = EBuffType.Elite
	};

	public readonly GClass2257 AttentionEliteExtraLootExp = new GClass2257
	{
		Id = EBuffId.AttentionEliteExtraLootExp,
		BuffType = EBuffType.Elite
	};

	public readonly SkillBuffClass MagDrillsLoadSpeed = new SkillBuffClass
	{
		Id = EBuffId.MagDrillsLoadSpeed
	};

	public readonly SkillBuffClass MagDrillsUnloadSpeed = new SkillBuffClass
	{
		Id = EBuffId.MagDrillsUnloadSpeed
	};

	public readonly SkillBuffClass MagDrillsInventoryCheckSpeed = new SkillBuffClass
	{
		Id = EBuffId.MagDrillsInventoryCheckSpeed
	};

	public readonly SkillBuffClass MagDrillsInventoryCheckAccuracy = new SkillBuffClass
	{
		Id = EBuffId.MagDrillsInventoryCheckAccuracy
	};

	public readonly GClass2257 MagDrillsInstantCheck = new GClass2257
	{
		Id = EBuffId.MagDrillsInstantCheck,
		BuffType = EBuffType.Elite
	};

	public readonly GClass2257 MagDrillsLoadProgression = new GClass2257
	{
		Id = EBuffId.MagDrillsLoadProgression,
		BuffType = EBuffType.Elite
	};

	public readonly SkillBuffClass CharismaBuff1 = new SkillBuffClass
	{
		Id = EBuffId.CharismaBuff1
	};

	public readonly SkillBuffClass CharismaBuff2 = new SkillBuffClass
	{
		Id = EBuffId.CharismaBuff2
	};

	public readonly SkillBuffClass CharismaDailyQuestsRerollDiscount = new SkillBuffClass
	{
		Id = EBuffId.CharismaDailyQuestsRerollDiscount
	};

	public readonly SkillBuffClass CharismaHealingDiscount = new SkillBuffClass
	{
		Id = EBuffId.CharismaHealingDiscount
	};

	public readonly SkillBuffClass CharismaInsuranceDiscount = new SkillBuffClass
	{
		Id = EBuffId.CharismaInsuranceDiscount
	};

	public readonly SkillBuffClass CharismaExfiltrationDiscount = new SkillBuffClass
	{
		Id = EBuffId.CharismaExfiltrationDiscount
	};

	public readonly SkillBuffClass CharismaEliteScavCaseDiscount = new SkillBuffClass
	{
		Id = EBuffId.CharismaScavCaseDiscount,
		BuffType = EBuffType.Elite
	};

	public readonly SkillBuffClass CharismaEliteFenceRepPenaltyReduction = new SkillBuffClass
	{
		Id = EBuffId.CharismaFenceRepPenaltyReduction,
		BuffType = EBuffType.Elite
	};

	public readonly SkillBuffClass CharismaEliteAdditionalDailyQuests = new SkillBuffClass
	{
		Id = EBuffId.CharismaAdditionalDailyQuests,
		BuffType = EBuffType.Elite
	};

	public readonly GClass2257 CharismaEliteBuff1 = new GClass2257
	{
		Id = EBuffId.CharismaEliteBuff1,
		BuffType = EBuffType.Elite
	};

	public readonly SkillBuffClass CharismaEliteBuff2 = new SkillBuffClass
	{
		Id = EBuffId.CharismaEliteBuff2,
		BuffType = EBuffType.Elite
	};

	public readonly GClass2256 ImmunityMiscEffects = new GClass2256
	{
		Id = EBuffId.ImmunityMiscEffects
	};

	public readonly GClass2256 ImmunityPoisonBuff = new GClass2256
	{
		Id = EBuffId.ImmunityPoisonBuff
	};

	public readonly GClass2255 ImmunityPainKiller = new GClass2255
	{
		Id = EBuffId.ImmunityPainKiller
	};

	public readonly GClass2254 ImmunityAvoidPoisonChance = new GClass2254
	{
		Id = EBuffId.ImmunityPoisonChance,
		BuffType = EBuffType.Elite
	};

	public readonly GClass2254 ImmunityAvoidMiscEffectsChance = new GClass2254
	{
		Id = EBuffId.ImmunityMiscEffectsChance,
		BuffType = EBuffType.Elite
	};

	public WeaponSkillClass Pistol;

	public WeaponSkillClass Revolver;

	public WeaponSkillClass SMG;

	public WeaponSkillClass Assault;

	public WeaponSkillClass Shotgun;

	public WeaponSkillClass Sniper;

	public WeaponSkillClass LMG;

	public WeaponSkillClass HMG;

	public WeaponSkillClass Launcher;

	public WeaponSkillClass AttachedLauncher;

	public WeaponSkillClass Misc;

	public WeaponSkillClass Melee;

	public WeaponSkillClass DMR;

	public SkillClass Throwing;

	public SkillClass AimDrills;

	public SkillClass RecoilControl;

	public SkillClass TroubleShooting;

	public SkillBuffClass AimMasterSpeed = new SkillBuffClass
	{
		Id = EBuffId.AimMasterSpeed
	};

	public SkillBuffClass AimMasterWiggle = new SkillBuffClass
	{
		Id = EBuffId.AimMasterWiggle
	};

	public GClass2257 AimMasterElite = new GClass2257
	{
		BuffType = EBuffType.Elite,
		Id = EBuffId.AimMasterElite
	};

	public SkillBuffClass RecoilControlImprove = new SkillBuffClass
	{
		Id = EBuffId.RecoilControlImprove
	};

	public SkillBuffClass RecoilControlElite = new SkillBuffClass
	{
		Id = EBuffId.RecoilControlElite,
		BuffType = EBuffType.Elite
	};

	public SkillBuffClass TroubleFixing = new SkillBuffClass
	{
		Id = EBuffId.TroubleFixing
	};

	public GClass2257 TroubleFixingDurElite = new GClass2257
	{
		Id = EBuffId.TroubleFixingDurElite,
		BuffType = EBuffType.Elite
	};

	public GClass2257 TroubleFixingMagElite = new GClass2257
	{
		Id = EBuffId.TroubleFixingMagElite,
		BuffType = EBuffType.Elite
	};

	public GClass2257 TroubleFixingAmmoElite = new GClass2257
	{
		Id = EBuffId.TroubleFixingAmmoElite,
		BuffType = EBuffType.Elite
	};

	public GClass2257 TroubleFixingExamineMalfElite = new GClass2257
	{
		Id = EBuffId.TroubleFixingExamineMalfElite,
		BuffType = EBuffType.Elite
	};

	public SkillBuffClass ThrowingStrengthBuff = new SkillBuffClass
	{
		Id = EBuffId.ThrowingStrengthBuff
	};

	public SkillBuffClass ThrowingEnergyExpenses = new SkillBuffClass
	{
		Id = EBuffId.ThrowingEnergyExpenses
	};

	public GClass2257 ThrowingEliteBuff = new GClass2257
	{
		BuffType = EBuffType.Elite,
		Id = EBuffId.ThrowingWeaponsBuffElite
	};

	public SkillBuffClass DrawSpeed = new SkillBuffClass
	{
		Id = EBuffId.DrawSpeed
	};

	public SkillBuffClass DrawSound = new SkillBuffClass
	{
		Id = EBuffId.DrawSound
	};

	public GClass2257 DrawElite = new GClass2257
	{
		Id = EBuffId.DrawElite,
		BuffType = EBuffType.Elite
	};

	public GClass2257 DrawTremor = new GClass2257
	{
		Id = EBuffId.DrawTremor,
		BuffType = EBuffType.Elite
	};

	public const float NO_EXPENSE_AIM_TIMING = 5f;

	public const float FAST_AIM_TIMING = 2f;

	public readonly SkillClass[] Skills;

	public readonly SkillClass[] DisplayList;

	[NonSerialized]
	public List<Action> Unsubscribers = new List<Action>();

	public readonly Dictionary<string, MasterSkillClass> Mastering = new Dictionary<string, MasterSkillClass>();

	public readonly Dictionary<Type, WeaponSkillClass> WeaponSkills = new Dictionary<Type, WeaponSkillClass>();

	public Player.BetterValueBlender NoExpenseAimSkillTimer = new Player.BetterValueBlender
	{
		Speed = 1f
	};

	public readonly Player.BetterValueBlender FastAimTimer = new Player.BetterValueBlender
	{
		Speed = 1f
	};

	public readonly GClass2261<GStruct279> SprintAction = new GClass2261<GStruct279>();

	public readonly GClass2261<GStruct279> PushUp = new GClass2261<GStruct279>();

	public readonly SkillActionClass ProneAction = new SkillActionClass();

	public readonly GClass2261<GStruct279> MovementAction = new GClass2261<GStruct279>();

	public readonly GClass2260<float> JumpAction = new GClass2260<float>();

	public readonly GClass2260<Item> WeaponReloadAction = new GClass2260<Item>();

	public readonly GClass2260<Item> WeaponShotAction = new GClass2260<Item>();

	public readonly GClass2260<Item> WeaponChamberAction = new GClass2260<Item>();

	public readonly SkillActionClass WeaponFixAction = new SkillActionClass();

	public readonly SkillActionClass WeaponDrawAction = new SkillActionClass();

	public readonly SkillActionClass WeaponAimAction = new SkillActionClass();

	public readonly GClass2260<float> ShotAfterAim = new GClass2260<float>();

	public readonly SkillActionClass RecoilAction = new SkillActionClass();

	public readonly SkillActionClass FistfightAction = new SkillActionClass();

	public readonly SkillActionClass ThrowAction = new SkillActionClass();

	public readonly SkillActionClass HoldBreathAction = new SkillActionClass();

	public readonly SkillActionClass DamageTakenAction = new SkillActionClass();

	public readonly GClass2260<IEffect> HealthNegativeEffect = new GClass2260<IEffect>();

	public readonly SkillActionClass LowHPDuration = new SkillActionClass();

	public readonly SkillActionClass OnlineAction = new SkillActionClass();

	public readonly SkillActionClass UniqueLoot = new SkillActionClass();

	public readonly SkillActionClass ExamineAction = new SkillActionClass();

	public readonly SkillActionClass ExamineWithInstruction = new SkillActionClass();

	public readonly GClass2260<bool> LockpickAction = new GClass2260<bool>();

	public readonly SkillActionClass RepairAction = new SkillActionClass();

	public readonly GClass2260<bool> FindAction = new GClass2260<bool>();

	public readonly GClass2260<int> TradeAction = new GClass2260<int>();

	public readonly SkillActionClass SearchAction = new SkillActionClass();

	public readonly SkillActionClass RaidLoadedAmmoAction = new SkillActionClass();

	public readonly SkillActionClass RaidUnloadedAmmoAction = new SkillActionClass();

	public readonly SkillActionClass MagazineCheckAction = new SkillActionClass();

	public readonly GClass2260<SkillClass> AnySkillUp = new GClass2260<SkillClass>();

	public readonly GClass2260<SkillClass> SkillProgress = new GClass2260<SkillClass>();

	public readonly GClass2260<float> HydrationChanged = new GClass2260<float>();

	public readonly GClass2260<float> EnergyChanged = new GClass2260<float>();

	public readonly GClass2260<GClass2251> KillAction = new GClass2260<GClass2251>();

	public readonly SkillActionClass SurgeryAction = new SkillActionClass();

	public readonly GClass2261<float> HideoutCraftTimerAction = new GClass2261<float>();

	public readonly SkillActionClass HideoutCompleteCraftAction = new SkillActionClass();

	public readonly SkillActionClass HideoutZoneUpgradeAction = new GClass2260<float>();

	public readonly SkillActionClass UniqueCrafting = new SkillActionClass();

	public readonly SkillActionClass StimulatorNegativeBuff = new SkillActionClass();

	public readonly SkillActionClass Exhaustion = new SkillActionClass();

	public readonly SkillActionClass Dehydration = new SkillActionClass();

	public readonly SkillActionClass WeaponRepairedAction = new SkillActionClass();

	public readonly SkillActionClass LightArmorDamageTakenAction = new SkillActionClass();

	public readonly SkillActionClass HeavyArmorDamageTakenAction = new SkillActionClass();

	public readonly Dictionary<EAreaType, GClass2261<(float, bool)>> ConsumptionActions = new Dictionary<EAreaType, GClass2261<(float, bool)>>();

	public SkillClass Perception;

	public SkillClass Intellect;

	public SkillClass Attention;

	public SkillClass Immunity;

	public SkillClass Charisma;

	public SkillClass Memory;

	public SkillClass Endurance;

	public SkillClass Strength;

	public SkillClass Vitality;

	public SkillClass Health;

	public SkillClass Metabolism;

	public SkillClass StressResistance;

	public SkillClass Sniping;

	public SkillClass CovertMovement;

	public SkillClass ProneMovement;

	public SkillClass FirstAid;

	public SkillClass FieldMedicine;

	public SkillClass LightVests;

	public SkillClass HeavyVests;

	public SkillClass WeaponModding;

	public SkillClass AdvancedModding;

	public SkillClass NightOps;

	public SkillClass SilentOps;

	public SkillClass Lockpicking;

	public SkillClass Surgery;

	public SkillClass Search;

	public SkillClass WeaponTreatment;

	public SkillClass MagDrills;

	public SkillClass Freetrading;

	public SkillClass Auctions;

	public SkillClass Cleanoperations;

	public SkillClass Barter;

	public SkillClass Shadowconnections;

	public SkillClass Taskperformance;

	public SkillClass Crafting;

	public SkillClass HideoutManagement;

	public readonly SkillBuffClass CovertMovementSoundVolume = new SkillBuffClass
	{
		Id = EBuffId.CovertMovementSoundVolume,
		BuffType = EBuffType.Simple
	};

	public readonly SkillBuffClass CovertMovementEquipment = new SkillBuffClass
	{
		Id = EBuffId.CovertMovementEquipment,
		BuffType = EBuffType.Simple
	};

	public readonly SkillBuffClass CovertMovementSpeed = new SkillBuffClass
	{
		Id = EBuffId.CovertMovementSpeed,
		BuffType = EBuffType.Simple
	};

	public readonly GClass2257 CovertMovementElite = new GClass2257
	{
		Id = EBuffId.CovertMovementElite,
		BuffType = EBuffType.Elite
	};

	public readonly SkillBuffClass CovertMovementLoud = new SkillBuffClass
	{
		Id = EBuffId.CovertMovementLoud,
		BuffType = EBuffType.Plebian
	};

	public readonly SkillBuffClass ProneMovementSpeed = new SkillBuffClass
	{
		Id = EBuffId.ProneMovementSpeed
	};

	public readonly SkillBuffClass ProneMovementVolume = new SkillBuffClass
	{
		BuffType = EBuffType.Switching,
		Id = EBuffId.ProneMovementVolume
	};

	public readonly GClass2257 ProneMovementEliteSprint = new GClass2257
	{
		BuffType = EBuffType.Elite,
		Id = EBuffId.ProneMovementElite
	};

	public readonly SkillBuffClass SearchBuffSpeed = new SkillBuffClass
	{
		BuffType = EBuffType.Plebian,
		Id = EBuffId.SearchSpeed
	};

	public readonly GClass2257 SearchDouble = new GClass2257
	{
		BuffType = EBuffType.Elite,
		Id = EBuffId.SearchDouble
	};

	public readonly SkillBuffClass SurgeryReducePenalty = new SkillBuffClass
	{
		BuffType = EBuffType.Switching,
		Id = EBuffId.SurgeryReducePenalty
	};

	public readonly SkillBuffClass SurgerySpeed = new SkillBuffClass
	{
		BuffType = EBuffType.Switching,
		Id = EBuffId.SurgerySpeed
	};

	public readonly SkillBuffClass HideoutResourceConsumption = new SkillBuffClass
	{
		BuffType = EBuffType.Simple,
		Id = EBuffId.HideoutResourceConsumption
	};

	public readonly SkillBuffClass ZoneBonusBoost = new SkillBuffClass
	{
		BuffType = EBuffType.Simple,
		Id = EBuffId.HideoutZoneBonusBoost
	};

	public readonly GClass2257 HideoutExtraSlots = new GClass2257
	{
		BuffType = EBuffType.Elite,
		Id = EBuffId.HideoutExtraSlots
	};

	public readonly SkillBuffClass SingleCraftTimeReduce = new SkillBuffClass
	{
		BuffType = EBuffType.Simple,
		Id = EBuffId.CraftingSingleTimeReduce
	};

	public readonly SkillBuffClass ContinueCraftTimeReduce = new SkillBuffClass
	{
		BuffType = EBuffType.Simple,
		Id = EBuffId.CraftingContinueTimeReduce
	};

	public readonly GClass2257 EliteCrafting = new GClass2257
	{
		BuffType = EBuffType.Elite,
		Id = EBuffId.CraftingElite
	};

	public readonly SkillBuffClass WeaponDurabilityLosOnShotReduce = new SkillBuffClass
	{
		BuffType = EBuffType.Simple,
		Id = EBuffId.WeaponDurabilityLossOnShotReduce
	};

	public readonly SkillBuffClass WearAmountRepairGunsReducePerLevel = new SkillBuffClass
	{
		BuffType = EBuffType.Simple,
		Id = EBuffId.WearAmountRepairGunsReducePerLevel
	};

	public readonly GClass2257 WearChanceRepairGunsReduceEliteLevel = new GClass2257
	{
		BuffType = EBuffType.Elite,
		Id = EBuffId.WearChanceRepairGunsReduceEliteLevel
	};

	public readonly SkillBuffClass LightVestMoveSpeedPenaltyReduction = new SkillBuffClass
	{
		BuffType = EBuffType.Simple,
		Id = EBuffId.LightVestMoveSpeedPenaltyReduction
	};

	public readonly SkillBuffClass LightVestMeleeWeaponDamageReduction = new SkillBuffClass
	{
		BuffType = EBuffType.Simple,
		Id = EBuffId.LightVestMeleeWeaponDamageReduction
	};

	public readonly SkillBuffClass LightVestRepairDegradationReduction = new SkillBuffClass
	{
		BuffType = EBuffType.Simple,
		Id = EBuffId.LightVestRepairDegradationReduction
	};

	public readonly GClass2257 LightVestBleedingProtection = new GClass2257
	{
		BuffType = EBuffType.Elite,
		Id = EBuffId.LightVestBleedingProtection
	};

	public readonly GClass2257 LightVestDeteriorationChanceOnRepairReduce = new GClass2257
	{
		BuffType = EBuffType.Elite,
		Id = EBuffId.LightVestDeteriorationChanceOnRepairReduce
	};

	public readonly SkillBuffClass HeavyVestMoveSpeedPenaltyReduction = new SkillBuffClass
	{
		BuffType = EBuffType.Simple,
		Id = EBuffId.HeavyVestMoveSpeedPenaltyReduction
	};

	public readonly SkillBuffClass HeavyVestBluntThroughputDamageReduction = new SkillBuffClass
	{
		BuffType = EBuffType.Simple,
		Id = EBuffId.HeavyVestBluntThroughputDamageReduction
	};

	public readonly SkillBuffClass HeavyVestRepairDegradationReduction = new SkillBuffClass
	{
		BuffType = EBuffType.Simple,
		Id = EBuffId.HeavyVestRepairDegradationReduction
	};

	public readonly GClass2257 HeavyVestNoBodyDamageDeflectChance = new GClass2257
	{
		BuffType = EBuffType.Elite,
		Id = EBuffId.HeavyVestNoBodyDamageDeflectChance
	};

	public readonly GClass2257 HeavyVestDeteriorationChanceOnRepairReduce = new GClass2257
	{
		BuffType = EBuffType.Elite,
		Id = EBuffId.HeavyVestDeteriorationChanceOnRepairReduce
	};

	public SkillClass BearAssaultoperations;

	public SkillClass BearAuthority;

	public SkillClass BearAksystems;

	public SkillClass BearHeavycaliber;

	public SkillClass BearRawpower;

	public SkillClass UsecArsystems;

	public SkillClass UsecDeepweaponmodding;

	public SkillClass UsecLongrangeoptics;

	public SkillClass UsecNegotiations;

	public SkillClass UsecTactics;

	public SkillClass BotReload;

	public SkillClass BotSoundGoef;

	public SkillBuffClass BotReloadSpeed = new SkillBuffClass();

	public SkillBuffClass BotSoundCoef = new SkillBuffClass();

	public float CarryingWeightRelativeModifier => 1f + (float)StrengthBuffLiftWeightInc;

	[field: NonSerialized]
	public BonusController BonusController { get; set; }

	public BackendConfigSettingsClass.GlobalSkillsSettings Settings => Singleton<BackendConfigSettingsClass>.Instance.SkillsSettings;

	public bool IsSearchDouble => SearchDouble.Value;

	public float AttentionExamineValue => AttentionExamine.Value;

	public float AttentionEliteLuckySearchValue => AttentionEliteLuckySearch.Value;

	public float AttentionLootSpeedValue => AttentionLootSpeed.Value;

	public float SearchBuffSpeedValue => SearchBuffSpeed.Value;

	public bool IsTroubleFixingExamineMalfElite => TroubleFixingExamineMalfElite.Value;

	public bool IsMagDrillsInstantCheck => MagDrillsInstantCheck.Value;

	public int MagDrillsLevel => MagDrills.Level;

	public event Action<AbstractSkillClass> OnSkillLevelChanged;

	public event Action<MasterSkillClass> WeaponMastered;

	public event Action<AbstractSkillClass> OnSkillExperienceChanged;

	public event Action<MasterSkillClass> OnMasteringExperienceChanged;

	public event Action<MasterSkillClass> NewWeaponMastered;

	public GClass2250 GetWeaponInfo(Item weapon)
	{
		Type type = weapon.GetType();
		if (typeof(GClass3308).IsAssignableFrom(type))
		{
			type = typeof(GClass3308);
		}
		float num = 0f;
		float num2 = 0f;
		float x = 0f;
		float deltaErgonomics = 0f;
		float value = RecoilControlImprove.Value;
		bool maxed = false;
		float doubleActionRecoilReduce = 0f;
		bool flag = false;
		float mountingBonusErgo = 0f;
		float bipodBonusErgo = 0f;
		if (WeaponBuffs.ContainsKey(type))
		{
			Dictionary<EBuffId, SkillBuffClass> dictionary = WeaponBuffs[type];
			num2 = dictionary[EBuffId.WeaponReloadBuff].Value + (float)BotReloadSpeed;
			num = dictionary[EBuffId.WeaponSwapBuff].Value;
			x = dictionary[EBuffId.WeaponRecoilBuff].Value;
			deltaErgonomics = dictionary[EBuffId.WeaponErgonomicsBuff].Value;
			maxed = dictionary[EBuffId.WeaponDoubleMastering].Value > 1f;
			doubleActionRecoilReduce = dictionary[EBuffId.WeaponDoubleActionRecoilReduceBuff].Value;
			mountingBonusErgo = dictionary[EBuffId.MountingErgonomicsGainPerLevel].Value;
			bipodBonusErgo = dictionary[EBuffId.BipodErgonomicsGainPerLevel].Value;
		}
		else
		{
			Debug.Log(type?.ToString() + "<color=yellow> has no buffs</color>");
		}
		if (WeaponBoolenBuffs.ContainsKey(type))
		{
			flag = WeaponBoolenBuffs[type][EBuffId.DrawElite];
		}
		int num3 = GetMastering(weapon.TemplateId)?.Level ?? 0;
		return new GClass2250
		{
			DeltaErgonomics = deltaErgonomics,
			SwapSpeed = 1f + num,
			ReloadSpeed = 1f + num2,
			RecoilSupression = new Vector2(x, value),
			FixSpeed = 1f + TroubleFixing.Value,
			AimMovementSpeed = (float)num3 * 0.05f,
			Maxed = maxed,
			AimSpeed = DrawSpeed,
			StiffDraw = ((bool)DrawElite || flag),
			DoubleActionRecoilReduce = doubleActionRecoilReduce,
			MountingBonusErgo = mountingBonusErgo,
			BipodBonusErgo = bipodBonusErgo
		};
	}

	public SkillBuffAbstractClass[] GetWeaponBuffs(Type ofType)
	{
		SkillBuffClass skillBuffClass = new SkillBuffClass
		{
			Id = EBuffId.WeaponReloadBuff
		}.PerLevel(0.004f);
		SkillBuffClass skillBuffClass2 = new SkillBuffClass
		{
			Id = EBuffId.WeaponSwapBuff
		}.PerLevel(0.008f);
		SkillBuffClass skillBuffClass3 = new SkillBuffClass
		{
			Id = EBuffId.WeaponRecoilBuff
		}.PerLevel(Settings.WeaponSkillRecoilBonusPerLevel);
		SkillBuffClass skillBuffClass4 = new SkillBuffClass
		{
			Id = EBuffId.WeaponErgonomicsBuff
		}.PerLevel(0.002f);
		SkillBuffClass skillBuffClass5 = new SkillBuffClass
		{
			Id = EBuffId.WeaponDoubleActionRecoilReduceBuff,
			HidenForPlayers = true
		}.PerLevel(0.01f);
		SkillBuffClass skillBuffClass6 = new SkillBuffClass
		{
			Id = EBuffId.WeaponDoubleMastering,
			BuffType = EBuffType.Elite
		}.Custom((int _) => 1f).Elite(2f).Default(1f);
		GClass2257 gClass = new GClass2257
		{
			Id = EBuffId.DrawElite,
			BuffType = EBuffType.Elite
		};
		SkillBuffClass skillBuffClass7 = new SkillBuffClass
		{
			Id = EBuffId.MountingErgonomicsGainPerLevel,
			HidenForPlayers = true
		}.PerLevel(Settings.MountingErgonomicsBonusPerLevel);
		SkillBuffClass skillBuffClass8 = new SkillBuffClass
		{
			Id = EBuffId.BipodErgonomicsGainPerLevel,
			HidenForPlayers = true
		}.PerLevel(Settings.BipodErgonomicsBonusPerLevel);
		WeaponBoolenBuffs.Add(ofType, new Dictionary<EBuffId, GClass2257> { 
		{
			EBuffId.DrawElite,
			gClass
		} });
		WeaponBuffs.Add(ofType, new Dictionary<EBuffId, SkillBuffClass>
		{
			{
				EBuffId.WeaponReloadBuff,
				skillBuffClass
			},
			{
				EBuffId.WeaponSwapBuff,
				skillBuffClass2
			},
			{
				EBuffId.WeaponRecoilBuff,
				skillBuffClass3
			},
			{
				EBuffId.WeaponErgonomicsBuff,
				skillBuffClass4
			},
			{
				EBuffId.WeaponDoubleMastering,
				skillBuffClass6
			},
			{
				EBuffId.WeaponDoubleActionRecoilReduceBuff,
				skillBuffClass5
			},
			{
				EBuffId.MountingErgonomicsGainPerLevel,
				skillBuffClass7
			},
			{
				EBuffId.BipodErgonomicsGainPerLevel,
				skillBuffClass8
			}
		});
		return new SkillBuffAbstractClass[9] { skillBuffClass, skillBuffClass2, skillBuffClass3, skillBuffClass6, skillBuffClass5, skillBuffClass4, gClass, skillBuffClass7, skillBuffClass8 };
	}

	public float GetBuffedGrenadeExpense(float raw)
	{
		if (!ThrowingEliteBuff)
		{
			return raw - raw * (float)ThrowingEnergyExpenses;
		}
		return 0f;
	}

	public void method_0()
	{
		float[][] array = new float[6][]
		{
			GClass1796.ArrayValues(Settings.Pistol),
			GClass1796.ArrayValues(Settings.Shotgun),
			GClass1796.ArrayValues(Settings.Assault),
			GClass1796.ArrayValues(Settings.Sniper),
			GClass1796.ArrayValues(Settings.DMR),
			GClass1796.ArrayValues(Settings.Revolver)
		};
		Pistol = new WeaponSkillClass(this, ESkillId.Pistol, typeof(PistolItemClass), array[0]);
		Revolver = new WeaponSkillClass(this, ESkillId.Revolver, typeof(RevolverItemClass), array[5]);
		SMG = new WeaponSkillClass(this, ESkillId.SMG, typeof(SmgItemClass), array[2]);
		Assault = new WeaponSkillClass(this, ESkillId.Assault, typeof(GClass3308), array[2]);
		Shotgun = new WeaponSkillClass(this, ESkillId.Shotgun, typeof(ShotgunItemClass), array[1]);
		Sniper = new WeaponSkillClass(this, ESkillId.Sniper, typeof(SniperRifleItemClass), array[3]);
		LMG = new WeaponSkillClass(this, ESkillId.LMG, typeof(MachineGunItemClass), array[2]);
		HMG = new WeaponSkillClass(this, ESkillId.HMG, typeof(int), array[2]);
		Launcher = new WeaponSkillClass(this, ESkillId.Launcher, typeof(LauncherItemClass), array[0]);
		AttachedLauncher = new WeaponSkillClass(this, ESkillId.AttachedLauncher, typeof(float), array[0]);
		Melee = new WeaponSkillClass(this, ESkillId.Melee, typeof(KnifeItemClass), array[0]);
		DMR = new WeaponSkillClass(this, ESkillId.DMR, typeof(MarksmanRifleItemClass), array[4]);
		Throwing = new SkillClass(this, ESkillId.Throwing, ESkillClass.Combat, new SkillActionClass[1] { ThrowAction.Factor(Settings.Throwing.ThrowAction) }, new SkillBuffAbstractClass[3]
		{
			ThrowingStrengthBuff.PerLevel(0.005f),
			ThrowingEnergyExpenses.PerLevel(0.01f),
			ThrowingEliteBuff
		});
		RecoilControl = new SkillClass(this, ESkillId.RecoilControl, ESkillClass.Combat, Array.Empty<SkillActionClass>(), new SkillBuffAbstractClass[1] { RecoilControlImprove.PerLevel(Settings.RecoilControl.RecoilBonusPerLevel) });
		AimDrills = new SkillClass(this, ESkillId.AimDrills, ESkillClass.Combat, new SkillActionClass[1] { WeaponShotAction.Where((Item _) => FastAimTimer.Value > 0f && FastAimTimer.Target == 0f).Factor(Settings.AimDrills.WeaponShotAction) }, new SkillBuffAbstractClass[4]
		{
			DrawSpeed.Max(0.5f),
			DrawElite,
			DrawTremor,
			DrawSound.Max(0.5f)
		});
		TroubleShooting = new SkillClass(this, ESkillId.TroubleShooting, ESkillClass.Combat, new SkillActionClass[1] { WeaponFixAction.Factor(Settings.TroubleShooting.SkillPointsPerMalfFix) }, new SkillBuffAbstractClass[5]
		{
			TroubleFixing.PerLevel(Settings.TroubleShooting.MalfRepairSpeedBonusPerLevel),
			TroubleFixingExamineMalfElite,
			TroubleFixingDurElite,
			TroubleFixingMagElite,
			TroubleFixingAmmoElite
		});
	}

	public SkillManager(SkillsDescriptorClass descriptor)
		: this()
	{
		foreach (SkillsDescriptorClass.GClass2225 item in GClass853.EmptyIfNull(descriptor.Common))
		{
			GetSkill(item.Id)?.UpdateFromInfo(item);
		}
		BackendConfigSettingsClass.GClass1734[] mastering = Singleton<BackendConfigSettingsClass>.Instance.Mastering;
		foreach (SkillsDescriptorClass.GClass2226 mastering2 in GClass853.EmptyIfNull(descriptor.Mastering))
		{
			MasterNewWeapon(mastering.FirstOrDefault((BackendConfigSettingsClass.GClass1734 x) => x.Id == mastering2.Id), mastering2.Progress);
		}
	}

	public SkillManager()
	{
		method_3();
		method_2();
		method_4();
		method_0();
		method_7();
		DisplayList = new SkillClass[43]
		{
			Endurance, Strength, Vitality, Health, StressResistance, Metabolism, Immunity, Perception, Intellect, Attention,
			Charisma, Pistol, Revolver, SMG, Assault, Shotgun, Sniper, LMG, HMG, Launcher,
			AttachedLauncher, Throwing, Melee, DMR, AimDrills, TroubleShooting, Surgery, CovertMovement, Search, MagDrills,
			FieldMedicine, FirstAid, LightVests, HeavyVests, NightOps, SilentOps, WeaponTreatment, Auctions, Cleanoperations, Shadowconnections,
			Taskperformance, Crafting, HideoutManagement
		};
		Skills = new SkillClass[2] { BotReload, BotSoundGoef }.Concat(DisplayList).ToArray();
		SkillClass[] skills = Skills;
		foreach (SkillClass skill in skills)
		{
			Unsubscribers.Add(skill.SkillLevelChanged.Subscribe(delegate
			{
				this.OnSkillLevelChanged?.Invoke(skill);
			}));
			Unsubscribers.Add(skill.SkillExperienceChanged.Subscribe(delegate
			{
				this.OnSkillExperienceChanged?.Invoke(skill);
			}));
		}
		method_1();
	}

	public void Init(BonusController bonusController)
	{
		BonusController = bonusController;
	}

	public void StartClientMode()
	{
		SkillClass[] skills = Skills;
		for (int i = 0; i < skills.Length; i++)
		{
			skills[i].Unsubscribe();
		}
		foreach (KeyValuePair<string, MasterSkillClass> item in Mastering)
		{
			item.Deconstruct(out var _, out var value);
			value.Unsubscribe();
		}
	}

	public float GetEffectiveness(int skillPointsEarned)
	{
		BackendConfigSettingsClass instance = Singleton<BackendConfigSettingsClass>.Instance;
		int skillFreshPoints = instance.SkillFreshPoints;
		int num = instance.SkillFreshPoints + instance.SkillPointsBeforeFatigue;
		if (skillPointsEarned < skillFreshPoints)
		{
			return instance.SkillFreshEffectiveness;
		}
		if (skillPointsEarned < num)
		{
			return 1f;
		}
		return Mathf.Max(instance.SkillMinEffectiveness, Mathf.Pow(instance.SkillFatiguePerPoint, 1 + skillPointsEarned - num));
	}

	public void OnWeaponDraw(Item item)
	{
		if (item != null)
		{
			BackendConfigSettingsClass.GClass1734 gClass = MasteringGroup(item.TemplateId);
			if (gClass != null && !Mastering.ContainsKey(gClass.Id))
			{
				MasterNewWeapon(gClass);
			}
		}
	}

	public static BackendConfigSettingsClass.GClass1734 MasteringGroup(string templateId)
	{
		if (Singleton<BackendConfigSettingsClass>.Instance.Associations.ContainsKey(templateId))
		{
			return Singleton<BackendConfigSettingsClass>.Instance.Associations[templateId];
		}
		Debug.LogWarning("Can't master something " + templateId);
		return null;
	}

	public double CalculateCharismaInsuranceDiscount(Profile.TraderInfo traderInfo)
	{
		return 1.0 - ((double)(float)CharismaInsuranceDiscount + (double)Math.Min(Charisma.Level, 50) * ((double)traderInfo.LoyaltyLevel - 1.0) / 3.0 * (double)Singleton<BackendConfigSettingsClass>.Instance.SkillsSettings.Charisma.BonusSettings.LevelBonusSettings.InsuranceTraderDiscount);
	}

	public void ChangeMasteringLevel(string weaponTemplateId, float value, bool silent = false)
	{
		if (Skills != null)
		{
			(Mastering.ContainsKey(weaponTemplateId) ? Mastering[weaponTemplateId] : MasterNewWeapon(Singleton<BackendConfigSettingsClass>.Instance.Mastering.FirstOrDefault((BackendConfigSettingsClass.GClass1734 x) => x.Id == weaponTemplateId), value)).SetCurrent(value, silent);
		}
	}

	public MasterSkillClass MasterNewWeapon(BackendConfigSettingsClass.GClass1734 group, float progress = 0f)
	{
		if (group == null)
		{
			Debug.LogWarning("You are trying to master something that not intended to be mastered");
			return null;
		}
		MasterSkillClass newMastering = new MasterSkillClass(group, new SkillActionClass[1] { WeaponShotAction.Where((Item x) => x != null && MasteringGroup(x.TemplateId) == group).Factor(1f, nonWeaponSkill: false) })
		{
			Current = progress
		};
		Unsubscribers.Add(newMastering.SkillLevelChanged.Subscribe(delegate
		{
			this.WeaponMastered?.Invoke(newMastering);
		}));
		Unsubscribers.Add(newMastering.SkillExperienceChanged.Subscribe(delegate
		{
			this.OnMasteringExperienceChanged?.Invoke(newMastering);
		}));
		Mastering.Add(group.Id, newMastering);
		this.NewWeaponMastered?.Invoke(newMastering);
		return newMastering;
	}

	public bool TryGetSkill(ESkillId skillId, out SkillClass skill)
	{
		SkillClass[] skills = Skills;
		int num = 0;
		SkillClass skillClass;
		while (true)
		{
			if (num < skills.Length)
			{
				skillClass = skills[num];
				if (skillClass.Id.Equals(skillId))
				{
					break;
				}
				num++;
				continue;
			}
			skill = null;
			return false;
		}
		skill = skillClass;
		return true;
	}

	public void method_1()
	{
		SkillClass[] skills = Skills;
		for (int i = 0; i < skills.Length; i++)
		{
			if (skills[i] is WeaponSkillClass weaponSkillClass)
			{
				switch (weaponSkillClass.Id)
				{
				case ESkillId.Pistol:
					WeaponSkills.Add(typeof(PistolItemClass), weaponSkillClass);
					break;
				case ESkillId.Revolver:
					WeaponSkills.Add(typeof(RevolverItemClass), weaponSkillClass);
					break;
				case ESkillId.SMG:
					WeaponSkills.Add(typeof(SmgItemClass), weaponSkillClass);
					break;
				case ESkillId.Assault:
					WeaponSkills.Add(typeof(AssaultRifleItemClass), weaponSkillClass);
					break;
				case ESkillId.Shotgun:
					WeaponSkills.Add(typeof(ShotgunItemClass), weaponSkillClass);
					break;
				case ESkillId.Sniper:
					WeaponSkills.Add(typeof(SniperRifleItemClass), weaponSkillClass);
					break;
				case ESkillId.LMG:
					WeaponSkills.Add(typeof(MachineGunItemClass), weaponSkillClass);
					break;
				case ESkillId.DMR:
					WeaponSkills.Add(typeof(MarksmanRifleItemClass), weaponSkillClass);
					break;
				}
			}
		}
	}

	public SkillClass GetSkill(ESkillId skillId)
	{
		SkillClass[] skills = Skills;
		int num = 0;
		SkillClass skillClass;
		while (true)
		{
			if (num < skills.Length)
			{
				skillClass = skills[num];
				if (skillClass.Id == skillId)
				{
					break;
				}
				num++;
				continue;
			}
			Debug.LogWarning("Can't find skill to upgrade: " + GClass867.ToStringNoBox(skillId));
			return null;
		}
		return skillClass;
	}

	[CanBeNull]
	public MasterSkillClass GetMastering(string templateId)
	{
		return Mastering.FirstOrDefault((KeyValuePair<string, MasterSkillClass> x) => x.Value.MasteringGroup.Templates.Contains(templateId)).Value;
	}

	[CanBeNull]
	public MasterSkillClass GetMasteringGroup(string groupId)
	{
		Mastering.TryGetValue(groupId, out var value);
		return value;
	}

	public void Terminate()
	{
		foreach (Action unsubscriber in Unsubscribers)
		{
			unsubscriber();
		}
		Unsubscribers.Clear();
		for (int i = 0; i < Skills.Length; i++)
		{
			Skills[i].Unsubscribe();
		}
		foreach (KeyValuePair<string, MasterSkillClass> item in Mastering)
		{
			item.Value.Unsubscribe();
		}
	}

	public void ApplyChanges(SkillManager skillManager)
	{
		SkillClass[] skills = skillManager.Skills;
		foreach (SkillClass newSkillInfo in skills)
		{
			Skills.FirstOrDefault((SkillClass x) => x.Id == newSkillInfo.Id)?.UpdateFromAnother(newSkillInfo);
		}
		foreach (var (key, baseSkill) in skillManager.Mastering)
		{
			if (Mastering.TryGetValue(key, out var value))
			{
				value.UpdateFromAnother(baseSkill);
			}
		}
	}

	public void method_2()
	{
		Perception = new SkillClass(this, ESkillId.Perception, ESkillClass.Mental, new SkillActionClass[2]
		{
			OnlineAction.Factor(2.5f),
			UniqueLoot.Factor(0.334f)
		}, new SkillBuffAbstractClass[2]
		{
			PerceptionLootDot.PerLevel(0.02f),
			PerceptionEliteNoIdea
		});
		List<SkillActionClass> list = new List<SkillActionClass>
		{
			ExamineAction.Factor(Settings.Intellect.ExamineAction),
			SkillProgress.Where((SkillClass skill) => skill.Id == ESkillId.Lockpicking).Factor(Settings.Intellect.SkillProgress),
			RepairAction.Factor(Settings.Intellect.RepairAction)
		};
		BackendConfigSettingsClass.GClass1757 crafting = Singleton<BackendConfigSettingsClass>.Instance.SkillsSettings.Crafting;
		if (crafting.CraftingPointsToIntelligence > 0f)
		{
			SkillActionClass skillActionClass = SkillProgress.Where((SkillClass skill) => skill.Id == ESkillId.Crafting).Factor(1f / crafting.CraftingPointsToIntelligence);
			skillActionClass.SimpleCalculation = true;
			list.Add(skillActionClass);
		}
		Intellect = new SkillClass(this, ESkillId.Intellect, ESkillClass.Mental, list.ToArray(), new SkillBuffAbstractClass[6]
		{
			IntellectEliteAmmoCounter,
			IntellectEliteContainerScope,
			IntellectEliteNaturalLearner,
			IntellectLearningSpeed.PerLevel(0.02f),
			IntellectWeaponMaintance.PerLevel(0.02f),
			IntellectRepairPointsCostReduction.PerLevel(Singleton<BackendConfigSettingsClass>.Instance.SkillsSettings.Intellect.RepairPointsCostReduction)
		});
		Attention = new SkillClass(this, ESkillId.Attention, ESkillClass.Mental, new SkillActionClass[3]
		{
			ExamineWithInstruction.Factor(Settings.Attention.ExamineWithInstruction),
			FindAction.Where((bool x) => x).Factor(Settings.Attention.FindActionTrue),
			FindAction.Where((bool x) => !x).Factor(Settings.Attention.FindActionFalse)
		}, new SkillBuffAbstractClass[4]
		{
			AttentionEliteExtraLootExp,
			AttentionEliteLuckySearch.Elite(0.5f).PerLevel(0f),
			AttentionLootSpeed.PerLevel(0.02f),
			AttentionExamine.PerLevel(0.02f)
		});
		Charisma = new SkillClass(this, ESkillId.Charisma, ESkillClass.Mental, new SkillActionClass[3]
		{
			SkillProgress.Where((SkillClass skill) => skill.Id == ESkillId.Intellect).Factor(Settings.Charisma.SkillProgressInt),
			SkillProgress.Where((SkillClass skill) => skill.Id == ESkillId.Attention).Factor(Settings.Charisma.SkillProgressAtn),
			SkillProgress.Where((SkillClass skill) => skill.Id == ESkillId.Perception).Factor(Settings.Charisma.SkillProgressPer)
		}, new SkillBuffAbstractClass[7]
		{
			CharismaDailyQuestsRerollDiscount.PerLevel(Settings.Charisma.BonusSettings.LevelBonusSettings.RepeatableQuestChangeDiscount),
			CharismaHealingDiscount.PerLevel(Settings.Charisma.BonusSettings.LevelBonusSettings.HealthRestoreDiscount),
			CharismaInsuranceDiscount.PerLevel(Settings.Charisma.BonusSettings.LevelBonusSettings.InsuranceDiscount),
			CharismaExfiltrationDiscount.PerLevel(Settings.Charisma.BonusSettings.LevelBonusSettings.PaidExitDiscount),
			CharismaEliteScavCaseDiscount.Elite(Settings.Charisma.BonusSettings.EliteBonusSettings.ScavCaseDiscount),
			CharismaEliteFenceRepPenaltyReduction.Elite(Settings.Charisma.BonusSettings.EliteBonusSettings.FenceStandingLossDiscount),
			CharismaEliteAdditionalDailyQuests.Elite(Settings.Charisma.BonusSettings.EliteBonusSettings.RepeatableQuestExtraCount)
		});
	}

	public void method_3()
	{
		Endurance = new SkillClass(this, ESkillId.Endurance, ESkillClass.Physical, new SkillActionClass[2]
		{
			SprintAction.Factor((GStruct279 movement) => (!(movement.Overweight > 0f)) ? (Settings.Endurance.SprintAction * (1f + Settings.Endurance.GainPerFatigueStack * movement.Fatigue)) : 0f),
			MovementAction.Factor((GStruct279 movement) => (!(movement.Overweight > 0f)) ? (Settings.Endurance.MovementAction * (1f + Settings.Endurance.GainPerFatigueStack * movement.Fatigue)) : 0f)
		}, new SkillBuffAbstractClass[6]
		{
			EnduranceBuffEnduranceInc.Max(0.5f).Elite(0.7f),
			EnduranceHands.PerLevel(0f).Elite(0.5f),
			EnduranceBuffJumpCostRed.Max(0.3f),
			EnduranceBuffBreathTimeInc.Max(1f),
			EnduranceBuffRestoration.Max(0.5f).Elite(0.75f),
			EnduranceBreathElite.PerLevel(0f).Elite(1f)
		});
		Strength = new SkillClass(this, ESkillId.Strength, ESkillClass.Physical, new SkillActionClass[5]
		{
			SprintAction.Factor((GStruct279 movement) => (!(movement.Overweight > 0f)) ? 0f : Mathf.Lerp(Settings.Strength.SprintActionMin, Settings.Strength.SprintActionMax, movement.Overweight)),
			MovementAction.Factor((GStruct279 movement) => (!(movement.Overweight > 0f)) ? 0f : Mathf.Lerp(Settings.Strength.MovementActionMin, Settings.Strength.MovementActionMax, movement.Overweight)),
			PushUp.Factor((GStruct279 movement) => (!(movement.Overweight > 0f)) ? 0f : Mathf.Lerp(Settings.Strength.PushUpMin, Settings.Strength.PushUpMax, movement.Overweight)),
			FistfightAction.Factor(0.2f),
			ThrowAction.Factor(0.2f)
		}, new SkillBuffAbstractClass[8]
		{
			StrengthBuffJumpHeightInc.Max(0.2f),
			StrengthBuffLiftWeightInc.Max(0.3f),
			StrengthBuffMeleePowerInc.Max(0.3f),
			StrengthBuffSprintSpeedInc.Max(0.2f),
			StrengthBuffThrowDistanceInc.Max(0.2f),
			StrengthBuffAimFatigue.Max(0.2f),
			StrengthBuffElite,
			StrengthBuffMeleeCrits.PerLevel(0f).Elite(0.5f)
		});
		Vitality = new SkillClass(this, ESkillId.Vitality, ESkillClass.Physical, new SkillActionClass[2]
		{
			DamageTakenAction.Factor(Settings.Vitality.DamageTakenAction),
			HealthNegativeEffect.Where((IEffect effect) => effect is GInterface341).Factor(Settings.Vitality.HealthNegativeEffect)
		}, new SkillBuffAbstractClass[4]
		{
			VitalityBuffBleedChanceRed.PerLevel(0.012f),
			VitalityBuffSurviobilityInc.PerLevel(0.004f),
			VitalityBuffRegeneration,
			VitalityBuffBleedStop
		});
		ESkillId[] skillsRelatedToHealth = new ESkillId[3]
		{
			ESkillId.Strength,
			ESkillId.Endurance,
			ESkillId.Vitality
		};
		Health = new SkillClass(this, ESkillId.Health, ESkillClass.Physical, new SkillActionClass[1] { SkillProgress.Where((SkillClass skill) => Array.IndexOf(skillsRelatedToHealth, skill.Id) >= 0).Factor(Settings.Health.SkillProgress) }, new SkillBuffAbstractClass[4]
		{
			HealthBreakChanceRed.PerLevel(0.012f),
			HealthEnergy.PerLevel(0.006f),
			HealthHydration.PerLevel(0.006f),
			HealthEliteAbsorbDamage
		});
		Metabolism = new SkillClass(this, ESkillId.Metabolism, ESkillClass.Physical, new SkillActionClass[2]
		{
			HydrationChanged.Where((float x) => x > 0f).Factor(Settings.Metabolism.HydrationRecoveryRate),
			EnergyChanged.Where((float x) => x > 0f).Factor(Settings.Metabolism.EnergyRecoveryRate)
		}, new SkillBuffAbstractClass[3]
		{
			MetabolismEliteBuffNoDyhydration,
			MetabolismRatioPlus.PerLevel(0.01f),
			MetabolismMiscDebuffTime.PerLevel(0.01f)
		});
		StressResistance = new SkillClass(this, ESkillId.StressResistance, ESkillClass.Physical, new SkillActionClass[2]
		{
			HealthNegativeEffect.Where((IEffect effect) => effect is GInterface357).Factor(Settings.StressResistance.HealthNegativeEffect),
			LowHPDuration.Factor(Settings.StressResistance.LowHPDuration)
		}, new SkillBuffAbstractClass[3]
		{
			StressPain.PerLevel(0.01f),
			StressTremor.PerLevel(0.012f),
			StressBerserk
		});
		Immunity = new SkillClass(this, ESkillId.Immunity, ESkillClass.Physical, new SkillActionClass[2]
		{
			HealthNegativeEffect.Where((IEffect effect) => effect is GInterface346).Factor(Settings.Immunity.HealthNegativeEffect),
			StimulatorNegativeBuff.Factor(Settings.Immunity.StimulatorNegativeBuff)
		}, new SkillBuffAbstractClass[5]
		{
			ImmunityMiscEffects.PerLevel(0.01f),
			ImmunityPoisonBuff.PerLevel(0.01f),
			ImmunityPainKiller.PerLevel(0.006f),
			ImmunityAvoidPoisonChance.Default(0f).Elite(0.9f),
			ImmunityAvoidMiscEffectsChance.Default(0f).Elite(0.9f)
		});
	}

	public void method_4()
	{
		Crafting = new GClass2248(this, ESkillId.Crafting, ESkillClass.Practical, new SkillActionClass[2]
		{
			HideoutCraftTimerAction.Factor((float craftTime) => Settings.Crafting.PointsPerSecond * craftTime),
			UniqueCrafting.Factor(Settings.Crafting.PointsPerOriginalCraft)
		}, new SkillBuffAbstractClass[3]
		{
			SingleCraftTimeReduce.PerLevel(Settings.Crafting.CraftTimeReductionPerLevel / 100f),
			ContinueCraftTimeReduce.PerLevel(Settings.Crafting.ProductionTimeReductionPerLevel / 100f),
			EliteCrafting
		});
		List<SkillActionClass> list = new List<SkillActionClass>
		{
			HideoutCompleteCraftAction.Factor(Settings.HideoutManagement.SkillPointsPerCraft),
			HideoutZoneUpgradeAction.Factor(Settings.HideoutManagement.SkillPointsPerAreaUpgrade)
		};
		foreach (KeyValuePair<EAreaType, BackendConfigSettingsClass.GClass1754.GClass1755> item in Settings.HideoutManagement.SkillPointsRate)
		{
			item.Deconstruct(out var key, out var value);
			EAreaType key2 = key;
			BackendConfigSettingsClass.GClass1754.GClass1755 pointsRate = value;
			GClass2261<(float, bool)> gClass = new GClass2261<(float, bool)>();
			gClass.Factor(((float, bool) tuple) => (!tuple.Item2) ? pointsRate.PointsPerResource : (pointsRate.PointsPerResource + Settings.HideoutManagement.SkillPointsRate[EAreaType.SolarPower].PointsPerResource));
			ConsumptionActions.Add(key2, gClass);
			list.Add(gClass);
		}
		HideoutManagement = new GClass2248(this, ESkillId.HideoutManagement, ESkillClass.Practical, list.ToArray(), new SkillBuffAbstractClass[3]
		{
			HideoutExtraSlots,
			ZoneBonusBoost.PerLevel((float)Settings.HideoutManagement.SkillBoostPercent),
			HideoutResourceConsumption.PerLevel(Settings.HideoutManagement.ConsumptionReductionPerLevel / 100f)
		});
		CovertMovement = new SkillClass(this, ESkillId.CovertMovement, ESkillClass.Practical, new SkillActionClass[1] { MovementAction.Factor((GStruct279 movement) => (!(movement.Noise < 1f)) ? 0f : Settings.CovertMovement.MovementAction) }, new SkillBuffAbstractClass[5]
		{
			CovertMovementSoundVolume.PerLevel(0.012f),
			CovertMovementLoud.PerLevel(0.006f).Elite(0.6f),
			CovertMovementElite,
			CovertMovementSpeed.Custom((int i) => 1f + (float)i * 0.01f),
			CovertMovementEquipment.PerLevel(0.012f)
		});
		FieldMedicine = new SkillClass(this, ESkillId.FieldMedicine, ESkillClass.Practical, Array.Empty<SkillActionClass>(), Array.Empty<SkillBuffAbstractClass>());
		Surgery = new SkillClass(this, ESkillId.Surgery, ESkillClass.Practical, new SkillActionClass[2]
		{
			SurgeryAction.Factor(Settings.Surgery.SurgeryAction),
			SkillProgress.Where((SkillClass x) => x.Id == ESkillId.FieldMedicine || x.Id == ESkillId.FirstAid).Factor(Settings.Surgery.SkillProgress)
		}, new SkillBuffAbstractClass[2]
		{
			SurgerySpeed.Max(0.2f).Elite(0.4f),
			SurgeryReducePenalty.PerLevel(0.01f).Elite(1f)
		});
		Search = new SkillClass(this, ESkillId.Search, ESkillClass.Practical, new SkillActionClass[2]
		{
			SearchAction.Factor(Settings.Search.SearchAction),
			FindAction.Where((bool x) => x).Factor(Settings.Search.FindAction)
		}, new SkillBuffAbstractClass[2]
		{
			SearchDouble,
			SearchBuffSpeed.PerLevel(0.01f)
		});
		Sniping = new SkillClass(this, ESkillId.Sniping, ESkillClass.Practical, new SkillActionClass[1] { KillAction.Where((GClass2251 p) => p.Distance >= 70f && p.HoldBreath && p.BodyPart == EBodyPart.Head).Factor(1f) }, Array.Empty<SkillBuffAbstractClass>());
		SkillActionClass[] actions = Array.Empty<SkillActionClass>();
		SkillBuffAbstractClass[] buffs = Array.Empty<SkillBuffAbstractClass>();
		ProneMovement = new SkillClass(this, ESkillId.ProneMovement, ESkillClass.Practical, actions, buffs);
		FirstAid = new SkillClass(this, ESkillId.FirstAid, ESkillClass.Practical, actions, buffs);
		LightVests = new SkillClass(this, ESkillId.LightVests, ESkillClass.Practical, new SkillActionClass[1] { LightArmorDamageTakenAction.Factor(0.05f) }, new SkillBuffAbstractClass[5]
		{
			LightVestMoveSpeedPenaltyReduction.PerLevel(Settings.LightVests.MoveSpeedPenaltyReductionLVestsReducePerLevel),
			LightVestMeleeWeaponDamageReduction.PerLevel(Settings.LightVests.MeleeDamageLVestsReducePerLevel),
			LightVestRepairDegradationReduction.PerLevel(Settings.LightVests.WearAmountRepairLVestsReducePerLevel),
			LightVestBleedingProtection,
			LightVestDeteriorationChanceOnRepairReduce
		});
		HeavyVests = new SkillClass(this, ESkillId.HeavyVests, ESkillClass.Practical, new SkillActionClass[1] { HeavyArmorDamageTakenAction.Factor(0.025f) }, new SkillBuffAbstractClass[5]
		{
			HeavyVestMoveSpeedPenaltyReduction.PerLevel(Settings.HeavyVests.MoveSpeedPenaltyReductionHVestsReducePerLevel),
			HeavyVestBluntThroughputDamageReduction.PerLevel(Settings.HeavyVests.BluntThroughputDamageHVestsReducePerLevel),
			HeavyVestRepairDegradationReduction.PerLevel(Settings.HeavyVests.WearAmountRepairHVestsReducePerLevel),
			HeavyVestNoBodyDamageDeflectChance,
			HeavyVestDeteriorationChanceOnRepairReduce
		});
		WeaponModding = new SkillClass(this, ESkillId.WeaponModding, ESkillClass.Practical, actions, buffs);
		AdvancedModding = new SkillClass(this, ESkillId.AdvancedModding, ESkillClass.Practical, actions, buffs);
		NightOps = new SkillClass(this, ESkillId.NightOps, ESkillClass.Practical, actions, buffs);
		SilentOps = new SkillClass(this, ESkillId.SilentOps, ESkillClass.Practical, actions, buffs);
		Lockpicking = new SkillClass(this, ESkillId.Lockpicking, ESkillClass.Practical, actions, buffs);
		WeaponTreatment = new GClass2248(this, ESkillId.WeaponTreatment, ESkillClass.Practical, new SkillActionClass[1] { WeaponRepairedAction.Factor(Settings.WeaponTreatment.SkillPointsPerRepair) }, new SkillBuffAbstractClass[3]
		{
			WeaponDurabilityLosOnShotReduce.PerLevel(Settings.WeaponTreatment.DurabilityLossReducePerLevel),
			WearAmountRepairGunsReducePerLevel.PerLevel(Settings.WeaponTreatment.WearAmountRepairGunsReducePerLevel),
			WearChanceRepairGunsReduceEliteLevel
		});
		MagDrills = new SkillClass(this, ESkillId.MagDrills, ESkillClass.Practical, new SkillActionClass[3]
		{
			RaidLoadedAmmoAction.Factor(Settings.MagDrills.RaidLoadedAmmoAction),
			RaidUnloadedAmmoAction.Factor(Settings.MagDrills.RaidUnloadedAmmoAction),
			MagazineCheckAction.Factor(Settings.MagDrills.MagazineCheckAction)
		}, new SkillBuffAbstractClass[6]
		{
			MagDrillsLoadSpeed.PerLevel(0.6f),
			MagDrillsUnloadSpeed.PerLevel(0.6f),
			MagDrillsInventoryCheckSpeed.PerLevel(0.8f),
			MagDrillsInventoryCheckAccuracy.Custom((int i) => Mathf.Clamp((float)i / 10f, 0f, 2f)),
			MagDrillsInstantCheck,
			MagDrillsLoadProgression
		});
		Freetrading = new SkillClass(this, ESkillId.Freetrading, ESkillClass.Practical, Array.Empty<SkillActionClass>(), Array.Empty<SkillBuffAbstractClass>());
		Auctions = new SkillClass(this, ESkillId.Auctions, ESkillClass.Practical, Array.Empty<SkillActionClass>(), Array.Empty<SkillBuffAbstractClass>());
		Cleanoperations = new SkillClass(this, ESkillId.Cleanoperations, ESkillClass.Practical, Array.Empty<SkillActionClass>(), Array.Empty<SkillBuffAbstractClass>());
		Barter = new SkillClass(this, ESkillId.Barter, ESkillClass.Practical, Array.Empty<SkillActionClass>(), Array.Empty<SkillBuffAbstractClass>());
		Shadowconnections = new SkillClass(this, ESkillId.Shadowconnections, ESkillClass.Practical, Array.Empty<SkillActionClass>(), Array.Empty<SkillBuffAbstractClass>());
		Taskperformance = new SkillClass(this, ESkillId.Taskperformance, ESkillClass.Practical, Array.Empty<SkillActionClass>(), Array.Empty<SkillBuffAbstractClass>());
	}

	public SkillClass[] method_5()
	{
		BearAssaultoperations = new SkillClass(this, ESkillId.BearAssaultoperations, ESkillClass.Special, Array.Empty<SkillActionClass>(), Array.Empty<SkillBuffAbstractClass>());
		BearAuthority = new SkillClass(this, ESkillId.BearAuthority, ESkillClass.Special, Array.Empty<SkillActionClass>(), Array.Empty<SkillBuffAbstractClass>());
		BearAksystems = new SkillClass(this, ESkillId.BearAksystems, ESkillClass.Special, Array.Empty<SkillActionClass>(), Array.Empty<SkillBuffAbstractClass>());
		BearHeavycaliber = new SkillClass(this, ESkillId.BearHeavycaliber, ESkillClass.Special, Array.Empty<SkillActionClass>(), Array.Empty<SkillBuffAbstractClass>());
		BearRawpower = new SkillClass(this, ESkillId.BearRawpower, ESkillClass.Special, Array.Empty<SkillActionClass>(), Array.Empty<SkillBuffAbstractClass>());
		return new SkillClass[5] { BearAssaultoperations, BearAuthority, BearAksystems, BearHeavycaliber, BearRawpower };
	}

	public SkillClass[] method_6()
	{
		UsecArsystems = new SkillClass(this, ESkillId.UsecArsystems, ESkillClass.Special, Array.Empty<SkillActionClass>(), Array.Empty<SkillBuffAbstractClass>());
		UsecDeepweaponmodding = new SkillClass(this, ESkillId.UsecDeepweaponmodding, ESkillClass.Special, Array.Empty<SkillActionClass>(), Array.Empty<SkillBuffAbstractClass>());
		UsecLongrangeoptics = new SkillClass(this, ESkillId.UsecLongrangeoptics, ESkillClass.Special, Array.Empty<SkillActionClass>(), Array.Empty<SkillBuffAbstractClass>());
		UsecNegotiations = new SkillClass(this, ESkillId.UsecNegotiations, ESkillClass.Special, Array.Empty<SkillActionClass>(), Array.Empty<SkillBuffAbstractClass>());
		UsecTactics = new SkillClass(this, ESkillId.UsecTactics, ESkillClass.Special, Array.Empty<SkillActionClass>(), Array.Empty<SkillBuffAbstractClass>());
		return new SkillClass[5] { UsecArsystems, UsecDeepweaponmodding, UsecLongrangeoptics, UsecNegotiations, UsecTactics };
	}

	public void method_7()
	{
		BotReload = new SkillClass(this, ESkillId.BotReload, ESkillClass.Special, Array.Empty<SkillActionClass>(), new SkillBuffAbstractClass[1] { BotReloadSpeed.PerLevel(0.1f) });
		BotSoundGoef = new SkillClass(this, ESkillId.BotSound, ESkillClass.Special, Array.Empty<SkillActionClass>(), new SkillBuffAbstractClass[1] { BotSoundCoef.PerLevel(0.02f) });
	}

	[CompilerGenerated]
	public bool method_8(Item _)
	{
		if (FastAimTimer.Value > 0f)
		{
			return FastAimTimer.Target == 0f;
		}
		return false;
	}

	[CompilerGenerated]
	public float method_9(float craftTime)
	{
		return Settings.Crafting.PointsPerSecond * craftTime;
	}

	[CompilerGenerated]
	public float method_10(GStruct279 movement)
	{
		if (!(movement.Noise < 1f))
		{
			return 0f;
		}
		return Settings.CovertMovement.MovementAction;
	}
}
