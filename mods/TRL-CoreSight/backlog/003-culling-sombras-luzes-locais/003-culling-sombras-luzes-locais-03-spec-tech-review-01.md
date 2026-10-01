# 003 — culling-sombras-luzes-locais · Review Técnica 01

**Mod:** TRL-CoreSight  
**Spec técnica revisada:** [003-culling-sombras-luzes-locais-02-spec-tech.md](003-culling-sombras-luzes-locais-02-spec-tech.md)  
**Data:** 2026-09-15T22:32:00Z  

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM` (review 01, ponto MM). Resolver até zerar bloqueadores antes de `/code-mod`.

---

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 2 · 🟢 Menores: 1 · ✅ Resolvidos: 3 · Total: 3

---

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | B — Edge Case | 🟡 Importante | Respeito ao estado `light.enabled` e lâmpadas destruíveis (`LampController`) | ✅ Resolvido em 2026-09-15 |
| PA-01-02 | C — Erro de Lógica | 🟡 Importante | Prevenção de Light Leaking em quartos escuros e banheiros | ✅ Resolvido em 2026-09-15 |
| PA-01-03 | A — Gap | 🟢 Menor | Telemetria de sombras locais otimizadas no overlay OnGUI de Debug | ✅ Resolvido em 2026-09-15 |

---

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

### PA-01-01 · B — Edge Case · 🟡 Importante

**Respeito ao estado `light.enabled` e lâmpadas destruíveis (`LampController`)**

**Problema:** No EFT, lâmpadas fluorescentes podem ser apagadas via interruptores em The Lab/Dorms ou explodidas por tiros (`LampController`). Se o mod tentasse manipular a propriedade `light.enabled`, poderia reativar lâmpadas que foram apagadas ou destruídas intencionalmente.

**Por que importa:** Reativar lâmpadas quebradas destrói a imersão e quebra a lógica do mapa.

**Sugestão:** O mod deve modificar **estritamente e apenas** a propriedade `light.shadows`. Jamais alterar `light.enabled` ou `light.intensity`. Se uma lâmpada for destruída ou apagada pelo jogo, seu componente continua desativado nativamente pelo EFT.

**Decisão:**
- `[x]` Aceitar sugestão  
**Resolução:** O gerenciador altera unicamente `light.shadows = LightShadows.None` ou `LightShadows.Hard`, sem tocar no estado de ativação da fonte de luz.

---

### PA-01-02 · C — Erro de Lógica · 🟡 Importante

**Prevenção de Light Leaking em quartos escuros e banheiros**

**Problema:** A desativação de sombras em luzes de alta intensidade ou raio longo faz a esfera de iluminação atravessar paredes sólidas finas (como divisórias de gesso em Dorms ou Resorts), iluminando cômodos que deveriam estar em completa escuridão.

**Por que importa:** O combate em Tarkov depende criticamente do contraste e de sombras táticas em corredores escuros.

**Sugestão:** Implementar filtro restritivo de intensidade ($\le 1.2$) e alcance ($\le 6.0$m) como padrão (`DisableWeakShadowsOnly`), além de disponibilizar o modo `DowngradeSoftToHard` que mantém a oclusão geométrica da sombra mas reduz o custo de amostragem na GPU.

**Decisão:**
- `[x]` Aceitar sugestão  
**Resolução:** Aprovada e incorporada a tripla estratégia na spec técnica (`DisableWeakShadowsOnly`, `DowngradeSoftToHard`, `DisableAllSecondaryShadows`).

---

### PA-01-03 · A — Gap · 🟢 Menor

**Telemetria de sombras locais otimizadas no overlay OnGUI de Debug**

**Problema:** Sem telemetria visual, o usuário não sabe quantas lâmpadas foram otimizadas na cena atual.

**Por que importa:** Facilita a auditoria em tempo real e a constatação da redução de carga na GPU.

**Sugestão:** Exibir a contagem `Optimized Lights: N / Total: M` na janela de debug (F11).

**Decisão:**
- `[x]` Aceitar sugestão  
**Resolução:** Métrica adicionada à exibição do `PerformanceManager.OnGUI()`.

---

## Histórico

| Data | Evento |
|---|---|
| 2026-09-15 | Review técnica 01 concluída: 0 bloqueadores 🔴, 2 importantes 🟡 resolvidos, 1 menor 🟢 resolvido. Aprovado para implementação de código (`/code-mod`). |
