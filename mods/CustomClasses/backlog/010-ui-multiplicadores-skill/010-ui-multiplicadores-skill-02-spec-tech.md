# 010 — UI dos multiplicadores de skill · Spec Técnica

**Mod:** CustomClasses
**Spec funcional:** [010-ui-multiplicadores-skill-01-spec.md](010-ui-multiplicadores-skill-01-spec.md)
**Criado:** 2026-06-07

> **Fonte das refs do EFT:** o dump versionado em `references/eft-decompiled/Assembly-CSharp/` é **parcial** e **não** contém `SkillPanel`/`SkillIcon`/`HoverTooltipArea`/`SimpleTooltip`. As assinaturas abaixo foram extraídas decompilando o **DLL real** `mods/CustomClasses/modded/Client/References/Assembly-CSharp.dll` (== `D:/SPT/EscapeFromTarkov_Data/Managed/Assembly-CSharp.dll`) via `ilspycmd -t <Tipo>`. Reproduzível: `ilspycmd -t EFT.UI.SkillPanel <dll>`. Membros são namespaces/campos **legíveis** (não ofuscados) — estáveis o suficiente p/ field-injection do Harmony, igual ao que o 005 já faz.

## 1. Estratégia

Refino **client-only** da apresentação do 005 (a escala de XP em si — server registry/router + `OnTriggerPatch`/gym — fica intacta). Três efeitos visuais na tela de Skills + uma extensão de payload server→client para carregar o **nome da classe**:

