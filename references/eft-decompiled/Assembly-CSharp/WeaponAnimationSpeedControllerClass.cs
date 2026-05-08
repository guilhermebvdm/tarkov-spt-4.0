using UnityEngine;

public abstract class WeaponAnimationSpeedControllerClass
{
	public static readonly int BOOL_ACTIVE = Animator.StringToHash("Active");

	public static readonly int BOOL_ADDAMMOINCHAMBER = Animator.StringToHash("AddAmmoInChamber");

	public static readonly int BOOL_ADDAMMOINMAG = Animator.StringToHash("AddAmmoInMag");

	public static readonly int BOOL_ALTFIRE = Animator.StringToHash("AltFire");

	public static readonly int BOOL_ARM = Animator.StringToHash("Arm");

	public static readonly int BOOL_ARMED = Animator.StringToHash("Armed");

	public static readonly int BOOL_BOLTACTIONRELOAD = Animator.StringToHash("BoltActionReload");

	public static readonly int BOOL_BOLTCATCH = Animator.StringToHash("BoltCatch");

	public static readonly int BOOL_CANRELOAD = Animator.StringToHash("CanReload");

	public static readonly int BOOL_DELAMMOCHAMBER = Animator.StringToHash("DelAmmoChamber");

	public static readonly int BOOL_DELAMMOFROMMAG = Animator.StringToHash("DelAmmoFromMag");

	public static readonly int BOOL_DISARM = Animator.StringToHash("Disarm");

	public static readonly int BOOL_DISCHARGE = Animator.StringToHash("Discharge");

	public static readonly int BOOL_FASTHIDE = Animator.StringToHash("FastHide");

	public static readonly int BOOL_FIRE = Animator.StringToHash("Fire");

	public static readonly int BOOL_SPRINT = Animator.StringToHash("Sprint");

	public static readonly int BOOL_GRIP = Animator.StringToHash("Grip");

	public static readonly int BOOL_HVAT = Animator.StringToHash("Hvat");

	public static readonly int BOOL_IDLE = Animator.StringToHash("Idle");

	public static readonly int BOOL_INCOMPATIBLEAMMO = Animator.StringToHash("IncompatibleAmmo");

	public static readonly int BOOL_INVENTORY = Animator.StringToHash("Inventory");

	public static readonly int BOOL_ISEXTERNALMAG = Animator.StringToHash("IsExternalMag");

	public static readonly int BOOL_LAUNCHERREADY = Animator.StringToHash("LauncherReady");

	public static readonly int BOOL_LOADONE = Animator.StringToHash("LoadOne");

	public static readonly int BOOL_MAGFULL = Animator.StringToHash("MagFull");

	public static readonly int BOOL_MAGIN = Animator.StringToHash("MagIn");

	public static readonly int BOOL_MAGINWEAPON = Animator.StringToHash("MagInWeapon");

	public static readonly int FLOAT_MAGINWEAPONFLOAT = Animator.StringToHash("MagInWeaponFloat");

	public static readonly int BOOL_MAGOUT = Animator.StringToHash("MagOut");

	public static readonly int BOOL_MAGSWAP = Animator.StringToHash("MagSwap");

	public static readonly int BOOL_MODSET = Animator.StringToHash("ModSet");

	public static readonly int BOOL_OFFBOLTCATCH = Animator.StringToHash("OffBoltCatch");

	public static readonly int BOOL_ONBOLTCATCH = Animator.StringToHash("OnBoltCatch");

	public static readonly int BOOL_PATROL = Animator.StringToHash("Patrol");

	public static readonly int BOOL_QUICKFIRE = Animator.StringToHash("QuickFire");

	public static readonly int BOOL_SETFIREMODE0 = Animator.StringToHash("SetFiremode0");

	public static readonly int BOOL_SETFIREMODE1 = Animator.StringToHash("SetFiremode1");

	public static readonly int BOOL_SHELLEJECT = Animator.StringToHash("ShellEject");

	public static readonly int BOOL_STOCKFOLDED = Animator.StringToHash("StockFolded");

	public static readonly int BOOL_USELEFTHAND = Animator.StringToHash("UseLeftHand");

	public static readonly int BOOL_RECHAMBER = Animator.StringToHash("Rechamber");

	public static readonly int BOOL_MALFUNCTIONREPAIR = Animator.StringToHash("MalfunctionRepair");

	public static readonly int BOOL_MISFIRESLIDE_UNKNOWN = Animator.StringToHash("MisfireSlideUnknown");

	public static readonly int BOOL_ROLLCYLINDER = Animator.StringToHash("RollCylinder");

	public static readonly int BOOL_ROLLTOZEROCAMORA = Animator.StringToHash("RollToZeroCamora");

	public static readonly int BOOL_MASTERINGRELOADABORTED = Animator.StringToHash("MasteringReloadAborted");

	public static readonly int BOOL_FIREMODE_SPRINT = Animator.StringToHash("FiremodeSprint");

	public static readonly int FLOAT_DOUBLEACTION = Animator.StringToHash("DoubleActionFireModeFloat");

	public static readonly int FLOAT_AIM_ANGLE = Animator.StringToHash("Aim_angle");

	public static readonly int FLOAT_AMMOINCHAMBER = Animator.StringToHash("AmmoInChamber");

	public static readonly int FLOAT_AMMOINMAG = Animator.StringToHash("AmmoInMag");

	public static readonly int FLOAT_AMMOCOUNTFORREMOVE = Animator.StringToHash("AmmoCountForRemove");

	public static readonly int FLOAT_CHARACTERID = Animator.StringToHash("CharacterID");

	public static readonly int FLOAT_DEFLECTED = Animator.StringToHash("Deflected");

	public static readonly int FLOAT_EMPTYLINKSCOUNT = Animator.StringToHash("EmptyLinksCount");

	public static readonly int FLOAT_FIREMODE = Animator.StringToHash("FireMode");

	public static readonly int FLOAT_GESTUREINDEX = Animator.StringToHash("GestureIndex");

	public static readonly int FLOAT_GRIPWEIGHT = Animator.StringToHash("GripWeight");

	public static readonly int FLOAT_IDLEPOSITION = Animator.StringToHash("IdlePosition");

	public static readonly int FLOAT_IDLEVAR = Animator.StringToHash("IdleVar");

	public static readonly int FLOAT_LAUNCHERID = Animator.StringToHash("LauncherID");

	public static readonly int FLOAT_LEFTHANDPROGRESS = Animator.StringToHash("LeftHandProgress");

	public static readonly int FLOAT_RADIOCOMMAND = Animator.StringToHash("RadioCommand");

	public static readonly int FLOAT_RELTYPENEW = Animator.StringToHash("RelTypeNew");

	public static readonly int FLOAT_RELTYPEOLD = Animator.StringToHash("RelTypeOld");

	public static readonly int FLOAT_SHELLSINWEAPON = Animator.StringToHash("ShellsInWeapon");

	public static readonly int FLOAT_SHOULDERREACH = Animator.StringToHash("ShoulderReach");

	public static readonly int FLOAT_SPEEDDRAW = Animator.StringToHash("SpeedDraw");

