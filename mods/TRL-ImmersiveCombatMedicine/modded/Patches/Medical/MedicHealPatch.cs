using HarmonyLib;
using EFT;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using BepInEx.Logging;
using System;
using System.Reflection;

namespace Band_Aid
{
    /// <summary>
    /// Harmony Prefix em Player.MedsController.ObservedMedsControllerClass.method_5
    /// Redireciona DoMedEffect para o paciente e faz bridge do EffectRemovedEvent.
    /// </summary>
    [HarmonyPatch]
    public static class MedicHealPatch
    {
        private static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("BandAid_Patch");

        public static bool IsRedirectingHeal = false;
        public static Player CurrentPatient = null;

        // Timestamp para diagnóstico de timing
        public static float RedirectStartTime = 0f;

        // Flag para bloquear method_5 "fantasma" após cleanup
        // Fica true durante toda a operação de cura, incluindo a transição
        public static bool BandAidHealActive = false;

        private static MethodInfo _method8Cached = null;
        private static MethodInfo _method9Cached = null;
        private static object _currentObservedMedsControllerClass = null;
        private static IHealthController _subscribedPatientHc = null;

        // Fix Salewa (sessão 2026-07-12): quando o DoMedEffect redirecionado ao paciente
        // retorna não-null, o MedEffect NATIVO já cura HP ao longo do tempo, remove
        // bleeds/fraturas no Residue() e consome HpResource do item. O HealRoutine
        // consulta esta flag para NÃO aplicar MedicalLogic.ApplyTreatment em dobro.
        public static bool NativeMedEffectApplied = false;

        // ref: CR-03 — última parte aplicada pelo redirect (p/ toast de conclusão)
        public static EBodyPart LastAppliedPart = EBodyPart.Common;

        // Cache de reflection (csharp-best-practices §3): resolvido 1x por tipo.
        // Nomes reais no SPT 4.0 são PascalCase (públicos: MedsController_0/Queue_0/Float_0,
        // Player.cs:19444/19453/19456); camelCase mantido como fallback + resolução por
        // TIPO como rede final (rename-proof entre builds do EFT).
        private static Type _fieldsOwnerType;
        private static FieldInfo _fiMedsController, _fiQueue, _fiFloat;

        private static void EnsureFieldCache(Type owner)
        {
            if (_fieldsOwnerType == owner) return;
            _fieldsOwnerType = owner;
            _fiMedsController = ResolveField(owner, "MedsController_0", "medsController_0", typeof(Player.MedsController));
            _fiQueue = ResolveField(owner, "Queue_0", "queue_0", typeof(System.Collections.Generic.Queue<EBodyPart>));
            _fiFloat = ResolveField(owner, "Float_0", "float_0", typeof(float));
        }

        private static FieldInfo ResolveField(Type owner, string pascalName, string camelName, Type fieldType)
        {
            var fi = AccessTools.Field(owner, pascalName) ?? AccessTools.Field(owner, camelName);
            if (fi == null)
            {
                foreach (var f in owner.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (f.FieldType == fieldType) { fi = f; break; }
                }
            }
            return fi;
        }

        private static readonly EBodyPart[] AllBodyParts = new EBodyPart[]
        {
            EBodyPart.Head, EBodyPart.Chest, EBodyPart.Stomach,
            EBodyPart.LeftArm, EBodyPart.RightArm,
            EBodyPart.LeftLeg, EBodyPart.RightLeg
        };

        static MethodBase TargetMethod()
        {
            Type class1172 = AccessTools.Inner(
                AccessTools.Inner(typeof(Player), "MedsController"),
                "ObservedMedsControllerClass"
            );
            if (class1172 == null) { Logger.LogError("Não encontrou ObservedMedsControllerClass!"); return null; }

            var method = AccessTools.Method(class1172, "method_5");
            if (method == null) { Logger.LogError("Não encontrou method_5!"); return null; }

            _method8Cached = AccessTools.Method(class1172, "method_8");
            _method9Cached = AccessTools.Method(class1172, "method_9");
            Logger.LogInfo($"MedicHealPatch alvo: {method.DeclaringType.FullName}.{method.Name}");
            return method;
        }

