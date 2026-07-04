# 013 — Versão do server dinâmica · Spec técnica

**Launcher:** Launcher4.0beta · **Data:** 2026-07-03 · **Kickoff:** [013-versao-server-dinamica-00-kickoff.md](./013-versao-server-dinamica-00-kickoff.md)

## Server (013S) — entregue

### Contrato (SP0, congelado 2026-07-03)

`GET /redline/server/version` → `200 OK`

```json
{ "version": "0.1.0-beta" }
```

- Fonte: `Launcher-Updater/server-version.txt` no disco do server, conteúdo com `Trim()`.
- Arquivo ausente, vazio ou ilegível → default embutido no controller: `"0.1.0-beta"`.
- **Não confundir** com `/launcher/server/version` do SPT core (versão do SPT) nem com `/redline/launcher/version` (versão do exe do launcher servida pelo `LauncherUpdaterController`).

### Arquivos

| Arquivo | Mudança |
|---|---|
| `mods/TarkovRedLine4.0/Server/TarkovRedLine.Server/Controllers/ServerVersionController.cs` | **Novo.** `[ApiController]` + `[Route("redline/server")]`, action `GetServerVersion()` em `[HttpGet("version")]`. Lê `server-version.txt` via `LauncherUpdaterController.GetUpdaterBasePath()`; fallback `DefaultServerVersion = "0.1.0-beta"` (const `internal`). |
| `mods/TarkovRedLine4.0/Server/TarkovRedLine.Server/Controllers/LauncherUpdaterController.cs` | `GetUpdaterBasePath()` promovido de `private` para `internal` (reuso do resolver da pasta `Launcher-Updater/`, que sobe até 4 níveis a partir de `AppDomain.CurrentDomain.BaseDirectory`). |

**Decisão registrada:** controller novo (e não action no `LauncherUpdaterController`) porque o contrato exige o prefixo de rota `redline/server`, distinto do `redline/launcher` do controller existente — uma action lá exigiria rota absoluta (`[HttpGet("/redline/server/version")]`), mais surpreendente que um controller dedicado de ~50 linhas. Helper de path é reutilizado, não duplicado.

**Build gate:** `dotnet build mods/TarkovRedLine4.0/Server/TarkovRedLine.Server/TarkovRedLine.Server.csproj -c Release` → **0 erros** (33 warnings pré-existentes de nullability em HwidManager/PasswordController/ModUpdater, não relacionados).

### Como testar

1. Deploy da DLL no server (parar `SPT.Server` → copiar DLL → reiniciar — ver rotina de deploy do mod).
2. Criar o arquivo de versão (opcional — sem ele vale o default):
   ```
   D:\SPT\SPT\Launcher-Updater\server-version.txt   ← conteúdo: 0.1.0-beta
   ```
3. Consultar:
   ```bash
   curl http://127.0.0.1:6969/redline/server/version
   # → {"version":"0.1.0-beta"}
   ```
4. Casos: arquivo com espaços/newline → vem trimado; arquivo vazio ou ausente → `0.1.0-beta`; editar o txt e repetir o curl → nova versão **sem rebuild** (lido a cada request).

## Launcher (013L) — plano (NÃO implementado neste item)

Metade cliente, a executar em item próprio (W2):

1. **Propriedade única** — expor a versão TRL do server em um único lugar: `ServerManager` (`launcher/Launcher4.0beta/project/SPT.Launcher.Base/Controllers/ServerManager.cs`), ex. `public static string TrlServerVersion { get; private set; } = "—";`, com método `GetTrlServerVersion()` no padrão dos demais (`GetVersion()`, `GetCompatibleGameVersion()`).
2. **Fetch junto do connect** — em `ServerManager.LoadServer()` (chamado por `LoadDefaultServerAsync`), após o `RequestConnect()` bem-sucedido, chamar o novo request `GET /redline/server/version` via `RequestHandler` (novo método, mesmo padrão de `RequestServerVersion()` em `SPT.Launcher.Base/MiniCommon/RequestHandler.cs`). Falha/offline → `"—"` (nunca lançar).
3. **Binding nos footers** — substituir o texto hardcoded "Versão do servidor: 0.10" pelo binding da propriedade (via `TrlVersionFooter`, que nasce no item 015 — tema TRL fundação) nas views que hoje têm o footer hardcoded: `LoginView.axaml`, `RegisterView.axaml`. `ClassSelectionView.axaml` fica de fora — o item 004L (classes com dados reais) é quem instala o `TrlVersionFooter` lá. ProfileView: verificar na hora — a varredura de hoje não encontrou o padrão "Versão do servidor" nela.

Dependências: 015 (`TrlVersionFooter`) para o passo 3; passos 1–2 são independentes e podem ir antes.

## Launcher (013L) — entregue

Executado em 2026-07-04 conforme o plano acima. Detalhes em [013-versao-server-dinamica-05-asbuild.md](./013-versao-server-dinamica-05-asbuild.md).

- `RequestHandler.RequestTrlServerVersion()` → `GET /redline/server/version` com `decompressResponse: false` (endpoint redline responde JSON puro, sem zlib — mesmo motivo do `RequestAccount`). Para isso, `Request.GetJson()` ganhou o parâmetro opcional `decompressResponse` (default `true`, espelho do `PostJson` — nenhum caller existente muda de comportamento).
- `ServerManager.TrlServerVersion` (default `"—"`), populada por `LoadTrlServerVersion()` chamado no fim de `LoadServer()` após connect bem-sucedido; qualquer falha mantém `"—"`. (Desvio menor do plano: sem método `GetTrlServerVersion()` público — a propriedade estática basta para os consumidores atuais.)
- Footers de `LoginView`/`RegisterView`: `TrlVersionFooter` recebeu `LauncherVersion="{x:Static LauncherUpdateHelper.CurrentVersion}"` e `ServerVersion="{x:Static ServerManager.TrlServerVersion}"` (views só existem depois do connect, então o valor estático já está resolvido). `ClassSelectionView` intocada (004L).
- `ProfileViewModel.ServerVersion`: default `"1.5.7"` → `ServerManager.TrlServerVersion`; o read do `config.json` do TarkovRedLine-ServerMod em `InitializeAsync()` foi **removido** (fonte local defasável, substituída pelo endpoint).

**Build gate:** `dotnet build SPT.Launcher.csproj` → **0 erros** (126 warnings pré-existentes de nullability/CA1416).
