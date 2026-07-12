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

            // Atualiza localmente primeiro (para Single Player funcionar igual)
            UpdateFaintedList(player.ProfileId, isFainted);

            // Se for Multiplayer (Fika), envia o sinal pela rede usando o pacote customizado INetSerializable
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.fika.core"))
            {
                try
                {
                    FikaPacketManager.SendTraumaFaintPacket(player.ProfileId, isFainted);
                }
                catch (Exception ex)
                {
                    TraumaState.Logger.LogError(string.Format("TrueTrauma: Erro ao enviar pacote Fika: {0}", ex.Message));
                }
            }
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