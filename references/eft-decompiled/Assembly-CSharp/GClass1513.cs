using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Comfort.Common;
using CommonAssets.Scripts.ArtilleryShelling.Client;
using CommonAssets.Scripts.ArtilleryShelling.Client.Audio;
using EFT;
using EFT.GlobalEvents.ArtilleryShellingEcents;
using UnityEngine;

public class GClass1513 : IDisposable
{
	[CompilerGenerated]
	public class Class1004
	{
		public GClass1513 gclass1513_0;

		public int projectileID;

		public BetterSource source;

		public void method_0()
		{
			gclass1513_0.method_10(projectileID, source);
		}
	}

	[NonSerialized]
	public const float Float_0 = 0.3f;

	[NonSerialized]
	public StaticManager StaticManager_0;

	[NonSerialized]
	public IEasyAssets IEasyAssets;

	[NonSerialized]
	public IReadOnlyDictionary<int, ArtilleryProjectileClient> IreadOnlyDictionary_0;

	[NonSerialized]
	public Dictionary<int, BetterSource> Dictionary_0 = new Dictionary<int, BetterSource>();

	[NonSerialized]
	public Dictionary<int, Coroutine> Dictionary_1 = new Dictionary<int, Coroutine>();

	[NonSerialized]
	public global::DependencyGraphClass<IEasyBundle>.GClass1661 Gclass1661_0;

	[NonSerialized]
	public ArtilleryShellingSoundsSO ArtilleryShellingSoundsSO_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public Action Action_0;

	public GClass1513(IReadOnlyDictionary<int, ArtilleryProjectileClient> activeClientProjectiles)
	{
		IreadOnlyDictionary_0 = activeClientProjectiles;
		StaticManager_0 = StaticManager.Instance;
		IEasyAssets = Singleton<IEasyAssets>.Instance;
	}

	public async Task Init()
	{
		ArtilleryShellingSoundsSO_0 = await method_0();
		method_1();
	}

	public async Task<ArtilleryShellingSoundsSO> method_0()
	{
		Gclass1661_0 = GClass1857.Retain(IEasyAssets, new string[1] { "assets/content/audio/data/soundcontainers/artilleryshellingsounds.bundle" });
		await GClass1857.LoadBundles(Gclass1661_0);
		return GClass1857.GetAsset<ArtilleryShellingSoundsSO>(IEasyAssets, "assets/content/audio/data/soundcontainers/artilleryshellingsounds.bundle");
	}

	public void method_1()
	{
		GlobalEventHandlerClass instance = GlobalEventHandlerClass.Instance;
		Action_0 = instance.SubscribeOnEvent<GClass3577>(method_2);
		Action_0 = (Action)Delegate.Combine(Action_0, instance.SubscribeOnEvent<GClass3578>(method_8));
		Action_0 = (Action)Delegate.Combine(Action_0, instance.SubscribeOnEvent<InitShellingProjectileFlyEvent>(method_9));
	}

	public void method_2(GClass3577 createEvent)
	{
		float distanceToListener = CameraClass.Instance.Distance(createEvent.StartPosition);
		method_3(createEvent.StartPosition, distanceToListener);
	}

	public void method_3(Vector3 startPosition, float distanceToListener)
	{
		float delay = GClass2313.CalculateSoundDelay(distanceToListener);
		StaticManager_0.StartCoroutine(method_4(delay, startPosition, distanceToListener));
	}

	public IEnumerator method_4(float delay, Vector3 startPosition, float distanceToListener)
	{
		yield return new WaitForSeconds(delay);
		if (!Bool_0)
		{
			method_5(startPosition, distanceToListener);
		}
	}

	public void method_5(Vector3 startPosition, float distanceToListener)
	{
		AudioMultipleClipContainer mortarShots = ArtilleryShellingSoundsSO_0.mortarShots;
		BetterSource betterSource = MonoBehaviourSingleton<BetterAudio>.Instance.PlayAtPoint(startPosition, mortarShots.GetClip(), distanceToListener, BetterAudio.AudioSourceGroupType.Gunshots, mortarShots.GetMaxDistance(), mortarShots.GetVolume(), EOcclusionTest.None, MonoBehaviourSingleton<BetterAudio>.Instance.CommonAmbientOutEffectsMixer, forceStereo: true);
		if (!(betterSource == null))
		{
			Vector2 mortarShotPitchRange = ArtilleryShellingSoundsSO_0.mortarShotPitchRange;
			float pitch = UnityEngine.Random.Range(mortarShotPitchRange.x, mortarShotPitchRange.y);
			betterSource.SetPitch(pitch);
		}
	}

