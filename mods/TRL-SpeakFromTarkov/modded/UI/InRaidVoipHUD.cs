using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using TRL_SpeakFromTarkov.Audio;

namespace TRL_SpeakFromTarkov.UI
{
    public enum HudVisibilityMode
    {
        Oculto,
        SempreVisivel,
        SyncHUD,
        CaptaVoz
    }

    /// <summary>
    /// HUD de VOIP em Raid: Barra vertical fina e elegante (largura reduzida de 14px para 7px),
    /// ancorada dinamicamente ao bordo esquerdo do painel de postura/stamina vanilla (BattleStancePanel).
    /// Exibe no topo os ícones de modo de captura (PTT, VAD, OPEN) redimensionados.
    /// </summary>
    public class InRaidVoipHUD : MonoBehaviour
    {
        public VoipProcessor Processor { get; set; } = null!;
        public byte CurrentChannel { get; set; }

        private Component _battleStancePanel = null!;
        private CanvasGroup _battleStanceCanvasGroup = null!;
        private Vector2 _originalStanceAnchoredPos;
        private bool _originalPosCaptured = false;
        private float _searchTimer = 0f;
        private float _voiceHoldTimer = 0f;

        private Texture2D? _bgTex;
        private Texture2D? _borderTex;
        private Texture2D? _greenTex;
        private Texture2D? _yellowTex;
        private Texture2D? _redTex;
        private Texture2D? _cyanTex;
        private Texture2D? _grayTex;

        // Ícones de Modo de Captura (PNG 400px escalados na UI)
        private Texture2D? _pttIconTex;
        private Texture2D? _vadIconTex;
        private Texture2D? _openIconTex;
        private Texture2D? _muteIconTex;

        private float _smoothLevel = 0f;
        private float _peakMax = 0.02f;

        public void Initialize()
        {
            _bgTex     = MakeTex(new Color(0.04f, 0.06f, 0.08f, 0.85f));
            _borderTex = MakeTex(new Color(0.30f, 0.35f, 0.30f, 0.60f));
            _greenTex  = MakeTex(new Color(0.15f, 0.90f, 0.30f, 0.90f));
            _yellowTex = MakeTex(new Color(0.95f, 0.85f, 0.20f, 0.90f));
            _redTex    = MakeTex(new Color(0.90f, 0.20f, 0.20f, 0.90f));
            _cyanTex   = MakeTex(new Color(0.20f, 0.85f, 0.95f, 0.90f));
            _grayTex   = MakeTex(new Color(0.40f, 0.45f, 0.40f, 0.40f));

            // Carregamento dos ícones de modo de captura
            _pttIconTex  = LoadPNG("ptt.png");
            _vadIconTex  = LoadPNG("vad.png");
            _openIconTex = LoadPNG("open.png");
            _muteIconTex = LoadPNG("mute.png");
        }

        private Texture2D MakeTex(Color c)
        {
            var t = new Texture2D(1, 1);
            t.SetPixel(0, 0, c);
            t.Apply();
            return t;
        }

        private Texture2D? LoadPNG(string filename)
        {
            try
            {
                string dllDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string path = Path.Combine(dllDir, "assets", filename);
                if (!File.Exists(path))
                {
                    path = Path.Combine(dllDir, filename);
                }
                if (!File.Exists(path))
                {
                    // Fallback para pasta raiz do mod
                    path = Path.Combine(dllDir, "..", filename);
                }
                if (File.Exists(path))
                {
                    byte[] bytes = File.ReadAllBytes(path);
                    Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (ImageConversion.LoadImage(tex, bytes))
                    {
                        return tex;
                    }
                }
            }
            catch (Exception ex)
            {
                VoIPPlugin.Log?.LogError($"[SFT] Erro ao carregar ícone {filename}: {ex.Message}");
            }
            return null;
        }

        private void OnDestroy()
        {
            DestroyTex(ref _bgTex);
            DestroyTex(ref _borderTex);
            DestroyTex(ref _greenTex);
            DestroyTex(ref _yellowTex);
            DestroyTex(ref _redTex);
            DestroyTex(ref _cyanTex);
            DestroyTex(ref _grayTex);
            DestroyTex(ref _pttIconTex);
            DestroyTex(ref _vadIconTex);
            DestroyTex(ref _openIconTex);
            DestroyTex(ref _muteIconTex);
        }

        private void DestroyTex(ref Texture2D? tex)
        {
            if (tex != null)
            {
                Destroy(tex);
                tex = null;
            }
        }

