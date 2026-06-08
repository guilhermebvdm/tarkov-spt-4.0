# 010 — UI dos multiplicadores de skill · As-Built

**Mod:** CustomClasses
**Spec funcional:** [010-ui-multiplicadores-skill-01-spec.md](010-ui-multiplicadores-skill-01-spec.md)
**Spec técnica:** [010-ui-multiplicadores-skill-02-spec-tech.md](010-ui-multiplicadores-skill-02-spec-tech.md)
**Última review técnica:** [010-ui-multiplicadores-skill-03-spec-tech-review-01.md](010-ui-multiplicadores-skill-03-spec-tech-review-01.md)
**Build inicial:** 2026-06-07

> Documentação **pós-implementação**. Refino client-only da UI do 005 + extensão do payload da rota p/ carregar o nome da classe. Compilado/instalado: client → `BepInEx/plugins/CustomClasses`, server → `SPT/user/mods/CustomClasses`. **0 warn/err** nos dois projetos.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `mods/CustomClasses/modded/Server/SkillMultipliersResponse.cs` | DTO record `{ className, multipliers }` (`[JsonPropertyName]`). |
| MODIFICADO | `mods/CustomClasses/modded/Server/SkillMultipliersRouter.cs` | rota devolve o DTO; `className = edition` quando há multiplicadores (senão `null`). |
| MODIFICADO | `mods/CustomClasses/modded/Client/SkillMultipliers.cs` | parseia `{ className, multipliers }` (classe `Payload`); expõe `ClassName`; `Reset()` zera nome. |
| CRIADO | `mods/CustomClasses/modded/Client/MultiplierFormat.cs` | helper central: cores (verde/vermelho/laranja-elite), `Percent`, `Marker` (▲/▼), `TooltipText`. Strings pt-BR isoladas p/ i18n (008). |
| MODIFICADO | `mods/CustomClasses/modded/Client/Patches/SkillPanelPatch.cs` | postfix em `method_1`: cria/reusa GO `CC_MultMarker` (TMP + `HoverTooltipArea`) à direita do `_name`; some quando sem fator. Removido o override das setas vanilla. |
| CRIADO | `mods/CustomClasses/modded/Client/Patches/SkillIconBorderPatch.cs` | postfix em `SkillIcon.Show`: `_border.color` verde/vermelho (Elite mantém laranja; sem fator reseta branco/laranja). |
| REMOVIDO | `mods/CustomClasses/modded/Client/Patches/SkillTooltipPatch.cs` | substituído pelo tooltip dedicado do marcador. |
| MODIFICADO | `mods/CustomClasses/modded/Client/Plugin.cs` | registra `SkillIconBorderPatch`; remove `SkillTooltipPatch`; tooltip da config F12 atualizado. |

## PA-NN-MM resolvidos durante o build

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | B — Edge Case · 🟡 | Elite tem **precedência** na borda: `SkillIconBorderPatch` só pinta verde/vermelho se `!skill.IsEliteLevel`; Elite mantém o laranja vanilla. Buff/debuff fica na seta+tooltip. |
| PA-01-02 | B — Edge Case · 🟡 | Setas ▲/▼ implementadas, isoladas em `MultiplierFormat.Marker()` — ponto único de troca se a fonte TMP não tiver os glyphs (validar in-game). |
| PA-01-03 | A — Gap · 🟡 | `SkillIconBorderPatch` ramo sem-fator **reseta** `_border.color` (branco; laranja se Elite) — evita vazar cor em células recicladas. |
| PA-01-04 | C — Erro de Lógica · 🟢 | Marcador esticado (0,0)-(1,1) por ora (hover cobre o nome+marcador) — anchor à direita fica como ajuste se incomodar no playtest. |
| PA-01-05 | A — Gap · 🟢 | Guards `is null` + try/catch mantidos nos dois patches (defensivo). |

## Mudanças posteriores

**2026-06-07 — `/apply-code-review` (code-review 01):** aplicados CR-01-02 e CR-01-03 em [SkillPanelPatch.cs](../../modded/Client/Patches/SkillPanelPatch.cs):
- **CR-01-02** — ramo `!has` desativa o marcador antes de mexer na mensagem (sem flash de tooltip vazio); removido `SetMessageText("")`.
- **CR-01-03** — `SimpleTooltip` resolvido 1x em `GetOrCreateMarker` (`area.Init(..., "")`); refreshes usam só `SetMessageText`.
- **Pendente:** CR-01-01 (🟡 modo grade — seta/tooltip só no modo lista) aguarda validação in-game.
- Recompilado 0 warn/err.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-07 | Build concluído via `/code-mod`. Compilado 0 warn/err (client 14.8 KB, server 54.5 KB). PA-01-01..05 resolvidos no build. |
| 2026-06-07 | `/apply-code-review` — CR-01-02 + CR-01-03 aplicados; CR-01-01 pendente (validação in-game). Recompilado 0 warn/err. |
