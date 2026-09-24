# 011 — Otimização de Rede do Host (AoI Culling & Multithreading) · Review Técnica 01

**Mod:** FIKA  
**Target / Fork:** `mods/FIKA/modded-V2/`  
**Spec Funcional:** [011-otimizacao-rede-host-aoi-multithreading-01-spec.md](011-otimizacao-rede-host-aoi-multithreading-01-spec.md)  
**Spec Técnica:** [011-otimizacao-rede-host-aoi-multithreading-02-spec-tech.md](011-otimizacao-rede-host-aoi-multithreading-02-spec-tech.md)  
**Data da Review:** 2026-09-15  
**Parecer Geral:** ⚠️ APROVADO COM RESSALVAS TÉCNICAS E RISCOS APONTADOS  

---

## 1. Avaliação Crítica de Arquitetura & Riscos Mapeados

Esta revisão técnica foi conduzida de forma estrita e independente sobre a arquitetura proposta em `02-spec-tech.md`. Foram identificados 5 pontos fracos e riscos que devem ser monitorados e mitigados nas fases de teste in-game e refinamento:

### ⚠️ RT-01: Risco de Thread Órfã no Teardown (`OnDestroy` com `Join(200)`)
- **Problema:** Em `BotStateManager.OnDestroy()`, a finalização da worker thread faz:
  ```csharp
  _workerRunning = false;
  _workSignal?.Set();
  if (_workerThread != null && _workerThread.IsAlive)
  {
      _workerThread.Join(200);
  }
  ```
- **Risco:** O tempo de 200 ms é extremamente agressivo. Se a thread de rede estiver no meio de um loop serializando múltiplos peers ou aguardando resposta do driver de rede UDP sob congestionamento, o `Join(200)` expirará silenciosamente. O GameObject da Unity será destruído, mas a thread continuará viva por alguns milissegundos tentando acessar `_server` ou buffers já descartados, gerando `NullReferenceException` no log de encerramento da raid.
- **Recomendação:** Aumentar o timeout do `Join` para 1.000 ms (1 segundo) e utilizar um `CancellationToken` ou proteção nula explícita em todos os campos consumidos no worker loop.

---

### ⚠️ RT-02: Risco de Log Flood por Desconexão Abrupta de Peer em Background
- **Problema:** No método `WorkerLoop()`, qualquer falha durante `ProcessAndSendSnapshot()` cai em um bloco genérico:
  ```csharp
  catch (Exception ex)
  {
      FikaGlobals.LogError($"[BotStateManager] Erro ao processar pacotes no worker de rede: {ex}");
  }
  ```
- **Risco:** Se um cliente cair da partida abruptamente (queda de internet, crash do cliente ou fechamento via Alt+F4), o socket do LiteNetLib pode lançar exceção no momento do `peer.Send()`. Como o tick de rede roda a 30 Hz, o worker disparará até **30 mensagens de erro por segundo no log do BepInEx**. Isso congela o jogo do host com I/O síncrono de escrita em disco (`LogOutput.log`).
- **Recomendação:** Filtrar exceções de socket/desconexão e aplicar *rate limit* no log de erro do worker (no máximo 1 log por segundo por tipo de erro).

---

### ⚠️ RT-03: Degradação de Banda por Jogadores Mortos ou em Modo Espectador
- **Problema:** A spec define que se `!player.HealthController.IsAlive`, o sistema atribui `HasPosition = false`.
- **Risco:** Quando `HasPosition` é falso, o AoI Culling é desativado para aquele peer, forçando o envio de **100% dos bots do mapa**. Se 3 de 4 amigos morrerem durante a incursão, o tráfego de rede do host atingirá o pico máximo exatamente no estágio final da raid. Além disso, se o jogador morto estiver observando os aliados através do modo `FreeCamera`, o culling deveria usar a posição da câmera livre (`CameraMain.transform.position`) em vez de abrir mão do filtro.
- **Recomendação:** Integrar com o `FreeCameraController`: se o jogador estiver morto e em FreeCam, usar a coordenada da câmera como âncora do AoI; se estiver em tela preta/morte, manter o heartbeat lento (~1 Hz) em vez de broadcast a 30 Hz.

---

### ⚠️ RT-04: Falta de Histerese na Borda das Zonas ("Edge Fluttering")
- **Problema:** A troca de faixas é estrita:
  - `< 250m` = 30 Hz
  - `> 250m` e `<= 500m` = 5 Hz
- **Risco:** Um bot patrulhando ou se movendo na faixa de 249m a 251m em relação a um jogador sofrerá oscilação abrupta de taxa de amostragem a cada tick. Embora o `PlayerSnapshotter` no cliente realize interpolação, essa transição rápida e sem zona de amortecimento (histerese) pode gerar micro-saltos visuais ("jitter") quando observado através de miras ópticas telescópicas (4x / 8x).
- **Recomendação:** Validar em raid real se snipers relatam jitter em bots patrulhando a 250m. Se confirmado, adotar uma histerese de ±10 metros (ex: entra na zona tática em 240m e só sai para a periférica em 260m).

---

### ⚠️ RT-05: Limitação da Heurística `InCombat`
- **Problema:** A checagem de combate avalia apenas:
  `item.InCombat = bot.AIData?.BotOwner?.Memory?.GoalEnemy != null;`
- **Risco:** No Tarkov, bots frequentemente entram em alerta investigativo (ouvindo passos, tiros ou movimentação) antes de estabelecer o `GoalEnemy` definitivo. Um Scav a 350m que ouve um tiro de um jogador e se posiciona para emboscada continuará na Zona Periférica (5 Hz) até que a linha de visão confirme o inimigo na memória formal.
- **Recomendação:** Avaliar se a verificação deve incluir `bot.AIData?.BotOwner?.Memory?.IsUnderFire == true` em uma iteração futura de refinamento.

---

## 2. Matriz de Rastreabilidade

| Requisito Funcional | Componente | Status Técnico | Observações |
| :--- | :--- | :--- | :--- |
| Zero-Alloc na Main Thread | `BotStateManager` | ✅ Atendido | Structs blittable puras copiadas em arrays fixos sem alocação. |
| Despacho UDP fora da Main Thread | `BotStateManager` / `WorkerLoop` | ✅ Atendido | Processamento delegado via thread separada com `AutoResetEvent`. |
| Filtragem Individualizada por Peer | `FikaServer.SendStatesToPeer` | ✅ Atendido | Métodos implementados no LiteNetLib com buffers `ReadOnlySpan<byte>`. |
| Interoperabilidade com Clientes | `FikaClient` | ✅ Atendido | Cliente suporta naturalmente N entidades via `MemoryMarshal.Cast`. |
| Desligamento Limpo da Raid | `BotStateManager.OnDestroy` | ⚠️ Atenção | Requer atenção ao timeout de 200ms sob alta latência de rede (RT-01). |

---

## 3. Conclusão da Review

A solução proposta é tecnicamente sólida e resolve diretamente o gargalo mais grave do cooperativo do FIKA (queda de FPS do Host). As ressalvas listadas acima não impedem o avanço para testes práticos, mas representam a lista prioritária de pontos de atenção durante as validações em raids reais.
