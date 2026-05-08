using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Audio.SpatialSystem;
using Comfort.Common;
using EFT;
using EFT.NextObservedPlayer;
using UnityEngine;

public class BaseSoundPlayer : MonoBehaviour, IActorEvents
{
	[Serializable]
	public class SoundElement
	{
		public string EventName = "";

		public int RollOff = 30;

		public float Volume = 1f;

		public AudioClip[] SoundClips;

		public AudioClip RandomSoundClip
		{
			get
			{
				if (SoundClips != null && SoundClips.Length != 0)
				{
					return SoundClips[UnityEngine.Random.Range(0, SoundClips.Length)];
				}
				return null;
			}
		}
	}

	public interface IObserverToPlayerBridge : IDisposable
	{
		IPlayer iPlayer { get; }

		EPointOfView PointOfView { get; }

		SoundElement PropIn { get; }

		SoundElement PropOut { get; }

		EnvironmentType Environment { get; }

		bool isWeaponTriggerPressed { get; }

		FirearmsAnimator FirearmsAnimator { get; }

		IWeapon Weapon { get; }

		float VolumeMultByWeaponErgonomic { get; }

		GClass2582 MovementContextAdapter { get; }

		bool UseSimplifiedSkeleton { get; }

		bool IsVisible { get; }

		event Action BreakLoop;
	}

	public class ObserverBridge : IObserverToPlayerBridge, IDisposable
	{
		[NonSerialized]
		public ObservedPlayerView ObservedPlayer;

		[NonSerialized]
		public HandsControllerClass HandsController;

		public IPlayer iPlayer => ObservedPlayer;

		public EPointOfView PointOfView => EPointOfView.ThirdPerson;

		public SoundElement PropIn => ObservedPlayer.ObservedPlayerController.AudioController.PropIn;

		public SoundElement PropOut => ObservedPlayer.ObservedPlayerController.AudioController.PropOut;

		public EnvironmentType Environment => ObservedPlayer.ObservedPlayerController.AudioController.Environment;

		public bool isWeaponTriggerPressed => ObservedPlayer.ObservedPlayerController.HandsController.IsWeaponTriggerPressing;

		public FirearmsAnimator FirearmsAnimator => ObservedPlayer.FirearmsAnimator;

		[field: NonSerialized]
		public IWeapon Weapon { get; }

		public float VolumeMultByWeaponErgonomic => ObservedPlayer.ObservedPlayerController.AudioController.CalculateAimingVolume();

		[field: NonSerialized]
		public GClass2582 MovementContextAdapter { get; }

		public bool UseSimplifiedSkeleton => ObservedPlayer.UsedSimplifiedSkeleton;

		public bool IsVisible => ObservedPlayer.IsVisible;

		public event Action BreakLoop;

		public ObserverBridge(ObservedPlayerView player)
		{
			ObservedPlayer = player;
			this.BreakLoop = null;
			HandsController = ObservedPlayer.ObservedPlayerController.HandsController;
			HandsController.BreakLoop += CallBreakLoop;
			Weapon = HandsController.Weapon;
			GClass2981 stateContext = player.ObservedPlayerController.StateContext;
			MovementContextAdapter = new GClass2584(stateContext);
		}

		public void CallBreakLoop()
		{
			this.BreakLoop?.Invoke();
		}

		public void Dispose()
		{
			MovementContextAdapter?.Dispose();
			if (HandsController != null)
			{
				HandsController.BreakLoop -= CallBreakLoop;
			}
			this.BreakLoop = null;
		}
	}

	public class PlayerBridge : IObserverToPlayerBridge, IDisposable
	{
		[NonSerialized]
		public Player Player;

		[NonSerialized]
		public IHandsController HandsController;

		[NonSerialized]
		public Player.FirearmController Firearm;

		public IPlayer iPlayer => Player;

		public EPointOfView PointOfView => Player.PointOfView;

		public SoundElement PropIn => Player.PropIn;

		public SoundElement PropOut => Player.PropOut;

		public EnvironmentType Environment => Player.Environment;

		public bool isWeaponTriggerPressed => Firearm.IsTriggerPressed;

		public FirearmsAnimator FirearmsAnimator => HandsController.FirearmsAnimator;

		[field: NonSerialized]
		public IWeapon Weapon { get; }

		public float VolumeMultByWeaponErgonomic => Firearm.CalculateAimingSoundVolume();

