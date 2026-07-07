# /update-memory

Atualiza a memória cronológica de sessão de chat em `mods/<mod>/memory/sessions.md`. Detecta automaticamente quais mods foram tocados na sessão atual, pergunta confirmação antes de gravar, e insere cada entrada na ordem cronológica correta (suporta chats paralelos no mesmo dia).

> **Skill obrigatória:** carregar `memory-curation` antes de redigir as entradas. Toda regra de granularidade de timestamps, classificação de pendências, densidade de refs, snapshot delta e merge por posicionamento vem dela.

> Trabalho meta-repo (infra, commands, skills) não tem mais destino dedicado — `memory/repo-sessions.md` foi descontinuado em 2026-07-06 (ver `.claude/skills/memory-curation/SKILL.md` §1). `git log` é a fonte de verdade para esse histórico.

## Uso

```bash
/update-memory [<mod>] [--all] [--dry]
```

- `/update-memory` (sem args) — **auto-detect**: o command varre a conversa atual, classifica trechos por mod usando a hierarquia de §3 da skill, e propõe um plano de atualização para o usuário **confirmar** antes de gravar.
- `/update-memory <mod>` — atualiza só `mods/<mod>/memory/sessions.md`, filtrando da conversa só o que é pertinente a esse mod.
- `/update-memory --all` — atualiza todos os mods detectados. Sem prompt de confirmação (modo batch).
- `/update-memory --dry` — mostra o que seria escrito (preview do delta) sem efetivar. Combinável com qualquer alvo.

## O que fazer

### 1. Classificar a conversa por escopo

Aplicar a hierarquia de §3 da skill `memory-curation`:

1. **Path explícito** (peso alto): edits/reads/grep que tocaram `mods/<X>/...` → trecho pertence a `<X>`.
2. **Command direcionado** (peso alto): `/code-mod <X>`, `/compile-mod <X>`, `/add-backlog-item <X>`, etc. Define "mod ativo" até outro command direcionado mudar.
3. **Menção textual** (peso médio).
4. **Trabalho meta-repo ou não-mod** (peso baixo): edits em `.claude/`, `.agents/`, `scripts/`, ou ações que não tocam nenhum mod. **Sem destino dedicado no repo** — descartar OU, se relevante, registrar como "Notas relevantes (não-mod)" no mod em foco.

### 2. Apresentar o plano de atualização

Se `/update-memory` foi chamado sem `--all`, **sempre perguntar antes de gravar**:

```text
📋 Detectei trabalho nos seguintes escopos nesta sessão:

  - mods/stancesAndCameraPositionSPT4.0.11 (N ações relevantes, M decisões-chave)

Plano de atualização:
  1. mods/stancesAndCameraPositionSPT4.0.11/memory/sessions.md
     → Nova entrada "Sessão 4 (2026-05-11, 14h GMT-3) — <título>"
     → Inserida AO FINAL (timestamp > entradas existentes deste dia)

Confirmar gravação? [y/N]
```

Se houver entradas do mesmo dia já existentes, calcular o **posicionamento cronológico correto** e mostrar na proposta (ver §10 da skill).

### 3. Detectar entradas pré-existentes do mesmo dia (merge cronológico)

Para cada arquivo de destino:

1. Ler `sessions.md` se existir.
2. Procurar headers `## YYYY-MM-DD ...` que correspondem ao dia da sessão atual.
3. Determinar a **sub-letra** da nova entrada:
   - Se não há entradas para o dia: nova entrada começa em `Sessão N` (próximo número crescente nesse mod) **sem sub-letra**.
   - Se há `Sessão N` ou `Sessão Na`: nova entrada é `Sessão Nb` (próxima letra disponível).
   - Se há `Sessão Na, Nb`: nova entrada é `Sessão Nc`.
4. Determinar a **posição no arquivo**:
   - Comparar timestamp GMT-3 da nova entrada com timestamps das entradas existentes do mesmo dia.
   - Inserir antes da primeira entrada com timestamp posterior, OU ao final se for o último cronologicamente.
   - **Sub-letras não precisam coincidir com ordem visual** — sub-letra é ID estável (ordem de gravação), posição é por timestamp.
