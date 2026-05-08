using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Systems.Effects;
using Audio.SpatialSystem;
using Comfort.Common;
using EFT;
using EFT.EnvironmentEffect;
using UnityEngine;

public class GrenadeEmission : MonoBehaviour
{
	public class Class625
	{
		public ParticleSystem ParticleSystem;

		public Material Material;
	}

	[Serializable]
	[CompilerGenerated]
	public class Class626
	{
		public static readonly Class626 class626_0 = new Class626();

		public static Func<ParticleSystem, bool> func_0;

		public bool method_0(ParticleSystem x)
		{
			return x.particleCount > 1;
		}
	}

	private Transform transform_0;

	private Vector3 vector3_0;

	private string string_0;

	public static bool EnableMBOITEmission = true;

	[SerializeField]
	private ParticleSystem _particleSystem;

	[SerializeField]
	private ParticleSystem[] _crucialSystems;

	[SerializeField]
	private ParticleSystem _fillSystem;

	[SerializeField]
	private List<ParticleSystem> _otherParticlesSystem;

	[SerializeField]
	private ParticleSystemRenderer[] _particleSystemRenderers = Array.Empty<ParticleSystemRenderer>();

	[SerializeField]
	private float _defaultStartFillDelay = 25f;

	[SerializeField]
	private float _startFillSize = 3.5f;

	[SerializeField]
	private float _startFillDistance = 1f;

	private float float_0 = 25f;

	private float float_1 = 65f;

	private bool bool_0;

	[SerializeField]
	private AudioClip _startAudioClip;

	[SerializeField]
	private AudioClip _audioClip;

	[SerializeField]
	private AudioClip _endAudioClip;

	[SerializeField]
	private float _removalDelay = 90f;

	private Vector3 vector3_1;

	private BetterSource betterSource_0;

	private List<Class625> list_0 = new List<Class625>();

	private float float_2;

	private float float_3;

	[Header("Auto-fill parameters")]
	[SerializeField]
	private ColliderExtrusion _extrusion;

	[SerializeField]
	private Vector2[] _startSpeed;

	private static readonly int int_0 = Shader.PropertyToID("_Indoor");

	private static readonly int int_1 = Shader.PropertyToID("_GrenadeWorldPosition");

	public Action OnRemove;

	public string PoolName
	{
		get
		{
			return string_0;
		}
		set
		{
			string_0 = value;
		}
	}

	public void AttachTo(Transform t, Vector3 offset)
	{
		transform_0 = t;
		vector3_0 = offset;
		vector3_1 = base.transform.position;
		if (!(transform_0 == null))
		{
			base.transform.position = transform_0.position + transform_0.rotation * vector3_0;
			base.transform.rotation = transform_0.rotation;
		}
	}

	public void OnValidate()
	{
		_startSpeed = new Vector2[_crucialSystems.Length];
		for (int i = 0; i < _crucialSystems.Length; i++)
		{
			_startSpeed[i] = new Vector2(_crucialSystems[i].main.startSpeed.constantMin, _crucialSystems[i].main.startSpeed.constantMax);
		}
		_extrusion = base.gameObject.GetComponentInChildren<ColliderExtrusion>();
		_particleSystemRenderers = GetComponentsInChildren<ParticleSystemRenderer>();
	}

	public void method_0()
	{
		if (!EnableMBOITEmission || !WindowsManager.InstanceIsActive())
		{
			return;
		}
		GClass1000 particlesManager = WindowsManager.Instance.ParticlesManager;
		if (particlesManager == null)
		{
			return;
		}
		for (int i = 0; i < _otherParticlesSystem.Count; i++)
		{
			if (WindowsManager.IsParticleSystemRendererSuitableForMBOIT(_otherParticlesSystem[i].GetComponent<ParticleSystemRenderer>()))
			{
				particlesManager.AddParticleSystem(_otherParticlesSystem[i], isDynamic: true);
			}
		}
	}

	public void StartEmission(float prewarm)
	{
		float_3 = Time.time - prewarm;
		vector3_1 = base.transform.position;
		method_7();
		UpdateExternalForces(indoor: false);
		method_0();
		betterSource_0 = Singleton<BetterAudio>.Instance.GetSource(BetterAudio.AudioSourceGroupType.Environment);
		betterSource_0.SetRolloff(25f);
		betterSource_0.StartTrackingPosition(base.transform);
		MonoBehaviourSingleton<SpatialAudioSystem>.Instance.ProcessSourceOcclusion(betterSource_0, EOcclusionTest.ContinuousPropagated);
		if (_startAudioClip == null)
		{
			betterSource_0.Loop = true;
			betterSource_0.Play(_audioClip, null, 1f, 1f, forceStereo: false, oneShot: false);
		}
		else
		{
			betterSource_0.Loop = false;
			betterSource_0.source1.clip = _startAudioClip;
			StartCoroutine(method_3(_startAudioClip.length));
		}
		if (prewarm > 0f)
		{
			method_8(prewarm);
		}
		_particleSystem.Play(withChildren: true);
		if (_fillSystem != null)
		{
			StartCoroutine(method_5());
		}
	}

