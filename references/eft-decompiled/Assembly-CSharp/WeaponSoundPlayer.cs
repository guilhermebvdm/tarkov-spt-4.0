using System;
using System.Collections;
using Audio.Data;
using Audio.SpatialSystem;
using Comfort.Common;
using EFT;
using EFT.Weapons.Data;
using UnityEngine;
using UnityEngine.Audio;

public class WeaponSoundPlayer : BaseSoundPlayer
{
	private const float SOUND_SPEED = 340.29f;

	private const float IMPACT_DIVIDER = 6f;

	private const float ORIGINAL_PITCH = 1f;

	private const float MAX_ALLOWED_PITCH = 1.045f;

	private const float MIN_ALLOWED_PITCH = 0.965f;

	public SoundBank Body;

	public SoundBank Tail;

	public SoundBank Doublet;

	public SoundBank BodySilenced;

	public SoundBank TailSilenced;

	public SoundBank DoubletSilenced;

	private bool _isSilenced;

	private float _prevDistance;

	private GClass890 _queue;

	private bool _isFiring;

	private float _firingLoopLength;

	private const int BEATS = 16;

	public bool Non_auto;

	private double _releaseTime;

	private float _occlusionReleaseTime;

	private double _startPlayingTime;

	private float _pitch = 1f;

	private float _balance = 1f;

	private double _startFire;

	private float _delay;

	private AudioMixerGroup _currentMixerGroup;

	private float _prevPitchMult;

	private Coroutine _releaseOcclusionCoroutine;

	private SoundBankWithSettings _aimingSoundsBank;

	private SoundBank _verticalMountSounds;

	private IEnumerator _mountCoroutine;

	public override bool IsSilenced
	{
		get
		{
			return _isSilenced;
		}
		set
		{
			_isSilenced = value;
			if (BodySilenced == null)
			{
				BodySilenced = Body;
			}
			if (TailSilenced == null)
			{
				TailSilenced = Tail;
			}
			try
			{
				_firingLoopLength = (IsSilenced ? BodySilenced.ClipLength : Body.ClipLength);
			}
			catch (Exception message)
			{
				Debug.LogError(message);
			}
		}
	}

	public bool IsAutoWeapon => !Non_auto;

	public float BeatLn => _firingLoopLength / 16f;

	public bool IsOccludedToListener => _currentMixerGroup != Singleton<BetterAudio>.Instance.GunshotMixerGroup;

	public float Distance => CameraClass.Instance.Distance(base.WeaponPosition);

	public float ReleaseTime
	{
		set
		{
			_releaseTime = value + 0.1f;
		}
	}

	public override void Init(BifacialTransform weaponHierarchy)
	{
		base.Init(weaponHierarchy);
		if (IsAutoWeapon)
		{
			playersBridge.BreakLoop += OnBreakLoop;
			CompositeDisposable.AddDisposable(delegate
			{
				playersBridge.BreakLoop -= OnBreakLoop;
			});
		}
		if (BodySilenced == null && Body == null)
		{
			return;
		}
		_firingLoopLength = (IsSilenced ? BodySilenced.ClipLength : Body.ClipLength);
		_currentMixerGroup = MonoBehaviourSingleton<BetterAudio>.Instance.GunshotMixerGroup;
		WeaponSounds asset = GClass1857.GetAsset<WeaponSounds>(Singleton<IEasyAssets>.Instance, "assets/content/audio/prefabs/shells/weaponsounds.bundle");
		ESoundWeaponType weaponType = GClass2579.GetWeaponType(playersBridge.Weapon);
		if (asset != null)
		{
			if (asset.AimingSounds.TryGetAimingBank(weaponType, out var bank))
			{
				_aimingSoundsBank = bank;
			}
			_verticalMountSounds = asset.VerticalMountSounds;
			playersBridge.MovementContextAdapter.EnterMountedState += EnterMountedStateHandler;
			playersBridge.MovementContextAdapter.ExitMountedState += ExitMountedStateHandler;
		}
	}

	public override void Update()
	{
		base.Update();
		if (_queue != null)
		{
			_queue.Pose(base.WeaponPosition);
			if (!_isFiring && AudioSettings.dspTime > _releaseTime)
			{
				Release();
			}
			if (!_isFiring && AudioSettings.dspTime < _releaseTime)
			{
				double num = AudioSettings.dspTime - _startFire;
				double num2 = _startPlayingTime - AudioSettings.dspTime;
				SmoothResetPitch((float)(num / num2));
			}
		}
	}

