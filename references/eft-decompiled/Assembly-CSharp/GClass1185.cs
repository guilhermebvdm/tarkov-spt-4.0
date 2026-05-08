using System;
using Audio.SpatialSystem;

public class GClass1185 : IDisposable
{
	[NonSerialized]
	public const int Int_0 = -1;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public ISpatialAudioRoom IspatialAudioRoom_0;

	[NonSerialized]
	public int Int_1 = -1;

	[NonSerialized]
	public Action Action_0;

	public GClass1185()
	{
		method_1(MonoBehaviourSingleton<SpatialAudioSystem>.Instance.ListenerCurrentRoom);
		Action_0 = GlobalEventHandlerClass.Instance.SubscribeOnEvent<GClass3573>(method_0);
	}

	public void method_0(GClass3573 roomChangedEvent)
	{
		method_1(roomChangedEvent.Room);
	}

	public void method_1(ISpatialAudioRoom newRoom)
	{
		if (IspatialAudioRoom_0?.ID == newRoom?.ID)
		{
			return;
		}
		Bool_0 = IspatialAudioRoom_0 != null && !GClass1118.IsOutdoor(IspatialAudioRoom_0.Type);
		if (newRoom != null && !GClass1118.IsOutdoor(newRoom.Type))
		{
			int roomToneClipHash = newRoom.RoomAmbientData.RoomToneClipHash;
			if (roomToneClipHash != -1 && roomToneClipHash != Int_1)
			{
				method_2();
				newRoom.PlayRoomToneSound();
				Int_1 = roomToneClipHash;
				IspatialAudioRoom_0 = newRoom;
			}
		}
		else
		{
			method_2();
			method_3();
		}
	}

	public void method_2()
	{
		if (Bool_0)
		{
			IspatialAudioRoom_0.StopRoomToneSound();
		}
	}

	public void Dispose()
	{
		Action_0?.Invoke();
		Action_0 = null;
		method_2();
		method_3();
	}

	public void method_3()
	{
		Bool_0 = false;
		Int_1 = -1;
		IspatialAudioRoom_0 = null;
	}
}
