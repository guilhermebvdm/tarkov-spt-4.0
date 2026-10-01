# 002 — Watchdog de Timeout de Inventário e Desync em Coop/Headless · Code Review 01

**Mod:** FIKA  
**Spec funcional:** [002-inventory-desync-watchdog-01-spec.md](002-inventory-desync-watchdog-01-spec.md)  
**Spec técnica:** [002-inventory-desync-watchdog-02-spec-tech.md](002-inventory-desync-watchdog-02-spec-tech.md)  
**Asbuild:** [002-inventory-desync-watchdog-05-asbuild.md](002-inventory-desync-watchdog-05-asbuild.md)  
**Data:** 2026-09-05  

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 3 · Total: 3

**Memória consultada:** topo de `mods/FIKA/memory/sessions.md` (Sessão 1) · pendências que afetam: nenhuma bloqueadora; [P-1.1] (validação de rede cooperativa Headless) diretamente relacionada a este cenário.

---

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | B — Bug latente | 🟡 Médio | Pacote de servidor tardio pós-timeout emite `LogError` espúrio | ✅ Aplicado em 2026-09-05 |
| CR-01-02 | B — Bug latente | 🟡 Médio | `_baseInventoryController.StrictSync` vulnerável a NRE em inicialização prematura | ✅ Aplicado em 2026-09-05 |
| CR-01-03 | F — Melhoria opcional | 🟢 Menor | `CALLBACK_TIMEOUT_SECONDS` hardcoded como constante privada | ✅ Aplicado em 2026-09-05 |

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

1. **Validação de API Pública:**
   - Confirmado em `FikaPlayer.cs:57` que `public readonly Dictionary<uint, Action<ServerOperationStatus>> OperationCallbacks` e em `FikaPlayer.cs:65` que `public bool WaitingForCallback` mantêm assinaturas e visibilidade originais. Nenhuma dependência externa quebra.
2. **Ciclo de Vida e Memória:**
   - Os dicionários de timestamp `_operationCallbackTimestamps` e `_proceedCallbackTimestamps` são instanciados por instância do jogador (`FikaPlayer`), sendo descartados pelo GC ao término da raid (sem retenção estática).
3. **Prevenção de Loops de Reentrada:**
   - Em `CleanupExpiredCallbacks()`, as entradas são removidas dos dicionários antes do disparo de `callback?.Invoke(...)` ou `callback?.Fail(...)`, prevenindo reavaliações cíclicas durante o unwind de inventário.

---

## Pontos

### CR-01-01 · Cat B — Bug latente · 🟡 Médio

**Pacote de servidor tardio pós-timeout emite `LogError` espúrio**

**Local:** [`mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/Players/FikaPlayer.cs:1905-1920`](../../modded/Fika-Plugin/Fika.Core/Main/Players/FikaPlayer.cs#L1905-L1920)

**Problema:**
No método `HandleCallbackFromServer`:
```csharp
if (OperationCallbacks.TryGetValue(operationCallbackPacket.CallbackId, out var callback))
{
    ...
}
else
{
    FikaGlobals.LogError($"Could not find CallbackId: {operationCallbackPacket.CallbackId}!");
}
```
Se uma operação demorar mais de 5 segundos devido a um pico severo de latência ou processamento lento no Host/Headless, o watchdog no cliente a expira e remove seu ID. Quando o pacote finalmente chega da rede, o ID não existe mais em `OperationCallbacks`, caindo no `else` e gerando um `LogError` que polui os logs e induz a falsas impressões de falha de rede.

**Por que importa:**
Dificulta a análise de logs de suporte de usuários, pois o erro parece um desync catastrófico quando na verdade é apenas o descarte ordenado de uma resposta tardia de um callback já expirado com segurança.

**Sugestão:**
Implementar um conjunto circular ou HashSet de tombstones (`_recentlyTimedOutOperationIds`) para diferenciar um ID nunca existente de um ID expirado pelo watchdog:
```csharp
if (_recentlyTimedOutOperationIds.Remove(operationCallbackPacket.CallbackId))
{
    FikaGlobals.LogWarning($"[Fika-Watchdog] Callback tardio {operationCallbackPacket.CallbackId} recebido do servidor e ignorado (já auto-drenado por timeout).");
    return;
}
```

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-02 · Cat B — Bug latente · 🟡 Médio

**`_baseInventoryController.StrictSync` vulnerável a NRE em inicialização prematura**

**Local:** [`mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/Players/FikaPlayer.cs:69`](../../modded/Fika-Plugin/Fika.Core/Main/Players/FikaPlayer.cs#L69)

**Problema:**
No getter de `WaitingForCallback`:
```csharp
if (!_baseInventoryController.StrictSync)
{
    return false;
}
```
Se `WaitingForCallback` for consultado por algum mod externo ou rotina de monitoramento antes da chamada de inicialização que atribui `_baseInventoryController` (em `FikaPlayer.Init`), o dereferenciamento dispara `NullReferenceException`.

**Por que importa:**
Mods de terceiros com listeners de eventos podem consultar o estado do jogador durante a fase de spawn ou no instanciamento inicial do GameObject.

**Sugestão:**
Adicionar verificação defensiva de null:
```csharp
if (_baseInventoryController?.StrictSync != true)
{
    return false;
}
```

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-03 · Cat F — Melhoria opcional · 🟢 Menor

**`CALLBACK_TIMEOUT_SECONDS` hardcoded como constante privada**

**Local:** [`mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/Players/FikaPlayer.cs:240`](../../modded/Fika-Plugin/Fika.Core/Main/Players/FikaPlayer.cs#L240)

**Problema:**
A janela de tolerância de 5 segundos está fixada como:
```csharp
private const float CALLBACK_TIMEOUT_SECONDS = 5.0f;
```

**Por que importa:**
Em servidores dedicados globais com rotas internacionais de alta latência ou conexões instáveis com SPT-Headless remoto, pode ser desejável ajustar o limiar para 8–10 segundos sem necessidade de recompilação binária do plugin.

**Sugestão:**
Tornar a propriedade estática configurável internamente ou via `FikaPlugin.Instance`:
```csharp
internal static float CallbackTimeoutSeconds { get; set; } = 5.0f;
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
| 2026-09-05 | Code review 01 criada via `/code-review`. 0🔴 / 0🟠 / 2🟡 / 1🟢. Sem bloqueadores 🔴. |
