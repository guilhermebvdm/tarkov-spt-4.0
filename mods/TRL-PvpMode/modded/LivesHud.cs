using System;
using Comfort.Common;
using EFT;
using UnityEngine;

namespace TarkovRedLine.PvpMode
{
    /// <summary>
    /// Indicador de vidas restantes no canto da tela.
    ///
    /// Desenhado com <c>OnGUI</c> em vez de um elemento da interface do jogo: é um texto só, não
    /// depende de nenhum tipo interno do Fika nem do EFT (que mudam entre versões) e some sozinho
    /// quando a guarda de contexto reprova. O custo é aceitável porque o corpo do método sai na
    /// primeira linha fora de partida.
    /// </summary>
    internal static class LivesHud
    {
        private const string INFINITY = "∞";   // ∞

        private static bool _broken;

        private static GUIStyle _style;
        private static GUIStyle _styleDowned;

        public static void Draw()
        {
            try
            {
                // OnGUI é chamado uma vez POR EVENTO (Layout, Repaint, teclado, mouse): sem este
                // filtro o corpo inteiro — incluindo duas alocações de string — roda 2 a 10+ vezes
                // por quadro, e só o Repaint chega a pintar algo (code review 003, E-05).
                if (Event.current.type != EventType.Repaint) return;
                if (_broken) return;
                if (!Settings.SHOW_LIVES_HUD.Value) return;

                // Modo inativo cobre tudo de uma vez: desligado no F12, pré-requisito ausente,
                // esconderijo, menu. Nunca prometer vidas que não existem.
                if (!RaidState.IsActive) return;

                var gameWorld = Singleton<GameWorld>.Instance;
                var player = gameWorld?.MainPlayer;
                if (player == null) return;

                // O fim de raid começa bem antes de GameWorld.OnDestroy: sem esta guarda o
                // contador fica por cima da tela de morte, contrariando a spec (E-06).
                if (!player.HealthController.IsAlive) return;

                EnsureStyles();

                var downed = player.ActiveHealthController is
                    Fika.Core.Main.ClientClasses.ClientHealthController { Downed: true };

                var value = RaidState.IsUnlimited ? INFINITY : RaidState.LivesLeft.ToString();
                var text = downed ? $"VIDAS: {value}" : $"Vidas: {value}";

                // Caído: destacado e centralizado — é a informação que decide a próxima ação.
                // De pé: discreto, no canto.
                var rect = downed
                    ? new Rect(0f, Screen.height * 0.62f, Screen.width, 40f)
                    : new Rect(24f, Screen.height - 64f, 240f, 30f);

                GUI.Label(rect, text, downed ? _styleDowned : _style);
            }
            catch (Exception ex)
            {
                // Desarma em vez de repetir: um erro aqui se repetiria a cada evento de GUI.
                _broken = true;
                Plugin.Log.LogError($"[TRL-PvpMode] LivesHud desativado apos erro: {ex.Message}");
            }
        }

        private static void EnsureStyles()
        {
            if (_style != null) return;

            _style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
            };
            _style.normal.textColor = new Color(0.85f, 0.85f, 0.85f, 0.75f);

            _styleDowned = new GUIStyle(_style)
            {
                fontSize = 24,
                alignment = TextAnchor.MiddleCenter,
            };
            _styleDowned.normal.textColor = new Color(1f, 0.35f, 0.35f, 0.95f);
        }
    }
}
