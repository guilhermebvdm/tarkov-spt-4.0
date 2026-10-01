using EFT;
using Comfort.Common;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using System;
using System.Collections;

namespace TRLImmersiveCombatMedicine.Helpers
{
    public static class AggroHelper
    {
        private static MethodInfo _comeToPatrolMethod;
        private static PropertyInfo _groupEnemiesProperty;
        private static FieldInfo _lastEnemyField;

        // Caches SAIN
        private static Type _sainComponentType;
        private static PropertyInfo _sainEnemyControllerProp;
        private static MethodInfo _sainRemoveEnemyMethod;
        private static bool _sainReflectionAttempted = false;

        public static void NeutralizeAggro(Player victim)
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null) return;

            string victimId = victim.ProfileId;

            foreach (var entity in gameWorld.RegisteredPlayers)
            {
                if (entity.IsAI && entity.HealthController.IsAlive && entity.AIData?.BotOwner != null)
                {
                    var bot = entity.AIData.BotOwner;
                    if (bot.ProfileId == victimId) continue;
                    // --- OTIMIZAÇÃO NOVA ---
                    // Se o bot já não tem inimigo, ou o inimigo não é a vítima, não precisamos rodar o código pesado do SAIN/Reflection.
                    bool isAngryAtVictim = false;
                    if (bot.Memory.GoalEnemy != null && bot.Memory.GoalEnemy.Person.ProfileId == victimId) isAngryAtVictim = true;
                    if (bot.EnemiesController.EnemyInfos.ContainsKey(victim)) isAngryAtVictim = true;

                    // Só roda a lógica pesada se o bot estiver, de fato, focado na vítima
                    if (!isAngryAtVictim) continue;
                    // -----------------------
                    try
                    {
                        RemoveFromGroupEnemies(bot, victim);
                        TryRemoveFromSAIN(bot, victimId);

                        bot.ShootData?.EndShoot();
                        if (bot.AimingManager?.CurrentAiming != null) bot.AimingManager.CurrentAiming.LoseTarget();

                        if (bot.Memory.GoalEnemy != null && bot.Memory.GoalEnemy.Person.ProfileId == victimId)
                            bot.Memory.GoalEnemy = null;

                        WipeMemoryResidue(bot.Memory);

                        if (bot.EnemiesController != null)
                        {
                            bool isTracked = false;
                            if (bot.EnemiesController.EnemyInfos != null)
                            {
                                foreach (var info in bot.EnemiesController.EnemyInfos.Values)
                                {
                                    if (info.Person.ProfileId == victimId) { isTracked = true; break; }
                                }
                            }
                            if (isTracked) bot.EnemiesController.Remove(victim);
                        }

                        ForceBackToPatrol(bot);
                    }
                    catch { }
                }
            }
        }

        public static void RestoreAggro(Player victim)
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null) return;

            Vector3 victimPos = victim.Transform.position;

            foreach (var entity in gameWorld.RegisteredPlayers)
            {
                if (entity.IsAI && entity.HealthController.IsAlive && entity.AIData?.BotOwner != null)
                {
                    var bot = entity.AIData.BotOwner;
                    if (bot.ProfileId == victim.ProfileId) continue;

                    try
                    {
                        // Força o bot a re-adicionar o jogador como inimigo
                        bot.BotsGroup?.CheckAndAddEnemy(victim, true);

                        // Adiciona ponto de busca na posição do jogador
                        bot.BotsGroup?.AddPointToSearch(victimPos, 50f, bot, true);

                        // Faz o bot olhar na direção do jogador
                        bot.Steering?.LookToPoint(victimPos);
                    }
                    catch { }
                }
            }
        }

        private static void TryRemoveFromSAIN(BotOwner bot, string victimId)
        {
            try
            {
                if (!_sainReflectionAttempted)
                {
                    _sainReflectionAttempted = true;
                    _sainComponentType = AccessTools.TypeByName("SAIN.Components.BotComponent");

                    if (_sainComponentType != null)
                    {
                        _sainEnemyControllerProp = AccessTools.Property(_sainComponentType, "EnemyController");
                        if (_sainEnemyControllerProp != null)
                        {
                            var controllerType = _sainEnemyControllerProp.PropertyType;
                            _sainRemoveEnemyMethod = AccessTools.Method(controllerType, "RemoveEnemy", new Type[] { typeof(string) });
                        }
                    }
                }

                if (_sainComponentType == null) return;

                var components = bot.GetPlayer.gameObject.GetComponents(typeof(MonoBehaviour));
                object sainBotInstance = null;

                foreach (var comp in components)
                {
                    if (comp.GetType().FullName == "SAIN.Components.BotComponent")
                    {
                        sainBotInstance = comp;
                        break;
                    }
                }

                if (sainBotInstance == null) return;

                if (_sainEnemyControllerProp != null && _sainRemoveEnemyMethod != null)
                {
                    object enemyController = _sainEnemyControllerProp.GetValue(sainBotInstance);
                    if (enemyController != null)
                    {
                        _sainRemoveEnemyMethod.Invoke(enemyController, new object[] { victimId });
                    }
                }
            }
            catch { }
        }

        private static void RemoveFromGroupEnemies(BotOwner bot, Player victim)
        {
            if (bot.BotsGroup == null) return;
            try
            {
                if (_groupEnemiesProperty == null)
                    _groupEnemiesProperty = typeof(BotsGroup).GetProperty("Enemies", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (_groupEnemiesProperty != null)
                {
                    if (_groupEnemiesProperty.GetValue(bot.BotsGroup) is IDictionary enemiesDict && enemiesDict.Contains(victim))
                    {
                        enemiesDict.Remove(victim);
                    }
                }
            }
            catch { }
        }

        private static void WipeMemoryResidue(BotMemoryClass memory)
        {
            if (memory == null) return;
            try
            {
                if (_lastEnemyField == null)
                {
                    _lastEnemyField = typeof(BotMemoryClass).GetField("LastEnemy", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                                   ?? typeof(BotMemoryClass).GetField("_lastEnemy", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                }
                _lastEnemyField?.SetValue(memory, null);
            }
            catch { }
        }

        private static void ForceBackToPatrol(BotOwner bot)
        {
            if (bot.PatrollingData == null) return;
            try
            {
                bot.PatrollingData.Unpause();
                if (_comeToPatrolMethod == null)
                    _comeToPatrolMethod = bot.PatrollingData.GetType().GetMethod("ComeToPatrol", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (_comeToPatrolMethod != null)
                {
                    var parameters = _comeToPatrolMethod.GetParameters();
                    if (parameters.Length == 2) _comeToPatrolMethod.Invoke(bot.PatrollingData, new object[] { true, true });
                    else if (parameters.Length == 1) _comeToPatrolMethod.Invoke(bot.PatrollingData, new object[] { true });
                    else _comeToPatrolMethod.Invoke(bot.PatrollingData, null);
                }
            }
            catch { }
        }

        public static void PauseBot(Player botPlayer)
        {
            if (botPlayer?.AIData?.BotOwner?.StandBy == null) return;
            var bot = botPlayer.AIData.BotOwner;
            try
            {
                bot.ShootData.EndShoot();
                if (bot.AimingManager?.CurrentAiming != null) bot.AimingManager.CurrentAiming.LoseTarget();
                bot.Steering.LookToMovingDirection();
                bot.StandBy.StandByType = BotStandByType.paused;
            }
            catch { }
        }

        public static void UnpauseBot(Player botPlayer)
        {
            if (botPlayer?.AIData?.BotOwner?.StandBy == null) return;
            var bot = botPlayer.AIData.BotOwner;
            try
            {
                bot.StandBy.StandByType = BotStandByType.active;
                bot.Memory.LastTimeHit = -1000f;
                bot.BotsGroup?.AddPointToSearch(botPlayer.Transform.position, 10f, bot, true);
            }
            catch { }
        }
    }
}
