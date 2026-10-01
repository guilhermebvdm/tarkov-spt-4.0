# 001 — Sincronização FIKA e Estabilidade de Mãos · Code Review 02

**Mod:** HandsAreNotBusy  
**Spec funcional:** *(item legado — sem spec funcional formal)*  
**Spec técnica:** *(item legado — sem spec técnica formal)*  
**Asbuild:** *(sem 05-asbuild.md formal)*  
**Review anterior:** [001-fika-sync-and-stability-04-code-review-01.md](001-fika-sync-and-stability-04-code-review-01.md)  
**Data:** 2026-09-05  

> Análise crítica das correções aplicadas em v1.7.3 para resolução da condição de corrida no Headless (`Default Inventory is currently being modified`), validação de resoluções de CR-01 e novos achados.

---

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 6 · Total: 6

**Status de Resoluções da Review Anterior (CR-01):**
- ✅ **CR-01-01 Resolvido:** `handsController?.Destroy()` foi realocado para antes de `player.SpawnController(emptyController)`. O controlador ativo não é mais destruído pós-spawn.
- ✅ **CR-01-02 Resolvido:** `_lastRegisteredNetworkManager?.UnregisterPacket<HanbClearInventoryPacket>()` adicionado a `OnNetworkManagerDestroyed`.
- ✅ **CR-01-06 Resolvido:** Comentários conceituais adicionados para `GEventArgs1` e `List_0` em `HANB_Component.cs:64-65`.

---

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-02-01 | B — Bug latente | 🟠 Forte | Ausência de debounce / flag de reentrada em `FixHandsController` permite corrotinas de reequipamento concorrentes | ✅ Aplicado em 2026-09-05 |
| CR-02-02 | B — Bug latente | 🟡 Médio | Acesso desprotegido a `targetPlayer.Profile.Nickname` em `HANB_FikaSync.cs` vulnerável a NRE | ✅ Aplicado em 2026-09-05 |
| CR-02-03 | F — Melhoria opcional | 🟢 Menor | Delay de 150ms em `ReequipRoutine` aplicado indiscriminadamente mesmo em Singleplayer local | ✅ Aplicado em 2026-09-05 |

---

## Categorias

- **A — Crítico** — bug grave, crash garantido, corrupção de estado, security issue.
- **B — Bug latente** — comportamento errado em cenário plausível, não acionado pelo caminho golden.
- **C — Gap vs. spec** — código não implementa critério de aceite, corner case, ou AC da spec.
- **D — Arquitetura** — viola padrões do repo, duplica código, leak de estado, abuso de reflection.
- **E — Legibilidade/manutenção** — nomes ruins, comentário "porquê" ausente, código morto, complexidade desnecessária.
- **F — Melhoria opcional** — refactor de qualidade, micro-otimização, simplificação.

## Impacto

- 🔴 **Bloqueador** — fix obrigatório antes de fechar o item.
- 🟠 **Forte** — fix recomendado; pode ser deferido para `06-fix-NN.md` futuro.
- 🟡 **Médio** — anotar, decidir caso a caso.
- 🟢 **Menor** — opcional.

---

## Confirmações de Evidência

