using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using EFT.UI;
using Fika.Core.Bundles;
using Fika.Core.Main.Utils;
using Fika.Core.Networking;
using Fika.Core.Networking.Http;
using Fika.Core.Networking.Models;
using Fika.Core.Networking.Models.Headless;
using Fika.Core.Networking.Websocket;
using Fika.Core.UI.Models;
using HarmonyLib;
using TMPro;
using UnityEngine.UI;
using static Fika.Core.UI.FikaUIGlobals;

namespace Fika.Core.UI.Custom;

public class MatchMakerUIScript : MonoBehaviour
{
    private MatchMakerUI _fikaMatchMakerUi;
    private LobbyEntry[] _matches;
    private readonly List<GameObject> _matchesListObjects = [];
    private bool _stopQuery;
    private GameObject _newBackButton;
    private string _profileId;
    private float _lastRefreshed;
    private bool _started;
    private Coroutine _serverQueryRoutine;
    private float _dotTimer;
    private int _dotCount;
    private GameObject _mmGameObject;
    private TMP_InputField _hostPasswordInput;
    private JoinSessionModal _joinSessionModal;

    private const float _dotInterval = 0.5f;

    internal DefaultUIButton AcceptButton;
    internal RaidSettings RaidSettings;
    internal DefaultUIButton BackButton;

    protected void OnEnable()
    {
        if (_started)
        {
            _stopQuery = false;
            if (_serverQueryRoutine == null)
            {
                _serverQueryRoutine = StartCoroutine(ServerQuery());
            }
        }
    }

    protected void OnDisable()
    {
        _stopQuery = true;
        if (_serverQueryRoutine != null)
        {
            StopCoroutine(_serverQueryRoutine);
            _serverQueryRoutine = null;
        }
        DestroyThis();
    }

    protected void Start()
    {
        _profileId = FikaBackendUtils.Profile.ProfileId;
        CreateMatchMakerUI();
        _serverQueryRoutine = StartCoroutine(ServerQuery());
        _started = true;
    }

    protected void Update()
    {
        if (_stopQuery && _serverQueryRoutine != null)
        {
            StopCoroutine(_serverQueryRoutine);
            _serverQueryRoutine = null;
        }

        if (!_fikaMatchMakerUi.LoadingScreen.activeSelf)
        {
            return;
        }

        _dotTimer += Time.deltaTime;

        if (_dotTimer >= _dotInterval)
        {
            _dotTimer = 0f;
            _dotCount = (_dotCount % 3) + 1;

            _fikaMatchMakerUi.LoadingAnimationText.text =
                new string('.', _dotCount);
        }
    }

    private void DestroyThis()
    {
        _stopQuery = true;
        if (_serverQueryRoutine != null)
        {
            StopCoroutine(_serverQueryRoutine);
            _serverQueryRoutine = null;
        }

        Destroy(_fikaMatchMakerUi);
        Destroy(this);
        Destroy(_mmGameObject);
    }

    protected void OnDestroy()
    {
        _stopQuery = true;
        if (_newBackButton != null)
        {
            Destroy(_newBackButton);
        }
    }

    private void CreateMatchMakerUI()
    {
        FikaBackendUtils.IsHeadlessRequester = false;

        var availableHeadlesses = FikaRequestHandler.GetAvailableHeadlesses();

        var matchMakerUiPrefab = InternalBundleLoader.Instance.GetFikaAsset(InternalBundleLoader.EFikaAsset.MatchmakerUI);
        var uiGameObj = Instantiate(matchMakerUiPrefab);
        _mmGameObject = uiGameObj;
        _fikaMatchMakerUi = uiGameObj.GetComponent<MatchMakerUI>();
        _fikaMatchMakerUi.transform.parent = transform;

        var rectTransform = _fikaMatchMakerUi.transform.GetChild(0).RectTransform();
        rectTransform.gameObject.AddComponent<UIDragComponent>().Init(rectTransform, true);

        if (_fikaMatchMakerUi.RaidGroupDefaultToClone.active)
        {
            _fikaMatchMakerUi.RaidGroupDefaultToClone.SetActive(false);
        }

        if (_fikaMatchMakerUi.DediSelection.active)
        {
            _fikaMatchMakerUi.DediSelection.SetActive(false);
        }

        // Ensure the IsSpectator field is reset every time the matchmaker UI is created
        FikaBackendUtils.IsSpectator = false;

        _fikaMatchMakerUi.SpectatorToggle.isOn = false;
        _fikaMatchMakerUi.SpectatorToggle.onValueChanged.AddListener((arg) =>
        {
            FikaBackendUtils.IsSpectator = !FikaBackendUtils.IsSpectator;
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuCheckBox);
        });

