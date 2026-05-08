using System;
using System.Runtime.CompilerServices;
using EFT;

public abstract class SkillBonusAbstractClass
{
	[NonSerialized]
	[CompilerGenerated]
	public MongoID MongoID_0;

	[NonSerialized]
	[CompilerGenerated]
	public double Double_0;

	public readonly bool IsVisible;

	public readonly bool Passive;

	public readonly bool Production;

	public readonly string Icon;

	public MongoID Id
	{
		[CompilerGenerated]
		get
		{
			return MongoID_0;
		}
		[CompilerGenerated]
		set
		{
			MongoID_0 = value;
		}
	}

	public double Value
	{
		[CompilerGenerated]
		get
		{
			return Double_0;
		}
		[CompilerGenerated]
		set
		{
			Double_0 = value;
		}
	}

	public abstract EBonusType BonusType { get; }

	public virtual string ValueFormat
	{
		get
		{
			if (!(ResultValue > 0.0))
			{
				return "{0}";
			}
			return "+{0}";
		}
	}

	public virtual Enum TargetType => BonusType;

	public virtual double ResultValue => Value;

	public virtual string LocalizationKey => $"hideout_{BonusType}";

	public virtual bool IsPositive => Value >= 0.0;

	public abstract double CalculateValue(double input);

	public SkillBonusAbstractClass(ProfileBonusesClass descriptor)
	{
		if (descriptor.BonusType != BonusType)
		{
			throw new Exception($"Wrong bonus type: {descriptor.BonusType}, {BonusType} expected.");
		}
		Id = descriptor.Id;
		Value = descriptor.Value;
		IsVisible = descriptor.IsVisible;
		Passive = descriptor.Passive;
		Production = descriptor.Production;
		Icon = descriptor.Icon;
	}

	public override string ToString()
	{
		return $"[Bonus type: {TargetType}, value: {ResultValue}]";
	}

	public virtual void Add(SkillBonusAbstractClass bonus)
	{
		Value += bonus.Value;
	}

	public virtual ProfileBonusesClass ToDescriptor()
	{
		return new ProfileBonusesClass
		{
			BonusType = BonusType,
			Id = Id,
			Value = Value,
			IsVisible = IsVisible,
			Passive = Passive,
			Production = Production,
			Icon = Icon
		};
	}

	public virtual SkillBonusAbstractClass Clone()
	{
		return ToDescriptor().ToBonus();
	}
}
