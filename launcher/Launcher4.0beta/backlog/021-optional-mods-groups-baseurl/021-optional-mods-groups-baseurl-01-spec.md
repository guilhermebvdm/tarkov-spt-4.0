# 021 — Mods opcionais: grupos faltantes + base-URL + descrição + I/O · Spec funcional

> **Data:** 2026-07-04<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [00-kickoff](./021-optional-mods-groups-baseurl-00-kickoff.md) · [AUDIT-2026-07-04](../../AUDIT-2026-07-04-code-product-ds.md)<br>

---

## Objetivo

Fechar o aceite parcial do item **009** (mods opcionais na tela Profile) corrigindo três defeitos reais e uma lacuna de conteúdo:

1. **Download que falha em silêncio** — `GetServerBaseUrl` derruba a porta e força `http`, então cada arquivo bate em `http://host:80/...`, a exceção é engolida como Warning, e o toggle aparece "ativado" mas **nada é baixado**. É um **gap de coop** (Fika PVE): os clientes divergem em assets sem nenhum erro visível; solo=host mascara.
2. **I/O + MD5 síncronos na UI thread** — download, gravação e hash bloqueiam a UI em grupos grandes.
3. **Escrita/exclusão fora do motor de sync** — `Path.Combine(GamePath, file.path)` sem guard de raiz, `File.WriteAllBytes` não-atômico, `File.Delete` permanente (não vai pra lixeira).
4. **Grupos e descrições incompletos** — o card do 009 exige **4 toggles** (Visceral/gore, Hollywood, PiP Disable, IRL) e **descrição em todos**; hoje o server só expõe `gore`/`grass`/`hollywood` e a descrição nova só alcança `hollywood`. Esta parte é **conteúdo/config do server** (gate humano), não código do launcher.

Escopo de **código** deste item = **launcher** (itens 1–3). O item 4 é **gate humano de conteúdo** documentado aqui porque o aceite do card depende dele.

## Contexto de defeito (por que falha hoje)

- O resto do launcher baixa mods por `RequestHandler`, que usa `request.RemoteEndPoint` (= `Server.Url`, ex. `https://host:6969`, esquema+porta preservados) e a via `WebRequest`, que honra o bypass de TLS self-signed global (`ServicePointManager.ServerCertificateValidationCallback`).
- `OptionalModsHelper` **reinventa** a base-URL de forma errada (`http://{host}`, porta 80) e usa `HttpClient` cru — que **não** honra o `ServicePointManager` (logo, mesmo se a URL fosse `https`, o TLS self-signed quebraria). Por isso o caminho correto é **reusar o `RequestHandler`**, não "consertar a string".

## Critérios de aceite (Given/When/Then testáveis)

### A. Base-URL correta / download que funciona
- [ ] **CA-021.1** — **Dado** um server em `Server.Url = https://host:6969`, **quando** o usuário ativa um grupo opcional, **então** cada arquivo é baixado do **mesmo endpoint base** que o sync usa (esquema + porta preservados — `https://host:6969/launcher/mods/download?file=...`), nunca `http://host` porta 80.
- [ ] **CA-021.2** — **Dado** que o server usa certificado self-signed (aceito pelo restante do launcher), **quando** o grupo é baixado, **então** o download **conclui** (mesma via `WebRequest`/`RequestHandler` que já funciona para o manifesto), sem erro de TLS.
- [ ] **CA-021.3** — Não sobra nenhuma outra construção de URL própria em `OptionalModsHelper`: a função `GetServerBaseUrl` é **removida** (ou substituída por `RequestHandler.GetBackendUrl()`), incluindo o caminho de `offFolders` (`optionals-manifest`/`optional-download`).

