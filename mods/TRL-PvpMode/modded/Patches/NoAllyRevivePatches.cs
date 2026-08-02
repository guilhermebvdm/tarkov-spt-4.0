using System;
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TarkovRedLine.PvpMode.Patches
{
    /// <summary>
    /// Tira "levantar companheiro" do menu de interação — a única saída do caído é renascer.
    ///
    /// Mantém o ragdoll de propósito: o corpo continua desligado nos outros clientes, e o fix de
    /// colisão do TRL-Fixes (que engancha em ReviveInteractable.RemoveRagdoll) continua valendo
    /// quando o jogador voltar.
    ///
    /// ⚠️ Roda no cliente de QUEM OLHA. Um par sem este mod continua enxergando a opção e consegue
    /// levantar o caído, furando a contagem de vidas. O mod precisa estar em todos os clientes
    /// (review 01, R-10) — blindagem pelo lado da vítima fica para o item 003.
    /// </summary>
    public class NoAllyRevivePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            // ref: fika-plugin/Fika.Core/Main/Components/ReviveInteractable.cs:155
            // Método public num tipo internal sealed — daí resolver o tipo por nome.
            => AccessTools.Method(FikaBridge.ReviveInteractableType, "GetActions");

        // __result precisa do tipo REAL de retorno; `object` geraria IL não verificável.
        [PatchPrefix]
        private static bool Prefix(ref ActionsReturnClass __result)
        {
            try
            {
                if (!Settings.ENABLED.Value) return true;

                // null já é retorno previsto pelo próprio método (:157) e o consumidor trata
                // com `?.` — ref: Assembly-CSharp/EFT/GamePlayerOwner.cs:883
                __result = null;
                return false;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TRL-PvpMode] NoAllyRevivePatch: {ex}");
                return true;
            }
        }
    }

    /// <summary>
    /// Impede que a morte dos companheiros force a morte de quem está caído.
    ///
    /// O Bleedout do Fika encerra o caído quando todos os humanos morrem (Bleedout.cs:56-61) —
    /// faz sentido quando a única saída é ser resgatado, e deixa de fazer quando o jogador tem
    /// vidas próprias e decide sozinho.
    /// </summary>
    public class BleedoutOnPlayerDeathPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            // ref: fika-plugin/Fika.Core/Main/Components/Bleedout.cs:49 (private)
            => AccessTools.Method(FikaBridge.BleedoutType, "OnPlayerDeath");

        [PatchPrefix]
        private static bool Prefix()
        {
            try
            {
                // Só bloqueia enquanto o modo estiver de fato governando a raid; fora disso o
                // comportamento nativo do Fika continua valendo.
                return !(Settings.ENABLED.Value && RaidState.HasLifeAvailable);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TRL-PvpMode] BleedoutOnPlayerDeathPatch: {ex}");
                return true;
            }
        }
    }
}
