using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using FastAnimatorSystem;

public abstract class GClass1346
{
	public class Class908
	{
		public int ControllerHashId;

		public Dictionary<int, GClass1312> States;

		public Dictionary<int, SpeedAnimParamClass> Parameters;

		public string AnimatorControllerObjectHash;

		public FastAnimatorControllerClass FastAnimatorController => States[ControllerHashId] as FastAnimatorControllerClass;
	}

	public class GClass1347 : ICloneable
	{
		public string UnityAnimatorControllerObjectHash;

		public List<GClass1352> Parameters;

		public List<GClass1348> States;

		public List<GClass1350> Transitions;

		public object Clone()
		{
			GClass1347 gClass = new GClass1347();
			gClass.UnityAnimatorControllerObjectHash = UnityAnimatorControllerObjectHash;
			gClass.Parameters = new List<GClass1352>(Parameters.Count);
			for (int i = 0; i < Parameters.Count; i++)
			{
				gClass.Parameters.Add(Parameters[i].Clone() as GClass1352);
			}
			gClass.States = new List<GClass1348>(States.Count);
			for (int j = 0; j < States.Count; j++)
			{
				gClass.States.Add(States[j].Clone() as GClass1348);
			}
			gClass.Transitions = new List<GClass1350>(Transitions.Count);
			for (int k = 0; k < Transitions.Count; k++)
			{
				gClass.Transitions.Add(Transitions[k].Clone() as GClass1350);
			}
			return gClass;
		}

		public void Serialize(BinaryWriter writer)
		{
			writer.Write(UnityAnimatorControllerObjectHash);
			writer.Write((short)Parameters.Count);
			for (int i = 0; i < Parameters.Count; i++)
			{
				Parameters[i].Serialize(writer);
			}
			writer.Write((short)States.Count);
			for (int j = 0; j < States.Count; j++)
			{
				States[j].Serialize(writer);
			}
			writer.Write((short)Transitions.Count);
			for (int k = 0; k < Transitions.Count; k++)
			{
				Transitions[k].Serialize(writer);
			}
		}

		public void Deserialize(BinaryReader reader)
		{
			UnityAnimatorControllerObjectHash = reader.ReadString();
			short num = reader.ReadInt16();
			Parameters = new List<GClass1352>(num);
			for (int i = 0; i < num; i++)
			{
				GClass1352 gClass = new GClass1352();
				gClass.Deserialize(reader);
				Parameters.Add(gClass);
			}
			short num2 = reader.ReadInt16();
			States = new List<GClass1348>(num2);
			for (int j = 0; j < num2; j++)
			{
				GClass1348 gClass2 = new GClass1348();
				gClass2.Deserialize(reader);
				States.Add(gClass2);
			}
			short num3 = reader.ReadInt16();
			Transitions = new List<GClass1350>(num3);
			for (int k = 0; k < num3; k++)
			{
				GClass1350 gClass3 = new GClass1350();
				gClass3.Deserialize(reader);
				Transitions.Add(gClass3);
			}
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
			return method_0((GClass1347)obj);
		}

		public override int GetHashCode()
		{
			return (((((UnityAnimatorControllerObjectHash.GetHashCode() * 397) ^ Parameters.GetHashCode()) * 397) ^ States.GetHashCode()) * 397) ^ Transitions.GetHashCode();
		}

		public bool method_0(GClass1347 other)
		{
			if (Parameters == null != (other.Parameters == null))
			{
				return false;
			}
			if (Parameters != null && !Parameters.SequenceEqual(other.Parameters))
			{
				return false;
			}
			if (States == null != (other.States == null))
			{
				return false;
			}
			if (States != null && !States.SequenceEqual(other.States))
			{
				return false;
			}
			if (Transitions == null != (other.Transitions == null))
			{
				return false;
			}
			if (Transitions != null && !Transitions.SequenceEqual(other.Transitions))
			{
				return false;
			}
			return UnityAnimatorControllerObjectHash == other.UnityAnimatorControllerObjectHash;
		}
	}

	public class GClass1348 : ICloneable
	{
		public enum BehType
		{
			None,
			EventsState,
			PlayerContainer
		}

		public string TypeName;

		public int Hash;

		public string Name;

		public int ParentStateMachine;

		public List<GClass1349> Motions;

		public float SpeedMultiplier;

		public int SpeedMultiplierParameterHash;

		public float LayerWeight;

		public int DefaultStateHash;

		public GClass1328[] Behaviors;

