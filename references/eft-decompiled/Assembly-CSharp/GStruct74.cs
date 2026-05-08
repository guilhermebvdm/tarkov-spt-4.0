using MultiFlare;
using UnityEngine;

public struct GStruct74(MultiFlare.Flare flare, int lightIndex, Rect uv)
{
	public int LightIndex = lightIndex;

	public Vector2 Scale = flare.Scale;

	public float Alpha = flare.Alpha;

	public float MinDist = flare.MinDist;

	public float MaxDist = flare.MaxDist;

	public float MinScale = flare.MinScale;

	public float MaxScale = flare.MaxScale;

	public float MinAlpha = flare.MinAlpha;

	public float MaxAlpha = flare.MaxAlpha;

	public Color Color = flare.Color;

	public Rect Uv = uv;
}
