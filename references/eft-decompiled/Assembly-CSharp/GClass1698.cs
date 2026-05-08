using System;
using System.Runtime.CompilerServices;

public class GClass1698 : GInterface382
{
	public struct GStruct160
	{
		public bool IsUsing;

		public string ItemId;
	}

	[NonSerialized]
	[CompilerGenerated]
	public GStruct160 Gstruct160_0;

	public GStruct160 Data
	{
		[CompilerGenerated]
		get
		{
			return Gstruct160_0;
		}
		[CompilerGenerated]
		set
		{
			Gstruct160_0 = value;
		}
	}

	public GClass1698()
	{
	}

	public GClass1698(GStruct160 data)
	{
		Data = data;
	}

	public void Serialize(EFTWriterClass writer)
	{
		GClass1290.WriteBool(writer, Data.IsUsing);
		if (Data.IsUsing)
		{
			GClass1290.WriteString(writer, Data.ItemId);
		}
	}

	public void Deserialize(EFTReaderClass reader)
	{
		bool isUsing;
		string itemId = ((isUsing = GClass1285.ReadBool(reader)) ? GClass1285.ReadString(reader) : string.Empty);
		Data = new GStruct160
		{
			IsUsing = isUsing,
			ItemId = itemId
		};
	}
}
