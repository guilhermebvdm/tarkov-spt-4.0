using Comfort.Common;
using UnityEngine;

public struct GStruct38
{
	public int hash;

	public AnimatorControllerParameterType type;

	public bool @bool;

	public int @int;

	public float @float;

	public int WriteToBuffer(byte[] buffer, int offset = 0)
	{
		int num = offset;
		offset += buffer.Write(hash, offset);
		offset += buffer.Write((int)type, offset);
		offset = ((type == AnimatorControllerParameterType.Bool || type == AnimatorControllerParameterType.Trigger) ? (offset + buffer.Write((byte)(@bool ? 1 : 0), offset)) : ((type != AnimatorControllerParameterType.Int) ? (offset + buffer.Write(@float, offset)) : (offset + buffer.Write(@int, offset))));
		return offset - num;
	}

	public void ReadFromBuffer(byte[] buffer, ref int offset)
	{
		hash = buffer.ReadInt32(ref offset);
		type = (AnimatorControllerParameterType)buffer.ReadInt32(ref offset);
		if (type != AnimatorControllerParameterType.Bool && type != AnimatorControllerParameterType.Trigger)
		{
			if (type == AnimatorControllerParameterType.Int)
			{
				@int = buffer.ReadInt32(ref offset);
			}
			else
			{
				@float = buffer.ReadSingle(ref offset);
			}
		}
		else
		{
			@bool = buffer.ReadByte(ref offset) == 1;
		}
	}

	public static GStruct38[] GetAnimatorParams(IAnimator animator)
	{
		int parameterCount = animator.parameterCount;
		GStruct38[] array = new GStruct38[parameterCount];
		for (int i = 0; i < parameterCount; i++)
		{
			array[i].method_0(animator, i);
		}
		return array;
	}

	public void method_0(IAnimator animator, int paramIndex)
	{
		AnimatorParameterInfo parameter = animator.GetParameter(paramIndex);
		hash = parameter.nameHash;
		type = parameter.type;
		if (type != AnimatorControllerParameterType.Bool && type != AnimatorControllerParameterType.Trigger)
		{
			if (type == AnimatorControllerParameterType.Int)
			{
				@int = animator.GetInteger(hash);
			}
			else
			{
				@float = animator.GetFloat(hash);
			}
		}
		else
		{
			@bool = animator.GetBool(hash);
		}
	}

	public void Apply(IAnimator animator)
	{
		if (type == AnimatorControllerParameterType.Bool)
		{
			animator.SetBool(hash, @bool);
		}
		else if (type == AnimatorControllerParameterType.Trigger)
		{
			if (@bool)
			{
				animator.SetTrigger(hash);
			}
		}
		else if (type == AnimatorControllerParameterType.Int)
		{
			animator.SetInteger(hash, @int);
		}
		else
		{
			animator.SetFloat(hash, @float);
		}
	}
}
