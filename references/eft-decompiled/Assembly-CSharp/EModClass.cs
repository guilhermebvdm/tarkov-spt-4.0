using System;

[Flags]
public enum EModClass
{
	None = 0,
	Functional = 1,
	Master = 2,
	Gear = 4,
	Auxiliary = 8,
	ArmorPlate = 0x10,
	All = 7
}
