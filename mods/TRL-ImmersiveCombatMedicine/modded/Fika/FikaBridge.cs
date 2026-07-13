using System;
using EFT;
using HarmonyLib;
using Comfort.Common;
using UnityEngine;

namespace TrueTrauma
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
            // ref: RANGE-READY — a duração vem do DEADLINE gravado na entrada (não da
            // config): quando a duração virar aleatória por desmaio (min-max futuro),
            // o pacote carrega automaticamente o valor ROLADO daquele desmaio.
            float duration = TRLImmersiveCombatMedicine.TRLImmersiveCombatMedicinePlugin.ConfigBlackoutDuration.Value;
            if (isFainted && TraumaState.BlackoutTimers.TryGetValue(player.ProfileId, out float deadline))
                duration = UnityEngine.Mathf.Max(1f, deadline - UnityEngine.Time.time);
            Band_Aid.BandAidNetworkHandler.SendTraumaFaintPacket(player.ProfileId, isFainted, duration, duration + 5f);
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
