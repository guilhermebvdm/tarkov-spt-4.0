using System.Collections.Generic;
using Newtonsoft.Json;

[JsonArray]
public class GClass1851 : List<GClass1852>
{
	public GClass1851()
	{
	}

	[JsonConstructor]
	public GClass1851(IEnumerable<GClass1852> collection)
		: base(collection)
	{
	}
}
