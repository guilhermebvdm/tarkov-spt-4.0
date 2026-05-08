using UnityEngine;

public class MuzzleSystem : ComponentSystem<MuzzleManager, MuzzleSystem>
{
	private Camera camera_0;

	public override bool HasUpdate => true;

	public override bool HasLateUpdate => true;

	public override void UpdateComponent(MuzzleManager component)
	{
		component.ManualUpdate();
	}

	public override void LateUpdate()
	{
		camera_0 = CameraClass.Instance.Camera;
		if (camera_0 == null)
		{
			camera_0 = Camera.main;
		}
		base.LateUpdate();
	}

	public override void LateUpdateComponent(MuzzleManager component)
	{
		component.ManualLateUpdate();
		component.LateUpdateMuzzleEffectsValues(camera_0);
	}
}
