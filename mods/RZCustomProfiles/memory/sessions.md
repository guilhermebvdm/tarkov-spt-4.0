# Memory — RZCustomProfiles

Memória cronológica de sessões de chat (timestamps em GMT-3, aproximados quando não puderem ser inferidos com precisão). Cada entrada resume o que foi feito. Atualizada ao fim de cada sessão de trabalho.

> Por que existe: o usuário trabalha múltiplos chats em paralelo. Este arquivo evita que cada chat reabra do zero — futuras sessões podem carregar contexto ao ler as últimas entradas.

## Estado atual (snapshot ao fim da última sessão)

Backlog [001-custom-profiles.md](../backlog/001-custom-profiles.md) tem **modelo de balanceamento ponderado** documentado: cada skill recebe multiplicador derivado da fórmula `BASELINE(15) / nivel_observado_no_lvl_43`, baseado em screenshots de personagem do usuário ([../assets/](../assets/)). Skills não observadas têm multiplicador por premissa (medical, sniper, night-ops, etc). Budget por classe: 18–22 pontos ponderados. As 10 classes foram re-calibradas — antes spread era 12.5–58.5 (4.7×), agora 19.6–21.8 (1.1×). Loadouts/inventário não foram tocados (escopo separado, ainda usam totais ~2M ₽ originais — pode ficar inconsistente quando inventário for revisitado).

Nenhum JSON em [../modded/profiles/](../modded/profiles/) ainda — só o `exampleProfile.json` template (idêntico em `original/`). Geração dos arquivos por classe é o próximo passo.

## Pendências / próximos passos conhecidos

- **(Próximo)** Gerar 10 arquivos `.json` em `modded/profiles/` a partir das composições re-balanceadas. Schema do `AdditionalStartingItems` é plano (`Tpl + Count`) — não está claro se suporta equipped/nested/slot; investigar antes de incluir loadout, ou começar só com skills.
- **(Aberto)** Revisitar tabelas de inventário/loadout do backlog (ainda calibradas para ~2M ₽ via 35 pontos antigos) — recalibrar para o novo target de poder "início de game" depois de validar skills em playtest.
- **(Aberto)** Validação empírica em playtest quando JSONs forem gerados — modelo de custo é design, não tem teste automatizado.

## Sessões

### 2026-05-16 — Balanceamento ponderado das 10 classes

- Identificado que tratar "1 ponto = 1 nível" no backlog antigo era injusto (ex: Metabolism 10 do Sobrevivencialista é praticamente grátis vs FirstAid 10 do Sanitarista que custa dezenas de horas).
- Usuário forneceu 4 screenshots do próprio personagem lvl 43 em [../assets/](../assets/) como dataset de referência.
- Definida fórmula `mult = BASELINE(15) / nivel_observado`, clamp `[0.25, 3.00]`. Tabela de multiplicadores criada com 30 skills observadas + 17 skills por premissa (FirstAid, Sniper, NightOps, Memory, Charisma, etc).
- Budget alvo: 18–22 pts ponderados por classe (target "início de game ~lvl 10-15"). Backlog atualizado com seção "Modelo de balanceamento" + tabelas de classe re-calibradas (Skill/Nível/Mult./Custo) + tabela "Referência rápida" atualizada.
- Plano: [C:\Users\guime\.claude\plans\precisamos-fazer-alguns-equilibrios-immutable-moonbeam.md](../../../../../Users/guime/.claude/plans/precisamos-fazer-alguns-equilibrios-immutable-moonbeam.md).
