# 021 — Mods opcionais: grupos faltantes + base-URL + descrição + I/O · Spec técnica

> **Data:** 2026-07-04<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [00-kickoff](./021-optional-mods-groups-baseurl-00-kickoff.md) · [AUDIT-2026-07-04](../../AUDIT-2026-07-04-code-product-ds.md) · [01-spec](./021-optional-mods-groups-baseurl-01-spec.md)<br>

---

## Mapa do defeito (file:line reais, confirmados por leitura)

### 1. Base-URL errada (raiz do "falha em silêncio")
`SPT.Launcher/Helpers/OptionalModsHelper.cs:45-57`
```csharp
private static string GetServerBaseUrl()
{
    var serverUrl = LauncherSettingsProvider.Instance.Server?.Url ?? "https://127.0.0.1:6969";
    try { var uri = new Uri(serverUrl); return $"http://{uri.Host}"; }   // ← derruba porta, força http
    catch { return "http://127.0.0.1"; }
}
```
- Usada em `DownloadOptionalGroupAsync` (`:218`) e `DownloadFromOpcionaisFolder` (`:314`).
- O download roda em `HttpClient` cru (`:219-220`, `:315-316`) — **não** honra o bypass TLS global, então nem "consertar para https" resolve sozinho.
- **Comparação com o caminho correto:** `SPT.Launcher.Base/Controllers/RequestHandler.cs:183-186` (`DownloadModFile`) monta `{request.RemoteEndPoint}/launcher/mods/download?file=...` e usa `DownloadBinary` (`:206-227`), que **lança** em erro (não engole). `request.RemoteEndPoint` é setado em `ChangeBackendUrl` (`:21-24`) = `Server.Url` (esquema+porta reais). O bypass TLS self-signed é process-wide via `SPT.Launcher.Base/MiniCommon/Request.cs:33` (`ServicePointManager.ServerCertificateValidationCallback = delegate { return true; }`), honrado por `WebRequest` — **não** por `HttpClient`/`SocketsHttpHandler`. ⇒ a correção **é reusar `RequestHandler`**, não editar a string da URL.

### 2. Exceção engolida (silêncio)
`OptionalModsHelper.cs:257-260` (grupo) e `:356-359`, `:362-365` (offFolders) — `catch { LogManager...Warning }`. Não há retorno de falha; `DownloadOptionalGroupAsync` é `async Task` (void-ish), o chamador não sabe que falhou.

### 3. I/O + MD5 na UI thread
- `OnOptionalToggled` inicia na UI thread e **não** usa `ConfigureAwait(false)` (`ProfileViewModel.cs:248-296`, awaits em `:267`/`:272`).
- `GetFileMd5` lê o arquivo inteiro e computa MD5 **síncrono** (`OptionalModsHelper.cs:239`, `:368-374`).
- `File.WriteAllBytes` + `Directory.CreateDirectory` síncronos (`:253-255`, `:351-354`).

### 4. Escrita/exclusão fora do motor
- `Path.Combine(GamePath, relativePath)` sem guard (`:234`, `:302`, `:351`).
- `File.WriteAllBytes` direto, sem `.sync-tmp`+move (`:255`, `:354`).
- `DeleteFileIfExists` → `File.Delete` permanente (`:386-389`) — não vai pra lixeira (diferente de `ProfileViewModel.DeleteToRecycleBin`, `:858-870`).

### 5. Lacuna de conteúdo (grupos + descrição) — server, não launcher
- `optionalGroups` de produção (TS): `TarkovRedLine-ServerMod/config.json:26-58` — só `gore`/`grass`/`hollywood`.
- Templates de descrição existem para 4 pastas: `TarkovRedLine.Server/Launcher-Updater-templates/Opcionais/{Hollywood,PiPDisable,IRL,Visceral}/description.json` — **`PiPDisable` e `IRL` ficam órfãos** (sem grupo em `optionalGroups`).
- Join descrição↔grupo: `ProfileViewModel.cs:353-378` (`FindOptionalDescriptor`, 3 passadas: `id` exato → `group.folders` contém `descriptor.id` → `descriptor.name == group.name`, tudo case-insensitive). Com os nomes atuais só `hollywood` casa (por `id` "hollywood" ≈ pasta "Hollywood"); `gore`/`grass` caem no fallback legado.
- **Discrepância de server (D-021.B):** `optionals-list` novo (com `description.json`) está no **C#** (`ModUpdater.cs:174-229`), mas o C# **não** taggeia opcionais no manifesto (`GenerateManifestAsync:303-437` só varre `mods_repo`, nunca seta `optional`/`optionalGroup`). O **TS** taggeia (`modUpdater.ts:108-168`) mas seu `optionals-list` devolve shape antigo (`modUpdater.ts:244-247`, só nomes). Nenhum entrega os dois lados.