### B. Falha visível (nunca silenciosa)
- [ ] **CA-021.4** — **Dado** que um ou mais arquivos do grupo falham no download/gravação, **quando** a operação termina, **então** a UI mostra um **estado de erro explícito** (texto de status de erro, contando quantos arquivos falharam) — não o `update_up_to_date` verde.
- [ ] **CA-021.5** — **Dado** um **enable** em que **todos** os arquivos falharam, **quando** a operação termina, **então** o toggle **não** permanece afirmando "ativado" com sucesso: o estado persistido (`IsOptionalEnabled`) reflete a falha (revertido para `false`) **ou** a UI sinaliza inequivocamente "não aplicado" (decisão D-021.A abaixo).
- [ ] **CA-021.6** — Toda falha continua sendo **logada** (como hoje), mas o log deixa de ser o **único** canal — a UI também informa.

### C. Off-thread (UI fluida)
- [ ] **CA-021.7** — **Dado** um grupo com muitos arquivos grandes, **quando** ativo/desativo, **então** a UI (janela, botões, barra) permanece responsiva: download, escrita em disco e cálculo de MD5 rodam **fora da UI thread**; só progresso/status são marshalados para a UI (via `Dispatcher.UIThread`).

### D. Robustez de escrita/exclusão (paridade com o motor)
- [ ] **CA-021.8** — **Dado** um `file.path` malicioso/anômalo (`../../..`, caminho absoluto), **quando** o grupo é baixado ou removido, **então** o alvo é **contido sob o GameRoot** (mesma defesa `ResolveUnderRoot` do engine); path que escapa é rejeitado e logado, não escrito/apagado.
- [ ] **CA-021.9** — **Dado** o desativar de um grupo sem `offFolders`, **quando** os arquivos são removidos, **então** vão para a **lixeira** (paridade com `DeleteToRecycleBin`), não `File.Delete` permanente.
- [ ] **CA-021.10** — **Dado** o baixar de um arquivo, **quando** é gravado, **então** a escrita é **atômica** (`.sync-tmp` + move), sem deixar arquivo parcial se cair a conexão no meio.

### E. Conteúdo do server (gate humano — ver §Gates)
- [ ] **CA-021.11** — Os **4 grupos** do card (Visceral/gore, Hollywood, PiP Disable, IRL) aparecem como toggle na tela Profile, cada um baixando arquivos reais.
- [ ] **CA-021.12** — **Todos** os 4 grupos exibem descrição vinda da **fonte nova** (`description.json` por pasta, via `optionals-list`), não só o fallback legado do `config.json`.

## Regras de negócio

- **RN-1 — Fonte única de base-URL.** Qualquer chamada de rede do fluxo de opcionais usa o backend já resolvido no connect (`RequestHandler`/`RemoteEndPoint`). O launcher **não** deriva host/porta/esquema por conta própria.
- **RN-2 — Sucesso é all-or-reported.** "Ativado com sucesso" só quando todos os arquivos do grupo baixaram e gravaram. Sucesso parcial ou total-falha → estado de erro visível.
- **RN-3 — Idempotência preservada.** O skip por hash local (arquivo já presente com MD5 igual ⇒ não rebaixa) continua valendo; não conta como falha.
- **RN-4 — Contenção sob GameRoot.** Nenhuma escrita/exclusão de opcional pode tocar fora do GameRoot, independente do que o server mandar no manifesto.
- **RN-5 — Descrição: nova tem precedência, legado é fallback.** Mantém a regra do 009 — descrição do `optionals-list` (por idioma, PT/EN) sobrepõe; sem descriptor, cai no `optionalGroups[].description` do `config.json`.

## Corner cases

- **CC-1 — Server offline / timeout no meio do grupo:** operação termina como **falha visível** (CA-021.4), não trava nem finge sucesso.
- **CC-2 — Grupo com `offFolders` (ex.: `grass`):** desativar baixa os arquivos "Off" (aplica config de desativação) em vez de deletar — esse caminho **também** passa a usar `RequestHandler` (não `GetServerBaseUrl`).
- **CC-3 — Toggle disparado durante update em andamento:** o semáforo (`_optionalToggleSemaphore`) já serializa; manter — não introduzir corrida ao trocar para off-thread.
- **CC-4 — `optionals-list` no shape antigo (array de nomes):** o parser tolerante do 009 continua válido (descriptor só com `Id/Name`, sem descrição) — sem descrição nova, cai no fallback (RN-5). **Não** é falha.
- **CC-5 — Cache de grupos vazio** (`_cachedGroupFiles` sem o grupo): já loga Warning e retorna; com B (falha visível) isso deve **também** virar estado de erro na UI, não sucesso silencioso.
- **CC-6 — Coop (Fika PVE):** com CA-021.1/2, todos os clientes passam a baixar de fato os mesmos assets; sem o fix, divergência silenciosa entre host e clientes. Registrar no teste in-game (gate).

