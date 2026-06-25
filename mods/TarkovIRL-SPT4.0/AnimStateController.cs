// Decompiled with JetBrains decompiler
// Type: TarkovIRL.AnimStateController
// Assembly: TarkovIRL, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C42939BD-7BF0-4586-ABE5-9D2EFC361A0B
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\TarkovIRL_WeaponsHandlingMod_1.0.0\BepInEx\plugins\TarkovIRL.dll

#nullable disable
namespace TarkovIRL;

internal class AnimStateController
{
  private static AnimStateController.EWeaponState _weaponState;
  public static readonly int _Blindfire1 = -1271366218;
  public static readonly int _Blindfire2 = 1276948056;
  public static readonly int _TurnState1 = -31136456;
  public static readonly int _TurnState2 = 287005718;
  public static readonly int _LeftShoulderStance = 0;
  public static readonly int _SideStepLeft = 731208140;
  public static readonly int _SideStepRight = 1696653403;
  public static readonly int _Running = -244792566;
  public static readonly int _WeaponIdle = -1479210739;
  public static readonly int _CheckMag = 351790385;
  public static readonly int _EjectLastRound = -1717115204;
  public static readonly int _ReachForMag = 1898839857;
  public static readonly int _HandOnMag = 1051831248;
  public static readonly int _PullMag = -1191338705;
  public static readonly int _ReplaceMag = -1821557201;
  public static readonly int _ReturnHandAfterCharging = -507049588;
  public static readonly int _ReturnHandWithoutCharging = 32228103;
  public static readonly int _ReturnHandNoMag = 1392741434;
  public static readonly int _FastReloadDumpMag = -1162171678;
  public static readonly int _WithdrawWeapon = -1205675207;
  public static readonly int _WithdrawWeaponQuick = -896401840;
  public static readonly int _PresentWeapon = -965243548;
  public static readonly int _Shoot = 590329303;
  public static readonly int _TriggerPull = -1160784120;
  public static readonly int _CheckChamber = -131286203;
  private static int _currentBodyStateHash = 0;
  private static int _currentWeaponStateHash = 0;

  public static void SetCurrentBodyAnimState(int state)
  {
    AnimStateController._currentBodyStateHash = state;
  }

  public static void SetCurrentWeaponAnimState(int state)
  {
    AnimStateController._currentWeaponStateHash = state;
    if (state == AnimStateController._HandOnMag)
      AnimStateController._weaponState = AnimStateController.EWeaponState.INTO_RELOAD;
    else if (state == AnimStateController._PullMag)
      AnimStateController._weaponState = AnimStateController.EWeaponState.MID_RELOAD;
    else if (state == AnimStateController._ReplaceMag)
      AnimStateController._weaponState = AnimStateController.EWeaponState.MID_RELOAD_2;
    else if (state == AnimStateController._ReturnHandAfterCharging || state == AnimStateController._ReturnHandWithoutCharging || state == AnimStateController._ReturnHandNoMag)
      AnimStateController._weaponState = AnimStateController.EWeaponState.OUT_OF_RELOAD;
    else if (state == AnimStateController._CheckMag)
      AnimStateController._weaponState = AnimStateController.EWeaponState.CHECK_MAG;
    else if (state == AnimStateController._FastReloadDumpMag)
      AnimStateController._weaponState = AnimStateController.EWeaponState.RELOAD_FAST;
    else if (state == AnimStateController._WeaponIdle)
      AnimStateController._weaponState = AnimStateController.EWeaponState.IDLE;
    else if (state == AnimStateController._WithdrawWeapon || state == AnimStateController._WithdrawWeaponQuick)
      AnimStateController._weaponState = AnimStateController.EWeaponState.ORDER_ARM;
    else if (state == AnimStateController._PresentWeapon)
      AnimStateController._weaponState = AnimStateController.EWeaponState.PRESENT_ARM;
    else if (state == AnimStateController._Shoot)
      AnimStateController._weaponState = AnimStateController.EWeaponState.SHOOT;
    else if (state == AnimStateController._TriggerPull)
      AnimStateController._weaponState = AnimStateController.EWeaponState.TRIGGER_PULL;
    else if (state == AnimStateController._CheckChamber)
      AnimStateController._weaponState = AnimStateController.EWeaponState.CHECK_CHAMBER;
    else
      AnimStateController._weaponState = AnimStateController.EWeaponState.UNKNOWN_STATE;
  }

  public static bool IsTurning
  {
    get
    {
      return AnimStateController._currentBodyStateHash == AnimStateController._TurnState1 || AnimStateController._currentBodyStateHash == AnimStateController._TurnState2;
    }
  }

  public static bool IsBlindfire
  {
    get
    {
      return AnimStateController._currentBodyStateHash == AnimStateController._Blindfire1 || AnimStateController._currentBodyStateHash == AnimStateController._Blindfire2;
    }
  }

  public static bool IsLeftShoulder
  {
    get => AnimStateController._currentBodyStateHash == AnimStateController._LeftShoulderStance;
  }

  public static bool IsSideStep
  {
    get
    {
      return AnimStateController._currentBodyStateHash == AnimStateController._SideStepLeft || AnimStateController._currentBodyStateHash == AnimStateController._SideStepRight;
    }
  }

  public static bool IsRunning => AnimStateController._currentBodyStateHash == -244792566;

  public static AnimStateController.EWeaponState WeaponState => AnimStateController._weaponState;

  public static int WeaponStateHash => AnimStateController._currentWeaponStateHash;

  public enum EWeaponState
  {
    INTO_RELOAD,
    MID_RELOAD,
    MID_RELOAD_2,
    OUT_OF_RELOAD,
    IDLE,
    CHECK_MAG,
    RELOAD_FAST,
    UNKNOWN_STATE,
    ORDER_ARM,
    PRESENT_ARM,
    TRIGGER_PULL,
    CHECK_CHAMBER,
    SHOOT,
  }
}
