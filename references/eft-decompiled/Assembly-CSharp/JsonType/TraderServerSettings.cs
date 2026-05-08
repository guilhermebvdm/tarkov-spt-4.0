using System;
using System.Collections.Generic;
using EFT;
using Newtonsoft.Json;

namespace JsonType;

[Serializable]
public class TraderServerSettings
{
	[JsonProperty("TraderServices")]
	public Dictionary<ETraderServiceType, BackendConfigSettingsClass.ServiceData> ServicesData_1;

	public IReadOnlyDictionary<ETraderServiceType, BackendConfigSettingsClass.ServiceData> ServicesData => ServicesData_1;
}
