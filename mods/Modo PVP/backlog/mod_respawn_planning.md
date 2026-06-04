# Planejamento: Mod de Respawn (SPT / Fika)

## 1. Visão Geral e Objetivo
O objetivo deste mod é criar uma funcionalidade de **Respawn (Morte Real + Reinstanciação)** para partidas cooperativas no Fika/SPT. Focado em eventos com amigos, onde o jogador morto renasce após um tempo de cooldown, utilizando exatamente o **mesmo loadout** do início da partida, mantendo seu cadáver original (ragdoll) no chão com os itens do momento da morte.

## 2. Conhecimentos Adquiridos da Engine do EFT/Fika

Durante o estudo do `Assembly-CSharp` e `Fika.Core`, mapeamos o fluxo do ciclo de vida do jogador:

- **Morte e Ragdoll (`FikaPlayer.cs`)**: Quando a vida chega a zero, `OnDead` é disparado. A engine cria o ragdoll chamando `CreateCorpse()` (que joga os itens visuais no corpo morto) e envia o pacote de morte pela rede para os demais jogadores. O jogador original (GameObject) se torna uma entidade invisível e morta.
- **Fim de Raid (`BaseLocalGame.cs` e `CoopGame.cs`)**: O evento de morte é pego pela partida, chamando o método `Stop()`. Ele levanta a tela preta de morte (`PreloaderUI.Instance.StartBlackScreenShow`), encerra os timers e, no callback dessa tela, joga o jogador de volta ao menu.
- **Instanciação de Player (`CoopGame.cs`)**: O jogador é inserido no mundo através do método `CreateLocalPlayer()` (que chama internamente o `vmethod_3`). Ele amarra a câmera (`PlayerCameraController.Create`), inicializa componentes de rede e aplica a configuração baseada no `Profile` (loadout/inventário).
- **Problema de Itens Duplicados (MongoIDs)**: Todo item no jogo exige um ID único (MongoID). Fazer o jogador renascer com uma referência exata do inventário passado quebrará a engine quando ela perceber o clone no jogador atual e no cadáver no chão. A solução é serializar o inventário para gerar novos IDs.

## 3. Abordagem e Lógica de Implementação

Nossa abordagem baseia-se em **Interceptação de Fluxo**, evitando que o raid acabe, limpando o "fantasma" da morte, e injetando um novo jogador.

### Passo 1: Backup do Inventário (Início do Raid)
No começo do raid (ex: Postfix no método `InitPlayer` do `CoopGame`), salvaremos o estado inicial do jogador. Faremos uma cópia profunda (Deep Copy) do `Profile.Inventory.Equipment` via serialização JSON (`EFTItemSerializerClass`) para armazenar na memória do plugin.

### Passo 2: Interceptando a Morte e a Tela Preta
Usaremos um **Prefix (Harmony Patch)** no método `Stop()` do `BaseLocalGame` ou `CoopGame`.
- Verificamos se o motivo da parada (`ExitStatus`) é `ExitStatus.Killed`.
- Se for, cancelamos a execução original (`return false`), parando o processo de encerramento de raid.
- Acionamos nós mesmos a tela preta (`PreloaderUI.Instance.StartBlackScreenShow(1f, 1f, NossoCallback)`).

### Passo 3: Limpeza e Cooldown (NossoCallback)
Dentro do callback do Fade da tela preta:
- Acionamos um `Task.Delay` (ex: 5 a 10 segundos).
- Chamamos `gparam_0.Player.Dispose()` e `AssetPoolObject.ReturnToPool(player.gameObject)` para destruir o antigo GameObject morto (o ragdoll `Corpse` não será afetado e continuará no mundo).
- Limpamos a referência desse jogador do `CoopHandler.Players` do Fika.

### Passo 4: O Re-Spawn
Ainda dentro do callback, recriamos o jogador:
- Desserializamos o inventário de backup, forçando a engine a **gerar novos MongoIDs** para os itens clonados. Substituímos isso no `Profile`.
- Invocamos o `CreateLocalPlayer()` (ou lógica adaptada equivalente) definindo as coordenadas (podem ser os SpawnPoints da raid, ou spawn points predefinidos).
- Reconectamos a Câmera e UI ao novo PlayerOwner (`gparam_0`).
- Enviamos os pacotes base de `SendCharacterPacket` para os clientes do Fika registrarem a "entrada" desse novo jogador.

### Passo 5: Godmode Inicial e Retorno ao Jogo
- Escondemos ou damos Fade-Out na tela preta.
- No novo jogador instanciado, aplicamos a proteção: `newPlayer.ActiveHealthController.SetDamageCoeff(0f)`.
- Aguardamos 5 segundos e retornamos o multiplicador de dano ao normal (`1f`).

## 4. Próximos Passos (To-Do)
- [ ] Criar o projeto BepInEx (C#).
- [ ] Implementar a estrutura base do Plugin e logs.
- [ ] Criar o `Patch` para o `BaseLocalGame.Stop` ou `CoopGame.Stop`.
- [ ] Mapear as chamadas exatas de UI (`PreloaderUI`) que estão expostas para Fade In e Fade Out.
- [ ] Configurar a lógica do Deep Copy de itens gerando novos MongoIDs.
