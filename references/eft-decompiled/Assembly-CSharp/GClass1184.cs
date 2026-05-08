using System;
using Audio.AmbientSubsystem;
using Audio.SpatialSystem;
using UnityEngine.Audio;

public class GClass1184 : IDisposable
{
	[NonSerialized]
	public IEnvironmentMixerParamsHandler[] IenvironmentMixerParamsHandler_0;

	[NonSerialized]
	public Action Action_0;

	[NonSerialized]
	public EAudioRoomTypeMask EaudioRoomTypeMask_0;

	public GClass1184(params IEnvironmentMixerParamsHandler[] paramsHandlers)
	{
		IenvironmentMixerParamsHandler_0 = paramsHandlers;
		Action_0 = GlobalEventHandlerClass.Instance.SubscribeOnEvent<GClass3573>(method_0);
		method_2();
	}

	public void method_0(GClass3573 changedEvent)
	{
		method_1(changedEvent.CurrentRoomType);
	}

	public void method_1(EAudioRoomTypeMask roomTypeMask)
	{
		if (!GClass867.Is(EaudioRoomTypeMask_0, roomTypeMask) && MonoBehaviourSingleton<BetterAudio>.Instantiated)
		{
			AudioMixer master = MonoBehaviourSingleton<BetterAudio>.Instance.Master;
			IEnvironmentMixerParamsHandler[] ienvironmentMixerParamsHandler_ = IenvironmentMixerParamsHandler_0;
			for (int i = 0; i < ienvironmentMixerParamsHandler_.Length; i++)
			{
				ienvironmentMixerParamsHandler_[i].Apply(master, roomTypeMask);
			}
			EaudioRoomTypeMask_0 = roomTypeMask;
		}
	}

	public void method_2()
	{
		if (SpatialAudioSystem.Initialized)
		{
			ISpatialAudioRoom listenerCurrentRoom = MonoBehaviourSingleton<SpatialAudioSystem>.Instance.ListenerCurrentRoom;
			method_1(listenerCurrentRoom.Type);
			EaudioRoomTypeMask_0 = listenerCurrentRoom.Type;
		}
	}

	~GClass1184()
	{
		Dispose();
	}

	public void Dispose()
	{
		Action_0?.Invoke();
		Action_0 = null;
	}
}
