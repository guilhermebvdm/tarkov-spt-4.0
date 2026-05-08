using System.Collections;
using UnityEngine;

public class HeatHazeEmitter : MonoBehaviour
{
	public Vector3 Bounds = new Vector3(1f, 1f, 1f);

	public Vector3 Center;

	[GAttribute6(0f, 2f, -1f)]
	public Vector2 LifetimeDelta = new Vector2(1f, 2f);

	[GAttribute6(0.01f, 1f, -1f)]
	public Vector2 Size = new Vector2(1f, 2f);

	public int Count = 5;

	public float VelocityFactor = 10f;

	public float Angle = 0.04f;

	public float DelayInSec = 0.1f;

	[HideInInspector]
	public float Chance = 1f;

	public void OnShot(GInterface50 emitter)
	{
		if (EFTHardSettings.Instance.HEAT_EMITTER_ENABLED && Count > 0 && !(Random.Range(0.001f, 1f) > Chance) && base.gameObject.activeInHierarchy)
		{
			StartCoroutine(method_0(emitter));
		}
	}

	public IEnumerator method_0(GInterface50 emitter)
	{
		yield return new WaitForSeconds(DelayInSec);
		method_1(emitter);
	}

	public void method_1(GInterface50 emitter)
	{
		float num = Random.Range(Size.x, Size.y);
		for (int i = 0; i < Count; i++)
		{
			Vector3 velocity = base.transform.rotation * Quaternion.Inverse(base.transform.localRotation) * base.transform.localPosition + Random.insideUnitSphere * Angle;
			velocity *= VelocityFactor;
			int num2 = Random.Range(0, 90);
			Vector3 position = base.transform.position;
			position += velocity.normalized * num * 0.5f;
			float lifetime = Random.Range(LifetimeDelta.x, LifetimeDelta.y);
			emitter.Emit(position, num2, velocity, num, lifetime, Color.white);
		}
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.DrawWireSphere(base.transform.position, Size.y);
	}
}
