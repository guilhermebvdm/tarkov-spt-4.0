using System;
using UnityEngine;

public class HoboCullingManager : ComponentSystem<DisablerCullingObjectBase, HoboCullingManager>
{
	public override bool HasUpdate => true;

	public override bool HasLateUpdate => false;

	public override void UpdateComponent(DisablerCullingObjectBase component)
	{
		component.ManualUpdate(Time.deltaTime);
	}

	public override void LateUpdateComponent(DisablerCullingObjectBase component)
	{
		throw new NotImplementedException();
	}
}