	public override void OnDisable()
	{
		if (_isFiring)
		{
			StopFiringLoop();
		}
		Release((float)(_releaseTime - AudioSettings.dspTime));
		UnsubscribeMountStateEvents();
		GClass7.StopBehaviourTimer(this, ref _mountCoroutine);
		base.OnDisable();
	}

	public override void OnDestroy()
	{
		if (_isFiring)
		{
			StopFiringLoop();
		}
		Release(_releaseTime - AudioSettings.dspTime);
		UnsubscribeMountStateEvents();
		GClass7.StopBehaviourTimer(this, ref _mountCoroutine);
		base.OnDestroy();
	}

	public void UnsubscribeMountStateEvents()
	{
		if (playersBridge != null)
		{
			playersBridge.MovementContextAdapter.EnterMountedState -= EnterMountedStateHandler;
			playersBridge.MovementContextAdapter.ExitMountedState -= ExitMountedStateHandler;
		}
	}

	public void EnterMountedStateHandler(float timeToMount)
	{
		GClass7.StopBehaviourTimer(this, ref _mountCoroutine);
		_mountCoroutine = GClass7.StartBehaviourTimer(this, timeToMount, PlayMountSound);
	}

	public void ExitMountedStateHandler(float timeToUnmount)
	{
		GClass7.StopBehaviourTimer(this, ref _mountCoroutine);
	}

	public void PlayMountSound()
	{
		if (!MonoBehaviourSingleton<BetterAudio>.Instantiated)
		{
			return;
		}
		AudioClip clip = _verticalMountSounds.PickSingleClip(0);
		bool flag = GClass2078.IsFirstPerson(playersBridge.PointOfView);
		float volume = _verticalMountSounds.RandomVolume * playersBridge.VolumeMultByWeaponErgonomic;
		if (MonoBehaviourSingleton<BetterAudio>.Instance.TryPlayAtPoint(out var source, base.WeaponPosition, clip, BetterAudio.AudioSourceGroupType.Weaponry, (int)_verticalMountSounds.Rolloff, volume, EOcclusionTest.None, null, !flag))
		{
			int priority = PriorityCalculator.CalculatePriority(Distance, _verticalMountSounds.Rolloff);
			source.SetPriority(priority);
			if (!flag && MonoBehaviourSingleton<SpatialAudioSystem>.Instantiated)
			{
				MonoBehaviourSingleton<SpatialAudioSystem>.Instance.ProcessSourceOcclusion(playersBridge.iPlayer, source);
			}
		}
	}

	public void Release(double timeLeftToPlay = 0.0)
	{
		if (_queue != null && Singleton<BetterAudio>.Instantiated)
		{
			Singleton<BetterAudio>.Instance.ReleaseQueueDelayed(_queue, timeLeftToPlay);
			_queue = null;
			StopSoundCoroutine();
		}
	}

	public void UpdateMixerGroup()
	{
		if (playersBridge.PointOfView != EPointOfView.FirstPerson && _queue != null && MonoBehaviourSingleton<SpatialAudioSystem>.Instance.TryGetPlayerAudioContainer(playersBridge.iPlayer.Id, out var container))
		{
			AudioMixerGroup gunshotOcclusionMixerGroup = GClass2579.GetGunshotOcclusionMixerGroup(container.OcclusionEffect);
			if (!(_currentMixerGroup == gunshotOcclusionMixerGroup))
			{
				_currentMixerGroup = gunshotOcclusionMixerGroup;
				_queue.SetMixerGroup(_currentMixerGroup);
			}
		}
	}

	public void UpdatePitch(float pitch)
	{
		if (_queue != null)
		{
			pitch = Mathf.Clamp(pitch, 0.965f, 1.045f);
			if (Mathf.Abs(pitch - _prevPitchMult) > Mathf.Epsilon)
			{
				_pitch = pitch;
				_prevPitchMult = pitch;
				_queue.SetPitch(_pitch);
			}
		}
	}

