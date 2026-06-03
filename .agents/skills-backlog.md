# Backlog de skills (Claude Code) para o repo

Propostas de skills (`.claude/skills/<nome>.md`) que automatizam tarefas recorrentes deste repo. Cada entrada tem:

- **O que faz** — escopo
- **Cenário** — `novo mod`, `evoluir 4.0`, `migrar 3.11 → 4.0`, `transversal`
- **Material que reusa** — wiki, docs, código, externos
- **Status** — `proposta` · `em design` · `prototipado` · `em uso` · `descontinuado`

> Convenção: ao mover uma skill para `prototipado` ou `em uso`, atualizar a tabela e linkar para `.claude/skills/<nome>.md`.

## Tabela executiva

| Skill | Cenário | Status | Prioridade |
|---|---|---|---|
| [`/migrate-3to4`](#migrate-3to4) | migrar 3.11 → 4.0 | proposta | 🔴 Alta |
| [`/classify-mod`](#classify-mod) | migrar / evoluir | proposta | 🔴 Alta |
| [`/scaffold-server-mod`](#scaffold-server-mod) | novo mod | proposta | 🟡 Média |
| [`/scaffold-client-mod`](#scaffold-client-mod) | novo mod | proposta | 🟡 Média |
| [`/audit-mod`](#audit-mod) | evoluir 4.0 | proposta | 🟡 Média |
| [`/upgrade-mod-version`](#upgrade-mod-version) | evoluir 4.0 | proposta | 🟢 Baixa |
| [`/debug-client-setup`](#debug-client-setup) | transversal | proposta | 🟢 Baixa |
| [`/lookup-id`](#lookup-id) | transversal | proposta | 🟢 Baixa |
| [`/check-mod-compat`](#check-mod-compat) | transversal | proposta | 🟢 Baixa |
| [`/modding-help`](#modding-help) | transversal | proposta | 🟢 Baixa |

---

## `/migrate-3to4`

**Cenário:** migrar 3.11 → 4.0
**Status:** proposta · 🔴 Alta

### Contexto técnico (de pesquisa web + wiki)

A migração 3.11 → 4.0 **não é um port** — é re-implementação. O time SPT reescreveu ~150 mil linhas à mão; mods em TypeScript **não funcionam** em 4.0. A skill precisa orientar reescrita guiada, não conversão automática.

Mudanças estruturais que a skill deve cobrir:

| Aspecto | SPT 3.x | SPT 4.0 |
|---|---|---|
| Linguagem do server | TypeScript / Node.js | C# / .NET 9 |
| Manifesto | `package.json` com campo `sptVersion` | classe `ModMetadata` (atributos `[Injectable]`, props `ModGuid`, `Name`, `Author`, `Version`, `SptVersion`, `License`) |
| DI | `container.register(...)` (tsyringe) | `[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + N)]` + injeção via construtor |
| Lifecycle | hooks variados (`postDBLoad`, etc.) | implementar `IOnLoad` com `async Task OnLoad()` |
| Configs | JSON/JSONC lidos manualmente | `ConfigLoader<T>` injetado, lê JSONC |
| Patches | overrides via override de método | mesmo, mas via DI; ainda há `Patcher_*` por convenção (ver [mods/RZ-SPTMods/](../mods/RZ-SPTMods/)) |
| EFT alvo | 0.16.1.3 (3.11) | 0.16.9.0.40087 (4.0) |
| Runtime | Node | .NET 9.0.15 |

### O que a skill faz

Recebe uma pasta de mod 3.x (ex: `mods/legacy-3x/<Nome>/`) e:

1. **Analisa** `package.json`, `mod.ts`, `src/**/*.ts` — extrai: nome, autor, versão, sptVersion, hooks usados, dependências (tsyringe `container.resolve`), classes principais.
2. **Mapeia** cada hook/registro para equivalente C# 4.0 (tabela acima).
3. **Gera plano** de re-implementação em [docs/migration/<mod>/plan.md](../docs/migration/) com:
   - Esqueleto da `ModMetadata` class
   - Lista de `[Injectable]` classes a criar
   - Mapeamento de cada feature TS → método C# proposto
   - Configs a portar (estrutura JSON preservada quando possível)
   - Riscos: APIs 3.x sem equivalente direto
4. **Atualiza** linha do mod em [docs/migration/mods-inventory.md](../docs/migration/mods-inventory.md): Status → `🔧 Desenvolver`.
5. **NÃO gera código C# automático** — a skill aborta se for solicitado, com aviso de que conversão direta é proibida (ver wiki FAQs_40).

### Material que reusa

- Wiki: [FAQs_40.md](../wiki/spt/FAQs_40.md), [How_SPT_Works.md](../wiki/spt/How_SPT_Works.md), [Updating_SPT.md](../wiki/spt/Updating_SPT.md)
- Repo: [mods/RZ-SPTMods/RZ-SPTMods/RZEssentials/](../mods/RZ-SPTMods/RZ-SPTMods/RZEssentials/) (exemplo real de DI + IOnLoad + Patcher_*)
- Inventário: [docs/migration/mods-inventory.md](../docs/migration/mods-inventory.md)
- Externos:
  - [github.com/sp-tarkov/server-mod-examples](https://github.com/sp-tarkov/server-mod-examples) — exemplos numerados (1 → complexo)
  - [github.com/sp-tarkov/server-csharp](https://github.com/sp-tarkov/server-csharp) — fonte do server
  - [deepwiki.com/sp-tarkov/server-csharp](https://deepwiki.com/sp-tarkov/server-csharp/1-overview) — doc gerada
  - [github.com/WelcomeToTarkov/WTT-CommonLib](https://github.com/WelcomeToTarkov/WTT-CommonLib/) — ex. `[Injectable]` + `IOnLoad`
  - NuGet: `SPTarkov.Server.Core 4.0.*`, `SPTarkov.DI 4.0.*`

### Riscos / decisões em aberto

- Quanto a skill deve "implementar" vs apenas "planejar"? Decisão atual: **apenas planejar**. Reescrita fica com humano + iteração.
- Como detectar quando uma feature 3.x não tem equivalente 4.0? Heurística: cross-check contra `server-mod-examples`.
- Versão dos NuGet evolui em pre-releases — skill deve ler do `.csproj` de [mods/RZ-SPTMods/](../mods/RZ-SPTMods/) o que está em uso (não hardcodar).

---

## `/classify-mod`

**Cenário:** migrar / evoluir
**Status:** proposta · 🔴 Alta

Preenche linha do [mods-inventory.md](../docs/migration/mods-inventory.md) (Tipo / Atuação / Categoria / Escopo / Status / Prioridade) seguindo a taxonomia já definida lá.

**Fluxo:**

1. Recebe nome ou pasta do mod.
2. Cross-check em [forge.sp-tarkov.com](https://forge.sp-tarkov.com/) — existe versão 4.0? Quem mantém? Última atualização?
3. Decide Status: 🟢 Instalar (4.0 publicado, estável) · ⬆️ Evoluir (interno, precisa update) · 🔧 Desenvolver (não há 4.0) · 🟠 Aguardar (autor anunciou WIP) · 🔴 Bloqueado · ⚫ Não incluir.
4. Sugere Prioridade baseada em [Recommended_Mods_40.md](../wiki/spt/Recommended_Mods_40.md) e dependências de outros mods do inventário.
5. Edita linha (com confirmação) ou cria nova em [mods-inventory.md](../docs/migration/mods-inventory.md), depois roda `node scripts/sync-mods-html.js`.

**Material:** [mods-inventory.md](../docs/migration/mods-inventory.md) (taxonomia + fonte única), forge.sp-tarkov.com, Recommended_Mods_40.md.

---

## `/scaffold-server-mod`

**Cenário:** novo mod
**Status:** proposta · 🟡 Média

Cria `mods/server/<Nome>/` com:

- `.csproj` referenciando `SPTarkov.Server.Core` e `SPTarkov.DI` (versão lida de mod existente do repo)
- `ModMetadata.cs` com `ModGuid`, `Name`, `Author`, `Version`, `SptVersion`, `License`
- Classe principal `[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]` + `IOnLoad` + `async Task OnLoad()`
- `config/<nome>.jsonc` + classe POCO + carga via `ConfigLoader<T>`
- README mínimo com instruções de build/deploy

**Material:** padrão de [mods/RZ-SPTMods/RZ-SPTMods/RZEssentials/](../mods/RZ-SPTMods/RZ-SPTMods/RZEssentials/), [server-mod-examples](https://github.com/sp-tarkov/server-mod-examples).

---

## `/scaffold-client-mod`

**Cenário:** novo mod
**Status:** proposta · 🟡 Média

Cria `mods/client/<Nome>/` com:

- `.csproj` referenciando `BepInEx.Core 5.x` e `HarmonyX 2.15`
- `Plugin.cs` com `[BepInPlugin]`, `[BepInDependency]`, lifecycle `Awake()`/`Start()`
- Classe de patches `Harmony` com `[HarmonyPatch]` Prefix/Postfix
- post-build copy para `<game-path>/BepInEx/plugins/`

**Material:** [Client_Modding_Quick_Guide.md](../wiki/spt/modding/tutorials/Client_Modding_Quick_Guide.md), [mods/RZ-SPTMods/RZ-SPTMods/RZEssentialsClient/](../mods/RZ-SPTMods/RZ-SPTMods/RZEssentialsClient/), [docs.bepinex.dev](https://docs.bepinex.dev/), [harmony.pardeike.net](https://harmony.pardeike.net/).

---

## `/audit-mod`

**Cenário:** evoluir 4.0
**Status:** proposta · 🟡 Média

Verifica `<mod>/`:

- SPT version no `ModMetadata` ou `package.json` confere com 4.0.x
- Refs `BepInEx.Core 5.x` + `HarmonyX 2.15` (client) / `SPTarkov.Server.Core 4.0.*` (server)
- Dependências declaradas e presentes em `mods/deps/`
- Cruza nome com [Known_Mod_Issues_40.md](../wiki/spt/Known_Mod_Issues_40.md) para listar issues conhecidos
- Sugere atualização de campos em [mods-inventory.md](../docs/migration/mods-inventory.md)

---

## `/upgrade-mod-version`

**Cenário:** evoluir 4.0
**Status:** proposta · 🟢 Baixa

Bump de semver respeitando [Updating_SPT.md](../wiki/spt/Updating_SPT.md): patch (Z) só pra hotfix compatível, minor pra feature, major se quebra. Atualiza `Version` na ModMetadata e adiciona linha no changelog do mod.

---

## `/debug-client-setup`

**Cenário:** transversal
**Status:** proposta · 🟢 Baixa

Checklist + comandos do [debug_dnSpy.md](../wiki/spt/modding/tutorials/debug_dnSpy.md):

1. Baixar build debug Unity (link MEGA da wiki)
2. Editar `boot.config`, `BepInEx.cfg`
3. Attach dnSpy: `Debug > Attach to Process (Unity)`
4. Aviso: jogo congela em breakpoint — usar 2 monitores

---

## `/lookup-id`

**Cenário:** transversal
**Status:** proposta · 🟢 Baixa

Recebe nome (item, trader, mapa, bot) e roteia consulta na ordem definida em [resources.md](resources.md):

1. `wiki/spt/modding/references/*` (offline, versionado)
2. db.sp-tarkov.com (fallback)
3. Tarkynator / tarkov.dev

Retorna ID + fonte.

---

## `/check-mod-compat`

**Cenário:** transversal
**Status:** proposta · 🟢 Baixa

Pré-instalação: cruza mod com [Known_Mod_Issues_40.md](../wiki/spt/Known_Mod_Issues_40.md), confere SPT version no manifesto, e checa conflitos com mods já em [mods-inventory.md](../docs/migration/mods-inventory.md). Equivalente local ao [SPT-Check-Mods](https://github.com/refringe/SPT-Check-Mods).

---

## `/modding-help`

**Cenário:** transversal
**Status:** proposta · 🟢 Baixa

Q&A guiado: usa hierarquia de [resources.md](resources.md) — wiki primeiro, deepwiki/external como fallback. Útil para perguntas tipo "como faço X em mod 4.0?" sem o agente improvisar.

---

## Como começar

Sugestão de ordem:

1. **`/classify-mod`** primeiro — é a menor (workflow + edição de tabela), valida se a estrutura `.claude/skills/` resolve antes de investir em scaffolders.
2. **`/migrate-3to4`** depois — ataca diretamente o cenário de maior volume (mods 3.x para portar).
3. Demais skills em ordem de prioridade conforme demanda real.
