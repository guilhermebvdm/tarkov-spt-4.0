using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace CameraRotationMod.Patches
{
    /// <summary>
    /// Item 018 (fix, 2026-09-09) — rastreamento robusto de "tecla de sprint pressionada" para o
    /// prone-run, independente de qual sub-estado nativo (parado/andando) esteja ativo no momento
    /// do input. Motivo: <see cref="ProneMoveStateClass.EnableSprint"/> só grava uma flag local
    /// (<c>Bool_5</c>) e NUNCA chama <c>Physical.Sprint()</c>/<c>MovementContext.EnableSprint()</c> —
    /// e <c>ProneIdleStateClass</c> (prone PARADO) nem sequer sobrescreve <c>EnableSprint</c>. Ou
    /// seja, segurar a tecla de correr parado, deitado, e só depois andar, nunca chegava a marcar a
    /// intenção — era exatamente o bug relatado ("prone não mudou nada na velocidade").
    ///
    /// Confirmado no Assembly que TODO input de sprint do jogador passa por um destes dois métodos
    /// PÚBLICOS e não-obfuscados de <c>Player</c>, não importa o sub-estado de movimento nem o modo
    /// (segurar ou alternar) configurado:
    /// - Tecla pressionada (segurar OU alternar) → <see cref="Player.ToggleSprint"/>
    ///   (ref: Assembly-CSharp/Class1728.cs:99-100, <c>ECommand.ToggleSprinting</c>).
    /// - Tecla solta (modo segurar) → <see cref="Player.EnableSprint(bool)"/> com <c>enable=false</c>
    ///   (ref: Assembly-CSharp/Class1728.cs:102-103, <c>ECommand.EndSprinting</c>).
    ///
    /// Não dá pra usar <c>Physical.Sprinting</c> pra saber o estado ANTES de alternar (como o
    /// original <c>Player.ToggleSprint</c> faz com <c>!Physical.Sprinting</c>) porque essa flag
    /// nunca vira true durante prone (pelo motivo acima) — por isso mantemos nosso PRÓPRIO flag,
    /// alternado no mesmo evento, em vez de espelhar um flag nativo que não reflete prone.
    /// </summary>
    public static class ProneRunSprintIntent
    {
        public static bool Held;

        // Fix 2026-09-09 (gap reportado pelo usuário: "acaba a stamina e o prone-run continua ligado,
        // fôlego infinito"). O prone-run não passa pelo dreno nativo de sprint (ver sobretaxa própria
        // em ProneRunMaxSpeedPatch), então também precisa da SUA PRÓPRIA checagem de exaustão — nada
        // nativo corta isso por nós. Critério de "vermelho"/exausto é o mesmo que o próprio jogo já
        // usa: PlayerPhysicalClass.Exhausted (Stamina.Current < 15f OU Oxygen.Current < 15f, ref:
        // Assembly-CSharp/PlayerPhysicalClass.cs:455-465, override de BasePhysicalClass.Exhausted).
        // Latch (não só "!Exhausted no frame seguinte"): assim que a stamina zera, força o boost fora
        // e só libera de novo quando ela sobe de volta PRA CIMA desse teto — sem isso, oscilar bem no
        // fundo do poço (14.9 vs 15.1) ligaria/desligaria o boost a cada frame.
        public static bool ExhaustedLatch;

        public static void ResetState()
        {
            Held = false;
            ExhaustedLatch = false;
        }

        /// <summary>Chamar 1x por patch por frame (idempotente) antes de decidir wantsBoost.</summary>
        public static bool UpdateExhaustionGate(BasePhysicalClass physical)
        {
            if (physical == null) return !ExhaustedLatch;

            bool exhausted;
            try
            {
                // Bug real (2026-09-09): MovementContext.Init() já chama o getter de MaxSpeed (que
                // aciona este patch) na CRIAÇÃO do player, antes de Physical.Stamina/Oxygen existirem
                // — Exhausted lê Stamina.Current/Oxygen.Current e estourava NullReferenceException
                // ali dentro, travando EFT.Player.Init() por completo (CreateLocalPlayer falhava pra
                // TODOS os cenários, inclusive solo — crash reportado pelo usuário). Qualquer exceção
                // aqui = estado interno ainda não pronto; não altera o latch, tenta de novo no próximo
                // frame (idêntico ao guard try/catch usado em todo o resto do mod).
                exhausted = physical.Exhausted;
            }
            catch
            {
                return !ExhaustedLatch;
            }

            if (exhausted) ExhaustedLatch = true;
            else if (ExhaustedLatch) ExhaustedLatch = false;
            return !ExhaustedLatch;
        }
    }

    public class PlayerToggleSprintPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.ToggleSprint));
        }

        [PatchPrefix]
        private static void Prefix(Player __instance)
        {
            if (!__instance.IsYourPlayer) return;
            ProneRunSprintIntent.Held = !ProneRunSprintIntent.Held;
        }
    }

    public class PlayerEnableSprintPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.EnableSprint), new[] { typeof(bool) });
        }

        [PatchPostfix]
        private static void Postfix(Player __instance, bool enable)
        {
            if (!__instance.IsYourPlayer) return;
            if (!enable) ProneRunSprintIntent.Held = false; // soltar a tecla (modo segurar) sempre força off
        }
    }

    // Item 018 — boost condicional de MaxSpeed em prone. Sem rampa (removida a pedido do usuário —
    // não fazia sentido no prone) e sem piso de postura (não se aplica ao prone).
    // Sobretaxa de stamina MANTIDA: vanilla não drena stamina durante sprint em prone (ver acima),
    // então sem sobretaxa o prone-run ficaria de graça.
    //
    // Fix 2026-09-09 (bug: "limite muda, velocidade real não muda"): multiplicar MaxSpeed sozinho
    // não move a agulha — CharacterMovementSpeed/ClampedSpeed são recomputados toda hora contra
    // StateSpeedLimit (default 1f, ref: Assembly-CSharp/EFT/MovementContext.cs:1801/1843-1845), que
    // nunca sobe acima da baseline (SetCharacterMovementSpeed(MaxSpeed) em Init, ref: .../
    // MovementContext.cs:1917) — então o "teto" reportado pela UI sobe mas a velocidade real fica
    // travada em ~1. A prova de que quem realmente acelera o passo é o parâmetro do Animator, não o
    // número: o mesmo padrão no crouch-run (CrouchRunMaxSpeedPatch) já chama
    // PlayerAnimatorEnableSprint(wantsBoost) e o usuário confirmou que o agachado corre mais rápido
    // de verdade — reaplicando aqui o mesmo gatilho (o método não faz nenhuma checagem de pose, ref:
    // .../MovementContext.cs:3832-3835, só repassa pro Animator).
    public class ProneRunMaxSpeedPatch : ModulePatch
    {
        private const float MovementEpsilonSqr = 0.0001f;

        private static System.Action _removeStaminaSurcharge;
        private static bool _animatorSprintOn;
        private static int _lastFrame = -1;

        public static void ResetState()
        {
            _removeStaminaSurcharge?.Invoke();
            _removeStaminaSurcharge = null;
            _animatorSprintOn = false;
            _lastFrame = -1;
        }

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(MovementContext), nameof(MovementContext.MaxSpeed));
        }

        [PatchPostfix]
        private static void Postfix(MovementContext __instance, ref float __result)
        {
            try
            {
                Player player = Traverse.Create(__instance).Field<Player>("_player").Value;
                if (player == null || !player.IsYourPlayer) return;

                bool staminaOk = ProneRunSprintIntent.UpdateExhaustionGate(player.Physical);
                bool featureOn = Plugin._EnableProneRun?.Value ?? false;
                bool wantsBoost = featureOn
                    && staminaOk
                    && __instance.IsInPronePose
                    && ProneRunSprintIntent.Held
                    && __instance.MovementDirection.sqrMagnitude > MovementEpsilonSqr;

                if (Time.frameCount != _lastFrame)
                {
                    _lastFrame = Time.frameCount;

                    if (wantsBoost != _animatorSprintOn)
                    {
                        _animatorSprintOn = wantsBoost;
                        __instance.PlayerAnimatorEnableSprint(wantsBoost);
                    }

                    if (wantsBoost && _removeStaminaSurcharge == null && player.Physical?.Stamina != null)
                    {
                        var consumption = new PlayerPhysicalClass.GClass773(PlayerPhysicalClass.EConsumptionType.Sprint)
                        {
                            PrimaryTarget = PlayerPhysicalClass.EConsumptionTarget.Base,
                            Delta = new GClass848<float>(() => Plugin._ProneRunStaminaSurcharge?.Value ?? 0f)
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
                __result *= Plugin._ProneRunSpeedMultiplier?.Value ?? 1f;
            }
            catch (System.Exception ex)
            {
                // Bug real 2026-09-09: este getter é chamado por MovementContext.Init() na criação do
                // player (antes de Physical.Stamina/Oxygen existirem) — qualquer NRE aqui derrubava
                // Player.Init() inteiro pra TODOS os cenários (inclusive solo). Nunca deixar escapar.
                Plugin.Logger.LogError($"[ProneRunMaxSpeed] Postfix falhou, caindo pro vanilla: {ex}");
            }
        }
    }

    // Item 018 (fix, 2026-09-09 — via ProneRunDebugUI): a spy provou que multiplicar MaxSpeed
    // (ProneRunMaxSpeedPatch acima) NUNCA moveu a agulha de verdade. Capturas ao vivo, boost
    // desligado → ligado: MaxSpeed 0.565 → 1.131 (multiplicador aplicando certinho), mas
    // SmoothedCharacterMovementSpeed E o parâmetro "Speed" do Animator ficaram EXATAMENTE iguais
    // (0.392 → 0.392) nas duas capturas, e Bool_5(nativo)=False nas duas — ou seja, a rampa nativa
    // de ProneMoveStateClass.ManualAnimatorMoveUpdate (ref: Assembly-CSharp/ProneMoveStateClass.cs:
    // 88-105) nunca chega a rodar, porque só liga via Bool_5, que só o método nativo
    // EnableSprint(bool) grava — e nosso fix do bug de detecção (PlayerToggleSprintPatch/
    // PlayerEnableSprintPatch acima) deliberadamente contorna esse caminho.
    //
    // Causa raiz de por que multiplicar MaxSpeed é insuficiente mesmo se Bool_5 estivesse ligado:
    // ClampedSpeed (o valor que RunStateClass.method_1 usa pra alimentar SmoothedCharacterMovementSpeed,
    // ref: RunStateClass.cs:318-320) é sempre Clamp(CharacterMovementSpeed, 0, StateSpeedLimit) — e
    // StateSpeedLimit nesta captura estava em 0.543 (não o default 1f — outra causa de slowdown já
    // ativa, ex.: Stance/peso), um teto que NÃO sobe com MaxSpeed (ref: MovementContext.cs:1843-1845).
    // Ou seja: mesmo com a rampa nativa ligada, ela nunca ultrapassaria ~0.543 — abaixo até da
    // baseline sem boost em cenários com StateSpeedLimit mais alto.
    //
    // Fix real: replicar o padrão de MovementContext.PreSprintAcceleration (ref: MovementContext.cs:
    // 2526-2542) — o mesmo canal que faz o sprint em pé/agachado acelerar de verdade — escrevendo
    // DIRETO em SmoothedCharacterMovementSpeed/CharacterMovementSpeed (que ignora StateSpeedLimit
    // completamente) em vez de depender de ClampedSpeed. Alvo é o multiplicador configurado (não o
    // 1f fixo que o nativo usa), porque é exatamente esse valor > 1 que dá o "mais rápido que o
    // normal" que a feature promete.
    public class ProneRunSpeedDriverPatch : ModulePatch
    {
        private const float MovementEpsilonSqr = 0.0001f;

        // Fix 2026-09-09 (rodada 2, mesma sessão de spy): a rodada anterior deste patch PROVOU
        // (via ProneRunDebugUI) que consegue escrever de verdade em SmoothedCharacterMovementSpeed/
        // Animator "Speed" (0.392 → 1.279 numa captura ao vivo), mas o usuário confirmou que o
        // rastejo continuou visualmente igual — ou seja, esse parâmetro de BLEND não controla a
        // velocidade real do movimento no chão em prone (a EFT usa Root Motion puro pra prone: o
        // deslocamento por frame é literalmente o delta do clipe de animação tocando, aplicado direto
        // em CharacterController.Move, ref: MovementContext.cs:2025-2054/1384-1394 — "Speed" só
        // seleciona ONDE na blend tree, sem escalar quão rápido o clipe toca).
        //
        // Quem escala a VELOCIDADE de reprodução de um clipe já em execução no Mecanim é
        // Animator.speed (multiplicador global de playback), não um parâmetro de blend tree — e a
        // própria EFT já expõe um wrapper pronto pra isso, nunca usado por nenhum código nativo no
        // Assembly (nenhum caller encontrado): MovementContext.PlayerAnimatorSetSprintToIdleSpeed
        // (ref: MovementContext.cs:3837-3840, literalmente `PlayerAnimator_1.Animator.speed = value`).
        // Root Motion escala proporcionalmente com Animator.speed (comportamento padrão do Mecanim) —
        // esse é o lever que deveria de fato acelerar o deslocamento físico do rastejo.
        private static bool _speedOverrideActive;
        private static bool _wasBoosting;

        // Fix 2026-09-09 (multiplayer): Animator.speed é 100% client-local — nem MovementInfoPacketStruct
        // nem o PlayerStateData compacto do Fika carregam esse valor (o Fika ainda por cima empacota
        // SmoothedCharacterMovementSpeed num ushort 0..1, cortando qualquer boost >1 antes de sair pela
        // rede). Sem sincronizar isso à parte, outros peers veem a POSIÇÃO avançar no ritmo boostado
        // (essa parte já replica certo) mas as pernas animando no ritmo normal — ver
        // Networking/ProneRunSpeedSyncPacket.cs. Throttle aqui (não em FikaSyncManager) pra não mandar
        // pacote todo frame: só em mudança perceptível ou keepalive periódico (auto-cura de perda de
        // pacote, já que o envio usa ReliableUnordered mas Unordered pode reordenar/atrasar).
        private const float SyncEpsilon = 0.03f;
        private const float SyncKeepaliveSeconds = 1f;
        private static float _lastSyncedSpeed = float.NaN;
        private static float _lastSyncTime = float.NegativeInfinity;

        public static void ResetState()
        {
            _speedOverrideActive = false;
            _wasBoosting = false;
            _lastSyncedSpeed = float.NaN;
            _lastSyncTime = float.NegativeInfinity;
        }

        private static void SyncAnimatorSpeed(Player player, float animatorSpeed)
        {
            bool changed = float.IsNaN(_lastSyncedSpeed) || Mathf.Abs(animatorSpeed - _lastSyncedSpeed) > SyncEpsilon;
            bool keepalive = Time.unscaledTime - _lastSyncTime > SyncKeepaliveSeconds;
            if (!changed && !keepalive) return;

            _lastSyncedSpeed = animatorSpeed;
            _lastSyncTime = Time.unscaledTime;
            Networking.FikaSyncManager.SendProneRunSpeed(player.ProfileId, animatorSpeed);
        }

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ProneMoveStateClass), nameof(ProneMoveStateClass.ManualAnimatorMoveUpdate), new[] { typeof(float) });
        }

        [PatchPostfix]
        private static void Postfix(ProneMoveStateClass __instance, float deltaTime)
        {
            try
            {
                MovementContext mc = __instance.MovementContext;
                Player player = Traverse.Create(mc).Field<Player>("_player").Value;
                if (player == null || !player.IsYourPlayer) return;

                bool featureOn = Plugin._EnableProneRun?.Value ?? false;
                bool moving = mc.IsInPronePose && mc.MovementDirection.sqrMagnitude > MovementEpsilonSqr;

                if (!featureOn || !moving)
                {
                    if (_speedOverrideActive)
                    {
                        _speedOverrideActive = false;
                        mc.PlayerAnimatorSetSprintToIdleSpeed(1f); // devolve o playback do Animator ao normal
                        SyncAnimatorSpeed(player, 1f); // avisa os peers que voltou ao normal (fora do throttle de mudança)
                    }
                    return;
                }

                bool staminaOk = ProneRunSprintIntent.UpdateExhaustionGate(player.Physical);
                bool wantsBoost = staminaOk && ProneRunSprintIntent.Held;

                // Pedido do usuário (2026-09-09): usar o mesmo lever (Animator.speed) pra destravar a
                // "barrinha" nativa de ajuste de velocidade (scroll wheel → Player.ChangeSpeed →
                // MovementState.ChangeSpeed base, ref: Assembly-CSharp/Class1728.cs:69-74/EFT/
                // MovementState.cs:248-252) em prone — hoje ela já mexe em CharacterMovementSpeed
                // normalmente (nenhum override bloqueia), só nunca tinha efeito visível pelo mesmo motivo
                // do bug do boost (Animator.speed nunca era tocado). Sem boost ativo, espelhamos o valor
                // que o scroll wheel (ou o baseline nativo) já deixou em CharacterMovementSpeed — permite
                // reduzir pra andar bem devagar (ex.: 0.5, "atirador se reposicionando") e volta a 1x
                // sozinho quando o jogador scrolla de volta ao padrão.
                float animatorSpeed;
                if (wantsBoost)
                {
                    float target = Mathf.Max(0f, Plugin._ProneRunSpeedMultiplier?.Value ?? 1f);
                    float rate = player.Physical?.PreSprintAcceleration ?? 1f; // mesma cadência do sprint nativo em pé/agachado
                    float next = Mathf.MoveTowards(mc.SmoothedCharacterMovementSpeed, target, rate * deltaTime);

                    mc.SmoothedCharacterMovementSpeed = next; // alimenta o Animator direto (ref: MovementContext.cs:794-808)
                    mc.CharacterMovementSpeed = next;          // mantém em sincronia p/ replicação (MovementInfoPacketStruct)
                    mc.RaiseChangeSpeedEvent();
                    animatorSpeed = next;
                    _wasBoosting = true;
                }
                else
                {
                    // Bug real 2026-09-09 (nº3): ao soltar o boost, CharacterMovementSpeed ficava
                    // preso no valor absoluto que a rampa alcançou (podendo passar de MaxSpeed, ex.:
                    // ~2.0) — nada o trazia de volta. Combinado com a normalização por ratio (nº2
                    // abaixo), isso faria o cálculo `CharacterMovementSpeed / MaxSpeed` disparar bem
                    // acima de 1 logo após soltar o boost, ANTES de qualquer scroll. Reset explícito
                    // pra baseline (MaxSpeed) na borda de descida, 1x só.
                    if (_wasBoosting)
                    {
                        _wasBoosting = false;
                        float baselineReset = mc.MaxSpeed;
                        mc.SmoothedCharacterMovementSpeed = baselineReset;
                        mc.CharacterMovementSpeed = baselineReset;
                        mc.RaiseChangeSpeedEvent();
                    }

                    // Sem boost (tecla solta OU stamina no vermelho): não mexe em CharacterMovementSpeed —
                    // é o valor que o scroll wheel nativo (ou o baseline) já definiu.
                    //
                    // Bug real 2026-09-09 (nº1): Animator.speed é o multiplicador GLOBAL de playback do
                    // Animator inteiro (ref: MovementContext.cs:3837-3840) — não só a perna/locomoção.
                    // O scroll wheel nativo (Player.ChangeSpeed) não tem piso nenhum e deixa
                    // CharacterMovementSpeed cair perto de 0 (ref: Class1728.cs:69-74 — decrementa
                    // DELTA_SPEED por tick, sem clamp mínimo prático). Espelhar isso direto em
                    // Animator.speed sem piso derrubava TODAS as animações do personagem em câmera
                    // lenta (levantar, mirar, etc.).
                    //
                    // Bug real 2026-09-09 (nº2, achado DEPOIS do piso acima): usar CharacterMovementSpeed
                    // BRUTO (valor absoluto) como Animator.speed também deixava o rastejo BASE (sem
                    // sprint, sem scroll) mais LENTO que o vanilla — porque a baseline nativa de
                    // CharacterMovementSpeed (= MaxSpeed no Init, ref: MovementContext.cs:1917) não é
                    // 1.0, é o que quer que Evaluate(WalkSpeed, Strength/60) resulte pra esse perfil
                    // (ex.: ~0.565 numa captura real do spy) — e o vanilla NUNCA toca em Animator.speed
                    // pra prone, então o rastejo normal sempre toca a 1.0 (native), não em
                    // "CharacterMovementSpeed cru". Fix: normalizar pela PRÓPRIA baseline
                    // (CharacterMovementSpeed / MaxSpeed) — dá exatamente 1.0 quando o jogador não
                    // mexeu no scroll (CharacterMovementSpeed == MaxSpeed por padrão), e escala pra
                    // baixo corretamente quando ele reduz.
                    float baseline = mc.MaxSpeed;
                    float ratio = baseline > 0.0001f ? mc.CharacterMovementSpeed / baseline : 1f;
                    float floor = Mathf.Clamp01(Plugin._ProneRunMinSpeedFloor?.Value ?? 0.4f);
                    animatorSpeed = Mathf.Max(floor, ratio);
                }

                _speedOverrideActive = true;
                mc.PlayerAnimatorSetSprintToIdleSpeed(animatorSpeed); // escala o playback do clipe (e o Root Motion junto)
                SyncAnimatorSpeed(player, animatorSpeed); // replica pros peers Fika — ver comentário da classe
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogError($"[ProneRunSpeedDriver] Postfix falhou, caindo pro vanilla: {ex}");
            }
        }

        /// <summary>Reset incondicional — usado pelo Exit patch abaixo, sem depender de _speedOverrideActive
        /// (mais seguro: garante que uma stance/pose nova nunca herde um Animator.speed sujo).</summary>
        internal static void ForceReset(Player player, MovementContext mc)
        {
            _speedOverrideActive = false;
            _wasBoosting = false; // idem — não deixa a próxima entrada em prone herdar um "acabei de soltar boost"
            mc.PlayerAnimatorSetSprintToIdleSpeed(1f);
            if (player != null) SyncAnimatorSpeed(player, 1f);
        }
    }

    // Bug real 2026-09-09 (reportado pelo usuário com print do spy): reduzir a velocidade em prone e
    // DEPOIS LEVANTAR deixava o personagem em câmera lenta permanente, mesmo já em pé/andando
    // normalmente (confirmado: Animator.speed = 0.400, preso no piso, com CurrentManagedState já
    // IdleStateClass). Causa: o reset de ProneRunSpeedDriverPatch só roda dentro do PRÓPRIO Postfix de
    // ProneMoveStateClass.ManualAnimatorMoveUpdate — método que só é chamado enquanto
    // CurrentManagedState AINDA é ProneMoveStateClass. Ao sair do prone de vez (levantar), esse método
    // para de rodar e o Animator.speed reduzido fica preso pra sempre — nada mais no jogo nunca toca
    // nesse valor (confirmado: PlayerAnimatorSetSprintToIdleSpeed não tem NENHUM outro caller nativo).
    //
    // Fix: Postfix em ProneMoveStateClass.Exit(bool) — dispara sempre que se sai do estado de prone EM
    // MOVIMENTO, pra QUALQUER estado seguinte (parado-prone, levantando, etc.), garantindo que o
    // Animator.speed nunca escape do prone com um valor != 1. Reset incondicional (não só quando
    // _speedOverrideActive) — mais barato que arriscar mais um caminho de escape não mapeado.
    //
    // Bug real 2026-09-09 (reportado pelo usuário): usar prone-run, levantar (ainda segurando/com o
    // sprint "alternado" ligado) e voltar pro prone reativava o boost sozinho, sem apertar nada de
    // novo — com um blip de animação de "acabou de soltar sprint" na hora que reentrava no prone.
    // Causa: ProneRunSprintIntent.Held (ref: PlayerToggleSprintPatch/PlayerEnableSprintPatch, mais
    // acima neste arquivo) só é limpo quando a tecla física é solta/alternada de novo — nunca ao SAIR
    // do prone. Se o jogador levanta com o sprint ainda fisicamente "ligado" (segurado, ou alternado
    // sem soltar), Held ficava true através da mudança de postura inteira, e a próxima entrada em
    // prone via wantsBoost=true direto no primeiro frame, sem uma nova intenção de tecla.
    //
    // Fix: zera Held explicitamente ao sair do prone-móvel — força uma tecla NOVA (aperto ou alternar)
    // depois de qualquer saída do prone antes do boost poder reengajar, em vez de herdar o estado
    // físico da tecla através da transição de postura.
    public class ProneMoveExitResetPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ProneMoveStateClass), nameof(ProneMoveStateClass.Exit), new[] { typeof(bool) });
        }

        [PatchPostfix]
        private static void Postfix(ProneMoveStateClass __instance)
        {
            try
            {
                MovementContext mc = __instance.MovementContext;
                Player player = Traverse.Create(mc).Field<Player>("_player").Value;
                if (player == null || !player.IsYourPlayer) return;

                ProneRunSpeedDriverPatch.ForceReset(player, mc);
                ProneRunSprintIntent.Held = false;
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogError($"[ProneMoveExitReset] Postfix falhou: {ex}");
            }
        }
    }
}
