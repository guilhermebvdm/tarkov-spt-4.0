# ⚫ TRL-PvpMode — versão arquivada (abordagem descartada)

> **Data do arquivamento:** 2026-08-01<br>
> **Status:** ⚫ Arquivado<br>
> **Substituído por:** [mods/TRL-PvpMode/](../TRL-PvpMode/)<br>

Esta pasta guarda a **primeira tentativa** de respawn do TRL-PvpMode. Ela **não é base** do mod atual e
**não deve ser compilada nem instalada**. Está preservada só como registro de engenharia reversa.

## Por que foi descartada

A abordagem aqui era a **radical**: interceptar a morte consumada (`FikaPlayer.OnDead`), destruir o
`LocalPlayer`, clonar o perfil com Sirenix, regerar os identificadores únicos de cada item e recriar o
jogador do zero via `CreateLocalPlayer()` — remontando na mão, em 12 blocos de reflexão, câmera, dono do
input, HUD, rede, eventos de morte e registro nas IAs.

Todo esse custo existia por **um** requisito: deixar o cadáver saqueável no chão com o loot do momento
da morte, o que obriga a ter dois inventários vivos ao mesmo tempo.

Esse requisito foi **descartado por decisão de produto** — no modo novo o jogador renasce com o próprio
equipamento intacto. Sem cadáver saqueável, não há duplicação de item, não há perfil para clonar e não
há jogador para recriar: basta usar o estado "downed" que o Fika já sincroniza, teleportar e religar.

## O que ainda vale aqui dentro

**Mapeamento da engine** (custou engenharia reversa e não existe documentado em outro lugar):

| Símbolo | O que é |
|---|---|
| `CoopGame.gparam_0` | O "dono" do input; todo o código nativo consulta esse campo para saber quem é o jogador |
| `CoopGame.dictionary_0` | Registro de jogadores da partida, indexado por `ProfileId` |
| `CoopHandler.ProcessQuitting` + `EQuitState.Dead` | O que encerra a raid quando você morre |
| `FreeCameraController.MainPlayer_DiedEvent` | O gatilho da câmera de espectador pós-morte |
| `CoopGame.CreateLocalPlayer()` | Instancia o jogador e amarra a câmera (privado, assíncrono) |
| `HostGameController.BotsController.AddActivePLayer()` | Re-registra o jogador na lista de alvos das IAs |

**Nota sobre [backlog/mod_pvp_pve_plan.md](backlog/mod_pvp_pve_plan.md):** apesar do nome da pasta, esse
documento **não pertence a este mod**. Ele descreve um mod de **servidor** (rotas SPT/Fika, isolamento de
salas no matchmaking, botão de evento na tela de mapa) — outro escopo, outra base de código. Ficou aqui por
acidente de organização; quando esse trabalho for atacado, nasce como mod próprio.

## Se um dia o cadáver saqueável voltar à mesa

O código aqui é o ponto de partida — com a ressalva de que nunca foi concluído nem validado em partida.
