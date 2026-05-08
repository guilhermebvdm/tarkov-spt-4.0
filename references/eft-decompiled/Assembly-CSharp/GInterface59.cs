using System.Collections.Generic;
using UnityEngine;

public interface GInterface59
{
	bool IsDebugModeEnabled { get; set; }

	IEnumerable<Camera> GetCameras();

	IEnumerable<GInterface58> GetBatches();
}
