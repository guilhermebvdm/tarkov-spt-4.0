# 006 — Trava de mãos ao equipar arma/faca/granada após ação recente (item não-carregador)

**Mod:** FIKA
**Status:** Backlog
**Criado:** 2026-09-10

## Visão geral

A tolerância de colisão de mãos introduzida pelos itens `003`/`004` (`IsSelfReferentialHandsTransition`) só libera a colisão `inOutHandsProcess` quando o item que está sendo validado agora é um carregador (`MagazineItemClass`). Isso deixa a mesma classe de trava (`GClass1561`, "is currently being modified") acontecer sempre que a ação seguinte a uma transição de mãos recente do mesmo jogador na mesma arma não for uma troca de carregador — reequipar a própria arma, sacar faca, arremessar granada ou trocar de arma. Este item generaliza a condição pro lado que ainda falta, sem enfraquecer a proteção contra colisão real entre dois jogadores disputando a mesma arma. Agrupa também dois achados relacionados de resiliência de rede/logging levantados na mesma auditoria (`docs/relatorio-auditoria-codigo-01.md`, achados `AUD-01-02` e `AUD-01-04`).

## Comportamento atual

- `ObservedInventoryController.IsSelfReferentialHandsTransition` (`modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs:218-223`) começa com:
  ```csharp
  if (item is not MagazineItemClass || inOutHandsProcess?.Item is not Weapon weapon)
  {
      return false;
  }
  ```
  Ou seja, se `item` (o item envolvido na operação sendo validada agora) não for um carregador, o método retorna `false` incondicionalmente — mesmo que a colisão seja com uma transição de mãos própria, recente, do mesmo jogador na mesma arma (o exato cenário que os itens `003`/`004` já tratam para carregador).
- Reproduzido pela auditoria em dois cenários reais:
  1. `SPT-ContinuousLoadAmmo` chama `Player.SetEmptyHands(...)` ao iniciar carregamento de munição fora do inventário e `Player.TrySetLastEquippedWeapon()` ao terminar. No Headless/Host, se o `Begin` de `SetEmptyHands` ainda estiver pendente quando `TrySetLastEquippedWeapon` chega, `item == weapon` (não é carregador) → rejeitado com `GClass1561("...Default Inventory is currently being modified")`.
  2. Curar um aliado (`TRL-ImmersiveCombatMedicine`) abre a mesma janela `Begin` com o item de cura; se a ação seguinte dentro da janela de graça (`GraceWindowSeconds = 0.35f`) for sacar faca, arremessar granada ou trocar de arma (em vez de trocar o carregador da arma atual, único caso coberto pelo item `004`), a mesma rejeição ocorre.
- Achados relacionados na mesma auditoria, mesmo item de backlog:
  - `NetPacketProcessor.GetCallbackFromData` (`modded/Fika-Plugin/Fika.Core/Networking/LiteNetLib/Utils/NetPacketProcessor.cs:83-91`) lança `ParseException` não capturada quando recebe um pacote de tipo desconhecido (mod de terceiro ausente ou com versão desalinhada); a exceção propaga sem barreira até `PollEvents()`/`Update()` (`LiteNetManager.cs:1436-1441`, `FikaClient.cs:495-501`, `FikaServer.cs:934-947`), descartando **todos os demais eventos de rede já enfileirados naquele lote/frame**, não só o pacote ruim. <!-- review: este é o antipattern já catalogado AP-11 ("causa raiz 4") em docs/technical/spt-antipatterns.md e docs/technical/fika-packet-desync-prevention-plan.md §2 — o guia documenta a mitigação por-mod (airbag no callback), mas hash nunca registrado não tem callback nenhum pra proteger; este item fecha esse gap com um airbag central. Ler o guia antes de implementar: ele também exige throttle de log em caminhos de alta frequência (§4 regra 4), incorporado no corner case de rajada abaixo. -->
  - `ClientInventoryOperationHandler.cs:58-65` e `:106-117` têm dois blocos `catch (Exception) { }` totalmente vazios (sem log), destoando do resto do arquivo, que loga todo outro branch de erro via `FikaGlobals.LogError`.

## Comportamento desejado

- `IsSelfReferentialHandsTransition` passa a tolerar a colisão residual independentemente do tipo de `item` sendo validado agora (arma, faca, granada, carregador ou qualquer outro), desde que a colisão seja com uma transição de mãos recente (dentro de `GraceWindowSeconds`) aberta pelo **mesmo jogador** na **mesma arma**, e desde que não se trate da própria arma colidindo consigo mesma (`item == weapon && movedItem == weapon`) — esse caso continua bloqueado, preservando a proteção original contra dois jogadores disputando a mesma arma.
- `NetPacketProcessor`/`FikaClient`/`FikaServer` capturam `ParseException` de pacote desconhecido, registram um aviso em log e continuam processando o restante do lote de eventos do mesmo frame — em vez de abortar o `Update()` inteiro.
- `ClientInventoryOperationHandler` registra em log qualquer exceção capturada nos dois pontos hoje silenciosos, no mesmo padrão (`FikaGlobals.LogError`) usado no resto do arquivo.

## Critérios de aceite

