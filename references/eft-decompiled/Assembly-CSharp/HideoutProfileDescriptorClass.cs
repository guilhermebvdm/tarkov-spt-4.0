using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using EFT;
using EFT.Hideout;
using Newtonsoft.Json;

public class HideoutProfileDescriptorClass
{
	[JsonProperty("Production")]
	public Dictionary<string, HideoutProductionsDataClass> Production = new Dictionary<string, HideoutProductionsDataClass>();

	[JsonProperty("Improvements")]
	public Dictionary<string, HideoutImprovementsDataClass> Improvements = new Dictionary<string, HideoutImprovementsDataClass>();

	[JsonProperty("Areas")]
	public GClass2204[] Areas = Array.Empty<GClass2204>();

	[JsonProperty("Seed")]
	public string RawSeed;

	[JsonProperty("Customization")]
	public Dictionary<EHideoutCustomizationType, MongoID?> GlobalCustomization;

	[JsonProperty("MannequinPoses")]
	public Dictionary<string, MongoID> MannequinPoses = new Dictionary<string, MongoID>();

	[NonSerialized]
	[CompilerGenerated]
	public HideoutSeedDataStruct Gstruct259_0;

	[JsonIgnore]
	public HideoutSeedDataStruct Seed
	{
		[CompilerGenerated]
		get
		{
			return Gstruct259_0;
		}
		[CompilerGenerated]
		set
		{
			Gstruct259_0 = value;
		}
	}

	[OnDeserialized]
	public void method_0(StreamingContext context)
	{
		if (string.IsNullOrEmpty(RawSeed))
		{
			Seed = default(HideoutSeedDataStruct);
			return;
		}
		byte[] array = GClass1371.FromHexString(RawSeed);
		int w = BitConverter.ToInt32(array);
		int z = BitConverter.ToInt32(array, 4);
		int y = BitConverter.ToInt32(array, 8);
		int x = BitConverter.ToInt32(array, 12);
		Seed = new HideoutSeedDataStruct(x, y, z, w);
	}
}
