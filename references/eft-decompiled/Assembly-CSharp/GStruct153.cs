using System;
using System.Runtime.CompilerServices;
using Diz.LanguageExtensions;
using JetBrains.Annotations;

public readonly struct GStruct153 : IInventoryEventResult
{
	public readonly IRaiseEvents Value;

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

	public GStruct153(IRaiseEvents value)
	{
		Value = value;
		Error_0 = null;
	}

	public GStruct153(Error error)
	{
		Value = null;
		Error_0 = error;
	}

	public static implicit operator GStruct153(Error error)
	{
		return new GStruct153(error);
	}
}
