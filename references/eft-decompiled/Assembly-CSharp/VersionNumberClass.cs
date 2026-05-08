using Newtonsoft.Json;
using UnityEngine;

public class VersionNumberClass
{
	[JsonProperty("major")]
	public string Major;

	[JsonProperty("minor")]
	public string Minor;

	[JsonProperty("game")]
	public string Game;

	[JsonProperty("backend")]
	public string Backend;

	[JsonProperty("taxonomy")]
	public string Taxonomy;

	public static VersionNumberClass Current => Create();

	public override string ToString()
	{
		return "Major: " + Major + ", Minor: " + Minor + ", Game: " + Game + ", Backend: " + Backend + ", Taxonomy: " + Taxonomy;
	}

	public static VersionNumberClass Create(string major = "0.16.9.0.40087", string minor = "", string backend = "6", string taxonomy = "341")
	{
		VersionNumberClass versionNumberClass = new VersionNumberClass
		{
			Major = major,
			Minor = minor,
			Game = minor,
			Backend = backend,
			Taxonomy = taxonomy
		};
		Debug.Log("Version: " + versionNumberClass);
		return versionNumberClass;
	}
}
