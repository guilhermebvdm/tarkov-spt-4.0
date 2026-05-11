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