	public void method_6(BetterSource source, AudioClip clip, float rolloff, float volume, float pitch, Transform parent)
	{
		source.StartTrackingPosition(parent);
		source.Loop = false;
		source.EnableSpatialization = false;
		source.source1.dopplerLevel = ArtilleryShellingSoundsSO_0.shellWhistleDopplerLevel;
		source.source1.rolloffMode = AudioRolloffMode.Custom;
		source.source1.SetCustomCurve(AudioSourceCurveType.CustomRolloff, ArtilleryShellingSoundsSO_0.whistleRolloffCurve);
		source.SetPitch(pitch);
		source.SetRolloff(rolloff);
		source.SetMixerGroup(MonoBehaviourSingleton<BetterAudio>.Instance.CommonAmbientOutEffectsMixer);
		source.Play(clip, null, 1f, volume, forceStereo: true, oneShot: false);
	}

	public IEnumerator method_7(BetterSource source, int projectileID, IAudioClipContainer clipContainer, Vector3 startPos, Vector3 endPos, float speed)
	{
		Vector2 whistleVolumeRange = ArtilleryShellingSoundsSO_0.whistleVolumeRange;
		float volume = UnityEngine.Random.Range(whistleVolumeRange.x, whistleVolumeRange.y);
		Vector2 whistlePitchRange = ArtilleryShellingSoundsSO_0.whistlePitchRange;
		float num = UnityEngine.Random.Range(whistlePitchRange.x, whistlePitchRange.y);
		float num2 = Vector3.Distance(startPos, endPos);
		float num3 = num2 / speed;
		AudioClip clip = clipContainer.GetClip();
		float num4 = num3 - clip.length / num;
		float num5 = 0f;
		ArtilleryProjectileClient value;
		while (true)
		{
			if (!(num5 < num3))
			{
				yield break;
			}
			num5 += Time.deltaTime;
			if (num5 >= num4 && IreadOnlyDictionary_0.TryGetValue(projectileID, out value))
			{
				break;
			}
			yield return null;
			if (Bool_0)
			{
				yield break;
			}
		}
		method_6(source, clip, clipContainer.GetMaxDistance(), volume, num, value.transform);
	}

	public void method_8(GClass3578 explosionEvent)
	{
		int projectileID = explosionEvent.ProjectileID;
		if (Dictionary_0.TryGetValue(projectileID, out var source))
		{
			source.StopTrackingPosition();
			if (!source.VolumeFadeOut(0.3f, delegate
			{
				method_10(projectileID, source);
			}))
			{
				method_10(projectileID, source);
			}
		}
	}

	public void method_9(InitShellingProjectileFlyEvent flyEvent)
	{
		int projectileID = flyEvent.ProjectileID;
		BetterSource source = MonoBehaviourSingleton<BetterAudio>.Instance.GetSource(BetterAudio.AudioSourceGroupType.Environment);
		AudioMultipleClipContainer shellWhistleLoop = ArtilleryShellingSoundsSO_0.shellWhistleLoop;
		method_12(projectileID);
		Dictionary_1[projectileID] = StaticManager_0.StartCoroutine(method_7(source, projectileID, shellWhistleLoop, flyEvent.StartPosition, flyEvent.EndPosition, flyEvent.Speed));
		Dictionary_0[flyEvent.ProjectileID] = source;
	}

	public void method_10(int id, BetterSource source)
	{
		method_12(id);
		Dictionary_0.Remove(id);
		method_11(source);
	}

	public void method_11(BetterSource source)
	{
		if (!(source == null))
		{
			source.StopTrackingPosition();
			source.source1.Stop();
			source.source1.dopplerLevel = 0f;
			source.Loop = false;
			source.Release();
		}
	}

	public void method_12(int projectileID)
	{
		if (Dictionary_1.TryGetValue(projectileID, out var value) && StaticManager_0 != null)
		{
			GClass7.TryStopCoroutine(StaticManager_0, ref value);
		}
		Dictionary_1[projectileID] = null;
	}

	public void method_13()
	{
		Action_0?.Invoke();
		Action_0 = null;
	}

	public void Dispose()
	{
		method_13();
		foreach (var (_, source) in Dictionary_0)
		{
			method_11(source);
		}
		Dictionary_0.Clear();
		Gclass1661_0?.Release();
		Bool_0 = true;
	}

	~GClass1513()
	{
		Dispose();
	}
}
