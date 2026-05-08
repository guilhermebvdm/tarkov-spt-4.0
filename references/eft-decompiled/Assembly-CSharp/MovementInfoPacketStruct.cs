using System;
using BitPacking;
using UnityEngine;

public struct MovementInfoPacketStruct
{
	[NonSerialized]
	public const int Int_0 = 31;

	[NonSerialized]
	public const int Int_1 = 70;

	[NonSerialized]
	public const int Int_2 = 8;

	[NonSerialized]
	public const float Float_0 = 6f;

	public const float USUAL_POSITION_X_RESOLUTION = 0.001953125f;

	public const float USUAL_POSITION_Y_RESOLUTION = 0.0009765625f;

	public const float USUAL_POSITION_Z_RESOLUTION = 0.001953125f;

	public const float PRECISE_POSITION_RESOLUTION_MULTIPLIER = 0.1f;

	public const float MOVEMENT_DIRECTION_RESOLUTION = 1f / 64f;

	[NonSerialized]
	public static GClass1378 Gclass1378_0 = new GClass1378(-6f, 6f, 0.001953125f, -6f, 6f, 0.0009765625f, -6f, 6f, 0.001953125f);

	[NonSerialized]
	public static GClass1378 Gclass1378_1 = new GClass1378(-6f, 6f, 0.0001953125f, -6f, 6f, 9.765625E-05f, -6f, 6f, 0.0001953125f);

	[NonSerialized]
	public static GClass1378 Gclass1378_2 = new GClass1378(-1f, 1f, 1f / 64f, -1f, 1f, 1f / 64f, -1f, 1f, 1f / 64f);

	[NonSerialized]
	public const float Float_1 = 9.536743E-07f;

	[NonSerialized]
	public const int Int_3 = 8;

	[NonSerialized]
	public static GClass1375 Gclass1375_0 = new GClass1375(0f, 360f, 1f / 64f, -90f, 90f, 1f / 64f);

	[NonSerialized]
	public static GStruct136 Gstruct136_0 = new GStruct136(0f, 1f, 1f / 128f, BitPackingTag.PoseLevelSettings);

	[NonSerialized]
	public static GStruct136 Gstruct136_1 = new GStruct136(0f, 1f, 1f / 128f, BitPackingTag.SpeedSettings);

	[NonSerialized]
	public static GStruct136 Gstruct136_2 = new GStruct136(-5f, 5f, 1f / 128f, BitPackingTag.TiltSettings);

	public static GClass1375 MOVEMENT_DIRECTION_QUANTIZER = new GClass1375(-1f, 1f, 1f / 64f, -1f, 1f, 1f / 64f);

	public bool SyncPositionApplied;

	public EPlayerState EPlayerState;

	public int AnimatorStateIndex;

	public int DiscreteDirection;

	public float PoseLevel;

	public float CharacterMovementSpeed;

	public float Tilt;

	public int Step;

	public int BlindFire;

	public bool SoftSurface;

	public bool LeftStance;

	public BasePhysicalClass.PhysicalStateStruct Stamina;

	public GStruct215 InteractWithDoorPacket;

	public PlayerInteractPacket InteractWithBtrPacket;

	public GStruct216 InteractWithTripwirePacket;

	public TransitInteractionPacketStruct InteractWithTransitPacket;

	public InteractPacketStruct InteractWithEventObjectPacket;

	public GStruct228 LootInteractionPacket;

	public StationaryPacketStruct StationaryWeaponPacket;

	public GStruct221 PlantItemPacket;

	public Vector3 HeadRotation;

	public bool IsGrounded;

	public Vector3 SurfaceNormal;

	public Vector3 PlayerSurfaceUpAlignNormal;

	public bool FullPositionSync;

	public GStruct223 VaultingPacket;

	public GStruct225 InteractionPacket;

	public GStruct226 InventoryInteractionPacket;

	public MountingPacketStruct MountingPacket;

	public GStruct224 LeftHandPacket;

	public static GClass1378 USUAL_POSITION_QUANTIZER => GClass1803.UsualPositionQuantizer;

	public static GClass1378 PRECISE_POSITION_QUANTIZER => GClass1803.PrecisePositionQuantizer;

	public bool HasImportantData
	{
		get
		{
			if (!InteractWithDoorPacket.HasInteraction && !InteractWithBtrPacket.HasInteraction && !InteractWithTripwirePacket.HasInteraction && !InteractWithTransitPacket.hasInteraction && !InteractWithEventObjectPacket.hasInteraction && !LootInteractionPacket.Interact && !StationaryWeaponPacket.HasInteraction && string.IsNullOrEmpty(PlantItemPacket.ItemId) && !SyncPositionApplied && !(Mathf.Abs(Tilt) > 0f) && Step == 0)
			{
				return BlindFire != 0;
			}
			return true;
		}
	}

