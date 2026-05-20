# Memory — RZCustomProfiles

Memória cronológica de sessões de chat (timestamps em GMT-3, aproximados quando não puderem ser inferidos com precisão). Cada entrada resume o que foi feito. Atualizada ao fim de cada sessão de trabalho.

> Por que existe: o usuário trabalha múltiplos chats em paralelo. Este arquivo evita que cada chat reabra do zero — futuras sessões podem carregar contexto ao ler as últimas entradas.

## Estado atual (snapshot ao fim da última sessão)

**Item 001 (perfis customizados temáticos) ENTREGUE em código.** 10 arquivos JSON gerados em [../modded/profiles/](../modded/profiles/) via novo script [../scripts/build-profile-jsons.js](../scripts/build-profile-jsons.js). Cada perfil tem:
- Budget de skills 28-32 pontos ponderados (modelo `BASELINE(15) / nivel_observado_no_lvl_43`)
- 1 estação extra de hideout em L1 (apenas estações sem pré-requisitos — Heating, Security, Workbench, MedStation, RestSpace, Generator, WaterCollector) + Stash:1 padrão
- Loadout ~2M ₽ (baseline + tema + primary + 3 backups) com regra de stack stack-aware (`AdditionalStartingItems` plano: cada item agregado, dividido em N entradas conforme stackMaxSize do EFT)
- Traders inalterados (decisão arquivada — opção de adicionar Standing/SalesSum para LL2 foi explorada e abortada)

Validações automatizadas no script confirmam: custo ponderado em [28, 32] para todas, total ₽ em [1.95M, 2.05M] para todas, ≤6 skills > 0, ≤10 por skill, encoding UTF-8 sem BOM. Item count por arquivo: 71-98 (após stack expansion).

Pendências de validação **empírica** (requerem ambiente SPT rodando — não bloqueiam asbuild):
- Smoke test do comportamento do mod ao receber `Count > stackMaxSize` (a regra atual é defensiva)
- Deploy num SPT 4.0.13 e verificar os 5 critérios de aceite da spec funcional in-game (launcher mostra 10 perfis, skills exatas, hideout temático, loadout no stash, traders inalterados)
- Verificar capacidade do stash inicial — se transbordar (10×28 slots = 280), aplicar mitigação documentada na spec técnica

## Pendências / próximos passos conhecidos

- **(Próximo)** Rodar [/compile-mod](../../) — não aplicável aqui (mod é só JSONs).
- **(Próximo)** Rodar [/code-review](../../) sobre o build.
- **(Validação empírica)** Smoke test + playtest in-game (ver §7 da spec técnica + checklist §8).
- **(Decisão arquivada)** Traders LL2 — 3 opções foram discutidas (manter como está / `StartingLevel` global / `StartingLevel` por classe). Pendente decisão futura caso queira ativar.

## Sessões

### 2026-05-17 — Item 002 entregue (redesign de skills com budget por categoria)

- **20 skills mortas no SPT 4.0.13** descobertas via audit do `globals.json` (todas com array vazio `[]`). Removidas do `SKILL_MULTS` no script — validação automática rejeita uso futuro.
- **Clamp do multiplicador ampliado de `[0.25, 3.00]` para `[0.25, 5.00]`** — skills observadas em nível 4 (Immunity, LightVests, HeavyVests, DMR) agora custam 3.75 em vez de 3.00 truncado.
- **Categorização Ph/M/C/P** explícita no script (`SKILL_CATEGORIES`). Validação garante cobertura mínima de 4 categorias por classe.
- **Cap de 6 skills removido** — vinha do 001 por inércia, usuário rejeitou. Saqueador/Gerente/Sobrevivencialista mantêm 7 skills.
- **Operador Noturno renomeado para Operador Furtivo** — NightOps/SilentOps/ProneMovement são todas mortas; tema reposicionado pra "stealth diurno" usando CovertMovement+Search+Perception.
- **Bug pego no build:** `StressResistance` é Ph, não M (eu tinha colocado erroneamente em M na spec). Médico de Combate ganhou `Attention 2` para cobrir M. Spec atualizada para refletir a categorização correta.
- 10 JSONs regenerados em `modded/profiles/` (operadorNoturno.json removido, operadorFurtivo.json criado) e deployados em `D:/SPT/SPT/user/mods/RZCustomProfiles/profiles/`. Validações OK (custo [28.61, 31.83], cobertura ≥1 por categoria, encoding UTF-8 sem BOM, slots ≤ 280).
- Etapas review-spec/technical-spec/code-review **puladas** a pedido do usuário — mod é só dados declarativos.

### 2026-05-17 — BaseProfile mudou para Zero to Hero (8) por overflow real

