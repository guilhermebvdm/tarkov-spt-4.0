using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Audio.SpatialSystem;
using Comfort.Common;
using DeferredDecals;
using EFT;
using EFT.Ballistics;
using EFT.EnvironmentEffect;
using EFT.Hideout.ShootingRange;
using EFT.Particles;
using UnityEngine;
using UnityEngine.Audio;

namespace Systems.Effects;

public class Effects : MonoBehaviour
{
	[Serializable]
	public struct MuzzleEffectContainer
	{
		public SimpleSparksRenderer Spark;

		public ParticleSystem Fume;

		public ParticleSystem Heat;

		public ParticleSystem HeatHaze;

		public MuzzleParticleContainer[] CommonSystems;
	}

	[Serializable]
	public class EmissionEffect
	{
		public string Key;

		public GrenadeEmission Instance;

		[NonSerialized]
		public List<GrenadeEmission> Cache;

		public GrenadeEmission GetEffect()
		{
			if (Instance == null)
			{
				return null;
			}
			if (Cache.Count < 1)
			{
				InstantiateNewEffect();
			}
			GrenadeEmission grenadeEmission = Cache[0];
			Cache.RemoveAt(0);
			grenadeEmission.gameObject.SetActive(value: true);
			return grenadeEmission;
		}

		public void Dispose(GrenadeEmission e)
		{
			Cache.Add(e);
			e.Clear();
			e.gameObject.SetActive(value: false);
		}

		public void InstantiateNewEffect()
		{
			if (Cache == null)
			{
				Cache = new List<GrenadeEmission>(3);
			}
			GrenadeEmission grenadeEmission = UnityEngine.Object.Instantiate(Instance);
			grenadeEmission.transform.parent = Singleton<Effects>.Instance.transform;
			grenadeEmission.gameObject.SetActive(value: false);
			grenadeEmission.PoolName = Key;
			Cache.Add(grenadeEmission);
		}
	}

	public struct GStruct73
	{
		public Effect Effect;

		public Vector3 Position;

		public Vector3 Normal;

		public BallisticCollider HitCollider;

		public bool WithDecal;

		public float Volume;

		public bool IsKnife;

		public bool IsHitPointVisible;

		public bool IsGrenade;

		public EPointOfView Pov;
	}

	[Serializable]
	public class Effect
	{
		[Serializable]
		public class ParticleSys
		{
			public enum Type
			{
				Forward,
				Cone,
				Hemisphere,
				Circle,
				ConeNormalized,
				Cone60
			}

			public Emitter Particle;

			public Vector2 Distance;

			public Type RandomType;

			public int MinCount;

			public int RandomCountRange;

			public bool UseRandomScale;

			public Vector3 RandomScale;