## Abordagem

### Launcher (código deste item)

**Passo 1 — Base-URL via `RequestHandler` (CA-021.1/2/3).**
- Adicionar wrappers em `RequestHandler` para os endpoints de opcionais que ainda não têm (paridade com `DownloadModFile`/`DownloadPerformanceFile`, `RequestHandler.cs:183-204`):
  - `RequestOptionalsManifest(string folder)` → `GetJson("/launcher/mods/optionals-manifest?folder=...")` (ou GET simples, sem zlib — hoje `DownloadFromOpcionaisFolder` lê JSON puro).
  - `DownloadOptionalFile(string folder, string file)` → `DownloadBinary($"{RemoteEndPoint}/launcher/mods/optional-download?folder=...&file=...")`.
- Em `OptionalModsHelper`: **remover `GetServerBaseUrl` e o `HttpClient`**; `DownloadOptionalGroupAsync` passa a chamar `RequestHandler.DownloadModFile(file.path)` (mesmo endpoint `download?file=`); `DownloadFromOpcionaisFolder` passa a usar os wrappers novos. Nota: `download?file=` já é o endpoint que os opcionais usam hoje (`:248`), só muda a **via** (RequestHandler em vez de HttpClient+URL torta).

**Passo 2 — Falha visível (CA-021.4/5/6).**
- `DownloadOptionalGroupAsync` e `DownloadFromOpcionaisFolder` retornam um resultado (ex.: `OptionalOpResult { int Total, int Ok, int Failed, List<string> FailedPaths }`) em vez de `Task` void. O `catch` interno incrementa `Failed` (continua logando).
- `OnOptionalToggled` (`ProfileViewModel.cs:248-296`) inspeciona o resultado: `Failed > 0` ⇒ setar `UpdateStatusText` de erro ("N de M arquivos falharam") e **não** exibir `update_up_to_date`. Total-falha ⇒ aplicar **D-021.A** (preferência: `SetOptionalEnabled(id,false)` + `toggle.IsEnabled = false` marshalado na UI, com guarda `Skip(1)` para não re-disparar o toggle).
- Cuidado: `SetOptionalEnabled` é chamado **antes** do download (`:253`). Mover a persistência do estado "ligado" para **depois** do sucesso, ou reverter no erro.

**Passo 3 — Off-thread (CA-021.7).**
- Envolver o corpo de rede/IO em `Task.Run(...)` **ou** garantir `ConfigureAwait(false)` em toda a cadeia de `OptionalModsHelper` e mover `GetFileMd5`/escrita para dentro do `Task.Run`. `DownloadModFile`/`DownloadBinary` já são síncronos-bloqueantes ⇒ chamar dentro de `Task.Run`.
- Progresso/status continuam via `OnProgressChanged`/`OnStatusMessageChanged`, já marshalados por `Dispatcher.UIThread.Post` no `ProfileViewModel:259-260`. Não tocar nesse contrato.

**Passo 4 — Robustez de escrita/exclusão (CA-021.8/9/10).**
- Contenção: replicar o guard `ResolveUnderRoot` (`SyncEngine.cs:248-258`) — resolver `Path.GetFullPath` e exigir `StartsWith(gameRootFullPrefix, OrdinalIgnoreCase)`; senão, rejeitar+logar (não escrever/apagar). Considerar extrair um helper público (`SyncPathUtil.ResolveUnderRoot(root, rel)`) para não duplicar — `SyncPathUtil` já é `public static` (`SyncPathUtil.cs:11`) e tem `ToLocalPath`/`ComputeMd5`.
- Escrita atômica: usar `.sync-tmp`+move (padrão de `SyncEngine.ApplyAtomic`, `:260-267`) em vez de `File.WriteAllBytes`.
- Exclusão: trocar `File.Delete` (`:388`) por `DeleteToRecycleBin`. Como `DeleteToRecycleBin` hoje é `private static` no `ProfileViewModel`, extrair para um helper compartilhado (ex.: `RecycleBinHelper`) ou expor — evita 3ª cópia (já há 2: `ProfileViewModel.cs:858`, `ModUpdateViewModel.cs:465`).
- Hash: reusar `SyncPathUtil.ComputeMd5` (`SyncPathUtil.cs:88`) em vez de `GetFileMd5` local.

