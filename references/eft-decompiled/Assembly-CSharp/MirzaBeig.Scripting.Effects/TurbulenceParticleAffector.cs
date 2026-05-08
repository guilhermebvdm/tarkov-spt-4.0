using UnityEngine;

namespace MirzaBeig.Scripting.Effects;

public class TurbulenceParticleAffector : ParticleAffector
{
	public enum NoiseType
	{
		PseudoPerlin,
		Perlin,
		Simplex,
		OctavePerlin,
		OctaveSimplex
	}

	[Header("Affector Controls")]
	public float speed = 1f;

	[Range(0f, 8f)]
	public float frequency = 1f;

	public NoiseType noiseType = NoiseType.Perlin;

	[Header("Octave Variant-Only Controls")]
	[Range(1f, 8f)]
	public int octaves = 1;

	[Range(0f, 4f)]
	public float lacunarity = 2f;

	[Range(0f, 1f)]
	public float persistence = 0.5f;

	private float float_4;

	private float float_5;

	private float float_6;

	private float float_7;

	private float float_8;

	private float float_9;

	private float float_10;

	public override void Awake()
	{
		base.Awake();
	}

	public override void Start()
	{
		base.Start();
		float_5 = Random.Range(-32f, 32f);
		float_6 = Random.Range(-32f, 32f);
		float_7 = Random.Range(-32f, 32f);
	}

	public override void Update()
	{
		float_4 = Time.time;
		base.Update();
	}

	public override void LateUpdate()
	{
		float_8 = float_4 * speed + float_5;
		float_9 = float_4 * speed + float_6;
		float_10 = float_4 * speed + float_7;
		base.LateUpdate();
	}

	public override Vector3 GetForce()
	{
		float num = parameters.particlePosition.x + float_8;
		float num2 = parameters.particlePosition.y + float_8;
		float num3 = parameters.particlePosition.z + float_8;
		float num4 = parameters.particlePosition.x + float_9;
		float num5 = parameters.particlePosition.y + float_9;
		float num6 = parameters.particlePosition.z + float_9;
		float num7 = parameters.particlePosition.x + float_10;
		float num8 = parameters.particlePosition.y + float_10;
		float num9 = parameters.particlePosition.z + float_10;
		Vector3 result = default(Vector3);
		switch (noiseType)
		{
		case NoiseType.PseudoPerlin:
		{
			float t = Mathf.PerlinNoise(num * frequency, num5 * frequency);
			float t2 = Mathf.PerlinNoise(num * frequency, num6 * frequency);
			float t3 = Mathf.PerlinNoise(num * frequency, num4 * frequency);
			t = Mathf.Lerp(-1f, 1f, t);
			t2 = Mathf.Lerp(-1f, 1f, t2);
			t3 = Mathf.Lerp(-1f, 1f, t3);
			Vector3 vector = Vector3.right * t;
			Vector3 vector2 = Vector3.up * t2;
			Vector3 vector3 = Vector3.forward * t3;
			return vector + vector2 + vector3;
		}
		default:
			result.x = Noise.perlin(num * frequency, num2 * frequency, num3 * frequency);
			result.y = Noise.perlin(num4 * frequency, num5 * frequency, num6 * frequency);
			result.z = Noise.perlin(num7 * frequency, num8 * frequency, num9 * frequency);
			return result;
		case NoiseType.Simplex:
			result.x = Noise.simplex(num * frequency, num2 * frequency, num3 * frequency);
			result.y = Noise.simplex(num4 * frequency, num5 * frequency, num6 * frequency);
			result.z = Noise.simplex(num7 * frequency, num8 * frequency, num9 * frequency);
			break;
		case NoiseType.OctavePerlin:
			result.x = Noise.octavePerlin(num, num2, num3, frequency, octaves, lacunarity, persistence);
			result.y = Noise.octavePerlin(num4, num5, num6, frequency, octaves, lacunarity, persistence);
			result.z = Noise.octavePerlin(num7, num8, num9, frequency, octaves, lacunarity, persistence);
			break;
		case NoiseType.OctaveSimplex:
			result.x = Noise.octaveSimplex(num, num2, num3, frequency, octaves, lacunarity, persistence);
			result.y = Noise.octaveSimplex(num4, num5, num6, frequency, octaves, lacunarity, persistence);
			result.z = Noise.octaveSimplex(num7, num8, num9, frequency, octaves, lacunarity, persistence);
			break;
		}
		return result;
	}

	public override void OnDrawGizmosSelected()
	{
		if (base.enabled)
		{
			base.OnDrawGizmosSelected();
		}
	}
}
