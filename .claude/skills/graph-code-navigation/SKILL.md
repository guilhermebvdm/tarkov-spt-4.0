---
name: graph-code-navigation
description: Como navegar o código (EFT decompilado, SPT server, FIKA, mods) pelos grafos do graphify em references/graphs/ — quando usar grafo vs Grep, receitas de query para os antipatterns (overrides de alvo virtual AP-03, callers de ponto de patch, cadeia input→efeito), regra "grafo aponta, leitura prova" e regeneração via scripts/update-graphs.sh. Aplicar durante /create-technical-spec, /review-technical-spec, /code-mod e /code-review para achar métodos/classes/nós certos com menos Grep manual.
---

# Graph Code Navigation

Os grafos de código (graphify, AST/tree-sitter) vivem em `references/graphs/<escopo>/graph.json` — ver [references/graphs/README.md](../../../references/graphs/README.md). Esta skill define **quando** e **como** usá-los nos commands de desenvolvimento.

## 1. Regra de evidência (inegociável)

**O grafo APONTA, a leitura do `arquivo.cs:linha` PROVA.** Nenhum nó/aresta do grafo entra em spec, review ou código sem reconferir o arquivo real. A hierarquia de evidência de `.agents/resources.md` permanece intacta — o grafo é camada de **navegação**, não fonte.

**A tabela de deofuscação tem o MESMO status.** [docs/files-from-4.1/consolidated-mappings.txt](../../../docs/files-from-4.1/consolidated-mappings.txt) traduz o nome ofuscado 4.0 → conceito/nome 4.1 (`GClass680 -> ABotProfileCreator`). Desde 2026-07-19 esses aliases estão **injetados no dump** (comentário no topo de cada `.cs`) e no `types-index.json` — 4.763 tipos. O alias **aponta** o conceito; a assinatura/fórmula ainda se **prova** no `arquivo.cs:linha`. Regras: a **direita** (nome 4.1) é rótulo de fonte comunitária, não oficial; **sem entrada ≠ não existe** (`GClass898`/`GClass3008` usados no repo não estão no mapa); cobre **tipos**, não membros (`method_5`, `_player`).

⚠️ **Busca por conceito não passa pelo grafo.** O graphify indexa **AST**, então os aliases (que vivem em comentário e no índice) **não são nós** — `query_graph "Localization"` não acha `GClass2348`. Quando você só sabe o conceito, o caminho é: **`references/eft-decompiled/types-index.json`** (ou `grep` do alias no dump) → obtém o FQN → **aí sim** grafo → `.cs`.

## 2. Quando usar grafo vs Grep

| Pergunta | Ferramenta |
|---|---|
| "Quem chama X?" / "quais métodos Y usa?" | grafo (`get_neighbors`, `query_graph`) |
| "Quais overrides/implementações de Y existem?" | grafo (nós com mesmo label `.Y()` + arestas `inherits`/`method`) |
| "Qual a cadeia de A até B?" (input→efeito) | grafo (`shortest_path` / `graphify path`) |
| "O que é impactado se eu mudar X?" | grafo (`graphify affected "X"`) |
| "Visão geral de um módulo/sistema" | `GRAPH_REPORT.md` do escopo (`get_community`, `god_nodes`) |
| String literal, nome de config, mensagem de log, valor de constante | **Grep direto** (grafo não indexa conteúdo de linha) |
| Confirmar assinatura/fórmula/linha exata | **Read no arquivo** (sempre, após o grafo apontar) |
| "O que é este `GClassNNNN`/`GStructNNNN`?" (nome ofuscado → conceito) | **tabela de deofuscação** (`docs/files-from-4.1/consolidated-mappings.txt`, grep `^GClass680 -> `) — aid, cobre só tipos |

## 3. Como consultar

**Via MCP** (só `graphify-eft` em `.mcp.json` — o grafo do EFT decompilado, estável e o mais consultado): tools `query_graph`, `get_node`, `get_neighbors`, `get_community`, `shortest_path`, `god_nodes`, `graph_stats`. **Grafos de mod NÃO têm server MCP** (eram um pin fixo que apontava pro mod errado e gerava churn entre os 2 PCs) — consultá-los sempre via CLI abaixo.