			public void Emit(Vector3 position, Vector3 normal, float distance)
			{
				if (distance < Distance.x || !(distance <= Distance.y))
				{
					return;
				}
				switch (RandomType)
				{
				case Type.Forward:
				{
					int num2 = ((RandomCountRange < 1) ? MinCount : GClass2608.Int(MinCount, RandomCountRange));
					for (int j = 0; j < num2; j++)
					{
						Vector3 vector2 = normal;
						vector2 = (UseRandomScale ? Vector3.Scale(vector2, RandomScale) : vector2);
						Particle.Emit(position, vector2);
					}
					break;
				}
				case Type.Cone:
				{
					int num5 = ((RandomCountRange < 1) ? MinCount : GClass2608.Int(MinCount, RandomCountRange));
					for (int m = 0; m < num5; m++)
					{
						Vector3 vector5 = normal + GClass2608.VectorNormalized();
						vector5 = (UseRandomScale ? Vector3.Scale(vector5, RandomScale) : vector5);
						Particle.Emit(position, vector5);
					}
					break;
				}
				case Type.Hemisphere:
				{
					int num4 = ((RandomCountRange < 1) ? MinCount : GClass2608.Int(MinCount, RandomCountRange));
					for (int l = 0; l < num4; l++)
					{
						Vector3 vector4 = GClass2608.VectorHemisphere();
						vector4 = (UseRandomScale ? Vector3.Scale(vector4, RandomScale) : vector4);
						Particle.Emit(position, vector4);
					}
					break;
				}
				case Type.Circle:
				{
					int num3 = ((RandomCountRange < 1) ? MinCount : GClass2608.Int(MinCount, RandomCountRange));
					for (int k = 0; k < num3; k++)
					{
						Vector3 vector3 = GClass2608.VectorCircle();
						vector3 = (UseRandomScale ? Vector3.Scale(vector3, RandomScale) : vector3);
						Vector3 pos = position;
						pos.y += 0.3f;
						Particle.Emit(pos, vector3);
					}
					break;
				}
				case Type.ConeNormalized:
				{
					int num6 = ((RandomCountRange < 1) ? MinCount : GClass2608.Int(MinCount, RandomCountRange));
					for (int n = 0; n < num6; n++)
					{
						Vector3 vector6 = Vector3.Normalize(normal + GClass2608.VectorHemisphere());
						vector6 = (UseRandomScale ? Vector3.Scale(vector6, RandomScale) : vector6);
						Particle.Emit(position, vector6);
					}
					break;
				}
				case Type.Cone60:
				{
					int num = ((RandomCountRange < 1) ? MinCount : GClass2608.Int(MinCount, RandomCountRange));
					for (int i = 0; i < num; i++)
					{
						Vector3 vector = GClass2608.Cone60();
						vector = (UseRandomScale ? Vector3.Scale(vector, RandomScale) : vector);
						Particle.Emit(position, vector);
					}
					break;
				}
				}
			}
		}

		public string Name;

		public MaterialType[] MaterialTypes;

		public BasicParticleSystemMediator BasicParticleSystemMediator;

		public ParticleSys[] Particles = new ParticleSys[0];

		public SoundBank Sound;

		public SoundBank SoundFP;

		public DecalSystem Decal;

		public bool Flash;

		public int FlareID;

		public float FlashMaxDist = 50f;

		public float FlashTime = 0.07f;

		public bool Light;

		public Color LightColor = new Color(1f, 0.83f, 0.45f, 1f);

		public float LightMaxDist = 50f;

		public float LightRange = 1.5f;

		public float LightIntensity = 5f;

		public float LightTime = 0.1f;

		public float ParticlesShift;

		public bool WithShadows;

		public bool Wind;

		public float WindIntensity = 1f;

		public float WindRadius = 20f;

		public float WindTime = 1f;

		public float WindMaxDist = 200f;

		public const string CHOKE_IMPACT_KEY = "Impact";

		public const string CHOKE_GRENADE_KEY = "Grenade";

		public bool UseDeferredDecals;

		[HideInInspector]
		public DeferredDecalRenderer DeferredDecals;

		public LightAllocationPoolClass LightPool;

		[NonSerialized]
		public Vector2 ImpactsGagRadius = new Vector2(1f, 3f);

		public void Emit(Vector3 position, Vector3 normal, BallisticCollider hitCollider, bool withDecal = true, float volume = 1f, bool isKnife = false, bool isHitPointVisible = true, bool isGrenade = false, EPointOfView pov = EPointOfView.ThirdPerson)
		{
			if (withDecal && !isKnife && !(hitCollider is HideoutTargetBallisticCollider))
			{
				if ((!UseDeferredDecals || (UseDeferredDecals && !EFTHardSettings.Instance.DEFERRED_DECALS_ENABLED)) && Decal != null)
				{
					Decal.Add(position, normal);
				}
				if (UseDeferredDecals && DeferredDecals != null && (MaterialTypes.Length != 0 || isGrenade) && EFTHardSettings.Instance.DEFERRED_DECALS_ENABLED)
				{
					DeferredDecals.DrawDecal(position, normal, hitCollider, isGrenade);
				}
			}
			float num = CameraClass.Instance.Distance(position);
			Vector3 position2 = position + normal * 0.05f;
			if (isHitPointVisible && pov == EPointOfView.ThirdPerson)
			{
				if (BasicParticleSystemMediator != null)
				{
					BasicParticleSystemMediator.Emit(position, Quaternion.LookRotation(normal));
				}
				for (int i = 0; i < Particles.Length; i++)
				{
					Particles[i].Emit(position + normal * ParticlesShift, normal, num);
				}
				if (Light && num < LightMaxDist)
				{
					LightPool.Add(position2, LightColor, LightRange, LightIntensity, LightTime, WithShadows);
				}
			}
			PlaySound(position2, num, volume, GClass2078.IsFirstPerson(pov), isKnife);
		}

