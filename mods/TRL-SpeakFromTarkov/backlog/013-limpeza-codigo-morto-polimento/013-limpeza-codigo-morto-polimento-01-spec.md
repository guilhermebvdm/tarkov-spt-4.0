# 013 — Limpeza de Código Morto e Polimento

**Mod:** TRL-SpeakFromTarkov
**Status:** Backlog
**Criado:** 2026-09-09

## Visão geral

Agrupa nove achados de baixa/média severidade da auditoria Review 03 (`docs/relatorio-auditoria-codigo-03.md`): um `AudioClip` runtime nunca destruído (`AUD-03-09`), timing incorreto de um patch Harmony em método `async` (`AUD-03-11`), um campo morto com comentário enganoso sobre o modelo de threading (`AUD-03-12`), um gap de lifecycle entre raids no HUD in-raid (`AUD-03-13`), catches silenciosos que escondem falhas de rede (`AUD-03-14`), dois patches Harmony que nunca são exercitados (`AUD-03-15`), uma alocação mínima por frame (`AUD-03-16`), metadados de assembly desatualizados (`AUD-03-17`) e um finalizer que suprime toda exceção de um método de UI genérico (`AUD-03-18`). Nenhum é bloqueador — o objetivo é fechar dívida técnica de baixo risco num único ciclo. Usuário confirmou concordância com as correções sugeridas para todos estes (2026-09-09).

## Comportamento atual

- `RemoteSpeaker.Initialize()` cria um `AudioClip` runtime que nunca é destruído explicitamente em `OnDestroy()`, acumulando lixo de memória (~192KB por jogador remoto) entre raids.
- `GameSessionPatcher.cs` tem um Postfix Harmony em `EFT.Player.Init` (método `async Task`) que dispara antes da lógica de inicialização completar de fato, por causa de como Harmony trata métodos assíncronos.
- `RemoteSpeaker.cs:41` declara `packetQueue` (`ConcurrentQueue<byte[]>`), nunca usado; o comentário adjacente descreve um modelo de "decodificação off-thread" que não corresponde à realidade (a recepção roda na main thread).
- `InRaidVoipHUD.cs` captura `_originalPosCaptured`/`_originalStanceAnchoredPos` uma única vez e nunca reseta esse estado quando o `BattleStancePanel` some entre raids.
- `SftNetwork.cs:338` e `MenuVoipHUD.cs:192` têm `catch { }` vazios em requisições HTTP, sem nenhum log — falhas de rede ficam invisíveis.
- `GameSessionPatcher.cs:76-104` mantém dois patches Harmony (`FikaVoipSendPatch`/`FikaVoipReceivePatch`) cujo alvo nunca é instanciado quando o mod está ativo.
- `InRaidVoipHUD.cs:273` aloca `new Vector3[4]` a cada `OnGUI` Repaint.
- `Properties/AssemblyInfo.cs` ainda identifica o projeto como `VoipUnlimited` (nome antigo) com versão travada em `1.0.0.0`.
- `GameSessionPatcher.cs` (`BoundSlotViewRefreshSelectViewPatch`) suprime incondicionalmente qualquer exceção de `BoundSlotView.RefreshSelectView`, método usado por toda a UI de drag-and-drop do jogo, não só pelo fluxo que motivou o patch.

## Comportamento desejado

- O `AudioClip` runtime do `RemoteSpeaker` é destruído explicitamente junto com o restante dos recursos do speaker.
- O Postfix em `EFT.Player.Init` passa a disparar depois da inicialização lógica de fato completar (ou a lógica é movida para um evento pós-init genuíno).
- Código morto (`packetQueue`, patches nunca exercitados) é removido ou documentado como redundância intencional.
- `InRaidVoipHUD` reseta corretamente o estado de posição capturada entre raids.
- Falhas de requisição HTTP nos canais de menu são logadas via `LogErrorThrottled` (já existente no mod), em vez de silenciadas.
- A alocação por frame em `InRaidVoipHUD.cs:273` é eliminada.
- Metadados do assembly refletem a identidade e versão atuais do mod.

## Critérios de aceite

