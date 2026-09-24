# 014 — Spec Técnica: Sincronização de Rede de Cadáver no FIKA

**Mod:** TRL-DynamicSpawn  
**Status:** ⚪ Backlog  
**Criado:** 2026-09-15T09:06:00-03:00  
**Ref:** `014-sincronizacao-rede-cadaver-fika`  

---

## 1. Arquitetura de Rede do FIKA e Cadáveres

### 1.1. Estrutura de Cadáveres no FIKA
- No **Host**: O bot morto é um `Player` ou `FikaBot` nativo cujo cadáver vive em `corpsePlayer.Corpse` (`EFT.Interactive.Corpse`).
- No **Cliente**: O bot é instanciado como `ObservedPlayer` (`Fika.Core.Main.Players.ObservedPlayer`), e ao morrer registra um `ObservedCorpse` no dicionário global de cadáveres:
  `Singleton<GameWorld>.Instance.ObservedPlayersCorpses.Add(NetId, observedCorpse);` ([ObservedPlayer.cs:1107](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/fika-plugin/Fika.Core/Main/Players/ObservedPlayer.cs#L1107)).

### 1.2. Protocolo do Pacote de Sincronização (`CorpseCleanupPacket`)
O pacote deve ser transmitido através dos mecanismos de broadcast do FIKA Server / Host Network Manager:

```
Estrutura do Pacote (CorpseCleanupPacket):
┌────────────────┬───────────────┬──────────────────────┬──────────────────────┐
│  Tipo (1 byte) │ NetId (4 bytes)│ PosX/Y/Z (12 bytes)  │ RotY (4 bytes)       │
└────────────────┴───────────────┴──────────────────────┴──────────────────────┘
Tamanho total: 21 bytes (baixo overhead)
Tipo 0 = ConvertToBackpack
Tipo 1 = DestroyCorpse
```

### 1.3. Pipeline de Envio e Recepção
1. **Envio (Host):**
   - No `CorpseCleanupManager.ConvertToBackpack` ou `DestroyCorpse`, após o processamento local, verificar se `FikaHelper.IsFikaInstalled()` e `FikaHelper.IsHost()`.
   - Serializar os dados usando um `NetDataWriter` estático reutilizável (zero alocação de heap).
   - Chamar o canal de envio confiável (`DeliveryMethod.ReliableOrdered`).
2. **Recepção (Cliente Convidado):**
   - O callback de rede recebe os bytes brutos do socket.
   - **Crucial:** O socket do LiteNetLib opera em **thread de background**!
   - Nenhuma API da Unity (`GameObject.Find`, `Destroy`, `GetComponent`, `Transform`) pode ser chamada nessa thread.
   - O payload deve ser enfileirado em uma `ConcurrentQueue` ou despachado para a **Main Thread** da Unity (ex.: corrotina de Update no cliente ou `UnityMainThreadDispatcher`).
3. **Aplicação na Cena do Cliente:**
   - Na Main Thread, busca o `ObservedPlayer` pelo `NetId`.
   - Se a ação for `ConvertToBackpack`:
     - Desativa os renderers do `ObservedPlayer.PlayerBody`.
     - Instancia a mochila localmente na posição sincronizada.
   - Se a ação for `DestroyCorpse`:
     - Remove do dicionário `ObservedPlayersCorpses`.
     - Invoca `Dispose()` e destrói o GameObject local do `ObservedPlayer`.

---

## 2. Riscos e Mitigações

1. **Risco:** Falha se um cliente conectar após a conversão de um cadáver (Late Joiner).
   - **Mitigação:** Cadáveres convertidos ou removidos mantêm registro no Host para que novos clientes sincronizem o estado correto durante a carga inicial do mapa.
2. **Risco:** Quebra de compilação quando o FIKA não estiver presente no SPT do usuário.
   - **Mitigação:** Todo o código dependente do assembly do FIKA deve ser isolado via reflexão ou compilado sob bridge opcional para garantir funcionamento transparente em modo solo sem FIKA.
