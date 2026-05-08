using System;
using EFT;
using EFT.Interactive;
using UnityEngine;

public class BotLootOpener : GClass429
{
	public const float DIR_SIDE_MAX_SQRT = 4f;

	[NonSerialized]
	public const float CHECK_RUN_DIST = 27.039997f;

	[NonSerialized]
	public const float MAX_DIST = 49f;

	[NonSerialized]
	public const float OPEN_DIST = 16f;

	[NonSerialized]
	public LootableContainer CurLoot;

	[NonSerialized]
	public float NextPosibleDoorOpenTime;

	[NonSerialized]
	public float TraversingEnd;

	[NonSerialized]
	public float DeltaTime = 12.2f;

	[NonSerialized]
	public float SearchCloseDoorTime;

	[NonSerialized]
	public bool TraversingLink;

	public BotLootOpener(BotOwner owner)
		: base(owner)
	{
		BotOwner_0 = owner;
	}

	public void Interact(LootableContainer door, EInteractionType Etype)
	{
		if (TraversingEnd < Time.time)
		{
			TraversingLink = true;
			TraversingEnd = Time.time + 2.5f;
			InteractionResult interactionResult = new InteractionResult(Etype);
			BotOwner_0.GetPlayer.CurrentManagedState.StartDoorInteraction(door, interactionResult, null);
		}
	}
}
