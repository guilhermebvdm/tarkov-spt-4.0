# 013 — Refinamentos de transição de stance: arma montada e sprint

**Mod:** stancesAndCameraPositionSPT4.0.11
**Status:** Backlog
**Criado:** 2026-06-21

## Visão geral

Após validar o item 012 (controlador de stamina) em raid, o usuário levantou três refinamentos de **transição de postura** em situações especiais: (1) ao usar uma **arma montada do cenário** (metralhadora fixa), o estado interno de stamina deve ser tratado como **Mount Active**; (2) ao **entrar** numa arma montada vindo de uma stance alternativa (1/2/3), forçar automaticamente a volta para **Stance 0** para não desalinhar a arma; (3) ao **começar a correr** a partir de uma stance alternativa, a arma não deve "piscar" pela Stance 0 antes da corrida — a transição deve partir da stance atual.

## Comportamento atual

- **Arma montada (stationary):** ao operar uma metralhadora fixa do cenário, o controlador de stamina (item 012) **não reconhece** o estado como mount — resolve o cenário como "sem mount" (Stand/ADS) em vez de Mount Active. (O jogo já bloqueia nativamente stance/ADS/breath em arma montada, mas o estado interno do mod fica incorreto.)
- **Entrar em arma montada em Stance 1/2/3:** a arma pode ficar **visualmente desalinhada/bagunçada**, porque os offsets da stance alternativa permanecem enquanto o jogo coloca o jogador na arma fixa.
- **Sprint a partir de Stance 1/2/3:** ao iniciar a corrida, a arma faz um **"flash"** — passa rapidamente pela Stance 0 (offsets zerados de forma animada) antes de a animação de corrida assumir.

## Comportamento desejado

- **Arma montada → Mount Active:** enquanto o jogador opera uma arma montada do cenário, o estado interno de stamina é **Mount Active** (mesmo tratamento/benefício do mount nativo). Não é preciso cercar manualmente stance/ADS/breath nesse caso — o jogo já bloqueia; basta o estado interno estar correto.
- **Forçar Stance 0 ao entrar em arma montada:** ao **entrar** numa arma montada estando em Stance 1/2/3, o sistema troca automaticamente para **Stance 0** (limpando os offsets), evitando o desalinhamento visual. Em seguida o estado segue como Mount Active.
- **Sprint sem flash da Stance 0:** ao iniciar a corrida a partir de Stance 1/2/3, a transição parte **direto da stance atual** para a corrida — sem exibir a Stance 0 no meio. Ao terminar a corrida, o jogador retorna à stance em que estava.

## Critérios de aceite

- [ ] Ao operar uma **arma montada do cenário**, o debug de stamina (item 012) mostrar **`Active Mount`** (e os buffs de Mount Active valerem), em vez de um cenário sem-mount.
- [ ] Ao **entrar** numa arma montada estando em **Stance 1, 2 ou 3**, o sistema mudar automaticamente para **Stance 0** — a arma fica alinhada (sem o desalinhamento visual atual).
- [ ] Ao **iniciar a corrida** a partir de Stance 1/2/3, a arma **não passar visualmente pela Stance 0** antes da corrida — a transição parte da stance atual.
- [ ] Ao **terminar a corrida** iniciada de uma stance alternativa, o jogador retornar à **mesma stance** de antes (sem regressão do comportamento atual de restaurar a stance).
- [ ] **Fika/multiplayer:** os três comportamentos aplicarem-se **somente ao jogador local** — nunca a bots/peers.
- [ ] **Estado entre raids:** sair da arma montada / fim de raid não deixar estado preso (ex.: continuar "Mount Active" após largar a metralhadora, ou stance travada pós-sprint).

## Corner cases

- [ ] **Sair da arma montada** (largar a metralhadora): o estado de stamina deixar de ser Mount Active na hora; a stance volta ao normal (não fica travada em Stance 0 nem em Mount Active).
- [ ] **Sprint com arma leve (TacSprint ativo) a partir de Stance 1/2/3:** a animação de TacSprint da stance deve continuar funcionando como hoje — o fix do flash não pode quebrar o TacSprint.
- [ ] **Sprint com arma pesada/grande** (sem TacSprint): a arma não pode ficar em pose quebrada/clipando durante a corrida ao remover o "flash" — a corrida deve usar a animação de sprint nativa, não os offsets da stance alternativa.
- [ ] **Entrar em arma montada já estando em Stance 0:** nenhuma troca de stance redundante (no-op).
- [ ] **Atirar/ADS durante arma montada:** comportamento nativo do jogo prevalece (o mod não interfere além do estado interno).
- [ ] **Soltar o sprint muito rápido** (tap): não deixar a stance num estado inconsistente entre o forçar-corrida e o restaurar.
- [ ] **Detecção contínua vs. transição:** o estado Mount Active da arma montada baseia-se no estado **atual** (operando a metralhadora agora), não num flag persistente — ao sair da montada, o estado limpa na hora. A troca forçada para Stance 0 é um evento **único** na entrada (não a cada frame).

## Fora de escopo

- [ ] Alterar a animação **nativa** de sprint do EFT ou de arma montada — o mod só ajusta o que ele próprio aplica (estado de stamina, offsets de stance, troca de stance).
- [ ] Bloquear o mount ativo (bipé) em Stance 1/2/3 — fora deste item.
- [ ] Novos multiplicadores de stamina para arma montada — reusa o cenário **Active Mount** já existente (item 012).

## Referências

- [012 — Controlador central de stamina](../012-controlador-central-stamina/012-controlador-central-stamina-01-spec.md) (estado Mount Active reusado pela arma montada)

## Histórico

| Data | Evento |
|---|---|
| 2026-06-21 | Item criado via `/add-backlog-item` (feedback de validação in-game do 012) |
| 2026-06-21 | Revisão `/review-spec` — 1 corner case adicionado (detecção contínua vs. transição); ACs verificáveis confirmados |
