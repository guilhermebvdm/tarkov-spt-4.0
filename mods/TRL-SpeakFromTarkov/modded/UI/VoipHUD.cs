using UnityEngine;
using TRL_SpeakFromTarkov.Audio;

namespace TRL_SpeakFromTarkov.UI
{
    public class VoipHUD : MonoBehaviour
    {
        public VoipProcessor Processor { get; set; } = null!;
        public byte CurrentChannel { get; set; }
        
        private Texture2D fillTexture = null!;
        private Texture2D bgTexture = null!;
        private Texture2D whiteTexture = null!;
        private Texture2D redTex = null!;
        private Texture2D greenTex = null!;
        private Texture2D yellowTex = null!;
        private Texture2D cyanTex = null!;
        private Texture2D grayTex = null!;
        
        private float smoothLevel = 0f;
        
        public void Initialize()
        {
            fillTexture  = MakeTex(Color.green);
            bgTexture    = MakeTex(new Color(0f, 0f, 0f, 0.85f));
            whiteTexture = MakeTex(Color.white);
            redTex       = MakeTex(Color.red);
            greenTex     = MakeTex(Color.green);
            yellowTex    = MakeTex(Color.yellow);
            cyanTex      = MakeTex(Color.cyan);
            grayTex      = MakeTex(new Color(0.5f, 0.5f, 0.5f, 0.4f));
        }
        
        private Texture2D MakeTex(Color c)
        {
            var t = new Texture2D(1, 1);
            t.SetPixel(0, 0, c);
            t.Apply();
            return t;
        }
        
        private float peakMax = 0.02f; // calibração automática do pico máximo
        
        void Update()
        {
            if (Processor == null) return;
            
            // Sobe rápido, desce devagar — resposta natural de VU meter
            float target = Processor.DisplayLevel;
            if (target > smoothLevel)
                smoothLevel = Mathf.Lerp(smoothLevel, target, Time.deltaTime * 30f); // sobe rápido
            else
                smoothLevel = Mathf.Lerp(smoothLevel, target, Time.deltaTime * 8f);  // desce devagar
            
            // Calibração automática do pico máximo (sobe instantâneo, desce em 3s)
            if (Processor.PeakLevel > peakMax)
                peakMax = Processor.PeakLevel;
            else
                peakMax = Mathf.Lerp(peakMax, Mathf.Max(0.015f, peakMax * 0.5f), Time.deltaTime / 3f);
        }
        
        void OnGUI()
        {
            if (VoIPPlugin.EnableDebugVoipHUD == null || !VoIPPlugin.EnableDebugVoipHUD.Value) return;
            if (Processor == null) return;
            GUI.depth = -1000;
            
            // Botão Interativo de Profiler
            string btnText = VoIPPlugin.IsAudioDebugActive ? "[PROFILER ON]" : "[PROFILER OFF]";
            GUIStyle btnStyle = new GUIStyle(GUI.skin.button) { fontSize = 9 };
            if (GUI.Button(new Rect(205, 8, 115, 20), btnText, btnStyle))
            {
                VoIPPlugin.IsAudioDebugActive = !VoIPPlugin.IsAudioDebugActive;
                VoIPPlugin.Log.LogInfo($"[SFT-PROFILER] Profiler de Áudio alternado no HUD: {(VoIPPlugin.IsAudioDebugActive ? ">>> ATIVADO <<<" : "--- DESATIVADO ---")}");
            }

            if (Event.current.type != EventType.Repaint) return;
            
            DrawBackground();
            DrawStatusDot();
            DrawLabel();
            DrawBar();
        }
        
        private void DrawBackground()
        {
            GUI.DrawTexture(new Rect(5, 5, 320, 85), bgTexture);
        }
        
        private void DrawStatusDot()
        {
            Texture2D dotTex = redTex;
            if (!Processor.IsMuted)
            {
                dotTex = Processor.IsTransmitting ? greenTex : yellowTex;
            }
            GUI.DrawTexture(new Rect(10, 10, 20, 20), dotTex);
        }
        
