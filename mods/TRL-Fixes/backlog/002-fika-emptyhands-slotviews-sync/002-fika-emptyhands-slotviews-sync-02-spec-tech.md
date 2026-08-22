# 002 — fika-emptyhands-slotviews-sync · Spec Técnica

**Mod:** TRL-Fixes  
**Status:** 🟢 Vivo  
**Data:** 2026-08-16  
**Autores:** [Antigravity]

---

## 1. Arquitetura e Evidência Canônica

### 1.1 Hierarquia de Evidência
1. **🥇 Servidor Fika:** [references/fika-plugin/Fika.Core/Networking/FikaServer.Callbacks.cs:121-154](../../../../references/fika-plugin/Fika.Core/Networking/FikaServer.Callbacks.cs#L121-L154) — método `OnProceedRequestPacketReceived` tenta `TryFindItemForProceedPacket(packet.ItemId)` incondicionalmente.
2. **🥇 Pacote de Rede:** [references/fika-plugin/Fika.Core/Networking/Packets/FirearmController/ProceedRequestPacket.cs:18-33](../../../../references/fika-plugin/Fika.Core/Networking/Packets/FirearmController/ProceedRequestPacket.cs#L18-L33) — serialização omite `ItemId` quando `ProceedType == EmptyHands`.
3. **🥇 Cliente Fika:** [references/fika-plugin/Fika.Core/Main/Players/ObservedPlayer.cs:1401-1475](../../../../references/fika-plugin/Fika.Core/Main/Players/ObservedPlayer.cs#L1401-L1475) — `RefreshSlotViews` usa `Dictionary<string, GClass768.GClass769>` indexado por `slot.FullId`.
4. **🥈 Mod TRL-Fixes:** [mods/TRL-Fixes/modded/Patches/](../../modded/Patches/) — aplicação de Harmony Prefixes defensivos.

---

## 2. Componentes e Estratégia de Patching

### 2.1 `FikaProceedEmptyHandsSafetyPatch`
- **Alvo:** `Fika.Core.Networking.FikaServer.OnProceedRequestPacketReceived`
- **Assinatura:** `void (ProceedRequestPacket packet, NetPeer peer)`
- **Estratégia:** `[PatchPrefix]` Harmony. Se `packet.ProceedType == EProceedType.EmptyHands` (byte `0`), constrói `ProceedResponsePacket` com `CallbackId = packet.CallbackId` e `Error = null`, envia via `SendDataToPeer` e retorna `false`.

### 2.2 `FikaRefreshSlotViewsSafetyPatch`
- **Alvo:** `Fika.Core.Main.Players.ObservedPlayer.RefreshSlotViews`
- **Assinatura:** `void ()`
- **Estratégia:** `[PatchPrefix]` Harmony. Re-implementa a vinculação de slots e substitui o dicionário simples por uma lista de pares chave-valor (`List<KeyValuePair<string, GClass768.GClass769>>`), permitindo múltiplos slots `mod_tactical` sem colisão de chaves.

---

## 3. Matriz de Arquivos Afetados

| Arquivo | Ação | Descrição |
|---|---|---|
| `Patches/FikaProceedEmptyHandsSafetyPatch.cs` | `[NEW]` | Patch para validação de mãos vazias no FikaServer |
| `Patches/FikaRefreshSlotViewsSafetyPatch.cs` | `[NEW]` | Patch defensivo para colisão de slots de armas em ObservedPlayer |
| `Plugin.cs` | `[MODIFY]` | Bump de versão 1.3.0 e inicialização dos novos patches |
| `TRLFixes.csproj` | `[MODIFY]` | Sincronização SemVer 1.3.0 |
| `CHANGELOG.md` | `[MODIFY]` | Registro da versão 1.3.0 |
