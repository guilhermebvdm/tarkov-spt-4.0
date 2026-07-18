# /code-review

Análise crítica do **código implementado** por `/code-mod`. Cria um arquivo novo `NNN-<slug>-04-code-review-NN.md` a cada execução (NN incremental). Achados priorizados em 6 categorias × 4 impactos. Resolver bloqueadores via `/apply-code-review` antes de fechar o item.

> **Skills obrigatórias:** carregar `spt-mod-best-practices`, `csharp-mod-best-practices` e `repo-workflow-best-practices` antes de revisar. Use os checklists ao fim de cada skill como base mínima da análise. Consultar `memory-curation` § "Consumo de memória por commands" (§14) para o passo de contexto de memória.

## Uso

```bash
/code-review <ref>
```

`<ref>` segue as regras do `/create-spec` (path da pasta, path de arquivo, ou `<mod> <NNN>`).

## Pré-condições

1. Existir `<NNN>-<slug>-01-spec.md` e `<NNN>-<slug>-02-spec-tech.md`.
2. Ter pelo menos um `<NNN>-<slug>-03-spec-tech-review-NN.md` **sem bloqueadores 🔴 pendentes**.
3. `/code-mod <ref>` já executado. Verificação em ordem de preferência:
   - **(a) caminho normal:** existir `<NNN>-<slug>-05-asbuild.md` (item novo passou pelo `/code-mod` atualizado).
   - **(b) fallback para itens legados:** detectar arquivos modificados em `mods/<mod>/modded/` cujos paths batem com a coluna "Arquivo" do §4 da spec técnica. Se ≥ 50% dos arquivos esperados existem como modificados, considerar `/code-mod` concluído. Logar aviso recomendando regerar o asbuild numa próxima rodada do mod.

Se alguma pré-condição falhar, parar com mensagem clara.

## O que fazer

1. **Resolver `<ref>`** → `<mod>`, `<NNN>`, `<slug>`, `<path-pasta>`.

2. **Validar pré-condições** (lista acima).

3. **Calcular `NN` da review.** Listar `<NNN>-<slug>-04-code-review-*.md` na pasta. Próximo NN = maior + 1, padded a 2 dígitos. Primeira review = `01`.

4. **Ler:**
   - **Memória do mod** — topo de `mods/<mod>/memory/sessions.md` (snapshot + pendências) + entradas que citam o item `<NNN>`. Aplicar `memory-curation` § "Consumo de memória por commands" (§14): reportar pendências que afetam esta tarefa; pendência 🔴 do item/mod → alertar antes de prosseguir. Bug/lição registrada na memória que reaparece no código = achado com ref à sessão. Se o arquivo não existir, registrar "sem memória prévia".
   - Spec funcional, spec técnica, **todos** os reviews técnicos (para conhecer pontos `🟡`/`🟢` aceitos como ressalvas e quais PA-NN-MM foram aplicados no build).
   - Reviews de code-review anteriores (`04-code-review-*.md`) — pontos já `✅ Resolvido` ou `✅ Aplicado` **não voltam**; pontos pendentes podem ser revalidados.
   - `05-asbuild.md` (se existir) — para a lista canônica de arquivos tocados.
   - Os arquivos de `mods/<mod>/modded/` modificados/criados pelo item. Resolver via `05-asbuild.md` quando disponível; caso contrário, via §4 da spec técnica.
   - O Assembly nas linhas citadas pelos stubs da spec técnica — confirmar que o código implementado bate com as refs.
   - **Grafo do mod** (skill `graph-code-navigation`): `graphify affected "<classe/método tocado>"` para mapear callers afetados pelo diff — impacto além dos arquivos tocados.

5. **Análise crítica em 6 categorias × 4 impactos:**

   **Categorias:**
   - **A — Crítico** — bug grave, crash garantido, corrupção de estado, security issue.
   - **B — Bug latente** — comportamento errado em cenário plausível, não acionado pelo caminho golden.
   - **C — Gap vs. spec** — código não implementa um critério de aceite, corner case ou AC de verificação manual da spec funcional/técnica.
   - **D — Arquitetura** — viola padrões do repo (sandbox, hierarquia de fontes, reuso), duplica código existente, abuso de reflection, leak de estado entre raids.
   - **E — Legibilidade/manutenção** — nomes ruins, falta de comentário onde o "porquê" é obscuro, código morto, complexidade desnecessária.
   - **F — Melhoria opcional** — refactor de qualidade de vida, micro-otimização, simplificação.

   **Impactos:**
   - 🔴 **Bloqueador** — fix obrigatório antes de fechar o item. Status do item volta a 🟡 no `mod-backlog.md` até resolução.
   - 🟠 **Forte** — fix recomendado; pode ser deferido para `06-fix-NN.md` futuro.
   - 🟡 **Médio** — anotar, decidir caso a caso.
   - 🟢 **Menor** — opcional.

