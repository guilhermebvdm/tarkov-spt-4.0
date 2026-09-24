# 009 — Arma em Mãos Absorve Tiro e Perde Durabilidade (Como Placa Balística)

**Mod:** VisceralCombat
**Status:** Backlog
**Criado:** 2026-09-21

## Visão geral

Colete e capacete já absorvem parte do dano de um tiro quando o personagem está vivo, perdendo durabilidade em troca (mecânica já existente no jogo, não implementada por este mod). A arma segurada nas mãos não tem nenhum comportamento equivalente — ela só perde durabilidade por uso (disparos, falta de limpeza), nunca por ser atingida por um tiro inimigo, mesmo quando o tiro fisicamente "deveria" ter acertado a arma em vez do corpo. Este item propõe dar à arma em mãos um comportamento parecido com uma placa balística: quando um tiro acerta a arma, ela absorve parte ou todo o impacto (o corpo recebe menos dano ou nenhum) e perde durabilidade proporcionalmente.

## Comportamento atual

- Colete e capacete absorvem dano de tiro e perdem durabilidade quando o personagem está vivo — comportamento já existente, não é deste mod.
- A arma em mãos não tem esse comportamento: um tiro que atinge a região onde a arma está (braço, mão) é resolvido inteiramente como dano ao corpo, sem nenhuma interação com a arma.
- A durabilidade da arma só é afetada por uso (cada disparo desgasta um pouco) e por manutenção (limpeza/kit de reparo) — nunca por ser alvo de um tiro inimigo.

## Comportamento desejado

- Um tiro que acertaria a arma em mãos (em vez do corpo) passa a poder ser absorvido por ela, reduzindo a durabilidade da arma proporcionalmente ao impacto do tiro.
- Quando a arma absorve o tiro, o corpo do personagem recebe menos dano desse tiro específico (ou nenhum, dependendo de como a absorção for calibrada) — mesma lógica de "a placa segurou o impacto" já usada pra colete/capacete.
- A perda de durabilidade por esse mecanismo se soma à perda por uso normal (mesmo valor de durabilidade do item, não um contador separado).
- Uma arma com durabilidade muito baixa ou zerada continua funcionando de forma segura (sem travar o jogo, sem quebrar animação) — mesmo comportamento que already existe hoje quando a durabilidade zera por uso.
- Jogador sem arma em mãos no momento do tiro (mãos vazias, faca, trocando de arma) não tem nada pra absorver o impacto — tiro é resolvido normalmente, como hoje.

## Critérios de aceite

- [ ] Um tiro que acerta a região da arma em mãos reduz a durabilidade dela, numa proporção relacionada à força/dano do tiro (tiro mais forte desgasta mais que um mais fraco).
- [ ] Quando a arma absorve o tiro, o dano que o corpo do personagem recebe desse tiro específico é reduzido (ou eliminado) em relação ao que seria sem a arma no caminho.
- [ ] Uma arma com durabilidade zerada ou muito baixa continua podendo ser usada normalmente (disparar, trocar, largar) sem erro ou trava.
- [ ] A perda de durabilidade por absorção de tiro é visível no mesmo lugar que a perda por uso (inventário/inspeção do item) — não é um contador escondido separado.
- [ ] **Fika/multiplayer:** outros jogadores da sala veem a durabilidade reduzida da arma do personagem atingido, através da sincronização de item que o próprio jogo já faz pra perda de durabilidade por uso — este item não introduz nenhuma sincronização de rede nova.
- [ ] **Estado entre raids:** a durabilidade perdida por este mecanismo persiste igual à perda por uso normal — acompanha o item pra fora da raid, mesma regra que já vale hoje pra degradação por disparo.

## Corner cases

- [ ] Jogador sem arma em mãos (mãos vazias, faca, ou trocando de arma bem no momento do tiro) — sem arma pra absorver, o tiro segue o comportamento normal de dano ao corpo.
- [ ] Arma já com durabilidade zerada/crítica no momento de absorver um novo tiro — não pode ficar negativa indefinidamente nem travar; precisa de um piso definido (ex.: zero é o mínimo).
- [ ] Múltiplos projéteis num único disparo (ex.: espingarda com vários chumbos) ou rajada automática muito rápida — cada projétil precisa resolver a absorção de forma independente e consistente, sem dobrar/multiplicar efeitos por engano.
- [ ] Interação com o mecanismo de chance de desmembramento por calibre/parte do corpo já existente neste mod (item `004`) — se a arma absorveu o tiro, esse tiro específico não deveria contar como um acerto de braço/corpo pra fins de cálculo de desmembramento.
- [ ] Bots (inimigos/aliados controlados por IA) também têm arma em mãos o tempo todo — o comportamento deve valer pra eles também, ou só pro jogador humano? <!-- review: decisão de escopo pendente -->
- [ ] Colete/placa balística hoje tem uma combinação de bloqueio parcial (redução de dano) e chance de penetração total dependendo do calibre/classe de proteção — a arma deveria seguir uma lógica semelhante (nem todo tiro é 100% absorvido), ou é sempre "se acertar a arma, absorve tudo"? <!-- review: decisão de design pendente -->

## Fora de escopo

- [x] Adicionar um colisor físico 3D dedicado na arma em mãos — descartado explicitamente pelo usuário (risco de auto-acerto no próprio cano, interferência em IK/animação, interferência em checagens de linha-de-visão de bots). A spec técnica deve investigar se dá pra reusar o mecanismo lógico que já resolve absorção de colete/capacete, sem precisar de um colisor novo.
- [x] Alterar como colete/capacete já absorvem dano hoje — esse comportamento já existe e não é tocado por este item.

## Referências

- Discussão desta sessão, motivada pela pergunta do usuário sobre por que colete/capacete absorvem tiro mas a arma não. Ver `mods/VisceralCombat/memory/sessions.md`, entrada mais recente.
- Tema relacionado, mas conceitualmente distinto, dos itens `006`/`007`/`008` (física de itens largados no chão) — este item é sobre dano/durabilidade em combate com o personagem **vivo**, não sobre física de item já largado.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-21 | Item criado via `/add-backlog-item` |
