using System;
using System.Collections.Generic;
using UnityEngine;
using TRL_SpeakFromTarkov.Audio;

namespace TRL_SpeakFromTarkov.UI
{
    /// <summary>
    /// Interactive Voice Calibration Wizard Component (100% English UI).
    /// Guides the player through 3 voice recording phases (Whisper, Normal, Loud)
    /// to calculate exact personal RMS sensitivity thresholds for their microphone.
    /// </summary>
    public class VoiceCalibrationHUD : MonoBehaviour
    {
        public VoipProcessor Processor { get; set; }
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

        private Texture2D _bgTex;
        private Texture2D _panelTex;
        private Texture2D _borderTex;
        private Texture2D _btnTex;
        private Texture2D _btnHoverTex;
        private Texture2D _barBgTex;
        private Texture2D _barFillTex;
        private Texture2D _greenTex;
        private Texture2D _yellowTex;
        private Texture2D _redTex;

        // Recording & Stats
        private bool _isRecordingPhase = false;
        private float _recordingTimer = 0f;
        private readonly List<float> _samplesRecorded = new List<float>();

        private float _calibratedWhisper = 0.015f;
        private float _calibratedNormal  = 0.060f;
        private float _calibratedLoud    = 0.180f;

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

            if (VoIPPlugin.WhisperThreshold != null) _calibratedWhisper = VoIPPlugin.WhisperThreshold.Value;
            if (VoIPPlugin.NormalThreshold != null)  _calibratedNormal  = VoIPPlugin.NormalThreshold.Value;
            if (VoIPPlugin.LoudThreshold != null)    _calibratedLoud    = VoIPPlugin.LoudThreshold.Value;
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

        private void DestroyTex(ref Texture2D tex)
        {
            if (tex != null)
            {
                Destroy(tex);
                tex = null;
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

            VoIPPlugin.Log?.LogInfo("[SFT-WIZARD] Voice Calibration Wizard opened.");
        }

        public void CloseWizard()
        {
            IsOpen = false;
            _isRecordingPhase = false;
            _samplesRecorded.Clear();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

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

            // Recarrega o ponteiro do cursor para manter o mouse ativo na interface
            if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            // Coleta de amostras RMS durante a gravação ativa
            if (_isRecordingPhase && Processor != null)
            {
                _recordingTimer += Time.deltaTime;
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

            // 1. Overlay de fundo escuro
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _bgTex);

            // 2. Modal Centralizado (520px x 360px)
            float modalW = 520f;
            float modalH = 360f;
            float posX = (Screen.width - modalW) / 2f;
            float posY = (Screen.height - modalH) / 2f;

            GUI.DrawTexture(new Rect(posX - 2, posY - 2, modalW + 4, modalH + 4), _borderTex);
            GUI.DrawTexture(new Rect(posX, posY, modalW, modalH), _panelTex);

            // Estilos de Texto em Inglês
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

            // Título Principal
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
            GUI.Label(new Rect(posX + 20, posY + 50, modalW - 40, 20), "Calibrate your microphone sensitivity for Whisper, Normal, and Loud voice levels.", subStyle);

            GUIStyle descStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                alignment = TextAnchor.UpperLeft,
                wordWrap = true,
                normal = { textColor = new Color(0.85f, 0.85f, 0.85f) }
            };

            string infoText = "This wizard will guide you through 3 voice recording steps:\n\n" +
                              "1. Whisper (Level 1)\n" +
                              "2. Normal Voice (Level 2)\n" +
                              "3. Loud Voice / Shouting (Level 3)\n\n" +
                              "You will read short tactical phrases while holding the recording button. Your personal threshold values will be automatically saved to BepInEx.";

            GUI.Label(new Rect(posX + 35, posY + 90, modalW - 70, 160), infoText, descStyle);

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

            // Caixa com a Frase em Inglês
            GUI.Box(new Rect(posX + 40, posY + 95, modalW - 80, 55), phraseText, phraseStyle);

            // VU Meter de Gravação ao Vivo
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

            // Feedback visual do status de gravação
            GUIStyle recStatusStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = _isRecordingPhase ? Color.green : Color.gray }
            };