		public void PlaySound(Vector3 position, float distance, float volume, bool isFirstPerson, bool isMeleeWeapon = false)
		{
			if (volume <= 0f)
			{
				return;
			}
			BetterAudio instance = MonoBehaviourSingleton<BetterAudio>.Instance;
			if (isFirstPerson && SoundFP != null)
			{
				AudioClip clip = SoundFP.PickSingleClip(0);
				instance.PlayNonspatial(clip, BetterAudio.AudioSourceGroupType.Nonspatial, 0f, SoundFP.RandomVolume);
			}
			else if (Sound != null)
			{
				EnvironmentType env = EnvironmentType.Outdoor;
				if (Sound.HasEnvironment)
				{
					EnvironmentManager instance2 = EnvironmentManager.Instance;
					env = ((!(instance2 == null)) ? instance2.GetEnvironmentByPos(position) : EnvironmentType.Outdoor);
				}
				if (Sound.Physical)
				{
					instance.PlayAtPointDistant(position, Sound, distance, volume, Mathf.Clamp(0.2f, 0.9f, distance / 30f), env, EOcclusionTest.OneShotPropagation, null, needUpdate: false);
				}
				else if (Sound.AllowLimitedPlay || isMeleeWeapon)
				{
					instance.LimitedPlay(out var _, position, Sound, distance, ImpactsGagRadius, 0.3f, volume, -1f, env, EOcclusionTest.OneShotPropagation, "Impact", needUpdate: false, oneShot: false);
				}
				else
				{
					instance.PlayAtPoint(position, Sound, distance, volume, -1f, env, EOcclusionTest.OneShotFullOcclusion, oneShot: false);
				}
			}
		}

		public void Clear()
		{
			DeferredDecals.Clear();
			Decal?.Clear();
		}
	}

	[CompilerGenerated]
	public class Class710
	{
		public string key;

		public bool _003CGetEmissionEffect_003Eb__0(EmissionEffect x)
		{
			return x.Key == key;
		}
	}

	[CompilerGenerated]
	public class Class711
	{
		public GrenadeEmission emission;

		public bool _003CDisposeEmissionEffect_003Eb__0(EmissionEffect x)
		{
			return x.Key == emission.PoolName;
		}
	}

	private static readonly int int_0 = Shader.PropertyToID("_Indoor");

	private static readonly int int_1 = 10;

	public DeferredDecalRenderer DeferredDecals;

	public bool UseDecalPainter;

	public TextureDecalsPainter TexDecals;

	public MuzzleEffectContainer MuzzleEffect;

	public SoundBank[] AdditionalSoundEffects;

	public Effect[] EffectsArray;

	public EmissionEffect[] EmissionEffects;

	private EffectsCommutator effectsCommutator_0;

	private LightAllocationPoolClass lightAllocationPoolClass;

	private float float_0;

	private Dictionary<MaterialType, Effect> dictionary_0;

	private Dictionary<string, Effect> dictionary_1;

	private List<GStruct73> list_0 = new List<GStruct73>(int_1);

	public EffectsCommutator EffectsCommutator => effectsCommutator_0;