- Deploy do mod no SPT install (`D:/SPT/SPT/user/mods/RZCustomProfiles/profiles/`) revelou que **Standard (BaseProfile 0) já traz itens iniciais ocupando slots do stash**, somando com nosso loadout e causando overflow real (~14-17 itens por classe não conseguiam ser colocados — log "stash full, could not place").
- Decisão: trocar para **BaseProfile 8 (SPT Zero to Hero)** que começa com stash VAZIO. Toda a capacidade dos 280 slots fica disponível pro nosso loadout.
- Backup × 2 restaurado (anteriormente havia tentado × 1 como mitigação intermediária; depois revertido).
- Resultado: 4 classes ainda no warning >238/280 slots (Fuzileiro, Batedor, Sobrevivencialista, Saqueador) — mas agora sem itens do Standard somando junto, devem caber em playtest. Aguardando confirmação.

### 2026-05-17 — Code review 01 aplicada + restrição Stash:1

- 4 pontos da code review 01 aplicados (CR-01-01 a 04), 1 rejeitado (CR-01-05 falso alarme).
- Validação de slots (CR-01-04) detectou overflow real em 7/10 classes. Auto-bump para Stash:2 foi implementado inicialmente, depois **revertido** por decisão de design — "Stash precisa ficar em L1, sem auto-bump".
- Mitigação aplicada: `backupCount: 3 → 2` em todas as classes (Armeiro já estava em 2). Trade-off: total ₽ caiu de ~2.0M para 1.63M–2.02M; faixa de validação relaxada para [1.5M, 2.05M].
- Resultado: todas as 10 classes cabem em Stash:1 (213–257 slots). 4 classes próximas do limite (Fuzileiro 242, Batedor 239, Sobrevivencialista 245, Saqueador 257) — warning de margem para packing emitido.
- Arquivos novos versionados: [scripts/extract-item-data.js](../scripts/extract-item-data.js) + [scripts/item-data.json](../scripts/item-data.json) (subset de 100 TPLs com stackMax/dims). build-loadouts.js deletado.

### 2026-05-17 — Build do item 001 (10 perfis customizados)

- Workflow completo: spec funcional (01) + 2 revisões → spec técnica (02) + review 01 (7 pontos, todos resolvidos) → code-mod.
- Decisões de design importantes durante a sessão:
  - Loadouts entram **simplificados** (Opção 1): todos os itens no stash, sem equipped/nested/slot.
  - Hideout restrito a estações **sem pré-requisitos** (Heating, Security, Workbench, MedStation, RestSpace, Generator, WaterCollector) — Caçador trocou ShootingRange→Heating, Batedor IntelligenceCenter→Security, Saqueador ScavCase→Security, Gerente IntelligenceCenter→Heating.
  - Traders: abortado. Premissa "perfis não alteram traders" mantida.
  - Nome de arquivo: camelCase sem acentos. Schema completo (zeros explícitos = identidade do Standard).
- Descoberta importante via [tools/tarkov-itemdb](../../../tools/tarkov-itemdb/): `stackMaxSize` por TPL — itens críticos (meds, mags, weapons) têm stack=1. Spec atualizada com regra stack-aware: `stackMax==1 → N entradas Count:1`, `stackMax>1 → ceil(qty/stackMax) entradas`. Sem essa regra os JSONs teriam perda silenciosa de itens.
- Implementação: [../scripts/build-profile-jsons.js](../scripts/build-profile-jsons.js) novo — agrega recipes (BASELINE+tema+primary+backup×N), resolve anchor → bsgId, consulta tarkov-itemdb para stackMaxSize, emite UTF-8 sem BOM. Validação interna: custo ponderado [28,32], total ₽ [1.95M, 2.05M], ≤6 skills, ≤10 por skill.
- Output: 10 JSONs em `modded/profiles/` (71-98 entradas em Items[] cada).

### 2026-05-16 — Balanceamento ponderado das 10 classes

- Identificado que tratar "1 ponto = 1 nível" no backlog antigo era injusto (ex: Metabolism 10 do Sobrevivencialista é praticamente grátis vs FirstAid 10 do Sanitarista que custa dezenas de horas).
- Usuário forneceu 4 screenshots do próprio personagem lvl 43 em [../assets/](../assets/) como dataset de referência.
- Definida fórmula `mult = BASELINE(15) / nivel_observado`, clamp `[0.25, 3.00]`. Tabela de multiplicadores criada com 30 skills observadas + 17 skills por premissa (FirstAid, Sniper, NightOps, Memory, Charisma, etc).
- Budget alvo: 18–22 pts ponderados por classe (target "início de game ~lvl 10-15"). Backlog atualizado com seção "Modelo de balanceamento" + tabelas de classe re-calibradas (Skill/Nível/Mult./Custo) + tabela "Referência rápida" atualizada.
- Plano: [C:\Users\guime\.claude\plans\precisamos-fazer-alguns-equilibrios-immutable-moonbeam.md](../../../../../Users/guime/.claude/plans/precisamos-fazer-alguns-equilibrios-immutable-moonbeam.md).
