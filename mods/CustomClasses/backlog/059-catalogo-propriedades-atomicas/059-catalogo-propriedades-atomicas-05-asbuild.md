# 059 — Catálogo de propriedades atômicas + fix da aba CLASS · As-Built

**Mod:** CustomClasses
**Spec funcional:** [059-catalogo-propriedades-atomicas-01-spec.md](059-catalogo-propriedades-atomicas-01-spec.md)
**Spec técnica:** [059-catalogo-propriedades-atomicas-02-spec-tech.md](059-catalogo-propriedades-atomicas-02-spec-tech.md)
**Última review técnica:** [059-catalogo-propriedades-atomicas-03-spec-tech-review-01.md](059-catalogo-propriedades-atomicas-03-spec-tech-review-01.md)
**Build inicial:** 2026-07-02

> Documentação pós-implementação. Client-side, só exibição. Compilado 0 erros / 0 warnings (DLL 103936 bytes).
> Duas fatias com checkpoint de compile: **A** = fix da aba; **B** = refactor do catálogo + 2 colunas.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `modded/Client/PerksCatalog.cs` | Reescrito no modelo atômico: enums `Polarity`/`ValueFormat`, `PerkLine` (IsPerk + ValueToken **derivados**), `PerkGroup`, `Library` (18 grupos, Pack Mule 1×), `ByClass` (chaves de grupo por classe), `LocalGroups()`, `IconSprite(PerkGroup)`, `BuildNotificationText()` compacto, `ValidateOnce()`. Removidos `Entry`/`BuildPanelText`/`SplitNameEffect`. |
| MODIFICADO | `modded/Client/MultiplierFormat.cs` | + `ValueToken(PerkLine)` (Percent `±NN%` via `Mathf.Abs`, Multiplier `×N`, Flag ""). |
| MODIFICADO | `modded/Client/Patches/SkillsClassTabPatch.cs` | **Fatia A:** `tabLabel` genérico "CLASS"/"CLASSE"; `StyleClassTab` esconde conteúdo nativo (preserva fundo) + overlay próprio `[ícone][CLASS]` (`BuildTabOverlay`); posição adjacente à esquerda (proxy de largura + `ClassTabOffsetX`). **Fatia B:** `BuildPanel` **2 colunas** (`BuildColumn` PerksCol/DrawbacksCol); `RefreshPanel` particiona por `group.IsPerk`; `BuildCard`→`BuildGroupCard` (nome + 1 linha por efeito, chip do token); `ClearChildren`. Removidos `BuildCard`/`PillifyValues`/`ValueRegex`. |
| MODIFICADO | `modded/Client/PerkDiagnostics.cs` | `AppendPerkList` itera grupos + linhas (`NameEn` + `ValueToken`/`LabelEn` + `IsPerk`/`Pending`). |
| MODIFICADO | `modded/Client/PerksConfig.cs` | + `ClassTabOffsetX` (F12, `Perks — UI`). Removidos `PerksPanelEnabled`/`PosX`/`PosY` + os `SettingChanged`→`Reposition` (overlay morto). |
| MODIFICADO | `modded/Client/Plugin.cs` | Limpou o comentário do overlay removido. |
| DELETADO | `modded/Client/Patches/SkillsPerksPanelPatch.cs` | Overlay legado desabilitado (único caller de `BuildPanelText`). |
| MODIFICADO | `PROPRIEDADES.md` | Nova seção `Perks — UI` com `Class Tab — X offset`. |

## PA-NN-MM resolvidos durante o build

> Todos os pontos da review 01 foram resolvidos na spec e refletidos no código nesta implementação.

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | A — Gap · 🟡 | Polaridade por propriedade codificada na `Library` (cada `PerkLine` tem `Polarity` explícita → `IsPerk` derivado). |
| PA-01-02 | C — Lógica · 🟢 | `ValueToken` usa `Mathf.RoundToInt(Mathf.Abs(m−1)·100)` (sem "2−m"). |
| PA-01-03 | B — Edge · 🟢 | Adrenaline: qualificador no label das linhas ("(combat window)" / "(janela de combate)"). |
| PA-01-04 | A — Gap · 🟢 | `BuildNotificationText` lista **todos** os grupos (nome colorido); "em breve" só no painel. |

## Notas de implementação (divergências/decisões)

- **Vanilla / colunas vazias:** a mensagem "sem perks/drawbacks" vai na **coluna esquerda** (não largura total) — edge raro (classe não-mod); simplicidade > perfeição.
- **Silent Looter / Overladen / Quick Hands / Combat Medic (parte):** efeitos sem multiplicador limpo → `ValueFormat.Flag` (perk/drawback explícito, sem chip).
- **F12 `ClassTabOffsetX`** somado ao X calculado — ajuste in-game sem recompilar (a posição do Tab é empírica).

## Mudanças posteriores

(vazio inicialmente — preenchido por `/apply-code-review`)

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-02 | Build concluído via `/code-mod` (Fatias A+B, compile 0/0, DLL 103936 bytes) |