1. **Borda colorida no ícone** — `EFT.UI.SkillIcon` já tem um `Image _border` que o vanilla pinta de branco (normal) ou laranja (elite). Postfix em `SkillIcon.Show(...)` sobrescreve `_border.color` para **verde** (buff) / **vermelho** (debuff) quando a skill tem fator ≠ 1. Sem fator → não toca (vanilla intacto).
2. **Marcador `±X%` + tooltip ao lado do nome** — Postfix em `SkillPanel.method_1()` (o "refresh" da linha, mesmo alvo do 005). Cria/atualiza um `GameObject` filho do `_name` (TextMeshProUGUI) com `▲ +X%` / `▼ -X%` colorido, e um `EFT.UI.HoverTooltipArea` nesse marcador apontando para `ItemUiContext.Instance.Tooltip` (`SimpleTooltip`) com a frase "…devido à Classe **\<Nome\>**". **Remove** o override das setas vanilla `_effectivenessUp/_effectivenessDown` que o 005 acionava (volta ao comportamento original).
3. **Nome da classe no client** — o router `/customclasses/skill-multipliers` passa a devolver `{ className, multipliers }` (hoje devolve só o dict). `className` = a `Edition` do perfil (que **é** o nome da classe — `registry.Set(name, …)` usa `def.Name`, [CustomClassesMod.cs:183](../../modded/Server/CustomClassesMod.cs#L183)).

**Alternativas descartadas:**
- *Tooltip dedicado via TMP `<link>` + `FindIntersectingLink`*: mais frágil/complexo que `HoverTooltipArea` (componente nativo pronto). Descartado. **Resolve o `<!-- review -->`** da spec funcional: o tooltip dedicado É viável (`HoverTooltipArea`+`SimpleTooltip`), não usaremos o fallback de anexar ao `SkillTooltip` nativo.
- *Anexar `±X%` ao `_name.text` (rich text)*: simples mas obriga o hover do tooltip a cobrir o nome inteiro e exige reconstruir o texto-base do nome a cada refresh (risco de acumular/duplicar). Preferido: GameObject separado.
- *Manter o `SkillTooltipPatch` do 005 ("XP da classe: +X%")*: redundante com o tooltip dedicado e com o formato antigo. **Será removido.**

## 2. Pontos de patch

| Alvo (Assembly real via ilspycmd) | Tipo | Motivo |
|---|---|---|
| `EFT.UI.SkillPanel.method_1()` | Postfix | refresh da linha: cria/atualiza marcador `±X%`+tooltip; deixa de mexer nas setas vanilla. Campos: `skillClass`, `_name`. |
| `EFT.UI.SkillIcon.Show(SkillClass, IHealthController, Action<bool,PointerEventData>)` | Postfix | pinta `_border.color` (verde/vermelho) conforme o fator da `skill`. |
| (server) `SkillMultipliersRouter` rota `/customclasses/skill-multipliers` | — | payload passa de `Dictionary` para `{ className, multipliers }`. |

**Assinaturas confirmadas (ilspycmd):**
- `SkillPanel` — `private TextMeshProUGUI _name;`, `private SkillClass skillClass;`, `private GameObject _effectivenessUp/_effectivenessDown;`, `private SkillIcon _skillIcon;`. Método `public void method_1()` (refresh). `Show(SkillClass skill, IHealthController healthController)`.
- `SkillIcon` — `private Image _border;`, `private Image _icon;`, `private SkillClass skillClass;`. `public void Show(SkillClass skill, [CanBeNull] IHealthController healthController, Action<bool, PointerEventData> onHover)`. Vanilla pinta `_border.color` em elite (`new Color32(183,112,0,255)`) / `Color.white`.
- `HoverTooltipArea` — `public void Init(SimpleTooltip tooltip, string text, bool rawText = false)`, `public void SetMessageText(string text, bool rawText = false)`; em `OnPointerEnter` só mostra se `String_0` não-vazio; resolve `ItemUiContext.Instance.Tooltip` no `Awake`.
- `SimpleTooltip` — `public CancellationToken Show(string text, Vector2? offset = null, float delay = 0f, float? maxWidth = null)`. `ItemUiContext.Instance.Tooltip` é um `SimpleTooltip`.

## 3. Novas propriedades F12 (BepInEx)

Nenhuma nova. Reusa `General/ShowMultiplierOnSkills` (já existe, [Plugin.cs:27](../../modded/Client/Plugin.cs#L27)) como master switch dos 3 efeitos. (Tooltip pt-BR já documentado no 005.)

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Server/SkillMultipliersResponse.cs` | CRIAR | DTO record `{ className, multipliers }` (`[JsonPropertyName]`). |
| `modded/Server/SkillMultipliersRouter.cs` | MODIFICAR | devolve o DTO (className = edition quando há multiplicadores). |
| `modded/Client/SkillMultipliers.cs` | MODIFICAR | parseia `{ className, multipliers }`; expõe `ClassName`. |
| `modded/Client/MultiplierFormat.cs` | CRIAR | helper central: cores, `Percent()`, `Marker()`, `TooltipText()` (centraliza strings p/ i18n 008). |
| `modded/Client/Patches/SkillPanelPatch.cs` | MODIFICAR | tira o override das setas vanilla; cria/atualiza marcador `±X%`+`HoverTooltipArea`. |
| `modded/Client/Patches/SkillIconBorderPatch.cs` | CRIAR | postfix em `SkillIcon.Show` → `_border.color`. |
| `modded/Client/Patches/SkillTooltipPatch.cs` | REMOVER | substituído pelo tooltip dedicado do marcador. |
| `modded/Client/Plugin.cs` | MODIFICAR | registra `SkillIconBorderPatch`; remove `SkillTooltipPatch`. |

## 5. Stubs de código

### SkillMultipliersResponse.cs (server)

```csharp
using System.Text.Json.Serialization;

namespace CustomClasses;

/// <summary>Item 010: payload da rota — fatores + nome da classe (p/ tooltip da UI).</summary>
public sealed record SkillMultipliersResponse
{
    [JsonPropertyName("className")]
    public string? ClassName { get; init; }

    [JsonPropertyName("multipliers")]
    public Dictionary<string, double>? Multipliers { get; init; }
}
```

### SkillMultipliersRouter.cs (server, trecho do route)

```csharp
new RouteAction<EmptyRequestData>(
    "/customclasses/skill-multipliers",
    (url, info, sessionId, output) =>
    {
        var edition = saveServer.GetProfile(sessionId)?.ProfileInfo?.Edition ?? string.Empty;  // ref: SaveServer.cs:118
        var mults = registry.Get(edition);
        var dto = new SkillMultipliersResponse
        {
            ClassName = mults.Count > 0 ? edition : null,   // edition == nome da classe (CustomClassesMod.cs:183)
            Multipliers = mults,
        };
        var json = jsonUtil.Serialize(dto) ?? "{}";
        return new ValueTask<string>(json);
    }),
```

### SkillMultipliers.cs (client — parsing + ClassName)

```csharp
public static string? ClassName { get; private set; }

// dentro de EnsureLoaded(), no lugar do Deserialize<Dictionary<...>>:
var payload = JsonConvert.DeserializeObject<Payload>(json);
if (payload?.Multipliers is null) return;
ClassName = payload.ClassName;
foreach (var kv in payload.Multipliers)
{
    if (Enum.TryParse<ESkillId>(kv.Key, ignoreCase: true, out var id) && Enum.IsDefined(typeof(ESkillId), id))
        Factors[id] = (float)kv.Value;
    else
        Plugin.Log?.LogWarning($"[CustomClasses] multiplicador p/ skill desconhecida '{kv.Key}' — ignorado.");
}

private sealed class Payload
{
    [JsonProperty("className")]  public string? ClassName;
    [JsonProperty("multipliers")] public Dictionary<string, double>? Multipliers;
}
// Reset() também zera ClassName.
```

### MultiplierFormat.cs (client — helper central)

```csharp
using UnityEngine;

namespace CustomClasses.Client;

/// <summary>Formatação central do buff/debuff (cor, %, marcador, tooltip). i18n (008) troca só as strings aqui.</summary>
internal static class MultiplierFormat
{
    public const string GreenHex = "#9ad27a";   // buff  (mesmo verde do 005)
    public const string RedHex   = "#d27a7a";   // debuff
    public static readonly Color Green = new(0.604f, 0.824f, 0.478f, 1f);
    public static readonly Color Red   = new(0.824f, 0.478f, 0.478f, 1f);

    public static bool IsActive(float factor) => Mathf.Abs(factor - 1f) > 1e-4f;
    public static int Percent(float factor) => Mathf.RoundToInt((factor - 1f) * 100f);
    public static Color BorderColor(float factor) => factor > 1f ? Green : Red;

    /// <summary>Marcador da linha: "▲ +50%" verde / "▼ -30%" vermelho (rich text TMP).</summary>
    public static string Marker(float factor)
    {
        var pct = Percent(factor);
        var up = factor > 1f;
        var hex = up ? GreenHex : RedHex;
        var arrow = up ? "▲" : "▼";
        return $"<color={hex}>{arrow} {(pct >= 0 ? "+" : "")}{pct}%</color>";
    }

    /// <summary>Frase do tooltip (rawText — preserva tags). className em negrito; % de buff/debuff colorido.</summary>
    public static string TooltipText(float factor, string? className)
    {
        var pct = Percent(factor);
        var up = factor > 1f;
        var hex = up ? GreenHex : RedHex;
        var word = up ? "buff" : "debuff";
        var amount = $"<color={hex}>{(pct >= 0 ? "+" : "")}{pct}% de {word}</color>";
        var cls = string.IsNullOrWhiteSpace(className) ? "sua Classe" : $"Classe <b>{className}</b>";
        return $"Você possui {amount} nessa skill devido à {cls}";
    }
}
```

### SkillIconBorderPatch.cs (client — novo)

```csharp
using System;
using System.Reflection;
using EFT;          // SkillClass
using EFT.UI;       // SkillIcon
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine.UI;   // Image

namespace CustomClasses.Client;

/// <summary>
///     (010) Borda colorida no ícone da skill: verde=buff / vermelho=debuff. Postfix em SkillIcon.Show.
///     ref: Assembly-CSharp.dll → EFT.UI.SkillIcon { Image _border; Show(SkillClass skill, IHealthController, Action) }.
///     Risco conhecido: SkillIcon.Class3053.method_1 reseta _border.color em StimulatorBuffEvent (só em raid c/ stim) — ver §7.
/// </summary>
internal class SkillIconBorderPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(SkillIcon), nameof(SkillIcon.Show));   // overload único
    }

    [PatchPostfix]
    private static void Postfix(SkillClass skill, Image ____border)
    {
        if (!Plugin.ShowOnUi || skill is null || ____border is null) return;
        try
        {
            SkillMultipliers.EnsureLoaded();
            if (SkillMultipliers.TryGet(skill.Id, out var f) && MultiplierFormat.IsActive(f))
            {
                ____border.color = MultiplierFormat.BorderColor(f);
            }
            // sem multiplicador: não mexe (deixa a cor vanilla — branco/elite).
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] skill icon border falhou: {ex.Message}");
        }
    }
}
```

### SkillPanelPatch.cs (client — modificado: marcador + tooltip)

```csharp
using System;
using System.Reflection;
using EFT;          // SkillClass
using EFT.UI;       // SkillPanel, HoverTooltipArea, ItemUiContext, SimpleTooltip
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;        // TextMeshProUGUI
using UnityEngine;  // GameObject, RectTransform

