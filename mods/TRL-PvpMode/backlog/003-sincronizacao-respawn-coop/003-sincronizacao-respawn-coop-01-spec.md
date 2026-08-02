# 003 — Sincronização do respawn em coop

**Mod:** TRL-PvpMode
**Status:** Backlog
**Criado:** 2026-08-01
**Depende de:** [002 — Renascer em spawn aleatório](../002-renascer-spawn-aleatorio/002-renascer-spawn-aleatorio-01-spec.md)

## Visão geral

O item 002 devolve o jogador ao mapa em outro lugar. Este item garante que **todo mundo veja isso
direito** — sem o corpo atravessando o mapa em linha reta, e com as IAs sabendo onde o jogador está de
verdade.

## Comportamento atual

Quando o jogador renasce, a posição nova chega aos outros participantes pelo fluxo normal de estado do
Fika, que **interpola entre a posição anterior e a nova**. Como não existe detecção de teleporte, o corpo
alheio percorre o caminho entre os dois pontos — para um respawn do outro lado do mapa, isso é um risco
visível atravessando cenário, e por alguns instantes o jogador aparece em coordenadas onde não está.

## Comportamento desejado

1. Ao renascer, os outros participantes veem o jogador **aparecer diretamente** no ponto novo — corte
   seco, sem trajeto intermediário.
2. As IAs passam a mirar/procurar na posição nova, não na antiga.
3. O corpo caído desaparece do local da morte no mesmo instante.
4. Se o aviso de renascimento se perder na rede, o jogador ainda assim converge para a posição correta
   pelo fluxo normal — a sincronia fina é uma melhoria, não uma dependência para a partida funcionar.

## Critérios de aceite

- [ ] Com dois clientes, um renascendo do outro lado do mapa: o segundo cliente **não** vê o boneco
      percorrer o trajeto — ele some de um ponto e aparece no outro.
- [ ] Depois do respawn, atirar no jogador recém-nascido acerta (a caixa de colisão acompanhou o corpo).
- [ ] Bots que perseguiam o jogador antes da morte não continuam atacando o lugar antigo.
- [ ] Perder o aviso de rede (simulável desligando o envio) não trava nem duplica nada — só volta ao
      comportamento com deslize.
- [ ] **Fika/multiplayer:** é o objeto deste item; validar com **2+ clientes reais**, nunca só como
      anfitrião.
- [ ] **Estado entre raids:** o registro do aviso de rede sobrevive à troca de partida (o Fika recria o
      gerenciador de rede a cada sessão) e nenhum estado de rede vaza de uma raid para outra.

## Corner cases

- [ ] **O aviso chega antes de o outro cliente saber que o jogador levantou.** A ordem entre o aviso de
      posição e o de estado não é garantida; nenhum dos dois pode depender do outro ter chegado.
- [ ] **O aviso chega para um cliente que não tem o jogador na lista** (entrou depois, ou já saiu).
      Precisa ser ignorado em silêncio, sem exceção.
- [ ] **Corpo truncado ou pacote malformado.** Não pode derrubar a fila de eventos do quadro — no Fika,
      uma exceção aí **quebra a partida inteira para todos**, não só o mod.
- [ ] **Servidor headless.** O anfitrião sem tela também precisa processar o aviso, porque é ele quem
      hospeda as IAs.
- [ ] **Troca de sessão** (sair da raid e entrar em outra). O gerenciador de rede é recriado e o registro
      antigo se perde silenciosamente.

## Fora de escopo

- [ ] **Blindar contra clientes sem o mod.** Um par sem o TRL-PvpMode ainda consegue levantar um caído,
      furando a contagem de vidas. Recusar os avisos de resgate do lado da vítima é trabalho próprio —
      ver pendência no fim deste documento.
- [ ] Reconexão restaurando o estado de caído.

## Pendência derivada

**Blindagem contra clientes sem o mod.** Descoberta no review do item 001 (R-10) e delegada para cá, mas
mantida fora do escopo desta entrega por ser um mecanismo diferente (recusar avisos de resgate na
vítima, não sincronizar posição). Deve virar item próprio no backlog.

## Referências

- [Guia de rede FIKA do repo](../../../../docs/technical/fika-packet-desync-prevention-plan.md) — fonte de
  verdade para pacote próprio; AP-11

## Histórico

| Data | Evento |
|---|---|
| 2026-08-01 | Item criado via `/add-backlog-item` |
| 2026-08-01 | Spec funcional criada via `/create-spec` |
