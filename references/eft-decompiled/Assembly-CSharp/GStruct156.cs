using System;
using System.Runtime.CompilerServices;
using Diz.LanguageExtensions;
using JetBrains.Annotations;

public readonly struct GStruct156<T> : IInventoryEventResult
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

	public GStruct156(T value)
	{
		Value = value;
		Error_0 = null;
	}

	public GStruct156(Error error)
	{
		Value = default(T);
		Error_0 = error;
	}

	public static implicit operator GStruct156<T>(Error error)
	{
		return new GStruct156<T>(error);
	}

	public static implicit operator GStruct156<T>(T value)
	{
		return new GStruct156<T>(value);
	}

	public GStruct156<TOut> Cast<TOut>(Func<T, TOut> selector)
	{
		Error error = Error;
		if (error == null)
		{
			return new GStruct156<TOut>(selector(Value));
		}
		return error;
	}
}
