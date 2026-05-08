using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using Audio.SpatialSystem;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using UnityEngine;

public class InteractiveSubscriber : MonoBehaviour
{
	[Serializable]
	public class ViewPerState
	{
		[GAttribute10(typeof(EDoorState))]
		public EDoorState State;

		public GameObject GameObject;

		public bool EnableDisable = true;

		public bool EnableContentInCoroutine;

		public Vector3 Spin;
	}

	[Serializable]
	public class PlaySettings
	{
		public AudioClip Clip;

		public bool Loop;

		public float Volume = 1f;

		public int MaxDistance = 100;

		public float SpatialBlend = 1f;

		public AnimationCurve CustomCurve;

		public AnimationCurve SpreadCurve;

		public float StartDelay;

		public float EndDelay;

		public float FadeOut;

		public float FadeIn;

		[GAttribute10(typeof(EDoorState))]
		public EDoorState State;

		[Header("vvv For one shot sounds only vvv")]
		[GAttribute10(typeof(EDoorState))]
		public EDoorState FromState;

		public Transform[] AdditionalPositions;

		public bool EnableAudioCulling = true;

		public bool Spatialize = true;

		public EOcclusionTest occlusionTest = EOcclusionTest.ContinuousPropagated;

		public BetterAudio.AudioSourceGroupType groupType = BetterAudio.AudioSourceGroupType.PropagatedExclusive;

		[field: NonSerialized]
		public BetterSource Source { get; set; }
	}

	[Serializable]
	[CompilerGenerated]
	public class Class549
	{
		public static readonly Class549 class549_0 = new Class549();

		public static Action action_0;

		public void method_0()
		{
		}
	}

	[CompilerGenerated]
	public class Class550
	{
		public Coroutine c;

		public InteractiveSubscriber interactiveSubscriber_0;

		public void method_0()
		{
			interactiveSubscriber_0.StopCoroutine(c);
		}
	}

	[CompilerGenerated]
	public class Class551
	{
		public Coroutine c;

		public InteractiveSubscriber interactiveSubscriber_0;

		public void method_0()
		{
			interactiveSubscriber_0.StopCoroutine(c);
		}
	}

	[CompilerGenerated]
	public class Class552
	{
		public PlaySettings sound;

		public InteractiveSubscriber interactiveSubscriber_0;
	}

	[CompilerGenerated]
	public class Class553
	{
		public PlaySettings closureSound;

		public Class552 class552_0;

		public void method_0()
		{
			if (closureSound.Source != null)
			{
				if (GClass842.GetDuration(closureSound.CustomCurve) > 0f)
				{
					closureSound.Source.source1.SetCustomCurve(AudioSourceCurveType.CustomRolloff, closureSound.CustomCurve);
				}
				if (GClass842.GetDuration(closureSound.SpreadCurve) > 0f)
				{
					closureSound.Source.source1.SetCustomCurve(AudioSourceCurveType.Spread, closureSound.SpreadCurve);
				}
				closureSound.Source.SetActive(active: true);
				closureSound.Source.Play(class552_0.sound.Clip, null, 1f, class552_0.sound.Volume, !class552_0.sound.Spatialize, oneShot: false);
				if (closureSound.EnableAudioCulling && Singleton<GameWorld>.Instance.AudioSourceCulling != null)
				{
					Singleton<GameWorld>.Instance.AudioSourceCulling.Register(closureSound.Source.source1, closureSound.Source.Loop);
				}
				if (closureSound.FadeIn > 0f)
				{
					closureSound.Source.VolumeFadeIn(closureSound.FadeIn, delegate
					{
					});
				}
			}
		}
	}

	[CompilerGenerated]
	public class Class554
	{
		public PlaySettings closureSound;

		public Class552 class552_0;

		public void method_0()
		{
			foreach (Transform item in class552_0.sound.AdditionalPositions.Concat(new Transform[1] { class552_0.interactiveSubscriber_0.transform }))
			{
				Vector3 position = item.position;
				BetterSource betterSource = Singleton<BetterAudio>.Instance.PlayAtPoint(position, closureSound.Clip, closureSound.groupType, closureSound.MaxDistance, closureSound.Volume, closureSound.occlusionTest);
				if (betterSource != null)
				{
					betterSource.source1.spatialBlend = closureSound.SpatialBlend;
				}
			}
		}
	}

	[CompilerGenerated]
	public class Class555
	{
		public BetterSource closureSource;

		public PlaySettings closureSound;

		public Action action_0;

