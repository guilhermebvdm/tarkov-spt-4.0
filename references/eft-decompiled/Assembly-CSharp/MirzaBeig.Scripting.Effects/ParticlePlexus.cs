using System;
using System.Collections.Generic;
using UnityEngine;

namespace MirzaBeig.Scripting.Effects;

[RequireComponent(typeof(ParticleSystem))]
[AddComponentMenu("Effects/Particle Plexus")]
public class ParticlePlexus : MonoBehaviour
{
	public float maxDistance = 1f;

	public int maxConnections = 5;

	public int maxLineRenderers = 100;

	[Range(0f, 1f)]
	public float widthFromParticle = 0.125f;

	[Range(0f, 1f)]
	public float colourFromParticle = 1f;

	[Range(0f, 1f)]
	public float alphaFromParticle = 1f;

	private ParticleSystem particleSystem_0;

	private ParticleSystem.Particle[] particle_0;

	private Vector3[] vector3_0;

	private Color[] color_0;

	private float[] float_0;

	private ParticleSystem.MainModule mainModule_0;

	public LineRenderer lineRendererTemplate;

	private List<LineRenderer> list_0 = new List<LineRenderer>();

	private Transform transform_0;

	[Header("General Performance Settings")]
	[Range(0f, 1f)]
	public float delay;

	private float float_1;

	public bool alwaysUpdate;

	private bool bool_0;

	public void Start()
	{
		particleSystem_0 = GetComponent<ParticleSystem>();
		mainModule_0 = particleSystem_0.main;
		transform_0 = base.transform;
	}

	public void OnDisable()
	{
		for (int i = 0; i < list_0.Count; i++)
		{
			list_0[i].enabled = false;
		}
	}

	public void OnBecameVisible()
	{
		bool_0 = true;
	}

	public void OnBecameInvisible()
	{
		bool_0 = false;
	}

