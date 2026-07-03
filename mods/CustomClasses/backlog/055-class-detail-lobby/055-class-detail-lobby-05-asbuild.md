# 055 — Detalhe da classe no lobby/loading da raid · As-Built

**Mod:** CustomClasses
**Spec funcional:** [055-class-detail-lobby-01-spec.md](055-class-detail-lobby-01-spec.md)
**Spec técnica:** [055-class-detail-lobby-02-spec-tech.md](055-class-detail-lobby-02-spec-tech.md)
**Última review técnica:** [055-class-detail-lobby-03-spec-tech-review-01.md](055-class-detail-lobby-03-spec-tech-review-01.md)
**Build inicial:** 2026-07-02

> Client-side, só exibição. Compilado 0/0 (DLL 105984 bytes). Duas fatias com checkpoint de compile:
> **1** = extração do `PerksPanelView` (refactor DRY do 059); **2** = patch soft-detect do loading FIKA + config.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `modded/Client/PerksPanelView.cs` | Painel reutilizável extraído do 059: `Build(Transform, TMP_FontAsset?)` + `Refresh(GameObject)` + cards (`BuildGroupCard`/`BuildColumn`/`BuildSectionHeader`/`BuildMessageCard`/`ClearChildren`) + classes `CardHover`/`FadeIn`. Consumido pela aba CLASS (053/059) e pelo loading (055). |
| MODIFICADO | `modded/Client/Patches/SkillsClassTabPatch.cs` | Cuida só da **aba** (clone do tab, toggle-group, overlay [ícone][CLASS], posição); delega o conteúdo a `PerksPanelView.Build`/`Refresh`. Removidos os métodos de painel + `CardHover`/`FadeIn` (movidos). |
| CRIADO | `modded/Client/Patches/ClassDetailLoadingPatch.cs` | Postfix soft-detect em `LoadingScreenUI.AddPlayer(int,string)` (FIKA); na linha do player local (`nickname == SkillMultipliers.Nickname`), anexa `LoadingClassHover` que monta/exibe o `PerksPanelView` compacto. Zero tipo FIKA no IL (reflection). |
| MODIFICADO | `modded/Client/PerksConfig.cs` | + `ClassDetailOnLoading` (bool, seção `Perks — UI`, default true). |
| MODIFICADO | `modded/Client/Plugin.cs` | Habilita `ClassDetailLoadingPatch` sob `if (TypeByName("LoadingScreenUI") != null)` (padrão SAIN). |
| MODIFICADO | `PROPRIEDADES.md` | Linha do `Class Detail on Loading Screen` na seção `Perks — UI`. |

## PA-NN-MM resolvidos durante o build

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | B — Edge · 🟡 | **Auto-visível** no `OnEnable` (não depende de EventSystem/raycast na tela transiente); hover/click = toggle opcional. |
| PA-01-02 | A — Gap · 🟢 | Fonte vem de `GetComponentInChildren<TMP_Text>()?.font` da própria linha (Nickname/Percentage). |
| PA-01-03 | C — Lógica · 🟢 | `_lastPanelClass` estático mantido no `PerksPanelView` (hosts não coexistem — invariante documentada). |

## Notas de implementação (divergências/decisões)

- **Painel compacto (não fill):** o `Build` ancora "fill" (bom pra aba); no loading o `LoadingClassHover` **reancora** o
  painel para 600×460 à direita da tela (`anchoredPosition (-60,0)`), pra não cobrir a tela de carregamento.
- **Sem hard-ref FIKA:** target por `AccessTools.TypeByName("LoadingScreenUI")` + `_loadingPlayers[netId]` castado a
  `UnityEngine.Component` — nenhum tipo FIKA no IL (degrada 100% solo). Log `[055] FIKA detectado` confirma o gate.
- **Reuso total do 059:** a aba CLASS e o loading renderizam o **mesmo** `PerksPanelView` (paridade garantida por construção).

## Mudanças posteriores

| Data | Origem | Mudança |
| --- | --- | --- |
| 2026-07-03 | code-review 02 · CR-02-01 (F-3) | `LoadingClassHover`: `Show()` com try/catch cobrindo `OnEnable` **e** `OnPointerEnter` (Build/Ensure protegidos). |
| 2026-07-03 | code-review 02 · CR-02-02 (F-9) | `OnDisable` esconde o painel (linha desativada sem destruir). DLL 106496 bytes, compile 0/0. |

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-02 | Build concluído via `/code-mod` (Fatias 1+2, compile 0/0, DLL 105984 bytes) |
