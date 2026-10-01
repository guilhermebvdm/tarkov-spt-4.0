# 008 — Falha ao descartar um jogador no fim da raid trava extração de todos

**Mod:** FIKA
**Status:** Backlog
**Criado:** 2026-09-11

## Visão geral

No fim da raid, `CoopGame.Stop()` itera todos os jogadores da sala (menos o próprio jogador local) para descartá-los (`Dispose()` + devolução ao pool de assets). Se o `Dispose()` de **um único** jogador lançar uma exceção, o bloco `try/catch` responsável loga o erro e em seguida relança a mesma exceção (`throw;`), abortando todo o restante do `Stop()` — nada do que vem depois do loop (parar o `GameTimer`, fechar a UI, destruir o `CoopHandler`, transicionar de volta ao menu) chega a executar. O resultado observado em raid real: tela preta após pressionar extrair, e nenhum jogador da sala consegue voltar ao menu — um problema de um único jogador (nesta ocorrência, causado por interação com outro mod) derruba a extração de todo mundo. Este item ataca esse ponto único de falha do lado FIKA, isolando a falha de disposal de um jogador sem abortar o teardown da raid para os demais.

## Comportamento atual

- `CoopGame.Stop()` (`modded/Fika-Plugin/Fika.Core/Main/GameMode/CoopGame.cs:779-800`) percorre uma cópia de `CoopHandler.Players.Values`, pulando o próprio jogador local, e para cada jogador restante chama `player.Dispose()` seguido de `AssetPoolObject.ReturnToPool(player.gameObject, true)` dentro de um `try/catch`.
- O `catch` (`:794-798`) loga a exceção (`"There was an error disposing of player [<nick>]: <ex>"`) e imediatamente faz `throw;` — relançando a mesma exceção para fora do loop e para fora de `Stop()`.
- Esse comportamento é **código original do FIKA** (idêntico em `original/Fika-Plugin/Fika.Core/Main/GameMode/CoopGame.cs:779-800`), não uma mudança introduzida por este repo.
- Tudo que vem depois do loop dentro de `Stop()` — destruir o `CoopHandler` (quando não é transição de mapa), montar o `ExitManager`, parar `GameTimer`, fechar `TimerPanel`, e o restante da sequência de saída da raid — nunca executa quando isso acontece, porque a exceção propaga antes de chegar lá.
- Reproduzido em raid real: um jogador que morreu durante a raid teve o GameObject da arma na mão destruído por outro sistema (interação cross-mod, tratada separadamente); ao tentar descartá-lo no fim da raid, `FirearmController.Destroy()` chamou `Animator.SetBool()` num Animator já destruído, lançando `NullReferenceException`, capturada e relançada por este bloco.

## Comportamento desejado

- Uma falha ao descartar um jogador específico no fim da raid é logada (mantendo o log atual, que já identifica o jogador pelo nick) mas **não** aborta o restante de `Stop()` — o loop continua para os próximos jogadores, e a sequência de saída da raid (parar timer, fechar UI, transição ao menu) executa normalmente para todos.
- O jogador cujo `Dispose()` falhou fica em estado best-effort (o que já foi descartado antes da exceção permanece descartado; o que não foi, fica pendente) — não é objetivo deste item garantir disposal 100% completo daquele jogador específico, só impedir que a falha dele impeça a extração dos demais.
- Nenhuma mudança de comportamento para o caminho feliz (todos os jogadores descartam sem erro) — a sequência de saída da raid continua idêntica.

## Critérios de aceite

