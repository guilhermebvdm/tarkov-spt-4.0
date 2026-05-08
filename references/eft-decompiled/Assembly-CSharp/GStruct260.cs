using System;
using System.Collections.Generic;
using EFT;
using EFT.Ballistics;
using EFT.NetworkPackets;
using JetBrains.Annotations;

public struct GStruct260
{
	public const int PROCESSED_CRITICAL_PACKETS_COMMON_MAX_COUNT = 7;

	[NonSerialized]
	public const int Int_0 = 2047;

	[NonSerialized]
	public const int Int_1 = 32;

	[NonSerialized]
	public const int Int_2 = 32;

	[NonSerialized]
	public const int Int_3 = 16;

	[NonSerialized]
	public const int Int_4 = 512;

	[NonSerialized]
	public const int Int_5 = 32;

	public ushort? RTT;

	public int ServerFixedUpdate;

	public int ServerTime;

	public float ServerWorldTime;

	public List<GStruct377> HitInfos;

	public List<GStruct370> DetailedHitInfo;

	public List<GStruct366> ArmorUpdates;

	public List<GStruct386> PoisonUpdates;

	public List<GStruct378> OperationStatuses;

	public List<BaseDescriptorClass> GlobalOperations;

	public GClass2244 CommonPacket;

	public NetworkHealthSyncPacketStruct? SyncHealthPacket;

	public GStruct269? AcceptHitDebugDataPacket;

	public GStruct270? QuestConditionValueChangedPacket;

	public GStruct270? AchievementConditionValueChangedPacket;

	public GStruct270? PrestigeConditionValueChangedPacket;

	public GStruct275? ShowStatNotificationPacket;

	public GStruct276? PlayerDiedPacket;

	public GStruct277? ClientConfirmCallbackPacket;

	public GStruct278? WeaponOverheatPacket;

	public GStruct261? ChangeSkillExperiencePacket;

	public GStruct262? ChangeMasteringExperiencePacket;

	public GStruct264? TradersInfoPacket;

	public GStruct265? StringNotificationPacket;

	public GStruct266? RadioTransmitterPacket;

	public GStruct179? LighthouseTraderZoneDataPacket;

	public GStruct267? LighthouseTraderZoneDebugToolPacket;

	public PlayerInteractPacket? InteractWithBtrPacket;

	public GStruct216? InteractWithTripwirePacket;

	public bool TalkDetected;

	public int CriticalPacketsProcessed;

	[NonSerialized]
	public bool Bool_0;

