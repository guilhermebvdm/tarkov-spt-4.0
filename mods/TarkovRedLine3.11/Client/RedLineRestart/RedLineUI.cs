using EFT.UI;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Comfort.Common;

namespace RedLineRestart
{
    public class RedLineUI
    {
        private GameObject _restartButton;
        private TextMeshProUGUI _btnTextComponent;
        private Image _btnImageComponent;

        private Rect _windowRect = new Rect(20, 20, 320, 240);
        private GUIStyle _customWindowStyle;
        private Texture2D _bgTexture;

        public bool IsButtonReady => _restartButton != null;

        public void UpdateButtonVisuals()
        {
            if (!IsButtonReady || _btnTextComponent == null || _btnImageComponent == null) return;

            if (RedLineState.CooldownLeft > 0)
            {
                _btnImageComponent.color = new Color(0.12f, 0.12f, 0.12f, 0.9f);
                _btnTextComponent.text = $"REINICIAR HOST ({RedLineState.CooldownLeft}s)"; 
                _btnTextComponent.color = new Color(0.45f, 0.45f, 0.45f, 1f);
            }
            else
            {
                _btnImageComponent.color = new Color(0.5f, 0.1f, 0.1f, 0.85f);
                _btnTextComponent.text = "REINICIAR HOST DEDICADO";
                _btnTextComponent.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            }
        }

        public void DrawWindow(MonoBehaviour plugin)
        {
            if (!IsButtonReady || RedLineState.CooldownLeft > 0)
            {
                RedLineState.ShowVoteWindow = false;
                return;
            }
            if (!RedLineState.ShowVoteWindow) return;

            if (_customWindowStyle == null)
            {
                _bgTexture = new Texture2D(1, 1);
                _bgTexture.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.9f));
                _bgTexture.Apply();
                _customWindowStyle = new GUIStyle(GUI.skin.window);
                _customWindowStyle.normal.background = _bgTexture;
            }