	public void OnValidate()
	{
		if (Application.isPlaying)
		{
			InitDictionaryAndNames();
		}
	}

	public void Awake()
	{
		effectsCommutator_0 = GetComponent<EffectsCommutator>();
		list_0 = new List<GStruct73>(int_1);
		lightAllocationPoolClass = new LightAllocationPoolClass();
		lightAllocationPoolClass.Init();
		InitDictionaryAndNames();
		Dictionary<Emitter, List<Effect>> dictionary = new Dictionary<Emitter, List<Effect>>();
		Effect[] effectsArray = EffectsArray;
		foreach (Effect effect in effectsArray)
		{
			Effect.ParticleSys[] particles = effect.Particles;
			foreach (Effect.ParticleSys particleSys in particles)
			{
				if (!dictionary.TryGetValue(particleSys.Particle, out var value))
				{
					dictionary.Add(particleSys.Particle, value = new List<Effect>());
				}
				value.Add(effect);
			}
		}
		Singleton<BetterAudio>.Instance.PrecacheGag("Impact");
		Singleton<BetterAudio>.Instance.PrecacheGag("Grenade");
	}

	public void InitDictionaryAndNames()
	{
		dictionary_0 = new Dictionary<MaterialType, Effect>(EffectsArray.Length, GClass866<MaterialType>.EqualityComparer);
		dictionary_1 = new Dictionary<string, Effect>(EffectsArray.Length);
		Effect[] effectsArray = EffectsArray;
		foreach (Effect effect in effectsArray)
		{
			effect.DeferredDecals = DeferredDecals;
			effect.LightPool = lightAllocationPoolClass;
			MaterialType[] materialTypes = effect.MaterialTypes;
			foreach (MaterialType key in materialTypes)
			{
				if (!dictionary_0.ContainsKey(key))
				{
					dictionary_0.Add(key, effect);
				}
			}
			dictionary_1.Add(effect.Name, effect);
		}
	}

	public void OnDestroy()
	{
		lightAllocationPoolClass.Destroy();
	}

	public void Update()
	{
		lightAllocationPoolClass.Update();
		if (list_0.Count != 0)
		{
			int num = Mathf.Min(int_1, list_0.Count);
			for (int i = 0; i < num; i++)
			{
				GStruct73 gStruct = list_0[i];
				gStruct.Effect.Emit(gStruct.Position, gStruct.Normal, gStruct.HitCollider, gStruct.WithDecal, gStruct.Volume, gStruct.IsKnife, gStruct.IsHitPointVisible, gStruct.IsGrenade, gStruct.Pov);
			}
			list_0.Clear();
		}
	}

	public void AddEffectEmit(Effect effect, Vector3 position, Vector3 normal, BallisticCollider hitCollider, bool withDecal = true, float volume = 1f, bool isKnife = false, bool isHitPointVisible = true, bool isGrenade = false, EPointOfView pov = EPointOfView.ThirdPerson)
	{
		GStruct73 item = new GStruct73
		{
			Effect = effect,
			Position = position,
			Normal = normal,
			HitCollider = hitCollider,
			WithDecal = withDecal,
			Volume = volume,
			IsKnife = isKnife,
			IsHitPointVisible = isHitPointVisible,
			IsGrenade = isGrenade,
			Pov = pov
		};
		list_0.Add(item);
	}

	public void PlayerMeshesHit(List<BodyRendererDataStruct> renderers, Vector3 point, Vector3 direction)
	{
		if (UseDecalPainter && TexDecals != null)
		{
			TexDecals.DrawDecal(renderers, point, direction);
		}
	}

	public void EmitBloodOnEnvironment(Vector3 position, Vector3 normal)
	{
		if (DeferredDecals != null)
		{
			DeferredDecals.EmitBloodOnEnvironment(position, normal);
		}
	}

	public void EmitBleeding(Vector3 position, Vector3 normal)
	{
		if (DeferredDecals != null)
		{
			DeferredDecals.EmitBleeding(position, normal);
		}
	}

