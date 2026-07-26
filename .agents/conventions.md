# Convenções do projeto

## Idioma

- **Português:** docs, comentários de PR, conversas com AI
- **Inglês:** código, nomes de arquivo, commits, branches

## Commits (Conventional Commits)

```
feat:     nova feature
fix:      correção de bug
chore:    tarefas de manutenção
docs:     mudança em documentação
refactor: refatoração sem mudança de comportamento
test:     adição/ajuste de testes
build:    build system / dependências
```

## Documentação

### Frontmatter obrigatório em `docs/**/*.md`

```yaml
---
title: Título descritivo
date: YYYY-MM-DD
status: 🟢 Vivo
authors: Nome
---
```

### Status disponíveis

| Status | Quando usar |
|---|---|
| 🟢 Vivo | Aprovado, ainda mutável |
| 🔵 Em andamento | Em construção, não confiar como referência |
| 🟠 Desatualizado | Manter por histórico, não seguir |
| ⚫ Arquivado | Ignorar, não ler |

### Renomear ou remover um doc de `docs/technical/`

`docs/technical/` é a camada canônica lida durante o ciclo de desenvolvimento (ver [docs/technical/README.md](../docs/technical/README.md)), e é referenciada de fora — commands, skills, memórias de mod e artefatos de backlog apontam para ela por nome de arquivo.

**Antes de commitar um rename ou remoção**, procurar quem aponta para o nome antigo:

```bash
grep -rl "<basename-antigo>" --include="*.md" docs mods .claude .agents
```

Atualizar as referências **vivas** (commands, skills, `WORKFLOW.md`, `.agents/`, outros docs). **Não** atualizar artefatos de backlog (`mods/*/backlog/`) nem memórias de sessão (`mods/*/memory/`) — são append-only por convenção; um link histórico quebrado ali é esperado e deve ficar.

> Caso real: `inventario-itens-spt4.md` → `spt4-items-inventory-hideout.md` deixou 5 referências órfãs em `mods/CustomClasses/`, todas em arquivos imutáveis.

### Histórico

A seção `## Histórico de Alterações` no rodapé é gerada automaticamente pelo git pre-commit hook a cada commit que toca arquivos em `docs/`. A coluna de descrição é preenchida com a primeira linha da mensagem de commit — por isso **a mensagem de commit deve funcionar como linha de histórico** (`docs(technical): reescreve guia X`, não `chore: ajustes`).

- Use `git commit -m "mensagem descritiva"` para que o histórico fique útil
- Para `git commit` interativo (sem `-m`), a descrição vai como `(sem mensagem)` (limitação do git: o pre-commit roda antes do editor abrir)
- Não edite a tabela `## Histórico` manualmente — se precisar corrigir, edite após o commit

## Validação manual de headers

```bash
# Um arquivo
bash .agents/hooks/validate-doc-header.sh docs/technical/algo.md

# Todos
find docs -name "*.md" ! -name "README.md" | while IFS= read -r f; do
  bash .agents/hooks/validate-doc-header.sh "$f"
done
```
