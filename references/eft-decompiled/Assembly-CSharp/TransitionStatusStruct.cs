using System;
using System.Runtime.CompilerServices;
using EFT;
using JsonType;

public struct TransitionStatusStruct
{
	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	[NonSerialized]
	[CompilerGenerated]
	public string String_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_1;

	[NonSerialized]
	[CompilerGenerated]
	public ESideType EsideType_0;

	[NonSerialized]
	[CompilerGenerated]
	public ERaidMode EraidMode_0;

	[NonSerialized]
	[CompilerGenerated]
	public EDateTime EdateTime_0;

	public readonly bool InTransition
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
	}

	public readonly string Location
	{
		[CompilerGenerated]
		get
		{
			return String_0;
		}
	}

	public readonly bool Training
	{
		[CompilerGenerated]
		get
		{
			return Bool_1;
		}
	}

	public readonly ESideType Side
	{
		[CompilerGenerated]
		get
		{
			return EsideType_0;
		}
	}

	public readonly ERaidMode Mode
	{
		[CompilerGenerated]
		get
		{
			return EraidMode_0;
		}
	}

	public readonly EDateTime Time
	{
		[CompilerGenerated]
		get
		{
			return EdateTime_0;
		}
	}

	public TransitionStatusStruct(string targetLocation, bool training, ESideType side, ERaidMode mode, EDateTime time)
	{
		Bool_0 = true;
		String_0 = targetLocation;
		Bool_1 = training;
		EsideType_0 = side;
		EraidMode_0 = mode;
		EdateTime_0 = time;
	}
}
