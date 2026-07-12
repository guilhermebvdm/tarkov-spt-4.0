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
        private Player _potentialTarget = null;

        // HUD de interaÃ§Ã£o (Canvas UI)
        private GameObject _interactCanvasObj;
        private Text _interactText;
        private Image _interactBg;

        // Controle da AnimaÃ§Ã£o
        private bool _isHealingInProgress = false;
        private Item _itemBeingUsed = null;
        private Coroutine _activeHealCoroutine = null;

        // N1: Detectar mudanÃ§a de raid para resetar flags estÃ¡ticas
        private GameWorld _lastGameWorld = null;

        // Harmony
        private Harmony _harmony;

        // Config (agora lidas do plugin principal)
        public static ConfigEntry<KeyboardShortcut> _medicInteractKey => TRLImmersiveCombatMedicinePlugin.MedicInteractKey;
        public static ConfigEntry<KeyboardShortcut> _emergencyDropKey => TRLImmersiveCombatMedicinePlugin.EmergencyDropKey;
        public static ConfigEntry<KeyboardShortcut> _shoulderTapKey => TRLImmersiveCombatMedicinePlugin.ShoulderTapKey;
        public static ConfigEntry<EBandAidPressMode> _medicInteractMode => TRLImmersiveCombatMedicinePlugin.MedicInteractMode;
        public static ConfigEntry<EBandAidPressMode> _emergencyDropMode => TRLImmersiveCombatMedicinePlugin.EmergencyDropMode;
        public static ConfigEntry<EBandAidPressMode> _shoulderTapMode => TRLImmersiveCombatMedicinePlugin.ShoulderTapMode;

        // DetecÃ§Ã£o de Hold
        private float _medicHoldTimer = 0f;
        private bool _medicHoldTriggered = false;
        private float _emergencyHoldTimer = 0f;
        private bool _emergencyHoldTriggered = false;
        private float _shoulderHoldTimer = 0f;
        private bool _shoulderHoldTriggered = false;
        private const float HOLD_THRESHOLD = 0.4f; // segundos para Hold

        // DetecÃ§Ã£o de DoubleTap
        private float _medicLastTapTime = -1f;
        private float _emergencyLastTapTime = -1f;
        private float _shoulderLastTapTime = -1f;
        private const float DOUBLE_TAP_WINDOW = 0.35f; // janela para double tap

        private void Awake()
        {
            Instance = this;
            
            CreateInteractHUD();

            // Registrar handler de resposta do handshake
            BandAidNetworkHandler.OnHealCheckResponse += OnHealCheckResponseHandler;
        }

        private void OnDestroy()
        {
            BandAidNetworkHandler.OnHealCheckResponse -= OnHealCheckResponseHandler;
        }

        private void OnHealCheckResponseHandler(BandAidHealCheckResponsePacket response)
        {
            // Verificar se temos um check pendente
            if (_pendingHealTimeout < 0 || _pendingHealItem == null || _pendingHealPatient == null) return;

            // Limpar timeout
            _pendingHealTimeout = -1f;

            if (response.Approved)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"Handshake aprovado! Iniciando cura com {_pendingHealStats?.Name}.");
                var mainPlayer = Singleton<GameWorld>.Instance?.MainPlayer;
                if (mainPlayer != null && _pendingHealItem != null && _pendingHealStats != null)
                {
                    _activeHealCoroutine = StartCoroutine(
                        HealRoutine(mainPlayer, _pendingHealPatient, _pendingHealItem, _pendingHealStats));
                }
            }
            else
            {
                NotificationManagerClass.DisplayMessageNotification(
                    response.DenyReason, ENotificationDurationType.Default, ENotificationIconType.Alert);
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"Handshake negado: {response.DenyReason}");
            }

            // Limpar estado pendente
            _pendingHealItem = null;
            _pendingHealStats = null;
            _pendingHealPatient = null;
        }

        // === HUD DE INTERAÃ‡ÃƒO (Canvas UI) ===
        private void CreateInteractHUD()
        {
            _interactCanvasObj = new GameObject("BandAid_InteractCanvas");
            DontDestroyOnLoad(_interactCanvasObj);

            Canvas canvas = _interactCanvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 90;

            CanvasScaler scaler = _interactCanvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // Background
            GameObject bgObj = new GameObject("InteractBg");
            bgObj.transform.SetParent(_interactCanvasObj.transform, false);
            _interactBg = bgObj.AddComponent<Image>();
            _interactBg.color = new Color(0.01f, 0.02f, 0.04f, 0.55f);
            _interactBg.raycastTarget = false;
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(1f, 0f);
            bgRect.anchorMax = new Vector2(1f, 0f);
            bgRect.pivot = new Vector2(1f, 0f);
            bgRect.anchoredPosition = new Vector2(-30, 120);

            var layout = bgObj.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 8, 8);
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 2f;
            var fitter = bgObj.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Borda sutil
            var outline = bgObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.12f, 0.25f, 0.30f, 0.30f);
            outline.effectDistance = new Vector2(0.5f, -0.5f);

            // Texto
            GameObject textObj = new GameObject("InteractText");
            textObj.transform.SetParent(bgObj.transform, false);
            _interactText = textObj.AddComponent<Text>();
            _interactText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            _interactText.fontSize = 16;
            _interactText.fontStyle = FontStyle.Normal;
            _interactText.color = Color.white;
            _interactText.alignment = TextAnchor.MiddleLeft;
            _interactText.supportRichText = true;
            _interactText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _interactText.verticalOverflow = VerticalWrapMode.Overflow;
            var textOutline = textObj.AddComponent<Outline>();
            textOutline.effectColor = new Color(0, 0, 0, 0.7f);
            textOutline.effectDistance = new Vector2(0.5f, -0.5f);

            _interactCanvasObj.SetActive(false);
        }

        private void UpdateInteractFont()
        {
            if (_interactText != null && BandAidUI.Instance != null)
            {
                try
                {
                    Font f = BandAidUI.Instance.GetFont();
                    if (f != null) _interactText.font = f;
                }
                catch { }
            }
        }

        private void Update()
        {
            // O registro de pacotes deve ocorrer independentemente de haver um jogador local.
            // Em servidores dedicados (Headless), o MainPlayer Ã© null. Se pularmos, os pacotes nunca sÃ£o registrados.
            BandAidNetworkHandler.CheckInit();

            if (Singleton<GameWorld>.Instance == null || Singleton<GameWorld>.Instance.MainPlayer == null)
            {
                // N1: Se GameWorld foi destruÃ­do, resetar tudo
                if (_lastGameWorld != null)
                {
                    ResetAllState();
                    _lastGameWorld = null;
                }
                return;
            }

            // N1: Detectar mudanÃ§a de raid (novo GameWorld)
            if (_lastGameWorld != Singleton<GameWorld>.Instance)
            {
                ResetAllState();
                _lastGameWorld = Singleton<GameWorld>.Instance;
            }

            ScanForPatient();

            // === EMERGENCY DROP ===
            if (_isHealingInProgress && CheckPressMode(_emergencyDropKey.Value, _emergencyDropMode.Value,
                ref _emergencyHoldTimer, ref _emergencyHoldTriggered, ref _emergencyLastTapTime))
            {
                EmergencyDrop();
                return;
            }

            // Item 7: BotÃ£o esquerdo do mouse cancela cura em andamento (sem dropar item)
            if (_isHealingInProgress && Input.GetMouseButtonDown(0))
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning("Mouse0 pressionado durante cura â€” cancelando tratamento (sem drop).");
                CancelHealInProgress();
                return;
            }

            // === TOQUE NO OMBRO (CQB) ===
            if (!_isHealingInProgress && _potentialTarget != null && !_isMedicModeActive &&
                CheckPressMode(_shoulderTapKey.Value, _shoulderTapMode.Value,
                ref _shoulderHoldTimer, ref _shoulderHoldTriggered, ref _shoulderLastTapTime))
            {
                SendShoulderTap(_potentialTarget);
            }

            // === MEDIC INTERACT ===
            if (CheckPressMode(_medicInteractKey.Value, _medicInteractMode.Value,
                ref _medicHoldTimer, ref _medicHoldTriggered, ref _medicLastTapTime))
            {
                if (_isMedicModeActive) DeactivateMedicMode();
                else if (_potentialTarget != null) ActivateMedicMode(_potentialTarget);
            }

            if (_isMedicModeActive && _targetPatient != null)
            {
                if (Vector3.Distance(Singleton<GameWorld>.Instance.MainPlayer.Position, _targetPatient.Position) > 2.5f)
                {
                    if (_isHealingInProgress)
                    {
                        _isHealingInProgress = false;
                        _itemBeingUsed = null;
                        MedicHealPatch.IsRedirectingHeal = false;
                        MedicHealPatch.CurrentPatient = null;
                        NotificationManagerClass.DisplayMessageNotification("Abortado!", ENotificationDurationType.Default, ENotificationIconType.Alert);
                    }
                    DeactivateMedicMode();
                    return;
                }

                if (!_isHealingInProgress) CheckManualInputs();

                // Timeout do handshake
                if (_pendingHealTimeout > 0 && Time.time > _pendingHealTimeout)
                {
                    _pendingHealTimeout = -1f;
                    _pendingHealItem = null;
                    _pendingHealStats = null;
                    _pendingHealPatient = null;
                    NotificationManagerClass.DisplayMessageNotification(
                        "Sem resposta do paciente (timeout).", ENotificationDurationType.Default, ENotificationIconType.Alert);
                }
            }
        }

        // Mapeamento EGameKey.Slot â†’ EBoundItem
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

                foreach (var keyGroup in keyBindings)
                {
                    if (!_slotMapping.ContainsKey(keyGroup.keyName)) continue;

                    // Verificar se a tecla foi ativada conforme o PressType configurado
                    if (IsSlotKeyTriggered(keyGroup))
                    {
                        ProcessHeal(_slotMapping[keyGroup.keyName]);
                        return; // SÃ³ processa um slot por frame
                    }
                }
            }
            catch (Exception ex)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning($"CheckManualInputs: {ex.Message}");
            }
        }

        private bool IsSlotKeyTriggered(EFT.InputSystem.KeyGroup keyGroup)
        {
            foreach (var variant in keyGroup.variants)
            {
                if (variant.keyCode == null || variant.keyCode.Count == 0) continue;

                // Para combos (ex: [LeftShift, Mouse3]):
                // TODAS as teclas devem estar pressionadas (GetKey),
                // e pelo menos UMA deve ter sido acionada NESTE frame (GetKeyDown).
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
                    return true;
            }
            return false;
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
                        $"Verificando {stats.Name}...", ENotificationDurationType.Default, ENotificationIconType.Quest);
                }
                else
                {
                    // === PACIENTE LOCAL (bot/self): ValidaÃ§Ã£o imediata ===
                    if (!MedicalLogic.CanUseItem(_targetPatient, stats))
                    {
                        NotificationManagerClass.DisplayMessageNotification(
                            $"{stats.Name}: Sem ferimento compatÃ­vel.", ENotificationDurationType.Default, ENotificationIconType.Alert);
                        return;
                    }
                    _activeHealCoroutine = StartCoroutine(HealRoutine(mainPlayer, _targetPatient, item, stats));
                }
            }
        }

        // === Estado pendente do handshake ===
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
            // - COM modifiers configurados (ex: Shift+F) â†’ usar shortcut.IsDown() que verifica modifiers
            // - SEM modifiers (ex: F sozinho) â†’ usar Input.GetKeyDown que ignora modifiers
            //   (necessÃ¡rio para funcionar enquanto corre com Shift, agachado com Ctrl, etc.)
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
        /// Envia um "toque no ombro" ao aliado prÃ³ximo (aviso CQB).
        /// </summary>
        private void SendShoulderTap(Player target)
        {
            if (target == null) return;

            // NotificaÃ§Ã£o local
            NotificationManagerClass.DisplayMessageNotification(
                $"Toque no ombro â†’ {target.Profile.Nickname}", 
                ENotificationDurationType.Default, ENotificationIconType.Quest);

            // Tocar gesto de mÃ£o "There" (apontar com a mÃ£o)
            var doctor = Singleton<GameWorld>.Instance?.MainPlayer;
            doctor?.MovementContext.SetInteractInHands(EInteraction.ThereGesture);

            // Enviar via rede para o aliado ver a mensagem
            BandAidNetworkHandler.SendShoulderTapPacket(target);
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"Toque no ombro enviado para {target.Profile.Nickname}");
        }

        /// <summary>
        /// Drop emergencial: mata animaÃ§Ã£o â†’ limpa mÃ£os â†’ dropa item.
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

            // === PASSO 1: SALVAR ITEM NA MEMÃ“RIA ===
            Item savedItem = _itemBeingUsed;
            string itemName = savedItem?.ShortName?.Localized() ?? "?";

            // Limpar flags do redirect
            MedicHealPatch.IsRedirectingHeal = false;
            MedicHealPatch.CurrentPatient = null;
            MedicHealPatch.CleanupPatientSubscription();
            MedicHealPatch.BandAidHealActive = false;
            _isHealingInProgress = false;
            _itemBeingUsed = null;
            // G10: Desregistrar evento de morte do paciente
            try { if (_targetPatient != null) _targetPatient.OnPlayerDeadOrUnspawn -= OnPatientDiedDuringHeal; } catch { }

            // === PASSO 2: MATAR ANIMAÃ‡ÃƒO IMEDIATAMENTE ===
            try
            {
                // CancelApplyingItem interrompe qualquer animaÃ§Ã£o de cura em andamento
                doctor.ActiveHealthController?.CancelApplyingItem();
                // ForceFinishAnimation (method_9) mata o callback visual restante
                MedicHealPatch.ForceFinishAnimation();
            }
            catch (Exception ex)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning($"EmergencyDrop MatarAnimaÃ§Ã£o: {ex.Message}");
            }

            // === PASSO 3: LIMPAR AS MÃƒOS (agora sem animaÃ§Ã£o de fechar kit) ===
            try
            {
                var spawnController = doctor.GetType().GetMethod("SpawnController",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (spawnController != null)
                    spawnController.Invoke(doctor, null);

                doctor.TrySetLastEquippedWeapon(true);
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo("EmergencyDrop: MÃ£os limpas (HANB)");
            }
            catch (Exception ex)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning($"EmergencyDrop Limpar MÃ£os: {ex.Message}");
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

            // === LIMPEZA DE MEMÃ“RIA ===
            savedItem = null;
            itemName = null;

            // Liberar movimento
            doctor.MovementContext.SetPhysicalCondition(EPhysicalCondition.UsingMeds, false);

            NotificationManagerClass.DisplayMessageNotification("Item dropado!", ENotificationDurationType.Default, ENotificationIconType.Alert);
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

            // Imobilizar o mÃ©dico
            doctor.MovementContext.SetPhysicalCondition(EPhysicalCondition.UsingMeds, true);

            NotificationManagerClass.DisplayMessageNotification($"Aplicando {itemUsed.ShortName.Localized()}...", ENotificationDurationType.Default, ENotificationIconType.Quest);

            // === ATIVAR REDIRECT ===
            MedicHealPatch.IsRedirectingHeal = true;
            MedicHealPatch.CurrentPatient = patient;
            MedicHealPatch.RedirectStartTime = UnityEngine.Time.time;

            TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning($"ðŸ” HealRoutine: REDIRECT ATIVADO | item={itemUsed.ShortName.Localized()} | UseTime={stats.UseTime}s | patient={patient.Profile.Nickname}");

            // Colocar o item mÃ©dico nas mÃ£os (aciona a animaÃ§Ã£o visual)
            try
            {
                doctor.SetInHands(itemUsed, (result) => { });
            }
            catch (Exception ex)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning($"SetInHands NullRef ignorado: {ex.Message}");
            }

            // Espera o tempo de uso do item (+2s para animaÃ§Ã£o completar visualmente)
            float totalUseTime = stats.UseTime + 2f;
            yield return new WaitForSeconds(totalUseTime);

            // G2: Guard â€” mÃ©dico morreu durante wait?
            if (doctor == null || !doctor.HealthController.IsAlive)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning("HealRoutine: MÃ©dico morreu durante cura, abortando.");
                CleanupHealState(patient);
                yield break;
            }

            // G10: Paciente morreu durante wait?
            if (_patientDiedDuringHeal || patient == null || !patient.HealthController.IsAlive)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning("HealRoutine: Paciente morreu durante cura, abortando.");
                CleanupHealState(patient);
                // Resetar mÃ£os do mÃ©dico que ainda estÃ¡ vivo
                try { doctor.MovementContext.SetPhysicalCondition(EPhysicalCondition.UsingMeds, false); } catch { }
                MedicHealPatch.ForceFinishAnimation();
                yield break;
            }

            TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning($"ðŸ” HealRoutine: UseTime TERMINOU | IsRedirecting={MedicHealPatch.IsRedirectingHeal} | T+{stats.UseTime}s");

            // === LIMPEZA DO REDIRECT ===
            MedicHealPatch.IsRedirectingHeal = false;
            MedicHealPatch.CurrentPatient = null;
            MedicHealPatch.CleanupPatientSubscription();

            // G3: Removido RemoveMedEffect no mÃ©dico (efeito estÃ¡ no paciente, nÃ£o no mÃ©dico)
            // ForceFinishAnimation chama method_9 diretamente â†’ cleanup â†’ callback â†’ jogo transiciona
            MedicHealPatch.ForceFinishAnimation();

            // Liberar movimento do mÃ©dico
            doctor.MovementContext.SetPhysicalCondition(EPhysicalCondition.UsingMeds, false);

            _isHealingInProgress = false;
            _itemBeingUsed = null;

            // G10: Desregistrar morte do paciente
            try { patient.OnPlayerDeadOrUnspawn -= OnPatientDiedDuringHeal; } catch { }

            // N3: Verificar se o item e o paciente ainda existem apÃ³s o UseTime
            if (patient != null && patient.HealthController.IsAlive && itemUsed != null)
            {
                try
                {
                    // Verificar se o item ainda Ã© vÃ¡lido (nÃ£o foi destruÃ­do/lootado)
                    var _ = itemUsed.TemplateId;
                    MedicalLogic.ApplyTreatment(doctor, patient, itemUsed, stats);
                    NotificationManagerClass.DisplayMessageNotification("Tratamento Completo.", ENotificationDurationType.Long, ENotificationIconType.Quest);
                }
                catch (Exception ex)
                {
                    TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning($"HealRoutine: Item destruÃ­do durante cura â€” {ex.Message}");
                    NotificationManagerClass.DisplayMessageNotification("Item perdido durante tratamento.", ENotificationDurationType.Default, ENotificationIconType.Alert);
                }
            }
            else
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning("HealRoutine: Paciente null/morto ou item null, tratamento nÃ£o aplicado.");
            }

            // === RESET DAS MÃƒOS (sem forÃ§ar puxada de arma â€” o jogo jÃ¡ faz automaticamente) ===
        }

        /// <summary>
        /// Limpa estado de cura (usado quando mÃ©dico ou paciente morre durante HealRoutine).
        /// </summary>
        private void CleanupHealState(Player patient)
        {
            MedicHealPatch.IsRedirectingHeal = false;
            MedicHealPatch.CurrentPatient = null;
            MedicHealPatch.CleanupPatientSubscription();
            MedicHealPatch.BandAidHealActive = false;
            _isHealingInProgress = false;
            _itemBeingUsed = null;
            try { patient.OnPlayerDeadOrUnspawn -= OnPatientDiedDuringHeal; } catch { }
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

            // Limpar redirect e flags
            MedicHealPatch.IsRedirectingHeal = false;
            MedicHealPatch.CurrentPatient = null;
            MedicHealPatch.CleanupPatientSubscription();
            MedicHealPatch.BandAidHealActive = false;
            _isHealingInProgress = false;
            _itemBeingUsed = null;

            // G10: Desregistrar morte do paciente
            try { if (_targetPatient != null) _targetPatient.OnPlayerDeadOrUnspawn -= OnPatientDiedDuringHeal; } catch { }

            // Cancelar efeito mÃ©dico nativo (como vanilla)
            try { doctor.ActiveHealthController?.CancelApplyingItem(); } catch { }

            // Finalizar animaÃ§Ã£o
            MedicHealPatch.ForceFinishAnimation();

            // Liberar movimento
            try { doctor.MovementContext.SetPhysicalCondition(EPhysicalCondition.UsingMeds, false); } catch { }

            NotificationManagerClass.DisplayMessageNotification("Tratamento cancelado.", ENotificationDurationType.Default, ENotificationIconType.Alert);
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo("CancelHealInProgress: cura cancelada (Mouse0).");
        }

                        private void ScanForPatient()
        {
                                    if (_isMedicModeActive) return;
            var mainPlayer = Singleton<GameWorld>.Instance.MainPlayer;
            if (mainPlayer == null) return;
            
            // Usar LookDirection do jogador ao invés do Camera.main que pode ser null no SPT 4.0
            Vector3 origin = mainPlayer.PlayerBones.WeaponRoot.position;
            Vector3 direction = mainPlayer.LookDirection;

                                    // Detectar todas as camadas; o Unity rejeita lixo (paredes/chão) através do GetComponentInParent<Player>
            LayerMask mask = Physics.AllLayers;
            Ray ray = new Ray(origin, direction);
            
            Player closest = null;
            float closestDist = float.MaxValue;

            // Usar RaycastAll para evitar que o corpo do prÃ³prio MainPlayer bloqueie o tiro
            var hits = Physics.SphereCastAll(ray, 0.4f, 2.5f, mask);
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"ScanForPatient: SphereCastAll hits: {hits.Length}");
            foreach (var hit in hits)
            {
                Player p = Singleton<GameWorld>.Instance.GetPlayerByCollider(hit.collider);
                if (p == null) p = hit.collider.GetComponentInParent<Player>();
                
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($" Hit: {hit.collider.name} (Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}) -> Player: {p?.Profile?.Nickname ?? "null"}");

                if (p != null && p != mainPlayer)
                {
                    float dist = Vector3.Distance(mainPlayer.Position, p.Position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = p;
                    }
                }
            }

            // Fallback para perto (OverlapSphere)
            if (closest == null)
            {
                Collider[] nearby = Physics.OverlapSphere(origin, 1.5f, mask);
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"ScanForPatient: OverlapSphere hits: {nearby.Length}");
                foreach (var col in nearby)
                {
                    Player p = Singleton<GameWorld>.Instance.GetPlayerByCollider(col);
                    if (p == null) p = col.GetComponentInParent<Player>();
                    
                    if (p != null && p != mainPlayer)
                    {
                        TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($" Nearby Hit: {col.name} -> Player: {p.Profile?.Nickname}");
                        float dist = Vector3.Distance(mainPlayer.Position, p.Position);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closest = p;
                        }
                    }
                }
            }

            // Fallback para perto (OverlapSphere)
            if (closest == null)
            {
                Collider[] nearby = Physics.OverlapSphere(origin, 1.5f, mask);
                foreach (var col in nearby)
                {
                    Player p = Singleton<GameWorld>.Instance.GetPlayerByCollider(col);
                    if (p != null && p != mainPlayer)
                    {
                        float dist = Vector3.Distance(mainPlayer.Position, p.Position);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closest = p;
                        }
                    }
                }
            }

            _potentialTarget = closest;
        }

        private void ActivateMedicMode(Player p) 
        { 
            _isMedicModeActive = true; 
            _targetPatient = p; 
            BandAidUI.Instance?.ShowUI(p);
            NotificationManagerClass.DisplayMessageNotification($"MÃ‰DICO: {p.Profile.Nickname}", ENotificationDurationType.Default, ENotificationIconType.Quest); 
        }

        private void DeactivateMedicMode() 
        { 
            // SeguranÃ§a: liberar UsingMeds se o modo mÃ©dico for interrompido durante cura
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
                    try { mainPlayer.ActiveHealthController?.CancelApplyingItem(); } catch { }
                }

                _isHealingInProgress = false;
                _itemBeingUsed = null;
                MedicHealPatch.IsRedirectingHeal = false;
                MedicHealPatch.CurrentPatient = null;
                MedicHealPatch.CleanupPatientSubscription();

                // N6: Finalizar animaÃ§Ã£o para evitar travamento
                MedicHealPatch.ForceFinishAnimation();
                // G10: Desregistrar evento de morte do paciente
                try { if (_targetPatient != null) _targetPatient.OnPlayerDeadOrUnspawn -= OnPatientDiedDuringHeal; } catch { }
            }
            _isMedicModeActive = false; 
            _targetPatient = null; 
            BandAidUI.Instance?.HideUI();
        }

        /// <summary>
        /// Chamado externamente pela UI quando o paciente sai de range.
        /// </summary>
        public void DeactivateMedicModeExternal()
        {
            DeactivateMedicMode();
        }

        /// <summary>
        /// N1: Reseta todas as flags estÃ¡ticas entre raids.
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
            _potentialTarget = null;

            MedicHealPatch.IsRedirectingHeal = false;
            MedicHealPatch.CurrentPatient = null;
            MedicHealPatch.BandAidHealActive = false;
            MedicHealPatch.CleanupPatientSubscription();

            BandAidUI.Instance?.HideUI();
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo("ResetAllState: flags resetadas (mudanÃ§a de raid).");
        }

        private bool _interactFontSet = false;

        private void LateUpdate()
        {
            if (Singleton<GameWorld>.Instance == null)
            {
                if (_interactCanvasObj != null) _interactCanvasObj.SetActive(false);
                return;
            }

            // Tenta definir a fonte uma vez
            if (!_interactFontSet && BandAidUI.Instance != null)
            {
                UpdateInteractFont();
                _interactFontSet = true;
            }

            if (!_isMedicModeActive && _potentialTarget != null)
            {
                _interactText.text = $"Operador: {_potentialTarget.Profile.Nickname}\n<color=#AABBCC>[Pressione F] Examinar</color>\n<color=#889999>[Duplo F] Tocar no ombro</color>";
                _interactCanvasObj.SetActive(true);
            }
            else
            {
                if (_interactCanvasObj != null) _interactCanvasObj.SetActive(false);
            }
        }
    }
}









