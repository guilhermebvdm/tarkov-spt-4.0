using System;

namespace Audio.SpatialSystem;

[Flags]
public enum EAudioRoomTypeMask
{
	None = 0,
	OutdoorGeneric = 1,
	OutdoorVehicle = 2,
	IndoorGeneric = 4,
	IndoorIsolated = 8,
	IndoorVehicle = 0x10,
	IndoorStairs = 0x20,
	OutdoorCommon = 0x40,
	IndoorBasement = 0x80,
	IndoorTransparent = 0x100,
	IndoorMetalThin = 0x200,
	IndoorWoodThick = 0x400,
	IndoorBrick = 0x800,
	IndoorConcrete = 0x1000,
	IndoorPlasterBoard = 0x2000,
	IndoorWoodThin = 0x4000,
	IndoorFabric = 0x8000,
	IndoorMetalThick = 0x10000,
	Outdoor = 0x43,
	Indoor = 0x1FFBC,
	Isolated = 0x88
}
