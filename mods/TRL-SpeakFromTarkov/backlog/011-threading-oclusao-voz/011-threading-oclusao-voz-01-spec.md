# 011 — Threading e Oclusão de Voz

**Mod:** TRL-SpeakFromTarkov
**Status:** Backlog
**Criado:** 2026-09-09

## Visão geral

Agrupa quatro achados de threading/física da auditoria Review 03 (`docs/relatorio-auditoria-codigo-03.md`): acesso a API do Unity fora da main thread dentro do callback de áudio (`AUD-03-02`), uma máscara de camadas mal formada que quebra a oclusão de voz por portas/objetos interativos (`AUD-03-03`), e duas race conditions sem sincronização entre a thread de captura de microfone e a main thread — no estado do pipeline de voz (`AUD-03-04`) e na configuração do filtro de ruído (`AUD-03-05`).

**Contexto dado pelo usuário (2026-09-09):** a separação de thread (`AUD-03-02`) foi uma otimização sugerida por outra IA (Gemini) e aprovada sem checagem técnica profunda por quem não tem esse conhecimento especializado — usuário deu autonomia total pra eu decidir a melhor abordagem daqui pra frente, sem precisar preservar a arquitetura atual de threading se ela não for a mais segura. Sobre `AUD-03-03`, o usuário confirma que pediu ao Gemini o efeito de oclusão por cômodos/portas e que a implementação não ficou precisa — a parte da correção do bitmask (`DoorLayer`/`InteractiveLayer` sem shift) trata só esse aspecto técnico; o pedido mais amplo de reverb/eco ambiental e ajuste da curva de queda de volume por distância virou o item `015` separado (ver `015-ambiente-acustico-reverb-distancia/`), por ser investigação/feature nova e não um bug de código.

## Comportamento atual

- `RemoteSpeaker.OnAudioFilterRead()` roda na audio thread do Unity (não a main thread) e, dentro dela, chama `Camera.main` e `Singleton<GameWorld>.Instance.MainPlayer` — APIs que a Unity só garante seguras na main thread. Isso pode lançar `UnityException` ou corromper o cache da tag `MainCamera`, principalmente em trocas de câmera (virar espectador, morrer).
- A máscara `_occlusionLayerMask` em `RemoteSpeaker.cs:282` e `BotVoiceBridge.cs:159` combina `LayerMaskClass.DoorLayer`/`InteractiveLayer` sem o shift de bits necessário, corrompendo a máscara — o `Physics.Linecast` de oclusão não testa corretamente contra portas/objetos interativos, então falar atrás de uma porta fechada não abafa a voz como deveria.
- `VoipProcessor` (modo de transmissão, mute, PTT, níveis de RMS) é escrito pela thread de captura de microfone e lido/escrito pela main thread (HUDs, `VoipController`, `MenuVoipHUD`) sem `volatile`/lock, podendo causar atraso perceptível no toggle de mute/PTT/modo.
- `AudioFilter` (limiares de ruído, RNNoise, AGC, limiter, corte do filtro passa-baixa) é reconfigurado pela main thread a cada frame a partir do F12, mas consumido pela thread de captura sem sincronização — pode causar descompasso momentâneo no gate de ruído ao mudar configuração durante transmissão ativa.

## Comportamento desejado

- Nenhuma API do Unity restrita à main thread (`Camera.main`, `Singleton<GameWorld>`, busca de `Transform`/`GameObject`) é chamada dentro de `OnAudioFilterRead`; os valores necessários (posição/direção do listener) são resolvidos uma vez por frame em `Update()` (main thread) e apenas lidos no callback de áudio.
- A oclusão física por porta/objeto interativo funciona de fato: falar atrás de uma porta fechada reduz o volume/aplica o abafamento configurado.
- O estado de transmissão (modo, mute, PTT, níveis) e a configuração do filtro de ruído são lidos e escritos de forma segura entre a thread de captura e a main thread, sem depender de timing para refletir corretamente.

## Critérios de aceite

- [ ] Nenhuma chamada a `Camera.main`, `Singleton<T>.Instance` ou qualquer API que acesse a cena/hierarquia do Unity ocorre dentro de `OnAudioFilterRead`.
- [ ] Falar atrás de uma porta fechada (dentro do alcance de oclusão configurado) reduz perceptivelmente o volume da voz ouvida por outro jogador, de forma consistente em testes repetidos.
- [ ] Ativar/desativar mute ou PTT reflete no estado de transmissão sem atraso perceptível, mesmo com a thread de captura processando ativamente.
- [ ] Mudar um limiar do filtro de ruído (ex.: RNNoise) no F12 durante transmissão ativa não corta nem distorce a fala em andamento — o novo valor passa a valer no próximo frame processado pela thread de captura, sem estado parcialmente atualizado.
- [ ] **Fika/multiplayer:** o comportamento é correto com múltiplos `RemoteSpeaker` simultâneos (vários jogadores falando ao mesmo tempo, cada um com sua própria checagem de oclusão e câmera/listener).
- [ ] **Estado entre raids:** os campos cacheados de listener (posição/direção) são recalculados do zero a cada raid via `Update()`, sem herdar valor obsoleto de uma raid anterior. O mesmo vale para qualquer estrutura de sincronização nova (snapshot/lock) introduzida pela correção de threading — não deve reter referência de jogador/objeto da raid anterior.

## Corner cases

- [ ] Troca de câmera (virar espectador, morrer, trocar de corpo) enquanto um `RemoteSpeaker` está tocando áudio no mesmo frame.
- [ ] Múltiplos `RemoteSpeaker` ativos simultaneamente, cada um checando oclusão contra geometria diferente no mesmo frame.
- [ ] Toggle de mute/PTT/modo exatamente no mesmo instante em que a thread de captura está no meio do processamento de um buffer de áudio.
- [ ] Jogador atravessando o vão de uma porta (transição aberto/fechado) — a oclusão não deve "piscar" de forma incoerente.
- [ ] Mudança de configuração do filtro de ruído no F12 (ex.: limiar do RNNoise) enquanto o jogador está transmitindo ativamente — não deve causar corte ou artefato audível na fala em andamento.
- [ ] Mod desativado (`EnableMod = false`) ou raid encerrada enquanto a thread de captura está no meio do processamento de um buffer — a thread deve parar de forma limpa, sem exception nem trabalho pendente vazando pra próxima raid. <!-- review: gap — a spec não verifica se o teardown atual de VoipController/MicrophoneCapturer já garante isso; confirmar na spec técnica antes de assumir que está resolvido -->

## Fora de escopo

- [ ] Efeitos de reverb/eco ambiental e ajuste da curva de atenuação por distância — movidos para o item `015-ambiente-acustico-reverb-distancia/`.

## Referências

- `docs/relatorio-auditoria-codigo-03.md` — achados `AUD-03-02`, `AUD-03-03`, `AUD-03-04`, `AUD-03-05`
- `references/eft-decompiled/Assembly-CSharp/LayerMaskClass.cs:82,125,131-132`

## Histórico

| Data | Evento |
|---|---|
| 2026-09-09 | Item criado via `/add-backlog-item` a partir dos achados AUD-03-02/03/04 da auditoria Review 03 |
| 2026-09-09 | Escopo ajustado após feedback do usuário: incluído AUD-03-05, autonomia total confirmada para a correção de threading, reverb/eco movido para o item 015 |
| 2026-09-09 | Revisão `/review-spec` — critério de aceite dedicado para AUD-03-05 adicionado (faltava), critério de estado-entre-raids ampliado para cobrir estruturas de sincronização novas, 1 corner case de teardown de thread adicionado e marcado `<!-- review -->` |
