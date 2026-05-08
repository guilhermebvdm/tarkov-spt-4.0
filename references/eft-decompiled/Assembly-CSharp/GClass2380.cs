using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GClass2380
{
	public enum IntersectionType
	{
		Disjoint,
		Intersects,
		Contains
	}

	public struct GStruct287
	{
		public Vector3 BoxCenter;

		public Vector3 BoxExtents;

		public bool IsInFrustum;
	}

	public struct GStruct288
	{
		public Vector3 SphereCenter;

		public float SphereRadius;

		public bool IsInFrustum;
	}

	public const int PlaneCount = 6;

	public const int CornerCount = 8;

	[NonSerialized]
	[CompilerGenerated]
	public Vector3 Vector3_0;

	[NonSerialized]
	public Vector3[] Vector3_1 = new Vector3[8];

	[NonSerialized]
	public Plane[] Plane_0 = new Plane[6];

	[NonSerialized]
	public Vector3[] Vector3_2 = new Vector3[6];

	[NonSerialized]
	public Vector3[] Vector3_3 = new Vector3[6];

	[NonSerialized]
	public float[] Float_0 = new float[6];

	public Vector3 Position
	{
		[CompilerGenerated]
		get
		{
			return Vector3_0;
		}
		[CompilerGenerated]
		set
		{
			Vector3_0 = value;
		}
	}

	public void CopyTo(GClass2380 other)
	{
		if (other != null)
		{
			Array.Copy(Vector3_1, other.Vector3_1, 8);
			Array.Copy(Plane_0, other.Plane_0, 6);
			Array.Copy(Vector3_2, other.Vector3_2, 6);
			Array.Copy(Vector3_3, other.Vector3_3, 6);
			Array.Copy(Float_0, other.Float_0, 6);
			other.Position = Position;
		}
		else
		{
			Debug.LogError("NULL!");
		}
	}

	public void GetCorners(Vector3[] outCorners)
	{
		if (outCorners == null || outCorners.Length < 8)
		{
			throw new InvalidOperationException("Destination array is null or too small");
		}
		Array.Copy(Vector3_1, outCorners, 8);
	}

	public void GetPlanes(Plane[] outPlanes)
	{
		if (outPlanes == null || outPlanes.Length < 6)
		{
			throw new InvalidOperationException("Destination array is null or too small");
		}
		Array.Copy(Plane_0, outPlanes, 6);
	}

	public void Update(Camera camera)
	{
		Update(camera, camera.farClipPlane);
	}

	public void Update(Camera camera, float farClipPlane)
	{
		Transform transform = camera.transform;
		Vector3 position = transform.position;
		Quaternion orientation = transform.rotation;
		Position = position;
		Vector3 vector = orientation * Vector3.forward;
		if (camera.orthographic)
		{
			method_0(camera);
		}
		else
		{
			method_2(ref position, ref orientation, camera.fieldOfView, camera.nearClipPlane, camera.farClipPlane, camera.aspect);
		}
		Plane_0[0] = new Plane(Vector3_1[4], Vector3_1[1], Vector3_1[0]);
		Plane_0[1] = new Plane(Vector3_1[6], Vector3_1[3], Vector3_1[2]);
		Plane_0[2] = new Plane(Vector3_1[7], Vector3_1[0], Vector3_1[3]);
		Plane_0[3] = new Plane(Vector3_1[5], Vector3_1[2], Vector3_1[1]);
		Plane_0[4] = new Plane(vector, position + vector * camera.nearClipPlane);
		Plane_0[5] = new Plane(-vector, position + vector * farClipPlane);
		for (int i = 0; i < 6; i++)
		{
			Plane plane = Plane_0[i];
			Vector3 normal = plane.normal;
			Vector3_2[i] = new Vector3(Mathf.Abs(normal.x), Mathf.Abs(normal.y), Mathf.Abs(normal.z));
			Vector3_3[i] = normal;
			Float_0[i] = plane.distance;
		}
	}

	public void UpdateEx(Vector3 position, Quaternion orientation, bool orthographic, float farClipPlane, float nearClipPlane, float fieldOfView, float aspect, float orthographicSize)
	{
		Position = position;
		Vector3 vector = orientation * Vector3.forward;
		if (orthographic)
		{
			method_1(position, orientation, farClipPlane, nearClipPlane, orthographicSize, aspect);
		}
		else
		{
			method_2(ref position, ref orientation, fieldOfView, nearClipPlane, farClipPlane, aspect);
		}
		Plane_0[0] = new Plane(Vector3_1[4], Vector3_1[1], Vector3_1[0]);
		Plane_0[1] = new Plane(Vector3_1[6], Vector3_1[3], Vector3_1[2]);
		Plane_0[2] = new Plane(Vector3_1[7], Vector3_1[0], Vector3_1[3]);
		Plane_0[3] = new Plane(Vector3_1[5], Vector3_1[2], Vector3_1[1]);
		Plane_0[4] = new Plane(vector, position + vector * nearClipPlane);
		Plane_0[5] = new Plane(-vector, position + vector * farClipPlane);
		for (int i = 0; i < 6; i++)
		{
			Plane plane = Plane_0[i];
			Vector3 normal = plane.normal;
			Vector3_2[i] = new Vector3(Mathf.Abs(normal.x), Mathf.Abs(normal.y), Mathf.Abs(normal.z));
			Vector3_3[i] = normal;
			Float_0[i] = plane.distance;
		}
	}

	public void Update(Vector3 position, Quaternion orientation, float fov, float nearClipPlane, float farClipPlane, float aspect)
	{
		Position = position;
		method_2(ref position, ref orientation, fov, nearClipPlane, farClipPlane, aspect);
		Vector3 vector = orientation * Vector3.forward;
		Plane_0[0] = new Plane(Vector3_1[4], Vector3_1[1], Vector3_1[0]);
		Plane_0[1] = new Plane(Vector3_1[6], Vector3_1[3], Vector3_1[2]);
		Plane_0[2] = new Plane(Vector3_1[7], Vector3_1[0], Vector3_1[3]);
		Plane_0[3] = new Plane(Vector3_1[5], Vector3_1[2], Vector3_1[1]);
		Plane_0[4] = new Plane(vector, position + vector * nearClipPlane);
		Plane_0[5] = new Plane(-vector, position + vector * farClipPlane);
		for (int i = 0; i < 6; i++)
		{
			Plane plane = Plane_0[i];
			Vector3 normal = plane.normal;
			Vector3_2[i] = new Vector3(Mathf.Abs(normal.x), Mathf.Abs(normal.y), Mathf.Abs(normal.z));
			Vector3_3[i] = normal;
			Float_0[i] = plane.distance;
		}
	}

	public bool Contains(ref Vector3 point)
	{
		int num = 0;
		while (true)
		{
			if (num < 6)
			{
				Vector3 vector = Vector3_3[num];
				float num2 = Float_0[num];
				if (!(vector.x * point.x + vector.y * point.y + vector.z * point.z + num2 >= 0f))
				{
					break;
				}
				num++;
				continue;
			}
			return true;
		}
		return false;
	}

	public IntersectionType GetSphereIntersection(ref Vector3 center, float radius, float frustumPadding = 0f)
	{
		bool flag = false;
		int num = 0;
		while (true)
		{
			if (num < 6)
			{
				Vector3 vector = Vector3_3[num];
				float num2 = Float_0[num];
				float num3 = vector.x * center.x + vector.y * center.y + vector.z * center.z + num2;
				if (!(num3 >= 0f - radius - frustumPadding))
				{
					break;
				}
				flag = flag || num3 <= radius;
				num++;
				continue;
			}
			if (!flag)
			{
				return IntersectionType.Contains;
			}
			return IntersectionType.Intersects;
		}
		return IntersectionType.Disjoint;
	}

	public IntersectionType GetSphereIntersection(ref BoundingSphere sphere, float frustumPadding = 0f)
	{
		bool flag = false;
		Vector3 position = sphere.position;
		float radius = sphere.radius;
		int num = 0;
		while (true)
		{
			if (num < 6)
			{
				Vector3 vector = Vector3_3[num];
				float num2 = Float_0[num];
				float num3 = vector.x * position.x + vector.y * position.y + vector.z * position.z + num2;
				if (!(num3 >= 0f - radius - frustumPadding))
				{
					break;
				}
				flag = flag || num3 <= radius;
				num++;
				continue;
			}
			if (!flag)
			{
				return IntersectionType.Contains;
			}
			return IntersectionType.Intersects;
		}
		return IntersectionType.Disjoint;
	}

	public void CullSpheres(GStruct288[] spheres, int sphereCount)
	{
		_ = Vector3.zero;
		Vector3 vector = Vector3_3[0];
		Vector3 vector2 = Vector3_3[1];
		Vector3 vector3 = Vector3_3[2];
		Vector3 vector4 = Vector3_3[3];
		Vector3 vector5 = Vector3_3[4];
		Vector3 vector6 = Vector3_3[5];
		float num = Float_0[0];
		float num2 = Float_0[1];
		float num3 = Float_0[2];
		float num4 = Float_0[3];
		float num5 = Float_0[4];
		float num6 = Float_0[5];
		for (int i = 0; i < sphereCount; i++)
		{
			GStruct288 gStruct = spheres[i];
			Vector3 sphereCenter = gStruct.SphereCenter;
			float sphereRadius = gStruct.SphereRadius;
			bool flag = false;
			flag = (flag = (flag = (flag = (flag = (flag = vector.x * sphereCenter.x + vector.y * sphereCenter.y + vector.z * sphereCenter.z + num < 0f - sphereRadius) || vector2.x * sphereCenter.x + vector2.y * sphereCenter.y + vector2.z * sphereCenter.z + num2 < 0f - sphereRadius) || vector3.x * sphereCenter.x + vector3.y * sphereCenter.y + vector3.z * sphereCenter.z + num3 < 0f - sphereRadius) || vector4.x * sphereCenter.x + vector4.y * sphereCenter.y + vector4.z * sphereCenter.z + num4 < 0f - sphereRadius) || vector5.x * sphereCenter.x + vector5.y * sphereCenter.y + vector5.z * sphereCenter.z + num5 < 0f - sphereRadius) || vector6.x * sphereCenter.x + vector6.y * sphereCenter.y + vector6.z * sphereCenter.z + num6 < 0f - sphereRadius;
			spheres[i].IsInFrustum = !flag;
		}
	}

	public bool IntersectsSphere(ref Vector3 center, float radius, float frustumPadding = 0f)
	{
		int num = 0;
		while (true)
		{
			if (num < 6)
			{
				Vector3 vector = Vector3_3[num];
				float num2 = Float_0[num];
				if (!(vector.x * center.x + vector.y * center.y + vector.z * center.z + num2 >= 0f - radius - frustumPadding))
				{
					break;
				}
				num++;
				continue;
			}
			return true;
		}
		return false;
	}

	public bool IntersectsSphere(ref BoundingSphere sphere, float frustumPadding = 0f)
	{
		Vector3 position = sphere.position;
		int num = 0;
		while (true)
		{
			if (num < 6)
			{
				Vector3 vector = Vector3_3[num];
				float num2 = Float_0[num];
				if (!(vector.x * position.x + vector.y * position.y + vector.z * position.z + num2 >= 0f - sphere.radius - frustumPadding))
				{
					break;
				}
				num++;
				continue;
			}
			return true;
		}
		return false;
	}

	public bool IntersectsBox(ref Bounds box, float frustumPadding = 0f)
	{
		if (box.Contains(Vector3_1[7]))
		{
			return true;
		}
		Vector3 center = box.center;
		Vector3 extents = box.extents;
		int num = 0;
		while (true)
		{
			if (num < 6)
			{
				Vector3 vector = Vector3_2[num];
				Vector3 vector2 = Vector3_3[num];
				float num2 = Float_0[num];
				float num3 = extents.x * vector.x + extents.y * vector.y + extents.z * vector.z;
				if (!(vector2.x * center.x + vector2.y * center.y + vector2.z * center.z + num3 >= 0f - num2 - frustumPadding))
				{
					break;
				}
				num++;
				continue;
			}
			return true;
		}
		return false;
	}

	public IntersectionType GetBoxIntersection(ref Bounds box, float frustumPadding = 0f)
	{
		Vector3 center = box.center;
		Vector3 extents = box.extents;
		bool flag = false;
		int num = 0;
		while (true)
		{
			if (num < 6)
			{
				Vector3 vector = Vector3_2[num];
				Vector3 vector2 = Vector3_3[num];
				float num2 = Float_0[num];
				float num3 = extents.x * vector.x + extents.y * vector.y + extents.z * vector.z;
				float num4 = vector2.x * center.x + vector2.y * center.y + vector2.z * center.z;
				if (!(num4 + num3 >= 0f - num2 - frustumPadding))
				{
					break;
				}
				flag = flag || num4 - num3 <= 0f - num2;
				num++;
				continue;
			}
			if (!flag)
			{
				return IntersectionType.Contains;
			}
			return IntersectionType.Intersects;
		}
		return IntersectionType.Disjoint;
	}

	public void CullBoxes(GStruct287[] boxes, int boxCount)
	{
		Vector3 vector = Vector3_2[0];
		Vector3 vector2 = Vector3_2[1];
		Vector3 vector3 = Vector3_2[2];
		Vector3 vector4 = Vector3_2[3];
		Vector3 vector5 = Vector3_2[4];
		Vector3 vector6 = Vector3_2[5];
		Vector3 vector7 = Vector3_3[0];
		Vector3 vector8 = Vector3_3[1];
		Vector3 vector9 = Vector3_3[2];
		Vector3 vector10 = Vector3_3[3];
		Vector3 vector11 = Vector3_3[4];
		Vector3 vector12 = Vector3_3[5];
		float num = Float_0[0];
		float num2 = Float_0[1];
		float num3 = Float_0[2];
		float num4 = Float_0[3];
		float num5 = Float_0[4];
		float num6 = Float_0[5];
		for (int i = 0; i < boxCount; i++)
		{
			GStruct287 gStruct = boxes[i];
			Vector3 boxCenter = gStruct.BoxCenter;
			Vector3 boxExtents = gStruct.BoxExtents;
			bool flag = false;
			flag = (flag = (flag = (flag = (flag = (flag = boxExtents.x * vector.x + boxExtents.y * vector.y + boxExtents.z * vector.z + (vector7.x * boxCenter.x + vector7.y * boxCenter.y + vector7.z * boxCenter.z) < 0f - num) || boxExtents.x * vector2.x + boxExtents.y * vector2.y + boxExtents.z * vector2.z + (vector8.x * boxCenter.x + vector8.y * boxCenter.y + vector8.z * boxCenter.z) < 0f - num2) || boxExtents.x * vector3.x + boxExtents.y * vector3.y + boxExtents.z * vector3.z + (vector9.x * boxCenter.x + vector9.y * boxCenter.y + vector9.z * boxCenter.z) < 0f - num3) || boxExtents.x * vector4.x + boxExtents.y * vector4.y + boxExtents.z * vector4.z + (vector10.x * boxCenter.x + vector10.y * boxCenter.y + vector10.z * boxCenter.z) < 0f - num4) || boxExtents.x * vector5.x + boxExtents.y * vector5.y + boxExtents.z * vector5.z + (vector11.x * boxCenter.x + vector11.y * boxCenter.y + vector11.z * boxCenter.z) < 0f - num5) || boxExtents.x * vector6.x + boxExtents.y * vector6.y + boxExtents.z * vector6.z + (vector12.x * boxCenter.x + vector12.y * boxCenter.y + vector12.z * boxCenter.z) < 0f - num6;
			boxes[i].IsInFrustum = !flag;
		}
	}

	public bool IntersectsOrientedBox(ref Bounds box, ref Vector3 right, ref Vector3 up, ref Vector3 forward, float frustumPadding = 0f)
	{
		Vector3 center = box.center;
		Vector3 extents = box.extents;
		int num = 0;
		while (true)
		{
			if (num < 6)
			{
				Vector3 lhs = Vector3_3[num];
				float num2 = Float_0[num];
				float num3 = extents.x * Mathf.Abs(Vector3.Dot(lhs, right)) + extents.y * Mathf.Abs(Vector3.Dot(lhs, up)) + extents.z * Mathf.Abs(Vector3.Dot(lhs, forward));
				if (!(lhs.x * center.x + lhs.y * center.y + lhs.z * center.z + num3 >= 0f - num2 - frustumPadding))
				{
					break;
				}
				num++;
				continue;
			}
			return true;
		}
		return false;
	}

	public IntersectionType GetOrientedBoxIntersection(ref Bounds box, ref Vector3 right, ref Vector3 up, ref Vector3 forward, float frustumPadding = 0f)
	{
		Vector3 center = box.center;
		Vector3 extents = box.extents;
		bool flag = false;
		int num = 0;
		while (true)
		{
			if (num < 6)
			{
				Vector3 lhs = Vector3_3[num];
				float num2 = Float_0[num];
				float num3 = extents.x * Mathf.Abs(Vector3.Dot(lhs, right)) + extents.y * Mathf.Abs(Vector3.Dot(lhs, up)) + extents.z * Mathf.Abs(Vector3.Dot(lhs, forward));
				float num4 = lhs.x * center.x + lhs.y * center.y + lhs.z * center.z;
				if (!(num4 + num3 >= 0f - num2 - frustumPadding))
				{
					break;
				}
				flag = flag || num4 - num3 <= 0f - num2;
				num++;
				continue;
			}
			if (!flag)
			{
				return IntersectionType.Contains;
			}
			return IntersectionType.Intersects;
		}
		return IntersectionType.Disjoint;
	}

	public void method_0(Camera camera)
	{
		Transform transform = camera.transform;
		Vector3 position = transform.position;
		Quaternion rotation = transform.rotation;
		float farClipPlane = camera.farClipPlane;
		float nearClipPlane = camera.nearClipPlane;
		Vector3 vector = rotation * Vector3.forward;
		Vector3 vector2 = rotation * Vector3.right * camera.orthographicSize * camera.aspect;
		Vector3 vector3 = rotation * Vector3.up * camera.orthographicSize;
		Vector3_1[0] = position + vector * farClipPlane - vector3 - vector2;
		Vector3_1[1] = position + vector * farClipPlane + vector3 - vector2;
		Vector3_1[2] = position + vector * farClipPlane + vector3 + vector2;
		Vector3_1[3] = position + vector * farClipPlane - vector3 + vector2;
		Vector3_1[4] = position + vector * nearClipPlane - vector3 - vector2;
		Vector3_1[5] = position + vector * nearClipPlane + vector3 - vector2;
		Vector3_1[6] = position + vector * nearClipPlane + vector3 + vector2;
		Vector3_1[7] = position + vector * nearClipPlane - vector3 + vector2;
	}

	public void method_1(Vector3 position, Quaternion orientation, float farClipPlane, float nearClipPlane, float orthographicSize, float aspect)
	{
		Vector3 vector = orientation * Vector3.forward;
		Vector3 vector2 = orientation * Vector3.right * orthographicSize * aspect;
		Vector3 vector3 = orientation * Vector3.up * orthographicSize;
		Vector3_1[0] = position + vector * farClipPlane - vector3 - vector2;
		Vector3_1[1] = position + vector * farClipPlane + vector3 - vector2;
		Vector3_1[2] = position + vector * farClipPlane + vector3 + vector2;
		Vector3_1[3] = position + vector * farClipPlane - vector3 + vector2;
		Vector3_1[4] = position + vector * nearClipPlane - vector3 - vector2;
		Vector3_1[5] = position + vector * nearClipPlane + vector3 - vector2;
		Vector3_1[6] = position + vector * nearClipPlane + vector3 + vector2;
		Vector3_1[7] = position + vector * nearClipPlane - vector3 + vector2;
	}

	public float GetNearHeight()
	{
		Vector3 vector = Vector3_1[4];
		Vector3 vector2 = Vector3_1[5] - vector;
		if (Vector3.Dot(vector2, vector2) > float.Epsilon)
		{
			return vector2.magnitude;
		}
		return 0f;
	}

	public void method_2(ref Vector3 position, ref Quaternion orientation, float fov, float nearClipPlane, float farClipPlane, float aspect)
	{
		float num = fov * 0.5f;
		Vector3 vector = Vector3.right * Mathf.Tan(num * (MathF.PI / 180f)) * aspect;
		Vector3 vector2 = Vector3.up * Mathf.Tan(num * (MathF.PI / 180f));
		Vector3 forward = Vector3.forward;
		Vector3 vector3 = forward - vector + vector2;
		float num2 = vector3.magnitude * farClipPlane;
		vector3.Normalize();
		vector3 *= num2;
		Vector3 vector4 = forward + vector + vector2;
		vector4.Normalize();
		vector4 *= num2;
		Vector3 vector5 = forward + vector - vector2;
		vector5.Normalize();
		vector5 *= num2;
		Vector3 vector6 = forward - vector - vector2;
		vector6.Normalize();
		vector6 *= num2;
		Vector3_1[0] = position + orientation * vector6;
		Vector3_1[1] = position + orientation * vector3;
		Vector3_1[2] = position + orientation * vector4;
		Vector3_1[3] = position + orientation * vector5;
		vector3 = forward - vector + vector2;
		num2 = vector3.magnitude * nearClipPlane;
		vector3.Normalize();
		vector3 *= num2;
		vector4 = forward + vector + vector2;
		vector4.Normalize();
		vector4 *= num2;
		vector5 = forward + vector - vector2;
		vector5.Normalize();
		vector5 *= num2;
		vector6 = forward - vector - vector2;
		vector6.Normalize();
		vector6 *= num2;
		Vector3_1[4] = position + orientation * vector6;
		Vector3_1[5] = position + orientation * vector3;
		Vector3_1[6] = position + orientation * vector4;
		Vector3_1[7] = position + orientation * vector5;
	}
}
