# 002 — Schema de classe + loader multi-classe · As-Built

**Mod:** CustomClasses
**Spec funcional:** [002-class-schema-loader-01-spec.md](002-class-schema-loader-01-spec.md)
**Spec técnica:** [002-class-schema-loader-02-spec-tech.md](002-class-schema-loader-02-spec-tech.md)
**Última review técnica:** [002-class-schema-loader-03-spec-tech-review-01.md](002-class-schema-loader-03-spec-tech-review-01.md)
**Build inicial:** 2026-06-07

> Documentação pós-implementação. **Compilado (0 warn/err) e instalado** — playtest pendente (ver Pendências). Item 🟡 até o playtest.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `mods/CustomClasses/modded/Server/ClassDefinition.cs` | DTO do JSON: `name` (obrig.), `enabled`, `baseEdition`, `description`, `skills`. |
| MODIFICADO | `mods/CustomClasses/modded/Server/CustomClassesMod.cs` | Vira **loader**: enumera `config/classes/*.json[c]`, desserializa, valida e registra cada classe (try/catch por arquivo, resumo no log). Substitui o `ClassDefinition` hardcoded do 001. |
| CRIADO | `mods/CustomClasses/modded/Server/config/classes/exampleClass.jsonc` | Exemplo auto-documentado (comentários) — "Example Class". |
| CRIADO | `mods/CustomClasses/modded/Server/config/classes/testClass.jsonc` | "Test Class" migrada do 001 (continuidade). |
| MODIFICADO | `.agents/scripts/compile-mod.sh` | server-csharp: copia `<csproj-dir>/config` → `SPT/user/mods/<mod>/config`. |

## PA-NN-MM resolvidos durante o build

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | C — Lógica · 🟡 | `ApplySkills` exige `Enum.IsDefined(typeof(SkillTypes), skill)` além do `TryParse` (rejeita skill numérica/indefinida). |
| PA-01-02 | B — Edge · 🟢 | `RegisterClass` loga skills aplicadas por-lado (`usec`/`bear`) e avisa se um lado aplicar 0 com skills configuradas. |
| PA-01-03 | B — Edge · 🟢 | Loader não-recursivo documentado (só topo de `config/classes/`). |
| PA-01-04 | A — Gap · 🟢 | Patch do `compile-mod.sh` inserido após `BUILT_SERVER=1`; **validado** (config no install). |
| PA-01-05 | A — Gap · 🟢 | Testes de JSONC + `enabled` omitido no checklist (a confirmar no playtest). |

Também herdados do 001: **CR-01-01** (LastAccess de skill nova via `TimeUtil`) e **CR-01-04** (strings → JSON) resolvidos.

## Estado vs. critérios de aceite

Compilado e instalado (a confirmar via playtest): loader lê `config/classes/*.json[c]`, registra N editions, skills/base por arquivo, malformado pulado, resumo no log; **config/ ship confirmado** no install. Build fix: `Path` ambíguo (`...Tables.Path` vs `System.IO.Path`) → qualificado `System.IO.Path`.

## Pendências (próximos passos)

1. ✅ `/compile-mod` — `CustomClasses-Server.dll` (17.9 KB, 0 warn/err) + `config/classes/{exampleClass,testClass}.jsonc` instalados em `SPT/user/mods/CustomClasses/`.
2. **Playtest — loader confirmado ✅ (2026-06-07):** log do servidor mostrou `Registered 'Example Class' (base 'SPT Zero to hero', skills usec=2/bear=2)`, `Registered 'Test Class' (...)` e `Loaded 2 class(es), skipped 0`. Confirma: leitura da pasta, base/skills por arquivo, log por-lado (PA-01-02). **PA-01-05 confirmado**: `exampleClass.jsonc` (com comentários) carregou e `testClass.jsonc` (sem `enabled`) registrou → JSONC tolerado + default de `enabled` mantido. Criação de perfil/skills in-game = mesmo mecanismo já validado no 001.
   - Pendente (opcional): corner cases de erro (arquivo malformado, `enabled:false`, `name` ausente, skill inválida, base inexistente) — mecanismo implementado, validação incremental.
3. Próximo: `/code-review` → 🟢.

## Mudanças posteriores

**2026-06-07 — code-review 01 aplicada** (`CustomClassesMod.cs`, `exampleClass.jsonc`): CR-01-01 (doc `baseEdition`=vanilla), CR-01-02 (`.Distinct()` nos arquivos), CR-01-03 (`Trim()` em `name`/`baseEdition`). Recompilado (0 warn/err). **Item → 🟢.**

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-07 | Build (implementação) concluído via `/code-mod` — compilado (0 warn/err) + instalado; config ship validado. Playtest pendente. |
| 2026-06-07 | Loader confirmado in-game (log: "Loaded 2 class(es), skipped 0"; base/skills por arquivo; PA-01-05 JSONC + `enabled` default OK). |
| 2026-06-07 | Code-review 01 aplicada (CR-01-01/02/03) + rebuild. **Item → 🟢.** |
