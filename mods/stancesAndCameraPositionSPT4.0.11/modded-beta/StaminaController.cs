using System;
using System.Reflection;
using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using HarmonyLib;
using UnityEngine;

namespace CameraRotationMod
{
    /// <summary>
    /// Autoridade ÚNICA da stamina de braço (HandsStamina) do jogador local — item 012.
    /// Roda 1×/frame no Plugin.Update: amostra o estado num único ponto, resolve UM StaminaScenario,
    /// e escreve hands.Current pelo multiplicador daquele cenário (o Process/Consume vanilla é neutralizado
    /// para o braço do MainPlayer enquanto ControllingHands). Absorve o antigo ArmStaminaCoordinator (fix-01).
    /// </summary>
    public enum StaminaScenario
    {
        Inactive,
        StandStance0, StandStance1, StandStance2, StandStance3, StandAds, StandHoldBreath,
        ProneHip, ProneAds, ProneHoldBreath,
        PassiveStance0, PassiveAds, PassiveHoldBreath,
        ActiveStance0, ActiveAds, ActiveHoldBreath
    }

    public static class StaminaController
    {
        public static StaminaScenario Current { get; private set; } = StaminaScenario.Inactive;
        public static string CurrentLabel { get; private set; } = "Inactive";
        public static bool ControllingHands { get; private set; }   // lido pelos Prefixes de neutralização
        private static StaminaScenario _prev = StaminaScenario.Inactive;

        // Índice = (int)StaminaScenario; preenchido por Plugin.BindStaminaManagement().
        public static ConfigEntry<float>[] Multipliers = new ConfigEntry<float>[16];

        // Backing fields dos eventos do GClass774 (re-disparados ao escrever Current para preservar tremor/barra).
        // ref: Assembly-CSharp/GClass774.cs:47 (action_1=OnThresholdPass), :53 (action_3=OnValueChanged)
        private static readonly FieldInfo _onThreshold = AccessTools.Field(typeof(GClass774), "action_1");
        private static readonly FieldInfo _onValueChanged = AccessTools.Field(typeof(GClass774), "action_3");
        private static bool _warnedFields;

        public static void Tick()
        {
            try
            {
                if (!StanceManager.IsActiveContext())
                {
                    SetScenario(StaminaScenario.Inactive);
                    ControllingHands = false;
                    return;
                }

                Player p = Singleton<GameWorld>.Instance?.MainPlayer;
                if (p == null) { SetScenario(StaminaScenario.Inactive); ControllingHands = false; return; }   // CR-01-01
                GClass774 hands = p.Physical?.HandsStamina;   // ref: BasePhysicalClass.cs:355
                // Sem arma de fogo em mãos → cede ao vanilla (spec 01: corner case mãos vazias).
                if (hands == null || !(p.HandsController is Player.FirearmController))
                {
                    SetScenario(StaminaScenario.Inactive);
                    ControllingHands = false;
                    return;
                }

                StaminaScenario s = Resolve(p);
                SetScenario(s);
                ControllingHands = true;

                ConfigEntry<float> cfg = Multipliers[(int)s];
                float mult = cfg != null ? cfg.Value : 1f;
                float delta = StanceManager.CachedAimDrainRate * (mult - 1f) * Time.deltaTime;

                float prev = hands.Current;   // ref: GClass774.cs:23
                float target = Mathf.Clamp(prev + delta, 0f, (float)hands.TotalCapacity);
                if (Mathf.Abs(target - prev) < 0.0001f) return;
                hands.Current = target;

                // Re-disparar eventos nativos (PA-01-02 null-guard). ref: GClass774.cs:261/268 (Consume), 364/367 (Process)
                if (_onValueChanged == null || _onThreshold == null)
                {
                    if (!_warnedFields) { _warnedFields = true; Plugin.Logger.LogWarning("[StaminaController] backing fields de evento não resolvidos — tremor/barra desativados."); }
                }
                else
                {
                    if ((int)prev != (int)target) (_onValueChanged.GetValue(hands) as Action)?.Invoke();           // barra
                    if ((prev >= 15f) != (target >= 15f)) (_onThreshold.GetValue(hands) as Action)?.Invoke();      // tremor (Exhausted<15, GClass774.cs:106)
                }
                if (delta < 0f && target <= 0f && prev > 0f) hands.HandleExpiration();   // ref: GClass774.cs:298
            }
            catch (Exception ex) { Plugin.Logger.LogError($"[StaminaController] {ex}"); }
        }

