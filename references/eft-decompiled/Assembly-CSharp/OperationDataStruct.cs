using System;
using System.Runtime.CompilerServices;
using Diz.LanguageExtensions;
using JetBrains.Annotations;

public readonly struct OperationDataStruct : IInventoryEventResult
{
	public readonly BaseInventoryOperationClass Value;

	[NonSerialized]
	[CompilerGenerated]
	public Error Error_0;

	[CanBeNull]
	public Error Error
	{
		[CompilerGenerated]
		get
		{
			return Error_0;
		}
	}

	public bool Succeeded => Error == null;

	public bool Failed => Error != null;

	public OperationDataStruct(BaseInventoryOperationClass value)
	{
		Value = value;
		Error_0 = null;
	}

	public OperationDataStruct(Error error)
	{
		Value = null;
		Error_0 = error;
	}

	public static implicit operator OperationDataStruct(BaseInventoryOperationClass value)
	{
		return new OperationDataStruct(value);
	}

	public static implicit operator OperationDataStruct(Error error)
	{
		return new OperationDataStruct(error);
	}
}