	public static readonly int FLOAT_SPEEDFIX = Animator.StringToHash("SpeedFix");

	public static readonly int FLOAT_SPEEDRELOAD = Animator.StringToHash("SpeedReload");

	public static readonly int FLOAT_STOCKANIMATIONINDEX = Animator.StringToHash("StockAnimationIndex");

	public static readonly int FLOAT_SWINGSPEED = Animator.StringToHash("SwingSpeed");

	public static readonly int FLOAT_THIRDPERSON = Animator.StringToHash("ThirdPerson");

	public static readonly int FLOAT_UNDERMOD = Animator.StringToHash("UnderMod");

	public static readonly int FLOAT_USETIMEMULTIPLIER = Animator.StringToHash("UseTimeMultiplier");

	public static readonly int FLOAT_WEAPONLEVEL = Animator.StringToHash("WeaponLevel");

	public static readonly int FLOAT_CHAMBERINDEX = Animator.StringToHash("ChamberIndex");

	public static readonly int FLOAT_CHAMBERINDEX_WITH_SHELL = Animator.StringToHash("ChamberIndexWithShell");

	public static readonly int FLOAT_CAMORAINDEX = Animator.StringToHash("CamoraIndex");

	public static readonly int FLOAT_CAMORAINDEXFORLOADAMMO = Animator.StringToHash("CamoraIndexForLoadAmmo");

	public static readonly int FLOAT_CAMORAINDEXWITHSHELLFORREMOVE = Animator.StringToHash("CamoraIndexWithShellForRemove");

	public static readonly int FLOAT_CAMORAINDEXFORUNLOADAMMO = Animator.StringToHash("CamoraIndexForRemoveAmmo");

	public static readonly int FLOAT_CAMORAFIREINDEX = Animator.StringToHash("CamoraFireIndex");

	public static readonly int FLOAT_SIGHTINGRANGE = Animator.StringToHash("SightingRange");

	public static readonly int FLOAT_ANIMATION_MOD_ID_FLOAT = Animator.StringToHash("UniqueAnimationModIdFloat");

	public static readonly int INT_ANIMATIONVARIANT = Animator.StringToHash("AnimationVariant");

	public static readonly int BOOL_NEXT_LIMB = Animator.StringToHash("NextLimb");

	public static readonly int INT_FIREVAR = Animator.StringToHash("FireVar");

	public static readonly int INT_GRENADEALTFIRE = Animator.StringToHash("GrenadeAltFire");

	public static readonly int INT_GRENADEFIRE = Animator.StringToHash("GrenadeFire");

	public static readonly int INT_LACTIONINDEX = Animator.StringToHash("LActionIndex");

	public static readonly int INT_PLAYERSTATE = Animator.StringToHash("PlayerState");

	public static readonly int INT_THIRDACTION = Animator.StringToHash("ThirdAction");

	public static readonly int INT_MALFUNCTION = Animator.StringToHash("Malfunction");

	public static readonly int INT_MALFTYPE = Animator.StringToHash("MalfType");

	public static readonly int INT_MOD_ANIMATION_ID = Animator.StringToHash("UniqueAnimationModId");

	public static readonly int TRIGGER_BUTTERFINGERS = Animator.StringToHash("Butterfingers");

	public static readonly int TRIGGER_CHECKAMMO = Animator.StringToHash("CheckAmmo");

	public static readonly int TRIGGER_CHECKCHAMBER = Animator.StringToHash("CheckChamber");

	public static readonly int TRIGGER_FIREMODECHECK = Animator.StringToHash("FiremodeCheck");

	public static readonly int TRIGGER_FIREMODESWITCH = Animator.StringToHash("FiremodeSwitch");

	public static readonly int TRIGGER_HAMMERARMED = Animator.StringToHash("HammerArmed");

	public static readonly int TRIGGER_FIRINGBULLET = Animator.StringToHash("FiringBullet");

	public static readonly int TRIGGER_GESTURE = Animator.StringToHash("Gesture");

	public static readonly int TRIGGER_GOMELE = Animator.StringToHash("GoMele");

	public static readonly int TRIGGER_HANDREADY = Animator.StringToHash("HandReady");

	public static readonly int TRIGGER_IDLETIME = Animator.StringToHash("IdleTime");

	public static readonly int TRIGGER_LOOK = Animator.StringToHash("Look");

	public static readonly int TRIGGER_MAGINFROMINV = Animator.StringToHash("MagInFromInv");

	public static readonly int TRIGGER_MAGOUTFROMINV = Animator.StringToHash("MagOutFromInv");

	public static readonly int TRIGGER_MAGSWAPEND = Animator.StringToHash("MagSwapEnd");

	public static readonly int TRIGGER_MODSWITCH = Animator.StringToHash("ModSwitch");

	public static readonly int TRIGGER_RADIO = Animator.StringToHash("Radio");

	public static readonly int TRIGGER_RELOAD = Animator.StringToHash("Reload");

	public static readonly int TRIGGER_RELOADFAST = Animator.StringToHash("ReloadFast");

	public static readonly int TRIGGER_STOCKSWITCH = Animator.StringToHash("StockSwitch");

	public static readonly int FLOAT_IS_AIMING = Animator.StringToHash("IsAimingFloat");

	public static readonly int FLOAT_PREV_AIMING_STATE = Animator.StringToHash("PrevIsAimingFloatState");

	public static readonly int TRIGGER_AIMING_IN = Animator.StringToHash("AimingIn");

	public static readonly int TRIGGER_AIMING_OUT = Animator.StringToHash("AimingOut");

	public static readonly int BOOL_BIPOD = Animator.StringToHash("Bipod");

	public static readonly int BOOL_MOUNTED = Animator.StringToHash("Mounted");

