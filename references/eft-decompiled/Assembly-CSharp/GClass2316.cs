using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using EFT.Hideout;
using Newtonsoft.Json;

public class GClass2316 : ISerializer<Stage>
{
	[Serializable]
	[CompilerGenerated]
	public class Class1535
	{
		public static readonly Class1535 class1535_0 = new Class1535();

		public static Func<ProfileBonusesClass, SkillBonusAbstractClass> func_0;

		public static Func<SkillBonusAbstractClass, ProfileBonusesClass> func_1;

		public SkillBonusAbstractClass method_0(ProfileBonusesClass bonus)
		{
			return bonus.ToBonus();
		}

		public ProfileBonusesClass method_1(SkillBonusAbstractClass bonus)
		{
			return bonus.ToDescriptor();
		}
	}

	[JsonProperty("requirements")]
	public Requirement[] Requirement_0;

	[JsonProperty("bonuses")]
	public ProfileBonusesClass[] Gclass2228_0;

	[JsonProperty("constructionTime")]
	public float Float_0;

	[JsonProperty("boost")]
	public Requirement[] Requirement_1;

	[JsonProperty("fuelSupply")]
	public HideoutControllerClass[] Gclass2450_0;

	[JsonProperty("improvements")]
	public GClass2427[] Gclass2427_0;

	[JsonProperty("startTime")]
	public double Double_0;

	[JsonProperty("autoUpgrade")]
	public bool Bool_0;

	[JsonProperty("displayInterface")]
	public bool Bool_1;

	[NonSerialized]
	public List<SkillBonusAbstractClass> List_0;

	public Stage Deserialize()
	{
		Requirement[] source = Requirement_0 ?? Array.Empty<Requirement>();
		ProfileBonusesClass[] gclass2228_ = Gclass2228_0;
		object obj;
		if (gclass2228_ == null)
		{
			obj = null;
		}
		else
		{
			obj = gclass2228_.Select((ProfileBonusesClass bonus) => bonus.ToBonus()).ToList();
			if (obj != null)
			{
				goto IL_0050;
			}
		}
		obj = new List<SkillBonusAbstractClass>();
		goto IL_0050;
		IL_0050:
		List_0 = (List<SkillBonusAbstractClass>)obj;
		Requirement[] source2 = Requirement_1 ?? Array.Empty<Requirement>();
		HideoutControllerClass[] source3 = Gclass2450_0 ?? Array.Empty<HideoutControllerClass>();
		GClass2427[] source4 = Gclass2427_0 ?? Array.Empty<GClass2427>();
		return new Stage
		{
			Requirements = new RelatedRequirements
			{
				Data = source.ToList()
			},
			Bonuses = new RelatedBonuses
			{
				Data = List_0
			},
			ConstructionTime = new RelatedConstructionTime
			{
				Data = Float_0
			},
			Boost = new RelatedBoost
			{
				Data = source2.ToList()
			},
			FuelSupply = new RelatedFuelSupply
			{
				Data = source3.ToList()
			},
			Improvements = new RelatedImprovement
			{
				Data = source4.ToList()
			},
			StartTime = EFTDateTimeClass.UniversalDateTimeFromUnixTime(Double_0),
			AutoUpgrade = Bool_0,
			DisplayInterface = Bool_1
		};
	}

	public object Serialize(Stage stage)
	{
		Requirement_0 = stage.Requirements.Data.ToArray();
		RelatedBonuses bonuses = stage.Bonuses;
		object obj;
		if (bonuses == null)
		{
			obj = null;
		}
		else
		{
			obj = bonuses.Data.Select((SkillBonusAbstractClass bonus) => bonus.ToDescriptor()).ToArray();
			if (obj != null)
			{
				goto IL_005b;
			}
		}
		obj = Array.Empty<ProfileBonusesClass>();
		goto IL_005b;
		IL_005b:
		Gclass2228_0 = (ProfileBonusesClass[])obj;
		Float_0 = stage.ConstructionTime.Data;
		Requirement_1 = stage.Boost.Data.ToArray();
		Gclass2450_0 = stage.FuelSupply.Data.ToArray();
		Double_0 = EFTDateTimeClass.ToUnixTime(stage.StartTime);
		Bool_0 = stage.AutoUpgrade;
		Bool_1 = stage.DisplayInterface;
		return this;
	}
}
