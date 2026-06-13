# /apply-code-review

Aplica em `modded/` os achados de um `04-code-review-NN.md` marcados como **Aceitar sugestão** ou **Aceitar com modificação**. Mantém rastreabilidade via IDs `CR-NN-MM` em comentários inline e atualiza o documento de review + `05-asbuild.md`.

> **Skills obrigatórias:** carregar `spt-mod-best-practices`, `csharp-mod-best-practices` e `repo-workflow-best-practices` antes de aplicar. Cada fix tocando código C# deve passar pelos mesmos checklists do `/code-mod`.

## Uso

```bash
/apply-code-review <ref> [--review NN]
```

- `<ref>` segue as regras do `/create-spec`.
- `--review NN` — qual rodada aplicar (default: maior NN existente em `04-code-review-*.md`).

## Escopo de uma chamada

**Uma rodada de review por execução.** Se houver `04-code-review-01.md` E `04-code-review-02.md` com achados pendentes em ambos, rodar 2 vezes (`--review 01` depois `--review 02`). Isso mantém a história de cada rodada limpa no documento e nos comentários inline (`CR-01-NN` vs `CR-02-NN`).

## Pré-condições

1. Existir pelo menos um `<NNN>-<slug>-04-code-review-NN.md`.
2. Pelo menos um achado dentro do review-alvo marcado `[x] Aceitar sugestão` ou `[x] Aceitar com modificação`. Se nenhum, parar e avisar.
3. Existir `<NNN>-<slug>-05-asbuild.md` (criado pelo `/code-mod`). Se não existir, criar agora antes de aplicar (ver §7 abaixo).

## O que fazer

1. **Resolver `<ref>`** → `<mod>`, `<NNN>`, `<slug>`, `<path-pasta>`.

2. **Validar pré-condições.**

3. **Resolver `NN`.** Se `--review` foi passado, usar. Senão, o maior NN existente em `04-code-review-*.md`. Se zero arquivos casam, parar.

4. **Ler:**
   - O `<NNN>-<slug>-04-code-review-NN.md` alvo.
   - Spec funcional, spec técnica, asbuild — para contexto.
   - **Memória do mod (leve)** — topo de `mods/<mod>/memory/sessions.md` (snapshot + pendências): fixes podem colidir com pendências abertas (ex.: "não validado in-game"). Pendência 🔴 do item/mod → alertar antes de prosseguir (`memory-curation` §14).
   - Arquivos de `mods/<mod>/modded/` que serão tocados (por CR-NN-MM).

5. **Iterar achados em ordem (por NN do CR-NN-MM):**

   Para cada achado marcado `[x] Aceitar sugestão`:
   - Aplicar a Sugestão exatamente em `mods/<mod>/modded/`.
   - Adicionar comentário inline no código modificado: `// ref: CR-NN-MM`. Para mudanças grandes (>1 método), um único comentário no topo do bloco basta.
   - No documento de review, trocar o título do achado de `### CR-NN-MM · Cat — Tipo · Impacto` para `### CR-NN-MM · Cat — Tipo · Impacto · ✅ Aplicado em YYYY-MM-DD`.
   - Adicionar bloco de Resolução logo após o bloco de Decisão:

     ```markdown
     **Resolução:** Sugestão aplicada conforme proposto.
     **Aplicação:** [arquivos tocados + descrição enxuta do diff]
     ```

   Para cada achado marcado `[x] Aceitar com modificação: <texto>`:
   - Interpretar o texto da modificação e aplicar. Se o texto for ambíguo demais para uma interpretação segura, marcar `⚠️ Skip — clarificar` (no lugar de ✅ Aplicado) e pular o achado. Não inventar.
   - Resolução documenta a interpretação adotada.

   Para cada achado marcado `[x] Rejeitar`:
   - Não tocar no código.
   - No documento, trocar título para `### CR-NN-MM · Cat — Tipo · Impacto · ⏭️ Rejeitado em YYYY-MM-DD`.
   - Adicionar `**Resolução:** Rejeitado — [texto da rejeição copiado]`.

   Para achados que continuam `[ ] Pendente`:
   - Não tocar. Permanecem para próxima chamada.

