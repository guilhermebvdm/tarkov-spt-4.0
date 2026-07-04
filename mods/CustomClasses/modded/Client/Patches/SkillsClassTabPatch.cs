using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EFT.UI;   // SkillsAndMasteringScreen, UIElement, LocalizedText
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomClasses.Client;

/// <summary>
///     Item 053 — controller da sub-aba CLASS. Implementa a interface nativa <c>GInterface486</c>
///     (<c>void Show()</c> + <c>Task&lt;bool&gt; TryHide()</c>) que o toggle-group (<c>GClass3808</c>) chama ao
///     selecionar/desselecionar a aba. Só liga/desliga o meu painel.
/// </summary>
internal class ClassTabController : GInterface486
{
    private readonly GameObject _panel;

    internal ClassTabController(GameObject panel)
    {
        _panel = panel;
    }

    public void Show()
    {
        _panel.SetActive(true);
        PerksPanelView.Refresh(_panel);   // 055: painel extraído p/ PerksPanelView (reuso com o loading da raid)
    }

    public Task<bool> TryHide()
    {
        _panel.SetActive(false);
        return Task.FromResult(true);   // sempre permite trocar de aba
    }
}

/// <summary>
///     Item 053 — sub-aba "CLASS" (primeira: CLASS | SKILLS | MASTERING) na tela de Skills. Cuida só da ABA
///     (clone do tab, toggle-group, rótulo NATIVO re-textado, posição); o CONTEÚDO (header + 2 colunas de perks/
///     drawbacks) é o <see cref="PerksPanelView"/> compartilhado (059, extraído no 055).
///     <list type="bullet">
///       <item>Aba: clona a <c>_masteringTab</c> (que está em estado NORMAL no Postfix — evita herdar a prancha
///       "selecionada" da SKILLS) e recria o toggle-group (<c>GClass3808</c>) com as 3 tabs. Idempotente.</item>
///       <item>Rótulo (fix in-game 2026-07-03): escreve "CLASS" nos TMPs NATIVOS das versões normal/selected do
///       Tab (ref: Tab.cs — `_normalVersion`/`_selectedVersion`/`_hoverGraphic`) → fonte, seleção e hover ficam
///       IDÊNTICOS a SKILLS/MASTERING de graça (o overlay antigo destoava). Ícone nativo recebe o brasão.</item>
///       <item>Posicionamento (fix in-game 2026-07-03): CLASS no INÍCIO DO CONTEÚDO (borda esquerda do
///       `_skillsScreen`) e SKILLS/MASTERING empurrados pra DIREITA (delta nativo preservado). Posições nativas
///       capturadas 1× por instância da tela (reaplicação idempotente).</item>
///     </list>
///     Via reflection (campos estáveis; tipos GClassNNNN obfuscados só no <c>ClassTabController : GInterface486</c>, ref de compile).
/// </summary>
internal class SkillsClassTabPatch : ModulePatch
{
    private const string TabName = "CC_ClassTab";

    private static bool _loggedTabImages;      // req 1: loga os nomes dos Images da aba 1× (ajuste fino do ícone).

    // 059 CLASS#1 (F12 live): última tela/aba montadas, p/ reposicionar no SettingChanged com a tela aberta.
    // MonoBehaviours → o == sobrecarregado do Unity detecta instância destruída (pooling/troca de cena).
    private static SkillsAndMasteringScreen? _lastScreen;
    private static Tab? _lastClassTab;
    private static bool _settingHooked;