	public void SetFillParams(float timePastSinceStart, float lifetimeLeft)
	{
		float_0 = Mathf.Max(0f, _defaultStartFillDelay - timePastSinceStart);
		float_1 = lifetimeLeft - float_0 - 1f;
	}

	public void UpdateExternalForces(bool indoor)
	{
		ParticleSystem[] crucialSystems = _crucialSystems;
		for (int i = 0; i < crucialSystems.Length; i++)
		{
			ParticleSystem.ExternalForcesModule externalForces = crucialSystems[i].externalForces;
			externalForces.enabled = !indoor;
		}
	}

	public void method_1(bool indoor)
	{
		if ((indoor && float_2 >= 1f) | (!indoor && float_2 <= 0f))
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		deltaTime = (indoor ? deltaTime : (0f - deltaTime));
		float_2 += deltaTime;
		float_2 = Mathf.Clamp01(float_2);
		for (int i = 0; i < list_0.Count; i++)
		{
			Class625 @class = list_0[i];
			@class.Material.SetFloat(int_0, float_2);
			if (WindowsManager.InstanceIsActive())
			{
				WindowsManager.Instance.ParticlesManager?.UpdateParticleSystemMaterial(@class.ParticleSystem, @class.Material);
			}
		}
	}

	public void LateUpdate()
	{
		method_2();
		if (transform_0 == null)
		{
			return;
		}
		base.transform.position = transform_0.position + transform_0.rotation * vector3_0;
		base.transform.rotation = transform_0.rotation;
		if ((bool)EnvironmentManager.Instance)
		{
			EnvironmentType environmentByPos = EnvironmentManager.Instance.GetEnvironmentByPos(base.transform.position);
			method_1(environmentByPos == EnvironmentType.Indoor);
		}
		if (Vector3.Distance(vector3_1, base.transform.position) > 0.25f)
		{
			vector3_1 = base.transform.position;
			UpdateExternalForces(bool_0);
			if (Time.time > float_3 + 2f)
			{
				Precaution();
			}
		}
	}

	public void method_2()
	{
		if (list_0 == null || list_0.Count <= 0 || !(_extrusion != null))
		{
			return;
		}
		foreach (Class625 item in list_0)
		{
			item.Material.SetVector(int_1, _extrusion.transform.position);
		}
	}

	[ContextMenu("Start Fill")]
	public void StartFill()
	{
		_particleSystem.Play(withChildren: true);
		TryAddParticleSystemToMBOIT(_particleSystem);
		if (_fillSystem != null)
		{
			StartCoroutine(method_5());
		}
		StartCoroutine(method_6(90));
	}

	public IEnumerator method_3(float delay)
	{
		yield return new WaitForSeconds(delay);
		betterSource_0.Loop = true;
		betterSource_0.Play(_audioClip, null, 1f, 1f, forceStereo: false, oneShot: false);
	}

	public IEnumerator method_4(float delay)
	{
		yield return new WaitForSeconds(delay);
		method_10();
	}

	public IEnumerator method_5()
	{
		yield return new WaitForSeconds(float_0);
		if (_extrusion != null)
		{
			float num = 0.1f;
			_extrusion.transform.position += Vector3.up * num;
			yield return _extrusion.CalculateCoroutine(_extrusion.transform.position);
			Vector3 depenetration = _extrusion.GetDepenetration();
			depenetration.y = Mathf.Clamp(depenetration.y, 0f - num, 0f);
			if (Vector3.Dot(_fillSystem.transform.position - base.transform.position, depenetration) > 0.1f)
			{
				Debug.Log("Depenetration goes awaaaay");
			}
			_extrusion.transform.position += depenetration;
		}
		Vector3[] array = new Vector3[3]
		{
			Vector3.forward,
			new Vector3(-0.9f, 0f, 0.5f),
			new Vector3(0.9f, 0f, -0.5f)
		};
		Vector3[] array2 = array;
		foreach (Vector3 vector in array2)
		{
			if (!Physics.Raycast(_fillSystem.transform.position + Vector3.up * 0.1f, vector, _startFillDistance * 0.66f, LayerMask.GetMask("LowPolyCollider")))
			{
				_fillSystem.Emit(new ParticleSystem.EmitParams
				{
					position = _fillSystem.transform.position + vector * _startFillDistance,
					startLifetime = float_1,
					velocity = Vector3.zero,
					randomSeed = 0u,
					startColor = Color.white,
					startSize = _startFillSize,
					rotation = 0f
				}, 1);
			}
			yield return new WaitForEndOfFrame();
		}
		TryAddParticleSystemToMBOIT(_fillSystem);
	}

	public IEnumerator method_6(int time)
	{
		yield return new WaitForSeconds(time);
		_particleSystem.Stop();
	}

