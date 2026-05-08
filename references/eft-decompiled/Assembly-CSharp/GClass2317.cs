using System;
using System.Collections.Generic;
using System.Linq;
using EFT;
using EFT.Hideout;
using Newtonsoft.Json;
using UnityEngine;

public class GClass2317 : ISerializer<AreaTemplate>
{
	[JsonProperty("_id")]
	public string Id;

	[JsonProperty("type")]
	public int Type;

	[JsonProperty("needsFuel")]
	public bool NeedsFuel;

	[JsonProperty("craftGivesExp")]
	public bool CraftGivesExp;

	[JsonProperty("takeFromSlotLocked")]
	public bool TakeFromSlotLocked;

	[JsonProperty("requirements")]
	public Requirement[] Requirements;

	[JsonProperty("stages")]
	public Dictionary<int, Stage> Stages = new Dictionary<int, Stage>();

	[JsonProperty("enabled")]
	public bool Enabled;

	[JsonProperty("displayLevel")]
	public bool DisplayLevel;

	[JsonProperty("parentArea")]
	public string ParentAreaId;

	public AreaTemplate Deserialize()
	{
		AreaTemplate areaTemplate = ScriptableObject.CreateInstance<AreaTemplate>();
		areaTemplate.Id = Id;
		areaTemplate.Type = (EAreaType)Type;
		areaTemplate.NeedsFuel = NeedsFuel;
		areaTemplate.CraftGivesExp = CraftGivesExp;
		areaTemplate.Stages = Stages;
		areaTemplate.Enabled = Enabled;
		areaTemplate.DisplayLevel = DisplayLevel;
		areaTemplate.TakeFromSlotLocked = TakeFromSlotLocked;
		areaTemplate.ParentAreaId = ParentAreaId;
		Requirement[] array = Requirements ?? Array.Empty<Requirement>();
		areaTemplate.Requirements = new RelatedRequirements
		{
			Data = array.ToList()
		};
		Requirement[] array2 = array;
		int i;
		for (i = 0; i < array2.Length; i++)
		{
			array2[i].Init(areaTemplate.Type, 0);
		}
		foreach (KeyValuePair<int, Stage> stage2 in Stages)
		{
			stage2.Deconstruct(out i, out var value);
			int sourceAreaLevel = i;
			Stage stage = value;
			foreach (Requirement requirement in stage.Requirements)
			{
				requirement.Init(areaTemplate.Type, sourceAreaLevel);
			}
			foreach (GClass2427 improvement in stage.Improvements)
			{
				array2 = improvement.Requirements;
				for (i = 0; i < array2.Length; i++)
				{
					array2[i].Init(areaTemplate.Type, sourceAreaLevel);
				}
			}
		}
		return areaTemplate;
	}

	public object Serialize(AreaTemplate template)
	{
		Id = template.Id;
		Type = (int)template.Type;
		NeedsFuel = template.NeedsFuel;
		CraftGivesExp = template.CraftGivesExp;
		Stages = template.Stages;
		Enabled = template.Enabled;
		DisplayLevel = template.DisplayLevel;
		TakeFromSlotLocked = template.TakeFromSlotLocked;
		Requirements = template.Requirements.Data.ToArray();
		ParentAreaId = template.ParentAreaId;
		return this;
	}
}