	public void FireBullet(AmmoItemClass ammo, Vector3 shotPosition, Vector3 shotDirection, float pitchMult, bool malfunctioned = false, bool multiShot = false, bool burstOf2Start = false)
	{
		SoundBank soundBank = ((!multiShot) ? (IsSilenced ? BodySilenced : Body) : (IsSilenced ? DoubletSilenced : Doublet));
		if (soundBank == null)
		{
			return;
		}
		if (_isFiring)
		{
			if (_queue != null)
			{
				if (IsAutoWeapon)
				{
					Balance(soundBank);
				}
				UpdateMixerGroup();
				UpdatePitch(pitchMult);
			}
			return;
		}
		float distance = Distance;
		_prevDistance = Distance;
		if (_queue == null)
		{
			_queue = Singleton<BetterAudio>.Instance.BorrowWeaponAudioQueue(BetterAudio.AudioSourceGroupType.Gunshots);
			if (_queue == null)
			{
				return;
			}
			_queue.Pose(base.WeaponPosition);
			_queue.SetRolloff(soundBank.Rolloff);
			_queue.SetMixerGroup(MonoBehaviourSingleton<BetterAudio>.Instance.GunshotMixerGroup);
			RegisterSourceForOcclusionProcess();
		}
		if (!(_queue.AudioSources[0].OcclusionVolumeFactor <= 0f))
		{
			_queue.SetMixerGroup(_currentMixerGroup);
			_queue.EnableStereo(playersBridge.PointOfView == EPointOfView.FirstPerson);
			if (IsAutoWeapon)
			{
				_isFiring = true;
			}
			AudioClip clip = null;
			AudioClip clip2 = null;
			soundBank.PickClipsByDistance(ref clip, ref clip2, ref _balance, (int)playersBridge.Environment, distance);
			_pitch = pitchMult;
			_prevPitchMult = pitchMult;
			float num = Mathf.Max((clip != null) ? clip.length : 0f, (clip2 != null) ? clip2.length : 0f);
			_delay = distance / 340.29f;
			_startFire = AudioSettings.dspTime;
			_releaseTime = _startFire + (double)num + 1.0;
			_occlusionReleaseTime = num / _pitch / 6f;
			Balance(soundBank);
			EnableSourceOcclusion(enabledOcclusion: true);
			UpdateMixerGroup();
			float sonicDelay = _delay;
			_queue.Enqueue(clip, clip2, _balance, _startFire + (double)_delay, Non_auto ? (num / _pitch) : 0f, soundBank.BaseVolume, _pitch);
			if (!IsAutoWeapon)
			{
				ReleaseOcclusion(_occlusionReleaseTime);
			}
			if ((!playersBridge.isWeaponTriggerPressed || malfunctioned || burstOf2Start) && IsAutoWeapon)
			{
				UpdatePitch(pitchMult);
				SoundBank soundBank2 = (IsSilenced ? TailSilenced : Tail);
				AudioClip clip3 = null;
				AudioClip clip4 = null;
				soundBank2.PickClipsByDistance(ref clip3, ref clip4, ref _balance, (int)playersBridge.Environment, distance);
				float num2 = Mathf.Max((clip3 != null) ? clip3.length : 0f, (clip4 != null) ? clip4.length : 0f);
				float num3 = (burstOf2Start ? (BeatLn * 2f) : BeatLn);
				sonicDelay = _delay + num3 / _pitch;
				_startPlayingTime = _startFire + (double)_delay + (double)(num3 / _pitch);
				_queue.Enqueue(clip3, clip4, _balance, _startPlayingTime, num2 / _pitch, soundBank2.BaseVolume, _pitch);
				_isFiring = false;
				_releaseTime = _startPlayingTime + (double)(num2 / _pitch);
				_occlusionReleaseTime = _delay + num3 / _pitch;
				ReleaseOcclusion(_occlusionReleaseTime);
			}
			FireSonicSound(sonicDelay, soundBank.Rolloff, ammo, shotPosition, shotDirection);
		}
	}

	public void ReleaseOcclusion(float endTime = 0f)
	{
		StopSoundCoroutine();
		_releaseOcclusionCoroutine = StartCoroutine(ReleaseOcclusionCoroutine(endTime));
	}

	public IEnumerator ReleaseOcclusionCoroutine(float endTime = 0f)
	{
		yield return new WaitForSeconds(endTime);
		EnableSourceOcclusion(_isFiring);
	}

