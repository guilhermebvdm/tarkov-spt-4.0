using EFT.InputSystem;
using EFT.InventoryLogic;
using EFT;
using UnityEngine;
using static TarkovIRL.AnimStateController;

namespace TarkovIRL
{
    internal class WeaponSelectionController
    {
        public enum EWeaponSelection { SLING, SHOULDER, PISTOL, OTHER }
        static EWeaponSelection _lastSelectedWeapon;

        static Vector3 _posLerp = Vector3.zero;
        static Vector3 _orderEndPos = Vector3.zero;
        static Vector3 _presentStartPos = Vector3.zero;

        static Vector3 _rotLerp = Vector3.zero;
        static Vector3 _orderEndRot = Vector3.zero;
        static Vector3 _presentStartRot = Vector3.zero;

        static Vector3 _posLerpHistory = Vector3.zero;
        static Vector3 _rotLerpHistory = Vector3.zero;
        static Vector3 _posLerpSmoothed = Vector3.zero;
        static Vector3 _rotLerpSmoothed = Vector3.zero;

        static readonly Vector3 Pistol_Start_Pos = new Vector3(0.05f, 0.1f, -0.2f);
        static readonly Vector3 Pistol_Start_Rot = new Vector3(0.1f, -0.2f, 0.05f);
        static readonly Vector3 Pistol_End_Pos = Vector3.zero;
        static readonly Vector3 Pistol_End_Rot = Vector3.zero;

        static readonly Vector3 Shoulder_Start_Pos = new Vector3(0.27f, -0.5f, -0.12f);
        static readonly Vector3 Shoulder_Start_Rot = new Vector3(-0.4f, -0.56f, 0f);
        static readonly Vector3 Shoulder_End_Pos = new Vector3(0.25f, -0.5f, -0.19f);
        static readonly Vector3 Shoulder_End_Rot = new Vector3(-0.4f, -0.56f, 0f);

        static readonly Vector3 Sling_Start_Pos = new Vector3(0.06f, -0.16f, -0.1f);
        static readonly Vector3 Sling_Start_Rot = new Vector3(0, -0.75f, -0.24f);
        static readonly Vector3 Sling_End_Pos = new Vector3(0.37f, -0.16f, -0.46f);
        static readonly Vector3 Sling_End_Rot = new Vector3(0, 1f, -0.24f);

        static readonly float TransformSmoothingDTMulti = 12f;

        static Player _player = null;

        static AnimationCurve _animSpeedCurvePhase1 = new AnimationCurve();
        static AnimationCurve _animSpeedCurvePhase2 = new AnimationCurve();

        static AnimationCurve _Flatcurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe (1f, 1f));
        static bool _transitionFirstFrame = false;
        static bool _transitionWindowOpen = false;
        static EWeaponState _previousState;

