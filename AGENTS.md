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
- `.agents/resources.md` — onde buscar informação (wiki local, APIs, DBs, deepwiki)
- `.agents/workflows/` — workflows reutilizáveis (manuais)
- `.agents/hooks/` — scripts compartilhados (validação, pre-commit, sync da wiki)
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

Cada item de backlog vive em `mods/<mod>/backlog/NNN-<slug>/` (numeração local por mod, 3 dígitos). O índice é `mods/<mod>/backlog/mod-backlog.md`.

| Comando | Ação | Output |
|---|---|---|
| [`/add-backlog-item`](.claude/commands/add-backlog-item.md) `<mod> <descrição>` | Cria entrada e pasta; invoca `/create-spec` | `mod-backlog.md` + pasta `NNN-<slug>/` |
| [`/create-spec`](.claude/commands/create-spec.md) `<ref>` | Spec funcional (critérios + corner cases) | `NNN-<slug>-spec.md` |
| [`/review-spec`](.claude/commands/review-spec.md) `<ref>` | Edita inline a spec — gaps/contradições | mesmo arquivo |
| [`/create-technical-spec`](.claude/commands/create-technical-spec.md) `<ref>` | Pré-código com refs ao Assembly | `NNN-<slug>-technical-spec.md` |
| [`/review-technical-spec`](.claude/commands/review-technical-spec.md) `<ref>` | Análise crítica incremental | `NNN-<slug>-technical-review-NN.md` (NN +1 a cada run) |
| [`/code-mod`](.claude/commands/code-mod.md) `<ref>` | Implementa em `modded/` | mudanças em `mods/<mod>/modded/` |

`<ref>` aceita: path da pasta, path de arquivo dentro da pasta, ou forma curta `<mod> <NNN>`.

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