	public void ClearCommandRelatedData()
	{
		SyncPositionApplied = false;
		InteractWithDoorPacket = default(GStruct215);
		InteractWithBtrPacket = default(PlayerInteractPacket);
		InteractWithTripwirePacket = default(GStruct216);
		InteractWithTransitPacket = default(TransitInteractionPacketStruct);
		InteractWithEventObjectPacket = default(InteractPacketStruct);
		LootInteractionPacket = default(GStruct228);
		StationaryWeaponPacket = default(StationaryPacketStruct);
		PlantItemPacket = default(GStruct221);
	}

	public override string ToString()
	{
		return string.Format($"TransformPosition: EPlayerState: {EPlayerState}, PoseLevel: {PoseLevel}, CharacterMovementSpeed: {CharacterMovementSpeed}");
	}

	public MovementInfoPacketStruct(EPlayerState ePlayerState, int animatorStateIndex, float poseLevel, float characterMovementSpeed, float tilt, int step, int blindFire, GStruct215 interactWithDoorPacket, PlayerInteractPacket interactWithBtrPacket, GStruct216 interactWithTripwirePacket, TransitInteractionPacketStruct interactWithTransitPacket, InteractPacketStruct interactWithEventObjectPacket, GStruct228 lootInteractionPacket, StationaryPacketStruct stationaryWeaponPacket, GStruct221 plantItemPacket, bool softSurface, bool leftStance, Vector3 headRotation, BasePhysicalClass.PhysicalStateStruct stamina, bool syncPositionApplied, int discreteMovementDirection, bool isGrounded, Vector3 surfaceNormal, Vector3 playerSurfaceUpAlignNormal, bool autoVaultingSettingEnable, GStruct223 vaultingPacket, GStruct225 interactionPacket, GStruct226 inventoryInteractionPacket, MountingPacketStruct mountingPacket, GStruct224 leftHandPacket)
	{
		this = default(MovementInfoPacketStruct);
		EPlayerState = ePlayerState;
		AnimatorStateIndex = animatorStateIndex;
		PoseLevel = poseLevel;
		CharacterMovementSpeed = characterMovementSpeed;
		Tilt = tilt;
		Step = step;
		BlindFire = blindFire;
		InteractWithDoorPacket = interactWithDoorPacket;
		InteractWithBtrPacket = interactWithBtrPacket;
		InteractWithTripwirePacket = interactWithTripwirePacket;
		InteractWithTransitPacket = interactWithTransitPacket;
		InteractWithEventObjectPacket = interactWithEventObjectPacket;
		LootInteractionPacket = lootInteractionPacket;
		StationaryWeaponPacket = stationaryWeaponPacket;
		PlantItemPacket = plantItemPacket;
		SoftSurface = softSurface;
		LeftStance = leftStance;
		HeadRotation = headRotation;
		Stamina = stamina;
		SyncPositionApplied = syncPositionApplied;
		DiscreteDirection = discreteMovementDirection;
		IsGrounded = isGrounded;
		SurfaceNormal = surfaceNormal;
		PlayerSurfaceUpAlignNormal = playerSurfaceUpAlignNormal;
		VaultingPacket = vaultingPacket;
		InteractionPacket = interactionPacket;
		InventoryInteractionPacket = inventoryInteractionPacket;
		MountingPacket = mountingPacket;
		LeftHandPacket = leftHandPacket;
	}