namespace CustomClasses.Client;

/// <summary>
///     (010) Marcador "▲ +X%/▼ -X%" à direita do nome + tooltip dedicado da classe. Postfix em SkillPanel.method_1.
///     ref: Assembly-CSharp.dll → EFT.UI.SkillPanel { TextMeshProUGUI _name; SkillClass skillClass; }.
///     Não mexe mais nas setas vanilla _effectivenessUp/Down (volta ao comportamento original).
/// </summary>
internal class SkillPanelPatch : ModulePatch
{
    private const string MarkerName = "CC_MultMarker";

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(SkillPanel), nameof(SkillPanel.method_1));
    }

    [PatchPostfix]
    private static void Postfix(SkillClass ___skillClass, TextMeshProUGUI ____name)
    {
        if (!Plugin.ShowOnUi || ___skillClass is null || ____name is null) return;
        try
        {
            SkillMultipliers.EnsureLoaded();
            var has = SkillMultipliers.TryGet(___skillClass.Id, out var f) && MultiplierFormat.IsActive(f);

            var marker = GetOrCreateMarker(____name);   // filho reusável (célula reciclada)
            var tmp = marker.GetComponent<TextMeshProUGUI>();
            var area = marker.GetComponent<HoverTooltipArea>();

            if (!has)
            {
                tmp.text = string.Empty;
                area.SetMessageText(string.Empty);   // String_0 vazio → não abre tooltip
                marker.SetActive(false);
                return;
            }

            tmp.text = MultiplierFormat.Marker(f);
            area.Init(ItemUiContext.Instance.Tooltip, MultiplierFormat.TooltipText(f, SkillMultipliers.ClassName), rawText: true);
            marker.SetActive(true);
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] skill panel marker falhou: {ex.Message}");
        }
    }

    /// <summary>Acha (ou cria 1x) o marcador como filho do _name, herdando fonte/material. Reusa em células recicladas.</summary>
    private static GameObject GetOrCreateMarker(TextMeshProUGUI name)
    {
        var existing = name.transform.Find(MarkerName);
        if (existing != null) return existing.gameObject;

        var go = new GameObject(MarkerName, typeof(RectTransform));
        go.transform.SetParent(name.transform, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.font = name.font;
        tmp.fontSharedMaterial = name.fontSharedMaterial;
        tmp.fontSize = name.fontSize;
        tmp.alignment = TextAlignmentOptions.MidlineRight;   // marcador à direita da coluna do nome
        tmp.enableWordWrapping = false;
        tmp.raycastTarget = true;                            // necessário p/ HoverTooltipArea receber o ponteiro

        go.AddComponent<HoverTooltipArea>();                 // Awake resolve ItemUiContext.Instance.Tooltip
        return go;
    }
}
```

### Plugin.cs (client — registro dos patches)

```csharp
new OnTriggerPatch().Enable();
new WorkoutBehaviourPatch().Enable();    // (a) gym
new SkillPanelPatch().Enable();          // (010) marcador ±X% + tooltip dedicado
new SkillIconBorderPatch().Enable();     // (010) borda colorida no ícone
// SkillTooltipPatch removido — substituído pelo tooltip dedicado do marcador.
```

## 6. Fluxo de dados

```
[server] CustomClassesMod.RegisterClass → registry.Set(edition=nome da classe, {skill:fator})   (CustomClassesMod.cs:183)
   └─ rota /customclasses/skill-multipliers → { className: edition, multipliers:{...} }            (SkillMultipliersRouter.cs)
