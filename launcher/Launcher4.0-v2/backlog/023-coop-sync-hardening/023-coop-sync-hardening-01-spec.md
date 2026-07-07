# 023 — Coop-sync hardening (Fika PVE) · Spec funcional

> **Data:** 2026-07-04<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [00-kickoff](./023-coop-sync-hardening-00-kickoff.md) · [AUDIT-2026-07-04](../../AUDIT-2026-07-04-code-product-ds.md)<br>

---

## Objetivo

Fechar os três gaps de coop apontados na auditoria para que o **launcher nunca quebre uma sessão Fika Coop PVE em silêncio**. O runtime é multiplayer com host + clientes; solo=host mascara esses bugs (o host quase nunca os sente sozinho). São três frentes independentes que compartilham o mesmo tema — "a operação de um jogador não pode sabotar a raid dos outros":

- **A — Quarentena de plugin client-only (Fika).** O motor de sync não pode mover `Fika.Core.dll` (e a família `Fika.*`) para `plugins-disabled` quando o arquivo está no disco do cliente mas **fora do manifesto** do server.
- **B — Operação destrutiva de conta durante sessão coop.** Excluir conta / wipe / mudar edição não pode rodar às cegas quando outros jogadores podem estar em raid — o gate atual só enxerga o estado **local** do host.
- **C — Auth headless dos clientes extras.** A entrada silenciosa (authkey, `--unattended`) na tailnet precisa funcionar para **N** clientes, não só o primeiro; e a falha precisa dizer *por que* falhou.

Fora de escopo os bloqueadores B1/B2/B3 da auditoria (RCE do auto-update, guard de `deleteFiles`, migração DS do `SettingsView`) — são outros itens.

## Contexto de código (âncoras reais)

- Fallback de regras marca `plugins`/`patchers` (e variantes `BepInEx/*`) como `mirror-move-disabled`: `SyncRuleResolver.cs:30-35`.
- `ScanExtras` move todo arquivo local sob esses prefixos **ausente do manifesto** para `<prefixo>-disabled/`: `SyncPlanner.cs:229-262` (`MoveToDisabled` + `BuildDisabledTarget`); skips existentes em `SyncPlanner.cs:222-227`.
- Manifesto do server = varredura recursiva de `mods_repo` (`ModUpdater.cs:319-329`); default `ignoredFiles = {"BepInEx/plugins/spt","user/mods/spt"}`, `folderRules["BepInEx/plugins"]="mirror-move-disabled"` (`ModUpdater.cs:364-377`). Se `Fika.Core.dll` **não** está em `mods_repo`, não entra no manifesto → vira "extra" → quarentena.
- Botões CONTA gated só por `CanStartGame` (estado local): `ProfileView.axaml:147-163`; `CanStartGame => LauncherSettingsProvider.Instance.CanStartGame` (`ProfileViewModel.cs:298`, = `!GameRunning && !IsUpdating`). `GameRunning` reflete **só o EFT deste launcher**, não a sessão coop.
- `DeleteAccountCommand` remove `{id}.json` no server: `ProfileViewModel.cs:1097-1160` (`RemoveAsync` em `:1118`); Wipe em `:1082-1090`.
- Tailscale headless: `up --authkey={authKey} --unattended --reset ...` em `TailscaleHelper.cs:179`; authkey de gist público + fallback embutido em `TailscaleHelper.cs:16-17`; falha vira `false` genérico em `TailscaleHelper.cs:204-210` → mensagem única em `ConnectServerViewModel.cs:90-99`.

## Regras de negócio

- **RN-1 (allowlist coop-safe).** Existe uma lista embutida no launcher de plugins **client-only essenciais ao coop** (família Fika). Arquivo que casa a lista, sob um prefixo mirror (`plugins`/`patchers`), **nunca** é quarentenado por `mirror-move-disabled`, mesmo ausente do manifesto. A lista é defesa-em-profundidade: se o server distribui Fika no manifesto, ele já está protegido (skip de manifesto); a allowlist só age quando o Fika é instalação puramente local.
- **RN-2 (allowlist não sobrepõe o server).** Se o Fika está no manifesto, o server manda (download/update normal). A allowlist **só** preserva o que é extra; nunca reverte um update, nunca reabilita algo que o manifesto define. Não há caminho em que a allowlist force o Fika a divergir do server.
- **RN-3 (visibilidade).** Toda vez que a allowlist preserva um arquivo que teria sido quarentenado, isso vira um `Warning` no plano/relatório (`last-update.json`) — preservação silenciosa é proibida (a auditoria classificou o bug atual como "recuperável mas silencioso").
- **RN-4 (gate coop em op destrutiva).** Excluir conta, wipe e mudar edição passam a **avisar explicitamente** sobre sessão coop na confirmação. O launcher assume o pior: não há como provar, só pelo estado local, que ninguém está em raid.
- **RN-5 (pré-check de sessão, se disponível).** Se o server expõe sinal de sessão/raid ativa ou peers conectados, a op destrutiva faz um pré-check e **bloqueia (ou exige confirmação reforçada)** quando há sessão viva. Sem endpoint, cai em RN-4 (aviso + confirmação forte). — decisão de produto (ver Gates).
- **RN-6 (authkey reusável).** A entrada headless via `--unattended` exige que a authkey compartilhada seja **reusável + efêmera + pré-autorizada**. Chave single-use só admite o primeiro cliente; os demais falham. Isso é propriedade do console Tailscale, não verificável em código → gate operacional.
- **RN-7 (erro de auth distinguível).** Quando `tailscale up` falha por chave rejeitada/esgotada/expirada, o launcher mostra mensagem **específica** ("chave de acesso à rede rejeitada/esgotada — avise o host"), diferente da falha genérica de rede, para o cliente extra saber que o problema não é a internet dele.

