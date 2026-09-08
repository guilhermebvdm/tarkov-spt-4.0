using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using SkillsExtended.Helpers;
using SPT.Reflection.Patching;

namespace SkillsExtended.Skills.LockPicking.Patches;

[IgnoreAutoPatch]
public class KeyCardDoorActionPatch : ModulePatch
{
    // ref: AUD-01-22 — assinatura de smethod_9 mudou (GetActionsClass.cs:1705, hoje trata loot de
    // container/trader, não porta com keycard). Feature incompleta e desativada ([IgnoreAutoPatch]).
    // Ver 004-corrigir-achados-restantes-auditoria-01-02-spec-tech.md §1.7 antes de reativar: é
    // preciso re-resolver o alvo real por assinatura, não só remover [IgnoreAutoPatch].
    protected override MethodBase GetTargetMethod() =>
        typeof(GetActionsClass).GetMethod("smethod_9", BindingFlags.Public | BindingFlags.Static);

    [PatchPostfix]
    private static void Postfix(ref ActionsReturnClass __result, GamePlayerOwner owner, KeycardDoor door)
    {
        /*
        if (WorldInteractionUtils.IsBotInteraction(owner)
            || !SkillsExtendedPlugin.SkillData.LockPicking.Enabled
            || Singleton<GameWorld>.Instance.MainPlayer.Side == EPlayerSide.Savage)
        {
            return;
        }

        door.AddInspectInteraction(__result, owner);
        door.AddKeyCardInteraction(__result, owner);
        */
    }
}