using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using EFT.UI;       // MenuScreen
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;
using UnityEngine;
using UnityEngine.UI;   // Image

namespace CustomClasses.Client;

/// <summary>
///     (015) Identidade da classe no painel do jogador do Menu-Overhaul, em 3 linhas:
///     linha 1 = [ícone + nome] na cor da classe; linha 2 = [NOME DA CLASSE] (CAPSLOCK, cor da classe);
///     o restante (EXP/nível) em branco para contraste. Sem o Menu-Overhaul o painel não existe → no-op.
///     Idempotente: reaplica a cada Show; cria a linha de classe e o ícone uma única vez (Find por nome).
///     ref: EFT.UI.MenuScreen.Show(Profile, MatchmakerPlayerControllerClass, ESessionMode);
///          Menu-Overhaul cria MainMenuPlayerModelView/BottomField/NicknameText (PlayerProfileFeaturesPatch).
/// </summary>
internal class MenuClassIdentityPatch : ModulePatch
{
    private const string ClassLineName = "CC_ClassLine";
    private const string MenuIconName = "CC_MenuIcon";

    // 067: última MenuScreen montada + a coroutine de aplicação em curso, p/ re-aplicar a cor AO VIVO quando o
    // usuário troca a cor no F12 (RefreshColors). O == do Unity detecta a instância destruída (troca de cena).
    private static MenuScreen? _lastMenu;
    private static Coroutine? _applyCo;

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.GetDeclaredMethods(typeof(MenuScreen))
            .First(m => m.Name == nameof(MenuScreen.Show) && m.GetParameters().Length == 3);
    }

    [PatchPostfix]
    private static void Postfix(MenuScreen __instance)
    {
        _lastMenu = __instance;   // 067: cacheia p/ o RefreshColors reinvocar ao vivo (mesmo com o toggle off)

        if (!Plugin.ShowClassOnPlayerName)
        {
            MenuOverhaulBridge.RestoreAccent();   // desligado → cor original do Menu-Overhaul
            return;
        }

        try
        {
            SkillMultipliers.EnsureLoaded();
            if (string.IsNullOrEmpty(SkillMultipliers.ClassName))
            {
                MenuOverhaulBridge.RestoreAccent();   // perfil vanilla → cor original do Menu-Overhaul
                return;
            }

            if (_applyCo != null)
            {
                Plugin.Instance?.StopCoroutine(_applyCo);   // 067: encerra um apply órfão de uma abertura anterior
            }

            _applyCo = Plugin.Instance?.StartCoroutine(ApplyToMenu(__instance));   // 067: track p/ o refresh cancelar
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] menu identity falhou: {ex.Message}");
        }
    }

    /// <summary>
    ///     067 — re-aplica a identidade/cor no menu quando o usuário troca a cor de uma classe no F12 (assinado a
    ///     <c>PerksConfig.ClassColorsChanged</c> no Plugin). Como o F12 abre POR CIMA do menu, é aqui que a mudança
    ///     é vista na hora: reexecuta o <see cref="ApplyToMenu"/>, que relê <c>SkillMultipliers.NameColor</c> (já
    ///     resolvido com o override) e re-empurra pro AccentColor do Menu-Overhaul. No-op fora do menu.
    /// </summary>
    internal static void RefreshColors()
    {
        // == do Unity: menu destruído (em raid / outra cena) → null. activeInHierarchy: menu oculto → não re-roda.
        if (!Plugin.ShowClassOnPlayerName || _lastMenu == null || !_lastMenu.gameObject.activeInHierarchy)
        {
            return;
        }

        var runner = Plugin.Instance;
        if (runner == null)
        {
            return;
        }

        if (_applyCo != null)
        {
            runner.StopCoroutine(_applyCo);   // cancela um apply em curso (evita coroutines sobrepostas ao arrastar o picker)
        }

        _applyCo = runner.StartCoroutine(ApplyToMenu(_lastMenu));
    }

    /// <summary>
    ///     ref: AUD-01-01 — transform do painel do Menu-Overhaul, cacheado entre aberturas do menu.
    ///     O <c>==</c> do Unity detecta a instância destruída; a atividade é conferida à parte (PA-02-04).
    /// </summary>
    private static Transform? _cachedPmv;

    private static IEnumerator ApplyToMenu(MenuScreen menu)
    {
        // ref: AUD-01-01 · PA-01-05 — sem o Menu-Overhaul o painel NUNCA existe, e o "no-op" custava 60
        // buscas GLOBAIS na cena (GameObject.Find percorre a hierarquia inteira por nome) + 90 frames de
        // coroutine viva, a cada MenuScreen.Show e a cada evento do picker de cor. IsPresent é O(1).
        //
        // ⚠️ Este bail PRESERVA o comportamento — cadeia provada (PA-01-05):
        //   1. `MainMenuPlayerModelView` é criado e NOMEADO pelo Menu-Overhaul
        //      (mods/SPT-Menu-Overhaul/modded/Patches/PlayerProfileFeaturesPatch.cs:302).
        //   2. Sem o MO esse objeto não existe → `nick` fica null nas 60 iterações.
        //   3. O guard abaixo (`menu == null || nick == null || nickname vazio`) já fazia `yield break`
        //      ANTES do FixTopGlow → nada daqui para baixo rodava hoje sem o MO. Sair antes não remove feature.
        //   (`Environment UI`/`Glow Canvas`/`TopGlowPve` são objetos do EFT que o MO apenas muta —
        //    MenuVisibilityController.cs:14-15 — mas isso é irrelevante: o caminho já era inalcançável.)
        if (!MenuOverhaulBridge.IsPresent)
        {
            yield break;
        }

        // PERF-INSTR AUD-01-01 — temporary, remove after validation
        var sw = PerkDiag.Enabled ? System.Diagnostics.Stopwatch.StartNew() : null;
        var finds = 0;

        // O Menu-Overhaul cria o painel do jogador de forma assíncrona (idempotente) → espera alguns frames.
        TextMeshProUGUI? nick = null;
        for (var i = 0; i < 60 && nick == null; i++)
        {
            // ⚠️ ref: PA-02-04 — o `==` do Unity cobre o objeto DESTRUÍDO, mas não o DESATIVADO. E o
            // GameObject.Find só encontra ATIVOS: hoje um painel velho desativado é ignorado sozinho a cada
            // frame; com cache, ele sequestraria a identidade do painel novo (escreveríamos num invisível).
            if (_cachedPmv == null || !_cachedPmv.gameObject.activeInHierarchy)
            {
                _cachedPmv = GameObject.Find("MainMenuPlayerModelView")?.transform;
                finds++;
            }

            nick = _cachedPmv != null
                ? _cachedPmv.Find("BottomField/NicknameText")?.GetComponent<TextMeshProUGUI>()
                : null;

            if (nick == null)
            {
                // ref: AUD-01-01 — poll a cada 3 frames em vez de todo frame. Mesma janela total de espera
                // (60 frames), 1/3 das varreduras globais no pior caso.
                yield return null;
                yield return null;
                yield return null;
                i += 2;
            }
        }

        // PERF-INSTR AUD-01-01 — temporary, remove after validation
        if (sw != null)
        {
            Plugin.Log?.LogInfo($"[CustomClasses][perf/AUD-01-01] menu apply: finds={finds} found={nick != null} ms={sw.Elapsed.TotalMilliseconds:F1}");
        }

        var nickname = SkillMultipliers.Nickname;
        if (menu == null || nick == null || string.IsNullOrEmpty(nickname))
        {
            yield break;   // sem Menu-Overhaul / fechou / sem nickname → no-op
        }

        var baseColor = ClassIdentityView.ResolveColor(SkillMultipliers.NameColor, Color.white);

        try
        {
            // EXP, top glow, botões e luzes do menu seguem a cor da classe (AccentColor do Menu-Overhaul).
            MenuOverhaulBridge.SetAccent(baseColor);

            // Linha 1: nome do jogador com GRADIENTE (vertex vence o .color que o MO aplica).
            nick.text = nickname;
            ClassIdentityView.ApplyGradient(nick, baseColor);

            // Linha 1: ícone da classe à esquerda do nome (tint silhueta). 006-fix: tamanho proporcional à fonte do nome.
            var icon = GetOrCreateMenuIcon(nick.transform);
            ClassIdentityView.ApplyClassIcon(icon, SkillMultipliers.IconFile, SkillMultipliers.NameColor, ClassIdentityView.IconSizeFor(nick));

            // Linha 2: nome da classe (CAPSLOCK) com gradiente, logo abaixo do nome.
            var classLine = GetOrCreateClassLine(nick.transform.parent, nick);
            classLine.text = SkillMultipliers.ClassName!.ToUpperInvariant();
            ClassIdentityView.ApplyGradient(classLine, baseColor);
            classLine.transform.SetSiblingIndex(nick.transform.GetSiblingIndex() + 1);

            // Glow/luzes/logo: força o MO a reaplicar a cor agora — o ambiente já existe (o nick apareceu) e o
            // AccentColor já é a cor da classe. Vence a corrida em que o SettingChanged disparou cedo demais.
            MenuOverhaulBridge.ReapplyLayout();
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] menu layout falhou: {ex.Message}");
        }

        // Espera o EFT/MO assentarem (o glow do PvE é (re)montado de forma assíncrona) e corrige o glow.
        // ref: AUD-01-01 — eram 90 frames contados um a um; a espera não precisa de granularidade de frame.
        // Realtime porque o menu não tem timeScale garantido. 90 frames @60fps ≈ 1,5 s.
        yield return new WaitForSecondsRealtime(1.5f);

        FixTopGlow(baseColor);
    }

    /// <summary>
    ///     (06-fix-02) Corrige o glow do topo no modo PvE. O <c>TopGlowPve</c> usa um SPRITE azul/ciano
    ///     (tema PvE); como o Unity UI renderiza <c>sprite × color</c>, o nosso tint laranja vira VERDE.
    ///     Por multiplicação não dá p/ "laranjar" um sprite azul → trocamos o sprite do PvE pelo do
    ///     <c>TopGlowRegular</c> (glow neutro do modo normal) e reaplicamos o tint da classe.
    /// </summary>
    private static void FixTopGlow(Color baseColor)
    {
        try
        {
            var glow = GameObject.Find("Environment UI")?.transform.Find("Common/Glow Canvas");
            var pve = glow?.Find("TopGlowPve")?.GetComponent<Image>();
            if (pve == null)
            {
                return;
            }

            var regular = glow!.Find("TopGlowRegular")?.GetComponent<Image>();
            if (regular != null && regular.sprite != null)
            {
                pve.sprite = regular.sprite;   // sprite neutro → tint laranja × neutro = laranja
            }

            pve.color = new Color(baseColor.r, baseColor.g, baseColor.b, pve.color.a);   // preserva o alpha do glow
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] glowfix falhou: {ex.Message}");
        }
    }

    /// <summary>Cria (1x) ou reusa o Image do ícone, ancorado à esquerda do nome.</summary>
    private static Image GetOrCreateMenuIcon(Transform nickTransform)
    {
        var existing = nickTransform.Find(MenuIconName);
        if (existing != null)
        {
            return existing.GetComponent<Image>();
        }

        var go = new GameObject(MenuIconName, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(nickTransform, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);   // borda esquerda do nome
        rt.pivot = new Vector2(1f, 0.5f);
        rt.sizeDelta = new Vector2(28f, 28f);
        rt.anchoredPosition = new Vector2(-8f, 0f);             // gap à esquerda
        var img = go.GetComponent<Image>();
        img.raycastTarget = false;
        return img;
    }

    /// <summary>Cria (1x) ou reusa o TMP da linha do nome da classe, herdando o estilo do nome.</summary>
    private static TextMeshProUGUI GetOrCreateClassLine(Transform bottomField, TextMeshProUGUI template)
    {
        var existing = bottomField.Find(ClassLineName);
        if (existing != null)
        {
            return existing.GetComponent<TextMeshProUGUI>();
        }

        var go = new GameObject(ClassLineName, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(bottomField, false);
        var t = go.GetComponent<TextMeshProUGUI>();
        t.font = template.font;
        t.fontSize = template.fontSize * 0.8f;
        t.alignment = template.alignment;
        t.enableWordWrapping = false;
        t.raycastTarget = false;
        return t;
    }
}
