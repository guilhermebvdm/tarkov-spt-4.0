using System;
using System.Runtime.CompilerServices;
using EFT;
using Newtonsoft.Json;

public class GClass1852
{
	[NonSerialized]
	[CompilerGenerated]
	public MongoID MongoID_0;

	[NonSerialized]
	[CompilerGenerated]
	public ECustomizationType EcustomizationType_0;

	[NonSerialized]
	[CompilerGenerated]
	public ECustomizationSource EcustomizationSource_0;

	[JsonProperty("id")]
	public MongoID Id
	{
		[CompilerGenerated]
		get
		{
			return MongoID_0;
		}
	}

	[JsonProperty("type")]
	public ECustomizationType Type
	{
		[CompilerGenerated]
		get
		{
			return EcustomizationType_0;
		}
	}

	[JsonProperty("source")]
	public ECustomizationSource Source
	{
		[CompilerGenerated]
		get
		{
			return EcustomizationSource_0;
		}
	}

	[JsonConstructor]
	public GClass1852([JsonProperty("id")] MongoID id, [JsonProperty("type")] ECustomizationType type, [JsonProperty("source")] ECustomizationSource source = ECustomizationSource.UnlockedInGame)
	{
		MongoID_0 = id;
		EcustomizationType_0 = type;
		EcustomizationSource_0 = source;
	}

	public override bool Equals(object otherObject)
	{
		if (otherObject is GClass1852 gClass)
		{
			return Id.Equals(gClass.Id);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Id.GetHashCode();
	}
}
