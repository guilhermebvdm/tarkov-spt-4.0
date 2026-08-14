using System;
using System.Collections.Generic;
using UnityEngine;
using TRL_SpeakFromTarkov.Audio;

namespace TRL_SpeakFromTarkov.UI
{
    /// <summary>
    /// Interactive Voice Calibration Wizard Component (100% English UI).
    /// Uses Peak (P95) and Active Trough (P10) statistical analysis across 3 recording phases
    /// (Whisper, Normal, Loud) to calculate exact personal RMS sensitivity thresholds for the player.
    /// </summary>
    public class VoiceCalibrationHUD : MonoBehaviour
    {
        public VoipProcessor Processor { get; set; } = null!;
        public bool IsOpen { get; private set; } = false;

        public enum CalibrationStep
        {
            Intro = 0,
            Whisper = 1,
            Normal = 2,
            Loud = 3,
            Summary = 4
        }

        public CalibrationStep CurrentStep { get; private set; } = CalibrationStep.Intro;

        private Texture2D? _bgTex;
        private Texture2D? _panelTex;
        private Texture2D? _borderTex;
        private Texture2D? _btnTex;
        private Texture2D? _btnHoverTex;
        private Texture2D? _barBgTex;
        private Texture2D? _barFillTex;
        private Texture2D? _greenTex;
        private Texture2D? _yellowTex;
        private Texture2D? _redTex;

        // Recording & Stats
        private bool _isRecordingPhase = false;
        private readonly List<float> _samplesRecorded = new List<float>();

        // Statistically captured Peak (P95) and Trough (P10) values per voice level
        private float _whisperMin = 0.003f, _whisperMax = 0.020f;
        private float _normalMin  = 0.025f, _normalMax  = 0.080f;
        private float _loudMin    = 0.090f, _loudMax    = 0.200f;

        private float _calibratedWhisperToNormal = 0.022f; // Traço 1
        private float _calibratedNormalToLoud    = 0.085f; // Traço 2
        private float _calibratedLoudMax         = 0.200f; // Topo 100%

        public void Initialize()
        {
            _bgTex       = MakeTex(new Color(0f, 0f, 0f, 0.70f));
            _panelTex    = MakeTex(new Color(0.08f, 0.10f, 0.12f, 0.95f));
            _borderTex   = MakeTex(new Color(0.35f, 0.40f, 0.35f, 0.80f));
            _btnTex      = MakeTex(new Color(0.18f, 0.22f, 0.26f, 0.90f));
            _btnHoverTex = MakeTex(new Color(0.28f, 0.35f, 0.42f, 0.95f));
            _barBgTex    = MakeTex(new Color(0.04f, 0.05f, 0.06f, 0.90f));
            _barFillTex  = MakeTex(new Color(0.25f, 0.80f, 0.35f, 0.90f));
            _greenTex    = MakeTex(new Color(0.15f, 0.90f, 0.30f, 0.90f));
            _yellowTex   = MakeTex(new Color(0.95f, 0.85f, 0.20f, 0.90f));
            _redTex      = MakeTex(new Color(0.90f, 0.20f, 0.20f, 0.90f));

            if (VoIPPlugin.WhisperThreshold != null) _calibratedWhisperToNormal = VoIPPlugin.WhisperThreshold.Value;
            if (VoIPPlugin.NormalThreshold != null)  _calibratedNormalToLoud    = VoIPPlugin.NormalThreshold.Value;
            if (VoIPPlugin.LoudThreshold != null)    _calibratedLoudMax         = VoIPPlugin.LoudThreshold.Value;
        }

        private Texture2D MakeTex(Color c)
        {
            var t = new Texture2D(1, 1);
            t.SetPixel(0, 0, c);
            t.Apply();
            return t;
        }

        private void OnDestroy()
        {
            DestroyTex(ref _bgTex);
            DestroyTex(ref _panelTex);
            DestroyTex(ref _borderTex);
            DestroyTex(ref _btnTex);
            DestroyTex(ref _btnHoverTex);
            DestroyTex(ref _barBgTex);
            DestroyTex(ref _barFillTex);
            DestroyTex(ref _greenTex);
            DestroyTex(ref _yellowTex);
            DestroyTex(ref _redTex);
        }