## Critérios de aceite (Given/When/Then, testáveis)

### Frente A — Allowlist coop-safe (Fika)

- [ ] **CA-A1.** *Given* `Fika.Core.dll` em `BepInEx/plugins/` no disco e **ausente** do manifesto; *When* o planner roda; *Then* o plano **não** contém `MoveToDisabled` para `Fika.Core.dll` (nenhuma ação o toca) e há um `Warning` "coop-safe" citando o arquivo.
- [ ] **CA-A2.** *Given* qualquer `Fika.*.dll` sob `BepInEx/plugins/` ausente do manifesto; *When* planner roda; *Then* mesmo resultado de CA-A1 (a regra casa a família, não só `Fika.Core`).
- [ ] **CA-A3.** *Given* um plugin não-Fika extra (`BepInEx/plugins/OldMod/old.dll`) ausente do manifesto; *When* planner roda; *Then* continua sendo `MoveToDisabled` (a allowlist **não** afrouxa a limpeza dos demais extras — regressão do comportamento 007).
- [ ] **CA-A4.** *Given* `Fika.Core.dll` **presente no manifesto** com hash diferente do disco; *When* planner roda; *Then* o plano é `Download` (update normal) — a allowlist não impede o server de atualizar o Fika (RN-2).
- [ ] **CA-A5.** *Given* Dev Mode ligado + `Fika.Core.dll` extra; *When* planner roda; *Then* preservado (como qualquer extra em Dev Mode) — a allowlist não conflita com R5.2.
- [ ] **CA-A6 (in-game, coop real).** *Given* host + ≥1 cliente, `Fika.Core.dll` local e fora do manifesto; *When* o cliente roda a verificação de arquivos e entra numa raid; *Then* `Fika.Core.dll` continua em `plugins/` (não foi para `plugins-disabled`) e o cliente conecta na sessão do host.

### Frente B — Op destrutiva ciente de coop

- [ ] **CA-B1.** *Given* a tela de perfil; *When* o usuário aciona EXCLUIR CONTA; *Then* o diálogo de confirmação (que já pede digitar o username) exibe aviso explícito de que, se houver sessão coop ativa, a exclusão pode corromper a raid dos outros jogadores.
- [ ] **CA-B2.** *Given* a tela de perfil; *When* o usuário aciona RESETAR PROGRESSO (WIPE) ou MUDAR EDIÇÃO; *Then* a confirmação exibe o mesmo aviso de coop (ambos disparam Remove/Register server-side).
- [ ] **CA-B3.** *Given* o diálogo destrutivo aberto; *When* o usuário fecha por ESC / clique-fora / qualquer retorno que não seja confirmação explícita; *Then* **nada** é excluído/resetado (confirmação só prossegue no `true` explícito — padrão `is not bool ... || !...`, alinhado ao `DeleteAccountCommand` atual em `ProfileViewModel.cs:1104`).
- [ ] **CA-B4 (condicional a endpoint — produto).** *Given* o server expõe sinal de raid/peers ativos e há ≥1 peer em sessão; *When* o usuário confirma uma op destrutiva; *Then* o launcher bloqueia com mensagem "há jogadores em sessão agora" (ou exige confirmação reforçada), sem chamar `RemoveAsync`.

### Frente C — Auth headless dos clientes extras

- [ ] **CA-C1.** *Given* `tailscale up` retorna código ≠ 0 com stderr indicando chave rejeitada/esgotada/expirada; *When* `EnsureTailscaleConnected` propaga o resultado; *Then* a UI mostra a mensagem específica de authkey (RN-7), distinta da falha de rede genérica.
- [ ] **CA-C2.** *Given* `tailscale up` falha por rede/DNS/control plane inacessível (sem assinatura de authkey no stderr); *When* propaga; *Then* mantém a mensagem genérica de rede atual (não classifica errado).
- [ ] **CA-C3.** *Given* authkey válida e reusável no console; *When* dois clientes headless rodam o fluxo em paralelo; *Then* ambos obtêm IP Tailscale e conectam (gate in-game — RN-6).
- [ ] **CA-C4.** *Given* a mudança de contrato de retorno do `EnsureTailscaleConnected`; *When* o caminho de sucesso roda; *Then* o comportamento pós-conexão (ConfigureFika, navegação) é idêntico ao atual (sem regressão do login normal).