		public void method_0()
		{
			if (!(closureSource != null))
			{
				return;
			}
			closureSource.SetActive(active: true);
			closureSource.VolumeFadeOut(closureSound.FadeOut, delegate
			{
				closureSound.Source = null;
				closureSource.Release();
				if (Singleton<GameWorld>.Instance.AudioSourceCulling != null)
				{
					Singleton<GameWorld>.Instance.AudioSourceCulling.Remove(closureSource.source1);
				}
				if (GClass842.GetDuration(closureSound.CustomCurve) > 0f)
				{
					closureSource.source1.SetCustomCurve(AudioSourceCurveType.CustomRolloff, EFTHardSettings.Instance.SoundRolloff);
				}
				if (GClass842.GetDuration(closureSound.SpreadCurve) > 0f)
				{
					closureSource.source1.SetCustomCurve(AudioSourceCurveType.Spread, EFTHardSettings.Instance.SpreadCurve);
				}
			});
		}

		public void method_1()
		{
			closureSound.Source = null;
			closureSource.Release();
			if (Singleton<GameWorld>.Instance.AudioSourceCulling != null)
			{
				Singleton<GameWorld>.Instance.AudioSourceCulling.Remove(closureSource.source1);
			}
			if (GClass842.GetDuration(closureSound.CustomCurve) > 0f)
			{
				closureSource.source1.SetCustomCurve(AudioSourceCurveType.CustomRolloff, EFTHardSettings.Instance.SoundRolloff);
			}
			if (GClass842.GetDuration(closureSound.SpreadCurve) > 0f)
			{
				closureSource.source1.SetCustomCurve(AudioSourceCurveType.Spread, EFTHardSettings.Instance.SpreadCurve);
			}
		}
	}

	public WorldInteractiveObject Subscribee;

	public QueuePlayer QueuePlayer;

	public ViewPerState[] Views;

	public PlaySettings[] Sounds;

	private Action action_0;

	public virtual void Start()
	{
		if (Subscribee != null)
		{
			Subscribee.OnDoorStateChanged += OnStatusChangedHandler;
		}
		OnStatusChangedHandler(Subscribee, Subscribee.DoorState, Subscribee.DoorState);
	}

	public virtual void OnStatusChangedHandler(WorldInteractiveObject subscribee, EDoorState previous, EDoorState next)
	{
		action_0?.Invoke();
		action_0 = null;
		ViewsSetActive(next);
		PlaySounds(previous, next);
	}

