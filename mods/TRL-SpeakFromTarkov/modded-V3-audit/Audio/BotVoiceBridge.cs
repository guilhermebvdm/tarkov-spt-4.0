using System;
using System.Collections.Generic;
using EFT;
using Comfort.Common;
using UnityEngine;

namespace TRL_SpeakFromTarkov.Audio
{
    public class BotVoiceBridge : MonoBehaviour
    {
        private float lastTriggerTime = 0f;
        private const float COOLDOWN_INTERVAL = 1.5f; // Debounce de 1.5s entre emissões

        private bool isSampling = false;
        private float samplingTimer = 0f;
        private const float SAMPLING_WINDOW = 0.50f; // Janela de 500ms (0.50s) para capturar o pico real da fala
        private float accumulatedPeakLevel = 0f;

        public void ProcessVoiceFrame(Player player, float displayLevel, bool isTransmitting)
        {
            if (player == null || player.HealthController == null || !player.HealthController.IsAlive) return;
            if (VoIPPlugin.EnableBotInteraction != null && !VoIPPlugin.EnableBotInteraction.Value) return;

            if (!isTransmitting)
            {
                isSampling = false;
                samplingTimer = 0f;
                accumulatedPeakLevel = 0f;
                return;
            }

            // Se o cooldown de 1.5s entre emissões estiver ativo, ignoramos novos disparos
            if (Time.time - lastTriggerTime < COOLDOWN_INTERVAL) return;

            // Início da janela de amostragem (250ms) ao começar a falar
            if (!isSampling)
            {
                isSampling = true;
                samplingTimer = 0f;
                accumulatedPeakLevel = displayLevel;
                return;
            }

            // Acumula o maior pico de energia registrado durante a janela de 250ms
            if (displayLevel > accumulatedPeakLevel)
            {
                accumulatedPeakLevel = displayLevel;
            }

            samplingTimer += Time.deltaTime;

            // Quando a janela de 250ms completa, avaliamos o maior pico atingido na frase!
            if (samplingTimer >= SAMPLING_WINDOW)
            {
                TriggerBotVoiceEvent(player, accumulatedPeakLevel);
                isSampling = false;
                samplingTimer = 0f;
                accumulatedPeakLevel = 0f;
            }
        }

        private void TriggerBotVoiceEvent(Player player, float peakLevel)
        {
            float power;
            EPhraseTrigger trigger;
            bool isAggressive = false;

            // Interpolação Matemática Gradual e Contínua do Raio em Metros
            if (peakLevel < 0.025f)
            {
                float t = Mathf.Clamp01(peakLevel / 0.025f);
                power = Mathf.Lerp(3.0f, 10.0f, t); // Sussurro: 3.0m a 10.0m
                trigger = EPhraseTrigger.OnMutter;
            }
            else if (peakLevel < 0.150f)
            {
                float t = Mathf.Clamp01((peakLevel - 0.025f) / (0.150f - 0.025f));
                power = Mathf.Lerp(10.0f, 30.0f, t); // Voz Normal: 10.0m a 30.0m
                trigger = EPhraseTrigger.NoisePhrase;
            }
            else
            {
                float t = Mathf.Clamp01((peakLevel - 0.150f) / (0.400f - 0.150f));
                power = Mathf.Lerp(30.0f, 60.0f, t); // Grito: 30.0m a 60.0m (Teto máx)
                trigger = EPhraseTrigger.OnFight;
                isAggressive = true; // Ativa a tag Combat para xingamentos do Duplo F1
            }

            lastTriggerTime = Time.time;

            Vector3 soundPos = player.PlayerBones != null && player.PlayerBones.Head != null 
                ? player.PlayerBones.Head.Original.position 
                : player.Transform.position;

            // 1. Sinaliza o sensor de audição da IA em 3D (distância exata e contínua em metros)
            if (Singleton<BotEventHandler>.Instantiated)
            {
                try
                {
                    Singleton<BotEventHandler>.Instance.PlaySound(
                        player, 
                        soundPos, 
                        power, 
                        AISoundType.step
                    );
                }
                catch (Exception ex)
                {
                    VoIPPlugin.Log.LogWarning($"[SFT] Erro ao notificar BotEventHandler: {ex.Message}");
                }
            }

            // 2. Dispara a fala nativa no próprio personagem
            float debugVolume = VoIPPlugin.BotVoiceDebugVolume != null ? VoIPPlugin.BotVoiceDebugVolume.Value : 0.5f;
            
            if (debugVolume > 0.001f)
            {
                try
                {
                    player.Say(trigger, demand: true, 0f, (ETagStatus)0, 100, aggressive: isAggressive);
                    if (VoIPPlugin.EnableDebugLogs != null && VoIPPlugin.EnableDebugLogs.Value)
                        VoIPPlugin.Log.LogInfo($"[SFT-BOT] Janela 250ms -> Pico RMS={peakLevel:F3} | Phrase={trigger} (Aggressive={isAggressive}) | Raio Gradual={power:F1}m | DebugVol={debugVolume * 100:F0}%");
                }
                catch (Exception ex)
                {
                    VoIPPlugin.Log.LogWarning($"[SFT] Aviso ao reproduzir fala do personagem: {ex.Message}");
                }
            }
            else
            {
                // Sinal de alcance sonoro enviado à IA sem voz audível (BotVoiceDebugVolume = 0%)
                if (VoIPPlugin.EnableDebugLogs != null && VoIPPlugin.EnableDebugLogs.Value)
                    VoIPPlugin.Log.LogInfo($"[SFT-BOT] Sinal emitido silenciosamente (0%) para IA: Pico RMS={peakLevel:F3} | Phrase={trigger} | Raio Gradual={power:F1}m");
            }

            // 3. Resposta Forçada 100% Instantânea dos Bots dentro do raio 'power'
            ForceBotResponsesInRadius(player, soundPos, power, trigger);
        }

