using System;
using System.Collections.Generic;
using JsonType;
using UnityEngine;

public abstract class GClass1409
{
	[NonSerialized]
	public static Dictionary<TaxonomyColor, Color> Dictionary_0 = new Dictionary<TaxonomyColor, Color>(GClass866<TaxonomyColor>.EqualityComparer)
	{
		{
			TaxonomyColor.blue,
			new Color32(28, 65, 86, byte.MaxValue)
		},
		{
			TaxonomyColor.yellow,
			new Color32(104, 102, 40, byte.MaxValue)
		},
		{
			TaxonomyColor.green,
			new Color32(21, 45, 0, byte.MaxValue)
		},
		{
			TaxonomyColor.red,
			new Color32(109, 36, 24, byte.MaxValue)
		},
		{
			TaxonomyColor.tracerYellow,
			new Color(2f, 1.311f, 0.573f, 1f)
		},
		{
			TaxonomyColor.tracerGreen,
			new Color(0.457f, 1.5f, 0.507f, 1f)
		},
		{
			TaxonomyColor.tracerRed,
			new Color(2f, 0.235f, 0.235f, 1f)
		},
		{
			TaxonomyColor.black,
			new Color32(0, 0, 0, byte.MaxValue)
		},
		{
			TaxonomyColor.grey,
			new Color32(29, 29, 29, byte.MaxValue)
		},
		{
			TaxonomyColor.violet,
			new Color32(76, 42, 85, byte.MaxValue)
		},
		{
			TaxonomyColor.orange,
			new Color32(60, 25, 0, byte.MaxValue)
		},
		{
			TaxonomyColor.@default,
			new Color32(127, 127, 127, byte.MaxValue)
		}
	};

	public static Color ToColor(this TaxonomyColor taxonomyColor)
	{
		return Dictionary_0[taxonomyColor];
	}
}
