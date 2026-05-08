using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class GClass1457
{
	public struct GStruct143
	{
		[NonSerialized]
		[CompilerGenerated]
		public Vector3 Vector3_0;

		[NonSerialized]
		[CompilerGenerated]
		public Vector3 Vector3_1;

		[NonSerialized]
		[CompilerGenerated]
		public Vector3 Vector3_2;

		[NonSerialized]
		[CompilerGenerated]
		public Vector3 Vector3_3;

		[NonSerialized]
		[CompilerGenerated]
		public Vector3 Vector3_4;

		public Vector3 localFrontTopLeft
		{
			[CompilerGenerated]
			readonly get
			{
				return Vector3_0;
			}
			[CompilerGenerated]
			set
			{
				Vector3_0 = value;
			}
		}

		public Vector3 localFrontTopRight
		{
			[CompilerGenerated]
			readonly get
			{
				return Vector3_1;
			}
			[CompilerGenerated]
			set
			{
				Vector3_1 = value;
			}
		}

		public Vector3 localFrontBottomLeft
		{
			[CompilerGenerated]
			readonly get
			{
				return Vector3_2;
			}
			[CompilerGenerated]
			set
			{
				Vector3_2 = value;
			}
		}

		public Vector3 localFrontBottomRight
		{
			[CompilerGenerated]
			readonly get
			{
				return Vector3_3;
			}
			[CompilerGenerated]
			set
			{
				Vector3_3 = value;
			}
		}

		public Vector3 localBackTopLeft => -localFrontBottomRight;

		public Vector3 localBackTopRight => -localFrontBottomLeft;

		public Vector3 localBackBottomLeft => -localFrontTopRight;

		public Vector3 localBackBottomRight => -localFrontTopLeft;

		public Vector3 frontTopLeft => localFrontTopLeft + origin;

		public Vector3 frontTopRight => localFrontTopRight + origin;

		public Vector3 frontBottomLeft => localFrontBottomLeft + origin;

		public Vector3 frontBottomRight => localFrontBottomRight + origin;

		public Vector3 backTopLeft => localBackTopLeft + origin;

		public Vector3 backTopRight => localBackTopRight + origin;

		public Vector3 backBottomLeft => localBackBottomLeft + origin;

		public Vector3 backBottomRight => localBackBottomRight + origin;

		public Vector3 origin
		{
			[CompilerGenerated]
			readonly get
			{
				return Vector3_4;
			}
			[CompilerGenerated]
			set
			{
				Vector3_4 = value;
			}
		}

		public GStruct143(Vector3 origin, Vector3 halfExtents, Quaternion orientation)
			: this(origin, halfExtents)
		{
			Rotate(orientation);
		}

		public GStruct143(Vector3 origin, Vector3 halfExtents)
		{
			this = default(GStruct143);
			localFrontTopLeft = new Vector3(0f - halfExtents.x, halfExtents.y, 0f - halfExtents.z);
			localFrontTopRight = new Vector3(halfExtents.x, halfExtents.y, 0f - halfExtents.z);
			localFrontBottomLeft = new Vector3(0f - halfExtents.x, 0f - halfExtents.y, 0f - halfExtents.z);
			localFrontBottomRight = new Vector3(halfExtents.x, 0f - halfExtents.y, 0f - halfExtents.z);
			this.origin = origin;
		}

		public void Rotate(Quaternion orientation)
		{
			localFrontTopLeft = smethod_1(localFrontTopLeft, Vector3.zero, orientation);
			localFrontTopRight = smethod_1(localFrontTopRight, Vector3.zero, orientation);
			localFrontBottomLeft = smethod_1(localFrontBottomLeft, Vector3.zero, orientation);
			localFrontBottomRight = smethod_1(localFrontBottomRight, Vector3.zero, orientation);
		}
	}

	public static void DrawBoxCastOnHit(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Vector3 direction, float hitInfoDistance, Color color)
	{
		origin = smethod_0(origin, direction, hitInfoDistance);
		DrawBox(origin, halfExtents, orientation, color);
	}

	public static void DrawBoxCastBox(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Vector3 direction, float distance, Color color)
	{
		direction.Normalize();
		GStruct143 box = new GStruct143(origin, halfExtents, orientation);
		GStruct143 box2 = new GStruct143(origin + direction * distance, halfExtents, orientation);
		Debug.DrawLine(box.backBottomLeft, box2.backBottomLeft, color);
		Debug.DrawLine(box.backBottomRight, box2.backBottomRight, color);
		Debug.DrawLine(box.backTopLeft, box2.backTopLeft, color);
		Debug.DrawLine(box.backTopRight, box2.backTopRight, color);
		Debug.DrawLine(box.frontTopLeft, box2.frontTopLeft, color);
		Debug.DrawLine(box.frontTopRight, box2.frontTopRight, color);
		Debug.DrawLine(box.frontBottomLeft, box2.frontBottomLeft, color);
		Debug.DrawLine(box.frontBottomRight, box2.frontBottomRight, color);
		DrawBox(box, color);
		DrawBox(box2, color);
	}

	public static void DrawBox(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Color color)
	{
		DrawBox(new GStruct143(origin, halfExtents, orientation), color);
	}

	public static void DrawBox(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Color color, float duration)
	{
		DrawBox(new GStruct143(origin, halfExtents, orientation), color, duration);
	}

	public static void DrawBox(GStruct143 box, Color color)
	{
		Debug.DrawLine(box.frontTopLeft, box.frontTopRight, color);
		Debug.DrawLine(box.frontTopRight, box.frontBottomRight, color);
		Debug.DrawLine(box.frontBottomRight, box.frontBottomLeft, color);
		Debug.DrawLine(box.frontBottomLeft, box.frontTopLeft, color);
		Debug.DrawLine(box.backTopLeft, box.backTopRight, color);
		Debug.DrawLine(box.backTopRight, box.backBottomRight, color);
		Debug.DrawLine(box.backBottomRight, box.backBottomLeft, color);
		Debug.DrawLine(box.backBottomLeft, box.backTopLeft, color);
		Debug.DrawLine(box.frontTopLeft, box.backTopLeft, color);
		Debug.DrawLine(box.frontTopRight, box.backTopRight, color);
		Debug.DrawLine(box.frontBottomRight, box.backBottomRight, color);
		Debug.DrawLine(box.frontBottomLeft, box.backBottomLeft, color);
	}

	public static void DrawBox(GStruct143 box, Color color, float time)
	{
		Debug.DrawLine(box.frontTopLeft, box.frontTopRight, color, time);
		Debug.DrawLine(box.frontTopRight, box.frontBottomRight, color, time);
		Debug.DrawLine(box.frontBottomRight, box.frontBottomLeft, color, time);
		Debug.DrawLine(box.frontBottomLeft, box.frontTopLeft, color, time);
		Debug.DrawLine(box.backTopLeft, box.backTopRight, color, time);
		Debug.DrawLine(box.backTopRight, box.backBottomRight, color, time);
		Debug.DrawLine(box.backBottomRight, box.backBottomLeft, color, time);
		Debug.DrawLine(box.backBottomLeft, box.backTopLeft, color, time);
		Debug.DrawLine(box.frontTopLeft, box.backTopLeft, color, time);
		Debug.DrawLine(box.frontTopRight, box.backTopRight, color, time);
		Debug.DrawLine(box.frontBottomRight, box.backBottomRight, color, time);
		Debug.DrawLine(box.frontBottomLeft, box.backBottomLeft, color, time);
	}

	public static Vector3 smethod_0(Vector3 origin, Vector3 direction, float hitInfoDistance)
	{
		return origin + direction.normalized * hitInfoDistance;
	}

	public static Vector3 smethod_1(Vector3 point, Vector3 pivot, Quaternion rotation)
	{
		Vector3 vector = point - pivot;
		return pivot + rotation * vector;
	}
}