	public static void SerializeDiffUsing(GInterface131 writer, ref MovementInfoPacketStruct current, MovementInfoPacketStruct prevFrame, bool isExtraPrecisionMovement = false)
	{
		writer.Write(current.SyncPositionApplied);
		bool flag = prevFrame.EPlayerState != current.EPlayerState;
		writer.Write(flag);
		if (flag)
		{
			int ePlayerState = (int)current.EPlayerState;
			writer.WriteLimitedInt32(ePlayerState, 0, 31, BitPackingTag.MovementInfoPacket0);
		}
		bool flag2 = prevFrame.AnimatorStateIndex != current.AnimatorStateIndex;
		writer.Write(flag2);
		if (flag2)
		{
			int animatorStateIndex = current.AnimatorStateIndex;
			writer.WriteLimitedInt32(animatorStateIndex, 0, 70, BitPackingTag.MovementInfoPacket1);
		}
		bool flag3 = prevFrame.DiscreteDirection != current.DiscreteDirection;
		writer.Write(flag3);
		if (flag3)
		{
			writer.SerializeLimitedInt32(ref current.DiscreteDirection, 0, 8, BitPackingTag.MovementInfoPacket1);
		}
		bool flag4 = Math.Abs(prevFrame.PoseLevel - current.PoseLevel) > Mathf.Epsilon;
		writer.Write(flag4);
		if (flag4)
		{
			GClass1374.Serialize(writer, ref current.PoseLevel, Gstruct136_0);
		}
		bool flag5 = Math.Abs(prevFrame.CharacterMovementSpeed - current.CharacterMovementSpeed) > Mathf.Epsilon;
		writer.Write(flag5);
		if (flag5)
		{
			GClass1374.Serialize(writer, ref current.CharacterMovementSpeed, Gstruct136_1);
		}
		else
		{
			current.CharacterMovementSpeed = prevFrame.CharacterMovementSpeed;
		}
		bool flag6 = Math.Abs(prevFrame.Tilt - current.Tilt) > Mathf.Epsilon;
		writer.Write(flag6);
		if (flag6)
		{
			GClass1374.Serialize(writer, ref current.Tilt, Gstruct136_2);
		}
		bool flag7 = Math.Abs(prevFrame.Step - current.Step) > 0;
		writer.Write(flag7);
		if (flag7)
		{
			int num = Math.Sign(current.Step);
			if (num == 0)
			{
				writer.Write(value: true);
			}
			else
			{
				writer.Write(value: false);
				writer.Write(num > 0);
			}
		}
		writer.WriteLimitedInt32(current.BlindFire, -1, 1, BitPackingTag.MovementInfoPacket3);
		writer.Write(current.SoftSurface);
		writer.Write(current.LeftStance);
		bool flag8 = GClass2298.AllVectorAbsComponentsLess(current.HeadRotation - prevFrame.HeadRotation, 0.1f);
		writer.Write(flag8);
		if (!flag8)
		{
			writer.WriteLimitedFloat(current.HeadRotation.x, -50f, 50f, 0.0625f, BitPackingTag.HeadRotationX);
			writer.WriteLimitedFloat(current.HeadRotation.y, -50f, 50f, 0.0625f, BitPackingTag.HeadRotationY);
		}
		writer.Write(current.Stamina.StaminaExhausted);
		writer.Write(current.Stamina.OxygenExhausted);
		writer.Write(current.Stamina.HandsExhausted);
		bool flag9 = !prevFrame.InteractWithDoorPacket.Equals(current.InteractWithDoorPacket);
		bool flag10 = !prevFrame.InteractWithBtrPacket.Equals(current.InteractWithBtrPacket);
		bool flag11 = !prevFrame.InteractWithTripwirePacket.Equals(current.InteractWithTripwirePacket);
		bool flag12 = !prevFrame.InteractWithTransitPacket.Equals(current.InteractWithTransitPacket);
		bool flag13 = !prevFrame.InteractWithEventObjectPacket.Equals(current.InteractWithEventObjectPacket);
		bool flag14 = !prevFrame.LootInteractionPacket.Equals(current.LootInteractionPacket);
		bool flag15 = !prevFrame.StationaryWeaponPacket.Equals(current.StationaryWeaponPacket);
		bool flag16 = !prevFrame.PlantItemPacket.Equals(current.PlantItemPacket);
		bool flag17 = flag9 || flag10 || flag11 || flag12 || flag13 || flag14 || flag16 || flag15;
		writer.Write(flag17);
		if (flag17)
		{
			writer.Write(flag9);
			if (flag9)
			{
				current.InteractWithDoorPacket.Serialize(writer);
			}
			writer.Write(flag10);
			if (flag10)
			{
				current.InteractWithBtrPacket.Serialize(writer);
			}
			writer.Write(flag11);
			if (flag11)
			{
				current.InteractWithTripwirePacket.Serialize(writer);
			}
			writer.Write(flag12);
			if (flag12)
			{
				current.InteractWithTransitPacket.Serialize(writer);
			}
			writer.Write(flag13);
			if (flag13)
			{
				current.InteractWithEventObjectPacket.Serialize(writer);
			}
			writer.Write(flag14);
			if (flag14)
			{
				current.LootInteractionPacket.Serialize(writer);
			}
			writer.Write(flag15);
			if (flag15)
			{
				current.StationaryWeaponPacket.Serialize(writer);
			}
			writer.Write(flag16);
			if (flag16)
			{
				current.PlantItemPacket.Serialize(writer);
			}
		}
		writer.Write(current.PlayerSurfaceUpAlignNormal);
		smethod_0(writer, ref current);
		smethod_1(writer, ref current);
		smethod_2(writer, ref current);
		smethod_3(writer, ref current);
		smethod_4(writer, ref current);
	}

