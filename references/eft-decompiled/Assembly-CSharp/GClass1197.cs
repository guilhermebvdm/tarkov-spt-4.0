using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Comfort.Common;
using DG.Tweening;
using EFT;
using EFT.EnvironmentEffect;
using EFT.HealthSystem;
using UnityEngine;
using UnityEngine.Audio;

public class GClass1197 : GInterface108
{
	[NonSerialized]
	public const string String_0 = "Audio/Events/Runddans/runddans_quest_done";

	[NonSerialized]
	public const string String_1 = "Audio/Events/Runddans/generator_repair_loop";

	[NonSerialized]
	public const string String_2 = "Audio/Events/Runddans/FrozenBuff/RunddansFrozenBuffSoundBank";

	[NonSerialized]
	public const string String_3 = "EventRadio";

	[NonSerialized]
	public const string String_4 = "EventRadioVolume";

	[NonSerialized]
	public const string String_5 = "EventRadioPitch";

	[NonSerialized]
	public const float Float_0 = 1f;

	[NonSerialized]
	public const float Float_1 = 2f;

	[NonSerialized]
	public const float Float_2 = 3f;

	[NonSerialized]
	public const float Float_3 = 2f;

	[NonSerialized]
	public const float Float_4 = -80f;

	[NonSerialized]
	public const float Float_5 = 0.1f;

	[NonSerialized]
	public const float Float_6 = 0f;

	[NonSerialized]
	public const float Float_7 = 1f;

	[NonSerialized]
	public const float Float_8 = 100f;

	[NonSerialized]
	public const float Float_9 = 30f;

	[NonSerialized]
	public const string String_6 = "Generator/Repair";

	[NonSerialized]
	public AudioMixer AudioMixer_0;

	[NonSerialized]
	public Action Action_0;

	[NonSerialized]
	public EventObject.EState Estate_0;

	[NonSerialized]
	public CancellationTokenSource CancellationTokenSource_0;

	[NonSerialized]
	public AudioClip AudioClip_0;

	[NonSerialized]
	public AudioClip AudioClip_1;

	[NonSerialized]
	public TagBank TagBank_0;

	[NonSerialized]
	public EventObject EventObject_0;

	[NonSerialized]
	public float Float_10;

	[NonSerialized]
	public string String_7;

	[NonSerialized]
	public IHealthController IhealthController_0;

