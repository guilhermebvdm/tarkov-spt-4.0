using System;
using System.Collections;
using System.Threading;
using Audio.SpatialSystem;
using Audio.SpatialSystem.Data;
using Comfort.Common;
using EFT;
using UnityEngine;

namespace Audio.AmbientSubsystem;

public class EnvironmentSoundBlendSystem : MonoBehaviour, GInterface101, IDisposable
{
	private const int int_0 = 0;

	private const float float_0 = 1f;

	private const float float_1 = 0f;

	[SerializeField]
	private AmbientSoundBlender[] _ambientSoundBlenders = Array.Empty<AmbientSoundBlender>();

	[SerializeField]
	private float _listenerHeightWeight = 0.5f;

	private readonly GClass1187 gclass1187_0 = new GClass1187();

	private Coroutine coroutine_0;

	private Transform transform_0;

	private Vector3 vector3_0;

	private Action action_0;

	private EAudioRoomTypeMask eaudioRoomTypeMask_0 = EAudioRoomTypeMask.Outdoor;

	private float float_2;

	private int int_1 = -1;

	private int int_2 = -1;

	private int int_3 = -1;

	private PortalData[] portalData_0 = Array.Empty<PortalData>();

	private short[] short_0 = Array.Empty<short>();

	private bool bool_0;

	private CancellationTokenSource cancellationTokenSource_0;

	private float float_3 = 1f;

	private bool bool_1;

	public void Init()
	{
		bool_1 = false;
		cancellationTokenSource_0?.Cancel();
		cancellationTokenSource_0 = new CancellationTokenSource();
		if (Singleton<AudioListenerConsistencyManager>.Instantiated)
		{
			transform_0 = Singleton<AudioListenerConsistencyManager>.Instance.transform;
			vector3_0 = transform_0.position;
			try
			{
				if (_ambientSoundBlenders.Length == 0)
				{
					_ambientSoundBlenders = GetComponentsInChildren<AmbientSoundBlender>();
				}
				AmbientSoundBlender[] ambientSoundBlenders = _ambientSoundBlenders;
				for (int i = 0; i < ambientSoundBlenders.Length; i++)
				{
					ambientSoundBlenders[i].Init(MonoBehaviourSingleton<BetterAudio>.Instance.Master);
				}
			}
			catch (Exception arg)
			{
				Debug.LogError($"Failed to init Ambient Blenders: {arg}");
				return;
			}
			method_0();
			method_1();
			action_0 = GlobalEventHandlerClass.Instance.SubscribeOnEvent<GClass3573>(method_2);
			action_0 = (Action)Delegate.Combine(action_0, GlobalEventHandlerClass.Instance.SubscribeOnEvent<GClass3568>(method_6));
		}
		else
		{
			Debug.LogError("[ SOUND : AudioListener Transform is null!]");
		}
	}

	public void method_0()
	{
		if (MonoBehaviourSingleton<SpatialAudioSystem>.Instantiated)
		{
			ISpatialAudioRoom listenerCurrentRoom = MonoBehaviourSingleton<SpatialAudioSystem>.Instance.ListenerCurrentRoom;
			if (listenerCurrentRoom != null && !GClass1118.IsOutdoor(listenerCurrentRoom.Type))
			{
				method_12(0f);
				method_13(1f);
			}
			else
			{
				method_12(1f, 0.5f, force: false);
				method_13(0f);
			}
		}
		else
		{
			method_12(1f, 0.5f, force: false);
			method_13(0f);
		}
	}

	public void method_1()
	{
		SpatialAudioSystem instance = MonoBehaviourSingleton<SpatialAudioSystem>.Instance;
		if (instance != null && instance.ListenerCurrentRoom.IsValid)
		{
			ISpatialAudioRoom listenerCurrentRoom = instance.ListenerCurrentRoom;
			method_3(listenerCurrentRoom, listenerCurrentRoom.Type, instance.ListenerCurrentOutdoorRoomID);
		}
	}

	public void ManualUpdate(float dt = 0f)
	{
	}

	public void method_2(GClass3573 roomChangedEvent)
	{
		ISpatialAudioRoom room = roomChangedEvent.Room;
		EAudioRoomTypeMask currentRoomType = roomChangedEvent.CurrentRoomType;
		int currentOutdoorRoomID = roomChangedEvent.CurrentOutdoorRoomID;
		method_3(room, currentRoomType, currentOutdoorRoomID);
	}

	public void method_3(ISpatialAudioRoom room, EAudioRoomTypeMask roomType, int outdoorRoomID)
	{
		if (GClass1118.IsIndoor(eaudioRoomTypeMask_0) && GClass1118.IsOutdoor(roomType))
		{
			eaudioRoomTypeMask_0 = roomType;
			int_1 = outdoorRoomID;
			method_4();
			return;
		}
		if (int_1 == outdoorRoomID && GClass1118.IsIndoor(eaudioRoomTypeMask_0) && GClass1118.IsIndoor(roomType))
		{
			float_3 = room.RoomAmbientData.OutdoorAmbientVolume;
			int_2 = room.ID;
			return;
		}
		int_1 = outdoorRoomID;
		eaudioRoomTypeMask_0 = roomType;
		if (!GClass1118.IsOutdoor(eaudioRoomTypeMask_0))
		{
			gclass1187_0.Dispose();
			method_5();
			method_14(ref coroutine_0);
			method_8(room);
		}
	}

	public void method_4()
	{
		float_3 = 1f;
		method_14(ref coroutine_0);
		AmbientSoundBlender[] ambientSoundBlenders = _ambientSoundBlenders;
		for (int i = 0; i < ambientSoundBlenders.Length; i++)
		{
			ambientSoundBlenders[i].BlendToOutdoor();
		}
		float_2 = 1f;
		gclass1187_0.Dispose();
	}

