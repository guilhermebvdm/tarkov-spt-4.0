# 012 — GC Pressure nas Telas de VOIP

**Mod:** TRL-SpeakFromTarkov
**Status:** Backlog
**Criado:** 2026-09-09

## Visão geral

Agrupa dois achados de performance de UI da auditoria Review 03 (`docs/relatorio-auditoria-codigo-03.md`) que não foram cobertos pela correção equivalente já aplicada em `InRaidVoipHUD`/`VoipHUD` (Review 02): alocação de `GUIStyle`/`List` a cada `OnGUI()` em três telas (`AUD-03-06`) e escrita síncrona em disco a cada movimento de slider (`AUD-03-07`).

**Nota sobre `AUD-03-08` (2026-09-09):** o usuário avaliou o risco de crescimento sem limite do JSON de volume por jogador e concluiu que não é um problema real na escala de uso dele (dezenas a ~100 jogadores distintos ao longo da vida do mod, arquivo leve, sem risco de performance). Achado fechado como **aceito como está** — não faz parte deste item.

## Comportamento atual

- `PlayerVolumeMixerHUD.cs`, `VoiceCalibrationHUD.cs` e `MenuVoipHUD.cs` alocam novos `GUIStyle` (e, no caso do `MenuVoipHUD`, novas `List<T>` via `.ToList()`) a cada chamada de `OnGUI()` — que roda 2-4 vezes por frame enquanto a tela está visível. `MenuVoipHUD` é o caso mais grave porque fica ativo continuamente no menu principal, não é um modal ocasional.
- `PlayerVolumeMixerHUD.SetPlayerVolume()` faz `File.WriteAllText` síncrono na main thread toda vez que o slider de volume muda mais de `0.01`, o que pode disparar várias escritas em disco por frame durante o arraste (inclusive dentro de raid, atalho Alt+P).

## Comportamento desejado

- As três telas constroem seus `GUIStyle` uma única vez (cacheados em campos, inicializados em `Awake()`/`Initialize()`), no mesmo padrão já usado em `InRaidVoipHUD`/`VoipHUD`. `MenuVoipHUD` para de alocar `List<T>` nova a cada `OnGUI()`.
- A persistência do volume em disco é feita de forma debounced (ao soltar o slider ou por throttle), não a cada pixel de movimento.

## Critérios de aceite

- [ ] Abrir e manter aberto o mixer de volume, o wizard de calibração ou o HUD do menu principal não produz alocação perceptível de `GUIStyle`/`List` por frame (verificável via profiler ou contagem de alocações antes/depois).
- [ ] Arrastar o slider de volume de um jogador não produz mais de 1 escrita em disco por segundo (ou apenas ao soltar o slider).
- [ ] **Fika/multiplayer:** o comportamento de volume por jogador continua correto (cada cliente mantém sua própria preferência de volume por `profileId`) após o debounce ser aplicado.
- [ ] **Estado entre raids:** N/A — o debounce de escrita não altera o que é persistido, só quando.

## Corner cases

- [ ] Mixer aberto durante uma raid ativa (Alt+P) — a redução de I/O síncrono não deve introduzir atraso perceptível ao aplicar o novo volume no áudio em tempo real.
- [ ] Arquivo de volume corrompido ou parcialmente escrito (interrupção no meio de uma escrita) — o load não deve quebrar a tela.
- [ ] `MenuVoipHUD` com o canal de menu tendo muitos membros entrando/saindo rapidamente — cache de lista não deve exibir membros desatualizados.
- [ ] Jogo fechado abruptamente (Alt+F4, crash) ou raid encerrada bem no meio do arraste do slider, antes do debounce disparar a escrita — o último valor ajustado deve ser persistido de qualquer forma, não perdido silenciosamente. <!-- review: gap — a spec não define um flush de segurança (ex.: no OnDisable/OnApplicationQuit) para o valor pendente; decidir na spec técnica -->

## Fora de escopo

- [ ] Mudança na lógica de cálculo de volume/atenuação em si — só a forma como a UI aloca objetos e persiste dados.
- [ ] Política de limpeza do JSON de volume por jogador (`AUD-03-08`) — avaliado pelo usuário e aceito como está, sem risco real na escala de uso do mod.

## Referências

- `docs/relatorio-auditoria-codigo-03.md` — achados `AUD-03-06`, `AUD-03-07`
- Correção equivalente já aplicada: `docs/relatorio-auditoria-codigo-02.md` — achado `AUD-02-01`

## Histórico

| Data | Evento |
|---|---|
| 2026-09-09 | Item criado via `/add-backlog-item` a partir dos achados AUD-03-06/07/08 da auditoria Review 03 |
| 2026-09-09 | AUD-03-08 removido do escopo — usuário avaliou e não vê risco real de crescimento do JSON na escala de uso dele |
| 2026-09-09 | Revisão `/review-spec` — 1 corner case de flush de segurança no fechamento abrupto adicionado e marcado `<!-- review -->`, "Fora de escopo" reescrito para ser afirmativo |
