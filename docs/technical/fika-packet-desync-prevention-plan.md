---
title: Prevenção de Desync e Erros no FIKA (NetPacketProcessor) — Diretrizes e Plano de Correção
date: 2026-07-24
status: 🟢 Vivo
authors: Guilherme + agente
---

# 🔍 Prevenção de Desync e Erros no FIKA (NetPacketProcessor)

## 📌 Contexto & Diagnóstico Técnico

No FIKA (multiplayer coop para SPT Tarkov), a transmissão de pacotes customizados `INetSerializable` é gerida pelo `NetPacketProcessor` do `LiteNetLib`, encapsulado em `Singleton<IFikaNetworkManager>.Instance` (`FikaClient` ou `FikaServer`).

Quando o FIKA reinicia a camada de rede (ex.: transições entre menus, lobbies, telas de carregamento e raids), uma nova instância de `IFikaNetworkManager` é criada. Se um mod:
1. Utilizar flags booleanas estáticas simples (`_initialized = true`) sem rastrear a **referência do objeto `IFikaNetworkManager`**, o mod assume que já registrou os pacotes, deixando a nova instância do `NetPacketProcessor` sem os handlers.
2. Chamar `UnregisterPacket<T>()` ao encerrar raids, removendo o hash do pacote do `NetPacketProcessor`.
3. Não registrar os pacotes a tempo antes do primeiro pacote da raid/lobby chegar.
4. Lançar exceções não tratadas dentro dos callbacks de recepção de pacotes.

Quando um pacote chega sem handler registrado ou quando o callback estoura uma exceção dentro de `FikaClient.OnNetworkReceive`, o LiteNetLib/FIKA descarta o **lote inteiro de rede (frame batch)** daquele frame. Isso causa travamentos visuais, jogadores congelados/patinando (desync) e exceções `ParseException: Undefined packet in NetDataReader: <HASH>`.

---

## 💡 Diretrizes & Boas Práticas FIKA (Regra de Ouro)

1. **Rastreamento por Referência de Instância (`EnsurePacketsRegistered`)**:
   Em vez de armazenar um `bool _initialized`, armazene `IFikaNetworkManager _lastRegisteredNetworkManager`. Compare se `Singleton<IFikaNetworkManager>.Instance != _lastRegisteredNetworkManager`. Se mudar, re-registre todos os pacotes imediatamente.
2. **Invocação em Frame-Zero no `Plugin.Update()`**:
   Chame `EnsurePacketsRegistered()` no ciclo `Update()` do plugin principal (BepInEx `MonoBehaviour`), garantindo que o registro ocorra em menos de 1 frame, inclusive durante menus, lobbies e telas de carregamento.
3. **NUNCA chamar `UnregisterPacket<T>()`**:
   Jamais desregistre pacotes ao sair de sessões ou encerrar raids. Caso a ação deva ser desativada fora de raid, trate via flag de estado no próprio callback (`if (!isRaidActive) return;`).
4. **Proteção Total em Callbacks (Airbag / Try-Catch)**:
   Envolva o corpo de todos os métodos de recepção de pacotes com blocos `try { ... } catch (Exception ex) { Log.LogError(ex); }` de nível raiz. Isso evita que exceções não tratadas corrompam a fila de recepção do FIKA.

---

## 🛠️ Plano de Refatoração por Mod

### 1. `TRL-DynamicSpawn`
- **Status de Auditoria**: 🟢 **Conforme** (Sem pacotes customizados).
- **Análise**: O mod gerencia spawning/despawning de bots inspecionando o estado do Fika via reflexão (`FikaBackendUtils.IsServer` / `IsSinglePlayer`). Ele não possui classes `INetSerializable` nem invoca `RegisterPacket` / `UnregisterPacket`.
- **Ação**: Nenhuma alteração de código de rede necessária.

### 2. `stancesAndCameraPositionSPT4.0.11`
- **Arquivos**:
  - [FikaSyncManager.cs](file:///d:/Projetos/GITHUB/tarkov-spt-4.0/mods/stancesAndCameraPositionSPT4.0.11/modded/Networking/FikaSyncManager.cs)
  - [Plugin.cs](file:///d:/Projetos/GITHUB/tarkov-spt-4.0/mods/stancesAndCameraPositionSPT4.0.11/modded/Plugin.cs)
- **Ações**:
  1. Implementar o padrão `EnsurePacketsRegistered()` em `FikaSyncManager.cs` guardando a referência de `_lastRegisteredNetworkManager`.
  2. Adicionar a chamada `FikaSyncManager.EnsurePacketsRegistered()` no `Update()` principal de `Plugin.cs`.
  3. Envolver `OnStanceSyncPacketReceived` com um bloco `try-catch` raiz.
  4. Garantir que `UnregisterPacket` nunca seja chamado.

### 3. `TRL-ImmersiveCombatMedicine`
- **Arquivos**:
  - [BandAidNetworkHandler.cs](file:///d:/Projetos/GITHUB/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidNetworkHandler.cs)
  - [TRLImmersiveCombatMedicinePlugin.cs](file:///d:/Projetos/GITHUB/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded/TRLImmersiveCombatMedicinePlugin.cs)
- **Ações**:
  1. Refatorar `BandAidNetworkHandler.cs` substituindo `CheckInit()` pelo padrão `EnsurePacketsRegistered()`, cobrindo os 6 pacotes (`BandAidHealPacket`, `BandAidShoulderTapPacket`, `BandAidHealCheckPacket`, `BandAidHealCheckResponsePacket`, `TraumaFaintPacket`, `BandAidTreatmentReportPacket`).
  2. Invocá-lo no topo do `Update()` em `TRLImmersiveCombatMedicinePlugin.cs` (antes dos filtros de raid).
  3. Proteger todos os 6 callbacks de rede com blocos `try-catch` raiz.
  4. Garantir ausência total de chamadas a `UnregisterPacket`.

---

## 🧪 Plano de Verificação

1. Compilar os mods afetados via script/VS:
   - `stancesAndCameraPositionSPT4.0.11`
   - `TRL-ImmersiveCombatMedicine`
2. Validar que as DLLs geradas registram os pacotes no frame zero e tratam eventuais falhas de rede em silêncio.
