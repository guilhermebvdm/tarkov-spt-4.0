using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

public class GClass2198
{
	[Serializable]
	[CompilerGenerated]
	public class Class1410
	{
		public static readonly Class1410 class1410_0 = new Class1410();

		public static Func<GClass2217, GClass2200> func_0;

		public GClass2200 method_0(GClass2217 damage)
		{
			return new GClass2200(damage);
		}
	}

	public EBodyPart LethalDamagePart;

	[CanBeNull]
	public GClass2200 LethalDamage;

	public readonly Dictionary<EBodyPart, List<GClass2200>> BodyParts = new Dictionary<EBodyPart, List<GClass2200>>();

	public GClass2198(GClass2216 descriptor)
	{
		LethalDamagePart = descriptor.LethalDamagePart;
		if (descriptor.LethalDamage != null)
		{
			LethalDamage = new GClass2200(descriptor.LethalDamage);
		}
		foreach (var (key, gClass2) in descriptor.BodyParts)
		{
			BodyParts.Add(key, gClass2.DamageList.Select((GClass2217 damage) => new GClass2200(damage)).ToList());
		}
	}

	public void Clear()
	{
		BodyParts.Clear();
		LethalDamage = null;
		LethalDamagePart = EBodyPart.Common;
	}
}
