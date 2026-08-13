using System;
using UnityEngine;
using TRL_SpeakFromTarkov.Audio;

namespace TRL_SpeakFromTarkov.UI
{
    /// <summary>
    /// HUD de VOIP em Raid: Barra vertical fina e elegante no canto inferior esquerdo da tela,
    /// ancorada dinamicamente ao bordo esquerdo do painel de postura/stamina vanilla (BattleStancePanel).
    /// </summary>
    public class InRaidVoipHUD : MonoBehaviour
    {
        public VoipProcessor Processor { get; set; }
        public byte CurrentChannel { get; set; }

        private Component _battleStancePanel;
        private float _searchTimer = 0f;

        private Texture2D _bgTex;
        private Texture2D _borderTex;
        private Texture2D _greenTex;
        private Texture2D _yellowTex;
        private Texture2D _redTex;
        private Texture2D _cyanTex;
        private Texture2D _grayTex;

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
            DestroyTex(ref _borderTex);
            DestroyTex(ref _greenTex);
            DestroyTex(ref _yellowTex);
            DestroyTex(ref _redTex);
            DestroyTex(ref _cyanTex);
            DestroyTex(ref _grayTex);
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

            // Busca por reflexão/nome do tipo para não arrastar dependência de DLLs externas (OdinSerializer)
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
                                break;
                            }
                        }
                    }
                    catch { }
                }
            }

            // Animação VU Meter suave (resposta de balística de áudio)
            float target = Processor.DisplayLevel;
            if (target > _smoothLevel)
                _smoothLevel = Mathf.Lerp(_smoothLevel, target, Time.deltaTime * 30f);
            else
                _smoothLevel = Mathf.Lerp(_smoothLevel, target, Time.deltaTime * 8f);

            // Ajuste automático do pico máximo
            if (Processor.PeakLevel > _peakMax)
                _peakMax = Processor.PeakLevel;
            else
                _peakMax = Mathf.Lerp(_peakMax, Mathf.Max(0.015f, _peakMax * 0.5f), Time.deltaTime / 3f);
        }

        private void OnGUI()
        {
            // Valida se o HUD de Raid está ativado no BepInEx (F12)
            if (VoIPPlugin.EnableInRaidVoipHUD == null || !VoIPPlugin.EnableInRaidVoipHUD.Value) return;
            if (Processor == null) return;
            if (Event.current.type != EventType.Repaint) return;

            // Posição base de ancoragem no canto esquerdo
            float posX = 15f;
            float posY = Screen.height - 150f;
            float barWidth = 14f;
            float barHeight = 110f;

            // Se o BattleStancePanel vanilla estiver ativo na hierarquia, ancorar perfeitamente ao seu lado esquerdo!
            if (_battleStancePanel != null && _battleStancePanel.gameObject != null && _battleStancePanel.gameObject.activeInHierarchy)
            {
                var rectTransform = _battleStancePanel.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    Vector3[] corners = new Vector3[4];
                    rectTransform.GetWorldCorners(corners);

                    // corners[0] = bottom-left, corners[1] = top-left
                    float panelLeft = corners[0].x;
                    float panelBottomOnScreen = Screen.height - corners[0].y;
                    float panelTopOnScreen = Screen.height - corners[1].y;
                    float panelH = Mathf.Clamp(panelBottomOnScreen - panelTopOnScreen, 80f, 140f);

                    posX = Mathf.Max(5f, panelLeft - barWidth - 8f);
                    posY = panelTopOnScreen;
                    barHeight = panelH;
                }
            }

            // Aplicar offsets configurados no BepInEx F12
            if (VoIPPlugin.InRaidHUDOffsetX != null) posX += VoIPPlugin.InRaidHUDOffsetX.Value;
            if (VoIPPlugin.InRaidHUDOffsetY != null) posY += VoIPPlugin.InRaidHUDOffsetY.Value;

            GUI.depth = -900;

            // 1. Fundo e Borda Externa Tática
            GUI.DrawTexture(new Rect(posX - 1, posY - 1, barWidth + 2, barHeight + 2), _borderTex);
            GUI.DrawTexture(new Rect(posX, posY, barWidth, barHeight), _bgTex);

            // 2. Indicador do Ponto de Status no Topo (3px de altura)
            Texture2D statusDotTex = _redTex;
            if (!Processor.IsMuted)
            {
                statusDotTex = Processor.IsTransmitting ? _greenTex : _yellowTex;
            }

            GUI.DrawTexture(new Rect(posX + 2, posY + 2, barWidth - 4, 3), statusDotTex);

            // 3. Preenchimento do VU Meter Vertical com Limiares Calibrados
            float fillAreaY = posY + 7;
            float fillAreaH = barHeight - 9;

            float whisperThresh = (VoIPPlugin.WhisperThreshold != null) ? VoIPPlugin.WhisperThreshold.Value : 0.015f;
            float normalThresh  = (VoIPPlugin.NormalThreshold != null)  ? VoIPPlugin.NormalThreshold.Value  : 0.060f;
            float loudThresh    = (VoIPPlugin.LoudThreshold != null)    ? VoIPPlugin.LoudThreshold.Value    : 0.180f;

            float maxRange = Mathf.Max(0.05f, loudThresh);
            float fill = Mathf.Clamp01(_smoothLevel / maxRange);

            float n1Pct = Mathf.Clamp(whisperThresh / maxRange, 0.15f, 0.40f);
            float n2Pct = Mathf.Clamp(normalThresh / maxRange, 0.45f, 0.80f);

            if (fill > 0.001f)
            {
                float currentFillH = fillAreaH * fill;
                float currentFillY = fillAreaY + (fillAreaH - currentFillH);

                Color color = !Processor.IsTransmitting
                    ? new Color(0.4f, 0.45f, 0.4f, 0.4f)
                    : (fill > n2Pct ? Color.red : fill > n1Pct ? Color.yellow : Color.green);

                var fillTex = MakeTex(color);
                GUI.DrawTexture(new Rect(posX + 2, currentFillY, barWidth - 4, currentFillH), fillTex);
                Destroy(fillTex);
            }

            // 4. Traços de Divisão dos Níveis de Voz Calibrados (Sussurro, Normal, Grito)
            float notch1Y = fillAreaY + fillAreaH * (1f - n1Pct);
            float notch2Y = fillAreaY + fillAreaH * (1f - n2Pct);

            GUI.DrawTexture(new Rect(posX + 1, notch1Y, barWidth - 2, 1), _borderTex);
            GUI.DrawTexture(new Rect(posX + 1, notch2Y, barWidth - 2, 1), _borderTex);

            // 5. Texto Miniaturizado da Frequência / Canal (Abaixo da Barra)
            string channelText = CurrentChannel == 0 ? "RAID" : CurrentChannel == 2 ? "SPEC" : $"CH{CurrentChannel}";
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 8,
                normal = { textColor = new Color(0.8f, 0.85f, 0.8f, 0.9f) }
            };
            GUI.Label(new Rect(posX - 8, posY + barHeight + 1, barWidth + 16, 12), channelText, labelStyle);
        }
    }
}
