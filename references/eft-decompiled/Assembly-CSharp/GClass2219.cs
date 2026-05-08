using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

[GAttribute29]
public class GClass2219
{
	[JsonProperty("Items", NullValueHandling = NullValueHandling.Ignore)]
	public List<GClass2218> Items = new List<GClass2218>();

	public GClass2219()
	{
	}

	public GClass2219(SessionCountersClass countersCollection)
	{
		foreach (KeyValuePair<SessionCountersClass.SessionCounterIdentifierValueClass, long> counter in countersCollection.Counters)
		{
			counter.Deconstruct(out var key, out var value);
			SessionCountersClass.SessionCounterIdentifierValueClass sessionCounterIdentifierValueClass = key;
			long value2 = value;
			List<string> key2 = sessionCounterIdentifierValueClass.Set.ToList();
			GClass2218 item = new GClass2218
			{
				Key = key2,
				Value = value2
			};
			Items.Add(item);
		}
	}
}
