using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace CameraRotationMod.Patches
{
    /// <summary>
    /// Gap reportado pelo usuário (2026-09-09): dá pra continuar rastejando em prone enquanto checa
    /// carregador/câmara (a animação de mão toca, mas o personagem continua andando pelo WASD). O
    /// usuário confirmou que a recarga (reload) já bloqueia movimento normalmente (nativo/vanilla) —
    /// então o problema é específico de ações "rápidas" de mão (checar mag, checar câmara, examinar
    /// arma, etc.) que a EFT deixa combinar com movimento por design em pé/agachado, mas que o usuário
    /// quer bloqueado especificamente em PRONE.
    ///
    /// Escopo confirmado pelo usuário: bloquear movimento WASD durante QUALQUER ação de mão, mas
    /// SÓ em prone — em pé/agachado o comportamento nativo (mover durante ação de mão) fica intocado.
    ///
    /// Ponto de patch: <see cref="MovementState.ApplyMotion(ref Vector3, float)"/> — já é <c>ref</c>
    /// nativamente (ref: Assembly-CSharp/EFT/MovementState.cs:164-172), não precisa do truque de
    /// Harmony pra parâmetro por valor. Nem <c>RunStateClass</c> nem <c>ProneMoveStateClass</c>
    /// sobrescrevem este método (auditado — só <c>LimitMotion</c>/<c>ManualAnimatorMoveUpdate</c> são
    /// overridden em ProneMoveStateClass), então o Prefix cobre o caminho real de movimento em prone
    /// via despacho virtual normal.
    ///
    /// Detecção de "mão ocupada": <see cref="FirearmsAnimator.IsIdling"/> — o mesmo sinal que a
    /// própria EFT já usa pra decidir se pode iniciar uma NOVA interação de mão (ref:
    /// Assembly-CSharp/EFT/MovementContext.cs:3171, <c>SetInteractInHands</c>: checa
    /// <c>!_player.HandsController.FirearmsAnimator.IsIdling()</c>) — reutilizamos o mesmo conceito
    /// nativo em vez de inventar uma detecção própria de "ação de mão".
    ///
    /// Zera o vetor de motion inteiro (não só a componente horizontal): o personagem não cai/pula
    /// estando prone, então zerar tudo não tem efeito colateral de queda — apenas trava a translação
    /// no lugar enquanto a animação de mão roda, sem tocar em rotação/mira.
    /// </summary>
    public class ProneHandsBusyMovementBlockPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(MovementState), nameof(MovementState.ApplyMotion), new[] { typeof(Vector3).MakeByRefType(), typeof(float) });
        }

        [PatchPrefix]
        private static void Prefix(MovementState __instance, ref Vector3 motion)
        {
            try
            {
                MovementContext mc = __instance.MovementContext;
                if (mc == null || !mc.IsInPronePose) return;

                Player player = Traverse.Create(mc).Field<Player>("_player").Value;
                if (player == null || !player.IsYourPlayer) return;

                FirearmsAnimator firearmsAnimator = player.HandsController?.FirearmsAnimator;
                if (firearmsAnimator != null && !firearmsAnimator.IsIdling())
                {
                    motion = Vector3.zero;
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogError($"[ProneHandsBusyMovementBlock] Prefix falhou, caindo pro vanilla: {ex}");
            }
        }
    }
}
