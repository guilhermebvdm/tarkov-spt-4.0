# 034 — Quarentena move a pasta do mod inteira + faxina de pastas vazias (plugins/patchers)

**Mod:** Launcher4.0-v2
**Status:** Backlog
**Criado:** 2026-08-01

## Visão geral

Quando o launcher tira um mod do jogo — porque o jogador o desligou na tela Mods e Configs, ou porque o mod saiu do manifesto do servidor — ele move os arquivos do mod para uma pasta de quarentena (`plugins-disabled/`, `patchers-disabled/`). Hoje esse move é feito **arquivo por arquivo**: o destino fica correto, mas a **pasta de origem do mod fica para trás, vazia** (ex.: `plugins/PiP-Disabler/` continua existindo, sem nada dentro). O objetivo deste item é mover a **pasta do mod inteira** de uma vez (quando ela vive em pasta própria) e, como rede de segurança, **remover as pastas vazias** que sobrarem sob `plugins/` e `patchers/`.

## Comportamento atual

- **Move por arquivo, casca deixada para trás.** Ao quarentenar um mod opcional desligado (`QuarantineDisabledOptionalMods`) ou um extra fora do manifesto sob regra `mirror-move-disabled` (`ScanExtras`), o planner gera uma ação `MoveToDisabled` **por arquivo**. O motor (`SyncEngine`, caso `MoveToDisabled` → `MoveWithOverwrite`) move cada arquivo individualmente, preservando a subestrutura no destino (`plugins-disabled/optional/PiP-Disabler/PiP-Disabler.dll`). A **pasta-pai na origem nunca é removida** — `plugins/PiP-Disabler/` permanece como casca vazia.
- **Confirmado na sessão (2026-08-01):** ao desligar o PiP-Disabler, o `.dll` e o `.bundle` foram corretamente movidos para `plugins-disabled/optional/PiP-Disabler/`, mas `plugins/PiP-Disabler/` ficou vazia no lugar. **Não houve perda de dados** — o mecanismo de destino está certo; o defeito é só a casca na origem.
- **Escopo hoje afetado.** Vale para os dois roots com regra `mirror-move-disabled`: `plugins/` (mods opcionais desligados → `plugins-disabled/optional/`; extras → `plugins-disabled/`) e `patchers/` (extras → `patchers-disabled/`; ex.: a pasta `WTT-ContentBackportPatcher`).

## Comportamento desejado

Três mecanismos, aplicados em todos os canais que movem para `*-disabled`:

**1. Mover a pasta do mod inteira.** Quando o mod a quarentenar vive numa **pasta própria** sob um root de espelho (`plugins/<Mod>/`, `patchers/<Mod>/`), o launcher move a **pasta inteira de uma vez** para `<root>-disabled/<origem>/<Mod>/`, em vez de arquivo por arquivo. A origem `plugins/<Mod>/` deixa de existir — sem casca vazia. O destino mantém o mesmo namespace por origem já em uso (`optional/` para mod opcional desligado; raiz para extra de espelho).

Como o launcher sabe qual é "a pasta do mod":
- **Mod opcional:** a fronteira vem do que o **servidor cataloga** (o `paths` do mod no `plugins-optional.json`). Quando um `paths` é uma **pasta** (ex.: `BepInEx/plugins/PiP-Disabler`), essa pasta inteira é a unidade a mover.
- **Extra fora do manifesto:** o launcher não tem catálogo — a "pasta do mod" é o **primeiro nível de diretório** sob o root (`plugins/<X>/…` → a unidade é `plugins/<X>/`). Um arquivo extra solto direto no root (`plugins/x.dll`) não tem pasta e segue como move de arquivo.

**2. Fallback para o modo por-arquivo quando há arquivo protegido (coop-safe).** Se **qualquer** arquivo sob a pasta do mod estiver protegido — plugin coop-essencial (família Fika), arquivo `ignored`, excluído da limpeza, protegido, ou já sob um segmento `-disabled` — o move-de-pasta-inteira **não** roda. O launcher cai no comportamento atual (move por arquivo apenas os liberados) e **deixa o(s) protegido(s) no lugar**. A pasta do mod permanece na origem (não fica vazia, porque o protegido continua lá) e não é apagada. Isso mantém a paridade host↔cliente de coop.