	public void Emit(MaterialType material, BallisticCollider hitCollider, Vector3 position, Vector3 normal, float volume = 1f, bool isKnife = false, bool isHitPointVisible = true, EPointOfView pov = EPointOfView.ThirdPerson)
	{
		if (dictionary_0.TryGetValue(material, out var value))
		{
			AddEffectEmit(value, position, normal, hitCollider, withDecal: true, volume, isKnife, isHitPointVisible, isGrenade: false, pov);
		}
	}

	public void PrewarmEmit(MaterialType material, BallisticCollider hitCollider, Vector3 position, Vector3 normal, float volume = 1f, bool isKnife = false, bool isHitPointVisible = true, EPointOfView pov = EPointOfView.ThirdPerson)
	{
		if (dictionary_0.TryGetValue(material, out var value))
		{
			value.Emit(position, normal, hitCollider, withDecal: true, volume, isKnife, isHitPointVisible, isGrenade: false, pov);
		}
	}

	public void TestEmit(int id, Vector3 position, Vector3 normal)
	{
		EffectsArray[id].Emit(position, normal, null);
	}

	public void Emit(string ename, Vector3 position, Vector3 normal)
	{
		if (dictionary_1.ContainsKey(ename))
		{
			AddEffectEmit(dictionary_1[ename], position, normal, null);
		}
	}

	public GrenadeEmission GetEmissionEffect(string key)
	{
		return EmissionEffects.FirstOrDefault((EmissionEffect x) => x.Key == key)?.GetEffect();
	}

	public void DisposeEmissionEffect(GrenadeEmission emission)
	{
		EmissionEffect emissionEffect = EmissionEffects.FirstOrDefault((EmissionEffect x) => x.Key == emission.PoolName);
		if (emissionEffect != null)
		{
			emissionEffect.Dispose(emission);
		}
		else
		{
			UnityEngine.Object.Destroy(emission.gameObject);
		}
	}

	public void EmitPlayerSoundOnly(MaterialType material, IPlayer player, float volumeMult = 1f, AudioMixerGroup mixerGroup = null)
	{
		BetterAudio instance = MonoBehaviourSingleton<BetterAudio>.Instance;
		if (!dictionary_0.TryGetValue(material, out var value) || instance == null)
		{
			return;
		}
		if (player.IsYourPlayer && value.SoundFP != null)
		{
			AudioClip clip = value.SoundFP.PickSingleClip(0);
			instance.PlayNonspatial(clip, BetterAudio.AudioSourceGroupType.Nonspatial, 0f, value.SoundFP.RandomVolume * volumeMult, mixerGroup);
		}
		else if (value.Sound != null)
		{
			AudioClip clip2 = value.Sound.PickSingleClip(0);
			if (instance.TryPlayAtPoint(out var source, player.Position, clip2, BetterAudio.AudioSourceGroupType.Character, (int)value.Sound.Rolloff, value.Sound.RandomVolume * volumeMult, EOcclusionTest.None, mixerGroup) && MonoBehaviourSingleton<SpatialAudioSystem>.Instantiated)
			{
				MonoBehaviourSingleton<SpatialAudioSystem>.Instance.ProcessSourceOcclusion(player, source);
			}
		}
	}

	public void EmitGrenade(string ename, Vector3 position, Vector3 normal, float volume = 1f)
	{
		if (dictionary_1.ContainsKey(ename))
		{
			AddEffectEmit(dictionary_1[ename], position, normal, null, withDecal: true, volume, isKnife: false, isHitPointVisible: true, isGrenade: true);
		}
	}

	public void ClearEffects(MaterialType materialType)
	{
		if (dictionary_0.TryGetValue(materialType, out var value))
		{
			value.Clear();
		}
	}

	public Effect Get(MaterialType id)
	{
		if (!dictionary_0.TryGetValue(id, out var value))
		{
			return null;
		}
		return value;
	}