        private void DestroyTex(ref Texture2D? tex)
        {
            if (tex != null)
            {
                Destroy(tex);
                tex = null;
            }
        }

        private Behaviour? _playerOwnerBehaviour;

        private static void SetGameInputBlocked(bool blocked)
        {
            try
            {
                var gpoType = System.Type.GetType("EFT.GamePlayerOwner, Assembly-CSharp");
                if (gpoType != null)
                {
                    var flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static;
                    gpoType.GetMethod("SetIgnoreInput", flags)?.Invoke(null, new object[] { blocked });
                    gpoType.GetMethod("SetIgnoreInputInNPCDialog", flags)?.Invoke(null, new object[] { blocked });
                    gpoType.GetMethod("SetIgnoreInputWithKeepResetLook", flags)?.Invoke(null, new object[] { blocked });

                    var cmdType = System.Type.GetType("EFT.InputSystem.ECommand, Assembly-CSharp");
                    if (cmdType != null)
                    {
                        string[] cmdNames = new[] { "Escape", "ToggleInventory", "ToggleShooting", "EndShooting", "Jump", "PressThrowGrenade", "ThrowGrenade", "ToggleProne", "ToggleDuck", "ReloadWeapon" };
                        var cmdList = System.Array.CreateInstance(cmdType, cmdNames.Length);
                        for (int i = 0; i < cmdNames.Length; i++)
                        {
                            cmdList.SetValue(System.Enum.Parse(cmdType, cmdNames[i]), i);
                        }

                        string methodName = blocked ? "AddIgnoreInputCommands" : "RemoveIgnoreInputCommands";
                        gpoType.GetMethod(methodName, flags)?.Invoke(null, new object[] { cmdList });
                    }
                }
            }
            catch (Exception ex)
            {
                VoIPPlugin.Log?.LogWarning($"[SFT-WIZARD] Reflection ignore input failed: {ex.Message}");
            }
        }

        public void OpenWizard()
        {
            IsOpen = true;
            CurrentStep = CalibrationStep.Intro;
            _isRecordingPhase = false;
            _samplesRecorded.Clear();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Bloqueia teclado, mouse, camera e comandos do jogador no Tarkov
            try
            {
                SetGameInputBlocked(true);

                var mainPlayer = Comfort.Common.Singleton<EFT.GameWorld>.Instance?.MainPlayer;
                if (mainPlayer != null)
                {
                    var behaviours = mainPlayer.GetComponents<Behaviour>();
                    foreach (var b in behaviours)
                    {
                        if (b != null && b.GetType().Name.Contains("PlayerOwner"))
                        {
                            _playerOwnerBehaviour = b;
                            b.enabled = false;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                VoIPPlugin.Log?.LogWarning($"[SFT-WIZARD] Nao foi possivel desativar PlayerOwner: {ex.Message}");
            }

            VoIPPlugin.Log?.LogInfo("[SFT-WIZARD] Voice Calibration Wizard opened.");
        }

        public void CloseWizard()
        {
            IsOpen = false;
            _isRecordingPhase = false;
            _samplesRecorded.Clear();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Restaura teclado, mouse, camera e comandos do jogador no Tarkov
            try
            {
                SetGameInputBlocked(false);

                if (_playerOwnerBehaviour != null)
                {
                    _playerOwnerBehaviour.enabled = true;
                    _playerOwnerBehaviour = null;
                }
            }
            catch (Exception ex)
            {
                VoIPPlugin.Log?.LogWarning($"[SFT-WIZARD] Nao foi possivel reativar PlayerOwner: {ex.Message}");
            }

            VoIPPlugin.Log?.LogInfo("[SFT-WIZARD] Voice Calibration Wizard closed.");
        }

        public void ToggleWizard()
        {
            if (IsOpen) CloseWizard();
            else OpenWizard();
        }

        private void Update()
        {
            if (!IsOpen) return;

            // ESC fecha o calibrador sem abrir o menu de pausa do Tarkov
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseWizard();
                return;
            }

            if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (_isRecordingPhase && Processor != null)
            {
                float currentRms = Processor.RawLevel;
                if (currentRms > 0.001f)
                {
                    _samplesRecorded.Add(currentRms);
                }
            }
        }

        private void OnGUI()
        {
            if (!IsOpen) return;

            GUI.depth = -1500;

            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _bgTex);

            float modalW = 520f;
            float modalH = 360f;
            float posX = (Screen.width - modalW) / 2f;
            float posY = (Screen.height - modalH) / 2f;

            GUI.DrawTexture(new Rect(posX - 2, posY - 2, modalW + 4, modalH + 4), _borderTex);
            GUI.DrawTexture(new Rect(posX, posY, modalW, modalH), _panelTex);

            GUIStyle headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.95f, 0.95f, 0.90f) }
            };

