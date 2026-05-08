using System;
using UnityEngine;

public class BaseLightSystem : ComponentSystem<BaseLight, BaseLightSystem>
{
	public override bool HasUpdate => true;

	public override bool HasLateUpdate => false;

	public override void UpdateComponent(BaseLight component)
	{
		component.ManualUpdate(Time.deltaTime);
	}

	public override void LateUpdateComponent(BaseLight component)
	{
		throw new NotImplementedException();
	}
}
