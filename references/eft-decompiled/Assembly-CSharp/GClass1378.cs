using System;
using UnityEngine;

public class GClass1378
{
	[NonSerialized]
	public FloatQuantizerStruct FloatQuantizerStruct;

	[NonSerialized]
	public FloatQuantizerStruct FloatQuantizerStruct_1;

	[NonSerialized]
	public FloatQuantizerStruct FloatQuantizerStruct_2;

	public GClass1378(float xMin, float xMax, float xResolution, float yMin, float yMax, float yResolution, float zMin, float zMax, float zResolution, bool checkBounds = false)
	{
		FloatQuantizerStruct = new FloatQuantizerStruct(xMin, xMax, xResolution, checkBounds);
		FloatQuantizerStruct_1 = new FloatQuantizerStruct(yMin, yMax, yResolution, checkBounds);
		FloatQuantizerStruct_2 = new FloatQuantizerStruct(zMin, zMax, zResolution, checkBounds);
	}

	public Vector3 Write(GInterface131 bitWriterStream, Vector3 value)
	{
		return new Vector3
		{
			x = GClass1370.Write(FloatQuantizerStruct, bitWriterStream, value.x),
			y = GClass1370.Write(FloatQuantizerStruct_1, bitWriterStream, value.y),
			z = GClass1370.Write(FloatQuantizerStruct_2, bitWriterStream, value.z)
		};
	}

	public void Read(IDataReader bitReaderStream, out Vector3 value)
	{
		GClass1370.Read(FloatQuantizerStruct, bitReaderStream, out value.x);
		GClass1370.Read(FloatQuantizerStruct_1, bitReaderStream, out value.y);
		GClass1370.Read(FloatQuantizerStruct_2, bitReaderStream, out value.z);
	}

	public override string ToString()
	{
		return string.Format("{0}: {1}, {2}: {3}, {4}: {5}", "_xFloatQuantizer", FloatQuantizerStruct, "_yFloatQuantizer", FloatQuantizerStruct_1, "_zFloatQuantizer", FloatQuantizerStruct_2);
	}

	public Bounds GetBounds()
	{
		return new Bounds
		{
			min = new Vector3(FloatQuantizerStruct.Min, FloatQuantizerStruct_1.Min, FloatQuantizerStruct_2.Min),
			max = new Vector3(FloatQuantizerStruct.Max, FloatQuantizerStruct_1.Max, FloatQuantizerStruct_2.Max)
		};
	}
}
