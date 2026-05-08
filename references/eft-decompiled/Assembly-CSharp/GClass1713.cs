public abstract class GClass1713
{
	public static GClass834<LootSyncStruct> LootSyncPacketsPool = new GClass834<LootSyncStruct>(16);

	public static GClass834<RagdollPacketStruct> CorpseSyncPacketsPool = new GClass834<RagdollPacketStruct>(16);

	public static GClass834<GrenadeDataPacketStruct> GrenadeSyncPacketsPool = new GClass834<GrenadeDataPacketStruct>(16);

	public static GClass834<ArtilleryPacketStruct> ArtilleryShellSyncPacketPool = new GClass834<ArtilleryPacketStruct>(16);

	public static GClass834<GStruct173> PlatformSyncPacketsPool = new GClass834<GStruct173>(8);

	public static GClass834<GStruct174> BorderZonePacketsPool = new GClass834<GStruct174>(8);

	public static GClass834<AirplaneDataPacketStruct> SynchronizableObjectPacketsPool = new GClass834<AirplaneDataPacketStruct>(16);
}