**Via CLI** (qualquer escopo, sem server — caminho padrão para mods):

```bash
graphify query "<pergunta>"        --graph references/graphs/<id>/graph.json --budget 2000
graphify path "<A>" "<B>"          --graph references/graphs/<id>/graph.json
graphify explain "<nó>"            --graph references/graphs/<id>/graph.json
graphify affected "<nó>" --depth 2 --graph references/graphs/<id>/graph.json
```

IDs de escopo: `mods/<mod>` · `eft-decompiled` · `fika-plugin` · `fika-server` · `fika-headless` · `spt-source`.

**Cadeias entre escopos** (ex.: patch do mod → método do EFT): cada grafo é isolado — consultar os dois separadamente (o ponto de costura é o alvo do patch), ou gerar um combinado com `graphify merge-graphs`.

## 4. Receitas mapeadas aos antipatterns

### AP-03 — auditar TODOS os overrides antes de patchear alvo virtual

Antes de escrever um patch em método `virtual`/`abstract`, enumerar todos os nós com aquele label e conferir quem chama base:

```bash
# 1. Grafo lista os candidatos (caso real: 15 nós .SetTriggerPressed() = base + 14 overrides)
graphify query "SetTriggerPressed" --graph references/graphs/eft-decompiled/graph.json
# 2. Para CADA nó retornado: Read no arquivo:linha e conferir se o override chama base.X()
# 3. Patchear o ponto de roteamento que cobre todos os caminhos
```

Sem esse passo, o patch "não dispara" silenciosamente (ver `docs/technical/spt-antipatterns.md` AP-03 — bug real do F4).

### Callers/callees de um ponto de patch (antes de escrever o stub)

`get_neighbors` no método alvo → mapear quem chama (o patch afeta todos esses caminhos) e o que ele chama (side-effects que um Prefix skip pularia — AP-04).

### Cadeia input→efeito (fluxo de dados da spec técnica §6)

`shortest_path`/`graphify path` entre o ponto de entrada (tecla/command) e o efeito final → esqueleto do diagrama A→B→C, depois confirmar cada hop lendo o arquivo.

### Reuso antes de inventar (no /code-mod)

`query_graph` no grafo do mod (`references/graphs/mods/<mod>/`) pelos conceitos da feature → utilities/patterns existentes em `modded/` aparecem antes de você criar duplicata.

### Impacto de um diff (no /code-review)

`graphify affected "<classe/método tocado>"` no grafo do mod → callers afetados além do arquivo do diff.

### Resolver nome ofuscado → conceito (deofuscação 4.0→4.1)

O grafo e o decompile rotulam nós com o nome ofuscado (`GClass680`, `GStruct80`). Ao **reportar** esses nós em spec/review, traduzir pelo conceito antes de escrever — o texto fica legível e já semeia a migração 4.1:

```bash
grep '^GClass680 -> ' docs/files-from-4.1/consolidated-mappings.txt   # → ABotProfileCreator
```

Regra: o alias **aponta** o conceito, a assinatura ainda se **prova** no `arquivo.cs:linha`. Cobre só **tipos** (não `method_5`/`_player`); **sem entrada ≠ não existe**; a direita (nome 4.1) é rótulo de fonte comunitária (AP-09). Atalho: o alias já vem no topo do `.cs` e no `types-index.json`, então `grep "<conceito>"` no dump costuma resolver sem consultar a tabela.

## 5. Manutenção

- Grafos **desatualizam** — regenerar via `bash scripts/update-graphs.sh <escopo>`: pós-`/code-mod`/fixes no mod tocado (`/update-mod-graph <mod>`), pós-mudança de pin no `references/manifest.json`.
- Grafo regenerado é commitado **junto** com a mudança de código que o motivou.
- **Fallback:** graphify/MCP indisponível → Grep manual com a MESMA disciplina (enumerar todos os overrides, mapear callers) — a auditoria é obrigatória; o grafo só a torna barata.
- Limitação conhecida: labels de métodos são curtos (`.Foo()`) e podem colidir entre classes — desambiguar pelo `src`/`loc` do nó e confirmar lendo o arquivo.
