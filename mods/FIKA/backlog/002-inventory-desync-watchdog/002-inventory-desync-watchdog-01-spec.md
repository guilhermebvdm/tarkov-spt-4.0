# 002 — Watchdog de Timeout de Inventário e Desync em Coop/Headless · Spec Funcional

**Mod:** FIKA  
**Status:** 🟢 Concluído  
**Data:** 2026-09-05  
**Autor:** Antigravity / saraiva  

---

## 1. Contexto e Problema

Em sessões cooperativas do FIKA (especialmente com servidor dedicado Headless ou com latência de rede variável), quando um jogador realiza ações de recarga (especialmente via inventário ou drag-and-drop de carregadores) ou operações de movimentação de itens que por qualquer motivo perdem a resposta ou confirmação do servidor (`OperationCallbackPacket` ou `ProceedResponsePacket`):

```text
[FikaPlayer] WaitingForCallback == true
OperationCallbacks.Count > 0 ou _proceedCallbacks.Count > 0
```

### Sintomas observáveis:
1. **Perda de controles de tiro e arremesso:** O jogador não consegue mais disparar armas primárias, secundárias ou pistolas de sinalização (flares), e não consegue arremessar granadas.
2. **Carregador piscando no inventário:** O magazine recarregado ou movido fica piscando indefinidamente no inventário, indicando operação pendente no cliente.
3. **Persistência de ações secundárias e Melee:** O jogador ainda consegue manipular armas (inspeção de câmara, checagem de munição) e atacar com faca/machado (Melee), pois essas ações não consultam `WaitingForCallback`.
4. **Ausência de mecanismo de auto-recuperação:** Se o pacote de resposta for perdido, o cliente fica em deadlock indefinido até que o jogador reinicie o cliente ou extraia da raid.

---

## 2. Objetivo

Implementar um mecanismo de auto-expiração (Watchdog com timeout) no `Fika.Core` que:
1. Rastreie o tempo de vida de callbacks pendentes de inventário e de proceed (`OperationCallbacks` e `_proceedCallbacks`).
2. Finalize graciosamente operações que excedam 5 segundos sem resposta do Host/Headless com status de falha de rede (`EOperationStatus.Failed`), restaurando imediatamente `WaitingForCallback = false`.
3. Destrave instantaneamente o gatilho (`CanPressTrigger`) e o arremesso de granadas (`CanThrow`).
4. Mantenha 100% da compatibilidade binária e semântica com todos os mods do ecossistema que dependem do `Fika.Core`.

---

## 3. Critérios de Aceite (AC)

- **AC-1:** Se uma operação de inventário enviada ao servidor não for confirmada em até 5 segundos, ela deve ser auto-drenada com `EOperationStatus.Failed`.
- **AC-2:** Ao expirar, `WaitingForCallback` deve retornar `false` no frame seguinte, liberando o gatilho (`CanPressTrigger`) de armas e o lançamento de granadas (`CanThrow`).
- **AC-3:** Se o callback de proceed (`_proceedCallbacks`) expirar após 5 segundos, ele deve ser finalizado com `Fail`, cancelando o estado de espera de mãos.
- **AC-4:** Nenhuma assinatura pública, campo público ou tipo do `Fika.Core` deve ser modificado, preservando compatibilidade com outros mods (`TRL-DynamicSpawn`, `stancesAndCameraPositionSPT4.0.11`, etc.).
- **AC-5:** Operações normais que respondem antes de 5 segundos não sofrem interferência.
