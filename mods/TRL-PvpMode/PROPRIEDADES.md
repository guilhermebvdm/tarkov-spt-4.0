# Propriedades F12 — TRL-PvpMode

> Todas as opções do menu **F12** (BepInEx ConfigurationManager). **1 seção · 8 opções.**
> Gerado de [modded/Settings.cs](modded/Settings.cs) em **2026-08-01**, para a **v0.5.0**.
>
> **Plugin:** `com.trl.pvpmode` — "TRL-PvpMode" · arquivo de config: `BepInEx/config/com.trl.pvpmode.cfg`

---

## Lives

| Propriedade (EN) | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Enable Lives Mode | Ligar modo de vidas | bool | `true` | — | Liga o modo de vidas por raid. Desligado, volta a valer o resgate padrão do Fika (companheiro pode te levantar e o tempo vem do servidor). |
| Lives Per Raid | Vidas por partida | int | `1` | -1 a 10 | Quantas vezes você pode renascer por partida. `-1` = ilimitado. `0` = nenhuma (morre de primeira). |
| Downed Timeout (s) | Tempo para decidir (s) | float | `60` | 0 a 600 | Tempo para decidir renascer, em segundos. Ao zerar, a morte é definitiva. `0` = sem limite: você fica caído até decidir. |
| Headshot Kills Instantly | Tiro na cabeça mata direto | bool | `false` | — | Tiro na cabeça encerra a partida na hora, ignorando as vidas restantes. |
| Show Lives Counter | Mostrar contador de vidas | bool | `true` | — | Mostra as vidas restantes na tela. Fica destacado enquanto você estiver caído. |
| Respawn Key | Tecla de renascer | KeyCode | `F5` | — | Tecla para renascer. Segure-a enquanto estiver caído. |
| Respawn Hold Time (s) | Tempo segurando (s) | float | `2` | 0.1 a 10 | Por quanto tempo a tecla precisa ficar pressionada. Soltar antes cancela sem gastar vida. |
| Spawn Protection (s) | Proteção ao renascer (s) | float | `5` | 0 a 30 | Tempo sem receber dano depois de renascer. `0` = sem proteção. |

---

## Pré-requisitos (fora do F12)

O modo **não funciona** sem estes dois. Ambos são avisados na tela no início da raid quando faltam:

1. **`reviveConfig.enabled: true`** no `fika.jsonc` do servidor. O modo é construído sobre o estado de
   caído do Fika, e o Fika só instala esse mecanismo quando a chave está ligada
   (`FikaConfig.cs:908`). Com ela desligada, morrer encerra a partida como no jogo original.
2. **O mod PlayerLives não pode estar instalado.** Ele intercepta o mesmo instante da morte e impede
   todo o resto de funcionar.

E um terceiro que o jogo não tem como avisar: **o mod precisa estar instalado em todos os clientes.**
O bloqueio de "levantar companheiro" roda na máquina de quem olha — um par sem o mod continua vendo a
opção e consegue levantar o caído, furando a contagem de vidas.

## Limitações conhecidas

| Limitação | Detalhe |
|---|---|
| **Mudar o tempo durante a raid** | O prazo é fotografado no instante da queda. Alterar `Downed Timeout` estando caído não muda a contagem em curso — vale a partir da próxima queda. |
| **Tempo `0` desliga a tecla nativa de desistir** | Com tempo zero, o componente de contagem do Fika sai antes de ler o teclado. Até o item 002 existir (tecla própria de renascer), configurar `0` deixa o jogador caído **sem saída**. |
| **A plaquinha de vida vista pelos companheiros usa o tempo do servidor** | O medidor do Fika lê o `bleedoutTime` do `fika.jsonc` direto, fora do alcance do mod. O número que os outros veem pode divergir do seu; o desfecho real segue o valor do F12. |
| **Reconexão estando caído** | Comportamento indefinido — restaurar o estado no rejoin depende de rede e está previsto para o item 003. |
| **Cair durante transição, extração ou dentro do veículo blindado** | Não tratado. O estado de caído trava o personagem, e nesses contextos não há como escapar até o item 002 existir (tecla de renascer). Combinado com tempo `0`, é um travamento sem saída. |
| **Vasculhar o corpo caído também some** | O bloqueio da opção "levantar companheiro" remove **todas** as ações sobre o caído, inclusive "Search" quando `allowLooting` está ligado no servidor. Coerente com cadáver saqueável estar fora de escopo. |

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-08-01 | Guilherme | Criação — 4 opções da seção `Lives` (item 001 do backlog). |
