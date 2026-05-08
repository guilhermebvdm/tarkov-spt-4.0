using UnityEngine;

namespace MirzaBeig.Scripting.Effects;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleFlocking : MonoBehaviour
{
	public struct GStruct118
	{
		public Bounds bounds;

		public int[] particles;

		public int particleCount;
	}

	[Header("N^2 Mode Settings")]
	public float maxDistance = 0.5f;

	[Header("Forces")]
	public float cohesion = 0.5f;

	public float separation = 0.25f;

	[Header("Voxel Mode Settings")]
	public bool useVoxels = true;

	public bool voxelLocalCenterFromBounds = true;

	public float voxelVolume = 8f;

	public int voxelsPerAxis = 5;

	private int int_0;

	private GStruct118[] gstruct118_0;

	private ParticleSystem particleSystem_0;

	private ParticleSystem.Particle[] particle_0;

	private Vector3[] vector3_0;

	private ParticleSystem.MainModule mainModule_0;

	[Header("General Performance Settings")]
	[Range(0f, 1f)]
	public float delay;

	private float float_0;

	public bool alwaysUpdate;

	private bool bool_0;

	public void Start()
	{
		particleSystem_0 = GetComponent<ParticleSystem>();
		mainModule_0 = particleSystem_0.main;
	}

	public void OnBecameVisible()
	{
		bool_0 = true;
	}

	public void OnBecameInvisible()
	{
		bool_0 = false;
	}

	public void method_0()
	{
		int num = voxelsPerAxis * voxelsPerAxis * voxelsPerAxis;
		gstruct118_0 = new GStruct118[num];
		float num2 = voxelVolume / (float)voxelsPerAxis;
		float num3 = num2 / 2f;
		float num4 = voxelVolume / 2f;
		Vector3 position = base.transform.position;
		int num5 = 0;
		for (int i = 0; i < voxelsPerAxis; i++)
		{
			float x = 0f - num4 + num3 + (float)i * num2;
			for (int j = 0; j < voxelsPerAxis; j++)
			{
				float y = 0f - num4 + num3 + (float)j * num2;
				for (int k = 0; k < voxelsPerAxis; k++)
				{
					float z = 0f - num4 + num3 + (float)k * num2;
					gstruct118_0[num5].particleCount = 0;
					gstruct118_0[num5].bounds = new Bounds(position + new Vector3(x, y, z), Vector3.one * num2);
					num5++;
				}
			}
		}
	}