        private void Update()
        {
            if (Processor == null) return;

            if (_battleStancePanel == null)
            {
                _searchTimer += Time.deltaTime;
                if (_searchTimer >= 2f)
                {
                    _searchTimer = 0f;
                    try
                    {
                        var comps = FindObjectsOfType<MonoBehaviour>();
                        for (int i = 0; i < comps.Length; i++)
                        {
                            if (comps[i] != null && comps[i].GetType().Name == "BattleStancePanel")
                            {
                                _battleStancePanel = comps[i];
                                _battleStanceCanvasGroup = _battleStancePanel.GetComponent<CanvasGroup>();
                                if (_battleStanceCanvasGroup == null)
                                {
                                    _battleStanceCanvasGroup = _battleStancePanel.GetComponentInParent<CanvasGroup>();
                                }
                                break;
                            }
                        }
                    }
                    catch { }
                }
            }

            // Desloca o painel de postura do jogo para a direita (eixo X) para dar respiro de tela
            if (_battleStancePanel != null && _battleStancePanel.gameObject != null)
            {
                try
                {
                    var rect = _battleStancePanel.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        if (!_originalPosCaptured)
                        {
                            _originalStanceAnchoredPos = rect.anchoredPosition;
                            _originalPosCaptured = true;
                        }

                        float shiftX = (VoIPPlugin.ShiftStancePanelX != null) ? VoIPPlugin.ShiftStancePanelX.Value : 15f;
                        Vector2 targetPos = _originalStanceAnchoredPos + new Vector2(shiftX, 0f);
                        if ((rect.anchoredPosition - targetPos).sqrMagnitude > 0.001f)
                        {
                            rect.anchoredPosition = targetPos;
                        }
                    }
                }
                catch { }
            }

            // Animação VU Meter suave
            float target = Processor.DisplayLevel;
            if (target > _smoothLevel)
                _smoothLevel = Mathf.Lerp(_smoothLevel, target, Time.deltaTime * 30f);
            else
                _smoothLevel = Mathf.Lerp(_smoothLevel, target, Time.deltaTime * 8f);

            if (Processor.PeakLevel > _peakMax)
                _peakMax = Processor.PeakLevel;
            else
                _peakMax = Mathf.Lerp(_peakMax, Mathf.Max(0.015f, _peakMax * 0.5f), Time.deltaTime / 3f);

            // Timer do modo "CaptaVoz": surge quando a voz for captada (transmitindo ou nivel de voz ativo)
            bool isCapturing = Processor != null && (Processor.IsTransmitting || Processor.DisplayLevel > 0.002f);
            if (isCapturing)
            {
                _voiceHoldTimer = 1.0f; // Mantem visivel por 1s apos o termino da fala
            }
            else if (_voiceHoldTimer > 0f)
            {
                _voiceHoldTimer -= Time.deltaTime;
            }
        }

        private void OnGUI()
        {
            if (VoIPPlugin.EnableMod != null && !VoIPPlugin.EnableMod.Value) return;
            if (VoIPPlugin.EnableInRaidVoipHUD == null || !VoIPPlugin.EnableInRaidVoipHUD.Value) return;
            if (Processor == null) return;
            if (Event.current.type != EventType.Repaint) return;

            // O nosso HUD existe ESTRITAMENTE se o HUD do jogo (BattleStancePanel) existir e estiver ativo na hierarquia
            if (_battleStancePanel == null || _battleStancePanel.gameObject == null || !_battleStancePanel.gameObject.activeInHierarchy)
            {
                return;
            }

            // Seleção de Modo de Visibilidade do HUD (Oculto, SempreVisivel, SyncHUD, CaptaVoz)
            var visibilityMode = (VoIPPlugin.HudVisibility != null) ? VoIPPlugin.HudVisibility.Value : HudVisibilityMode.SyncHUD;
            float hudAlpha = 1f;

            switch (visibilityMode)
            {
                case HudVisibilityMode.Oculto:
                    hudAlpha = 0f;
                    break;

                case HudVisibilityMode.SempreVisivel:
                    hudAlpha = 1f;
                    break;

                case HudVisibilityMode.SyncHUD:
                    if (_battleStanceCanvasGroup != null)
                    {
                        hudAlpha = _battleStanceCanvasGroup.alpha;
                    }
                    break;

                case HudVisibilityMode.CaptaVoz:
                    if (_voiceHoldTimer > 0f)
                    {
                        hudAlpha = Mathf.Clamp01(_voiceHoldTimer / 0.3f);
                    }
                    else
                    {
                        hudAlpha = 0f;
                    }
                    break;
            }

            // Se o HUD estiver oculto ou transparente, não desenha
            if (hudAlpha <= 0.01f) return;

            Color oldGuiColor = GUI.color;
            GUI.color = new Color(oldGuiColor.r, oldGuiColor.g, oldGuiColor.b, oldGuiColor.a * hudAlpha);

            // Largura reduzida pela metade (7px)
            float barWidth = 7f;
            float barHeight = 110f;
            float posX = 15f;
            float posY = Screen.height - 150f;

            var rectTransform = _battleStancePanel.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Vector3[] corners = new Vector3[4];
                rectTransform.GetWorldCorners(corners);

                float panelLeft = corners[0].x;
                float panelBottomOnScreen = Screen.height - corners[0].y;
                float panelTopOnScreen = Screen.height - corners[1].y;
                float panelH = Mathf.Clamp(panelBottomOnScreen - panelTopOnScreen, 80f, 140f);

                posX = Mathf.Max(5f, panelLeft - barWidth - 8f);
                posY = panelTopOnScreen;
                barHeight = panelH;
            }