		[field: NonSerialized]
		public GClass2582 MovementContextAdapter { get; }

		public bool UseSimplifiedSkeleton => Player.UsedSimplifiedSkeleton;

		public bool IsVisible => true;

		public event Action BreakLoop;

		public PlayerBridge(Player player, IHandsController handsController)
		{
			Player = player;
			HandsController = handsController;
			this.BreakLoop = null;
			Firearm = null;
			if (HandsController is Player.FirearmController firearmController)
			{
				Weapon = firearmController.Weapon;
				Firearm = firearmController;
				Firearm.BreakLoop += CallBreakLoop;
			}
			MovementContextAdapter = new GClass2583(player.MovementContext);
		}

		public void CallBreakLoop()
		{
			this.BreakLoop?.Invoke();
		}

		public void Dispose()
		{
			MovementContextAdapter?.Dispose();
			if (Firearm != null)
			{
				Firearm.BreakLoop -= CallBreakLoop;
			}
			this.BreakLoop = null;
		}
	}

	[CompilerGenerated]
	public class Class540
	{
		public string soundName;

		public bool _003CSoundEventHandler_003Eb__0(SoundElement soundElement)
		{
			if (!(soundElement.EventName == soundName))
			{
				return soundElement.EventName == "Snd" + soundName;
			}
			return true;
		}
	}

	[CompilerGenerated]
	public class Class541
	{
		public string soundName;

		public bool _003CSoundAtPointEventHandler_003Eb__0(SoundElement elem)
		{
			if (!(elem.EventName == soundName))
			{
				return elem.EventName == "Snd" + soundName;
			}
			return true;
		}
	}

	[CompilerGenerated]
	public class Class542
	{
		public string eventName;

		public bool _003CEventReceiver_003Eb__0(SoundElement soundElement)
		{
			return soundElement.EventName == eventName;
		}

		public bool _003CEventReceiver_003Eb__1(SoundElement soundElement)
		{
			return soundElement.EventName == eventName;
		}
	}

	private const double CLIP_SOURCE_RELEASE_DELAY = 1.0;

	protected IObserverToPlayerBridge playersBridge;

	private BetterSource _clipsSource;

	protected BifacialTransform _weaponHierarchy;

	private bool _isClipSourceReleased = true;

	private bool _isClipSourceStartPlaying;

	private double _clipSourceReleaseTime;

	protected GClass1180 PriorityCalculator;

	[SerializeField]
	public List<SoundElement> AdditionalSounds = new List<SoundElement>();

	[SerializeField]
	public SoundElement[] MainSounds = new SoundElement[8]
	{
		new SoundElement
		{
			EventName = "ShellEject"
		},
		new SoundElement
		{
			EventName = "AddAmmoInChamber"
		},
		new SoundElement
		{
			EventName = "OnBoltCatch"
		},
		new SoundElement
		{
			EventName = "OffBoltCatch"
		},
		new SoundElement
		{
			EventName = "SetFiremode1"
		},
		new SoundElement
		{
			EventName = "SetFiremode0"
		},
		new SoundElement
		{
			EventName = "MagOut"
		},
		new SoundElement
		{
			EventName = "MagIn"
		}
	};

	protected readonly CompositeDisposableClass CompositeDisposable = new CompositeDisposableClass();

	private FirearmsAnimator _firearmsAnimator;

	public virtual bool IsSilenced { get; set; }

	public Vector3 WeaponPosition
	{
		get
		{
			if (!playersBridge.IsVisible)
			{
				return playersBridge.iPlayer.Position;
			}
			return _weaponHierarchy.position;
		}
	}

	public Transform WeaponTransform
	{
		get
		{
			if (!playersBridge.IsVisible)
			{
				return playersBridge.iPlayer.Transform.Original;
			}
			return _weaponHierarchy.Original;
		}
	}

	public void Init(IHandsController handsController, BifacialTransform weaponHierarchy, Player player)
	{
		playersBridge = new PlayerBridge(player, handsController);
		Init(weaponHierarchy);
	}

	public void Init(BifacialTransform weaponHierarchy, ObservedPlayerView player)
	{
		playersBridge = new ObserverBridge(player);
		Init(weaponHierarchy);
	}

