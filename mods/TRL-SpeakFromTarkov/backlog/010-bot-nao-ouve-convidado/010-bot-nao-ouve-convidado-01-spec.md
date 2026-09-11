# 010 — Bot Não Ouve Convidado

**Mod:** TRL-SpeakFromTarkov
**Status:** Backlog
**Criado:** 2026-09-09

## Visão geral

Bots não reagem à voz de jogadores **convidados** (quem não é o host da partida) do jeito que reagem à voz do host — sintoma relatado pelo usuário e investigado em 2026-09-09. A hipótese de causa raiz mais provável (levantada pelo usuário, com evidência de suporte encontrada em `references/fika-plugin/`): o mod chama `Singleton<IBotGame>.Instance.BotsController`/`Singleton<BotEventHandler>.Instance` **localmente**, na máquina de quem fala. No FIKA, há uma separação clara entre `HostGameController`/`FikaServer` (que possuem `BotStateManager`, responsável por simular e transmitir o estado dos bots) e `ClientGameController` (usado por convidados, sem nenhuma lógica própria de `BotsController`/spawn de bot) — forte indício de que os bots são simulados de forma autoritativa só no host, e convidados só recebem posição/estado já processado. Se for esse o caso, quando um **convidado** fala, o código está notificando uma cópia não-autoritativa do bot (ou uma lista vazia) — o bot "de verdade", que todo mundo observa, nunca recebe o estímulo. Quando o **host** fala, funciona, porque ele tem a cópia autoritativa local. Esta hipótese precisa ser **confirmada com um teste direcionado** (comparar o comportamento com o host falando vs. um convidado falando) antes de fechar a spec técnica. Se confirmada, a correção exige que o convidado avise o host (via pacote de rede) que atingiu o volume de fala necessário, com a potência/posição calculadas, para que o **host** aplique `PlaySound`/`ForceBotResponsesInRadius` usando seu próprio `BotsController` autoritativo.

O item também inclui uma regressão relacionada, encontrada na auditoria técnica Review 03 (`docs/relatorio-auditoria-codigo-03.md`, achado `AUD-03-01`): um commit não documentado (início de setembro/2026, sem entrada em `memory/sessions.md`) trocou o tipo de som usado para notificar bots sobre a voz do jogador de `AISoundType.step` para `AISoundType.gun` em `Audio/BotVoiceBridge.cs`. Isso faz os bots reagirem à fala do jogador como se tivessem sido alvejados por um tiro real, entrando em pânico e procurando cobertura sem nenhum combate ocorrendo. Inclui também `AUD-03-10` (chamada `SayPhrase()` sem efeito comprovado na IA vanilla), por ser parte do mesmo mecanismo de notificação de voz para bots.

**Contexto dado pelo usuário (2026-09-09) sobre a origem do `AISoundType.gun`:** a troca para `.gun` foi uma tentativa de corrigir o mesmo sintoma de fundo (bot parecendo não reagir à voz) — mas mirando na hipótese errada. O usuário associava o problema ao `Bot Speech Debug Volume` (seção Debug do F12, padrão 0%, opção usada pra não ouvir a fala forçada do próprio personagem enquanto usa o microfone) estar zerado. Pela leitura do código, essa hipótese de volume não se sustenta: o mod já tem, no mesmo arquivo, um mecanismo separado e incondicional (`ForceBotResponsesInRadius`, linhas 146-233) que força bots dentro do raio calculado a reagir/responder **independente de `Bot Speech Debug Volume` e independente do `AISoundType`** usado no sensor nativo — a hipótese de autoridade host/convidado (acima) explica o sintoma de forma muito mais completa.

## Comportamento atual

Quando `EnableBotInteraction` está ativo (padrão `true`) e o jogador fala perto de um bot hostil, `BotVoiceBridge.cs:108` chama `Singleton<BotEventHandler>.Instance.PlaySound(player, soundPos, power, AISoundType.gun)`. O sensor de audição nativo do bot (`BotHearingSensor`) trata `AISoundType.gun` como disparo de arma real: pula a checagem probabilística normal de distância, marca `Memory.Spotted(byHit:false)` e, dentro do raio `BULLET_FEEL_DIST`, chama `Memory.SetPanicPoint(...)` e `Memory.SetUnderFire(enemy)`, podendo disparar `BotTalk.Say(EPhraseTrigger.SniperPhrase)`. O bot entra em modo de pânico de combate só por ouvir o jogador falar. Separadamente, `SayPhrase()` (linha 101) notifica um evento de frase que o handler nativo (`BotReceiver`) ignora para os triggers usados aqui — não tem efeito comprovado na IA vanilla.

## Comportamento desejado

A voz do jogador deve notificar os bots como um som de proximidade comum (equivalente a um passo), não como um disparo de arma — sem acionar o ramo de pânico/combate (`SetPanicPoint`/`SetUnderFire`) que só faz sentido para som de tiro real. **Ao mesmo tempo**, a garantia de que o bot sempre percebe e reage à fala do jogador — mesmo com `Bot Speech Debug Volume` em 0% (jogador não ouvindo a própria fala forçada) — precisa continuar valendo, e precisa valer **igualmente para o host e para qualquer convidado da squad**. Se a hipótese de autoridade host/convidado se confirmar, a notificação ao sistema de bots deve ser sempre processada pelo host (via mensagem de rede do convidado avisando volume/posição, quando quem fala não é o host), em vez de cada cliente tentar notificar sua própria cópia local dos bots. `SayPhrase()` deve ser removida ou documentada como best-effort sem depender dela (ver também `AUD-03-10`).

## Critérios de aceite

