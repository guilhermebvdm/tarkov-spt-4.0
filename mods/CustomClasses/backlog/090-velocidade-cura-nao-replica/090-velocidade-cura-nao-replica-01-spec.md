# 090 — Velocidade da animação de cura não replica pros outros jogadores (Fika)

**Mod:** CustomClasses
**Status:** Backlog
**Criado:** 2026-09-08

## Visão geral

Quando um Médico de Combate tem a velocidade de cura acelerada por perk (Rapid Care no curativo, Swift Surgeon na cirurgia — item 072), esse ritmo mais rápido só é sentido pelo PRÓPRIO jogador que está curando. Qualquer outro jogador no raid continua vendo o gesto de cura correr no ritmo padrão/vanilla — seja o Médico se autocurando (observado por um colega de squad), seja curando um aliado pela integração do TRL-ImmersiveCombatMedicine (item 077; nesse caso o HP do paciente já chega no tempo certo, só a encenação visual diverge). As duas experiências divergem: quem cura sente/vê que terminou rápido; quem observa vê a ação se arrastar pelo tempo cheio.

## Comportamento atual

- O item 072 acelera a animação e o efeito de cura do Médico de Combate quando ele SE AUTOCURA, através de um ajuste de velocidade que só é aplicado no cliente de quem está usando o item.
- Confirmado no decompile do cliente EFT: o mecanismo nativo que ajusta essa velocidade só executa quando o jogo tem acesso ao estado de saúde "de verdade" daquele personagem — e isso só acontece no cliente que É DONO do personagem. Em qualquer outro cliente (um colega de squad observando, ou o processo de outro jogador), o mesmo personagem é representado por uma réplica sem esse acesso, então o ajuste de velocidade nunca roda ali — a animação corre sempre no ritmo padrão.
- O item 077 já resolve, para a cura de ALIADO via TRL-ImmersiveCombatMedicine, a parte que importa mecanicamente: o ganho de HP e a remoção de sangramento no paciente já chegam no tempo certo (acelerado), porque o ICM manda essa informação por um pacote de rede próprio, enviado só depois que o tempo (já acelerado) se esgota. Isso já está correto e não é o alvo deste item.
- O que o item 077 NÃO resolve é a mesma limitação do 072: o gesto visual do médico (o "usar o item") continua rodando no ritmo padrão em QUALQUER outro cliente — inclusive no cliente do próprio paciente sendo curado, e no de qualquer terceiro observador. O paciente vê o médico "trabalhando" no ritmo normal mesmo depois do seu HP já ter mudado por baixo.
- Relato do usuário (origem deste item): um jogador Médico de Combate cura um aliado (outro jogador humano) pelo ICM; o Médico sente/vê a própria animação rápida (perk ativo), mas o jogador sendo curado — e qualquer outro observador no raid — vê o gesto de cura correr no ritmo normal, bem mais devagar do que o médico percebeu. A mesma coisa acontece quando o Médico se autocura na frente de um colega de squad.

## Comportamento desejado

- Enquanto um Médico de Combate estiver com o perk de velocidade de cura ativo (Rapid Care ou Swift Surgeon) e estiver usando um item médico — em SI MESMO ou em um ALIADO via ICM —, todo mundo que observar essa ação deve ver o MESMO ritmo acelerado, não o padrão. Os dois cenários (auto-cura e cura de aliado) têm peso igual: nenhum dos dois é opcional.
- A duração real do efeito de cura em si (quando/quanto o sangramento para, quando o HP sobe) não muda por causa deste item — na auto-cura ela já é correta para o próprio médico; na cura de aliado ela já é correta pro paciente (item 077). Este item é só sobre a REPRESENTAÇÃO VISUAL da velocidade bater entre todos os clientes.
- O caso 100% vanilla (o pequeno bônus nativo da skill Cirurgia, sem nenhum perk do CustomClasses envolvido) fica fora de escopo por decisão de produto — ver "Fora de escopo".

