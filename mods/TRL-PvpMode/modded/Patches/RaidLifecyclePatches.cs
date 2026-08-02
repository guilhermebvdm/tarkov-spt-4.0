using System;
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TarkovRedLine.PvpMode.Patches
{
    /// <summary>
    /// Início de raid: zera o estado, valida as pré-condições e aplica o tempo de decisão.
    /// ref: Assembly-CSharp/EFT/GameWorld.cs:2584 (public virtual)
    /// </summary>
    public class GameWorldOnGameStartedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            => AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));

        [PatchPostfix]
        private static void Postfix(GameWorld __instance)
        {
            // RaidState.Begin já faz as guardas de contexto (nulo / esconderijo).
            try { RaidState.Begin(__instance); }
            catch (Exception ex) { Plugin.Log.LogError($"[TRL-PvpMode] GameWorldOnGameStartedPatch: {ex}"); }
        }
    }

    /// <summary>
    /// Fim de raid. <see cref="RaidState.End"/> é idempotente.
    /// ref: Assembly-CSharp/EFT/GameWorld.cs:2111 (public virtual)
    ///
    /// Nota: BaseLocalGame&lt;TPlayerOwner&gt; NÃO é patchado. É genérico aberto (o Harmony recusa) e
    /// CoopGame sobrescreve Stop sem chamar a base, então um postfix na base nunca rodaria (AP-03).
    /// GameWorld.OnDestroy cobre todos os caminhos de saída na prática — mesmo caminho já
    /// percorrido pelo mod de stances.
    /// </summary>
    public class GameWorldOnDestroyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            => AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnDestroy));

        [PatchPostfix]
        private static void Postfix()
        {
            try { RaidState.End(); }
            catch (Exception ex) { Plugin.Log.LogError($"[TRL-PvpMode] GameWorldOnDestroyPatch: {ex}"); }
        }
    }
}
