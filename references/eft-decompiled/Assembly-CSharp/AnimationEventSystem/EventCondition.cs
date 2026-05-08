using System;
using System.IO;

namespace AnimationEventSystem;

[Serializable]
public class EventCondition : ICloneable
{
	public delegate bool Delegate4(EventCondition condition, IAnimator animator);

	public enum EConditionType
	{
		None = -1,
		IntEqual,
		IntNotEqual,
		IntGreaterThan,
		IntLessThan,
		IntGreaterEqualThan,
		IntLessEqualThan,
		FloatGreaterThan,
		FloatLessThan,
		BoolEqual,
		EConditionTypeEnumsCount
	}

	[NonSerialized]
	public static Delegate4[] CONDITION_DELEGATES;

	public bool BoolValue;

	public float FloatValue;

	public int IntValue;

	public string ParameterName;

	[NonSerialized]
	public int CachedNameHash;

	[NonSerialized]
	public EConditionType ConditionMode_1 = EConditionType.None;

	public EEventConditionParamTypes ConditionParamType;

	public EEventConditionModes ConditionMode;

	static EventCondition()
	{
		CONDITION_DELEGATES = new Delegate4[9];
		CONDITION_DELEGATES[0] = smethod_8;
		CONDITION_DELEGATES[1] = smethod_7;
		CONDITION_DELEGATES[2] = smethod_6;
		CONDITION_DELEGATES[3] = smethod_5;
		CONDITION_DELEGATES[4] = smethod_4;
		CONDITION_DELEGATES[5] = smethod_3;
		CONDITION_DELEGATES[6] = smethod_2;
		CONDITION_DELEGATES[7] = smethod_1;
		CONDITION_DELEGATES[8] = smethod_0;
	}

	public static bool smethod_0(EventCondition condition, IAnimator animator)
	{
		return animator.GetBool(condition.CachedNameHash) == condition.BoolValue;
	}

	public static bool smethod_1(EventCondition condition, IAnimator animator)
	{
		return animator.GetFloat(condition.CachedNameHash) < condition.FloatValue;
	}

	public static bool smethod_2(EventCondition condition, IAnimator animator)
	{
		return animator.GetFloat(condition.CachedNameHash) > condition.FloatValue;
	}

	public static bool smethod_3(EventCondition condition, IAnimator animator)
	{
		return animator.GetInteger(condition.CachedNameHash) <= condition.IntValue;
	}

	public static bool smethod_4(EventCondition condition, IAnimator animator)
	{
		return animator.GetInteger(condition.CachedNameHash) >= condition.IntValue;
	}

	public static bool smethod_5(EventCondition condition, IAnimator animator)
	{
		return animator.GetInteger(condition.CachedNameHash) < condition.IntValue;
	}

	public static bool smethod_6(EventCondition condition, IAnimator animator)
	{
		return animator.GetInteger(condition.CachedNameHash) > condition.IntValue;
	}

	public static bool smethod_7(EventCondition condition, IAnimator animator)
	{
		return animator.GetInteger(condition.CachedNameHash) != condition.IntValue;
	}

	public static bool smethod_8(EventCondition condition, IAnimator animator)
	{
		return animator.GetInteger(condition.CachedNameHash) == condition.IntValue;
	}

	public bool IsSucceed(IAnimator animator)
	{
		if (ConditionMode_1 == EConditionType.None)
		{
			CachedNameHash = animator.StringToHash(ParameterName);
			method_0();
		}
		return CONDITION_DELEGATES[(int)ConditionMode_1](this, animator);
	}

	public string ToString(IAnimator animator)
	{
		return $"{ParameterName} {ConditionMode}: {IsSucceed(animator)}";
	}

	public void method_0()
	{
		switch (ConditionParamType)
		{
		case EEventConditionParamTypes.Int:
			switch (ConditionMode)
			{
			case EEventConditionModes.Equal:
				ConditionMode_1 = EConditionType.IntEqual;
				break;
			case EEventConditionModes.NotEqual:
				ConditionMode_1 = EConditionType.IntNotEqual;
				break;
			case EEventConditionModes.GreaterThan:
				ConditionMode_1 = EConditionType.IntGreaterThan;
				break;
			case EEventConditionModes.LessThan:
				ConditionMode_1 = EConditionType.IntLessThan;
				break;
			case EEventConditionModes.GreaterEqualThan:
				ConditionMode_1 = EConditionType.IntGreaterEqualThan;
				break;
			case EEventConditionModes.LessEqualThan:
				ConditionMode_1 = EConditionType.IntLessEqualThan;
				break;
			}
			break;
		case EEventConditionParamTypes.Float:
			switch (ConditionMode)
			{
			case EEventConditionModes.LessThan:
				ConditionMode_1 = EConditionType.FloatLessThan;
				break;
			case EEventConditionModes.GreaterThan:
				ConditionMode_1 = EConditionType.FloatGreaterThan;
				break;
			}
			break;
		case EEventConditionParamTypes.Boolean:
			ConditionMode_1 = EConditionType.BoolEqual;
			break;
		}
	}

	public object Clone()
	{
		return new EventCondition
		{
			BoolValue = BoolValue,
			FloatValue = FloatValue,
			IntValue = IntValue,
			ConditionMode_1 = ConditionMode_1,
			CachedNameHash = CachedNameHash,
			ConditionMode = ConditionMode,
			ConditionParamType = ConditionParamType,
			ParameterName = ParameterName
		};
	}

	public void Serialize(BinaryWriter writer)
	{
		writer.Write(BoolValue);
		writer.Write(FloatValue);
		writer.Write(IntValue);
		writer.Write((short)ConditionMode_1);
		writer.Write(CachedNameHash);
		writer.Write((short)ConditionMode);
		writer.Write((short)ConditionParamType);
		writer.Write(ParameterName);
	}

	public void Deserialize(BinaryReader reader)
	{
		BoolValue = reader.ReadBoolean();
		FloatValue = reader.ReadSingle();
		IntValue = reader.ReadInt32();
		ConditionMode_1 = (EConditionType)reader.ReadInt16();
		CachedNameHash = reader.ReadInt32();
		ConditionMode = (EEventConditionModes)reader.ReadInt16();
		ConditionParamType = (EEventConditionParamTypes)reader.ReadInt16();
		ParameterName = reader.ReadString();
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((EventCondition)obj);
	}

	public override int GetHashCode()
	{
		return (int)(((uint)(((((((((BoolValue.GetHashCode() * 397) ^ FloatValue.GetHashCode()) * 397) ^ IntValue) * 397) ^ ((ParameterName != null) ? ParameterName.GetHashCode() : 0)) * 397) ^ CachedNameHash) * 397) ^ (uint)ConditionMode_1) * 397) ^ (int)ConditionParamType;
	}

	public bool Equals(EventCondition other)
	{
		if (BoolValue == other.BoolValue && FloatValue.Equals(other.FloatValue) && IntValue == other.IntValue && ParameterName == other.ParameterName && CachedNameHash == other.CachedNameHash && ConditionMode_1 == other.ConditionMode_1 && ConditionParamType == other.ConditionParamType)
		{
			return ConditionMode == other.ConditionMode;
		}
		return false;
	}
}
