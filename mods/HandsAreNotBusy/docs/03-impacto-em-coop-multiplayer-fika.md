---
title: "HandsAreNotBusy — Impacto em Coop Multiplayer e FIKA"
date: 2026-09-04
status: 🟢 Vivo
authors: Antigravity
---

# HandsAreNotBusy — Impacto em Coop Multiplayer e FIKA

Embora o **HandsAreNotBusy** seja eficaz em ambientes puramente singleplayer para destravamento local de armas, sua aplicação em sessões multiplayer cooperativas (via **FIKA Client / Server / Headless**) introduz graves riscos de **descompasso cliente-servidor** e **deadlock irreversível de inventário**.

---

## 1. Origem do Descompasso Arquitetural

No FIKA, todas as transições de troca de mãos e operações de inventário operam sob modelo autoritativo cliente-servidor:
1. O cliente inicia uma ação e envia um pacote `ProceedRequestPacket` ou `InventoryPacket` ao Host/Headless.
2. O servidor valida se a entidade remota (`ObservedPlayer`) pode executar a transição e se o inventário está livre de travas.

Quando o usuário aciona o HANB via `ResetKey`:
- O HANB executa `inventoryController.RemoveActiveEvent(queuedEvent)` **apenas no cliente local**.
- **O Host/Headless não recebe nenhuma notificação desse expurgo.**
- No servidor, os eventos pendentes continuam registrados em `TraderControllerClass.List_0` da réplica do jogador.

```mermaid
sequenceDiagram
    participant C as Convidado (Cliente)
    participant H as HANB Plugin
    participant S as Host / Servidor FIKA

    Note over C,S: Operação de Reload Falha (Not enough space in target)
    C->>C: Carregador fica piscando (Evento em List_0)
    S->>S: Servidor mantém réplica com evento em List_0
    C->>H: Pressiona ResetKey (End)
    H->>C: Limpa List_0 localmente ("Cleared 3 stuck inventory operations")
    Note over C: Cliente acredita estar livre
    C->>S: ProceedRequestPacket (Puxar AK-104 / Glock)
    S->>S: CheckItemAction() -> Encontra evento antigo na List_0 do servidor!
    S-->>C: HandleCallbackResponse: "Cannot apply item ... because Default Inventory is currently being modified"
    Note over C: Mãos travam definitivamente; o tiro não sai
```

---

## 2. Sintomas Observados em Partidas Reais

Conforme auditado em logs de produção multiplayer (Reserve):
1. **Piscamento Permanente de Itens:** O item envolvido na falha (carregador) pisca sem parar.
2. **Rejeição em Cascata de Callbacks:**
   ```text
   [Error : Fika.Core] [HandleCallbackResponse]: Could not execute callback with id 9 on the server: Player cannot equip item with id ... because item Default Inventory is currently being modified
   ```
3. **Incapacidade Permanente de Disparar:** Como o servidor rejeita a ativação da arma nas mãos, a arma não fica armada na simulação de rede e os tiros não são computados.

---

## 3. Recomendações e Diretrizes Técnicas

- **Para Ambientes Multiplayer FIKA:** O destravamento deve ser sincronizado via pacote de rede bidirecional que ordene ao servidor limpar também os `ActiveEvents` da réplica do jogador correspondente.
- **Prevenção na Origem:** Impedir recargas (`ReloadMag`) quando não houver slots compatíveis disponíveis no colete, redirecionando para descarte no chão (`QuickReloadMag`).
