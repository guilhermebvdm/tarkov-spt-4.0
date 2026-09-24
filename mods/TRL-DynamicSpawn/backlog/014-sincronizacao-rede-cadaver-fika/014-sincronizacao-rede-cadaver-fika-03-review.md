# 014 — Revisão Técnica Crítica: Sincronização de Rede de Cadáver no FIKA

**Mod:** TRL-DynamicSpawn  
**Status:** ⚪ Backlog  
**Criado:** 2026-09-15T09:07:00-03:00  
**Ref:** `014-sincronizacao-rede-cadaver-fika`  

---

## 1. Análise Crítica de Complexidade e Risco

### 1.1. Dependência Direta do Item 013
* **Avaliação:** O Item 014 não deve ser implementado antes da conclusão e homologação do Item 013. Se a criação da mochila visual ou o snap de física no Host apresentar falhas locais, replicar esse estado para N clientes apenas multiplicará os bugs em rede.
* **Decisão:** O Item 014 permanece em estado ⚪ Backlog até que o Item 013 seja validado com sucesso.

### 1.2. Segurança de Threads (Unity Main Thread Dispatcher)
* **Avaliação:** A biblioteca de rede do FIKA (`LiteNetLib`) invoca os callbacks de recebimento de pacotes em uma thread de worker de I/O de rede. Qualquer tentativa de acessar `GameObject.Instantiate`, `Destroy` ou `Renderer.forceRenderingOff` direto nessa thread causará uma exceção fatal:
  `UnityEngine.UnityException: Internal_CreateGameObject can only be called from the main thread.`
* **Requisito Obrigatório de Arquitetura:**
  - O deserializador de rede deve apenas extrair uma struct de dados pura (`CorpseEventData`) e depositá-la em uma fila thread-safe (`ConcurrentQueue<CorpseEventData>`).
  - Um componente `MonoBehaviour` no cliente (ex.: `CorpseClientReceiver : MonoBehaviour`) deve drenar a fila em seu `Update()` na Main Thread da Unity.

### 1.3. Destruição do Inventário de Saque
* **Ponto Crítico:** Quando o Host destrói o `corpsePlayer` (`DestroyCorpse`), o inventário no servidor deixa de existir.
* **Trade-off:** No modo `Backpack Convert`, o inventário permanece vivo na memória do Host, permitindo que convidados continuem saqueando. No modo `Destroy Corpse`, o saque é perdido tanto no Host quanto nos convidados. O mod deve garantir que se um convidado estiver no meio da busca de loot (tela de inventário aberta) quando o tempo de vida expirar, a limpeza deve ser adiada para não desconectar ou travar a UI do convidado.

---

## 2. Parecer e Próximos Passos

A spec técnica do Item 014 é robusta e viável. Fica aprovada em backlog técnico, condicionada à entrega prévia das correções de física e visual do Item 013.
