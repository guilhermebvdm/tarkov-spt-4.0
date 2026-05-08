using System;
using System.Runtime.InteropServices;
using BitPacking;
using EFT.SynchronizableObjects;
using UnityEngine;

public struct AirplaneDataPacketStruct
{
	[StructLayout(LayoutKind.Explicit)]
	public struct GStruct47
	{
		[FieldOffset(0)]
		public GStruct43 AirplaneDataPacket;

		[FieldOffset(0)]
		public AirdropDataPacketStruct AirdropDataPacket;

		[FieldOffset(0)]
		public GStruct44 DevelopDataPacket;

		[FieldOffset(0)]
		public GStruct45 TripwireDataPacket;
	}

	public bool Develop;

	public int ObjectId;

	public Vector3 Position;

	public Vector3 Rotation;

	public SynchronizableObjectType ObjectType;

	public bool Outdated;

	public bool IsStatic;

	public bool IsActive;

	public GStruct47 PacketData;

	[NonSerialized]
	public static GClass1373 Gclass1373_0 = new GClass1373(1f / 64f, BitPackingTag.LootSyncPacketRotationQuantizer);
}