            if (VoIPPlugin.InRaidHUDOffsetX != null) posX += VoIPPlugin.InRaidHUDOffsetX.Value;
            if (VoIPPlugin.InRaidHUDOffsetY != null) posY += VoIPPlugin.InRaidHUDOffsetY.Value;

            GUI.depth = -900;

            // 1. Ícone do Modo de Captura (PTT / VAD / OPEN / MUTE) na parte de BAIXO da barra
            Texture2D? modeIcon = null;
            if (Processor.IsMuted && _muteIconTex != null)
            {
                modeIcon = _muteIconTex;
            }
            else
            {
                switch (Processor.CurrentMode)
                {
                    case VoipProcessor.VoipMode.PTT:
                        modeIcon = _pttIconTex;
                        break;
                    case VoipProcessor.VoipMode.VAD:
                        modeIcon = _vadIconTex;
                        break;
                    case VoipProcessor.VoipMode.Open:
                        modeIcon = _openIconTex;
                        break;
                }
            }

            float iconSize = 40f; // Tamanho de 40px redimensionado suavemente pela GUI
            float iconX = posX + (barWidth / 2f) - (iconSize / 2f);
            float iconY = posY + barHeight + 4f; // Posicionado na parte de BAIXO da barra

            if (modeIcon != null)
            {
                Color prevColor = GUI.color;
                if (Processor.IsMuted)
                {
                    GUI.color = new Color(1.0f, 0.4f, 0.4f, 0.60f);
                }
                else if (Processor.IsTransmitting)
                {
                    GUI.color = Color.white;
                }
                else
                {
                    GUI.color = new Color(0.85f, 0.90f, 0.85f, 0.75f);
                }

                GUI.DrawTexture(new Rect(iconX, iconY, iconSize, iconSize), modeIcon);
                GUI.color = prevColor;
            }

            // 2. Fundo e Borda Externa Tática (Largura de 7px)
            GUI.DrawTexture(new Rect(posX - 1, posY - 1, barWidth + 2, barHeight + 2), _borderTex);
            GUI.DrawTexture(new Rect(posX, posY, barWidth, barHeight), _bgTex);

            // 3. Ponto Indicador de Status de Transmissão no Topo da Barra (3px)
            Texture2D? statusDotTex = _redTex;
            if (!Processor.IsMuted)
            {
                statusDotTex = Processor.IsTransmitting ? _greenTex : _yellowTex;
            }
            GUI.DrawTexture(new Rect(posX + 1, posY + 1, barWidth - 2, 3), statusDotTex);

            // 4. Preenchimento do VU Meter Vertical com Limiares Calibrados
            float fillAreaY = posY + 5;
            float fillAreaH = barHeight - 7;

            float whisperThresh = (VoIPPlugin.WhisperThreshold != null) ? VoIPPlugin.WhisperThreshold.Value : 0.015f;
            float normalThresh  = (VoIPPlugin.NormalThreshold != null)  ? VoIPPlugin.NormalThreshold.Value  : 0.060f;
            float loudThresh    = (VoIPPlugin.LoudThreshold != null)    ? VoIPPlugin.LoudThreshold.Value    : 0.180f;

            float maxRange = Mathf.Max(0.05f, loudThresh);
            float fill = Mathf.Clamp01(_smoothLevel / maxRange);

            float n1Pct = Mathf.Clamp01(whisperThresh / maxRange);
            float n2Pct = Mathf.Clamp01(normalThresh / maxRange);

            if (fill > 0.001f)
            {
                float currentFillH = fillAreaH * fill;
                float currentFillY = fillAreaY + (fillAreaH - currentFillH);

                Color color = !Processor.IsTransmitting
                    ? new Color(0.4f, 0.45f, 0.4f, 0.4f)
                    : (fill > n2Pct ? Color.red : fill > n1Pct ? Color.yellow : Color.green);

                var fillTex = MakeTex(color);
                GUI.DrawTexture(new Rect(posX + 1, currentFillY, barWidth - 2, currentFillH), fillTex);
                Destroy(fillTex);
            }

            // 5. Traços de Divisão dos Níveis de Voz Calibrados
            float notch1Y = fillAreaY + fillAreaH * (1f - n1Pct);
            float notch2Y = fillAreaY + fillAreaH * (1f - n2Pct);

            GUI.DrawTexture(new Rect(posX, notch1Y, barWidth, 1), _borderTex);
            GUI.DrawTexture(new Rect(posX, notch2Y, barWidth, 1), _borderTex);

            GUI.color = oldGuiColor;
        }
    }
}
