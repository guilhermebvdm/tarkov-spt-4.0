# 020 — Infra web Blazor (padrão Skills-Extended) · Kickoff

**Mod:** CustomClasses · **Data:** 2026-06-09 · **Origem:** plano aprovado do editor web de classes (`~/.claude/plans/`, sessão 2026-06-09; renumerado 019→020)
**Wave:** W1 (inicia **após merge do 019** — DoD usa a cópia de wwwroot do compile-mod) · **Deps:** 019

> Brief de kickoff — insumo para `/create-spec 020`. Não é a spec.

## Objetivo

Habilitar o mod a servir páginas web pelo próprio server SPT (padrão Skills-Extended): Blazor Server + MudBlazor via `IModWebMetadata` + `SPTarkov.Server.Web`. Walking skeleton da UI do editor.

## Escopo

- **csproj:** `Sdk` → `Microsoft.NET.Sdk.Web`; `PackageReference SPTarkov.Server.Web 4.0.2` (MudBlazor vem transitivo; host chama `AddMudServices`). Bump `SPTarkov.Server.Core` 4.0.0→4.0.2 — **premissa a verificar:** compat binária com o server 4.0.13 instalado (DoD inclui boot com as 11 classes). Fallbacks: Core 4.0.0 + Web 4.0.2, ou alinhar à versão real do install. Nota: o pin atual em 4.0.0 tem comentário deliberado ("pinned vs SkillDistribution") no csproj.
- **Metadata:** `CustomClassesMetadata` implementa `IModWebMetadata` (marker — o host registra as páginas Razor da assembly e monta `wwwroot/`).
- **Estrutura:** `Web/{Layouts,Pages,Shared}` + `_imports.razor` + `wwwroot/`; `BaseLayout` MudBlazor com nav; home placeholder (smoke test: lista as edition keys de `GetProfileTemplates()` — a lista REAL de classes é do 024).
- **Convenção de rotas:** páginas Blazor roteiam pela diretiva `@page` na **raiz** do server → todas sob `@page "/customclasses/..."` (evita colisão com outros mods web); estáticos do wwwroot em `/{AssemblyName}/` = `/CustomClasses-Server/...`.
- **Ícones de classe no `wwwroot/icons/` do server** (habilita preview no editor — dropdown do 025; o client mantém a cópia local em `BepInEx/plugins/CustomClasses/icons/`). Estratégia de cópia (build via compile-mod vs vendored no repo) decidida na tech-spec.
- Verificar que `csproj_kind` do compile-mod segue classificando como server (ref `SPTarkov.*`) e que o filtro `OWN_ASSEMBLIES` ignora os artefatos extras do Sdk.Web (staticwebassets/manifests).

## Refs

- [mods/Skills-Extended/modded/Server/Server.csproj](../../../Skills-Extended/modded/Server/Server.csproj) — padrão a replicar (Sdk.Web + Web 4.0.2)
- `mods/Skills-Extended/modded/Server/Web/` — Layouts/Pages/_imports (BaseLayout com Save é referência do 025)
- `references/spt-source/Libraries/SPTarkov.Server.Web/SPTWeb.cs` — `InitializeSptBlazor`/`UseSptBlazor` (mount de wwwroot, descoberta de páginas)
- [modded/Server/CustomClassesMetadata.cs](../../modded/Server/CustomClassesMetadata.cs)

## DoD (resumo)

- `https://localhost:6969/customclasses` abre com layout MudBlazor e lista as edition keys.
- Server 4.0.13 boota com as 11 classes carregando (verificação do bump NuGet).
- `/compile-mod CustomClasses` instala DLL + wwwroot e o client continua funcionando.
