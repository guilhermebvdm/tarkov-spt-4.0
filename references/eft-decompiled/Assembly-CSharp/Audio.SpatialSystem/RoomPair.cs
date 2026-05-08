using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Audio.SpatialSystem;

[Serializable]
public class RoomPair : IEquatable<RoomPair>, IBinarySerializable
{
	[Serializable]
	public struct Route : IBinarySerializable
	{
		public short[] PortalIDs;

		public float HeuristicCost;

		public float TraverseDistance;

		public void Write(BinaryWriter writer)
		{
			writer.Write(PortalIDs.Length);
			for (int i = 0; i < PortalIDs.Length; i++)
			{
				short value = PortalIDs[i];
				writer.Write(value);
			}
			writer.Write(HeuristicCost);
			writer.Write(TraverseDistance);
		}

		public void Read(BinaryReader reader)
		{
			int num = reader.ReadInt32();
			PortalIDs = new short[num];
			GClass1138 data = GClass3670.GetData<GClass1138>();
			for (int i = 0; i < num; i++)
			{
				short id = reader.ReadInt16();
				if (data.TryGetPortalById(id, out var portal))
				{
					PortalIDs[i] = portal.ID;
				}
			}
			HeuristicCost = reader.ReadSingle();
			TraverseDistance = reader.ReadSingle();
		}
	}

	[Serializable]
	public struct AudioPortalData
	{
		public float closureLevel;

		public float3 center;

		public float traversalMaxCost;

		public float depth;

		public float frontRoomWallOcclusion;

		public float backRoomWallOcclusion;

		public bool isFrontRoomOutdoor;

		public bool isBackRoomOutdoor;

		public float3 portalNormal;

		public float3 portalRight;

		public float3 portalUp;

		public float2 portalHalfSize;
	}

	[Serializable]
	[CompilerGenerated]
	public class Class789
	{
		public static readonly Class789 class789_0 = new Class789();

		public static Func<Route, int> func_0;

		public int method_0(Route x)
		{
			return x.PortalIDs.Length;
		}
	}

	public uint ID;

	public short FirstRoomID;

	public short SecondRoomID;

	public Route[] Routes;

	public int TotalPortalsCount;

	public byte ShortestRouteLength;

	public RoomPair()
	{
	}

	public RoomPair(uint id, SpatialAudioRoom firstRoom, SpatialAudioRoom secondRoom, Route[] routes, int totalPortalsCount)
	{
		ID = id;
		FirstRoomID = firstRoom.ID;
		SecondRoomID = secondRoom.ID;
		Routes = routes;
		TotalPortalsCount = totalPortalsCount;
		ShortestRouteLength = (byte)routes.OrderBy((Route x) => x.PortalIDs.Length).First().PortalIDs.Length;
	}

	public bool IsMatch(ISpatialAudioRoom firstRoom, ISpatialAudioRoom secondRoom, out bool isReversed)
	{
		if (FirstRoomID == firstRoom.ID && SecondRoomID == secondRoom.ID)
		{
			isReversed = false;
			return true;
		}
		if (FirstRoomID == secondRoom.ID && SecondRoomID == firstRoom.ID)
		{
			isReversed = true;
			return true;
		}
		isReversed = false;
		return false;
	}

	public bool Equals(RoomPair other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (FirstRoomID == other.FirstRoomID)
		{
			return SecondRoomID == other.SecondRoomID;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (this != obj)
		{
			if (obj is RoomPair other)
			{
				return Equals(other);
			}
			return false;
		}
		return true;
	}

	public override int GetHashCode()
	{
		return (FirstRoomID.GetHashCode() * 397) ^ SecondRoomID.GetHashCode();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(ID);
		writer.Write(FirstRoomID);
		writer.Write(SecondRoomID);
		writer.Write(Routes.Length);
		for (int i = 0; i < Routes.Length; i++)
		{
			Routes[i].Write(writer);
		}
		writer.Write(TotalPortalsCount);
		writer.Write(ShortestRouteLength);
	}

	public void Read(BinaryReader reader)
	{
		ID = reader.ReadUInt32();
		FirstRoomID = reader.ReadInt16();
		SecondRoomID = reader.ReadInt16();
		int num = reader.ReadInt32();
		Routes = new Route[num];
		for (int i = 0; i < num; i++)
		{
			Routes[i].Read(reader);
		}
		TotalPortalsCount = reader.ReadInt32();
		ShortestRouteLength = reader.ReadByte();
	}
}