### Server (gate humano, D-021.B — fora do código do launcher)
- Escolher o server de produção e completar a metade que falta:
  - Se **TS**: subir `optionals-list` para o shape com `description.json` (paridade com `ModUpdater.cs:174-229`); adicionar grupos `PiPDisable`/`IRL` em `config.json` com `folders`/`targetSubDir` reais + arquivos em `Opcionais/`.
  - Se **C#**: taggear opcionais no `GenerateManifestAsync` (varrer `Opcionais/<folders>` e emitir `optional:true, optionalGroup:id`, como o TS faz).
- Alinhar nomes: para a descrição nova alcançar **todos** os grupos pelo join (`FindOptionalDescriptor`), garantir, por grupo, uma pasta descriptor cujo nome (= `descriptor.Id`) case com `group.id` **ou** conste em `group.folders` (case-insensitive). Recomendação: `Opcionais/<id>/description.json` por grupo (`gore`, `grass`, `hollywood`, `pipdisable`, `irl`).

## Arquivos a tocar (launcher)

| Arquivo | Mudança |
|---|---|
| `SPT.Launcher/Helpers/OptionalModsHelper.cs` | remover `GetServerBaseUrl`+`HttpClient`; usar `RequestHandler`; retornar resultado (Total/Ok/Failed); off-thread; guard de path; escrita atômica; lixeira; reusar `SyncPathUtil.ComputeMd5` |
| `SPT.Launcher.Base/Controllers/RequestHandler.cs` | novos wrappers `RequestOptionalsManifest(folder)` e `DownloadOptionalFile(folder,file)` (paridade `:183-204`) |
| `SPT.Launcher/ViewModels/ProfileViewModel.cs` | `OnOptionalToggled` (`:248-296`): consumir resultado, estado de erro visível, D-021.A (reverter toggle), mover `SetOptionalEnabled` para pós-sucesso |
| `SPT.Launcher.Base/Sync/SyncPathUtil.cs` *(opcional)* | expor `ResolveUnderRoot(root, rel)` público reusável |
| *(novo, opcional)* `SPT.Launcher/Helpers/RecycleBinHelper.cs` | consolidar `DeleteToRecycleBin` (hoje duplicado em `ProfileViewModel.cs:858`, `ModUpdateViewModel.cs:465`) |

## Contratos / DTOs

- **Resultado de operação (novo):**
  ```csharp
  public sealed class OptionalOpResult {
      public int Total; public int Ok; public int Failed;
      public List<string> FailedPaths = new();
      public bool AllOk => Failed == 0;
  }
  ```
- **Endpoints (inalterados no server):** `GET /launcher/mods/download?file=` · `GET /launcher/mods/optionals-manifest?folder=` · `GET /launcher/mods/optional-download?folder=&file=` · `GET /launcher/mods/optionals-list`. Só muda o **cliente** (via `RequestHandler`).
- **Descriptor (item 009, inalterado):** `OptionalFolderDescriptor { Id, Name, DescriptionPt, DescriptionEn }` (`OptionalModsHelper.cs:408-414`); parsing tolerante aos dois shapes preservado (`:109-174`).

## Riscos