	[NonSerialized]
	public BetterSource BetterSource_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	public bool IsRunning
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
		[CompilerGenerated]
		set
		{
			Bool_0 = value;
		}
	}

	public GClass1197()
	{
		AudioMixer_0 = MonoBehaviourSingleton<BetterAudio>.Instance.Master.FindMatchingGroups("EventRadio").First()?.audioMixer;
	}

	public void Run()
	{
		method_0().HandleExceptions();
	}

	public async Task method_0()
	{
		CancellationTokenSource_0 = new CancellationTokenSource();
		CancellationToken token = CancellationTokenSource_0.Token;
		try
		{
			if (RunddansControllerAbstractClass.Exist<RunddansControllerAbstractClass>(out var runddansController))
			{
				KeyValuePair<int, EventObject> keyValuePair = runddansController.Objects.FirstOrDefault();
				if (keyValuePair.Key != 0)
				{
					AudioClip_0 = await method_1<AudioClip>("Audio/Events/Runddans/runddans_quest_done", token);
					AudioClip_1 = await method_1<AudioClip>("Audio/Events/Runddans/generator_repair_loop", token);
					TagBank_0 = await method_1<TagBank>("Audio/Events/Runddans/FrozenBuff/RunddansFrozenBuffSoundBank", token);
					EventObject_0 = keyValuePair.Value;
					EventObject_0.OnStateChanged += method_9;
					EventObject_0.OnFinish += method_4;
					Action_0 = (Action)Delegate.Combine(Action_0, GlobalEventHandlerClass.Instance.SubscribeOnEvent<GClass3574>(method_6));
					method_2();
					method_11();
					IsRunning = true;
				}
			}
		}
		catch (OperationCanceledException)
		{
			Debug.Log("RunddansEventAudioPlayer stopped on Init.");
		}
		catch (Exception arg)
		{
			Debug.Log(string.Format("{0} Init failed: {1}.", "RunddansEventAudioPlayer", arg));
		}
	}

	public async Task<T> method_1<T>(string path, CancellationToken cancellationToken) where T : UnityEngine.Object
	{
		ResourceRequest resourceRequest = Resources.LoadAsync<T>(path);
		while (!resourceRequest.isDone)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				cancellationToken.ThrowIfCancellationRequested();
			}
			await Task.Yield();
		}
		cancellationToken.ThrowIfCancellationRequested();
		return resourceRequest.asset as T;
	}

	public void method_2()
	{
		GameWorld instance = Singleton<GameWorld>.Instance;
		if (!(instance == null))
		{
			Player mainPlayer = instance.MainPlayer;
			if (mainPlayer == null)
			{
				instance.OnPersonAdd += method_3;
			}
			else
			{
				method_3(mainPlayer);
			}
			EnvironmentManager.Instance.OnPlayerEnvironmentChanged += method_8;
		}
	}

	public void method_3(IPlayer player)
	{
		if (player.IsYourPlayer)
		{
			IhealthController_0 = player.HealthController;
			String_7 = player.ProfileId;
			IhealthController_0.EffectStartedEvent += method_7;
		}
	}

	public void method_4()
	{
		if (!(EventObject_0 == null) && MonoBehaviourSingleton<BetterAudio>.Instantiated && !(GamePlayerOwner.MyPlayer == null) && method_5() && GClass2313.IsInRange(EventObject_0.transform.position, 100f))
		{
			MonoBehaviourSingleton<BetterAudio>.Instance.PlayNonspatial(AudioClip_0, BetterAudio.AudioSourceGroupType.NonspatialBypass, 0f, 0.5f);
		}
	}

	public bool method_5()
	{
		if (RunddansControllerAbstractClass.Exist<RunddansControllerAbstractClass>(out var runddansController) && runddansController.IsValid<TransitControllerAbstractClass>(GamePlayerOwner.MyPlayer, out var _, out var info))
		{
			return info.events;
		}
		return false;
	}

	public void method_6(GClass3574 interactionEvent)
	{
		if (interactionEvent.InteractionTag != "Generator/Repair")
		{
			return;
		}
		Player myPlayer = GamePlayerOwner.MyPlayer;
		if (!(myPlayer == null) && myPlayer.HealthController.IsAlive)
		{
			if (interactionEvent.InteractionStatus == EInteractionStatus.Started)
			{
				myPlayer.PlayInteractionSound(AudioClip_1);
			}
			else
			{
				myPlayer.StopInteractionSound(0.1f);
			}
		}
	}

	public void Stop()
	{
		IsRunning = false;
		method_11();
		Action_0?.Invoke();
		Action_0 = null;
		try
		{
			CancellationTokenSource_0?.Cancel();
			CancellationTokenSource_0?.Dispose();
		}
		catch (ObjectDisposedException)
		{
			Debug.Log("RunddansEventAudioPlayer token source is disposed");
		}
		if (EventObject_0 != null)
		{
			EventObject_0.OnFinish -= method_4;
			EventObject_0.OnStateChanged -= method_9;
		}
		if (IhealthController_0 != null)
		{
			IhealthController_0.EffectStartedEvent -= method_7;
		}
		if (EnvironmentManager.Instance != null)
		{
			EnvironmentManager.Instance.OnPlayerEnvironmentChanged -= method_8;
		}
	}

	public void method_7(IEffect healthEffect)
	{
		if (healthEffect is GInterface372 && !(Time.realtimeSinceStartup < Float_10))
		{
			Player myPlayer = GamePlayerOwner.MyPlayer;
			Singleton<GameWorld>.Instance.SpeakerManager.GetSpeaker(myPlayer.PlayerId)?.PlayExternal(TagBank_0, EPhraseTrigger.Frozen, (ETagStatus)0);
			Float_10 = Time.realtimeSinceStartup + 30f;
		}
	}

	public void method_8(string profileID, EnvironmentType environment)
	{
		if (!(String_7 != profileID) && environment == EnvironmentType.Indoor)
		{
			Float_10 = Time.realtimeSinceStartup;
		}
	}

	public void method_9(EventObject.EState state)
	{
		method_10(state);
	}

	public void method_10(EventObject.EState state)
	{
		if (Estate_0 != state)
		{
			AudioMixer_0.DOKill();
			bool num = state == EventObject.EState.Running;
			float endValue = (num ? 0f : (-80f));
			float endValue2 = (num ? 1f : 0.1f);
			float duration = (num ? 1f : 3f);
			float duration2 = (num ? 2f : 2f);
			AudioMixer_0.DOSetFloat("EventRadioVolume", endValue, duration);
			AudioMixer_0.DOSetFloat("EventRadioPitch", endValue2, duration2);
			Estate_0 = state;
		}
	}

	public void method_11()
	{
		if (AudioMixer_0 == null)
		{
			Debug.LogError("Event mixer EventRadio is null!");
			return;
		}
		AudioMixer_0.DOKill();
		AudioMixer_0.SetFloat("EventRadioVolume", -80f);
		AudioMixer_0.SetFloat("EventRadioPitch", 0.1f);
	}
}
