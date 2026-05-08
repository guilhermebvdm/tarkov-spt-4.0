using System;
using System.IO;
using System.Runtime.CompilerServices;
using EFT;

public class GClass1331 : GClass1328, IPlayerStateContainerBehaviour, IStateBehaviour
{
	public bool IsDefaultState;

	public EPlayerState Name;

	public EStateType Type;

	public float RotationSpeedClamp = 99f;

	public float StateSensitivity = 1f;

	public bool CanInteract;

	public bool DisableRootMotion;

	public bool CreateUniqueMovementStateObject;

	public float AnimationAuthority;

	public float AuthoritySpeed = 4f;

	public string StateFullName;

	public float StateLength;

	public int StateFullNameHash;

	[NonSerialized]
	[CompilerGenerated]
	public BaseMovementState BaseMovementState_0;

	public bool DefaultState
	{
		get
		{
			return IsDefaultState;
		}
		set
		{
			IsDefaultState = value;
		}
	}

	public EPlayerState PlayerStateName
	{
		get
		{
			return Name;
		}
		set
		{
			Name = value;
		}
	}

	public BaseMovementState EncapsulatedState
	{
		[CompilerGenerated]
		get
		{
			return BaseMovementState_0;
		}
		[CompilerGenerated]
		set
		{
			BaseMovementState_0 = value;
		}
	}

	public int FullNameHash => StateFullNameHash;

	public void Init(Func<EPlayerState, int, bool, BaseMovementState> newStateFunc)
	{
		if (StateFullNameHash == 0)
		{
			throw new Exception("no StateFullNameHash for PlayerStateContainer: " + Name);
		}
		EncapsulatedState = newStateFunc(Name, StateFullNameHash, CreateUniqueMovementStateObject);
		if (EncapsulatedState != null)
		{
			EncapsulatedState.RotationSpeedClamp = RotationSpeedClamp;
			EncapsulatedState.StateSensitivity = StateSensitivity;
			EncapsulatedState.CanInteract = CanInteract;
			EncapsulatedState.AnimationAuthority = AnimationAuthority;
			EncapsulatedState.AuthoritySpeed = AuthoritySpeed;
			EncapsulatedState.Name = Name;
			EncapsulatedState.AnimatorStateName = StateFullName;
			EncapsulatedState.AnimatorStateHash = StateFullNameHash;
			EncapsulatedState.StateLength = StateLength;
			EncapsulatedState.Type = Type;
		}
	}

	public void Deinit()
	{
		EncapsulatedState = null;
	}

	public override GClass1328 Clone()
	{
		return new GClass1331
		{
			IsDefaultState = IsDefaultState,
			Name = Name,
			RotationSpeedClamp = RotationSpeedClamp,
			StateSensitivity = StateSensitivity,
			CanInteract = CanInteract,
			DisableRootMotion = DisableRootMotion,
			CreateUniqueMovementStateObject = CreateUniqueMovementStateObject,
			AnimationAuthority = AnimationAuthority,
			AuthoritySpeed = AuthoritySpeed,
			StateFullName = StateFullName,
			StateLength = StateLength,
			StateFullNameHash = StateFullNameHash
		};
	}

	public override void Serialize(BinaryWriter writer)
	{
		writer.Write(IsDefaultState);
		writer.Write((short)Name);
		writer.Write(RotationSpeedClamp);
		writer.Write(StateSensitivity);
		writer.Write(CanInteract);
		writer.Write(DisableRootMotion);
		writer.Write(CreateUniqueMovementStateObject);
		writer.Write(AnimationAuthority);
		writer.Write(AuthoritySpeed);
		writer.Write(StateFullName);
		writer.Write(StateLength);
		writer.Write(StateFullNameHash);
	}

