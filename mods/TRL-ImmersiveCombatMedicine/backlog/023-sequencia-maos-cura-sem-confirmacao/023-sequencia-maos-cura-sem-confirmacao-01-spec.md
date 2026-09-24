# 023 — Sequência de mãos da cura sem confirmação de operação

**Mod:** TRL-ImmersiveCombatMedicine
**Status:** Backlog
**Criado:** 2026-09-12

## Visão geral

Curar um aliado exige simular o uso do item de cura no próprio médico sem consumi-lo do jeito nativo: o item é colocado na mão pra tocar a animação, a animação é forçada a terminar, e a arma original é devolvida às mãos. Hoje essa sequência dispara cada chamada (colocar item na mão → forçar fim da animação → devolver arma) sem esperar confirmação real de que o passo anterior concluiu — assume sucesso por tempo fixo ou por chamada síncrona. Essa é exatamente a mesma classe de problema que, do lado do FIKA, abriu janelas de colisão de mãos que já levaram a 3 rodadas de correção (itens `003`, `004` e `006`, todos tolerando o sintoma em vez de evitar a causa). Este item investiga se dá pra reforçar essa sequência na origem — aqui, no `TRL-ImmersiveCombatMedicine` — em vez de depender só da tolerância construída do lado FIKA.

## Comportamento atual

- `HealRoutine` (`Patches/Medical/BandAidController.cs`, ~linhas 561-629) coloca o item de cura na mão via `SetInHands(itemUsed, (result) => { })` — o callback que informaria sucesso/falha da operação é vazio, o resultado é ignorado. Em seguida, o código só espera um tempo fixo (`UseTime` + margem) antes de chamar `ForceFinishAnimation()`, que devolve a arma — sem nenhuma confirmação real de que a transição de mãos concluiu.
- `EmergencyDrop` (mesmo arquivo, ~linhas 481-517) dispara três operações que mexem em mãos/inventário em sequência imediata — `ForceFinishAnimation()` → `TrySetLastEquippedWeapon(true)` → `InventoryController.ThrowItem(savedItem)` — cada uma só com `try/catch` isolado, sem esperar confirmação da anterior antes de seguir pra próxima.
- `Helpers/HandsStateGuard.cs` já existe e impede **iniciar uma nova interação** enquanto as mãos estão ocupadas com item de cura/comida (o próprio comentário do arquivo cita prevenção da trava `"hands controller can't perform this operation"`) — mas não participa da sequência interna acima; só é checado na entrada, antes de tudo começar.
- Esse mecanismo (`ForceFinishAnimation`/`SetLastEquippedWeapon`) é citado nominalmente na memória do FIKA (`mods/FIKA/memory/sessions.md`, Sessão 4) como o gatilho direto da colisão que o item `004` do FIKA precisou tolerar — ou seja, a causa já é conhecida cruzando os dois mods, só nunca foi atacada deste lado.
- Achado cruzado no próprio mod: `revisao-item-06-protocolo-de-rede-fika.md` (2026-08-15) já registrou que os pacotes de rede **próprios** do mod (handshake de cura entre médico e paciente), quando compartilhavam a fila `ReliableOrdered` do FIKA, geravam a mesma trava `"Default Inventory is currently being modified"` — corrigido movendo esse tráfego pra `DeliveryMethod.ReliableUnordered`. Essa correção cobre só o protocolo de rede do mod, não as chamadas diretas `SetInHands`/`TrySetLastEquippedWeapon` que são o alvo deste item.
- <!-- review: gap — falta escopo importante. `revisao-item-04-animacao-e-redirecionamento.md` §2 documenta um "Ownership Guard": toda essa sequência (`SetInHands`/`ForceFinishAnimation`/devolver arma) só roda quando o médico é o `MainPlayer` local — pra bots curando e pra peers remotos, o fluxo cai em execução vanilla intocada (o guard retorna `true` sem tocar mãos). Ou seja, este item afeta só a experiência do JOGADOR HUMANO local quando ele próprio está curando (como médico), nunca quando ele é o paciente sendo curado por outro, nem quando um bot cura alguém. Isso deveria estar explícito aqui pra não gerar confusão na spec técnica sobre "curar" em qual direção/ator. -->

