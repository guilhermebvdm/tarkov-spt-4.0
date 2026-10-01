# 014 — Invasor de raid não vira inimigo dos bots (corrida com registro de jogador vivo)

**Mod:** FIKA
**Status:** Backlog
**Criado:** 2026-09-24

## Visão geral

Ao entrar numa raid já em andamento (invasão), um jogador pode nunca ser reconhecido pelos bots que já estavam ativos antes dele — eles simplesmente o ignoram indefinidamente, mesmo estando visível e próximo. A causa provável é uma dependência de tempo entre "o jogo terminar de reconhecer o novo jogador como vivo" e "cada grupo de bots tentar registrar esse jogador como um possível inimigo": se a tentativa de registro acontece um instante antes desse reconhecimento terminar, ela falha silenciosamente — sem erro, sem nova tentativa depois — e o jogador fica permanentemente fora da percepção daqueles bots. Reproduzido em raid real: jogador invadindo uma raid já em andamento via headless, parado por ~20 minutos sem nenhum bot reagir.

## Comportamento atual

- Pouco depois de um jogador terminar de carregar numa raid, cada grupo de bots ativo tenta registrá-lo como um possível inimigo.
- Se, nesse exato instante, o jogo ainda não tiver terminado de marcar aquele jogador como "vivo" internamente, a tentativa de registro é descartada silenciosamente — sem erro, sem aviso.
- A confirmação que o nosso próprio código de conexão verifica hoje checa só uma etapa anterior e mais genérica (se o jogador entrou na lista geral de rastreamento de personagens) — não confirma se cada grupo de bots individualmente conseguiu de fato registrá-lo como inimigo. Por isso essa falha nunca aparece nos nossos logs, nem como sucesso nem como erro.
- Bots que já estavam ativos antes do jogador entrar nunca ganham uma segunda chance de notá-lo — não existe nenhuma tentativa periódica de repetir esse registro especificamente pra esse tipo de falha.

## Comportamento desejado

- Um jogador que invade/entra numa raid já em andamento termina sempre registrado como um possível inimigo pra todo grupo de bots relevante, independente do momento exato em que isso acontece.
- Se a primeira tentativa de registro acontecer cedo demais (o jogo ainda não terminou de marcar o jogador como vivo), o sistema tenta de novo automaticamente, **por um número/tempo limitado e definido de tentativas** — não indefinidamente. Se todas as tentativas se esgotarem sem sucesso, isso fica registrado de forma visível (não mais silencioso), permitindo investigar se a causa é outra além da corrida original.
- Bots que já estavam ativos antes da chegada do jogador passam a notá-lo e podem engajá-lo normalmente, do mesmo jeito que qualquer outro inimigo válido.

## Critérios de aceite

- [ ] Um jogador que invade uma raid já em andamento é percebido e pode ser engajado pelos bots que já estavam ativos antes dele, em testes repetidos.
- [ ] Mesmo quando a tentativa inicial de registro coincide com o instante em que o jogador ainda não está totalmente reconhecido como vivo pelo jogo, o sistema tenta de novo (número/tempo limitado, não indefinido) até conseguir, em vez de desistir silenciosamente na primeira tentativa.
- [ ] Se todas as tentativas se esgotarem sem sucesso, isso gera um registro visível (ao menos em modo de diagnóstico) — nunca fica em silêncio total como hoje.
- [ ] Nenhuma falha de registro fica completamente sem rastro — pelo menos em modo de diagnóstico, é possível confirmar que uma tentativa falhou e que uma nova tentativa aconteceu depois.
- [ ] Jogadores que entram no início normal da raid (sem ser invasão) continuam funcionando exatamente como hoje, sem regressão.
- [ ] **Fika/multiplayer:** comportamento correto tanto hospedando localmente quanto via headless, com qualquer número de jogadores entrando em qualquer ordem e timing.
- [ ] **Estado entre raids:** o mecanismo de nova tentativa cria um estado pendente e de curta duração (uma tentativa agendada, aguardando confirmar sucesso). Esse estado precisa ser descartado por completo quando a raid termina (extração, morte, MIA, queda de conexão do host) — nenhuma tentativa pendente pode sobreviver pra próxima raid nem ficar presa em memória.

## Corner cases

- [ ] Vários jogadores invadindo a mesma raid quase ao mesmo tempo — cada um precisa ser registrado corretamente, sem que o atraso/nova tentativa de um mascare ou atrapalhe o registro de outro.
- [ ] Jogador que invade e sai (desconecta) muito rápido, antes do registro terminar — o mecanismo de nova tentativa não pode ficar tentando indefinidamente registrar alguém que já não está mais na raid.
- [ ] Jogador entra durante uma rajada grande de outros eventos de rede (ex.: sincronização de vários corpos/bots de uma vez, característica de raids invadidas em andamento) — esse atraso extra não pode fazer o registro do jogador falhar de vez nem demorar além do razoável.
- [ ] Um grupo de bots deixa de existir (todos os membros morrem/são removidos) entre a tentativa inicial que falhou e a nova tentativa — a nova tentativa não deve tentar registrar o jogador num grupo que já não existe mais.
- [ ] O próprio jogador morre ou é removido da raid entre a tentativa inicial e a nova tentativa — o mecanismo de retry precisa parar de tentar nesse caso, em vez de continuar indefinidamente atrás de alguém que já saiu.
- [ ] A raid inteira termina (host encerra, queda de conexão, etc.) enquanto ainda existe uma tentativa de registro pendente/agendada — essa tentativa precisa ser cancelada e descartada junto com o resto do encerramento da raid, nunca disparar depois, nem vazar pra próxima raid.

## Fora de escopo

- [ ] A definir

## Referências

- [010-join-in-progress-senha-lobby-headless/](../010-join-in-progress-senha-lobby-headless/) — cenário (invasão de raid) em que o bug foi reportado e reproduzido.
- Investigação técnica realizada nesta sessão (ainda não formalizada em documento próprio): identificado, por leitura direta do código do próprio jogo, um caminho de registro de inimigo que retorna silenciosamente sem sucesso quando o jogo ainda não reconheceu o jogador como vivo naquele instante — sem log, sem evento disparado, sem nova tentativa. Confirmado que esse caminho é do próprio jogo (não é comportamento introduzido por nenhum dos nossos mods), mas a janela de corrida fica mais exposta especificamente no fluxo de invasão tardia. **Diagnóstico ainda não confirmado 100% em log real de raid** — baseado em leitura de código; precisa de instrumentação de diagnóstico dedicada antes da spec técnica pra fechar com certeza.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-24 | Item criado via `/add-backlog-item`, a partir de investigação de bug relatado em raid real (jogador invadindo raid em andamento nunca é percebido pelos bots) — causa provável identificada por leitura direta do código do jogo nesta sessão |
| 2026-09-24 | Revisão `/review-spec` — 2 gaps corrigidos ("tenta até conseguir" sem limite virou critério mensurável com teto de tentativas + log ao esgotar; "Estado entre raids" era N/A frágil, reescrito reconhecendo o estado pendente da nova tentativa e exigindo descarte no fim de raid) + 1 corner case adicionado (raid termina com tentativa de registro ainda pendente) |