	public void method_7()
	{
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem particleSystem in componentsInChildren)
		{
			ParticleSystemRenderer component = particleSystem.GetComponent<ParticleSystemRenderer>();
			list_0.Add(new Class625
			{
				ParticleSystem = particleSystem,
				Material = component.material
			});
		}
	}

	public void method_8(float prewarmTime)
	{
		ParticleSystem[] crucialSystems = _crucialSystems;
		for (int i = 0; i < crucialSystems.Length; i++)
		{
			crucialSystems[i].Simulate(prewarmTime, withChildren: false, restart: true);
		}
	}

	public void StopEmission(Grenade grenade)
	{
		_particleSystem.Stop(withChildren: true);
		if (betterSource_0 != null)
		{
			if (_endAudioClip == null)
			{
				betterSource_0.VolumeFadeOut(1f, method_10);
			}
			else
			{
				betterSource_0.Loop = false;
				betterSource_0.Play(_endAudioClip, null, 1f, 1f, forceStereo: false, oneShot: false);
				StartCoroutine(method_4(_endAudioClip.length));
			}
		}
		StartCoroutine(method_9(_removalDelay, 5f));
	}

	public IEnumerator method_9(float delay, float refreshRate)
	{
		yield return new WaitForSeconds(delay);
		while (_crucialSystems.Any((ParticleSystem x) => x.particleCount > 1))
		{
			yield return new WaitForSeconds(refreshRate);
		}
		OnRemove?.Invoke();
		EnableRenderers(value: true);
		Singleton<Effects>.Instance.DisposeEmissionEffect(this);
	}

	public void method_10()
	{
		if (!(betterSource_0 == null))
		{
			betterSource_0.Release();
			betterSource_0 = null;
			transform_0 = null;
		}
	}

	public void Clear()
	{
		transform_0 = null;
		_particleSystem.Clear();
		if (!EnableMBOITEmission || !WindowsManager.InstanceIsActive())
		{
			return;
		}
		GClass1000 particlesManager = WindowsManager.Instance.ParticlesManager;
		if (particlesManager == null)
		{
			return;
		}
		method_11(_particleSystem, particlesManager);
		method_11(_fillSystem, particlesManager);
		for (int i = 0; i < _crucialSystems.Length; i++)
		{
			method_11(_crucialSystems[i], particlesManager);
		}
		for (int j = 0; j < _otherParticlesSystem.Count; j++)
		{
			if (!(_otherParticlesSystem[j] == null))
			{
				particlesManager.RemoveParticleSystem(_otherParticlesSystem[j]);
			}
		}
	}

	public void method_11(ParticleSystem system, GClass1000 manager)
	{
		if (!(system == null) && WindowsManager.IsParticleSystemRendererSuitableForMBOIT(system.GetComponent<ParticleSystemRenderer>()))
		{
			manager.RemoveParticleSystem(system);
		}
	}

	public void Stall(Throwable obj)
	{
		Precaution();
	}

	public void Precaution()
	{
		float t = 1f;
		float t2 = 1f;
		Ray ray = new Ray(base.transform.position + Vector3.up * 0.2f, base.transform.forward);
		if (Physics.Raycast(ray, out var hitInfo, 1f, LayerMask.GetMask("Terrain", "LowPolyCollider")))
		{
			t = hitInfo.distance;
		}
		float num = Mathf.Lerp(-1f, 1f, t);
		if (num < 0f)
		{
			ray.direction = -base.transform.forward;
			if (Physics.Raycast(ray, out hitInfo, 1f, LayerMask.GetMask("Terrain", "LowPolyCollider")))
			{
				t2 = hitInfo.distance;
			}
			num = Mathf.Lerp(0f, num, t2);
		}
		TryAddParticleSystemToMBOIT(_particleSystem);
		for (int i = 0; i < _crucialSystems.Length; i++)
		{
			ParticleSystem particleSystem = _crucialSystems[i];
			ParticleSystem.MainModule main = particleSystem.main;
			Vector2 vector = _startSpeed[i] * num;
			main.startSpeed = ((vector.y < vector.x) ? new ParticleSystem.MinMaxCurve(vector.y, vector.x) : new ParticleSystem.MinMaxCurve(vector.x, vector.y));
			TryAddParticleSystemToMBOIT(particleSystem);
		}
	}

	public void TryAddParticleSystemToMBOIT(ParticleSystem system)
	{
		if (!(system == null) && EnableMBOITEmission && WindowsManager.InstanceIsActive() && WindowsManager.IsParticleSystemRendererSuitableForMBOIT(system.GetComponent<ParticleSystemRenderer>()))
		{
			WindowsManager.Instance.ParticlesManager?.AddParticleSystem(system, isDynamic: true);
		}
	}

	public void EnableRenderers(bool value)
	{
		ParticleSystemRenderer[] particleSystemRenderers = _particleSystemRenderers;
		for (int i = 0; i < particleSystemRenderers.Length; i++)
		{
			particleSystemRenderers[i].enabled = value;
		}
	}
}