## Critérios de aceite

- [ ] Médico com Rapid Care/Swift Surgeon ativo SE AUTOCURA: um colega de squad observando essa cena vê a animação correr no MESMO ritmo acelerado que o médico vê no próprio cliente, dentro de uma tolerância de rede pequena (ex.: <0,5s de diferença perceptível).
- [ ] Médico com o mesmo perk ativo cura um ALIADO (outro jogador humano) pelo TRL-ImmersiveCombatMedicine: o aliado sendo curado, e qualquer terceiro observador, veem o gesto correr no MESMO ritmo acelerado — mesma tolerância acima.
- [ ] Médico sem o perk ativo (classe diferente, ou perk desligado no F12): nenhuma mudança de comportamento em nenhum dos dois cenários — todo mundo continua vendo o ritmo padrão, igual a hoje.
- [ ] A duração real da cura (HP/sangramento) não muda em nenhum dos dois cenários — só a velocidade da encenação visual passa a bater entre clientes.
- [ ] **Fika/multiplayer:** obrigatório por natureza — este item existe inteiramente por causa da replicação em coop (em partida solo não há "outro jogador observando" para divergir). Cobrir os 3 papéis nos dois cenários: quem cura, quem é curado (quando aplicável) e um terceiro observador.
- [ ] **Estado entre raids:** a informação de velocidade é transmitida e consumida a cada operação de cura; qualquer registro temporário que a solução crie (ex.: mapear jogador→fator ativo para saber o que replicar) deve ser limpo ao fim de CADA cura e, por segurança, também ao fim do raid — nenhum valor de uma cura vaza pra próxima nem atravessa raids.

## Corner cases

- [ ] Rede com lag momentâneo ou pacote perdido: uma pequena janela de atraso na réplica é aceitável, mas o ritmo não pode ficar permanentemente dessincronizado a partir daí (precisa se autocorrigir na cura seguinte, não acumular).
- [ ] O perk é desligado no F12 NO MEIO de uma cura já em andamento — a réplica vista pelos outros deve acompanhar de forma consistente, sem travar num ritmo "congelado" no valor antigo.
- [ ] A cura é cancelada no meio da animação (auto-cura ou cura de aliado) — a réplica vista pelos outros jogadores deve parar/cancelar junto, não continuar tocando sozinha até o fim do tempo padrão.
- [ ] Médico troca de item/pose NO MEIO da animação (cancela e troca por outro item com fator diferente) — a réplica vista pelos outros precisa acompanhar o fator do item/ação ATUAL, não herdar o fator da tentativa anterior.
- [ ] Um observador entra no raid (ou reconecta) DEPOIS que a cura já começou — ao ver a cena pela primeira vez, já precisa estar no ritmo correto, não herdar o padrão até a próxima cura.
- [ ] TRL-ImmersiveCombatMedicine ausente ou desatualizado: o cenário de auto-cura continua funcionando normalmente (não depende do ICM); o cenário de cura de aliado simplesmente não existe sem o ICM, então não há nada pra replicar ali.
- [ ] CustomClasses e ICM COEXISTEM mas a ponte entre eles falha por algum motivo (reflection quebrada por update de um dos dois) — a replicação da velocidade deve degradar de forma segura (fail-open: ritmo padrão pra todo mundo), nunca travar a animação de ninguém.
- [ ] A solução não pode adicionar lag perceptível a nenhum outro sistema de rede do Fika (inventário, movimento, os pacotes de cura já existentes do ICM) — qualquer mensagem nova para sincronizar isso é adicional e de baixa frequência (só dispara quando uma cura com perk começa), nunca no caminho quente de outros pacotes.
- [ ] Se a solução envolver um pacote de rede novo, uma mensagem malformada ou de uma versão antiga do mod (peer desatualizado) NÃO pode derrubar a fila de eventos de rede do frame inteiro — isso já aconteceu antes neste repositório com outro pacote mal-tratado (sintoma: "jogadores patinando"/perda de movimento de todo mundo, não só de quem mandou o pacote ruim). Descartar o pacote inválido, nunca deixar uma exceção escapar do callback de recebimento.
- [ ] Dois médicos curando ao mesmo tempo em cenas separadas (cada um se autocurando, ou um curando o outro) — cada réplica deve levar o ritmo de quem está executando AQUELA ação específica, sem misturar os fatores.
- [ ] Bot como OPERADOR (curando a si mesmo ou um aliado): não se aplica — bots já são bloqueados de usar qualquer perk de classe por uma regra de gating já estabelecida no mod (item 075); não há velocidade de perk pra replicar num bot.

