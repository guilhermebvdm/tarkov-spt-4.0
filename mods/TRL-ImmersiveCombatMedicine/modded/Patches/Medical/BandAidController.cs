using BepInEx;
using Comfort.Common;
using EFT;
using EFT.Communications;
using EFT.InventoryLogic;
using EFT.HealthSystem;
using EFT.UI;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Configuration;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TRLImmersiveCombatMedicine;
using UnityEngine.UI;
using Band_Aid;

namespace TRLImmersiveCombatMedicine
{
    public class BandAidController : MonoBehaviour
    {
        public static BandAidController Instance;
        private bool _isMedicModeActive = false;
        private Player _targetPatient = null;

        // Controle da Animação
        private bool _isHealingInProgress = false;
        private Item _itemBeingUsed = null;
        private Coroutine _activeHealCoroutine = null;

        // N1: Detectar mudança de raid para resetar flags estáticas
        private GameWorld _lastGameWorld = null;

        // Harmony

        // Config (agora lidas do plugin principal)
        public static ConfigEntry<KeyboardShortcut> _medicInteractKey => TRLImmersiveCombatMedicinePlugin.MedicInteractKey;
        public static ConfigEntry<KeyboardShortcut> _emergencyDropKey => TRLImmersiveCombatMedicinePlugin.EmergencyDropKey;
        public static ConfigEntry<EBandAidPressMode> _medicInteractMode => TRLImmersiveCombatMedicinePlugin.MedicInteractMode;
        public static ConfigEntry<EBandAidPressMode> _emergencyDropMode => TRLImmersiveCombatMedicinePlugin.EmergencyDropMode;

        // Detecção de Hold
        private float _medicHoldTimer = 0f;
        private bool _medicHoldTriggered = false;
        private float _emergencyHoldTimer = 0f;
        private bool _emergencyHoldTriggered = false;
        private const float HOLD_THRESHOLD = 0.4f; // segundos para Hold

        // Detecção de DoubleTap
        private float _medicLastTapTime = -1f;
        private float _emergencyLastTapTime = -1f;
        private const float DOUBLE_TAP_WINDOW = 0.35f; // janela para double tap

        private void Awake()
        {
            Instance = this;

            // Registrar handler de resposta do handshake
            BandAidNetworkHandler.OnHealCheckResponse += OnHealCheckResponseHandler;
        }

        private void OnDestroy()
        {
            BandAidNetworkHandler.OnHealCheckResponse -= OnHealCheckResponseHandler;
        }

        private void OnHealCheckResponseHandler(BandAidHealCheckResponsePacketV2 response)
        {
            // Verificar se temos um check pendente
            if (_pendingHealTimeout < 0 || _pendingHealItem == null || _pendingHealPatient == null) return;

            // ref: G-5 (coop-heal-matrix) — resposta stale de um check anterior não
            // pode aprovar item/paciente errados: conferir com o estado pendente.
            if (!string.IsNullOrEmpty(response.ItemTemplateId) &&
                response.ItemTemplateId != _pendingHealItem.TemplateId.ToString())
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning(
                    $"Handshake: resposta para item {response.ItemTemplateId} ≠ pendente {_pendingHealItem.TemplateId} — ignorada.");
                return;
            }
            if (!string.IsNullOrEmpty(response.PatientProfileId) &&
                response.PatientProfileId != _pendingHealPatient.ProfileId)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning(
                    $"Handshake: resposta de paciente {response.PatientProfileId} ≠ pendente {_pendingHealPatient.ProfileId} — ignorada.");
                return;
            }

            // Limpar timeout
            _pendingHealTimeout = -1f;