## Fora de escopo

- Reescrever o fluxo de opcionais **inteiro** dentro do `SyncEngine` (planner + baseline + actions). Aqui reusamos apenas as **primitivas** (contenção de path, escrita atômica, lixeira); a migração completa para actions do engine fica para item futuro.
- Lógica de exclusão mútua **PiP × Dynamic External Resolution** (é só texto na descrição hoje; deferido **P-009.1**, exige teste in-game).
- Unificar as duas implementações de server (C# `TarkovRedLine.Server` × TS `TarkovRedLine-ServerMod`) — ver decisão de produto D-021.B.
- UI nova / redesenho da lista de opcionais.

## Decisões de produto (precisam do humano)

- **D-021.A — Comportamento do toggle em falha total.** Preferência da spec: **reverter** `IsOptionalEnabled → false` e mostrar erro (o usuário vê que não aplicou e pode retentar). Alternativa: manter marcado + banner "pendente/erro". Escolher A antes do código.
- **D-021.B — Qual server é a fonte de verdade?** Há **duas** implementações divergentes:
  - C# `TarkovRedLine.Server/Controllers/ModUpdater.cs` — `optionals-list` já devolve o shape **novo** (`{id,name,description:{pt,en}}` lendo `description.json`), **porém** o gerador de manifesto **não** taggeia arquivos como `optional`/`optionalGroup` (⇒ `_cachedGroupFiles` vazio ⇒ download de grupo não baixa nada).
  - TS `TarkovRedLine-ServerMod/src/modUpdater.ts` — **taggeia** os opcionais corretamente (scan de `Opcionais/<folders>`), **mas** `optionals-list` devolve o shape **antigo** (só nomes de pasta, sem descrição).
  - Ou seja: **nenhum dos dois** entrega, sozinho, "4 grupos com download real + descrição nova". O humano decide qual server é o de produção e completa a metade que falta nele. O launcher (este item) fica **robusto a ambos**.

## Gates

### Gates de build (agente)
`dotnet build SPT.Launcher.csproj -c Release` · `dotnet test SPT.Launcher.Tests.csproj -c Release` — verdes. **Nunca rodar o exe.**

### Gates humanos (validação in-game / produção — obrigatórios)
Regra do projeto: **escrita em arquivos SPT precisa de validação no jogo, não só build/hash.**

- [ ] **G-1 — Download real de cada grupo.** Com o server de produção rodando, ativar cada um dos 4 grupos no launcher e **confirmar em disco** que os arquivos chegaram ao GamePath (paths + contagem batem com o manifesto).
- [ ] **G-2 — Efeito in-game.** Entrar em raid e confirmar que o efeito do grupo aparece (ex.: gore/Visceral, HollywoodFX, PiP desligado, IRL) — build verde não prova asset carregado.
- [ ] **G-3 — Falha visível.** Forçar um cenário de falha (server parado / arquivo removido do repo) e confirmar que a UI mostra erro e o toggle **não** finge sucesso.
- [ ] **G-4 — Coop (Fika PVE).** Com host + ao menos 1 cliente extra, ativar um grupo e confirmar que **os dois** clientes baixaram os assets (sem divergência silenciosa). Solo não cobre este gate.
- [ ] **G-5 — Conteúdo do server (D-021.B).** Adicionar/alinhar `optionalGroups` (PiPDisable, IRL) + pastas `Opcionais/` + alinhar nomes de pasta ↔ `id` para que a descrição nova alcance todos; regenerar manifesto. Inspeção humana do `config.json` de produção e do `Opcionais/`.
