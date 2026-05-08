using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Systems.Effects;
using Comfort.Common;
using UnityEngine;

public class MuzzleManager : BaseSystemComponent<MuzzleManager>, GInterface53, GInterface54, GInterface52, GInterface50
{
	[Serializable]
	[CompilerGenerated]
	public class Class621
	{
		public static readonly Class621 class621_0 = new Class621();

		public static Func<MuzzleFume, GameObject> func_0;

		public static Func<MuzzleEffect, GameObject> func_1;

		public static Func<MuzzleJet, float> func_2;

		public GameObject method_0(MuzzleFume f)
		{
			return f.gameObject;
		}

		public GameObject method_1(MuzzleEffect x)
		{
			return x.gameObject;
		}

		public float method_2(MuzzleJet x)
		{
			return x.Chance;
		}
	}

	[CompilerGenerated]
	public class Class622
	{
		public EMuzzleParticlePivot pivot;

		public bool method_0(MuzzleParticleContainer container)
		{
			return container.Pivot == pivot;
		}
	}

	[CompilerGenerated]
	public class Class623
	{
		public GameObject[] launcherGo;

		public bool method_0(MuzzleEffect x)
		{
			return !launcherGo.Contains(x.gameObject);
		}
	}

	private const string string_0 = "Muzzle Light";

	public Material JetMaterial;

	public int AtlasXCount;

	public int AtlasYCount;

	public AnimationCurve MoveCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

	public AnimationCurve JetLightCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

	public float ShotLength = 0.1f;

	public bool TestPlay;

	public bool TestShoot;

	public bool TestHold;

	public float TestDebugPosition;

	public float TestDelay;

	public MuzzleLight Light;

	public string MeshParentName = "weapon";

	private MuzzleJet[] muzzleJet_0;

	private MuzzleSparks[] muzzleSparks_0;

	private MuzzleFume[] muzzleFume_0;

	private MuzzleFume[] muzzleFume_1;

	private MuzzleSmoke[] muzzleSmoke_0;

	private MuzzleParticlePivot[] muzzleParticlePivot_0;

	private HeatEmitter[] heatEmitter_0;

	private HeatHazeEmitter[] heatHazeEmitter_0;

	private Vector2 vector2_0;

	private float float_0;

	public Transform Hierarchy;

	private float float_1;

	private static readonly int int_0 = Shader.PropertyToID("_ShotVals");

	private float float_2;

	private float float_3;

	private bool bool_0;

	private bool bool_1;

	[CompilerGenerated]
	private GameObject[] gameObject_0;

	public GameObject[] MuzzleJets
	{
		[CompilerGenerated]
		get
		{
			return gameObject_0;
		}
		[CompilerGenerated]
		set
		{
			gameObject_0 = value;
		}
	}

	public void Awake()
	{
		if (JetLightCurve.postWrapMode == WrapMode.ClampForever && MoveCurve.postWrapMode == WrapMode.ClampForever)
		{
			float_2 = Mathf.Max(GClass842.GetDuration(JetLightCurve), GClass842.GetDuration(MoveCurve));
		}
		else
		{
			float_2 = float.MaxValue;
		}
		float_3 = ((!(Light != null) || Light.LightIntensityCurve.postWrapMode != WrapMode.ClampForever) ? float.MaxValue : GClass842.GetDuration(Light.LightIntensityCurve));
	}

	public void ManualUpdate()
	{
		float num = 1f - (float_0 - Time.time) / ShotLength;
		if (num <= float_2 || !bool_0)
		{
			SetT(num);
			bool_0 = num > float_2;
		}
		method_1();
	}

	public void ManualLateUpdate()
	{
		float num = 1f - (float_0 - Time.time) / ShotLength;
		if (num <= float_3 || !bool_1)
		{
			method_2(num);
			bool_1 = num > float_3;
		}
		Effects instance = Singleton<Effects>.Instance;
		if ((bool)instance)
		{
			instance.UpdateMBOITIndoorAttenuation();
		}
	}

	public override void OnDisable()
	{
		base.OnDisable();
		method_3();
	}

	public void OnDestroy()
	{
		Effects instance = Singleton<Effects>.Instance;
		if (!(instance == null))
		{
			MuzzleParticleContainer[] commonSystems = instance.MuzzleEffect.CommonSystems;
			instance.TryRemoveFromMBOITParticleManager(commonSystems);
		}
	}

	public void SetT(float t)
	{
		if (JetMaterial != null)
		{
			JetMaterial.SetVector(int_0, new Vector4(1f - JetLightCurve.Evaluate(t), MoveCurve.Evaluate(t), 0f, 0f));
		}
	}

	public void LauncherShot()
	{
		if (muzzleFume_1 != null)
		{
			for (int i = 0; i < muzzleFume_1.Length; i++)
			{
				muzzleFume_1[i].Emit(this);
			}
		}
	}

