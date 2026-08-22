# TRL-FikaSync-ClimbableLadders

**Versão:** 1.0.0 · **Licença:** MIT
**SPT Version:** 4.0.13 · **EFT Version:** 0.16.9
**Dependências:** `com.fika.core` (Fika), `com.tarkin.ladders` (Climbable Ladders)

---

## O que é

Mod ponte de sincronização multijogador em rede e animação em terceira pessoa para o mod **Climbable Ladders** no **Fika Coop**.

Permite que outros jogadores na sessão cooperativa visualizem a movimentação e animação procedural completa dos membros (braços nos degraus, pernas, rotação da pelve e posicionamento do corpo) quando um jogador sobe/desce escadas verticais ou realiza giros em barras horizontais, eliminando o problema de visualização estática/dura em terceira pessoa.

## Estrutura desta pasta

| Pasta / Arquivo | Conteúdo |
|---|---|
| `modded/` | Código-fonte C# do plugin de sincronização Fika. |
| `assets/` | Recursos visuais e documentação externa. |
| `backlog/` | Tarefas, itens de melhoria e bugs rastreados. |
| `builds/` | DLLs e pacotes gerados para distribuição. |
| `scripts/` | Scripts auxiliares de build. |
| `mod.json` | Metadados do mod para o inventário do workspace. |

## Mapa de código

Grafo do código deste mod (graphify): [`GRAPH_REPORT.md`](../../references/graphs/mods/TRL-FikaSync-ClimbableLadders/GRAPH_REPORT.md). Regenerar após mudanças: `/update-mod-graph "TRL-FikaSync-ClimbableLadders"`.