	public static string ParameterHashToName(int hash)
	{
		if (hash == BOOL_ACTIVE)
		{
			return "Active";
		}
		if (hash == BOOL_ADDAMMOINCHAMBER)
		{
			return "AddAmmoInChamber";
		}
		if (hash == BOOL_ADDAMMOINMAG)
		{
			return "AddAmmoInMag";
		}
		if (hash == BOOL_ALTFIRE)
		{
			return "AltFire";
		}
		if (hash == BOOL_ARM)
		{
			return "Arm";
		}
		if (hash == BOOL_ARMED)
		{
			return "Armed";
		}
		if (hash == BOOL_BOLTACTIONRELOAD)
		{
			return "BoltActionReload";
		}
		if (hash == BOOL_BOLTCATCH)
		{
			return "BoltCatch";
		}
		if (hash == BOOL_CANRELOAD)
		{
			return "CanReload";
		}
		if (hash == BOOL_DELAMMOCHAMBER)
		{
			return "DelAmmoChamber";
		}
		if (hash == BOOL_DELAMMOFROMMAG)
		{
			return "DelAmmoFromMag";
		}
		if (hash == BOOL_DISARM)
		{
			return "Disarm";
		}
		if (hash == BOOL_DISCHARGE)
		{
			return "Discharge";
		}
		if (hash == BOOL_FASTHIDE)
		{
			return "FastHide";
		}
		if (hash == BOOL_FIRE)
		{
			return "Fire";
		}
		if (hash == BOOL_GRIP)
		{
			return "Grip";
		}
		if (hash == BOOL_HVAT)
		{
			return "Hvat";
		}
		if (hash == BOOL_IDLE)
		{
			return "Idle";
		}
		if (hash == BOOL_INCOMPATIBLEAMMO)
		{
			return "IncompatibleAmmo";
		}
		if (hash == BOOL_INVENTORY)
		{
			return "Inventory";
		}
		if (hash == BOOL_ISEXTERNALMAG)
		{
			return "IsExternalMag";
		}
		if (hash == BOOL_LAUNCHERREADY)
		{
			return "LauncherReady";
		}
		if (hash == BOOL_LOADONE)
		{
			return "LoadOne";
		}
		if (hash == BOOL_MAGFULL)
		{
			return "MagFull";
		}
		if (hash == BOOL_MAGIN)
		{
			return "MagIn";
		}
		if (hash == BOOL_MAGINWEAPON)
		{
			return "MagInWeapon";
		}
		if (hash == BOOL_MAGOUT)
		{
			return "MagOut";
		}
		if (hash == BOOL_MAGSWAP)
		{
			return "MagSwap";
		}
		if (hash == BOOL_MODSET)
		{
			return "ModSet";
		}
		if (hash == BOOL_OFFBOLTCATCH)
		{
			return "OffBoltCatch";
		}
		if (hash == BOOL_ONBOLTCATCH)
		{
			return "OnBoltCatch";
		}
		if (hash == BOOL_PATROL)
		{
			return "Patrol";
		}
		if (hash == BOOL_QUICKFIRE)
		{
			return "QuickFire";
		}
		if (hash == BOOL_SETFIREMODE0)
		{
			return "SetFiremode0";
		}
		if (hash == BOOL_SETFIREMODE1)
		{
			return "SetFiremode1";
		}
		if (hash == BOOL_SHELLEJECT)
		{
			return "ShellEject";
		}
		if (hash == BOOL_STOCKFOLDED)
		{
			return "StockFolded";
		}
		if (hash == BOOL_USELEFTHAND)
		{
			return "UseLeftHand";
		}
		if (hash == BOOL_RECHAMBER)
		{
			return "Rechamber";
		}
		if (hash == BOOL_MALFUNCTIONREPAIR)
		{
			return "MalfunctionRepair";
		}
		if (hash == BOOL_MISFIRESLIDE_UNKNOWN)
		{
			return "MisfireSlideUnknown";
		}
		if (hash == BOOL_ROLLCYLINDER)
		{
			return "RollCylinder";
		}
		if (hash == BOOL_MASTERINGRELOADABORTED)
		{
			return "MasteringReloadAborted";
		}
		if (hash == FLOAT_DOUBLEACTION)
		{
			return "DoubleActionFireModeFloat";
		}
		if (hash == FLOAT_AIM_ANGLE)
		{
			return "Aim_angle";
		}
		if (hash == FLOAT_AMMOINCHAMBER)
		{
			return "AmmoInChamber";
		}
		if (hash == FLOAT_AMMOINMAG)
		{
			return "AmmoInMag";
		}
		if (hash == FLOAT_AMMOCOUNTFORREMOVE)
		{
			return "AmmoCountForRemove";
		}
		if (hash == FLOAT_CHARACTERID)
		{
			return "CharacterID";
		}
		if (hash == FLOAT_DEFLECTED)
		{
			return "Deflected";
		}
		if (hash == FLOAT_EMPTYLINKSCOUNT)
		{
			return "EmptyLinksCount";
		}
		if (hash == FLOAT_FIREMODE)
		{
			return "FireMode";
		}
		if (hash == FLOAT_GESTUREINDEX)
		{
			return "GestureIndex";
		}
		if (hash == FLOAT_GRIPWEIGHT)
		{
			return "GripWeight";
		}
		if (hash == FLOAT_IDLEPOSITION)
		{
			return "IdlePosition";
		}
		if (hash == FLOAT_IDLEVAR)
		{
			return "IdleVar";
		}
		if (hash == FLOAT_LAUNCHERID)
		{
			return "LauncherID";
		}
		if (hash == FLOAT_LEFTHANDPROGRESS)
		{
			return "LeftHandProgress";
		}
		if (hash == FLOAT_RADIOCOMMAND)
		{
			return "RadioCommand";
		}
		if (hash == FLOAT_RELTYPENEW)
		{
			return "RelTypeNew";
		}
		if (hash == FLOAT_RELTYPEOLD)
		{
			return "RelTypeOld";
		}
		if (hash == FLOAT_SHELLSINWEAPON)
		{
			return "ShellsInWeapon";
		}
		if (hash == FLOAT_SHOULDERREACH)
		{
			return "ShoulderReach";
		}
		if (hash == FLOAT_SPEEDDRAW)
		{
			return "SpeedDraw";
		}
		if (hash == FLOAT_SPEEDFIX)
		{
			return "SpeedFix";
		}
		if (hash == FLOAT_SPEEDRELOAD)
		{
			return "SpeedReload";
		}
		if (hash == FLOAT_STOCKANIMATIONINDEX)
		{
			return "StockAnimationIndex";
		}
		if (hash == FLOAT_SWINGSPEED)
		{
			return "SwingSpeed";
		}
		if (hash == FLOAT_THIRDPERSON)
		{
			return "ThirdPerson";
		}
		if (hash == FLOAT_UNDERMOD)
		{
			return "UnderMod";
		}
		if (hash == FLOAT_USETIMEMULTIPLIER)
		{
			return "UseTimeMultiplier";
		}
		if (hash == FLOAT_WEAPONLEVEL)
		{
			return "WeaponLevel";
		}
		if (hash == FLOAT_CAMORAINDEX)
		{
			return "CamoraIndex";
		}
		if (hash == FLOAT_CAMORAINDEXFORLOADAMMO)
		{
			return "CamoraIndexForLoadAmmo";
		}
		if (hash == FLOAT_CAMORAINDEXWITHSHELLFORREMOVE)
		{
			return "CamoraIndexWithShellForRemove";
		}
		if (hash == FLOAT_CAMORAINDEXFORUNLOADAMMO)
		{
			return "CamoraIndexForRemoveAmmo";
		}
		if (hash == FLOAT_CAMORAFIREINDEX)
		{
			return "CamoraFireIndex";
		}
		if (hash == FLOAT_CHAMBERINDEX)
		{
			return "ChamberIndex";
		}
		if (hash == FLOAT_CHAMBERINDEX_WITH_SHELL)
		{
			return "ChamberIndexWithShell";
		}
		if (hash == INT_ANIMATIONVARIANT)
		{
			return "AnimationVariant";
		}
		if (hash == INT_FIREVAR)
		{
			return "FireVar";
		}
		if (hash == INT_GRENADEALTFIRE)
		{
			return "GrenadeAltFire";
		}
		if (hash == INT_GRENADEFIRE)
		{
			return "GrenadeFire";
		}
		if (hash == INT_LACTIONINDEX)
		{
			return "LActionIndex";
		}
		if (hash == INT_PLAYERSTATE)
		{
			return "PlayerState";
		}
		if (hash == INT_THIRDACTION)
		{
			return "ThirdAction";
		}
		if (hash == INT_MALFUNCTION)
		{
			return "Malfunction";
		}
		if (hash == INT_MALFTYPE)
		{
			return "MalfType";
		}
		if (hash == TRIGGER_BUTTERFINGERS)
		{
			return "Butterfingers";
		}
		if (hash == TRIGGER_CHECKAMMO)
		{
			return "CheckAmmo";
		}
		if (hash == TRIGGER_CHECKCHAMBER)
		{
			return "CheckChamber";
		}
		if (hash == TRIGGER_FIREMODECHECK)
		{
			return "FiremodeCheck";
		}
		if (hash == TRIGGER_FIREMODESWITCH)
		{
			return "FiremodeSwitch";
		}
		if (hash == TRIGGER_HAMMERARMED)
		{
			return "HammerArmed";
		}
		if (hash == TRIGGER_FIRINGBULLET)
		{
			return "FiringBullet";
		}
		if (hash == TRIGGER_GESTURE)
		{
			return "Gesture";
		}
		if (hash == TRIGGER_GOMELE)
		{
			return "GoMele";
		}
		if (hash == TRIGGER_HANDREADY)
		{
			return "HandReady";
		}
		if (hash == TRIGGER_IDLETIME)
		{
			return "IdleTime";
		}
		if (hash == TRIGGER_LOOK)
		{
			return "Look";
		}
		if (hash == TRIGGER_MAGINFROMINV)
		{
			return "MagInFromInv";
		}
		if (hash == TRIGGER_MAGOUTFROMINV)
		{
			return "MagOutFromInv";
		}
		if (hash == TRIGGER_MAGSWAPEND)
		{
			return "MagSwapEnd";
		}
		if (hash == TRIGGER_MODSWITCH)
		{
			return "ModSwitch";
		}
		if (hash == TRIGGER_RADIO)
		{
			return "Radio";
		}
		if (hash == TRIGGER_RELOAD)
		{
			return "Reload";
		}
		if (hash == TRIGGER_RELOADFAST)
		{
			return "ReloadFast";
		}
		if (hash == TRIGGER_STOCKSWITCH)
		{
			return "StockSwitch";
		}
		if (hash == BOOL_FIREMODE_SPRINT)
		{
			return "FiremodeSprint";
		}
		if (hash == BOOL_SPRINT)
		{
			return "Sprint";
		}
		return "undefined";
	}

