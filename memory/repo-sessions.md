# Memory — Repo-wide (tarkov-spt-4.0)

Memória cronológica de trabalho **transversal ao repo** (infraestrutura de agents, commands, skills, scripts, convenções, templates) — coisas que NÃO pertencem a um mod específico.

Trabalho específico de cada mod fica em `mods/<mod>/memory/sessions.md`. Este arquivo é o complemento top-level para o que afeta o workflow geral.

> Por que existe: o usuário trabalha múltiplos chats em paralelo. Sessões de manutenção de meta-infraestrutura precisam ser rastreáveis tanto quanto trabalho em código. Entradas em ordem cronológica GMT-3.

## Estado atual (snapshot ao fim da última sessão)

- **Workflow de backlog:** `/add-backlog-item` → `/create-spec` → `/review-spec` (inline edits) → `/create-technical-spec` → `/review-technical-spec` (NN incremental) → `/code-mod` (gera `05-asbuild.md`) → `/code-review` (NN incremental) → `/apply-code-review` → `/compile-mod`.
- **Convenção de naming canônica:** `NNN-<slug>-MM-tipo[-NN].md` onde `MM` é a posição no ciclo: `01-spec`, `02-spec-tech`, `03-spec-tech-review-NN`, `04-code-review-NN`, `05-asbuild`, `06-fix-NN`.
- **Skills ativas:**
  - `spt-mod-best-practices` — lifecycle SPT 4.0 / EFT 0.16.x, raid hooks, leaks, Harmony.
  - `csharp-mod-best-practices` — C# / runtime para BepInEx.
  - `repo-workflow-best-practices` — convenção de naming, rastreabilidade PA-NN-MM/CR-NN-MM, sandbox `modded/` vs `original/`.
  - `memory-curation` — regras de redação para `sessions.md` / `repo-sessions.md`.
- **Commands custom:** `/add-backlog-item`, `/create-spec`, `/review-spec`, `/create-technical-spec`, `/review-technical-spec`, `/code-mod`, `/code-review`, `/apply-code-review`, `/compile-mod`, `/add-mod-repo-for-modding`, `/update-mods-inventory`, `/add-mod-inventory-list`, `/update-memory`.
- **Mods no repo (5):** `stancesAndCameraPositionSPT4.0.11` (ativo), `SPT-Realism-Mod-Client` (vendor pinned), `SPT-DynamicMaps` (vendor pinned), `RZCustomProfiles` (vendor pinned), `RZ-SPTMods` (vendor pinned).
- **Tools cross-cutting:** `tools/tarkov-itemdb/` — DB unificada (SPT + tarkov.dev + tarkov-market) com viewer HTML pra calibração manual do flea. Edit pelo viewer atualiza `prices.json` + `checks.dat` (MD5) + audit log. Env: `SPT_PATH`, `TARKOV_MARKET_API_KEY`. Detalhes em [tools/tarkov-itemdb/README.md](../tools/tarkov-itemdb/README.md) + [tools/tarkov-itemdb/docs/spt-internals.md](../tools/tarkov-itemdb/docs/spt-internals.md).
- **LiveFleaPrices mod:** **desativado** (renomeado `<SPT>/user/mods/DrakiaXYZ-LiveFleaPrices.disabled/`) — substituído por calibração manual via viewer. `prices.json` hoje é autoral.
- **Memory system:** ativo. 5 pastas `mods/*/memory/` + 1 top-level. Sessions com timestamps GMT-3 HH:MM (relógio do sistema via `Bash date '+%Y-%m-%d %H:%M'`).

## Pendências / próximos passos conhecidos

- **Item 002 do stances mod aguarda validação in-raid de F4** após 06-fix-01 (ver `mods/stancesAndCameraPositionSPT4.0.11/memory/sessions.md`).
- **Drift potencial no asbuild do stances mod** (referência a `06-fix-02` não rastreável nesta sessão) — investigar antes de gerar fix-02 novo com numeração duplicada.

## 2026-05-11 02:00 (GMT-3) — Sessão 1b: validação end-to-end do memory system

**Tema central:** primeira execução real de `/update-memory --all` + fechamento de pendências da Sessão 1a.

**Decisões-chave:**

- **Backfill com tilde aceito como convenção definitiva** para sessões pré-existentes sem timestamp HH:MM. Reforça §2 da skill `memory-curation`: relógio do sistema é fonte de verdade para entradas novas; `~HH:MM` é o marcador honesto para inferências históricas.
- **Mod-level sessions.md NÃO ganham nova entrada** quando o único delta da sessão é edição de timestamp em header data-only (refactor de formato, não trabalho novo). Aplicação prática do skill §12 ("não criar entrada vazia").

**Atividade cronológica:**

