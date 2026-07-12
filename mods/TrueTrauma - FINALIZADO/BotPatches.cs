using HarmonyLib;
using EFT;
using UnityEngine;
using Comfort.Common;
using System.Collections.Generic;

namespace TrueTrauma
{
    // Patch 1: BotsGroup.IsEnemy — "esse jogador JÁ É meu inimigo?"
    [HarmonyPatch(typeof(BotsGroup), "IsEnemy")]
    public static class BotIsEnemyPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(IPlayer player, ref bool __result)
        {
            if (player == null) return true;

            if (TraumaState.FaintedPlayerIds.Contains(player.ProfileId))
            {
                __result = false;
                return false;
            }

            return true;
        }
    }

    // Patch 2: BotsGroup.IsPlayerEnemy — "esse jogador DEVERIA ser meu inimigo?"
    [HarmonyPatch(typeof(BotsGroup), "IsPlayerEnemy")]
    public static class BotIsPlayerEnemyPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(IPlayer player, ref bool __result)
        {
            if (player == null) return true;

            if (TraumaState.FaintedPlayerIds.Contains(player.ProfileId))
            {
                __result = false;
                return false;
            }

            return true;
        }
    }

    // Patch 3: BotsGroup.CheckAndAddEnemy — "devo checar e adicionar esse jogador como inimigo?"
    [HarmonyPatch(typeof(BotsGroup), "CheckAndAddEnemy")]
    public static class BotCheckAndAddEnemyPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(IPlayer player, ref bool __result)
        {
            if (player == null) return true;

            if (TraumaState.FaintedPlayerIds.Contains(player.ProfileId))
            {
                __result = false;
                return false;
            }

            return true;
        }
    }

    // Patch 4 (CATCH-ALL): BotsGroup.AddEnemy — impede QUALQUER adição de jogador desmaiado
    // Parâmetro no Assembly-CSharp: AddEnemy(IPlayer person, EBotEnemyCause cause)
    [HarmonyPatch(typeof(BotsGroup), "AddEnemy")]
    public static class BotAddEnemyPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(IPlayer person, ref bool __result)
        {
            if (person == null) return true;

            if (TraumaState.FaintedPlayerIds.Contains(person.ProfileId))
            {
                __result = false;
                return false;
            }

            return true;
        }
    }

    // Patch 5: BotMemoryClass.AddEnemy — impede que a memória do bot registre jogador desmaiado
    // Parâmetro: AddEnemy(IPlayer enemy, BotSettingsClass groupInfo, bool onActivation)
    [HarmonyPatch(typeof(BotMemoryClass), "AddEnemy")]
    public static class BotMemoryAddEnemyPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(IPlayer enemy)
        {
            if (enemy == null) return true;

            if (TraumaState.FaintedPlayerIds.Contains(enemy.ProfileId))
            {
                return false;
            }

            return true;
        }
    }

    // Patch 6: BotOwner.CalcGoal — A cada "tick" de recálculo de objetivo do bot,
    // se o GoalEnemy for um jogador desmaiado, limpa o alvo e para de atirar.
    [HarmonyPatch(typeof(BotOwner), "CalcGoal")]
    public static class BotCalcGoalPatch
    {
        [HarmonyPostfix]
        public static void Postfix(BotOwner __instance)
        {
            if (__instance == null || !__instance.HealthController.IsAlive) return;
            if (__instance.Memory == null) return;

            var goalEnemy = __instance.Memory.GoalEnemy;
            if (goalEnemy != null && TraumaState.FaintedPlayerIds.Contains(goalEnemy.Person.ProfileId))
            {
                __instance.Memory.GoalEnemy = null;
                __instance.ShootData?.EndShoot();
            }
        }
    }

    // Patch 7: Remove jogadores desmaiados da lista de inimigos existente a cada tick
    // Isso garante que mesmo inimigos já registrados sejam removidos ao desmaiar
    [HarmonyPatch(typeof(BotOwner), "UpdateManual")]
    public static class BotUpdateManualPatch
    {
        [HarmonyPostfix]
        public static void Postfix(BotOwner __instance)
        {
            if (__instance == null || !__instance.HealthController.IsAlive) return;
            if (__instance.Memory == null || __instance.BotsGroup == null) return;
            if (TraumaState.FaintedPlayerIds.Count == 0) return;

            // Remove jogadores desmaiados da lista de inimigos do grupo
            var enemiesToRemove = new List<IPlayer>();
            foreach (var kvp in __instance.BotsGroup.Enemies)
            {
                if (TraumaState.FaintedPlayerIds.Contains(kvp.Key.ProfileId))
                {
                    enemiesToRemove.Add(kvp.Key);
                }
            }

            foreach (var enemy in enemiesToRemove)
            {
                __instance.BotsGroup.RemoveEnemy(enemy);
                __instance.Memory.DeleteInfoAboutEnemy(enemy);
            }
        }
    }
}