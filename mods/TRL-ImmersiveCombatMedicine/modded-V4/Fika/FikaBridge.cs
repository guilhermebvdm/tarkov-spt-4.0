using System;
using EFT;
using HarmonyLib;
using Comfort.Common;
using UnityEngine;
using TRLImmersiveCombatMedicine.Trauma;
using TRLImmersiveCombatMedicine.Medical;

namespace TRLImmersiveCombatMedicine.Fika
{
    public static class FikaBridge
    {
        // Chama isso quando desmaiar/acordar
        public static void SyncFaintStatus(Player player, bool isFainted)
        {
            if (player == null) return;

            // Atualiza localmente
            UpdateFaintedList(player.ProfileId, isFainted);

            // ref: CR-02 — AUTORIDADE de emissão: só o processo DONO do estado envia
            // pacote sobre este ProfileId (humano local ou bot deste host). Sem o
            // guard, um futuro fix do CR-01-28 faria o host emitir false prematuro
            // sobre players remotos usando a config local errada.
            if (!player.IsYourPlayer && !player.IsAI) return;

            // ref: CR-01-02 — propaga aos peers (host espelha timers e controla o
            // aggro dos bots). A duração viaja no pacote.
            // ref: item 008 (RANGE-READY concluído) — o pacote carrega o valor ROLADO daquele
            // desmaio (via BlackoutTimers, linha abaixo) sempre que isFainted=true; o fallback só
            // serve o caso defensivo em que BlackoutTimers ainda não tem entrada (não deveria
            // ocorrer — HealthPatches.cs grava os dois juntos — mas evita duration<=0 no pacote).
            // Fallback usa o MÍNIMO configurado (não o antigo ConfigBlackoutDuration, removido):
            // errar para o lado de uma duração mais curta é mais seguro que travar o alvo mais
            // tempo que o configurado.
            float duration = TRLImmersiveCombatMedicinePlugin.ConfigBlackoutDurationMin.Value;
            if (isFainted && TraumaState.BlackoutTimers.TryGetValue(player.ProfileId, out float deadline))
                duration = Mathf.Max(1f, deadline - Time.time);
            BandAidNetworkHandler.SendTraumaFaintPacket(player.ProfileId, isFainted, duration, duration + 5f);
        }

        // Método auxiliar para gerenciar a lista
        public static void UpdateFaintedList(string profileId, bool isFainted)
        {
            if (isFainted)
            {
                if (!TraumaState.FaintedPlayerIds.Contains(profileId))
                    TraumaState.FaintedPlayerIds.Add(profileId);
            }
            else
            {
                if (TraumaState.FaintedPlayerIds.Contains(profileId))
                    TraumaState.FaintedPlayerIds.Remove(profileId);
            }
        }
    }
}