1. Backfill em `mods/stancesAndCameraPositionSPT4.0.11/memory/sessions.md` — 3 headers ganharam `~16:00`, `~14:00`, `~00:30`.
2. Backfill em `mods/SPT-Realism-Mod-Client/memory/sessions.md` — header ganhou `~18:00`.
3. Primeira invocação de `/update-memory --all` — esta entrada é o output canônico do fluxo end-to-end.

**Pendências abertas nesta sessão:** nenhuma — Sessão 1a fechou limpa.

**Cross-refs:**

- Resolve [P-1.1 🟢] da Sessão 1a (backfill de `~HH:MM` em sessions.md existentes).
- Resolve [P-1.2 🟢] da Sessão 1a (primeira execução real de `/update-memory --all`).

## 2026-05-11 ~01:55 (GMT-3) — Sessão 1a: criação do memory system + skill + command `/update-memory`

**Tema central:** introduzir camada de memória cronológica por mod + top-level para evitar releitura completa de chat em sessões paralelas.

**Decisões-chave:**

- **Estrutura dupla:** `mods/<mod>/memory/sessions.md` (escopo do mod) + `memory/repo-sessions.md` (meta-infra). Justificativa: 80% do trabalho do usuário é por mod; o resto é repo-wide e merecia arquivo próprio em vez de poluir cada `sessions.md`.
- **Merge cronológico por posicionamento, não fusão de parágrafos** (chats paralelos no mesmo dia): cada sessão vira sub-letra (`Sessão Na`, `Nb`, `Nc`) com timestamp GMT-3; posicionamento no arquivo é por timestamp, sub-letras são IDs estáveis (ordem de gravação).
- **Timestamps obrigatoriamente HH:MM**, obtidos via `Bash date '+%Y-%m-%d %H:%M'` (relógio do sistema = fonte de verdade). Backfill de sessões anteriores aceita `~HH:MM` aproximado com tilde, mas entradas novas devem ser exatas.
- **Auto-detect com confirmação ON por default**: o command varre a conversa, classifica por mod via hierarquia de §3 da skill, propõe plano, pede `y/N`. Modo `--all` skipa o prompt (uso script).
- **Append-only**: nunca editar texto de entrada existente. Reposicionamento move bloco inteiro. Snapshot "Estado atual" é o único campo reescrito (delta, não acumulação).

**Atividade cronológica:**

1. Usuário pediu criação de pastas `memory/` por mod e arquivo cronológico GMT-3 — criadas 5 pastas, populadas com templates.
2. Discussão de nome do command — confirmado `/update-memory` com auto-detect.
3. Skill `memory-curation` redigida em 13 seções + checklist final, cobrindo granularidade, classificação por mod, merge de chats paralelos, pendências tri-camada (🔴🟡🟢), imutabilidade, snapshot delta, densidade de refs.
4. Decisões do usuário registradas: (a) top-level também; (b) merge por posicionamento sem fusão; (c) confirmação ON.
5. Command `/update-memory` criado em `.claude/commands/update-memory.md`, consumindo a skill, com 4 modos (`<mod>`, `--all`, `--repo`, `--dry`).
6. Skill §2 + §10 e command passo 4 ajustados para HH:MM obrigatório após pedido específico do usuário.
7. `memory/repo-sessions.md` (este arquivo) criado.

**Pendências abertas nesta sessão:**

- [P-1.1 🟢] Backfill dos `sessions.md` existentes com `~HH:MM` aproximados nos headers data-only.
- [P-1.2 🟢] Primeira execução real de `/update-memory --all` para validar o fluxo end-to-end.

**Cross-refs:**

- Trabalho paralelo no mod stances neste mesmo dia: ver `mods/stancesAndCameraPositionSPT4.0.11/memory/sessions.md` §"2026-05-11 — Sessão 3".
- Renomeação de convenção de naming (16 arquivos) foi registrada no mod stances (cobaia), mas o impacto é repo-wide — ver lista de mudanças repo-wide naquela sessão.

## 2026-05-11 ~00:30 (GMT-3) — Sessão 0: renomeação convenção + skill `repo-workflow` + commands `/code-review` e `/apply-code-review`

> Reconstruída por backfill — timestamps aproximados a partir do contexto dos commits e da sessão de trabalho.

**Tema central:** consolidar convenção de naming dos artefatos de backlog e formalizar a fase de code-review como ciclo independente (criar review imutável, depois aplicar com IDs CR-NN-MM em comentários inline).

**Decisões-chave:**

