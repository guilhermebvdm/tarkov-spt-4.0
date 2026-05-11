# Memory — SPT-Realism-Mod-Client

Memória cronológica de sessões de chat (timestamps em GMT-3, aproximados quando não puderem ser inferidos com precisão). Cada entrada resume o que foi feito. Atualizada ao fim de cada sessão de trabalho.

> Por que existe: o usuário trabalha múltiplos chats em paralelo. Este arquivo evita que cada chat reabra do zero — futuras sessões podem carregar contexto ao ler as últimas entradas.

## Estado atual (snapshot ao fim da última sessão)

- **Adicionado ao repo** em 2026-05-09 via `/add-mod-repo-for-modding`. Upstream: `https://github.com/space-commits/SPT-Realism-Mod-Client.git`.
- **Versão upstream capturada:** plugin `RealismMod` 1.6.3 (BepInPlugin GUID `shwng.camerarotation` — não, espera, esse é do stances; o GUID real do RealismMod ver em `original/Plugin.cs`).
- **`PROPRIEDADES.md` gerado** documentando ~150 entries do F12 BepInEx em 20 seções, em pt-BR, com índice clicável no topo.
- **Status:** mod ainda **sem modificações em código** (`modded/` é cópia idêntica de `original/`). Nenhum item de backlog criado ainda.
- **Mod instalado em SPT** pelo usuário (presente em `D:/SPT/BepInEx/plugins/` ao lado dos outros).

## Pendências / próximos passos conhecidos

- `mod.json` e `README.md` do mod ainda precisam ser preenchidos com `spt_version` correto (lembrete do script `/add-mod-repo-for-modding`).
- Sem backlog criado. Próximo passo natural seria `/add-backlog-item` se houver mudança desejada — ou deixar como vendor pinned (referência apenas).

## 2026-05-09 ~18:00 (GMT-3) — Sessão 1: adição inicial do mod

Em ordem aproximada:

1. **`/add-mod-repo-for-modding https://github.com/space-commits/SPT-Realism-Mod-Client.git`** — clonou o repo upstream, criou estrutura padrão (`original/`, `modded/`, `assets/`, `backlog/`, `builds/`, `scripts/`), `.git/` do clone removido, SHA HEAD capturado, `mod.json` e `README.md` templates renderizados.
2. **Geração automática do `PROPRIEDADES.md`** — script identificou `Config.Bind(` em `original/PluginConfig.cs`; extraiu todas as ~150 entries do F12 com seção, nome, tipo, default, faixa, tooltip pt-BR traduzido fielmente. Marcações `(Avançado)` baseadas em `IsAdvanced=true` do `ConfigurationManagerAttributes`.
3. **Reorganização do `PROPRIEDADES.md`:** 20 seções fora de ordem (seções .1 e .2 estavam após .10). Reordenado para 0→1→2→…→19. Criado índice ordenado clicável no topo com anchor links.
4. **Correções de markdown linting:**
   - MD060 (table column style) — separador `|---|---|` → `| --- | --- |`.
   - MD026 (trailing punctuation) — `## .1. Misc. Settings.` → `## .1. Misc. Settings`.
   - MD028 (blank line inside blockquote) — `>` inserido em linhas em branco entre quotes adjacentes.
5. **Commit atômico + push** apenas dos arquivos relacionados ao novo mod (`mods/SPT-Realism-Mod-Client/`) + slash command novo, deixando outras mudanças em progresso unstaged.

## Notas relevantes (não-mod)

- **Mod externo `hazelify.StanceSync.dll`** (já instalado em `D:/SPT/BepInEx/plugins/`) foi identificado como causa de bug "shoulder swap durante lean" reportado pelo usuário. Não está relacionado ao SPT-Realism-Mod-Client nem ao stances mod. Solução: desabilitar `Sync leaning with shoulder swapping?` no F12 daquele plugin.
