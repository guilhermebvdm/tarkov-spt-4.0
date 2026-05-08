using System;
using System.Runtime.CompilerServices;
using Diz.LanguageExtensions;
using JetBrains.Annotations;

public readonly struct GStruct154<T> : IInventoryEventResult where T : IRaiseEvents
{
	public readonly T Value;

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

	public GStruct154(T value)
	{
		Value = value;
		Error_0 = null;
	}

	public GStruct154(Error error)
	{
		Value = default(T);
		Error_0 = error;
	}

	public static implicit operator GStruct154<T>(Error error)
	{
		return new GStruct154<T>(error);
	}

	public static implicit operator GStruct154<T>(T value)
	{
		return new GStruct154<T>(value);
	}

	public static implicit operator GStruct153(GStruct154<T> value)
	{
		if (!value.Succeeded)
		{
			return new GStruct153(value.Error);
		}
		return new GStruct153(value.Value);
	}
}
