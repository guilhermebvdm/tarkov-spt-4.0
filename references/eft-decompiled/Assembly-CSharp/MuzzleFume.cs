using UnityEngine;

public class MuzzleFume : MuzzleEffect
{
	public float StartPos;

	public float EmitterRadius;

	public float ConusSize = 1f;

	public AnimationCurve Sizes = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	public AnimationCurve Speeds = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	public AnimationCurve LifeTimes = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	public float SizesRnd = 0.5f;

	public float Size = 1f;

	public float Speed = 1f;

	public float LifeTime = 1f;

	public Gradient Color;

	public int CountMin = 1;

	public int CountRange = 3;

	private Vector3 vector3_0;

	private Transform transform_0;

	public void Awake()
	{
		transform_0 = base.transform;
		vector3_0 = transform_0.position - transform_0.up * StartPos;
	}

	public void UpdateValues()
	{
		if (!(transform_0 == null))
		{
			vector3_0 = transform_0.position;
		}
	}

	public void Emit(GInterface52 emitter)
	{
		Vector3 position = transform_0.position;
		Vector3 vector = -transform_0.up;
		Vector3 vector2 = position + vector * StartPos;
		Vector3 vector3 = position - vector3_0;
		int num = ((CountRange < 1) ? CountMin : GClass2608.Int(CountMin, CountRange));
		for (int i = 0; i < num; i++)
		{
			Vector3 position2 = vector2;
			if (EmitterRadius >= float.Epsilon)
			{
				position2 += GClass2608.VectorNormalized() * EmitterRadius;
			}
			float time = GClass2608.Float();
			Vector3 velocity = vector3 / 2f + (vector + GClass2608.VectorNormalized() * ConusSize).normalized * Speeds.Evaluate(time) * Speed;
			float size = (Sizes.Evaluate(time) + Random.Range(0f - SizesRnd, SizesRnd)) * Size;
			float lifetime = LifeTimes.Evaluate(time) * LifeTime;
			emitter.Emit(position2, velocity, size, lifetime, Color.Evaluate(time));
		}
	}
}
