# 008 — i18n (multilíngue pt-BR/en) · As-Built

**Mod:** CustomClasses
**Spec funcional:** [008-i18n-01-spec.md](008-i18n-01-spec.md)
**Spec técnica:** [008-i18n-02-spec-tech.md](008-i18n-02-spec-tech.md)
**Última review técnica:** [008-i18n-03-spec-tech-review-01.md](008-i18n-03-spec-tech-review-01.md)
**Build inicial:** 2026-06-07

> Híbrido. **(A) server:** descrição de edition por idioma (segue a língua do servidor, fallback en). **(B) client:** seletor de língua no F12 (default English) p/ os textos in-game do mod. Compilado 0 warn/err (client 15.4 KB, server 60.9 KB).

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `modded/Server/LocalizedText.cs` | Tipo `{En,Pt}` + `Resolve(locale)` + `JsonConverter` (string OU objeto `{en,pt}`). |
| MODIFICADO | `modded/Server/ClassDefinition.cs` | `Description` → `LocalizedText?`. |
| MODIFICADO | `modded/Server/CustomClassesMod.cs` | Injeta `LocaleService`; resolve a descrição por `GetDesiredServerLocale()` no `RegisterClass`. |
| MODIFICADO | `modded/Client/Plugin.cs` | + `enum Language` + `ConfigEntry<Language> Lang` (default English); tooltip F12 bilíngue. |
| MODIFICADO | `modded/Client/MultiplierFormat.cs` | `TooltipText` en+pt escolhido por `Plugin.Lang`. |
| MODIFICADO | `scripts/class-recipes.js` | `description` das 10 classes → `{ en, pt }` (traduções). |
| REGENERADO | `modded/Server/config/classes/*.jsonc` | descrições bilíngues. |
| MODIFICADO | `modded/Server/config/classes/_docs/exampleClass.jsonc` | documenta `description` string|objeto. |
| CRIADO | `PROPRIEDADES.md` | as 3 ConfigEntry do F12 (`EnableSkillMultipliers`, `ShowMultiplierOnSkills`, `Language`). |

## PA-NN-MM resolvidos durante o build

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | C · ✅ | `LocaleService` (`[Injectable] Singleton`) injetado no ctor. |
| PA-01-02 | C · 🟡 | `[JsonConverter]` mantido no `LocalizedText`; compila. **A validar no boot do server** (classe com `description` objeto carrega). Plano B (DTO sem string-legada) só se falhar. |
| PA-01-03 | B · 🟢 | Enum `Language { English, Portugues }` simples em `Plugin`. |
| PA-01-04 | A · 🟢 | `PROPRIEDADES.md` criado com as 3 props. |

## Mudanças posteriores

(vazio inicialmente — preenchido por `/apply-code-review`)

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-07 | Build concluído via `/code-mod`. 0 warn/err. Descrições bilíngues nas 10 classes; seletor de língua F12 (default English). |
