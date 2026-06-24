using UnderFire.Patch;
using System;
using System.Reflection;
using SPT;
using BepInEx;
using UnityEngine;
using EFT;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;
using EFT;
using EFT.UI;
using BepInEx.Configuration;
using Comfort.Common;
using EFT.HealthSystem;

namespace UnderFire
{
    [BepInPlugin("com.rpmwpm.UnderFire", "rpmwpm.UnderFire", "2.0.1")]
    public class Plugin : BaseUnityPlugin
    {
        internal static float interval = 0f;
        internal static float cooldownTime = 0f;
        internal static ManualLogSource logSource = null;
        internal static GameWorld gameWorld = null;
        internal static Player player = null;
        internal static ActiveHealthController activeHealthController;
        public static bool isAdrenalineActive = false;
        internal static bool changesReverted = false;
        internal static float originalStaminaRestoreRate;

        internal static ConfigEntry<bool> SuppressEnabled { get; set; }
        internal static ConfigEntry<bool> HitEnabled { get; set; }
        internal static ConfigEntry<int> SuppressDistance { get; set; }
        internal static ConfigEntry<int> SnipeDistance { get; set; }
        internal static ConfigEntry<float> DurationSecs { get; set; }
        internal static ConfigEntry<float> CooldownSecs { get; set; }
        internal static ConfigEntry<bool> TunnelVisionEnabled { get; set; }
        internal static ConfigEntry<bool> PainKillerEnabled { get; set; }
        internal static ConfigEntry<bool> TremorEnabled { get; set; }
        internal static ConfigEntry<bool> StaminaBurstEnabled { get; set; }
        internal static ConfigEntry<int> StaminaRestoreInc { get; set; }

