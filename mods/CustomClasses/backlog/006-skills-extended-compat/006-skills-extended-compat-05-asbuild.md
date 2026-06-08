# 006 — Compat opcional com Skills-Extended · As-Built

**Mod:** CustomClasses
**Spec funcional:** [006-skills-extended-compat-01-spec.md](006-skills-extended-compat-01-spec.md)
**Spec técnica:** [006-skills-extended-compat-02-spec-tech.md](006-skills-extended-compat-02-spec-tech.md)
**Última review técnica:** [006-skills-extended-compat-03-spec-tech-review-01.md](006-skills-extended-compat-03-spec-tech-review-01.md)
**Build inicial:** 2026-06-07

> Server-side. Soft-detect do Skills-Extended (sem dependência hard) + aviso quando uma classe usa skill do SE sem o SE instalado + exemplo no Médico de Combate. Client **inalterado** (XP scaling + UI do 005/010 já são genéricos por `ESkillId`). Compilado 0 warn/err.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `mods/CustomClasses/modded/Server/SkillsExtendedCompat.cs` | Helper: `ModGuid` (`com.cj.SkillsExtended`), conjunto das 4 skills do SE, `IsPresent(loadedMods)`. |
| MODIFICADO | `mods/CustomClasses/modded/Server/CustomClassesMod.cs` | Injeta `IReadOnlyList<SptMod> loadedMods`; computa `_seInstalled` (log 1x); aviso por skill-do-SE sem o SE no loop de `skillMultipliers`. |
| MODIFICADO | `mods/CustomClasses/scripts/build-class-jsons.js` | `medicoDeCombate` += `FirstAid: 1.5`, `FieldMedicine: 1.5` (exemplo testável). |
| REGENERADO | `mods/CustomClasses/modded/Server/config/classes/medicoDeCombate.jsonc` | via gerador. |
| MODIFICADO | `mods/CustomClasses/modded/Server/config/classes/_docs/exampleClass.jsonc` | Documenta `skillMultipliers` (005/010) + as 4 skills do SE (006). |

## PA-NN-MM resolvidos durante o build

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | C · ✅ | `IsPresent` usa `m.ModMetadata?.ModGuid` + `string.Equals(Ordinal)` — null-safe. |
| PA-01-02 | B · 🟢 | Aviso mantido por-classe (mais acionável); agregar só se virar ruído. |
| PA-01-03 | A · 🟢 | Documentado em `config/classes/_docs/exampleClass.jsonc`. |

## Mudanças posteriores

(vazio inicialmente — preenchido por `/apply-code-review`)

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-07 | Build concluído via `/code-mod`. Server 0 warn/err (59.4 KB). Client inalterado. Exemplo: Médico de Combate (FirstAid/FieldMedicine). |
