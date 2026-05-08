using System;
using BitPacking;
using EFT;

public struct GStruct243
{
	[NonSerialized]
	public const int Int_0 = -1;

	[NonSerialized]
	public const int Int_1 = 15;

	public GStruct236 ChangeFireMode;

	public bool ChangeCalibrationPoint;

	public bool ToggleAim;

	public int AimingIndex;

	public bool ExamineWeapon;

	public bool CheckAmmo;

	public bool CheckChamber;

	public bool CheckFireMode;

	public bool ToggleLauncher;

	public bool ReloadBoltAction;

	public bool ToggleBipod;

	public GStruct238 ToggleTacticalCombo;

	public GStruct241 ChangeSightsMode;

	public GStruct240 LauncherRangeStatePacket;

	public GStruct235? FiredShotInfos;

	public GStruct373 FlareShotInfo;

	public GStruct384 RocketShotInfo;

	public GStruct381 LauncherReloadInfo;

	public GStruct372 EnableInventoryPacket;

	public GStruct376 HideWeaponPacket;

	public GStruct245 ReloadMagPacket;

	public GStruct246 QuickReloadMagPacket;

	public GStruct250 ReloadWithAmmoPacket;

	public GStruct247 ReloadBarrelsPacket;

	public GStruct382<GStruct444> ShotsForApprovement;

	public GStruct367 CompassPacket;

	public GStruct266 RadioTransmitterPacket;

	public GStruct179 LighthouseTraderZoneDataPacket;

	public GStruct249 CylinderMagStatusPacket;

	public GStruct248 RollCylinderPacket;

	public bool IsInImportantState;

	public EInteraction Gesture;

	public float TimeStamp;

	public GStruct244 SetLeftStancePacket;

	public bool Boolean_0
	{
		get
		{
			if (!ChangeFireMode.ChangeFireMode && !IsInImportantState && !ToggleAim && !ExamineWeapon && !CheckAmmo && !CheckChamber && !CheckFireMode && !ToggleTacticalCombo.ToggleTacticalCombo && !ChangeSightsMode.ChangeSightMode && !LauncherRangeStatePacket.ChangeLauncherRange && !ToggleLauncher && !FiredShotInfos.HasValue && !FlareShotInfo.WasShot && !FlareShotInfo.StartOneShotFire && !EnableInventoryPacket.EnableInventory && !HideWeaponPacket.HideWeapon && !ReloadMagPacket.Reload && !LauncherReloadInfo.Reload && !QuickReloadMagPacket.Reload && !ReloadWithAmmoPacket.Reload && !ReloadBarrelsPacket.Reload && ShotsForApprovement.Length <= 0 && Gesture == EInteraction.None && !CompassPacket.Toggle && !ReloadBoltAction && !RollCylinderPacket.RollCylinder && !SetLeftStancePacket.HasNewValue && !RocketShotInfo.HasCriticalData)
			{
				return ToggleBipod;
			}
			return true;
		}
	}

	public bool Boolean_1
	{
		get
		{
			if (!ReloadMagPacket.Reload && !ReloadBarrelsPacket.Reload && !ReloadWithAmmoPacket.Reload && !QuickReloadMagPacket.Reload)
			{
				return LauncherReloadInfo.Reload;
			}
			return true;
		}
	}

	public static void DeserializeDiffUsing(IDataReader reader, ref GStruct243 firearmPacket, GStruct243 prevFrame)
	{
		GStruct236.DeserializeDiffUsing(reader, ref firearmPacket.ChangeFireMode);
		firearmPacket.ToggleAim = reader.ReadBool();
		if (firearmPacket.ToggleAim)
		{
			firearmPacket.AimingIndex = reader.ReadLimitedInt32(-1, 15);
		}
		firearmPacket.ExamineWeapon = reader.ReadBool();
		firearmPacket.CheckAmmo = reader.ReadBool();
		firearmPacket.CheckChamber = reader.ReadBool();
		firearmPacket.CheckFireMode = reader.ReadBool();
		firearmPacket.ToggleLauncher = reader.ReadBool();
		firearmPacket.ReloadBoltAction = reader.ReadBool();
		firearmPacket.ToggleBipod = reader.ReadBool();
		GStruct248.Serialize(reader, ref firearmPacket.RollCylinderPacket);
		if (reader.ReadBool())
		{
			firearmPacket.Gesture = (EInteraction)reader.ReadLimitedInt32(0, GClass2075.Count - 1);
		}
		GClass2995.Serialize(reader, ref firearmPacket.FlareShotInfo);
		GClass2996.Serialize(reader, ref firearmPacket.RocketShotInfo);
		GClass2993.Serialize(reader, ref firearmPacket.LauncherReloadInfo);
		GClass2988.Serialize(reader, ref firearmPacket.CompassPacket);
		GStruct238.Serialize(reader, ref firearmPacket.ToggleTacticalCombo);
		GStruct241.Serialize(reader, ref firearmPacket.ChangeSightsMode);
		GStruct240.Serialize(reader, ref firearmPacket.LauncherRangeStatePacket);
		firearmPacket.FiredShotInfos = GStruct235.Deserialize(reader);
		GClass2986.Serialize(reader, ref firearmPacket.EnableInventoryPacket);
		GClass2987.Serialize(reader, ref firearmPacket.HideWeaponPacket);
		GStruct245.DeserializeDiffUsing(reader, ref firearmPacket.ReloadMagPacket, prevFrame.ReloadMagPacket);
		GStruct246.DeserializeDiffUsing(reader, ref firearmPacket.QuickReloadMagPacket, prevFrame.QuickReloadMagPacket);
		GStruct249.Serialize(reader, ref firearmPacket.CylinderMagStatusPacket);
		GStruct250.Serialize(reader, ref firearmPacket.ReloadWithAmmoPacket);
		GStruct247.Serialize(reader, ref firearmPacket.ReloadBarrelsPacket);
		GClass2998.Serialize(reader, ref firearmPacket.ShotsForApprovement);
		firearmPacket.IsInImportantState = reader.ReadBool();
		GStruct244.Serialize(reader, ref firearmPacket.SetLeftStancePacket);
	}

