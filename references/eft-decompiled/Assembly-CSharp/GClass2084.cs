using BitPacking;
using EFT;
using EFT.NetworkPackets;

public abstract class GClass2084
{
	public delegate void GDelegate67<T>(GInterface131 writer, ref T? t) where T : struct;

	public delegate T? GDelegate68<T>(IDataReader reader) where T : struct;

	public static void Serialize<T>(this GInterface131 writer, GInterface217<T> nestable, GDelegate67<T> serializeDelegate) where T : struct, GInterface217<T>
	{
		T? t = (T?)(object)nestable;
		serializeDelegate(writer, ref t);
	}

	public static GInterface217<T> Deserialize<T>(this IDataReader reader, GDelegate68<T> deserializeDelegate) where T : struct, GInterface217<T>
	{
		T? val = deserializeDelegate(reader);
		if (val.HasValue)
		{
			return val.GetValueOrDefault();
		}
		return null;
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct187? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			writer.Write(packet.Value.BodyPart);
			writer.Write(packet.Value.Value);
		}
	}

	public static void DeserializePacket(this IDataReader reader, ref GStruct187? packet)
	{
		if (reader.ReadBool())
		{
			packet = new GStruct187
			{
				BodyPart = reader.ReadEnum<EBodyPart>(),
				Value = reader.ReadFloat()
			};
		}
		else
		{
			packet = null;
		}
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct190? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			writer.Write(packet.Value.FriendlyUsecs);
			writer.Write(packet.Value.FriendlyZryachiy);
		}
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct191? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			writer.Write(packet.Value.btrSupportValue);
		}
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct207? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			writer.Write(packet.Value.Value);
		}
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct208? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			writer.Write(packet.Value.Value);
		}
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct209? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			writer.Write(packet.Value.Value);
		}
	}

	public static void DeserializePacket(this IDataReader reader, ref GStruct190? packet)
	{
		if (reader.ReadBool())
		{
			packet = new GStruct190
			{
				FriendlyUsecs = reader.ReadBool(),
				FriendlyZryachiy = reader.ReadBool()
			};
		}
		else
		{
			packet = null;
		}
	}

	public static void DeserializePacket(this IDataReader reader, ref GStruct191? packet)
	{
		if (reader.ReadBool())
		{
			packet = new GStruct191
			{
				btrSupportValue = reader.ReadInt32()
			};
		}
		else
		{
			packet = null;
		}
	}

	public static void DeserializePacket(this IDataReader reader, ref GStruct207? packet)
	{
		if (reader.ReadBool())
		{
			packet = new GStruct207
			{
				Value = reader.ReadBool()
			};
		}
		else
		{
			packet = null;
		}
	}

	public static void DeserializePacket(this IDataReader reader, ref GStruct208? packet)
	{
		if (reader.ReadBool())
		{
			packet = new GStruct208
			{
				Value = reader.ReadString(1600u)
			};
		}
		else
		{
			packet = null;
		}
	}

	public static void DeserializePacket(this IDataReader reader, ref GStruct209? packet)
	{
		if (reader.ReadBool())
		{
			packet = new GStruct209
			{
				Value = reader.ReadVector3()
			};
		}
		else
		{
			packet = null;
		}
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct188? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			writer.Write(packet.GetValueOrDefault().Position);
		}
	}

	public static void DeserializePacket(this IDataReader reader, ref GStruct188? packet)
	{
		if (reader.ReadBool())
		{
			packet = new GStruct188
			{
				Position = reader.ReadVector3()
			};
		}
		else
		{
			packet = null;
		}
	}

	public static void DeserializePacket(this IDataReader reader, ref GStruct194? packet)
	{
		if (reader.ReadBool())
		{
			packet = default(GStruct194);
		}
		else
		{
			packet = null;
		}
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct196? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			writer.Write(packet.GetValueOrDefault().EventState);
		}
	}

	public static void DeserializePacket(this IDataReader reader, ref GStruct196? packet)
	{
		if (reader.ReadBool())
		{
			packet = new GStruct196(reader.ReadEnum<EEventState>());
		}
		else
		{
			packet = null;
		}
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct194? packet)
	{
		writer.Write(packet.HasValue);
	}

	public static void DeserializePacket(this IDataReader reader, ref GStruct195? packet)
	{
		if (reader.ReadBool())
		{
			int count = reader.ReadInt32();
			packet = new GStruct195
			{
				Count = count
			};
		}
		else
		{
			packet = null;
		}
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct195? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			writer.Write(packet.Value.Count);
		}
	}

	public static void DeserializePacket(this IDataReader reader, ref GStruct197? packet)
	{
		if (reader.ReadBool())
		{
			bool openDoors = reader.ReadBool();
			packet = new GStruct197
			{
				OpenDoors = openDoors
			};
		}
		else
		{
			packet = null;
		}
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct197? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			writer.Write(packet.Value.OpenDoors);
		}
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct189? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			writer.Write(packet.GetValueOrDefault().Value);
		}
	}

	public static void DeserializePacket(this IDataReader reader, ref GStruct189? packet)
	{
		if (reader.ReadBool())
		{
			packet = new GStruct189
			{
				Value = reader.ReadFloat()
			};
		}
		else
		{
			packet = null;
		}
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct192? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			GStruct192 valueOrDefault = packet.GetValueOrDefault();
			writer.Write(valueOrDefault.Command);
			writer.Write(valueOrDefault.ContainerId);
		}
	}

	public static void DeserializePacket(this IDataReader reader, ref GStruct192? packet)
	{
		if (reader.ReadBool())
		{
			packet = new GStruct192
			{
				Command = reader.ReadInt32(),
				ContainerId = reader.ReadString(1600u)
			};
		}
		else
		{
			packet = null;
		}
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct202? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			writer.Write(packet.GetValueOrDefault().AccessLevel);
		}
	}

	public static void DeserializePacket(this IDataReader reader, ref GStruct202? packet)
	{
		if (reader.ReadBool())
		{
			packet = new GStruct202
			{
				AccessLevel = reader.ReadInt32()
			};
		}
		else
		{
			packet = null;
		}
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct203? packet)
	{
		writer.Write(packet.HasValue);
	}

	public static void DeserializePacket(this IDataReader reader, ref GStruct203? packet)
	{
		if (reader.ReadBool())
		{
			packet = default(GStruct203);
		}
		else
		{
			packet = null;
		}
	}

	public static void DeserializePacket(this IDataReader reader, ref GStruct198? packet)
	{
		if (reader.ReadBool())
		{
			packet = default(GStruct198);
		}
		else
		{
			packet = null;
		}
	}

	public static void DeserializePacket(this IDataReader reader, ref GStruct199? packet)
	{
		if (reader.ReadBool())
		{
			packet = new GStruct199
			{
				SpawnNearPlayer = reader.ReadBool(),
				PlayerPosition = reader.ReadVector3()
			};
		}
		else
		{
			packet = null;
		}
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct198? packet)
	{
		writer.Write(packet.HasValue);
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct199? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			GStruct199 valueOrDefault = packet.GetValueOrDefault();
			writer.Write(valueOrDefault.SpawnNearPlayer);
			writer.Write(valueOrDefault.PlayerPosition);
		}
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct193? packet)
	{
		writer.Write(packet.HasValue);
	}

	public static void DeserializePacket(this IDataReader reader, ref GStruct193? packet)
	{
		if (reader.ReadBool())
		{
			packet = default(GStruct193);
		}
		else
		{
			packet = null;
		}
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct200? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			GStruct200 valueOrDefault = packet.GetValueOrDefault();
			writer.Write((byte)valueOrDefault.Part);
			writer.Write(valueOrDefault.Damage);
		}
	}

	public static void DeserializePacket(this IDataReader reader, ref GStruct200? packet)
	{
		if (reader.ReadBool())
		{
			packet = new GStruct200
			{
				Part = (EBodyPartColliderType)reader.ReadByte(),
				Damage = reader.ReadFloat()
			};
		}
		else
		{
			packet = null;
		}
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct201? packet)
	{
		writer.Write(packet.HasValue);
	}

	public static void DeserializePacket(this IDataReader reader, ref GStruct201? packet)
	{
		if (reader.ReadBool())
		{
			packet = default(GStruct201);
		}
		else
		{
			packet = null;
		}
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct204? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			writer.Write(packet.GetValueOrDefault().IsEncoded);
		}
	}

	public static void DeserializePacket(this IDataReader reader, ref GStruct204? packet)
	{
		if (reader.ReadBool())
		{
			packet = new GStruct204
			{
				IsEncoded = reader.ReadBool()
			};
		}
		else
		{
			packet = null;
		}
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct205? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			writer.Write(packet.GetValueOrDefault().Active);
		}
	}

	public static void DeserializePacket(this IDataReader reader, ref GStruct205? packet)
	{
		if (reader.ReadBool())
		{
			packet = new GStruct205
			{
				Active = reader.ReadBool()
			};
		}
		else
		{
			packet = null;
		}
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct238? packet)
	{
		writer.Write(packet.HasValue);
		if (!packet.HasValue)
		{
			return;
		}
		GStruct238 valueOrDefault = packet.GetValueOrDefault();
		writer.Write(valueOrDefault.ToggleTacticalCombo);
		if (valueOrDefault.ToggleTacticalCombo)
		{
			int value = valueOrDefault.TacticalComboStatuses.Length;
			writer.SerializeLimitedInt32(ref value, 0, 7, BitPackingTag.ToggleTacticalComboPacket0);
			for (int i = 0; i < value; i++)
			{
				GStruct239.Serialize(writer, ref valueOrDefault.TacticalComboStatuses[i]);
			}
		}
	}

	public static void DeserializePacket(this IDataReader reader, ref GStruct238? packet)
	{
		if (!reader.ReadBool())
		{
			packet = null;
			return;
		}
		GStruct238 value = default(GStruct238);
		bool flag = (value.ToggleTacticalCombo = reader.ReadBool());
		int value2 = 0;
		reader.SerializeLimitedInt32(ref value2, 0, 7, BitPackingTag.ToggleTacticalComboPacket0);
		if (flag)
		{
			value.TacticalComboStatuses = new GStruct239[value2];
			for (int i = 0; i < value2; i++)
			{
				GStruct239.Serialize(reader, ref value.TacticalComboStatuses[i]);
			}
		}
		packet = value;
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct237? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			GStruct237 valueOrDefault = packet.GetValueOrDefault();
			writer.Write(valueOrDefault.IsSilent);
			int value = valueOrDefault.LightStates.Length;
			writer.SerializeLimitedInt32(ref value, 0, 7, BitPackingTag.ToggleTacticalComboPacket0);
			for (int i = 0; i < value; i++)
			{
				FirearmLightStateStruct.Serialize(writer, ref valueOrDefault.LightStates[i]);
			}
		}
	}

	public static void DeserializePacket(this IDataReader reader, ref GStruct237? packet)
	{
		if (!reader.ReadBool())
		{
			packet = null;
			return;
		}
		GStruct237 value = default(GStruct237);
		bool isSilent = reader.ReadBool();
		value.IsSilent = isSilent;
		int value2 = 0;
		reader.SerializeLimitedInt32(ref value2, 0, 7, BitPackingTag.ToggleTacticalComboPacket0);
		value.LightStates = new FirearmLightStateStruct[value2];
		for (int i = 0; i < value2; i++)
		{
			FirearmLightStateStruct.Serialize(reader, ref value.LightStates[i]);
		}
		packet = value;
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct183? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			writer.Write(packet.GetValueOrDefault().ItemId);
		}
	}

	public static void DeserializePacket(this IDataReader reader, ref GStruct183? packet)
	{
		if (reader.ReadBool())
		{
			packet = new GStruct183
			{
				ItemId = reader.ReadString(1600u)
			};
		}
		else
		{
			packet = null;
		}
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct184? packet)
	{
		writer.Write(packet.HasValue);
	}

	public static void DeserializePacket(this IDataReader reader, ref GStruct184? packet)
	{
		packet = (reader.ReadBool() ? default(GStruct184) : default(GStruct184));
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct186? packet)
	{
		if (packet.HasValue)
		{
			GStruct186 valueOrDefault = packet.GetValueOrDefault();
			writer.Write(valueOrDefault.CutsceneSceneNameHash);
			writer.Write(valueOrDefault.CutsceneID);
			writer.Write(valueOrDefault.IsStarted);
			writer.Write(valueOrDefault.TransformPosition);
		}
	}

	public static void DeserializePacket(this IDataReader reader, ref GStruct186? packet)
	{
		if (reader.ReadBool())
		{
			packet = new GStruct186
			{
				CutsceneSceneNameHash = reader.ReadInt32(),
				CutsceneID = reader.ReadInt32(),
				IsStarted = reader.ReadBool(),
				TransformPosition = reader.ReadVector3()
			};
		}
		else
		{
			packet = null;
		}
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct185? packet)
	{
		if (packet.HasValue)
		{
			GStruct185 valueOrDefault = packet.GetValueOrDefault();
			writer.Write(valueOrDefault.origin);
			writer.Write(valueOrDefault.direction);
		}
	}

	public static void DeserializePacket(this IDataReader reader, ref GStruct185? packet)
	{
		if (reader.ReadBool())
		{
			packet = new GStruct185
			{
				origin = reader.ReadVector3(),
				direction = reader.ReadVector3()
			};
		}
		else
		{
			packet = null;
		}
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct387? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			GStruct387 valueOrDefault = packet.GetValueOrDefault();
			writer.Write(valueOrDefault.Iteration);
			writer.Write(valueOrDefault.Position);
			writer.Write(valueOrDefault.Reason);
		}
	}

	public static GStruct387? DeserializeSyncPositionPacket(this IDataReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		return new GStruct387
		{
			Iteration = reader.ReadByte(),
			Position = reader.ReadVector3(),
			Reason = reader.ReadEnum<SyncPositionReason>()
		};
	}

	public static GClass2244 DeserializeCommonPacket(this IDataReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		return GClass2244.Deserialize(reader);
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct389? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			GStruct389 valueOrDefault = packet.GetValueOrDefault();
			writer.Write(valueOrDefault.Hash);
			writer.Write(valueOrDefault.PrimarySlotHash);
			writer.Write(valueOrDefault.PrimarySlotMagHash);
			writer.Write(valueOrDefault.PrimarySlotChamberHash);
			writer.Write(valueOrDefault.SecondarySlotHash);
			writer.Write(valueOrDefault.SecondarySlotMagHash);
			writer.Write(valueOrDefault.SecondarySlotChamberHash);
			writer.Write(valueOrDefault.PistolSlotHash);
			writer.Write(valueOrDefault.PistolSlotMagHash);
			writer.Write(valueOrDefault.PistolSlotChamberHash);
			writer.Write(valueOrDefault.VestSlotHash);
			writer.Write(valueOrDefault.BackpackSlotHash);
			writer.Write(valueOrDefault.SafeContainerSlotHash);
			writer.Write(valueOrDefault.PocketsSlotHash);
		}
	}

	public static GStruct389? DeserializeInventoryHashPacket(this IDataReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		return new GStruct389
		{
			Hash = reader.ReadInt32(),
			PrimarySlotHash = reader.ReadInt32(),
			PrimarySlotMagHash = reader.ReadInt32(),
			PrimarySlotChamberHash = reader.ReadInt32(),
			SecondarySlotHash = reader.ReadInt32(),
			SecondarySlotMagHash = reader.ReadInt32(),
			SecondarySlotChamberHash = reader.ReadInt32(),
			PistolSlotHash = reader.ReadInt32(),
			PistolSlotMagHash = reader.ReadInt32(),
			PistolSlotChamberHash = reader.ReadInt32(),
			VestSlotHash = reader.ReadInt32(),
			BackpackSlotHash = reader.ReadInt32(),
			SafeContainerSlotHash = reader.ReadInt32(),
			PocketsSlotHash = reader.ReadInt32()
		};
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct390? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			writer.WriteBytesAndSize(packet.GetValueOrDefault().Bytes);
		}
	}

	public static GStruct390? DeserializeDeathInventorySyncPacket(this IDataReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		return new GStruct390
		{
			Bytes = reader.ReadBytesAlloc(1600u)
		};
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct269? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			GStruct269 valueOrDefault = packet.GetValueOrDefault();
			writer.Write(valueOrDefault.ClientShots.Length);
			for (int i = 0; i < valueOrDefault.ClientShots.Length; i++)
			{
				NetworkPlayer.ClientShot clientShot = valueOrDefault.ClientShots[i];
				writer.Write(clientShot.Approved);
				writer.Write(clientShot.BodyPart);
				writer.Write(clientShot.Damage);
				writer.Write(clientShot.LeftBodyPartHealth);
				writer.Write(clientShot.TargetName);
			}
			Serialize(writer, valueOrDefault.Nested, SerializePacket);
		}
	}

	public static GStruct269? DeserializeAcceptHitDebugDataPacket(this IDataReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		int num = reader.ReadInt32();
		NetworkPlayer.ClientShot[] array = new NetworkPlayer.ClientShot[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = new NetworkPlayer.ClientShot
			{
				Approved = reader.ReadBool(),
				BodyPart = reader.ReadInt32(),
				Damage = reader.ReadFloat(),
				LeftBodyPartHealth = reader.ReadFloat(),
				TargetName = reader.ReadString(1600u)
			};
		}
		return new GStruct269
		{
			ClientShots = array,
			Nested = Deserialize(reader, DeserializeAcceptHitDebugDataPacket)
		};
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct270? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			writer.Write(packet.Value.ProgressData.ConditionalIdHash);
			GStruct382<GStruct274> conditions = packet.Value.ProgressData.Conditions;
			writer.WriteLimitedInt32(conditions.Length, 0, 64, BitPackingTag.HandsChangePacket22);
			for (int i = 0; i < conditions.Length; i++)
			{
				GStruct274 condition = conditions[i];
				Serialize(writer, ref condition);
			}
			Serialize(writer, packet.Value.Nested, SerializePacket);
		}
	}

	public static GStruct270? DeserializeConditionValueChangedPacket(this IDataReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		int conditionalIdHash = reader.ReadInt32();
		int num = reader.ReadLimitedInt32(0, 64);
		GStruct382<GStruct274> conditions = default(GStruct382<GStruct274>);
		for (int i = 0; i < num; i++)
		{
			GStruct274 condition = default(GStruct274);
			Serialize(reader, ref condition);
			conditions.Add(condition);
		}
		return new GStruct270
		{
			ProgressData = new GStruct273
			{
				ConditionalIdHash = conditionalIdHash,
				Conditions = conditions
			},
			Nested = Deserialize(reader, DeserializeConditionValueChangedPacket)
		};
	}

	public static void Serialize(this ISerializer stream, ref GStruct274 condition)
	{
		GClass2064.Serialize(stream, ref condition.ConditionId);
		stream.Serialize(ref condition.Status);
		bool value = condition.Value <= 15.0;
		float value2 = (float)condition.Value;
		stream.Serialize(ref value);
		if (value)
		{
			stream.SerializeLimitedFloat(ref value2, -1f, 15f, 0.01f, BitPackingTag.ConditionValue1);
		}
		else
		{
			stream.SerializeLimitedFloat(ref value2, 15f, 5000000f, 0.01f, BitPackingTag.ConditionValue2);
		}
		stream.Serialize(ref condition.Notify);
		if (stream.StreamMode == EBitStreamMode.Reading)
		{
			condition.Value = value2;
		}
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct275? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			GStruct275 valueOrDefault = packet.GetValueOrDefault();
			writer.Write(valueOrDefault.LocalizationKey1);
			writer.Write(valueOrDefault.LocalizationKey2);
			writer.Write(valueOrDefault.Value);
			Serialize(writer, valueOrDefault.Nested, SerializePacket);
		}
	}

	public static GStruct275? DeserializeShowStatNotificationPacket(this IDataReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		return new GStruct275
		{
			LocalizationKey1 = reader.ReadUInt32(),
			LocalizationKey2 = reader.ReadUInt32(),
			Value = reader.ReadInt32(),
			Nested = Deserialize(reader, DeserializeShowStatNotificationPacket)
		};
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct276? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			GStruct276 valueOrDefault = packet.GetValueOrDefault();
			writer.Write(valueOrDefault.Aggressor);
			writer.Write(valueOrDefault.AggressorMainProfileName);
			writer.Write((byte)valueOrDefault.AggressorSide);
			writer.Write((byte)valueOrDefault.ColliderType);
			writer.Write(valueOrDefault.WeaponName);
			writer.Write((byte)valueOrDefault.MemberCategory);
			writer.Write((byte)valueOrDefault.Role);
		}
	}

	public static GStruct276? DeserializePlayerDiedPacket(this IDataReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		return new GStruct276
		{
			Aggressor = reader.ReadString(1600u),
			AggressorMainProfileName = reader.ReadString(1600u),
			AggressorSide = (EPlayerSide)reader.ReadByte(),
			ColliderType = (EBodyPartColliderType)reader.ReadByte(),
			WeaponName = reader.ReadString(1600u),
			MemberCategory = (EMemberCategory)reader.ReadByte(),
			Role = (WildSpawnType)reader.ReadByte()
		};
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct277? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			GStruct277 valueOrDefault = packet.GetValueOrDefault();
			writer.Write(valueOrDefault.CallbackId);
			writer.Write(valueOrDefault.Error);
			Serialize(writer, valueOrDefault.Nested, SerializePacket);
		}
	}

	public static GStruct277? DeserializeClientConfirmCallbackPacket(this IDataReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		return new GStruct277
		{
			CallbackId = reader.ReadUInt32(),
			Error = reader.ReadString(1600u),
			Nested = Deserialize(reader, DeserializeClientConfirmCallbackPacket)
		};
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct261? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			GStruct261 valueOrDefault = packet.GetValueOrDefault();
			writer.Write(valueOrDefault.SkillId);
			writer.Write(valueOrDefault.Value);
			writer.Write(valueOrDefault.Effectiveness);
			writer.Write(valueOrDefault.PointsEarned);
			Serialize(writer, valueOrDefault.Nested, SerializePacket);
		}
	}

	public static GStruct261? DeserializeChangeSkillExperiencePacket(this IDataReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		return new GStruct261
		{
			SkillId = reader.ReadByte(),
			Value = reader.ReadFloat(),
			Effectiveness = reader.ReadFloat(),
			PointsEarned = reader.ReadFloat(),
			Nested = Deserialize(reader, DeserializeChangeSkillExperiencePacket)
		};
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct262? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			GStruct262 valueOrDefault = packet.GetValueOrDefault();
			writer.Write(valueOrDefault.GroupName);
			writer.Write(valueOrDefault.Value);
			Serialize(writer, valueOrDefault.Nested, SerializePacket);
		}
	}

	public static GStruct262? DeserializeChangeMasteringLevelPacket(this IDataReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		return new GStruct262
		{
			GroupName = reader.ReadString(1600u),
			Value = reader.ReadFloat(),
			Nested = Deserialize(reader, DeserializeChangeMasteringLevelPacket)
		};
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct263? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			writer.Write(packet.GetValueOrDefault().Value);
		}
	}

	public static GStruct263? DeserializeSwitchRenderersPacket(this IDataReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		return new GStruct263
		{
			Value = reader.ReadBool()
		};
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct264? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			GStruct264 valueOrDefault = packet.GetValueOrDefault();
			writer.Write(valueOrDefault.TraderId);
			writer.Write(valueOrDefault.Standing);
			Serialize(writer, valueOrDefault.Nested, SerializePacket);
		}
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct265? packet)
	{
		writer.Write(value: false);
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct266? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			GStruct266 valueOrDefault = packet.GetValueOrDefault();
			writer.Write(valueOrDefault.IsEncoded);
			writer.Write(valueOrDefault.Status);
			writer.Write(valueOrDefault.IsAggressor);
		}
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct179? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			GStruct179 valueOrDefault = packet.GetValueOrDefault();
			writer.Write(valueOrDefault.AllowedPlayers.Length);
			for (int i = 0; i < valueOrDefault.AllowedPlayers.Length; i++)
			{
				GStruct434 gStruct = valueOrDefault.AllowedPlayers[i];
				writer.Write(gStruct.Nickname);
				writer.Write(gStruct.Status);
			}
			writer.Write(valueOrDefault.UnallowedPlayers.Length);
			for (int j = 0; j < valueOrDefault.UnallowedPlayers.Length; j++)
			{
				GStruct434 gStruct2 = valueOrDefault.UnallowedPlayers[j];
				writer.Write(gStruct2.Nickname);
				writer.Write(gStruct2.Status);
			}
		}
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct267? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			writer.Write(packet.GetValueOrDefault().Active);
		}
	}

	public static GStruct264? DeserializeTradersInfoPacket(this IDataReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		return new GStruct264
		{
			TraderId = reader.ReadString(1600u),
			Standing = reader.ReadDouble(),
			Nested = Deserialize(reader, DeserializeTradersInfoPacket)
		};
	}

	public static GStruct265? DeserializeStringNotificationPacket(this IDataReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		return new GStruct265
		{
			Message = reader.ReadString(1600u),
			Nested = Deserialize(reader, DeserializeStringNotificationPacket)
		};
	}

	public static GStruct266? DeserializeRadioTransmitterPacket(this IDataReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		return new GStruct266
		{
			IsEncoded = reader.ReadBool(),
			Status = reader.ReadEnum<RadioTransmitterStatus>(),
			IsAggressor = reader.ReadBool()
		};
	}

	public static GStruct179? DeserializeLighthouseTraderZonePacket(this IDataReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		int num = reader.ReadInt32();
		GStruct434[] array = new GStruct434[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = new GStruct434
			{
				Nickname = reader.ReadString(1600u),
				Status = reader.ReadEnum<RadioTransmitterStatus>()
			};
		}
		int num2 = reader.ReadInt32();
		GStruct434[] array2 = new GStruct434[num2];
		for (int j = 0; j < num2; j++)
		{
			array2[j] = new GStruct434
			{
				Nickname = reader.ReadString(1600u),
				Status = reader.ReadEnum<RadioTransmitterStatus>()
			};
		}
		return new GStruct179
		{
			AllowedPlayers = array,
			UnallowedPlayers = array2
		};
	}

	public static GStruct267? DeserializeighthouseTraderZoneDebugToolPacket(this IDataReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		return new GStruct267
		{
			Active = reader.ReadBool()
		};
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct278? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			GStruct278 valueOrDefault = packet.GetValueOrDefault();
			writer.Write(valueOrDefault.WeaponId);
			writer.Write(valueOrDefault.LastShotTime);
			writer.Write(valueOrDefault.LastOverheat);
			writer.Write(valueOrDefault.SlideOnOverheatReached);
		}
	}

	public static GStruct278? DeserializeWeaponOverheatPacket(this IDataReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		return new GStruct278
		{
			WeaponId = reader.ReadString(1600u),
			LastShotTime = reader.ReadFloat(),
			LastOverheat = reader.ReadFloat(),
			SlideOnOverheatReached = reader.ReadBool()
		};
	}

	public static void SerializePacket(this GInterface131 writer, ref PlayerInteractPacket? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			PlayerInteractPacket valueOrDefault = packet.GetValueOrDefault();
			writer.Write(valueOrDefault.HasInteraction);
			writer.Write((byte)valueOrDefault.InteractionType);
			writer.Write(valueOrDefault.SideId);
			writer.Write(valueOrDefault.SlotId);
			writer.Write(valueOrDefault.Fast);
		}
	}

	public static PlayerInteractPacket? DeserializeInteractWithBtrPacket(this IDataReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		return new PlayerInteractPacket
		{
			HasInteraction = reader.ReadBool(),
			InteractionType = (EInteractionType)reader.ReadByte(),
			SideId = reader.ReadByte(),
			SlotId = reader.ReadByte(),
			Fast = reader.ReadBool()
		};
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct216? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			GStruct216 valueOrDefault = packet.GetValueOrDefault();
			writer.Write(valueOrDefault.HasInteraction);
			writer.Write(valueOrDefault.Id);
		}
	}

	public static GStruct216? DeserializeInteractWithTripwirePacket(this IDataReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		return new GStruct216
		{
			HasInteraction = reader.ReadBool(),
			Id = reader.ReadInt32()
		};
	}
}
