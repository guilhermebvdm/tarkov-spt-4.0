using UnityEngine;

public class GClass1044
{
	public enum ActionState
	{
		Idle,
		Fire,
		Reload,
		GetPistol,
		GetRifle,
		Misfire
	}

	public enum PoseState
	{
		Stay,
		Sit,
		Lay,
		Jump
	}

	public enum SpeedState
	{
		Walk,
		Run,
		Sprint
	}

	public ActionState Action;

	public bool Aim;

	public int EquipedItem;

	public int Ammo;

	public bool Forvard;

	public bool Backward;

	public bool Right;

	public bool Left;

	public PoseState Pose;

	public SpeedState Speed;

	public bool IsHit;

	public void ReadState(int state)
	{
		method_0(write: false, ref state);
	}

	public int WriteState()
	{
		int state = 0;
		method_0(write: true, ref state);
		return state;
	}

	public void method_0(bool write, ref int state)
	{
		int position = 0;
		int value = (int)Action;
		int value2 = (int)Pose;
		int value3 = (int)Speed;
		Ammo = Mathf.Clamp(Ammo, 0, 255);
		smethod_1(write, ref state, ref position, ref value, 2);
		smethod_0(write, ref state, ref position, ref Aim);
		smethod_1(write, ref state, ref position, ref EquipedItem, 4);
		smethod_1(write, ref state, ref position, ref Ammo, 8);
		smethod_0(write, ref state, ref position, ref Forvard);
		smethod_0(write, ref state, ref position, ref Backward);
		smethod_0(write, ref state, ref position, ref Right);
		smethod_0(write, ref state, ref position, ref Left);
		smethod_1(write, ref state, ref position, ref value2, 2);
		smethod_1(write, ref state, ref position, ref value3, 2);
		smethod_0(write, ref state, ref position, ref IsHit);
		Action = (ActionState)value;
		Pose = (PoseState)value2;
		Speed = (SpeedState)value3;
	}

	public static void smethod_0(bool write, ref int state, ref int position, ref bool value)
	{
		if (write)
		{
			if (value)
			{
				state |= 1 << position;
			}
		}
		else
		{
			value = (state & (1 << position)) != 0;
		}
		position++;
	}

	public static void smethod_1(bool write, ref int state, ref int position, ref int value, int length)
	{
		int num = (1 << length) - 1;
		if (write)
		{
			state |= (value & num) << position;
		}
		else
		{
			int num2 = state >> position;
			value = num2 & num;
		}
		position += length;
	}
}