## Fora de escopo

- [x] Divergência residual do pequeno bônus 100% VANILLA da skill Cirurgia (sem nenhum perk do CustomClasses envolvido) — é o comportamento nativo do jogo, pequeno o suficiente pra não ter sido reportado, e corrigi-lo tocaria a skill nativa de TODO jogador (não só quem tem perk), aumentando bastante o custo/risco deste item.
  <!-- review: default aplicado por mim para destravar a spec — se quiser cobrir o caso 100% vanilla também, é só pedir. -->
- [x] Mudar a duração real do efeito de cura (quando o HP/sangramento efetivamente muda) — isso já está correto nos dois cenários; aqui é só a representação visual da velocidade.
- [x] Sincronização "frame-perfect" entre clientes — uma pequena tolerância de rede é aceitável, na mesma linha de outros efeitos visuais já replicados pelo Fika.
- [x] Bot como PACIENTE (aliado curado pelo ICM é um bot, não um jogador humano) — esse caminho tem uma limitação conhecida e SEPARADA (a cura real do bot, não só a animação, corre no tempo vanilla — achado PA-01-01 do item 077), documentada mas não reaberta por este item. Este item cobre só observadores/pacientes HUMANOS.

## Referências

- Achado confirmado no decompile do cliente EFT: o mecanismo de velocidade da animação de cura só executa no cliente dono do personagem (early-return quando o personagem é uma réplica de outro jogador).
- `mods/CustomClasses/modded/Client/Patches/ClassMedicPatches.cs` — item 072 (Rapid Care/Swift Surgeon, auto-cura).
- `mods/CustomClasses/backlog/mod-backlog.md` — itens 072 e 077 (perk de origem e a ponte já existente com o ICM).
- `mods/TRL-ImmersiveCombatMedicine/modded-V3(review)/Patches/Medical/BandAidController.cs` e `CustomClassesBridge.cs` — cenário de cura de aliado onde o usuário observou o problema; a ponte de tempo/HP entre os dois mods já existe e funciona (não é o alvo deste item).

## Histórico

| Data | Evento |
|---|---|
| 2026-09-08 | Item criado (renumerado de 087 para 090 por colisão de número no backlog) após investigação equivocada em 2 tentativas anteriores no backlog de `TRL-ImmersiveCombatMedicine` (item 022, removido de lá): 1ª tentativa assumiu paciente-BOT, 2ª tentativa não achou o mecanismo certo porque o relato era sobre o item 072 do CustomClasses (auto-cura), não sobre o protocolo de rede do ICM. Causa raiz confirmada no decompile do EFT e escopo definido com o usuário: cobrir auto-cura E o gesto visual da cura de aliado com peso igual, deixando o caso 100% vanilla fora de escopo por padrão. |
| 2026-09-08 | Revisão `/review-spec` — critério "Estado entre raids" reescrito (N/A trocado por comportamento verificável: limpar qualquer registro temporário por cura e por raid); adicionados 3 corner cases (limpeza de estado ao fim do raid já embutida acima, robustez de pacote de rede novo contra o modo de falha já visto neste repo — AP-11 "jogadores patinando", bot como operador não se aplica por causa do gating do item 075); adicionado 1 item em Fora de escopo (bot como paciente — limitação separada, PA-01-01 do 077, não reaberta aqui) |
