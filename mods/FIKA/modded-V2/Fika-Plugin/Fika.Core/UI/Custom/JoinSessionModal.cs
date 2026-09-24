using System;
using Comfort.Common;
using EFT.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Fika.Core.Bundles;
using Fika.Core.Main.Utils;

namespace Fika.Core.UI.Custom;

public class JoinSessionModal : MonoBehaviour
{
    private GameObject _modalInstance;
    private TextMeshProUGUI _titleText;
    private Button _closeButton;
    private Button _actionButton;
    private TextMeshProUGUI _actionButtonText;
    private TMP_InputField _passwordInput;
    private Action<string> _onConfirm;
    private Action _onCancel;

    public static JoinSessionModal Create(Transform parent)
    {
        var go = new GameObject("FikaJoinSessionModalHolder");
        go.transform.SetParent(parent, false);
        var modal = go.AddComponent<JoinSessionModal>();
        modal.Initialize();
        return modal;
    }

    public void Initialize()
    {
        if (_modalInstance != null)
        {
            return;
        }

        var mmPrefab = InternalBundleLoader.Instance.GetFikaAsset(InternalBundleLoader.EFikaAsset.MatchmakerUI);
        if (mmPrefab == null)
        {
            FikaGlobals.LogError("JoinSessionModal: MatchmakerUI prefab not found");
            return;
        }

        var mmUi = mmPrefab.GetComponent<MatchMakerUI>();
        if (mmUi == null || mmUi.DediSelection == null)
        {
            FikaGlobals.LogError("JoinSessionModal: Could not find DediSelection in MatchmakerUI prefab");
            return;
        }

        _modalInstance = Instantiate(mmUi.DediSelection, transform);
        _modalInstance.name = "JoinSessionModalWindow";
        _modalInstance.SetActive(false);

        // Desativar toggle e dropdown de headless
        var toggles = _modalInstance.GetComponentsInChildren<Toggle>(true);
        foreach (var toggle in toggles)
        {
            toggle.gameObject.SetActive(false);
        }

        var dropdowns = _modalInstance.GetComponentsInChildren<TMP_Dropdown>(true);
        foreach (var dropdown in dropdowns)
        {
            dropdown.gameObject.SetActive(false);
        }

        // Localizar botões
        var buttons = _modalInstance.GetComponentsInChildren<Button>(true);
        foreach (var btn in buttons)
        {
            if (btn.name.IndexOf("Close", StringComparison.OrdinalIgnoreCase) >= 0 || btn.name.IndexOf("X", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                _closeButton = btn;
            }
            else
            {
                _actionButton = btn;
            }
        }

        if (_closeButton == null && buttons.Length > 0) _closeButton = buttons[0];
        if (_actionButton == null && buttons.Length > 1) _actionButton = buttons[1];

        // Localizar TextMeshPro para título
        var texts = _modalInstance.GetComponentsInChildren<TextMeshProUGUI>(true);
        if (texts.Length > 0)
        {
            _titleText = texts[0];
        }

        if (_actionButton != null)
        {
            _actionButtonText = _actionButton.GetComponentInChildren<TextMeshProUGUI>();
            _actionButton.onClick.RemoveAllListeners();
            _actionButton.onClick.AddListener(() =>
            {
                Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.ButtonClick);
                var pwd = _passwordInput != null ? _passwordInput.text.Trim() : "";
                Hide();
                _onConfirm?.Invoke(pwd);
            });
        }

        if (_closeButton != null)
        {
            _closeButton.onClick.RemoveAllListeners();
            _closeButton.onClick.AddListener(() =>
            {
                Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.ButtonClick);
                Hide();
                _onCancel?.Invoke();
            });
        }

        // Injetar InputField de senha a partir do SendItemMenu
        try
        {
            var sendItemPrefab = InternalBundleLoader.Instance.GetFikaAsset(InternalBundleLoader.EFikaAsset.SendItemMenu);
            if (sendItemPrefab != null)
            {
                var sendItemUi = sendItemPrefab.GetComponent<SendItemUI>();
                if (sendItemUi != null && sendItemUi.PlayersFilter != null)
                {
                    var inputGo = Instantiate(sendItemUi.PlayersFilter.gameObject, _modalInstance.transform);
                    inputGo.name = "ModalPasswordInputField";
                    _passwordInput = inputGo.GetComponent<TMP_InputField>();
                    _passwordInput.text = "";
                    _passwordInput.contentType = TMP_InputField.ContentType.Password;

                    var rect = inputGo.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        var modalRect = _modalInstance.GetComponent<RectTransform>();
                        float inputWidth = modalRect != null ? Mathf.Clamp(modalRect.sizeDelta.x - 36f, 180f, 215f) : 210f;
                        rect.anchoredPosition = new Vector2(0, 10);
                        rect.sizeDelta = new Vector2(inputWidth, 32f);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            FikaGlobals.LogError($"JoinSessionModal: Failed to inject input field: {ex}");
        }
    }

    public void Show(string title, string buttonLabel, bool hasPassword, Action<string> onConfirm, Action onCancel = null)
    {
        if (_modalInstance == null)
        {
            Initialize();
        }

        if (_modalInstance == null)
        {
            // Fallback caso falhe instanciação da UI
            onConfirm?.Invoke(null);
            return;
        }

        _onConfirm = onConfirm;
        _onCancel = onCancel;

        if (_titleText != null)
        {
            _titleText.text = title;
        }

        if (_actionButtonText != null)
        {
            _actionButtonText.text = buttonLabel;
        }

        if (_passwordInput != null)
        {
            _passwordInput.text = "";
            if (_passwordInput.placeholder != null)
            {
                var ph = _passwordInput.placeholder.GetComponent<TextMeshProUGUI>();
                if (ph != null)
                {
                    ph.text = hasPassword ? "Digite a senha da raid..." : "Partida sem senha (opcional)";
                }
            }
        }

        _modalInstance.SetActive(true);
        _modalInstance.transform.SetAsLastSibling();
    }

    public void Hide()
    {
        if (_modalInstance != null)
        {
            _modalInstance.SetActive(false);
        }
    }
}