- [ ] Equipar a arma principal imediatamente após carregar munição continuamente fora do inventário (`SPT-ContinuousLoadAmmo`, uso repetido) não gera mais o erro "is currently being modified" nem trava a mão do jogador.
- [ ] Sacar faca, arremessar granada ou trocar para a arma secundária logo após curar um aliado (dentro da janela de graça de `GraceWindowSeconds`) não gera mais a mesma trava.
- [ ] Um pacote de tipo desconhecido recebido pelo cliente ou pelo servidor gera um aviso no log, sem lançar exceção não tratada e sem descartar os demais eventos de rede já enfileirados no mesmo lote.
- [ ] Uma exceção capturada em `ClientInventoryOperationHandler` (nos dois pontos hoje com `catch` vazio) aparece no log com a mensagem/stack trace, em vez de ser silenciada.
- [ ] `node scripts/check-packet-hashes.js` continua reportando 0 colisões de CRC-16 depois da mudança (nenhum tipo/nome novo introduzido por este item — critério de regressão, não de feature nova).
- [ ] **Fika/multiplayer:** dois jogadores tentando pegar a mesma arma real ao mesmo tempo (concorrência genuína, não uma transição de mãos própria) continua sendo rejeitado com `GClass1561` — a generalização não pode enfraquecer essa proteção. Validado especificamente em raid Headless com 2+ jogadores.
- [ ] **Estado entre raids:** nenhuma das três correções deste item introduz estado novo. A tolerância de colisão de mãos reaproveita o `ConditionalWeakTable` já existente do item `003` (escopado ao `TraderControllerClass` do raid atual, não sobrevive entre raids); o airbag central de pacote desconhecido é um `try/catch` sem estado (não guarda referência a `NetPeer`/pacote); o logging adicionado em `ClientInventoryOperationHandler` só formata a exceção já capturada, sem reter nada.

## Corner cases

- [ ] Dois jogadores diferentes tentam pegar a mesma arma (saque real concorrente) — deve continuar sendo rejeitado, não tolerado pela generalização.
- [ ] Fold/dobra de coronha (`FoldOperationClass`) reentrando na mesma arma logo após um saque — não deve ser confundido com uma transição legítima de item diferente; o bloqueio de `item == weapon && movedItem == weapon` cobre esse caso.
- [ ] A janela de graça (`GraceWindowSeconds`) expira exatamente durante a checagem (`elapsed` igual ou levemente acima do limite) — comportamento de borda não deve gerar exceção nem falso-positivo.
- [ ] Pacote de tipo desconhecido chega em rajada (vários pacotes ruins seguidos, ex: mod desatualizado enviando repetidamente em alta frequência) — o log usa o padrão de throttle já exigido pelo guia canônico (`fika-packet-desync-prevention-plan.md` §4 regra 4: stack completo na primeira ocorrência de cada hash, resumo periódico depois) em vez de logar toda ocorrência; demais pacotes válidos no mesmo lote continuam sendo processados normalmente.
- [ ] Exceção real (não cosmética) dentro de `ClientInventoryOperationHandler` que hoje é engolida — ao logar, confirmar que o handler ainda retorna corretamente ao pool sem ficar em estado inconsistente (não introduzir uma nova falha ao só adicionar o log).

## Fora de escopo

- [x] Calibração de `GraceWindowSeconds` (hoje `0.35f`, `TODO confirmar`) — pendência conhecida `P-3.1`, tratada em item de backlog separado.
- [x] Construir detecção de colisão de hash (achado `AUD-01-03`, revisado) — **já existe** como `node scripts/check-packet-hashes.js` (0 colisões hoje, ver `docs/technical/spt-antipatterns.md` AP-11); este item só confirma que o script continua limpo depois da mudança (ver critério de aceite acima), não precisa construir nada.
- [x] Negociação de capacidades entre peers para broadcast de pacotes de terceiros (achado `AUD-01-09`) — item de roadmap arquitetural, não correção cirúrgica.
- [x] Duplicação `Climbable Ladders/ladders.fika` vs `TRL-FikaSync-ClimbableLadders` (achado `AUD-01-05`) — ação de configuração/`.sln`, não `/code-mod` do FIKA.
- [x] Retorno "fantasma" de `FikaUIGlobals.ShowFikaMessage` fora da main thread (achado `AUD-01-07`) — cosmético, sem impacto prático hoje.
- [x] Confirmação por leitura de código do caminho de reanimação (achado `AUD-01-08`) — é nota de documentação, não mudança de comportamento.

## Referências

- [Relatório de Auditoria Técnica de Código — FIKA (Review 01)](../../docs/relatorio-auditoria-codigo-01.md) — achados `AUD-01-01`, `AUD-01-02`, `AUD-01-04`.
- [004-colisao-cura-swap-magazine/](../004-colisao-cura-swap-magazine/) — item anterior que introduziu `IsSelfReferentialHandsTransition`, generalizando só o lado `movedItem`.
- [003-magazine-swap-inplace-fix/](../003-magazine-swap-inplace-fix/) — item que introduziu `InOutHandsProcessTimestampPatch` e o mecanismo de correlação `Begin`/`Succeed`.
- [docs/technical/spt-antipatterns.md](../../../../docs/technical/spt-antipatterns.md) — `AP-11` (pacote FIKA que desalinha ou corrompe o stream compartilhado), antipattern já catalogado que cobre a parte de rede deste item.
- [docs/technical/fika-packet-desync-prevention-plan.md](../../../../docs/technical/fika-packet-desync-prevention-plan.md) — guia canônico de rede FIKA; §2 descreve a "causa raiz 4" (mecanismo comum de `ParseException` derrubando a fila do frame) que este item corrige na fronteira central; §4 regra 4 exige throttle de log; §7 traz o checklist de auditoria de rede completo.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-10 | Item criado via `/add-backlog-item`, a partir dos achados `AUD-01-01`/`AUD-01-02`/`AUD-01-04` da auditoria comparativa `docs/relatorio-auditoria-codigo-01.md` |
| 2026-09-10 | Revisão `/review-spec` — 1 gap + 3 corner cases/critérios corrigidos ou adicionados; achado `AUD-01-03` do relatório de auditoria corrigido por carambola (detecção de colisão de hash já existe, `AP-11`) |