        public static void UpdateAnimationPump(float dt)
        {
            if (!PrimeMover.IsWeaponTrans.Value) return;

            _posLerpHistory.x += _posLerp.x;
            _posLerpHistory.y += _posLerp.y;
            _posLerpHistory.z += _posLerp.z;

            _posLerpHistory.x -= _posLerpSmoothed.x;
            _posLerpHistory.y -= _posLerpSmoothed.y;
            _posLerpHistory.z -= _posLerpSmoothed.z;

            _rotLerpHistory.x += _rotLerp.x;
            _rotLerpHistory.y += _rotLerp.y;
            _rotLerpHistory.z += _rotLerp.z;

            _rotLerpHistory.x -= _rotLerpSmoothed.x;
            _rotLerpHistory.y -= _rotLerpSmoothed.y;
            _rotLerpHistory.z -= _rotLerpSmoothed.z;

            _posLerpSmoothed.x = _posLerpHistory.x * dt * TransformSmoothingDTMulti;
            _posLerpSmoothed.y = _posLerpHistory.y * dt * TransformSmoothingDTMulti;
            _posLerpSmoothed.z = _posLerpHistory.z * dt * TransformSmoothingDTMulti;

            _rotLerpSmoothed.x = _rotLerpHistory.x * dt * TransformSmoothingDTMulti;
            _rotLerpSmoothed.y = _rotLerpHistory.y * dt * TransformSmoothingDTMulti;
            _rotLerpSmoothed.z = _rotLerpHistory.z * dt * TransformSmoothingDTMulti;


            if (_transitionFirstFrame)
            {
                _transitionFirstFrame = false;
                _transitionWindowOpen = true;
            }

            if (!_transitionWindowOpen)
            {
                return;
            }

            EWeaponState state = AnimStateController.WeaponState;
            float animProgress = _player.HandsAnimator.Animator.GetCurrentAnimatorStateInfo(1).normalizedTime;
            float efficiencyMod = Mathf.Clamp(EfficiencyController.EfficiencyModifierInverse, 0.5f, 1.5f);
            float proneMod = PlayerMotionController.IsProne ? 0.5f : 1f;
            float foldedStockMulti = WeaponController.IsStockFolded ? 1.5f : 1f;
            float finalSpeedMulti = efficiencyMod * proneMod * WeaponController.GetWeaponMulti(true) * PrimeMover.TransitionSpeedMulti.Value * foldedStockMulti;

            if (state == EWeaponState.ORDER_ARM)
            {
                float speed = _animSpeedCurvePhase1.Evaluate(animProgress) * finalSpeedMulti;
                SetAnimSpeed(_player, speed);
                _posLerp = Vector3.Lerp(Vector3.zero, _orderEndPos, animProgress);
                _rotLerp = Vector3.Lerp(Vector3.zero, _orderEndRot, animProgress);
                //UtilsTIRL.Log($" animProgress of {state} - {animProgress} at speed {speed}");
            }
            else if (state == EWeaponState.PRESENT_ARM)
            {
                float speed = _animSpeedCurvePhase2.Evaluate(animProgress) * finalSpeedMulti;
                SetAnimSpeed(_player, speed);
                _posLerp = Vector3.Lerp(_presentStartPos, Vector3.zero, animProgress);
                _rotLerp = Vector3.Lerp(_presentStartRot, Vector3.zero, animProgress);
                //UtilsTIRL.Log($" animProgress of {state} - {animProgress} at speed {speed}");

            }
            // cleanup transition
            if (_previousState == EWeaponState.PRESENT_ARM && state == EWeaponState.IDLE)
            {
                SetAnimSpeed(_player, 1f);
                _transitionWindowOpen = false;
                _posLerp = Vector3.zero;
                _rotLerp = Vector3.zero;
            }
            _previousState = state;

            //UtilsTIRL.Log($"transition open at state {state}/{AnimStateController.WeaponStateHash} - {animProgress}");

        }

        public static void GetWeaponSelectionTransforms(out Vector3 pos, out Quaternion rot)
        {
            pos = _posLerpSmoothed;
            rot = TIRLUtils.GetQuatFromV3(_rotLerpSmoothed);
        }

