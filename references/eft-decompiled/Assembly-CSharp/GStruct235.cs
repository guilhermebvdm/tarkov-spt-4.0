using System;
using System.Runtime.CompilerServices;
using BitPacking;
using EFT;
using UnityEngine;

public struct GStruct235 : GInterface217<GStruct235>
{
	[NonSerialized]
	public static GClass1378 Gclass1378_0 = new GClass1378(-1f, 1f, 0.00048828125f, -1f, 1f, 0.00048828125f, -1f, 1f, 0.00048828125f);

	[NonSerialized]
	public static GClass1378 Gclass1378_1 = new GClass1378(-2f, 2f, 0.00048828125f, -2f, 2f, 0.00048828125f, -2f, 2f, 0.00048828125f);

	public bool IsPrimaryActive;

	public int AmmoAfterShot;

	public EShotType EShotType;

	public Vector3 ShotPosition;

	public Vector3 ShotDirection;

	public Vector3 FireportPosition;

	public int ChamberIndex;

	public float Overheat;

	public bool UnderbarrelShot;

	[NonSerialized]
	[CompilerGenerated]
	public GInterface217<GStruct235> Ginterface217_0;

	public GInterface217<GStruct235> Nested
	{
		[CompilerGenerated]
		readonly get
		{
			return Ginterface217_0;
		}
		[CompilerGenerated]
		set
		{
			Ginterface217_0 = value;
		}
	}

	public static void Serialize(GInterface131 writer, ref GStruct235? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			GStruct235 valueOrDefault = packet.GetValueOrDefault();
			writer.WriteLimitedInt32(valueOrDefault.ChamberIndex, 0, 10, BitPackingTag.ChamberFireIndex);
			writer.Write((byte)valueOrDefault.AmmoAfterShot);
			writer.Write(valueOrDefault.IsPrimaryActive);
			writer.Write(valueOrDefault.EShotType);
			writer.Write(valueOrDefault.UnderbarrelShot);
			if (valueOrDefault.EShotType == EShotType.RegularShot || valueOrDefault.EShotType == EShotType.JamedShot || valueOrDefault.EShotType == EShotType.SoftSlidedShot || valueOrDefault.EShotType == EShotType.HardSlidedShot || valueOrDefault.EShotType == EShotType.Feed)
			{
				GClass1803.UsualPositionQuantizer.Write(writer, valueOrDefault.ShotPosition);
				Gclass1378_0.Write(writer, valueOrDefault.ShotDirection);
				Vector3 value = valueOrDefault.FireportPosition - valueOrDefault.ShotPosition;
				Gclass1378_1.Write(writer, value);
			}
			if (valueOrDefault.EShotType != EShotType.DryFire)
			{
				writer.WriteLimitedFloat(valueOrDefault.Overheat, 0f, 250f, 0.01f, BitPackingTag.Overheat);
			}
			GClass2084.Serialize(writer, valueOrDefault.Nested, Serialize);
		}
	}

	public static GStruct235? Deserialize(IDataReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		GStruct235 value = new GStruct235
		{
			ChamberIndex = reader.ReadLimitedInt32(0, 10),
			AmmoAfterShot = reader.ReadByte(),
			IsPrimaryActive = reader.ReadBool(),
			EShotType = reader.ReadEnum<EShotType>(),
			UnderbarrelShot = reader.ReadBool()
		};
		if (value.EShotType == EShotType.RegularShot || value.EShotType == EShotType.JamedShot || value.EShotType == EShotType.SoftSlidedShot || value.EShotType == EShotType.HardSlidedShot || value.EShotType == EShotType.Feed)
		{
			GClass1803.UsualPositionQuantizer.Read(reader, out value.ShotPosition);
			Gclass1378_0.Read(reader, out value.ShotDirection);
			Gclass1378_1.Read(reader, out var value2);
			value.FireportPosition = value.ShotPosition + value2;
		}
		if (value.EShotType != EShotType.DryFire)
		{
			value.Overheat = reader.ReadLimitedFloat(0f, 250f, 0.01f);
		}
		value.Nested = GClass2084.Deserialize(reader, Deserialize);
		return value;
	}

	public override string ToString()
	{
		return $"AmmoAfterShot: {AmmoAfterShot}, EShotType: {EShotType}, Next: {Nested}";
	}
}
