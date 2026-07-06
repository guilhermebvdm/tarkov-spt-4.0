# 018 — Segurança do auto-update (assinatura + cert pinning) · Spec técnica

> **Data:** 2026-07-04<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [00-kickoff](./018-auto-update-security-00-kickoff.md) · [01-spec](./018-auto-update-security-01-spec.md) · [AUDIT-2026-07-04](../../AUDIT-2026-07-04-code-product-ds.md) §B1<br>

---

## Abordagem (3 camadas)

1. **Verificação de assinatura do exe (primária — para o RCE).** Chave pública embutida no launcher. No release, assina-se o conteúdo do `Tarkov Red Line.exe` com a privada → `.sig` detached publicado no server. O launcher baixa o exe para **quarentena**, verifica `.sig` contra a chave pública (`RSA.VerifyData`/`ECDsa.VerifyData` sobre os bytes) e só então promove. Falha → fail-closed (01-spec RN-1).
2. **Cert pinning (secundária — defesa em profundidade).** Substituir `ServerCertificateCustomValidationCallback => true` por validação de **pin SPKI SHA-256** com lista embutida (2 pins p/ rotação). Não dá pra ligar validação de CA normal porque o cert é self-signed.
3. **Orquestração fail-closed.** Reordenar `DownloadAndPatchAsync` para: baixar→verificar→(só se OK)→escrever `.bat`→`Process.Start`→`Environment.Exit`. Hoje o download já vai direto pro exe final e o `.bat` roda sem checagem.

## Estado atual (file:line reais, confirmados por leitura)

`SPT.Launcher/Helpers/LauncherUpdateHelper.cs`:
- `:13-14` — `CurrentVersion` lê `AssemblyVersion` (item 014, manter single-source; fallback `"2.0.0"`).
- `:16` — `CheckAndUpdateAsync(string serverUrl, IProgress<int>)`.
- `:20-23` e `:74-77` — **os dois** `HttpClientHandler` com `ServerCertificateCustomValidationCallback => true` (o bug de TLS).
- `:30` `/redline/launcher/version`; `:37-38` parse `version` via `JsonDocument`.
- `:40-45` dispara `DownloadAndPatchAsync` se `IsNewerVersion`.
- `:56-63` `IsNewerVersion` (usa `Version.TryParse`; hoje não limpa sufixo `+hash`).
- `:65-147` `DownloadAndPatchAsync`: `:71` monta `updateExe` = **arquivo final** `SPT.Launcher_Update.exe`; `:86` `FileStream` escreve **direto no final**; `:118` fecha; `:124-133` monta+grava o `.bat`; `:135-143` `Process.Start`; `:146` `Environment.Exit(0)`.

`SPT.Launcher/ViewModels/ConnectServerViewModel.cs`:
- `:54` Gist que semeia `Server.Url`; `:64` fallback `https://100.106.152.7:6969`.
- `:102-120` gate `DisableUpdates` + `Progress<int>` + chamada a `CheckAndUpdateAsync` (`:114`) + `return` se `isUpdating` (`:115-119`).

Server `TarkovRedLine.Server/Controllers/LauncherUpdaterController.cs`:
- `:12-31` `GetUpdaterBasePath()` (internal static, já reusado por `ServerVersionController`); `:33-36` `GetLauncherExePath()` = `Launcher-Updater/Tarkov Red Line.exe`.
- `:38-67` `GET version` (lê `FileVersionInfo`, limpa `+`); `:69-79` `GET download` (`PhysicalFile`).

Contexto de testabilidade: `SPT.Launcher.Tests.csproj` referencia **apenas `SPT.Launcher.Base`** (não `SPT.Launcher`). `LauncherUpdateHelper` vive em `SPT.Launcher` (app) → **a lógica pura de verificação precisa morar em `SPT.Launcher.Base`** para ser testável em xUnit, espelhando o motor de sync (`SPT.Launcher.Base/Sync/`). `HwidHelper.cs:37` já usa `System.Security.Cryptography.SHA256` — o namespace está disponível no Base.

## Arquivos a tocar

