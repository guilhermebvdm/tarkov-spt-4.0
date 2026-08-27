using System;
using EFT;
using HarmonyLib;
using UnityEngine;

namespace TRLFixes.Patches
{
    /// <summary>
    /// Previne a trava de controles ao pegar/equipar um item do chão (Pickup) enquanto a mira é desligada.
    ///
    /// Sintoma: o corpo do personagem congela — não anda, não agacha, não vira a visão — mas o inventário e a
    /// troca de arma continuam respondendo.
    ///
    /// Mecanismo (confirmado no decompilado, Player.cs:12142 e :14569-14588): o Pickup nativo inicia uma
    /// transição na máquina de estados de movimento e, no meio dela, desliga a mira. O setter
    /// FirearmController.IsAiming chama method_63/method_64, que acessam `FirearmsAnimator` SEM checar null.
    /// Enquanto o inventário reconstrói a hierarquia do equipamento existe uma janela em que esse animador é
    /// nulo → NullReferenceException dentro do setter → a transição do MovementContext aborta no meio e o
    /// bloqueio de input nunca é liberado.
    ///
    /// O finalizer engole SOMENTE NullReferenceException nesse setter, deixando a transição concluir.
    ///
    /// ⚠️ Isto é um remendo sobre bug do jogo base, não uma correção da causa. Por isso o logging é forense:
    /// a PRIMEIRA ocorrência sai com a pilha de chamadas completa (é ela que confirma — ou refuta — o
    /// mecanismo acima, que nunca foi capturado em raid), e as seguintes saem com throttle para não inundar
    /// o console. Se o log mostrar uma origem diferente da descrita, o remendo está mascarando outra coisa e
    /// precisa ser reavaliado.
    /// </summary>
    public class PickupAimingSafetyPatch
    {
        private const float ThrottleSeconds = 5f;

        private static bool _firstOccurrenceLogged;
        private static int _swallowedCount;
        private static float _lastLogTime = -999f;

        public void Enable()
        {
            try
            {
                var harmony = new Harmony("com.trl.fixes.pickupaimingsafety");
                var setter = AccessTools.PropertySetter(typeof(Player.FirearmController), "IsAiming");

                if (setter == null)
                {
                    Plugin.Log?.LogError("TRL-Fixes: setter FirearmController.IsAiming nao encontrado — PickupAimingSafetyPatch NAO aplicado.");
                    return;
                }

                var finalizer = AccessTools.Method(typeof(PickupAimingSafetyPatch), nameof(Finalizer));
                harmony.Patch(setter, finalizer: new HarmonyMethod(finalizer));
                Plugin.Log?.LogInfo("TRL-Fixes: Hook no FirearmController.IsAiming (pickup/equip safety) aplicado com sucesso!");
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"TRL-Fixes: Erro ao aplicar patch PickupAimingSafetyPatch: {ex}");
            }
        }

        public static Exception Finalizer(Exception __exception)
        {
            if (!(__exception is NullReferenceException)) return __exception;

            _swallowedCount++;

            if (!_firstOccurrenceLogged)
            {
                _firstOccurrenceLogged = true;
                _lastLogTime = Time.realtimeSinceStartup;
                // Pilha completa: é o que permite confirmar se a origem e mesmo method_63/method_64 tocando
                // um FirearmsAnimator nulo, ou se outro mod esta produzindo NRE neste mesmo setter.
                Plugin.Log?.LogWarning(
                    "[TRL-Fixes] Preveniu trava de controles no FirearmController.IsAiming (pickup/equip race condition). " +
                    $"PRIMEIRA ocorrencia — pilha completa abaixo para diagnostico:\n{__exception}");
                return null;
            }

            float now = Time.realtimeSinceStartup;
            if (now - _lastLogTime >= ThrottleSeconds)
            {
                _lastLogTime = now;
                Plugin.Log?.LogWarning(
                    $"[TRL-Fixes] Trava de controles prevenida no FirearmController.IsAiming ({_swallowedCount}x nesta sessao): {__exception.Message}");
            }

            return null; // engole a NRE — a transição do MovementContext conclui e o input não trava
        }
    }
}
