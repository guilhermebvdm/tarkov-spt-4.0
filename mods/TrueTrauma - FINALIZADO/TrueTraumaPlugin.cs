using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Reflection;
using EFT;
using UnityEngine;

namespace TrueTrauma
{
    [BepInPlugin("com.umbigopreto.truetrauma", "TrueTrauma", "1.0.0")]
    public class TrueTraumaPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> ConfigMasterEnabled;
        public static ConfigEntry<bool> ConfigLegsEnabled;
        public static ConfigEntry<bool> ConfigArmsEnabled;
        public static ConfigEntry<bool> ConfigStomachEnabled;
        public static ConfigEntry<bool> ConfigBlackoutEnabled;
        public static ConfigEntry<float> ConfigBlackoutDuration;

        private void Awake()
        {
            TraumaState.Logger = Logger;
            TraumaState.Logger.LogInfo("TrueTrauma: v1.0.0 - Host Authority Edition");

            ConfigMasterEnabled = Config.Bind("1. Geral", "Ativar Mod", true, "Liga ou desliga todo o funcionamento do mod.");
            ConfigBlackoutEnabled = Config.Bind("2. Mecanicas", "Sistema de Desmaio", true, "Ativa o desmaio ao receber muito dano massivo.");
            ConfigLegsEnabled = Config.Bind("2. Mecanicas", "Sistema de Pernas", true, "Cair no chão ao perder as pernas.");
            ConfigArmsEnabled = Config.Bind("2. Mecanicas", "Sistema de Braços", true, "Perder a mira ao perder os braços.");
            ConfigStomachEnabled = Config.Bind("2. Mecanicas", "Sistema de Estomago", true, "Ficar sem ar ao tomar tiro no estômago.");
            ConfigBlackoutDuration = Config.Bind("3. Balanceamento", "Duracao do Desmaio", 20f, "Quanto tempo (segundos) o jogador fica desmaiado.");

            TraumaState.PlayerField = typeof(MovementContext).GetField("_player", BindingFlags.NonPublic | BindingFlags.Instance);

            try
            {
                new Harmony("com.umbigopreto.truetrauma").PatchAll();
            }
            catch (Exception ex)
            {
                TraumaState.Logger.LogError(string.Format("TrueTrauma Patch Error: {0}", ex.Message));
            }

            try
            {
                // Registra o patch de limpeza de raid
                var harmony = new Harmony("com.umbigopreto.truetrauma.cleanup");
                var onGameStarted = typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
                if (onGameStarted != null)
                {
                    harmony.Patch(onGameStarted, prefix: new HarmonyMethod(typeof(TrueTraumaPlugin), nameof(OnRaidStartCleanup)));
                    TraumaState.Logger.LogInfo("TrueTrauma: Patch de limpeza de raid registrado.");
                }
            }
            catch (Exception ex)
            {
                TraumaState.Logger.LogError(string.Format("TrueTrauma Cleanup Patch Error: {0}", ex.Message));
            }

            try
            {
                if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.fika.core"))
                {
                    FikaPacketManager.Initialize();
                }
            }
            catch (Exception ex)
            {
                TraumaState.Logger.LogError(string.Format("TrueTrauma Fika Integration Error: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Limpa todos os dicionários estáticos ao iniciar nova raid.
        /// Previne memory leak e falsos positivos entre raids consecutivas.
        /// </summary>
        public static void OnRaidStartCleanup()
        {
            TraumaState.BlackoutTimers.Clear();
            TraumaState.BlackoutStartTimes.Clear();
            TraumaState.GraceTimers.Clear();
            TraumaState.FaintedPlayerIds.Clear();
            TraumaState.ImpactTimers.Clear();
            TraumaState.AimingFatigueTimers.Clear();
            TraumaState.VoiceCooldowns.Clear();
            TraumaState.BotLegsBrokenStartTimes.Clear();
            TraumaState.EffectIntensity = 0f;
            TraumaState.Logger.LogInfo("TrueTrauma: Estado limpo para nova raid.");
        }

        private void Update()
        {
            if (!ConfigMasterEnabled.Value || !ConfigBlackoutEnabled.Value)
            {
                TraumaState.EffectIntensity = 0f;
                if (AudioListener.volume != 1f) AudioListener.volume = 1f;
                return;
            }

            var gameWorld = Comfort.Common.Singleton<GameWorld>.Instance;
            if (gameWorld == null || gameWorld.MainPlayer == null) return;

            // Efeito visual apenas para o jogador local
            string localId = gameWorld.MainPlayer.ProfileId;
            float targetIntensity = 0f;

            if (TraumaState.BlackoutStartTimes.TryGetValue(localId, out float startTime))
            {
                float duration = ConfigBlackoutDuration.Value;
                float timeElapsed = Time.time - startTime;

                if (timeElapsed <= duration)
                {
                    if (timeElapsed <= 1f) targetIntensity = Mathf.Lerp(0f, 1f, timeElapsed / 1f);
                    else if (timeElapsed <= (duration - 2f)) targetIntensity = 1f;
                    else targetIntensity = Mathf.Lerp(1f, 0f, (timeElapsed - (duration - 2f)) / 2f);
                }
            }

            TraumaState.EffectIntensity = Mathf.Lerp(TraumaState.EffectIntensity, targetIntensity, Time.deltaTime * 5f);
            AudioListener.volume = Mathf.Lerp(1f, 0.05f, TraumaState.EffectIntensity);
        }
    }
}