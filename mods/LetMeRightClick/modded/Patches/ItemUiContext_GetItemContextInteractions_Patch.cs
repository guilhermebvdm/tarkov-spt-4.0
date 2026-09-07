using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace LetMeRightClick.Patches;

internal class ItemUiContext_GetItemContextInteractions_Patch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ItemUiContext)
            .GetMethod(nameof(ItemUiContext.ShowContextMenu));
    }

    [PatchTranspiler]
    public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> codeInstructions)
    {
        var instr = codeInstructions
            .ToList();

        instr.RemoveRange(0, 17);

        return instr;
    }
}