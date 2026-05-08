using System;
using System.Runtime.CompilerServices;
using Diz.LanguageExtensions;
using JetBrains.Annotations;

public readonly struct GStruct155 : IInventoryEventResult
{
	[NonSerialized]
	[CompilerGenerated]
	public Error Error_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	[CanBeNull]
	public Error Error
	{
		[CompilerGenerated]
		get
		{
			return Error_0;
		}
	}

	public bool Succeeded => !Failed;

	public bool Failed
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
	}

	public GStruct155(Error error)
	{
		Error_0 = error;
		Bool_0 = true;
	}

	public static implicit operator GStruct155(Error error)
	{
		return new GStruct155(error);
	}
}
