using System;
using EFT;
using UnityEngine;

public class GClass285(BotOwner bot) : GClass177<GClass26>(bot)
{
	[NonSerialized]
	public BifacialTransform BifacialTransform_0;

	[NonSerialized]
	public Vector3 Vector3_0 = Vector3.up;

	[NonSerialized]
	public bool Bool_0;

	public override void Awake()
	{
		BifacialTransform_0 = BotOwner_0.Transform;
		Bool_0 = false;
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		PlaceForCheck currentPlaceForCheck = BotOwner_0.Memory.CurrentPlaceForCheck;
		if (currentPlaceForCheck == null)
		{
			return;
		}
		Vector3 position = currentPlaceForCheck.Position;
		Vector3 vector = position + Vector3_0 - BifacialTransform_0.position;
		if (!Physics.Raycast(BifacialTransform_0.position, vector, vector.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
		{
			if (Vector3.Dot(BotOwner_0.GetPlayer.LookDirection, vector) > 0.8f)
			{
				BotOwner_0.BotsGroup.PlacesForCheck.Remove(currentPlaceForCheck);
				Bool_0 = false;
			}
			else
			{
				BotOwner_0.Steering.LookToPoint(position + Vector3_0);
			}
		}
		else if (!Bool_0)
		{
			BotOwner_0.GoToPoint(position);
			Bool_0 = true;
		}
	}
}
