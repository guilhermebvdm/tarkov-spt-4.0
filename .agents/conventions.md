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

### Histórico

A seção `## Histórico` no rodapé é gerada automaticamente pelo git pre-commit hook. Não edite manualmente — se precisar corrigir, edite a tabela diretamente.

## Validação manual de headers

```bash
# Um arquivo
bash .agents/hooks/validate-doc-header.sh docs/technical/algo.md

# Todos
find docs -name "*.md" ! -name "README.md" | while IFS= read -r f; do
  bash .agents/hooks/validate-doc-header.sh "$f"
done
```
