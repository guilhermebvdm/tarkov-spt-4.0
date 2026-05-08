using System;
using UnityEngine;

public class GClass1375
{
	[NonSerialized]
	public FloatQuantizerStruct FloatQuantizerStruct;

	[NonSerialized]
	public FloatQuantizerStruct FloatQuantizerStruct_1;

	public GClass1375(float xMin, float xMax, float xResolution, float yMin, float yMax, float yResolution, bool checkBounds = false)
	{
		FloatQuantizerStruct = new FloatQuantizerStruct(xMin, xMax, xResolution, checkBounds);
		FloatQuantizerStruct_1 = new FloatQuantizerStruct(yMin, yMax, yResolution, checkBounds);
	}

	public Vector2 Write(GInterface131 bitWriterStream, Vector2 value)
	{
		return new Vector2
		{
			x = GClass1370.Write(FloatQuantizerStruct, bitWriterStream, value.x),
			y = GClass1370.Write(FloatQuantizerStruct_1, bitWriterStream, value.y)
		};
	}

	public void Read(IDataReader bitReaderStream, out Vector2 value)
	{
		GClass1370.Read(FloatQuantizerStruct, bitReaderStream, out value.x);
		GClass1370.Read(FloatQuantizerStruct_1, bitReaderStream, out value.y);
	}
}
