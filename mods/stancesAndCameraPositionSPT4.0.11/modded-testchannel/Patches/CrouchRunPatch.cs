using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace CameraRotationMod.Patches
{
    // Item 018 — impede o SetPoseLevel(1f) que RunStateClass.EnableSprint aplicaria ao pedir sprint
    // agachado. ref: Assembly-CSharp/RunStateClass.cs:401-420
    public class CrouchRunEnableSprintPatch : ModulePatch
    {
        // Só significa "o jogo está pedindo sprint agora" (evento down/up da tecla). NUNCA usar
        // sozinho para decidir boost/dreno — ver CrouchRunMaxSpeedPatch, que recomputa o movimento
        // real a cada frame. Sem essa separação, segurar a tecla de sprint PARADO (ex.: o mesmo bind
        // usado pra "prender a respiração" durante ADS) vazaria velocidade/stamina extra.
        public static bool SprintKeyHeld;

        public static void ResetState() => SprintKeyHeld = false;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(RunStateClass), nameof(RunStateClass.EnableSprint));
        }

        [PatchPrefix]
        private static bool Prefix(RunStateClass __instance, bool enabled, bool isToggle)
        {
            try
            {
                bool featureOn = Plugin._EnableCrouchRun?.Value ?? false;
                if (!featureOn)
                {
                    return true;
                }

                // MovementContext é campo público em MovementState (base de RunStateClass).
                // ref: Assembly-CSharp/EFT/MovementState.cs:12
                MovementContext movementContext = __instance.MovementContext;

                // Só o jogador local — bots/peers Fika passam direto pro vanilla.
                Player player = Traverse.Create(movementContext).Field<Player>("_player").Value;
                if (player == null || !player.IsYourPlayer) return true;

                if (!enabled) { SprintKeyHeld = false; return true; } // deixa o original rodar (soltar tecla)
                if (movementContext.PoseLevel >= 1f || movementContext.IsInPronePose) { SprintKeyHeld = false; return true; } // já em pé / prone: fora de escopo
                if (movementContext.MovementDirection.y <= 0.1f) { return true; } // espelha a condição original (não é pedido de sprint pra frente)
                if (!movementContext.CanSprint) { SprintKeyHeld = false; return true; }

                // Réplica do lado "manter sprint" do original, SEM o SetPoseLevel(1f):
                movementContext.EnableSprint(enabled);
                SprintKeyHeld = movementContext.IsSprintEnabled;
                if (!SprintKeyHeld) { movementContext.EnableSprint(enable: false); movementContext.ResetSpeedAfterSprint(); }
                return false; // skip do original — pose não é tocada
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogError($"[CrouchRun] Prefix falhou, caindo pro vanilla: {ex}");
                SprintKeyHeld = false;
                return true;
            }
        }
    }

    // Bug real 2026-09-09 (AP-03 — despacho virtual): depois de levantar segurando sprint (agachado
    // → em pé), o jogo troca CurrentManagedState pra SprintStateClass (estado DEDICADO de sprint em
    // pé). SprintStateClass : RunStateClass SOBRESCREVE EnableSprint com implementação própria que
    // NÃO chama base.EnableSprint (ref: Assembly-CSharp/SprintStateClass.cs:97-100) — então toda
    // chamada de EnableSprint feita enquanto CurrentManagedState é SprintStateClass (inclusive soltar
    // a tecla) passa batido pelo Prefix acima (que só intercepta o MethodInfo de RunStateClass.
    // EnableSprint especificamente — despacho virtual não cruza pra um override que não chama base).
    // Resultado: SprintKeyHeld ficava PRESO em true depois de soltar o sprint em pé, e a próxima vez
    // que o jogador tentava agachar (voltando pro RunStateClass "puro"), CrouchRunMaxSpeedPatch via
    // um SprintKeyHeld obsoleto = true e reengajava o boost de corrida no meio da tentativa de
    // agachar — sintoma relatado: "aperto pra abaixar ele tenta correr em pé", só resolvia parando
    // o movimento por completo (o que eventualmente disparava um EnableSprint(false) genuíno de volta
    // no RunStateClass "puro", finalmente limpando a flag).
    //
    // Fix: Postfix dedicado no MethodInfo de SprintStateClass.EnableSprint — só RE-SINCRONIZA
    // SprintKeyHeld com o estado real (IsSprintEnabled) depois que o método próprio já rodou; não
    // precisa interceptar/pular nada aqui (SprintStateClass já está em pé, não tem pose pra forçar).
    public class SprintStateEnableSprintSyncPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(SprintStateClass), nameof(SprintStateClass.EnableSprint), new[] { typeof(bool), typeof(bool) });
        }

        [PatchPostfix]
        private static void Postfix(SprintStateClass __instance, bool enabled)
        {
            try
            {
                MovementContext movementContext = __instance.MovementContext;
                Player player = Traverse.Create(movementContext).Field<Player>("_player").Value;
                if (player == null || !player.IsYourPlayer) return;

                // Espelha a mesma regra usada no Prefix de RunStateClass.EnableSprint: só é
                // "sprint pedido de verdade" se o pedido foi aceito (IsSprintEnabled reflete o
                // resultado JÁ aplicado por SprintStateClass.EnableSprint, que rodou antes deste Postfix).
                CrouchRunEnableSprintPatch.SprintKeyHeld = enabled && movementContext.IsSprintEnabled;
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogError($"[CrouchRun] SprintStateEnableSprintSyncPatch.Postfix falhou: {ex}");
            }
        }
    }

    // Item 018 — boost condicional de MaxSpeed, animação de sprint e sobretaxa de stamina opcional
    // durante o crouch-run. Sem rampa (removida a pedido do usuário). O Prefix acima já chama
    // movementContext.EnableSprint(enabled), que aciona o dreno NATIVO de sprint normalmente
    // (confirmado in-game pelo usuário como suficiente e bem aplicado) — a sobretaxa abaixo é
    // OPCIONAL, por cima do dreno nativo (default 0 = sem sobretaxa, comportamento antigo). Re-
    // adicionada a pedido do usuário em 2026-09-09 (tinha sido removida em 2026-09-09 mais cedo na
    // mesma sessão, quando ele confirmou que o dreno nativo sozinho já bastava — voltou atrás).
    // Tudo recalculado 1x por frame a partir do movimento REAL (nunca confia só na tecla segurada).
    // ref: Assembly-CSharp/EFT/MovementContext.cs:910
    public class CrouchRunMaxSpeedPatch : ModulePatch
    {
        private const float MovementEpsilonSqr = 0.0001f;

        private static bool _animatorSprintOn; // liga/desliga "baixar arma e balançar braços"
        private static System.Action _removeStaminaSurcharge;
        private static int _lastFrame = -1;

        public static void ResetState()
        {
            _animatorSprintOn = false;
            _removeStaminaSurcharge?.Invoke();
            _removeStaminaSurcharge = null;
            _lastFrame = -1;
        }

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(MovementContext), nameof(MovementContext.MaxSpeed));
        }

        [PatchPostfix]
        private static void Postfix(MovementContext __instance, ref float __result)
        {
            Player player = Traverse.Create(__instance).Field<Player>("_player").Value;
            if (player == null || !player.IsYourPlayer) return;

            bool wantsBoost = CrouchRunEnableSprintPatch.SprintKeyHeld
                && __instance.PoseLevel < 1f
                && !__instance.IsInPronePose
                && __instance.MovementDirection.sqrMagnitude > MovementEpsilonSqr;

            if (Time.frameCount != _lastFrame) // avança os efeitos só 1x por frame, não por leitura da property
            {
                _lastFrame = Time.frameCount;

                if (wantsBoost != _animatorSprintOn)
                {
                    _animatorSprintOn = wantsBoost;
                    __instance.PlayerAnimatorEnableSprint(wantsBoost);
                }

                // Piso de postura REMOVIDO a pedido do usuário (2026-09-09): o vanilla já dá uma
                // "levantadinha" sozinho ao iniciar a corrida agachado (comportamento nativo de
                // transição, fora do nosso controle) — empilhar o piso deste mod em cima disso deixava
                // o personagem quase em pé. Ver histórico da spec técnica/asbuild pro código removido
                // (_poseFloorEngaged/_poseLevelBeforeRun/_CrouchRunPoseFloorThreshold/Target).

                // Sobretaxa de stamina OPCIONAL (default 0 = desligada) — mesmo padrão do prone-run
                // (ProneRunMaxSpeedPatch), reusando PlayerPhysicalClass.GClass773/AddConsumption.
                if (wantsBoost && _removeStaminaSurcharge == null && player.Physical?.Stamina != null)
                {
                    var consumption = new PlayerPhysicalClass.GClass773(PlayerPhysicalClass.EConsumptionType.Sprint)
                    {
                        PrimaryTarget = PlayerPhysicalClass.EConsumptionTarget.Base,
                        Delta = new GClass848<float>(() => Plugin._CrouchRunStaminaSurcharge?.Value ?? 0f)
                    };
                    _removeStaminaSurcharge = player.Physical.Stamina.AddConsumption(consumption);
                }
                else if (!wantsBoost && _removeStaminaSurcharge != null)
                {
                    _removeStaminaSurcharge.Invoke();
                    _removeStaminaSurcharge = null;
                }
            }

            if (!wantsBoost) return;
            __result *= Plugin._CrouchRunSpeedMultiplier?.Value ?? 1f;
        }
    }
}
