using System;
using System.Linq;
using System.Runtime.CompilerServices;
using BitPacking;

public struct GStruct238
{
	[Serializable]
	[CompilerGenerated]
	public class Class1381
	{
		public static readonly Class1381 class1381_0 = new Class1381();

		public static Func<GStruct239, string> func_0;

		public string method_0(GStruct239 x)
		{
			return x.ToString();
		}
	}

	public bool ToggleTacticalCombo;

	public GStruct239[] TacticalComboStatuses;

	public static void Serialize(ISerializer stream, ref GStruct238 packet)
	{
		stream.Serialize(ref packet.ToggleTacticalCombo);
		if (packet.ToggleTacticalCombo)
		{
			int value = ((stream.StreamMode == EBitStreamMode.Writing) ? packet.TacticalComboStatuses.Length : 0);
			stream.SerializeLimitedInt32(ref value, 0, 7, BitPackingTag.ToggleTacticalComboPacket0);
			if (stream.StreamMode == EBitStreamMode.Reading)
			{
				packet.TacticalComboStatuses = new GStruct239[value];
			}
			for (int i = 0; i < value; i++)
			{
				GStruct239.Serialize(stream, ref packet.TacticalComboStatuses[i]);
			}
		}
	}

	public override string ToString()
	{
		return string.Format("ToggleTacticalCombo: {0}, TacticalComboStatuses: {1}", ToggleTacticalCombo, (TacticalComboStatuses == null || TacticalComboStatuses.Length == 0) ? "empty" : string.Join(", ", TacticalComboStatuses.Select((GStruct239 x) => x.ToString()).ToArray()));
	}
}
