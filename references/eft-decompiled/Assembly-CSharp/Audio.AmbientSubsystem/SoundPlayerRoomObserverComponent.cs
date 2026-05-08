using System;
using System.Collections.Generic;
using Audio.SpatialSystem.Data;
using UnityEngine;

namespace Audio.AmbientSubsystem;

[RequireComponent(typeof(BaseAmbientSoundPlayer))]
public class SoundPlayerRoomObserverComponent : MonoBehaviour
{
	private const int int_0 = -1;

	[SerializeField]
	private List<SpatialAudioRoom> _observedRooms;

	private ISoundPlayer isoundPlayer_0;

	private readonly HashSet<int> hashSet_0 = new HashSet<int>();

	private Action action_0;

	private int int_1 = -1;

	public void Awake()
	{
		isoundPlayer_0 = GetComponent<ISoundPlayer>();
		method_0();
		method_1();
	}

	public void method_0()
	{
		for (int i = 0; i < _observedRooms.Count; i++)
		{
			SpatialAudioRoom spatialAudioRoom = _observedRooms[i];
			if (spatialAudioRoom == null)
			{
				_observedRooms.Remove(spatialAudioRoom);
			}
			else
			{
				hashSet_0.Add(spatialAudioRoom.ID);
			}
		}
	}

	public void method_1()
	{
		if (_observedRooms.Count == 0)
		{
			Debug.LogError("Observed Rooms List Is Empty!");
			return;
		}
		GlobalEventHandlerClass instance = GlobalEventHandlerClass.Instance;
		action_0 = instance.SubscribeOnEvent<GClass3573>(method_3);
		action_0 = (Action)Delegate.Combine(action_0, instance.SubscribeOnEvent<GClass3566>(method_2));
	}

	public void method_2(GClass3566 ambientAudioSystemInitializedEvent)
	{
		method_4(int_1);
	}

	public void method_3(GClass3573 changedEvent)
	{
		if (changedEvent.InteractionState != EPlayerRoomInteractionState.Exit)
		{
			int currentRoomID = changedEvent.CurrentRoomID;
			method_4(currentRoomID);
			int_1 = currentRoomID;
		}
	}

	public void method_4(int roomID)
	{
		if (roomID == -1)
		{
			return;
		}
		if (hashSet_0.Contains(roomID))
		{
			if (!isoundPlayer_0.IsPlaying)
			{
				isoundPlayer_0.Play();
			}
		}
		else
		{
			isoundPlayer_0.Stop();
		}
	}

	public void OnDestroy()
	{
		action_0?.Invoke();
		action_0 = null;
	}
}
