using System;
using UnityEngine;

public class FloatingObjectManager : ComponentSystem<FloatingObject, FloatingObjectManager>
{
	public override bool HasUpdate => true;

	public override bool HasLateUpdate => false;

	public override void UpdateComponent(FloatingObject component)
	{
		component.ManualUpdate(Time.deltaTime);
	}

	public override void LateUpdateComponent(FloatingObject component)
	{
		throw new NotImplementedException();
	}
}