	public override string ToString()
	{
		return string.Format("{0}: {1}\n", "RTT", RTT) + string.Format("{0}: {1}\n", "ServerFixedUpdate", ServerFixedUpdate) + string.Format("{0}: {1}\n", "ServerTime", ServerTime) + string.Format("{0}: {1}\n", "ServerWorldTime", ServerWorldTime) + string.Format("{0}: {1}\n", "_hasData", Bool_0) + string.Format("{0}: {1}\n", "HitInfos", HitInfos?.Count ?? 0) + string.Format("{0}: {1}\n", "DetailedHitInfo", DetailedHitInfo?.Count ?? 0) + string.Format("{0}: {1}\n", "ArmorUpdates", ArmorUpdates?.Count ?? 0) + string.Format("{0}: {1}\n", "PoisonUpdates", PoisonUpdates?.Count ?? 0) + string.Format("{0}: {1}\n", "OperationStatuses", OperationStatuses?.Count ?? 0) + string.Format("{0}: {1}\n", "GlobalOperations", GlobalOperations?.Count ?? 0) + string.Format("{0}: {1}\n", "CommonPacket", CommonPacket) + string.Format("{0}: {1}\n", "SyncHealthPacket", SyncHealthPacket) + string.Format("{0}: {1}\n", "AcceptHitDebugDataPacket", AcceptHitDebugDataPacket) + string.Format("{0}: {1}\n", "QuestConditionValueChangedPacket", QuestConditionValueChangedPacket) + string.Format("{0}: {1}\n", "AchievementConditionValueChangedPacket", AchievementConditionValueChangedPacket) + string.Format("{0}: {1}\n", "PrestigeConditionValueChangedPacket", PrestigeConditionValueChangedPacket) + string.Format("{0}: {1}\n", "ShowStatNotificationPacket", ShowStatNotificationPacket) + string.Format("{0}: {1}\n", "PlayerDiedPacket", PlayerDiedPacket) + string.Format("{0}: {1}\n", "ClientConfirmCallbackPacket", ClientConfirmCallbackPacket) + string.Format("{0}: {1}\n", "WeaponOverheatPacket", WeaponOverheatPacket) + string.Format("{0}: {1}\n", "ChangeSkillExperiencePacket", ChangeSkillExperiencePacket) + string.Format("{0}: {1}\n", "ChangeMasteringExperiencePacket", ChangeMasteringExperiencePacket) + string.Format("{0}: {1}\n", "TradersInfoPacket", TradersInfoPacket) + string.Format("{0}: {1}\n", "StringNotificationPacket", StringNotificationPacket) + "RadioTransmitterPacket:" + $"{RadioTransmitterPacket}\n" + string.Format("{0}: {1}\n", "TalkDetected", TalkDetected) + string.Format("{0}: {1}\n", "CriticalPacketsProcessed", CriticalPacketsProcessed);
	}

	public bool HasData()
	{
		if (!Bool_0 && CommonPacket == null && !SyncHealthPacket.HasValue && !AcceptHitDebugDataPacket.HasValue && !QuestConditionValueChangedPacket.HasValue && !AchievementConditionValueChangedPacket.HasValue && !PrestigeConditionValueChangedPacket.HasValue && !ShowStatNotificationPacket.HasValue && !PlayerDiedPacket.HasValue && !ClientConfirmCallbackPacket.HasValue && !WeaponOverheatPacket.HasValue && !TradersInfoPacket.HasValue && !StringNotificationPacket.HasValue && !RadioTransmitterPacket.HasValue && !LighthouseTraderZoneDataPacket.HasValue && !LighthouseTraderZoneDebugToolPacket.HasValue && !InteractWithBtrPacket.HasValue && !InteractWithTripwirePacket.HasValue && !RTT.HasValue)
		{
			return CriticalPacketsProcessed > 0;
		}
		return true;
	}

	public void FlushSkills()
	{
		Bool_0 = Bool_0 || ChangeSkillExperiencePacket.HasValue || ChangeMasteringExperiencePacket.HasValue;
	}

	public void AddChangeSkillExperiencePacket(byte skillId, float value, float effectiveness, float pointsEarned)
	{
		GStruct261 child = new GStruct261
		{
			SkillId = skillId,
			Value = value,
			Effectiveness = effectiveness,
			PointsEarned = pointsEarned
		};
		GClass2243.AttachToOrUpdate(ref child, ref ChangeSkillExperiencePacket);
	}

	public void AddChangeMasteringExperiencePacket(string groupName, float value)
	{
		GStruct262 child = new GStruct262
		{
			GroupName = groupName,
			Value = value
		};
		GClass2243.AttachToOrUpdate(ref child, ref ChangeMasteringExperiencePacket);
	}

	public void AddTraderInfoPacket(string traderId, double standing)
	{
		GClass2243.AttachTo(new GStruct264
		{
			TraderId = traderId,
			Standing = standing
		}, ref TradersInfoPacket);
	}

	public void AddStringNotificationPacket(string message)
	{
		GClass2243.AttachTo(new GStruct265
		{
			Message = message
		}, ref StringNotificationPacket);
	}

	public void AddClientRadioTransmitterPacket(bool isEncoded, RadioTransmitterStatus status, bool isAgressor)
	{
		GClass2243.AttachTo(new GStruct266
		{
			IsEncoded = isEncoded,
			Status = status,
			IsAggressor = isAgressor
		}, ref RadioTransmitterPacket);
	}

