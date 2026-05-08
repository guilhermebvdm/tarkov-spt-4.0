using System;
using System.Runtime.CompilerServices;

public struct Struct273
{
	[NonSerialized]
	[CompilerGenerated]
	public int Int_0;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_1;

	public int Group
	{
		[CompilerGenerated]
		readonly get
		{
			return Int_0;
		}
		[CompilerGenerated]
		set
		{
			Int_0 = value;
		}
	}

	public int Category
	{
		[CompilerGenerated]
		readonly get
		{
			return Int_1;
		}
		[CompilerGenerated]
		set
		{
			Int_1 = value;
		}
	}

	public Struct273(int group, int category)
	{
		this = default(Struct273);
		Group = group;
		Category = category;
	}

	public override bool Equals(object obj)
	{
		if (obj is Struct273 @struct && @struct.Group == Group && @struct.Category == Category)
		{
			return true;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Group.GetHashCode() ^ Category.GetHashCode();
	}
}
