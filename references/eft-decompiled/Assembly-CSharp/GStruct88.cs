using Unity.Mathematics;

public readonly struct GStruct88(GStruct88.ENodeType type, float3 position, float portalClosureLevel = 0f, float portalTraversalMaxCost = 0f, float portalDepth = 0f, float frontRoomWallOcclusion = 1f, float backRoomWallOcclusion = 1f, bool isFrontRoomOutdoor = false, bool isBackRoomOutdoor = false)
{
	public enum ENodeType
	{
		Emitter,
		Portal,
		Listener
	}

	public readonly ENodeType Type = type;

	public readonly float3 Position = position;

	public readonly float PortalClosureLevel = portalClosureLevel;

	public readonly float PortalTraversalMaxCost = portalTraversalMaxCost;

	public readonly float PortalDepth = portalDepth;

	public readonly float FrontRoomWallOcclusion = frontRoomWallOcclusion;

	public readonly float BackRoomWallOcclusion = backRoomWallOcclusion;

	public readonly bool IsFrontRoomOutdoor = isFrontRoomOutdoor;

	public readonly bool IsBackRoomOutdoor = isBackRoomOutdoor;

	public GStruct88 Clone()
	{
		return new GStruct88(Type, Position, PortalClosureLevel, PortalTraversalMaxCost, PortalDepth, FrontRoomWallOcclusion, BackRoomWallOcclusion, IsFrontRoomOutdoor, IsBackRoomOutdoor);
	}
}
