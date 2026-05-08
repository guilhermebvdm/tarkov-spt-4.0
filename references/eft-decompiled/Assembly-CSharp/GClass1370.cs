using UnityEngine;

public abstract class GClass1370
{
	public static uint QuantizeFloatValue(this FloatQuantizerStruct floatQuantizer, float value)
	{
		return (uint)Mathf.FloorToInt(Mathf.Clamp((value - floatQuantizer.Min) / floatQuantizer.Delta, 0f, 1f) * (float)floatQuantizer.MaxIntegerValue + 0.5f);
	}

	public static float DequantizeUIntValue(this FloatQuantizerStruct floatQuantizer, uint value)
	{
		return (float)value / (float)floatQuantizer.MaxIntegerValue * floatQuantizer.Delta + floatQuantizer.Min;
	}

	public static float Write(this FloatQuantizerStruct floatQuantizer, GInterface131 bitWriterStream, float value)
	{
		uint value2 = QuantizeFloatValue(floatQuantizer, value);
		bitWriterStream.WriteBits(value2, floatQuantizer.BitsRequired);
		return DequantizeUIntValue(floatQuantizer, value2);
	}

	public static void Read(this FloatQuantizerStruct floatQuantizer, IDataReader bitReaderStream, out float value)
	{
		value = DequantizeUIntValue(floatQuantizer, bitReaderStream.ReadBits(floatQuantizer.BitsRequired));
	}
}