	public static void SetActive(IAnimator animator, bool active)
	{
		animator.SetBool(BOOL_ACTIVE, active);
	}

	public static void SetAddAmmoInChamber(IAnimator animator, bool addAmmoInChamber)
	{
		animator.SetBool(BOOL_ADDAMMOINCHAMBER, addAmmoInChamber);
	}

	public static void SetAddAmmoInMag(IAnimator animator, bool addAmmoInMag)
	{
		animator.SetBool(BOOL_ADDAMMOINMAG, addAmmoInMag);
	}

	public static void SetAltFire(IAnimator animator, bool altFire)
	{
		animator.SetBool(BOOL_ALTFIRE, altFire);
	}

	public static void SetArm(IAnimator animator, bool arm)
	{
		animator.SetBool(BOOL_ARM, arm);
	}

	public static void SetArmed(IAnimator animator, bool armed)
	{
		animator.SetBool(BOOL_ARMED, armed);
	}

	public static void SetBoltActionReload(IAnimator animator, bool boltActionReload)
	{
		animator.SetBool(BOOL_BOLTACTIONRELOAD, boltActionReload);
	}

	public static void SetBoltCatch(IAnimator animator, bool boltCatch)
	{
		animator.SetBool(BOOL_BOLTCATCH, boltCatch);
	}

	public static void SetCanReload(IAnimator animator, bool canReload)
	{
		animator.SetBool(BOOL_CANRELOAD, canReload);
	}

	public static void SetDelAmmoChamber(IAnimator animator, bool delAmmoChamber)
	{
		animator.SetBool(BOOL_DELAMMOCHAMBER, delAmmoChamber);
	}

	public static void SetDelAmmoFromMag(IAnimator animator, bool delAmmoFromMag)
	{
		animator.SetBool(BOOL_DELAMMOFROMMAG, delAmmoFromMag);
	}

	public static void SetDisarm(IAnimator animator, bool disarm)
	{
		animator.SetBool(BOOL_DISARM, disarm);
	}

	public static void SetDischarge(IAnimator animator, bool discharge)
	{
		animator.SetBool(BOOL_DISCHARGE, discharge);
	}

	public static void SetFastHide(IAnimator animator, bool fastHide)
	{
		animator.SetBool(BOOL_FASTHIDE, fastHide);
	}

	public static void SetFire(IAnimator animator, bool fire)
	{
		animator.SetBool(BOOL_FIRE, fire);
	}

	public static void SetSprint(IAnimator animator, bool value)
	{
		animator.SetBool(BOOL_SPRINT, value);
	}

	public static void SetGrip(IAnimator animator, bool grip)
	{
		animator.SetBool(BOOL_GRIP, grip);
	}

	public static void SetHvat(IAnimator animator, bool hvat)
	{
		animator.SetBool(BOOL_HVAT, hvat);
	}

	public static void SetIdle(IAnimator animator, bool idle)
	{
		animator.SetBool(BOOL_IDLE, idle);
	}

	public static void SetIncompatibleAmmo(IAnimator animator, bool incompatibleAmmo)
	{
		animator.SetBool(BOOL_INCOMPATIBLEAMMO, incompatibleAmmo);
	}

	public static void SetInventory(IAnimator animator, bool inventory)
	{
		animator.SetBool(BOOL_INVENTORY, inventory);
	}

	public static void SetIsExternalMag(IAnimator animator, bool isExternalMag)
	{
		animator.SetBool(BOOL_ISEXTERNALMAG, isExternalMag);
	}

	public static void SetLauncherReady(IAnimator animator, bool launcherReady)
	{
		animator.SetBool(BOOL_LAUNCHERREADY, launcherReady);
	}

	public static void SetLoadOne(IAnimator animator, bool loadOne)
	{
		animator.SetBool(BOOL_LOADONE, loadOne);
	}

	public static void SetMagFull(IAnimator animator, bool magFull)
	{
		animator.SetBool(BOOL_MAGFULL, magFull);
	}

	public static void SetMagIn(IAnimator animator, bool magIn)
	{
		animator.SetBool(BOOL_MAGIN, magIn);
	}

	public static void SetMagInWeapon(IAnimator animator, bool magInWeapon)
	{
		animator.SetBool(BOOL_MAGINWEAPON, magInWeapon);
	}

	public static void SetMagInWeaponFloat(IAnimator animator, bool magInWeapon)
	{
		float value = (magInWeapon ? 1f : 0f);
		animator.SetFloat(FLOAT_MAGINWEAPONFLOAT, value);
	}

	public static void SetMagOut(IAnimator animator, bool magOut)
	{
		animator.SetBool(BOOL_MAGOUT, magOut);
	}

	public static void SetMagSwap(IAnimator animator, bool magSwap)
	{
		animator.SetBool(BOOL_MAGSWAP, magSwap);
	}

