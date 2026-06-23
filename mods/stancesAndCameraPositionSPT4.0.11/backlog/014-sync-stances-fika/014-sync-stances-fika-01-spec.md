# 014 — Corrigir sync visual de stances no Fika

**Mod:** stancesAndCameraPositionSPT4.0.11
**Status:** Backlog
**Criado:** 2026-06-22

## Visão geral

Em coop (Fika), ao trocar de postura (stance), os outros jogadores **não veem a arma acompanhar** a stance — apenas o tronco/braço gira, enquanto a arma fica imóvel ou desalinhada. Este item corrige a **aplicação remota** do sync para que o jogador observado veja a **postura completa** (braço **e** arma juntos), igual ao que o dono da postura vê de si mesmo, e garante que isso **coexista** com o lean (inclinar) e a troca de ombro nativos do jogo.

## Comportamento atual

- A troca de stance **é enviada** corretamente pela rede para os outros jogadores (o estado da postura chega ao cliente remoto).
- Mas, no jogador remoto, a postura é aplicada **no lugar errado**: gira o **tronco** do personagem, não a arma. Resultado: para os outros, o braço/tronco muda, mas a **arma permanece imóvel/desalinhada** — a leitura visual fica quebrada (parece que o jogador está "bugado").
- Isso compromete combate e movimentação tática em coop, porque a postura do outro jogador não é legível.

## Comportamento desejado

- No jogador remoto, a stance é aplicada de modo que **braço e arma se movam juntos**, reproduzindo **a mesma pose** que o dono da postura vê localmente.
- A postura remota é **aditiva** sobre a animação nativa: ela **soma** ao que o jogo já faz (lean, troca de ombro, mira), em vez de sobrescrever.
- **Lean** (inclinar esquerda/direita) e **troca de ombro** continuam funcionando normalmente e podem ser **combinados** com qualquer stance sem que um anule o outro visualmente.
- Cada jogador remoto recebe **sua própria** postura; o jogador local não é afetado pela lógica remota.

## Critérios de aceite

- [ ] Em coop, quando um jogador troca de stance, os outros veem **a arma acompanhar** a stance (não só o tronco/braço).
- [ ] A postura vista no jogador remoto **corresponde visualmente** à pose que o próprio jogador vê de si (mesma direção/ângulo de braço + arma).
- [ ] **Lean + stance:** um jogador em Low Ready (e em qualquer stance) inclinando para a esquerda/direita é visto pelos outros **com o lean E a stance** aplicados juntos, sem conflito.
- [ ] **Troca de ombro + stance:** um jogador em High Ready (ou outra stance) trocando de ombro é visto **com a troca de ombro E a stance** aplicadas, sem conflito.
- [ ] **Sequências combinadas** funcionam nos dois sentidos: stance→depois lean; lean→depois stance; trocar de ombro estando numa stance ≠ 0.
- [ ] O lean e a troca de ombro **nativos** do jogo continuam funcionando (a correção não os quebra nem os sobrescreve).
- [ ] **Fika/multiplayer:** a postura é aplicada **ao jogador remoto correto** (cada um a sua); a lógica remota **não** altera a visão/arma do jogador local.
- [ ] **Estado entre raids:** ao sair e reentrar em raid, e quando um jogador remoto sai/morre, não fica "animador órfão" aplicando postura num personagem que não existe mais; sem exceções no log.

## Corner cases

- [ ] **Jogador remoto entra/sai da raid** (spawn/despawn): a aplicação de postura começa/encerra corretamente para aquele jogador.
- [ ] **Jogador remoto morre**: a postura para de ser aplicada (corpo não fica "posando").
- [ ] **ADS + stance no remoto:** mirar combina com a stance sem travar a arma.
- [ ] **Troca rápida de stance**: a postura remota acompanha sem ficar "presa" numa stance anterior.
- [ ] **Arma guardada / sem arma de fogo** no jogador remoto: não aplicar postura de arma (sem erro).
- [ ] **Vários jogadores remotos ao mesmo tempo**: cada um com sua postura, sem misturar.
- [ ] **Transição suave**: ao trocar de stance, a arma do jogador remoto **interpola** suavemente para a nova pose (não "teleporta"), coerente com a transição vista localmente.

## Fora de escopo

- [ ] Sincronizar o **mount passivo** / ícones (item 011) — separado, não entra aqui.
- [ ] Mudar o **protocolo de rede** já existente além do necessário para a correção (o envio do estado já funciona; o foco é a aplicação remota).
- [ ] Sincronizar efeitos de stamina/recoil (item 012) entre jogadores — locais por natureza.

## Referências

- [006 — Sync visual das stances](../006-sync-visual-stances/) (implementação que este item corrige)
- `mods/StanceSync` (referência **só-local** de como lean/troca de ombro são detectados e como coexistir com o vanilla — **não** faz sync de rede)

## Histórico

| Data | Evento |
|---|---|
| 2026-06-22 | Item criado via `/add-backlog-item` (diagnóstico por 3 sub-agents: aplicação remota usa o transform do tronco em vez do da arma) |
| 2026-06-22 | Revisão `/review-spec` — 1 corner case adicionado (transição suave no remoto) |