5. **NUNCA editar texto de entradas existentes.** Append-only de novas entradas; reposicionamento é só ordem (mover bloco inteiro, sem fundir conteúdo).

### 4. Obter timestamp atual

**Antes de redigir a entrada**, rodar:

```bash
date '+%Y-%m-%d %H:%M'
```

para obter `YYYY-MM-DD HH:MM` exato do relógio do sistema (assumido como GMT-3 — fuso local do usuário). **Não estimar** — o relógio é a fonte de verdade. Se por algum motivo a chamada falhar, parar e pedir o horário ao usuário em vez de inventar.

### 5. Redigir a entrada

Seguir o template da skill §5:

```markdown
## YYYY-MM-DD HH:MM (GMT-3) — Sessão N[<letra>]: <título resumido>

**Tema central:** [1 linha]

**Decisões-chave:**
- [Decisão 1]: <o quê> — <por quê>. Ref: <file:linha ou artefato>.

**Lições / hipóteses descartadas:**
- <hipótese/abordagem testada> — falhou/foi descartada porque <causa raiz>. Ref: <artefato/file:linha>.

**Atividade cronológica:**
1. <ação> — <resultado>.

**Pendências abertas nesta sessão** (se houver):
- [P-N.M] <descrição>. Categoria: 🔴 bloqueador / 🟡 débito / 🟢 ideia.

**Cross-refs:**
- Resolve [P-X.Y] de YYYY-MM-DD (se aplicável).
- Trabalho paralelo em outro mod: ver `mods/<outro>/memory/sessions.md` YYYY-MM-DD.
```

**A seção "Lições / hipóteses descartadas" é obrigatória** (skill §5). Antes de gravar, varrer a conversa por: bugs diagnosticados (com causa raiz), abordagens revertidas, premissas refutadas ("tentamos X, falhou porque Y"). Se nada qualifica, registrar a justificativa explícita: `Nenhuma lição nova — sessão de <tipo>.` Sessão que tocou código sem decisão "— porquê" nem lição é red flag — voltar à conversa e extrair o raciocínio.

### 6. Atualizar "Estado atual" e "Pendências" no topo

Aplicar regra de **delta, não acumulação** (skill §6):

- Reescrever os bullets do snapshot para refletir o **estado AO FIM** da sessão atual.
- Não acumular bullets antigos. Substituir os que mudaram, manter os que ainda são verdade.
- Pendências resolvidas: removidas do topo, marcadas com `✅ Resolvido em YYYY-MM-DD` na entrada que resolveu (com link bidirecional).
- **IDs obrigatórios:** todo bullet de pendência no topo carrega `[P-N.M]` (skill §7). Ao reescrever o topo, preservar IDs existentes; bullets legados sem ID recebem ID retroativo derivado da sessão de origem.
- **Alertas de tamanho:** snapshot ou pendências com >10 bullets → avisar na confirmação e propor consolidação/promoção a backlog; **>15 bullets → parar** e pedir decisão do usuário antes de gravar.

Para sessões longas, opcional incluir antes da reescrita uma seção curta:

```markdown
### Delta vs. último snapshot
- REMOVIDO: <fact>
- ADICIONADO: <fact>
- MUDOU: <field> de X para Y
```

### 7. Idempotência — verificar duplicidade antes de gravar

Antes de inserir uma nova entrada:

- Comparar o tema central + decisões-chave da nova entrada com as últimas 2-3 entradas do arquivo.
- Se ≥ 80% do conteúdo já existe (ex.: mesma sessão sendo registrada de novo): **avisar e parar**:
  ```
  ⚠️ Entrada Sessão Na (2026-05-11) já cobre o trabalho desta sessão.
     Última atualização: 5 minutos atrás.
     Nada novo a registrar.
  ```
- Se há **delta novo** (ações depois da última gravação): criar uma NOVA sub-letra (Sessão Nb), só com o delta.

