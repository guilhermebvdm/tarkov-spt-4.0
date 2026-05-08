using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public abstract class GClass2076
{
	[NonSerialized]
	public static Dictionary<byte, Vector2> Dictionary_0 = new Dictionary<byte, Vector2>
	{
		{
			0,
			new Vector2(0f, 0f)
		},
		{
			1,
			new Vector2(1f, 0f)
		},
		{
			2,
			new Vector2(1f, 1f)
		},
		{
			3,
			new Vector2(0f, 1f)
		},
		{
			4,
			new Vector2(-1f, 1f)
		},
		{
			5,
			new Vector2(-1f, 0f)
		},
		{
			6,
			new Vector2(-1f, -1f)
		},
		{
			7,
			new Vector2(0f, -1f)
		},
		{
			8,
			new Vector2(1f, -1f)
		}
	};

	[NonSerialized]
	public static Dictionary<EMovementDirection, float> Dictionary_1 = new Dictionary<EMovementDirection, float>(GClass866<EMovementDirection>.Count, GClass866<EMovementDirection>.EqualityComparer)
	{
		{
			EMovementDirection.None,
			-1f
		},
		{
			EMovementDirection.Right,
			2f
		},
		{
			EMovementDirection.FrontRight,
			1.5f
		},
		{
			EMovementDirection.Front,
			1f
		},
		{
			EMovementDirection.FrontLeft,
			0.5f
		},
		{
			EMovementDirection.Left,
			0f
		},
		{
			EMovementDirection.BackLeft,
			3.5f
		},
		{
			EMovementDirection.Back,
			3f
		},
		{
			EMovementDirection.BackRight,
			2.5f
		}
	};

	public static EMovementDirection ConvertToMovementDirection(Vector2 movementDirection)
	{
		float x = movementDirection.x;
		float y = movementDirection.y;
		if (Math.Abs(x) < float.Epsilon)
		{
			if (y >= float.Epsilon)
			{
				return EMovementDirection.Front;
			}
			if (y <= -1E-45f)
			{
				return EMovementDirection.Back;
			}
			return EMovementDirection.None;
		}
		float num = Math.Abs(y / x);
		if (num > 2f)
		{
			if (!(y > 0f))
			{
				return EMovementDirection.Back;
			}
			return EMovementDirection.Front;
		}
		if (num < 0.5f)
		{
			if (!(x > 0f))
			{
				return EMovementDirection.Left;
			}
			return EMovementDirection.Right;
		}
		if (x > 0f)
		{
			if (!(y > 0f))
			{
				return EMovementDirection.BackRight;
			}
			return EMovementDirection.FrontRight;
		}
		if (!(y > 0f))
		{
			return EMovementDirection.BackLeft;
		}
		return EMovementDirection.FrontLeft;
	}

	public static float InvertDirection(this EMovementDirection direction)
	{
		return Dictionary_1[direction];
	}

	public static Vector2 ToVector2(this EMovementDirection direction)
	{
		return Dictionary_0[(byte)direction];
	}

	public static bool IsDiagonal(this EMovementDirection direction)
	{
		if (direction != EMovementDirection.BackLeft && direction != EMovementDirection.BackRight && direction != EMovementDirection.FrontLeft)
		{
			return direction == EMovementDirection.FrontRight;
		}
		return true;
	}
}