	public void Shot(bool isVisible = true, float sqrCameraDistance = 0f)
	{
		float_0 = Time.time + ShotLength;
		if (JetMaterial != null)
		{
			MuzzleJet.RandomizeMaterial(JetMaterial, vector2_0);
		}
		if (muzzleParticlePivot_0 != null && (isVisible || (!isVisible && sqrCameraDistance < 4f)))
		{
			for (int i = 0; i < muzzleParticlePivot_0.Length; i++)
			{
				muzzleParticlePivot_0[i].Play(this);
			}
		}
		if (muzzleSparks_0 != null && (isVisible || (!isVisible && sqrCameraDistance < 4f)))
		{
			for (int j = 0; j < muzzleSparks_0.Length; j++)
			{
				muzzleSparks_0[j].Emit(this);
			}
		}
		if (muzzleFume_0 != null && ((isVisible && sqrCameraDistance < 100f) || (!isVisible && sqrCameraDistance < 4f)))
		{
			for (int k = 0; k < muzzleFume_0.Length; k++)
			{
				muzzleFume_0[k].Emit(this);
			}
		}
		if (muzzleSmoke_0 != null && ((isVisible && sqrCameraDistance < 100f) || (!isVisible && sqrCameraDistance < 4f)))
		{
			for (int l = 0; l < muzzleSmoke_0.Length; l++)
			{
				muzzleSmoke_0[l].Shot();
			}
		}
		if (float_1 > 0f && (isVisible || (!isVisible && sqrCameraDistance < 400f)))
		{
			Light.method_0();
		}
		if (heatEmitter_0 != null && (isVisible || (!isVisible && sqrCameraDistance < 400f)))
		{
			for (int m = 0; m < heatEmitter_0.Length; m++)
			{
				heatEmitter_0[m].OnShot();
			}
		}
		if (heatHazeEmitter_0 != null && (isVisible || (!isVisible && sqrCameraDistance < 400f)))
		{
			for (int n = 0; n < heatHazeEmitter_0.Length; n++)
			{
				heatHazeEmitter_0[n].OnShot(this);
			}
		}
	}

	public void Play(EMuzzleParticlePivot pivot, Transform pTransform)
	{
		Effects instance = Singleton<Effects>.Instance;
		MuzzleParticleContainer[] commonSystems = instance.MuzzleEffect.CommonSystems;
		instance.TryAddToMBOITParticleManager(commonSystems);
		ParticleSystem rootParticleSystem = commonSystems.First((MuzzleParticleContainer container) => container.Pivot == pivot).RootParticleSystem;
		rootParticleSystem.transform.position = pTransform.position;
		rootParticleSystem.transform.rotation = pTransform.rotation;
		rootParticleSystem.Stop(withChildren: true);
		rootParticleSystem.Play(withChildren: true);
	}

	void GInterface53.Play(EMuzzleParticlePivot pivot, Transform pTransform)
	{
		//ILSpy generated this explicit interface implementation from .override directive in Play
		this.Play(pivot, pTransform);
	}

	public void Emit(Vector3 position, Vector3 velocity, float time, float gravity, float drag, float lifeTime, byte emission, byte size, byte turbulence, byte frequency)
	{
		Singleton<Effects>.Instance.MuzzleEffect.Spark.EmitSeg(position, velocity, time, gravity, drag, lifeTime, emission, size, turbulence, frequency);
	}

	void GInterface54.Emit(Vector3 position, Vector3 velocity, float time, float gravity, float drag, float lifeTime, byte emission, byte size, byte turbulence, byte frequency)
	{
		//ILSpy generated this explicit interface implementation from .override directive in Emit
		this.Emit(position, velocity, time, gravity, drag, lifeTime, emission, size, turbulence, frequency);
	}

	public void Emit_1(Vector3 position, Vector3 velocity, float size, float lifetime, Color32 color)
	{
		Singleton<Effects>.Instance.MuzzleEffect.Fume.Emit(new ParticleSystem.EmitParams
		{
			position = position,
			startLifetime = lifetime,
			velocity = velocity,
			randomSeed = GClass2608.Uint(),
			startColor = color,
			startSize = size,
			rotation = GClass2608.FloatRotation()
		}, 1);
	}

	void GInterface52.Emit(Vector3 position, Vector3 velocity, float size, float lifetime, Color32 color)
	{
		//ILSpy generated this explicit interface implementation from .override directive in Emit_1
		this.Emit_1(position, velocity, size, lifetime, color);
	}

	public void Emit_1(Vector3 position, float rotation, Vector3 velocity, float size, float lifetime, Color32 color)
	{
		Singleton<Effects>.Instance.MuzzleEffect.HeatHaze.Emit(new ParticleSystem.EmitParams
		{
			position = position,
			rotation = rotation,
			velocity = velocity,
			startSize = size,
			startLifetime = lifetime,
			startColor = color
		}, 1);
	}

	void GInterface50.Emit(Vector3 position, float rotation, Vector3 velocity, float size, float lifetime, Color32 color)
	{
		//ILSpy generated this explicit interface implementation from .override directive in Emit_1
		this.Emit_1(position, rotation, velocity, size, lifetime, color);
	}

