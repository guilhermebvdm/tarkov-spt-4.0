using MultiFlare;
using UnityEngine;

public struct GStruct76(FlareLight flareLight)
{
	public float Alpha = flareLight.Alpha;

	public float Scale = flareLight.Scale;

	public bool Enabled = flareLight.Enabled;

	public Vector3 WorldPos = flareLight.transform.position;
}