	public static void SetMalfunction(IAnimator animator, int val)
	{
		animator.SetInteger(INT_MALFUNCTION, val);
	}

	public static void SetMalfunctionType(IAnimator animator, int val)
	{
		animator.SetInteger(INT_MALFTYPE, val);
	}

	public static void SetChamberIndexForLoadAmmo(IAnimator animator, float val)
	{
		animator.SetFloat(FLOAT_CHAMBERINDEX, val);
	}

	public static void SetChamberIndexWithShell(IAnimator animator, float val)
	{
		animator.SetFloat(FLOAT_CHAMBERINDEX_WITH_SHELL, val);
	}

	public static void SetMalfunctionRepair(IAnimator animator, bool val)
	{
		animator.SetBool(BOOL_MALFUNCTIONREPAIR, val);
	}

	public static void SetMisfireSlideUnknown(IAnimator animator, bool val)
	{
		animator.SetBool(BOOL_MISFIRESLIDE_UNKNOWN, val);
	}

	public static void SetRechamber(IAnimator animator, bool val)
	{
		animator.SetBool(BOOL_RECHAMBER, val);
	}

	public static void SetModSet(IAnimator animator, bool modSet)
	{
		animator.SetBool(BOOL_MODSET, modSet);
	}

	public static void SetOffBoltCatch(IAnimator animator, bool offBoltCatch)
	{
		animator.SetBool(BOOL_OFFBOLTCATCH, offBoltCatch);
	}

	public static void SetOnBoltCatch(IAnimator animator, bool onBoltCatch)
	{
		animator.SetBool(BOOL_ONBOLTCATCH, onBoltCatch);
	}

	public static void SetPatrol(IAnimator animator, bool patrol)
	{
		animator.SetBool(BOOL_PATROL, patrol);
	}

	public static void SetQuickFire(IAnimator animator, bool quickFire)
	{
		animator.SetBool(BOOL_QUICKFIRE, quickFire);
	}

	public static void SetSetFiremode0(IAnimator animator, bool setFiremode0)
	{
		animator.SetBool(BOOL_SETFIREMODE0, setFiremode0);
	}

	public static void SetSetFiremode1(IAnimator animator, bool setFiremode1)
	{
		animator.SetBool(BOOL_SETFIREMODE1, setFiremode1);
	}

	public static void SetShellEject(IAnimator animator, bool shellEject)
	{
		animator.SetBool(BOOL_SHELLEJECT, shellEject);
	}

	public static void SetStockFolded(IAnimator animator, bool stockFolded)
	{
		animator.SetBool(BOOL_STOCKFOLDED, stockFolded);
	}

	public static void SetUseLeftHand(IAnimator animator, bool useLeftHand)
	{
		animator.SetBool(BOOL_USELEFTHAND, useLeftHand);
	}

	public static void SetRollToZeroCamora(IAnimator animator, bool value)
	{
		animator.SetBool(BOOL_ROLLTOZEROCAMORA, value);
	}

	public static void SetRollCylinder(IAnimator animator, bool value)
	{
		animator.SetBool(BOOL_ROLLCYLINDER, value);
	}

	public static void SetDoubleAction(IAnimator animator, float value)
	{
		animator.SetFloat(FLOAT_DOUBLEACTION, value);
	}

	public static void SetAim_angle(IAnimator animator, float aim_angle)
	{
		animator.SetFloat(FLOAT_AIM_ANGLE, aim_angle);
	}

	public static void SetAimingFloat(IAnimator animator, float aimingFloat)
	{
		animator.SetFloat(FLOAT_IS_AIMING, aimingFloat);
	}

	public static void SetPrevAimingFloat(IAnimator animator, float prevAimingFloat)
	{
		animator.SetFloat(FLOAT_PREV_AIMING_STATE, prevAimingFloat);
	}

	public static void SetAimingInTrigger(IAnimator animator)
	{
		animator.SetTrigger(TRIGGER_AIMING_IN);
	}

	public static void ResetAimingInTrigger(IAnimator animator)
	{
		animator.ResetTrigger(TRIGGER_AIMING_IN);
	}

	public static void SetAimingOutTrigger(IAnimator animator)
	{
		animator.SetTrigger(TRIGGER_AIMING_OUT);
	}

	public static void ResetAimingOutTrigger(IAnimator animator)
	{
		animator.ResetTrigger(TRIGGER_AIMING_OUT);
	}

	public static void SetAmmoInChamber(IAnimator animator, float ammoInChamber)
	{
		animator.SetFloat(FLOAT_AMMOINCHAMBER, ammoInChamber);
	}

	public static void SetAmmoInMag(IAnimator animator, float ammoInMag)
	{
		animator.SetFloat(FLOAT_AMMOINMAG, ammoInMag);
	}

	public static void SetMasteringReloadAborted(IAnimator animator, bool reloadAborted)
	{
		animator.SetBool(BOOL_MASTERINGRELOADABORTED, reloadAborted);
	}

	public static void SetAmmoCountForRemove(IAnimator animator, float ammoInMag)
	{
		animator.SetFloat(FLOAT_AMMOCOUNTFORREMOVE, ammoInMag);
	}

	public static void SetCharacterID(IAnimator animator, float characterID)
	{
		animator.SetFloat(FLOAT_CHARACTERID, characterID);
	}

	public static void SetDeflected(IAnimator animator, float deflected)
	{
		animator.SetFloat(FLOAT_DEFLECTED, deflected);
	}

	public static void SetEmptyLinksCount(IAnimator animator, float emptyLinksCount)
	{
		animator.SetFloat(FLOAT_EMPTYLINKSCOUNT, emptyLinksCount);
	}

	public static void SetFireMode(IAnimator animator, float fireMode)
	{
		animator.SetFloat(FLOAT_FIREMODE, fireMode);
	}

	public static void SetGestureIndex(IAnimator animator, float gestureIndex)
	{
		animator.SetFloat(FLOAT_GESTUREINDEX, gestureIndex);
	}

	public static void SetGripWeight(IAnimator animator, float gripWeight)
	{
		animator.SetFloat(FLOAT_GRIPWEIGHT, gripWeight);
	}

	public static void SetIdlePosition(IAnimator animator, float idlePosition)
	{
		animator.SetFloat(FLOAT_IDLEPOSITION, idlePosition);
	}

	public static void SetIdleVar(IAnimator animator, float idleVar)
	{
		animator.SetFloat(FLOAT_IDLEVAR, idleVar);
	}

	public static void SetLauncherID(IAnimator animator, float launcherID)
	{
		animator.SetFloat(FLOAT_LAUNCHERID, launcherID);
	}

	public static void SetLeftHandProgress(IAnimator animator, float leftHandProgress)
	{
		animator.SetFloat(FLOAT_LEFTHANDPROGRESS, leftHandProgress);
	}

	public static void SetRadioCommand(IAnimator animator, float radioCommand)
	{
		animator.SetFloat(FLOAT_RADIOCOMMAND, radioCommand);
	}

	public static void SetRelTypeNew(IAnimator animator, float relTypeNew)
	{
		animator.SetFloat(FLOAT_RELTYPENEW, relTypeNew);
	}