            GUIStyle subHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.70f, 0.75f, 0.70f) }
            };

            GUIStyle phraseBoxStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 13,
                fontStyle = FontStyle.Italic,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                normal = { textColor = new Color(0.90f, 0.95f, 0.85f), background = _barBgTex }
            };

            GUIStyle btnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white, background = _btnTex },
                hover = { textColor = Color.yellow, background = _btnHoverTex }
            };

            GUI.Label(new Rect(posX, posY + 15, modalW, 25), "VOICE CALIBRATION WIZARD", headerStyle);

            switch (CurrentStep)
            {
                case CalibrationStep.Intro:
                    DrawIntroScreen(posX, posY, modalW, modalH, subHeaderStyle, btnStyle);
                    break;

                case CalibrationStep.Whisper:
                    DrawRecordingStep(posX, posY, modalW, modalH, "STEP 1 OF 3: WHISPER", 
                        "\"Enemy approaching, keep absolute radio silence...\"", 
                        CalibrationStep.Normal, subHeaderStyle, phraseBoxStyle, btnStyle);
                    break;

                case CalibrationStep.Normal:
                    DrawRecordingStep(posX, posY, modalW, modalH, "STEP 2 OF 3: NORMAL VOICE", 
                        "\"Visual contact at one hundred meters, covering sector north.\"", 
                        CalibrationStep.Loud, subHeaderStyle, phraseBoxStyle, btnStyle);
                    break;

                case CalibrationStep.Loud:
                    DrawRecordingStep(posX, posY, modalW, modalH, "STEP 3 OF 3: LOUD VOICE / SHOUT", 
                        "\"Watch out, grenade in the hallway! Fall back!\"", 
                        CalibrationStep.Summary, subHeaderStyle, phraseBoxStyle, btnStyle);
                    break;

                case CalibrationStep.Summary:
                    DrawSummaryScreen(posX, posY, modalW, modalH, subHeaderStyle, btnStyle);
                    break;
            }
        }

        private void DrawIntroScreen(float posX, float posY, float modalW, float modalH, GUIStyle subStyle, GUIStyle btnStyle)
        {
            GUI.Label(new Rect(posX + 20, posY + 45, modalW - 40, 20), "Calibrate your microphone sensitivity for Whisper, Normal, and Loud voice levels.", subStyle);

            GUIStyle descStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                alignment = TextAnchor.UpperLeft,
                wordWrap = true,
                normal = { textColor = new Color(0.85f, 0.85f, 0.85f) }
            };

            string infoText = "This wizard analyzes your active voice Peak (P95) and Trough (P10) levels:\n\n" +
                              "1. Whisper (Level 1)\n" +
                              "2. Normal Voice (Level 2)\n" +
                              "3. Loud Voice / Shouting (Level 3)\n\n" +
                              "You will read tactical phrases while holding the recording button. Your exact boundary thresholds (Notch 1 & Notch 2) will be calculated and saved.";

            GUI.Label(new Rect(posX + 35, posY + 85, modalW - 70, 160), infoText, descStyle);

            if (GUI.Button(new Rect(posX + 150, posY + 280, 220, 35), "Start Calibration", btnStyle))
            {
                CurrentStep = CalibrationStep.Whisper;
                _isRecordingPhase = false;
                _samplesRecorded.Clear();
            }

            if (GUI.Button(new Rect(posX + modalW - 80, posY + 15, 65, 22), "Close", btnStyle))
            {
                CloseWizard();
            }
        }

        private void DrawRecordingStep(float posX, float posY, float modalW, float modalH, string stepTitle, string phraseText, CalibrationStep nextStep, GUIStyle subStyle, GUIStyle phraseStyle, GUIStyle btnStyle)
        {
            GUI.Label(new Rect(posX + 20, posY + 45, modalW - 40, 20), stepTitle, subStyle);

            GUIStyle instructionStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.9f, 0.9f, 0.7f) }
            };

            GUI.Label(new Rect(posX + 20, posY + 70, modalW - 40, 20), "Hold the button below and read out loud the phrase:", instructionStyle);

            GUI.Box(new Rect(posX + 40, posY + 95, modalW - 80, 55), phraseText, phraseStyle);

            float barX = posX + 40;
            float barY = posY + 165;
            float barW = modalW - 80;
            float barH = 18f;

            GUI.DrawTexture(new Rect(barX, barY, barW, barH), _barBgTex);
            float liveLevel = Processor != null ? Processor.DisplayLevel : 0f;
            float fillW = Mathf.Clamp01(liveLevel * 10f) * barW;
            if (fillW > 1f)
            {
                GUI.DrawTexture(new Rect(barX, barY, fillW, barH), _barFillTex);
            }

            GUIStyle recStatusStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = _isRecordingPhase ? Color.green : Color.gray }
            };

            string statusMsg = _isRecordingPhase 
                ? $"RECORDING... ({_samplesRecorded.Count} samples captured)" 
                : (_samplesRecorded.Count > 0 ? "Voice Sample Captured!" : "Ready to Record");
            
            GUI.Label(new Rect(posX, posY + 190, modalW, 20), statusMsg, recStatusStyle);

            Rect holdBtnRect = new Rect(posX + 130, posY + 220, 260, 38);
            Event e = Event.current;

            if (e.type == EventType.MouseDown && holdBtnRect.Contains(e.mousePosition))
            {
                _isRecordingPhase = true;
                _samplesRecorded.Clear();
            }
            else if (e.type == EventType.MouseUp && _isRecordingPhase)
            {
                _isRecordingPhase = false;
                ProcessStepResult(CurrentStep);
            }

            string btnHoldText = _isRecordingPhase ? ">>> RECORDING (RELEASE TO FINISH) <<<" : "HOLD TO RECORD VOICE SAMPLE";
            GUI.Button(holdBtnRect, btnHoldText, btnStyle);

            bool canAdvance = _samplesRecorded.Count >= 5;
            if (canAdvance)
            {
                if (GUI.Button(new Rect(posX + modalW - 160, posY + 295, 120, 32), "Next Step >>", btnStyle))
                {
                    _isRecordingPhase = false;
                    _samplesRecorded.Clear();
                    CurrentStep = nextStep;
                }
            }

            if (GUI.Button(new Rect(posX + 40, posY + 295, 100, 32), "Cancel", btnStyle))
            {
                CloseWizard();
            }
        }

        private void ProcessStepResult(CalibrationStep step)
        {
            if (_samplesRecorded.Count < 5) return;

            // Ordena as amostras para extrair os percentis P10 (Vale Ativo) e P95 (Pico Ativo)
            _samplesRecorded.Sort();

            int p10Idx = Mathf.Clamp((int)(_samplesRecorded.Count * 0.10f), 0, _samplesRecorded.Count - 1);
            int p95Idx = Mathf.Clamp((int)(_samplesRecorded.Count * 0.95f), 0, _samplesRecorded.Count - 1);

            float activeTrough = _samplesRecorded[p10Idx];
            float activePeak   = _samplesRecorded[p95Idx];

            switch (step)
            {
                case CalibrationStep.Whisper:
                    _whisperMin = activeTrough;
                    _whisperMax = activePeak;
                    break;
                case CalibrationStep.Normal:
                    _normalMin = activeTrough;
                    _normalMax = activePeak;
                    break;
                case CalibrationStep.Loud:
                    _loudMin = activeTrough;
                    _loudMax = activePeak;

                    // Ao concluir as 3 fases, calcula os pontos de corte exatos dos Traços:
                    // Traço 1 (Whisper -> Normal) = Pico da fase Sussurro
                    _calibratedWhisperToNormal = (float)Math.Round(_whisperMax, 3);
                    
                    // Traço 2 (Normal -> Loud) = Pico da fase Voz Normal
                    _calibratedNormalToLoud = (float)Math.Round(_normalMax, 3);
                    
                    // Topo 100% da Barra = Teto de escala grafica confortavel (max 0.150)
                    _calibratedLoudMax = (float)Math.Round(Mathf.Min(0.150f, Mathf.Max(_calibratedNormalToLoud + 0.020f, _loudMax)), 3);
                    if (_calibratedLoudMax < 0.150f) _calibratedLoudMax = 0.150f;
                    break;
            }

            VoIPPlugin.Log?.LogInfo($"[SFT-WIZARD] Step {step} analyzed (P10={activeTrough:F3}, P95={activePeak:F3}). Notch1 (Whisper->Normal): {_calibratedWhisperToNormal:F3}, Notch2 (Normal->Loud): {_calibratedNormalToLoud:F3}, Max: {_calibratedLoudMax:F3}");
        }

        private void DrawSummaryScreen(float posX, float posY, float modalW, float modalH, GUIStyle subStyle, GUIStyle btnStyle)
        {
            GUI.Label(new Rect(posX + 20, posY + 45, modalW - 40, 20), "CALIBRATION COMPLETE!", subStyle);

            GUIStyle summaryStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.9f, 0.95f, 0.9f) }
            };

            string summaryText = $"Calculated Boundaries (Peak & Trough Analysis):\n\n" +
                                 $"Notch 1 (Whisper -> Normal):  {_calibratedWhisperToNormal:F3} RMS\n" +
                                 $"Notch 2 (Normal -> Loud):    {_calibratedNormalToLoud:F3} RMS\n" +
                                 $"Max Ceiling (100% Top):       {_calibratedLoudMax:F3} RMS\n\n" +
                                 $"Whisper Range: [{_whisperMin:F3} - {_whisperMax:F3}] | Normal Range: [{_normalMin:F3} - {_normalMax:F3}]";

            GUI.Label(new Rect(posX + 30, posY + 75, modalW - 60, 160), summaryText, summaryStyle);

            if (GUI.Button(new Rect(posX + 70, posY + 270, 160, 36), "Save & Apply", btnStyle))
            {
                SaveAndApply();
                CloseWizard();
            }

            if (GUI.Button(new Rect(posX + 250, posY + 270, 110, 36), "Recalibrate", btnStyle))
            {
                CurrentStep = CalibrationStep.Whisper;
                _isRecordingPhase = false;
                _samplesRecorded.Clear();
            }

            if (GUI.Button(new Rect(posX + 370, posY + 270, 80, 36), "Cancel", btnStyle))
            {
                CloseWizard();
            }
        }

        private void SaveAndApply()
        {
            float vWhisperToNormal = (float)Math.Round(_calibratedWhisperToNormal, 3);
            float vNormalToLoud    = (float)Math.Round(_calibratedNormalToLoud, 3);
            float vLoudMax         = (float)Math.Round(_calibratedLoudMax, 3);
            float vVad             = (float)Math.Round(Mathf.Max(0.002f, _whisperMin), 3);

            if (VoIPPlugin.WhisperThreshold != null) VoIPPlugin.WhisperThreshold.Value = vWhisperToNormal;
            if (VoIPPlugin.NormalThreshold != null)  VoIPPlugin.NormalThreshold.Value  = vNormalToLoud;
            if (VoIPPlugin.LoudThreshold != null)    VoIPPlugin.LoudThreshold.Value    = vLoudMax;
            if (VoIPPlugin.VADThreshold != null)     VoIPPlugin.VADThreshold.Value     = vVad;

            VoIPPlugin.Log?.LogInfo($"[SFT-WIZARD] Calibrated P95/P10 boundaries saved to BepInEx! Notch1: {vWhisperToNormal:F3}, Notch2: {vNormalToLoud:F3}, Max: {vLoudMax:F3}, VAD: {vVad:F3}");
        }
    }
}