	public static void smethod_0(GInterface131 writer, ref MovementInfoPacketStruct current)
	{
		writer.Write(current.VaultingPacket.HasCriticalData);
		if (current.VaultingPacket.HasCriticalData)
		{
			current.VaultingPacket.Serialize(writer);
		}
	}

	public static void smethod_1(GInterface131 writer, ref MovementInfoPacketStruct current)
	{
		writer.Write(current.InteractionPacket.HasCriticalData);
		if (current.InteractionPacket.HasCriticalData)
		{
			current.InteractionPacket.Serialize(writer);
		}
	}

	public static void smethod_2(GInterface131 writer, ref MovementInfoPacketStruct current)
	{
		writer.Write(current.InventoryInteractionPacket.HasCriticalData);
		if (current.InventoryInteractionPacket.HasCriticalData)
		{
			current.InventoryInteractionPacket.Serialize(writer);
		}
	}

	public static void smethod_3(GInterface131 writer, ref MovementInfoPacketStruct current)
	{
		writer.Write(current.MountingPacket.HasCriticalData);
		if (current.MountingPacket.HasCriticalData)
		{
			current.MountingPacket.Serialize(writer);
		}
	}

	public static void smethod_4(GInterface131 writer, ref MovementInfoPacketStruct current)
	{
		writer.Write(current.LeftHandPacket.HasCriticalData);
		if (current.LeftHandPacket.HasCriticalData)
		{
			current.LeftHandPacket.Serialize(writer);
		}
	}

	public static void smethod_5(GInterface131 writer, Vector2 currentRotation, Vector2 prevFrameRotation)
	{
		_ = writer.BitsWritten;
		bool flag = (currentRotation - prevFrameRotation).sqrMagnitude >= float.Epsilon;
		writer.Write(flag);
		if (flag)
		{
			currentRotation.x = (currentRotation.x + 360f) % 360f;
			Gclass1375_0.Write(writer, currentRotation);
		}
	}

	public static Vector2 smethod_6(IDataReader reader, Vector2 prevFrameRotation)
	{
		if (reader.ReadBool())
		{
			Gclass1375_0.Read(reader, out var value);
			return value;
		}
		return prevFrameRotation;
	}

	public static Vector3 smethod_7(GInterface131 writer, Vector3 currentPosition, Vector3 prevFramePosition, bool isExtraPrecisionMovement, bool forceSyncPosition)
	{
		_ = writer.BitsWritten;
		Vector3 vector = currentPosition - prevFramePosition;
		bool flag = forceSyncPosition || vector.sqrMagnitude > 9.536743E-07f;
		writer.Write(flag);
		Vector3 result = currentPosition;
		if (flag)
		{
			bool flag2 = GClass858.AllAbsComponentsLessThen(vector, 6f) && !forceSyncPosition;
			writer.Write(flag2);
			if (flag2)
			{
				GClass1378 gClass = (isExtraPrecisionMovement ? Gclass1378_1 : Gclass1378_0);
				result = prevFramePosition + gClass.Write(writer, vector);
			}
			else
			{
				result = (isExtraPrecisionMovement ? PRECISE_POSITION_QUANTIZER : USUAL_POSITION_QUANTIZER).Write(writer, currentPosition);
			}
		}
		return result;
	}

	public static Vector3 smethod_8(IDataReader reader, Vector3 prevPosition, bool isExtraPrecisionMovement)
	{
		if (reader.ReadBool())
		{
			if (reader.ReadBool())
			{
				(isExtraPrecisionMovement ? Gclass1378_1 : Gclass1378_0).Read(reader, out var value);
				return prevPosition + value;
			}
			(isExtraPrecisionMovement ? PRECISE_POSITION_QUANTIZER : USUAL_POSITION_QUANTIZER).Read(reader, out var value2);
			return value2;
		}
		return prevPosition;
	}

