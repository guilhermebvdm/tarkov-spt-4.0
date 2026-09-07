# 001 — Fix descarte de mochila ("ZZ" / DropBackpack) no Headless/Host · Spec Funcional

**Mod:** FIKA  
**Status:** 🟢 Concluído  
**Data:** 2026-09-03  
**Autor:** Antigravity / saraiva  

---

## 1. Contexto e Problema

Em sessões multiplayer com servidor dedicado **Fika Headless** (ou Host dedicado), quando um jogador remoto utiliza a tecla de atalho de descarte rápido de mochila (duplo toque em `Z` — comando nativo `DropBackpack`), o item cai no chão no cliente que descartou, mas o servidor Headless rejeita a operação com o seguinte erro:

```text
[Error  : Fika.Core] [HandleResult]: Error in operation: hands controller can't perform this operation
[Error  :Fika.Server] ItemControllerExecutePacket::Operation conversion failed: Could not find item owner with id: ...
```

### Sintomas observáveis:
1. A mochila cai no chão, porém seu inventário e propriedade física ficam em estado inconsistente ("desync").
2. Ao tentar interagir ou pegar a mochila de volta do chão, o jogador não consegue pegá-la, ou a mochila fica inacessível.
3. Tentativas subsequentes de mover itens da mochila disparam rejeições sucessivas com perda de propriedade do item (`Could not find item owner with id`).

---

## 2. Objetivo

Permitir que qualquer jogador conectado a uma raid multiplayer (seja via Headless ou Host coop) descarte sua mochila pelo comando rápido `DropBackpack` ("ZZ") com total sincronização de rede, sem que o `HandsController` do proxy remoto (`ObservedPlayer`) rejeite o descarte.

---

## 3. Critérios de Aceite (AC)

- **AC-1:** Ao pressionar "ZZ" para dropar a mochila em raid multiplayer conectada ao Headless, a mochila deve se desprender e cair normalmente no chão.
- **AC-2:** O servidor Headless não deve registrar logs de erro `hands controller can't perform this operation` durante o descarte da mochila.
- **AC-3:** O jogador (ou qualquer colega de equipe) deve conseguir pegar a mochila do chão e equipá-la ou abrir seu inventário sem erros de `Could not find item owner`.
- **AC-4:** O descarte de armas diretamente das mãos do jogador remoto continua funcionando sem regressões.
- **AC-5:** O jogador local em sessão singleplayer ou cliente local não tem seu comportamento de animação afetado.
