using System.Collections.Generic;
using EFT;

public abstract class GClass1854 : IViewFilter
{
	public abstract HashSet<EBodyModelPart> AllowedParts { get; }

	public GClass2197 FilterCustomization(GClass2197 customization)
	{
		GClass2197 gClass = new GClass2197(customization);
		foreach (EBodyModelPart value in GClass866<EBodyModelPart>.Values)
		{
			if (!AllowedParts.Contains(value))
			{
				gClass.Remove(value);
			}
		}
		return gClass;
	}

	public GClass1854()
	{
	}
}
