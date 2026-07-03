using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CustomClasses.Client;

/// <summary>
///     (055/057) Detalhe da classe na tela de carregamento do FIKA — PER-PLAYER (057): cada linha com classe
///     resolvida ganha tint do nickname (cor da classe) + <see cref="LoadingClassHover"/> com o popover da classe
///     DAQUELE player. Resolução via <see cref="ClassIdentities"/> (mapa nickname→classe do server, refetch por
///     tela — PA-01-04) com fallback local (rota ausente → comportamento 055). Raid scav local → no-op (PA-01-02).
///     **Nenhum tipo FIKA no IL** — alvos por <c>AccessTools.TypeByName</c>/reflection (padrão SAIN, Plugin.cs:126);
///     a linha sai de <c>_loadingPlayers[netId]</c> castada para <see cref="Component"/>. Degrada 100% solo.
///     ref: fika-plugin/Fika.Core/UI/Custom/LoadingScreenUI.cs:97 (AddPlayer) / :14 (_loadingPlayers) /
///     LoadingScreenPlayer.cs:7 (Nickname) / FikaBackendUtils.cs:47 (IsScav).
/// </summary>
internal class ClassDetailLoadingPatch : ModulePatch
{
    private static readonly Type? UiType = AccessTools.TypeByName("LoadingScreenUI");
    private static readonly FieldInfo? PlayersField = UiType != null ? AccessTools.Field(UiType, "_loadingPlayers") : null;

    // 057 PA-01-02: raid scav local → sem identidade em nenhuma linha (o loading só conhece nicknames de PMC).
    private static readonly Type? BackendUtilsType = AccessTools.TypeByName("FikaBackendUtils");
    private static readonly MethodInfo? IsScavGetter =
        BackendUtilsType != null ? AccessTools.PropertyGetter(BackendUtilsType, "IsScav") : null;

    // 057 PA-01-04: instância da tela mudou (nova raid/loading) → refetch do mapa (perfis novos sem restart).
    // ref: CR-01-09 — retenção do shell destruído entre raids é intencional (só p/ ReferenceEquals; sem recursos Unity).
    private static object? _lastLoadingScreen;

    // ref: CR-01-08 — FieldInfo do TMP `Nickname` cacheado 1× (o tipo da row é sempre LoadingScreenPlayer).
    private static FieldInfo? _nicknameField;

    protected override MethodBase GetTargetMethod()
    {
        // ref: fika-plugin/Fika.Core/UI/Custom/LoadingScreenUI.cs:97
        return AccessTools.Method(UiType, "AddPlayer", new[] { typeof(int), typeof(string) });
    }

    [PatchPostfix]
    private static void Postfix(object __instance, int netId, string nickname)
    {
        try
        {
            if (PerksConfig.ClassDetailOnLoading?.Value != true || string.IsNullOrEmpty(nickname))
            {
                return;
            }

            // PA-01-02: scav run local → no-op total. ref: fika-plugin FikaBackendUtils.cs:47 (getter público).
            if (IsScavGetter?.Invoke(null, null) is true)
            {
                return;
            }

            // PA-01-04: nova tela de loading → Reset (o TryResolve abaixo refaz o fetch; 1×/raid).
            if (!ReferenceEquals(_lastLoadingScreen, __instance))
            {
                _lastLoadingScreen = __instance;
                ClassIdentities.Reset();
            }

            // 057: resolve a identidade DESTE nickname (o mapa cobre todos os perfis do server, local incluso).
            // Fallback (rota ausente → mapa vazio): nickname local resolve via SkillMultipliers (comportamento 055).
            if (!ClassIdentities.TryResolve(nickname, out var id))
            {
                SkillMultipliers.EnsureLoaded();
                if (!string.Equals(nickname, SkillMultipliers.Nickname, StringComparison.Ordinal))
                {
                    return;
                }

                var local = ClassIdentities.Local();   // ref: CR-01-06 — sem null-forgiving
                if (local == null)
                {
                    return;   // local vanilla → linha intocada (critério da 01-spec)
                }

                id = local;
            }

            // GameObject da linha via _loadingPlayers[netId] (LoadingScreenPlayer é MonoBehaviour → Component).
            if (PlayersField?.GetValue(__instance) is not IDictionary dict || !dict.Contains(netId))
            {
                return;
            }

            if (dict[netId] is not Component row)
            {
                return;
            }

            // Campo público `Nickname` do LoadingScreenPlayer via reflection (zero tipos FIKA no IL).
            // ref: fika-plugin/Fika.Core/UI/Custom/LoadingScreenPlayer.cs:7 · CR-01-08 (FieldInfo cacheado)
            _nicknameField ??= AccessTools.Field(row.GetType(), "Nickname");
            var nickTmp = _nicknameField?.GetValue(row) as TextMeshProUGUI;

            // ref: CR-01-05 — espelha o early-return do AddPlayer do FIKA (LoadingScreenUI.cs:99-101): netId já
            // existente → row VELHA com outro nickname; não aplicar a identidade de B na linha de A.
            if (nickTmp != null && !string.Equals(nickTmp.text, nickname, StringComparison.Ordinal))
            {
                return;
            }

            // (a) 057: identidade SEM hover (tint-only — PA-01-03): nickname na cor da classe.
            //     ref: CR-01-04 — classe SEM nameColor → não pinta (ApplyGradient pintaria branco por cima do
            //     estilo FIKA, sem revert); a identidade dessas classes fica só no popover.
            if (nickTmp != null && !string.IsNullOrWhiteSpace(id.NameColor))
            {
                ClassIdentityView.ApplyGradient(nickTmp, id.NameColor, Color.white);   // ref: ClassIdentityView.cs:39
            }

            // (b) popover no hover, parametrizado pela identidade DESTE player (idempotente: reusa o componente).
            var hover = row.GetComponent<LoadingClassHover>();
            if (hover == null)
            {
                hover = row.gameObject.AddComponent<LoadingClassHover>();
            }

            hover.Identity = id;
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (057) class detail loading: {ex.Message}");
        }
    }
}