		public object Clone()
		{
			GClass1348 gClass = new GClass1348();
			gClass.TypeName = TypeName;
			gClass.Hash = Hash;
			gClass.Name = Name;
			gClass.ParentStateMachine = ParentStateMachine;
			if (Motions != null)
			{
				gClass.Motions = new List<GClass1349>(Motions.Count);
				for (int i = 0; i < Motions.Count; i++)
				{
					gClass.Motions.Add(Motions[i].Clone() as GClass1349);
				}
			}
			gClass.SpeedMultiplier = SpeedMultiplier;
			gClass.SpeedMultiplierParameterHash = SpeedMultiplierParameterHash;
			gClass.LayerWeight = LayerWeight;
			gClass.DefaultStateHash = DefaultStateHash;
			if (Behaviors != null)
			{
				gClass.Behaviors = new GClass1328[Behaviors.Length];
				for (int j = 0; j < Behaviors.Length; j++)
				{
					gClass.Behaviors[j] = Behaviors[j].Clone();
				}
			}
			return gClass;
		}

		public void Serialize(BinaryWriter writer)
		{
			writer.Write(TypeName);
			writer.Write(Hash);
			writer.Write(Name);
			writer.Write(ParentStateMachine);
			if (Motions == null)
			{
				writer.Write((short)0);
			}
			else
			{
				writer.Write((short)Motions.Count);
				for (int i = 0; i < Motions.Count; i++)
				{
					Motions[i].Serialize(writer);
				}
			}
			writer.Write(SpeedMultiplier);
			writer.Write(SpeedMultiplierParameterHash);
			writer.Write(LayerWeight);
			writer.Write(DefaultStateHash);
			if (Behaviors == null)
			{
				writer.Write((short)0);
				return;
			}
			writer.Write((short)Behaviors.Length);
			for (int j = 0; j < Behaviors.Length; j++)
			{
				GClass1328 gClass = Behaviors[j];
				if (gClass is GClass1330)
				{
					writer.Write((short)1);
				}
				else if (gClass is GClass1331)
				{
					writer.Write((short)2);
				}
				else
				{
					writer.Write((short)0);
				}
				gClass.Serialize(writer);
			}
		}

		public void Deserialize(BinaryReader reader)
		{
			TypeName = reader.ReadString();
			Hash = reader.ReadInt32();
			Name = reader.ReadString();
			ParentStateMachine = reader.ReadInt32();
			short num = reader.ReadInt16();
			if (num == 0)
			{
				Motions = null;
			}
			else
			{
				Motions = new List<GClass1349>(num);
				for (int i = 0; i < num; i++)
				{
					GClass1349 gClass = new GClass1349();
					gClass.Deserialize(reader);
					Motions.Add(gClass);
				}
			}
			SpeedMultiplier = reader.ReadSingle();
			SpeedMultiplierParameterHash = reader.ReadInt32();
			LayerWeight = reader.ReadSingle();
			DefaultStateHash = reader.ReadInt32();
			short num2 = reader.ReadInt16();
			if (num2 == 0)
			{
				Behaviors = new GClass1328[0];
				return;
			}
			Behaviors = new GClass1328[num2];
			for (int j = 0; j < num2; j++)
			{
				switch ((BehType)reader.ReadInt16())
				{
				case BehType.EventsState:
					Behaviors[j] = new GClass1330();
					break;
				case BehType.PlayerContainer:
					Behaviors[j] = new GClass1331();
					break;
				case BehType.None:
					throw new NotImplementedException("Failed to deserialize FastBehaviour");
				}
				Behaviors[j].Deserialize(reader);
			}
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
			return method_0((GClass1348)obj);
		}

		public override int GetHashCode()
		{
			return (((((((((((((((((((TypeName != null) ? TypeName.GetHashCode() : 0) * 397) ^ Hash) * 397) ^ ((Name != null) ? Name.GetHashCode() : 0)) * 397) ^ ParentStateMachine) * 397) ^ ((Motions != null) ? Motions.GetHashCode() : 0)) * 397) ^ SpeedMultiplier.GetHashCode()) * 397) ^ SpeedMultiplierParameterHash) * 397) ^ LayerWeight.GetHashCode()) * 397) ^ DefaultStateHash) * 397) ^ ((Behaviors != null) ? Behaviors.GetHashCode() : 0);
		}

		public bool method_0(GClass1348 other)
		{
			if (Motions == null != (other.Motions == null))
			{
				return false;
			}
			if (Motions != null && !Motions.SequenceEqual(other.Motions))
			{
				return false;
			}
			if (Behaviors == null != (other.Behaviors == null))
			{
				return false;
			}
			if (Behaviors != null && !Enumerable.SequenceEqual(Behaviors, other.Behaviors))
			{
				return false;
			}
			if (TypeName == other.TypeName && Hash == other.Hash && Name == other.Name && ParentStateMachine == other.ParentStateMachine && SpeedMultiplier.Equals(other.SpeedMultiplier) && SpeedMultiplierParameterHash == other.SpeedMultiplierParameterHash && LayerWeight.Equals(other.LayerWeight))
			{
				return DefaultStateHash == other.DefaultStateHash;
			}
			return false;
		}
	}

