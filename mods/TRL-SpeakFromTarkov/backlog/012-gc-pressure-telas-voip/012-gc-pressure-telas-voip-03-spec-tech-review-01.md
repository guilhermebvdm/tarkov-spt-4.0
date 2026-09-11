# 012 — GC Pressure nas Telas de VOIP · Review Técnica 01

**Mod:** TRL-SpeakFromTarkov
**Spec técnica revisada:** [012-gc-pressure-telas-voip-02-spec-tech.md](012-gc-pressure-telas-voip-02-spec-tech.md)
**Data:** 2026-09-09

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 1 · Total: 1

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | A — Gap | 🟢 Menor | Stub de `Update()` não mostra onde entra em relação ao `if (IsOpen)` já existente | ✅ Resolvido |

## Categorias

- **A — Gaps de Especificação:** informações ausentes que ambiguam a implementação
- **B — Edge Cases:** cenários válidos não cobertos
- **C — Erros de Lógica:** pressupostos errados, contradições, código incompatível com SPT 4.0+

## Impacto

- 🔴 **Bloqueador** — impede implementar ou causa bug/crash garantido
- 🟡 **Importante** — pode causar comportamento errado em cenário relevante
- 🟢 **Menor** — qualidade/clareza, não bloqueia

---

## Pontos

### PA-01-01 · ✅ Resolvido em 2026-09-09 · A — Gap · 🟢 Menor

**Stub de `Update()` não mostra onde entra em relação ao `if (IsOpen)` já existente**

**Problema:** Confirmei em `UI/PlayerVolumeMixerHUD.cs:237-252` que `Update()` **já existe** e tem toda a sua lógica atual (fechar cursor, `Escape` fecha o mixer) dentro de um único `if (IsOpen) { ... }`. O stub da seção 5 mostra o throttle de `FlushPendingSave()` como se fosse adicionado a um `Update()` vazio, sem indicar se entra dentro ou fora desse bloco condicional existente.

**Por que importa:** Não muda o comportamento funcional (com `Close()` já flushando de imediato, `_saveDirty` nunca fica `true` enquanto `IsOpen == false` sob o design da spec) — é só uma questão de clareza pra quem for implementar não precisar decidir isso na hora.

**Sugestão:** Ajustar o stub da seção 5 pra mostrar o método completo:
```csharp
void Update()
{
    if (IsOpen)
    {
        if (Cursor.lockState != CursorLockMode.None) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        if (Input.GetKeyDown(KeyCode.Escape)) { Close(); }

        // NOVO — throttle de persistência, ver FlushPendingSave()
        if (_saveDirty && Time.realtimeSinceStartup - _lastSaveRealtime >= SaveThrottleSeconds)
        {
            FlushPendingSave();
        }
    }
}
```

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Stub de `Update()` na spec técnica agora mostra o método completo, com o throttle dentro do bloco `if (IsOpen)` existente.

---

## Memória consultada

Snapshot de `mods/TRL-SpeakFromTarkov/memory/sessions.md` (v1.5.3, sem pendências 🔴/🟡 relacionadas a este item).
