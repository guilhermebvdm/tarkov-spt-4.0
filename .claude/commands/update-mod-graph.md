# /update-mod-graph

Regenera o grafo de código de um mod (graphify) e publica em `references/graphs/mods/<mod>/`. Usar após **grande atualização ou finalização de trabalho** no código do mod: fim de item de backlog, rodada de fixes aplicada, merge de várias mudanças.

> **Skill relacionada:** `graph-code-navigation` (como os grafos são consumidos). Infra: [references/graphs/README.md](../../references/graphs/README.md).

## Uso

```bash
/update-mod-graph [<mod>]
```

- `<mod>` — nome da pasta em `mods/`. Se omitido, detectar o mod ativo da sessão (mesma hierarquia do `/update-memory` §1: path explícito > command direcionado > menção); se ambíguo, perguntar.

## O que fazer

1. **Resolver `<mod>`** e validar que `mods/<mod>/modded/` existe. Se não, listar mods disponíveis e parar.

2. **Verificar graphify instalado.** Se `graphify` não estiver no PATH (nem em `~/.local/bin`), **abortar com aviso** (não falhar silenciosamente):
   ```text
   ❌ graphify não instalado. Instale com:
      python -m pip install --user uv && python -m uv tool install graphifyy
   Ver references/graphs/README.md.
   ```

3. **Capturar contagem anterior** (se existir): nós/arestas do `references/graphs/mods/<mod>/GRAPH_REPORT.md` ou via `graphify query` stats.

4. **Rodar a regeneração** (extração incremental, sem LLM):
   ```bash
   bash scripts/update-graphs.sh <mod>
   ```
   O escopo é auto-descoberto pelo glob `mods/*/modded` — **sem registro manual**.

5. **Reportar delta:**
   ```text
   ✓ Grafo atualizado — mods/<mod>
   Nós: N_antes → N_depois · Arestas: M_antes → M_depois
   Publicado em: references/graphs/mods/<mod>/
   ⚠️ Commitar o grafo JUNTO com a mudança de código que o motivou
      (mesmo commit ou o imediatamente seguinte).
   ```

## Regras

- **Só toca** `<mod>/modded/graphify-out/` (working, gitignored) e `references/graphs/mods/<mod>/` (publicado, versionado). Nunca o código do mod.
- `scripts/update-graphs.sh` é o ponto único de verdade da regeneração — este command é um wrapper com relatório de delta.
- Sem mudança de topologia ("No code-graph topology changes detected") não é erro — reportar "sem mudanças".