1. **Eliminação do Erro no Headless:**
   - A introdução do delay assíncrono de 150ms (`yield return new WaitForSeconds(0.15f)`) em [HANB_Component.cs:121](../../modded/HANB_Component.cs#L121) garante que o pacote `HanbClearInventoryPacket` chegue e seja processado no servidor antes de o cliente solicitar `TrySetLastEquippedWeapon()`, eliminando o erro de rejeição de inventário em modificação.
2. **Resolução de Fallback no Host:**
   - O novo passo de busca em `_lastRegisteredNetworkManager.CoopHandler.Players` em [HANB_FikaSync.cs:139-142](../../modded/HANB_FikaSync.cs#L139-L142) cobre jogadores conectados ao servidor antes da instanciação de réplicas no `GameWorld`.

---

## Pontos

### CR-02-01 · Cat B — Bug latente · 🟠 Forte

**Ausência de debounce / flag de reentrada em `FixHandsController` permite corrotinas de reequipamento concorrentes**

**Local:** [`mods/HandsAreNotBusy/modded/HANB_Component.cs:46-50, 110`](../../modded/HANB_Component.cs#L46-L50)

**Problema:**
No método `Update`:
```csharp
if (HANB_Plugin.ResetKey.Value.IsDown())
{
    FixHandsController(_player);
}
```
Se o jogador pressionar repetidamente a tecla de reset com rapidez (ou mantiver pressionada caso ocorra repetição de evento), múltiplos ciclos de `FixHandsController` serão despachados. Como `ReequipRoutine` aguarda 150ms, a segunda execução destruirá o controlador enquanto a primeira corrotina ainda está em espera, disparando múltiplas tentativas concorrentes de equipar armas e resetar status de mãos.

**Por que importa:**
Pode causar nova colisão de operações de mãos no cliente ou deixar o jogador com animação procedural dessincronizada.

**Sugestão:**
Adicionar uma flag de guarda booleana `_isFixing` no componente:
```csharp
if (_isFixing) return;
_isFixing = true;
...
// No final de ReequipRoutine:
_isFixing = false;
```

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-02-02 · Cat B — Bug latente · 🟡 Médio

**Acesso desprotegido a `targetPlayer.Profile.Nickname` em `HANB_FikaSync.cs` vulnerável a NRE**

**Local:** [`mods/HandsAreNotBusy/modded/HANB_FikaSync.cs:174, 178`](../../modded/HANB_FikaSync.cs#L174)

**Problema:**
Nas linhas de log de informação do servidor:
```csharp
_logger?.LogInfo($"[HANB-Fika] Host limpou com sucesso {length} operações travadas no inventário de {targetPlayer.Profile.Nickname} ({packet.ProfileId}).");
```
e
```csharp
_logger?.LogInfo($"[HANB-Fika] Host verificou inventário de {targetPlayer.Profile.Nickname}: nenhuma operação travada em List_0.");
```
Se por qualquer motivo de ciclo de vida de rede o jogador remoto tiver `targetPlayer.Profile == null` no momento em que o pacote é tratado no Host, o acesso a `.Nickname` dispara uma `NullReferenceException`. Embora contida pelo `catch`, a exceção interrompe prematuramente a execução antes das linhas 185–186 (`targetPlayer.ProcessStatus = None; targetPlayer.SetInventoryOpened(false);`).

**Por que importa:**
Impede que o reset de estado de processo e o fechamento de inventário do jogador ocorram no servidor.

**Sugestão:**
Substituir por null-propagation segura com fallback para `ProfileId`:
```csharp
string nick = targetPlayer.Profile?.Nickname ?? packet.ProfileId;
```

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-02-03 · Cat F — Melhoria opcional · 🟢 Menor

**Delay de 150ms em `ReequipRoutine` aplicado indiscriminadamente mesmo em Singleplayer local**

**Local:** [`mods/HandsAreNotBusy/modded/HANB_Component.cs:121`](../../modded/HANB_Component.cs#L121)

**Problema:**
A corrotina sempre executa:
```csharp
yield return new WaitForSeconds(0.15f);
```
mesmo quando o jogador está em raid puramente local (SPT offline) sem conexão de rede ativa com servidor FIKA.

**Por que importa:**
Em singleplayer local, a sincronização é 100% imediata e em memória. O atraso de 150ms gera uma leve percepção de demora no reequipamento que só é estritamente necessária quando há latência de rede entre cliente e host/headless.

**Sugestão:**
Aguardar 150ms apenas quando conectado em rede como cliente, ou apenas 1 frame (`yield return null;`) em singleplayer:
```csharp
if (FikaBackendUtils.ClientType == EClientType.Client)
{
    yield return new WaitForSeconds(0.15f);
}
else
{
    yield return null;
}
```

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-05 | Code review 02 criada via `/code-review`. 0🔴 / 1🟠 / 1🟡 / 1🟢 / 3✅. Bloqueadores da CR-01 resolvidos. |
