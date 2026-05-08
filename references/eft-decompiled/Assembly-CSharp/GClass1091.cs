using System;
using Bsg.GameSettings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class GClass1091 : JsonConverter<IGameSetting>
{
	public override void WriteJson(JsonWriter writer, IGameSetting value, JsonSerializer serializer)
	{
		object objectValue = value.ObjectValue;
		if (objectValue != null)
		{
			JToken.FromObject(objectValue, serializer).WriteTo(writer);
		}
		else
		{
			JValue.CreateNull().WriteTo(writer);
		}
	}

	public override IGameSetting ReadJson(JsonReader reader, Type objectType, IGameSetting existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		if (!hasExistingValue)
		{
			throw new ArgumentException("GameSettingsJsonConverter can work only with existingValue");
		}
		existingValue.ObjectValue = serializer.Deserialize(reader, existingValue.ObjectType);
		return existingValue;
	}
}