**Restrição de processo (não é do comportamento, é de como implementar):** dado o histórico relatado pelo usuário de que estabilizar essa animação foi muito custoso da primeira vez, a spec técnica e o `/code-mod` deste item devem propor a mudança de forma incremental/reversível (ex: guard/flag fácil de desligar, ou passos pequenos e testáveis um de cada vez) em vez de reescrever a sequência inteira de uma vez — sempre em `modded-V4` (nunca in-place nas versões anteriores). Ver `docs/technical/spt-antipatterns.md` se houver um AP-NN sobre mudança incremental em código sensível; se não houver, tratar como restrição só deste item mesmo.

## Comportamento desejado

- A sequência de simular o uso do item de cura (colocar na mão → tocar animação → devolver a arma) só avança de um passo pro próximo depois de confirmar que o passo anterior de fato concluiu — não por tempo fixo assumido, nem por chamada síncrona sem verificação de resultado.
- Se um passo falhar ou não confirmar dentro de um prazo razoável, o mod se recupera de forma segura (loga o problema e ainda assim devolve a arma ao médico) em vez de simplesmente prosseguir como se tivesse dado certo.
- `EmergencyDrop` passa a confirmar (ou serializar corretamente) as três operações de mãos/inventário, em vez de dispará-las em sequência imediata sem esperar nada entre elas.
- Nenhuma mudança no resultado percebido pelo jogador no caminho feliz (cura normal, sem latência de rede) — a diferença só aparece sob condição de atraso/colisão.

## Critérios de aceite

- [ ] Curar um aliado sob latência de rede simulada (ex: rajada de spawn de bot, o mesmo cenário do item `007` do FIKA) não deixa a arma do médico presa nas mãos do item de cura, nem gera a trava `"Default Inventory is currently being modified"` no host/peer.
- [ ] `EmergencyDrop` (cancelamento de cura no meio) sempre devolve a arma corretamente ao médico e dropa o item de cura, mesmo sob a mesma condição de latência simulada.
- [ ] O callback de `SetInHands` deixa de ser ignorado — uma falha real da operação (ex: hands controller rejeitando) é logada e tratada, não silenciosamente descartada.
- [ ] Nenhuma regressão perceptível no tempo de cura em condições normais (sem latência) — esperar confirmação não pode virar demora sensível quando tudo está respondendo normalmente.
- [ ] **Fika/multiplayer:** validado especificamente curando aliado em raid Headless/coop com 2+ jogadores sob rajada de spawn de bot (mesmo cenário de teste do item `007` do FIKA) — sem trava de mãos nem log de colisão do lado FIKA durante ou logo após a cura.
- [ ] **Estado entre raids:** N/A — a correção é só de sequenciamento dentro de uma única cura; não introduz nem depende de estado persistente entre raids.

## Corner cases

- [ ] Paciente morre durante a espera de confirmação do passo de colocar o item de cura na mão — já existe guard pra morte durante o `WaitForSeconds` atual; confirmar que o novo esquema de confirmação não abre uma janela sem esse guard equivalente.
- [ ] Médico é interrompido ou morre durante a própria transição de mãos, antes de qualquer confirmação chegar — a arma deve ficar recuperável ao reviver/reconectar, não presa indefinidamente.
- [ ] `EmergencyDrop` disparado antes da confirmação do `SetInHands` original ter chegado (cancelamento quase instantâneo, logo após iniciar a cura).
- [ ] Sessão singleplayer/hideout (sem FIKA ativo, sem rede) — esperar por confirmação não pode introduzir atraso artificial onde a operação hoje é efetivamente instantânea.
- [ ] Duas curas em sequência rápida pelo mesmo médico (uma cura termina, outra em aliado diferente começa logo em seguida) — a segunda não pode iniciar antes da primeira ter sido confirmada como concluída.
- [ ] A confirmação do próprio passo "devolver a arma" falha ou nunca chega — o item de cura não pode ficar preso na mão do médico indefinidamente só porque a confirmação não veio; precisa de um limite de segurança (não esperar pra sempre), sem virar a mesma armadilha de "assumir sucesso" que este item existe pra corrigir.
- [ ] Médico extrai da raid (sem morrer) enquanto uma cura ainda está em andamento, antes de qualquer confirmação chegar — a sequência precisa terminar de forma limpa nesse encerramento também, não só no caminho de morte/interrupção já coberto acima.