	public class GClass1349 : ICloneable
	{
		public float Duration;

		public string Name;

		public bool IsLooping;

		public int ClipInstanceId;

		public object Clone()
		{
			return new GClass1349
			{
				Duration = Duration,
				Name = Name,
				IsLooping = IsLooping,
				ClipInstanceId = ClipInstanceId
			};
		}

		public void Serialize(BinaryWriter writer)
		{
			writer.Write(Duration);
			writer.Write(Name);
			writer.Write(IsLooping);
			writer.Write(ClipInstanceId);
		}

		public void Deserialize(BinaryReader reader)
		{
			Duration = reader.ReadSingle();
			Name = reader.ReadString();
			IsLooping = reader.ReadBoolean();
			ClipInstanceId = reader.ReadInt32();
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
			return method_0((GClass1349)obj);
		}

		public override int GetHashCode()
		{
			return (((((Duration.GetHashCode() * 397) ^ ((Name != null) ? Name.GetHashCode() : 0)) * 397) ^ IsLooping.GetHashCode()) * 397) ^ ClipInstanceId;
		}

		public bool method_0(GClass1349 other)
		{
			if (Duration.Equals(other.Duration) && Name == other.Name && IsLooping == other.IsLooping)
			{
				return ClipInstanceId == other.ClipInstanceId;
			}
			return false;
		}
	}

	public class GClass1350 : ICloneable
	{
		public string Name;

		public int Hash;

		public int SourceStateId;

		public int DestinationStateId;

		public bool IsMuted;

		public bool IsSolo;

		public bool IsFixedDuration = true;

		public float ExitTime;

		public float Duration;

		public bool HasExitTime;

		public List<GClass1351> Conditions;

		public float DestinationStateOffset;

		public ETransitionInterruptionSource InterruptionSource;

		public bool IsOrderedInterruption;

		public object Clone()
		{
			GClass1350 gClass = new GClass1350();
			gClass.Name = Name;
			gClass.Hash = Hash;
			gClass.SourceStateId = SourceStateId;
			gClass.DestinationStateId = DestinationStateId;
			gClass.IsMuted = IsMuted;
			gClass.IsSolo = IsSolo;
			gClass.ExitTime = ExitTime;
			gClass.Duration = Duration;
			gClass.HasExitTime = HasExitTime;
			gClass.DestinationStateOffset = DestinationStateOffset;
			gClass.InterruptionSource = InterruptionSource;
			gClass.IsOrderedInterruption = IsOrderedInterruption;
			gClass.Conditions = new List<GClass1351>(Conditions.Count);
			for (int i = 0; i < Conditions.Count; i++)
			{
				gClass.Conditions.Add(Conditions[i].Clone() as GClass1351);
			}
			return gClass;
		}

		public void Serialize(BinaryWriter writer)
		{
			writer.Write(Name);
			writer.Write(Hash);
			writer.Write(SourceStateId);
			writer.Write(DestinationStateId);
			writer.Write(IsMuted);
			writer.Write(IsSolo);
			writer.Write(ExitTime);
			writer.Write(Duration);
			writer.Write(HasExitTime);
			writer.Write(DestinationStateOffset);
			writer.Write((short)InterruptionSource);
			writer.Write(IsOrderedInterruption);
			if (Conditions == null)
			{
				writer.Write((short)0);
				return;
			}
			writer.Write((short)Conditions.Count);
			for (int i = 0; i < Conditions.Count; i++)
			{
				Conditions[i].Serialize(writer);
			}
		}

		public void Deserialize(BinaryReader reader)
		{
			Name = reader.ReadString();
			Hash = reader.ReadInt32();
			SourceStateId = reader.ReadInt32();
			DestinationStateId = reader.ReadInt32();
			IsMuted = reader.ReadBoolean();
			IsSolo = reader.ReadBoolean();
			ExitTime = reader.ReadSingle();
			Duration = reader.ReadSingle();
			HasExitTime = reader.ReadBoolean();
			DestinationStateOffset = reader.ReadSingle();
			InterruptionSource = (ETransitionInterruptionSource)reader.ReadInt16();
			IsOrderedInterruption = reader.ReadBoolean();
			short num = reader.ReadInt16();
			if (num != 0)
			{
				Conditions = new List<GClass1351>(num);
				for (int i = 0; i < num; i++)
				{
					GClass1351 gClass = new GClass1351();
					gClass.Deserialize(reader);
					Conditions.Add(gClass);
				}
			}
			else
			{
				Conditions = new List<GClass1351>();
			}
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
			return method_0((GClass1350)obj);
		}