- [ ] Se o `Dispose()` de um jogador da sala lançar uma exceção no fim da raid, os demais jogadores da sala ainda conseguem extrair normalmente e retornar ao menu principal (sem tela preta).
- [ ] A exceção do jogador com falha continua aparecendo no log com o mesmo formato atual (`"There was an error disposing of player [<nick>]: <ex>"`), sem perda de informação de diagnóstico.
- [ ] Jogadores subsequentes no mesmo loop (que vêm depois do jogador com falha na iteração de `CoopHandler.Players.Values`) também são descartados normalmente — a falha de um não impede o processamento dos seguintes.
- [ ] `Destroy(CoopHandler)`, `GameTimer.TryStop()`, fechamento de `TimerPanel` e o restante da sequência de `Stop()` executam mesmo quando um jogador falhou o disposal.
- [ ] **Fika/multiplayer:** validado especificamente em raid Headless/coop com 2+ jogadores, forçando (ou reproduzindo) a falha de disposal em um jogador não-local enquanto outros extraem — todos os jogadores saem da raid normalmente, não só o que não teve falha.
- [ ] **Estado entre raids:** N/A — a correção só muda o tratamento de uma exceção dentro da sequência de término de UMA raid; não introduz nem depende de estado persistente entre raids.

## Corner cases

- [ ] Falha de disposal no **próprio jogador local** (`myPlayer`) — fora do escopo deste loop especificamente (ele é pulado via `player.IsYourPlayer`, linha 784-787), mas vale confirmar que nenhum caminho equivalente de disposal do jogador local tem o mesmo padrão de `throw;` sem isolamento em outro ponto de `Stop()`.
- [ ] Múltiplos jogadores falhando o `Dispose()` na mesma raid (não só um) — cada falha deve ser logada individualmente, sem que a primeira suprima o log das seguintes.
- [ ] `AssetPoolObject.ReturnToPool` falhando separadamente de `player.Dispose()` (ex: `Dispose()` funciona mas o retorno ao pool falha) — confirmar que o tratamento cobre os dois pontos do `try`, não só o primeiro.
- [ ] Transição de mapa (`FikaBackendUtils.IsTransit`) — o `Destroy(CoopHandler)` já é condicionado a `!IsTransit` (linha 806-809); confirmar que isolar a falha do loop não muda esse comportamento condicional existente.
- [ ] Falha ocorre no **último** jogador da lista vs. no **primeiro** — não deve haver diferença de comportamento; todo o restante de `Stop()` deve rodar em ambos os casos.

## Fora de escopo

- [x] Corrigir a causa raiz do `NullReferenceException` observado em raid real (Animator de arma já destruído por interação cross-mod) — tratado como item separado no mod de origem, fora do FIKA. Este item é a rede de segurança geral, não a correção do gatilho específico.
- [x] Garantir disposal 100% completo do jogador que falhou — fora de escopo; o objetivo é não travar os demais, não recuperar o estado do jogador com falha.
- [ ] A definir: se vale adicionar telemetria/agregação (ex: contar quantos jogadores falharam disposal numa mesma raid) além do log já existente — decisão de desenho para a spec técnica.

## Referências

- [002-inventory-desync-watchdog/](../002-inventory-desync-watchdog/) — mesmo padrão de filosofia já aplicado no FIKA deste repo: isolar/conter falha em vez de deixar travar a experiência inteira (lá, watchdog de timeout; aqui, catch sem rethrow).
- [006-colisao-maos-item-nao-carregador/](../006-colisao-maos-item-nao-carregador/) — mesmo padrão aplicado a `catch` vazios em `ClientInventoryOperationHandler` (logar em vez de engolir silenciosamente); este item é o inverso — logar sem interromper o fluxo, em vez de logar e interromper.
- [docs/technical/fika-packet-desync-prevention-plan.md](../../../../docs/technical/fika-packet-desync-prevention-plan.md) §2 causa 4 — mesmo formato de bug estrutural (uma exceção não contida derruba processamento de todo mundo, não só da origem do erro), só que no caminho de rede em vez do caminho de disposal de fim de raid.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-11 | Item criado via `/add-backlog-item`, a partir de bug relatado em raid real (extração travada em tela preta para toda a sala) — causa imediata é um `throw;` em `CoopGame.Stop()` (código original do FIKA) que aborta o teardown inteiro da raid quando o `Dispose()` de um único jogador falha |
