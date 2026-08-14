using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using TRL_SpeakFromTarkov.Audio;

namespace TRL_SpeakFromTarkov.UI
{
    /// <summary>
    /// HUD de VOIP em Raid: Barra vertical fina e elegante (largura reduzida de 14px para 7px),
    /// ancorada dinamicamente ao bordo esquerdo do painel de postura/stamina vanilla (BattleStancePanel).
    /// Exibe no topo os ícones de modo de captura (PTT, VAD, OPEN) redimensionados.
    /// </summary>
    public class InRaidVoipHUD : MonoBehaviour
    {
        public VoipProcessor Processor { get; set; }
        public byte CurrentChannel { get; set; }

        private Component _battleStancePanel;
        private CanvasGroup _battleStanceCanvasGroup;
        private float _searchTimer = 0f;

        private Texture2D _bgTex;
        private Texture2D _borderTex;
        private Texture2D _greenTex;
        private Texture2D _yellowTex;
        private Texture2D _redTex;
        private Texture2D _cyanTex;
        private Texture2D _grayTex;

        // Ícones de Modo de Captura (PNG 400px escalados na UI)
        private Texture2D _pttIconTex;
        private Texture2D _vadIconTex;
        private Texture2D _openIconTex;
        private Texture2D _muteIconTex;

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

        private Texture2D LoadPNG(string filename)
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

        private void DestroyTex(ref Texture2D tex)
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
        }

        private void OnGUI()
        {
            if (VoIPPlugin.EnableInRaidVoipHUD == null || !VoIPPlugin.EnableInRaidVoipHUD.Value) return;
            if (Processor == null) return;
            if (Event.current.type != EventType.Repaint) return;

            // O nosso HUD existe ESTRITAMENTE se o HUD do jogo (BattleStancePanel) existir e estiver ativo na hierarquia
            if (_battleStancePanel == null || _battleStancePanel.gameObject == null || !_battleStancePanel.gameObject.activeInHierarchy)
            {
                return;
            }

            // Sincronização 1:1 de Opacidade com a mecânica de Autohide / Always Visible do HUD do Tarkov
            float hudAlpha = 1f;
            if (VoIPPlugin.AlwaysVisibleInRaidHUD != null && VoIPPlugin.AlwaysVisibleInRaidHUD.Value)
            {
                hudAlpha = 1f; // Opção "Manter HUD de VOIP Sempre Visível" ativada no F12
            }
            else if (_battleStanceCanvasGroup != null)
            {
                hudAlpha = _battleStanceCanvasGroup.alpha;
            }

            // Se o HUD do jogo estiver ocultado pelo autohide (opacidade zero) e "Sempre Visível" estiver desativado, o nosso HUD não aparece
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

            // 1. Ícone do Modo de Captura (PTT / VAD / OPEN / MUTE) no topo da barra
            Texture2D modeIcon = null;
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

            float iconSize = 30f; // Tamanho de 30px redimensionado suavemente pela GUI
            float iconX = posX + (barWidth / 2f) - (iconSize / 2f);
            float iconY = posY - iconSize - 4f;

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
            Texture2D statusDotTex = _redTex;
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

            // 6. Texto Miniaturizado da Frequência / Canal (Abaixo da Barra)
            string channelText = CurrentChannel == 0 ? "RAID" : CurrentChannel == 2 ? "SPEC" : $"CH{CurrentChannel}";
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 8,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.8f, 0.85f, 0.8f, 0.9f) }
            };
            GUI.Label(new Rect(posX - 10, posY + barHeight + 1, barWidth + 20, 12), channelText, labelStyle);
            GUI.color = oldGuiColor;
        }
    }
}