**3. Faxina de pastas vazias (rede de segurança).** Ao final do sync, o launcher varre os roots de espelho-com-quarentena — hoje `plugins/` e `patchers/`; genericamente, todo root com regra `mirror-move-disabled` — e remove **toda pasta vazia** encontrada, **de baixo para cima** (uma pasta que contém apenas subpastas vazias fica vazia quando elas são removidas, e também sai). **Nunca** remove o próprio root, mesmo que ele fique momentaneamente sem nada. Cobre as cascas deixadas pelo fallback por-arquivo, por extras movidos individualmente, e por syncs de versões anteriores (antes deste item).

Mods que são um **`.dll` solto** direto no root (`plugins/Foo.dll`, sem pasta própria) seguem sendo movidos como arquivo — não há pasta para levar.

## Critérios de aceite

- [ ] **CA-034.1 (mod-pasta opcional desligado):** desligar na tela um mod opcional que vive em pasta (ex.: PiP-Disabler) e sincronizar → a pasta some de `plugins/` e reaparece **inteira** em `plugins-disabled/optional/<Mod>/`; **nenhuma** casca vazia `plugins/<Mod>/` permanece.
- [ ] **CA-034.2 (mod-pasta extra fora do manifesto):** um mod-pasta que saiu do manifesto (regra `mirror-move-disabled`) → a pasta inteira vai para `plugins-disabled/<Mod>/` (ou `patchers-disabled/<Mod>/`), sem casca na origem.
- [ ] **CA-034.3 (`.dll` solto inalterado):** um mod que é só um `.dll` direto em `plugins/` (sem pasta própria) continua sendo movido como arquivo; nenhuma pasta de terceiros é movida junto por engano.
- [ ] **CA-034.4 (fallback coop-safe):** um mod-pasta cujo diretório contém um arquivo coop-essencial → a pasta **não** é movida inteira; o arquivo protegido permanece em `plugins/<Mod>/`, os demais vão para a quarentena, e a pasta **não** é apagada.
- [ ] **CA-034.5 (faxina de vazias):** após o sync, **nenhuma** pasta vazia permanece sob `plugins/` ou `patchers/`, inclusive cascas deixadas por syncs anteriores; os roots `plugins/` e `patchers/` **nunca** são removidos, mesmo se momentaneamente vazios.
- [ ] **CA-034.6 (patchers):** o comportamento de CA-034.1/2 vale igualmente para `patchers/` (ex.: `WTT-ContentBackportPatcher`) — mod-pasta desligado/extra vai inteiro para `patchers-disabled/`, sem casca.
- [ ] **CA-034.7 (relatório inteligível):** mover a pasta de um mod para a quarentena gera **uma única entrada agregada** no relatório do sync (`last-update.json`) — "o mod X foi movido para a quarentena" —, não uma linha por arquivo (decisão do usuário, 2026-08-01). A faxina de pastas vazias **não** conta como "arquivo atualizado" nem polui o relatório com ruído. Alinhar o rótulo com o item 031 (notificação de sync).
- [ ] **Fika/multiplayer:** o fallback coop-safe (mecanismo 2) garante que um plugin coop-essencial dentro da pasta de um mod nunca seja arrastado para a quarentena junto com a pasta — a paridade de mods host↔cliente é preservada. O mecanismo é 100% client-side (roda no sync local, sem pacote de rede); o guard coop-safe existente segue como segunda barreira.
- [ ] **Estado entre raids:** `N/A` — a ação acontece no launcher (fluxo de login/pré-jogo), nunca dentro do EFT durante um raid; nenhum estado de raid é criado ou alterado.

## Corner cases

