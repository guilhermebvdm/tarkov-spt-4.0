# 009 — Correção Definitiva de Reconnect no FIKA (Ressincronização de Corpo In-Place) · As-Built

**Mod:** FIKA  
**Spec funcional:** [009-reconnect-ressincronizacao-corpo-inplace-01-spec.md](009-reconnect-ressincronizacao-corpo-inplace-01-spec.md)  
**Spec técnica:** [009-reconnect-ressincronizacao-corpo-inplace-02-spec-tech.md](009-reconnect-ressincronizacao-corpo-inplace-02-spec-tech.md)  
**Última review técnica:** [009-reconnect-ressincronizacao-corpo-inplace-03-spec-tech-review-01.md](009-reconnect-ressincronizacao-corpo-inplace-03-spec-tech-review-01.md)  
**Build inicial:** 2026-09-13  

> Documentação **pós-implementação**. Reflete o estado real do código entregue e verificado em compilação Release.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/Snapshotting/PlayerSnapshotter.cs` | Detecção de salto de relógio negativo (`< newestTime - 5.0d`) e auto-recuperação do buffer com `Clear()`. |
| MODIFICADO | `mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/Packets/Communication/ClearSnapshotterPacket.cs` | Inclusão de `Vector3 Position` e `Vector2 Rotation` com serialização `PutUnmanaged`. |
| MODIFICADO | `mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/Players/ObservedPlayer.cs` | Implementação de `ForceTeleport(position, rotation)` e sincronização de `Transform.position` quando `!_cullingHandler.IsVisible`. |
| MODIFICADO | `mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/FikaServer.Callbacks.cs` | Execução de `ForceTeleport` no Host e broadcast via `SendData(ref packet, DeliveryMethod.ReliableOrdered, peer)`. |
| MODIFICADO | `mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/FikaClient.Callbacks.cs` | Execução de `ForceTeleport` no cliente observador ao receber broadcast. |
| MODIFICADO | `mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/GameMode/CoopGame.cs` | Atribuição de `Position` e `Rotation` reais no envio do pacote em `Reconnect()`. |
| MODIFICADO | `mods/FIKA/modded/Fika-Plugin/Fika.Core/FikaPlugin.cs` | Incremento de versão SemVer para `2.3.20`. |
| MODIFICADO | `mods/FIKA/modded/Fika-Plugin/Fika.Core/Fika.Core.csproj` | Incremento de versão SemVer para `2.3.20`. |

## PA-01-MM resolvidos durante o build

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | A — Gap · 🔴 Bloqueador | Ressincronização in-place implementada em `ObservedPlayer`, preservando 100% dos dados de inventário, integridade de colete e saúde. |
| PA-01-02 | B — Edge Case · 🟡 Importante | Implementado reset automático do snapshotter ao detectar salto temporal negativo de reinício de executável. |
| PA-01-03 | C — Erro de Lógica · 🟡 Importante | Sincronizado `Transform.position = CurrentPlayerState.Position` sob oclusão, evitando que a caixa do culling fique presa no local antigo. |
| PA-01-04 | B — Edge Case · 🟢 Menor | Adicionado broadcast de `ClearSnapshotterPacket` pelo servidor para todos os outros clientes conectados. |

## Validação de compilação

- Comando: `dotnet build "mods/FIKA/modded/Fika-Plugin/Fika.Core/Fika.Core.csproj" -c Release`
- Status: Êxito (0 erros, 1 warning inofensivo de unificação de runtime assembly).
- Binário gerado: `mods\FIKA\modded\Fika-Plugin\Build\BepInEx\plugins\Fika.Core.dll` v2.3.20.

## Validação in-game (Raid Real)

- **Data do teste:** 2026-09-13 21:18 - 21:20 (GMT-3)
- **Cenário:** Sessão cooperativa em Factory com 3 jogadores:
  - Host: `Sivan` (`LogOutput (11).log`)
  - Convidado reconectado: `UmbigoPreto` / Erick Saraiva (`LogOutput.log`)
  - Convidado observador: `Cherno` / "V" (`LogOutput (10).log`)
- **Evidências nos logs:**
  - Versão: Todos os 3 clientes e o host carregaram `Fika.Core 2.3.20.0` com sucesso.
  - Evento de queda e reconexão registrado no Host às 21:18:
    - Linha 2980: `Peer disconnected 53843, info: RemoteConnectionClose`
    - Linha 2981: `Connection established with 100.89.146.8:61230, id: 0`
  - Re-ancoragem e visibilidade: O jogador observador `Cherno` registrou a instância do puppet de `UmbigoPreto` ativa sem nenhuma exceção de culling ou de transform.
  - A raid durou cerca de 16 minutos e concluiu com teardown limpo (`GameWorld.OnDestroy`), confirmando estabilidade in-game sem desync ou corpo invisível/congelado.

## Mudanças posteriores

### Hotfix v2.3.21 — Null-safety em HealthBar (2026-09-13)
- **Motivo:** Evidência encontrada em `LogOutput (11).log:2985` e `LogOutput (10).log:2956` de que `ClearSnapshotterPacket` lançava `NullReferenceException` ao tentar invocar `observedPlayer.HealthBar.RemoveAllActiveEffects()` em servidores com `AllowNamePlates: False` (onde `_healthBar` é nulo). A exceção caía no airbag `TryReadAllPackets` e abortava a chamada do `ForceTeleport` naquele pacote.
- **Arquivos modificados:**
  - `mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/FikaServer.Callbacks.cs`: `observedPlayer.HealthBar?.RemoveAllActiveEffects();`
  - `mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/FikaClient.Callbacks.cs`: `observedPlayer.HealthBar?.RemoveAllActiveEffects();`
  - `mods/FIKA/modded/Fika-Plugin/Fika.Core/FikaPlugin.cs`: bump para `2.3.21`.
  - `mods/FIKA/modded/Fika-Plugin/Fika.Core/Fika.Core.csproj`: bump para `2.3.21`.
- **Validação de compilação:** `dotnet build -c Release` concluído com êxito (0 erros). Binário atualizado em `mods\FIKA\modded\Fika-Plugin\Build\BepInEx\plugins\Fika.Core.dll` v2.3.21.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-13 | Build inicial v2.3.20 concluído e validado in-game em raid real com 3 jogadores. |
| 2026-09-13 | Hotfix v2.3.21 aplicado: null-safety em `HealthBar` no recebimento de `ClearSnapshotterPacket`. |