	public void method_5()
	{
		if (GClass3670.TryGetData<GClass1123>(out var dataContainer) && dataContainer.TryGetOutdoorPortalData(int_1, out var data))
		{
			portalData_0 = data;
			if (dataContainer.TryGetIndoorRoomsIdsByOutRoomID(int_1, out var ids))
			{
				short_0 = ids;
			}
		}
	}

	public void method_6(GClass3568 stateEvent)
	{
		if (stateEvent.PortalState != BaseSpatialAudioPortal.PortalState.InProgressClose && stateEvent.PortalState != BaseSpatialAudioPortal.PortalState.InProgressOpen)
		{
			method_7(stateEvent.PortalID);
		}
	}

	public void method_7(int portalID)
	{
		if (GClass3670.TryGetData<GClass1123>(out var dataContainer) && dataContainer.TryUpdateConcretePortalData(portalID))
		{
			if (dataContainer.TryGetOutdoorPortalData(int_1, out var data))
			{
				portalData_0 = data;
			}
			bool_0 = true;
		}
	}

	public void method_8(ISpatialAudioRoom newRoom)
	{
		if (!(transform_0 == null))
		{
			if (int_3 != int_1 && GClass3670.TryGetData<GClass1123>(out var dataContainer))
			{
				dataContainer.FullUpdatePortalsData(int_1);
				int_3 = int_1;
			}
			AmbientSoundBlender[] ambientSoundBlenders = _ambientSoundBlenders;
			for (int i = 0; i < ambientSoundBlenders.Length; i++)
			{
				ambientSoundBlenders[i].BlendToIndoor();
			}
			int_2 = newRoom.ID;
			coroutine_0 = StartCoroutine(method_9(newRoom, cancellationTokenSource_0.Token));
		}
	}

	public IEnumerator method_9(ISpatialAudioRoom room, CancellationToken cancellationToken)
	{
		float_3 = room.RoomAmbientData.OutdoorAmbientVolume;
		gclass1187_0.PrepareData(portalData_0, short_0);
		bool flag = true;
		GStruct108 optimalPortalData = GStruct108.Default();
		while (true)
		{
			if (!GClass1118.IsIndoor(eaudioRoomTypeMask_0))
			{
				gclass1187_0.Dispose();
				break;
			}
			if (cancellationToken.IsCancellationRequested)
			{
				break;
			}
			if (!method_11() || bool_0 || flag)
			{
				if (bool_0)
				{
					gclass1187_0.Dispose();
					gclass1187_0.PrepareData(portalData_0, short_0);
					bool_0 = false;
				}
				gclass1187_0.Schedule(transform_0.position, int_2, _listenerHeightWeight);
				while (!gclass1187_0.IsCompleted)
				{
					if (cancellationToken.IsCancellationRequested)
					{
						yield break;
					}
					yield return null;
				}
				gclass1187_0.Complete();
				optimalPortalData = gclass1187_0.OptimalCalculatedData;
			}
			if (optimalPortalData.portalData.id != 0)
			{
				method_10(in optimalPortalData, float_3, Time.deltaTime);
			}
			else if (flag || !Mathf.Approximately(float_2, float_3))
			{
				AmbientSoundBlender[] ambientSoundBlenders = _ambientSoundBlenders;
				for (int i = 0; i < ambientSoundBlenders.Length; i++)
				{
					ambientSoundBlenders[i].BlendToTargetValue(float_3);
				}
				float_2 = float_3;
			}
			flag = false;
			yield return null;
		}
	}

	public void method_10(in GStruct108 optimalPortalData, float maxOutdoorVolume, float dt)
	{
		AmbientSoundBlender[] ambientSoundBlenders = _ambientSoundBlenders;
		for (int i = 0; i < ambientSoundBlenders.Length; i++)
		{
			ambientSoundBlenders[i].CalculateAndApplyMixerParameters(in optimalPortalData, transform_0.position, maxOutdoorVolume, dt);
		}
	}

	public bool method_11()
	{
		if (GClass940.IsPositionIdentical(transform_0.position, vector3_0, 0.2f))
		{
			return true;
		}
		vector3_0 = transform_0.position;
		return false;
	}

	public void method_12(float value, float targetBlendDuration = 0.5f, bool force = true)
	{
		AmbientSoundBlender[] ambientSoundBlenders = _ambientSoundBlenders;
		for (int i = 0; i < ambientSoundBlenders.Length; i++)
		{
			ambientSoundBlenders[i].SetOutdoorMixerParams(value, targetBlendDuration, force);
		}
		float_2 = value;
	}

	public void method_13(float value, float targetBlendDuration = 0.5f, bool force = true)
	{
		AmbientSoundBlender[] ambientSoundBlenders = _ambientSoundBlenders;
		for (int i = 0; i < ambientSoundBlenders.Length; i++)
		{
			ambientSoundBlenders[i].SetIndoorMixerParams(value, targetBlendDuration, force);
		}
	}

	public void method_14(ref Coroutine coroutine)
	{
		if (coroutine != null)
		{
			StopCoroutine(coroutine);
			coroutine = null;
		}
	}

	public void OnDestroy()
	{
		Dispose();
	}

	public void Dispose()
	{
		if (!bool_1)
		{
			action_0?.Invoke();
			action_0 = null;
			cancellationTokenSource_0?.Cancel();
			cancellationTokenSource_0 = null;
			method_14(ref coroutine_0);
			AmbientSoundBlender[] ambientSoundBlenders = _ambientSoundBlenders;
			for (int i = 0; i < ambientSoundBlenders.Length; i++)
			{
				ambientSoundBlenders[i].Dispose();
			}
			gclass1187_0?.Dispose();
			bool_1 = true;
		}
	}
}
