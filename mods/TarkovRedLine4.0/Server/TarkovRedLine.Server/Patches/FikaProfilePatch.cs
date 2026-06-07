using System.Collections.Generic;

namespace TarkovRedLine.Server.Patches;

public static class FikaProfilePatch
{
    public static void Enable()
    {
        // TODO: Em C#, o ProfileHelper é da classe SPTarkov.Server.Core.Services.ProfileHelper.
        // Precisamos usar Harmony para dar um Postfix no GetCompleteProfile() e garantir que
        // profile.Characters.Pmc.TradersInfo e profile.Dialogues sejam instanciados se vierem nulos.
        
        /* Exemplo de uso do Harmony:
         * var harmony = new Harmony("com.saraiva.tarkovredline");
         * var original = AccessTools.Method(typeof(ProfileHelper), "GetCompleteProfile");
         * var postfix = AccessTools.Method(typeof(FikaProfilePatch), "Postfix");
         * harmony.Patch(original, postfix: new HarmonyMethod(postfix));
         */
    }

    // public static void Postfix(ref SptProfile __result)
    // {
    //     if (__result?.Characters?.Pmc != null)
    //     {
    //         if (__result.Characters.Pmc.TradersInfo == null)
    //             __result.Characters.Pmc.TradersInfo = new Dictionary<string, TraderInfo>();
    //     }
    //     if (__result?.Dialogues == null)
    //     {
    //         __result.Dialogues = new Dictionary<string, Dialogue>();
    //     }
    // }
}