	public void PlaySounds(EDoorState fromState, EDoorState state)
	{
		PlaySettings[] sounds = Sounds;
		foreach (PlaySettings sound in sounds)
		{
			if ((state & sound.State) != EDoorState.None)
			{
				if (sound.Loop)
				{
					if ((bool)sound.Source)
					{
						if (Singleton<GameWorld>.Instance.AudioSourceCulling != null)
						{
							Singleton<GameWorld>.Instance.AudioSourceCulling.Remove(sound.Source.source1);
						}
						sound.Source.Release();
					}
					sound.Source = Singleton<BetterAudio>.Instance.GetSource(sound.groupType, activateSource: false);
					if (MonoBehaviourSingleton<SpatialAudioSystem>.Instantiated)
					{
						MonoBehaviourSingleton<SpatialAudioSystem>.Instance.ProcessSourceOcclusion(base.gameObject, sound.Source, sound.occlusionTest, 30000f, default(Vector3), staticPosition: true);
					}
					sound.Source.SetMixerGroup(MonoBehaviourSingleton<BetterAudio>.Instance.CommonAmbientEffectsMixer);
					sound.Source.transform.SetPositionAndRotation(base.transform.position, base.transform.rotation);
					sound.Source.StartTrackingPosition(base.transform);
					sound.Source.Loop = sound.Loop;
					sound.Source.source1.spatialBlend = sound.SpatialBlend;
					sound.Source.source1.clip = sound.Clip;
					sound.Source.SetRolloff(sound.MaxDistance);
					sound.Source.EnableSpatialization = sound.Spatialize;
					PlaySettings closureSound = sound;
					GClass855.WaitSeconds(this, sound.StartDelay, delegate
					{
						if (closureSound.Source != null)
						{
							if (GClass842.GetDuration(closureSound.CustomCurve) > 0f)
							{
								closureSound.Source.source1.SetCustomCurve(AudioSourceCurveType.CustomRolloff, closureSound.CustomCurve);
							}
							if (GClass842.GetDuration(closureSound.SpreadCurve) > 0f)
							{
								closureSound.Source.source1.SetCustomCurve(AudioSourceCurveType.Spread, closureSound.SpreadCurve);
							}
							closureSound.Source.SetActive(active: true);
							closureSound.Source.Play(sound.Clip, null, 1f, sound.Volume, !sound.Spatialize, oneShot: false);
							if (closureSound.EnableAudioCulling && Singleton<GameWorld>.Instance.AudioSourceCulling != null)
							{
								Singleton<GameWorld>.Instance.AudioSourceCulling.Register(closureSound.Source.source1, closureSound.Source.Loop);
							}
							if (closureSound.FadeIn > 0f)
							{
								closureSound.Source.VolumeFadeIn(closureSound.FadeIn, delegate
								{
								});
							}
						}
					});
				}
				else if (QueuePlayer != null)
				{
					QueuePlayer.Play(sound.Clip);
				}
				else
				{
					if (sound.FromState == EDoorState.None || (fromState & sound.FromState) == 0)
					{
						continue;
					}
					PlaySettings closureSound2 = sound;
					GClass855.WaitSeconds(this, sound.StartDelay, delegate
					{
						foreach (Transform item in sound.AdditionalPositions.Concat(new Transform[1] { base.transform }))
						{
							Vector3 position = item.position;
							BetterSource betterSource = Singleton<BetterAudio>.Instance.PlayAtPoint(position, closureSound2.Clip, closureSound2.groupType, closureSound2.MaxDistance, closureSound2.Volume, closureSound2.occlusionTest);
							if (betterSource != null)
							{
								betterSource.source1.spatialBlend = closureSound2.SpatialBlend;
							}
						}
					});
				}
				continue;
			}
			PlaySettings closureSound3 = sound;
			BetterSource closureSource = closureSound3.Source;
			GClass855.WaitSeconds(this, sound.EndDelay, delegate
			{
				if (closureSource != null)
				{
					closureSource.SetActive(active: true);
					closureSource.VolumeFadeOut(closureSound3.FadeOut, delegate
					{
						closureSound3.Source = null;
						closureSource.Release();
						if (Singleton<GameWorld>.Instance.AudioSourceCulling != null)
						{
							Singleton<GameWorld>.Instance.AudioSourceCulling.Remove(closureSource.source1);
						}
						if (GClass842.GetDuration(closureSound3.CustomCurve) > 0f)
						{
							closureSource.source1.SetCustomCurve(AudioSourceCurveType.CustomRolloff, EFTHardSettings.Instance.SoundRolloff);
						}
						if (GClass842.GetDuration(closureSound3.SpreadCurve) > 0f)
						{
							closureSource.source1.SetCustomCurve(AudioSourceCurveType.Spread, EFTHardSettings.Instance.SpreadCurve);
						}
					});
				}
			});
		}
	}

	public virtual void ViewsSetActive(EDoorState state)
	{
		ViewPerState[] views = Views;
		foreach (ViewPerState viewPerState in views)
		{
			if (viewPerState.EnableDisable)
			{
				bool flag = (state & viewPerState.State) != 0;
				if (viewPerState.EnableContentInCoroutine)
				{
					Coroutine c = StartCoroutine(method_0(viewPerState.GameObject.transform, flag));
					action_0 = (Action)Delegate.Combine(action_0, (Action)delegate
					{
						StopCoroutine(c);
					});
				}
				else
				{
					viewPerState.GameObject.SetActive(flag);
				}
			}
			if (viewPerState.Spin.sqrMagnitude > 0f && (state & viewPerState.State) != EDoorState.None)
			{
				Coroutine c2 = StartCoroutine(method_1(viewPerState.GameObject.transform, viewPerState.Spin));
				action_0 = (Action)Delegate.Combine(action_0, (Action)delegate
				{
					StopCoroutine(c2);
				});
			}
		}
	}

	public IEnumerator method_0(Transform parent, bool isActive)
	{
		foreach (Transform item in parent)
		{
			item.gameObject.SetActive(isActive);
			yield return null;
		}
	}

	public void OnDestroy()
	{
		if (Subscribee != null)
		{
			Subscribee.OnDoorStateChanged -= OnStatusChangedHandler;
		}
	}

	public IEnumerator method_1(Transform t, Vector3 spin)
	{
		while (true)
		{
			t.Rotate(spin, Space.Self);
			yield return null;
		}
	}

	public void OnDrawGizmosSelected()
	{
		PlaySettings[] sounds = Sounds;
		foreach (PlaySettings playSettings in sounds)
		{
			Gizmos.color = new Color(1f, 0f, 1f, 0.1f);
			Gizmos.DrawSphere(base.transform.position, playSettings.MaxDistance);
			Transform[] additionalPositions = playSettings.AdditionalPositions;
			foreach (Transform obj in additionalPositions)
			{
				Gizmos.color = new Color(0f, 1f, 1f, 0.1f);
				Gizmos.DrawSphere(obj.position, playSettings.MaxDistance);
			}
		}
	}
}
