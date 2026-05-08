using System;
using System.Collections.Generic;
using CommonAssets.Scripts.Audio.RadioSystem;
using Newtonsoft.Json;

public class GClass1499
{
	public class GClass1500
	{
		public class GClass1501
		{
			public ERadioStation Station;

			public bool Enabled;
		}

		public bool EnabledBroadcast;

		public List<GClass1501> RadioStations = new List<GClass1501>();

		[NonSerialized]
		public Dictionary<ERadioStation, bool> Dictionary_0 = new Dictionary<ERadioStation, bool>();

		public bool GetStationEnabledState(ERadioStation station)
		{
			if (Dictionary_0.Count == 0)
			{
				method_0();
			}
			bool value;
			return Dictionary_0.TryGetValue(station, out value) && value;
		}

		public void method_0()
		{
			foreach (GClass1501 radioStation in RadioStations)
			{
				Dictionary_0[radioStation.Station] = radioStation.Enabled;
			}
		}
	}

	[JsonProperty("RadioBroadcastSettings")]
	public GClass1500 RadioSettings = new GClass1500();
}
