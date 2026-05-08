using System;
using System.IO;
using UnityEngine;

namespace Koenigz.PerfectCulling.EFT;

[Serializable]
public struct OrientedBounds : IBinarySerializable
{
	public Bounds bounds;

	public Quaternion rotation;

	public Bounds AABB
	{
		get
		{
			Bounds result = new Bounds
			{
				center = bounds.center
			};
			Vector3 extents = bounds.extents;
			Vector3 vector = rotation * new Vector3(extents.x, extents.y, extents.z);
			result.Encapsulate(vector + bounds.center);
			result.Encapsulate(-vector + bounds.center);
			vector = rotation * new Vector3(extents.x, extents.y, 0f - extents.z);
			result.Encapsulate(vector + bounds.center);
			result.Encapsulate(-vector + bounds.center);
			vector = rotation * new Vector3(extents.x, 0f - extents.y, extents.z);
			result.Encapsulate(vector + bounds.center);
			result.Encapsulate(-vector + bounds.center);
			vector = rotation * new Vector3(extents.x, 0f - extents.y, 0f - extents.z);
			result.Encapsulate(vector + bounds.center);
			result.Encapsulate(-vector + bounds.center);
			return result;
		}
	}

	public Vector3 ClosestPointOriented(Vector3 p, out bool isPointInside)
	{
		Vector3 vector = p - this.bounds.center;
		vector = Quaternion.Inverse(rotation) * vector;
		Bounds bounds = new Bounds(Vector3.zero, this.bounds.size);
		Vector3 vector2 = bounds.ClosestPoint(vector);
		vector2 = rotation * vector2 + this.bounds.center;
		isPointInside = bounds.Contains(vector);
		return vector2;
	}

	public bool Contains(Vector3 worldPosition)
	{
		return bounds.Contains(Quaternion.Inverse(rotation) * (worldPosition - bounds.center));
	}

	public void Write(BinaryWriter writer)
	{
		GClass881.Write(writer, bounds.center);
		GClass881.Write(writer, bounds.size);
		GClass881.Write(writer, rotation);
	}

	public void Read(BinaryReader reader)
	{
		bounds.center = GClass881.ReadVector3(reader);
		bounds.size = GClass881.ReadVector3(reader);
		rotation = GClass881.ReadQuaternion(reader);
	}

	public override string ToString()
	{
		return $"{bounds.center} | {bounds.size} | {rotation.ToString()}";
	}

	public float SqrDistance(Vector3 point)
	{
		Vector3 point2 = Quaternion.Inverse(rotation) * (point - bounds.center) + bounds.center;
		return bounds.SqrDistance(point2);
	}

	public float Distance(Vector3 point)
	{
		return Mathf.Sqrt(SqrDistance(point));
	}
}