            _windowRect = GUI.Window(999, _windowRect, (id) =>
            {
                GUILayout.BeginVertical();

                // -----------------------------------------------------------------------
                // TELA 4: Votação VETADA (com opção de reverter)
                // -----------------------------------------------------------------------
                if (RedLineState.InProgress && RedLineState.IsVetoed)
                {
                    GUI.color = Color.red;
                    GUILayout.Label("VOTAÇÃO VETADA!");
                    GUILayout.Label($"Encerrando em: {RedLineState.TimeLeft}s");
                    GUI.color = Color.white;
                    GUILayout.Space(10);

                    // Se eu votei, dou a chance de cancelar (principalmente se fui eu que vetei)
                    if (RedLineState.VotedInThisSession)
                    {
                        GUI.color = Color.yellow;
                        if (GUILayout.Button("RETIRAR MEU VOTO"))
                        {
                            plugin.StartCoroutine(RedLineAPI.CancelVote());
                        }
                        GUI.color = Color.white;
                    }

                    GUILayout.Space(10);
                    if (GUILayout.Button("FECHAR JANELA")) RedLineState.ShowVoteWindow = false;
                }

                // -----------------------------------------------------------------------
                // VOTAÇÃO ATIVA (Sem Veto)
                // -----------------------------------------------------------------------
                else if (RedLineState.InProgress)
                {
                    GUI.color = Color.cyan;
                    GUILayout.Label($"VOTAÇÃO ATIVA: {RedLineState.TimeLeft}s");
                    GUI.color = Color.white;
                    GUILayout.Label($"QUANTIDADE DE VOTOS 'SIM': {RedLineState.YesVotes}");
                    GUILayout.Space(10);

                    // TELA 2: "Já Votado"
                    if (RedLineState.VotedInThisSession)
                    {
                        GUILayout.Label("[VOTO REGISTRADO]");
                        GUI.color = Color.yellow;
                        if (GUILayout.Button("CANCELAR MEU VOTO"))
                        {
                            plugin.StartCoroutine(RedLineAPI.CancelVote());
                        }
                        GUI.color = Color.white;
                    }
                    // TELA 3: "Ainda vai votar"
                    else
                    {
                        GUILayout.Label("[AGUARDANDO SEU VOTO]");
                        if (GUILayout.Button("(Y) VOTAR SIM")) plugin.StartCoroutine(RedLineAPI.SendVote(true));
                        GUILayout.Space(5);
                        if (GUILayout.Button("(N) VOTAR NÃO")) plugin.StartCoroutine(RedLineAPI.SendVote(false));

                        // Host pode cancelar se votos == 0
                        if (RedLineState.AmITheInitiator && RedLineState.YesVotes == 0)
                        {
                            GUILayout.Space(15);
                            GUI.color = new Color(1f, 0.3f, 0.3f);
                            if (GUILayout.Button("CANCELAR ABERTURA DA VOTAÇÃO"))
                            {
                                plugin.StartCoroutine(RedLineAPI.TerminateSession());
                            }
                            GUI.color = Color.white;
                        }
                    }

                    GUILayout.Space(15);
                    if (GUILayout.Button("FECHAR JANELA")) RedLineState.ShowVoteWindow = false;
                }
                // -----------------------------------------------------------------------
                // TELA 1: "Deseja iniciar?"
                // -----------------------------------------------------------------------
                else
                {
                    GUILayout.Label("DESEJA INICIAR A VOTAÇÃO?");
                    if (GUILayout.Button("INICIAR AGORA"))
                    {
                        RedLineState.AmITheInitiator = true;
                        plugin.StartCoroutine(RedLineAPI.SendVote(true));
                    }
                    GUILayout.Space(15);
                    if (GUILayout.Button("FECHAR JANELA")) RedLineState.ShowVoteWindow = false;
                }

                GUILayout.EndVertical();
                GUI.DragWindow();
            }, "REINICIAR HOST DEDICADO", _customWindowStyle);
        }

        // TryCreateButton permanece igual...
        public void TryCreateButton()
        {
            /* Mantenha o código existente do TryCreateButton que eu mandei na resposta anterior */
            if (_restartButton != null) return;
            var menuTaskBar = Object.FindObjectOfType<MenuTaskBar>();
            if (menuTaskBar == null) return;
            var fieldInfo = typeof(MenuTaskBar).GetField("_toggleButtons", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo == null) return;
            var buttonsDict = fieldInfo.GetValue(menuTaskBar) as Dictionary<EMenuType, AnimatedToggle>;
            if (buttonsDict == null || buttonsDict.Count == 0) return;

            var referenceBtn = buttonsDict.Values.First();
            var referenceTextInfo = referenceBtn.GetComponentInChildren<TextMeshProUGUI>();

            _restartButton = new GameObject("BtnRestartRedLine");
            _restartButton.transform.SetParent(referenceBtn.transform.parent, false);

            var le = _restartButton.AddComponent<LayoutElement>();
            le.minHeight = 30;
            le.flexibleWidth = 0;

            var fitter = _restartButton.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var hGroup = _restartButton.AddComponent<HorizontalLayoutGroup>();
            hGroup.childControlWidth = true;
            hGroup.childControlHeight = true;
            hGroup.childAlignment = TextAnchor.MiddleCenter;
            hGroup.padding = new RectOffset(4, 4, 0, 0);
            hGroup.spacing = 2;

            _btnImageComponent = _restartButton.AddComponent<Image>();
            _btnImageComponent.color = new Color(0.5f, 0.1f, 0.1f, 0.85f);

            var btn = _restartButton.AddComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                if (RedLineState.CooldownLeft <= 0) RedLineState.ShowVoteWindow = !RedLineState.ShowVoteWindow;
                Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuDropdown);
            });

            if (buttonsDict.ContainsKey(EMenuType.RagFair))
            {
                var ragfairBtn = buttonsDict[EMenuType.RagFair];
                if (ragfairBtn != null)
                {
                    var targetIcon = ragfairBtn.GetComponentsInChildren<Image>(true).FirstOrDefault(x => x.sprite != null);
                    if (targetIcon != null)
                    {
                        var iconObj = new GameObject("RedLine_Icon");
                        iconObj.transform.SetParent(_restartButton.transform, false);
                        iconObj.transform.SetAsFirstSibling();
                        var iconImg = iconObj.AddComponent<Image>();
                        iconImg.sprite = targetIcon.sprite;
                        iconImg.preserveAspect = true;
                        iconImg.color = new Color(0.9f, 0.9f, 0.9f, 1f);
                        iconObj.GetComponent<RectTransform>().sizeDelta = new Vector2(20, 20);
                    }
                }
            }

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(_restartButton.transform, false);
            _btnTextComponent = textObj.AddComponent<TextMeshProUGUI>();
            if (referenceTextInfo != null)
            {
                _btnTextComponent.font = referenceTextInfo.font;
                _btnTextComponent.fontSize = referenceTextInfo.fontSize;
            }
            _btnTextComponent.text = "REINICIAR HOST DEDICADO";
            _btnTextComponent.alignment = TextAlignmentOptions.Center;
            _btnTextComponent.enableWordWrapping = false;

            _restartButton.transform.SetAsLastSibling();
        }
    }
}