        public static void Process(ECommand command, Player player)
        {

            if (player == null)
            {
                return;
            }

            _player = player;
            if (AnimStateController.WeaponState == AnimStateController.EWeaponState.UNKNOWN_STATE)
            {
                return;
            }
            //UtilsTIRL.Log($"weapon state {AnimStateController.WeaponState}");
            //TIRLUtils.Log($"selection processing", true);

            Item newLastWeapon = player.LastEquippedWeaponOrKnifeItem;
            Item slingWeapon = player.Inventory.Equipment.GetSlot(EquipmentSlot.FirstPrimaryWeapon).ContainedItem;
            Item shoulderWeapon = player.Inventory.Equipment.GetSlot(EquipmentSlot.SecondPrimaryWeapon).ContainedItem;
            Item holsterWeapon = player.Inventory.Equipment.GetSlot(EquipmentSlot.Holster).ContainedItem;

            if (newLastWeapon == null)
            {
                player.TrySetLastEquippedWeapon(true);
                if (newLastWeapon == null)
                { 
                    return; 
                }
            }

            // TODO : no case for coming from nothing -- if you have no weapon at all and pick one up,
            // null ref and you can't proceed without doing something else with your hands

            if(slingWeapon != null)
            {
                if (newLastWeapon.Equals(slingWeapon))
                {
                    if (command == ECommand.SelectFirstPrimaryWeapon)
                    {
                        return;
                    }
                    _lastSelectedWeapon = EWeaponSelection.SLING;
                }
            }
            if (shoulderWeapon != null)
            {
                if (newLastWeapon.Equals(shoulderWeapon))
                {
                    if (command == ECommand.SelectSecondPrimaryWeapon)
                    {
                        return;
                    }
                    _lastSelectedWeapon = EWeaponSelection.SHOULDER;
                }
            }
            if (holsterWeapon != null)
            {
                if (newLastWeapon.Equals(holsterWeapon))
                {
                    if (command == ECommand.SelectSecondaryWeapon)
                    {
                        return;
                    }
                    _lastSelectedWeapon = EWeaponSelection.PISTOL;
                }
                if (newLastWeapon.Equals(holsterWeapon))
                {
                    if (command == ECommand.QuickSelectSecondaryWeapon)
                    {
                        if (slingWeapon == null)
                        {
                            return;
                        }
                    }
                    _lastSelectedWeapon = EWeaponSelection.PISTOL;
                }
            }

            // TODO: find cases and information for selecting from vest
            if (command == ECommand.SelectFirstPrimaryWeapon && slingWeapon == null)
            {
                return;
            }
            if (command == ECommand.SelectSecondPrimaryWeapon && shoulderWeapon == null)
            {
                return;
            }
            if (command == ECommand.SelectSecondaryWeapon && holsterWeapon == null)
            {
                return;
            }

            // a new process is ensured
            _transitionFirstFrame = true;

            // swap between sling and shoulder
            if (command == ECommand.SelectFirstPrimaryWeapon && _lastSelectedWeapon == EWeaponSelection.SHOULDER)
            {
                ProcessShoulderToSling();
            }
            if (command == ECommand.SelectSecondPrimaryWeapon && _lastSelectedWeapon == EWeaponSelection.SLING)
            {
                ProcessSlingToShoulder();
            }

            // quick swap bx sling and pistol
            if (command == ECommand.SelectSecondaryWeapon && _lastSelectedWeapon == EWeaponSelection.SLING)
            {
                ProcessSlingToPistol();
            }

            if (command == ECommand.QuickSelectSecondaryWeapon && _lastSelectedWeapon == EWeaponSelection.SLING)
            {
                ProcessQuickSlingToPistol();
            }

            // quick swap between pistol and shoulder should be disabled
            if (command == ECommand.SelectSecondaryWeapon && _lastSelectedWeapon == EWeaponSelection.SHOULDER)
            {
                ProcessShoulderToPistol();
            }
            if (command == ECommand.QuickSelectSecondaryWeapon && _lastSelectedWeapon == EWeaponSelection.SHOULDER)
            {
                ProcessShoulderToPistol();
            }

            // transition from pistol to shoulder
            if (command == ECommand.SelectSecondPrimaryWeapon && _lastSelectedWeapon == EWeaponSelection.PISTOL)
            {
                ProcessPistolToShoulder();
            }

            // pistol to sling
            if (command == ECommand.SelectFirstPrimaryWeapon && _lastSelectedWeapon == EWeaponSelection.PISTOL)
            {
                ProcessPistolToSling();
            }

            // transition to slot
            bool selectionFromSlot = command == ECommand.PressSlot0;
            selectionFromSlot &= command == ECommand.PressSlot4;
            selectionFromSlot &= command == ECommand.PressSlot5;
            selectionFromSlot &= command == ECommand.PressSlot6;
            selectionFromSlot &= command == ECommand.PressSlot7;
            selectionFromSlot &= command == ECommand.PressSlot8;
            selectionFromSlot &= command == ECommand.PressSlot9;
            selectionFromSlot &= command == ECommand.SelectFastSlot0;
            selectionFromSlot &= command == ECommand.SelectFastSlot4;
            selectionFromSlot &= command == ECommand.SelectFastSlot5;
            selectionFromSlot &= command == ECommand.SelectFastSlot6;
            selectionFromSlot &= command == ECommand.SelectFastSlot7;
            selectionFromSlot &= command == ECommand.SelectFastSlot8;
            selectionFromSlot &= command == ECommand.SelectFastSlot9;
            if (selectionFromSlot && _lastSelectedWeapon == EWeaponSelection.SHOULDER)
            {
                ProcessShoulderToSlot();
            }
        }

