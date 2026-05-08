using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Audio.ConfiguredAudioPlayer;
using Comfort.Common;
using EFT.UI;
using UnityEngine.Audio;

public class GClass1046 : GInterface61
{
	[CompilerGenerated]
	public class Class742
	{
		public int remaining;

		public Action onCompleted;

		public void method_0()
		{
			if (--remaining == 0)
			{
				onCompleted?.Invoke();
			}
		}
	}

	[NonSerialized]
	public const string String_0 = "targrad_quill";

	[NonSerialized]
	public const string String_1 = "targrad_quit_game";

	[NonSerialized]
	public const string String_2 = "targrad_start_game";

	[NonSerialized]
	public const float Float_0 = 2f;

	[NonSerialized]
	public GClass3669 Gclass3669_0;

	[NonSerialized]
	public GClass3619 Gclass3619_0;

	[NonSerialized]
	public AudioClipDataConfigurator AudioClipDataConfigurator_0;

	[NonSerialized]
	public GInterface90 Ginterface90_0;

	[NonSerialized]
	public GInterface90 Ginterface90_1;

	[NonSerialized]
	public GInterface90 Ginterface90_2;

	[NonSerialized]
	public GInterface90 Ginterface90_3;

	[NonSerialized]
	public List<GInterface90> List_0 = new List<GInterface90>();

	[NonSerialized]
	public CompositeDisposableClass CompositeDisposableClass;

	public GClass1046(GClass3669 animationController, GClass3619 dialogController, AudioClipDataConfigurator clipData)
	{
		Gclass3669_0 = animationController;
		Gclass3619_0 = dialogController;
		AudioClipDataConfigurator_0 = clipData;
		if (MonoBehaviourSingleton<BetterAudio>.Exist(out var component))
		{
			AudioMixerGroup masterMixerGroup = component.MasterMixerGroup;
			Ginterface90_0 = new GClass1176(component, BetterAudio.AudioSourceGroupType.Nonspatial, masterMixerGroup);
			Ginterface90_1 = new GClass1176(component, BetterAudio.AudioSourceGroupType.Nonspatial, masterMixerGroup);
			Ginterface90_2 = new GClass1177(component, BetterAudio.AudioSourceGroupType.Nonspatial, masterMixerGroup);
			Ginterface90_3 = new GClass1177(component, BetterAudio.AudioSourceGroupType.Nonspatial, masterMixerGroup);
			List_0.Clear();
			List_0.Add(Ginterface90_0);
			List_0.Add(Ginterface90_1);
			List_0.Add(Ginterface90_2);
			List_0.Add(Ginterface90_3);
		}
		else
		{
			GClass722.Instance.LogError($"Can't initialize {typeof(GClass1046)}");
		}
	}

	public void Initialize()
	{
		CompositeDisposableClass?.Dispose();
		CompositeDisposableClass = new CompositeDisposableClass(6);
		GUISounds instance = Singleton<GUISounds>.Instance;
		if (instance == null)
		{
			GClass722.Instance.LogError("GUISounds not found in Execute");
		}
		else
		{
			Gclass3669_0.OnMusicChanged += method_0;
			CompositeDisposableClass.AddDisposable(delegate
			{
				Gclass3669_0.OnMusicChanged -= method_0;
			});
			Gclass3669_0.OnSoundChanged += method_1;
			CompositeDisposableClass.AddDisposable(delegate
			{
				Gclass3669_0.OnSoundChanged -= method_1;
			});
			Gclass3669_0.OnStartTypeWriter += method_2;
			CompositeDisposableClass.AddDisposable(delegate
			{
				Gclass3669_0.OnStartTypeWriter -= method_2;
			});
			Gclass3669_0.OnCompleteTypeWriter += method_3;
			CompositeDisposableClass.AddDisposable(delegate
			{
				Gclass3669_0.OnCompleteTypeWriter -= method_3;
			});
			Gclass3669_0.OnSkipTypeWriter += method_3;
			CompositeDisposableClass.AddDisposable(delegate
			{
				Gclass3669_0.OnSkipTypeWriter -= method_3;
			});
			Gclass3619_0.OnActionFinished += method_4;
			CompositeDisposableClass.AddDisposable(delegate
			{
				Gclass3619_0.OnActionFinished -= method_4;
			});
			instance.SetHideoutLowPassFilter(active: false, 2f, forced: true);
		}
		method_5("targrad_start_game");
	}

	public void method_0(string musicName)
	{
		method_6(Ginterface90_0, musicName);
	}

	public void method_1(string musicName)
	{
		method_6(Ginterface90_1, musicName);
	}

	public void method_2(string text)
	{
		method_6(Ginterface90_2, "targrad_quill");
	}

	public void method_3()
	{
		Ginterface90_2.Stop();
	}

	public void method_4(GClass3629 action)
	{
		if (action.Type == GClass3629.EActionType.QuitAction)
		{
			method_5("targrad_quit_game");
		}
	}

	public void method_5(string soundName)
	{
		method_6(Ginterface90_3, soundName);
	}

	public void Release(bool allowFadeOut = false, Action onCompleted = null)
	{
		CompositeDisposableClass?.Dispose();
		int remaining = List_0.Count;
		if (remaining == 0)
		{
			onCompleted?.Invoke();
			return;
		}
		foreach (GInterface90 item in List_0)
		{
			item.Release(allowFadeOut, delegate
			{
				if (--remaining == 0)
				{
					onCompleted?.Invoke();
				}
			});
		}
		GUISounds instance = Singleton<GUISounds>.Instance;
		if (instance != null)
		{
			instance.SetHideoutLowPassFilter(active: true, 2f);
		}
	}

	public void method_6(GInterface90 player, string soundName)
	{
		if (AudioClipDataConfigurator_0.TryGetValue(soundName, out var value))
		{
			player.Play(value);
		}
		else
		{
			GClass722.Instance.LogError("can't find audio config for: " + soundName);
		}
	}

	[CompilerGenerated]
	public void method_7()
	{
		Gclass3669_0.OnMusicChanged -= method_0;
	}

	[CompilerGenerated]
	public void method_8()
	{
		Gclass3669_0.OnSoundChanged -= method_1;
	}

	[CompilerGenerated]
	public void method_9()
	{
		Gclass3669_0.OnStartTypeWriter -= method_2;
	}

	[CompilerGenerated]
	public void method_10()
	{
		Gclass3669_0.OnCompleteTypeWriter -= method_3;
	}

	[CompilerGenerated]
	public void method_11()
	{
		Gclass3669_0.OnSkipTypeWriter -= method_3;
	}

	[CompilerGenerated]
	public void method_12()
	{
		Gclass3619_0.OnActionFinished -= method_4;
	}
}
