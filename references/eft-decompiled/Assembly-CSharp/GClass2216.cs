using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Newtonsoft.Json;

[GAttribute29]
public class GClass2216
{
	[Serializable]
	[CompilerGenerated]
	public class Class1417
	{
		public static readonly Class1417 class1417_0 = new Class1417();

		public static Func<KeyValuePair<EBodyPart, GClass2215>, EBodyPart> func_0;

		public static Func<KeyValuePair<EBodyPart, GClass2215>, List<GClass2217>> func_1;

		public static Func<KeyValuePair<EBodyPart, List<GClass2217>>, EBodyPart> func_2;

		public static Func<KeyValuePair<EBodyPart, List<GClass2217>>, GClass2215> func_3;

		public EBodyPart method_0(KeyValuePair<EBodyPart, GClass2215> pair)
		{
			return pair.Key;
		}

		public List<GClass2217> method_1(KeyValuePair<EBodyPart, GClass2215> pair)
		{
			return pair.Value.DamageList;
		}

		public EBodyPart method_2(KeyValuePair<EBodyPart, List<GClass2217>> pair)
		{
			return pair.Key;
		}

		public GClass2215 method_3(KeyValuePair<EBodyPart, List<GClass2217>> pair)
		{
			return new GClass2215(pair.Value);
		}
	}

	[JsonProperty("LethalDamagePart")]
	public EBodyPart LethalDamagePart;

	[JsonProperty("LethalDamage")]
	[GAttribute30]
	public GClass2217 LethalDamage;

	[JsonIgnore]
	public Dictionary<EBodyPart, GClass2215> BodyParts = new Dictionary<EBodyPart, GClass2215>();

	[JsonProperty("BodyParts")]
	[GAttribute31]
	public Dictionary<EBodyPart, List<GClass2217>> Dictionary_0 = new Dictionary<EBodyPart, List<GClass2217>>();

	public GClass2216()
	{
	}

	public GClass2216(GClass2198 damageHistory)
	{
		LethalDamagePart = damageHistory.LethalDamagePart;
		if (damageHistory.LethalDamage != null)
		{
			LethalDamage = new GClass2217(damageHistory.LethalDamage);
		}
		foreach (var (key, bodyPartDamage) in damageHistory.BodyParts)
		{
			BodyParts.Add(key, new GClass2215(bodyPartDamage));
		}
	}

	[OnSerializing]
	public void method_0(StreamingContext context)
	{
		Dictionary_0 = BodyParts.ToDictionary((KeyValuePair<EBodyPart, GClass2215> pair) => pair.Key, (KeyValuePair<EBodyPart, GClass2215> pair) => pair.Value.DamageList);
	}

	[OnDeserialized]
	public void method_1(StreamingContext context)
	{
		BodyParts = Dictionary_0.ToDictionary((KeyValuePair<EBodyPart, List<GClass2217>> pair) => pair.Key, (KeyValuePair<EBodyPart, List<GClass2217>> pair) => new GClass2215(pair.Value));
	}
}