	public static void SetRelTypeOld(IAnimator animator, float relTypeOld)
	{
		animator.SetFloat(FLOAT_RELTYPEOLD, relTypeOld);
	}

	public static void SetModAnimationUniqueId(IAnimator animator, int id)
	{
		animator.SetInteger(INT_MOD_ANIMATION_ID, id);
		animator.SetFloat(FLOAT_ANIMATION_MOD_ID_FLOAT, (id > 0) ? 1f : 0f);
	}

	public static void SetShellsInWeapon(IAnimator animator, float shellsInWeapon)
	{
		animator.SetFloat(FLOAT_SHELLSINWEAPON, shellsInWeapon);
	}

	public static void SetShoulderReach(IAnimator animator, float shoulderReach)
	{
		animator.SetFloat(FLOAT_SHOULDERREACH, shoulderReach);
	}

	public static void SetSpeedDraw(IAnimator animator, float speedDraw)
	{
		animator.SetFloat(FLOAT_SPEEDDRAW, speedDraw);
	}

	public static void SetSpeedFix(IAnimator animator, float speedFix)
	{
		animator.SetFloat(FLOAT_SPEEDFIX, speedFix);
	}

	public static void SetSpeedReload(IAnimator animator, float speedReload)
	{
		animator.SetFloat(FLOAT_SPEEDRELOAD, speedReload);
	}

	public static void SetStockAnimationIndex(IAnimator animator, float stockAnimationIndex)
	{
		animator.SetFloat(FLOAT_STOCKANIMATIONINDEX, stockAnimationIndex);
	}

	public static void SetSwingSpeed(IAnimator animator, float swingSpeed)
	{
		animator.SetFloat(FLOAT_SWINGSPEED, swingSpeed);
	}

	public static void SetThirdPerson(IAnimator animator, float thirdPerson)
	{
		animator.SetFloat(FLOAT_THIRDPERSON, thirdPerson);
	}

	public static void SetUnderMod(IAnimator animator, float underMod)
	{
		animator.SetFloat(FLOAT_UNDERMOD, underMod);
	}

	public static void SetUseTimeMultiplier(IAnimator animator, float useTimeMultiplier)
	{
		animator.SetFloat(FLOAT_USETIMEMULTIPLIER, useTimeMultiplier);
	}

	public static void SetUnderbarrelSightingRange(IAnimator animator, float val)
	{
		animator.SetFloat(FLOAT_SIGHTINGRANGE, val);
	}

	public static void SetWeaponLevel(IAnimator animator, float weaponLevel)
	{
		animator.SetFloat(FLOAT_WEAPONLEVEL, weaponLevel);
	}

	public static void SetCamoraIndex(IAnimator animator, float camoraIndex)
	{
		animator.SetFloat(FLOAT_CAMORAINDEX, camoraIndex);
	}

	public static void SetCamoraFireIndex(IAnimator animator, float camoraFireIndex)
	{
		animator.SetFloat(FLOAT_CAMORAFIREINDEX, camoraFireIndex);
	}

	public static void SetCamoraIndexForLoadAmmo(IAnimator animator, float camoraIndex)
	{
		animator.SetFloat(FLOAT_CAMORAINDEXFORLOADAMMO, camoraIndex);
	}

	public static void SetCamoraIndexWithShellForRemove(IAnimator animator, float camoraIndex)
	{
		animator.SetFloat(FLOAT_CAMORAINDEXWITHSHELLFORREMOVE, camoraIndex);
	}

	public static void SetCamoraIndexForUnloadAmmo(IAnimator animator, float camoraIndex)
	{
		animator.SetFloat(FLOAT_CAMORAINDEXFORUNLOADAMMO, camoraIndex);
	}

	public static void SetAnimationVariant(IAnimator animator, int animationVariant)
	{
		animator.SetInteger(INT_ANIMATIONVARIANT, animationVariant);
	}

	public static void SetNextLimb(IAnimator animator, bool nextLimb)
	{
		animator.SetBool(BOOL_NEXT_LIMB, nextLimb);
	}

	public static void SetFireVar(IAnimator animator, int fireVar)
	{
		animator.SetInteger(INT_FIREVAR, fireVar);
	}

	public static void SetGrenadeAltFire(IAnimator animator, int grenadeAltFire)
	{
		animator.SetInteger(INT_GRENADEALTFIRE, grenadeAltFire);
	}

	public static void SetGrenadeFire(IAnimator animator, int grenadeFire)
	{
		animator.SetInteger(INT_GRENADEFIRE, grenadeFire);
	}

	public static void SetLActionIndex(IAnimator animator, int lActionIndex)
	{
		animator.SetInteger(INT_LACTIONINDEX, lActionIndex);
	}

	public static void SetPlayerState(IAnimator animator, int playerState)
	{
		animator.SetInteger(INT_PLAYERSTATE, playerState);
	}

	public static void SetThirdAction(IAnimator animator, int thirdAction)
	{
		animator.SetInteger(INT_THIRDACTION, thirdAction);
	}

	public static void TriggerButterfingers(IAnimator animator)
	{
		animator.SetTrigger(TRIGGER_BUTTERFINGERS);
	}

	public static void ResetTriggerButterfingers(IAnimator animator)
	{
		animator.ResetTrigger(TRIGGER_BUTTERFINGERS);
	}

	public static void TriggerCheckAmmo(IAnimator animator)
	{
		animator.SetTrigger(TRIGGER_CHECKAMMO);
	}

	public static void ResetTriggerCheckAmmo(IAnimator animator)
	{
		animator.ResetTrigger(TRIGGER_CHECKAMMO);
	}

	public static void TriggerCheckChamber(IAnimator animator)
	{
		animator.SetTrigger(TRIGGER_CHECKCHAMBER);
	}

	public static void ResetTriggerCheckChamber(IAnimator animator)
	{
		animator.ResetTrigger(TRIGGER_CHECKCHAMBER);
	}

	public static void TriggerFiremodeCheck(IAnimator animator)
	{
		animator.SetTrigger(TRIGGER_FIREMODECHECK);
	}

	public static void ResetTriggerFiremodeCheck(IAnimator animator)
	{
		animator.ResetTrigger(TRIGGER_FIREMODECHECK);
	}

	public static void TriggerFiremodeSwitch(IAnimator animator)
	{
		animator.SetTrigger(TRIGGER_FIREMODESWITCH);
	}

	public static void TriggerHammerArmed(IAnimator animator)
	{
		animator.SetTrigger(TRIGGER_HAMMERARMED);
	}

	public static void ResetTriggerFiremodeSwitch(IAnimator animator)
	{
		animator.ResetTrigger(TRIGGER_FIREMODESWITCH);
	}

	public static void TriggerFiringBullet(IAnimator animator)
	{
		animator.SetTrigger(TRIGGER_FIRINGBULLET);
	}

	public static void ResetTriggerFiringBullet(IAnimator animator)
	{
		animator.ResetTrigger(TRIGGER_FIRINGBULLET);
	}

	public static void TriggerGesture(IAnimator animator)
	{
		animator.SetTrigger(TRIGGER_GESTURE);
	}

	public static void ResetTriggerGesture(IAnimator animator)
	{
		animator.ResetTrigger(TRIGGER_GESTURE);
	}

