using System;
using UnityEngine;

[Serializable]
public class TOD_StarParameters
{
	[Tooltip("Tiling of the stars texture.")]
	[GAttribute0(0f)]
	public float Tiling = 6f;

	[Tooltip("Brightness of the stars.")]
	[GAttribute0(0f)]
	public float Brightness = 3f;

	[Tooltip("Type of the stars position calculation.")]
	public TOD_StarsPositionType Position = TOD_StarsPositionType.Rotating;
}