6. **Renderizar `.agents/templates/code-review.md.tmpl`** preenchendo:
   - `{{NUM}}`, `{{TITLE}}`, `{{SLUG}}`, `{{MOD}}`, `{{CREATED_AT}}`, `{{REVIEW_NN}}`.

7. **Adicionar pontos** no formato:

   ```markdown
   ### CR-NN-MM · Cat — Tipo · Impacto

   **Título resumido**

   **Local:** [`mods/<mod>/modded/<arquivo>.cs:<linha-faixa>`](../../modded/<arquivo>.cs#L<linha>)

   **Problema:** [descrição precisa, com snippet de 3–6 linhas quando aplicável]

   **Por que importa:** [consequência concreta — sintoma observável ou risco]

   **Sugestão:** [proposta acionável — diff ou descrição do que mudar. Específica o suficiente para o usuário aceitar com um "ok" ou contrapor]

   **Decisão:**
   - `[ ]` Pendente
   - `[ ]` Aceitar sugestão
   - `[ ]` Aceitar com modificação: _________________
   - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________
   ```

   - `NN` = número desta rodada de code-review.
   - `MM` = ordem do ponto nesta rodada (`01`, `02`, …).
   - IDs **permanentes**, nunca reutilizados entre rodadas.
   - **Toda sugestão deve ser acionável.** Não vale "revisar a abordagem"; vale "trocar `Foo()` por `Bar()` em `arquivo.cs:42` porque...".

8. **Atualizar o índice e contadores** no topo do arquivo de review.

9. **Reportar:**

   ```text
   ✓ Code review NN criada: <path>
     Memória consultada: snapshot de YYYY-MM-DD (Sessão N) · pendências que afetam: [P-N.M ...] / nenhuma
     🔴 Bloqueadores: N · 🟠 Fortes: N · 🟡 Médios: N · 🟢 Menores: N
   Status:
     [se houver 🔴]: item NÃO está pronto pra fechar — rode /apply-code-review primeiro.
     [se sem 🔴]: item pode ser fechado; pontos 🟠/🟡/🟢 são opcionais.
   Próximo passo:
     Marque "Aceitar sugestão" (ou Aceitar com modificação) para cada achado a corrigir.
     Rode /apply-code-review <ref> para aplicar — o command lê o último 04-code-review-NN.md.
     Achado deferido/regressão que virar fix pós-validação → usar .agents/templates/fix.md.tmpl (06-fix-NN).
   ```

## Regras

- **Sempre criar arquivo novo** — nunca editar review anterior. Reviews são artefatos imutáveis (só ganham anotações de Resolução posteriormente via `/apply-code-review`).
- Pontos `✅ Resolvido` / `✅ Aplicado` em reviews anteriores **não voltam** a ser levantados. Se um ponto pendente da review anterior foi tratado fora do `/apply-code-review` (ex.: fix manual), mencionar no novo arquivo: `✅ CR-01-03 resolvido fora do fluxo automatizado — fechado.`
- Cada ponto deve citar **o trecho exato do código** (com path + linha) ou o **arquivo:linha do Assembly** que sustenta a crítica. Análise sem evidência não vai.
- Se a spec técnica afirma `arquivo.cs:N` mas o código não implementa o stub correspondente, isso é Categoria C (Gap vs. spec). Se o código implementa mas o `arquivo.cs:N` do Assembly mudou ou não existe, isso é Categoria A (Crítico).
- **Referência ao EFT sem conceito (readiness 4.1).** Diff que introduz `GClassNNNN`/`GStructNNNN` hardcoded (`typeof`/`AccessTools`) sem comentário nomeando o conceito da [tabela de deofuscação](../../docs/files-from-4.1/consolidated-mappings.txt) → Categoria E (legibilidade). Se for nome **semântico que o 4.1 renomeia** (`ItemAttributeClass`, `DamageInfoStruct`, `NotificationManagerClass`, `HealthControllerClass` — **não** `EWeaponClass`/`Player`, estáveis/ausentes do mapa) → Categoria D. O reviewer nomeia o conceito; **não** exigir o FQN 4.1 pinado inline (AP-09). Ausência no mapa não é achado (sem entrada ≠ não existe).
- Versão alvo: SPT 4.0+ / EFT 0.16.x.