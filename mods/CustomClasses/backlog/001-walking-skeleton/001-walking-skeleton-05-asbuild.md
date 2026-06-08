# 001 — Scaffold + 1 classe (walking skeleton) · As-Built

**Mod:** CustomClasses
**Spec funcional:** [001-walking-skeleton-01-spec.md](001-walking-skeleton-01-spec.md)
**Spec técnica:** [001-walking-skeleton-02-spec-tech.md](001-walking-skeleton-02-spec-tech.md)
**Última review técnica:** [001-walking-skeleton-03-spec-tech-review-01.md](001-walking-skeleton-03-spec-tech-review-01.md)
**Build inicial:** 2026-06-07

> Documentação **pós-implementação**. Reflete o código entregue pelo `/code-mod`. **Importante:** o código foi escrito mas **ainda não foi compilado nem validado in-game** — `/compile-mod` + playtest são os próximos passos (ver "Pendências"). Por isso o item está 🟡, não 🟢.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `mods/CustomClasses/modded/Server/CustomClasses.Server.csproj` | Projeto C# net9.0 (`AssemblyName=CustomClasses-Server`), PackageReference `SPTarkov.Server.Core/DI/Common` 4.0.0. |
| CRIADO | `mods/CustomClasses/modded/Server/CustomClassesMetadata.cs` | Record `AbstractModMetadata` (GUID `customclasses.mdj`, autor mdj, SPT `~4.0.0`, MIT). |
| CRIADO | `mods/CustomClasses/modded/Server/CustomClassesMod.cs` | `[Injectable] IOnLoad` em `PostDBModLoader+1`: clona a edition base e injeta a classe de teste como nova edition com skills estáticas. |

## PA-NN-MM resolvidos durante o build

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | A — Gap · 🟡 | `BaseEdition` virou campo da `ClassDefinition` (default `"Standard"`); se a chave base não existir, aborta com log claro **listando as chaves disponíveis** (confirma a chave no 1º boot); sem fallback. |
| PA-01-02 | A — Gap · 🟡 | Pacotes `SPTarkov.*` pinados em `4.0.0` no csproj (comentário instruindo subir p/ 4.0.2 se falhar). Resolução final depende do 1º `restore`/build. |
| PA-01-03 | C — Lógica · 🟢 | Guarda `if (sides is null) { log; return; }` após `cloner.Clone` (retorna `T?`). |
| PA-01-04 | B — Edge · 🟢 | Documentado na §7 da spec técnica (idioma misto no launcher se server locale ≠ en) — sem mudança de código. |
| PA-01-05 | B — Edge · 🟢 | v1/v2 do launcher leem `GetProfileTemplates` (confirmado no código); aceite depende de playtest (pendência). |
| PA-01-06 | A — Gap · 🟢 | Documentado na §7 (injeção em memória no `OnLoad`; remover DLL → edition some no próximo boot, sem cleanup). |

## Estado vs. critérios de aceite

**Validado em isolamento (2026-06-07):** edition "Test Class" aparece no launcher com descrição en; criação sem erro + log de confirmação; skills exatas (Endurance 5 / Strength 3) em USEC e BEAR; demais no padrão; **stash vazio** (base Zero to hero). Idempotência/colisão (guarda `ContainsKey`) e remoção (injeção em memória) corretas por construção. Coexistência com RZCustomProfiles fica para o item 007 (clobber).

## Pendências (próximos passos)

1. ✅ **`/compile-mod CustomClasses`** — feito em 2026-06-07: `CustomClasses-Server.dll` (14.8 KB) compilado (0 warnings/erros, `SPTarkov.* 4.0.0`) e instalado em `D:/SPT/SPT/user/mods/CustomClasses/`. Confirmou PA-01-02 e PA-01-03 (0 warnings). Fix de build: faltava `using SPTarkov.Server.Core.Models.Eft.Common` (PmcData).
2. **Playtest (parcial ✅, 2026-06-07):** com o RZCustomProfiles desabilitado, a edition "Test Class" aparece no launcher e o personagem nasce com **Endurance 5 / Strength 3** in-game (critérios 1-4 OK, em isolamento). Log "Registered edition 'Test Class' ... base 'Standard'" confirmado → PA-01-01 (chave base) e PA-01-05 resolvidos.
   - **Mudança pós-playtest:** `BaseEdition` trocada de `"Standard"` → `"SPT Zero to hero"` (stash **vazio** — a classe controla 100% dos itens; Standard trazia itens indesejados). Recompilado/reinstalado.
   - **Re-teste ✅ (2026-06-07):** novo perfil com base Zero to hero nasceu com **stash vazio + Endurance 5 / Strength 3**.
   - **Achado (→ item 007):** com o **RZCustomProfiles ativo**, ele **clobbera** o dicionário de templates (roda depois do nosso e reconstrói), e a nossa edition some do launcher. Walking skeleton validado **em isolamento**; coexistência é o item 007. (Para desabilitar um server mod, mover a pasta para **fora** de `user/mods` — renomear dentro não basta, o SPT lê o DLL de qualquer subpasta.)
3. ✅ Re-teste OK → item transicionado para **🟢**. (Idempotência/remoção: mecanismo correto; confirmação em uso prolongado é incremental.)

## Mudanças posteriores

**2026-06-07 — code-review 01 aplicada** (`CustomClassesMod.cs`):
- **CR-01-02:** `ApplySkills` retorna nº de skills aplicadas (`-1` se o lado não tem skills); `OnLoad` loga `Warning` se um lado faltar e reporta a contagem real.
- **CR-01-03:** comentário documentando o deep clone do `ICloner.Clone`.
- Deferidos: CR-01-01 (→002), CR-01-04 (→002), CR-01-05 (→007). Recompilado (0 warn/err).

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-07 | Build (implementação) concluído via `/code-mod` — código escrito, ainda não compilado/validado |
| 2026-06-07 | `/compile-mod` OK — `CustomClasses-Server.dll` compilado (0 warn/err) e instalado; fix `using ...Eft.Common` (PmcData). Playtest pendente. |
| 2026-06-07 | Playtest parcial OK (sem RZ): edition aparece + Endurance 5/Strength 3. Base trocada Standard→"SPT Zero to hero" (stash vazio) + rebuild. Achado: RZCustomProfiles clobbera templates (→ item 007). |
| 2026-06-07 | Re-teste OK (stash vazio + skills) + code-review 01 aplicada (CR-01-02/03) + rebuild. **Item → 🟢.** |
