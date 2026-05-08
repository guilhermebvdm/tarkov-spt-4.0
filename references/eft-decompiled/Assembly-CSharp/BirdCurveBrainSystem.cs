using System;
using EFT.Animals;
using UnityEngine;

public class BirdCurveBrainSystem : ComponentSystem<BirdCurveBrain, BirdCurveBrainSystem>
{
	public override bool HasUpdate => true;

	public override bool HasLateUpdate => false;

	public override void UpdateComponent(BirdCurveBrain component)
	{
		component.ManualUpdate(Time.deltaTime);
	}

	public override void LateUpdateComponent(BirdCurveBrain component)
	{
		throw new NotImplementedException();
	}
}