	public virtual void Init(BifacialTransform weaponHierarchy)
	{
		_weaponHierarchy = weaponHierarchy;
		_firearmsAnimator = playersBridge.FirearmsAnimator;
		_firearmsAnimator.AddEventsConsumer(this);
		CompositeDisposable.AddDisposable(delegate
		{
			_firearmsAnimator.RemoveEventsConsumer(this);
		});
		PriorityCalculator = (playersBridge.UseSimplifiedSkeleton ? new GClass1180(192, 128, 96) : new GClass1180(64, 32, 0));
		ReleaseClipsSource();
	}

	public void SoundEventHandler(string soundName)
	{
		SoundElement soundElement = AdditionalSounds.Find((SoundElement soundElement2) => soundElement2.EventName == soundName || soundElement2.EventName == "Snd" + soundName);
		if (soundElement != null)
		{
			PlayRandomClip(soundElement);
		}
	}

	public void SoundAtPointEventHandler(string soundName)
	{
		SoundElement soundElement = AdditionalSounds.Find((SoundElement elem) => elem.EventName == soundName || elem.EventName == "Snd" + soundName);
		if (soundElement != null && !(soundElement.RandomSoundClip == null))
		{
			PlayClip(soundElement.RandomSoundClip, soundElement.RollOff, soundElement.Volume);
		}
	}

	public void EventReceiver(string eventName)
	{
		SoundElement soundElement = Array.Find(MainSounds, (SoundElement soundElement3) => soundElement3.EventName == eventName);
		if (soundElement != null)
		{
			PlayRandomClip(soundElement);
		}
		SoundElement soundElement2 = AdditionalSounds.Find((SoundElement soundElement3) => soundElement3.EventName == eventName);
		if (soundElement2 != null)
		{
			PlayRandomClip(soundElement2);
		}
	}

	public void PlayRandomClip(SoundElement soundElement)
	{
		if (soundElement != null)
		{
			PlayClip(soundElement.RandomSoundClip, soundElement.RollOff, soundElement.Volume);
		}
	}

	public void PlayClip(AudioClip clip, int rolloff, float volume)
	{
		if (clip == null)
		{
			return;
		}
		bool stereo;
		float num = ((stereo = GClass2078.IsFirstPerson(playersBridge.PointOfView)) ? 0f : CameraClass.Instance.SqrDistance(WeaponPosition));
		int num2 = rolloff * rolloff;
		if (!(num > (float)num2))
		{
			int priority = PriorityCalculator.CalculatePriority(num, num2);
			if (_isClipSourceReleased)
			{
				SetupClipSource(clip);
			}
			PlayClipInternal(clip, rolloff, volume, priority, stereo);
		}
	}

	public void PlayClipInternal(AudioClip clip, int rolloff, float volume, int priority, bool stereo)
	{
		if (!stereo && MonoBehaviourSingleton<SpatialAudioSystem>.Instantiated)
		{
			MonoBehaviourSingleton<SpatialAudioSystem>.Instance.ProcessSourceOcclusion(playersBridge.iPlayer, _clipsSource);
		}
		if (!(_clipsSource.OcclusionVolumeFactor <= 0f))
		{
			_clipsSource.SetRolloff(rolloff);
			_clipsSource.SetPriority(priority);
			_clipsSource.Play(clip, null, 1f, volume, stereo);
			_isClipSourceStartPlaying = true;
			_clipSourceReleaseTime = Math.Max(AudioSettings.dspTime, _clipSourceReleaseTime) + (double)clip.length + 1.0;
		}
	}

	public void SetupClipSource(AudioClip clip)
	{
		_clipsSource = Singleton<BetterAudio>.Instance.GetSource(BetterAudio.AudioSourceGroupType.Weaponry);
		_clipsSource.StartTrackingPosition(WeaponTransform);
		_isClipSourceReleased = false;
	}

	public virtual void Update()
	{
		if (_isClipSourceStartPlaying && !_isClipSourceReleased && _clipsSource.PlayBackState == BetterSource.EPlayBackState.Stopped && AudioSettings.dspTime > _clipSourceReleaseTime)
		{
			ReleaseClipsSource();
		}
	}

	public virtual void OnDestroy()
	{
		CompositeDisposable.Dispose();
		_firearmsAnimator = null;
		playersBridge?.Dispose();
		playersBridge = null;
		if (_clipsSource != null && _clipsSource.source1 != null)
		{
			ReleaseClipsSource();
			return;
		}
		_clipsSource = null;
		_isClipSourceReleased = true;
		_isClipSourceStartPlaying = false;
	}

	public virtual void OnDisable()
	{
		ReleaseClipSourceDelayed();
	}

