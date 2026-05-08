using System.Collections.Generic;
using Newtonsoft.Json;

public class GClass1791
{
	public readonly IReadOnlyList<GClass2661> Templates;

	[JsonConstructor]
	public GClass1791([JsonProperty("elements")] List<GClass2661> templates)
	{
		Templates = templates;
	}
}
