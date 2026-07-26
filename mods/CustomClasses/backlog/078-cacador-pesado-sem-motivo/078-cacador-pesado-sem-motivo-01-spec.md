# 078 — Caçador pesado sem motivo

**Mod:** CustomClasses
**Status:** Backlog
**Criado:** 2026-07-26

## Visão geral

O usuário relatou que, jogando de Caçador, a sensação de "peso"/lentidão de movimento aparece mesmo com uma carga total de apenas 31.7 kg — bem abaixo de qualquer limiar de sobrepeso conhecido do jogo. O único efeito de velocidade ligado à classe Caçador hoje é o drawback "Rooted" (redução de ~15% na velocidade **apenas enquanto mira**, ADS); não deveria existir nenhuma penalidade de peso ligada ao andar/correr livre para essa classe. Este item investiga se a sensação relatada é um vazamento de efeito, um indicador visual incorreto, ou uma mecânica vanilla do jogo mal-entendida.

## Comportamento atual

- Com carga leve (31.7 kg), o jogador **percebe** o Caçador como pesado/lento durante o jogo normal (não necessariamente só ao mirar).
- A classe Caçador não tem, por design, nenhum drawback vinculado ao peso da carga — o único drawback documentado é a lentidão ao mirar (Rooted).
- Não está confirmado ainda se a sensação vem de (a) velocidade de movimento realmente reduzida fora da mira, (b) um indicador de peso na tela mostrando estado de "sobrecarregado" incorretamente, ou (c) a mecânica padrão do jogo (o limiar real de sobrepeso é relativo aos atributos do personagem, não um valor fixo em kg, e pode estar mais baixo do que o esperado para este personagem).

## Comportamento desejado

- Andando ou correndo sem mirar, com carga leve (ex.: 31.7 kg), o Caçador deve se mover na mesma velocidade que qualquer outra classe sem drawback de peso — nenhuma lentidão "invisível" deve se aplicar fora da mira.
- Se houver um indicador visual de peso/carga na tela, ele deve refletir o estado real de carga do personagem (não marcar "pesado"/sobrecarregado com uma carga que está longe do limiar real de sobrepeso).
- Caso a causa seja um vazamento de efeito de outra classe (ex.: a lentidão do Tanque ou a inércia extra do Saqueador aplicando por engano no Caçador), o vazamento deve ser eliminado.
- Caso a causa seja a mecânica padrão do jogo (limiar de sobrepeso calculado pelos atributos do personagem, não um valor fixo), isso deve ser explicado ao usuário como comportamento esperado, não um bug do mod.

## Critérios de aceite

- [ ] Andando/correndo sem mirar com carga leve (31.7 kg), a velocidade de movimento do Caçador é idêntica à de uma classe sem drawback de peso, na mesma condição.
- [ ] Ao mirar (ADS), a velocidade cai o esperado pelo Rooted e volta ao normal assim que a mira é solta — a lentidão não persiste fora da mira.
- [ ] Se existir indicador visual de peso/sobrecarga, ele não aponta "pesado" com uma carga muito abaixo do limiar real de sobrepeso do personagem.
- [ ] Nenhum efeito de peso de outra classe (inércia do Saqueador, lentidão do Tanque) é observado no Caçador.
- [ ] **Fika/multiplayer:** o comportamento é o mesmo jogando como host ou como cliente da partida — a sensação de peso não depende de quem hospeda.
- [ ] **Estado entre raids:** sair e reentrar em uma nova raid não deixa nenhum resíduo de lentidão "grudado" de uma raid anterior.

## Corner cases

- [ ] Trocar de classe (editor web ou perfil novo) no meio de uma sessão — efeito de peso de uma classe anterior não deve persistir na classe nova.
- [ ] Peso oscila perto do limiar real de sobrepeso do jogo (ganhar/perder item durante a raid) — a sensação de peso deve acompanhar a mudança em tempo real, não travar em "pesado".
- [ ] Jogador entra em raid como Scav (não como o personagem PMC Caçador) — nenhum drawback de classe deveria se aplicar nesse modo.
- [ ] Interação com outros mods que mexem em velocidade/estamina/peso (ex.: mod de posturas) rodando junto — descartar interferência cruzada.

## Fora de escopo

- [ ] A definir

## Referências

- [class-design.md](../../docs/class-design.md) — definição do drawback Rooted (Caçador, −15% vel. ADS) e do desenho de 1 drawback por classe.
- Item 074 no [mod-backlog.md](../mod-backlog.md) *(auditoria de eficácia dos perks/drawbacks de velocidade — mesma família de patches de movimento; sem pasta própria, documentado inline na tabela)*.
- Memória do mod: pendências P-16.1 (validação dos fixes de movimento v0.2.4) e a regra de gate por instância (evitar vazamento entre classes/bots).

## Histórico

| Data | Evento |
|---|---|
| 2026-07-26 | Item criado via `/add-backlog-item` |
