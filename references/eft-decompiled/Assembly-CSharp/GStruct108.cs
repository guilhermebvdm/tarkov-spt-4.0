using Audio.SpatialSystem.Data;

public struct GStruct108(float score, float distanceToListener, PortalData portalData)
{
	public float score = score;

	public float distanceToListener = distanceToListener;

	public readonly PortalData portalData = portalData;

	public static GStruct108 Default()
	{
		return new GStruct108(float.MaxValue, float.MaxValue, PortalData.Default());
	}
}
