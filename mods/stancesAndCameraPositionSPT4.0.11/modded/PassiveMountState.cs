using UnityEngine;

namespace CameraRotationMod
{
    public enum EBracingDir { None, Top, Left, Right }

    // Estado do mount PASSIVO (item 011). O mount ATIVO é 100% vanilla — isto é apenas a camada de
    // "arma encostada numa superfície, sem a tecla". Atualizado pelo PassiveMountDetectPatch (hook em
    // Player.FirearmController.method_11) e revalidado/solto pelo PassiveMountUI.Update (timeout, caso o
    // detector pare de rodar com a arma guardada) e no raid end. Reflete SOMENTE o jogador local.
    public static class PassiveMountState
    {
        public static bool IsBracing { get; private set; }
        public static EBracingDir Direction { get; private set; }

        // Time.time da última passada do detector (positiva OU negativa). Usado para soltar o estado
        // quando method_11 deixa de ser chamado (PA-01-03).
        public static float LastDetectTick;

        public static void SetBracing(EBracingDir dir)
        {
            IsBracing = true;
            Direction = dir;
        }

        public static void ClearBracing()
        {
            IsBracing = false;
            Direction = EBracingDir.None;
        }

        public static void Reset()
        {
            IsBracing = false;
            Direction = EBracingDir.None;
            LastDetectTick = 0f;
        }
    }
}