	public void AddLighthouseTraderZoneDebugToolPacket(bool isActive)
	{
		GClass2243.AttachTo(new GStruct267
		{
			Active = isActive
		}, ref LighthouseTraderZoneDebugToolPacket);
	}

	public void AddAcceptHitDebugDataPacket(NetworkPlayer.ClientShot[] clientShots)
	{
		GClass2243.AttachTo(new GStruct269
		{
			ClientShots = clientShots
		}, ref AcceptHitDebugDataPacket);
	}

	public void AddQuestConditionValueChangedPacket(GStruct273 conditionalData)
	{
		GClass2243.AttachTo(new GStruct270
		{
			ProgressData = conditionalData
		}, ref QuestConditionValueChangedPacket);
	}

	public void AddAchievementConditionValueChangedPacket(GStruct273 conditionalData)
	{
		GClass2243.AttachTo(new GStruct270
		{
			ProgressData = conditionalData
		}, ref AchievementConditionValueChangedPacket);
	}

	public void AddPrestigeConditionValueChangedPacket(GStruct273 conditionalData)
	{
		GClass2243.AttachTo(new GStruct270
		{
			ProgressData = conditionalData
		}, ref PrestigeConditionValueChangedPacket);
	}

	public void AddShowStatNotificationPacket(uint localizationKey1, uint localizationKey2, int value)
	{
		GClass2243.AttachTo(new GStruct275
		{
			LocalizationKey1 = localizationKey1,
			LocalizationKey2 = localizationKey2,
			Value = value
		}, ref ShowStatNotificationPacket);
	}

	public void AddPlayerDiedPacket(string aggressor, EPlayerSide aggressorSide, EBodyPartColliderType colliderType, string weaponName, EMemberCategory memberCategory, string aggressorMainCharacterName, WildSpawnType role, int prestigeLevel)
	{
		PlayerDiedPacket = new GStruct276
		{
			Aggressor = aggressor,
			AggressorSide = aggressorSide,
			ColliderType = colliderType,
			WeaponName = weaponName,
			MemberCategory = memberCategory,
			AggressorMainProfileName = aggressorMainCharacterName,
			Role = role,
			PrestigeLevel = prestigeLevel
		};
	}

	public void AddClientConfirmCallbackPacket(uint callbackId, [CanBeNull] string error)
	{
		GClass2243.AttachTo(new GStruct277
		{
			CallbackId = callbackId,
			Error = error
		}, ref ClientConfirmCallbackPacket);
	}

	public void AddWeaponOverheatPacket(string weaponId, float lastShotTime, float lastOverheat, bool slideReached)
	{
		GClass2243.AttachTo(new GStruct278
		{
			WeaponId = weaponId,
			LastShotTime = lastShotTime,
			LastOverheat = lastOverheat,
			SlideOnOverheatReached = slideReached
		}, ref WeaponOverheatPacket);
	}

	public void AddSyncHealthPacket(NetworkHealthSyncPacketStruct packet)
	{
		GClass2243.AttachTo(packet, ref SyncHealthPacket);
	}

	public void AddHitInfo(GStruct377 hitInfo)
	{
		Bool_0 = true;
		if (HitInfos == null)
		{
			HitInfos = new List<GStruct377>(2);
		}
		HitInfos.Add(hitInfo);
	}

	public void AddDetailedHitInfo(EDamageType damageType, int damage, int absorbed, int staminaLoss, EBodyPart part, MaterialType special)
	{
		method_0(new GStruct370
		{
			DamageType = damageType,
			Damage = damage,
			Absorbed = absorbed,
			StaminaLoss = staminaLoss,
			Part = part,
			Special = special switch
			{
				MaterialType.GlassVisor => EHitSpecial.Visor, 
				MaterialType.Helmet => EHitSpecial.Helmet, 
				MaterialType.HelmetRicochet => EHitSpecial.Ricochet, 
				_ => EHitSpecial.None, 
			}
		});
	}