	public static void DeserializeDiffUsing(IDataReader reader, ref MovementInfoPacketStruct movementInfoPacket, MovementInfoPacketStruct prevFrame, bool isExtraPrecisionMovement = false)
	{
		movementInfoPacket.SyncPositionApplied = reader.ReadBool();
		movementInfoPacket.EPlayerState = (reader.ReadBool() ? ((EPlayerState)reader.ReadLimitedInt32(0, 31)) : prevFrame.EPlayerState);
		movementInfoPacket.AnimatorStateIndex = (reader.ReadBool() ? reader.ReadLimitedInt32(0, 70) : prevFrame.AnimatorStateIndex);
		if (reader.ReadBool())
		{
			movementInfoPacket.DiscreteDirection = reader.ReadLimitedInt32(0, 8);
		}
		movementInfoPacket.PoseLevel = prevFrame.PoseLevel;
		if (reader.ReadBool())
		{
			GClass1374.Serialize(reader, ref movementInfoPacket.PoseLevel, Gstruct136_0);
		}
		movementInfoPacket.CharacterMovementSpeed = prevFrame.CharacterMovementSpeed;
		if (reader.ReadBool())
		{
			GClass1374.Serialize(reader, ref movementInfoPacket.CharacterMovementSpeed, Gstruct136_1);
		}
		movementInfoPacket.Tilt = prevFrame.Tilt;
		if (reader.ReadBool())
		{
			GClass1374.Serialize(reader, ref movementInfoPacket.Tilt, Gstruct136_2);
		}
		if (reader.ReadBool())
		{
			if (reader.ReadBool())
			{
				movementInfoPacket.Step = 0;
			}
			else
			{
				bool flag = reader.ReadBool();
				movementInfoPacket.Step = (flag ? 1 : (-1));
			}
		}
		else
		{
			movementInfoPacket.Step = prevFrame.Step;
		}
		movementInfoPacket.BlindFire = reader.ReadLimitedInt32(-1, 1);
		movementInfoPacket.SoftSurface = reader.ReadBool();
		movementInfoPacket.LeftStance = reader.ReadBool();
		if (!reader.ReadBool())
		{
			movementInfoPacket.HeadRotation = new Vector3(reader.ReadLimitedFloat(-50f, 50f, 0.0625f), reader.ReadLimitedFloat(-50f, 50f, 0.0625f), 0f);
		}
		else
		{
			movementInfoPacket.HeadRotation = prevFrame.HeadRotation;
		}
		movementInfoPacket.Stamina.StaminaExhausted = reader.ReadBool();
		movementInfoPacket.Stamina.OxygenExhausted = reader.ReadBool();
		movementInfoPacket.Stamina.HandsExhausted = reader.ReadBool();
		if (reader.ReadBool())
		{
			if (reader.ReadBool())
			{
				movementInfoPacket.InteractWithDoorPacket.Deserialize(reader);
			}
			if (reader.ReadBool())
			{
				movementInfoPacket.InteractWithBtrPacket.Deserialize(reader);
			}
			if (reader.ReadBool())
			{
				movementInfoPacket.InteractWithTripwirePacket.Deserialize(reader);
			}
			if (reader.ReadBool())
			{
				movementInfoPacket.InteractWithTransitPacket.Deserialize(reader);
			}
			if (reader.ReadBool())
			{
				movementInfoPacket.InteractWithEventObjectPacket.Deserialize(reader);
			}
			if (reader.ReadBool())
			{
				movementInfoPacket.LootInteractionPacket.Deserialize(reader);
			}
			if (reader.ReadBool())
			{
				movementInfoPacket.StationaryWeaponPacket.Deserialize(reader);
			}
			if (reader.ReadBool())
			{
				movementInfoPacket.PlantItemPacket.Deserialize(reader);
			}
		}
		movementInfoPacket.IsGrounded = reader.ReadBool();
		Gclass1378_2.Read(reader, out movementInfoPacket.SurfaceNormal);
		movementInfoPacket.PlayerSurfaceUpAlignNormal = reader.ReadVector3();
		movementInfoPacket.VaultingPacket.HasCriticalData = reader.ReadBool();
		if (movementInfoPacket.VaultingPacket.HasCriticalData)
		{
			movementInfoPacket.VaultingPacket.Deserialize(reader);
		}
		movementInfoPacket.InteractionPacket.HasCriticalData = reader.ReadBool();
		if (movementInfoPacket.InteractionPacket.HasCriticalData)
		{
			movementInfoPacket.InteractionPacket.Deserialize(reader);
		}
		movementInfoPacket.InventoryInteractionPacket.HasCriticalData = reader.ReadBool();
		if (movementInfoPacket.InventoryInteractionPacket.HasCriticalData)
		{
			movementInfoPacket.InventoryInteractionPacket.Deserialize(reader);
		}
		movementInfoPacket.MountingPacket.HasCriticalData = reader.ReadBool();
		if (movementInfoPacket.MountingPacket.HasCriticalData)
		{
			movementInfoPacket.MountingPacket.Deserialize(reader);
		}
		movementInfoPacket.LeftHandPacket.HasCriticalData = reader.ReadBool();
		if (movementInfoPacket.LeftHandPacket.HasCriticalData)
		{
			movementInfoPacket.LeftHandPacket.Deserialize(reader);
		}
	}
}