[client] SkillMultipliers.EnsureLoaded() → fetch → Factors[ESkillId]=fator + ClassName            (SkillMultipliers.cs)
[abrir tela Skills]
   ├─ SkillIcon.Show(skill,…)  ──Postfix──▶ SkillIconBorderPatch → _border.color = verde/vermelho  (SkillIcon._border)
   └─ SkillPanel.method_1()    ──Postfix──▶ SkillPanelPatch
            ├─ cria/reusa GO "CC_MultMarker" filho de _name (TextMeshProUGUI + HoverTooltipArea)
            ├─ tmp.text = "▲ +X%"/"▼ -X%" (MultiplierFormat.Marker)
            └─ area.Init(ItemUiContext.Instance.Tooltip, MultiplierFormat.TooltipText(f, ClassName), rawText:true)
                  └─ hover no marcador → SimpleTooltip.Show("…devido à Classe <b>Nome</b>")        (HoverTooltipArea/SimpleTooltip)
```

## 7. Riscos e dependências

- **Reset da borda por stim buff:** `SkillIcon.Class3053.method_1` faz `_border.color = elite?laranja:branco` quando `StimulatorBuffEvent` dispara (só **em raid** com estimulante de SkillRate ativo). Na tela de Skills do menu/hideout não há stim → sem reset. Se o reset incomodar em raid, mover a pintura p/ um postfix adicional em `SkillIcon` que rode no refresh — fica como follow-up, **não** bloqueia (cenário raro). Registrar como ponto da review.
- **Células recicladas (scroll):** a lista reusa `SkillPanel`/`SkillIcon`. O marcador é **reusado** (busca por `CC_MultMarker`) e **sempre** reescrito/escondido a cada `method_1` (cobre o corner case "vazar entre skills"). A borda é repintada a cada `Show`; quando a skill reciclada não tem fator, **não** repintamos — risco de "borda colorida vazar" de uma skill anterior para uma sem-fator na mesma célula reciclada. **Mitigação:** no `SkillIconBorderPatch`, quando `!IsActive`, resetar explicitamente `____border.color = Color.white` (exceto elite). Incorporar no code-mod. **(ponto de atenção)**
- **`ItemUiContext.Instance` nulo:** improvável na tela de Skills (existe no menu/hideout). `Init` é chamado dentro de try/catch; se nulo, loga e segue (marcador aparece sem tooltip). `HoverTooltipArea.Awake` também resolve sozinho.
- **`TextAlignmentOptions.MidlineRight` / posição:** ajuste fino visual validado in-game; o anchoring esticado (0,0)-(1,1) sobre o `_name` mantém o marcador na direita sem empurrar layout (corner case "nome longo": pode sobrepor o fim do nome — aceitável; nome legível no tooltip nativo do ícone).
- **Compat 005:** o payload da rota muda de forma; o client é atualizado no mesmo build → sem janela de incompatibilidade (client e server do mod sobem juntos). Mods externos não consomem essa rota.
- **i18n (008):** todas as strings pt-BR centralizadas em `MultiplierFormat` — o 008 troca só ali.
- **Plugin BepInEx:** trocar DLL do client exige **restart do jogo** (não só do server).

## 8. Checklist de implementação

- [ ] `SkillMultipliersResponse.cs` (DTO) criado.
- [ ] `SkillMultipliersRouter.cs` devolve `{ className, multipliers }` (className só quando há fatores).
- [ ] `SkillMultipliers.cs` parseia o novo payload + expõe `ClassName`; `Reset()` zera `ClassName`.
- [ ] `MultiplierFormat.cs` criado (cores, Percent, Marker, TooltipText).
- [ ] `SkillPanelPatch.cs`: remove override das setas vanilla; cria/reusa marcador + `HoverTooltipArea`; esconde quando sem fator.
- [ ] `SkillIconBorderPatch.cs`: pinta `_border`; **reseta p/ branco quando sem fator** (não-elite).
- [ ] `SkillTooltipPatch.cs` removido; `Plugin.cs` atualizado (registra border, remove tooltip).
- [ ] `/compile-mod CustomClasses` (server + client) 0 warn/err.
- [ ] Playtest: buff→borda/seta/texto verde + tooltip com nome da classe em negrito; debuff→vermelho; skill sem fator→vanilla; rolar a lista não vaza cor/marcador.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-07 | Spec técnica criada via `/create-technical-spec` (refs via ilspycmd no Assembly-CSharp.dll real; SkillPanel/SkillIcon/HoverTooltipArea/SimpleTooltip confirmados) |