- [ ] O `AudioClip` runtime criado por `RemoteSpeaker` é destruído (`Destroy()`) no `OnDestroy()` do componente.
- [ ] O Postfix Harmony em `EFT.Player.Init` só executa lógica que depende de estado pós-inicialização depois que esse estado de fato existe (via `MethodType.Async` ou evento pós-init).
- [ ] O finalizer de `BoundSlotView.RefreshSelectView` deixa de suprimir incondicionalmente qualquer exceção — só suprime o tipo específico que motivou o patch originalmente, relançando qualquer outra.
- [ ] `packetQueue` é removido de `RemoteSpeaker.cs` e o comentário sobre o modelo de threading é corrigido para descrever a realidade (decode síncrono na main thread via `PollEvents`).
- [ ] `_originalPosCaptured` é resetado corretamente quando o `BattleStancePanel` é perdido entre raids, sem reter a posição-base de uma raid anterior.
- [ ] Uma falha na requisição HTTP de `sft/channels/announce` ou `sft/channels/list` aparece no log do BepInEx (via `LogErrorThrottled`), em vez de ser engolida silenciosamente.
- [ ] `FikaVoipSendPatch`/`FikaVoipReceivePatch` são removidos (ou, se mantidos por precaução, documentados explicitamente como redundância defensiva intencional).
- [ ] `InRaidVoipHUD.cs:273` não aloca mais um novo array por `OnGUI` Repaint.
- [ ] `AssemblyInfo.cs` reflete `TRL-SpeakFromTarkov` como nome do produto e a versão do plugin atual.
- [ ] **Fika/multiplayer:** o Postfix corrigido em `EFT.Player.Init` continua disparando corretamente para o jogador local (`IsYourPlayer`) e não introduz efeito colateral pros objetos `Player` de jogadores remotos/observados, já que esse patch roda pra toda instância de `Player` criada na sessão coop, não só a local. <!-- review: N/A original era frágil demais para um patch que roda em todo Player.Init da sessão — confirmar isso explicitamente na spec técnica em vez de assumir "limpeza local" -->
- [ ] **Estado entre raids:** o reset de `_originalPosCaptured` é a mudança principal com efeito nesse eixo; a destruição do `AudioClip` do `RemoteSpeaker` também tem efeito acumulativo entre raids (é uma correção de vazamento raid-a-raid, não só um ajuste local de uma raid) — validar ambos.

## Corner cases

- [ ] `BattleStancePanel` com posição-base diferente entre raids (resolução/HUD scale mudou no meio da sessão) — o reset deve capturar a nova posição corretamente, não travar na antiga nem na primeira captura.
- [ ] Servidor SPT fora do ar durante o teste do log de erro HTTP — confirmar que o log aparece uma vez e é throttled nas tentativas subsequentes (mesmo padrão já usado em `LogErrorThrottled`).
- [ ] Remover os patches mortos não deve alterar o comportamento observável do mod com `EnableMod=true` ou `EnableMod=false` (dado que eles já são no-op nos dois estados).
- [ ] Uma exceção real e não relacionada ao propósito original do patch (ex.: um bug de outro mod) ocorrendo dentro de `BoundSlotView.RefreshSelectView` deve voltar a aparecer no log depois da correção do finalizer, em vez de ser silenciada.
- [ ] Múltiplos `RemoteSpeaker` sendo destruídos em sequência rápida (fim de raid com vários jogadores remotos) não deve gerar erro ao destruir os `AudioClip` correspondentes.

## Fora de escopo

- [ ] Revisão de padrões similares aos achados em partes do mod não citadas na auditoria Review 03 — este item corrige só o que foi apontado, não faz uma varredura nova.
- [ ] Processo automatizado de sincronizar `AssemblyVersion`/`AssemblyFileVersion` a cada release futura — este item só corrige o valor atual; manter sincronizado nas próximas versões é responsabilidade do fluxo normal de bump de versão.

## Referências

- `docs/relatorio-auditoria-codigo-03.md` — achados `AUD-03-09`, `AUD-03-11`, `AUD-03-12` a `AUD-03-18`

## Histórico

| Data | Evento |
|---|---|
| 2026-09-09 | Item criado via `/add-backlog-item` a partir dos achados AUD-03-12 a 17 da auditoria Review 03 |
| 2026-09-09 | Escopo ampliado após feedback do usuário: incluídos AUD-03-09 (AudioClip), AUD-03-11 (timing async) e AUD-03-18 (finalizer) — concordância confirmada para todos |
| 2026-09-09 | Revisão `/review-spec` — critério Fika/multiplayer (estava N/A frágil) reescrito e marcado `<!-- review -->`, critério de estado-entre-raids ampliado para incluir o AudioClip, "Fora de escopo" preenchido |