        private static int _botOcclusionMask = -1;

        private void ForceBotResponsesInRadius(Player player, Vector3 soundPos, float power, EPhraseTrigger trigger)
        {
            try
            {
                if (!Singleton<IBotGame>.Instantiated || Singleton<IBotGame>.Instance.BotsController == null) return;

                var botOwners = Singleton<IBotGame>.Instance.BotsController.Bots.BotOwners;
                if (botOwners == null) return;

                if (_botOcclusionMask == -1)
                {
                    try
                    {
                        _botOcclusionMask = LayerMaskClass.HighPolyWithTerrainMask | LayerMaskClass.DoorLayer | LayerMaskClass.InteractiveLayer;
                    }
                    catch
                    {
                        _botOcclusionMask = LayerMask.GetMask("HighPolyWithRaycast", "Terrain", "LowPoly", "Interactive", "Doors", "Default");
                    }
                }

                EPhraseTrigger responsePhrase = trigger == EPhraseTrigger.OnMutter 
                    ? EPhraseTrigger.OnMutter 
                    : (trigger == EPhraseTrigger.NoisePhrase ? EPhraseTrigger.Greetings : EPhraseTrigger.OnFight);

                float sqrPower = power * power;
                float occludedPowerMult = trigger == EPhraseTrigger.OnMutter ? 0f : (trigger == EPhraseTrigger.NoisePhrase ? 0.50f : 0.65f);
                float sqrOccludedPower = (power * occludedPowerMult) * (power * occludedPowerMult);

                int countResponding = 0;
                foreach (BotOwner bot in botOwners)
                {
                    if (bot == null || bot.IsDead || bot.BotTalk == null) continue;

                    // Ignora se o perfil do bot for o próprio jogador
                    if (bot.ProfileId == player.ProfileId) continue;

                    Vector3 botHeadPos = bot.Position + Vector3.up * 1.5f;
                    float sqrDist = (soundPos - bot.Position).sqrMagnitude;

                    if (sqrDist <= sqrPower)
                    {
                        // Checagem de oclusão por paredes/portas (Zero-Alloc):
                        bool isOccluded = Physics.Linecast(soundPos, botHeadPos, out RaycastHit hit, _botOcclusionMask, QueryTriggerInteraction.Ignore);
                        if (isOccluded)
                        {
                            if (trigger == EPhraseTrigger.OnMutter) continue; // Sussurro não atravessa paredes de concreto
                            if (sqrDist > sqrOccludedPower) continue; // Voz abafada tem alcance reduzido através de paredes
                        }

                        try
                        {
                            bot.BotTalk.Say(responsePhrase, sayImmediately: true);
                            countResponding++;
                        }
                        catch (Exception botEx)
                        {
                            VoIPPlugin.Log.LogWarning($"[SFT] Erro ao forçar fala no bot {bot.Id}: {botEx.Message}");
                        }
                    }
                }

                if (countResponding > 0)
                {
                    VoIPPlugin.Log.LogInfo($"[SFT-BOT] 100% GARANTIDO: {countResponding} bot(s) a menos de {power:F1}m responderam com {responsePhrase}!");
                }
            }
            catch (Exception ex)
            {
                VoIPPlugin.Log.LogWarning($"[SFT] Erro na varredura de resposta 100% dos bots: {ex.Message}");
            }
        }
    }
}