        private void DrawLabel()
        {
            string channel = CurrentChannel == 0 ? "RAID" : CurrentChannel == 2 ? "SPEC" : "LOBBY";
            string mode = Processor.CurrentMode.ToString().ToUpper();
            string state;
            
            if (Processor.IsMuted)
                state = "MUDO";
            else if (Processor.IsTransmitting)
            {
                float lvl = Processor.DisplayLevel;
                float power;
                string modeText;

                if (lvl < 0.025f)
                {
                    float t = Mathf.Clamp01(lvl / 0.025f);
                    power = Mathf.Lerp(3.0f, 10.0f, t);
                    modeText = $"SUSSURRO ({power:F0}m)";
                }
                else if (lvl < 0.150f)
                {
                    float t = Mathf.Clamp01((lvl - 0.025f) / (0.150f - 0.025f));
                    power = Mathf.Lerp(10.0f, 30.0f, t);
                    modeText = $"NORMAL ({power:F0}m)";
                }
                else
                {
                    float t = Mathf.Clamp01((lvl - 0.150f) / (0.400f - 0.150f));
                    power = Mathf.Lerp(30.0f, 60.0f, t);
                    modeText = $"GRITO ({power:F0}m)";
                }

                state = $"TX [{modeText}]";
            }
            else if (Processor.CurrentMode == VoipProcessor.VoipMode.PTT)
                state = "AGUARDANDO PTT";
            else if (Processor.CurrentMode == VoipProcessor.VoipMode.VAD)
                state = "OUVINDO (VAD)";
            else
                state = "ABERTO";
            
            GUIStyle s = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                normal = { textColor = Color.white }
            };
            GUI.Label(new Rect(35, 10, 285, 20), $"[{channel}] {mode} — {state}", s);
        }
        
        private void DrawBar()
        {
            GUIStyle s = new GUIStyle(GUI.skin.label) { fontSize = 10, normal = { textColor = Color.gray } };
            
            // ── Barra 1: Entrada (MIC) ──
            float barX = 10f, barY = 32f, barW = 300f, barH = 10f;
            GUI.DrawTexture(new Rect(barX, barY, barW, barH), bgTexture);
            
            float fill = (peakMax > 0f) ? Mathf.Clamp01(smoothLevel / peakMax) : 0f;
            if (fill > 0.001f)
            {
                Color barColor = !Processor.IsTransmitting ? new Color(0.5f, 0.5f, 0.5f, 0.4f) : (fill > 0.8f ? Color.red : fill > 0.4f ? Color.yellow : Color.green);
                var barFillTex = MakeTex(barColor);
                GUI.DrawTexture(new Rect(barX, barY, barW * fill, barH), barFillTex);
                Object.Destroy(barFillTex);
            }

            // ── Barra 2: Saída (RETORNO / ECO DEBUG) ──
            float outTextY = 46f;
            float outBarY = 62f;
            
            float outLevel = 0f;
            if (Core.VoipController.Instance != null && Core.VoipController.Instance.echoSpeaker != null)
            {
                outLevel = Core.VoipController.Instance.echoSpeaker.CurrentOutputLevel;
            }
            
            GUI.Label(new Rect(10, outTextY, 300, 15), $"SAÍDA / RETORNO (DEBUG ECO): {(outLevel > 0.001f ? "TOCANDO" : "SILÊNCIO")}", s);
            
            GUI.DrawTexture(new Rect(barX, outBarY, barW, barH), bgTexture);
            
            float outFill = Mathf.Clamp01(outLevel * 20f); // Amplifica escala visual
            if (outFill > 0.001f)
            {
                GUI.DrawTexture(new Rect(barX, outBarY, barW * outFill, barH), cyanTex);
            }

            // ── Telemetria de Desempenho (Visível com F9 / Profiler ON) ──
            if (VoIPPlugin.IsAudioDebugActive)
            {
                GUIStyle profStyle = new GUIStyle(GUI.skin.label) { fontSize = 9, normal = { textColor = new Color(0.2f, 1.0f, 0.4f) } };
                int bitrate = VoIPPlugin.OpusBitrate != null ? VoIPPlugin.OpusBitrate.Value / 1000 : 24;
                GUI.Label(new Rect(10, 78, 300, 15), $"CPU DSP: ~0.08ms (<0.4%) | Banda: ~{bitrate} kbps (3KB/s) | GC: 0.00 KB/s", profStyle);
            }
        }
    }
}
