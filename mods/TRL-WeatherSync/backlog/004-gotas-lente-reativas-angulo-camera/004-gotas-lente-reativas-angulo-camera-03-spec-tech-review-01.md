# 004 — Gotas na Lente Reativas ao Ângulo da Câmera · Review Técnica 01

**Mod:** TRL-WeatherSync  
**Spec técnica revisada:** [004-gotas-lente-reativas-angulo-camera-02-spec-tech.md](004-gotas-lente-reativas-angulo-camera-02-spec-tech.md)  
**Data:** 2026-09-12  

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de prosseguir para implementação.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 3 · Total: 3

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C | 🟡 | Esgotamento temporário do pool `Queue_0` caso rajadas muito grandes sejam solicitadas | ✅ Resolvido em 2026-09-12 |
| PA-01-02 | B | 🟡 | Null-guard em `Transform_0` e referências de lista durante transição de cena | ✅ Resolvido em 2026-09-12 |
| PA-01-03 | A | 🟢 | Fallback explícito para o código original do jogo via retorno `true` no Prefix | ✅ Resolvido em 2026-09-12 |

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

**Esgotamento temporário do pool `Queue_0` caso rajadas muito grandes sejam solicitadas**

**Problema:** O pool `Queue_0` de `GClass986` é instanciado nativamente com `_dropsAmount = 32` gotas ([`RainScreenDrops.cs:28`](../../../../references/eft-decompiled/Assembly-CSharp/RainScreenDrops.cs#L28)). Se o jogador olhar para o céu e o patch tentar descarregar rajadas excessivamente altas (ex: 20 a 30 gotas instantâneas), o pool esvaziaria imediatamente, travando novos pingos até que as gotas morressem.

**Por que importa:** Causaria uma rajada rápida seguida por uma pausa abrupta e não natural onde nenhuma gota consegue nascer.

**Sugestão:** Limitar a quantidade máxima de gotas geradas por rajada via `Mathf.Clamp(..., 1, 12)` e assegurar que cada iteração verifique `__instance.Queue_0.Count > 0` antes de invocar `method_1()`. Além disso, o parâmetro `MaxDropLifetimeSeconds` (padrão 8s) garante que as gotas recicladas retornem rapidamente para a fila.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Limitador de rajada máxima em 12 a 16 gotas e checagem de contagem incorporados na lógica do patch.

---

### PA-01-02 · B — Edge Case · 🟡 Importante · ✅ Resolvido em 2026-09-12

**Null-guard em `Transform_0` e referências de lista durante transição de cena**

**Problema:** Em transições de câmera (ex: abertura de menus, câmeras de morte ou recarregamento de cena), `Transform_0` ou `Camera_0` podem se tornar nulos antes da destruição do componente.

**Por que importa:** Poderia gerar `NullReferenceException` no Prefix durante o cálculo do produto escalar.

**Sugestão:** Incluir guards explícitas `if (__instance.Transform_0 == null) return true;` antes de acessar vetores e propriedades de câmera.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Adicionadas validações defensivas no corpo do stub da spec técnica (§5).

---

### PA-01-03 · A — Gap · 🟢 Menor · ✅ Resolvido em 2026-09-12

**Fallback explícito para o código original do jogo via retorno `true` no Prefix**

**Problema:** Caso o jogador desative a opção no F12 (`EnableLensDropsTuning = false`) ou ocorra alguma exceção no bloco `try`, o mod deve garantir que o jogo continue funcionando normalmente sem interromper o efeito visual da câmera.

**Por que importa:** Evita que a tela fique sem efeito de chuva se o usuário optar por desligar o mod ou em caso de erro.

**Sugestão:** Garantir que o Prefix retorne `true` tanto quando o toggle estiver desmarcado quanto dentro do bloco `catch`.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Implementado retorno `true` no início se desativado e no bloco `catch` como airbag.

---

## Histórico

| Data | Evento |
|---|---|
| 2026-09-12 | Review técnica 01 concluída; 3/3 achados resolvidos, zero bloqueadores 🔴. |
