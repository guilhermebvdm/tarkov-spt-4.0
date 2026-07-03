# tarkov-spt-4.0 — Contexto para Agentes

Repositório de mods para SPT 4.0 (Single Player Tarkov). Lido por qualquer assistente AI (Claude Code, Gemini, Codex, Cursor).

## Workspace

- **SPT Version:** 4.0.x — Tarkov [PREENCHER versão]
- **Game Version:** Escape From Tarkov 0.16.9
- **Game install path:** por máquina, em `.spt-path` na raiz (gitignored; copie de `.spt-path.example`). Lido pelo `compile-mod.sh`. Default `D:/SPT`.
- **Mod types:** Client e Server
- **Compatibilidade:** SPT 3.x e 4.0 são arquiteturalmente incompatíveis — nunca misturar padrões

## Estrutura

- `.agents/` — contexto e workflows compartilhados (este é o hub principal)
- `.agents/workspace.md` — detalhes técnicos do workspace
- `.agents/conventions.md` — convenções do projeto
- `.agents/resources.md` — onde buscar informação (wiki local, APIs, DBs, deepwiki)
- `.agents/workflows/` — workflows reutilizáveis (manuais)
- `.agents/hooks/` — scripts compartilhados (validação, pre-commit, sync da wiki)
- `design-system/` — TRL Design System: padrão visual **obrigatório** para todo editor web de mod (tokens + componentes CSS; ler `design-system/CLAUDE.md` antes de estilizar qualquer editor)
- `docs/` — documentação técnica e arquitetural
- `references/` — fontes read-only de verdade (não editar): `eft-decompiled/` (Assembly EFT), `spt-source/` (código-fonte do servidor SPT) e repositórios do FIKA (conexão coop: `fika-server/`, `fika-plugin/` e `fika-headless/`)
- `wiki/` — snapshot read-only de github.com/sp-tarkov/wiki (CC BY-NC-ND 4.0; sincronizado via `.agents/hooks/sync-wiki.sh` — não editar)
- `.claude/settings.json` — config do Claude Code (referencia hooks em .agents/)

## Convenções

- **Idioma:** Português para docs e respostas; Inglês para código, commits, nomes de arquivo
- **Commits:** Conventional Commits em inglês (`feat:`, `fix:`, `chore:`, `docs:`)
- **Status de docs:** 🟢 Vivo · 🔵 Em andamento · 🟠 Desatualizado · ⚫ Arquivado
- **Frontmatter obrigatório** em `docs/**/*.md` (exceto README): `title`, `date`, `status`, `authors`
- **Comunicação:** Seja breve e objetivo nas respostas. Explique o raciocínio apenas quando necessário. Tenha preferencia por respostas estruturadas em bullets e tabelas.

## Setup (todo dev novo no repo)

```bash
git clone https://github.com/guilhermebvdm/tarkov-spt-4.0.git
cd tarkov-spt-4.0
bash .agents/hooks/install-hooks.sh    # Instala git pre-commit hook
node scripts/setup-references.js       # Clona as referências vendorizadas (spt-source, FIKA)
cp .spt-path.example .spt-path         # Define o path local do SPT/EFT (ajuste se != D:/SPT)
```

Dependência opcional (recomendada): `jq` para o hook do Claude Code funcionar.
- Windows: `winget install jqlang.jq`
- Linux: `apt install jq`

`references/spt-source/` e os repositórios do FIKA (`fika-server/`, `fika-plugin/`, `fika-headless/`) são **gitignored** (referências locais). Em máquina nova, rodar `node scripts/setup-references.js` para clonar tudo — o script lê o inventário canônico [references/manifest.json](references/manifest.json) (`--check` confere o que falta). Detalhes em [references/README.md](references/README.md).

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
1. [.agents/workspace.md](.agents/workspace.md) — contexto técnico SPT 4.0
2. [.agents/conventions.md](.agents/conventions.md) — regras do projeto
3. [.agents/resources.md](.agents/resources.md) — onde buscar informação (wiki local, APIs, DBs, deepwiki)

A pasta [wiki/spt/](wiki/spt/) é um snapshot read-only do upstream (sincronizado via `.agents/hooks/sync-wiki.sh`) — **não editar**.

Para tarefas específicas, consulte `.agents/workflows/` (quando existirem).

## Fluxo de backlog (slash commands)

> 📋 **Fonte de verdade do ciclo de desenvolvimento** (commands, artefatos, skills, memória, grafos de código): **[WORKFLOW.md](WORKFLOW.md)** — não duplicar a tabela aqui.

Resumo: cada item vive em `mods/<mod>/backlog/NNN-<slug>/` (índice em `mod-backlog.md`); artefatos seguem `NNN-<slug>-MM-tipo[-NN].md`; o ciclo vai de `/add-backlog-item` a `/update-mod-graph`. `<ref>` aceita: path da pasta, path de arquivo dentro dela, ou forma curta `<mod> <NNN>`.

## Hierarquia de evidência (spec/review técnicas)

Ordem canônica ao **citar evidência** (sempre com `arquivo.cs:linha`). **Paths e detalhes:** [.agents/resources.md](.agents/resources.md) → §"Hierarquia de evidência (spec/review técnicas)" (fonte de verdade — não duplicar paths aqui).

1. 🥇 Assembly descompilado (cliente EFT) — `references/eft-decompiled/`
2. 🥇 Servidor SPT — `references/spt-source/` (gitignored — ver [references/README.md](references/README.md))
3. 🥇 FIKA (coop) — `references/fika-{server,plugin,headless}/` (`Fika.Core` no plugin)
4. 🥈 Código do mod — `mods/<mod>/{original,modded}/`
5. 🥉 Wiki SPT — `wiki/spt/`
6. 🪛 Web — último recurso (`[fonte externa]`)

## ⚠️ Observações importantes

- **Nunca editar `SPT_Data/database/` direto.** Esses arquivos fazem parte da distribuição do SPT — qualquer atualização do SPT os sobrescreve e a edição se perde. Além disso, alterar a database em disco invalida o `SPT_Data/checks.dat` (integridade) e gera arquivos pesados (`looseLoot.json` ~42 MB, `items.json` ~19 MB) difíceis de versionar/distribuir. O jeito correto é um **mod de servidor** que aplica os patches em memória no `postDBLoad` (modelo usado por SVM e pela maioria dos mods de servidor): sobrevive a updates, pesa quase nada e é diffável.
