using EFT;
using EFT.UI;
using RealismMod;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace TarkovIRL
{
    internal static class EfficiencyController
    {
        // readonlys
        static readonly float _LerpSpeed = 1f;

        // stacking condiitions
        static readonly int _HeavyBleedHash = -1302691610;//updated
        static readonly int _LightBleedHash = -1302691609;//updated
        static readonly int _FreshWoundHash = -2109260636;
        static readonly int _BoneBreakHash = -1302691612;//updated

        // binary conditions
        static readonly int _TremorsHash = -139892197;//updated
        static readonly int _PainHash = -139892193;//updated
        static readonly int _FatigueHash = -543176719;//updated
        static readonly int _ToxicationHash = -1705976133;//updated

        static readonly int[] _NominalEffectStates = new int[] { -1705976135, 1022907221 , -139892192 };
        static readonly int[] _KnownEffectStates = new int[] { -1302691609, -1302691612, -139892197, -139892193 , -1302691610 , -543176719 , -1705976133 };

        // lerps
        static float _efficiencyLerpTarget = 0;
        static float _efficiencyLerpTargetWithoutWeight = 0;
        static float _efficiencyLerp = 0;
        static float _efficiencyLastFrame = 0;

        static float debugTimer = 0;

        static Dictionary<Injury, float> _injuryTimes = new Dictionary<Injury, float>();

        static int _heavyBleedCountLastFrame = 0;
        static int _lightBleedCountLastFrame = 0;
        static int _boneBreakCountLastFrame = 0;

        public static void UpdateEfficiencyLerp(float dt)
        {
            float efficiencyLerpMulti = _efficiencyLerpTarget < _efficiencyLastFrame ? 1f / _efficiencyLerpTarget : _efficiencyLerpTarget;
            _efficiencyLerp = Mathf.Lerp(_efficiencyLerp, _efficiencyLerpTarget, dt * _LerpSpeed * PrimeMover.EfficiencyLerpMulti.Value * efficiencyLerpMulti);
            _efficiencyLastFrame = _efficiencyLerpTarget;

            debugTimer += dt;
            if (debugTimer > 0.2f)
            {
                debugTimer = 0;
            }
        }

        static void CheckInjuryChange(int boneBreakCount, int heavyBleedCount, int lightBleedCount)
        {
            if (boneBreakCount > _boneBreakCountLastFrame)
            {
                int breaksToAdd = boneBreakCount - _boneBreakCountLastFrame;
                for (int i = 0; i < breaksToAdd; i++)
                {
                    AddInjuryToEffects(new Injury(Injury.EInjury.BONE_BREAK, Time.time));
                }
            }
            if (boneBreakCount < _boneBreakCountLastFrame)
            {
                RemoveInjuryEffect(Injury.EInjury.BONE_BREAK);
            }

            if (heavyBleedCount > _heavyBleedCountLastFrame)
            {
                int heavyBleedsToAdd = heavyBleedCount - _heavyBleedCountLastFrame;
                for (int i = 0; i < heavyBleedsToAdd; i++)
                {
                    AddInjuryToEffects(new Injury(Injury.EInjury.HEAVY_BLEED, Time.time));
                }
            }
            if (heavyBleedCount < _heavyBleedCountLastFrame)
            {
                RemoveInjuryEffect(Injury.EInjury.HEAVY_BLEED);
            }

            if (lightBleedCount > _lightBleedCountLastFrame)
            {
                int lightBleedsToAdd = lightBleedCount - _lightBleedCountLastFrame;
                for (int i = 0; i < lightBleedsToAdd; i++)
                {
                    AddInjuryToEffects(new Injury(Injury.EInjury.LIGHT_BLEED, Time.time));
                }
            }
            if (lightBleedCount < _lightBleedCountLastFrame)
            {
                RemoveInjuryEffect(Injury.EInjury.LIGHT_BLEED);
            }
            _boneBreakCountLastFrame = boneBreakCount;
            _heavyBleedCountLastFrame = heavyBleedCount;
            _lightBleedCountLastFrame = lightBleedCount;
        }

        static float ReturnInjuryEffectCoef()
        {
            float effectWeight = 0;
            foreach (var injury in _injuryTimes)
            {
                float timeSinceInjury = Time.time - injury.Key.TimeInflicted;
                float injuryEffectNorm = Mathf.Clamp01(timeSinceInjury / injury.Key.TimeUntilEffect);
                effectWeight += injury.Key.InjuryWeight * injuryEffectNorm;
            }
            float totalEffect = GetInjuryImpact(effectWeight);
            TIRLUtils.Log($"total injury effect is ({totalEffect})", false);
            return totalEffect;
        }

        static void AddInjuryToEffects(Injury injury)
        {
            _injuryTimes.Add(injury, Time.time);
        }

        static void RemoveInjuryEffect(Injury.EInjury injuryType)
        {
            Injury earliestInjuryOfType = null;
            foreach (var oldInjury in _injuryTimes)
            {
                if (earliestInjuryOfType == null && oldInjury.Key.InjuryType == injuryType)
                {
                    earliestInjuryOfType = oldInjury.Key;
                }
                else if (earliestInjuryOfType != null && oldInjury.Key.InjuryType == injuryType && oldInjury.Key.TimeInflicted < earliestInjuryOfType.TimeInflicted)
                {
                    earliestInjuryOfType = oldInjury.Key;
                }
            }
            if (earliestInjuryOfType != null)
            {
                _injuryTimes.Remove(earliestInjuryOfType);
            }
        }

        public static void UpdateEfficiency(Player player)
        {
            //
            // get raw values
            //
            float hydrationNorm = player.HealthController.Hydration.Normalized;
            float nutritionNorm = player.HealthController.Energy.Normalized;
            float overWeightVal = 1f - Mathf.Clamp01(player.Physical.Overweight);
            //
            int freshWoundCount = 0;
            int heavyBleedCount = 0;
            int lightBleedCount = 0;
            int boneBreakCount = 0;
            //
            int tremorCount = 0;
            int painCount = 0;
            int fatigueCount = 0;
            int intoxCount = 0;
            //
            foreach (var effect in player.HealthController.GetAllEffects())
            {
                if (!IsEffectKnown(effect, _NominalEffectStates) && !IsEffectKnown(effect, _KnownEffectStates))
                {
                    TIRLUtils.Log($"effect type {effect}({effect.Type.FullName.GetHashCode()}) on bodypart {effect.BodyPart}", false);
                }
                if (effect.Type.FullName.GetHashCode() == _HeavyBleedHash) heavyBleedCount++;
                if (effect.Type.FullName.GetHashCode() == _LightBleedHash) lightBleedCount++;
                if (effect.Type.FullName.GetHashCode() == _FreshWoundHash) freshWoundCount++;
                if (effect.Type.FullName.GetHashCode() == _BoneBreakHash) boneBreakCount++;
                //
                if (effect.Type.FullName.GetHashCode() == _TremorsHash) tremorCount++;
                if (effect.Type.FullName.GetHashCode() == _PainHash) painCount++;
                if (effect.Type.FullName.GetHashCode() == _FatigueHash) fatigueCount++;
                //
                if (effect.Type.FullName.GetHashCode() == _ToxicationHash) intoxCount++;
            }
            TIRLUtils.Log($"heavyBleedCount {heavyBleedCount}, lightBleedCount {lightBleedCount}, freshWoundCount {freshWoundCount}, boneBreakCount {boneBreakCount} , tremorCount {tremorCount}, painCount {painCount}, fatigueCount {fatigueCount}, intoxCount {intoxCount}", false);

            CheckInjuryChange(boneBreakCount, heavyBleedCount, lightBleedCount);

            float healthCommon = player.HealthController.GetBodyPartHealth(EBodyPart.Common).Normalized;
            float stamNormalized = player.Physical.Stamina.NormalValue;
            float handStamNormalized = player.Physical.HandsStamina.NormalValue;

            //
            // compute outcomes
            //
            float hydroMulti = GetInjuryImpact(hydrationNorm, 10f);
            float nutritionMulti = GetInjuryImpact(nutritionNorm, 10f);
            float overWeightMulti = GetInjuryImpact(overWeightVal, PrimeMover.EfficiencyOverweightIpmact.Value);
            //
            float healthMulti = GetInjuryImpact(healthCommon, 40f);
            float stamMulti = GetInjuryImpact(stamNormalized, 20f);
            float handStamMulti = GetInjuryImpact(handStamNormalized, 20f);
            //
            float woundMulti = GetInjuryImpact(5f, freshWoundCount);
            //
            float tremorMulti = GetInjuryImpact(5f, tremorCount);
            float painMulti = GetInjuryImpact(5f, painCount);
            float fatigueMulti = GetInjuryImpact(5f, fatigueCount);
            //
            float intoxMulti = GetInjuryImpact(10f, intoxCount);
            //
            float injuryMulti = ReturnInjuryEffectCoef() * woundMulti * tremorMulti * painMulti;
            injuryMulti = RealismWrapper.IsAdrenaline ? 1f : injuryMulti;
            hydroMulti = RealismWrapper.IsAdrenaline ? 1f : hydroMulti;
            nutritionMulti = RealismWrapper.IsAdrenaline ? 1f : nutritionMulti;
            float overdoseMulti = RealismWrapper.IsOverdose ? 1.2f : 1f;

            //
            // negative effects
            //
            float negativeEffects = hydroMulti * nutritionMulti * healthMulti * stamMulti * fatigueMulti * handStamMulti * injuryMulti * intoxMulti * overdoseMulti;
            //negativeEffects *= PrimeMover.ArtificalInjury.Value;

            // positive effects (incl from Realism)
            float adrenalineBuff = RealismWrapper.IsAdrenaline ? 0.5f : 1f;
            float sidestepDebuff = AnimStateController.IsSideStep ? 1.3f : 1f;
            float highReadyBuff = StanceController.CurrentStance == EStance.HighReady ? 0.85f : 1f;
            float lowReadyDebuff = StanceController.CurrentStance == EStance.LowReady ? 1.1f : 1f;
            float shortStockBuff = StanceController.CurrentStance == EStance.ShortStock ? 0.7f : 1f;
            float activeAimDebuff = StanceController.CurrentStance == EStance.ActiveAiming ? 1.1f : 1f;
            float mountedBuff = StanceController.IsMounting ? 0.5f : 1f;
            float leftShoulderDebuff = StanceController.IsLeftShoulder ? 1.1f : 1f;
            float sprintingmulti = player.IsSprintEnabled ? 1.5f : 1f;
            float speedMulti = 0.5f + (player.Speed * 0.5f);
            if (!PlayerMotionController.IsPlayerMovement)
            {
                speedMulti = 0.5f;
            }
            float poseMulti = 0.5f + (player.PoseLevel * 0.5f);
            float proneMulti = player.IsInPronePose ? 0.5f : 1f;

            // final mobility
            float positiveEffects = speedMulti * poseMulti * proneMulti * sprintingmulti * highReadyBuff * lowReadyDebuff * mountedBuff * shortStockBuff * leftShoulderDebuff * activeAimDebuff * sidestepDebuff * adrenalineBuff;

            // final output
            _efficiencyLerpTarget = negativeEffects * positiveEffects * overWeightMulti;
            _efficiencyLerpTargetWithoutWeight = negativeEffects * positiveEffects;
            TIRLUtils.Log($"overWeightMulti {overWeightMulti}, healthMulti {healthMulti}, stamMulti {stamMulti}, injuryMulti {injuryMulti} , negativeEffects total {negativeEffects}", false);
        }

        public static float EfficiencyModifier
        {
            get { return _efficiencyLerp; }
        }

        public static float EfficiencyModifierInverse
        {
            get { return 1f / _efficiencyLerp; }
        }

        static float GetInjuryImpact(float value, float impactWeightPercent)
        {
            float impactWeightAdjusted = impactWeightPercent * 0.01f * PrimeMover.EfficiencyInjuryImpact.Value;
            return 1f + ((1f - value) * impactWeightAdjusted);
        }

        static float GetInjuryImpact(float impactWeightPercent, int multipleInstances)
        {
            if (multipleInstances == 0)
            {
                return 1f;
            }

            float impactWeightAdjusted = impactWeightPercent * 0.01f * PrimeMover.EfficiencyInjuryImpact.Value;
            float finalEffectImpact = (1f + impactWeightAdjusted) * multipleInstances;
            return finalEffectImpact;
        }

        static float GetInjuryImpact(float impactWeightPercent)
        {
            float impactWeightAdjusted = impactWeightPercent * 0.01f * PrimeMover.EfficiencyInjuryImpact.Value;
            float finalEffectImpact = (1f + impactWeightAdjusted);
            return finalEffectImpact;
        }

        static bool IsEffectKnown(IEffect effect, int[] effectsArray)
        {
            foreach (var stateHash in effectsArray)
            {
                if (effect.Type.FullName.GetHashCode() == stateHash)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