        _fikaMatchMakerUi.LoadingAnimationText.SetText(string.Empty);

        _fikaMatchMakerUi.DedicatedToggle.isOn = false;
        _fikaMatchMakerUi.DedicatedToggle.onValueChanged.AddListener((_) => Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuCheckBox));

        if (availableHeadlesses.Length == 0)
        {
            _fikaMatchMakerUi.DedicatedToggle.interactable = false;
            var dedicatedText = _fikaMatchMakerUi.DedicatedToggle.gameObject.GetComponentInChildren<TextMeshProUGUI>();
            if (dedicatedText != null)
            {
                dedicatedText.color = new(1f, 1f, 1f, 0.5f);
            }

            var dediTooltipArea = _fikaMatchMakerUi.DedicatedToggle.GetOrAddComponent<HoverTooltipArea>();
            dediTooltipArea.enabled = true;
            dediTooltipArea.SetMessageText(LocaleUtils.UI_NO_DEDICATED_CLIENTS.Localized());
        }

        if (availableHeadlesses.Length >= 1)
        {
            if (FikaPlugin.Instance.Settings.UseHeadlessIfAvailable.Value)
            {
                _fikaMatchMakerUi.DedicatedToggle.isOn = true;
            }

            _fikaMatchMakerUi.HeadlessSelection.gameObject.SetActive(true);
            _fikaMatchMakerUi.HeadlessSelection.onValueChanged.AddListener((_) => Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuDropdownSelect));

            _fikaMatchMakerUi.HeadlessSelection.ClearOptions();

            List<TMP_Dropdown.OptionData> optionDatas = [];

            // Sort availableHeadlesses alphabetically by Alias
            Array.Sort(availableHeadlesses,
                (x, y) => string.Compare(x.Alias, y.Alias, StringComparison.OrdinalIgnoreCase));
            for (var i = 0; i < availableHeadlesses.Length; i++)
            {
                var user = availableHeadlesses[i];
                optionDatas.Add(new()
                {
                    text = user.Alias
                });
            }

            _fikaMatchMakerUi.HeadlessSelection.AddOptions(optionDatas);
        }

        try
        {
            // Root cause of every earlier layout bug: DedicatedToggle/Dropdown are anchored
            // to DediSelectionFrame's CENTER (confirmed against the actual asset bundle).
            // Growing DediSelectionFrame's height — no matter who else is parented there —
            // always pushes its center down by half the growth, dragging them with it while
            // the top-anchored Header stays put (verified empirically, including after
            // reparenting the password field alongside them). The fix is architectural, not
            // another patch: move the checkbox, dropdown and password field into a new
            // content container that stacks them with a VerticalLayoutGroup instead of fixed
            // per-child anchors, and anchor that container to the TOP — like Header — so it
            // never drifts when the panel resizes. Only StartButton (bottom-anchored)
            // and this new container (top-anchored) are direct children left exposed to
            // DediSelectionFrame's size, and neither reacts badly to it.
            var dediRect = _fikaMatchMakerUi.DediSelection.GetComponent<RectTransform>();
            var dediFrameRect = _fikaMatchMakerUi.DediSelection.transform.Find("DediSelectionFrame") as RectTransform;
            var startBtnRect = _fikaMatchMakerUi.StartButton.GetComponent<RectTransform>();
            var headerRect = dediFrameRect != null ? dediFrameRect.Find("Header") as RectTransform : null;
            var toggleRect = _fikaMatchMakerUi.DedicatedToggle.GetComponent<RectTransform>();
            var dropdownRect = _fikaMatchMakerUi.HeadlessSelection.GetComponent<RectTransform>();

            const float fieldHeight = 32f;
            const float gap = 10f;

            if (dediRect != null && dediFrameRect != null && startBtnRect != null && headerRect != null && toggleRect != null && dropdownRect != null)
            {
                // DediSelection starts deactivated (hidden until the player clicks Host —
                // see the SetActive(false) above) and Unity's layout system can't correctly
                // measure/rebuild RectTransforms that aren't part of an active hierarchy.
                // Force it active for this block's measurements, then restore whatever it
                // was.
                bool wasDediSelectionActive = _fikaMatchMakerUi.DediSelection.activeSelf;
                _fikaMatchMakerUi.DediSelection.SetActive(true);

                var contentGo = new GameObject("DediSelectionContent", typeof(RectTransform));
                var contentRect = contentGo.GetComponent<RectTransform>();
                contentRect.SetParent(dediFrameRect, false);
                contentRect.anchorMin = new Vector2(0.5f, 1f);
                contentRect.anchorMax = new Vector2(0.5f, 1f);
                contentRect.pivot = new Vector2(0.5f, 1f);
                contentRect.sizeDelta = new Vector2(dediFrameRect.rect.width, 0f);

                var layout = contentGo.AddComponent<VerticalLayoutGroup>();
                layout.childAlignment = TextAnchor.UpperCenter;
                layout.spacing = gap;
                layout.childControlWidth = false;
                layout.childControlHeight = false;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;

                var fitter = contentGo.AddComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

                // Move the real controls (not clones) into the stack, in visual order.
                // Reparenting doesn't touch their listeners/references — only where they
                // sit in the hierarchy. None of Toggle/TMP_Dropdown/TMP_InputField
                // implement ILayoutElement, so without an explicit LayoutElement the group
                // has no preferred size to read for them even with childControlHeight
                // false — it was computing the container's total height as if they
                // contributed ~0, which is why StartButton (positioned off that height)
                // landed right under the header instead of under the real content.
                toggleRect.SetParent(contentRect, false);
                SetPreferredSize(toggleRect);
                dropdownRect.SetParent(contentRect, false);
                SetPreferredSize(dropdownRect);

                var sendItemPrefab = InternalBundleLoader.Instance.GetFikaAsset(InternalBundleLoader.EFikaAsset.SendItemMenu);
                if (sendItemPrefab != null)
                {
                    var sendItemUi = sendItemPrefab.GetComponent<SendItemUI>();
                    if (sendItemUi != null && sendItemUi.PlayersFilter != null)
                    {
                        var pwdInputObj = Instantiate(sendItemUi.PlayersFilter.gameObject, contentRect);
                        pwdInputObj.name = "DediSessionPasswordInput";
                        _hostPasswordInput = pwdInputObj.GetComponent<TMP_InputField>();
                        _hostPasswordInput.text = "";
                        _hostPasswordInput.contentType = TMP_InputField.ContentType.Password;
                        if (_hostPasswordInput.placeholder != null)
                        {
                            var ph = _hostPasswordInput.placeholder.GetComponent<TextMeshProUGUI>();
                            if (ph != null)
                            {
                                ph.text = "Senha da Sessão (Opcional)...";
                            }
                        }

                        var pwdRect = pwdInputObj.GetComponent<RectTransform>();
                        if (pwdRect != null)
                        {
                            // The dropdown's own RectTransform (300) is wider than
                            // DediSelectionFrame itself (275) — its sprite has transparent
                            // padding baked in that hides the overhang on screen, but our
                            // field's background sprite is opaque edge-to-edge, so matching
                            // that raw number verbatim reads as visibly wider than the
                            // dropdown. Use a narrower, safely-proven width off the frame.
                            float inputWidth = dediFrameRect.rect.width - 45f;
                            pwdRect.sizeDelta = new Vector2(inputWidth, fieldHeight);
                            SetPreferredSize(pwdRect);
                        }
                    }
                }

                // Force an immediate layout pass so the container's height (normally
                // computed lazily on the next canvas update) is available right now. Read
                // it as a LOCAL size, not world corners — contentRect hasn't been
                // positioned yet at this point, so its world position is still whatever
                // default (0,0) it started at.
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
                float contentHeight = contentRect.rect.height;

                // Compute every final target FIRST, all from measurements that won't
                // change (Header's current position, content's own height, the button's
                // own height) — then apply the resize, and only THEN move content/button
                // into place. Doing it in the opposite order bit us already: resizing
                // DediSelection drags DediSelectionFrame with it (it's centered on
                // DediSelection's own anchor, the same kind of drift that broke the
                // checkbox/dropdown one level down) which would silently invalidate any
                // position computed beforehand.
                var headerCorners = new Vector3[4];
                headerRect.GetWorldCorners(headerCorners);
                float headerBottomWorldY = headerCorners[0].y;

                // contentHeight/buttonHeight/gap are all LOCAL (unscaled) design units —
                // GetWorldCorners numbers already have the canvas's scale baked in (this
                // panel's is ~1.3x, confirmed by comparing rect.size against measured world
                // corners). Subtracting a local size straight from a world Y silently
                // shrinks every gap/height by that scale factor — scale local lengths up to
                // world units before mixing them into world-space arithmetic.
                float scale = dediFrameRect.lossyScale.y;
                if (Mathf.Approximately(scale, 0f))
                {
                    scale = 1f;
                }

                float contentTopWorldY = headerBottomWorldY - (gap * scale);
                float contentBottomWorldY = contentTopWorldY - (contentHeight * scale);
                float buttonHeight = startBtnRect.rect.height;
                float buttonTopWorldY = contentBottomWorldY - (gap * scale);
                float buttonBottomWorldY = buttonTopWorldY - (buttonHeight * scale);
                float targetFrameBottomWorldY = buttonBottomWorldY - (gap * scale);

                var frameCorners = new Vector3[4];
                dediFrameRect.GetWorldCorners(frameCorners);
                float frameBottomWorldY = frameCorners[0].y;

                if (frameBottomWorldY > targetFrameBottomWorldY)
                {
                    var frameCornersBefore = new Vector3[4];
                    dediFrameRect.GetWorldCorners(frameCornersBefore);
                    float frameTopWorldY = frameCornersBefore[1].y;
                    PositionPatchByWorldTopBottom(dediFrameRect, frameTopWorldY, targetFrameBottomWorldY);

                    // DediSelectionFrame is now at its final size/position. Extending
                    // DediSelection (the outer border/shadow) too would drag it again
                    // (same center-anchor drift), so record where it just settled and snap
                    // it straight back afterward.
                    var frameCornersAfter = new Vector3[4];
                    dediFrameRect.GetWorldCorners(frameCornersAfter);
                    float frameTopWorldYFinal = frameCornersAfter[1].y;

                    var dediCornersBefore = new Vector3[4];
                    dediRect.GetWorldCorners(dediCornersBefore);
                    float dediTopWorldY = dediCornersBefore[1].y;

                    PositionPatchByWorldTopBottom(dediRect, dediTopWorldY, targetFrameBottomWorldY);
                    MoveTopEdgeToWorldY(dediFrameRect, frameTopWorldYFinal);
                }

                // Only now, with the panel at its final size, place the content stack and
                // the button — nothing will move again after this.
                MoveTopEdgeToWorldY(contentRect, contentTopWorldY);
                MoveTopEdgeToWorldY(startBtnRect, buttonTopWorldY);

                _fikaMatchMakerUi.DediSelection.SetActive(wasDediSelectionActive);
            }
        }
        catch (Exception ex)
        {
            FikaGlobals.LogError($"MatchMakerUIScript: Failed to inject host password input: {ex}");
        }

        _joinSessionModal = JoinSessionModal.Create(_fikaMatchMakerUi.transform);

        var hostTooltipArea = _fikaMatchMakerUi.RaidGroupHostButton.GetOrAddComponent<HoverTooltipArea>();
        hostTooltipArea.enabled = true;
        hostTooltipArea.SetMessageText(LocaleUtils.UI_HOST_RAID_TOOLTIP.Localized());

        _fikaMatchMakerUi.RaidGroupHostButton.onClick.AddListener(() =>
        {
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.ButtonClick);
            if (!_fikaMatchMakerUi.DediSelection.activeSelf)
            {
                _fikaMatchMakerUi.DediSelection.SetActive(true);
            }
            else
            {
                _fikaMatchMakerUi.DediSelection.SetActive(false);
            }
        });

        _fikaMatchMakerUi.CloseButton.onClick.AddListener(() =>
        {
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.ButtonClick);
            if (_fikaMatchMakerUi.DediSelection.active)
            {
                _fikaMatchMakerUi.DediSelection.SetActive(false);
            }
        });

        _fikaMatchMakerUi.DedicatedToggle.onValueChanged.AddListener((_) => Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuCheckBox));

        _fikaMatchMakerUi.StartButton.onClick.AddListener(async () =>
        {
            ToggleLoading(true);

            var sessionPassword = string.IsNullOrWhiteSpace(_hostPasswordInput?.text) ? null : _hostPasswordInput.text.Trim();

            var tarkovApplication = (TarkovApplication)Singleton<ClientApplication<ISession>>.Instance;
            var session = tarkovApplication.Session;

            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.ButtonClick);

            if (!_fikaMatchMakerUi.DedicatedToggle.isOn)
            {
                if (FikaPlugin.Instance.Settings.ForceIP.Value != "")
                {
                    // We need to handle DNS entries as well
                    var ip = FikaPlugin.Instance.Settings.ForceIP.Value;
                    try
                    {
                        var dnsAddress = Dns.GetHostAddresses(FikaPlugin.Instance.Settings.ForceIP.Value);
                        if (dnsAddress.Length > 0)
                        {
                            ip = dnsAddress[0].ToString();
                        }
                    }
                    catch
                    {

                    }

                    if (!IPAddress.TryParse(ip, out _))
                    {
                        Singleton<PreloaderUI>.Instance.ShowCriticalErrorScreen(LocaleUtils.UI_ERROR_FORCE_IP_HEADER.Localized(),
                            string.Format(LocaleUtils.UI_ERROR_FORCE_IP.Localized(), ip),
                            ErrorScreen.EButtonType.OkButton, 10f);

                        ToggleLoading(false);
                        return;
                    }
                }

                if (FikaPlugin.Instance.Settings.ForceBindIP.Value != "Disabled")
                {
                    if (!IPAddress.TryParse(FikaPlugin.Instance.Settings.ForceBindIP.Value, out _))
                    {
                        Singleton<PreloaderUI>.Instance.ShowCriticalErrorScreen(LocaleUtils.UI_ERROR_BIND_IP_HEADER.Localized(),
                            string.Format(LocaleUtils.UI_ERROR_BIND_IP.Localized(), FikaPlugin.Instance.Settings.ForceBindIP.Value),
                            ErrorScreen.EButtonType.OkButton, 10f);

                        ToggleLoading(false);
                        return;
                    }
                }

                await FikaBackendUtils.CreateMatch(FikaBackendUtils.Profile.ProfileId, FikaBackendUtils.PMCName, RaidSettings, sessionPassword);
                AcceptButton.OnClick.Invoke();
            }
            else
            {
                FikaPlugin.HeadlessRequesterWebSocket ??= new HeadlessRequesterWebSocket();

                if (!FikaPlugin.HeadlessRequesterWebSocket.Connected)
                {
                    FikaPlugin.HeadlessRequesterWebSocket.Connect();
                }

                var raidSettings = Traverse.Create(tarkovApplication).Field<RaidSettings>("_raidSettings").Value;

                var headlessSessionId = availableHeadlesses[0].HeadlessSessionID;
                var multipleHeadlesses = availableHeadlesses.Length > 1;

                if (multipleHeadlesses)
                {
                    var selectedHeadless = _fikaMatchMakerUi.HeadlessSelection.value;
                    headlessSessionId = availableHeadlesses[selectedHeadless].HeadlessSessionID;
                }

                StartHeadlessRequest request = new()
                {
                    HeadlessSessionID = headlessSessionId,
                    Time = raidSettings.SelectedDateTime,
                    LocationId = raidSettings.SelectedLocation._Id,
                    SpawnPlace = raidSettings.PlayersSpawnPlace,
                    MetabolismDisabled = raidSettings.MetabolismDisabled,
                    BotSettings = raidSettings.BotSettings,
                    Side = raidSettings.Side,
                    TimeAndWeatherSettings = raidSettings.TimeAndWeatherSettings,
                    WavesSettings = raidSettings.WavesSettings,
                    CustomRaidSettings = FikaBackendUtils.CustomRaidSettings,
                    UseEvent = raidSettings.transitionType.HasFlagNoBox(ELocationTransition.Event),
                    Password = sessionPassword
                };

                var response = await FikaRequestHandler.StartHeadless(request);
                FikaBackendUtils.IsHeadlessRequester = true;

                if (!string.IsNullOrEmpty(response.Error))
                {
                    PreloaderUI.Instance.ShowErrorScreen(LocaleUtils.UI_DEDICATED_ERROR.Localized(), response.Error);
                    ToggleLoading(false);
                    FikaBackendUtils.IsHeadlessRequester = false;
                }
                else
                {
                    NotificationManagerClass.DisplaySingletonWarningNotification(LocaleUtils.STARTING_RAID_ON_DEDICATED.Localized());
                }
            }
        });

        _fikaMatchMakerUi.RefreshButton.onClick.AddListener(ManualRefresh);

        var tooltipArea = _fikaMatchMakerUi.RefreshButton.GetOrAddComponent<HoverTooltipArea>();
        tooltipArea.enabled = true;
        tooltipArea.SetMessageText(LocaleUtils.UI_REFRESH_RAIDS.Localized());

        AcceptButton.gameObject.SetActive(false);
        AcceptButton.enabled = false;
        AcceptButton.Interactable = false;

        _newBackButton = Instantiate(BackButton.gameObject, BackButton.transform.parent);
        UnityEngine.Events.UnityEvent newEvent = new();
        newEvent.AddListener(BackButton.OnClick.Invoke);
        var newButtonComponent = _newBackButton.GetComponent<DefaultUIButton>();
        Traverse.Create(newButtonComponent).Field("OnClick").SetValue(newEvent);

        if (!_newBackButton.active)
        {
            _newBackButton.SetActive(true);
        }

        BackButton.gameObject.SetActive(false);
    }

    // Moves a RectTransform (via anchoredPosition) so its world-space top edge lands
    // exactly at targetWorldY — regardless of its own pivot/anchor setup. Used instead of
    // reasoning about nested anchor math for a layout baked in a compiled Unity prefab we
    // can't read as text.
    private static void MoveTopEdgeToWorldY(RectTransform rect, float targetWorldY)
    {
        var corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        float currentTopY = corners[1].y;

        float parentScaleY = rect.parent != null ? rect.parent.lossyScale.y : 1f;
        if (Mathf.Approximately(parentScaleY, 0f))
        {
            parentScaleY = 1f;
        }

        rect.anchoredPosition += new Vector2(0f, (targetWorldY - currentTopY) / parentScaleY);
    }

    // Resizes and repositions a RectTransform so its world-space top and bottom edges land
    // exactly at the given Y values — used to size a background patch to fill a gap whose
    // bounds were measured from two unrelated elements.
    private static void PositionPatchByWorldTopBottom(RectTransform rect, float worldTopY, float worldBottomY)
    {
        // sizeDelta maps to world size through the rect's OWN accumulated scale (parent
        // scale times whatever localScale this object has on top of it), not just the
        // parent's — DediSelection carries its own ~1.3x localScale baked into the
        // prefab, on top of an unscaled parent, so using parent.lossyScale alone (correct
        // for anchoredPosition deltas, which live in the parent's frame) undersized the
        // divisor and made the resulting box 1.3x too tall.
        float ownScaleY = rect.lossyScale.y;
        if (Mathf.Approximately(ownScaleY, 0f))
        {
            ownScaleY = 1f;
        }

        rect.sizeDelta = new Vector2(rect.sizeDelta.x, (worldTopY - worldBottomY) / ownScaleY);
        MoveTopEdgeToWorldY(rect, worldTopY);
    }

    // Toggle/TMP_Dropdown/TMP_InputField don't implement ILayoutElement, so a
    // VerticalLayoutGroup has no preferred size to read for them — even with
    // childControlHeight/Width false, it needs this to compute the container's own total
    // size via ContentSizeFitter. Report their current (explicitly set) size back to it.
    private static void SetPreferredSize(RectTransform rect)
    {
        var layoutElement = rect.gameObject.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = rect.gameObject.AddComponent<LayoutElement>();
        }
        layoutElement.preferredWidth = rect.sizeDelta.x;
        layoutElement.preferredHeight = rect.sizeDelta.y;
    }

    private void ToggleLoading(bool enabled)
    {
        _fikaMatchMakerUi.RaidGroupHostButton.interactable = !enabled;
        _fikaMatchMakerUi.DediSelection.SetActive(!enabled);
        _fikaMatchMakerUi.StartButton.interactable = !enabled;
        _fikaMatchMakerUi.ServerBrowserPanel.SetActive(!enabled);

        _fikaMatchMakerUi.LoadingScreen.SetActive(enabled);

        if (enabled)
        {
            if (_serverQueryRoutine != null)
            {
                StopCoroutine(_serverQueryRoutine);
                _serverQueryRoutine = null;
            }
            return;
        }

        _serverQueryRoutine = StartCoroutine(ServerQuery());
    }

    private void AutoRefresh()
    {
        _matches = FikaRequestHandler.LocationRaids(RaidSettings);

        _lastRefreshed = Time.time;

        RefreshUI();
    }

    private void ManualRefresh()
    {
        Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.ButtonClick);
        _matches = FikaRequestHandler.LocationRaids(RaidSettings);

        _lastRefreshed = Time.time;

        RefreshUI();
    }

    public static async Task<bool> JoinMatch(string profileId, string serverId, Button button, bool reconnect, string password = null)
    {
        if (button != null)
        {
            button.enabled = false;
        }

        FikaBackendUtils.IsReconnect = reconnect;
        NotificationManagerClass.DisplayMessageNotification(LocaleUtils.CONNECTING_TO_SESSION.Localized(),
            iconType: EFT.Communications.ENotificationIconType.EntryPoint);
        using var pingingClient = await NetManagerUtils.CreatePingingClient();

        if (pingingClient.Init(serverId))
        {
            FikaGlobals.LogInfo("Attempting to connect to host session...");
            var knockMessage = FikaBackendUtils.ServerGuid.ToString();
            var success = await pingingClient.AttemptToPingHost(knockMessage, reconnect);

            if (!success)
            {
                Singleton<PreloaderUI>.Instance.ShowCriticalErrorScreen(
                LocaleUtils.UI_ERROR_CONNECTING.Localized(),
                LocaleUtils.UI_UNABLE_TO_CONNECT.Localized(),
                ErrorScreen.EButtonType.OkButton, 10f);

                if (button != null)
                {
                    button.enabled = true;
                }
                return false;
            }
        }
        else
        {
            Singleton<PreloaderUI>.Instance.ShowCriticalErrorScreen(
                LocaleUtils.UI_ERROR_CONNECTING.Localized(),
                LocaleUtils.UI_PINGER_START_FAIL.Localized(),
                ErrorScreen.EButtonType.OkButton, 10f);
            return false;
        }

        if (FikaBackendUtils.JoinMatch(profileId, serverId, out var result, out var errorMessage, password))
        {
            FikaBackendUtils.GroupId = result.ServerId;
            FikaBackendUtils.ClientType = EClientType.Client;

            AddPlayerRequest data = new(FikaBackendUtils.GroupId, profileId, FikaBackendUtils.IsSpectator);
            FikaRequestHandler.UpdateAddPlayer(data);

            return true;
        }
        else
        {
            Singleton<PreloaderUI>.Instance.ShowErrorScreen("ERROR JOINING", errorMessage, null);
            if (button != null)
            {
                button.enabled = true;
            }
            return false;
        }
    }

    private void RefreshUI()
    {
        if (_matches == null)
        {
            // not initialized
            return;
        }

        if (_matchesListObjects != null)
        {
            // cleanup old objects
            foreach (var match in _matchesListObjects)
            {
                Destroy(match);
            }
        }

        // create lobby listings
        for (var i = 0; i < _matches.Length; ++i)
        {
            var entry = _matches[i];

            if (entry.ServerId == _profileId)
            {
                continue;
            }

            // server object
            var server = Instantiate(_fikaMatchMakerUi.RaidGroupDefaultToClone, _fikaMatchMakerUi.RaidGroupDefaultToClone.transform.parent);
            server.SetActive(true);
            _matchesListObjects.Add(server);

            server.name = entry.ServerId;
            string entryServerId = entry.ServerId;

            var localPlayerInRaid = false;
            var localPlayerDead = false;
            foreach ((MongoID profileId, var player) in entry.Players)
            {
                if (profileId == _profileId)
                {
                    localPlayerInRaid = true;
                    localPlayerDead = player;
                }
            }

            // player label
            var playerLabel = GameObject.Find("PlayerLabel");
            playerLabel.name = "PlayerLabel" + i;
            var sessionName = entry.HostUsername;
            if (entry.HasPassword)
            {
                sessionName = "🔒 " + sessionName;
            }
            playerLabel.GetComponentInChildren<TextMeshProUGUI>().SetText(sessionName);

            // players count label
            var playerCountLabel = GameObject.Find("PlayerCountLabel");
            playerCountLabel.name = "PlayerCountLabel" + i;
            var playerCount = entry.IsHeadless ? entry.PlayerCount - 1 : entry.PlayerCount;
            playerCountLabel.GetComponentInChildren<TextMeshProUGUI>().SetText("{0}", playerCount);

            // player join button
            var joinButton = GameObject.Find("JoinButton");
            joinButton.name = "JoinButton" + i;
            var button = joinButton.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                if (_fikaMatchMakerUi.DediSelection.activeSelf)
                {
                    _fikaMatchMakerUi.DediSelection.SetActive(false);
                }

                Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.ButtonClick);
                FikaBackendUtils.HostLocationId = entry.Location;

                var isInvade = entry.Status == LobbyEntry.ELobbyStatus.IN_GAME && !localPlayerInRaid;
                var title = isInvade ? "INVADIR RAID" : "ENTRAR NA SESSÃO";
                var btnLabel = isInvade ? "INVADIR" : "ENTRAR";

                _joinSessionModal.Show(title, btnLabel, entry.HasPassword, async (enteredPassword) =>
                {
                    ToggleLoading(true);
                    // Use the captured id, not server.name: a background refresh (ServerQuery,
                    // every 5s) destroys and recreates every row's GameObject, and this callback
                    // only runs once the user confirms the modal, which can happen well after that.
                    // Reading server.name at that point throws NullReferenceException if the row
                    // was already destroyed — reliably reproducible once the target raid transits
                    // to another map while the invade confirmation is still open.
                    var success = await JoinMatch(_profileId, entryServerId, button, localPlayerInRaid, enteredPassword);
                    if (success)
                    {
                        AcceptButton.OnClick.Invoke();
                        return;
                    }
                    ToggleLoading(false);
                });
            });

            HoverTooltipArea tooltipArea;
            var image = server.GetComponent<Image>();

            if (RaidSettings.LocationId != entry.Location && !(RaidSettings.LocationId.StartsWith("sandbox", StringComparison.OrdinalIgnoreCase) &&
                entry.Location.StartsWith("sandbox", StringComparison.OrdinalIgnoreCase)))
            {
                button.enabled = false;
                if (image != null)
                {
                    image.color = new(0.5f, image.color.g / 2, image.color.b / 2, 0.75f);
                }

                tooltipArea = joinButton.GetOrAddComponent<HoverTooltipArea>();
                tooltipArea.enabled = true;
                tooltipArea.SetMessageText(string.Format(LocaleUtils.UI_CANNOT_JOIN_RAID_OTHER_MAP.Localized(),
                    ColorizeText(EColor.BLUE, entry.Location.Localized())));

                continue;
            }

            if (RaidSettings.SelectedDateTime != entry.Time)
            {
                button.enabled = false;
                if (image != null)
                {
                    image.color = new(0.5f, image.color.g / 2, image.color.b / 2, 0.75f);
                }

                tooltipArea = joinButton.GetOrAddComponent<HoverTooltipArea>();
                tooltipArea.enabled = true;
                tooltipArea.SetMessageText(LocaleUtils.UI_CANNOT_JOIN_RAID_OTHER_TIME.Localized());

                continue;
            }

            if (RaidSettings.Side != entry.Side)
            {
                var errorText = "ERROR";
                if (RaidSettings.Side == ESideType.Pmc)
                {
                    errorText = LocaleUtils.UI_CANNOT_JOIN_RAID_SCAV_AS_PMC.Localized();
                }
                else if (RaidSettings.Side == ESideType.Savage)
                {
                    errorText = LocaleUtils.UI_CANNOT_JOIN_RAID_PMC_AS_SCAV.Localized();
                }

                button.enabled = false;
                if (image != null)
                {
                    image.color = new(0.5f, image.color.g / 2, image.color.b / 2, 0.75f);
                }

                tooltipArea = joinButton.GetOrAddComponent<HoverTooltipArea>();
                tooltipArea.enabled = true;
                tooltipArea.SetMessageText(errorText);

                continue;
            }

            switch (entry.Status)
            {
                case LobbyEntry.ELobbyStatus.LOADING:
                    {
                        button.enabled = false;
                        if (image != null)
                        {
                            image.color = new(0.5f, image.color.g / 2, image.color.b / 2, 0.75f);
                        }

                        tooltipArea = joinButton.GetOrAddComponent<HoverTooltipArea>();
                        tooltipArea.enabled = true;
                        tooltipArea.SetMessageText(LocaleUtils.UI_HOST_STILL_LOADING.Localized());
                    }
                    break;
                case LobbyEntry.ELobbyStatus.IN_GAME:
                    if (!localPlayerInRaid)
                    {
                        button.enabled = true;
                        var btnText = joinButton.GetComponentInChildren<TextMeshProUGUI>();
                        if (btnText != null)
                        {
                            btnText.SetText("INVADIR");
                        }

                        tooltipArea = joinButton.GetOrAddComponent<HoverTooltipArea>();
                        tooltipArea.enabled = true;
                        tooltipArea.SetMessageText("Raid em andamento — Invadir partida");
                    }
                    else
                    {
                        if (!localPlayerDead)
                        {
                            tooltipArea = joinButton.GetOrAddComponent<HoverTooltipArea>();
                            tooltipArea.enabled = true;
                            tooltipArea.SetMessageText(LocaleUtils.UI_REJOIN_RAID.Localized());
                        }
                        else
                        {
                            button.enabled = false;
                            if (image != null)
                            {
                                image.color = new(0.5f, image.color.g / 2, image.color.b / 2, 0.75f);
                            }

                            tooltipArea = joinButton.GetOrAddComponent<HoverTooltipArea>();
                            tooltipArea.enabled = true;
                            tooltipArea.SetMessageText(LocaleUtils.UI_CANNOT_REJOIN_RAID_DIED.Localized());
                        }
                    }
                    break;
                case LobbyEntry.ELobbyStatus.COMPLETE:
                    tooltipArea = joinButton.GetOrAddComponent<HoverTooltipArea>();
                    tooltipArea.enabled = true;
                    tooltipArea.SetMessageText(LocaleUtils.UI_JOIN_RAID.Localized());
                    break;
            }
        }
    }

    public IEnumerator ServerQuery()
    {
        while (!_stopQuery)
        {
            AutoRefresh();

            while (Time.time < _lastRefreshed + 5)
            {
                yield return null;
            }
        }
    }
}