6. **Atualizar contadores** no Resumo do documento de review:
   - Bloqueadores/Fortes/Médios/Menores ainda pendentes (não-✅ e não-⏭️).
   - Resolvidos = quantos ficaram ✅ Aplicado.
   - Total inalterado.

7. **Atualizar `05-asbuild.md`:**
   - Se não existir, criar via `.agents/templates/asbuild.md.tmpl` (caso o item venha de `/code-mod` legado).
   - Adicionar entrada no Histórico: `YYYY-MM-DD | Aplicação de N achados de code-review NN via /apply-code-review — IDs: CR-NN-01, CR-NN-03, …`.
   - Atualizar a tabela "Arquivos alterados" com novas modificações (não recriar — acrescentar).

8. **Adicionar entrada no Histórico do `04-code-review-NN.md`:**

   ```markdown
   | YYYY-MM-DD | Aplicação automática de N achados via `/apply-code-review` — IDs aplicados: CR-NN-01, CR-NN-03; rejeitados: CR-NN-05 |
   ```

9. **Atualizar status no `mod-backlog.md`** se necessário:
   - Se a aplicação resolveu todos os 🔴 da última rodada de code-review, e o item estava 🟡: voltar para 🟢.
   - Se algum 🔴 ainda pende: manter 🟡 (ou voltar para 🟡 se estava 🟢).

10. **Reportar:**

    ```text
    ✓ Apply code-review concluído — <NNN> <Título> (rodada NN)
    Memória consultada: snapshot de YYYY-MM-DD (Sessão N) · pendências que afetam: [P-N.M ...] / nenhuma
    Achados aplicados: N (IDs: CR-NN-01, CR-NN-03, CR-NN-05)
    Achados rejeitados: M (IDs: CR-NN-04)
    Achados pulados (clarificar): K (IDs: CR-NN-07)
    Achados pendentes: P (não marcados)
    Arquivos alterados:
      - mods/<mod>/modded/<X>.cs (modificado)
      - mods/<mod>/modded/<Y>.cs (criado)
    Documento atualizado: 04-code-review-NN.md
    Asbuild atualizado: 05-asbuild.md
    Próximo:
      /compile-mod <mod>
      Opcional: nova rodada /code-review para validar correções.
      Achado deferido/regressão que virar fix pós-validação → usar .agents/templates/fix.md.tmpl (06-fix-NN).
      Mudança de código substancial → /update-mod-graph <mod> (regenera o grafo; commit junto).
    ```

## Regras

- **Sandbox = `modded/`.** Achados que tocam arquivo fora de `modded/` (ex.: `PROPRIEDADES.md`, `mod-backlog.md`) são permitidos quando documentados na Sugestão; mesmo guard que `/code-mod`.
- **Não criar arquivos fora do escopo do achado.** Se a Sugestão pede criar `Patches/Foo.cs` mas o impacto cascata em outros 3 arquivos, **parar e abrir nova rodada de code-review**. Não improvisar fora do plano.
- **Comentário inline obrigatório.** Cada fix deve ter pelo menos um `// ref: CR-NN-MM` no código tocado. Comentário enxuto — detalhes ficam no doc, não no código.
- **Imutabilidade da review:** o documento de review original é **anotado**, nunca reescrito. Adicionar Resolução, marcar ✅/⏭️/⚠️, mas preservar o texto original do Problema/Sugestão.
- **Compilação efetiva** (gerar `.dll`) **não** está no escopo deste comando — usar `/compile-mod` em seguida.
- **Ambiguidade:** quando "Aceitar com modificação" tiver texto vago demais, **marcar ⚠️ Skip e perguntar** ao usuário em vez de inventar.