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

        // PREFIX (não postfix), espelhando o Fika: para tipo desconhecido o vanilla
        // pode devolver ActionsReturnClass não-nulo/vazio ou lançar — bypass total
        // quando o alvo é nosso componente; qualquer outro tipo segue intocado.
        [HarmonyPrefix]
        public static bool Prefix(GamePlayerOwner owner, GInterface177 interactive, ref ActionsReturnClass __result)
        {
            if (interactive is MedicInteractable medic)
            {
                __result = medic.GetActions(owner);
                return false;
            }
            return true;
        }
    }

    // NOTA: o antigo MedicPlayerRangePatch (postfix em InteractionsChangedHandler,
    // range preso aos 2,5 m do InteractablePlayer vanilla) foi substituído pelo
    // prompt dirigido do BandAidController.UpdateNativePrompt — regra única de
    // distância via config, sem flicker de evento e sem dois escritores brigando
    // pelo AvailableInteractionState.
}