	public void LateUpdate()
	{
		int num = list_0.Count;
		if (num > maxLineRenderers)
		{
			for (int i = maxLineRenderers; i < num; i++)
			{
				UnityEngine.Object.Destroy(list_0[i].gameObject);
			}
			list_0.RemoveRange(maxLineRenderers, num - maxLineRenderers);
			num -= num - maxLineRenderers;
		}
		if (!alwaysUpdate && !bool_0)
		{
			return;
		}
		int maxParticles = mainModule_0.maxParticles;
		if (particle_0 == null || particle_0.Length < maxParticles)
		{
			particle_0 = new ParticleSystem.Particle[maxParticles];
			vector3_0 = new Vector3[maxParticles];
			color_0 = new Color[maxParticles];
			float_0 = new float[maxParticles];
		}
		float_1 += Time.deltaTime;
		if (!(float_1 >= delay))
		{
			return;
		}
		float_1 = 0f;
		int num2 = 0;
		if (maxConnections > 0 && maxLineRenderers > 0)
		{
			particleSystem_0.GetParticles(particle_0);
			int particleCount = particleSystem_0.particleCount;
			float num3 = maxDistance * maxDistance;
			ParticleSystemSimulationSpace simulationSpace = mainModule_0.simulationSpace;
			ParticleSystemScalingMode scalingMode = mainModule_0.scalingMode;
			Transform customSimulationSpace = mainModule_0.customSimulationSpace;
			Color startColor = lineRendererTemplate.startColor;
			Color endColor = lineRendererTemplate.endColor;
			float a = lineRendererTemplate.startWidth * lineRendererTemplate.widthMultiplier;
			float a2 = lineRendererTemplate.endWidth * lineRendererTemplate.widthMultiplier;
			for (int j = 0; j < particleCount; j++)
			{
				vector3_0[j] = particle_0[j].position;
				color_0[j] = particle_0[j].GetCurrentColor(particleSystem_0);
				float_0[j] = particle_0[j].GetCurrentSize(particleSystem_0);
			}
			Vector3 vector = default(Vector3);
			if (simulationSpace == ParticleSystemSimulationSpace.World)
			{
				for (int k = 0; k < particleCount; k++)
				{
					if (num2 == maxLineRenderers)
					{
						break;
					}
					Color b = color_0[k];
					Color startColor2 = Color.LerpUnclamped(startColor, b, colourFromParticle);
					startColor2.a = Mathf.LerpUnclamped(startColor.a, b.a, alphaFromParticle);
					float startWidth = Mathf.LerpUnclamped(a, float_0[k], widthFromParticle);
					int num4 = 0;
					for (int l = k + 1; l < particleCount; l++)
					{
						vector.x = vector3_0[k].x - vector3_0[l].x;
						vector.y = vector3_0[k].y - vector3_0[l].y;
						vector.z = vector3_0[k].z - vector3_0[l].z;
						if (vector.x * vector.x + vector.y * vector.y + vector.z * vector.z <= num3)
						{
							LineRenderer item;
							if (num2 == num)
							{
								item = UnityEngine.Object.Instantiate(lineRendererTemplate, transform_0, worldPositionStays: false);
								list_0.Add(item);
								num++;
							}
							item = list_0[num2];
							item.enabled = true;
							item.SetPosition(0, vector3_0[k]);
							item.SetPosition(1, vector3_0[l]);
							item.startColor = startColor2;
							b = color_0[l];
							Color endColor2 = Color.LerpUnclamped(endColor, b, colourFromParticle);
							endColor2.a = Mathf.LerpUnclamped(endColor.a, b.a, alphaFromParticle);
							item.endColor = endColor2;
							item.startWidth = startWidth;
							item.endWidth = Mathf.LerpUnclamped(a2, float_0[l], widthFromParticle);
							num2++;
							num4++;
							if (num4 == maxConnections || num2 == maxLineRenderers)
							{
								break;
							}
						}
					}
				}
			}
			else
			{
				Vector3 zero = Vector3.zero;
				Quaternion identity = Quaternion.identity;
				Vector3 one = Vector3.one;
				Transform transform = transform_0;
				switch (simulationSpace)
				{
				default:
					throw new NotSupportedException($"Unsupported scaling mode '{simulationSpace}'.");
				case ParticleSystemSimulationSpace.Custom:
					transform = customSimulationSpace;
					zero = transform.position;
					identity = transform.rotation;
					one = transform.localScale;
					break;
				case ParticleSystemSimulationSpace.Local:
					zero = transform.position;
					identity = transform.rotation;
					one = transform.localScale;
					break;
				}
				Vector3 vector2 = Vector3.zero;
				Vector3 vector3 = Vector3.zero;
				for (int m = 0; m < particleCount; m++)
				{
					if (num2 == maxLineRenderers)
					{
						break;
					}
					if (simulationSpace == ParticleSystemSimulationSpace.Local || simulationSpace == ParticleSystemSimulationSpace.Custom)
					{
						switch (scalingMode)
						{
						case ParticleSystemScalingMode.Hierarchy:
							vector2 = transform.TransformPoint(vector3_0[m]);
							break;
						case ParticleSystemScalingMode.Local:
							vector2.x = vector3_0[m].x * one.x;
							vector2.y = vector3_0[m].y * one.y;
							vector2.z = vector3_0[m].z * one.z;
							vector2 = identity * vector2;
							vector2.x += zero.x;
							vector2.y += zero.y;
							vector2.z += zero.z;
							break;
						case ParticleSystemScalingMode.Shape:
							vector2 = identity * vector3_0[m];
							vector2.x += zero.x;
							vector2.y += zero.y;
							vector2.z += zero.z;
							break;
						default:
							throw new NotSupportedException($"Unsupported scaling mode '{scalingMode}'.");
						}
					}
					Color b2 = color_0[m];
					Color startColor3 = Color.LerpUnclamped(startColor, b2, colourFromParticle);
					startColor3.a = Mathf.LerpUnclamped(startColor.a, b2.a, alphaFromParticle);
					float startWidth2 = Mathf.LerpUnclamped(a, float_0[m], widthFromParticle);
					int num5 = 0;
					for (int n = m + 1; n < particleCount; n++)
					{
						if (simulationSpace == ParticleSystemSimulationSpace.Local || simulationSpace == ParticleSystemSimulationSpace.Custom)
						{
							switch (scalingMode)
							{
							case ParticleSystemScalingMode.Hierarchy:
								vector3 = transform.TransformPoint(vector3_0[n]);
								break;
							case ParticleSystemScalingMode.Local:
								vector3.x = vector3_0[n].x * one.x;
								vector3.y = vector3_0[n].y * one.y;
								vector3.z = vector3_0[n].z * one.z;
								vector3 = identity * vector3;
								vector3.x += zero.x;
								vector3.y += zero.y;
								vector3.z += zero.z;
								break;
							case ParticleSystemScalingMode.Shape:
								vector3 = identity * vector3_0[n];
								vector3.x += zero.x;
								vector3.y += zero.y;
								vector3.z += zero.z;
								break;
							default:
								throw new NotSupportedException($"Unsupported scaling mode '{scalingMode}'.");
							}
						}
						vector.x = vector3_0[m].x - vector3_0[n].x;
						vector.y = vector3_0[m].y - vector3_0[n].y;
						vector.z = vector3_0[m].z - vector3_0[n].z;
						if (vector.x * vector.x + vector.y * vector.y + vector.z * vector.z <= num3)
						{
							LineRenderer item2;
							if (num2 == num)
							{
								item2 = UnityEngine.Object.Instantiate(lineRendererTemplate, transform_0, worldPositionStays: false);
								list_0.Add(item2);
								num++;
							}
							item2 = list_0[num2];
							item2.enabled = true;
							item2.SetPosition(0, vector2);
							item2.SetPosition(1, vector3);
							item2.startColor = startColor3;
							b2 = color_0[n];
							Color endColor3 = Color.LerpUnclamped(endColor, b2, colourFromParticle);
							endColor3.a = Mathf.LerpUnclamped(endColor.a, b2.a, alphaFromParticle);
							item2.endColor = endColor3;
							item2.startWidth = startWidth2;
							item2.endWidth = Mathf.LerpUnclamped(a2, float_0[n], widthFromParticle);
							num2++;
							num5++;
							if (num5 == maxConnections || num2 == maxLineRenderers)
							{
								break;
							}
						}
					}
				}
			}
		}
		for (int num6 = num2; num6 < num; num6++)
		{
			if (list_0[num6].enabled)
			{
				list_0[num6].enabled = false;
			}
		}
	}
}
