using System;
using UnityEngine;

public struct GStruct32
{
	public readonly Vector3 Center;

	public readonly Vector3 Right;

	public readonly Vector3 Up;

	public readonly Vector3 Forward;

	public readonly float SizeDistance;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public Vector3 Vector3_1;

	[NonSerialized]
	public Vector3 Vector3_2;

	[NonSerialized]
	public Vector3 Vector3_3;

	[NonSerialized]
	public Vector3 Vector3_4;

	[NonSerialized]
	public Vector3 Vector3_5;

	[NonSerialized]
	public Vector3 Vector3_6;

	[NonSerialized]
	public Vector3 Vector3_7;

	public GStruct32(Vector3 center, Vector3 size, Quaternion orientation)
	{
		Center = center;
		SizeDistance = size.magnitude;
		Vector3 vector = size / 2f;
		Vector3 vector2 = -vector;
		Vector3_0 = center + orientation * vector2;
		Vector3_1 = center + orientation * new Vector3(vector.x, vector2.y, vector2.z);
		Vector3_2 = center + orientation * new Vector3(vector2.x, vector.y, vector2.z);
		Vector3_3 = center + orientation * new Vector3(vector.x, vector.y, vector2.z);
		Vector3_4 = center + orientation * new Vector3(vector2.x, vector2.y, vector.z);
		Vector3_5 = center + orientation * new Vector3(vector.x, vector2.y, vector.z);
		Vector3_6 = center + orientation * new Vector3(vector2.x, vector.y, vector.z);
		Vector3_7 = center + orientation * vector;
		Right = orientation * Vector3.right;
		Up = orientation * Vector3.up;
		Forward = orientation * Vector3.forward;
	}

	public Vector3 GetVertex(int i)
	{
		return i switch
		{
			0 => Vector3_0, 
			1 => Vector3_1, 
			2 => Vector3_2, 
			3 => Vector3_3, 
			4 => Vector3_4, 
			5 => Vector3_5, 
			6 => Vector3_6, 
			7 => Vector3_7, 
			_ => throw new ArgumentException("Index must be in range [0..7]"), 
		};
	}
}