### Gate transversal de build

- [ ] **CA-G.** `dotnet build SPT.Launcher.csproj -c Release`, `dotnet test SPT.Launcher.Tests.csproj -c Release` e `dotnet build TarkovRedLine.Server.csproj -c Release` verdes. Nunca rodar o exe.

## Corner cases

- **CC-1.** Fika distribuído pelo server E presente local (manifesto cobre): o skip de manifesto (`SyncPlanner.cs:223`) já protege; a allowlist é redundante e inofensiva ali. Nenhuma dupla-ação.
- **CC-2.** Arquivo Fika já em `plugins-disabled` (quarentenado por um sync anterior ao fix): o fix **planeja preservar dali pra frente**, mas não desfaz automaticamente a quarentena existente. Documentar no gate in-game: pode exigir mover manualmente `Fika.Core.dll` de volta uma vez (ou reinstalar Fika) antes do primeiro sync com o fix.
- **CC-3.** Allowlist ampla demais preservaria DLLs de terceiros que casam por acaso — por isso o casamento é por **família de nome de assembly Fika** (prefixo `Fika.` + `.dll`) sob prefixo mirror, não por substring solta. Membros exatos = decisão de produto.
- **CC-4.** Op destrutiva com o server offline (`NoConnection`): fluxo atual já trata (`ProfileViewModel.cs:1143`); o aviso de coop não muda isso.
- **CC-5.** Host fecha o EFT mas o SPT.Server segue no ar com clientes conectados: `GameRunning=false` no host, então o gate local **libera** a exclusão. É exatamente o cenário que RN-4/RN-5 endereçam (o gate local é insuficiente por construção).
- **CC-6.** Authkey via gist válida mas single-use já consumida: sem CA-C1, o cliente extra vê "falha de rede" e culpa a própria internet. Com o fix, vê que é a chave.

## Fora de escopo

- Reverter automaticamente quarentenas Fika pré-existentes em `plugins-disabled` (só preserva dali pra frente — ver CC-2).
- Trocar a authkey embutida por emissão efêmera via endpoint autenticado (é o item de segurança 🟡 da auditoria, `TailscaleHelper.cs:17`; aqui só validamos reusabilidade + melhoramos o erro).
- Endpoint novo de presença/raid no server, se ele ainda não existir (CA-B4 é condicional; criar o endpoint é trabalho de server, fora deste item do launcher).
- Refatorar `OnOptionalToggled`/`GetServerBaseUrl` (gap de coop de opcionais é outro achado da auditoria, não deste item).

## Gates humanos (validação obrigatória — escrita em arquivo SPT exige jogo, não só build)

- **G-023.1 (in-game coop — Frente A).** Com `Fika.Core.dll` local e fora do manifesto, host + ≥1 cliente: rodar a verificação de arquivos no cliente e confirmar que o DLL **não** foi para `plugins-disabled` e que a raid conecta. Solo=host **não** valida isto — precisa de segundo cliente real.
- **G-023.2 (inspeção de produção — Frente A).** Antes de considerar entregue, inspecionar o `mods_repo/BepInEx/plugins/` do server: os plugins Fika client-only estão lá (entram no manifesto) ou não? Decidir se a allowlist é a defesa primária ou só a rede de segurança. Confirmar a **lista exata** de assemblies Fika essenciais.
- **G-023.3 (produto — Frente B).** Decidir CA-B4: existe hoje sinal de raid/peers ativos no TarkovRedLine.Server ou no Fika consultável pelo launcher? Se sim, ativar o pré-check; se não, ficar só no aviso (RN-4) e registrar a decisão.
- **G-023.4 (console Tailscale — Frente C).** Inspecionar no admin console que a authkey compartilhada é **reusável + efêmera + pré-autorizada**. Rodar o teste de dois clientes headless (CA-C3) e confirmar que ambos entram.
- **G-023.5 (produção — Frente B).** Nunca validar exclusão/wipe com uma sessão coop viva; testar as ops destrutivas apenas com o servidor sem clientes, inspecionando `{id}.json` antes/depois.

## Gates de build

`dotnet build SPT.Launcher.csproj -c Release` · `dotnet test SPT.Launcher.Tests.csproj -c Release` · `dotnet build TarkovRedLine.Server.csproj -c Release` — verdes. Detalhe na [05-asbuild](./023-coop-sync-hardening-05-asbuild.md) (a produzir na fase de código). Nunca rodar o exe.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-04 | Guilherme | Criação — spec funcional das 3 frentes de coop-hardening a partir do kickoff + auditoria. |
