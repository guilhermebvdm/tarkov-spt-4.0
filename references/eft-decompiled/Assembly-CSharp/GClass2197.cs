using System.Collections.Generic;
using EFT;

public class GClass2197 : Dictionary<EBodyModelPart, MongoID>
{
	public GClass2197()
	{
	}

	public GClass2197(Dictionary<EBodyModelPart, MongoID> original)
		: base((IDictionary<EBodyModelPart, MongoID>)original)
	{
	}

	public GClass2197 GetPartsWithVisual()
	{
		GClass2197 gClass = new GClass2197();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<EBodyModelPart, MongoID> current = enumerator.Current;
			if (current.Key != EBodyModelPart.DogTag && current.Key != EBodyModelPart.Voice)
			{
				gClass.Add(current.Key, current.Value);
			}
		}
		return gClass;
	}
}