        private void SetupConfigOptions()
        {
            string mainSetting = "1. Main Settings";
            ConfigDefinition enableOnSuppressDef = new ConfigDefinition(mainSetting, "Enable Adrenaline On Suppression");
            ConfigDescription enableOnSuppressDesc = new ConfigDescription("Enables Adrenaline when the player is being suppressed (shot at).",
                null, new ConfigurationManagerAttributes { IsAdvanced = false, Order = 10 });
            ConfigDefinition enableOnHitDef = new ConfigDefinition(mainSetting, "Enable Adrenaline On Hit");
            ConfigDescription enableOnHitDesc = new ConfigDescription("Enables Adrenaline when the player is shot."
                , null, new ConfigurationManagerAttributes { IsAdvanced = false, Order = 9 });

            string suppSetting = "2. Suprression Settings";
            ConfigDefinition suppressDistanceDef = new ConfigDefinition(suppSetting, "Max Suppression Distance for Bullets in Meters");
            ConfigDescription suppressDistanceDesc = new ConfigDescription("How close the bullets have to fly by in order for the Adrenaline to activate from the suppression effect (a value of 5 means the bullets have to fly within 5 meters for suppression to activate).",
                new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { IsAdvanced = false, ShowRangeAsPercent = false, Order = 8 });
            ConfigDefinition snipeDistanceDef = new ConfigDefinition(suppSetting, "Max Shooter Distance in Meters");
            ConfigDescription snipeDistanceDesc = new ConfigDescription("How close the shooter has to be in order for the Adrenaline to activate from the suppression effect (this is useful if you don't like being suppressed from snipers).",
                new AcceptableValueRange<int>(0, 300), new ConfigurationManagerAttributes { IsAdvanced = false, ShowRangeAsPercent = false, Order = 7 });

            string effectSetting = "3. Adrenaline Effects Settings";
            ConfigDefinition adrenalineDurationDef = new ConfigDefinition(effectSetting, "Duration of Adrenaline Effect in Secs");
            ConfigDescription adrenalineDurationDesc = new ConfigDescription("How long the effects of adrenaline lasts.",
                new AcceptableValueRange<float>(0f, 1200f), new ConfigurationManagerAttributes { IsAdvanced = false, ShowRangeAsPercent = false, Order = 6 });
            ConfigDefinition adrenalineCooldownDef = new ConfigDefinition(effectSetting, "Cooldown of Adrenaline Effect in Secs");
            ConfigDescription adrenalineCooldownDesc = new ConfigDescription("How long after the adrenaline effect is triggered before it can be triggered again.",
                new AcceptableValueRange<float>(0f, 1200f), new ConfigurationManagerAttributes { IsAdvanced = false, ShowRangeAsPercent = false, Order = 5 });
            ConfigDefinition tunnelVisionEnableDef = new ConfigDefinition(effectSetting, "Enable Tunnel Vision Effect");
            ConfigDescription tunnelVisionEnableDesc = new ConfigDescription("Enables the tunnel vision effect when Adrenaline is activated.",
                null, new ConfigurationManagerAttributes { IsAdvanced = false, Order = 4 });
            ConfigDefinition painKillerEnableDef = new ConfigDefinition(effectSetting, "Enable Pain Killer Effect");
            ConfigDescription painKillerEnableDesc = new ConfigDescription("Enables the pain killer effect when Adrenaline is activated (it lasts for Duration / 2).",
                null, new ConfigurationManagerAttributes { IsAdvanced = false, Order = 3 });
            ConfigDefinition tremorEnableDef = new ConfigDefinition(effectSetting, "Enable Tremor Effect");
            ConfigDescription tremorEnableDesc = new ConfigDescription("Enables the tremor effect when Adrenaline is activated.",
                null, new ConfigurationManagerAttributes { IsAdvanced = false, Order = 2 });
            ConfigDefinition staminaBurstEnableDef = new ConfigDefinition(effectSetting, "Enable Stamina Burst Effect");
            ConfigDescription staminaBurstEnableDesc = new ConfigDescription("Enables the stamina burst effect (this adds some stamina) when Adrenaline is activated.",
                null, new ConfigurationManagerAttributes { IsAdvanced = false, Order = 1 });
            ConfigDefinition staminaRestoreIncDef = new ConfigDefinition(effectSetting, "Stamina Restore Rate Increase");
            ConfigDescription staminaRestoreIncDesc = new ConfigDescription("The rate of stamina points to increase the restore rate by (set to 0 to disable).",
                new AcceptableValueRange<int>(0, 20), new ConfigurationManagerAttributes { IsAdvanced = false, Order = 0 });


            SuppressEnabled = Config.Bind(enableOnSuppressDef, true, enableOnSuppressDesc);
            HitEnabled = Config.Bind(enableOnHitDef, false, enableOnHitDesc);
            SuppressDistance = Config.Bind(suppressDistanceDef, 5, suppressDistanceDesc);
            SnipeDistance = Config.Bind(snipeDistanceDef, 80, snipeDistanceDesc);
            DurationSecs = Config.Bind(adrenalineDurationDef, 60f, adrenalineDurationDesc);
            CooldownSecs = Config.Bind(adrenalineCooldownDef, 180f, adrenalineCooldownDesc);
            TunnelVisionEnabled = Config.Bind(tunnelVisionEnableDef, true, tunnelVisionEnableDesc);
            PainKillerEnabled = Config.Bind(painKillerEnableDef, true, painKillerEnableDesc);
            TremorEnabled = Config.Bind(tremorEnableDef, true, tremorEnableDesc);
            StaminaBurstEnabled = Config.Bind(staminaBurstEnableDef, true, staminaBurstEnableDesc);
            StaminaRestoreInc = Config.Bind(staminaRestoreIncDef, 2, staminaRestoreIncDesc);

        }

        private void Awake()
        {
            logSource = Logger;
            SetupConfigOptions();
            GClass897.OnShoot += CheckFiredBullet;
            ConsoleScreen.Processor.RegisterCommandGroup<UnderFire.DebugCommands>();
            new UnderFirePatch().Enable();
        }

        private void Update()
        {
            float delta = Time.deltaTime;
            interval += delta;
            if (interval < 1f) return;
            if (isAdrenalineActive)
            {
                cooldownTime += interval;

                if (cooldownTime > DurationSecs.Value / 2 && !changesReverted)
                {
                    player.Physical.StaminaRestoreRate = originalStaminaRestoreRate;
                    //logSource.LogWarning($"Updated Restore Rate {player.Physical.StaminaRestoreRate}");
                    changesReverted = true;
                }

                if (cooldownTime > CooldownSecs.Value)
                {
                    cooldownTime = 0f;
                    isAdrenalineActive = false;
                    //logSource.LogWarning($"Cooldown complete, reverting isAdrenalineActive to {isAdrenalineActive}");
                }
            }
            interval = 0f;
        }

