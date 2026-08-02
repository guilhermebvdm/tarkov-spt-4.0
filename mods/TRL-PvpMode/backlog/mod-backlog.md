# Backlog — TRL-PvpMode

> Índice de itens de backlog. Cada linha aponta para uma pasta `NNN-<slug>/` com a spec funcional, técnica e revisões.
>
> **Escopo do modo "vidas por raid"** derivado da sessão de 2026-08-01. Decisões de produto fechadas com o host:
> (a) o estado de morte reusa o **downed do Fika** — exige `reviveConfig.enabled: true` no `fika.jsonc`;
> (b) **sem revive por aliado** — a única saída é renascer; (c) **timer configurável** — se estourar, morte
> definitiva; (d) o jogador **mantém o equipamento** ao renascer (sem cadáver saqueável).
>
> A tentativa anterior de respawn (destruir e recriar o `LocalPlayer`) está arquivada em
> [`mods/TRL-PvpMode-deprecated/`](../../TRL-PvpMode-deprecated/DEPRECATED.md) e **não é base** deste trabalho.

| # | Título | Resumo | Pasta | Status |
|---|---|---|---|---|
| 001 | Morte desligada com timer | Fundação do modo. Interceptar a morte antes do fim de raid e entrar no **downed do Fika** (`FikaPlayer.ToggleDowned(true)`): trava movimento e eixos **sem prazo**, guarda a arma, escurece a tela e sincroniza sozinho via `DownedSyncPacket` — o corpo é desligado nos outros clientes pelo `ReviveInteractable`. Exige neutralizar dois bloqueios: `ClientHealthController.CanBeDowned` (exige aliado vivo ⇒ **quebra jogando solo ou como último vivo**) e `Bleedout.OnPlayerDeath` (força a morte quando todos os humanos caem). Desabilitar o resgate por aliado em `ReviveInteractable.GetActions` **mantendo o ragdoll**. Timer configurável no F12 reusando o painel de contagem do `Bleedout`; ao estourar, morte definitiva com encerramento normal da raid. Pré-requisitos operacionais: `reviveConfig.enabled: true` no `fika.jsonc` e **desinstalar o PlayerLives** (ambos disputam o mesmo ponto de morte). | [001-morte-desligada-timer/](./001-morte-desligada-timer/) | ⚪ |
| 002 | Renascer em spawn aleatório | Tecla de renascer (segurar, no lugar do "desistir" do Fika): consome uma vida, sorteia ponto via `ISpawnSystem.SelectSpawnPoint(ESpawnCategory.Player, side)` com filtro de distância mínima de inimigos, teleporta com `Player.Teleport`, restaura vida e membros enegrecidos, religa o boneco (`ToggleDowned(false)` ⇒ `RemoveRagdoll`) e aplica invulnerabilidade curta. **Ordem obrigatória: teleportar → religar → sincronizar** — com o `ObservedPlayer` desabilitado durante o downed, religar antes faz o corpo reaparecer na posição antiga. Equipamento intacto (decisão de produto: sem cadáver saqueável). Vidas resetadas no `GameWorld.OnGameStarted`. | [002-renascer-spawn-aleatorio/](./002-renascer-spawn-aleatorio/) | ⚪ |
| 003 | Sincronização do respawn em coop | Garantir que anfitrião, outros jogadores e IAs vejam o jogador no local novo **sem deslize**. Hoje o `ObservedPlayer` interpola posição com `Vector3.LerpUnclamped` entre snapshots e **não detecta teleporte** — o corpo atravessaria o mapa em linha reta. Solução: pacote próprio via `IFikaNetworkManager.RegisterPacket<T>` que, ao chegar, chama `PlayerSnapshotter.Clear()` e crava posição/rotação. Inclui validar hitbox funcional pós-respawn (cobertura do TRL-Fixes, que engancha em `RemoveRagdoll`) e o re-registro do jogador nos alvos das IAs. **Validação obrigatória com 2+ clientes** — solo/anfitrião mascara bug de cliente. | [003-sincronizacao-respawn-coop/](./003-sincronizacao-respawn-coop/) | ⚪ |
| 004 | Contador de vidas na tela | Vidas restantes visíveis durante a raid e, principalmente, **no estado caído** — é ali que a decisão de gastar uma vida é tomada. Suporte a vidas infinitas (`-1`). Quantidade inicial configurável no F12. | [004-contador-vidas-tela/](./004-contador-vidas-tela/) | ⚪ |
| 005 | Finalizar o caído | Permitir que inimigos consumem a morte de quem está caído, ignorando a contagem e as vidas — e a opção irmã "granada mata direto". **Desmembrado do 001 no review técnico (R-01):** ao cair, o Fika zera o coeficiente de dano e o corpo vai para a camada de cadáver, então `Kill` nunca é alcançado e provavelmente o tiro sequer gera evento de dano. Exige interceptar o caminho do dano e **validar em partida se é possível de todo** — antes de qualquer código, um teste que responda se um corpo caído recebe tiro. | [005-finalizar-caido/](./005-finalizar-caido/) | ⚪ |

## Legenda

- ⚪ Backlog · 🟡 Em progresso · 🟢 Entregue · 🔴 Cancelado

## Fluxo

1. `/add-backlog-item <mod> <descrição>` → cria entrada + invoca `/create-spec`
2. `/create-spec <ref>` → spec funcional (critérios de aceite + corner cases)
3. `/review-spec <ref>` → editor crítico da spec funcional
4. `/create-technical-spec <ref>` → pré-código com refs ao Assembly
5. `/review-technical-spec <ref>` → cria review-NN.md (incremental); resolver até zerar
6. `/code-mod <ref>` → implementa em `modded/`
