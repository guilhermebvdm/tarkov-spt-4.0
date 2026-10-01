using System.Collections.Generic;
using System.Reflection;
using EFT.Interactive;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SkillsExtended.Skills.SilentOps.Patches;

// Grava, por porta, se a interação em andamento é do jogador local. WorldInteractiveObject.PlaySound
// (patcheado por DoorSoundPatch) não recebe esse dado diretamente — só SmoothDoorOpenCoroutine tem
// `isLocalInteraction`, um nível acima na pilha de chamada.
// ref: AUD-01-01 (relatorio-auditoria-codigo-01.md)
internal class DoorInteractionTrackerPatch : ModulePatch
{
    // chave = WorldInteractiveObject.Id (string)
    // ref: Assembly-CSharp/EFT.Interactive/WorldInteractiveObject.cs:468
    internal static readonly Dictionary<string, bool> LastInteractionIsLocal = new();

    protected override MethodBase GetTargetMethod()
    {
        // ref: Assembly-CSharp/EFT.Interactive/WorldInteractiveObject.cs:1088
        // SmoothDoorOpenCoroutine(EDoorState state, bool isLocalInteraction, float speed = 1f)
        // virtual — overrides auditados em Door.cs:394 e KeycardDoor.cs:74, ambos chamam base.
        return AccessTools.Method(typeof(WorldInteractiveObject), nameof(WorldInteractiveObject.SmoothDoorOpenCoroutine));
    }

    [PatchPrefix]
    private static void Prefix(WorldInteractiveObject __instance, bool isLocalInteraction)
    {
        // O Prefix roda de forma síncrona na CONSTRUÇÃO do IEnumerator (quando SmoothDoorOpenCoroutine
        // é chamado), antes de qualquer yield — o valor fica gravado antes da coroutine começar a rodar.
        LastInteractionIsLocal[__instance.Id] = isLocalInteraction;
    }
}