            string statusMsg = _isRecordingPhase 
                ? $"RECORDING VOICE... ({_samplesRecorded.Count} samples captured)" 
                : (_samplesRecorded.Count > 0 ? "Voice Sample Captured!" : "Ready to Record");
            
            GUI.Label(new Rect(posX, posY + 190, modalW, 20), statusMsg, recStatusStyle);

            // Botão "Segurar para Gravar"
            Rect holdBtnRect = new Rect(posX + 130, posY + 220, 260, 38);
            Event e = Event.current;

            if (e.type == EventType.MouseDown && holdBtnRect.Contains(e.mousePosition))
            {
                _isRecordingPhase = true;
                _recordingTimer = 0f;
                _samplesRecorded.Clear();
            }
            else if (e.type == EventType.MouseUp && _isRecordingPhase)
            {
                _isRecordingPhase = false;
                ProcessStepResult(CurrentStep);
            }

            string btnHoldText = _isRecordingPhase ? ">>> RECORDING (RELEASE TO FINISH) <<<" : "HOLD TO RECORD VOICE SAMPLE";
            GUI.Button(holdBtnRect, btnHoldText, btnStyle);

            // Botões de Navegação ("Next Step" / "Cancel")
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
            if (_samplesRecorded.Count == 0) return;

            float sum = 0f;
            float max = 0f;
            foreach (float s in _samplesRecorded)
            {
                sum += s;
                if (s > max) max = s;
            }
            float avg = sum / _samplesRecorded.Count;

            switch (step)
            {
                case CalibrationStep.Whisper:
                    _calibratedWhisper = Mathf.Max(0.005f, avg);
                    break;
                case CalibrationStep.Normal:
                    _calibratedNormal = Mathf.Max(_calibratedWhisper + 0.010f, avg);
                    break;
                case CalibrationStep.Loud:
                    _calibratedLoud = Mathf.Max(_calibratedNormal + 0.020f, max);
                    break;
            }

            VoIPPlugin.Log?.LogInfo($"[SFT-WIZARD] Step {step} recorded. Result -> Whisper: {_calibratedWhisper:F4}, Normal: {_calibratedNormal:F4}, Loud: {_calibratedLoud:F4}");
        }

        private void DrawSummaryScreen(float posX, float posY, float modalW, float modalH, GUIStyle subStyle, GUIStyle btnStyle)
        {
            GUI.Label(new Rect(posX + 20, posY + 45, modalW - 40, 20), "CALIBRATION COMPLETE!", subStyle);

            GUIStyle summaryStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.9f, 0.95f, 0.9f) }
            };

            string summaryText = $"Calculated Microphone Thresholds:\n\n" +
                                 $"Level 1 (Whisper):  {_calibratedWhisper:F4} RMS\n" +
                                 $"Level 2 (Normal):   {_calibratedNormal:F4} RMS\n" +
                                 $"Level 3 (Loud):     {_calibratedLoud:F4} RMS\n\n" +
                                 $"Click 'Save & Apply' to update your BepInEx settings.";

            GUI.Label(new Rect(posX + 40, posY + 85, modalW - 80, 150), summaryText, summaryStyle);

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
            if (VoIPPlugin.WhisperThreshold != null) VoIPPlugin.WhisperThreshold.Value = _calibratedWhisper;
            if (VoIPPlugin.NormalThreshold != null)  VoIPPlugin.NormalThreshold.Value  = _calibratedNormal;
            if (VoIPPlugin.LoudThreshold != null)    VoIPPlugin.LoudThreshold.Value    = _calibratedLoud;

            VoIPPlugin.Log?.LogInfo($"[SFT-WIZARD] Calibrated thresholds saved to BepInEx! Whisper: {_calibratedWhisper:F4}, Normal: {_calibratedNormal:F4}, Loud: {_calibratedLoud:F4}");
        }
    }
}
