using Newtonsoft.Json;

public class ProfileInsuranceClass
{
	[JsonProperty("insuredItems")]
	public InsuredItemClass[] insuredItems;
}
