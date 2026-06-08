# 009 — Ocultar edições vanilla no launcher · As-Built

**Mod:** CustomClasses
**Spec funcional:** [009-ocultar-edicoes-vanilla-01-spec.md](009-ocultar-edicoes-vanilla-01-spec.md)
**Spec técnica:** [009-ocultar-edicoes-vanilla-02-spec-tech.md](009-ocultar-edicoes-vanilla-02-spec-tech.md)
**Última review técnica:** [009-ocultar-edicoes-vanilla-03-spec-tech-review-01.md](009-ocultar-edicoes-vanilla-03-spec-tech-review-01.md)
**Build inicial:** 2026-06-07

> Mod server-side. `IOnLoad` config-driven que adiciona as keys das edições vanilla a `CoreConfig.Features.CreateNewProfileTypesBlacklist`. Não toca templates/perfis → perfis já criados seguem jogáveis. Compilado 0 warn/err, instalado em `SPT/user/mods/CustomClasses`.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `mods/CustomClasses/modded/Server/HiddenEditionsConfig.cs` | DTO record (`hide` = List<string>). |
| CRIADO | `mods/CustomClasses/modded/Server/HiddenEditionsLoader.cs` | `IOnLoad` (PostDBModLoader+1): lê o `.jsonc` + `blacklist.Add(key)`; guard de arquivo ausente; log. |
| CRIADO | `mods/CustomClasses/modded/Server/config/hidden-editions.jsonc` | Default com as 7 vanilla (`Standard`, `Left Behind`, `Prepare To Escape`, `Edge Of Darkness`, `Unheard`, `Tournament`, `SPT Easy start`). |

## PA-NN-MM resolvidos durante o build

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | C — Erro de Lógica · 🟡 | **Aceito v1-only.** A ocultação usa a blacklist, lida pelo `LauncherController.Connect` (v1, `/launcher/server/connect` = launcher SPT atual). **Limitação conhecida:** o launcher v2 (`/launcher/v2/types` → `LauncherV2Controller.Types`) NÃO filtra pela blacklist — se um dia o v2 virar default, as vanilla reaparecem (follow-up: patch/override do `LauncherV2Controller.Types`). Documentado no XML-doc do loader. |
| PA-01-02 | C · ✅ | Keys do default batem 1:1 com `templates/profiles.json` (incl. `SPT Easy start` minúsculo). |
| PA-01-03 | B · ✅ | `.jsonc` com comentários OK (`JsonUtil` usa `ReadCommentHandling.Skip`). |
| PA-01-04 | A · 🟢 | Reusado o padrão de path/leitura do `CustomClassesMod.OnLoad`; `FileUtil.FileExists` (FileUtil.cs:58) p/ guard de ausência. |

## Mudanças posteriores

(vazio inicialmente — preenchido por `/apply-code-review`)

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-07 | Build concluído via `/code-mod`. Server compila 0 warn/err (57.9 KB). Config instalada em `SPT/user/mods/CustomClasses/config/`. |