| Arquivo | Mudança |
|---|---|
| `SPT.Launcher.Base/Security/UpdateSignatureVerifier.cs` **(novo)** | Puro/testável: `bool Verify(byte[] payload, byte[] signature, string publicKeyPem, string algorithm)` — `RSA.ImportFromPem` + `VerifyData(SHA256, Pkcs1)` (ou `ECDsa`). Sem I/O. |
| `SPT.Launcher.Base/Security/UpdateTrust.cs` **(novo)** | Constantes embutidas: `EmbeddedPublicKeyPem`, `SpkiPins` (`string[]` de SHA-256 base64), `SignatureAlgorithm`. Fonte da confiança do cliente (01-spec RN-2). |
| `SPT.Launcher.Base/Security/CertificatePinning.cs` **(novo)** | `bool IsPinned(X509Certificate2 cert)` — computa SPKI SHA-256 de `cert.PublicKey` e compara com `UpdateTrust.SpkiPins`. Testável (cert fixture). |
| `SPT.Launcher.Base/Helpers/UpdateVersion.cs` **(novo, opcional)** | Extrai `IsNewerVersion` p/ o Base + limpa sufixo `+hash` (CA-8). Testável. |
| `SPT.Launcher/Helpers/LauncherUpdateHelper.cs` | Remover os dois callbacks `=> true` (`:22,76`) → `handler.ServerCertificateCustomValidationCallback = (s,c,ch,e) => CertificatePinning.IsPinned(c)`. Buscar `signature`/`algorithm` no JSON de version. Reescrever `DownloadAndPatchAsync`: baixar p/ `*.tmp` (quarentena), recomputar SHA-256, `UpdateSignatureVerifier.Verify` → só então `File.Move` p/ o exe final, escrever `.bat`, `Process.Start`, `Exit`. Falha → apagar `.tmp`, logar, **retornar sem promover**. |
| `SPT.Launcher/ViewModels/ConnectServerViewModel.cs` | Surface do erro: quando a verificação falha, exibir aviso não-bloqueante via `connectModel.InfoText` e **seguir** o login (não `return`). Requer `CheckAndUpdateAsync` sinalizar "falhou-verificação" vs "up-to-date" (ver Contratos). |
| `TarkovRedLine.Server/Controllers/LauncherUpdaterController.cs` | `GET version` passa a incluir `signature` (base64 de `Launcher-Updater/Tarkov Red Line.exe.sig`) + `algorithm`; ausência do `.sig` → campo `null`/omitido (cliente fail-closed). Reusar `GetUpdaterBasePath()`. Opcional: `GET /redline/launcher/signature` servindo o `.sig` cru. |
| `SPT.Launcher.Tests/Security/*` **(novo)** | Ver Plano de teste. |

## Contratos / DTOs

**`GET /redline/launcher/version`** (estendido — o cliente já faz `JsonDocument.Parse` em `:37`):
```json
{
  "version": "2.1.0",
  "signature": "<base64 da assinatura detached sobre os bytes do exe>",
  "algorithm": "RSA-SHA256"
}
```
- `signature` ausente/`null` → cliente aborta o update (01-spec CA-3). `algorithm` ∈ {`RSA-SHA256`, `ECDSA-SHA256`} conforme D-018.1.

**`GET /redline/launcher/download`** — inalterado (`PhysicalFile` do exe).

**Assinatura (autoridade = hash recomputado):** o cliente **ignora** qualquer hash anunciado; recomputa SHA-256 dos bytes baixados e chama `VerifyData(bytesDoExe, signature, SHA256, Pkcs1)`. Verdadeiro ⇒ os bytes são exatamente os assinados pela chave privada (01-spec CA-6). Não há caminho onde bytes não-verificados sejam promovidos/executados.

**`CheckAndUpdateAsync` — retorno.** Hoje `Task<bool>` (true = vai reiniciar). Trocar por um resultado de 3 estados p/ a UI distinguir falha de verificação de "sem update":
```csharp
enum UpdateOutcome { UpToDate, Restarting, VerificationFailed, NetworkError }
```
`ConnectServerViewModel.cs:114-119` passa a: `Restarting` → mostra "Reiniciando" e `return`; `VerificationFailed` → aviso não-bloqueante + segue login; `UpToDate`/`NetworkError` → segue login silencioso.

**Chave pública embutida.** PEM em `UpdateTrust.EmbeddedPublicKeyPem` (source no repo — é pública). A **privada** nunca entra no repo nem em path sincronizado via Syncthing (D-018.2).

**Contrato de operação (release).** Ao publicar: assinar o exe com a privada → gerar `Launcher-Updater/Tarkov Red Line.exe.sig` ao lado do exe → o server passa a servir version(+signature)+download coerentes. Espelha o padrão de `ServerVersionController` lendo `server-version.txt` do `Launcher-Updater`.

## Riscos

