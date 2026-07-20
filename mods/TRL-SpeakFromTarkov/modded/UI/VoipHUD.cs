using UnityEngine;
using TRL_SpeakFromTarkov.Audio;

namespace TRL_SpeakFromTarkov.UI
{
    public class VoipHUD : MonoBehaviour
    {
        public VoipProcessor Processor { get; set; }
        public byte CurrentChannel { get; set; }
        
        private Texture2D fillTexture;
        private Texture2D bgTexture;
        private Texture2D whiteTexture;
        
        private float smoothLevel = 0f;
        
        // Diagnóstico: log do DisplayLevel a cada 2s
        private float debugLogTimer = 0f;
        
        public void Initialize()
        {
            fillTexture  = MakeTex(Color.green);
            bgTexture    = MakeTex(new Color(0f, 0f, 0f, 0.75f));
            whiteTexture = MakeTex(Color.white);
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
            // Renderiza apenas na fase de repaint para evitar bugs com Time.deltaTime=0
            if (Event.current.type != EventType.Repaint) return;
            if (Processor == null) return;
            
            GUI.depth = -1000;
            
            DrawBackground();
            DrawStatusDot();
            DrawLabel();
            DrawBar();
        }
        
        private void DrawBackground()
        {
            // Fundo escuro cobrindo todo o painel
            GUI.DrawTexture(new Rect(5, 5, 320, 55), bgTexture);
        }
        
        private void DrawStatusDot()
        {
            Color dot = Color.red;
            if (!Processor.IsMuted)
            {
                if (Processor.CurrentMode == VoipProcessor.VoipMode.Open)
                    dot = Color.green;
                else if (Processor.CurrentMode == VoipProcessor.VoipMode.PTT)
                    dot = Processor.IsPTTActive ? Color.green : Color.yellow;
                else // VAD
                    dot = smoothLevel > VoIPPlugin.VADThreshold.Value ? Color.green : Color.yellow;
            }
            
            var dotTex = MakeTex(dot);
            GUI.DrawTexture(new Rect(10, 10, 20, 20), dotTex);
            Object.Destroy(dotTex); // limpa a textura temporária
        }
        
        private void DrawLabel()
        {
            string channel = CurrentChannel == 0 ? "RAID" : CurrentChannel == 2 ? "SPEC" : "LOBBY";
            string mode = Processor.CurrentMode.ToString().ToUpper();
            string state;
            
            if (Processor.IsMuted)
                state = "MUDO";
            else if (Processor.CurrentMode == VoipProcessor.VoipMode.PTT)
                state = Processor.IsPTTActive ? "TX" : "...";
            else if (Processor.CurrentMode == VoipProcessor.VoipMode.VAD)
                state = smoothLevel > VoIPPlugin.VADThreshold.Value ? "FALANDO" : "OUVINDO";
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
            float barX = 10f, barY = 38f, barW = 300f, barH = 14f;
            
            // Fundo da barra
            GUI.DrawTexture(new Rect(barX, barY, barW, barH), bgTexture);
            
            // Borda branca
            GUI.DrawTexture(new Rect(barX - 1, barY - 1, barW + 2, 1), whiteTexture);
            GUI.DrawTexture(new Rect(barX - 1, barY + barH, barW + 2, 1), whiteTexture);
            GUI.DrawTexture(new Rect(barX - 1, barY - 1, 1, barH + 2), whiteTexture);
            GUI.DrawTexture(new Rect(barX + barW, barY - 1, 1, barH + 2), whiteTexture);
            
            // Preenchimento da barra
            // Normalização dinâmica: barra relativa ao maior pico já captado
            float fill = (peakMax > 0f) ? Mathf.Clamp01(smoothLevel / peakMax) : 0f;
            
            if (fill > 0.001f)
            {
                Color barColor = fill > 0.8f ? Color.red : fill > 0.4f ? Color.yellow : Color.green;
                var barFillTex = MakeTex(barColor);
                GUI.DrawTexture(new Rect(barX, barY, barW * fill, barH), barFillTex);
                Object.Destroy(barFillTex);
            }
        }
    }
}