	public override void Deserialize(BinaryReader reader)
	{
		IsDefaultState = reader.ReadBoolean();
		Name = (EPlayerState)reader.ReadInt16();
		RotationSpeedClamp = reader.ReadSingle();
		StateSensitivity = reader.ReadSingle();
		CanInteract = reader.ReadBoolean();
		DisableRootMotion = reader.ReadBoolean();
		CreateUniqueMovementStateObject = reader.ReadBoolean();
		AnimationAuthority = reader.ReadSingle();
		AuthoritySpeed = reader.ReadSingle();
		StateFullName = reader.ReadString();
		StateLength = reader.ReadSingle();
		StateFullNameHash = reader.ReadInt32();
	}

	public override void OnStateEnter(FastAnimatorProcessorClass fastAnimatorProcessor, in AnimatorStateInfoWrapper layerInfo, int layerIndex)
	{
		if (EncapsulatedState != null)
		{
			EncapsulatedState.AnimatorStateHash = StateFullNameHash;
			EncapsulatedState.AnimatorStateName = StateFullName;
			EncapsulatedState.RotationSpeedClamp = RotationSpeedClamp;
			EncapsulatedState.StateSensitivity = StateSensitivity;
			EncapsulatedState.CanInteract = CanInteract;
			EncapsulatedState.Type = Type;
			EncapsulatedState.OnStateEnter(layerInfo.normalizedTime);
			if (DisableRootMotion)
			{
				EncapsulatedState.DisableRootMotion = true;
			}
		}
	}

	public override void OnStateUpdate(FastAnimatorProcessorClass fastAnimatorProcessor, in AnimatorStateInfoWrapper layerInfo, int layerIndex)
	{
	}

	public override void OnStateExit(FastAnimatorProcessorClass fastAnimatorProcessor, in AnimatorStateInfoWrapper layerInfo, int layerIndex)
	{
		if (EncapsulatedState != null)
		{
			EncapsulatedState.OnStateExit(layerInfo.normalizedTime);
			if (DisableRootMotion)
			{
				EncapsulatedState.DisableRootMotion = false;
			}
		}
	}

	public override void OnStateMove(FastAnimatorProcessorClass fastAnimatorProcessor, in AnimatorStateInfoWrapper layerInfo, int layerIndex)
	{
		EncapsulatedState?.OnStateMove(layerInfo.normalizedTime);
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
		return method_0((GClass1331)obj);
	}

	public override int GetHashCode()
	{
		return (int)(((((((((((((((((((((uint)(IsDefaultState.GetHashCode() * 397) ^ (uint)Name) * 397) ^ (uint)RotationSpeedClamp.GetHashCode()) * 397) ^ (uint)StateSensitivity.GetHashCode()) * 397) ^ (uint)CanInteract.GetHashCode()) * 397) ^ (uint)DisableRootMotion.GetHashCode()) * 397) ^ (uint)CreateUniqueMovementStateObject.GetHashCode()) * 397) ^ (uint)AnimationAuthority.GetHashCode()) * 397) ^ (uint)AuthoritySpeed.GetHashCode()) * 397) ^ (uint)((StateFullName != null) ? StateFullName.GetHashCode() : 0)) * 397) ^ (uint)StateLength.GetHashCode()) * 397) ^ StateFullNameHash;
	}

	public bool method_0(GClass1331 other)
	{
		if (IsDefaultState == other.IsDefaultState && Name == other.Name && RotationSpeedClamp.Equals(other.RotationSpeedClamp) && StateSensitivity.Equals(other.StateSensitivity) && CanInteract == other.CanInteract && DisableRootMotion == other.DisableRootMotion && CreateUniqueMovementStateObject == other.CreateUniqueMovementStateObject && AnimationAuthority.Equals(other.AnimationAuthority) && AuthoritySpeed.Equals(other.AuthoritySpeed) && StateFullName == other.StateFullName && StateLength.Equals(other.StateLength))
		{
			return StateFullNameHash == other.StateFullNameHash;
		}
		return false;
	}
}