	public void method_0(GStruct370 hitInfo)
	{
		Bool_0 = true;
		if (DetailedHitInfo == null)
		{
			DetailedHitInfo = new List<GStruct370>(2);
		}
		DetailedHitInfo.Add(hitInfo);
	}

	public void UpdateArmor(GStruct366 a)
	{
		Bool_0 = true;
		if (ArmorUpdates == null)
		{
			ArmorUpdates = new List<GStruct366>(1);
		}
		for (int i = 0; i < ArmorUpdates.Count; i++)
		{
			if (!(ArmorUpdates[i].Id != a.Id))
			{
				ArmorUpdates.RemoveAt(i);
				break;
			}
		}
		ArmorUpdates.Add(a);
	}

	public void UpdateSideEffectResource(GStruct386 update)
	{
		Bool_0 = true;
		if (PoisonUpdates == null)
		{
			PoisonUpdates = new List<GStruct386>(1);
		}
		for (int i = 0; i < PoisonUpdates.Count; i++)
		{
			if (!(PoisonUpdates[i].Id != update.Id))
			{
				PoisonUpdates.RemoveAt(i);
				break;
			}
		}
		PoisonUpdates.Add(update);
	}

	public void AddOperationStatus(GStruct378 operationStatus)
	{
		Bool_0 = true;
		if (OperationStatuses == null)
		{
			OperationStatuses = new List<GStruct378>(2);
		}
		OperationStatuses.Add(operationStatus);
	}

	public void AddGlobalOperation(BaseDescriptorClass descriptor)
	{
		if (descriptor != null)
		{
			Bool_0 = true;
			if (GlobalOperations == null)
			{
				GlobalOperations = new List<BaseDescriptorClass>(2);
			}
			GlobalOperations.Add(descriptor);
		}
	}