- [ ] **CC-1 (pasta compartilhada por dois mods):** `plugins/<Shared>/` contém arquivos de dois mods, um ligado e outro desligado (ou um catalogado e um extra). Mover a pasta inteira levaria o mod ligado junto. Definir: se a pasta contém **algum arquivo que não faz parte do conjunto sendo quarentenado** (pertence a mod ligado, é entrada de manifesto que fica, etc.), cair no modo por-arquivo — nunca mover a pasta inteira.
- [ ] **CC-2 (faxina apaga pasta que um mod espera vazia):** um mod pode depender de uma pasta vazia existir (saída/cache criada no primeiro boot). A faxina "qualquer pasta vazia" a removeria. **Risco aceito pelo usuário** (2026-08-01) ao escolher "qualquer pasta vazia" em vez de "só as que nós esvaziamos". Registrar o risco; mitigação (lista de exceção) fica fora de escopo.
- [ ] **CC-3 (Dev Mode):** em Dev Mode o launcher **não** move mods para quarentena (guard existente) **e** a faxina de pastas vazias também é **pulada** (decisão do usuário, 2026-08-01) — coerente com "Dev Mode preserva arquivos locais" e para não apagar pastas de trabalho de quem desenvolve mods.
- [ ] **CC-9 (mod opcional com `paths` misto — pasta + `.dll`):** um mod opcional pode catalogar **os dois** (ex.: DragonDenDevTool = `plugins/DragonDenDevTool` **+** `plugins/Drexira.DragonDenDevTool.dll`). Ao desligar, a **pasta** vai inteira para a quarentena (mecanismo 1) **e** o `.dll` solto vai como arquivo (mecanismo do `.dll` solto) — nenhum dos dois fica para trás.
- [ ] **CC-10 (pasta do mod com subpastas aninhadas):** um mod-pasta com várias camadas (ex.: BorkelRNVG com `Assets/…/…`) é movido com toda a subestrutura preservada no destino; a origem não deixa nenhuma subpasta órfã.
- [ ] **CC-4 (destino de quarentena já existe):** `plugins-disabled/optional/<Mod>/` já existe de um desligamento anterior. Um move de pasta atômico falharia com destino existente. Definir: mesclar (mover o conteúdo por cima, sobrescrevendo homônimos) e então remover a origem esvaziada.
- [ ] **CC-5 (idempotência):** rodar o sync duas vezes seguidas não duplica nem gera erro — na segunda passada o mod já está na quarentena e não há o que mover; a faxina não encontra pasta vazia nova.
- [ ] **CC-6 (nunca varrer a própria quarentena):** a faxina de vazias deve pular qualquer caminho sob um segmento `-disabled` — nunca entra em `plugins-disabled/`/`patchers-disabled/` para "limpar".
- [ ] **CC-7 (casing da pasta no Windows):** ao mover a pasta, preservar o nome exato como está no disco (casing visível ao jogador), não a forma normalizada em minúsculas.
- [ ] **CC-8 (falha parcial no move):** se o move de uma pasta falhar no meio, o erro é isolado por-mod (não derruba o plano inteiro nem perde arquivos) — mesma garantia por-ação que o motor já dá hoje.

## Fora de escopo

- [ ] Limpeza/retenção do **conteúdo dentro** de `*-disabled` — a quarentena continua acumulando indefinidamente; enxugá-la é outro item.
- [ ] Restaurar arquivos **da quarentena** ao religar um mod (hoje religar rebaixa do servidor) — tema separado.
- [ ] Lista de exceção de pastas vazias que devem sobreviver à faxina (ver CC-2).
- [ ] A assimetria de backup "sobrescreve vs nome-livre" entre os canais de move e os de copy (`MoveWithOverwrite` × `ResolveFreeBackupRelative`) — tema separado.

## Referências

- [Item 007 — Sincronização de arquivos](../007-sincronizacao-arquivos/) (regras por pasta; `plugins`/`patchers` = espelho movendo removidos para `*-disabled`)
- [Item 030 — Tela "Mods e Configs"](../030-mods-e-configs-tela/) (quarentena por origem, D-14: `plugins-disabled/optional/`)
- [Item 031 — Notificação de sync](../031-notificacao-sync-mensagem-final/) (correlato: rótulo da ação de mover para `*-disabled` no relatório/UI)

## Histórico

| Data | Evento |
|---|---|
| 2026-08-01 | Item criado via `/create-spec`. Comportamento e decisões fechados com o usuário em sessão: mover a pasta do mod inteira (com fallback coop-safe por-arquivo), `.dll` solto inalterado, faxina de "qualquer pasta vazia" sob plugins/patchers. Origem: casca vazia `plugins/PiP-Disabler/` observada após desligar o mod. |
| 2026-08-01 | Revisão `/review-spec` — 2 gaps + 2 corner cases corrigidos; 2 trechos marcados `<!-- review -->`. Adicionados: definição operacional da "pasta do mod" (opcional via `paths` do servidor × extra via 1º nível); faxina generalizada para roots `mirror-move-disabled` + definição de "vazia" recursiva; CA-034.7 (relatório inteligível, correlato item 031); CC-9 (`paths` misto pasta+dll, ex. DragonDen); CC-10 (subpastas aninhadas). |
| 2026-08-01 | Decisões do usuário travadas (markers removidos): CA-034.7 → **1 entrada agregada** por pasta movida; CC-3 → faxina de vazias **pulada em Dev Mode**. |