		public override int GetHashCode()
		{
			return (int)(((uint)(((((((((((((((((((((((Name != null) ? Name.GetHashCode() : 0) * 397) ^ Hash) * 397) ^ SourceStateId) * 397) ^ DestinationStateId) * 397) ^ IsMuted.GetHashCode()) * 397) ^ IsSolo.GetHashCode()) * 397) ^ ExitTime.GetHashCode()) * 397) ^ Duration.GetHashCode()) * 397) ^ HasExitTime.GetHashCode()) * 397) ^ ((Conditions != null) ? Conditions.GetHashCode() : 0)) * 397) ^ DestinationStateOffset.GetHashCode()) * 397) ^ (uint)InterruptionSource) * 397) ^ IsOrderedInterruption.GetHashCode();
		}

		public bool method_0(GClass1350 other)
		{
			if (Conditions == null != (other.Conditions == null))
			{
				return false;
			}
			if (Conditions != null && !Conditions.SequenceEqual(other.Conditions))
			{
				return false;
			}
			if (Name == other.Name && Hash == other.Hash && SourceStateId == other.SourceStateId && DestinationStateId == other.DestinationStateId && IsMuted == other.IsMuted && IsSolo == other.IsSolo && ExitTime.Equals(other.ExitTime) && Duration.Equals(other.Duration) && HasExitTime == other.HasExitTime && DestinationStateOffset.Equals(other.DestinationStateOffset) && InterruptionSource == other.InterruptionSource)
			{
				return IsOrderedInterruption == other.IsOrderedInterruption;
			}
			return false;
		}
	}

	public class GClass1351 : ICloneable
	{
		public string ParameterName;

		public int ParameterNameHash;

		public GClass1345.EConditionModes ConditionMode;

		public string ThresholdValue;

		public object Clone()
		{
			return new GClass1351
			{
				ParameterName = ParameterName,
				ParameterNameHash = ParameterNameHash,
				ConditionMode = ConditionMode,
				ThresholdValue = ThresholdValue
			};
		}

		public void Serialize(BinaryWriter writer)
		{
			writer.Write(ParameterName);
			writer.Write(ParameterNameHash);
			writer.Write((short)ConditionMode);
			writer.Write(ThresholdValue);
		}

		public void Deserialize(BinaryReader reader)
		{
			ParameterName = reader.ReadString();
			ParameterNameHash = reader.ReadInt32();
			ConditionMode = (GClass1345.EConditionModes)reader.ReadInt16();
			ThresholdValue = reader.ReadString();
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
			return method_0((GClass1351)obj);
		}

		public override int GetHashCode()
		{
			return (int)(((uint)(((((ParameterName != null) ? ParameterName.GetHashCode() : 0) * 397) ^ ParameterNameHash) * 397) ^ (uint)ConditionMode) * 397) ^ ((ThresholdValue != null) ? ThresholdValue.GetHashCode() : 0);
		}

		public bool method_0(GClass1351 other)
		{
			if (ParameterName == other.ParameterName && ParameterNameHash == other.ParameterNameHash && ConditionMode == other.ConditionMode)
			{
				return ThresholdValue == other.ThresholdValue;
			}
			return false;
		}
	}

	public class GClass1352 : ICloneable
	{
		public string Name;

		public int Hash;

		public ThresholdClass.EAnimatorValueType Type;

		public string Value;

		public bool IsControlledByAnimationCurver;

		public bool DefaultBoolValue;

		public float DefaultFloatValue;

		public int DefaultIntValue;

		public object Clone()
		{
			return new GClass1352
			{
				Name = Name,
				Hash = Hash,
				Type = Type,
				Value = Value,
				IsControlledByAnimationCurver = IsControlledByAnimationCurver,
				DefaultBoolValue = DefaultBoolValue,
				DefaultFloatValue = DefaultFloatValue,
				DefaultIntValue = DefaultIntValue
			};
		}

		public void Serialize(BinaryWriter writer)
		{
			writer.Write(Name);
			writer.Write(Hash);
			writer.Write((short)Type);
			writer.Write(Value);
			writer.Write(IsControlledByAnimationCurver);
			writer.Write(DefaultBoolValue);
			writer.Write(DefaultFloatValue);
			writer.Write(DefaultIntValue);
		}

		public void Deserialize(BinaryReader reader)
		{
			Name = reader.ReadString();
			Hash = reader.ReadInt32();
			Type = (ThresholdClass.EAnimatorValueType)reader.ReadInt16();
			Value = reader.ReadString();
			IsControlledByAnimationCurver = reader.ReadBoolean();
			DefaultBoolValue = reader.ReadBoolean();
			DefaultFloatValue = reader.ReadSingle();
			DefaultIntValue = reader.ReadInt32();
		}

		public bool method_0(GClass1352 other)
		{
			if (Name == other.Name && Hash == other.Hash && Type == other.Type && Value == other.Value && IsControlledByAnimationCurver == other.IsControlledByAnimationCurver && DefaultBoolValue == other.DefaultBoolValue && DefaultFloatValue.Equals(other.DefaultFloatValue))
			{
				return DefaultIntValue == other.DefaultIntValue;
			}
			return false;
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
			return method_0((GClass1352)obj);
		}

