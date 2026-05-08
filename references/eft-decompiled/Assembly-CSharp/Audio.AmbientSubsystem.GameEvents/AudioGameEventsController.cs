using System;
using System.Linq;
using EFT;
using UnityEngine;

namespace Audio.AmbientSubsystem.GameEvents;

public class AudioGameEventsController : MonoBehaviour
{
	private GInterface104[] ginterface104_0 = Array.Empty<GInterface104>();

	private GInterface108 ginterface108_0;

	private GInterface107 ginterface107_0;

	public void Init()
	{
		ginterface107_0 = new GClass1194();
		ginterface104_0 = LocationScene.GetAllObjectsNoBehaviour<GInterface104>().ToArray();
	}

	public GInterface108 method_0(EEventType eventType)
	{
		return eventType switch
		{
			EEventType.Christmas => ginterface107_0.CreateChristmasEventPlayer(), 
			EEventType.Halloween => ginterface107_0.CreateHalloweenEventPlayer(), 
			_ => ginterface107_0.CreateFakeEventPlayer(), 
		};
	}

	public void RunEvent(EEventType eventType)
	{
		method_1(eventType);
		ginterface108_0?.Stop();
		ginterface108_0 = method_0(eventType);
		ginterface108_0.Run();
	}

	public void StopCurrentEvent()
	{
		ginterface108_0?.Stop();
	}

	public void method_1(EEventType eventType)
	{
		GInterface104[] array = ginterface104_0;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ChangeSoundContent(eventType);
		}
	}

	public void OnDestroy()
	{
		StopCurrentEvent();
	}
}