	public void LateUpdate()
	{
		if (!alwaysUpdate && !bool_0)
		{
			return;
		}
		if (useVoxels)
		{
			int num = voxelsPerAxis * voxelsPerAxis * voxelsPerAxis;
			if (gstruct118_0 == null || gstruct118_0.Length < num)
			{
				method_0();
			}
		}
		int maxParticles = mainModule_0.maxParticles;
		if (particle_0 == null || particle_0.Length < maxParticles)
		{
			particle_0 = new ParticleSystem.Particle[maxParticles];
			vector3_0 = new Vector3[maxParticles];
			if (useVoxels)
			{
				for (int i = 0; i < gstruct118_0.Length; i++)
				{
					gstruct118_0[i].particles = new int[maxParticles];
				}
			}
		}
		float_0 += Time.deltaTime;
		if (!(float_0 >= delay))
		{
			return;
		}
		float num2 = float_0;
		float_0 = 0f;
		particleSystem_0.GetParticles(particle_0);
		int particleCount = particleSystem_0.particleCount;
		float num3 = cohesion * num2;
		float num4 = separation * num2;
		for (int j = 0; j < particleCount; j++)
		{
			vector3_0[j] = particle_0[j].position;
		}
		if (useVoxels)
		{
			int num5 = gstruct118_0.Length;
			float num6 = voxelVolume / (float)voxelsPerAxis;
			for (int k = 0; k < particleCount; k++)
			{
				for (int l = 0; l < num5; l++)
				{
					if (gstruct118_0[l].bounds.Contains(vector3_0[k]))
					{
						gstruct118_0[l].particles[gstruct118_0[l].particleCount] = k;
						gstruct118_0[l].particleCount++;
						break;
					}
				}
			}
			for (int m = 0; m < num5; m++)
			{
				if (gstruct118_0[m].particleCount <= 1)
				{
					continue;
				}
				for (int n = 0; n < gstruct118_0[m].particleCount; n++)
				{
					Vector3 vector = vector3_0[gstruct118_0[m].particles[n]];
					Vector3 vector2;
					if (voxelLocalCenterFromBounds)
					{
						vector2 = gstruct118_0[m].bounds.center - vector3_0[gstruct118_0[m].particles[n]];
					}
					else
					{
						for (int num7 = 0; num7 < gstruct118_0[m].particleCount; num7++)
						{
							if (num7 != n)
							{
								vector += vector3_0[gstruct118_0[m].particles[num7]];
							}
						}
						vector /= (float)gstruct118_0[m].particleCount;
						vector2 = vector - vector3_0[gstruct118_0[m].particles[n]];
					}
					float sqrMagnitude = vector2.sqrMagnitude;
					vector2.Normalize();
					Vector3 zero = Vector3.zero;
					zero += vector2 * num3;
					zero -= vector2 * ((1f - sqrMagnitude / num6) * num4);
					Vector3 velocity = particle_0[gstruct118_0[m].particles[n]].velocity;
					velocity.x += zero.x;
					velocity.y += zero.y;
					velocity.z += zero.z;
					particle_0[gstruct118_0[m].particles[n]].velocity = velocity;
				}
				gstruct118_0[m].particleCount = 0;
			}
		}
		else
		{
			float num8 = maxDistance * maxDistance;
			Vector3 vector4 = default(Vector3);
			for (int num9 = 0; num9 < particleCount; num9++)
			{
				int num10 = 1;
				Vector3 vector3 = vector3_0[num9];
				for (int num11 = 0; num11 < particleCount; num11++)
				{
					if (num11 != num9)
					{
						vector4.x = vector3_0[num9].x - vector3_0[num11].x;
						vector4.y = vector3_0[num9].y - vector3_0[num11].y;
						vector4.z = vector3_0[num9].z - vector3_0[num11].z;
						if (Vector3.SqrMagnitude(vector4) <= num8)
						{
							num10++;
							vector3 += vector3_0[num11];
						}
					}
				}
				if (num10 != 1)
				{
					vector3 /= (float)num10;
					Vector3 vector5 = vector3 - vector3_0[num9];
					float sqrMagnitude2 = vector5.sqrMagnitude;
					vector5.Normalize();
					Vector3 zero2 = Vector3.zero;
					zero2 += vector5 * num3;
					zero2 -= vector5 * ((1f - sqrMagnitude2 / num8) * num4);
					Vector3 velocity2 = particle_0[num9].velocity;
					velocity2.x += zero2.x;
					velocity2.y += zero2.y;
					velocity2.z += zero2.z;
					particle_0[num9].velocity = velocity2;
				}
			}
		}
		particleSystem_0.SetParticles(particle_0, particleCount);
	}

	public void OnDrawGizmosSelected()
	{
		float num = voxelVolume / (float)voxelsPerAxis;
		float num2 = num / 2f;
		float num3 = voxelVolume / 2f;
		Vector3 position = base.transform.position;
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(position, Vector3.one * voxelVolume);
		Gizmos.color = Color.white;
		for (int i = 0; i < voxelsPerAxis; i++)
		{
			float x = 0f - num3 + num2 + (float)i * num;
			for (int j = 0; j < voxelsPerAxis; j++)
			{
				float y = 0f - num3 + num2 + (float)j * num;
				for (int k = 0; k < voxelsPerAxis; k++)
				{
					float z = 0f - num3 + num2 + (float)k * num;
					Gizmos.DrawWireCube(position + new Vector3(x, y, z), Vector3.one * num);
				}
			}
		}
	}
}