		public override int GetHashCode()
		{
			return (int)(((((((((((uint)(((((Name != null) ? Name.GetHashCode() : 0) * 397) ^ Hash) * 397) ^ (uint)Type) * 397) ^ (uint)((Value != null) ? Value.GetHashCode() : 0)) * 397) ^ (uint)IsControlledByAnimationCurver.GetHashCode()) * 397) ^ (uint)DefaultBoolValue.GetHashCode()) * 397) ^ (uint)DefaultFloatValue.GetHashCode()) * 397) ^ DefaultIntValue;
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class909
	{
		public static readonly Class909 class909_0 = new Class909();

		public static Func<TransitionClass, GClass1350> func_0;

		public static Func<CurrentStateAbstractClass, IEnumerable<GClass1350>> func_1;

		public static Func<SpeedAnimParamClass, GClass1352> func_2;

		public static Func<GClass1326, GClass1349> func_3;

		public static Func<GClass1345, GClass1351> func_4;

		public IEnumerable<GClass1350> method_0(CurrentStateAbstractClass s)
		{
			return from t in s.GetOutTransitions()
				select smethod_16(t);
		}

		public GClass1350 method_1(TransitionClass t)
		{
			return smethod_16(t);
		}

		public GClass1352 method_2(SpeedAnimParamClass p)
		{
			return smethod_9(p);
		}

		public GClass1349 method_3(GClass1326 x)
		{
			return smethod_18(x);
		}

		public GClass1351 method_4(GClass1345 c)
		{
			return smethod_17(c);
		}
	}

	[NonSerialized]
	public static string String_0 = "FastAnimatorController";

	[NonSerialized]
	public static string String_1 = "LayerStateMachine";

	[NonSerialized]
	public static string String_2 = "StateMachine";

	[NonSerialized]
	public static string String_3 = "AnimatedState";

	[NonSerialized]
	public static string String_4 = "OneDBlendTreeState";

	public static GClass1347 DeserializeToFlat(byte[] data)
	{
		BinaryReader reader = new BinaryReader(new MemoryStream(data));
		GClass1347 gClass = new GClass1347();
		gClass.Deserialize(reader);
		return gClass;
	}

	public static FastAnimatorControllerClass ConvertFlatToFast(GClass1347 flatFastAnimatorController)
	{
		int hash = flatFastAnimatorController.States[0].Hash;
		Class908 @class = new Class908
		{
			ControllerHashId = hash,
			AnimatorControllerObjectHash = flatFastAnimatorController.UnityAnimatorControllerObjectHash,
			States = new Dictionary<int, GClass1312>(flatFastAnimatorController.States.Count),
			Parameters = new Dictionary<int, SpeedAnimParamClass>(flatFastAnimatorController.Parameters.Count)
		};
		foreach (GClass1348 state in flatFastAnimatorController.States)
		{
			if (state.TypeName == String_0)
			{
				GClass1312 gClass = smethod_1(state, @class);
				@class.States.Add(gClass.Hash, gClass);
			}
		}
		foreach (GClass1352 parameter in flatFastAnimatorController.Parameters)
		{
			SpeedAnimParamClass speedAnimParamClass = smethod_5(parameter, @class);
			@class.Parameters.Add(speedAnimParamClass.NameHash, speedAnimParamClass);
		}
		foreach (GClass1348 state2 in flatFastAnimatorController.States)
		{
			if (!(state2.TypeName == String_0))
			{
				GClass1312 gClass2 = smethod_1(state2, @class);
				@class.States.Add(gClass2.Hash, gClass2);
			}
		}
		foreach (GClass1350 transition in flatFastAnimatorController.Transitions)
		{
			smethod_0(transition, @class);
		}
		foreach (KeyValuePair<int, GClass1312> state3 in @class.States)
		{
			if (state3.Value is CurrentStateAbstractClass stateMachine)
			{
				GClass1323.SortTransitions(stateMachine);
			}
			if (state3.Value is GClass1315 stateMachine2)
			{
				GClass1323.SortStates(stateMachine2);
			}
		}
		@class.FastAnimatorController.RefreshAllStates();
		return @class.FastAnimatorController;
	}

	public static FastAnimatorControllerClass Deserialize(byte[] data)
	{
		return ConvertFlatToFast(DeserializeToFlat(data));
	}

	public static TransitionClass smethod_0(this GClass1350 flatAnimatorControllerTransition, Class908 context)
	{
		if (!context.States.ContainsKey(flatAnimatorControllerTransition.SourceStateId))
		{
			throw new Exception($"There is no source state with id: {flatAnimatorControllerTransition.SourceStateId}, for transition {flatAnimatorControllerTransition.Name}:{flatAnimatorControllerTransition.Hash}");
		}
		if (!context.States.ContainsKey(flatAnimatorControllerTransition.DestinationStateId))
		{
			throw new Exception($"There is no destination state with id: {flatAnimatorControllerTransition.DestinationStateId}, for transition {flatAnimatorControllerTransition.Name}:{flatAnimatorControllerTransition.Hash}");
		}
		CurrentStateAbstractClass currentStateAbstractClass = context.States[flatAnimatorControllerTransition.SourceStateId] as CurrentStateAbstractClass;
		CurrentStateAbstractClass destinationState = context.States[flatAnimatorControllerTransition.DestinationStateId] as CurrentStateAbstractClass;
		GClass1345[] array = new GClass1345[flatAnimatorControllerTransition.Conditions.Count];
		for (int i = 0; i < flatAnimatorControllerTransition.Conditions.Count; i++)
		{
			array[i] = smethod_4(flatAnimatorControllerTransition.Conditions[i], context);
		}
		TransitionClass transitionClass = new TransitionClass(flatAnimatorControllerTransition.Name, flatAnimatorControllerTransition.Hash, currentStateAbstractClass, destinationState, flatAnimatorControllerTransition.IsMuted, flatAnimatorControllerTransition.IsSolo, flatAnimatorControllerTransition.IsFixedDuration, flatAnimatorControllerTransition.ExitTime, flatAnimatorControllerTransition.Duration, flatAnimatorControllerTransition.HasExitTime, flatAnimatorControllerTransition.DestinationStateOffset, array, flatAnimatorControllerTransition.InterruptionSource, flatAnimatorControllerTransition.IsOrderedInterruption);
		GClass1323.AddTransition(currentStateAbstractClass, transitionClass);
		return transitionClass;
	}

	public static GClass1312 smethod_1(this GClass1348 controllerEntity, Class908 context)
	{
		string typeName = controllerEntity.TypeName;
		if (typeName == String_0)
		{
			string animatorControllerObjectHash = context.AnimatorControllerObjectHash;
			return new FastAnimatorControllerClass(controllerEntity.Name, controllerEntity.Hash, animatorControllerObjectHash);
		}
		if (typeName == String_1)
		{
			GClass1316 gClass = new GClass1316(controllerEntity.Name, controllerEntity.Hash, controllerEntity.DefaultStateHash)
			{
				LayerWeight = controllerEntity.LayerWeight,
				Gclass1328_0 = smethod_2(controllerEntity.Behaviors)
			};
			GClass1323.AddLayer(context.States[controllerEntity.ParentStateMachine] as FastAnimatorControllerClass, gClass);
			return gClass;
		}
		if (typeName == String_2)
		{
			GClass1317 gClass2 = new GClass1317(controllerEntity.Name, controllerEntity.Hash, controllerEntity.DefaultStateHash)
			{
				Gclass1328_0 = smethod_2(controllerEntity.Behaviors)
			};
			GClass1323.AddStateMachine(context.States[controllerEntity.ParentStateMachine] as GClass1315, gClass2);
			return gClass2;
		}
		SpeedAnimParamClass value = null;
		context.Parameters.TryGetValue(controllerEntity.SpeedMultiplierParameterHash, out value);
		if (typeName == String_3)
		{
			GClass1326 clipInfo = smethod_3(controllerEntity.Motions[0]);
			GClass1322 gClass3 = new GClass1322(controllerEntity.Name, controllerEntity.Hash, controllerEntity.SpeedMultiplier, value, clipInfo)
			{
				Gclass1328_0 = smethod_2(controllerEntity.Behaviors)
			};
			GClass1323.AddState(context.States[controllerEntity.ParentStateMachine] as GClass1315, gClass3);
			return gClass3;
		}
		if (typeName == String_4)
		{
			List<GClass1326> list = new List<GClass1326>(controllerEntity.Motions?.Count ?? 0);
			if (controllerEntity.Motions != null)
			{
				for (int i = 0; i < controllerEntity.Motions.Count; i++)
				{
					list.Add(smethod_3(controllerEntity.Motions[i]));
				}
			}
			MoveAllOperationClass moveAllOperationClass = new MoveAllOperationClass(controllerEntity.Name, controllerEntity.Hash, controllerEntity.SpeedMultiplier, value, list)
			{
				Gclass1328_0 = smethod_2(controllerEntity.Behaviors)
			};
			GClass1323.AddState(context.States[controllerEntity.ParentStateMachine] as GClass1315, moveAllOperationClass);
			return moveAllOperationClass;
		}
		throw new NotImplementedException(typeName);
	}

	public static GClass1328[] smethod_2(GClass1328[] behaviors)
	{
		GClass1328[] array = new GClass1328[behaviors.Length];
		for (int i = 0; i < behaviors.Length; i++)
		{
			array[i] = behaviors[i].Clone();
		}
		return array;
	}

	public static GClass1326 smethod_3(this GClass1349 motion)
	{
		return GClass1327.Create(motion.Duration, motion.Name, motion.IsLooping, motion.ClipInstanceId);
	}

	public static GClass1345 smethod_4(this GClass1351 flatTranstionCondition, Class908 context)
	{
		SpeedAnimParamClass speedAnimParamClass = context.Parameters[flatTranstionCondition.ParameterNameHash];
		ThresholdClass thresholdClass = new ThresholdClass(speedAnimParamClass.ValueType);
		thresholdClass.SetValue(flatTranstionCondition.ThresholdValue);
		return new GClass1345(speedAnimParamClass, thresholdClass, flatTranstionCondition.ConditionMode);
	}

	public static SpeedAnimParamClass smethod_5(this GClass1352 flatAnimatorParameter, Class908 context)
	{
		SpeedAnimParamClass speedAnimParamClass = new SpeedAnimParamClass(flatAnimatorParameter.Name, flatAnimatorParameter.Type, flatAnimatorParameter.Hash, flatAnimatorParameter.IsControlledByAnimationCurver, flatAnimatorParameter.DefaultFloatValue, flatAnimatorParameter.DefaultIntValue, flatAnimatorParameter.DefaultBoolValue);
		speedAnimParamClass.SetValue(flatAnimatorParameter.Value);
		GClass1323.AddParameter(context.FastAnimatorController, speedAnimParamClass);
		return speedAnimParamClass;
	}

	public static MemoryStream Serialize(FastAnimatorControllerClass fastAnimatorController)
	{
		GClass1347 gClass = new GClass1347();
		List<GClass1348> list = new List<GClass1348> { smethod_15(fastAnimatorController) };
		list.AddRange(smethod_6(fastAnimatorController));
		gClass.UnityAnimatorControllerObjectHash = fastAnimatorController.UnityAnimatorControllerObjectHash;
		gClass.Parameters = smethod_8(fastAnimatorController);
		gClass.States = list;
		gClass.Transitions = smethod_7(fastAnimatorController);
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(memoryStream);
		gClass.Serialize(writer);
		return memoryStream;
	}

	public static List<GClass1348> smethod_6(this FastAnimatorControllerClass fastAnimatorController)
	{
		List<GClass1348> list = new List<GClass1348>();
		for (int i = 0; i < fastAnimatorController.LayersCount; i++)
		{
			GClass1316 layerStateMachine = fastAnimatorController.GetLayerStateMachine(i);
			list.AddRange(smethod_10(layerStateMachine));
		}
		return list;
	}

	public static List<GClass1350> smethod_7(this FastAnimatorControllerClass fastAnimatorController)
	{
		List<GClass1350> list = new List<GClass1350>();
		Queue<GClass1315> queue = new Queue<GClass1315>();
		for (int i = 0; i < fastAnimatorController.LayersCount; i++)
		{
			GClass1316 layerStateMachine = fastAnimatorController.GetLayerStateMachine(i);
			queue.Enqueue(layerStateMachine);
			while (queue.Count > 0)
			{
				GClass1315 gClass = queue.Dequeue();
				list.AddRange(gClass.GetStatesEnumerator().SelectMany((CurrentStateAbstractClass s) => from t in s.GetOutTransitions()
					select smethod_16(t)));
				foreach (GClass1315 item in gClass.GetStateMachinesEnumerator())
				{
					queue.Enqueue(item);
				}
			}
		}
		return list;
	}

	public static List<GClass1352> smethod_8(this FastAnimatorControllerClass fastAnimatorController)
	{
		return (from p in fastAnimatorController.GetParameters()
			select smethod_9(p)).ToList();
	}

	public static GClass1352 smethod_9(this SpeedAnimParamClass animatorParameter)
	{
		return new GClass1352
		{
			Name = animatorParameter.Name,
			Hash = animatorParameter.NameHash,
			Type = animatorParameter.ValueType,
			Value = animatorParameter.GetStringValue(),
			IsControlledByAnimationCurver = animatorParameter.IsControlledByAnimationCurve,
			DefaultBoolValue = animatorParameter.DefaultBoolValue,
			DefaultFloatValue = animatorParameter.DefaultFloatValue,
			DefaultIntValue = animatorParameter.DefaultIntValue
		};
	}

	public static List<GClass1348> smethod_10(this GClass1316 layerStateMachine)
	{
		List<GClass1348> list = new List<GClass1348>();
		Queue<GClass1315> queue = new Queue<GClass1315>();
		queue.Enqueue(layerStateMachine);
		while (queue.Count > 0)
		{
			GClass1315 gClass = queue.Dequeue();
			list.Add(smethod_11(gClass));
			for (int i = 0; i < gClass.StatesCount; i++)
			{
				CurrentStateAbstractClass state = gClass.GetState(i);
				list.Add(smethod_12(state));
			}
			for (int j = 0; j < gClass.StatesMachinesCount; j++)
			{
				queue.Enqueue(gClass.GetStateMachine(j));
			}
		}
		return list;
	}

	public static GClass1348 smethod_11(this GClass1315 layerStateMachine)
	{
		GClass1348 gClass = new GClass1348
		{
			Name = layerStateMachine.Name,
			Hash = layerStateMachine.Hash,
			TypeName = layerStateMachine.GetType().Name,
			ParentStateMachine = layerStateMachine.ParentStateMachine.Hash,
			DefaultStateHash = layerStateMachine.DefaultStateHash,
			Behaviors = layerStateMachine.Gclass1328_0
		};
		if (layerStateMachine is GClass1316 gClass2)
		{
			gClass.LayerWeight = gClass2.LayerWeight;
		}
		return gClass;
	}

	public static GClass1348 smethod_12(this CurrentStateAbstractClass controllerState)
	{
		if (controllerState is GClass1322)
		{
			return smethod_13((GClass1322)controllerState);
		}
		if (!(controllerState is MoveAllOperationClass))
		{
			throw new NotImplementedException("Unknown state: " + controllerState);
		}
		return smethod_14((MoveAllOperationClass)controllerState);
	}

	public static GClass1348 smethod_13(this GClass1322 controllerState)
	{
		return new GClass1348
		{
			Name = controllerState.Name,
			Hash = controllerState.Hash,
			TypeName = controllerState.GetType().Name,
			SpeedMultiplier = controllerState.Float_0,
			SpeedMultiplierParameterHash = ((controllerState.SpeedAnimParamClass != null) ? controllerState.SpeedAnimParamClass.NameHash : 0),
			ParentStateMachine = controllerState.ParentStateMachine.Hash,
			Motions = new List<GClass1349> { smethod_18(controllerState.ClipInfo) },
			Behaviors = controllerState.Gclass1328_0
		};
	}

	public static GClass1348 smethod_14(this MoveAllOperationClass controllerState)
	{
		return new GClass1348
		{
			Name = controllerState.Name,
			Hash = controllerState.Hash,
			TypeName = controllerState.GetType().Name,
			SpeedMultiplier = controllerState.Float_0,
			SpeedMultiplierParameterHash = ((controllerState.SpeedAnimParamClass != null) ? controllerState.SpeedAnimParamClass.NameHash : 0),
			ParentStateMachine = controllerState.ParentStateMachine.Hash,
			Motions = controllerState.BlendSettings.Select((GClass1326 x) => smethod_18(x)).ToList(),
			Behaviors = controllerState.Gclass1328_0
		};
	}

	public static GClass1348 smethod_15(this GClass1312 fastAnimatorController)
	{
		return new GClass1348
		{
			Name = fastAnimatorController.Name,
			Hash = fastAnimatorController.Hash,
			TypeName = fastAnimatorController.GetType().Name,
			Behaviors = fastAnimatorController.Gclass1328_0
		};
	}

	public static GClass1350 smethod_16(this TransitionClass transition)
	{
		return new GClass1350
		{
			Name = transition.Name,
			Hash = transition.Hash,
			SourceStateId = transition.SourceState.Hash,
			DestinationStateId = transition.DestinationState.Hash,
			IsMuted = transition.IsMuted,
			IsSolo = transition.IsSolo,
			IsFixedDuration = transition.IsFixedDuration,
			Duration = transition.Duration,
			ExitTime = transition.ExitTimeNormalized,
			HasExitTime = transition.HasExitTime,
			DestinationStateOffset = transition.NormalizedDestinationStateOffset,
			Conditions = transition.Conditions.Select((GClass1345 c) => smethod_17(c)).ToList(),
			InterruptionSource = transition.InterruptionSource,
			IsOrderedInterruption = transition.IsOrderedInterruption
		};
	}

	public static GClass1351 smethod_17(this GClass1345 condition)
	{
		return new GClass1351
		{
			ParameterName = condition.Parameter.Name,
			ParameterNameHash = condition.Parameter.NameHash,
			ConditionMode = condition.ConditionMode,
			ThresholdValue = condition.Threshold.GetStringValue()
		};
	}

	public static GClass1349 smethod_18(this GClass1326 abstractMotion)
	{
		if (abstractMotion is GClass1327 gClass)
		{
			return new GClass1349
			{
				Name = gClass.Name,
				Duration = gClass.Duration,
				IsLooping = gClass.IsLooping,
				ClipInstanceId = gClass.ClipInstanceId
			};
		}
		return new GClass1349
		{
			Name = abstractMotion.Name,
			Duration = abstractMotion.Duration
		};
	}
}
