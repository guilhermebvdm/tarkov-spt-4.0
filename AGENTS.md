# tarkov-spt-4.0 — Contexto para Agentes

Repositório de mods para SPT 4.0 (Single Player Tarkov). Lido por qualquer assistente AI (Claude Code, Gemini, Codex, Cursor).

## Workspace

- **SPT Version:** 4.0.x — Tarkov [PREENCHER versão]
- **Game Version:** Escape From Tarkov 0.16.9
- **Game install path:** [PREENCHER]
- **Mod types:** Client e Server
- **Compatibilidade:** SPT 3.x e 4.0 são arquiteturalmente incompatíveis — nunca misturar padrões

## Estrutura

- `.agents/` — contexto e workflows compartilhados (este é o hub principal)
- `.agents/workspace.md` — detalhes técnicos do workspace
- `.agents/conventions.md` — convenções do projeto
- `.agents/workflows/` — workflows reutilizáveis (manuais)
- `.agents/hooks/` — scripts compartilhados (validação, pre-commit)
- `docs/` — documentação técnica e arquitetural
- `.claude/settings.json` — config do Claude Code (referencia hooks em .agents/)

## Convenções

- **Idioma:** Português para docs e respostas; Inglês para código, commits, nomes de arquivo
- **Commits:** Conventional Commits em inglês (`feat:`, `fix:`, `chore:`, `docs:`)
- **Status de docs:** 🟢 Vivo · 🔵 Em andamento · 🟠 Desatualizado · ⚫ Arquivado
- **Frontmatter obrigatório** em `docs/**/*.md` (exceto README): `title`, `date`, `status`, `authors`

## Setup (todo dev novo no repo)

```bash
git clone https://github.com/guilhermebvdm/tarkov-spt-4.0.git
cd tarkov-spt-4.0
bash .agents/hooks/install-hooks.sh    # Instala git pre-commit hook
```

Dependência opcional (recomendada): `jq` para o hook do Claude Code funcionar.
- Windows: `winget install jqlang.jq`
- Linux: `apt install jq`

## Comandos úteis

```bash
# Validar headers de todos os docs manualmente
find docs -name "*.md" ! -name "README.md" | while IFS= read -r f; do
  bash .agents/hooks/validate-doc-header.sh "$f"
done

# Pular pre-commit num amend (evita histórico duplicado)
git commit --amend --no-verify

# Alternativa: setar GIT_AMEND=1
GIT_AMEND=1 git commit --amend
```

## Para AI assistants

Antes de qualquer tarefa, leia:
1. `.agents/workspace.md` — contexto técnico SPT 4.0
2. `.agents/conventions.md` — regras do projeto

Para tarefas específicas, consulte `.agents/workflows/` (quando existirem).
