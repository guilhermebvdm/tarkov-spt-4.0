using System;
using EFT;
using EFT.Hideout;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class GClass2318 : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(Requirement);
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		throw new InvalidOperationException("Use default serialization.");
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		JObject jObject = JObject.Load(reader);
		string text = jObject["type"]?.Value<string>();
		if (!Enum.TryParse<ERequirementType>(text, out var result))
		{
			throw new ArgumentException("Can't parse '" + text + "'");
		}
		Requirement requirement;
		switch (result)
		{
		default:
			throw new NotSupportedException($"Type {result} is not supported!");
		case ERequirementType.Area:
		{
			string value = jObject["areaType"]?.Value<string>();
			requirement = new AreaRequirement
			{
				AreaType = (EAreaType)Enum.Parse(typeof(EAreaType), value)
			};
			break;
		}
		case ERequirementType.Item:
			requirement = new ItemRequirement();
			break;
		case ERequirementType.TraderUnlock:
			requirement = new TraderUnlockRequirement();
			break;
		case ERequirementType.TraderLoyalty:
			requirement = new TraderLoyaltyRequirement();
			break;
		case ERequirementType.Skill:
			requirement = new SkillRequirement();
			break;
		case ERequirementType.Resource:
			requirement = new ResourceRequirement();
			break;
		case ERequirementType.Tool:
			requirement = new ToolRequirement();
			break;
		case ERequirementType.QuestComplete:
			requirement = new QuestRequirement();
			break;
		case ERequirementType.Health:
			requirement = new HealthRequirement();
			break;
		case ERequirementType.BodyPartBuff:
			requirement = new BodyPartBuffRequirement();
			break;
		case ERequirementType.GameVersion:
			requirement = new GameVersionRequirement();
			break;
		}
		serializer.Populate(jObject.CreateReader(), requirement);
		return requirement;
	}
}