        static void ProcessShoulderToSling()
        {
            _animSpeedCurvePhase1 = PrimeMover.Instance.OrderShoulderCurve;
            _animSpeedCurvePhase2 = _Flatcurve;

            _orderEndPos = Shoulder_End_Pos;
            _orderEndRot = Shoulder_End_Rot;

            _presentStartPos = Sling_Start_Pos;
            _presentStartRot = Sling_Start_Rot;
        }

        static void ProcessSlingToShoulder()
        {
            _animSpeedCurvePhase1 = _Flatcurve;
            _animSpeedCurvePhase2 = PrimeMover.Instance.PresentShoulderCurve;

            _orderEndPos = Sling_End_Pos;
            _orderEndRot = Sling_End_Rot;

            _presentStartPos = Shoulder_Start_Pos;
            _presentStartRot = Shoulder_Start_Rot;
        }

        // pistol
        static void ProcessSlingToPistol()
        {
            _animSpeedCurvePhase1 = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));
            _animSpeedCurvePhase2 = new AnimationCurve(new Keyframe(0f, 0.3f), new Keyframe(1f, 0.3f));

            _orderEndPos = Sling_End_Pos;
            _orderEndRot = Sling_End_Rot;

            _presentStartPos = Pistol_Start_Pos;
            _presentStartRot = Pistol_Start_Rot;
        }

        static void ProcessQuickSlingToPistol()
        {
            _animSpeedCurvePhase1 = new AnimationCurve(new Keyframe(0f, 1.25f), new Keyframe(1f, 0.75f));
            _animSpeedCurvePhase2 = new AnimationCurve(new Keyframe(0f, 1.2f), new Keyframe(1f, 2f));

            _orderEndPos = Sling_End_Pos;
            _orderEndRot = Sling_End_Rot;

            _presentStartPos = Pistol_Start_Pos;
            _presentStartRot = Pistol_Start_Rot;
        }

        static void ProcessShoulderToPistol()
        {
            _animSpeedCurvePhase1 = PrimeMover.Instance.OrderShoulderCurve;
            _animSpeedCurvePhase2 = new AnimationCurve(new Keyframe(0f, 0.3f), new Keyframe(1f, 0.3f));

            _orderEndPos = Shoulder_End_Pos;
            _orderEndRot = Shoulder_End_Rot;

            _presentStartPos = Pistol_Start_Pos;
            _presentStartRot = Pistol_Start_Rot;
        }
        static void ProcessPistolToShoulder()
        {
            _animSpeedCurvePhase1 = new AnimationCurve(new Keyframe(0f, 0.25f), new Keyframe(1f, 0.25f));
            _animSpeedCurvePhase2 = PrimeMover.Instance.PresentShoulderCurve;

            _orderEndPos = Pistol_End_Pos;
            _orderEndRot = Pistol_End_Pos;

            _presentStartPos = Shoulder_Start_Pos;
            _presentStartRot = Shoulder_Start_Rot;

        }   
        static void ProcessPistolToSling()
        {
            _animSpeedCurvePhase1 = new AnimationCurve(new Keyframe(0f, 0.35f), new Keyframe(1f, 0.25f));
            _animSpeedCurvePhase2 = new AnimationCurve(new Keyframe(0f, 1.75f), new Keyframe(1f, 1.75f));

            _orderEndPos = Pistol_End_Pos;
            _orderEndRot = Pistol_End_Pos;

            _presentStartPos = Sling_Start_Pos;
            _presentStartRot = Sling_Start_Rot;
        }
        static void ProcessShoulderToSlot()
        {
            // not completely thought out
            _animSpeedCurvePhase1 = PrimeMover.Instance.OrderShoulderCurve;
            _animSpeedCurvePhase2 = _Flatcurve;

            _orderEndPos = Shoulder_End_Pos;
            _orderEndRot = Shoulder_End_Rot;

            _presentStartPos = Vector3.zero;
            _presentStartRot = Vector3.zero;

        }

        static void SetAnimSpeed(Player player, float speed)
        {
            if (player == null)
            {
                return;
            }
            player.HandsAnimator.SetAnimationSpeed(speed);
        }
    }
}
