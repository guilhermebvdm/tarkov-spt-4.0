using System;
using System.Runtime.CompilerServices;
using EFT;

[GAttribute29]
public abstract class BaseDescriptorClass : GInterface192
{
	[NonSerialized]
	[CompilerGenerated]
	public ushort Ushort_0;

	[NonSerialized]
	[CompilerGenerated]
	public MongoID MongoID_0;

	public ushort OperationId
	{
		[CompilerGenerated]
		get
		{
			return Ushort_0;
		}
		[CompilerGenerated]
		set
		{
			Ushort_0 = value;
		}
	}

	public MongoID OwnerId
	{
		[CompilerGenerated]
		get
		{
			return MongoID_0;
		}
		[CompilerGenerated]
		set
		{
			MongoID_0 = value;
		}
	}

	public GInterface192 Operation
	{
		set
		{
			OperationId = value.OperationId;
			OwnerId = value.OwnerId;
		}
	}

	public abstract GStruct152<BaseInventoryOperationClass> ToInventoryOperation(IPlayer player);

	public override string ToString()
	{
		return GetType().ToString();
	}

	public BaseDescriptorClass()
	{
	}
}
