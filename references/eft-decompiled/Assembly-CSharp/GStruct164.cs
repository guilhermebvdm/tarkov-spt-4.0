using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public struct GStruct164 : IDisposable
{
	[NonSerialized]
	[CompilerGenerated]
	public int Int_0;

	public float RemoteTime;

	public GStruct165? InteractiveObjectsStatusPacket;

	public GStruct166? UpdateExfiltrationPointPacket;

	public GStruct167? LampChangeStatePacket;

	public GStruct168? WindowHitPacket;

	public List<LootSyncStruct> LootSyncPackets;

	public List<RagdollPacketStruct> CorpseSyncPackets;

	public List<GrenadeDataPacketStruct> GrenadeSyncPackets;

	public List<ArtilleryPacketStruct> ArtilleryShellSyncPackets;

	public List<GStruct173> PlatformSyncPackets;

	public List<GStruct174> BorderZonePackets;

	public List<AirplaneDataPacketStruct> SynchronizableObjectPackets;

	public int PriorityCounter
	{
		[CompilerGenerated]
		readonly get
		{
			return Int_0;
		}
		[CompilerGenerated]
		set
		{
			Int_0 = value;
		}
	}

	public bool HasData
	{
		get
		{
			if (!InteractiveObjectsStatusPacket.HasValue && !UpdateExfiltrationPointPacket.HasValue && !LampChangeStatePacket.HasValue && !WindowHitPacket.HasValue && LootSyncPackets.Count <= 0 && CorpseSyncPackets.Count <= 0 && GrenadeSyncPackets.Count <= 0 && ArtilleryShellSyncPackets.Count <= 0 && PlatformSyncPackets.Count <= 0 && BorderZonePackets.Count <= 0)
			{
				return SynchronizableObjectPackets.Count > 0;
			}
			return true;
		}
	}

	public static GStruct164 Create()
	{
		return new GStruct164
		{
			LootSyncPackets = GClass1713.LootSyncPacketsPool.Withdraw(),
			CorpseSyncPackets = GClass1713.CorpseSyncPacketsPool.Withdraw(),
			GrenadeSyncPackets = GClass1713.GrenadeSyncPacketsPool.Withdraw(),
			ArtilleryShellSyncPackets = GClass1713.ArtilleryShellSyncPacketPool.Withdraw(),
			PlatformSyncPackets = GClass1713.PlatformSyncPacketsPool.Withdraw(),
			BorderZonePackets = GClass1713.BorderZonePacketsPool.Withdraw(),
			SynchronizableObjectPackets = GClass1713.SynchronizableObjectPacketsPool.Withdraw()
		};
	}

	public void Dispose()
	{
		if (LootSyncPackets != null)
		{
			GClass1713.LootSyncPacketsPool.Return(LootSyncPackets);
			LootSyncPackets = null;
		}
		if (SynchronizableObjectPackets != null)
		{
			GClass1713.SynchronizableObjectPacketsPool.Return(SynchronizableObjectPackets);
			SynchronizableObjectPackets = null;
		}
		if (CorpseSyncPackets != null)
		{
			GClass1713.CorpseSyncPacketsPool.Return(CorpseSyncPackets);
			CorpseSyncPackets = null;
		}
		if (GrenadeSyncPackets != null)
		{
			GClass1713.GrenadeSyncPacketsPool.Return(GrenadeSyncPackets);
			GrenadeSyncPackets = null;
		}
		if (ArtilleryShellSyncPackets != null)
		{
			GClass1713.ArtilleryShellSyncPacketPool.Return(ArtilleryShellSyncPackets);
			ArtilleryShellSyncPackets = null;
		}
		if (PlatformSyncPackets != null)
		{
			GClass1713.PlatformSyncPacketsPool.Return(PlatformSyncPackets);
			PlatformSyncPackets = null;
		}
		if (BorderZonePackets != null)
		{
			GClass1713.BorderZonePacketsPool.Return(BorderZonePackets);
			BorderZonePackets = null;
		}
	}

	public GStruct164 Clone()
	{
		GStruct164 result = this;
		result.LootSyncPackets = GClass1713.LootSyncPacketsPool.Withdraw();
		result.LootSyncPackets.AddRange(LootSyncPackets);
		result.CorpseSyncPackets = GClass1713.CorpseSyncPacketsPool.Withdraw();
		result.CorpseSyncPackets.AddRange(CorpseSyncPackets);
		result.GrenadeSyncPackets = GClass1713.GrenadeSyncPacketsPool.Withdraw();
		result.GrenadeSyncPackets.AddRange(GrenadeSyncPackets);
		result.ArtilleryShellSyncPackets = GClass1713.ArtilleryShellSyncPacketPool.Withdraw();
		result.ArtilleryShellSyncPackets.AddRange(ArtilleryShellSyncPackets);
		result.PlatformSyncPackets = GClass1713.PlatformSyncPacketsPool.Withdraw();
		result.PlatformSyncPackets.AddRange(PlatformSyncPackets);
		result.BorderZonePackets = GClass1713.BorderZonePacketsPool.Withdraw();
		result.BorderZonePackets.AddRange(BorderZonePackets);
		result.SynchronizableObjectPackets = GClass1713.SynchronizableObjectPacketsPool.Withdraw();
		result.SynchronizableObjectPackets.AddRange(SynchronizableObjectPackets);
		return result;
	}
}