## Fora de escopo

- [x] Corrigir o protocolo de rede próprio do mod (`BandAidNetworkHandler`/pacotes de handshake médico-paciente) — já resolvido e validado (`revisao-item-06-protocolo-de-rede-fika.md`), não é o alvo deste item.
- [x] Alterar `IsSelfReferentialHandsTransition`/`HandsBookkeepingTimestampPatch` do lado FIKA (itens `003`/`004`/`006`) — continuam necessários como rede de segurança para qualquer OUTRA fonte de colisão; este item ataca a causa na origem, não substitui a tolerância do lado FIKA.
- [ ] A definir: se vale reaproveitar `HandsStateGuard` pra também guardar contra reentrância da própria sequência (nova cura iniciando antes da anterior confirmar), ou se isso pede um mecanismo novo — decisão de desenho pra spec técnica.

## Referências

- [FIKA — 003-magazine-swap-inplace-fix](../../../FIKA/backlog/003-magazine-swap-inplace-fix/) — mecanismo que primeiro tolerou colisão de transição de mãos por carregador.
- [FIKA — 004-colisao-cura-swap-magazine](../../../FIKA/backlog/004-colisao-cura-swap-magazine/) — cita `MedicHealPatch.ForceFinishAnimation()` deste mod como o gatilho direto da colisão tratada.
- [FIKA — 006-colisao-maos-item-nao-carregador](../../../FIKA/backlog/006-colisao-maos-item-nao-carregador/) — generalização da tolerância; a causa raiz real confirmada lá (`GEventArgs9`/`GEventArgs10`, bookkeeping de `Drop`) tem a mesma forma do problema deste item.
- [FIKA — 007-fila-inventario-bloqueada-spawn-bot](../../../FIKA/backlog/007-fila-inventario-bloqueada-spawn-bot/) — cenário de rajada de spawn de bot usado como condição de teste de latência para este item.
- [`revisao-item-06-protocolo-de-rede-fika.md`](../../revisao-item-06-protocolo-de-rede-fika.md) — achado independente, do próprio mod, sobre colisão com a fila `ReliableOrdered` do FIKA (protocolo de rede próprio, não as chamadas de mãos que são o alvo deste item).
- `Helpers/HandsStateGuard.cs` — guarda de entrada já existente, criada pelo próprio mod pra esse mesmo tipo de trava; não cobre a sequência interna que este item ataca.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-12 | Item criado via `/add-backlog-item`, a partir de hipótese do usuário (a sequência soltar-arma/animar-forçado/devolver-arma da cura pode ser a mesma classe de bug que já exigiu 3 rodadas de fix no FIKA) confirmada por leitura de código: `SetInHands` com callback vazio + espera por tempo fixo, e `EmergencyDrop` disparando 3 operações de mãos/inventário sem confirmação entre elas |
| 2026-09-12 | Revisão `/review-spec` — 1 gap de escopo (sequência só roda pro médico = `MainPlayer` local, nunca bots/peers remotos) + 2 corner cases adicionados (confirmação do passo de devolver arma nunca chega; extração durante cura em andamento) + 1 restrição de processo adicionada (mudança incremental/reversível em `modded-V4`, dado o histórico de fragilidade relatado pelo usuário) |
| 2026-09-12 | **Nota pós-implementação (itens 024/025, Fix 02):** o critério de aceite "`EmergencyDrop` sempre devolve a arma corretamente ao médico" (linha 32) ficou **obsoleto** — `TrySetLastEquippedWeapon()` foi removida por completo de `EmergencyDrop`/`EmergencyDropSelf` depois de causar dois bugs reais em raid (arma puxada e guardada sozinha; travamento total de mãos ao repetir o drop rápido), documentados em `mods/TRL-ImmersiveCombatMedicine/backlog/025-emergencydrop-autocirurgia/025-emergencydrop-autocirurgia-06-fix-02.md`. `EmergencyDrop` agora só solta as mãos (instantâneo), sem tentar reequipar a arma — o jogador reequipa manualmente. A confirmação de callback que este item adicionou ao `SetInHands` de `HealRoutine` continua válida e em uso; só a parte que tocava `TrySetLastEquippedWeapon` deixou de existir. |
