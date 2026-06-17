using UnityEngine;
using System.Collections.Generic;
using CameraRotationMod;

namespace shwngFpsCameraStances
{
    public struct TransitionCurveSet
    {
        public AnimationCurve PitchXCurve;
        public AnimationCurve YawYCurve;   // (Tombo / Roll físico)
        public AnimationCurve RollZCurve;  // (Apontar / Yaw físico)
        public AnimationCurve PosXCurve;   // (Sideways)
        public AnimationCurve PosYCurve;   // (Forward/Backward)
        public AnimationCurve PosZCurve;   // (Up/Down)

        public static TransitionCurveSet CreateEmpty()
        {
            return new TransitionCurveSet
            {
                PitchXCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f)),
                YawYCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f)),
                RollZCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f)),
                PosXCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f)),
                PosYCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f)),
                PosZCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f))
            };
        }

        public void SmoothAll()
        {
            SmoothCurve(PitchXCurve);
            SmoothCurve(YawYCurve);
            SmoothCurve(RollZCurve);
            SmoothCurve(PosXCurve);
            SmoothCurve(PosYCurve);
            SmoothCurve(PosZCurve);
        }

        private static void SmoothCurve(AnimationCurve curve)
        {
            if (curve == null) return;
            for (int i = 0; i < curve.length; i++)
            {
                curve.SmoothTangents(i, 0f);
            }
        }
    }

    public static class StanceTransitionCurves
    {
        private static bool _initialized = false;
        
        // Dicionário mapeando a origem e destino para o set de curvas.
        private static Dictionary<(Stance, Stance), TransitionCurveSet> _curvesMap = new Dictionary<(Stance, Stance), TransitionCurveSet>();
        
        private static TransitionCurveSet _fallbackSet;
        private static TransitionCurveSet _adsInStance0Set;
        private static TransitionCurveSet _adsOutStance0Set;
        private static TransitionCurveSet _adsInStance1Set;
        private static TransitionCurveSet _adsOutStance1Set;
        private static TransitionCurveSet _adsInStance2Set;
        private static TransitionCurveSet _adsOutStance2Set;

        public static void Initialize()
        {
            if (_initialized) return;

            _fallbackSet = TransitionCurveSet.CreateEmpty();

            // 1. Hipfire para High Ready (Stance.Default -> Stance.Stance1)
            var hipfireToHighReady = TransitionCurveSet.CreateEmpty();
            hipfireToHighReady.PitchXCurve = new AnimationCurve(
                new Keyframe(0.0f, 0.0f),
                new Keyframe(0.15f, 6.5f),
                new Keyframe(0.40f, 10.0f),
                new Keyframe(0.75f, -1.5f),
                new Keyframe(0.85f, 0.5f),
                new Keyframe(1.0f, 0.0f)
            );
            // JSON "yawCurve" (Apontar Esq/Dir) -> RollZCurve no código
            hipfireToHighReady.RollZCurve = new AnimationCurve(
                new Keyframe(0.0f, 0.0f),
                new Keyframe(0.35f, 2.0f),
                new Keyframe(0.80f, -0.6f),
                new Keyframe(1.0f, 0.0f)
            );
            // JSON "rollCurve" (Tombar Arma) -> YawYCurve no código
            hipfireToHighReady.YawYCurve = new AnimationCurve(
                new Keyframe(0.0f, 0.0f),
                new Keyframe(0.45f, 3.2f),
                new Keyframe(0.80f, -0.2f),
                new Keyframe(1.0f, 0.0f)
            );
            // Modificado: O usuário solicitou que o impacto seja Forward/Backward (Eixo Y), não Up/Down
            hipfireToHighReady.PosYCurve = new AnimationCurve(
                new Keyframe(0.0f, 0.0f),
                new Keyframe(0.30f, -0.065f),
                new Keyframe(0.65f, 0.008f),
                new Keyframe(1.0f, 0.0f)
            );
            hipfireToHighReady.SmoothAll();
            _curvesMap.Add((Stance.Default, Stance.Stance1), hipfireToHighReady);

            // 2. High Ready para Hipfire (Stance.Stance1 -> Stance.Default)
            var highReadyToHipfire = TransitionCurveSet.CreateEmpty();
            highReadyToHipfire.PitchXCurve = new AnimationCurve(
                new Keyframe(0.0f, 0.0f),
                new Keyframe(0.20f, -8.5f),
                new Keyframe(0.55f, -12.5f),
                new Keyframe(0.85f, 1.5f),
                new Keyframe(1.0f, 0.0f)
            );
            // JSON "yawCurve" (Apontar Esq/Dir) -> RollZCurve
            highReadyToHipfire.RollZCurve = new AnimationCurve(
                new Keyframe(0.0f, 0.0f),
                new Keyframe(0.40f, -2.4f),
                new Keyframe(0.75f, 0.4f),
                new Keyframe(1.0f, 0.0f)
            );
            // Modificado: O usuário solicitou que o impacto seja Forward/Backward (Eixo Y)
            highReadyToHipfire.PosYCurve = new AnimationCurve(
                new Keyframe(0.0f, 0.0f),
                new Keyframe(0.35f, 0.045f),
                new Keyframe(0.80f, -0.006f),
                new Keyframe(1.0f, 0.0f)
            );
            highReadyToHipfire.SmoothAll();
            _curvesMap.Add((Stance.Stance1, Stance.Default), highReadyToHipfire);

            // 3. Hipfire para Low Ready (Stance.Default -> Stance.Stance2)
            var hipfireToLowReady = TransitionCurveSet.CreateEmpty();
            hipfireToLowReady.PitchXCurve = new AnimationCurve(
                new Keyframe(0.0f, 0.0f),
                new Keyframe(0.15f, -5.0f),
                new Keyframe(0.45f, -7.0f),
                new Keyframe(0.75f, 1.0f),
                new Keyframe(0.90f, -0.2f),
                new Keyframe(1.0f, 0.0f)
            );
            // JSON "forwardBackwardCurve" -> PosYCurve
            hipfireToLowReady.PosYCurve = new AnimationCurve(
                new Keyframe(0.0f, 0.0f),
                new Keyframe(0.30f, -0.065f),
                new Keyframe(0.70f, 0.012f),
                new Keyframe(1.0f, 0.0f)
            );
            // Removido PosXCurve para manter o foco apenas no kick Y
            hipfireToLowReady.SmoothAll();
            _curvesMap.Add((Stance.Default, Stance.Stance2), hipfireToLowReady);

            // 4. Low Ready para Hipfire (Stance.Stance2 -> Stance.Default)
            var lowReadyToHipfire = TransitionCurveSet.CreateEmpty();
            lowReadyToHipfire.PitchXCurve = new AnimationCurve(
                new Keyframe(0.0f, 0.0f),
                new Keyframe(0.20f, -6.0f),
                new Keyframe(0.50f, -9.0f),
                new Keyframe(0.80f, 1.2f),
                new Keyframe(1.0f, 0.0f)
            );
            // JSON "forwardBackwardCurve" -> PosYCurve
            lowReadyToHipfire.PosYCurve = new AnimationCurve(
                new Keyframe(0.0f, 0.0f),
                new Keyframe(0.35f, 0.045f), // Push
                new Keyframe(0.75f, -0.008f), // Pull
                new Keyframe(1.0f, 0.0f)
            );
            // Removido PosZCurve (Up/Down) para manter o foco no kick Forward/Backward
            lowReadyToHipfire.SmoothAll();
            _curvesMap.Add((Stance.Stance2, Stance.Default), lowReadyToHipfire);

            // ================== ADS CURVES (STANCE 0 - VANILLA) ==================
            _adsInStance0Set = TransitionCurveSet.CreateEmpty();
            _adsInStance0Set.PosYCurve = new AnimationCurve( // Forward/Backward kick
                new Keyframe(0.0f, 0.0f),
                new Keyframe(0.20f, -0.045f),
                new Keyframe(0.50f, -0.06f),
                new Keyframe(0.80f, 0.004f),
                new Keyframe(1.0f, 0.0f)
            );
            _adsInStance0Set.PosZCurve = new AnimationCurve( // Up/Down
                new Keyframe(0.0f, 0.0f),
                new Keyframe(0.30f, 0.015f),
                new Keyframe(0.70f, -0.002f),
                new Keyframe(1.0f, 0.0f)
            );
            _adsInStance0Set.PitchXCurve = new AnimationCurve( // Cano levanta
                new Keyframe(0.0f, 0.0f),
                new Keyframe(0.25f, -2.0f),
                new Keyframe(0.65f, 0.2f),
                new Keyframe(1.0f, 0.0f)
            );
            _adsInStance0Set.YawYCurve = new AnimationCurve( // Aponta levemente pro lado
                new Keyframe(0.0f, 0.0f),
                new Keyframe(0.35f, 1.5f),
                new Keyframe(0.75f, -0.2f),
                new Keyframe(1.0f, 0.0f)
            );
            _adsInStance0Set.RollZCurve = new AnimationCurve( // Tomba levemente a arma
                new Keyframe(0.0f, 0.0f),
                new Keyframe(0.40f, -1.0f),
                new Keyframe(0.80f, 0.2f),
                new Keyframe(1.0f, 0.0f)
            );
            _adsInStance0Set.SmoothAll();

            _adsOutStance0Set = TransitionCurveSet.CreateEmpty();
            _adsOutStance0Set.PosYCurve = new AnimationCurve(
                new Keyframe(0.0f, 0.0f),
                new Keyframe(0.25f, -0.03f),
                new Keyframe(0.70f, 0.004f),
                new Keyframe(1.0f, 0.0f)
            );
            _adsOutStance0Set.PosZCurve = new AnimationCurve(
                new Keyframe(0.0f, 0.0f),
                new Keyframe(0.30f, -0.022f),
                new Keyframe(0.80f, 0.004f),
                new Keyframe(1.0f, 0.0f)
            );
            _adsOutStance0Set.PitchXCurve = new AnimationCurve(
                new Keyframe(0.0f, 0.0f),
                new Keyframe(0.30f, 2.4f),
                new Keyframe(0.75f, -0.4f),
                new Keyframe(1.0f, 0.0f)
            );
            _adsOutStance0Set.YawYCurve = new AnimationCurve(
                new Keyframe(0.0f, 0.0f),
                new Keyframe(0.35f, -1.5f),
                new Keyframe(0.75f, 0.2f),
                new Keyframe(1.0f, 0.0f)
            );
            _adsOutStance0Set.RollZCurve = new AnimationCurve(
                new Keyframe(0.0f, 0.0f),
                new Keyframe(0.40f, 1.0f),
                new Keyframe(0.80f, -0.2f),
                new Keyframe(1.0f, 0.0f)
            );
            _adsOutStance0Set.SmoothAll();

            // ================== ADS CURVES (STANCE 1 - HIGH READY) ==================
            // Origin is already high, so we dive down instead of lifting
            _adsInStance1Set = TransitionCurveSet.CreateEmpty();
            _adsInStance1Set.PosYCurve = new AnimationCurve( // Forward/Backward
                new Keyframe(0.0f, 0.0f),
                new Keyframe(0.20f, 0.0f),
                new Keyframe(0.30f, -0.1f), // Afasta a arma do rosto
                new Keyframe(0.75f, 0.01f), 
                new Keyframe(1.0f, 0.0f)
            );
            _adsInStance1Set.PosZCurve = new AnimationCurve( // Up/Down
                new Keyframe(0.0f, 0.0f),
                new Keyframe(0.15f, 0.0f),
                new Keyframe(0.35f, 0.08f), // Mergulha a arma por baixo da visão
                new Keyframe(0.85f, -0.01f),
                new Keyframe(1.0f, 0.0f)
            );
            _adsInStance1Set.PitchXCurve = new AnimationCurve( // Cano sobe/desce
                new Keyframe(0.0f, 0.0f),
                new Keyframe(0.25f, 16.0f), // Chute extremo para forçar o cano pra baixo (deitar a arma)
                new Keyframe(0.55f, 5.0f),
                new Keyframe(0.80f, -1.5f),
                new Keyframe(1.0f, 0.0f)
            );
            _adsInStance1Set.YawYCurve = new AnimationCurve(
                new Keyframe(0.0f, 0.0f),
                new Keyframe(0.40f, 1.2f),
                new Keyframe(0.80f, -0.3f),
                new Keyframe(1.0f, 0.0f)
            );
            _adsInStance1Set.RollZCurve = new AnimationCurve(
                new Keyframe(0.0f, 0.0f),
                new Keyframe(0.40f, -2.5f),
                new Keyframe(0.80f, 0.5f),
                new Keyframe(1.0f, 0.0f)
            );
            _adsInStance1Set.SmoothAll();

            _adsOutStance1Set = TransitionCurveSet.CreateEmpty();
            _adsOutStance1Set.PosYCurve = new AnimationCurve(
                new Keyframe(0.0f, 0.0f),
                new Keyframe(0.25f, -0.03f),
                new Keyframe(0.70f, 0.004f),
                new Keyframe(1.0f, 0.0f)
            );
            _adsOutStance1Set.PosZCurve = new AnimationCurve(
                new Keyframe(0.0f, 0.0f),
                new Keyframe(0.30f, 0.022f), // Lifts up
                new Keyframe(0.80f, -0.004f),
                new Keyframe(1.0f, 0.0f)
            );
            _adsOutStance1Set.PitchXCurve = new AnimationCurve(
                new Keyframe(0.0f, 0.0f),
                new Keyframe(0.30f, -2.4f), // Barrel lifts
                new Keyframe(0.75f, 0.4f),
                new Keyframe(1.0f, 0.0f)
            );
            _adsOutStance1Set.YawYCurve = new AnimationCurve(
                new Keyframe(0.0f, 0.0f),
                new Keyframe(0.35f, -1.5f),
                new Keyframe(0.75f, 0.2f),
                new Keyframe(1.0f, 0.0f)
            );
            _adsOutStance1Set.RollZCurve = new AnimationCurve(
                new Keyframe(0.0f, 0.0f),
                new Keyframe(0.40f, 1.0f),
                new Keyframe(0.80f, -0.2f),
                new Keyframe(1.0f, 0.0f)
            );
            _adsOutStance1Set.SmoothAll();

            // ================== ADS CURVES (STANCE 2 - LOW READY) ==================
            // Identical to Stance 0 for now
            _adsInStance2Set = _adsInStance0Set;
            _adsOutStance2Set = _adsOutStance0Set;

            _initialized = true;
        }

        private static TransitionCurveSet GetCurveSet(Stance from, Stance to)
        {
            if (!_initialized) Initialize();
            
            var key = (from, to);
            if (_curvesMap.TryGetValue(key, out TransitionCurveSet set))
            {
                return set;
            }
            
            // Fallbacks
            if (to == Stance.Stance1 && _curvesMap.TryGetValue((Stance.Default, Stance.Stance1), out var fallbackHigh))
            {
                return fallbackHigh;
            }
            if (to == Stance.Stance2 && _curvesMap.TryGetValue((Stance.Default, Stance.Stance2), out var fallbackLow))
            {
                return fallbackLow;
            }
            if (to == Stance.Default && _curvesMap.TryGetValue((Stance.Stance1, Stance.Default), out var fallbackDefault))
            {
                return fallbackDefault; 
            }

            return _fallbackSet;
        }

        public static Vector3 EvaluateRotation(float progress, Stance from, Stance to)
        {
            var set = GetCurveSet(from, to);
            return new Vector3(
                set.PitchXCurve.Evaluate(progress),
                set.YawYCurve.Evaluate(progress),
                set.RollZCurve.Evaluate(progress)
            );
        }

        public static Vector3 EvaluatePosition(float progress, Stance from, Stance to)
        {
            var set = GetCurveSet(from, to);
            return new Vector3(
                set.PosXCurve.Evaluate(progress),
                set.PosYCurve.Evaluate(progress),
                set.PosZCurve.Evaluate(progress)
            );
        }

        public static Vector3 EvaluateADSRotation(float progress, bool isEnteringAds, Stance originStance)
        {
            if (!_initialized) Initialize();
            TransitionCurveSet set;
            if (originStance == Stance.Stance1) set = isEnteringAds ? _adsInStance1Set : _adsOutStance1Set;
            else if (originStance == Stance.Stance2) set = isEnteringAds ? _adsInStance2Set : _adsOutStance2Set;
            else set = isEnteringAds ? _adsInStance0Set : _adsOutStance0Set;

            return new Vector3(
                set.PitchXCurve.Evaluate(progress),
                set.YawYCurve.Evaluate(progress),
                set.RollZCurve.Evaluate(progress)
            );
        }

        public static Vector3 EvaluateADSPosition(float progress, bool isEnteringAds, Stance originStance)
        {
            if (!_initialized) Initialize();
            TransitionCurveSet set;
            if (originStance == Stance.Stance1) set = isEnteringAds ? _adsInStance1Set : _adsOutStance1Set;
            else if (originStance == Stance.Stance2) set = isEnteringAds ? _adsInStance2Set : _adsOutStance2Set;
            else set = isEnteringAds ? _adsInStance0Set : _adsOutStance0Set;

            return new Vector3(
                set.PosXCurve.Evaluate(progress),
                set.PosYCurve.Evaluate(progress),
                set.PosZCurve.Evaluate(progress)
            );
        }
    }
}
