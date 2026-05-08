using UnityEngine;

public class ParticleSystemAdapter : Emitter
{
	public ParticleSystem ParticleSystemObject;

	public RundomRange Lifetime;

	public RundomRange Speed;

	public RundomRange Size;

	public bool RotationRandom;

	public float Rotation;

	public Color32 Color;

	private static readonly int int_0 = 10;

	private int int_1;

	private float[] float_0 = new float[int_0];

	private float[] float_1 = new float[int_0];

	private float[] float_2 = new float[int_0];

	private float[] float_3 = new float[int_0];

	private uint[] uint_0 = new uint[int_0];

	public void Awake()
	{
		if (ParticleSystemObject == null)
		{
			ParticleSystemObject = GetComponent<ParticleSystem>();
		}
		Lifetime.Init();
		Speed.Init();
		Size.Init();
		method_0();
	}

	public override void Emit(Vector3 pos, Vector3 vel)
	{
		float startLifetime = float_0[int_1];
		float num = float_1[int_1];
		float startSize = float_2[int_1];
		float rotation = (RotationRandom ? float_3[int_1] : Rotation);
		ParticleSystemObject.Emit(new ParticleSystem.EmitParams
		{
			position = pos,
			startLifetime = startLifetime,
			velocity = vel * num,
			randomSeed = uint_0[int_1],
			startColor = Color,
			startSize = startSize,
			rotation = rotation
		}, 1);
		int_1++;
		int_1 %= int_0;
	}

	public void Emit(Vector3 pos, Vector3 vel, float sizeMult)
	{
		float startLifetime = float_0[int_1];
		float num = float_1[int_1];
		float num2 = float_2[int_1];
		float rotation = (RotationRandom ? float_3[int_1] : Rotation);
		num2 *= sizeMult;
		ParticleSystemObject.Emit(new ParticleSystem.EmitParams
		{
			position = pos,
			startLifetime = startLifetime,
			velocity = vel * num,
			randomSeed = uint_0[int_1],
			startColor = Color,
			startSize = num2,
			rotation = rotation
		}, 1);
		int_1++;
		int_1 %= int_0;
	}

	public void method_0()
	{
		float_0 = new float[int_0];
		float_1 = new float[int_0];
		float_2 = new float[int_0];
		float_3 = new float[int_0];
		uint_0 = new uint[int_0];
		for (int i = 0; i < int_0; i++)
		{
			float_0[i] = Lifetime.Value;
			float_1[i] = Speed.Value;
			float_2[i] = Size.Value;
			float_3[i] = GClass2608.FloatRotation();
			uint_0[i] = GClass2608.Uint();
		}
	}
}
