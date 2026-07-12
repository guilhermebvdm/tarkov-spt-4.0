using EFT;
using EFT.Interactive;
using UnityEngine;

namespace TRLImmersiveCombatMedicine
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

        public static void Ensure(Player target)
        {
            if (target == null || target.gameObject == null) return;
            if (target.gameObject.GetComponent<MedicInteractable>() != null) return;
            var comp = target.gameObject.AddComponent<MedicInteractable>();
            comp.Target = target;
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
                Name = "Examinar (Médico)"
            });
            actions.Actions.Add(new ActionsTypesClass
            {
                Action = ShoulderTap,
                Name = "Tocar no ombro"
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
