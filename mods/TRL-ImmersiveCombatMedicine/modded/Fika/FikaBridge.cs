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

            // ref: CR-01-02 — propaga aos peers (host espelha timers e controla o
            // aggro dos bots). Antes era local-only: bots continuavam atirando no
            // client desmaiado porque o host nunca sabia do desmaio.
            Band_Aid.BandAidNetworkHandler.SendTraumaFaintPacket(player.ProfileId, isFainted);
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
