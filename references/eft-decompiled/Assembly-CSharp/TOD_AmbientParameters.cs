using System;
using UnityEngine;

[Serializable]
public class TOD_AmbientParameters
{
	[Tooltip("Ambient light mode.")]
	public TOD_AmbientType Mode = TOD_AmbientType.Color;

	[Tooltip("Refresh interval of the ambient light probe in seconds.")]
	[GAttribute0(0f)]
	public float UpdateInterval = 1f;
}