- [ ] Bots dentro do raio de detecção de voz do jogador reagem percebendo a presença (mesmo comportamento de detecção por som de passo), sem entrar em `SetUnderFire`/`SetPanicPoint` apenas por ouvir o jogador falar.
- [ ] Bots que já estavam em combate ativo não têm o estado de alerta artificialmente escalado só por ouvir a voz do jogador durante o combate.
- [ ] O parâmetro de som usado em `BotVoiceBridge.cs:108` volta a ser `AISoundType.step` (ou equivalente não-letal), revertendo a regressão.
- [ ] **Verificação em teste real (obrigatória antes de fechar o item):** com `Bot Speech Debug Volume` em 0%, falar perto de um bot dentro do raio calculado continua fazendo o bot reagir/responder de forma perceptível, confirmando que a garantia independe tanto do volume local quanto do `AISoundType`. Se isso falhar mesmo com `ForceBotResponsesInRadius` presente no código, é um segundo bug a investigar na spec técnica — não presumir resolvido sem esse teste.
- [ ] `SayPhrase()` (achado `AUD-03-10`) é removida ou documentada como best-effort sem depender dela para IA vanilla.
- [ ] **Teste direcionado host vs. convidado (obrigatório, decide o desenho da spec técnica):** comparar o comportamento do bot ao ouvir o host falar vs. ao ouvir um convidado (não-host) falar, no mesmo raio e condição. Se houver diferença, confirma a hipótese de autoridade host/convidado — a spec técnica deve desenhar o envio de um aviso do convidado pro host (posição + potência calculada) para que o host aplique a notificação usando seu `BotsController` autoritativo.
- [ ] **Fika/multiplayer:** um convidado falando deve produzir a mesma reação de bot que o host falando na mesma posição/distância — este é o critério central deste item, não um adicional.
- [ ] **Estado entre raids:** N/A — nenhuma mudança introduz estado persistente entre raids.

## Corner cases

- [ ] Jogador falando muito perto de múltiplos bots simultaneamente — nenhum deles deve entrar em `SetPanicPoint`/`SetUnderFire` só por isso.
- [ ] Bot no limite do raio de detecção de som — o comportamento de detecção por voz deve ser consistente com o de outros sons "step" já existentes no jogo, sem alcance artificialmente maior herdado do tratamento de tiro.
- [ ] Bot já ciente da posição do jogador por outro meio (visual, outro som) — a fala não deve reforçar indevidamente esse estado como se fosse um tiro adicional.
- [ ] `Output Volume` (seção General, mínimo 0.1, afeta o que **outros jogadores reais** ouvem de você) não deve ser confundido com `Bot Speech Debug Volume` — confirmar que a correção não depende acidentalmente do primeiro.
- [ ] Convidado falando enquanto o host está temporariamente sem conexão/latência alta — a mensagem de aviso ao host não deve travar o cliente do convidado nem acumular fila indefinidamente se atrasar ou se perder.
- [ ] Vários convidados falando ao mesmo tempo — o host precisa processar cada aviso independentemente, sem um sobrescrever o outro.
- [ ] Sessão sem convidados (host jogando solo com bots) — o comportamento não deve mudar em nada, já que hoje já funciona nesse caso.
- [ ] Convidado com `EnableBotInteraction` desligado localmente, mas host com a opção ligada (ou vice-versa) — qual configuração vale? <!-- review: gap — a spec não define se é a config de quem fala ou a do host que decide se a notificação ocorre; decisão precisa entrar na spec técnica -->
- [ ] Convidado reportando uma potência (raio de detecção) ou posição visivelmente incompatível com o que o host já sabe sobre aquele jogador (spoofing, bug de cliente, ou apenas latência normal de rede) — o host deve ter algum critério pra aceitar/ajustar/rejeitar esse dado antes de aplicar ao bot. <!-- review: gap de segurança/robustez — a spec não define se o host valida ou usa sua própria posição autoritativa do jogador em vez de confiar no valor reportado pelo convidado; decisão precisa entrar na spec técnica -->

## Fora de escopo

- [ ] Qualquer ajuste de raio/potência de detecção de voz além de trocar o tipo de som e garantir a independência do volume local.
- [ ] Mudanças na forma como o `BotStateManager` do FIKA sincroniza posição/animação dos bots (isso é internals do FIKA, fora do controle do mod).

## Referências

- `docs/relatorio-auditoria-codigo-03.md` — achados `AUD-03-01`, `AUD-03-10`
- `references/eft-decompiled/Assembly-CSharp/BotHearingSensor.cs:90-117,150-174`
- `Audio/BotVoiceBridge.cs:146-233` (`ForceBotResponsesInRadius`, mecanismo já existente de resposta 100% garantida)
- `references/fika-plugin/Fika.Core/Main/Components/BotStateManager.cs` (só referencia `HostGameController`/`FikaServer`, evidência de bots autoritativos no host)
- `references/fika-plugin/Fika.Core/Main/GameMode/ClientGameController.cs` (sem lógica própria de `BotsController`, usado por convidados)

## Histórico

| Data | Evento |
|---|---|
| 2026-09-09 | Item criado via `/add-backlog-item` a partir do achado AUD-03-01 da auditoria Review 03 |
| 2026-09-09 | Escopo reformulado após feedback do usuário: revert de AISoundType já não resolve o problema real relatado (bot não reage à voz de convidados); hipótese de autoridade host/convidado levantada pelo usuário e sustentada por evidência em FIKA, tornou-se o centro deste item |
| 2026-09-09 | Revisão `/review-spec` — 2 corner cases adicionados (config divergente host/convidado; validação de potência/posição reportada), 2 marcados `<!-- review -->` para decisão na spec técnica, "Fora de escopo" reescrito para ser afirmativo em vez de "A definir" |
