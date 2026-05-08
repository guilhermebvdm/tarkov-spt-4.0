using System.Collections.Generic;
using Systems.Effects;
using Comfort.Common;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class WeaponOverHeatSystem : ComponentSystem<WeaponPrefab, WeaponOverHeatSystem>
{
	public override bool HasUpdate => true;

	public override bool HasLateUpdate => true;

	public override void LateUpdate()
	{
		if (Singleton<Effects>.Instantiated && CameraClass.Exist && !(CameraClass.Instance.Camera == null) && EFTHardSettings.Instance.HEAT_EMITTER_ENABLED)
		{
			base.LateUpdate();
		}
	}

	public override void UpdateComponent(WeaponPrefab component)
	{
		component.ManualUpdate();
	}

	public override void LateUpdateComponent(WeaponPrefab component)
	{
		bool flag = HotObject.NeedProcessEffects(CameraClass.Instance.Camera.transform.position, component.transform.position);
		List<HotObject> hotObjects = component.HotObjects;
		for (int i = 0; i < hotObjects.Count; i++)
		{
			HotObject hotObject = hotObjects[i];
			if (hotObject.UseHeatHaze && flag)
			{
				hotObject.ManualSyncEffects();
				hotObject.SyncPosition();
			}
		}
	}
}
