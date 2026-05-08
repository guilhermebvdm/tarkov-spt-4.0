using System;
using System.Runtime.CompilerServices;

public struct GStruct261 : GInterface216<GStruct261>, GInterface217<GStruct261>
{
	public byte SkillId;

	public float Value;

	public float Effectiveness;

	public float PointsEarned;

	[NonSerialized]
	[CompilerGenerated]
	public GInterface217<GStruct261> Ginterface217_0;

	public GInterface217<GStruct261> Nested
	{
		[CompilerGenerated]
		readonly get
		{
			return Ginterface217_0;
		}
		[CompilerGenerated]
		set
		{
			Ginterface217_0 = value;
		}
	}

	public bool TryUpdate(GStruct261 source)
	{
		if (SkillId != source.SkillId)
		{
			return false;
		}
		Value = source.Value;
		Effectiveness = source.Effectiveness;
		PointsEarned = source.PointsEarned;
		return true;
	}

	bool GInterface216<GStruct261>.TryUpdate(GStruct261 source)
	{
		//ILSpy generated this explicit interface implementation from .override directive in TryUpdate
		return this.TryUpdate(source);
	}
}