        private static void OnPatientEffectRemoved(IEffect effect)
        {
            // ref: CR-01-08 — o method_8 vanilla reage a GInterface376 (marcador do
            // MedEffect em progresso, Player.cs:19617); GInterface350 é marcador de
            // efeito tipo painkiller (só Berserk implementa) e mantinha o bridge inerte.
            if (!(effect is GInterface376))
                return;

            try
            {
                CleanupPatientSubscription();
                if (_currentObservedMedsControllerClass != null && _method8Cached != null)
                {
                    // G9: Bridge só executa se temos instância válida
                    Logger.LogInfo("Bridge: MedEffect do paciente terminou → notificando method_8");
                    _method8Cached.Invoke(_currentObservedMedsControllerClass, new object[] { effect });
                }
                else
                {
                    Logger.LogWarning("Bridge: EffectRemoved mas _currentObservedMedsControllerClass ou _method8Cached é null — ignorando");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Erro no bridge EffectRemoved: {ex.Message}");
            }
        }

        /// <summary>
        /// ref: CR-02 — cancela o MedEffect NATIVO criado no PACIENTE pelo redirect.
        /// Os aborts cancelavam só o controller do MÉDICO (vazio no redirect) — o
        /// paciente continuava curando e o item sendo consumido DEPOIS do "Abortado!".
        /// Paciente local apenas (remoto não tem MedEffect nativo).
        /// </summary>
        // ref: CR-03-02 — referência do efeito criado PELO redirect, para cancel
        // direcionado (CancelApplyingItem é blanket: força-residua TODOS os MedEffects
        // do paciente em Common — inclusive a automedicação do próprio bot e efeitos
        // remanescentes de heals anteriores bem-sucedidos).
        private static IEffect _currentPatientEffect;
        private static MethodInfo _forceResidueCached;

        /// <summary>
        /// ref: CR-04-14 — limpeza do rastreamento no SUCESSO da cura: sem isso a
        /// referência estática ao MedEffect (e ao AHC do paciente) atravessava raids
        /// e um abort futuro poderia força-residuar efeito de cura ANTIGA já concluída.
        /// </summary>
        public static void ClearNativeEffectTracking()
        {
            _currentPatientEffect = null;
            NativeMedEffectApplied = false;
        }

        public static void CancelNativePatientEffect()
        {
            if (!NativeMedEffectApplied) return;
            try
            {
                if (_currentPatientEffect != null)
                {
                    if (_forceResidueCached == null)
                        _forceResidueCached = AccessTools.Method(_currentPatientEffect.GetType(), "ForceResidue");
                    _forceResidueCached.Invoke(_currentPatientEffect, null);
                    Logger.LogInfo("CancelNativePatientEffect: MedEffect DO redirect força-residuado (cancel direcionado).");
                }
                else
                {
                    // Fallback: referência perdida — cancel blanket (comportamento antigo)
                    CurrentPatient?.ActiveHealthController?.CancelApplyingItem();
                    Logger.LogWarning("CancelNativePatientEffect: sem referência do efeito — CancelApplyingItem blanket usado como fallback.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"CancelNativePatientEffect: {ex.Message}");
            }
            _currentPatientEffect = null;
            NativeMedEffectApplied = false;
        }

        /// <summary>
        /// true se a instância é a operação de meds que ESTE redirect está acompanhando.
        /// Usado pelo AnimCleanupPatch para ignorar method_9 de operações de bots/peers.
        /// </summary>
        public static bool IsCurrentInstance(object instance)
        {
            return instance != null && ReferenceEquals(instance, _currentObservedMedsControllerClass);
        }

        public static void CleanupPatientSubscription()
        {
            if (_subscribedPatientHc != null)
            {
                _subscribedPatientHc.EffectRemovedEvent -= OnPatientEffectRemoved;
                _subscribedPatientHc = null;
            }
            // NÃO limpar _currentObservedMedsControllerClass aqui — pode ser necessário para ForceFinishAnimation
        }

        /// <summary>
        /// Força finalização da animação chamando method_9 diretamente.
        /// Usado pelo HealRoutine quando não há MedEffect ativo (DoMedEffect retornou null).
        /// </summary>
        public static void ForceFinishAnimation()
        {
            if (_currentObservedMedsControllerClass != null && _method9Cached != null)
            {
                try
                {
                    Logger.LogInfo("ForceFinishAnimation: chamando method_9 diretamente");
                    _method9Cached.Invoke(_currentObservedMedsControllerClass, null);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning($"ForceFinishAnimation erro: {ex.Message}");
                }
            }
            _currentObservedMedsControllerClass = null;
            BandAidHealActive = false;
        }

        /// <summary>
        /// Resolve o Player dono da operação de meds (via MedsController_0._player).
        /// null se não conseguir resolver.
        /// </summary>
        private static Player ResolveOperationOwner(object __instance)
        {
            try
            {
                EnsureFieldCache(__instance.GetType());
                if (_fiMedsController == null) return null;
                var medsController = _fiMedsController.GetValue(__instance);
                if (medsController == null) return null;
                var playerField = AccessTools.Field(typeof(Player.ItemHandsController), "_player");
                return playerField?.GetValue(medsController) as Player;
            }
            catch { return null; }
        }

        static bool Prefix(object __instance)
        {
            // ref: fix mão travada 2026-07-12 — bots (e peers) usam o MESMO
            // ObservedMedsControllerClass para os meds DELES. Interceptar operação
            // alheia sobrescrevia _currentObservedMedsControllerClass com a instância
            // do bot → ForceFinishAnimation finalizava a operação errada e a do médico
            // ficava órfã (mão presa até o HANB). Só interceptamos o MainPlayer.
            Player operationOwner = ResolveOperationOwner(__instance);
            var localMainPlayer = Comfort.Common.Singleton<GameWorld>.Instance?.MainPlayer;
            bool isDoctorOperation = operationOwner != null && localMainPlayer != null && operationOwner == localMainPlayer;

            // ref: CR-02 — FAIL-CLOSED durante redirect: se os campos foram renomeados
            // (update do EFT) e o owner não resolve, devolver ao original durante um
            // redirect ativo executaria DoMedEffect no MÉDICO + dupla aplicação no
            // wake do HealRoutine. Bloquear e logar é o modo de falha seguro.
            if (operationOwner == null && (IsRedirectingHeal || BandAidHealActive))
            {
                Logger.LogError("method_5: ownership guard DEGRADADO (owner não resolvido — campos renomeados?) durante redirect — bloqueando operação por segurança.");
                return false;
            }

            // === DIAGNÓSTICO: SEMPRE loga quando method_5 é chamado ===
            float timeSinceRedirect = UnityEngine.Time.time - RedirectStartTime;
            Logger.LogWarning($"🔍 method_5 CHAMADO | owner={operationOwner?.Profile?.Nickname ?? "?"} | doutorOp={isDoctorOperation} | IsRedirecting={IsRedirectingHeal} | BandAidActive={BandAidHealActive} | Patient={CurrentPatient?.Profile?.Nickname ?? "null"} | T+{timeSinceRedirect:F1}s desde redirect");

            if (!isDoctorOperation)
                return true; // meds de bot/peer: nunca interceptar nem bloquear

            if (!IsRedirectingHeal || CurrentPatient == null)
            {
                // G5: Se BandAidHealActive=true, bloquear self-heal vanilla DO MÉDICO
                // para evitar que _currentObservedMedsControllerClass seja sobrescrito por outra instância
                if (BandAidHealActive)
                {
                    Logger.LogWarning("⚠️ method_5: BLOQUEADO (BandAidHealActive — protegendo _currentObservedMedsControllerClass)");
                    return false;
                }
                return true;
            }

            try
            {
                // ref: fix Salewa 2026-07-12 — nomes eram camelCase e não existem (case-sensitive)
                if (_fiMedsController == null) { Logger.LogWarning("Campo MedsController não encontrado (nome nem tipo)"); return true; }
                var medsController = _fiMedsController.GetValue(__instance);

                var doctor = operationOwner;

                var item = ((Player.AbstractHandsController)medsController).Item;
                if (item == null) { Logger.LogWarning("Item é null"); return true; }

                var queue = _fiQueue != null
                    ? (System.Collections.Generic.Queue<EBodyPart>)_fiQueue.GetValue(__instance)
                    : null;
                EBodyPart bodyPart1 = EBodyPart.Common;
                if (queue != null && queue.Count > 0)
                    bodyPart1 = queue.Peek();

                // Sem o campo de amount, passar null deixa o jogo decidir pelo item
                float? amount = _fiFloat != null ? (float?)(float)_fiFloat.GetValue(__instance) : null;

                var patientHc = CurrentPatient.ActiveHealthController;

                // === PACIENTE REMOTO: sem ActiveHealthController ===
                // ObservedCoopPlayer não tem ActiveHC → DoMedEffect impossível.
                // A animação roda normalmente, e ApplyTreatment no final envia via rede.
                if (patientHc == null)
                {
                    Logger.LogInfo($"Paciente remoto ({CurrentPatient.Profile?.Nickname}) — sem ActiveHC. Animação sem MedEffect.");
                    _currentObservedMedsControllerClass = __instance;

                    var animField = AccessTools.Field(medsController.GetType(), "firearmsAnimator_0");
                    if (animField != null)
                    {
                        var anim = animField.GetValue(medsController);
                        if (anim != null)
                        {
                            var setMult = AccessTools.Method(anim.GetType(), "SetUseTimeMultiplier");
                            setMult?.Invoke(anim, new object[] { 1f });
                        }
                    }

                    var method6Remote = AccessTools.Method(__instance.GetType(), "method_6");
                    method6Remote?.Invoke(__instance, null);

                    Logger.LogInfo("✅ Animação para paciente remoto — ForceFinishAnimation será chamado após UseTime");
                    return false;
                }

                Logger.LogInfo($"method_5: DoMedEffect no paciente {CurrentPatient.Profile.Nickname} | bodyPart={bodyPart1} | amount={(amount.HasValue ? amount.Value.ToString("F1") : "auto")} | item={item.ShortName.Localized()}");

                EBodyPart appliedPart = bodyPart1;
                IEffect result = patientHc.DoMedEffect(item, bodyPart1, amount);

                if (result == null)
                {
                    Logger.LogWarning($"DoMedEffect retornou NULL para bodyPart={bodyPart1} — tentando fallback...");
                    foreach (var bp in AllBodyParts)
                    {
                        result = patientHc.DoMedEffect(item, bp, amount);
                        if (result != null)
                        {
                            appliedPart = bp;
                            Logger.LogInfo($"✅ Cura redirecionada para {CurrentPatient.Profile.Nickname} em {bp} (fallback)");
                            break;
                        }
                    }
                }
                else
                {
                    Logger.LogInfo($"✅ Cura redirecionada para {CurrentPatient.Profile.Nickname} em {bodyPart1}");
                }

                if (result == null)
                {
                    // DoMedEffect falhou em todas as body parts.
                    // NÃO retornar true (original rodaria → DoMedEffect no médico → null → animação cancela!)
                    // Em vez disso, manter a animação tocando sem MedEffect.
                    // O tratamento real será aplicado por MedicalLogic.ApplyTreatment no final do HealRoutine.
                    Logger.LogWarning("⚠️ DoMedEffect NULL em todas body parts — mantendo animação sem MedEffect");
                    
                    // Armazenar instância para ForceFinishAnimation poder chamar method_9 depois
                    _currentObservedMedsControllerClass = __instance;

                    var animatorField2 = AccessTools.Field(medsController.GetType(), "firearmsAnimator_0");
                    if (animatorField2 != null)
                    {
                        var animator2 = animatorField2.GetValue(medsController);
                        if (animator2 != null)
                        {
                            var setMultMethod2 = AccessTools.Method(animator2.GetType(), "SetUseTimeMultiplier");
                            setMultMethod2?.Invoke(animator2, new object[] { 1f });
                        }
                    }

                    var method6Alt = AccessTools.Method(__instance.GetType(), "method_6");
                    method6Alt?.Invoke(__instance, null);

                    Logger.LogInfo("✅ Animação mantida (sem MedEffect) — ForceFinishAnimation será chamado após UseTime");
                    return false;
                }

                // MedEffect nativo criado no paciente → HealRoutine NÃO deve duplicar
                // via MedicalLogic.ApplyTreatment (cura + consumo aconteceriam 2x).
                NativeMedEffectApplied = true;
                _currentPatientEffect = result; // ref: CR-03-02 — p/ cancel direcionado

                // Feedback do membro-alvo no HUD: se aplicou em Common, o jogo escolheu
                // a parte internamente — IEffect expõe BodyPart tipado (ref: CR-03,
                // reflection era desnecessária).
                if (appliedPart == EBodyPart.Common && result != null)
                {
                    try { appliedPart = result.BodyPart; } catch { }
                }
                LastAppliedPart = appliedPart;
                BandAidUI.Instance?.ShowTreatment(appliedPart, item.ShortName?.Localized());

                // Bridge: subscrever EffectRemovedEvent do paciente
                CleanupPatientSubscription();
                _currentObservedMedsControllerClass = __instance;
                _subscribedPatientHc = CurrentPatient.HealthController;
                _subscribedPatientHc.EffectRemovedEvent += OnPatientEffectRemoved;

                // Replicar "else" do method_5 original
                float num = doctor.Skills.SurgerySpeed.Value / 100f;
                EBodyPart checkPart = bodyPart1;
                if (bodyPart1 == EBodyPart.Common && queue != null && queue.Count > 0)
                    checkPart = queue.Peek();

                try
                {
                    ValueStruct bodyPartHealth = CurrentPatient.HealthController.GetBodyPartHealth(checkPart);
                    if ((double)bodyPartHealth.Maximum - (double)bodyPartHealth.Current < 10.0)
                        num += 0.2f;
                }
                catch { }

                var animatorField = AccessTools.Field(medsController.GetType(), "firearmsAnimator_0");
                if (animatorField != null)
                {
                    var animator = animatorField.GetValue(medsController);
                    if (animator != null)
                    {
                        var setMultMethod = AccessTools.Method(animator.GetType(), "SetUseTimeMultiplier");
                        setMultMethod?.Invoke(animator, new object[] { 1f + num });
                    }
                }

                var method6 = AccessTools.Method(__instance.GetType(), "method_6");
                method6?.Invoke(__instance, null);

                Logger.LogInfo("✅ method_5 redirecionado com sucesso — animação continua");
                return false;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Erro no MedicHealPatch: {ex}");
                return true;
            }
        }
    }

    /// <summary>
    /// Postfix em ObservedMedsControllerClass.method_9 (cleanup/finalização).
    /// Reseta IsRedirectingHeal quando method_9 executa durante redirect.
    /// Também loga stack trace para diagnóstico.
    /// </summary>
    [HarmonyPatch]
    public static class AnimCleanupPatch
    {
        private static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("BandAid_SPY");

        static MethodBase TargetMethod()
        {
            Type class1172 = AccessTools.Inner(
                AccessTools.Inner(typeof(Player), "MedsController"),
                "ObservedMedsControllerClass"
            );
            return AccessTools.Method(class1172, "method_9");
        }

        [HarmonyPostfix]
        static void Postfix(object __instance)
        {
            float timeSinceRedirect = UnityEngine.Time.time - MedicHealPatch.RedirectStartTime;

            if (!MedicHealPatch.IsRedirectingHeal)
            {
                // G4: Não é nosso redirect — early return para evitar triplo reset
                return;
            }

            // ref: fix mão travada 2026-07-12 — method_9 de operação de BOT/peer
            // terminando durante o redirect não pode resetar o estado do médico.
            if (!MedicHealPatch.IsCurrentInstance(__instance))
            {
                Logger.LogInfo("AnimCleanupPatch: method_9 de outra instância (bot/peer) durante redirect — ignorado.");
                return;
            }

            // Capturar quem chamou method_9
            var stackTrace = new System.Diagnostics.StackTrace(true);
            Logger.LogWarning($"=== 🔍 method_9 (CLEANUP) durante redirect | T+{timeSinceRedirect:F1}s ===");
            for (int i = 0; i < Math.Min(stackTrace.FrameCount, 10); i++)
            {
                var frame = stackTrace.GetFrame(i);
                var method = frame.GetMethod();
                if (method != null)
                    Logger.LogWarning($"  [{i}] {method.DeclaringType?.FullName}.{method.Name}");
            }
            Logger.LogWarning($"=== FIM ===");

            // Limpar estado do redirect
            Logger.LogInfo("AnimCleanupPatch: method_9 executou → IsRedirectingHeal = false");
            MedicHealPatch.IsRedirectingHeal = false;
            MedicHealPatch.CurrentPatient = null;
            MedicHealPatch.CleanupPatientSubscription();
        }
    }
}