	public static void TriggerGoMele(IAnimator animator)
	{
		animator.SetTrigger(TRIGGER_GOMELE);
	}

	public static void ResetTriggerGoMele(IAnimator animator)
	{
		animator.ResetTrigger(TRIGGER_GOMELE);
	}

	public static void TriggerHandReady(IAnimator animator)
	{
		animator.SetTrigger(TRIGGER_HANDREADY);
	}

	public static void ResetTriggerHandReady(IAnimator animator)
	{
		animator.ResetTrigger(TRIGGER_HANDREADY);
	}

	public static void TriggerIdleTime(IAnimator animator)
	{
		animator.SetTrigger(TRIGGER_IDLETIME);
	}

	public static void ResetTriggerIdleTime(IAnimator animator)
	{
		animator.ResetTrigger(TRIGGER_IDLETIME);
	}

	public static void TriggerLook(IAnimator animator)
	{
		animator.SetTrigger(TRIGGER_LOOK);
	}

	public static void ResetTriggerLook(IAnimator animator)
	{
		animator.ResetTrigger(TRIGGER_LOOK);
	}

	public static void TriggerMagInFromInv(IAnimator animator)
	{
		animator.SetTrigger(TRIGGER_MAGINFROMINV);
	}

	public static void ResetTriggerMagInFromInv(IAnimator animator)
	{
		animator.ResetTrigger(TRIGGER_MAGINFROMINV);
	}

	public static void TriggerMagOutFromInv(IAnimator animator)
	{
		animator.SetTrigger(TRIGGER_MAGOUTFROMINV);
	}

	public static void ResetTriggerMagOutFromInv(IAnimator animator)
	{
		animator.ResetTrigger(TRIGGER_MAGOUTFROMINV);
	}

	public static void TriggerMagSwapEnd(IAnimator animator)
	{
		animator.SetTrigger(TRIGGER_MAGSWAPEND);
	}

	public static void ResetTriggerMagSwapEnd(IAnimator animator)
	{
		animator.ResetTrigger(TRIGGER_MAGSWAPEND);
	}

	public static void TriggerModSwitch(IAnimator animator)
	{
		animator.SetTrigger(TRIGGER_MODSWITCH);
	}

	public static void ResetTriggerModSwitch(IAnimator animator)
	{
		animator.ResetTrigger(TRIGGER_MODSWITCH);
	}

	public static void TriggerRadio(IAnimator animator)
	{
		animator.SetTrigger(TRIGGER_RADIO);
	}

	public static void ResetTriggerRadio(IAnimator animator)
	{
		animator.ResetTrigger(TRIGGER_RADIO);
	}

	public static void TriggerReload(IAnimator animator)
	{
		animator.SetTrigger(TRIGGER_RELOAD);
	}

	public static void ResetTriggerReload(IAnimator animator)
	{
		animator.ResetTrigger(TRIGGER_RELOAD);
	}

	public static void TriggerReloadFast(IAnimator animator)
	{
		animator.SetTrigger(TRIGGER_RELOADFAST);
	}

	public static void ResetTriggerReloadFast(IAnimator animator)
	{
		animator.ResetTrigger(TRIGGER_RELOADFAST);
	}

	public static void TriggerStockSwitch(IAnimator animator)
	{
		animator.SetTrigger(TRIGGER_STOCKSWITCH);
	}

	public static void ResetTriggerStockSwitch(IAnimator animator)
	{
		animator.ResetTrigger(TRIGGER_STOCKSWITCH);
	}

	public static void SetBipod(IAnimator animator, bool value)
	{
		animator.SetBool(BOOL_BIPOD, value);
	}

	public static void SetMounted(IAnimator animator, bool value)
	{
		animator.SetBool(BOOL_MOUNTED, value);
	}

	public static bool GetBoolActive(IAnimator animator)
	{
		return animator.GetBool(BOOL_ACTIVE);
	}

	public static bool GetBoolAddAmmoInChamber(IAnimator animator)
	{
		return animator.GetBool(BOOL_ADDAMMOINCHAMBER);
	}

	public static bool GetBoolAddAmmoInMag(IAnimator animator)
	{
		return animator.GetBool(BOOL_ADDAMMOINMAG);
	}

	public static bool GetBoolAltFire(IAnimator animator)
	{
		return animator.GetBool(BOOL_ALTFIRE);
	}

	public static bool GetBoolArm(IAnimator animator)
	{
		return animator.GetBool(BOOL_ARM);
	}

	public static bool GetBoolArmed(IAnimator animator)
	{
		return animator.GetBool(BOOL_ARMED);
	}

	public static bool GetBoolBoltActionReload(IAnimator animator)
	{
		return animator.GetBool(BOOL_BOLTACTIONRELOAD);
	}

	public static bool GetBoolBoltCatch(IAnimator animator)
	{
		return animator.GetBool(BOOL_BOLTCATCH);
	}

	public static bool GetBoolCanReload(IAnimator animator)
	{
		return animator.GetBool(BOOL_CANRELOAD);
	}

	public static bool GetBoolDelAmmoChamber(IAnimator animator)
	{
		return animator.GetBool(BOOL_DELAMMOCHAMBER);
	}

	public static bool GetBoolDelAmmoFromMag(IAnimator animator)
	{
		return animator.GetBool(BOOL_DELAMMOFROMMAG);
	}

	public static bool GetBoolDisarm(IAnimator animator)
	{
		return animator.GetBool(BOOL_DISARM);
	}

	public static bool GetBoolDischarge(IAnimator animator)
	{
		return animator.GetBool(BOOL_DISCHARGE);
	}

	public static bool GetBoolFastHide(IAnimator animator)
	{
		return animator.GetBool(BOOL_FASTHIDE);
	}

	public static bool GetBoolFire(IAnimator animator)
	{
		return animator.GetBool(BOOL_FIRE);
	}

	public static bool GetBoolGrip(IAnimator animator)
	{
		return animator.GetBool(BOOL_GRIP);
	}

	public static bool GetBoolHvat(IAnimator animator)
	{
		return animator.GetBool(BOOL_HVAT);
	}

	public static bool GetBoolIdle(IAnimator animator)
	{
		return animator.GetBool(BOOL_IDLE);
	}

	public static bool GetBoolIncompatibleAmmo(IAnimator animator)
	{
		return animator.GetBool(BOOL_INCOMPATIBLEAMMO);
	}

	public static bool GetBoolInventory(IAnimator animator)
	{
		return animator.GetBool(BOOL_INVENTORY);
	}

	public static bool GetBoolIsExternalMag(IAnimator animator)
	{
		return animator.GetBool(BOOL_ISEXTERNALMAG);
	}

	public static bool GetBoolLauncherReady(IAnimator animator)
	{
		return animator.GetBool(BOOL_LAUNCHERREADY);
	}

	public static bool GetBoolLoadOne(IAnimator animator)
	{
		return animator.GetBool(BOOL_LOADONE);
	}

	public static bool GetBoolMagFull(IAnimator animator)
	{
		return animator.GetBool(BOOL_MAGFULL);
	}

	public static bool GetBoolMagIn(IAnimator animator)
	{
		return animator.GetBool(BOOL_MAGIN);
	}

