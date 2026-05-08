using UnityEngine;

namespace Koenigz.PerfectCulling.EFT;

public interface IAutocullAutomated
{
	bool IsAutocullVisible { get; set; }

	Bounds AutocullObjectBounds { get; }

	bool IsDynamicCullingObject { get; }
}
