using System;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using EFT.HealthSystem;

public class GClass2182 : GInterface381
{
	[Serializable]
	[CompilerGenerated]
	public class Class1409
	{
		public static readonly Class1409 class1409_0 = new Class1409();

		public static Func<Profile.ProfileHealthClass.ProfileBodyPartHealthClass, float> func_0;

		public static Func<Profile.ProfileHealthClass.ProfileBodyPartHealthClass, float> func_1;

		public float method_0(Profile.ProfileHealthClass.ProfileBodyPartHealthClass part)
		{
			return part.Health.Current;
		}

		public float method_1(Profile.ProfileHealthClass.ProfileBodyPartHealthClass part)
		{
			return part.Health.Maximum;
		}
	}

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
	public Profile.ProfileHealthClass ProfileHealthClass;

	[NonSerialized]
	public ValueStruct ValueStruct_4;

	public ValueStruct Hydration
	{
		[CompilerGenerated]
		get
		{
			return ValueStruct_0;
		}
	}

	public ValueStruct Energy
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

	public ValueStruct GetBodyPartHealth(EBodyPart bodyPart, bool rounded = false)
	{
		return new ValueStruct
		{
			Current = ProfileHealthClass.BodyParts.Values.Sum((Profile.ProfileHealthClass.ProfileBodyPartHealthClass part) => part.Health.Current),
			Maximum = ProfileHealthClass.BodyParts.Values.Sum((Profile.ProfileHealthClass.ProfileBodyPartHealthClass part) => part.Health.Maximum)
		};
	}

	public GClass2182(Profile.ProfileHealthClass healthInfo)
	{
		ProfileHealthClass = healthInfo;
		ValueStruct_0 = new ValueStruct
		{
			Current = healthInfo.Hydration.Current,
			Maximum = healthInfo.Hydration.Maximum
		};
		ValueStruct_1 = new ValueStruct
		{
			Current = healthInfo.Energy.Current,
			Maximum = healthInfo.Energy.Maximum
		};
		ValueStruct_2 = new ValueStruct
		{
			Current = healthInfo.Temperature.Current,
			Maximum = healthInfo.Temperature.Maximum
		};
		ValueStruct_3 = new ValueStruct
		{
			Current = healthInfo.Poison.Current,
			Maximum = healthInfo.Poison.Maximum
		};
	}
}
