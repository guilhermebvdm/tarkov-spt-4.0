using System;
using System.Runtime.CompilerServices;
using Diz.Binding;
using EFT;
using UnityEngine;

public abstract class AbstractSkillClass
{
	[NonSerialized]
	public const float Float_0 = 100f;

	public const int MAX_LEVEL_W_BUFF = 60;

	public ESkillId Id;

	[NonSerialized]
	public readonly BindableEvent SkillLevelChanged = new BindableEvent();

	[NonSerialized]
	public readonly BindableEvent SkillExperienceChanged = new BindableEvent();

	[NonSerialized]
	[CompilerGenerated]
	public float Float_1;

	public DateTime LastCall;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	[CompilerGenerated]
	public SkillManager.SkillActionClass[] Gclass2259_0;

	public float Current
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

	public SkillManager.SkillActionClass[] Actions
	{
		[CompilerGenerated]
		get
		{
			return Gclass2259_0;
		}
	}

	public int Level => GetLevelForValue(method_1(Current));

	public int SummaryLevel => Mathf.Min((Buff > 0) ? 60 : 51, Level + Buff);

	public virtual int Buff
	{
		get
		{
			return Int_0;
		}
		set
		{
			Int_0 = value;
		}
	}

	public virtual float PointsEarned => 0f;

	public virtual float Effectiveness => 1f;

	public virtual float LevelProgress => method_0(Current);

	public virtual int GetLevelForValue(float value)
	{
		return (int)(value / 100f);
	}

	public AbstractSkillClass(SkillManager.SkillActionClass[] actions)
	{
		Gclass2259_0 = actions;
		SkillManager.SkillActionClass[] actions2 = Actions;
		for (int i = 0; i < actions2.Length; i++)
		{
			actions2[i].ExternalEvent += OnTrigger;
		}
	}

	public float method_0(float progress)
	{
		return progress / 100f - (float)Level;
	}

	public virtual void OnTrigger(SkillManager.SkillActionClass skillAction, float val)
	{
		SetCurrent(Current + val, silent: true);
		LastCall = EFTDateTimeClass.UtcNow;
	}

	public virtual void SetPointsEarnedInSession(float fatigue, bool updateEffectiveness = true)
	{
	}

	public float method_1(float value)
	{
		return Mathf.Clamp(value, 0f, 5100f);
	}

	public virtual void SetCurrent(float value, bool silent = false)
	{
		bool flag;
		if (!((flag = Mathf.Approximately(Current, value)) && silent))
		{
			int level = Level;
			_ = Current;
			Current = method_1(value);
			if (!flag)
			{
				SkillExperienceChanged?.Invoke();
			}
			if (!silent || Level != level)
			{
				LevelChanged();
			}
		}
	}

	public virtual void LevelChanged()
	{
		SkillLevelChanged?.Invoke();
	}

	public virtual void Unsubscribe()
	{
		SkillManager.SkillActionClass[] actions = Actions;
		for (int i = 0; i < actions.Length; i++)
		{
			actions[i].ExternalEvent -= OnTrigger;
		}
	}

	public void UpdateFromAnother(AbstractSkillClass baseSkill)
	{
		bool num = Level != baseSkill.Level;
		Current = baseSkill.Current;
		SetPointsEarnedInSession(baseSkill.PointsEarned);
		if (num)
		{
			LevelChanged();
		}
	}
}
