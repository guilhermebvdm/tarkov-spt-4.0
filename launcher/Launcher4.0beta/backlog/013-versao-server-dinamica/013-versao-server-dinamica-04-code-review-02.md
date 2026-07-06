# 013 — Versão do server dinâmica (013L) · Code Review 02 (adversarial)

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Commit revisado:** `50cfce1` · **Insumos:** [02-spec-tech](./013-versao-server-dinamica-02-spec-tech.md) · [05-asbuild](./013-versao-server-dinamica-05-asbuild.md) · [review-01 (server)](./013-versao-server-dinamica-04-code-review-01.md)

> Review de contexto limpo (revisor não escreveu o código). Escopo: metade launcher (013L) — `ServerManager.cs`, footers `LoginView`/`RegisterView`, `ProfileViewModel.ServerVersion`. Build gate: `dotnet build SPT.Launcher.csproj` → **0 erros** (warnings pré-existentes). Nota: a working tree contém WIP do item 007 em `ProfileViewModel.cs`/`ProfileView.axaml` — a leitura de código foi feita no estado exato do commit via `git show 50cfce1:`.

**Placar:** 0 🔴 · 2 🟡 · 2 🟢

---

## Suspeita declarada: race do `x:Static` (read-once) — NÃO confirmada

Trace completo do fluxo real:

1. `ConnectServerViewModel.ConnectServer()` (`ConnectServerViewModel.cs:129/137`) → `await ServerManager.LoadDefaultServerAsync(url)`.
2. `LoadDefaultServerAsync` → `Task.Run(() => LoadServer(server))` (`ServerManager.cs:144-147`).
3. `LoadServer` (`ServerManager.cs:123-142`): `RequestConnect()` OK → **`LoadTrlServerVersion()` roda sincronamente na linha 139, antes do `return true`**. Não é fire-and-forget — é chamada bloqueante dentro da mesma Task que o `ConnectServer` está aguardando.
4. Só depois de `LoadServer` retornar vêm `PingServer()`, `GetVersion()` e `NavigateTo(new LoginViewModel(...))` (`ConnectServerViewModel.cs:171`).

Portanto **não existe janela** em que LoginView/RegisterView sejam construídas antes do fetch terminar (com sucesso ou falha). Caminho de RETRY (`RetryCommand` → `ConnectServer` de novo) segue a mesma ordem; reconexão via `NoConnection → ConnectServerViewModel` idem (o `WhenActivated` chama `ConnectServer`). `RegisterView` só nasce por navegação a partir da LoginView (pós-connect). `ProfileViewModel` lê `ServerManager.TrlServerVersion` no field-initializer (`ProfileViewModel.cs:67`), i.e., na construção — que só ocorre pós-login, portanto pós-connect. Todos os consumidores nascem com o valor já resolvido.

O risco residual do read-once não é race, é **staleness** — coberto no CR-02-01.

---

## Achados

### CR-02-01 [🟡] Fetch único sem retry — falha transitória congela "—" pela sessão inteira (e o design read-once torna refetch inócuo)

`ServerManager.cs:31-47` + `ServerManager.cs:139`. O fetch acontece exatamente uma vez por connect. Cenário: no momento do connect o `/redline/server/version` falha transitoriamente (exceção de IO no server → o próprio endpoint tem fallback, mas timeout/reset de conexão no cliente ainda é possível logo após reconexão Tailscale — exatamente o cenário que motivou o retry de 2 tentativas do `LoadServer` no `ConnectServerViewModel.cs:132-138`; note que esse retry só reexecuta o fetch se o **connect** falhou junto). Resultado: footers e ProfileView mostram "—" até o usuário forçar uma reconexão. Agravante de design: como `TrlServerVersion` é setter silencioso (propriedade estática sem notificação) e os footers são `x:Static` read-once, qualquer refetch futuro só teria efeito para views construídas depois dele. **Fix mínimo:** 1 retry curto dentro de `LoadTrlServerVersion()` (mesmo padrão do connect); **fix melhor:** refetch lazy — se `TrlServerVersion == "—"` quando `LoginViewModel`/`ProfileViewModel` são construídos, tentar de novo (síncrono e barato; a view lê o static depois do ctor do VM).

### CR-02-02 [🟡] Defaults "15.0"/"0.10" do `TrlVersionFooter` são runtime, não design-time — ClassSelectionView exibe versões fabricadas

