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
using UnityEngine.UI;

namespace Band_Aid
{
    public enum EBandAidPressMode
    {
        Press,      // Aperta e solta
        Hold,       // Segura por um tempo
        DoubleTap   // Aperta duas vezes rápido
    }

    [BepInPlugin("com.umbigopreto.bandaid", "Band-Aid", "2.5.0")]
    public class BandAidPlugin : BaseUnityPlugin
    {
        public static BandAidPlugin Instance;
        private bool _isMedicModeActive = false;
        private Player _targetPatient = null;
        private Player _potentialTarget = null;

        // HUD de interação (Canvas UI)
        private GameObject _interactCanvasObj;
        private Text _interactText;
        private Image _interactBg;

        // Controle da Animação
        private bool _isHealingInProgress = false;
        private Item _itemBeingUsed = null;
        private Coroutine _activeHealCoroutine = null;

        // N1: Detectar mudança de raid para resetar flags estáticas
        private GameWorld _lastGameWorld = null;

        // Harmony
        private Harmony _harmony;

        // Config — Teclas
        private static ConfigEntry<KeyboardShortcut> _medicInteractKey;
        private static ConfigEntry<KeyboardShortcut> _emergencyDropKey;
        private static ConfigEntry<KeyboardShortcut> _shoulderTapKey;
        // Config — Modos de pressão
        private static ConfigEntry<EBandAidPressMode> _medicInteractMode;
        private static ConfigEntry<EBandAidPressMode> _emergencyDropMode;
        private static ConfigEntry<EBandAidPressMode> _shoulderTapMode;

        // Detecção de Hold
        private float _medicHoldTimer = 0f;
        private bool _medicHoldTriggered = false;
        private float _emergencyHoldTimer = 0f;
        private bool _emergencyHoldTriggered = false;
        private float _shoulderHoldTimer = 0f;
        private bool _shoulderHoldTriggered = false;
        private const float HOLD_THRESHOLD = 0.4f; // segundos para Hold

        // Detecção de DoubleTap
        private float _medicLastTapTime = -1f;
        private float _emergencyLastTapTime = -1f;
        private float _shoulderLastTapTime = -1f;
        private const float DOUBLE_TAP_WINDOW = 0.35f; // janela para double tap

        private void Awake()
        {
            Instance = this;
            Logger.LogInfo("Band-Aid Plugin v2.5 carregado. Animação Nativa + HUD.");
            ItemDatabase.Initialize();

            // Harmony patch
            _harmony = new Harmony("com.umbigopreto.bandaid");
            _harmony.PatchAll();
            Logger.LogInfo("Harmony patches aplicados.");

            // Config — Teclas
            _medicInteractKey = Config.Bind("Keybinds", "Medic Interact Key", new KeyboardShortcut(KeyCode.F),
                "Tecla para interagir com paciente (ativar/desativar modo médico).");
            _emergencyDropKey = Config.Bind("Keybinds", "Emergency Drop Key", new KeyboardShortcut(KeyCode.F),
                "Tecla para drop emergencial do item durante animação de cura.");
            _shoulderTapKey = Config.Bind("Keybinds", "Shoulder Tap Key", new KeyboardShortcut(KeyCode.F),
                "Tecla para toque no ombro (aviso CQB ao aliado próximo).");

            // Config — Modos
            _medicInteractMode = Config.Bind("Keybinds", "Medic Interact Mode", EBandAidPressMode.Hold,
                "Modo de ativação: Press (aperta e solta), Hold (segura), DoubleTap (aperta 2x).");
            _emergencyDropMode = Config.Bind("Keybinds", "Emergency Drop Mode", EBandAidPressMode.Press,
                "Modo de ativação do drop emergencial.");
            _shoulderTapMode = Config.Bind("Keybinds", "Shoulder Tap Mode", EBandAidPressMode.DoubleTap,
                "Modo de ativação do toque no ombro.");

            // Inicializar carregador de imagens
            string pluginPath = System.IO.Path.GetDirectoryName(Info.Location);
            ImageLoader.Init(pluginPath);

            var uiHost = new GameObject("BandAidUI_Host");
            DontDestroyOnLoad(uiHost);
            uiHost.AddComponent<BandAidUI>();
            // TORNIQUETE DESATIVADO — manter vanilla por enquanto
            // uiHost.AddComponent<TourniquetManager>();

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
                Logger.LogInfo($"Handshake aprovado! Iniciando cura com {_pendingHealStats?.Name}.");
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
                Logger.LogInfo($"Handshake negado: {response.DenyReason}");
            }

            // Limpar estado pendente
            _pendingHealItem = null;
            _pendingHealStats = null;
            _pendingHealPatient = null;
        }

        // === HUD DE INTERAÇÃO (Canvas UI) ===
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
            // Em servidores dedicados (Headless), o MainPlayer é null. Se pularmos, os pacotes nunca são registrados.
            BandAidNetworkHandler.CheckInit();

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

