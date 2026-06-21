using System;
using System.Runtime.CompilerServices;
using Comfort.Common;
using Diz.Binding;
using EFT;
using UnityEngine;

public class SkillClass : AbstractSkillClass
{
	public const int MAX_LEVEL = 50;

	public const int DEFAULT_EXP_LEVEL = 9;

	public const int ADDITIONAL_EXP_PER_LEVEL = 10;

	public readonly SkillManager.SkillBuffAbstractClass[] Buffs;

	public readonly ESkillClass Class;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public float Float_4 = float.MaxValue;

	public readonly BindableEvent OnLevelUp = new BindableEvent();

	[NonSerialized]
	[CompilerGenerated]
	public SkillManager SkillManager_0;

	public readonly bool Locked;

	public SkillManager SkillManager
	{
		[CompilerGenerated]
		get
		{
			return SkillManager_0;
		}
	}

	public int LevelExp => method_2(base.Level);

	public bool IsEliteLevel => base.Level > 50;

	public float BaseProgress => Math.Max(GClass2340.InRaid ? (LevelProgress - PointsEarned / (float)LevelExp) : method_0(base.Current - PointsEarned), 0f);

	public float ProgressValue => (float)Math.Round(GClass2340.InRaid ? PointsEarned : method_4(), 2);

	public override float PointsEarned => Float_2;

	public override float Effectiveness => Float_3;

	public override int Buff
	{
		get
		{
			return Int_0;
		}
		set
		{
			Int_0 = value;
			method_3();
			OnLevelUp?.Invoke();
		}
	}

	public SkillClass(SkillManager skillManager, ESkillId id, ESkillClass skillClass, SkillManager.SkillActionClass[] actions, SkillManager.SkillBuffAbstractClass[] buffs)
		: base(actions)
	{
		SkillManager_0 = skillManager;
		Id = id;
		Class = skillClass;
		Buffs = buffs;
		Float_3 = SkillManager.GetEffectiveness(0);
		Locked = buffs.Length == 0;
	}

	public void UpdateFromServer(GStruct261 skillPacket, bool silent = false)
	{
		Float_3 = skillPacket.Effectiveness;
		Float_2 = skillPacket.PointsEarned;
		SetCurrent(skillPacket.Value, silent);
	}

	public void UpdateFromInfo(SkillsDescriptorClass.GClass2225 skillInfo)
	{
		float progress = skillInfo.Progress;
		base.Current = Mathf.Max(progress, 0f);
		SetPointsEarnedInSession(skillInfo.PointsEarnedDuringSession);
		LastCall = EFTDateTimeClass.UniversalDateTimeFromUnixTime(skillInfo.LastAccess);
		method_3();
	}

	public override void SetPointsEarnedInSession(float fatigue, bool updateEffectiveness = true)
	{
		Float_2 = fatigue;
		if (updateEffectiveness)
		{
			Float_3 = SkillManager.GetEffectiveness((int)Float_2);
		}
	}

	public float CalculateExpOnFirstLevels(float val)
	{
		float num = val;
		val = 0f;
		int num2 = base.Level;
		float num3 = base.Current % 100f * (float)(num2 + 1) / 10f;
		while (!(num <= 0f) && num2 < 9)
		{
			float num4 = (float)method_2(num2) - num3;
			float num5 = Mathf.Min(num, num4);
			val += 10f / (float)(num2 + 1) * num5;
			num -= num4;
			num3 = 0f;
			num2++;
		}
		if (num > 0f)
		{
			val += num;
		}
		return val;
	}

	public int method_2(int level)
	{
		if (level >= 9)
		{
			return 100;
		}
		return (level + 1) * 10;
	}

	public void AddPointsEarnedForWorkout(float pointsEarnedDelta)
	{
		Float_2 += pointsEarnedDelta;
	}

	public float UseEffectiveness(float input)
	{
		if (Time.time > Float_4)
		{
			Float_3 = 1f;
			Float_2 = Mathf.Min(Float_2, Singleton<BackendConfigSettingsClass>.Instance.SkillFreshPoints);
		}
		float num = 0f;
		int num2 = Mathf.CeilToInt(input);
		for (int i = 0; i < num2; i++)
		{
			float float_ = Float_2;
			float num3 = Mathf.Min(1f, input);
			input -= num3;
			float num4 = num3 * Float_3;
			num += num4;
			Float_2 += num4;
			if ((int)Float_2 > (int)float_)
			{
				Float_3 = SkillManager.GetEffectiveness((int)Float_2);
			}
		}
		if (Float_3 <= 1f)
		{
			Float_4 = Time.time + (float)Singleton<BackendConfigSettingsClass>.Instance.SkillFatigueReset;
		}
		return num;
	}

	public override void LevelChanged()
	{
		method_3();
		SkillManager.AnySkillUp.Complete(this);
		LastCall = EFTDateTimeClass.UtcNow;
		base.LevelChanged();
		OnLevelUp?.Invoke();
	}

	public void method_3()
	{
		int obj = Math.Min(base.SummaryLevel, 50);
		SkillManager.SkillBuffAbstractClass[] buffs = Buffs;
		for (int i = 0; i < buffs.Length; i++)
		{
			buffs[i].BaseRuleFunc?.Invoke(obj);
		}
		if (base.SummaryLevel > 50)
		{
			buffs = Buffs;
			for (int i = 0; i < buffs.Length; i++)
			{
				buffs[i].EliteRuleFunc?.Invoke();
			}
			if (base.Level > 50)
			{
				Unsubscribe();
			}
		}
	}

	public float method_4()
	{
		if ((int)(base.Current / 100f) == (int)((base.Current - PointsEarned) / 100f))
		{
			return PointsEarned / 100f * (float)method_2(base.Level);
		}
		float num = 100f - (base.Current - PointsEarned) % 100f;
		float num2 = (PointsEarned - num) % 100f;
		float num3 = (float)Math.Floor((PointsEarned - num) / 100f);
		int num4 = base.Level;
		float num5 = 0f;
		if (num2 > 0f)
		{
			num5 += num2 / 100f * (float)method_2(num4);
			num4--;
		}
		for (int i = 0; (float)i < num3; i++)
		{
			num5 += (float)method_2(num4);
			num4--;
		}
		return num5 + num / 100f * (float)method_2(num4);
	}

	public override void OnTrigger(SkillManager.SkillActionClass skillAction, float val)
	{
		SkillManager.SkillProgress.Complete(this, val);
		if (!skillAction.SimpleCalculation)
		{
			val = UseEffectiveness(val);
			val = (float)SkillManager.BonusController.Calculate(this, val);
		}
		if (base.Level < 9)
		{
			val = CalculateExpOnFirstLevels(val);
		}
		base.OnTrigger(skillAction, val);
	}

	public void SetLevel(int level)
	{
		base.Current = 100f * (float)level;
		LevelChanged();
	}
}
