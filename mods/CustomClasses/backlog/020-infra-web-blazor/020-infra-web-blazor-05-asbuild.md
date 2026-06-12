# 020 — Infra web Blazor · As-built

**Mod:** CustomClasses
**Status:** Concluído (boot test OK)
**Data:** 2026-06-10
**Refs:** [01-spec](./020-infra-web-blazor-01-spec.md) · [02-spec-tech](./020-infra-web-blazor-02-spec-tech.md)

## Arquivos

| Arquivo | Mudança |
|---|---|
| `modded/Server/CustomClasses.Server.csproj` | Sdk → `Microsoft.NET.Sdk.Web` + `OutputType Library`; pacotes SPTarkov.* 4.0.0→**4.0.2**; + `SPTarkov.Server.Web 4.0.2` |
| `modded/Server/CustomClassesMetadata.cs` | + `IModWebMetadata` (using `SPTarkov.Server.Web`) |
| `modded/Server/Web/_imports.razor` | novo |
| `modded/Server/Web/Layouts/BaseLayout.razor` | novo — MudBlazor dark, AppBar, drawer + NavMenu |
| `modded/Server/Web/Shared/NavMenu.razor` | novo — Home/Classes (ambos `/customclasses` por ora) |
| `modded/Server/Web/Pages/Home.razor` | novo — `@page "/customclasses"`, smoke test `DatabaseService.GetProfileTemplates()` em MudTable |
| `modded/Server/wwwroot/icons/` | novo — 12 PNGs vendored (cópia de `Client/icons/`) + `ATTRIBUTION.md` |
| `scripts/build-icons.mjs` | emite os PNGs nos dois destinos (Client/icons e Server/wwwroot/icons) |

## Build

- `dotnet build -c Release` → **0 erros, 0 warnings** (4.0.2 em tudo, sem fallback). MudBlazor 8.13.0 transitivo.
- `compile-mod.sh CustomClasses`: client + server OK; anti-clobber de config disparou (install tinha `armeiro.jsonc` mais novo + `_test019.jsonc` órfão) → rerun com **`--force-config`** conforme decisão da orquestração (hoje o repo é a verdade; o editor web ainda não escreve no install). Install final: DLL + `config/` + `wwwroot/`.

## Boot test (2026-06-10)

- Porta 6969 livre → server iniciado: `D:/SPT/SPT/SPT.Server.exe` com `DISABLE_VIRTUAL_TERMINAL=1` (sem o env var, crash `Unable to get console mode` quando stdout é redirecionado — ver spec-tech §Riscos).
- Log: `[CustomClasses] Loaded 12 class(es), skipped 0` — **12**, não 11: item 016 (Peladão) entrou após o kickoff. 12× `Registered '<classe>'`.
- `curl -k https://26.207.194.149:6969/customclasses` → **200**, 28 KB de HTML com `CustomClasses — Class Editor`, assets MudBlazor e editions renderizadas (Standard, Edge Of Darkness, Armeiro, chips vanilla). `https://127.0.0.1:6969` **não** responde — o install binda no IP Radmin configurado em `http.json` (não é regressão do mod).
- Server finalizado ao término do teste (porta 6969 liberada).

## Pendências

- Smoke visual em browser real (curl validou só o HTML pré-renderizado; o circuito interativo SignalR não foi exercitado) — cobre no item 024.
- Build integrado com os refactors paralelos 021/022 (`CustomClassesMod.cs`/registries/services) — orquestrador builda depois; neste momento o projeto inteiro compilou limpo.
- NavMenu "Classes" aponta para `/customclasses` (placeholder) até o item 024.
