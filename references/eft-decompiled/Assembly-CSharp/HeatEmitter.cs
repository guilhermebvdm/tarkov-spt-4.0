using Systems.Effects;
using Comfort.Common;
using UnityEngine;

public class HeatEmitter : MonoBehaviour
{
	public Vector3 Bounds = new Vector3(1f, 1f, 1f);

	public Vector3 Center;

	[GAttribute6(0f, 3f, -1f)]
	public Vector2 LifetimeDelta = new Vector2(1f, 2f);

	[GAttribute6(0.01f, 1f, -1f)]
	public Vector2 Size = new Vector2(1f, 2f);

	public float IrradiationRate = 0.016f;

	public float HeatIncreasePerShot = 0.006f;

	public AnimationCurve CountByHeat = new AnimationCurve(new Keyframe(0f, 10f), new Keyframe(1f, 10f));

	public AnimationCurve LifeTimeByHeat = new AnimationCurve(new Keyframe(0f, 0.15f), new Keyframe(1f, 0.3f));

	public float _heat = 1f;

	private float float_0;

	private Vector3 vector3_0;

	public float VelocityFactor = 10f;

	public void Start()
	{
		vector3_0 = base.transform.position;
	}

	public void Emit()
	{
		if (!EFTHardSettings.Instance.HEAT_EMITTER_ENABLED)
		{
			return;
		}
		float num = CountByHeat.Evaluate(_heat);
		if (num <= 0f)
		{
			return;
		}
		float num2 = 1f / num;
		float_0 = ((float_0 == 0f) ? Time.deltaTime : float_0);
		Vector3 vector = base.transform.position - vector3_0;
		while (float_0 + num2 < Time.time)
		{
			float num3 = LifeTimeByHeat.Evaluate(_heat);
			for (int i = 0; (float)i < num; i++)
			{
				Vector3 position = new Vector3(Random.Range(0f - Bounds.x, Bounds.x), Random.Range(0f - Bounds.y, Bounds.y), Random.Range(0f - Bounds.z, Bounds.z)) / 2f + Center;
				position = base.transform.TransformPoint(position);
				Singleton<Effects>.Instance.MuzzleEffect.Heat.Emit(new ParticleSystem.EmitParams
				{
					position = position,
					velocity = vector * VelocityFactor,
					startSize = Random.Range(Size.x, Size.y),
					startLifetime = num3 + Random.Range(LifetimeDelta.x, LifetimeDelta.y),
					startColor = Color.white
				}, 1);
			}
			float_0 += num2;
		}
	}

	public void LateUpdate()
	{
		Emit();
		if (_heat > 0f)
		{
			_heat -= IrradiationRate * Time.deltaTime;
		}
		vector3_0 = base.transform.position;
	}

	public void OnShot()
	{
		_heat = Mathf.Clamp01(_heat + HeatIncreasePerShot);
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(base.transform.position + base.transform.localRotation * Center, Bounds);
	}
}
