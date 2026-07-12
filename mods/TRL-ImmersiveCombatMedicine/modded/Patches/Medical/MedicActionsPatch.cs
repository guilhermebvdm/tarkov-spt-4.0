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

        [HarmonyPostfix]
        public static void Postfix(GamePlayerOwner owner, GInterface177 interactive, ref ActionsReturnClass __result)
        {
            // Só age quando o vanilla não produziu ações e o alvo é nosso componente
            // — nunca sobrescreve loot/portas/Fika revive.
            if (__result == null && interactive is MedicInteractable medic)
            {
                __result = medic.GetActions(owner);
            }
        }
    }
}
