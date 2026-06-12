# 020 — Infra web Blazor · Spec técnica

**Mod:** CustomClasses
**Status:** Implementado
**Criado:** 2026-06-10
**Refs:** [01-spec](./020-infra-web-blazor-01-spec.md) · `mods/Skills-Extended/modded/Server/` (padrão replicado) · `references/spt-source/Libraries/SPTarkov.Server.Web/`

## Decisões técnicas

### 1. Versões NuGet (finais)

| Pacote | Antes | Depois | Nota |
|---|---|---|---|
| `SPTarkov.Server.Core` | 4.0.0 (pinned vs SkillDistribution) | **4.0.2** | mesma versão do Skills-Extended |
| `SPTarkov.Server.Web` | — | **4.0.2** | traz MudBlazor **8.13.0** transitivo |
| `SPTarkov.DI` | 4.0.0 | **4.0.2** | |
| `SPTarkov.Common` | 4.0.0 | **4.0.2** | |

Restore e build limpos na primeira tentativa (sem fallback necessário). Boot no server SPT 4.0.13 instalado confirma compat binária: 12 classes registradas, página servida.

### 2. csproj (Sdk.Web)

- `Sdk="Microsoft.NET.Sdk.Web"` + **`<OutputType>Library</OutputType>`** — o default do Sdk.Web é `Exe`; sem isso o build exige `Main`. Mesmo padrão do `Server.csproj` do SE.
- `AssemblyName CustomClasses-Server`, `RootNamespace CustomClasses`, `net9.0` preservados — o `AssemblyName` define a rota dos estáticos (`/CustomClasses-Server/...`).
- O `compile-mod.sh` segue classificando como **server** (`csproj_kind` casa `SPTarkov\.`) e o filtro `OWN_ASSEMBLIES` (por `AssemblyName`) ignora artefatos extras do Sdk.Web — só `CustomClasses-Server.dll` é instalada. O suporte a `wwwroot/` no install já existia (item 019, compile-mod.sh:340-345, clobber intencional).

### 3. Metadata / host (achados do SPTWeb.cs)

- `IModWebMetadata` é um **marker vazio** em `SPTarkov.Server.Web` (arquivo `IModBlazorMetadata.cs`). `CustomClassesMetadata` agora implementa `AbstractModMetadata, IModWebMetadata`.
- `SPTWeb.InitializeSptBlazor`: filtra mods com o marker, chama `AddMudServices()` (MudBlazor já vem registrado pelo host — o mod não registra nada), `AddApplicationPart` por assembly e `AddRazorComponents().AddInteractiveServerComponents()`.
- `SPTWeb.UseSptBlazor`: `AddAdditionalAssemblies` (descoberta das páginas `@page`) e, se existir `wwwroot/` ao lado da DLL do mod, monta `PhysicalFileProvider` em `RequestPath = "/{AssemblyName}"` → `/CustomClasses-Server/icons/*.png`.
- Consequência: rotas `@page` são **globais** → convenção: todas as páginas do mod sob `/customclasses/...`.
- DI: páginas Blazor enxergam os services SPT (ex.: `DatabaseService`) direto via `@inject` — mesmo mecanismo que o SE usa com `FileUtil`/`JsonUtil`.

### 4. Web/ (estrutura e namespaces)

| Arquivo | Namespace/rota | Nota |
|---|---|---|
| `Web/_imports.razor` | usings `CustomClasses`, `CustomClasses.Web.Shared`, `CustomClasses.Web.Layouts` | sem os usings de Authorization do SE (não usados) |
| `Web/Layouts/BaseLayout.razor` | — | providers MudBlazor (Theme dark/Popover/Snackbar/Dialog), AppBar "CustomClasses — Class Editor", drawer Mini com hover (estrutura do SE, sem Save/UpdateChecker) |
| `Web/Shared/NavMenu.razor` | — | Home + Classes (ambos `/customclasses` por ora; Classes vira página própria no item 024); ícones Material em vez de PNG |
| `Web/Pages/Home.razor` | `@page "/customclasses"` | smoke test: injeta `DatabaseService`, lista keys de `GetProfileTemplates()` (ref: DatabaseService.cs:141) numa MudTable com chip vanilla/mod |

**Flag "vanilla":** `HashSet` estático com as 9 editions do SPT 4.0 (`SPT_Data/database/templates/profiles.json`: Standard, Left Behind, Prepare To Escape, Edge Of Darkness, Unheard, Tournament, SPT Developer, SPT Easy start, SPT Zero to hero). Tudo que não está no set foi injetado por mod em runtime. Heurística suficiente para smoke test; deliberadamente NÃO lê os registries do mod (itens 021/022 em refactor paralelo).

### 5. Ícones — estratégia vendored

- `Server/wwwroot/icons/*.png` é **cópia vendored no repo** (decisão registrada no kickoff/orquestração): os 12 PNGs de `Client/icons/` + `ATTRIBUTION.md` (game-icons.net é CC BY 3.0 — atribuição acompanha a cópia distribuída).
- `scripts/build-icons.mjs` agora rasteriza uma vez (`toBuffer`) e grava nos **dois destinos** (`Client/icons/` e `Server/wwwroot/icons/`) — regenerar arte mantém os dois em sincronia.
- URL de consumo futuro (item 025): `/CustomClasses-Server/icons/<classe>.png`.

## Riscos/observações

- O server binda no IP configurado em `http.json` (neste install, IP Radmin `26.207.194.149`) — `127.0.0.1` não responde; o DoD "localhost:6969" vale para installs com bind default.
- `SPT.Server.exe` com stdout redirecionado (sem console) crasha em `SetConsoleOutputMode`; bypass: env `DISABLE_VIRTUAL_TERMINAL=1` (ref: spt-source `SPTarkov.Server/Program.cs:270-295`). Útil para boot tests automatizados.
