using System;
using System.Runtime.CompilerServices;
using System.Text;
using EFT;

public class GClass2200
{
	public enum EDamageResult
	{
		[GAttribute24("None")]
		None,
		[GAttribute24("Regular")]
		Regular,
		[GAttribute24("Lethal")]
		Lethal
	}

	[NonSerialized]
	[CompilerGenerated]
	public float Float_0;

	public readonly EDamageType Type;

	public readonly string SourceId;

	public readonly EBodyPart? OverDamageFrom;

	public readonly bool Blunt;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_1;

	public float Amount
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

	public float ImpactsCount
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

	public GClass2200(GClass2217 descriptor)
		: this(descriptor.Type, descriptor.Amount, descriptor.OverDamageFrom, descriptor.Blunt, descriptor.SourceId)
	{
		ImpactsCount = descriptor.ImpactsCount;
	}

	public GClass2200(EDamageType type, float amount, EBodyPart? overDamageFrom, bool blunt = false, string sourceId = null)
	{
		Type = type;
		Amount = amount;
		OverDamageFrom = overDamageFrom;
		Blunt = blunt;
		SourceId = sourceId;
		ImpactsCount = 1f;
	}

	public bool IsEqual(GClass2200 damageStats, bool compareShots = false)
	{
		if (!compareShots && !string.IsNullOrEmpty(SourceId))
		{
			return false;
		}
		if (SourceId == damageStats.SourceId && Type == damageStats.Type)
		{
			return OverDamageFrom == damageStats.OverDamageFrom;
		}
		return false;
	}

	public void Add(GClass2200 damage)
	{
		Amount += damage.Amount;
		ImpactsCount += damage.ImpactsCount;
	}

	public void Remove(float amount)
	{
		Amount -= amount;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder(GClass2348.Localized(string.IsNullOrEmpty(SourceId) ? ("DamageType_" + GClass867.ToStringNoBox(Type)) : (SourceId + " Name")));
		if (!string.IsNullOrEmpty(SourceId) && ImpactsCount > 1f)
		{
			stringBuilder.AppendFormat(" ({0})", ImpactsCount);
		}
		if (Blunt)
		{
			stringBuilder.AppendFormat(" <b>({0})</b>", GClass2348.Localized("UI/DamageProperty_Blunt"));
		}
		if (OverDamageFrom.HasValue)
		{
			stringBuilder.AppendFormat(" <b>({0} {1})</b>", GClass2348.Localized("UI/DamageProperty_Collateral"), GClass2348.Localized(OverDamageFrom.ToString()));
		}
		return stringBuilder.ToString();
	}

	public GClass2200 Clone()
	{
		return new GClass2200(new GClass2217(this));
	}
}