	public static void Deserialize(IDataReader reader, ref GStruct260 selfPlayerInfo)
	{
		if (reader.ReadBool())
		{
			selfPlayerInfo.RTT = reader.ReadUInt16();
			selfPlayerInfo.ServerFixedUpdate = reader.ReadLimitedInt32(0, 512);
			selfPlayerInfo.ServerTime = reader.ReadLimitedInt32(0, 512);
		}
		selfPlayerInfo.ServerWorldTime = reader.ReadFloat();
		bool num = reader.ReadBool();
		bool flag = reader.ReadBool();
		bool flag2 = reader.ReadBool();
		bool flag3 = reader.ReadBool();
		bool flag4 = reader.ReadBool();
		bool flag5 = reader.ReadBool();
		if (num)
		{
			int num2 = reader.ReadLimitedInt32(1, 16);
			for (int i = 0; i < num2; i++)
			{
				GStruct377 hitInfo = default(GStruct377);
				GClass3005.Serialize(reader, ref hitInfo);
				selfPlayerInfo.AddHitInfo(hitInfo);
			}
		}
		if (flag)
		{
			int num3 = reader.ReadLimitedInt32(1, 32);
			for (int j = 0; j < num3; j++)
			{
				GStruct366 armorUpdate = default(GStruct366);
				GClass3003.Serialize(reader, ref armorUpdate);
				selfPlayerInfo.UpdateArmor(armorUpdate);
			}
		}
		if (flag2)
		{
			int num4 = reader.ReadLimitedInt32(1, 16);
			for (int k = 0; k < num4; k++)
			{
				GStruct386 sideEffectUpdate = default(GStruct386);
				GClass3004.Serialize(reader, ref sideEffectUpdate);
				selfPlayerInfo.UpdateSideEffectResource(sideEffectUpdate);
			}
		}
		if (flag3)
		{
			int num5 = reader.ReadLimitedInt32(1, 16);
			for (int l = 0; l < num5; l++)
			{
				GStruct370 dHitInfo = default(GStruct370);
				GClass3007.Serialize(reader, ref dHitInfo);
				selfPlayerInfo.method_0(dHitInfo);
			}
		}
		if (flag4)
		{
			int num6 = reader.ReadLimitedInt32(1, 32);
			for (int m = 0; m < num6; m++)
			{
				GStruct378 operationStatus = default(GStruct378);
				GClass3006.Serialize(reader, ref operationStatus);
				selfPlayerInfo.AddOperationStatus(operationStatus);
			}
		}
		if (flag5)
		{
			int num7 = reader.ReadLimitedInt32(1, 32);
			for (int n = 0; n < num7; n++)
			{
				int num8 = reader.ReadInt32();
				byte[] array = new byte[num8];
				reader.ReadBytes(array, 0, num8);
				BaseDescriptorClass descriptor = GClass3695.ReadPolymorph<BaseDescriptorClass>(new EFTReaderClass(array));
				selfPlayerInfo.AddGlobalOperation(descriptor);
			}
		}
		selfPlayerInfo.CommonPacket = GClass2084.DeserializeCommonPacket(reader);
		selfPlayerInfo.SyncHealthPacket = GClass3061.DeserializeSyncHealthPacket(reader);
		selfPlayerInfo.AcceptHitDebugDataPacket = GClass2084.DeserializeAcceptHitDebugDataPacket(reader);
		selfPlayerInfo.QuestConditionValueChangedPacket = GClass2084.DeserializeConditionValueChangedPacket(reader);
		selfPlayerInfo.AchievementConditionValueChangedPacket = GClass2084.DeserializeConditionValueChangedPacket(reader);
		selfPlayerInfo.PrestigeConditionValueChangedPacket = GClass2084.DeserializeConditionValueChangedPacket(reader);
		selfPlayerInfo.ShowStatNotificationPacket = GClass2084.DeserializeShowStatNotificationPacket(reader);
		selfPlayerInfo.PlayerDiedPacket = GClass2084.DeserializePlayerDiedPacket(reader);
		selfPlayerInfo.ClientConfirmCallbackPacket = GClass2084.DeserializeClientConfirmCallbackPacket(reader);
		selfPlayerInfo.WeaponOverheatPacket = GClass2084.DeserializeWeaponOverheatPacket(reader);
		selfPlayerInfo.ChangeSkillExperiencePacket = GClass2084.DeserializeChangeSkillExperiencePacket(reader);
		selfPlayerInfo.ChangeMasteringExperiencePacket = GClass2084.DeserializeChangeMasteringLevelPacket(reader);
		selfPlayerInfo.TradersInfoPacket = GClass2084.DeserializeTradersInfoPacket(reader);
		selfPlayerInfo.StringNotificationPacket = GClass2084.DeserializeStringNotificationPacket(reader);
		selfPlayerInfo.RadioTransmitterPacket = GClass2084.DeserializeRadioTransmitterPacket(reader);
		selfPlayerInfo.LighthouseTraderZoneDataPacket = GClass2084.DeserializeLighthouseTraderZonePacket(reader);
		selfPlayerInfo.LighthouseTraderZoneDebugToolPacket = GClass2084.DeserializeighthouseTraderZoneDebugToolPacket(reader);
		selfPlayerInfo.InteractWithBtrPacket = GClass2084.DeserializeInteractWithBtrPacket(reader);
		selfPlayerInfo.InteractWithTripwirePacket = GClass2084.DeserializeInteractWithTripwirePacket(reader);
		reader.Serialize(ref selfPlayerInfo.TalkDetected);
		if (reader.ReadBool())
		{
			selfPlayerInfo.CriticalPacketsProcessed = reader.ReadLimitedInt32(0, 7);
		}
		else
		{
			selfPlayerInfo.CriticalPacketsProcessed = reader.ReadLimitedInt32(0, 2047);
		}
	}
}
