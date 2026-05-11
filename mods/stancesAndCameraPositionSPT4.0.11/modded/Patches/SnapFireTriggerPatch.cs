using System;
using System.Reflection;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CameraRotationMod.Patches
{
    /// <summary>
    /// backlog 002 F4 — Snap to Stance 0 on Fire.
    ///
    /// Prefix em `Player.FirearmController.SetTriggerPressed(bool)` (ref Player.cs:13668).
    /// Resolução por reflection — ver Plugin.ResolveFirearmControllerSetTrigger.
    ///
    /// HISTÓRICO TÉCNICO (06-fix-01): O patch target ANTERIOR era a operation-base aninhada
    /// (Player.cs:3810). Falhou porque C# virtual dispatch executa o IL do override diretamente,
    /// e apenas 1 de 14 overrides chama `base.SetTriggerPressed()` (Player.cs:3184). Os 13 overrides
    /// restantes — incluindo o default weapon op em Player.cs:2712 — pulam a base, então o Prefix
    /// nunca disparava. Patchear `Player.FirearmController.SetTriggerPressed` (linha 13668, routing
    /// para CurrentOperation) captura todos os caminhos ANTES da virtual dispatch.
    ///
    /// Estratégia (preservada do design original):
    /// - press=true em stance snap-elegível → bloqueia o trigger (return false), snap imediato.
    /// - press=false: agenda ressurreição para frame seguinte se elapsed >= threshold.
    /// - Frame N+1: dispatch synthetic true. Frame N+2: dispatch synthetic false (para fullauto não esvaziar mag).
    /// - Reentry guard via [ThreadStatic] _inSyntheticCall — sem isso, MethodBase.Invoke chamaria
    ///   o próprio Prefix do método patcheado → recursão infinita.
    /// </summary>
    public class SnapFireTriggerPatch : ModulePatch
    {
        // PA-02-01 reentry guard. Harmony patcheia o método; chamar MethodBase.Invoke nele passa
        // pelo wrapper → Prefix dispara de novo. [ThreadStatic] cobre callers em qualquer thread.
        [ThreadStatic] private static bool _inSyntheticCall;

        // PA-02-04 + PA-03-01: cachear args evita allocation por button-up.
        private static readonly object[] _trueArgs  = new object[] { true };
        private static readonly object[] _falseArgs = new object[] { false };

        private static MethodBase _originalSetTrigger;

        protected override MethodBase GetTargetMethod()
        {
            // PA-02-05: GetTargetMethod NÃO deve ser chamado se FirearmControllerSetTrigger for null —
            // Plugin.Awake garante isso checando antes de Enable(). Defesa em profundidade: throw em
            // vez de retornar null silencioso (Harmony.Patch lançaria NRE de qualquer forma).
            _originalSetTrigger = Plugin.FirearmControllerSetTrigger
                ?? throw new InvalidOperationException(
                    "SnapFireTriggerPatch não deveria ser registrado quando " +
                    "Plugin.FirearmControllerSetTrigger é null. Awake deve checar antes de Enable().");
            return _originalSetTrigger;
        }

        /// <summary>
        /// Despacha trigger sintético (true ou false) fora da pilha do Prefix.
        /// PA-03-01: pulse de 2 frames — true em N+1, false em N+2 — para parar fullauto após ~1 tiro.
        /// Chamado por StanceManager.TryDispatchPendingResurrect via Update.
        /// </summary>
        public static void RaiseSyntheticTrigger(Player.FirearmController fc, bool pressed)
        {
            if (fc == null || _originalSetTrigger == null) return;
            _inSyntheticCall = true;
            try { _originalSetTrigger.Invoke(fc, pressed ? _trueArgs : _falseArgs); }
            catch (Exception ex) { Plugin.Logger.LogError($"[F4] synthetic trigger {pressed} failed: {ex}"); }
            finally { _inSyntheticCall = false; }
        }

        [PatchPrefix]
        private static bool Prefix(Player.FirearmController __instance, bool pressed)
        {
            // PA-02-01: bypass durante a ressurreição — deixa o trigger sintético passar.
            if (_inSyntheticCall) return true;

            try
            {
                // ref: CR-01-01 — filtrar para apenas MainPlayer (em multiplayer Fika, o patch
                // dispara para FirearmControllers de outros players; snap só faz sentido no local).
                var mainPlayer = Singleton<GameWorld>.Instance?.MainPlayer;
                if (mainPlayer == null || __instance != mainPlayer.HandsController) return true;

                if (pressed)
                {
                    if (StanceManager.TryInterceptTriggerDown(__instance))
                        return false;     // skip original: tiro NÃO sai
                    return true;           // caminho normal — sem snap
                }
                else
                {
                    // PA-02-03 + PA-03-01: agenda ressurreição (synthetic true) e reset (synthetic false)
                    // para os frames seguintes. Sem closure no Prefix (PA-02-04).
                    StanceManager.OnTriggerUpAfterIntercept(__instance, _originalSetTrigger);
                    return true;           // button-up natural sempre propaga (operation precisa resetar)
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"[SnapFireTriggerPatch] {ex}");
                return true; // defensivo: em erro, vanilla segue
            }
        }
    }
}
