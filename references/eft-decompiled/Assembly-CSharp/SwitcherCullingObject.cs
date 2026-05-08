using EFT.Interactive;
using UnityEngine;

[ExecuteInEditMode]
public class SwitcherCullingObject : DisablerCullingObject
{
	[Header("Синяя стрелка (ось Z) коллайдеров должна указывать на вход в помещение")]
	[Header("Первая проверка делается по радиусу, дальше по коллайдерам")]
	[SerializeField]
	private float _firstCheckRadius = 10f;

	private bool bool_1 = true;

	private ColliderReporter colliderReporter_0;

	private Vector3 vector3_0;

	private bool bool_2;

	public override bool HasEntered => bool_2;

	public override void ManualUpdate(float dt)
	{
		base.ManualUpdate(dt);
		if (bool_1 && method_4())
		{
			bool_1 = false;
		}
	}

	public bool method_4()
	{
		Camera camera = method_5();
		if (camera != null)
		{
			float num = Vector3.Distance(camera.transform.position, base.transform.position);
			bool_2 = num < _firstCheckRadius;
			UpdateComponentsStatusOnUpdate();
			return true;
		}
		return false;
	}

	public override void OnTriggerEnterEvent(ColliderReporter colliderReporter, Collider collider)
	{
		if (colliderReporter_0 == null && IsRightCollider(collider))
		{
			Vector3 position = collider.transform.position;
			colliderReporter_0 = colliderReporter;
			vector3_0 = position;
		}
	}

	public override void OnTriggerExitEvent(ColliderReporter colliderReporter, Collider collider)
	{
		if (colliderReporter_0 != null && IsRightCollider(collider))
		{
			bool flag;
			if ((flag = Vector3.Dot(collider.transform.position - vector3_0, colliderReporter_0.transform.forward) > 0f) != bool_2)
			{
				bool_2 = flag;
				UpdateComponentsStatusOnUpdate();
			}
			colliderReporter_0 = null;
		}
	}

	public Camera method_5()
	{
		Camera result = null;
		if (CameraClass.Instance.Camera != null)
		{
			return CameraClass.Instance.Camera;
		}
		return result;
	}
}
