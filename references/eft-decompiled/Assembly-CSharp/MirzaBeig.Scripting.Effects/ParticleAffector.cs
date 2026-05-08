using System;
using System.Collections.Generic;
using UnityEngine;

namespace MirzaBeig.Scripting.Effects;

public abstract class ParticleAffector : MonoBehaviour
{
	public struct GetForceParameters
	{
		public float distanceToAffectorCenterSqr;

		public Vector3 scaledDirectionToAffectorCenter;

		public Vector3 particlePosition;
	}

	[Header("Common Controls")]
	public float radius = float.PositiveInfinity;

	public float force = 5f;

	public Vector3 offset = Vector3.zero;

	private float float_0;

	private float float_1;

	private float float_2;

	private Vector3 vector3_0;

	private float[] float_3;

	public AnimationCurve scaleForceByDistance = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	private ParticleSystem particleSystem_0;

	public List<ParticleSystem> _particleSystems;

	private int int_0;

	private List<ParticleSystem> list_0 = new List<ParticleSystem>();

	private ParticleSystem.Particle[][] particle_0;

	private ParticleSystem.MainModule[] mainModule_0;

	private Renderer[] renderer_0;

	protected ParticleSystem currentParticleSystem;

	protected GetForceParameters parameters;

	public bool alwaysUpdate;

	public float scaledRadius => radius * base.transform.lossyScale.x;

	public virtual void Awake()
	{
	}

	public virtual void Start()
	{
		particleSystem_0 = GetComponent<ParticleSystem>();
	}

	public virtual void PerParticleSystemSetup()
	{
	}

	public virtual Vector3 GetForce()
	{
		return Vector3.zero;
	}

	public virtual void Update()
	{
	}

	public void AddParticleSystem(ParticleSystem particleSystem)
	{
		_particleSystems.Add(particleSystem);
	}

	public void RemoveParticleSystem(ParticleSystem particleSystem)
	{
		_particleSystems.Remove(particleSystem);
	}