            ScanForPatient();

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
                Logger.LogWarning("Mouse0 pressionado durante cura — cancelando tratamento (sem drop).");
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
                if (!_targetPatient.HealthController.IsAlive || Vector3.Distance(Singleton<GameWorld>.Instance.MainPlayer.Position, _targetPatient.Position) > 2f)
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
                        return; // Só processa um slot por frame
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"CheckManualInputs: {ex.Message}");
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
                    // === PACIENTE LOCAL (bot/self): Validação imediata ===
                    if (!MedicalLogic.CanUseItem(_targetPatient, stats))
                    {
                        NotificationManagerClass.DisplayMessageNotification(
                            $"{stats.Name}: Sem ferimento compatível.", ENotificationDurationType.Default, ENotificationIconType.Alert);
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
                $"Toque no ombro → {target.Profile.Nickname}", 
                ENotificationDurationType.Default, ENotificationIconType.Quest);

            // Tocar gesto de mão "There" (apontar com a mão)
            var doctor = Singleton<GameWorld>.Instance?.MainPlayer;
            doctor?.MovementContext.SetInteractInHands(EInteraction.ThereGesture);

            // Enviar via rede para o aliado ver a mensagem
            BandAidNetworkHandler.SendShoulderTapPacket(target);
            Logger.LogInfo($"Toque no ombro enviado para {target.Profile.Nickname}");
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
                Logger.LogWarning($"EmergencyDrop MatarAnimação: {ex.Message}");
            }

            // === PASSO 3: LIMPAR AS MÃOS (agora sem animação de fechar kit) ===
            try
            {
                var spawnController = doctor.GetType().GetMethod("SpawnController",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (spawnController != null)
                    spawnController.Invoke(doctor, null);

                doctor.TrySetLastEquippedWeapon(true);
                Logger.LogInfo("EmergencyDrop: Mãos limpas (HANB)");
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"EmergencyDrop Limpar Mãos: {ex.Message}");
            }

            // === PASSO 4: DROPAR ITEM SALVO ===
            if (savedItem != null)
            {
                try
                {
                    doctor.InventoryController.ThrowItem(savedItem);
                    Logger.LogInfo($"Drop emergencial: {itemName} dropado.");
                }
                catch (Exception ex)
                {
                    Logger.LogWarning($"Erro ao dropar {itemName}: {ex.Message}");
                }
            }

            // === LIMPEZA DE MEMÓRIA ===
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
            Logger.LogWarning($"Paciente {player?.Profile?.Nickname} morreu durante cura!");
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

            NotificationManagerClass.DisplayMessageNotification($"Aplicando {itemUsed.ShortName.Localized()}...", ENotificationDurationType.Default, ENotificationIconType.Quest);

            // === ATIVAR REDIRECT ===
            MedicHealPatch.IsRedirectingHeal = true;
            MedicHealPatch.CurrentPatient = patient;
            MedicHealPatch.RedirectStartTime = UnityEngine.Time.time;

            Logger.LogWarning($"🔍 HealRoutine: REDIRECT ATIVADO | item={itemUsed.ShortName.Localized()} | UseTime={stats.UseTime}s | patient={patient.Profile.Nickname}");

            // Colocar o item médico nas mãos (aciona a animação visual)
            try
            {
                doctor.SetInHands(itemUsed, (result) => { });
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"SetInHands NullRef ignorado: {ex.Message}");
            }

            // Espera o tempo de uso do item (+2s para animação completar visualmente)
            float totalUseTime = stats.UseTime + 2f;
            yield return new WaitForSeconds(totalUseTime);

            // G2: Guard — médico morreu durante wait?
            if (doctor == null || !doctor.HealthController.IsAlive)
            {
                Logger.LogWarning("HealRoutine: Médico morreu durante cura, abortando.");
                CleanupHealState(patient);
                yield break;
            }

            // G10: Paciente morreu durante wait?
            if (_patientDiedDuringHeal || patient == null || !patient.HealthController.IsAlive)
            {
                Logger.LogWarning("HealRoutine: Paciente morreu durante cura, abortando.");
                CleanupHealState(patient);
                // Resetar mãos do médico que ainda está vivo
                try { doctor.MovementContext.SetPhysicalCondition(EPhysicalCondition.UsingMeds, false); } catch { }
                MedicHealPatch.ForceFinishAnimation();
                yield break;
            }

            Logger.LogWarning($"🔍 HealRoutine: UseTime TERMINOU | IsRedirecting={MedicHealPatch.IsRedirectingHeal} | T+{stats.UseTime}s");

            // === LIMPEZA DO REDIRECT ===
            MedicHealPatch.IsRedirectingHeal = false;
            MedicHealPatch.CurrentPatient = null;
            MedicHealPatch.CleanupPatientSubscription();

            // G3: Removido RemoveMedEffect no médico (efeito está no paciente, não no médico)
            // ForceFinishAnimation chama method_9 diretamente → cleanup → callback → jogo transiciona
            MedicHealPatch.ForceFinishAnimation();

            // Liberar movimento do médico
            doctor.MovementContext.SetPhysicalCondition(EPhysicalCondition.UsingMeds, false);

            _isHealingInProgress = false;
            _itemBeingUsed = null;

            // G10: Desregistrar morte do paciente
            try { patient.OnPlayerDeadOrUnspawn -= OnPatientDiedDuringHeal; } catch { }

            // N3: Verificar se o item e o paciente ainda existem após o UseTime
            if (patient != null && patient.HealthController.IsAlive && itemUsed != null)
            {
                try
                {
                    // Verificar se o item ainda é válido (não foi destruído/lootado)
                    var _ = itemUsed.TemplateId;
                    MedicalLogic.ApplyTreatment(doctor, patient, itemUsed, stats);
                    NotificationManagerClass.DisplayMessageNotification("Tratamento Completo.", ENotificationDurationType.Long, ENotificationIconType.Quest);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning($"HealRoutine: Item destruído durante cura — {ex.Message}");
                    NotificationManagerClass.DisplayMessageNotification("Item perdido durante tratamento.", ENotificationDurationType.Default, ENotificationIconType.Alert);
                }
            }
            else
            {
                Logger.LogWarning("HealRoutine: Paciente null/morto ou item null, tratamento não aplicado.");
            }

            // === RESET DAS MÃOS (sem forçar puxada de arma — o jogo já faz automaticamente) ===
        }

        /// <summary>
        /// Limpa estado de cura (usado quando médico ou paciente morre durante HealRoutine).
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

            // Cancelar efeito médico nativo (como vanilla)
            try { doctor.ActiveHealthController?.CancelApplyingItem(); } catch { }

            // Finalizar animação
            MedicHealPatch.ForceFinishAnimation();

            // Liberar movimento
            try { doctor.MovementContext.SetPhysicalCondition(EPhysicalCondition.UsingMeds, false); } catch { }

            NotificationManagerClass.DisplayMessageNotification("Tratamento cancelado.", ENotificationDurationType.Default, ENotificationIconType.Alert);
            Logger.LogInfo("CancelHealInProgress: cura cancelada (Mouse0).");
        }

        private void ScanForPatient()
        {
            if (_isMedicModeActive) return;
            var camera = Camera.main;
            if (camera == null) return;
            var mainPlayer = Singleton<GameWorld>.Instance.MainPlayer;
            if (mainPlayer == null) return;

            LayerMask mask = LayerMask.GetMask("Player", "HighPolyCollider", "HitCollider");
            Ray ray = new Ray(camera.transform.position, camera.transform.forward);

            // === TENTATIVA 1: SphereCast (raio 0.8f, distância 1.5f) ===
            if (Physics.SphereCast(ray, 0.8f, out RaycastHit hit, 1.5f, mask))
            {
                Player p = Singleton<GameWorld>.Instance.GetPlayerByCollider(hit.collider);
                if (p != null && p.HealthController.IsAlive && p != mainPlayer)
                {
                    _potentialTarget = p;
                    return;
                }
            }

            // === TENTATIVA 2: OverlapSphere (fallback para distância muito curta) ===
            // SphereCast NÃO detecta colliders quando a origem já está DENTRO deles.
            // Quando a câmera "entra" no corpo do paciente, o ray passa direto.
            // OverlapSphere detecta colliders sobrepostos ao ponto de origem.
            Collider[] nearby = Physics.OverlapSphere(camera.transform.position, 1.5f, mask);
            Player closest = null;
            float closestDist = float.MaxValue;
            foreach (var col in nearby)
            {
                Player p = Singleton<GameWorld>.Instance.GetPlayerByCollider(col);
                if (p != null && p.HealthController.IsAlive && p != mainPlayer)
                {
                    float dist = Vector3.Distance(mainPlayer.Position, p.Position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = p;
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
            NotificationManagerClass.DisplayMessageNotification($"MÉDICO: {p.Profile.Nickname}", ENotificationDurationType.Default, ENotificationIconType.Quest); 
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
                    try { mainPlayer.ActiveHealthController?.CancelApplyingItem(); } catch { }
                }

                _isHealingInProgress = false;
                _itemBeingUsed = null;
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
            _potentialTarget = null;

            MedicHealPatch.IsRedirectingHeal = false;
            MedicHealPatch.CurrentPatient = null;
            MedicHealPatch.BandAidHealActive = false;
            MedicHealPatch.CleanupPatientSubscription();

            BandAidUI.Instance?.HideUI();
            Logger.LogInfo("ResetAllState: flags resetadas (mudança de raid).");
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