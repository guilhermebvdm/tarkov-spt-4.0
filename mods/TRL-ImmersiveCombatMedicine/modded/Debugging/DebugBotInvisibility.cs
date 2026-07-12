using System;
using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using EFT.Communications;
using UnityEngine;

namespace TRLImmersiveCombatMedicine
{
    /// <summary>
    /// Feature de DEBUG (host-only): torna o MainPlayer invisível para a IA.
    /// Mecanismo (validado no decompilado EFT 0.16.x):
    ///  - BotSpawner.DeletePlayer(player): remove de AllPlayers (bots futuros não o
    ///    conhecem — ctor de BotsGroup lê AllPlayers) e chama DeletePlayerCauseDead em
    ///    todos os grupos → RemoveEnemy → Memory.DeleteInfoAboutEnemy em cada bot.
    ///    O loop de visão (LookSensor.CheckAllEnemies) só itera EnemyInfos → sem
    ///    EnemyInfo, bots não miram nem atiram.
    ///  - Reversão: BotsController.AddActivePLayer(player) (sic) — mesmo caminho que o
    ///    Fika host usa para registrar o LocalPlayer no início do raid.
    ///  - NUNCA usar BotsController.DestroyInfo: além de DeletePlayer ele executa
    ///    player.AIData.Dispose() — irreversível na mesma sessão.
    /// Limitações (aceitas para debug): atirar em/matar bot re-agroa (handlers de
    /// hit/kill adicionam enemy sem consultar AllPlayers); sons ainda geram pontos de
    /// investigação (bots vêm olhar, mas não atacam); host-only — peers Fika continuam
    /// visíveis para os bots.
    /// </summary>
    public static class DebugBotInvisibility
    {
        private static ConfigEntry<bool> _botsIgnoreMe;
        private static bool _applied;          // estado por-raid (BotsController morre com o raid)
        private static float _nextTick;

        /// <summary>Chamar no Awake do plugin.</summary>
        public static void Init(ConfigFile config)
        {
            _botsIgnoreMe = config.Bind(
                "5. Debug", "Invisivel para Bots", false,
                "DEBUG (host-only): bots deixam de mirar/atirar no jogador (remove do registro de players da IA + apaga memoria de inimigo). Atirar num bot re-agroa. Peers Fika continuam visiveis.");
            _botsIgnoreMe.SettingChanged += (_, __) => Apply(_botsIgnoreMe.Value);
        }

        /// <summary>Reconciliação periódica (chamada do Update em raid): reaplica o
        /// estado desejado quando o toggle mudou fora de raid ou o BotsController
        /// ainda não existia no momento do toggle/início de raid.</summary>
        public static void Tick()
        {
            if (_botsIgnoreMe == null || Time.time < _nextTick) return;
            _nextTick = Time.time + 2f;
            if (_botsIgnoreMe.Value != _applied) Apply(_botsIgnoreMe.Value);
        }

        /// <summary>Chamar quando o GameWorld do raid morre (ResetAllState).</summary>
        public static void OnRaidEnded()
        {
            _applied = false;
        }

        private static void Apply(bool invisible)
        {
            try
            {
                // Gate host/in-raid: em cliente Fika (não-host) IBotGame nunca é instanciado.
                if (!Singleton<GameWorld>.Instantiated || !Singleton<IBotGame>.Instantiated) return;

                Player player = Singleton<GameWorld>.Instance.MainPlayer;
                var botsController = Singleton<IBotGame>.Instance.BotsController;
                if (player == null || botsController == null || botsController.BotSpawner == null) return;

                if (invisible && !_applied)
                {
                    botsController.BotSpawner.DeletePlayer(player);

                    // Cinto-e-suspensório: EnemyInfo órfão criado por hit entre o toggle e a aplicação.
                    try
                    {
                        foreach (var bot in botsController.Bots.BotOwners)
                            bot?.Memory?.DeleteInfoAboutEnemy(player);
                    }
                    catch { }

                    _applied = true;
                    TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning("[Debug] Invisível para bots: ATIVADO");
                    NotificationManagerClass.DisplayMessageNotification(
                        "DEBUG: invisível para bots", ENotificationDurationType.Default, ENotificationIconType.Quest);
                }
                else if (!invisible && _applied)
                {
                    botsController.AddActivePLayer(player);
                    _applied = false;
                    TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning("[Debug] Invisível para bots: DESATIVADO");
                    NotificationManagerClass.DisplayMessageNotification(
                        "DEBUG: visível para bots novamente", ENotificationDurationType.Default, ENotificationIconType.Alert);
                }
            }
            catch (Exception ex)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning($"DebugBotInvisibility.Apply: {ex.Message}");
            }
        }
    }
}
