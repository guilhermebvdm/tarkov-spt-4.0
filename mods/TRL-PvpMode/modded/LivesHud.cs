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

        /// <summary>Chamado no inicio de cada raid: um erro isolado nao pode apagar o indicador
        /// pelo resto da SESSAO, e os estilos vem de uma cena que ja nao existe (F-06).</summary>
        public static void Reset()
        {
            _broken = false;
            _style = null;
            _styleDowned = null;
            _stylePrompt = null;
            _styleNoLives = null;
        }


        private static GUIStyle _style;
        private static GUIStyle _styleDowned;
        private static GUIStyle _stylePrompt;
        private static GUIStyle _styleNoLives;

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

                var downed = player.ActiveHealthController is
                    Fika.Core.Main.ClientClasses.ClientHealthController { Downed: true };

                // Desmaio do TRL-ImmersiveCombatMedicine nao e morte: o jogador acorda sozinho e
                // nao gasta vida. Mostrar "segure para renascer" ali promete o que nao se aplica.
                if (FikaBridge.IsFaintedByCombatMedicine(player.ProfileId)) return;

                // Esconder na tela de fim de raid (E-06) SEM esconder no estado caido - que e o
                // unico momento para o qual este indicador existe. IsAlive e false durante todo o
                // caido, entao testa-lo sozinho tornaria o ramo destacado inalcancavel (F-02).
                if (!player.HealthController.IsAlive && !downed) return;

                EnsureStyles();

                var value = RaidState.IsUnlimited ? INFINITY : RaidState.LivesLeft.ToString();
                var text = downed ? $"VIDAS: {value}" : $"Vidas: {value}";

                // A instrução da tecla é o que faltava: sem ela o jogador cai, vê o contador e não
                // tem como saber o que fazer com ele. Descoberto no primeiro teste in-game.
                if (downed) DrawRespawnPrompt();

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

        /// <summary>
        /// Diz qual tecla segurar e mostra o quanto já foi segurado.
        ///
        /// A barra não é enfeite: ela é a única forma de o jogador distinguir "estou segurando a
        /// tecla certa e falta tempo" de "esta tecla não está chegando ao jogo" — distinção que
        /// custou uma raid inteira de tentativa e erro no primeiro teste.
        /// </summary>
        private static void DrawRespawnPrompt()
        {
            var hasLife = RaidState.HasLifeAvailable;
            var key = Settings.RESPAWN_KEY.Value.ToString();

            var message = hasLife
                ? $"Segure  [{key}]  para renascer"
                : "Sem vidas restantes — esta morte e definitiva";

            var promptRect = new Rect(0f, Screen.height * 0.70f, Screen.width, 30f);
            GUI.Label(promptRect, message, hasLife ? _stylePrompt : _styleNoLives);

            if (!hasLife) return;

            // Barra de progresso do segurar.
            var progress = Patches.RespawnInputPatch.HoldProgress;
            const float barWidth = 320f;
            const float barHeight = 10f;
            var barX = (Screen.width - barWidth) * 0.5f;
            var barY = Screen.height * 0.70f + 34f;

            GUI.color = new Color(1f, 1f, 1f, 0.25f);
            GUI.DrawTexture(new Rect(barX, barY, barWidth, barHeight), Texture2D.whiteTexture);

            if (progress > 0f)
            {
                GUI.color = new Color(0.45f, 0.95f, 0.45f, 0.9f);
                GUI.DrawTexture(new Rect(barX, barY, barWidth * progress, barHeight), Texture2D.whiteTexture);
            }

            GUI.color = Color.white;
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

            _stylePrompt = new GUIStyle(_style)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter,
            };
            _stylePrompt.normal.textColor = new Color(1f, 0.95f, 0.75f, 0.95f);

            _styleNoLives = new GUIStyle(_stylePrompt);
            _styleNoLives.normal.textColor = new Color(1f, 0.45f, 0.45f, 0.9f);
        }
    }
}