	public static bool GetBoolMagInWeapon(IAnimator animator)
	{
		return animator.GetBool(BOOL_MAGINWEAPON);
	}

	public static bool GetBoolMagOut(IAnimator animator)
	{
		return animator.GetBool(BOOL_MAGOUT);
	}

	public static bool GetBoolMagSwap(IAnimator animator)
	{
		return animator.GetBool(BOOL_MAGSWAP);
	}

	public static bool GetBoolModSet(IAnimator animator)
	{
		return animator.GetBool(BOOL_MODSET);
	}

	public static bool GetBoolOffBoltCatch(IAnimator animator)
	{
		return animator.GetBool(BOOL_OFFBOLTCATCH);
	}

	public static bool GetBoolOnBoltCatch(IAnimator animator)
	{
		return animator.GetBool(BOOL_ONBOLTCATCH);
	}

	public static bool GetBoolPatrol(IAnimator animator)
	{
		return animator.GetBool(BOOL_PATROL);
	}

	public static bool GetBoolQuickFire(IAnimator animator)
	{
		return animator.GetBool(BOOL_QUICKFIRE);
	}

	public static bool GetBoolSetFiremode0(IAnimator animator)
	{
		return animator.GetBool(BOOL_SETFIREMODE0);
	}

	public static bool GetBoolSetFiremode1(IAnimator animator)
	{
		return animator.GetBool(BOOL_SETFIREMODE1);
	}

	public static bool GetBoolShellEject(IAnimator animator)
	{
		return animator.GetBool(BOOL_SHELLEJECT);
	}

	public static bool GetBoolStockFolded(IAnimator animator)
	{
		return animator.GetBool(BOOL_STOCKFOLDED);
	}

	public static bool GetBoolUseLeftHand(IAnimator animator)
	{
		return animator.GetBool(BOOL_USELEFTHAND);
	}

	public static float GetFloatAim_angle(IAnimator animator)
	{
		return animator.GetFloat(FLOAT_AIM_ANGLE);
	}

	public static float GetFloatAmmoInChamber(IAnimator animator)
	{
		return animator.GetFloat(FLOAT_AMMOINCHAMBER);
	}

	public static float GetFloatAmmoInMag(IAnimator animator)
	{
		return animator.GetFloat(FLOAT_AMMOINMAG);
	}

	public static float GetFloatCharacterID(IAnimator animator)
	{
		return animator.GetFloat(FLOAT_CHARACTERID);
	}

	public static float GetFloatDeflected(IAnimator animator)
	{
		return animator.GetFloat(FLOAT_DEFLECTED);
	}

	public static float GetFloatEmptyLinksCount(IAnimator animator)
	{
		return animator.GetFloat(FLOAT_EMPTYLINKSCOUNT);
	}

	public static float GetFloatFireMode(IAnimator animator)
	{
		return animator.GetFloat(FLOAT_FIREMODE);
	}

	public static float GetFloatGestureIndex(IAnimator animator)
	{
		return animator.GetFloat(FLOAT_GESTUREINDEX);
	}

	public static float GetFloatGripWeight(IAnimator animator)
	{
		return animator.GetFloat(FLOAT_GRIPWEIGHT);
	}

	public static float GetFloatIdlePosition(IAnimator animator)
	{
		return animator.GetFloat(FLOAT_IDLEPOSITION);
	}

	public static float GetFloatIdleVar(IAnimator animator)
	{
		return animator.GetFloat(FLOAT_IDLEVAR);
	}

	public static float GetFloatLauncherID(IAnimator animator)
	{
		return animator.GetFloat(FLOAT_LAUNCHERID);
	}

	public static float GetFloatLeftHandProgress(IAnimator animator)
	{
		return animator.GetFloat(FLOAT_LEFTHANDPROGRESS);
	}

	public static float GetFloatRadioCommand(IAnimator animator)
	{
		return animator.GetFloat(FLOAT_RADIOCOMMAND);
	}

	public static float GetFloatRelTypeNew(IAnimator animator)
	{
		return animator.GetFloat(FLOAT_RELTYPENEW);
	}

	public static float GetFloatRelTypeOld(IAnimator animator)
	{
		return animator.GetFloat(FLOAT_RELTYPEOLD);
	}

	public static float GetFloatShellsInWeapon(IAnimator animator)
	{
		return animator.GetFloat(FLOAT_SHELLSINWEAPON);
	}

	public static float GetFloatShoulderReach(IAnimator animator)
	{
		return animator.GetFloat(FLOAT_SHOULDERREACH);
	}

	public static float GetFloatSpeedDraw(IAnimator animator)
	{
		return animator.GetFloat(FLOAT_SPEEDDRAW);
	}

	public static float GetFloatSpeedFix(IAnimator animator)
	{
		return animator.GetFloat(FLOAT_SPEEDFIX);
	}

	public static float GetFloatSpeedReload(IAnimator animator)
	{
		return animator.GetFloat(FLOAT_SPEEDRELOAD);
	}

	public static float GetFloatStockAnimationIndex(IAnimator animator)
	{
		return animator.GetFloat(FLOAT_STOCKANIMATIONINDEX);
	}

	public static float GetFloatSwingSpeed(IAnimator animator)
	{
		return animator.GetFloat(FLOAT_SWINGSPEED);
	}

	public static float GetFloatThirdPerson(IAnimator animator)
	{
		return animator.GetFloat(FLOAT_THIRDPERSON);
	}

	public static float GetFloatUnderMod(IAnimator animator)
	{
		return animator.GetFloat(FLOAT_UNDERMOD);
	}

	public static float GetFloatUseTimeMultiplier(IAnimator animator)
	{
		return animator.GetFloat(FLOAT_USETIMEMULTIPLIER);
	}

	public static float GetFloatWeaponLevel(IAnimator animator)
	{
		return animator.GetFloat(FLOAT_WEAPONLEVEL);
	}

	public static int GetIntAnimationVariant(IAnimator animator)
	{
		return animator.GetInteger(INT_ANIMATIONVARIANT);
	}

	public static int GetIntFireVar(IAnimator animator)
	{
		return animator.GetInteger(INT_FIREVAR);
	}

	public static int GetIntGrenadeAltFire(IAnimator animator)
	{
		return animator.GetInteger(INT_GRENADEALTFIRE);
	}

	public static int GetIntGrenadeFire(IAnimator animator)
	{
		return animator.GetInteger(INT_GRENADEFIRE);
	}

	public static int GetIntLActionIndex(IAnimator animator)
	{
		return animator.GetInteger(INT_LACTIONINDEX);
	}

	public static int GetIntPlayerState(IAnimator animator)
	{
		return animator.GetInteger(INT_PLAYERSTATE);
	}

	public static int GetIntThirdAction(IAnimator animator)
	{
		return animator.GetInteger(INT_THIRDACTION);
	}

	public static float GetCamoraIndex(IAnimator animator)
	{
		return animator.GetInteger(FLOAT_CAMORAINDEX);
	}

	public static int GetSightingRange(IAnimator animator)
	{
		return animator.GetInteger(FLOAT_SIGHTINGRANGE);
	}
}
