using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[GAttribute29]
public class GClass2215
{
	[Serializable]
	[CompilerGenerated]
	public class Class1416
	{
		public static readonly Class1416 class1416_0 = new Class1416();

		public static Func<GClass2200, GClass2217> func_0;

		public GClass2217 method_0(GClass2200 damage)
		{
			return new GClass2217(damage);
		}
	}

	public List<GClass2217> DamageList = new List<GClass2217>();

	public GClass2215()
	{
	}

	public GClass2215(IEnumerable<GClass2200> bodyPartDamage)
	{
		DamageList.AddRange(bodyPartDamage.Select((GClass2200 damage) => new GClass2217(damage)));
	}

	public GClass2215(IEnumerable<GClass2217> bodyPartDamage)
	{
		DamageList.AddRange(bodyPartDamage);
	}
}
