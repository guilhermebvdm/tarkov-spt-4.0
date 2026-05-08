using System;
using System.Runtime.CompilerServices;
using Diz.LanguageExtensions;
using JetBrains.Annotations;

public readonly struct GStruct152<T> : IInventoryEventResult where T : BaseInventoryOperationClass
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

	public GStruct152(BaseInventoryOperationClass value)
	{
		Value = value;
		Error_0 = null;
	}

	public GStruct152(Error error)
	{
		Value = null;
		Error_0 = error;
	}

	public static implicit operator GStruct152<T>(BaseInventoryOperationClass value)
	{
		return new GStruct152<T>(value);
	}

	public static implicit operator OperationDataStruct(GStruct152<T> value)
	{
		if (!value.Succeeded)
		{
			return new OperationDataStruct(value.Error);
		}
		return new OperationDataStruct(value.Value);
	}

	public static implicit operator GStruct152<T>(Error error)
	{
		return new GStruct152<T>(error);
	}
}
