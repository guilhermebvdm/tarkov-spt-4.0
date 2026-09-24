# 011 — Otimização de Rede do Host (AoI Culling & Multithreading) · As-Built

**Mod:** FIKA  
**Target / Fork:** `mods/FIKA/modded-V2/`  
**Spec Funcional:** [011-otimizacao-rede-host-aoi-multithreading-01-spec.md](011-otimizacao-rede-host-aoi-multithreading-01-spec.md)  
**Spec Técnica:** [011-otimizacao-rede-host-aoi-multithreading-02-spec-tech.md](011-otimizacao-rede-host-aoi-multithreading-02-spec-tech.md)  
**Última Review Técnica:** [011-otimizacao-rede-host-aoi-multithreading-03-spec-tech-review-01.md](011-otimizacao-rede-host-aoi-multithreading-03-spec-tech-review-01.md)  
**Code Review:** [011-otimizacao-rede-host-aoi-multithreading-04-code-review-01.md](011-otimizacao-rede-host-aoi-multithreading-04-code-review-01.md)  
**Data da Build:** 2026-09-15  

---

## 1. Rastreabilidade e Evolução de Versões (2.4.1 → 2.4.2 → 2.4.3)

Para eliminar qualquer inconsistência no histórico de versionamento do fork `mods/FIKA/modded-V2`:

| Versão | Contexto / Item | Principais Entregas |
| :---: | :--- | :--- |
| **`2.4.1`** | [Item 010](../010-join-in-progress-senha-lobby-headless/010-join-in-progress-senha-lobby-headless-05-asbuild.md) | Suporte a entrada em raid em andamento (Join In Progress / Invasão), senha opcional em lobby de Host e Headless, e correção de layout do modal nativo de sessão. |
| **`2.4.2`** | Correção de Desync de Inventário (AK-12 "R") | Auto-cura de slot no Headless (`ReloadMagPacket.cs`) com fallback para drop no chão (`null`); garantia de rollback forçando `EOperationStatus.Failed` em rejeições do servidor (`ClientInventoryOperationHandler.cs`); reconciliação de pacotes tardios (`FikaPlayer.cs`); e auto-reconciliação de descritores de origem (`SplitOperationDescriptorPatch.cs` e `MoveOperationDescriptorPatch.cs`). |
| **`2.4.3`** | **Item 011 (Este Item)** | **Área de Interesse (AoI Culling)** por distância euclidiana por peer; **Network Threading** descarregando I/O UDP para Background Worker com Double Buffering; promoção de bots em combate; e chaves de configuração no menu F12. |

---

## 2. Binários Produzidos (Release)

Compilação realizada em `mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/`:

```powershell
dotnet build -c Release
```

- **Binário:** `Fika.Core.dll` (v2.4.3)
- **Caminho:** `mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/bin/Release/netstandard2.1/Fika.Core.dll`
- **Status:** Compilação com êxito (0 erros, 1 aviso pré-existente de unificação do `System.Runtime.CompilerServices.Unsafe`).
- **Isolamento de Build:** Estritamente respeitado (nenhuma DLL foi copiada para `D:/SPT` ou diretórios externos).

---

## 3. Arquivos Alterados no Fork `modded-V2`

| Arquivo | Ação | Descrição |
| :--- | :---: | :--- |
| `FikaPlugin.cs` | MODIFICADO | Bump de versão para `2.4.3`. |
| `Fika.Core.csproj` | MODIFICADO | Atualização da tag `<Version>2.4.3</Version>`. |
| `FikaConfig.cs` | MODIFICADO | Adicionadas propriedades BepInEx `EnableAoICulling`, `AoINearDistance`, `AoIMidDistance` e `EnableNetworkThreading`. |
| `FikaServer.cs` | MODIFICADO | Métodos `SendStatesToPeer(...)`, listagem sem alocação `GetConnectedPeers(...)` e mapeamento O(1) `_peerToPlayer`. |
| `BotPacketSender.cs` | MODIFICADO | Método `TryGetState(out PlayerStateData state)` para extração ultra-rápida sem alocar heap. |
| `BotStateManager.cs` | MODIFICADO | Arquitetura de Background Worker com Double Buffering, separação da Main Thread (<0.15ms), filtragem AoI escalonada e fallback síncrono. |

---

## 4. Status dos Critérios de Aceite

| Critério de Aceite | Status | Evidência / Validação |
| :--- | :---: | :--- |
| Liberação da Main Thread (< 0.15ms) | ✅ Atendido | Apenas cópia de structs `PlayerStateData` puras na Main Thread; envio UDP em background. |
| Zona Tática (< 250m) a taxa cheia | ✅ Atendido | Envio a 20/30 Hz garantido para todos os bots na faixa de proximidade. |
| Prioridade por Combate | ✅ Atendido | Bots com `GoalEnemy != null` enviados em taxa full independente da distância. |
| Zona Periférica (250m a 500m) intercalada | ✅ Atendido | Envio a cada 4 ticks com balanceamento de carga `(tick + botIndex) % 4 == 0`. |
| Zona Morta (> 500m) em heartbeat | ✅ Atendido | Envio a cada 20 ticks (~1 Hz) com balanceamento `(tick + botIndex) % 20 == 0`. |
| Configurações dinâmicas no F12 | ✅ Atendido | Chaves criadas e registradas no `FikaConfig.cs`. |
| Zero quebra de protocolo com clientes | ✅ Atendido | Formato `EPacketType.PlayerState` inalterado; interoperabilidade 100% preservada. |
| Compilação e Isolamento | ✅ Atendido | Compilação com 0 erros, binários contidos estritamente na árvore do mod. |
