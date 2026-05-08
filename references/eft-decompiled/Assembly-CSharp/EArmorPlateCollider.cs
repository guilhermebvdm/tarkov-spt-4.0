using System;

[Flags]
public enum EArmorPlateCollider : short
{
	Plate_Granit_SAPI_chest = 1,
	Plate_Granit_SAPI_back = 2,
	Plate_Granit_SSAPI_side_left_high = 4,
	Plate_Granit_SSAPI_side_left_low = 8,
	Plate_Granit_SSAPI_side_right_high = 0x10,
	Plate_Granit_SSAPI_side_right_low = 0x20,
	Plate_Korund_chest = 0x40,
	Plate_6B13_back = 0x80,
	Plate_Korund_side_left_high = 0x100,
	Plate_Korund_side_left_low = 0x200,
	Plate_Korund_side_right_high = 0x400,
	Plate_Korund_side_right_low = 0x800
}
