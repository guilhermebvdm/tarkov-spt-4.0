# 059 — Catálogo de propriedades atômicas + fix da aba CLASS · Spec Técnica

**Mod:** CustomClasses
**Spec funcional:** [059-catalogo-propriedades-atomicas-01-spec.md](059-catalogo-propriedades-atomicas-01-spec.md)
**Criado:** 2026-07-02

> Fonte primária: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Alguns tipos de UI (`SkillsAndMasteringScreen`, `NotificationManagerClass`, `EFTHardSettings.StaticIcons`) **não estão** no decompile versionado do repo — são ancorados no **código do mod (§4)** onde já estão em uso e comprovadamente compilam, com confirmação cruzada na DLL instalada (`D:/SPT/…/Assembly-CSharp.dll`, recon desta sessão via `ilspycmd`).

## 1. Estratégia

**Não há novos pontos de patch no Assembly.** O `Postfix` em `SkillsAndMasteringScreen.Show` (monta a aba CLASS)
e o `Postfix` de notificação em `GameWorld` já existem. Este item é **refactor do código do mod** (client-side,
só exibição):

- **Fatia A — aba CLASS** (dentro do `Postfix` existente): clonar a `_masteringTab` (estado normal), **esconder
  o conteúdo nativo do Tab** (texto + ícone) **preservando o fundo**, sobrepor um label próprio `[ícone][CLASS]`,
  e reposicionar a aba à esquerda da SKILLS **sem mover** SKILLS/MASTERING. Manipula membros do `Tab`
  (canônico, via campos serializados) — não patcheia o `Tab`.
- **Fatia B — catálogo + display**: reescrever `PerksCatalog` para um modelo **`PerkGroup` → `PerkLine[]`** onde
  perk/drawback + o token de valor são **derivados** de `Multiplier` + `Polarity` + `ValueFormat` (biblioteca
  compartilhada por chave). Trocar o painel de 1 lista para **2 colunas** (perks/drawbacks). Atualizar os
  consumidores (`SkillsClassTabPatch` cards, `RaidPerksNotificationPatch` compacto, `PerkDiagnostics`), remover
  o código morto (`SkillsPerksPanelPatch` + `BuildPanelText`).

Alternativa descartada: setar o texto nativo do Tab nas 2 versões (tentado no 053 — o `_selectedVersion` não
renderiza confiável). Por isso o **label próprio sobreposto**.

## 2. Pontos de patch

Nenhum patch Harmony **novo**. Alvos manipulados (não patcheados) / patches existentes reusados:

