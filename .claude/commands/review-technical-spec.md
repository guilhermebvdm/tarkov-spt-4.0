# /review-technical-spec

Análise crítica da spec técnica. **Cria** um arquivo novo `NNN-<slug>-03-spec-tech-review-NN.md` a cada execução (NN incremental). Resolver até zerar bloqueadores antes de `/code-mod`.

> **Skills obrigatórias:** carregar `spt-mod-best-practices`, `csharp-mod-best-practices` e `repo-workflow-best-practices` antes de revisar. Use os checklists ao fim de cada skill como base mínima da revisão. Consultar `memory-curation` § "Consumo de memória por commands" (§14) para o passo de contexto de memória.

## Uso

```
/review-technical-spec <ref>
```

`<ref>` segue as regras do `/create-spec`.

## O que fazer

1. **Resolver `<ref>`** → `<path-pasta>`, `<NNN>`, `<slug>`, `<mod>`.

2. **Pré-condição.** Existir `<NNN>-<slug>-02-spec-tech.md` com conteúdo real. Se não, parar.

3. **Calcular `NN` da review.** Listar arquivos `<NNN>-<slug>-03-spec-tech-review-*.md` na pasta. Próximo NN = maior + 1, padded a 2 dígitos. Primeira review = `01`.

4. **Ler:**
   - **Memória do mod** — topo de `mods/<mod>/memory/sessions.md` (snapshot + pendências) + entradas que citam o item `<NNN>`. Aplicar `memory-curation` § "Consumo de memória por commands" (§14): reportar pendências que afetam esta tarefa; pendência 🔴 do item/mod → alertar antes de prosseguir. Pendências 🟡 relacionadas que a spec técnica não endereça são candidatas a ponto Categoria A (Gap). Se o arquivo não existir, registrar "sem memória prévia".
   - A spec técnica completa.
   - A spec funcional (`<NNN>-<slug>-01-spec.md`) — para conferir se a spec técnica responde aos critérios de aceite.
   - Reviews anteriores `<NNN>-<slug>-03-spec-tech-review-*.md` — pontos já resolvidos não devem ser refeitos; pontos pendentes podem ser revalidados.
   - Os arquivos do Assembly citados na spec técnica — confirmar que as linhas batem com o que a spec afirma.
   - A seção **9. Conformidade com skills** da spec técnica — conferir cada evidência citada. Check marcado ✅ sem evidência verificável = ponto **Categoria C — 🔴 Bloqueador**; check N/A com razão frágil = ponto Categoria A.
   - **Grafo de código** (skill `graph-code-navigation`): `get_neighbors` no alvo de patch para verificar se a spec auditou todos os overrides/callers (evidência negativa barata — auditoria ausente em alvo virtual = ponto Categoria C, AP-03).
   - `mods/<mod>/modded/` — checar conflitos com patches existentes.

5. **Análise crítica em 3 categorias × 3 impactos:**

   **Categorias:**
   - **A — Gaps de Especificação:** falta info pra implementar (ex: comportamento em estado nulo não definido, fluxo incompleto, dependência não documentada).
   - **B — Edge Cases:** cenário válido ignorado (ex: troca rápida de postura, fim de raid, mod externo do mesmo escopo).
   - **C — Erros de Lógica:** pressuposto errado (ex: linha citada não bate com Assembly real, classe não existe com aquele nome, código incompatível com SPT 4.0+).

   **Impactos:**
   - 🔴 **Bloqueador** — impede implementar ou causa bug/crash garantido.
   - 🟡 **Importante** — comportamento errado em cenário relevante.
   - 🟢 **Menor** — qualidade/clareza.

6. **Renderizar `.agents/templates/technical-review.md.tmpl`** preenchendo:
   - `{{NUM}}`, `{{TITLE}}`, `{{SLUG}}`, `{{MOD}}`, `{{CREATED_AT}}`, `{{REVIEW_NN}}`.

7. **Adicionar pontos** no formato:
   ```markdown
   ### PA-NN-MM · Cat — Tipo · Impacto

   **Título resumido**

   **Problema:** [descrição precisa]

   **Por que importa:** [consequência concreta]

   **Sugestão:** [proposta concreta de resolução — o que mudar na spec técnica, qual abordagem adotar, qual trecho reescrever. Específica o suficiente para o usuário aceitar com um "ok" ou contrapor com outro caminho.]

   **Decisão:** `[ ]` Pendente · `[ ]` Aceitar sugestão · `[ ]` Caminho alternativo (descrever)
   ```
   - `NN` = número desta review (ex: `02`)
   - `MM` = ordem do ponto nesta review (`01`, `02`...)
   - IDs **permanentes** — nunca reutilizar.
   - **Toda sugestão deve ser acionável** — não vale "revisar o ponto X". Vale "trocar a chamada `Foo()` por `Bar()` em `arquivo.cs:42` porque..." ou "adicionar seção sobre estado nulo descrevendo Y".

8. **Atualizar o índice e contadores** no topo do arquivo de review.

9. **Reportar:**
   ```
   ✓ Review NN criada: <path>
     🔴 Bloqueadores: N · 🟡 Importantes: N · 🟢 Menores: N
   Status:
     [se houver 🔴]: NÃO está pronto pra build — resolver bloqueadores primeiro.
     [se sem 🔴]: pode iniciar /code-mod; pontos 🟡/🟢 podem ser resolvidos durante.
   Próximo passo:
     Para cada ponto, o usuário deve marcar "Aceitar sugestão" ou descrever caminho alternativo.
     Após decisão, atualizar a spec técnica conforme acordado e marcar o ponto como ✅ Resolvido.
   ```

## Regras

- **Sempre criar arquivo novo** — nunca editar review anterior.
- Pontos resolvidos em reviews anteriores **não voltam** a ser levantados.
- Se um ponto pendente da review anterior tiver sido resolvido na spec técnica, mencionar no novo arquivo: `✅ PA-01-03 resolvido na spec — fechado.`
- Cada ponto deve citar **o trecho exato da spec técnica** ou o **arquivo:linha do Assembly** que sustenta a crítica. Análise sem evidência não vai.
- Se a spec técnica afirma `arquivo.cs:N` mas o Assembly não tem aquilo, isso é Categoria C — Bloqueador.
