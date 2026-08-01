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

## Versionamento e Compilação de Mods

- **Bump de Versão (SemVer):** Toda compilação deve evoluir a versão `x.y.z` do mod (sincronizada no `Plugin.cs`, `.csproj` e/ou `package.json`). `z` (patch) para ajustes/fixes; `y` (minor) para novas features; `x` (major) para breaking changes.
- **Isolamento da Pasta do Jogo:** A compilação por assistentes de IA gera binários (`.dll`) **exclusivamente na pasta do mod** (`mods/<mod>/builds/` ou `mods/<mod>/modded/bin/Release/`). Nunca copiar ou instalar automaticamente na pasta do jogo (`D:/SPT` ou `.spt-path`), priorizando o fluxo de versionamento e controle local no workspace.