| Alvo | Tipo | Motivo |
|---|---|---|
| [`Tab.cs:17`](../../../../references/eft-decompiled/Assembly-CSharp/Tab.cs#L17) `_normalVersion` / [`:20`](../../../../references/eft-decompiled/Assembly-CSharp/Tab.cs#L20) `_selectedVersion` | leitura (reflection) | as 2 "versões" gráficas; esconder texto/ícone nativos preservando o fundo |
| [`Tab.cs:26`](../../../../references/eft-decompiled/Assembly-CSharp/Tab.cs#L26) `_targetImage` | leitura | fundo/parchment — **não** esconder; distinguir do ícone |
| [`Tab.cs:37`](../../../../references/eft-decompiled/Assembly-CSharp/Tab.cs#L37) `LocalizedText` | escrita | anular p/ o texto não ser re-localizado |
| [`Tab.cs:61`](../../../../references/eft-decompiled/Assembly-CSharp/Tab.cs#L61) `OnSelectionChanged` | subscribe | reaplicar o label do meu overlay na (de)seleção |
| [`Tab.cs:147`](../../../../references/eft-decompiled/Assembly-CSharp/Tab.cs#L147) `UpdateVisual(bool)` | chamada | forçar visual normal em SKILLS/MASTERING (já usado no 053) |
| `SkillsAndMasteringScreen.Show(Profile, InventoryController, IHealthController)` | **Postfix existente** | [`SkillsClassTabPatch.cs:64-70`](../../modded/Client/Patches/SkillsClassTabPatch.cs#L64) — já monta a aba; **confirmado na DLL** (recon) |
| `NotificationManagerClass.DisplayMessageNotification(text, ENotificationDurationType.Long)` | chamada existente | [`RaidPerksNotificationPatch.cs:73`](../../modded/Client/Patches/RaidPerksNotificationPatch.cs#L73) — toast pequeno → notificação **compacta** |
| `EFTHardSettings.Instance.StaticIcons.SkillIdSprites` (`Dictionary<ESkillId, Sprite>`) | leitura existente | [`PerksCatalog.IconSprite`](../../modded/Client/PerksCatalog.cs) — sprite do ícone; **confirmado na DLL** |

## 3. Novas propriedades F12 (BepInEx)

**Opcional** — só se a posição da aba não fechar no cálculo (`sRt.x − larguraCLASS − gap`). Fazer *só se necessário*:

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| `Perks — UI` | `Class Tab — X offset` | float | `0` | −400 a 400 | sim | Ajuste fino da posição horizontal do botão da aba CLASS (px). Só use se a aba não alinhar. |

> Se adicionada: campo em `PerksConfig.cs` + `Config.Bind` no `Plugin.cs` (Awake) + linha em `PROPRIEDADES.md`.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Client/PerksCatalog.cs` | MODIFICAR | Reescrever: `Polarity`/`ValueFormat` enums, `PerkLine` (derivação), `PerkGroup`, `Library`, `ByClass` (chaves de grupo), `LocalGroups()`, `IconSprite(PerkGroup)`, `BuildNotificationText()` compacto. Remover `Entry`/`BuildPanelText`/`SplitNameEffect`. `using System.Linq`. |
| `modded/Client/MultiplierFormat.cs` | MODIFICAR | Helper `ValueToken(PerkLine)` reusando `Percent(factor)` ([`:21`](../../modded/Client/MultiplierFormat.cs#L21)) + `GreenHex`/`RedHex` ([`:11-12`](../../modded/Client/MultiplierFormat.cs#L11)). |
| `modded/Client/Patches/SkillsClassTabPatch.cs` | MODIFICAR | Fatia A (label overlay + posição). `BuildPanel` → 2 colunas (`PerksCol`/`DrawbacksCol`). `RefreshPanel` particiona por `group.IsPerk`. `BuildCard`→`BuildGroupCard`. Remover `PillifyValues`. |
| `modded/Client/Patches/RaidPerksNotificationPatch.cs` | MODIFICAR | Usa o `BuildNotificationText()` compacto (1 linha/grupo). |
| `modded/Client/PerkDiagnostics.cs` | MODIFICAR | `AppendPerkList` → grupos + linhas (`group.NameEn` + `line` EN), marcador por `line.IsPerk` ([`:141`](../../modded/Client/PerkDiagnostics.cs#L141)). |
| `modded/Client/Patches/SkillsPerksPanelPatch.cs` | **DELETAR** | Único caller de `BuildPanelText`; já desabilitado no `Plugin.cs`. Confirmar por grep que nada mais referencia. |

## 5. Stubs de código

```csharp
// modded/Client/PerksCatalog.cs  (modelo novo — compilável)
using System;
using System.Collections.Generic;
using System.Linq;
using EFT;            // ESkillId
using UnityEngine;    // Sprite

namespace CustomClasses.Client;

internal enum Polarity { HigherBetter, LowerBetter }
internal enum ValueFormat { Percent, Multiplier, Flag }

internal sealed class PerkLine
{
    public string LabelEn = "", LabelPt = "";
    public ValueFormat Format;
    public float Multiplier = 1f;
    public Polarity Polarity;
    public bool FlagIsPerk;
    public bool Pending;

    public bool IsPerk => Format == ValueFormat.Flag
        ? FlagIsPerk
        : (Polarity == Polarity.HigherBetter) == (Multiplier > 1f);
    public string Label => GameLocale.IsPortuguese ? LabelPt : LabelEn;
    public string ValueToken => MultiplierFormat.ValueToken(this);           // "+30%" / "×0.85" / ""
    public string Text => (ValueToken.Length > 0 ? ValueToken + " " : "") + Label;
}

internal sealed class PerkGroup
{
    public string NameEn = "", NamePt = "";
    public ESkillId? Icon, IconAlt;
    public PerkLine[] Lines = Array.Empty<PerkLine>();
    public bool IsPerk => Lines.Length > 0 && Lines[0].IsPerk;
    public string Name => GameLocale.IsPortuguese ? NamePt : NameEn;
    public bool AllPending => Lines.Length > 0 && Lines.All(l => l.Pending);
    public bool IsHomogeneous => Lines.All(l => l.IsPerk == Lines[0].IsPerk); // invariante (logar aviso se false)
}

internal static partial class PerksCatalog
{
    private static PerkLine P(string en, string pt, ValueFormat fmt, float mult, Polarity pol, bool pending = false)
        => new() { LabelEn = en, LabelPt = pt, Format = fmt, Multiplier = mult, Polarity = pol, Pending = pending };
    private static PerkLine Flag(string en, string pt, bool isPerk, bool pending = false)
        => new() { LabelEn = en, LabelPt = pt, Format = ValueFormat.Flag, FlagIsPerk = isPerk, Pending = pending };

    private static readonly Dictionary<string, PerkGroup> Library = new(StringComparer.OrdinalIgnoreCase)
    {
        ["pack_mule"] = new() { NameEn = "Pack Mule", NamePt = "Mula de Carga", Icon = ESkillId.Strength,
            Lines = new[] { P("carry limit", "limite de carga", ValueFormat.Percent, 1.3f, Polarity.HigherBetter) } },
        ["heavy_frame"] = new() { NameEn = "Heavy Frame", NamePt = "Estrutura Pesada", Icon = ESkillId.Endurance,
            Lines = new[] {
                P("move speed", "velocidade", ValueFormat.Percent, 0.9f, Polarity.HigherBetter),
                P("hunger/thirst", "fome/sede", ValueFormat.Percent, 1.3f, Polarity.LowerBetter) } },
        // … demais grupos (bunker, bulwark, iron_lungs, combat_medic{Pending}, quick_hands{Flag,Pending}, …)
    };

    private static readonly Dictionary<string, string[]> ByClass = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Tank"]      = new[] { "pack_mule", "bulwark", "bunker", "heavy_frame" },
        ["Scavenger"] = new[] { "quick_hands", "silent_looter", "pack_mule", "overladen" },
        // … Combat Medic / Rifleman / Hunter / Stealth
    };

    internal static PerkGroup[]? LocalGroups()
    {
        var key = SkillMultipliers.ClassNameEn;   // gating idioma-independente
        return key != null && ByClass.TryGetValue(key, out var keys)
            ? keys.Select(k => Library.TryGetValue(k, out var g) ? g : null).Where(g => g != null).ToArray()!
            : null;
    }

    internal static Sprite? IconSprite(PerkGroup g)   // Icon → IconAlt (igual ao 053)
    {
        try
        {
            var dict = EFTHardSettings.Instance?.StaticIcons?.SkillIdSprites;   // Dictionary<ESkillId,Sprite>
            if (dict == null) return null;
            var s = g.Icon != null ? dict.GetValueOrDefault(g.Icon.Value) : null;
            return s == null && g.IconAlt != null ? dict.GetValueOrDefault(g.IconAlt.Value) : s;
        }
        catch { return null; }
    }
}
```

```csharp
// modded/Client/MultiplierFormat.cs  (helper do token — PA-01-02: Mathf.Abs, sem "2−m")
internal static string ValueToken(PerkLine l) => l.Format switch
{
    ValueFormat.Percent    => (l.Multiplier > 1f ? "+" : "−") + Mathf.RoundToInt(Mathf.Abs(l.Multiplier - 1f) * 100f) + "%",
    ValueFormat.Multiplier => "×" + l.Multiplier.ToString("0.##"),
    _                      => "",   // Flag (qualitativa)
};
```
> Reusa a mesma cor/estilo do `MultiplierFormat` ([`GreenHex`/`RedHex`:11-12](../../modded/Client/MultiplierFormat.cs#L11)); `Percent(factor)` existente ([`:21`](../../modded/Client/MultiplierFormat.cs#L21)) fica p/ os patches de skill do 010.

```csharp
// SkillsClassTabPatch.cs — Fatia A (esboço do label overlay; preserva o fundo nativo)
private static void StyleClassTab(Tab tab, string label)
{
    var normal   = AccessTools.Field(typeof(Tab), "_normalVersion")?.GetValue(tab) as GameObject;   // ref: Tab.cs:17
    var selected = AccessTools.Field(typeof(Tab), "_selectedVersion")?.GetValue(tab) as GameObject; // ref: Tab.cs:20
    var target   = AccessTools.Field(typeof(Tab), "_targetImage")?.GetValue(tab) as Image;          // ref: Tab.cs:26 (fundo — NÃO esconder)
    foreach (var v in new[] { normal, selected })
        foreach (var tmp in v?.GetComponentsInChildren<TextMeshProUGUI>(true) ?? Array.Empty<TextMeshProUGUI>())
            tmp.text = "";                                     // esconde texto nativo
    // esconder Images de ícone nativas (name contém "icon"), preservando `target`; overlay [ícone][CLASS] no root do Tab (idempotente).
}
```

## 6. Fluxo de dados

```
[Skills abre] → SkillsAndMasteringScreen.Show → [Postfix existente] SkillsClassTabPatch
   Fatia A: clona _masteringTab → CLASS; StyleClassTab (esconde nativo, overlay [ícone][CLASS]); reposiciona à esquerda
   Fatia B: ClassTabController.Show → RefreshPanel
        → PerksCatalog.LocalGroups()  (ClassNameEn → Library)
        → particiona group.IsPerk  →  PerksCol (esquerda) / DrawbacksCol (direita)
        → BuildGroupCard: [ícone group][NameGroup] + por PerkLine: [chip line.ValueToken (cor line.IsPerk)][line.Label]
[Raid inicia] → GameWorld → [Postfix existente] RaidPerksNotificationPatch
        → BuildNotificationText()  (1 linha/grupo, cor group.IsPerk)  → DisplayMessageNotification (compacto)
```

Derivação (prova): `line.IsPerk = (Polarity==HigherBetter) == (mult>1)` — Bulwark `damage taken ×0.85 · LowerBetter`
→ `false==false` = **perk**; Shaky Hands `recoil ×1.25 · LowerBetter` → `false==true` = **drawback**.

**Polaridade por propriedade** (canônico p/ o `/code-mod` — PA-01-01):

| `HigherBetter` (maior = melhor) | `LowerBetter` (menor = melhor) |
|---|---|
| move speed · carry limit · ergonomics · melee damage · breath-hold duration · draw/aim speed | damage taken · recoil · hunger/thirst drain · aim-punch · noise/volume · ADS time · inertia · move-while-ADS |

Convenções de autoria: **(a)** perk **condicional** (Adrenaline — janela de combate; PA-01-03) → o qualificador
entra no `LabelEn/Pt` da linha (ex.: `recoil (combat window)` / `recuo (janela de combate)`), **sem** campo novo.
**(b)** a **notificação** lista **todos** os grupos (nome colorido por `IsPerk`); o marcador **"em breve"** dos
deferidos fica **só no painel** (PA-01-04).

## 7. Riscos e dependências

- **Patches existentes em `modded/Client/Patches/`:** `SkillsClassTabPatch` (modificado), `RaidPerksNotificationPatch`
  (modificado), `SkillsPerksPanelPatch` (**deletado** — confirmar 0 refs no `Plugin.cs`). Nenhum conflito novo.
- **Tab fix empírico:** distinguir "ícone nativo" de "fundo" depende dos nomes dos Images → **log `[053-tabicon]`**
  guia; largura da aba pode ser **0 pré-layout** → usar `sRt.rect.width` como proxy ou `ForceRebuildLayoutImmediate`.
- **Notificação:** `DisplayMessageNotification` é toast pequeno → **compacto obrigatório** (1 linha/grupo).
- **`PillifyValues` vira código morto** após o refactor → remover (evita confusão).
- **Ordem de init:** sem mudança — os patches já registram no `Awake` do `Plugin`.
- **Sessão paralela do editor** (`modded/Server`): este item é **client-only** → sem colisão.

## 8. Checklist de implementação

**Fatia A (aba):**
- [x] Trocar `tabLabel` p/ genérico "CLASS"/"CLASSE".
- [x] `StyleClassTab`: esconder texto + ícone nativos (preservar `_targetImage`); overlay `[ícone][CLASS]` idempotente; reaplicar em `OnSelectionChanged`.
- [x] Reposicionar CLASS à esquerda da SKILLS (`sRt.x − larguraCLASS − gap`, com proxy de largura); não mover SKILLS/MASTERING.
- [x] Compile → checkpoint.

**Fatia B (catálogo + display):**
- [x] `PerksCatalog`: enums `Polarity`/`ValueFormat`, `PerkLine`/`PerkGroup`, `Library` (dados das 6 classes, deferidos marcados), `ByClass` (chaves), `LocalGroups`, `IconSprite`, `BuildNotificationText` compacto; remover `Entry`/`BuildPanelText`/`SplitNameEffect`; `using System.Linq`.
- [x] `MultiplierFormat.ValueToken` (Percent/Multiplier/Flag).
- [x] `SkillsClassTabPatch`: `BuildPanel` 2 colunas; `RefreshPanel` particiona; `BuildGroupCard`; remover `PillifyValues`; caso vanilla largura total.
- [x] `RaidPerksNotificationPatch` + `PerkDiagnostics` adaptados.
- [x] **Deletar** `SkillsPerksPanelPatch.cs`; grep 0 refs.
- [x] Compile 0/0 → checkpoint.

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid — AP-01 | N/A | UI de **menu**; painel/aba vivem na `SkillsAndMasteringScreen` (destruídos com a tela). O único hook de `GameWorld` (notificação) já existe e **não** muda de lifecycle. |
| 2 | Filtro MainPlayer/Fika — AP-02 | ✅ | Exibição gateada por `SkillMultipliers.ClassNameEn` (perfil local); nenhum patch novo reage a ação de player. Fika: local por cliente (spec §Critérios). |
| 3 | Ofuscados/virtuais por assinatura; overrides — AP-03 | ✅ | `Tab` (tipo global, campos serializados por reflection), `GClass3808`/`SkillsAndMasteringScreen.Show` resolvidos por assinatura/predicado (já no 053). Chamo `Tab.UpdateVisual` (virtual) — não patcheio. |
| 4 | Estado via API canônica; side-effects — AP-04 | ✅ | `Tab.UpdateVisual`/`Select`, `SkillIdSprites`, `DisplayMessageNotification` — todos canônicos. Side-effect da notificação (toast) mapeado → compacto. |
| 5 | Estado entre raids | ✅ | Idempotência da aba (guard por `TabName`) + guard `_lastPanelClass` do painel + reabrir reconstrói (spec §Corner cases / §Critérios). Sem estado raid-scoped. |
| 6 | ConfigEntry sem ambiguidade — AP-05 | N/A | Só o `ClassTabOffsetX` **opcional** (float px, faixa −400..400, neutro=0) — semântica clara; §3. |
| 7 | Reentry-guard — AP-07 | N/A | Nenhuma re-invocação de método patcheado; sem recursão. |
| 8 | Flags/caches vs contexto após troca — AP-08 | ✅ | Troca de classe/reabertura: `_lastPanelClass` revalida antes de reconstruir (spec §Corner cases); a aba não duplica. |

## Histórico

| Data | Evento |
|---|---|
| 2026-07-02 | Spec técnica criada via `/create-technical-spec` |
| 2026-07-02 | Review 01 aplicada: +tabela de polaridade (PA-01-01), stub `ValueToken` com `Mathf.Abs` (PA-01-02), convenções perk condicional (PA-01-03) e notificação `AllPending` (PA-01-04) |
