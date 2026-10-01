# 003 — Chuva Fraca Mais Visível (Tamanho Mínimo de Gota) · Review Técnica 01

**Mod:** TRL-WeatherSync  
**Spec técnica revisada:** [003-chuva-fraca-visivel-02-spec-tech.md](003-chuva-fraca-visivel-02-spec-tech.md)  
**Data:** 2026-09-12  

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM` (review 01, ponto MM). Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 3 · Total: 3

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C | 🟡 | Risco de spam de log no console a cada frame em caso de falha no Postfix | ✅ Resolvido em 2026-09-12 |
| PA-01-02 | B | 🟡 | Distorção de proporção das gotas em resoluções com aspecto não-quadrado | ✅ Resolvido em 2026-09-12 |
| PA-01-03 | A | 🟢 | Null-guard antecipado para `___material_0` antes da leitura do vetor | ✅ Resolvido em 2026-09-12 |

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

### PA-01-01 · C — Erro de Lógica · 🟡 Importante · ✅ Resolvido em 2026-09-12

**Risco de spam de log no console a cada frame em caso de falha no Postfix**

**Problema:** O método `RainFallDrops.method_2` é executado todo frame quando há chuva (`RainFallDrops.cs:101-108`). Em uma implementação de Postfix com `try/catch` padrão, caso ocorra alguma exceção (ex: material destruído ou nulo durante transição de cena), o `Log.LogError` seria chamado 60 a 144 vezes por segundo, degradando severamente o FPS do jogador e inflando os arquivos de log.

**Por que importa:** Pode transformar um erro pontual de renderização em travamentos severos de framerate durante a raid.

**Sugestão:** Incorporar uma flag estática de supressão (`private static bool _hasLoggedError`) no corpo do patch, garantindo que qualquer falha eventual seja registrada apenas uma única vez no log.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Adotada a flag estática `_hasLoggedError` no stub da spec técnica (§5), assegurando zero impacto de log spam em runtime.

---

### PA-01-02 · B — Edge Case · 🟡 Importante · ✅ Resolvido em 2026-09-12

**Distorção de proporção das gotas em resoluções com aspecto não-quadrado**

**Problema:** O código vanilla do EFT (`RainFallDrops.cs:135-137`) multiplica explicitamente o componente X do vetor de tamanho pela razão de aspecto da tela: `float x = (float)Screen.height / (float)Screen.width; a = Vector2.Scale(a, new Vector2(x, 1f));`. Se o patch aplicasse um piso simples `new Vector2(minSize, minSize)` sem considerar a razão de aspecto, as gotas ficariam desproporcionalmente largas em monitores widescreen (16:9) e ultrawide (21:9).

**Por que importa:** A gota de chuva perderia o aspecto de fio vertical e pareceria uma barra grossa distorcida.

**Sugestão:** Multiplicar o piso mínimo do eixo X por `(float)Screen.height / (float)Screen.width` antes de comparar com `currentSize.x`, mantendo exatamente a convenção espacial utilizada pelo shader nativo da BSG.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Fórmula `float minWidth = minSize * ((float)Screen.height / (float)Screen.width)` formalizada e incluída nos stubs (§5) e fluxo de dados (§6) da spec técnica.

---

### PA-01-03 · A — Gap · 🟢 Menor · ✅ Resolvido em 2026-09-12

**Null-guard antecipado para `___material_0` antes da leitura do vetor**

**Problema:** Durante o carregamento inicial da raid ou em transições de clima, o campo `material_0` pode estar em processo de instanciação por `RainFallDrops.Init()`. Chamar `___material_0.GetVector()` sem checagem prévia dispararia `NullReferenceException`.

**Por que importa:** Gera exceções evitáveis durante ciclos de transição de cena.

**Sugestão:** Incluir `if (minSize <= 0f || ___material_0 == null) return;` logo no topo do Postfix antes de acessar métodos do material.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Guarda nula antecipada adicionada ao stub da spec técnica (§5).

---

## Histórico

| Data | Evento |
|---|---|
| 2026-09-12 | Review técnica 01 concluída; 3/3 achados resolvidos (zero bloqueadores 🔴 em aberto). |
