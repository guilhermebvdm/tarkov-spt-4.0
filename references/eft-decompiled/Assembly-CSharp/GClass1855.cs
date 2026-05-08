using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;

public class GClass1855 : GClass1854
{
	public static readonly GClass1855 Default = new GClass1855();

	[NonSerialized]
	[CompilerGenerated]
	public HashSet<EBodyModelPart> HashSet_0 = new HashSet<EBodyModelPart>
	{
		EBodyModelPart.Body,
		EBodyModelPart.Feet,
		EBodyModelPart.Head,
		EBodyModelPart.Hands
	};

	public override HashSet<EBodyModelPart> AllowedParts
	{
		[CompilerGenerated]
		get
		{
			return HashSet_0;
		}
	}
}