	public void SmoothResetPitch(float time)
	{
		float pitch = Mathf.Lerp(_pitch, 1f, time);
		UpdatePitch(pitch);
	}

	public void StopSoundCoroutine()
	{
		if (_releaseOcclusionCoroutine != null)
		{
			StopCoroutine(_releaseOcclusionCoroutine);
			_releaseOcclusionCoroutine = null;
		}
	}

	public void EnableSourceOcclusion(bool enabledOcclusion)
	{
		if (playersBridge.PointOfView != EPointOfView.FirstPerson)
		{
			GClass890 queue = _queue;
			if (queue != null)
			{
				queue.AudioSources[0]?.IncludeInOcclusionProcess(enabledOcclusion);
			}
			GClass890 queue2 = _queue;
			if (queue2 != null)
			{
				queue2.AudioSources[1]?.IncludeInOcclusionProcess(enabledOcclusion);
			}
		}
	}

	public void RegisterSourceForOcclusionProcess()
	{
		if (playersBridge.PointOfView != EPointOfView.FirstPerson && MonoBehaviourSingleton<SpatialAudioSystem>.Instantiated)
		{
			EnableSourceOcclusion(enabledOcclusion: true);
			MonoBehaviourSingleton<SpatialAudioSystem>.Instance.ProcessSourceOcclusion(playersBridge.iPlayer, _queue.AudioSources[0]);
			MonoBehaviourSingleton<SpatialAudioSystem>.Instance.ProcessSourceOcclusion(playersBridge.iPlayer, _queue.AudioSources[1]);
		}
	}

	public void Balance(SoundBank bank)
	{
		if (_queue == null || (object)Body == null)
		{
			return;
		}
		float distance = Distance;
		if (!(Mathf.Abs(distance - _prevDistance) < Mathf.Epsilon))
		{
			_prevDistance = distance;
			BetterSource[] audioSources = _queue.AudioSources;
			for (int i = 0; i < audioSources.Length; i++)
			{
				bank.method_1(audioSources[i], distance);
			}
			int priority = PriorityCalculator.CalculatePriority(distance, bank.Rolloff);
			_queue.SetPriority(priority);
		}
	}

	public void OnBreakLoop()
	{
		if (base.gameObject.activeSelf && _isFiring)
		{
			StopFiringLoop();
		}
	}

	public void StopFiringLoop()
	{
		int num = (int)((AudioSettings.dspTime - _startFire) / (double)BeatLn) + 1;
		_startPlayingTime = _startFire + (double)_delay + (double)((float)num * BeatLn / _pitch);
		AudioClip clip = null;
		AudioClip clip2 = null;
		SoundBank soundBank = (IsSilenced ? TailSilenced : Tail);
		soundBank.PickClipsByDistance(ref clip, ref clip2, ref _balance, (int)playersBridge.Environment, Distance);
		float num2 = Mathf.Max((clip != null) ? clip.length : 0f, (clip2 != null) ? clip2.length : 0f);
		_queue?.Enqueue(clip, clip2, _balance, _startPlayingTime, num2 / _pitch, soundBank.BaseVolume, _pitch);
		_isFiring = false;
		_occlusionReleaseTime = _delay + (float)num * BeatLn / _pitch;
		_releaseTime = _startPlayingTime + (double)(num2 / _pitch);
		ReleaseOcclusion(_occlusionReleaseTime);
	}

	public void FireSonicSound(float sonicDelay, float rolloff, AmmoItemClass ammo, Vector3 shotPosition, Vector3 shotDirection)
	{
		if (!(CameraClass.Instance.Camera == null) && !GClass2078.IsFirstPerson(playersBridge.PointOfView) && ammo.ProjectileCount == 1)
		{
			GClass897.Shoot(new SonicBulletSoundPlayer.GClass898(ammo, shotPosition, shotDirection, CameraClass.Instance.Camera, rolloff, sonicDelay, IsOccludedToListener));
		}
	}

	public void PlayAimingSound(float volume)
	{
		if (!(_aimingSoundsBank == null) && _aimingSoundsBank.TryGetRandomClip(out var clip))
		{
			PlayClip(clip, (int)_aimingSoundsBank.Rolloff, volume * _aimingSoundsBank.BaseVolume);
		}
	}
}