            if (response.Approved)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"Handshake aprovado! Iniciando cura com {_pendingHealStats?.Name}.");
                var mainPlayer = Singleton<GameWorld>.Instance?.MainPlayer;
                if (mainPlayer != null && _pendingHealItem != null && _pendingHealStats != null)
                {
                    // ref: CR-04-15 — só setar quando a HealRoutine de fato inicia
                    // (setado fora do guard ficava stale para a cura seguinte)
                    _expectedTreatmentPart = (EBodyPart)response.ExpectedBodyPart;
                    _activeHealCoroutine = StartCoroutine(
                        HealRoutine(mainPlayer, _pendingHealPatient, _pendingHealItem, _pendingHealStats));
                }
            }
            else
            {
                // ref: item 010 — tradução acontece AQUI, no médico (exibidor): cada peer vê no
                // PRÓPRIO idioma, independente do idioma de quem gerou a recusa (wire format
                // carrega só o ID, nunca texto).
                string denyText = MedicLocale.GetDenyReasonText(response.DenyReasonId, response.ItemTemplateId);
                NotificationManagerClass.DisplayMessageNotification(
                    denyText, ENotificationDurationType.Default, ENotificationIconType.Alert);
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"Handshake negado: {denyText}");
            }

            // Limpar estado pendente
            _pendingHealItem = null;
            _pendingHealStats = null;
            _pendingHealPatient = null;
        }

        private void Update()
        {
            // O registro de pacotes deve ocorrer independentemente de haver um jogador local.
            // Em servidores dedicados (Headless), o MainPlayer é null. Se pularmos, os pacotes nunca são registrados.
            try { BandAidNetworkHandler.CheckInit(); }
            catch (Exception ex)
            {
                // Guard: uma exceção aqui mataria o Update inteiro a cada frame.
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogError($"CheckInit exception: {ex}");
            }

            if (Singleton<GameWorld>.Instance == null || Singleton<GameWorld>.Instance.MainPlayer == null)
            {
                // N1: Se GameWorld foi destruído, resetar tudo
                if (_lastGameWorld != null)
                {
                    ResetAllState();
                    _lastGameWorld = null;
                }
                return;
            }

            // N1: Detectar mudança de raid (novo GameWorld)
            if (_lastGameWorld != Singleton<GameWorld>.Instance)
            {
                ResetAllState();
                _lastGameWorld = Singleton<GameWorld>.Instance;
            }

            // ref: CR-05 — consumo pendente sem report dentro do timeout → fallback
            MedicalLogic.TickPendingConsumes();

            // Interação via pipeline NATIVO: garante um MedicInteractable em cada
            // player/bot vivo; o prompt aparece no ActionPanel do jogo (como loot).
            EnsureMedicInteractables();

            // Regra ÚNICA de distância (config): o controller dirige o ActionPanel
            // nativo com scan próprio — prompt e F usam a mesma detecção, sem os caps
            // do raycast vanilla (1,3m/2,5m) e sem flicker de evento.
            UpdateNativePrompt();

            // Debug: reconcilia o toggle "Invisível para Bots" (rate-limit interno)
            DebugBotInvisibility.Tick();

            // === EMERGENCY DROP ===
            if (_isHealingInProgress && CheckPressMode(_emergencyDropKey.Value, _emergencyDropMode.Value,
                ref _emergencyHoldTimer, ref _emergencyHoldTriggered, ref _emergencyLastTapTime))
            {
                EmergencyDrop();
                return;
            }

            // Item 7: Botão esquerdo do mouse cancela cura em andamento (sem dropar item)
            if (_isHealingInProgress && Input.GetMouseButtonDown(0))
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning("Mouse0 pressionado durante cura — cancelando tratamento (sem drop).");
                CancelHealInProgress();
                return;
            }

            // === MEDIC INTERACT (só FECHAR — a abertura vem do ActionPanel nativo) ===
            if (_isMedicModeActive && CheckPressMode(_medicInteractKey.Value, _medicInteractMode.Value,
                ref _medicHoldTimer, ref _medicHoldTriggered, ref _medicLastTapTime))
            {
                DeactivateMedicMode();
            }

            if (_isMedicModeActive && _targetPatient != null)
            {
                // Regra única: mesma distância do prompt (config) + 1 m de margem
                // feet-to-feet — o modo não fecha sozinho onde o prompt ainda aparece.
                if (Vector3.Distance(Singleton<GameWorld>.Instance.MainPlayer.Position, _targetPatient.Position) >
                    TRLImmersiveCombatMedicinePlugin.MedicInteractDistance.Value + 1f)
                {
                    // ref: CR-01-11 — NÃO zerar _isHealingInProgress aqui: o cleanup
                    // completo do DeactivateMedicMode (StopCoroutine, CancelApplyingItem,
                    // ForceFinishAnimation, UsingMeds=false) é gated por essa flag.
                    // Zerá-la antes deixava a HealRoutine viva aplicando o tratamento
                    // a qualquer distância depois do "Abortado!".
                    if (_isHealingInProgress)
                        NotificationManagerClass.DisplayMessageNotification(MedicLocale.Get(MedicTextId.Aborted), ENotificationDurationType.Default, ENotificationIconType.Alert);
                    DeactivateMedicMode();
                    return;
                }

                if (!_isHealingInProgress) CheckManualInputs();
            }

            // ref: CR-01-10 — timeout do handshake avaliado SEMPRE (fora do gate do
            // modo médico): fechar o modo com check pendente não pode deixar o estado
            // vivo para uma resposta tardia iniciar cura fora de contexto.
            if (_pendingHealTimeout > 0 && Time.time > _pendingHealTimeout)
            {
                _pendingHealTimeout = -1f;
                _pendingHealItem = null;
                _pendingHealStats = null;
                _pendingHealPatient = null;
                NotificationManagerClass.DisplayMessageNotification(
                    MedicLocale.Get(MedicTextId.NoPatientResponseTimeout), ENotificationDurationType.Default, ENotificationIconType.Alert);
            }
        }

        // Mapeamento EGameKey.Slot → EBoundItem
        private static readonly Dictionary<EFT.InputSystem.EGameKey, EBoundItem> _slotMapping = new Dictionary<EFT.InputSystem.EGameKey, EBoundItem>
        {
            { EFT.InputSystem.EGameKey.Slot4, EBoundItem.Item4 },
            { EFT.InputSystem.EGameKey.Slot5, EBoundItem.Item5 },
            { EFT.InputSystem.EGameKey.Slot6, EBoundItem.Item6 },
            { EFT.InputSystem.EGameKey.Slot7, EBoundItem.Item7 },
            { EFT.InputSystem.EGameKey.Slot8, EBoundItem.Item8 },
            { EFT.InputSystem.EGameKey.Slot9, EBoundItem.Item9 },
            { EFT.InputSystem.EGameKey.Slot0, EBoundItem.Item10 },
        };

        private void CheckManualInputs()
        {
            if (_targetPatient == null || !_targetPatient.HealthController.IsAlive) return;
            try
            {
                var settings = Singleton<SharedGameSettingsClass>.Instance?.Control?.Settings;
                if (settings == null) return;

                var keyBindings = settings.UserKeyBindings?.Value;
                if (keyBindings == null) return;

                // Match-mais-longo entre TODOS os slots: com "Mouse4" num slot e
                // "Shift+Mouse4" em outro, pressionar Shift+Mouse4 dispara os dois
                // testes — vence o combo com MAIS teclas (o mais específico).
                // (Problema herdado do 3.11: o primeiro match vencia e o slot de
                // Mouse4 puro "roubava" o Shift+Mouse4.)
                EBoundItem bestSlot = default;
                int bestKeyCount = 0;

                foreach (var keyGroup in keyBindings)
                {
                    if (!_slotMapping.ContainsKey(keyGroup.keyName)) continue;

                    int keyCount = TriggeredVariantKeyCount(keyGroup);
                    if (keyCount > bestKeyCount)
                    {
                        bestKeyCount = keyCount;
                        bestSlot = _slotMapping[keyGroup.keyName];
                    }
                }

                if (bestKeyCount > 0)
                    ProcessHeal(bestSlot);
            }
            catch (Exception ex)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning($"CheckManualInputs: {ex.Message}");
            }
        }

        /// <summary>
        /// Retorna o tamanho (nº de teclas) do maior variant do grupo acionado neste
        /// frame — 0 se nenhum. Variant acionado = TODAS as teclas seguradas (GetKey)
        /// e pelo menos UMA pressionada NESTE frame (GetKeyDown).
        /// </summary>
        private int TriggeredVariantKeyCount(EFT.InputSystem.KeyGroup keyGroup)
        {
            int best = 0;
            foreach (var variant in keyGroup.variants)
            {
                if (variant.keyCode == null || variant.keyCode.Count == 0) continue;
                if (variant.keyCode.Count <= best) continue;

                bool allHeld = true;
                bool anyJustPressed = false;

                foreach (var key in variant.keyCode)
                {
                    if (!Input.GetKey(key))
                    {
                        allHeld = false;
                        break;
                    }
                    if (Input.GetKeyDown(key))
                    {
                        anyJustPressed = true;
                    }
                }

                if (allHeld && anyJustPressed)
                    best = variant.keyCode.Count;
            }
            return best;
        }

        private void ProcessHeal(EBoundItem slot)
        {
            var mainPlayer = Singleton<GameWorld>.Instance.MainPlayer;
            if (mainPlayer.Inventory.FastAccess.BoundItems.TryGetValue(slot, out Item item))
            {
                ItemStats stats = ItemDatabase.GetStats(item.TemplateId.ToString());

                bool isRemotePatient = !((_targetPatient.HealthController) is ActiveHealthController);

                if (isRemotePatient)
                {
                    // === PACIENTE REMOTO: Handshake via rede ===
                    // Step 1: enviar check ao paciente, guardar dados pendentes
                    _pendingHealSlot = slot;
                    _pendingHealItem = item;
                    _pendingHealStats = stats;
                    _pendingHealPatient = _targetPatient;
                    _pendingHealTimeout = Time.time + 3f; // timeout 3s

                    BandAidNetworkHandler.SendHealCheck(mainPlayer, _targetPatient, item.TemplateId.ToString());
                    NotificationManagerClass.DisplayMessageNotification(
                        MedicLocale.Get(MedicTextId.CheckingItem, stats.Name), ENotificationDurationType.Default, ENotificationIconType.Quest);
                }
                else
                {
                    // === PACIENTE LOCAL (bot/self): Validação imediata ===
                    if (!MedicalLogic.CanUseItem(_targetPatient, stats))
                    {
                        NotificationManagerClass.DisplayMessageNotification(
                            MedicLocale.Get(MedicTextId.NoCompatibleWoundLocal, stats.Name), ENotificationDurationType.Default, ENotificationIconType.Alert);
                        return;
                    }
                    _activeHealCoroutine = StartCoroutine(HealRoutine(mainPlayer, _targetPatient, item, stats));
                }
            }
        }

        // === Estado pendente do handshake ===
        // Membro-alvo esperado, vindo da resposta do handshake (mostrado pré-animação)
        private EBodyPart _expectedTreatmentPart = EBodyPart.Common;
        private EBoundItem _pendingHealSlot;
        private Item _pendingHealItem;
        private ItemStats _pendingHealStats;
        private Player _pendingHealPatient;
        private float _pendingHealTimeout = -1f;

        /// <summary>
        /// Verifica se a tecla foi ativada conforme o modo configurado (Press, Hold, DoubleTap).
        /// </summary>
        private bool CheckPressMode(KeyboardShortcut shortcut, EBandAidPressMode mode,
            ref float holdTimer, ref bool holdTriggered, ref float lastTapTime)
        {
            KeyCode key = shortcut.MainKey;
            if (key == KeyCode.None) return false;

            // Smart modifier check:
            // - COM modifiers configurados (ex: Shift+F) → usar shortcut.IsDown() que verifica modifiers
            // - SEM modifiers (ex: F sozinho) → usar Input.GetKeyDown que ignora modifiers
            //   (necessário para funcionar enquanto corre com Shift, agachado com Ctrl, etc.)
            bool hasModifiers = shortcut.Modifiers.Any();

            switch (mode)
            {
                case EBandAidPressMode.Press:
                    return hasModifiers ? shortcut.IsDown() : Input.GetKeyDown(key);

                case EBandAidPressMode.Hold:
                    bool isHeld = hasModifiers ? shortcut.IsPressed() : Input.GetKey(key);
                    if (isHeld)
                    {
                        holdTimer += Time.deltaTime;
                        if (holdTimer >= HOLD_THRESHOLD && !holdTriggered)
                        {
                            holdTriggered = true;
                            return true;
                        }
                    }
                    if (Input.GetKeyUp(key))
                    {
                        holdTimer = 0f;
                        holdTriggered = false;
                    }
                    return false;

                case EBandAidPressMode.DoubleTap:
                    bool justPressed = hasModifiers ? shortcut.IsDown() : Input.GetKeyDown(key);
                    if (justPressed)
                    {
                        float now = Time.time;
                        if (lastTapTime > 0 && (now - lastTapTime) <= DOUBLE_TAP_WINDOW)
                        {
                            lastTapTime = -1f;
                            return true;
                        }
                        lastTapTime = now;
                    }
                    return false;
            }
            return false;
        }

        /// <summary>
        /// Envia um "toque no ombro" ao aliado próximo (aviso CQB).
        /// </summary>
        private void SendShoulderTap(Player target)
        {
            if (target == null) return;

            // Notificação local
            NotificationManagerClass.DisplayMessageNotification(
                MedicLocale.Get(MedicTextId.ShoulderTapSent, target.Profile.Nickname),
                ENotificationDurationType.Default, ENotificationIconType.Quest);

            // Tocar gesto de mão "There" (apontar com a mão)
            var doctor = Singleton<GameWorld>.Instance?.MainPlayer;
            doctor?.MovementContext.SetInteractInHands(EInteraction.ThereGesture);

            // Enviar via rede para o aliado ver a mensagem
            BandAidNetworkHandler.SendShoulderTapPacket(target);
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"Toque no ombro enviado para {target.Profile.Nickname}");
        }

        /// <summary>
        /// Drop emergencial: mata animação → limpa mãos → dropa item.
        /// </summary>
        private void EmergencyDrop()
        {
            var doctor = Singleton<GameWorld>.Instance.MainPlayer;

            // Parar a coroutine de cura
            if (_activeHealCoroutine != null)
            {
                StopCoroutine(_activeHealCoroutine);
                _activeHealCoroutine = null;
            }

            // === PASSO 1: SALVAR ITEM NA MEMÓRIA ===
            Item savedItem = _itemBeingUsed;
            string itemName = savedItem?.ShortName?.Localized() ?? "?";

            MedicHealPatch.CancelNativePatientEffect(); // ref: CR-02
            BandAidUI.Instance?.ClearTreatment();
            // Limpar flags do redirect
            MedicHealPatch.IsRedirectingHeal = false;
            MedicHealPatch.CurrentPatient = null;
            MedicHealPatch.CleanupPatientSubscription();
            MedicHealPatch.BandAidHealActive = false;
            _isHealingInProgress = false;
            _itemBeingUsed = null;
            // G10: Desregistrar evento de morte do paciente
            try { if (_targetPatient != null) _targetPatient.OnPlayerDeadOrUnspawn -= OnPatientDiedDuringHeal; } catch { }

            // === PASSO 2: MATAR ANIMAÇÃO IMEDIATAMENTE ===
            try
            {
                // CancelApplyingItem interrompe qualquer animação de cura em andamento
                doctor.ActiveHealthController?.CancelApplyingItem();
                // ForceFinishAnimation (method_9) mata o callback visual restante
                MedicHealPatch.ForceFinishAnimation();
            }
            catch (Exception ex)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning($"EmergencyDrop MatarAnimação: {ex.Message}");
            }

            // === PASSO 3: LIMPAR AS MÃOS (agora sem animação de fechar kit) ===
            try
            {
                var spawnController = doctor.GetType().GetMethod("SpawnController",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (spawnController != null)
                    spawnController.Invoke(doctor, null);

                doctor.TrySetLastEquippedWeapon(true);
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo("EmergencyDrop: Mãos limpas (HANB)");
            }
            catch (Exception ex)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning($"EmergencyDrop Limpar Mãos: {ex.Message}");
            }

            // === PASSO 4: DROPAR ITEM SALVO ===
            if (savedItem != null)
            {
                try
                {
                    doctor.InventoryController.ThrowItem(savedItem);
                    TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"Drop emergencial: {itemName} dropado.");
                }
                catch (Exception ex)
                {
                    TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning($"Erro ao dropar {itemName}: {ex.Message}");
                }
            }

            // === LIMPEZA DE MEMÓRIA ===
            savedItem = null;
            itemName = null;

            // Liberar movimento
            doctor.MovementContext.SetPhysicalCondition(EPhysicalCondition.UsingMeds, false);
            ReleaseSurgeryImmobilize(doctor);   // 077 — solta HealingLegs + reseta anim mult

            NotificationManagerClass.DisplayMessageNotification(MedicLocale.Get(MedicTextId.ItemDropped), ENotificationDurationType.Default, ENotificationIconType.Alert);
        }

        private bool _patientDiedDuringHeal = false;

        private void OnPatientDiedDuringHeal(Player player)
        {
            _patientDiedDuringHeal = true;
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning($"Paciente {player?.Profile?.Nickname} morreu durante cura!");
        }

        private IEnumerator HealRoutine(Player doctor, Player patient, Item itemUsed, ItemStats stats)
        {
            _isHealingInProgress = true;
            _itemBeingUsed = itemUsed;
            _patientDiedDuringHeal = false;
            MedicHealPatch.BandAidHealActive = true;

            // G10: Subscrever morte do paciente
            try { patient.OnPlayerDeadOrUnspawn += OnPatientDiedDuringHeal; }
            catch { }

            // Imobilizar o médico
            doctor.MovementContext.SetPhysicalCondition(EPhysicalCondition.UsingMeds, true);

            // 077 — perks de TEMPO/MOVIMENTO do Médico na cura de ALIADO (via CustomClasses; no-op se ausente).
            // O operador é o MainPlayer local, então o gate (classe) é resolvido localmente pela bridge (sem 057/packet).
            float allyTimeMult = CustomClassesBridge.AllyHealTimeMult(stats.IsSurgery);   // 1 se não-Médico/sem CC
            // Imobiliza o operador na CIRURGIA de aliado (HealingLegs = não anda), EXCETO Médico com Mobile Surgery.
            // Curativo NÃO enraíza (só UsingMeds, que permite andar — CanWalk só checa HealingLegs). Fail-safe: sem
            // CustomClasses, AllyMobileSurgeon()=false → ninguém anda (comportamento seguro do ICM standalone).
            if (stats.IsSurgery && !CustomClassesBridge.AllyMobileSurgeon())
                doctor.MovementContext.SetPhysicalCondition(EPhysicalCondition.HealingLegs, true); // ref: MovementContext.cs:1578/1296
            // Animação acompanha o UseTime encurtado. Setado ANTES do SetInHands, no início de CADA HealRoutine
            // (invariante do review PA-01-03 — não depende do reset do cleanup anterior).
            MedicHealPatch.AllyAnimSpeedMult = allyTimeMult > 0f ? 1f / allyTimeMult : 1f;

            NotificationManagerClass.DisplayMessageNotification(MedicLocale.Get(MedicTextId.ApplyingItem, itemUsed.ShortName.Localized()), ENotificationDurationType.Default, ENotificationIconType.Quest);

            // Feedback do membro-alvo: paciente remoto já informou o alvo esperado na
            // resposta do handshake (visível ANTES da animação); Common = "..." quando
            // desconhecido (o redirect local ou o report final refinam).
            BandAidUI.Instance?.ShowTreatment(_expectedTreatmentPart, itemUsed.ShortName?.Localized());
            _expectedTreatmentPart = EBodyPart.Common;

            // === ATIVAR REDIRECT ===
            MedicHealPatch.IsRedirectingHeal = true;
            MedicHealPatch.CurrentPatient = patient;
            MedicHealPatch.RedirectStartTime = UnityEngine.Time.time;
            MedicHealPatch.NativeMedEffectApplied = false;

            TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning($"HealRoutine: REDIRECT ATIVADO | item={itemUsed.ShortName.Localized()} | UseTime={stats.UseTime}s | patient={patient.Profile.Nickname}");

            // Colocar o item médico nas mãos (aciona a animação visual)
            try
            {
                doctor.SetInHands(itemUsed, (result) => { });
            }
            catch (Exception ex)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning($"SetInHands NullRef ignorado: {ex.Message}");
            }

            // Espera o tempo de uso do item (+2s para animação completar visualmente)
            // 077 — Swift Surgeon (cirurgia) / Rapid Care (demais) encurtam o procedimento de aliado p/ o Médico.
            float totalUseTime = stats.UseTime * allyTimeMult + 2f;
            yield return new WaitForSeconds(totalUseTime);

            // G2: Guard — médico morreu durante wait?
            if (doctor == null || !doctor.HealthController.IsAlive)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning("HealRoutine: Médico morreu durante cura, abortando.");
                CleanupHealState(patient);
                yield break;
            }

            // G10: Paciente morreu durante wait?
            if (_patientDiedDuringHeal || patient == null || !patient.HealthController.IsAlive)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning("HealRoutine: Paciente morreu durante cura, abortando.");
                CleanupHealState(patient);
                // Resetar mãos do médico que ainda está vivo
                try { doctor.MovementContext.SetPhysicalCondition(EPhysicalCondition.UsingMeds, false); } catch { }
                ReleaseSurgeryImmobilize(doctor);   // 077 — solta HealingLegs + reseta anim mult
                MedicHealPatch.ForceFinishAnimation();
                yield break;
            }

            TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning($"HealRoutine: UseTime TERMINOU | IsRedirecting={MedicHealPatch.IsRedirectingHeal} | T+{stats.UseTime}s");

            // === LIMPEZA DO REDIRECT ===
            MedicHealPatch.IsRedirectingHeal = false;
            MedicHealPatch.CurrentPatient = null;
            MedicHealPatch.CleanupPatientSubscription();

            // G3: Removido RemoveMedEffect no médico (efeito está no paciente, não no médico)
            // ForceFinishAnimation chama method_9 diretamente → cleanup → callback → jogo transiciona
            MedicHealPatch.ForceFinishAnimation();

            // Liberar movimento do médico
            doctor.MovementContext.SetPhysicalCondition(EPhysicalCondition.UsingMeds, false);
            ReleaseSurgeryImmobilize(doctor);   // 077 — solta HealingLegs + reseta anim mult

            _isHealingInProgress = false;
            _itemBeingUsed = null;

            // G10: Desregistrar morte do paciente
            try { patient.OnPlayerDeadOrUnspawn -= OnPatientDiedDuringHeal; } catch { }

            // ref: fix Salewa 2026-07-12 — se o redirect criou MedEffect NATIVO no paciente,
            // o próprio jogo cura HP, remove bleeds/fraturas (Residue) e consome HpResource;
            // chamar ApplyTreatment aqui dobraria cura e consumo.
            if (MedicHealPatch.NativeMedEffectApplied)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo("HealRoutine: MedEffect nativo aplicado no paciente — ApplyTreatment programático pulado.");
                // ref: CR-03 — expor a parte tratada na notificação (a feature do
                // membro-alvo já resolve essa informação)
                string treatmentText = MedicHealPatch.LastAppliedPart != EBodyPart.Common
                    ? MedicLocale.Get(MedicTextId.TreatmentCompleteWithPart, BandAidUI.PartLabel(MedicHealPatch.LastAppliedPart))
                    : MedicLocale.Get(MedicTextId.TreatmentComplete);
                NotificationManagerClass.DisplayMessageNotification(treatmentText, ENotificationDurationType.Long, ENotificationIconType.Quest);
                // ref: CR-04-14 — sucesso também limpa o rastreamento (referência
                // estática ao MedEffect não atravessa curas/raids)
                MedicHealPatch.ClearNativeEffectTracking();
            }
            // N3: Verificar se o item e o paciente ainda existem após o UseTime
            else if (patient != null && patient.HealthController.IsAlive && itemUsed != null)
            {
                try
                {
                    // Verificar se o item ainda é válido (não foi destruído/lootado)
                    var _ = itemUsed.TemplateId;
                    MedicalLogic.ApplyTreatment(doctor, patient, itemUsed, stats);
                    NotificationManagerClass.DisplayMessageNotification(MedicLocale.Get(MedicTextId.TreatmentComplete), ENotificationDurationType.Long, ENotificationIconType.Quest);
                }
                catch (Exception ex)
                {
                    TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning($"HealRoutine: Item destruído durante cura — {ex.Message}");
                    NotificationManagerClass.DisplayMessageNotification(MedicLocale.Get(MedicTextId.ItemLostDuringTreatment), ENotificationDurationType.Default, ENotificationIconType.Alert);
                }
            }
            else
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning("HealRoutine: Paciente null/morto ou item null, tratamento não aplicado.");
            }

            // === RESET DAS MÃOS (sem forçar puxada de arma — o jogo já faz automaticamente) ===
        }

        /// <summary>
        /// Limpa estado de cura (usado quando médico ou paciente morre durante HealRoutine).
        /// </summary>
        private void CleanupHealState(Player patient)
        {
            MedicHealPatch.CancelNativePatientEffect(); // ref: CR-02
            BandAidUI.Instance?.ClearTreatment();
            MedicHealPatch.IsRedirectingHeal = false;
            MedicHealPatch.CurrentPatient = null;
            MedicHealPatch.CleanupPatientSubscription();
            MedicHealPatch.BandAidHealActive = false;
            MedicHealPatch.AllyAnimSpeedMult = 1f;   // 077 — reset da velocidade extra (médico morto não passa pelo release)
            _isHealingInProgress = false;
            _itemBeingUsed = null;
            try { patient.OnPlayerDeadOrUnspawn -= OnPatientDiedDuringHeal; } catch { }
        }

        /// <summary>
        /// 077 — libera a imobilização da cirurgia de aliado (<c>HealingLegs</c>) e reseta a velocidade extra da
        /// animação. Pareado com CADA ponto que solta <c>UsingMeds</c> (fim/drop/cancel/deactivate/paciente-morto)
        /// + <c>ResetAllState</c>. Reset incondicional é seguro (desligar já-desligado = no-op); nunca desliga um
        /// <c>HealingLegs</c> de AUTO-cirurgia (aquele contexto não passa pelo HealRoutine).
        /// </summary>
        private void ReleaseSurgeryImmobilize(Player doctor)
        {
            try { if (doctor != null) doctor.MovementContext.SetPhysicalCondition(EPhysicalCondition.HealingLegs, false); } catch { }
            MedicHealPatch.AllyAnimSpeedMult = 1f;
        }

        /// <summary>
        /// Cancela cura em andamento de forma suave (sem dropar item).
        /// Equivalente ao vanilla Mouse0 durante uso de medkit.
        /// </summary>
        private void CancelHealInProgress()
        {
            var doctor = Singleton<GameWorld>.Instance?.MainPlayer;
            if (doctor == null) return;

            // Parar coroutine
            if (_activeHealCoroutine != null)
            {
                StopCoroutine(_activeHealCoroutine);
                _activeHealCoroutine = null;
            }

            MedicHealPatch.CancelNativePatientEffect(); // ref: CR-02
            BandAidUI.Instance?.ClearTreatment();
            // Limpar redirect e flags
            MedicHealPatch.IsRedirectingHeal = false;
            MedicHealPatch.CurrentPatient = null;
            MedicHealPatch.CleanupPatientSubscription();
            MedicHealPatch.BandAidHealActive = false;
            _isHealingInProgress = false;
            _itemBeingUsed = null;

            // G10: Desregistrar morte do paciente
            try { if (_targetPatient != null) _targetPatient.OnPlayerDeadOrUnspawn -= OnPatientDiedDuringHeal; } catch { }

            // Cancelar efeito médico nativo (como vanilla)
            try { doctor.ActiveHealthController?.CancelApplyingItem(); } catch { }

            // Finalizar animação
            MedicHealPatch.ForceFinishAnimation();

            // Liberar movimento
            try { doctor.MovementContext.SetPhysicalCondition(EPhysicalCondition.UsingMeds, false); } catch { }
            ReleaseSurgeryImmobilize(doctor);   // 077 — solta HealingLegs + reseta anim mult

            NotificationManagerClass.DisplayMessageNotification(MedicLocale.Get(MedicTextId.TreatmentCancelled), ENotificationDurationType.Default, ENotificationIconType.Alert);
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo("CancelHealInProgress: cura cancelada (Mouse0).");
        }

        // === INTERAÇÃO NATIVA ===
        // O prompt não é mais desenhado por canvas próprio nem detectado por SphereCast:
        // cada player/bot vivo ganha um MedicInteractable (InteractableObject), que o
        // raycast de interação vanilla encontra e o ActionPanel nativo exibe (como loot).
        private float _nextInteractableSweep = 0f;

        private void EnsureMedicInteractables()
        {
            if (Time.time < _nextInteractableSweep) return;
            _nextInteractableSweep = Time.time + 2f;

            var gameWorld = Singleton<GameWorld>.Instance;
            var mainPlayer = gameWorld.MainPlayer;
            var players = gameWorld.AllAlivePlayersList;
            for (int i = 0; i < players.Count; i++)
            {
                Player p = players[i];
                if (p == null || p == mainPlayer) continue;
                if (p.HealthController == null || !p.HealthController.IsAlive) continue;
                MedicInteractable.Ensure(p);
            }
        }

        // === PROMPT DIRIGIDO PELO CONTROLLER (regra única de distância) ===
        // O ActionPanel nativo executa SelectedAction.Action() no F sem revalidar
        // distância (EFT.UI.ActionPanel.TranslateCommand, ECommand.BeginInteracting —
        // decompilado do DLL real). Logo, quem controla AvailableInteractionState
        // controla prompt E acionamento com a MESMA regra.
        private GamePlayerOwner _gamePlayerOwner;
        private ActionsReturnClass _ourPromptActions;   // só limpamos o que NÓS setamos
        private Player _promptTarget;
        private float _nextPromptScan = 0f;
        private static readonly RaycastHit[] _scanHits = new RaycastHit[24];

        private void UpdateNativePrompt()
        {
            if (Time.time < _nextPromptScan) return;
            _nextPromptScan = Time.time + 0.1f; // 10 Hz: responsivo sem custo por frame

            var mainPlayer = Singleton<GameWorld>.Instance.MainPlayer;

            if (_gamePlayerOwner == null)
                _gamePlayerOwner = mainPlayer.gameObject.GetComponent<GamePlayerOwner>();
            if (_gamePlayerOwner == null) return; // sem owner (ex.: headless) — patches cobrem ≤1,3m

            Player target = _isMedicModeActive ? null : ScanForMedicTarget(mainPlayer);
            var current = _gamePlayerOwner.AvailableInteractionState.Value;

            if (target != null)
            {
                // Vanilla tem prompt próprio (porta/loot/revive)? Não interferir.
                if (current != null && current != _ourPromptActions) return;

                if (current == null || _promptTarget != target)
                {
                    MedicInteractable.Ensure(target);
                    var medic = target.GetComponent<MedicInteractable>();
                    var actions = medic != null ? medic.GetActions(_gamePlayerOwner) : null;
                    if (actions == null) return;
                    actions.InitSelected();
                    _ourPromptActions = actions;
                    _promptTarget = target;
                    _gamePlayerOwner.AvailableInteractionState.Value = actions;
                }
                // mesmo alvo e prompt já é nosso: não reescrever (preserva scroll/seleção)
            }
            else if (current != null && current == _ourPromptActions)
            {
                _gamePlayerOwner.ClearInteractionState();
                _ourPromptActions = null;
                _promptTarget = null;
            }
        }

        /// <summary>
        /// Scan por alvo médico: SphereCast da origem de interação do jogador na direção
        /// do olhar, distância = config MedicInteractDistance, respeitando oclusão
        /// (parede entre médico e alvo bloqueia; corpos/triggers não bloqueiam).
        /// </summary>
        private Player ScanForMedicTarget(Player mainPlayer)
        {
            float maxDist = TRLImmersiveCombatMedicinePlugin.MedicInteractDistance.Value;
            Vector3 origin;
            try { origin = mainPlayer.PlayerBones.LootRaycastOrigin.position; }
            catch { origin = mainPlayer.PlayerBones.WeaponRoot.position; }
            Vector3 direction = mainPlayer.LookDirection;

            var gameWorld = Singleton<GameWorld>.Instance;
            int count = Physics.SphereCastNonAlloc(new Ray(origin, direction), 0.25f, _scanHits, maxDist,
                Physics.AllLayers, QueryTriggerInteraction.Ignore);

            // Ordenar por distância (insertion sort no buffer — sem alocação)
            for (int i = 1; i < count; i++)
            {
                var key = _scanHits[i];
                int j = i - 1;
                while (j >= 0 && _scanHits[j].distance > key.distance) { _scanHits[j + 1] = _scanHits[j]; j--; }
                _scanHits[j + 1] = key;
            }

            for (int i = 0; i < count; i++)
            {
                var col = _scanHits[i].collider;
                Player p = gameWorld.GetPlayerByCollider(col);
                if (p == null) p = col.GetComponentInParent<Player>();

                if (p != null)
                {
                    if (p == mainPlayer) continue;                      // próprio corpo
                    if (p.HealthController == null || !p.HealthController.IsAlive) continue; // cadáver não bloqueia
                    return p;                                           // alvo válido mais próximo
                }

                // Sólido que não é player antes do alvo = oclusão (parede/obstáculo)
                return null;
            }
            return null;
        }

        /// <summary>
        /// Chamado pela ação "Examinar (Médico)" do ActionPanel nativo (MedicInteractable).
        /// </summary>
        public void ActivateMedicModeExternal(Player patient)
        {
            if (patient == null || _isMedicModeActive) return;
            ActivateMedicMode(patient);
        }

        /// <summary>
        /// Chamado pela ação "Tocar no ombro" do ActionPanel nativo (MedicInteractable).
        /// </summary>
        public void SendShoulderTapExternal(Player target)
        {
            SendShoulderTap(target);
        }

        private void ActivateMedicMode(Player p) 
        { 
            _isMedicModeActive = true; 
            _targetPatient = p; 
            BandAidUI.Instance?.ShowUI(p);
            NotificationManagerClass.DisplayMessageNotification(MedicLocale.Get(MedicTextId.MedicExamining, p.Profile.Nickname), ENotificationDurationType.Default, ENotificationIconType.Quest);
        }

        private void DeactivateMedicMode() 
        { 
            // Segurança: liberar UsingMeds se o modo médico for interrompido durante cura
            if (_isHealingInProgress)
            {
                // N6: Parar coroutine ativa
                if (_activeHealCoroutine != null)
                {
                    StopCoroutine(_activeHealCoroutine);
                    _activeHealCoroutine = null;
                }

                var mainPlayer = Singleton<GameWorld>.Instance?.MainPlayer;
                if (mainPlayer != null)
                {
                    mainPlayer.MovementContext.SetPhysicalCondition(EPhysicalCondition.UsingMeds, false);
                    ReleaseSurgeryImmobilize(mainPlayer);   // 077 — solta HealingLegs + reseta anim mult
                    try { mainPlayer.ActiveHealthController?.CancelApplyingItem(); } catch { }
                }

                _isHealingInProgress = false;
                _itemBeingUsed = null;
                MedicHealPatch.CancelNativePatientEffect(); // ref: CR-02
                BandAidUI.Instance?.ClearTreatment();
                MedicHealPatch.IsRedirectingHeal = false;
                MedicHealPatch.CurrentPatient = null;
                MedicHealPatch.CleanupPatientSubscription();

                // N6: Finalizar animação para evitar travamento
                MedicHealPatch.ForceFinishAnimation();
                // G10: Desregistrar evento de morte do paciente
                try { if (_targetPatient != null) _targetPatient.OnPlayerDeadOrUnspawn -= OnPatientDiedDuringHeal; } catch { }
            }
            _isMedicModeActive = false;
            _targetPatient = null;
            BandAidUI.Instance?.HideUI();

            // ref: CR-01-10 — fechar o modo médico invalida qualquer check pendente
            _pendingHealTimeout = -1f;
            _pendingHealItem = null;
            _pendingHealStats = null;
            _pendingHealPatient = null;
        }

        /// <summary>
        /// ProfileId do paciente atualmente examinado (null fora do modo médico).
        /// ref: CR-03 — usado pela identidade do TreatmentReport.
        /// </summary>
        public string ActivePatientProfileId => _isMedicModeActive ? _targetPatient?.ProfileId : null;

        /// <summary>
        /// ref: CR-05 — descarte networked DIFERIDO: no fim forçado da animação o
        /// item ainda está nas mãos e a RemoveOperation falha em CanExecute (100%
        /// dos descartes no log 2-PCs — itens de 1 uso nunca sumiam, kits zerados
        /// nunca desapareciam). Espera as mãos liberarem (MedsController sair) e
        /// tenta com retry espaçado.
        /// </summary>
        // ref: CR-04-16/18 — dedup por item + tracking p/ parar no fim da raid
        private readonly HashSet<string> _activeDiscardIds = new HashSet<string>();
        private readonly List<Coroutine> _discardCoroutines = new List<Coroutine>();

        public void ScheduleNetworkedDiscard(Player doctor, Item item)
        {
            string itemId = item?.Id;
            if (itemId == null) return;
            if (!_activeDiscardIds.Add(itemId))
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo(
                    "Descarte já agendado para este item — ignorando duplicata (CR-04-18).");
                return;
            }
            _discardCoroutines.Add(StartCoroutine(DeferredDiscardRoutine(doctor, item, itemId)));
        }

        public void StopAllDeferredDiscards()
        {
            foreach (var c in _discardCoroutines)
                if (c != null) StopCoroutine(c);
            _discardCoroutines.Clear();
            _activeDiscardIds.Clear();
        }

        private IEnumerator DeferredDiscardRoutine(Player doctor, Item item, string itemId)
        {
            // ref: CR-04-16 — a coroutine vive no GO do plugin (sessão inteira):
            // amarrar ao GameWorld da raid em que nasceu; se a raid trocar/acabar,
            // abortar (item/controller daquela raid estão mortos).
            var bornWorld = Singleton<GameWorld>.Instance;
            try
            {
                // 1) Esperar as mãos liberarem (timeout 6s — depois tenta mesmo assim)
                float handsDeadline = Time.time + 6f;
                while (Time.time < handsDeadline && doctor != null &&
                       doctor.HandsController is Player.MedsController)
                {
                    yield return new WaitForSeconds(0.25f);
                }
                yield return new WaitForSeconds(0.2f); // folga p/ o pipeline fechar eventos

                // 2) Até 4 tentativas espaçadas
                for (int attempt = 1; attempt <= 4; attempt++)
                {
                    if (doctor == null || Singleton<GameWorld>.Instance != bornWorld) yield break;

                    var watch = new MedicalLogic.DiscardWatch();
                    int r = MedicalLogic.StartDiscardAttempt(doctor, item, watch);
                    if (r == MedicalLogic.DISCARD_DONE) yield break;

                    if (r == MedicalLogic.DISCARD_SENT)
                    {
                        // ref: CR-04-02 — o Fika valida em async (Task.Yield): esperar
                        // o callback disparar (cap 1.2s) antes de decidir.
                        float cbDeadline = Time.time + 1.2f;
                        while (!watch.CallbackFired && Time.time < cbDeadline)
                            yield return null;

                        if (watch.CallbackFired && !watch.Failed)
                        {
                            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo(
                                "Descarte confirmado pelo pipeline (CR-04-02).");
                            yield break;
                        }
                        // Failed ou timeout sem callback (ex.: player morto — callback
                        // nunca dispara): retry; o guard de CurrentAddress da próxima
                        // tentativa evita double-discard se a operação executou tarde.
                    }

                    TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning(
                        $"Descarte diferido: tentativa {attempt}/4 sem confirmação — nova em 0.75s.");
                    yield return new WaitForSeconds(0.75f);
                }
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogError(
                    "Descarte diferido: TODAS as tentativas falharam — item zerado permanece no inventário (reportar).");
            }
            finally
            {
                _activeDiscardIds.Remove(itemId);
            }
        }

        /// <summary>
        /// Chamado externamente pela UI quando o paciente sai de range.
        /// </summary>
        public void DeactivateMedicModeExternal()
        {
            DeactivateMedicMode();
        }

        /// <summary>
        /// N1: Reseta todas as flags estáticas entre raids.
        /// </summary>
        private void ResetAllState()
        {
            if (_activeHealCoroutine != null)
            {
                StopCoroutine(_activeHealCoroutine);
                _activeHealCoroutine = null;
            }
            _isHealingInProgress = false;
            _isMedicModeActive = false;
            _itemBeingUsed = null;
            _targetPatient = null;

            MedicalLogic.ClearPendingConsumes(); // ref: CR-05 — sem débito entre raids
            StopAllDeferredDiscards();           // ref: CR-04-16 — coroutines não atravessam raids
            _expectedTreatmentPart = EBodyPart.Common; // ref: CR-04-15
            MedicHealPatch.CancelNativePatientEffect(); // ref: CR-02
            MedicHealPatch.ClearNativeEffectTracking(); // ref: CR-04-14
            BandAidUI.Instance?.ClearTreatment();
            MedicHealPatch.IsRedirectingHeal = false;
            MedicHealPatch.CurrentPatient = null;
            MedicHealPatch.BandAidHealActive = false;
            MedicHealPatch.CleanupPatientSubscription();
            MedicHealPatch.AllyAnimSpeedMult = 1f;   // 077 — velocidade extra de animação não atravessa raids

            BandAidUI.Instance?.HideUI();
            DebugBotInvisibility.OnRaidEnded();

            // ref: CR-01-09/13 — este método roda quando o GameWorld morre (polling em
            // Update), cobrindo morte/extract/alt-F4 SEM depender de patch em
            // OnDestroy (que overrides do Fika poderiam pular): estado de trauma e o
            // AudioListener.volume (estático global do Unity!) não vazam para o menu.
            TrueTrauma.TraumaState.ResetAll();
            AudioListener.volume = 1f;

            // Estado do prompt dirigido (GamePlayerOwner morre com a raid)
            _gamePlayerOwner = null;
            _ourPromptActions = null;
            _promptTarget = null;

            // ref: CR-01-10 — handshake pendente não sobrevive à raid (senão pina o
            // Player destruído e uma resposta stale poderia iniciar cura órfã)
            _pendingHealTimeout = -1f;
            _pendingHealItem = null;
            _pendingHealStats = null;
            _pendingHealPatient = null;
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo("ResetAllState: flags resetadas (mudança de raid).");
        }

    }
}