	public virtual void LateUpdate()
	{
		float_0 = scaledRadius;
		float_1 = float_0 * float_0;
		float_2 = force * Time.deltaTime;
		vector3_0 = base.transform.position + offset;
		if (_particleSystems.Count != 0)
		{
			if (list_0.Count != _particleSystems.Count)
			{
				list_0.Clear();
				list_0.AddRange(_particleSystems);
			}
			else
			{
				for (int i = 0; i < _particleSystems.Count; i++)
				{
					list_0[i] = _particleSystems[i];
				}
			}
		}
		else if ((bool)particleSystem_0)
		{
			if (list_0.Count == 1)
			{
				list_0[0] = particleSystem_0;
			}
			else
			{
				list_0.Clear();
				list_0.Add(particleSystem_0);
			}
		}
		else
		{
			list_0.Clear();
			list_0.AddRange(GClass870.FindUnityObjectsOfType<ParticleSystem>());
		}
		parameters = default(GetForceParameters);
		int_0 = list_0.Count;
		if (particle_0 == null || particle_0.Length < int_0)
		{
			particle_0 = new ParticleSystem.Particle[int_0][];
			mainModule_0 = new ParticleSystem.MainModule[int_0];
			renderer_0 = new Renderer[int_0];
			float_3 = new float[int_0];
			for (int j = 0; j < int_0; j++)
			{
				mainModule_0[j] = list_0[j].main;
				renderer_0[j] = list_0[j].GetComponent<Renderer>();
				float_3[j] = list_0[j].externalForces.multiplier;
			}
		}
		int num = 0;
		ParticleSystemSimulationSpace simulationSpace;
		while (true)
		{
			if (num >= int_0)
			{
				return;
			}
			if (renderer_0[num].isVisible || alwaysUpdate)
			{
				int maxParticles = mainModule_0[num].maxParticles;
				if (particle_0[num] == null || particle_0[num].Length < maxParticles)
				{
					particle_0[num] = new ParticleSystem.Particle[maxParticles];
				}
				currentParticleSystem = list_0[num];
				PerParticleSystemSetup();
				int particles = currentParticleSystem.GetParticles(particle_0[num]);
				simulationSpace = mainModule_0[num].simulationSpace;
				ParticleSystemScalingMode scalingMode = mainModule_0[num].scalingMode;
				Transform transform = currentParticleSystem.transform;
				Transform customSimulationSpace = mainModule_0[num].customSimulationSpace;
				if (simulationSpace == ParticleSystemSimulationSpace.World)
				{
					for (int k = 0; k < particles; k++)
					{
						parameters.particlePosition = particle_0[num][k].position;
						parameters.scaledDirectionToAffectorCenter.x = vector3_0.x - parameters.particlePosition.x;
						parameters.scaledDirectionToAffectorCenter.y = vector3_0.y - parameters.particlePosition.y;
						parameters.scaledDirectionToAffectorCenter.z = vector3_0.z - parameters.particlePosition.z;
						parameters.distanceToAffectorCenterSqr = parameters.scaledDirectionToAffectorCenter.sqrMagnitude;
						if (parameters.distanceToAffectorCenterSqr < float_1)
						{
							float time = parameters.distanceToAffectorCenterSqr / float_1;
							float num2 = scaleForceByDistance.Evaluate(time);
							Vector3 vector = GetForce();
							float num3 = float_2 * num2 * float_3[num];
							vector.x *= num3;
							vector.y *= num3;
							vector.z *= num3;
							Vector3 velocity = particle_0[num][k].velocity;
							velocity.x += vector.x;
							velocity.y += vector.y;
							velocity.z += vector.z;
							particle_0[num][k].velocity = velocity;
						}
					}
				}
				else
				{
					Vector3 zero = Vector3.zero;
					Quaternion identity = Quaternion.identity;
					Vector3 one = Vector3.one;
					Transform transform2 = transform;
					if (simulationSpace != ParticleSystemSimulationSpace.Local)
					{
						if (simulationSpace != ParticleSystemSimulationSpace.Custom)
						{
							break;
						}
						transform2 = customSimulationSpace;
						zero = transform2.position;
						identity = transform2.rotation;
						one = transform2.localScale;
					}
					else
					{
						zero = transform2.position;
						identity = transform2.rotation;
						one = transform2.localScale;
					}
					for (int l = 0; l < particles; l++)
					{
						parameters.particlePosition = particle_0[num][l].position;
						if (simulationSpace == ParticleSystemSimulationSpace.Local || simulationSpace == ParticleSystemSimulationSpace.Custom)
						{
							switch (scalingMode)
							{
							case ParticleSystemScalingMode.Hierarchy:
								parameters.particlePosition = transform2.TransformPoint(particle_0[num][l].position);
								break;
							case ParticleSystemScalingMode.Local:
								parameters.particlePosition = Vector3.Scale(parameters.particlePosition, one);
								parameters.particlePosition = identity * parameters.particlePosition;
								parameters.particlePosition += zero;
								break;
							case ParticleSystemScalingMode.Shape:
								parameters.particlePosition = identity * parameters.particlePosition;
								parameters.particlePosition += zero;
								break;
							default:
								throw new NotSupportedException($"Unsupported scaling mode '{scalingMode}'.");
							}
						}
						parameters.scaledDirectionToAffectorCenter.x = vector3_0.x - parameters.particlePosition.x;
						parameters.scaledDirectionToAffectorCenter.y = vector3_0.y - parameters.particlePosition.y;
						parameters.scaledDirectionToAffectorCenter.z = vector3_0.z - parameters.particlePosition.z;
						parameters.distanceToAffectorCenterSqr = parameters.scaledDirectionToAffectorCenter.sqrMagnitude;
						if (!(parameters.distanceToAffectorCenterSqr < float_1))
						{
							continue;
						}
						float time2 = parameters.distanceToAffectorCenterSqr / float_1;
						float num4 = scaleForceByDistance.Evaluate(time2);
						Vector3 vector2 = GetForce();
						float num5 = float_2 * num4 * float_3[num];
						vector2.x *= num5;
						vector2.y *= num5;
						vector2.z *= num5;
						if (simulationSpace == ParticleSystemSimulationSpace.Local || simulationSpace == ParticleSystemSimulationSpace.Custom)
						{
							switch (scalingMode)
							{
							case ParticleSystemScalingMode.Hierarchy:
								vector2 = transform2.InverseTransformVector(vector2);
								break;
							case ParticleSystemScalingMode.Local:
								vector2 = Quaternion.Inverse(identity) * vector2;
								vector2 = Vector3.Scale(vector2, new Vector3(1f / one.x, 1f / one.y, 1f / one.z));
								break;
							case ParticleSystemScalingMode.Shape:
								vector2 = Quaternion.Inverse(identity) * vector2;
								break;
							default:
								throw new NotSupportedException($"Unsupported scaling mode '{scalingMode}'.");
							}
						}
						Vector3 velocity2 = particle_0[num][l].velocity;
						velocity2.x += vector2.x;
						velocity2.y += vector2.y;
						velocity2.z += vector2.z;
						particle_0[num][l].velocity = velocity2;
					}
				}
				currentParticleSystem.SetParticles(particle_0[num], particles);
			}
			num++;
		}
		throw new NotSupportedException($"Unsupported scaling mode '{simulationSpace}'.");
	}

	public void OnApplicationQuit()
	{
	}

	public virtual void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(base.transform.position + offset, scaledRadius);
	}

	public ParticleAffector()
	{
	}
}
