# 001 — Desmembramento de Perna em Bots Vivos · Review Técnica 01

**Mod:** VisceralCombat
**Spec técnica revisada:** [001-alive-leg-dismemberment-02-spec-tech.md](001-alive-leg-dismemberment-02-spec-tech.md)
**Data:** 2026-08-11

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM` (review 01, ponto MM). Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 1 · ✅ Resolvidos: 1 · Total: 1

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | B — Edge Case | 🟢 Menor | Zero-vector warning ao encolher o osso da coxa | ✅ Resolvido |

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

### PA-01-01 · B — Edge Case · 🟢 Menor

**Zero-vector warning ao encolher o osso da coxa**

**Problema:** Encolher o osso da perna para `0.001f` faz o solver de animação C++ da Unity emitir `Look rotation viewing vector is zero`.

**Por que importa:** Pode poluir os logs de console durante a raid.

**Sugestão:** Ajustar `RagdollHelperClass.limbSize` para `Vector3(0.1f, 0.1f, 0.1f)`, que atende o threshold de precisão em ponto flutuante de 32-bit da Unity sem exibir visualmente o osso amputado.

**Decisão:**
- `[x]` Aceitar sugestão
**Resolução:** Resolvido em `RagdollHelperClass.cs` definindo `limbSize = Vector3(0.1f, 0.1f, 0.1f)`.
