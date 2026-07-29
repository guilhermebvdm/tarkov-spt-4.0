using EFT;
using System;
using System.Reflection;
using SPT.Reflection.Patching;
using HarmonyLib;
using UnityEngine;

namespace AgonySFX.Patches
{
    public class DeathSoundPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player).GetMethod("OnDead", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        [PatchPrefix]
        public static bool Prefix(ref EFT.Player __instance, EDamageType damageType)
        {
            // Load AgonySoundChance from plugin
            float agonyChance = AgonySFX.Plugin.AgonySoundChance.Value;
            float randomRoll = UnityEngine.Random.value;
            bool debugLogs = AgonySFX.Plugin.DebugLogs.Value;

            EPhraseTrigger trigger;

            __instance.LastDamageType = damageType;
            //Non-weapon induced damage (e.g., bleedout, grenade, etc.) always triggers OnAgony
            if (!__instance.LastDamageType.IsWeaponInduced())
            {
                trigger = EPhraseTrigger.OnAgony;
            }
            else
            {
                // If the damage is weapon-induced, decide whether
                // to trigger OnAgony or OnDeath based on the agonyChance probability.
                trigger = randomRoll < agonyChance ? EPhraseTrigger.OnAgony : EPhraseTrigger.OnDeath;
                
                if(debugLogs)
                {
                    Plugin.agonySFXLogger.LogInfo($"Rolled: {randomRoll}, Agony Chance: {agonyChance}, Triggered: {trigger}, Damage Type: {__instance.LastDamageType}");
                }
            }

            //Check if bot should vocalize depending on damage location.
            //Prevents headshot damage from playing death sounds.
            if (__instance.ShouldVocalizeDeath(__instance.LastDamagedBodyPart))
            {
                TagBank tagBank = __instance.Speaker.Play(trigger, __instance.HealthStatus, true, default(int?));
                if (tagBank != null)
                {
                    TaggedClip taggedClip = tagBank.Match((int)__instance.HealthStatus);
                    float time = taggedClip?.Length ?? 0f;
                }
            }
            else
            {
                __instance.Speaker.Shut();
            }

            return true;
        }
    }
}