- **R-1 — `HttpClient` → `WebRequest` muda semântica de timeout/cancelamento.** `HttpClient.Timeout` era 5 min (`:220`); `DownloadBinary` usa 30 s (`RequestHandler.cs:212`). Arquivos opcionais grandes podem estourar 30 s. **Mitigar:** parametrizar timeout no wrapper de download de opcional (ex.: manter 5 min para binários grandes).
- **R-2 — Cancelamento em voo.** `WebRequest` síncrono em `Task.Run` não aborta no meio (mesma limitação do engine, AUDIT 🟢 `SyncEngine.cs:116-118`). Aceitável: o run para entre arquivos; documentar.
- **R-3 — Reverter toggle re-dispara o subscribe.** `toggle.WhenAnyValue(IsEnabled).Skip(1).Subscribe(...)` (`ProfileViewModel.cs:625`) — setar `IsEnabled=false` no erro dispara `OnOptionalToggled` de novo (loop de "desativar" que tenta baixar offFolders). **Mitigar:** flag de supressão ao reverter, ou reverter só o estado persistido sem tocar `toggle.IsEnabled`, ou usar um bool guard.
- **R-4 — Duplicação de `DeleteToRecycleBin`.** Extrair helper toca `ProfileViewModel`/`ModUpdateViewModel` (arquivos compartilhados com outros itens — ver Paralelismo). Alternativa de menor blast radius: copiar o método privado para `OptionalModsHelper` e consolidar depois.
- **R-5 — Aceite E depende de server (D-021.B).** CA-021.11/12 **não fecham** só com código do launcher; travados no gate humano G-5. Deixar explícito para não marcar o item "done" prematuramente.
- **R-6 — Coop.** Até o fix, cada cliente Fika falha o download em silêncio ⇒ assets divergentes sem erro. G-4 é o único gate que expõe isso (solo=host mascara).

## Plano de teste

### Unit (`SPT.Launcher.Tests`, xUnit — projeto já existe, ver `Sync/*.cs`)
Componente testável sem exe: extrair a lógica pura (resolução de path, montagem de resultado, decisão de sucesso/falha) de `OptionalModsHelper` para métodos injetáveis (downloader como `Func`/interface, como o engine faz com `SyncDownloader`).
- `OptionalModsPathTests`: `ResolveUnderRoot` aceita path normal; rejeita `../../` e absoluto (paridade com defesa do engine).
- `OptionalOpResultTests`: N ok + M falhas ⇒ `AllOk==false`, `Failed==M`, `FailedPaths` populado; 0 falhas ⇒ `AllOk`.
- `OptionalDownloadTests` (downloader fake): todos ok ⇒ resultado ok; um lança ⇒ conta falha, continua os demais, não relança; skip por hash igual não conta como falha nem como download.
- (se extrair) `RequestHandler` monta URL com `RemoteEndPoint` (esquema+porta preservados), nunca `http://host` porta 80.

### Integração / manual (gates humanos §Gates da 01-spec)
G-1..G-5 — download real, efeito in-game, falha visível, coop, conteúdo do server. **Nunca rodar o exe pelo agente.**

### Build
`dotnet build SPT.Launcher.csproj -c Release` · `dotnet test SPT.Launcher.Tests.csproj -c Release` · (se tocar server) `dotnet build TarkovRedLine.Server.csproj -c Release` — verdes.

## Nota de paralelismo (arquivos compartilhados com outros itens)

- **`ProfileViewModel.cs` — hub dos itens 019–023.** É o arquivo mais disputado do lote. Este item toca **só** `OnOptionalToggled` (`:248-296`) e a criação de toggles (`:616-627`); evitar mexer no fluxo de sync/update (`:590-680`) que outros itens (007/017 já mergeados; 019/022/023 em voo) também tocam. Coordenar merge / manter diffs cirúrgicos.
- **`OptionalModsHelper.cs` — compartilhado com 019 e 021.** Se 019 também refatora este helper, alinhar a assinatura do resultado (`OptionalOpResult`) e a remoção de `GetServerBaseUrl` para não conflitar.
- **`RequestHandler.cs` — base compartilhada.** Adicionar wrappers é aditivo (baixo risco de conflito), mas vários itens tocam esse arquivo; agrupar as adições.
- **`SyncPathUtil.cs`/`SyncEngine.cs` — motor de sync (007/016/017, mergeados).** Só **reusar** primitivas; se expor `ResolveUnderRoot` público, é mudança aditiva — não alterar comportamento existente do engine.
- **`ModUpdateViewModel.cs` — itens 024/025 (Legacy/DS) e o fluxo de update.** Só relevante se a consolidação de `DeleteToRecycleBin` (R-4) tocar aqui; se o risco de conflito for alto, adiar a consolidação e manter cópia local.
- **Server `ModUpdater.cs` / `config.json` / `Opcionais/` — gate humano (D-021.B, G-5).** Fora do código do launcher; sincronizar com quem opera o server de produção.
