using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using JetBrains.Annotations;
using Newtonsoft.Json;

[GAttribute29]
public class GClass2214
{
	[Serializable]
	[CompilerGenerated]
	public class Class1415
	{
		public static readonly Class1415 class1415_0 = new Class1415();

		public static Func<KeyValuePair<EBodyModelPart, MongoID>, EBodyModelPart> func_0;

		public static Func<KeyValuePair<EBodyModelPart, MongoID>, MongoID> func_1;

		public EBodyModelPart method_0(KeyValuePair<EBodyModelPart, MongoID> bodyPert)
		{
			return bodyPert.Key;
		}

		public MongoID method_1(KeyValuePair<EBodyModelPart, MongoID> bodyPart)
		{
			return bodyPart.Value;
		}
	}

	[JsonProperty("Info")]
	public GClass1410 Info;

	[JsonProperty("Customization")]
	[CanBeNull]
	[GAttribute30]
	public Dictionary<EBodyModelPart, MongoID> Customization;

	[JsonProperty("Equipment")]
	[CanBeNull]
	[GAttribute30]
	public GClass2211 Equipment;

	public GClass2214()
	{
	}

	public GClass2214(LastPlayerStateClass playerVisual)
	{
		Info = GClass3694.ClonePolymorph(playerVisual.Info);
		Customization = playerVisual.Customization?.ToDictionary((KeyValuePair<EBodyModelPart, MongoID> bodyPert) => bodyPert.Key, (KeyValuePair<EBodyModelPart, MongoID> bodyPart) => bodyPart.Value);
		if (playerVisual.Equipment != null)
		{
			Equipment = new GClass2211(playerVisual.Equipment);
		}
	}
}
