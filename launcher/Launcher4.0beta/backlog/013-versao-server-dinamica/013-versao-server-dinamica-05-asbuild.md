# 013 — Versão do server dinâmica (013L) · As-built

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Insumo:** [02-spec-tech](./013-versao-server-dinamica-02-spec-tech.md)

## Arquivos alterados

| Arquivo | Mudança |
|---|---|
| `project/SPT.Launcher.Base/MiniCommon/Request.cs` | `GetJson()` ganhou parâmetro opcional `decompressResponse = true` (espelho do `PostJson`). Necessário porque os endpoints `/redline/*` respondem JSON puro (ASP.NET), sem zlib — callers existentes inalterados. |
| `project/SPT.Launcher.Base/Controllers/RequestHandler.cs` | Novo `RequestTrlServerVersion()` → `GET /redline/server/version` com `decompressResponse: false`. |
| `project/SPT.Launcher.Base/Controllers/ServerManager.cs` | Nova propriedade `TrlServerVersion { get; private set; } = "—"` + DTO privado `TrlServerVersionResponse` + `LoadTrlServerVersion()` (try/catch, nunca lança). Chamada no fim de `LoadServer()` **após** o `RequestConnect()` bem-sucedido. |
| `project/SPT.Launcher/Views/LoginView.axaml` | Footer: `LauncherVersion="{x:Static helpers:LauncherUpdateHelper.CurrentVersion}"` + `ServerVersion="{x:Static base:ServerManager.TrlServerVersion}"` (xmlns `base` → `SPT.Launcher;assembly=SPT.Launcher.Base`). Só o footer — fundo/layout intocados. |
| `project/SPT.Launcher/Views/RegisterView.axaml` | Idem LoginView. |
| `project/SPT.Launcher/ViewModels/ProfileViewModel.cs` | `_serverVersion` default `"1.5.7"` → `ServerManager.TrlServerVersion`; removido o bloco de `InitializeAsync()` que lia `serverVersion` do `config.json` do TarkovRedLine-ServerMod (fonte local defasável). |

## Decisões e assunções

- **`x:Static` (read-once) em vez de binding reativo nos footers**: Login/Register/Profile só são instanciadas depois do connect (fluxo `ConnectServerViewModel → LoadDefaultServerAsync → navegação`), então `TrlServerVersion` já está resolvida quando o XAML carrega. Falha de fetch → footer mostra `"—"` (fallback do contrato).
- **Launcher version**: fonte por ora é a const `LauncherUpdateHelper.CurrentVersion` ("1.4.7") — o item 014 unifica depois.
- **Defaults do `TrlVersionFooter` ("15.0"/"0.10") mantidos** no controle: são só fallback de design-time; todos os usos reais agora passam valores.
- `ClassSelectionView`/`ClassSelectionViewModel` intocadas (item 004L, outro agente).

## Build

`dotnet build project/SPT.Launcher/SPT.Launcher.csproj` → **0 erros**, 126 warnings pré-existentes (nullability CS86xx + CA1416 registry), 8.7s.

Validação em runtime (footer mostrando a versão real vinda do endpoint) fica com o orquestrador — launcher não é executado por este agente.