	public void ClearDecal()
	{
		TexDecals.Clear();
		foreach (Effect value in dictionary_0.Values)
		{
			value.Clear();
		}
	}

	public void CacheEffects()
	{
		EmissionEffect[] emissionEffects = EmissionEffects;
		for (int i = 0; i < emissionEffects.Length; i++)
		{
			emissionEffects[i].InstantiateNewEffect();
		}
	}

	public void TryAddToMBOITParticleManager(MuzzleParticleContainer[] containers)
	{
		if (!WindowsManager.InstanceIsActive())
		{
			return;
		}
		GClass1000 particlesManager = WindowsManager.Instance.ParticlesManager;
		if (particlesManager == null || containers == null || containers.Length == 0)
		{
			return;
		}
		for (int i = 0; i < containers.Length; i++)
		{
			MuzzleParticleContainer.GStruct68[] muzzleParticleData = containers[i].MuzzleParticleData;
			for (int j = 0; j < muzzleParticleData.Length; j++)
			{
				MuzzleParticleContainer.GStruct68 gStruct = muzzleParticleData[j];
				if (WindowsManager.IsParticleSystemRendererSuitableForMBOIT(gStruct.Renderer))
				{
					particlesManager.AddParticleSystem(gStruct.System, isDynamic: true);
				}
			}
		}
	}

	public void TryRemoveFromMBOITParticleManager(MuzzleParticleContainer[] containers)
	{
		if (!WindowsManager.InstanceIsActive())
		{
			return;
		}
		GClass1000 particlesManager = WindowsManager.Instance.ParticlesManager;
		if (particlesManager == null || containers == null || containers.Length == 0)
		{
			return;
		}
		for (int i = 0; i < containers.Length; i++)
		{
			MuzzleParticleContainer.GStruct68[] muzzleParticleData = containers[i].MuzzleParticleData;
			for (int j = 0; j < muzzleParticleData.Length; j++)
			{
				MuzzleParticleContainer.GStruct68 gStruct = muzzleParticleData[j];
				if (WindowsManager.IsParticleSystemRendererSuitableForMBOIT(gStruct.Renderer))
				{
					particlesManager.RemoveParticleSystem(gStruct.System);
				}
			}
		}
	}

	public void UpdateMBOITIndoorAttenuation()
	{
		if (!EnvironmentManager.Instance)
		{
			return;
		}
		MuzzleParticleContainer[] commonSystems = MuzzleEffect.CommonSystems;
		if (commonSystems != null && commonSystems.Length != 0)
		{
			MuzzleParticleContainer[] array = commonSystems;
			foreach (MuzzleParticleContainer muzzleParticleContainer in array)
			{
				EnvironmentType environmentByPos = EnvironmentManager.Instance.GetEnvironmentByPos(muzzleParticleContainer.transform.position);
				UpdateMBOITIndoorAttenuation(environmentByPos == EnvironmentType.Indoor, muzzleParticleContainer);
			}
		}
	}

	public void UpdateMBOITIndoorAttenuation(bool indoor, MuzzleParticleContainer container)
	{
		if ((indoor && float_0 >= 1f) | (!indoor && float_0 <= 0f))
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		deltaTime = (indoor ? deltaTime : (0f - deltaTime));
		float_0 += deltaTime;
		float_0 = Mathf.Clamp01(float_0);
		MuzzleParticleContainer.GStruct68[] muzzleParticleData = container.MuzzleParticleData;
		for (int i = 0; i < muzzleParticleData.Length; i++)
		{
			MuzzleParticleContainer.GStruct68 gStruct = muzzleParticleData[i];
			gStruct.Material.SetFloat(int_0, float_0);
			if (WindowsManager.InstanceIsActive())
			{
				WindowsManager.Instance.ParticlesManager?.UpdateParticleSystemMaterial(gStruct.System, gStruct.Material);
			}
		}
	}
}
