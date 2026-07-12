using System.Linq;
using System.Reflection;
using EFT;
using HarmonyLib;

namespace TRLImmersiveCombatMedicine
{
    /// <summary>
    /// Injeta as ações médicas no painel de interação NATIVO (o mesmo do loot).
    /// Alvo: GetActionsClass.GetAvailableActions(GamePlayerOwner, GInterface177) —
    /// mesmo ponto que o Fika 2.3.4 patcheia para o prompt de revive
    /// (GetActionsClass_GetAvailableActions_Patch).
    /// </summary>
    [HarmonyPatch]
    public static class MedicActionsPatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(GetActionsClass)
                .GetMethods()
                .First(x => x.Name == nameof(GetActionsClass.GetAvailableActions)
                    && x.GetParameters()[0].ParameterType == typeof(GamePlayerOwner));
        }

        // [DEBUG-ICM] última chamada logada — remover após diagnóstico
        private static float _dbgNextLog = 0f;

        // PREFIX (não postfix), espelhando o Fika: para tipo desconhecido o vanilla
        // pode devolver ActionsReturnClass não-nulo/vazio ou lançar — bypass total
        // quando o alvo é nosso componente; qualquer outro tipo segue intocado.
        [HarmonyPrefix]
        public static bool Prefix(GamePlayerOwner owner, GInterface177 interactive, ref ActionsReturnClass __result)
        {
            // [DEBUG-ICM] o que o pipeline nativo está entregando ao painel (1 log/2s)
            if (interactive != null && UnityEngine.Time.time >= _dbgNextLog)
            {
                _dbgNextLog = UnityEngine.Time.time + 2f;
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning(
                    $"[DEBUG-ICM] GetAvailableActions: interactive={interactive.GetType().Name}");
            }

            if (interactive is MedicInteractable medic)
            {
                __result = medic.GetActions(owner);
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning(
                    $"[DEBUG-ICM] MedicInteractable interceptado → ações={__result?.Actions?.Count ?? 0}");
                return false;
            }
            return true;
        }
    }
}