- **R-1 — Brick por verificação sobre binário errado (TOCTOU).** Se verificar um stream e promover outro arquivo, abre janela. Mitig.: verificar o **arquivo em disco** (`.tmp`) e `File.Move` **o mesmo** arquivo; nunca rebaixar.
- **R-2 — Cliente sem chave pública / pin errado** trava auto-update silenciosamente (01-spec corner case). Mitig.: Gate G-4/G-5 + log claro `[AutoUpdate]`.
- **R-3 — Rotação de cert quebra clientes** (pin fixo). Mitig.: `SpkiPins` com 2 entradas; publicar novo pin num release antes de rotacionar (D-018.4).
- **R-4 — `Version.TryParse` com `+hash`** (o server já limpa `+` em `version`, mas o cliente não). Mitig.: `UpdateVersion` limpa sufixo antes do parse (CA-8).
- **R-5 — Falso senso com só pinning.** Pinning sozinho não impede RCE se a chave privada de TLS vazar; a assinatura é a barreira real. Ordem de prioridade: assinatura > pinning.
- **R-6 — Coop silencioso** (01-spec: `.sig` ausente trava todos). Mitig.: Gate G-3 em cliente coop real.
- **R-7 — Quebrar o item 014** (single-source de versão). Não regressar `CurrentVersion` (`:13-14`) ao extrair `UpdateVersion`.

## Plano de teste

**Unit (`SPT.Launcher.Tests/Security/`, xUnit — mesmo estilo de `Sync/SyncSeedTests.cs`):**
- `UpdateSignatureVerifierTests` — par de chaves de teste gerado em memória (`RSA.Create()`): assinatura válida sobre payload ⇒ `true`; payload com 1 byte trocado ⇒ `false`; assinatura de **outra** chave ⇒ `false`; signature vazia/`null`/base64 inválido ⇒ `false` (sem throw).
- `CertificatePinningTests` — cert self-signed gerado (`CertificateRequest`): SPKI na lista ⇒ `true`; fora da lista ⇒ `false`; cert `null` ⇒ `false`.
- `UpdateVersionTests` — `2.1.0 > 2.0.0` ⇒ novo; iguais ⇒ não; `"2.1.0+abc"` limpa e compara; malformado ⇒ não-novo, sem exceção (CA-8).

**Integração leve (sem rede):** helper de verificação recebendo um arquivo `.tmp` + `.sig` de fixture → verifica que a promoção só ocorre no caminho válido e que o `.tmp` é apagado no inválido (a parte de I/O que puder ser injetada; `Process.Start`/`Exit` ficam fora do unit).

**Gates (01-spec G-1..G-5):** builds verdes + validação in-game em segundo cliente (feliz + adulterado + coop) + inspeção do `Launcher-Updater` de produção. Nunca rodar o exe pelos gates automáticos.

## Nota de paralelismo (arquivos compartilhados com outros itens)

- **`ConnectServerViewModel.cs` — hub compartilhado.** Também tocado por **006** (Tailscale/Dev Mode bypass, `:72-100`, `:85-89`) e citado por **013** (footers de versão). Este item mexe só no bloco de update (`:102-120`) + surface de erro. Colisão baixa mas **mesmo arquivo** → sequenciar ou merge cuidadoso; não tocar o branch do Tailscale (006) nem a lógica do Gist (deixado p/ D-018.3).
- **`LauncherUpdateHelper.cs` — majoritariamente deste item.** Item **014** já fez `CurrentVersion` single-source (`:13-14`) — não regressar (R-7).
- **`LauncherUpdaterController.cs` (server) — deste item.** `GetUpdaterBasePath()` é reusado por `ServerVersionController` (item 013) — **reusar**, não duplicar. `ModUpdater.cs` tem uma cópia própria de `GetUpdaterBasePath` (não unificar aqui p/ não ampliar escopo).
- **Novos arquivos em `SPT.Launcher.Base/Security/` e `SPT.Launcher.Tests/Security/`** — sem colisão com outros itens.
- Fora deste hub: **019-023** giram em torno de `ProfileViewModel.cs`; **019/021** em `OptionalModsHelper.cs`; **024/025** em `Legacy.axaml`. Este item não toca nenhum deles.

## Gates

`dotnet build SPT.Launcher.csproj -c Release` · `dotnet test SPT.Launcher.Tests.csproj -c Release` · `dotnet build TarkovRedLine.Server.csproj -c Release` — verdes. Validação in-game obrigatória (01-spec G-2/G-3/G-4). Nunca rodar o exe pelos gates.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-04 | Guilherme | Criação — spec técnica do item 018 (verifier em Base p/ testabilidade, pinning SPKI, orquestração fail-closed, contratos de endpoint). |