- **Convenção `NNN-<slug>-MM-tipo[-NN].md`** adotada como única — antes existia variação `<slug>-spec.md` vs `<slug>-technical-review-NN.md` sem prefixo numérico de posição. Justificativa: ordem visual no `ls`/IDE bate com ordem do ciclo.
- **Code review formalizada em 2 etapas**: `/code-review` cria `04-code-review-NN.md` imutável (6 categorias × 4 impactos, IDs `CR-NN-MM` permanentes); `/apply-code-review` aplica achados marcados, adiciona comentários `// ref: CR-NN-MM` no código tocado, anota Resolução na review original. Reviews jamais reescritas.
- **`/code-mod` passa a gerar `05-asbuild.md`** ao final — antes não havia documento canônico de "o que foi entregue".
- **Item 003 do stances** ganhou nota de "exceção documentada" (pulou tech-spec/review por trivialidade — não vira precedente para itens normais).

**Atividade cronológica:**

1. Script `scripts/migrate-backlog-naming.sh` redigido e executado: 16 arquivos renomeados, 14 .md com refs internas atualizadas via sed.
2. Skill `repo-workflow-best-practices` criada em `.claude/skills/repo-workflow-best-practices/SKILL.md`.
3. Template `.agents/templates/code-review.md.tmpl` criado.
4. Template `.agents/templates/asbuild.md.tmpl` criado.
5. Commands `/code-review`, `/apply-code-review` criados em `.claude/commands/`.
6. Commands existentes (`create-spec`, `review-spec`, `create-technical-spec`, `review-technical-spec`, `code-mod`) atualizados para nova convenção.
7. Mod stances usado como cobaia: artefatos 001, 002, 003 renomeados para nova convenção.

**Pendências abertas nesta sessão:** nenhuma — fechamento limpo da infra.

**Cross-refs:**

- Aplicação prática no item 002 do stances: ver `mods/stancesAndCameraPositionSPT4.0.11/memory/sessions.md` §"2026-05-10 — Sessão 2" (CR-01-01 a CR-01-06).

## 2026-05-16/17 (GMT-3) — Sessão: criação do `tools/tarkov-itemdb/` + calibração de flea

**Tema central:** construir uma base de dados unificada de itens (SPT local + tarkov.dev + tarkov-market) com viewer HTML, e usá-la pra desativar o mod `DrakiaXYZ-LiveFleaPrices` e calibrar `prices.json` manualmente. Originalmente nasceu como suporte ao RZCustomProfiles (precisávamos de imagens + preços validados pra montar loadouts), mas evoluiu pra meta-DB pessoal antes da integração com o mod.

**Decisões-chave:**

- **Pipeline em camadas** (`tools/tarkov-itemdb/scripts/`): `fetch-tarkov-dev` → `fetch-tarkov-market` → `load-spt` → `normalize` → `build.js` (orquestrador). Caches `cache/*-raw.json` gitignored, output `data/items.json` versionado em formato "1 linha por Tpl" (diffs estáveis em ~14 MB).
- **`priceFleaCanonical` com prioridade `tarkov-market avg24h > tarkov.dev avg24h > tarkov.dev lastLow > spt`** — sem blending. SPT é o último fallback **por design** (canonical existe pra servir de referência externa durante calibração; usar SPT seria circular).
- **Sufixo de métrica nos nomes** (`priceFleaDevLastLow` vs `priceFleaDevAvg24h`) — explícito pra evitar comparações enviesadas entre fontes que medem coisas diferentes.
- **Validação `checks.dat` reverse-engineered**: base64 + JSON 2-space + MD5 hex uppercase + trailing newline. Hash refresh automático no startup do `serve.js` e após cada edit do viewer.
- **Source of truth do flea = `<SPT>/SPT_Data/database/templates/prices.json`** (não o cache do mod, não o `data/items.json`). Viewer reescreve esse arquivo + atualiza `checks.dat` + sincroniza `data/items.json` + grava audit log em `logs/price-edits.jsonl`.
- **LiveFleaPrices desativado renomeando DLL** (`.dll.disabled`), não deletado — reversível. Folder também renomeada (`.disabled`) pra dupla segurança contra varredura recursiva do loader.
- **Convenção viewer**: click em célula de preço (Flea SPT) abre editor inline; click em outras células expande detalhe (linha-detail). Toast no canto pra feedback.

**Atividade cronológica:**

