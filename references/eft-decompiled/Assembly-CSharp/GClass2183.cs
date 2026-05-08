using System;
using EFT;

public class GClass2183
{
	public readonly MongoID TemplateId;

	[NonSerialized]
	public int Int_0;

	public EWishlistGroup Group
	{
		get
		{
			return (EWishlistGroup)Int_0;
		}
		set
		{
			Int_0 = (int)value;
		}
	}

	public GClass2183(MongoID templateId, EWishlistGroup group)
	{
		TemplateId = templateId;
		Int_0 = (int)group;
	}
}
