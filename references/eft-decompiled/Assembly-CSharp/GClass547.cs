using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass547
{
	[NonSerialized]
	[CompilerGenerated]
	public PathControllerClass PathControllerClass;

	[NonSerialized]
	[CompilerGenerated]
	public EBotMoverState EbotMoverState_0;

	[NonSerialized]
	[CompilerGenerated]
	public BotPathFinderClass BotPathFinderClass;

	[NonSerialized]
	[CompilerGenerated]
	public Vector3? Nullable_0;

	public PathControllerClass PathController
	{
		[CompilerGenerated]
		get
		{
			return PathControllerClass;
		}
	}

	public EBotMoverState State
	{
		[CompilerGenerated]
		get
		{
			return EbotMoverState_0;
		}
	}

	public BotPathFinderClass PathFinder
	{
		[CompilerGenerated]
		get
		{
			return BotPathFinderClass;
		}
	}

	public Vector3? LastTargetPoint
	{
		[CompilerGenerated]
		get
		{
			return Nullable_0;
		}
		[CompilerGenerated]
		set
		{
			Nullable_0 = value;
		}
	}

	public GClass547(BotOwner owner, EBotMoverState state, BotMover botMover, AICoversData covers)
	{
		PathControllerClass = new PathControllerClass(owner);
		BotPathFinderClass = new BotPathFinderClass(owner, botMover, PathController, covers);
		EbotMoverState_0 = state;
	}
}
