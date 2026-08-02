# Propriedades F12 — TRL-PvpMode

> Todas as opções do menu **F12** (BepInEx ConfigurationManager). **1 seção · 9 opções.**
> Gerado de [modded/Settings.cs](modded/Settings.cs) em **2026-08-01**, para a **v0.8.0**.
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
| Min Spawn Distance (m) | Distância mínima ao renascer (m) | float | `80` | 0 a 500 | Distância mínima de qualquer jogador ou bot vivo ao sortear onde renascer. Se nenhum ponto atender, o filtro é relaxado em vez de impedir o renascimento. |
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

3. **O mod precisa estar instalado em TODOS os participantes — inclusive no anfitrião sem tela.**
   Isto não é preferência: é requisito de funcionamento. O anfitrião retransmite o aviso de
   renascimento em bytes crus, **antes** de decodificá-lo. Numa máquina sem o mod, o Fika não sabe o
   que é aquele pacote e lança erro no meio do processamento de rede — e como ele não protege esse
   ponto, **todos os eventos de rede enfileirados naquele quadro são descartados**, para todos os
   pares e todos os mods. O sintoma é o clássico "jogador patinando". Além disso, um par sem o mod
   continua enxergando a opção de levantar o caído e consegue usá-la, furando a contagem de vidas.

## Limitações conhecidas

| Limitação | Detalhe |
|---|---|
| **Opções valem por partida** | `Lives Per Raid`, `Downed Timeout` e `Enable Lives Mode` são lidos **no início da raid**. Mudá-los durante a partida não tem efeito nenhum até a partida seguinte. |
| **A plaquinha de vida vista pelos companheiros usa o tempo do servidor** | O medidor do Fika lê o `bleedoutTime` do `fika.jsonc` direto, fora do alcance do mod. O número que os outros veem pode divergir do seu; o desfecho real segue o valor do F12. |
| **Configuração diferente entre jogadores não é suportada** | Todos precisam da **mesma** configuração. Se quem cai está com o modo desligado e quem observa está com ele ligado, o observador não vê a opção de levantar e o caído não tem tecla de renascer — situação sem saída que não existe sem o mod. |
| **`grenadesKills` do servidor é ignorado** | O mod assume por inteiro a decisão de "o que mata na hora", então a opção de granada do `fika.jsonc` deixa de valer. Só a de cabeça tem equivalente no F12. |
| **Bots ainda procuram no lugar da morte** | Ao renascer, o corpo aparece no ponto novo para todos, mas a memória de inimigo dos bots guarda a última posição conhecida num cache próprio que o teleporte não invalida. Eles vasculham o ponto antigo por um tempo antes de desistir. |
| **Reconexão estando caído** | Comportamento indefinido — restaurar o estado ao reentrar na partida não foi implementado. |
| **Cair durante transição, extração ou dentro do veículo blindado** | Não tratado. O estado de caído trava o personagem; renascer nesses contextos não foi testado. |
| **Vasculhar o corpo caído também some** | O bloqueio da opção "levantar companheiro" remove **todas** as ações sobre o caído, inclusive "Search" quando `allowLooting` está ligado no servidor. Coerente com cadáver saqueável estar fora de escopo. |

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-08-01 | Guilherme | Criação — 4 opções da seção `Lives` (item 001 do backlog). |
| 2026-08-01 | Guilherme | +3 opções do item 002 (tecla, tempo segurando, proteção ao renascer). |
| 2026-08-01 | Guilherme | +1 opção do item 004 (contador na tela). |
| 2026-08-02 | Guilherme | +1 opção do review do 002 (distância mínima ao renascer); limitações reescritas contra o comportamento real; pré-requisito de rede elevado a requisito duro após o review de coop. |
