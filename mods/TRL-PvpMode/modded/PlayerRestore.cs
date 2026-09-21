using System;
using EFT;
using UnityEngine;

namespace TarkovRedLine.PvpMode
{
    /// <summary>
    /// Devolve o personagem ao estado "inteiro" depois de renascer.
    ///
    /// Restaurar a vida não basta: o EFT guarda o desgaste do jogador em três sistemas
    /// independentes, e cada um precisa ser limpo pelo seu próprio caminho.
    ///
    ///  1. <b>Vida e membros</b> — <c>RestoreFullHealth</c>.
    ///  2. <b>Efeitos de saúde</b> — fratura, dor, tremor, contusão, atordoamento, fadiga crônica.
    ///     Ficam registrados POR MEMBRO, e intoxicação é marcada como permanente, ficando de fora
    ///     da remoção comum.
    ///  3. <b>Recursos físicos</b> — stamina de perna, stamina de braço, oxigênio e fadiga. NÃO são
    ///     efeitos: vivem em <c>Player.Physical</c> e sobrevivem intactos a qualquer limpeza de
    ///     efeito. É o que fazia o jogador renascer com o braço tremendo e sem fôlego.
    ///
    /// Nomes das interfaces de efeito confirmados pelo mapa de deofuscação 4.1
    /// (<c>docs/files-from-4.1/consolidated-mappings.txt</c>) e conferidos no dump 4.0.
    /// </summary>
    internal static class PlayerRestore
    {
        public static void RestoreAfterRespawn(Player player)
        {
            if (player == null) return;

            RestoreHealthAndEffects(player);
            RestorePhysicalResources(player);
            ClearCombatMedicineTrauma(player);
        }

        /// <summary>
        /// Vida, membros destruídos e todo efeito negativo.
        ///
        /// <c>RemoveNegativeEffects</c> age POR MEMBRO (ActiveHealthController.cs:3637) — chamá-lo
        /// só com <c>EBodyPart.Common</c> deixava fratura de perna, dor e tremor intactos, e o
        /// jogador renascia mancando.
        ///
        /// O filtro interno dele ignora duas famílias: <c>IDesirable</c> (buffs — analgésico,
        /// berserk) e <c>IPermanent</c>. Intoxicação é permanente, então só sai pelo método
        /// dedicado abaixo. Manter os buffs é proposital: não faz sentido tirar o efeito bom de
        /// quem acabou de renascer.
        /// </summary>
        private static void RestoreHealthAndEffects(Player player)
        {
            var hc = player.ActiveHealthController;
            if (hc == null) return;

            try { hc.RestoreFullHealth(); }
            catch (Exception ex) { Plugin.Log.LogWarning($"[TRL-PvpMode] RestoreFullHealth: {ex.Message}"); }

            foreach (EBodyPart part in Enum.GetValues(typeof(EBodyPart)))
            {
                try { hc.RemoveNegativeEffects(part); }
                catch (Exception ex) { Plugin.Log.LogWarning($"[TRL-PvpMode] Efeitos de {part}: {ex.Message}"); }
            }

            // Intoxicação comum e letal: marcadas como permanentes, têm remoção própria.
            // ref: Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs:3655
            try { hc.method_18(); }
            catch (Exception ex) { Plugin.Log.LogWarning($"[TRL-PvpMode] Intoxicacao: {ex.Message}"); }
        }

        /// <summary>
        /// Stamina de perna, stamina de braço, oxigênio e fadiga.
        ///
        /// Estes NÃO são efeitos de saúde: ficam em <c>Player.Physical</c>
        /// (Assembly-CSharp/EFT/Player.cs:24428) e nenhuma limpeza de efeito os alcança. Por isso
        /// o jogador renascia com o braço tremendo e cansado mesmo com a vida cheia.
        ///
        /// <c>Overuse</c> é a penalidade acumulada por uso excessivo — é ela que faz a barra
        /// drenar mais rápido e demorar a voltar. O próprio jogo zera as três em
        /// <c>PlayerPhysicalClass.method_24()</c> (PlayerPhysicalClass.cs:1076).
        ///
        /// <c>InvokeChangedAction()</c> é obrigatório depois de escrever: é o que avisa a interface
        /// e os ouvintes da mudança. Sem ele o valor muda por dentro e a barra na tela continua
        /// mostrando o estado velho (mesma armadilha do AP-04).
        /// </summary>
        private static void RestorePhysicalResources(Player player)
        {
            var physical = player.Physical;
            if (physical == null) return;

            try
            {
                RefillMeter(physical.Stamina);
                RefillMeter(physical.HandsStamina);
                RefillMeter(physical.Oxygen);

                physical.Fatigue = 0f;

                // Caminho do próprio jogo para zerar as três penalidades de uso excessivo.
                if (physical is PlayerPhysicalClass playerPhysical) playerPhysical.method_24();
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning($"[TRL-PvpMode] Recursos fisicos: {ex.Message}");
            }
        }

        /// <summary>Enche um medidor até a capacidade e avisa a interface.</summary>
        private static void RefillMeter(GClass774 meter)
        {
            if (meter == null) return;

            meter.Current = meter.TotalCapacity;
            meter.Overuse = 0f;
            meter.DisableRestoration = 0f;
            meter.InvokeChangedAction();
        }

        /// <summary>
        /// Limpa o trauma do TRL-ImmersiveCombatMedicine para ESTE jogador.
        ///
        /// Cirúrgico de propósito: o mod expõe um <c>ResetAll()</c>, mas ele apaga o estado de
        /// TODOS os jogadores da partida — em cooperativo isso limparia o desmaio de quem está
        /// caído do outro lado do mapa. Removemos só as entradas do identificador deste jogador.
        /// </summary>
        private static void ClearCombatMedicineTrauma(Player player)
        {
            try { FikaBridge.ClearCombatMedicineTraumaFor(player.ProfileId); }
            catch (Exception ex) { Plugin.Log.LogWarning($"[TRL-PvpMode] Trauma do ICM: {ex.Message}"); }
        }
    }
}