/// <summary>
///     (055/057) Na linha de UM player, mostra no hover o painel de detalhe da classe DELE (<see cref="Identity"/>,
///     setada pelo Postfix — 057). Hover-only (o painel auto-visível cobria o carrossel do deploy); só atua se
///     houver GraphicRaycaster + EventSystem. PA-01-02 (055): a fonte vem de um TMP da própria linha.
/// </summary>
internal sealed class LoadingClassHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private GameObject? _panel;

    /// <summary>057 — identidade da classe DESTE player (setada pelo Postfix; re-setada a cada AddPlayer).</summary>
    internal ClassIdentities.Identity? Identity;

    private void Awake() => EnsureRaycast();   // garante que o hover pegue em qualquer ponto da linha do player

    // hover-only (feedback in-game): NÃO mostra sozinho — o painel auto-visível cobria o carrossel do deploy.
    private void OnEnable()
    {
        if (_panel != null)
        {
            _panel.SetActive(false);
        }
    }

    /// <summary>Um Image transparente raycast-target no root da linha, p/ o hover disparar em toda a área do nome.</summary>
    private void EnsureRaycast()
    {
        var img = GetComponent<Image>();
        if (img == null)
        {
            img = gameObject.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0f);   // transparente — só captura o ponteiro
        }

        img.raycastTarget = true;
    }

    // Pegada VISUAL do popover na tela (constante, independente da escala).
    private const float BaseWidth = 600f;
    private const float BaseHeight = 460f;

    /// <summary>Monta (lazy) e exibe o painel — TODO o caminho protegido (F-3: OnPointerEnter fica fora do try/catch do Postfix).</summary>
    private void Show()
    {
        try
        {
            if (Identity == null)
            {
                return;   // ref: CR-01-07 — sem identidade → não mostrar (nunca cair pro LOCAL numa linha remota)
            }

            Ensure();
            if (_panel != null)
            {
                ApplyScale();   // lê o F12 a cada hover → ajuste "live" sem reiniciar
                PerksPanelView.Refresh(_panel, Identity);   // 057: classe DESTE player
                DisableRaycast();   // PA-01-11: DEPOIS do Refresh — o rebuild de cards cria Graphics novos
                _panel.SetActive(true);
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (055) show detail: {ex.Message}");
        }
    }

    private void Ensure()
    {
        if (_panel != null)
        {
            return;
        }

        var font = GetComponentInChildren<TMP_Text>()?.font;   // PA-01-02: fonte da própria linha
        var canvas = GetComponentInParent<Canvas>();
        var parent = canvas != null ? canvas.transform : transform.root;

        _panel = PerksPanelView.Build(parent, font);

        // o Build ancora "fill" (bom p/ a aba); no loading reancoramos COMPACTO à direita da tela (não cobre tudo).
        // sizeDelta fica no ApplyScale (depende da escala do F12).
        var rt = (RectTransform)_panel.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(1f, 0.5f);
        rt.pivot = new Vector2(1f, 0.5f);
        rt.anchoredPosition = new Vector2(-60f, 0f);
    }

    /// <summary>
    ///     057 PA-01-11: popover é SÓ-exibição no loading — raycast desligado no painel inteiro pra não roubar
    ///     o hover de outra linha (roubo → OnPointerExit → esconde → flicker). CardHover deixa de atuar aqui (ok).
    ///     Chamado após CADA Refresh (o rebuild de cards cria Graphics novos raycast-target).
    /// </summary>
    private void DisableRaycast()
    {
        if (_panel == null)
        {
            return;
        }

        foreach (var g in _panel.GetComponentsInChildren<Graphic>(true))
        {
            g.raycastTarget = false;
        }
    }

    /// <summary>
    ///     Zoom-out do popover (CLASS#3 rende mais cards): escala o conteúdo (default 75%) e COMPENSA o rect
    ///     lógico (÷ escala) → a pegada visual continua ~600×460, com mais espaço interno pros cards.
    ///     Pivot (1, 0.5) → a escala encolhe a partir da borda direita/centro, sem deslocar o painel.
    /// </summary>
    private void ApplyScale()
    {
        if (_panel == null)
        {
            return;
        }

        var s = Mathf.Clamp(PerksConfig.LoadingPanelScale?.Value ?? 0.75f, 0.5f, 1f);
        var rt = (RectTransform)_panel.transform;
        rt.localScale = new Vector3(s, s, 1f);
        rt.sizeDelta = new Vector2(BaseWidth / s, BaseHeight / s);
    }

    // hover = toggle OPCIONAL (refinamento; só dispara se houver GraphicRaycaster + EventSystem).
    public void OnPointerEnter(PointerEventData eventData) => Show();

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_panel != null)
        {
            _panel.SetActive(false);   // popover: some ao tirar o mouse do nome
        }
    }

    // F-9: se a linha for desativada (sem destruir), esconde o painel junto (ele é filho do Canvas, não da linha).
    private void OnDisable()
    {
        if (_panel != null)
        {
            _panel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (_panel != null)
        {
            Destroy(_panel);
        }
    }
}