        private static StaminaScenario Resolve(Player p)
        {
            if (p == null) return StaminaScenario.Inactive;   // CR-01-01: defesa em camadas
            EFT.Animations.ProceduralWeaponAnimation pwa = p.ProceduralWeaponAnimation;
            bool ads = pwa != null && pwa.IsAiming;
            bool hb = p.Physical != null && p.Physical.HoldingBreath;   // ref: PlayerPhysicalClass.HoldingBreath

            if (pwa != null && pwa.IsMountedState)   // Active Mount
                return hb ? StaminaScenario.ActiveHoldBreath : ads ? StaminaScenario.ActiveAds : StaminaScenario.ActiveStance0;
            if (PassiveMountState.IsBracing && Plugin._EnablePassiveMount.Value && Plugin._PassiveStaminaSave.Value)   // Passive Mount
                return hb ? StaminaScenario.PassiveHoldBreath : ads ? StaminaScenario.PassiveAds : StaminaScenario.PassiveStance0;
            if (p.IsInPronePose)   // Prone (ignora a stance)
                return hb ? StaminaScenario.ProneHoldBreath : ads ? StaminaScenario.ProneAds : StaminaScenario.ProneHip;

            // Stand up sem mount
            if (hb) return StaminaScenario.StandHoldBreath;
            if (ads) return StaminaScenario.StandAds;
            switch (StanceManager.CurrentStance)
            {
                case Stance.Stance1: return StaminaScenario.StandStance1;
                case Stance.Stance2: return StaminaScenario.StandStance2;
                case Stance.Stance3: return StaminaScenario.StandStance3;
                default: return StaminaScenario.StandStance0;
            }
        }

        private static void SetScenario(StaminaScenario s)
        {
            Current = s;
            if (s == _prev) return;
            _prev = s;
            CurrentLabel = Label(s);
            if (Plugin._DebugStaminaState != null && Plugin._DebugStaminaState.Value)
                Plugin.Logger.LogInfo($"STAMINA STATE: {CurrentLabel}");
        }

        public static string Label(StaminaScenario s)
        {
            switch (s)
            {
                case StaminaScenario.StandStance0: return "Stand up sem mount - Stance 0";
                case StaminaScenario.StandStance1: return "Stand up sem mount - Stance 1";
                case StaminaScenario.StandStance2: return "Stand up sem mount - Stance 2";
                case StaminaScenario.StandStance3: return "Stand up sem mount - Stance 3";
                case StaminaScenario.StandAds: return "Stand up sem mount - ADS";
                case StaminaScenario.StandHoldBreath: return "Stand up sem mount - Hold Breath";
                case StaminaScenario.ProneHip: return "Prone sem mount - Hipfire";
                case StaminaScenario.ProneAds: return "Prone sem mount - ADS";
                case StaminaScenario.ProneHoldBreath: return "Prone sem mount - Hold Breath";
                case StaminaScenario.PassiveStance0: return "Passive Mount - Stance 0";
                case StaminaScenario.PassiveAds: return "Passive Mount - ADS";
                case StaminaScenario.PassiveHoldBreath: return "Passive Mount - Hold Breath";
                case StaminaScenario.ActiveStance0: return "Active Mount - Stance 0";
                case StaminaScenario.ActiveAds: return "Active Mount - ADS";
                case StaminaScenario.ActiveHoldBreath: return "Active Mount - Hold Breath";
                default: return "Inactive";
            }
        }

        public static void Reset()
        {
            Current = _prev = StaminaScenario.Inactive;
            CurrentLabel = "Inactive";
            ControllingHands = false;
        }
    }
}
