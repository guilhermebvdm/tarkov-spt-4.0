using System;

public struct GStruct110
{
	public float bakeWidth;

	public float bakeHeight;

	public float fov;

	public float nearClipPlane;

	public float farClipPlane;

	public int debugMode;

	[Obsolete("Unused")]
	public int showConsole;

	[Obsolete("Unused, drawing meshes uses individual settings")]
	public int useWireFrame;

	public int outputPixelData;
}
