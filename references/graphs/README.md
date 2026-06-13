# Grafos de código (graphify)

Grafos de conhecimento extraídos por [graphify](https://github.com/safishamsi/graphify) (AST via tree-sitter, **sem LLM** para código) das fontes de código do repo. **Esta pasta é VERSIONADA** — os dois PCs sincronizam via git sem regenerar.

> **Grafo = navegação, NÃO evidência.** O grafo aponta onde olhar (callers, overrides, cadeias); a prova é sempre a leitura do `arquivo.cs:linha`. Ver skill `graph-code-navigation` e `.agents/resources.md` § Hierarquia de evidência.

## Layout

| Pasta | Fonte | Conteúdo |
|---|---|---|
| `mods/<mod>/` | `mods/<mod>/modded/` | grafo do código do mod (evolui com `/code-mod`) |
| `eft-decompiled/` | `references/eft-decompiled/Assembly-CSharp/` | grafo do cliente EFT decompilado (~58k nós) |
| `fika-plugin/`, `fika-server/`, `fika-headless/` | `references/fika-*/` | grafos do FIKA |
| `spt-source/` | `references/spt-source/` | grafo do servidor SPT |

Cada pasta tem `graph.json` (dados, consumido pelo MCP e pela CLI), `GRAPH_REPORT.md` (resumo legível: comunidades, hubs) e `graph.html` (visualização — só para grafos pequenos).

## Instalação (uma vez por PC — desktop `guime` e notebook `guimello`)

```bash
python -m pip install --user uv
python -m uv tool install graphifyy        # pacote PyPI é "graphifyy" (nome temporário)
python -m uv tool update-shell             # põe ~/.local/bin no PATH (reabrir shell)
```

Binários: `graphify` (CLI) e `graphify-mcp` (MCP server stdio).

## Regeneração — ponto único: `scripts/update-graphs.sh`

```bash
bash scripts/update-graphs.sh                       # todos os escopos
bash scripts/update-graphs.sh <mod>                 # um mod (ou via /update-mod-graph <mod>)
bash scripts/update-graphs.sh eft-decompiled        # uma reference
```

- Escopos de mods são **auto-descobertos** (`mods/*/modded`) — mod novo via `/add-mod-repo-for-modding` entra sozinho.
- O working output fica em `<escopo>/graphify-out/` (gitignored); o script **publica** os artefatos aqui.
- Ignore rules: `.graphifyignore` na raiz (substitui o `.gitignore` para o graphify — necessário porque `spt-source`/`fika-*` são gitignored mas DEVEM entrar no grafo).
- **Quando regenerar:** após `/code-mod`/fixes num mod (→ `/update-mod-graph <mod>`); após mudança de pin no `references/manifest.json` (→ escopo da reference).
- **Regra de commit:** grafo regenerado entra **no mesmo commit (ou no imediatamente seguinte)** da mudança de código que o motivou.

## MCP (consulta estrutural nas sessões)

`.mcp.json` na raiz registra servers stdio (`graphify-mcp <graph.json>`): permanentes só para os escopos de consulta frequente — `graphify-eft` e o mod ativo em desenvolvimento. Tools: `query_graph`, `get_node`, `get_neighbors`, `get_community`, `shortest_path`, `god_nodes`, `graph_stats`.

- **Primeiro uso em cada PC:** o Claude Code pede aprovação interativa dos servers do `.mcp.json` — aprovar uma vez; não é "MCP quebrado".
- **Demais escopos** (fika, spt-source, outros mods): consultar sob demanda via CLI — `graphify query "<pergunta>" --graph references/graphs/<id>/graph.json` (também `path`, `explain`, `affected`) — ou ler o `GRAPH_REPORT.md`.
- **Cadeias entre escopos** (ex.: patch do mod → método do EFT): consultar cada grafo separadamente, ou gerar um grafo combinado com `graphify merge-graphs`.

## Notas

- Grafos de `spt-source`/`fika-*` referenciam fontes **gitignored** — num clone fresco, rode `node scripts/setup-references.js` antes de confiar nos paths apontados.
- `graph.json` do eft-decompiled tem ~46MB (<100MB do GitHub). Se algum grafo estourar 100MB, migrar para git LFS ou reduzir escopo.
- **Wiki:** fora dos grafos por ora — o pipeline de markdown do graphify usa LLM (requer API key, ex. `GEMINI_API_KEY`) e a wiki é CC BY-NC-ND (derivado versionado só se o repo for privado). Decisão pendente registrada na memória repo-wide.
