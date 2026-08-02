# 002 — Renascer em spawn aleatório · Spec Técnica

**Mod:** TRL-PvpMode
**Spec funcional:** [002-renascer-spawn-aleatorio-01-spec.md](002-renascer-spawn-aleatorio-01-spec.md)
**Criado:** 2026-08-01

## 1. Estratégia

Cinco passos, **nesta ordem**, disparados quando o jogador completa o segurar da tecla:

```
consumir vida → teleportar → religar o boneco → curar → proteger
```

A ordem não é arbitrária:

- **Teleportar antes de religar.** Durante o estado de caído o componente `ObservedPlayer` está
  desabilitado nos outros clientes ([ReviveInteractable.cs:100](../../../../references/fika-plugin/Fika.Core/Main/Components/ReviveInteractable.cs#L100)).
  Religar primeiro faria o corpo reaparecer na posição antiga e só depois deslizar até a nova.
- **Curar depois de religar.** `ToggleDowned(false)` faz `IsAlive = true`
  ([FikaPlayer.cs:651](../../../../references/fika-plugin/Fika.Core/Main/Players/FikaPlayer.cs#L651));
  restaurar vida antes disso opera sobre um controlador que se considera morto.
- **Proteger por último.** `ToggleDowned(false)` faz `SetDamageCoeff(1f)`
  ([:648](../../../../references/fika-plugin/Fika.Core/Main/Players/FikaPlayer.cs#L648)) — aplicar a
  invulnerabilidade antes seria sobrescrito por ele.

**Leitura de input própria, não a do Fika.** O componente `Bleedout` sai de `Update` antes de
`CheckForKeys()` quando o prazo é zero ([Bleedout.cs:79-86](../../../../references/fika-plugin/Fika.Core/Main/Components/Bleedout.cs#L79-L86)),
então pendurar a tecla nele quebraria justamente a configuração "sem limite" (item 001, R-05). Lemos
o teclado num postfix de `Player.UpdateTick`, filtrado por `IsYourPlayer`.

## 2. Pontos de patch

| Alvo | Tipo | Motivo |
|---|---|---|
| [`Player.UpdateTick()`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs) | Postfix | Lê a tecla de renascer enquanto o jogador está caído e conta o tempo de pressão. **Roda para todo jogador e bot do mapa** — o filtro `IsYourPlayer` é a primeira linha, e todo o resto fica atrás dele (AP-02) |

Nenhum outro patch. O respawn em si usa API pública:

| Chamada | Onde |
|---|---|
| `ISpawnSystem.SelectSpawnPoint(ESpawnCategory.Player, side, …)` | `Singleton<IFikaGame>.Instance.GameController.SpawnSystem` ([BaseGameController.cs:116](../../../../references/fika-plugin/Fika.Core/Main/GameMode/BaseGameController.cs#L116), público) |
| `Player.Teleport(Vector3)` | [ActiveHealthController vizinho — EFT/Player.cs:31308](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L31308) |
| `FikaPlayer.ToggleDowned(false)` | [FikaPlayer.cs:595](../../../../references/fika-plugin/Fika.Core/Main/Players/FikaPlayer.cs#L595) (public virtual) — **já envia o `DownedSyncPacket`** avisando os outros clientes |
| `ActiveHealthController.RestoreFullHealth()` | [:3607](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs#L3607) |
| `ActiveHealthController.SetDamageCoeff(float)` | [:3673](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs#L3673) |

**Sorteio do ponto:** `SelectSpawnPoint` recebe um `profileId`. Passar um identificador **aleatório a
cada renascimento** faz o sistema de spawn tratar o pedido como se fosse de outro personagem, e ele
aplica sozinho a lógica de afastamento de jogadores já presentes — em vez de reimplementarmos um filtro
de distância. O ponto sorteado é comparado com a posição da morte; se coincidir, tenta-se de novo um
número limitado de vezes, e depois aceita-se o que vier (mapas pequenos podem não ter alternativa).

## 3. Novas propriedades F12

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| `Lives` | `Respawn Key` | KeyCode | `F5` | — | Tecla para renascer. Segure-a enquanto estiver caído. |
| `Lives` | `Respawn Hold Time (s)` | float | `2` | 0.1 a 10 | Por quanto tempo a tecla precisa ficar pressionada. Soltar antes cancela sem gastar vida. |
| `Lives` | `Spawn Protection (s)` | float | `5` | 0 a 30 | Tempo sem receber dano depois de renascer. `0` = sem proteção. |

## 4. Arquivos

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Settings.cs` | MODIFICAR | Três entradas novas |
| `modded/RespawnService.cs` | CRIAR | Sorteio do ponto, a sequência de cinco passos e a janela de proteção |
| `modded/Patches/RespawnInputPatch.cs` | CRIAR | Postfix de `Player.UpdateTick` — segurar a tecla e o tique da proteção |
| `modded/RaidState.cs` | MODIFICAR | Zerar o estado de respawn no `Begin`/`End` |

## 5. Riscos

- **`SelectSpawnPoint` pode devolver um ponto dentro de geometria.** É o mesmo sistema que a partida usa
  no início, então o risco é o mesmo do spawn normal — aceito.
- **Sem `SpawnSystem`** (partida sem Fika, ou controlador ainda não pronto): o respawn aborta com aviso
  em vez de teleportar para lugar nenhum.
- **A sincronia fina fica no item 003.** Aqui o `ToggleDowned(false)` já avisa os outros clientes que o
  jogador levantou, mas a posição nova chega pelo fluxo normal de estado, com interpolação — pode
  aparecer um deslize. É exatamente o que o 003 resolve.
- **Corrida entre o tempo esgotando e o segurar completando.** Ambos terminam no mesmo quadro no pior
  caso; a guarda `Downed` no início da sequência garante que só um dos dois efetive.

## 9. Conformidade

| # | Check | Status | Evidência |
|---|---|---|---|
| 1 | Lifecycle | ✅ | Estado de respawn zerado em `RaidState.Begin/End` |
| 2 | Filtro MainPlayer/Fika | ✅ | `UpdateTick` roda para todo mundo; `IsYourPlayer` é a primeira linha |
| 3 | Overrides auditados | ✅ | `ToggleDowned` é chamado, não patchado — `ObservedPlayer` sobrescreve, mas quem chamamos é a instância local (`FikaPlayer`) |
| 4 | API canônica | ✅ | Tudo por chamada pública (`Teleport`, `ToggleDowned`, `RestoreFullHealth`, `SetDamageCoeff`); nenhuma escrita direta de campo |
| 5 | Estado entre raids | ✅ | `RaidState.Begin` zera; nenhum estático fora dele |
| 6 | Defaults sem ambiguidade | ✅ | §3 define o neutro de `Spawn Protection` (`0` = sem proteção) |
| 7 | Reentrância | ✅ | A sequência sai cedo se o jogador não estiver `Downed`, o que impede disparo duplo |
| 8 | Cache stale | ✅ | Nada cacheado entre quadros além do contador de pressão, zerado ao soltar |
| 9 | Patch-point no dump | ✅ | `Player.Teleport` :31308; `RestoreFullHealth` :3607; `SetDamageCoeff` :3673 |
| 10 | Skill EFT | N/A | Não usa skill |
| 11 | Pacote FIKA | N/A | Nenhum pacote próprio — item 003 |

## Histórico

| Data | Evento |
|---|---|
| 2026-08-01 | Spec técnica criada via `/create-technical-spec` |
