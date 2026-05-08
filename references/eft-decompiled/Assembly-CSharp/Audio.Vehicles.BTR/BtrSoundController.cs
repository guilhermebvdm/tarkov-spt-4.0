using System;
using System.Collections;
using System.Collections.Generic;
using Audio.Data;
using Audio.RadioSystem;
using Audio.SpatialSystem;
using Comfort.Common;
using EFT;
using EFT.GlobalEvents;
using EFT.Vehicle;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio.Vehicles.BTR;

[Serializable]
public class BtrSoundController : MonoBehaviour
{
	private const float MIN_TRANSITION_TIME = 0.1f;

	[SerializeField]
	private SoundSuspensionController _suspensionController;

	[SerializeField]
	private VehicleMovementSoundContext _indoorSoundContext;

	[SerializeField]
	private VehicleMovementSoundContext _outdoorSoundContext;

	[SerializeField]
	private SpatialAudioRoom _btrIndoorRoom;

	[SerializeField]
	private VehicleRotationSoundPlayer _indoorTurret;

	[SerializeField]
	private VehicleRotationSoundPlayer _outdoorTurret;

	[SerializeField]
	private BtrDriverSoundBankContainer _btrDriverSoundContainer;

	[SerializeField]
	private Transform _driverTransform;

	[SerializeField]
	private List<AudioClip> _readyToDepartureClips = new List<AudioClip>();

	[SerializeField]
	private float _readyToDepartureBTRSignalDelay = 5f;

	[SerializeField]
	private BtrDoorSoundHandler[] _doorSoundHandlers;

	[SerializeField]
	private ClientBroadcastPlayer _broadcastPlayer;

	private BetterSource _airHornSource;

	private readonly Dictionary<EnvironmentType, VehicleMovementSoundContext> _contextByEnvironment = new Dictionary<EnvironmentType, VehicleMovementSoundContext>(2);

	private GInterface83 _phraseController = new GClass1116();

	private Coroutine _transitionCoroutine;

	private BTRView _btrView;

	private bool _initialized;

	private float _currentNormalizedSpeed;

	private float _goInDurationSeconds;

	private float _goOutDurationSeconds;

	private Action _unsubscribeOnDestroy;

	public void Initialize(BTRView btrView, float goInDurationMs, float goOutDurationMs)
	{
		_btrView = btrView;
		_goInDurationSeconds = goInDurationMs / 1000f;
		_goOutDurationSeconds = goOutDurationMs / 1000f;
		if (!(_outdoorSoundContext == null) && !(_indoorSoundContext == null))
		{
			_contextByEnvironment.Clear();
			_contextByEnvironment[EnvironmentType.Indoor] = _indoorSoundContext;
			_contextByEnvironment[EnvironmentType.Outdoor] = _outdoorSoundContext;
			foreach (var (_, vehicleMovementSoundContext2) in _contextByEnvironment)
			{
				vehicleMovementSoundContext2.Init();
				vehicleMovementSoundContext2.method_7(EVehicleMovementStatus.Started);
				vehicleMovementSoundContext2.SetSpatialBlend(1f);
			}
			_phraseController = new GClass1117(_btrDriverSoundContainer, _driverTransform);
			method_5();
			TransitToEnvironment(EnvironmentType.Outdoor);
			_airHornSource = MonoBehaviourSingleton<BetterAudio>.Instance.GetSource(BetterAudio.AudioSourceGroupType.Environment);
			_airHornSource.SetActive(active: true);
			_airHornSource.SetParent(_driverTransform, worldPositionStay: false);
			_btrIndoorRoom.Initialize();
			if (_broadcastPlayer != null && Singleton<GameWorld>.Instantiated)
			{
				Singleton<GameWorld>.Instance.ClientBroadcastSyncController?.RegisterBroadcastPlayer(_broadcastPlayer);
			}
			_initialized = true;
		}
		else
		{
			Debug.LogError("Btr sound engines not initialized correct!");
		}
	}

	public void Update()
	{
		_phraseController.ManualUpdate();
		if (_btrView == null || _btrView.BtrState != EBtrState.Running)
		{
			return;
		}
		_currentNormalizedSpeed = method_4();
		foreach (var (_, vehicleMovementSoundContext2) in _contextByEnvironment)
		{
			vehicleMovementSoundContext2.UpdateRotation(_btrView.TargetRotation.eulerAngles.x);
			vehicleMovementSoundContext2.UpdateSpeed(_currentNormalizedSpeed, Time.deltaTime);
		}
		_suspensionController.UpdateImpactPlayers();
	}

	public void method_0(BtrReadyToDepartureEvent readyToDepartureEvent)
	{
		if (_airHornSource != null)
		{
			int index = UnityEngine.Random.Range(0, _readyToDepartureClips.Count - 1);
			_airHornSource.SetSpatialBlend();
			_airHornSource.source1.clip = _readyToDepartureClips[index];
			_airHornSource.source1.PlayDelayed(_readyToDepartureBTRSignalDelay);
		}
	}