	public void UpdateJetsAndFumes()
	{
		Transform transform = TransformHelperClass.FindTransform(Hierarchy, "mod_launcher");
		muzzleFume_1 = (transform ? GClass6.GetComponentsInChildrenActiveIgnoreFirstLevel<MuzzleFume>(transform).ToArray() : new MuzzleFume[0]);
		GameObject[] launcherGo = muzzleFume_1.Select((MuzzleFume f) => f.gameObject).ToArray();
		MuzzleJets = (from x in Hierarchy.GetComponentsInChildren<MuzzleEffect>(includeInactive: true)
			where !launcherGo.Contains(x.gameObject)
			select x.gameObject).Distinct().ToArray();
		List<GameObject> list = new List<GameObject>();
		int num = 0;
		GameObject gameObject = null;
		for (int num2 = 0; num2 < MuzzleJets.Length; num2++)
		{
			if (MuzzleJets[num2].name == "extractor_smoke")
			{
				gameObject = MuzzleJets[num2];
				continue;
			}
			int num3 = TransformHelperClass.NumParents(MuzzleJets[num2].transform, Hierarchy);
			if (num < num3)
			{
				list.Clear();
				list.Add(MuzzleJets[num2]);
				num = num3;
			}
			else if (num == num3)
			{
				list.Add(MuzzleJets[num2]);
			}
		}
		if (gameObject != null)
		{
			list.Add(gameObject);
		}
		MuzzleJets = list.ToArray();
		muzzleParticlePivot_0 = method_0<MuzzleParticlePivot>();
		muzzleFume_0 = method_0<MuzzleFume>();
		muzzleSmoke_0 = method_0<MuzzleSmoke>();
		muzzleSparks_0 = method_0<MuzzleSparks>();
		muzzleJet_0 = method_0<MuzzleJet>();
		float_1 = muzzleJet_0.Sum((MuzzleJet x) => x.Chance);
		vector2_0 = new Vector2(1f / (float)AtlasXCount, 1f / (float)AtlasYCount);
		if (JetMaterial != null)
		{
			MuzzleJet.UpdateOrCreateMesh(muzzleJet_0, TransformHelperClass.FindTransform(Hierarchy, MeshParentName) ?? Hierarchy, JetMaterial, vector2_0);
		}
		heatHazeEmitter_0 = method_0<HeatHazeEmitter>();
	}

	public T[] method_0<T>() where T : MonoBehaviour
	{
		List<T> list = new List<T>();
		for (int i = 0; i < MuzzleJets.Length; i++)
		{
			T[] components = MuzzleJets[i].GetComponents<T>();
			if (components.Length != 0)
			{
				list.AddRange(components);
			}
		}
		return list.ToArray();
	}

	public void method_1()
	{
		if (muzzleFume_0 != null)
		{
			for (int i = 0; i < muzzleFume_0.Length; i++)
			{
				muzzleFume_0[i].UpdateValues();
			}
		}
	}

	public void LateUpdateMuzzleEffectsValues(Camera cam)
	{
		if (muzzleSmoke_0 == null)
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		if (deltaTime < float.Epsilon)
		{
			return;
		}
		for (int i = 0; i < muzzleSmoke_0.Length; i++)
		{
			MuzzleSmoke muzzleSmoke = muzzleSmoke_0[i];
			if (!(muzzleSmoke == null) && muzzleSmoke.enabled)
			{
				muzzleSmoke.LateUpdateValues(cam, deltaTime);
			}
		}
	}

	public void method_2(float t)
	{
		if (Light != null && muzzleJet_0 != null && muzzleJet_0.Length != 0 && float_1 > 0f)
		{
			Light.SetIntensity(Light.LightIntensityCurve.Evaluate(t));
		}
	}

	public void method_3()
	{
		if (muzzleSmoke_0 == null)
		{
			return;
		}
		for (int num = muzzleSmoke_0.Length - 1; num >= 0; num--)
		{
			MuzzleSmoke muzzleSmoke = muzzleSmoke_0[num];
			if (muzzleSmoke != null)
			{
				muzzleSmoke.Clear();
			}
		}
	}

	public void method_4()
	{
		if (!(Hierarchy == null))
		{
			Light = GClass6.GetComponentInChildrenActiveIgnoreFirstLevel<MuzzleLight>(Hierarchy);
			if (!(Light != null))
			{
				Light = new GameObject("Muzzle Light", typeof(MuzzleLight)).GetComponent<MuzzleLight>();
				Transform transform = TransformHelperClass.FindTransformRecursive(Hierarchy, "fireport");
				Transform transform2 = ((transform == null) ? Hierarchy : transform.transform);
				Light.transform.parent = transform2.parent;
				Light.transform.position = transform2.position;
				Light.transform.localPosition = new Vector3(Light.transform.localPosition.x - 0.05f, Light.transform.localPosition.y, Light.transform.localPosition.z);
			}
		}
	}
}