### 8. Garbage collection de pendências (>30 dias)

A cada rodada (skill §7): **rodar `date '+%Y-%m-%d'`** e, para cada bullet `[P-N.M] (aberta YYYY-MM-DD)` do topo, subtrair a data inline da data atual (diff literal — não resolver ID→sessão). Para cada pendência **>30 dias sem progresso**, incluir no plano de atualização uma proposta com 3 opções:

```text
🗑️ GC: [P-2.1] "<descrição>" está aberta há 34 dias (Sessão 2, 2026-05-09).
   ( ) Promover a item de backlog (/add-backlog-item)
   ( ) Descartar — com nota "descartada por <razão>"
   ( ) Manter — justificar: ___
```

A decisão do usuário é registrada na entrada da sessão atual (não na entrada original — append-only).

### 9. Promoção de lições recorrentes

Aplicar skill §15: se a mesma classe de erro/lição aparece em **≥2 sessões** (deste mod ou de outros), incluir no relatório final:

```text
💡 Candidata a promoção: <lição> — vista em <Sessão X, Sessão Y>.
   Destino sugerido: docs/technical/spt-antipatterns.md §AP-NN / skill <nome>.
   Aprovar?
```

A edição do doc/skill só acontece com aprovação do usuário (trabalho repo-wide, sem registro dedicado — ver skill §1). A memória do mod ganha link para o destino promovido — não duplica.

### 10. Confirmar e gravar

Após confirmação do usuário (ou com `--all` que skipa):

1. Aplicar as edições nos arquivos de destino.
2. Reportar:
   ```text
   ✓ Memória atualizada — sessão YYYY-MM-DD
   Arquivos modificados:
     - mods/stancesAndCameraPositionSPT4.0.11/memory/sessions.md
       → Adicionada Sessão 4b (posicionada antes da 4a por timestamp 09h < 14h)
       → Snapshot "Estado atual" atualizado (3 bullets reescritos)
       → 2 pendências adicionadas (P-4b.1 🟡, P-4b.2 🟢)
       → 1 pendência resolvida: P-3.2 ✅ (criada em 2026-05-09 Sessão 3)
   Lições registradas: N (ou "ausência justificada")
   GC: K pendências >30d decididas
   Promoções propostas: M
   ```
3. **Gancho do grafo de código:** se a classificação do passo 1 detectou mudança de **código** em algum mod (não só docs/spec/memória), perguntar:
   ```text
   Houve mudança de código em <mod> nesta sessão — rodar /update-mod-graph <mod> agora? [y/N]
   ```
   Sessões só de docs/spec/memória não disparam a pergunta.

## Regras

- **Skill `memory-curation` é fonte única de verdade** para regras de redação. Este command é mecânica + workflow; conteúdo segue a skill.
- **Sandbox de gravação:** apenas `mods/<X>/memory/sessions.md`. Nunca toca outros arquivos.
- **Append-only para entradas existentes.** Reposicionamento move o bloco inteiro; nunca edita texto antigo (anti-revisionismo, skill §8).
- **Confirmação default ON.** Modo batch (`--all`) só por flag explícita, para uso em scripts.
- **Auto-detect conservador.** Quando ambíguo entre 2 mods, perguntar em vez de chutar.
- **NÃO criar entrada vazia.** Se a sessão não teve decisão tomada nem descoberta relevante (skill §12), avisar e não gravar:
  ```
  ⚠️ Sessão sem conteúdo qualificado para memória.
     Nenhuma decisão tomada, nenhuma descoberta, nenhuma mudança de código.
     Nada a registrar.
  ```

## Exemplos

```bash
# Auto-detect (caso comum): pergunta confirmação antes de gravar.
/update-memory

# Forçar só o mod stances.
/update-memory stancesAndCameraPositionSPT4.0.11

# Preview do que seria escrito, sem gravar.
/update-memory --dry

# Batch: atualiza todos os mods detectados, sem prompt.
/update-memory --all
```