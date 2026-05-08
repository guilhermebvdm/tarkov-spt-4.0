using System;
using System.Collections.Generic;
using Dissonance.Config;
using Dissonance.Networking;
using UnityEngine;

namespace EFT;

public class TrafficCollectScenario : MonoBehaviour
{
	public class Class394 : LoggerClass
	{
		public Class394()
			: base("traffic", LoggerMode.Add)
		{
		}
	}

	public struct Struct303
	{
		public TimeSpan TimeSpan;

		public GStruct177 GameSent;

		public GStruct177 GameReceived;

		public GStruct177 VoipSent;

		public GStruct177 VoipReceived;
	}

	private IGame iGame;

	private TimeSpan timeSpan_0;

	private Class394 class394_0;

	private DateTime dateTime_0;

	private readonly List<Struct303> list_0 = new List<Struct303>();

	private Struct303 struct303_0;

	public static TimeSpan TimeSpan_0 => TimeSpan.FromMinutes(1.0);

	public static TrafficCollectScenario smethod_0(IGame game, TimeSpan? step = null, Class394 logger = null)
	{
		TrafficCollectScenario trafficCollectScenario = game.gameObject.AddComponent<TrafficCollectScenario>();
		trafficCollectScenario.iGame = game;
		trafficCollectScenario.timeSpan_0 = step ?? TimeSpan_0;
		trafficCollectScenario.class394_0 = logger ?? new Class394();
		trafficCollectScenario.class394_0.LogInfo($"VoiceSettings:{VoiceSettings.Instance}");
		return trafficCollectScenario;
	}

	public GClass1898 TrafficData()
	{
		return new GClass1898
		{
			GameSent = struct303_0.GameSent,
			GameReceived = struct303_0.GameReceived,
			VoipSent = struct303_0.VoipSent,
			VoipReceived = struct303_0.VoipReceived
		};
	}

	public void Update()
	{
		DateTime utcNow = DateTime.UtcNow;
		if (!(utcNow - dateTime_0 < timeSpan_0))
		{
			dateTime_0 = utcNow;
			struct303_0 = method_0();
			list_0.Add(struct303_0);
			class394_0.LogInfo($"TimeSpan:{struct303_0.TimeSpan.TotalMinutes} GameSent:{struct303_0.GameSent} GameReceived:{struct303_0.GameReceived} VoipSent:{struct303_0.VoipSent} VoipReceived:{struct303_0.VoipReceived}");
		}
	}

	public Struct303 method_0()
	{
		return new Struct303
		{
			TimeSpan = DateTime.UtcNow - dateTime_0,
			GameSent = smethod_1(TrafficCounters.GameSentTraffic),
			GameReceived = smethod_1(TrafficCounters.GameReceivedTraffic),
			VoipSent = smethod_1(TrafficCounters.VoipSentTraffic),
			VoipReceived = smethod_1(TrafficCounters.VoipReceivedTraffic)
		};
	}

	public static GStruct177 smethod_1(TrafficCounter counter)
	{
		return new GStruct177
		{
			Bytes = counter.Bytes,
			BytesPerSecond = counter.BytesPerSecond,
			Packets = counter.Packets
		};
	}
}