1. **Planejamento iterativo (5 passes de revisão do plano):** corrigiu naming bug crítico (`priceFleaSptLastLow` baked-in assumption que SPT é lastLow — sobreviveria só enquanto LiveFleaPrices ativo; renomeado para `priceFleaSpt` agnóstico).
2. **Probe schema tarkov.dev** via GraphQL: confirmou `id`, `normalizedName`, imagens (3 tamanhos), `sellFor`/`buyFor` por vendor com `priceRUB`. Categorias via root query `itemCategories` (não `categories`).
3. **`load-spt.js`** parseou items.json (4.462 reais, 120 nodes descartados), prices.json (2.613 entries inicial), handbook (4.216 itens + 87 categorias), 12 traders + assorts. Resolveu nome via locale `en.json`. Detectou flea-banned via `_props.CanSellOnRagfair` (649) + `ragfair.dynamic.blacklist.custom` (8).
4. **Pipeline rodou**: 5.630 Tpls na union, 3.650 com 3 fontes, 4.339 tradeable.
5. **Spot checks** revelaram bug: IFAK `conditionType: "none"` — meds usam `MaxHpResource`, não `MaxResource`. Fix em `deriveConditionType`.
6. **Viewer HTML construído**: sidebar de categorias (com ícones emoji + auto-expand depth ≤ 0 + skip de intermediários "Compound item" / "Searchable item"), tabela com 9 colunas + filtros + busca, click expande detalhe, indicadores ▲▼ % vs Flea SPT.
7. **Investigação do mod LiveFleaPrices** (fonte do prices.json desfasado): código no GitHub revelou que mod baixa de repo estático do Drakia (`SPT-LiveFleaPriceDB`), não tarkov.dev direto. Refresh hardcoded em 1h via background task após boot. Config tinha `pvePrices: false` (puxando PVP) + última sync em 2026-03-02 (76 dias antes).
8. **Confusão temporária com duplicidade `<SPT>/user/` vs `<SPT>/SPT/user/`**: o caminho ativo é o segundo. Usuário deletou o primeiro confirmando que amigos não tinham essa pasta.
9. **Calibração definitiva:** copiou `prices-pve.json` (fresh, 3.245 entries) sobre `prices.json` no SPT_Data. Desativou mod (rename DLL + folder). `load-spt.js` revertido pra ler só `prices.json` canônico (sem mais lógica de cache do mod).
10. **`checks.dat` validation reverse-engineered** após erro de boot do SPT: MD5 hex uppercase em base64 JSON. Função `updateSptChecks()` em `serve.js` atualiza hashes idempotentemente, chamada no startup e após cada edit. Também resolve warning preexistente de `items.json` (modificado por algum mod desde 10/maio — origem desconhecida, hash apenas atualizado).
11. **Edit endpoint `POST /api/price`**: valida tpl + price, escreve prices.json com indent 4 + newline, sincroniza items.json, recalcula `consolidated`, atualiza checks.dat, anexa JSONL ao audit log. Frontend com edit-form inline (Enter/Esc), toast de feedback (verde 3s / vermelho 5s).
12. **Fika Discord Presence** quebrou porque `<SPT>/user/logs/` sumiu junto com a pasta deletada. Corrigido `LogFolderPath` no config pra `<SPT>/SPT/user/logs/`.
13. **Documentação consolidada** ao fim da sessão: README expandido (setup, viewer/edit, troubleshoot, re-habilitar mod), novo `docs/spt-internals.md` (checks.dat, LiveFleaPrices upstream, trader assort gotchas, 3 taxonomias de categoria, locale shape), apêndice em `.agents/workspace.md` (env vars + tool registrado), esta entrada.

**Pendências abertas nesta sessão:**

- [P-2.1 🟡] Cache do tarkov-market é o snapshot de 2026-05-04 (copiado do `mods/RZCustomProfiles/scripts/cache/`). Refetch real exige `TARKOV_MARKET_API_KEY` — usuário tem mas não setou env nesta sessão.
- [P-2.2 🟡] Integração de volta com `mods/RZCustomProfiles/`: o backlog tem 10 perfis montados com `anchor-items.json`, mas o `data/items.json` é mais rico. Substituir referências do backlog e gerar os `.json` reais dos perfis fica pra próxima sessão.
- [P-2.3 🟢] Outra máquina rodando o server SPT: outros 2 colaboradores precisam de `SPT_PATH` correto + chave de API + pipeline build. Setup documentado no README "Setup em máquina nova".

**Cross-refs:**

- Workspace registrou o tool: `.agents/workspace.md` §"Tools de apoio" e §"Env vars".
- Reverse-engineering reusável: `tools/tarkov-itemdb/docs/spt-internals.md` (consulta futura em sessões de debug do SPT mesmo sem o tool — checks.dat e LiveFleaPrices são gerais).
- Item 002 do stances continua aberto (P-1.3 da Sessão 1b) — sem progresso aqui.

**O que NÃO foi feito (escopo cortado intencionalmente):**

- `prices.json` semantics presumida = lastLow: presunção empiricamente quebrada (M4A1 SPT=132k vs dev-lastLow=30k antes da troca). Mod escrevia mistura de avg + multiplier histórico. Documentado como "semântica opaca" em vez de bake-in.
- Filtro real "só 10/10" pra itens com condição variável (chaves, durabilidade) — não exposto pelas APIs públicas, fora de escopo. Mitigação atual: campo `conditionType` ("none" / "uses" / "durability" / "resource") + badge visual.
- Câmbio dinâmico USD/EUR → RUB: lido do handbook (USD=120, EUR=133 em SPT 4.0.13). Estático. Re-rodar `build.js` re-lê se valores mudarem.