	public static void SerializeDiffUsing(GInterface131 writer, GStruct243 current, GStruct243 prevFrame)
	{
		GStruct236.SerializeDiffUsing(writer, current.ChangeFireMode);
		writer.Write(current.ToggleAim);
		if (current.ToggleAim)
		{
			writer.WriteLimitedInt32(current.AimingIndex, -1, 15, BitPackingTag.ScopeIndexInsideSight0);
		}
		writer.Write(current.ExamineWeapon);
		writer.Write(current.CheckAmmo);
		writer.Write(current.CheckChamber);
		writer.Write(current.CheckFireMode);
		writer.Write(current.ToggleLauncher);
		writer.Write(current.ReloadBoltAction);
		writer.Write(current.ToggleBipod);
		GStruct248.Serialize(writer, ref current.RollCylinderPacket);
		bool flag = current.Gesture != EInteraction.None;
		writer.Write(flag);
		if (flag)
		{
			int gesture = (int)current.Gesture;
			writer.WriteLimitedInt32(gesture, 0, GClass2075.Count - 1, BitPackingTag.HandsChangePacket14);
		}
		GClass2995.Serialize(writer, ref current.FlareShotInfo);
		GClass2996.Serialize(writer, ref current.RocketShotInfo);
		GClass2993.Serialize(writer, ref current.LauncherReloadInfo);
		GClass2988.Serialize(writer, ref current.CompassPacket);
		GStruct238.Serialize(writer, ref current.ToggleTacticalCombo);
		GStruct241.Serialize(writer, ref current.ChangeSightsMode);
		GStruct240.Serialize(writer, ref current.LauncherRangeStatePacket);
		GStruct235.Serialize(writer, ref current.FiredShotInfos);
		GClass2986.Serialize(writer, ref current.EnableInventoryPacket);
		GClass2987.Serialize(writer, ref current.HideWeaponPacket);
		GStruct245.SerializeDiffUsing(writer, current.ReloadMagPacket, prevFrame.ReloadMagPacket);
		GStruct246.SerializeDiffUsing(writer, current.QuickReloadMagPacket, prevFrame.QuickReloadMagPacket);
		GStruct249.Serialize(writer, ref current.CylinderMagStatusPacket);
		GStruct250.Serialize(writer, ref current.ReloadWithAmmoPacket);
		GStruct247.Serialize(writer, ref current.ReloadBarrelsPacket);
		GClass2998.Serialize(writer, ref current.ShotsForApprovement);
		writer.Write(current.IsInImportantState);
		GStruct244.Serialize(writer, ref current.SetLeftStancePacket);
	}

	public void Add(GStruct235 firedShotInfo)
	{
		GClass2243.AttachTo(firedShotInfo, ref FiredShotInfos);
	}

	public bool HasShotsRealShots()
	{
		GStruct235? gStruct = FiredShotInfos;
		while (true)
		{
			if (gStruct.HasValue)
			{
				if (gStruct.Value.EShotType != EShotType.DryFire)
				{
					break;
				}
				gStruct = GClass2243.GetNested(gStruct.Value);
				continue;
			}
			return false;
		}
		return true;
	}

	public override string ToString()
	{
		return string.Format("{0}: {1}, {2}: {3}, {4}: {5}, {6}: {7}, {8}: {9}, {10}: {11}, {12}: {13}, {14}: {15}, {16}: {17}, {18}: {19}, {20}: {21}, {22}: {23}, {24}: {25}, {26}: {27}, {28}: {29}, {30}: {31}, {32}: {33}, {34}: {35}, {36}: {37}, {38}: {39}, {40}: {41}, {42}: {43}, {44}: {45}", "ChangeFireMode", ChangeFireMode, "ChangeCalibrationPoint", ChangeCalibrationPoint, "ToggleAim", ToggleAim, "AimingIndex", AimingIndex, "ExamineWeapon", ExamineWeapon, "CheckAmmo", CheckAmmo, "CheckChamber", CheckChamber, "ToggleLauncher", ToggleLauncher, "ToggleBipod", ToggleBipod, "HideWeaponPacket", HideWeaponPacket, "ToggleTacticalCombo", ToggleTacticalCombo, "ChangeSightsMode", ChangeSightsMode, "FiredShotInfos", FiredShotInfos, "LauncherReloadInfo", LauncherReloadInfo, "EnableInventoryPacket", EnableInventoryPacket, "ReloadMagPacket", ReloadMagPacket, "QuickReloadMagPacket", QuickReloadMagPacket, "ReloadWithAmmoPacket", ReloadWithAmmoPacket, "ReloadBarrelsPacket", ReloadBarrelsPacket, "ShotsForApprovement", ShotsForApprovement, "IsInImportantState", IsInImportantState, "Gesture", Gesture, "TimeStamp", TimeStamp);
	}
}
