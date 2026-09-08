# Backlog — Skills-Extended

> Índice de itens de backlog. Cada linha aponta para uma pasta `NNN-<slug>/` com a spec funcional, técnica e revisões.

| # | Título | Resumo | Pasta | Status |
|---|---|---|---|---|
| 001 | Corrigir bugs críticos da auditoria 01 | 5 achados 🔴 críticos: patches que usam o MainPlayer em vez do dono real da ação (porta/movimento), null-check ausente numa coroutine multi-frame, e 2 race conditions no servidor (sacrifício cultista / geração de scav) em coop Fika | [001-corrigir-bugs-criticos-auditoria-01/](./001-corrigir-bugs-criticos-auditoria-01/) | 🟢 |
| 002 | Corrigir cap de medicamento por instância | Achado crítico `AUD-01-03` (desmembrado do item 001): cap de Field Medicine de qualquer estimulante usa a skill do MainPlayer em vez do dono real do efeito; exige mover o patch pro método de instância em vez do utilitário estático atual | [002-corrigir-cap-medicamento-instancia/](./002-corrigir-cap-medicamento-instancia/) | 🟢 |
| 003 | Corrigir achados altos da auditoria 01 | 9 achados 🟠 Alto: mutação de templates compartilhados (arma/remédio), gatilho sem gating de raid, cap de skill exibido incorreto, NRE em telas de trader, leak de evento sem teardown, dicionário sem limpeza no headless, e um bug de copy-paste em reflection não utilizada | [003-corrigir-achados-altos-auditoria-01/](./003-corrigir-achados-altos-auditoria-01/) | 🟢 |
| 004 | Corrigir achados restantes da auditoria 01 | 13 achados restantes: 6 🟡 Médio (bug comportamental remanescente, nullability escondida, alocações evitáveis), 5 🔵 Baixo (higiene de recursos, assinatura dormente, catch vazio) e 2 💡 Otimização (reflection não cacheada, reassert por frame) | [004-corrigir-achados-restantes-auditoria-01/](./004-corrigir-achados-restantes-auditoria-01/) | 🟢 |

## Legenda

- ⚪ Backlog · 🟡 Em progresso · 🟢 Entregue · 🔴 Cancelado

## Fluxo

1. `/add-backlog-item <mod> <descrição>` → cria entrada + invoca `/create-spec`
2. `/create-spec <ref>` → spec funcional (critérios de aceite + corner cases)
3. `/review-spec <ref>` → editor crítico da spec funcional
4. `/create-technical-spec <ref>` → pré-código com refs ao Assembly
5. `/review-technical-spec <ref>` → cria review-NN.md (incremental); resolver até zerar
6. `/code-mod <ref>` → implementa em `modded/`