Asbuild afirma: "Defaults do `TrlVersionFooter` ('15.0'/'0.10') mantidos no controle: são só fallback de design-time; todos os usos reais agora passam valores". Incorreto duas vezes: (1) default de `StyledProperty` (`TrlVersionFooter.axaml.cs:24/34`) **aplica em runtime** quando o uso não seta a propriedade; (2) existe uso real sem valores — `ClassSelectionView.axaml:72` (no commit) instancia `<cc:TrlVersionFooter .../>` pelado. Cenário: usuário chega à tela de seleção de classe e lê "Versão do launcher: 15.0 | Versão do servidor: 0.10" — dado falso porém plausível, pior que "—". Pré-existente do item 015 e a spec delega a ClassSelectionView ao 004L, mas o registro do asbuild mascara o gap. **Fix:** trocar os defaults do controle para `"—"` (1 linha cada, seguro para todos os usos) e deixar o 004L ligar os valores reais; corrigir a frase do asbuild.

### CR-02-03 [🟢 menor] Rastreabilidade: `Request.cs`/`RequestHandler.cs` listados no asbuild não estão neste commit

O asbuild do 013L lista `Request.GetJson(decompressResponse)` e `RequestHandler.RequestTrlServerVersion()` como arquivos alterados, mas `50cfce1` não os contém — eles entraram no commit do item 004L (`8ef2265`, sessões paralelas tocando os mesmos arquivos base). Conteúdo verificado presente e correto no HEAD (`Request.cs:90-107`, `RequestHandler.cs:100-106`); é só higiene de commit/registro. Sem ação de código.

### CR-02-04 [🟢 menor] `Request.Send()` retorna null em erro de rede → NRE dentro do `GetJson` como controle de fluxo

`Request.cs:68-88`: falha de rede é engolida e `Send` retorna `null`; `GetJson` então lança NRE em `stream.CopyTo` — que o try/catch de `LoadTrlServerVersion` captura, mantendo "—". Funciona, mas o caminho de erro real é uma NullReferenceException acidental, padrão pré-existente da classe (todos os outros callers convivem com ele). Sem ação neste item; registrado para eventual limpeza da classe `Request`.

---

## Áreas auditadas, sem achados

- **Contrato do endpoint:** `ServerVersionController` retorna `{ "version": ... }`; DTO privado `TrlServerVersionResponse.version` casa (Newtonsoft é case-insensitive de todo modo). `decompressResponse: false` correto — endpoint ASP.NET responde JSON puro, espelho do `RequestAccount` (`/redline/profile/get`).
- **Fallback nunca quebra:** `IsNullOrWhiteSpace(data?.version)` cobre `null`/`""`/whitespace; try/catch total; "—" é o único estado de falha. Footer não lança em nenhum cenário.
- **xmlns/assembly nos footers:** `clr-namespace:SPT.Launcher;assembly=SPT.Launcher.Base` correto (namespace real do `ServerManager`); `x:Static` com propriedade estática de getter público compila e resolve (build verde).
- **Remoção do read de `config.json` do ServerMod:** bloco inteiro removido de `InitializeAsync` (inclusive o `LogManager.Warning` associado); nenhuma outra referência restante ao `serverVersion` local no launcher; fonte única agora é o endpoint.
- **`ProfileViewModel.ServerVersion`:** setter reativo mantido (a UI atualizaria se algo setasse), default do field-initializer resolvido pós-connect. ProfileView (`Grid` VERSÃO) bound corretamente.

---

## Resoluções (2026-07-04, /apply-code-review)

| CR | Resolução |
|---|---|
| CR-02-01 🟡 | **Aplicado (parcial, decisão registrada)** — refetch lazy na ProfileView: `ServerManager.RefreshTrlServerVersionIfUnknown()` (público, no-op quando resolvida) + no ctor do `ProfileViewModel`, se `"—"`, `Task.Run` refaz o fetch e posta em `ServerVersion` via `Dispatcher` (propriedade reativa → a UI atualiza). `// ref: CR-02-01` nos dois pontos. **Aceito sem fix:** footers de Login/Register continuam "—" na sessão degradada — são `x:Static` read-once e um refetch síncrono no ctor do VM rodaria na UI thread com até 15s de timeout (o freeze seria pior que o "—"); a tela onde a versão importa de verdade (ProfileView) agora se recupera sozinha. |
| CR-02-02 🟡 | ✅ **Resolvido pelo orquestrador** — commit `4f1ad30` ligou o footer da `ClassSelectionView` (valores reais em vez dos defaults "15.0"/"0.10"). Frase incorreta do asbuild ("defaults são só design-time") corrigida lá. |
| CR-02-03 🟢 | Anotado no asbuild: `Request.cs`/`RequestHandler.cs` entraram via commit `8ef2265` (004L, sessões paralelas nos mesmos arquivos base) — conteúdo correto no HEAD, só higiene de registro. |
| CR-02-04 🟢 | Não endereçado (padrão pré-existente da classe `Request`; registrado para limpeza futura). |

Gates: build launcher **0 erros** · `dotnet test` **52/52**.
