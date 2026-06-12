# 020 — Infra web Blazor (padrão Skills-Extended) · Spec

**Mod:** CustomClasses
**Status:** Implementado
**Criado:** 2026-06-10
**Origem:** [020-infra-web-blazor-00-kickoff.md](./020-infra-web-blazor-00-kickoff.md)

## Visão geral

Habilitar o mod a servir páginas web pelo próprio server SPT, replicando o padrão do Skills-Extended: Blazor Server + MudBlazor via marker `IModWebMetadata` + pacote `SPTarkov.Server.Web`. Entrega o walking skeleton da UI do editor de classes — layout base, navegação e uma home com smoke test de acesso ao DI do server.

## Escopo

1. **csproj** migra para `Sdk="Microsoft.NET.Sdk.Web"`, mantendo `AssemblyName CustomClasses-Server` e `net9.0`; adiciona `SPTarkov.Server.Web` e alinha os pacotes `SPTarkov.*` à versão usada pelo Skills-Extended.
2. **`CustomClassesMetadata`** implementa `IModWebMetadata` (marker vazio) — o host SPT registra as páginas Razor da assembly e monta `wwwroot/` em `/{AssemblyName}/`.
3. **`Web/`**: `_imports.razor`, `Layouts/BaseLayout.razor` (MudBlazor dark, AppBar "CustomClasses — Class Editor", drawer com NavMenu), `Shared/NavMenu.razor` (Home + Classes), `Pages/Home.razor` com `@page "/customclasses"`.
4. **Convenção de rotas:** todas as páginas do mod sob `/customclasses/...` (rotas `@page` são globais no server — prefixo evita colisão com outros mods web). Estáticos em `/CustomClasses-Server/...`.
5. **`wwwroot/icons/`** com os PNGs de classe (cópia vendored dos PNGs do Client); `scripts/build-icons.mjs` passa a emitir nos dois destinos.

## Critérios de aceite (= DoD do kickoff)

- [x] `https://<host>:6969/customclasses` abre com layout MudBlazor e lista as edition keys de `GetProfileTemplates()` (vanilla + classes do mod). — verificado via curl: HTML renderizado contém AppBar, MudBlazor e editions.
- [x] Server SPT 4.0.13 boota com as classes carregando (verificação do bump NuGet) — log: `Loaded 12 class(es), skipped 0` (12, não 11: o item 016 adicionou a classe Peladão depois do kickoff).
- [x] `/compile-mod CustomClasses` instala DLL + `wwwroot/` (suporte a wwwroot já existia do item 019) e o client continua compilando.

## Fora de escopo

- Lista REAL de classes na UI (item 024); dropdown de ícones (item 025); qualquer escrita de config pelo editor.
- `CustomClassesMod.cs` / registries (itens 021/022, em paralelo) — não tocados.