    // Posições NATIVAS de SKILLS/MASTERING capturadas 1× por instância da tela (antes de qualquer push) —
    // o Reposition roda a cada Show/SettingChanged e precisa calcular sempre a partir do estado ORIGINAL.
    private static object? _nativesCapturedFor;
    private static float _skillsNativeX;
    private static float _masteringNativeX;

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.GetDeclaredMethods(typeof(SkillsAndMasteringScreen))
            .First(m => m.Name == nameof(SkillsAndMasteringScreen.Show) && m.GetParameters().Length == 3);
    }

    [PatchPostfix]
    private static void Postfix(SkillsAndMasteringScreen __instance)
    {
        try
        {
            if (__instance == null)
            {
                return;
            }

            EnsureSettingHook();   // 059 CLASS#1: F12 live (reposiciona no SettingChanged)

            // idempotência: a tela dá Show() várias vezes; só monta a aba 1×.
            // CR-03-05: na reabertura (screen pooled) NÃO re-normalizo a seleção aqui — o `Show` nativo termina em
            // `gclass3808_0.Show(null)` → `SelectTab(Tab_2)`, que restaura a ÚLTIMA aba (não força SKILLS). Por isso
            // a reabertura não re-quebra o double-select. Se um build futuro do EFT trocar por `Show(_skillsTab)`,
            // revisitar (mover a des-seleção das não-CLASS pra fora deste guard).
            var existingTab = __instance.GetComponentsInChildren<Tab>(true).FirstOrDefault(x => x.name == TabName);
            if (existingTab != null)
            {
                // 059 CLASS#1: aba já montada → só REPOSICIONA (F12 live também entre aberturas da tela).
                _lastScreen = __instance;
                _lastClassTab = existingTab;
                RepositionClassTab(__instance, existingTab);
                // (review CR-UI5-05) re-estiliza também na reabertura: troca de classe in-session deixava o
                // brasão/tint da classe ANTIGA na aba até o 1º clique (StyleClassTab é idempotente).
                StyleClassTab(existingTab, GameLocale.IsPortuguese ? "CLASSE" : "CLASS");
                return;
            }

            var t = typeof(SkillsAndMasteringScreen);
            var skillsTab = AccessTools.Field(t, "_skillsTab")?.GetValue(__instance) as Tab;
            var masteringTab = AccessTools.Field(t, "_masteringTab")?.GetValue(__instance) as Tab;
            var skillsScreen = AccessTools.Field(t, "_skillsScreen")?.GetValue(__instance) as UIElement;
            var groupField = AccessTools.Field(t, "gclass3808_0");
            var oldGroup = groupField?.GetValue(__instance);

            if (skillsTab == null || masteringTab == null || skillsScreen == null || groupField == null || oldGroup == null)
            {
                Plugin.Log?.LogWarning("[CustomClasses] (053) tab: campos da SkillsAndMasteringScreen não resolvidos.");
                return;
            }

            var font = __instance.GetComponentInChildren<TextMeshProUGUI>(true)?.font ?? TMP_Settings.defaultFontAsset;

            // 059: rótulo GENÉRICO "CLASS"/"CLASSE" (não o nome da classe — o header do painel já mostra "TANK").
            SkillMultipliers.EnsureLoaded();
            var tabLabel = GameLocale.IsPortuguese ? "CLASSE" : "CLASS";

            // (a) clona a aba MASTERING (estado NORMAL no Postfix → clone limpo, sem a prancha "selecionada" da SKILLS).
            var classTab = UnityEngine.Object.Instantiate(masteringTab.gameObject, masteringTab.transform.parent).GetComponent<Tab>();
            classTab.name = TabName;
            classTab.transform.SetSiblingIndex(0);

            // 059: LocalizedText re-localizaria o texto nativo → remove IMEDIATO (rodada 3: Destroy deferido
            // deixava o componente vivo até o fim do frame — janela p/ re-localizar por cima do nosso texto).
            foreach (var loc in classTab.GetComponentsInChildren<LocalizedText>(true))
            {
                UnityEngine.Object.DestroyImmediate(loc);
            }

            classTab.LocalizedText = null;
            StyleClassTab(classTab, tabLabel);
            classTab.OnSelectionChanged += (tab, _) => StyleClassTab(tab, tabLabel);   // reaplica na (de)seleção

            // painel: filho do content pai (comum a skills/mastering), preenche a área, começa escondido.
            var panel = PerksPanelView.Build(skillsScreen.transform.parent, font);

            // (b) registra o controller na aba CLASS.
            classTab.Init(new ClassTabController(panel));

            // (c) recria o toggle-group com as 3 tabs (Close antes p/ não duplicar handlers).
            AccessTools.Method(oldGroup.GetType(), "Close")?.Invoke(oldGroup, null);

            var tabsArray = new[] { classTab, skillsTab, masteringTab };
            var ctor = AccessTools.Constructor(oldGroup.GetType(), new[] { typeof(Tab[]), typeof(Tab), typeof(bool) });
            if (ctor == null)
            {
                Plugin.Log?.LogWarning("[CustomClasses] (053) ctor do toggle-group não encontrado.");
                return;
            }

            var newGroup = ctor.Invoke(new object[] { tabsArray, skillsTab, false });   // false = NÃO reordena siblings ao selecionar
            if (newGroup == null)
            {
                Plugin.Log?.LogWarning("[CustomClasses] (053) toggle-group não instanciou.");
                return;
            }

            groupField.SetValue(__instance, newGroup);

            // CLASS é a aba DEFAULT (pedido do usuário). SelectTab NÃO deseleciona as outras → forço o visual normal
            // nelas e escondo o conteúdo de SKILLS (senão CLASS + SKILLS aparecem ambas preenchidas).
            AccessTools.Method(newGroup.GetType(), "Show", new[] { typeof(Tab), typeof(bool) })
                ?.Invoke(newGroup, new object[] { classTab, true });   // seleciona CLASS + dispara o meu painel
            skillsTab.UpdateVisual(false);
            masteringTab.UpdateVisual(false);
            skillsScreen.Close();   // esconde a lista de skills (CLASS é o default agora)
            StyleClassTab(classTab, tabLabel);   // reaplica com a versão SELECTED já ativa (garante o texto no estado ativo)

            // as abas têm posição FIXA (não é layout group). Fix in-game 2026-07-03: CLASS no início do
            // CONTEÚDO e SKILLS/MASTERING empurrados pra direita (o RepositionClassTab captura as posições
            // nativas 1× nesta instância antes do primeiro push).
            var mRt = (RectTransform)masteringTab.transform;
            var bar = (RectTransform)skillsTab.transform.parent;
            LayoutRebuilder.ForceRebuildLayoutImmediate(bar);

            _lastScreen = __instance;
            _lastClassTab = classTab;
            RepositionClassTab(__instance, classTab);

            Plugin.Log?.LogInfo($"[CustomClasses][053-tabs] barLG={bar.GetComponent<LayoutGroup>()?.GetType().Name ?? "none"} | CLASS={((RectTransform)classTab.transform).anchoredPosition} w={((RectTransform)classTab.transform).rect.width:F0} | SKILLS={((RectTransform)skillsTab.transform).anchoredPosition} | MASTER={mRt.anchoredPosition}");
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (053) aba CLASS falhou: {ex.Message}");
        }
    }

    /// <summary>
    ///     059 CLASS#1 (fix in-game 2026-07-03) — CLASS começa no INÍCIO DO CONTEÚDO (borda esquerda do
    ///     `_skillsScreen`, a "linha vermelha" do feedback) + offset do F12; SKILLS e MASTERING são EMPURRADOS
    ///     pra direita (SKILLS = fim da CLASS + gap; MASTERING preserva o delta nativo p/ SKILLS). Posições
    ///     nativas capturadas 1× por instância (idempotente — roda a cada Show e no SettingChanged do F12).
    ///     No-op se a barra tiver LayoutGroup.
    /// </summary>
    private static void RepositionClassTab(SkillsAndMasteringScreen screen, Tab classTab)
    {
        try
        {
            var t = typeof(SkillsAndMasteringScreen);
            var skillsTab = AccessTools.Field(t, "_skillsTab")?.GetValue(screen) as Tab;
            var masteringTab = AccessTools.Field(t, "_masteringTab")?.GetValue(screen) as Tab;
            var skillsScreen = AccessTools.Field(t, "_skillsScreen")?.GetValue(screen) as UIElement;
            if (skillsTab == null || masteringTab == null || classTab == null)
            {
                return;
            }

            var bar = (RectTransform)skillsTab.transform.parent;
            if (bar.GetComponent<LayoutGroup>() != null)
            {
                return;   // layout group manda na posição — offset manual não se aplica
            }

            var sRt = (RectTransform)skillsTab.transform;
            var mRt = (RectTransform)masteringTab.transform;
            var cRt = (RectTransform)classTab.transform;

            // captura as posições NATIVAS 1× por instância da tela (antes do primeiro push desta instância).
            if (!ReferenceEquals(_nativesCapturedFor, screen))
            {
                _nativesCapturedFor = screen;
                _skillsNativeX = sRt.anchoredPosition.x;
                _masteringNativeX = mRt.anchoredPosition.x;
            }

            const float gap = 24f;
            var classW = cRt.rect.width > 1f ? cRt.rect.width : sRt.rect.width;
            var offsetX = PerksConfig.ClassTabOffsetX?.Value ?? 0f;

            // alvo da CLASS: borda esquerda do CONTEÚDO (skillsScreen) convertida pro espaço local da barra.
            // fallback (conversão indisponível): posição nativa da SKILLS.
            var classX = _skillsNativeX;
            if (skillsScreen != null && skillsScreen.transform is RectTransform contentRt)
            {
                var worldLeft = contentRt.TransformPoint(new Vector3(contentRt.rect.xMin, 0f, 0f));
                var barLocalLeft = bar.InverseTransformPoint(worldLeft).x;
                // anchoredPosition ↔ localPosition diferem por uma constante (mesmo pai/âncora) → converte por delta.
                var anchoredMinusLocal = cRt.anchoredPosition.x - cRt.localPosition.x;
                classX = barLocalLeft + anchoredMinusLocal - cRt.rect.xMin;   // xMin: alinha a BORDA, não o pivot
            }

            classX += offsetX;
            cRt.anchoredPosition = new Vector2(classX, sRt.anchoredPosition.y);

            // SKILLS: logo após a CLASS; MASTERING: preserva o delta nativo em relação à SKILLS.
            var skillsX = classX + classW + gap;
            sRt.anchoredPosition = new Vector2(skillsX, sRt.anchoredPosition.y);
            mRt.anchoredPosition = new Vector2(skillsX + (_masteringNativeX - _skillsNativeX), mRt.anchoredPosition.y);

            AlignPanelToTab(skillsScreen, cRt);
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (059) reposition tab: {ex.Message}");
        }
    }

    /// <summary>
    ///     Rodada 3 (fix in-game 2026-07-03) — o CONTEÚDO do painel alinha com a aba: a borda esquerda dos
    ///     cards coincide com a borda esquerda da aba CLASS, e a borda direita dos drawbacks recebe a MESMA
    ///     margem espelhada. Ajusta offsetMin/offsetMax.x do painel (anchors 0..1); o padding interno (28px,
    ///     ref: PerksPanelView.Build) é descontado. Roda junto com o Reposition (live com o F12).
    /// </summary>
    private static void AlignPanelToTab(UIElement? skillsScreen, RectTransform classTabRt)
    {
        if (skillsScreen == null)
        {
            return;
        }

        var panelTf = skillsScreen.transform.parent.Find(PerksPanelView.PanelName) as RectTransform;
        if (panelTf == null || panelTf.parent is not RectTransform parentRt)
        {
            return;
        }

        const float panelPadding = 28f;   // VerticalLayoutGroup.padding.left/right do painel
        var worldTabLeft = classTabRt.TransformPoint(new Vector3(classTabRt.rect.xMin, 0f, 0f));
        var localFromLeft = parentRt.InverseTransformPoint(worldTabLeft).x - parentRt.rect.xMin;
        var margin = Mathf.Max(0f, localFromLeft - panelPadding);
        panelTf.offsetMin = new Vector2(margin, panelTf.offsetMin.y);
        panelTf.offsetMax = new Vector2(-margin, panelTf.offsetMax.y);
    }

    /// <summary>Assina o SettingChanged do offset (1×): mexeu no slider do F12 → reposiciona na hora.</summary>
    private static void EnsureSettingHook()
    {
        if (_settingHooked || PerksConfig.ClassTabOffsetX == null)
        {
            return;
        }

        _settingHooked = true;
        PerksConfig.ClassTabOffsetX.SettingChanged += (_, _) =>
        {
            // == do Unity: instância destruída (pooling/cena) → null → ignora até a próxima montagem.
            if (_lastScreen != null && _lastClassTab != null)
            {
                RepositionClassTab(_lastScreen, _lastClassTab);
            }
        };
    }

    /// <summary>
    ///     059 (fix in-game 2026-07-03) — rótulo "CLASS" escrito nos TMPs NATIVOS do Tab (as versões
    ///     `_normalVersion`/`_selectedVersion` têm cada uma seu label — ref: Tab decompilado) → fonte, tamanho,
    ///     estado selecionado e HOVER ficam idênticos a SKILLS/MASTERING sem overlay nenhum. O ícone nativo
    ///     (herdado do clone da MASTERING) recebe o brasão da classe; sem brasão → ícone escondido.
    ///     Idempotente e reaplicável na (de)seleção (LocalizedText já foi destruído no build).
    /// </summary>
    private static void StyleClassTab(Tab tab, string label)
    {
        try
        {
            // TEXTO: re-texta TODOS os TMPs nativos (normal + selected) — estilo/estados nativos preservados.
            // Rodada 5 (in-game): o label da versão NORMAL vem INATIVO no clone → sem o SetActive o "CLASS"
            // some quando a aba está desselecionada (a rodada 4 removeu por medo de badge fantasma — não há;
            // rodada 3 com a ativação mostrava o texto). Reativado.
            var tmps = tab.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in tmps)
            {
                tmp.text = label;
                tmp.enabled = true;
                if (tmp.color.a < 0.05f)
                {
                    tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, 1f);
                }

                if (!tmp.gameObject.activeSelf)
                {
                    tmp.gameObject.SetActive(true);   // o pai (versão normal/selected) segue controlando a visibilidade
                }
            }

            // ÍCONE: troca o sprite nativo (medalha da MASTERING clonada) pelo brasão da classe, nas duas
            // versões. Preserva o fundo (`_targetImage` — ref: Tab decompilado) e os gráficos de hover.
            // Rodada 5: o sprite nativo da prancha é PRÉ-escurecido; o brasão é silhueta BRANCA → na versão
            // SELECIONADA tinge de escuro via Image.color (mesmo tom do texto nativo na prancha).
            var targetImage = AccessTools.Field(typeof(Tab), "_targetImage")?.GetValue(tab) as Image;
            var selectedVersion = AccessTools.Field(typeof(Tab), "_selectedVersion")?.GetValue(tab) as GameObject;
            var crest = ClassIconCache.Get(SkillMultipliers.IconFile);
            var images = tab.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if (img == targetImage || !img.gameObject.name.ToLowerInvariant().Contains("icon"))
                {
                    continue;
                }

                if (crest != null)
                {
                    // Rodada 4 (texto empurrado pra FORA da aba): com layout controlando a largura, um Image sem
                    // LayoutElement usa preferredWidth = LARGURA EM PX DO SPRITE — o brasão (~512px) desenhava
                    // pequeno (preserveAspect) mas OCUPAVA um rect gigante, expulsando o rótulo. Cap 1× no
                    // tamanho do sprite NATIVO (medalha) antes do swap; fallback 24px.
                    var le = img.GetComponent<LayoutElement>();
                    if (le == null)
                    {
                        le = img.gameObject.AddComponent<LayoutElement>();
                        var native = img.sprite != null && img.sprite != crest ? img.sprite : null;
                        le.preferredWidth = native != null ? native.rect.width : 24f;
                        le.preferredHeight = native != null ? native.rect.height : 24f;
                    }

                    img.sprite = crest;
                    img.preserveAspect = true;

                    // Rodada 5: prancha selecionada = fundo claro → brasão escuro (como o texto nativo);
                    // versão normal = fundo escuro → brasão claro (cor nativa preservada).
                    if (selectedVersion != null && img.transform.IsChildOf(selectedVersion.transform))
                    {
                        img.color = new Color(0.07f, 0.07f, 0.07f, 1f);
                    }

                    img.gameObject.SetActive(true);
                }
                else
                {
                    img.gameObject.SetActive(false);   // sem brasão → some (não deixa a medalha errada)
                }
            }

            if (!_loggedTabImages)
            {
                _loggedTabImages = true;
                Plugin.Log?.LogInfo($"[CustomClasses][053-tabicon] images=[{string.Join(", ", images.Select(i => $"{i.gameObject.name} w={((RectTransform)i.transform).rect.width:F0} spr={(i.sprite != null ? i.sprite.name : "null")}"))}]");
                // Rodada 3 — diagnóstico do texto: caminho, ativo, enabled, alpha e fonte de cada TMP da aba.
                foreach (var tmp in tmps)
                {
                    Plugin.Log?.LogInfo(
                        $"[CustomClasses][053-tabtext] '{TabPath(tmp.transform, tab.transform)}' active={tmp.gameObject.activeInHierarchy} " +
                        $"enabled={tmp.enabled} text='{tmp.text}' size={tmp.fontSize} alpha={tmp.color.a:F2} font={(tmp.font != null ? tmp.font.name : "null")}");
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (059) style tab: {ex.Message}");
        }
    }

    /// <summary>Caminho relativo de um transform dentro da aba (diagnóstico do rótulo).</summary>
    private static string TabPath(Transform t, Transform root)
    {
        var path = t.name;
        while (t.parent != null && t.parent != root)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }

        return path;
    }
}
