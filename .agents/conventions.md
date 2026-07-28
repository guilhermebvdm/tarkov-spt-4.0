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

A seção `## Histórico` no rodapé é gerada automaticamente pelo git pre-commit hook a cada commit que toca arquivos em `docs/`. A coluna **Descrição** é preenchida com a primeira linha da mensagem de commit.

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

## Estrutura Padrão de Mods (`mods/<NomeDoMod>/`)

A organização da pasta do mod varia dependendo do tipo de origem:

### 1. Mods de Terceiros / Forks / Descompilados (ex: VisceralCombat, AutoGym)
- `original/`: Código-fonte ou arquivos originais preservados sem edição (referência de auditoria).
- `modded/`: Cópia de trabalho editável onde são feitas correções de bugs e novas funcionalidades.

### 2. Mods Próprios (Criados do zero no projeto, ex: TRL-Fixes, TRL-ImmersiveScopes)
- O código-fonte vive diretamente na raiz do mod ou em subpastas lógicas (`src/`, `Client/`, `Server/`), **sem necessidade de duplicação em `original/` e `modded/`**.

### Arquivos e Governança Comuns a Todos os Mods
- `mod.json`: Metadados do mod (`name`, `version_base`, `spt_version`, `created_at`, URLs upstream se houver e licença).
- `README.md`: Documentação principal com objetivo do mod e visão geral.
- `PROPRIEDADES.md`: Mapeamento estruturado das opções de configuração do menu F12 (BepInEx `Config.Bind`), quando aplicável.
- `memory/sessions.md`: Histórico da sessão, lições aprendidas, hipóteses descartadas e pendências.
- `assets/`: Bundles, tabelas de dados de suporte (ex: JSONs) e arquivos de licença.
- `backlog/`: Especificações funcionais/técnicas e artefatos de tarefas.
- `docs/`: Documentação técnica e relatórios do mod.