	public void ReleaseClipSourceDelayed()
	{
		if (!_isClipSourceReleased && MonoBehaviourSingleton<BetterAudio>.Instantiated)
		{
			MonoBehaviourSingleton<BetterAudio>.Instance.AddToAudioSourceQueue(_clipsSource, _clipSourceReleaseTime);
			_clipsSource.StopTrackingPosition();
			_clipsSource = null;
			_isClipSourceReleased = true;
			_isClipSourceStartPlaying = false;
		}
	}

	public void ReleaseClipsSource()
	{
		if (!_isClipSourceReleased)
		{
			_clipsSource.Release();
			_clipsSource = null;
			_isClipSourceReleased = true;
			_isClipSourceStartPlaying = false;
		}
	}

	public void OnUseProp(bool boolParam)
	{
		PlayRandomClip(boolParam ? playersBridge.PropIn : playersBridge.PropOut);
	}

	void IActorEvents.OnUseProp(bool boolParam)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnUseProp
		this.OnUseProp(boolParam);
	}

	public void OnAddAmmoInChamber()
	{
	}

	void IActorEvents.OnAddAmmoInChamber()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnAddAmmoInChamber
		this.OnAddAmmoInChamber();
	}

	public void OnAddAmmoInMag()
	{
	}

	void IActorEvents.OnAddAmmoInMag()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnAddAmmoInMag
		this.OnAddAmmoInMag();
	}

	public void OnArm()
	{
	}

	void IActorEvents.OnArm()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnArm
		this.OnArm();
	}

	public void OnCook()
	{
	}

	void IActorEvents.OnCook()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnCook
		this.OnCook();
	}

	public void OnDelAmmoChamber()
	{
	}

	void IActorEvents.OnDelAmmoChamber()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnDelAmmoChamber
		this.OnDelAmmoChamber();
	}

	public void OnDelAmmoFromMag()
	{
	}

	void IActorEvents.OnDelAmmoFromMag()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnDelAmmoFromMag
		this.OnDelAmmoFromMag();
	}

	public void OnDisarm()
	{
	}

	void IActorEvents.OnDisarm()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnDisarm
		this.OnDisarm();
	}

	public void OnFireEnd()
	{
	}

	void IActorEvents.OnFireEnd()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnFireEnd
		this.OnFireEnd();
	}

	public void OnFiringBullet()
	{
	}

	void IActorEvents.OnFiringBullet()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnFiringBullet
		this.OnFiringBullet();
	}

	public void OnFoldOff()
	{
	}

	void IActorEvents.OnFoldOff()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnFoldOff
		this.OnFoldOff();
	}

	public void OnFoldOn()
	{
	}

	void IActorEvents.OnFoldOn()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnFoldOn
		this.OnFoldOn();
	}

	public void OnIdleStart()
	{
	}

	void IActorEvents.OnIdleStart()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnIdleStart
		this.OnIdleStart();
	}

	public void OnMalfunctionOff()
	{
	}

	void IActorEvents.OnMalfunctionOff()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnMalfunctionOff
		this.OnMalfunctionOff();
	}

	public void OnMagHide()
	{
	}

	void IActorEvents.OnMagHide()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnMagHide
		this.OnMagHide();
	}

	public void OnMagIn()
	{
	}

	void IActorEvents.OnMagIn()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnMagIn
		this.OnMagIn();
	}

	public void OnMagOut()
	{
	}

	void IActorEvents.OnMagOut()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnMagOut
		this.OnMagOut();
	}

	public void OnMagShow()
	{
	}

	void IActorEvents.OnMagShow()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnMagShow
		this.OnMagShow();
	}

	public void OnMessageName()
	{
	}

	void IActorEvents.OnMessageName()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnMessageName
		this.OnMessageName();
	}

	public void OnModChanged()
	{
	}

	void IActorEvents.OnModChanged()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnModChanged
		this.OnModChanged();
	}

	public void OnOffBoltCatch()
	{
	}

	void IActorEvents.OnOffBoltCatch()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnOffBoltCatch
		this.OnOffBoltCatch();
	}

	public void OnOnBoltCatch()
	{
	}

	void IActorEvents.OnOnBoltCatch()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnOnBoltCatch
		this.OnOnBoltCatch();
	}

	public void OnPutMagToRig()
	{
	}

	void IActorEvents.OnPutMagToRig()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnPutMagToRig
		this.OnPutMagToRig();
	}

	public void OnRemoveShell()
	{
	}

	void IActorEvents.OnRemoveShell()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnRemoveShell
		this.OnRemoveShell();
	}

	public void OnReplaceSecondMag()
	{
	}

	void IActorEvents.OnReplaceSecondMag()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnReplaceSecondMag
		this.OnReplaceSecondMag();
	}

	public void OnShellEject()
	{
	}

	void IActorEvents.OnShellEject()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnShellEject
		this.OnShellEject();
	}

	public void OnShowAmmo(bool BoolParam)
	{
	}

	void IActorEvents.OnShowAmmo(bool BoolParam)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnShowAmmo
		this.OnShowAmmo(BoolParam);
	}

	public void OnSliderOut()
	{
	}

	public void OnSound(string StringParam)
	{
		SoundEventHandler(StringParam);
	}

	void IActorEvents.OnSound(string StringParam)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnSound
		this.OnSound(StringParam);
	}

	public void OnSoundAtPoint(string StringParam)
	{
		SoundAtPointEventHandler(StringParam);
	}

	void IActorEvents.OnSoundAtPoint(string StringParam)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnSoundAtPoint
		this.OnSoundAtPoint(StringParam);
	}

	public void OnStartUtilityOperation()
	{
	}

	void IActorEvents.OnStartUtilityOperation()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnStartUtilityOperation
		this.OnStartUtilityOperation();
	}

	public void OnThirdAction(int IntParam)
	{
	}

	void IActorEvents.OnThirdAction(int IntParam)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnThirdAction
		this.OnThirdAction(IntParam);
	}

	public void OnUseSecondMagForReload()
	{
	}

	void IActorEvents.OnUseSecondMagForReload()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnUseSecondMagForReload
		this.OnUseSecondMagForReload();
	}

	public void OnWeapIn()
	{
	}

	void IActorEvents.OnWeapIn()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnWeapIn
		this.OnWeapIn();
	}

	public void OnWeapOut()
	{
	}

	void IActorEvents.OnWeapOut()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnWeapOut
		this.OnWeapOut();
	}

	public void OnBackpackDrop()
	{
	}

	void IActorEvents.OnBackpackDrop()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnBackpackDrop
		this.OnBackpackDrop();
	}

	public void OnComboPlanning()
	{
	}

	void IActorEvents.OnComboPlanning()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnComboPlanning
		this.OnComboPlanning();
	}

	public void OnLauncherAppeared()
	{
	}

	void IActorEvents.OnLauncherAppeared()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnLauncherAppeared
		this.OnLauncherAppeared();
	}

	public void OnLauncherDisappeared()
	{
	}

	void IActorEvents.OnLauncherDisappeared()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnLauncherDisappeared
		this.OnLauncherDisappeared();
	}

	public void OnShowMag()
	{
	}

	void IActorEvents.OnShowMag()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnShowMag
		this.OnShowMag();
	}

	public void OnCurrentAnimStateEnded()
	{
	}

	void IActorEvents.OnCurrentAnimStateEnded()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnCurrentAnimStateEnded
		this.OnCurrentAnimStateEnded();
	}

	public void OnSetActiveObject(int objectID)
	{
	}

	void IActorEvents.OnSetActiveObject(int objectID)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnSetActiveObject
		this.OnSetActiveObject(objectID);
	}

	public void OnDeactivateObject(int objectID)
	{
	}

	void IActorEvents.OnDeactivateObject(int objectID)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnDeactivateObject
		this.OnDeactivateObject(objectID);
	}

	public void ReloadTest()
	{
	}

	void IActorEvents.ReloadTest()
	{
		//ILSpy generated this explicit interface implementation from .override directive in ReloadTest
		this.ReloadTest();
	}

	public void BipodOpen()
	{
	}

	void IActorEvents.BipodOpen()
	{
		//ILSpy generated this explicit interface implementation from .override directive in BipodOpen
		this.BipodOpen();
	}

	public void BipodClose()
	{
	}

	void IActorEvents.BipodClose()
	{
		//ILSpy generated this explicit interface implementation from .override directive in BipodClose
		this.BipodClose();
	}

	public void OutUse()
	{
	}

	void IActorEvents.OutUse()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OutUse
		this.OutUse();
	}

	public void AimReady()
	{
	}

	public void IdleReady()
	{
	}

	public void DropWeapon()
	{
	}
}
