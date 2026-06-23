using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using EFT;
using System.Security.Policy;

namespace TarkovIRL
{
    internal class PlayerMotionController
    {
        public enum EPlayerDir { FWD, BWD, LEFT, RIGHT, FWDLEFT, FWDRIGHT, BWDLEFT, BWDRIGHT, NONE };
        static EPlayerDir _dir;

        //vars
        static Vector2 _playerRotLastFrame = Vector2.zero;
        static Vector3 _playerPosLastFrame = Vector3.zero;

        static float _playerRotationHistory = 0;
        static float _playerRotationAvg = 0;

        static float _horizontalRotationHistory = 0;
        static float _horizontalRotationValue = 0;

        static float _verticalRotationHistory = 0;
        static float _verticalRotationValue = 0;

        static bool _playerMoving = false;
        static float _verticalAvg = 0;
        static float _playerSpeed = 0;
        static float _leanNormalized = 0;
        static float _armStam = 0;

        static public bool IsAiming = false;
        static public bool IsSprinting = false;
        static public bool IsProne = false;
        static public bool IsHoldingBreath = false;
        static public bool IsAugmentedBreath = false;

        static Vector3 _lastPositionRecorded = Vector3.zero;
        static Vector2 _lastRotationRecorded = Vector2.zero;

        public static void UpdateMovementMeasurementsInFDT(float fdt)
        {
            

            UpdateIsMovingBool(_lastPositionRecorded);
            UpdateRotationEngine(_lastRotationRecorded, fdt);
        }

        public static void UpdateMovementInformation(Player player)
        {
            _lastPositionRecorded = player.Position;
            _lastRotationRecorded = player.Rotation;
            IsAiming = player.ProceduralWeaponAnimation.IsAiming;
            IsProne = player.IsInPronePose;
            IsSprinting = player.IsSprintEnabled;
            _playerSpeed = player.Speed;
            _leanNormalized = player.MovementContext.Tilt / 5f;
            _armStam = player.Physical.HandsStamina.NormalValue;
            UpdateMovementDirection(player.InputDirection);
            //TIRLUtils.Log($"player input: {player.InputDirection}", true);
        }
        static void UpdateIsMovingBool(Vector3 position)
        {
            float dist = Vector3.Distance(position, _playerPosLastFrame);
            if (dist > PrimeMover.MotionTrackingThreshold.Value)
            {
                _playerMoving = true;
            }
            else
            {
                _playerMoving = false;
            }
            _playerPosLastFrame = position;
        }

        static void UpdateRotationEngine(Vector2 newRot, float dt)
        {
            //
            // One could say this function is the heart of the mod, if there is one
            //

            // vertical 
            _verticalAvg = newRot.y > _playerRotLastFrame.y ? 1f : -1f;

            float distance = Vector2.Distance(newRot, _playerRotLastFrame) * dt;
            float horizontalMovement = newRot.x - _playerRotLastFrame.x;
            float verticalMovement = newRot.y - _playerRotLastFrame.y;

            horizontalMovement *= dt;
            verticalMovement *= dt;

            if (horizontalMovement < -1f)
            {
                horizontalMovement = 0;
            }
            if (horizontalMovement > 1f) 
            {
                horizontalMovement = 0;
            }

            // general rot detection
            _playerRotationHistory += distance;
            _playerRotationHistory -= _playerRotationAvg;
            _playerRotationHistory = Mathf.Clamp(_playerRotationHistory, 0, PrimeMover.RotationHistoryClamp.Value);
            _playerRotationAvg = _playerRotationHistory * dt * PrimeMover.RotationAverageDTMulti.Value;

            // horizontal tracking
            _horizontalRotationHistory += horizontalMovement;
            _horizontalRotationHistory -= _horizontalRotationValue;
            _horizontalRotationValue = _horizontalRotationHistory * dt * PrimeMover.RotationAverageDTMulti.Value;

            // vertical tracking
            _verticalRotationHistory += verticalMovement;
            _verticalRotationHistory -= _verticalRotationValue;
            _verticalRotationValue = _verticalRotationHistory * dt * PrimeMover.RotationAverageDTMulti.Value;

            // set for next frame
            _playerRotLastFrame = newRot;
        }

        public static float GetNormalSpeed()
        {
            return _playerSpeed / .6f; ;
        }

        public static float VerticalTrend
        {
            get { return _verticalAvg; }
        }

        public static bool IsPlayerMovement
        {
            get { return _playerMoving; }
        }

        public static float RotationDelta
        {
            get { return _playerRotationAvg; }
        }

        public static float HorizontalRotationDelta
        {
            get { return _horizontalRotationValue; }
        }

        public static float VerticalRotationDelta
        {
            get { return _verticalRotationValue; }
        }

        public static float LeanNormal
        {
            get
            {
                return _leanNormalized;
            }
        }

        static void UpdateMovementDirection(Vector3 playerInput)
        {
            bool forward = playerInput.y > 0;
            bool backward = playerInput.y < 0;
            bool leftward = playerInput.x < 0;
            bool rightward = playerInput.x > 0;

            if (!_playerMoving)
            {
                _dir = EPlayerDir.NONE;
                //TIRLUtils.Log($"player not moving", false);
                return;
            }

            if (forward && !leftward && !rightward) _dir = EPlayerDir.FWD;
            else if (forward && leftward) _dir = EPlayerDir.FWDLEFT;
            else if (forward && rightward) _dir = EPlayerDir.FWDRIGHT;
            else if (backward && !leftward && !rightward) _dir = EPlayerDir.BWD;
            else if (backward && leftward) _dir = EPlayerDir.BWDLEFT;
            else if (backward && rightward) _dir = EPlayerDir.BWDRIGHT;
            else if (leftward) _dir = EPlayerDir.LEFT;
            else if (rightward) _dir = EPlayerDir.RIGHT;
            else _dir = EPlayerDir.NONE;
        }

        public static EPlayerDir Direction
        {
            get { return _dir; }
        }

        public static float ArmStamNorm
        {
            get { return _armStam; }
        }
    }
}
