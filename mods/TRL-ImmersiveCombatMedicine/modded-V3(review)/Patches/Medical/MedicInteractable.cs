using EFT;
using EFT.Interactive;
using UnityEngine;

namespace TRLImmersiveCombatMedicine.Medical
{
    /// <summary>
    /// InteractableObject anexado a cada player/bot vivo para que o raycast de
    /// interação NATIVO (Player.InteractionRaycast → GameWorld.FindInteractable →
    /// GetComponentInParent&lt;InteractableObject&gt;) o encontre — mesmo pipeline de
    /// loot/portas. As ações entram no ActionPanel nativo via MedicActionsPatch
    /// (padrão do ReviveInteractable do Fika 2.3.4).
    /// </summary>
    public class MedicInteractable : InteractableObject
    {
        public Player Target { get; private set; }

        /// <returns>true se anexou um componente novo neste chamado.</returns>
        public static bool Ensure(Player target)
        {
            if (target == null || target.gameObject == null) return false;
            if (target.gameObject.GetComponent<MedicInteractable>() != null) return false;
            var comp = target.gameObject.AddComponent<MedicInteractable>();
            comp.Target = target;
            return true;
        }

        public ActionsReturnClass GetActions(GamePlayerOwner owner)
        {
            if (Target == null || owner?.Player == null) return null;
            if (Target == owner.Player) return null;

            // Morto → remover o componente para o raycast voltar a achar o Corpse
            // (senão este InteractableObject pode "roubar" o prompt de loot do corpo).
            if (Target.HealthController == null || !Target.HealthController.IsAlive)
            {
                Destroy(this);
                return null;
            }

            var actions = new ActionsReturnClass();
            actions.Actions.Add(new ActionsTypesClass
            {
                Action = Examine,
                Name = MedicLocale.Get(MedicTextId.ActionExamine)
            });
            actions.Actions.Add(new ActionsTypesClass
            {
                Action = ShoulderTap,
                Name = MedicLocale.Get(MedicTextId.ActionShoulderTap)
            });
            return actions;
        }

        private void Examine()
        {
            BandAidController.Instance?.ActivateMedicModeExternal(Target);
        }

        private void ShoulderTap()
        {
            BandAidController.Instance?.SendShoulderTapExternal(Target);
        }
    }
}
