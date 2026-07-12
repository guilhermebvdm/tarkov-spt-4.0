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

    /// <summary>
    /// Estende o alcance do prompt médico para a detecção NATIVA de players
    /// (PLAYER_RAYCAST_DISTANCE = 2,5 m): entre ~1,3 m (limite de InteractableObject
    /// genérico no InteractionRaycast) e 2,5 m o vanilla seta Player.InteractablePlayer
    /// mas não gera ações — este postfix preenche o painel a partir do
    /// MedicInteractable do alvo. Assim "se o prompt aparece, examinar funciona".
    /// </summary>
    [HarmonyPatch(typeof(GamePlayerOwner), nameof(GamePlayerOwner.InteractionsChangedHandler))]
    public static class MedicPlayerRangePatch
    {
        [HarmonyPostfix]
        public static void Postfix(GamePlayerOwner __instance)
        {
            // Algo já produziu ações (loot/porta/Fika revive/nosso prefix) → não tocar.
            if (__instance.AvailableInteractionState.Value != null) return;

            var target = __instance.Player != null ? __instance.Player.InteractablePlayer : null;
            if (target == null) return;

            var medic = target.GetComponent<MedicInteractable>();
            if (medic == null) return;

            var actions = medic.GetActions(__instance);
            if (actions == null) return;

            actions.InitSelected();
            __instance.AvailableInteractionState.Value = actions;
        }
    }
}