        internal void CheckFiredBullet(SonicBulletSoundPlayer.GClass898 sonicInfo)
        {
            if (!SuppressEnabled.Value) return;
            if (isAdrenalineActive) return;
            if (sonicInfo == null) return;
            if (sonicInfo.ShotPosition == null) return;
            if (sonicInfo.IsOccluded) return;
            if (sonicInfo.Ammo.InitialSpeed < 0f) return;
            Vector3 shotPosition = sonicInfo.ShotPosition;
            float dist = CameraClass.Instance.Distance(shotPosition);
            if (dist > SnipeDistance.Value) return; // Dont do Adrenaline for Sniping
            Vector3 shotDirection = sonicInfo.ShotDirection;
            Vector3 playerPos = sonicInfo.Camera.transform.position;
            float t = -Vector3.Dot(shotPosition - playerPos, shotDirection) / Vector3.Dot(shotDirection, shotDirection);
            Vector3 closestPoint = shotPosition + t * shotDirection;
            float closestDistance = Vector3.Distance(closestPoint, playerPos);
            //logSource.LogWarning($"Closest Bullet Distance is: {closestDistance}");
            if (closestDistance < SuppressDistance.Value)
            {
                //logSource.LogWarning($"Starting Adrenaline from Suppression");
                StartAdrenaline();
            }
        }


        internal static void StartAdrenaline()
        {
            if (!Singleton<GameWorld>.Instantiated) return;
            if (gameWorld == null) gameWorld = Singleton<GameWorld>.Instance;
            if (player == null) player = gameWorld.MainPlayer;
            activeHealthController = player.ActiveHealthController;
            ApplyAdrenalineEffects(activeHealthController);
            isAdrenalineActive = true;
        }

        protected class TunnelVision : ActiveHealthController.GClass3008, GInterface363, IEffect { }
        internal class Tremor : ActiveHealthController.GClass3008, GInterface361, IEffect, GInterface331
        {
            public override float DefaultDelayTime
            {
                //get { return ActiveHealthController.GClass2813.GClass2823_0.Tremor.DefaultDelay; }
                get { return 3f; }
            }
            public override float DefaultResidueTime
            {
                get { return ActiveHealthController.GClass3008.GClass3019_0.Tremor.DefaultResidueTime; }
            }
        }
        protected class PainKiller : ActiveHealthController.GClass3008, GInterface358, IEffect, GInterface332, GInterface334, GInterface330
        {
            public string ItemTemplateId { get; set; }
            public float MaxDuration { get; set; }
            public void UpdateWithSameOne(float strength)
            {
                float num = this.MaxDuration * strength;
                base.AddWorkTime(new float?(Mathf.Clamp(base.TimeLeft + num, 0f, this.MaxDuration)), true);
            }
        }

        internal static void ApplyAdrenalineEffects(ActiveHealthController ach)
        {
            if (TunnelVisionEnabled.Value) ach.AddEffect<TunnelVision>(EBodyPart.Head, 0.5f, DurationSecs.Value, 10f);
            if (PainKillerEnabled.Value) ach.AddEffect<PainKiller>(EBodyPart.Head, 0.5f, DurationSecs.Value / 2, 10f);
            if (TremorEnabled.Value) ach.AddEffect<Tremor>(EBodyPart.Head, 0.5f, DurationSecs.Value, 10f);
            originalStaminaRestoreRate = player.Physical.StaminaRestoreRate;
            player.Physical.StaminaRestoreRate += StaminaRestoreInc.Value;
            //logSource.LogWarning($"Original Stamina Rate: {originalStaminaRestoreRate}");
            //logSource.LogWarning($"Updated Stamina Rate: {player.Physical.StaminaRestoreRate}");
            if (!StaminaBurstEnabled.Value) return;
            float curStamina = player.Physical.Stamina.Current;
            float totalStamina = player.Physical.Stamina.TotalCapacity.Value;
            if (curStamina + 50 < totalStamina)
            {
                player.Physical.Stamina.Current += 50;
            }
            else player.Physical.Stamina.Current = totalStamina;
            changesReverted = false;
        }
    }
}