	public void method_1(EnvironmentType environment)
	{
		if (MonoBehaviourSingleton<BetterAudio>.Instantiated && !(_airHornSource == null))
		{
			BetterAudio instance = MonoBehaviourSingleton<BetterAudio>.Instance;
			AudioMixerGroup mixerGroup = ((environment == EnvironmentType.Outdoor) ? instance.VehicleOutMixer : instance.VehicleInMixer);
			_airHornSource.SetMixerGroup(mixerGroup);
		}
	}

	public void method_2(EnvironmentType environment)
	{
		BtrDoorSoundHandler[] doorSoundHandlers = _doorSoundHandlers;
		for (int i = 0; i < doorSoundHandlers.Length; i++)
		{
			doorSoundHandlers[i].SetEnvironment(environment);
		}
	}

	public void method_3(EnvironmentType environment)
	{
		_broadcastPlayer.Mute(environment == EnvironmentType.Outdoor);
	}

	public float method_4()
	{
		return _btrView.CurrentSpeed / _btrView.MoveSpeed;
	}

	public void method_5()
	{
		_unsubscribeOnDestroy = (Action)Delegate.Combine(_unsubscribeOnDestroy, GlobalEventHandlerClass.Instance.SubscribeOnEvent<BtrPauseMoveEvent>(method_7));
		_unsubscribeOnDestroy = (Action)Delegate.Combine(_unsubscribeOnDestroy, GlobalEventHandlerClass.Instance.SubscribeOnEvent<BtrReadyToDepartureEvent>(method_0));
	}

	public void method_6()
	{
		_unsubscribeOnDestroy?.Invoke();
		_unsubscribeOnDestroy = null;
	}

	public void method_7(BtrPauseMoveEvent moveEvent)
	{
		EVehicleMovementStatus nextStatus = (moveEvent.PauseOn ? EVehicleMovementStatus.Idle : EVehicleMovementStatus.Running);
		foreach (KeyValuePair<EnvironmentType, VehicleMovementSoundContext> item in _contextByEnvironment)
		{
			item.Deconstruct(out var _, out var value);
			value.method_7(nextStatus);
		}
	}

	public void TransitToEnvironment(EnvironmentType environment, bool fast = false, Action callback = null)
	{
		if (_initialized)
		{
			float transitionTime = method_8(environment, fast);
			method_11();
			_transitionCoroutine = StartCoroutine(method_9(environment, transitionTime, callback));
			_suspensionController.SetEnvironment(environment);
			method_1(environment);
			method_2(environment);
			method_3(environment);
		}
	}

	public float method_8(EnvironmentType environment, bool fast)
	{
		if (fast)
		{
			return 0.1f;
		}
		if (environment != EnvironmentType.Outdoor)
		{
			return _goInDurationSeconds;
		}
		return _goOutDurationSeconds;
	}

	public IEnumerator method_9(EnvironmentType environment, float transitionTime, Action callback = null)
	{
		if (transitionTime <= 0f)
		{
			method_10(_indoorSoundContext, _outdoorSoundContext, environment, 1f);
			method_10(_indoorTurret, _outdoorTurret, environment, 1f);
			callback?.Invoke();
			yield break;
		}
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		float num = 0f;
		while (num <= transitionTime)
		{
			num = Time.realtimeSinceStartup - realtimeSinceStartup;
			float t = num / transitionTime;
			method_10(_indoorSoundContext, _outdoorSoundContext, environment, t);
			method_10(_indoorTurret, _outdoorTurret, environment, t);
			yield return null;
		}
		method_10(_indoorSoundContext, _outdoorSoundContext, environment, 1f);
		method_10(_indoorTurret, _outdoorTurret, environment, 1f);
		callback?.Invoke();
	}

	public void method_10(GInterface218 inControl, GInterface218 outControl, EnvironmentType environment, float t)
	{
		GInterface218 obj = ((environment == EnvironmentType.Outdoor) ? outControl : inControl);
		((environment == EnvironmentType.Outdoor) ? inControl : outControl).SetBaseVolume(1f - t);
		obj.SetBaseVolume(t);
	}

	public void method_11()
	{
		if (_transitionCoroutine != null)
		{
			StopCoroutine(_transitionCoroutine);
			_transitionCoroutine = null;
		}
	}

	public void UpdateBtrAudioRoom(EnvironmentType environment, IPlayer player)
	{
		if (player.IsAI)
		{
			return;
		}
		try
		{
			if (MonoBehaviourSingleton<SpatialAudioSystem>.Instantiated && _btrIndoorRoom != null)
			{
				switch (environment)
				{
				case EnvironmentType.Indoor:
					MonoBehaviourSingleton<SpatialAudioSystem>.Instance.AddPlayerCurrentRoom(_btrIndoorRoom, player);
					break;
				case EnvironmentType.Outdoor:
					MonoBehaviourSingleton<SpatialAudioSystem>.Instance.RemovePlayerCurrentRoom(_btrIndoorRoom, player);
					break;
				}
			}
		}
		catch (Exception arg)
		{
			Debug.LogError($"Critical error with BTR sound: {arg}");
		}
	}

	public void OnDestroy()
	{
		method_6();
		_phraseController.Dispose();
		if (_airHornSource != null)
		{
			_airHornSource.Release();
		}
		_indoorTurret?.Dispose();
		_outdoorTurret?.Dispose();
	}
}
