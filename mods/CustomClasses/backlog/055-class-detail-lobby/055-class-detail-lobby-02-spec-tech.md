# 055 — UI: detalhe da classe no lobby/loading da raid · Spec Técnica

**Mod:** CustomClasses
**Spec funcional:** [055-class-detail-lobby-01-spec.md](055-class-detail-lobby-01-spec.md)
**Criado:** 2026-07-02

> Fonte primária: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/) para tipos do EFT base. **O ponto de entrada deste item é UI do FIKA** (tela de carregamento coop), que **não está** no decompile do EFT — a fonte de verdade é o **código do FIKA** ([references/fika-plugin/Fika.Core/UI/Custom/](../../../../references/fika-plugin/Fika.Core/UI/Custom/), 🥇 coop na hierarquia de evidência) + o **código do mod (§4)** já existente. Toda ref cita `arquivo.cs:linha`.

## 1. Estratégia

**Postfix (soft-detect) em `LoadingScreenUI.AddPlayer(int netId, string nickname)`** (FIKA). Quando a linha do
**player local** é adicionada, anexa um handler de interação (hover) ao `GameObject` daquela linha que **mostra/esconde
um painel de detalhe da classe local** — o **mesmo painel** de 2 colunas do 059, reusado.

- **Sem hard-dependency do FIKA:** o mod já trata deps externas por **soft-detect** (`AccessTools.TypeByName`, padrão
  SAIN em [`Plugin.cs:126`](../../modded/Client/Plugin.cs#L126)). O patch só é habilitado se `TypeByName("LoadingScreenUI") != null`, e o alvo é resolvido por `AccessTools.Method(TypeByName(...), "AddPlayer", …)` — **nenhum tipo FIKA no
  IL do mod** (degrada 100% solo). Reusa a instrução `[SPT-fika-softdetect]`.
- **Player local sem tipo FIKA:** compara `nickname == SkillMultipliers.Nickname` — exatamente o gate do 015 em
  [`PlayerNamePanelPatch.cs:48-49`](../../modded/Client/Patches/PlayerNamePanelPatch.cs#L48). Dispensa `IFikaNetworkManager.NetId` (evita o `Singleton<>` genérico de tipo FIKA).
- **GameObject da linha sem tipo FIKA:** lê o campo privado `_loadingPlayers` (`Dictionary<int,LoadingScreenPlayer>`,
  [`LoadingScreenUI.cs:14`](../../../../references/fika-plugin/Fika.Core/UI/Custom/LoadingScreenUI.cs#L14)) via reflection, indexa por `netId` como `IDictionary`, e faz cast do valor para `UnityEngine.Component` (o `LoadingScreenPlayer` é `MonoBehaviour`) → `.gameObject`. `Component` é Unity, não FIKA.
- **Reuso do painel (DRY):** extrair o construtor/refresh do painel do 059 (`SkillsClassTabPatch.BuildPanel`/`RefreshPanel`
  + cards) para um **`PerksPanelView`** compartilhado; a aba CLASS (053/059) e o loading (055) chamam o mesmo código.

**Alternativas descartadas:** (a) hard-ref `Fika.Core.dll` no csproj — quebra o padrão soft-detect do mod e arrisca
`TypeLoadException` solo; (b) patchar o **lobby** (`MatchMakerUIScript`/`ListPlayer`) — mais persistente, mas o usuário
escolheu o loading; fica como fallback (corner case da spec) se a tela de carregamento se provar curta demais no gate.

## 2. Pontos de patch

| Alvo | Tipo | Motivo |
|---|---|---|
| [`LoadingScreenUI.AddPlayer(int,string)`](../../../../references/fika-plugin/Fika.Core/UI/Custom/LoadingScreenUI.cs#L97) | **Postfix** (soft-detect) | funil único que instancia a linha de cada player no loading; anexa interação na do player local |
| [`LoadingScreenUI._loadingPlayers`](../../../../references/fika-plugin/Fika.Core/UI/Custom/LoadingScreenUI.cs#L14) | leitura (reflection) | `Dictionary<int,LoadingScreenPlayer>` → pega o `GameObject` da linha por `netId` |
| [`LoadingScreenPlayer` (MonoBehaviour)](../../../../references/fika-plugin/Fika.Core/UI/Custom/LoadingScreenPlayer.cs#L5) | cast p/ `Component` | obter `.gameObject` da linha sem referenciar o tipo FIKA |
| [`PlayerNamePanelPatch.cs:48`](../../modded/Client/Patches/PlayerNamePanelPatch.cs#L48) `nickname == SkillMultipliers.Nickname` | padrão reusado | identifica o player **local** sem tipo FIKA |
| [`SkillsClassTabPatch.BuildPanel/RefreshPanel`](../../modded/Client/Patches/SkillsClassTabPatch.cs#L284) (059) | refatorar → `PerksPanelView` | painel de 2 colunas reusado pelos 2 pontos de entrada |
| [`Plugin.cs:126`](../../modded/Client/Plugin.cs#L126) `AccessTools.TypeByName` (SAIN) | padrão reusado | gate de habilitação do patch por presença do FIKA |

## 3. Novas propriedades F12 (BepInEx)

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| `Perks — UI` | `Class Detail on Loading Screen` | bool | `true` | — | não | Mostra o detalhe da sua classe (perks/drawbacks) ao passar o mouse no seu nome na tela de carregamento da raid (FIKA). |

> Campo em `PerksConfig.cs` + `Config.Bind` no `Plugin.cs` + linha em `PROPRIEDADES.md`.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Client/PerksPanelView.cs` | **CRIAR** | Extrai do 059 o painel reutilizável: `Build(Transform, TMP_FontAsset) → GameObject` (header + 2 colunas + watermark + fade-in) e `Refresh(GameObject)` (preenche via `PerksCatalog.LocalGroups()`). Recebe `BuildGroupCard`/`BuildColumn`/`BuildSectionHeader`/`BuildMessageCard`/`ClearChildren` + as classes `CardHover`/`FadeIn`. |
| `modded/Client/Patches/SkillsClassTabPatch.cs` | MODIFICAR | `BuildPanel`/`RefreshPanel` passam a **delegar** para `PerksPanelView.Build`/`Refresh`. Remove os métodos de card movidos (sem duplicação). Comportamento da aba CLASS inalterado. |
| `modded/Client/Patches/ClassDetailLoadingPatch.cs` | **CRIAR** | Postfix soft-detect em `LoadingScreenUI.AddPlayer`; na linha do player local, anexa `LoadingClassHover` (mostra/esconde o painel `PerksPanelView`). Gate por `PerksConfig.ClassDetailOnLoading` + classe local não-nula. |
| `modded/Client/Plugin.cs` | MODIFICAR | Bind do novo F12; habilita `ClassDetailLoadingPatch` dentro de `if (TypeByName("LoadingScreenUI") != null)`. |
| `modded/Client/PerksConfig.cs` | MODIFICAR | + `ClassDetailOnLoading` (bool, seção `Perks — UI`). |
| `PROPRIEDADES.md` | MODIFICAR | Linha do novo toggle na seção `Perks — UI`. |

## 5. Stubs de código

```csharp
// modded/Client/PerksPanelView.cs  (extraído do 059 — compilável)
using TMPro;
using UnityEngine;

namespace CustomClasses.Client;

/// <summary>Painel reutilizável de detalhe da classe (header + 2 colunas perks/drawbacks). Usado pela aba CLASS
/// (053/059) e pelo loading da raid (055). Só exibição; lê a classe local via PerksCatalog/SkillMultipliers.</summary>
internal static class PerksPanelView
{
    internal const string PanelName = "CC_ClassPanel";

    /// <summary>Cria o painel (escondido). Header + Columns(PerksCol/DrawbacksCol) + Watermark + FadeIn.</summary>
    internal static GameObject Build(Transform parent, TMP_FontAsset? font) { /* corpo movido do SkillsClassTabPatch.BuildPanel */ return null!; }

    /// <summary>Preenche header + colunas via PerksCatalog.LocalGroups(). Idempotente por classe.</summary>
    internal static void Refresh(GameObject panel) { /* corpo movido do SkillsClassTabPatch.RefreshPanel */ }

    // + BuildGroupCard / BuildColumn / BuildSectionHeader / BuildMessageCard / ClearChildren (movidos)
}
```

```csharp
// modded/Client/Patches/ClassDetailLoadingPatch.cs
using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CustomClasses.Client;

/// <summary>
///     (055) Detalhe da classe LOCAL na tela de carregamento do FIKA. Postfix soft-detect em
///     LoadingScreenUI.AddPlayer(netId, nickname): na linha do player local (nickname == SkillMultipliers.Nickname)
///     anexa um hover que mostra/esconde o painel PerksPanelView (2 colunas). Sem tipos FIKA no IL (reflection).
///     ref: fika-plugin/Fika.Core/UI/Custom/LoadingScreenUI.cs:97 (AddPlayer) / :14 (_loadingPlayers)
/// </summary>
internal class ClassDetailLoadingPatch : ModulePatch
{
    private static readonly Type? UiType = AccessTools.TypeByName("LoadingScreenUI");
    private static readonly FieldInfo? PlayersField = UiType != null ? AccessTools.Field(UiType, "_loadingPlayers") : null;

    protected override MethodBase GetTargetMethod()
        => AccessTools.Method(UiType, "AddPlayer", new[] { typeof(int), typeof(string) });   // ref: LoadingScreenUI.cs:97

    [PatchPostfix]
    private static void Postfix(object __instance, int netId, string nickname)
    {
        try
        {
            if (PerksConfig.ClassDetailOnLoading?.Value != true) return;
            SkillMultipliers.EnsureLoaded();
            if (SkillMultipliers.ClassName == null) return;                       // classe vanilla → nada
            if (string.IsNullOrEmpty(nickname)
                || !string.Equals(nickname, SkillMultipliers.Nickname, StringComparison.Ordinal)) return;   // só o local (padrão 015)

            // pega o GameObject da linha via _loadingPlayers[netId] (cast p/ Component — sem tipo FIKA).
            if (PlayersField?.GetValue(__instance) is not IDictionary dict || !dict.Contains(netId)) return;
            if (dict[netId] is not Component row) return;

            if (row.GetComponent<LoadingClassHover>() != null) return;            // idempotência
            row.gameObject.AddComponent<LoadingClassHover>();                     // constrói o painel sob demanda (lazy)
        }
        catch (Exception ex) { Plugin.Log?.LogError($"[CustomClasses] (055) class detail loading: {ex.Message}"); }
    }
}

/// <summary>
///     Na linha do player local, mostra o painel de detalhe. PA-01-01: **auto-visível** (não depende de
///     EventSystem/raycast na tela transiente) — monta e exibe no OnEnable; o hover/click é toggle OPCIONAL
///     (só atua se o EventSystem estiver ativo). PA-01-02: fonte vem de um TMP da própria linha.
/// </summary>
internal sealed class LoadingClassHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private GameObject? _panel;

    private void OnEnable() { Ensure(); if (_panel != null) { PerksPanelView.Refresh(_panel); _panel.SetActive(true); } }   // auto-visível (default)

    private void Ensure()
    {
        if (_panel != null) return;
        var font = GetComponentInChildren<TMP_Text>()?.font;                    // PA-01-02: fonte da própria linha (Nickname/Percentage)
        var canvas = GetComponentInParent<Canvas>();                            // parent = Canvas do loading
        _panel = PerksPanelView.Build((canvas != null ? canvas.transform : transform.root), font as TMP_FontAsset);
        // TODO(code-mod): ancorar compacto ao lado da linha; ver §7.
    }

    // hover = toggle OPCIONAL (refinamento; só dispara se houver GraphicRaycaster+EventSystem).
    public void OnPointerEnter(PointerEventData e) { Ensure(); PerksPanelView.Refresh(_panel!); _panel!.SetActive(true); }
    public void OnPointerExit(PointerEventData e) { /* auto-visível: mantém aberto; ver decisão de gate */ }
    private void OnDestroy() { if (_panel != null) Destroy(_panel); }
}
```

## 6. Fluxo de dados

```
[A] FIKA monta a tela de carregamento → LoadingScreenUI.AddPlayer(netId, nickname)   (LoadingScreenUI.cs:97)
      ↓ Postfix (soft-detect, só se TypeByName("LoadingScreenUI") != null)
[B] nickname == SkillMultipliers.Nickname ?  (PlayerNamePanelPatch.cs:48 — padrão local)
      ↓ sim (player local)
[C] _loadingPlayers[netId] → Component.gameObject  (LoadingScreenUI.cs:14)  → AddComponent<LoadingClassHover>
      ↓ hover
[D] PerksPanelView.Build/Refresh → header (SkillMultipliers.ClassName/IconFile/NameColor) +
    2 colunas via PerksCatalog.LocalGroups()  → painel de detalhe (idêntico ao 059)
```

## 7. Riscos e dependências

- **Toca o 059 (recém-entregue):** a extração do `PerksPanelView` move os métodos de card do `SkillsClassTabPatch`. A
  aba CLASS deve continuar idêntica — o `/code-review` + compile validam a paridade. Mitigação: mover sem alterar
  assinaturas/corpo (só relocar + trocar a chamada por `PerksPanelView.*`).
- **Loading transiente:** a tela pode ser curta; o hover pode ter janela pequena. Fallback documentado: painel
  auto-visível (sem hover) ou host no lobby (`MatchMakerUIScript`) — decidir no gate (corner case da spec).
- **Parent/Canvas do painel:** o painel precisa de um `Canvas` ativo do loading; se a hierarquia diferir, ancorar em
  `transform.root`. Posição ao lado da linha é ajuste fino do `/code-mod` + gate.
- **FIKA ausente (solo):** `UiType == null` → patch não habilitado (gate no `Plugin.cs`); nada quebra.
- **Conflito com 015:** pontos distintos (`PlayerNamePanel`/confirmation vs `LoadingScreenUI`/loading) → sem overlap.
- **Estado entre raids:** `LoadingScreenUI` é recriado por raid (`OnDestroy`→`ClearData`, [LoadingScreenUI.cs:120](../../../../references/fika-plugin/Fika.Core/UI/Custom/LoadingScreenUI.cs#L120)); o hover/painel morre junto → sem leak nem resíduo.

## 8. Checklist de implementação

- [ ] Criar `PerksPanelView.cs` movendo `BuildPanel`→`Build`, `RefreshPanel`→`Refresh` + cards/`CardHover`/`FadeIn` do 059.
- [ ] `SkillsClassTabPatch` delega ao `PerksPanelView` (aba CLASS inalterada); remover os métodos movidos.
- [ ] `PerksConfig.ClassDetailOnLoading` (bool, `Perks — UI`) + bind no `Plugin.cs` + `PROPRIEDADES.md`.
- [ ] Criar `ClassDetailLoadingPatch.cs` (Postfix soft-detect + `LoadingClassHover`).
- [ ] Habilitar o patch no `Plugin.cs` sob `if (TypeByName("LoadingScreenUI") != null)`.
- [ ] Compile 0/0; conferir que a aba CLASS (059) segue idêntica (paridade do reuso).

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start/stop idempotentes — AP-01 | ✅ | Sem estado estático de raid; o `LoadingClassHover`/painel vivem no `LoadingScreenPlayer` e morrem com o `LoadingScreenUI.OnDestroy`→`ClearData` (LoadingScreenUI.cs:120). Idempotência: guard `GetComponent<LoadingClassHover>()` no Postfix. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a player — AP-02 | ✅ | Gate `nickname == SkillMultipliers.Nickname` (só player local; §5) — não reage a outros players. É UI, não ação de raid. |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; overrides auditados — AP-03 | ✅ | `AddPlayer(int,string)` resolvido por nome+assinatura via `AccessTools.Method(TypeByName(...))`; método concreto (não virtual), caller único (LoadingScreenUI.cs:104). |
| 4 | Mudança de estado via API canônica; side-effects mapeados — AP-04 | ✅ | Não muda estado do jogo — só cria UI própria (painel filho). Leitura de `_loadingPlayers` é read-only. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA | ✅ | Painel/hover recriados por raid junto com o `LoadingScreenUI`; sem persistência. §7 + spec §Estado entre raids. |
| 6 | Semântica/defaults/faixas de cada ConfigEntry — AP-05 | ✅ | `ClassDetailOnLoading` bool default `true`; estado neutro (false = sem ponto de entrada). §3. |
| 7 | Reentry-guard em método patcheado re-invocado — AP-07 | N/A | Postfix não re-invoca `AddPlayer`; sem recursão. |
| 8 | Flags/caches validados após troca de contexto — AP-08 | ✅ | `PerksPanelView.Refresh` relê `SkillMultipliers`/`PerksCatalog` a cada hover (reflete a classe atual); sem cache de classe no hover. |

## Histórico

| Data | Evento |
|---|---|
| 2026-07-02 | Spec técnica criada via `/create-technical-spec` (Postfix soft-detect no FIKA loading + reuso do painel 059) |
