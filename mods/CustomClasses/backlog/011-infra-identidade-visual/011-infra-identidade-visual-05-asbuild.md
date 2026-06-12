# 011 — Infra de identidade visual da classe · As-Built

**Mod:** CustomClasses
**Spec funcional:** [011-infra-identidade-visual-01-spec.md](011-infra-identidade-visual-01-spec.md)
**Spec técnica:** [011-infra-identidade-visual-02-spec-tech.md](011-infra-identidade-visual-02-spec-tech.md)
**Última review técnica:** [011-infra-identidade-visual-03-spec-tech-review-01.md](011-infra-identidade-visual-03-spec-tech-review-01.md)
**Build inicial:** 2026-06-08

> Base híbrida para 012/013. Server expõe identidade (nome/ícone/cor) por classe; client tem cache de ícone (PNG→Sprite) + componente de "selo"; compile-mod entrega os PNGs ao client. Compilado **0 warn/err** (client 18.5 KB, server 62.0 KB). 10 PNGs placeholder instalados no plugin.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `modded/Server/ClassDefinition.cs` | + `iconFile`, `nameColor` (opcionais). |
| CRIADO | `modded/Server/ClassVisualRegistry.cs` | Singleton `edition → (iconFile?, nameColor?)`; registra **toda** classe (`Contains`/`Get`). |
| MODIFICADO | `modded/Server/CustomClassesMod.cs` | injeta `ClassVisualRegistry`; `Set(...)` no registro efetivo (PA-01-01). |
| MODIFICADO | `modded/Server/SkillMultipliersResponse.cs` | + `iconFile`, `nameColor`. |
| MODIFICADO | `modded/Server/SkillMultipliersRouter.cs` | devolve identidade quando `visualRegistry.Contains(edition)` (mesmo sem multiplicadores). |
| MODIFICADO | `modded/Client/SkillMultipliers.cs` | expõe `IconFile`/`NameColor`; identidade setada mesmo sem multiplicadores. |
| CRIADO | `modded/Client/UI/ClassIconCache.cs` | PNG→Sprite cacheado, sanitiza path, null-safe, `Dispose`. |
| CRIADO | `modded/Client/UI/ClassIdentityView.cs` | factory idempotente do selo (ícone+nome colorido). |
| MODIFICADO | `modded/Client/Plugin.cs` | `OnDestroy` → `ClassIconCache.Dispose()`. |
| MODIFICADO | `modded/Client/CustomClasses.Client.csproj` | + refs `UnityEngine.TextRenderingModule`, `UnityEngine.ImageConversionModule`. |
| CRIADO | `modded/Client/icons/*.png` | 10 placeholders (1 por classe, círculo colorido + inicial). |
| MODIFICADO | `scripts/build-class-jsons.js` | + `CLASS_VISUAL` (iconFile/nameColor por classe) + emite no JSON. |
| REGENERADO | `modded/Server/config/classes/*.jsonc` | 10 classes com `iconFile`/`nameColor`. |
| MODIFICADO | `.agents/scripts/compile-mod.sh` | ramo client copia `modded/Client/icons` → plugin; +2 DLLs no resolve_references. |
| MODIFICADO | `modded/Server/config/classes/_docs/exampleClass.jsonc` | documenta `iconFile`/`nameColor`. |

## PA-NN-MM resolvidos durante o build

| ID | Categoria · Impacto | Resolução |
| --- | --- | --- |
| PA-01-01 | C · 🟡 | `classVisualRegistry.Set` movido para junto de `templates[name] = sides` (só no registro efetivo, após validações). |
| PA-01-02 | C · 🟢 | `ClassIdentityView` aplica `iconSize` no `LayoutElement` (passado a `CreateContainer`). |
| PA-01-03 | B · 🟢 | `BuildOrRefresh` recria o container se faltar `Icon`/`Label` (defensivo). |
| PA-01-04 | A · 🟢 | Confirmado: `modded/Client/icons/*.png` versionável (não cai no `.gitignore`) e copiado ao plugin. |

## Mudanças posteriores

(vazio inicialmente — preenchido por `/apply-code-review`)

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-08 | Build concluído via `/code-mod`. 0 warn/err. 10 PNGs no plugin; rota devolve identidade (inclusive classe sem multiplicador). +2 refs Unity (TextRendering/ImageConversion). |
