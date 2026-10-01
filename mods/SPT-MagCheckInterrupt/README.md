# SPT-MagCheckInterrupt

**Versão base:** 1.0.2 · **Licença:** MIT  
**Upstream:** https://github.com/ozen-m/SPT-MagCheckInterrupt.git @ `d62e7c7ee380ce79bc49e4439f70679ad4f2dfaf` (branch `v1.0.2`)  
**Forge:** https://github.com/ozen-m/SPT-MagCheckInterrupt/releases/tag/v1.0.2  

---

## O que é

O **MagCheckInterrupt** permite interromper a animação de checagem de munição/carregador (Alt+T) no meio do processo e fazer a transição imediata para a animação de recarga (R), sem precisar esperar que o personagem termine de inspecionar e recolocar o carregador na arma.

### Funcionalidades
- Transição instantânea da inspeção de carregador para a recarga de arma ao pressionar a tecla de recarga.
- Conclusão natural da checagem caso o jogador não pressione recarga.
- Recurso de **Slow Animation** (desaceleração da animação durante a janela de decisão) para dar tempo de reagir à quantidade de balas observadas.
- Sincronização e compatibilidade dedicada com **Project Fika** (netcode de host/client) e **UIFixes** (recurso *Reload in Place*).

---

## Estrutura desta pasta

| Pasta / Arquivo | Conteúdo |
|---|---|
| [`docs/`](docs/) | Documentação técnica modular completa (arquitetura, operações, patches, FIKA e UIFixes). |
| [`original/`](original/) | Clone do repositório oficial, sem `.git`. **Não modificar.** Referência intocada usada para diff e atualizações. |
| [`modded/`](modded/) | Cópia de trabalho. Modificações e correções vão aqui. |
| [`PROPRIEDADES.md`](PROPRIEDADES.md) | Mapeamento completo de todas as opções do menu de configuração F12 (BepInEx Configuration Manager) com traduções pt-BR e tooltips. |
| [`assets/`](assets/) | Imagens, prints, documentação externa. |
| [`backlog/`](backlog/) | Ideias, bugs, relatórios de auditoria e planos de melhoria. |
| [`builds/`](builds/) | Builds geradas para distribuição local (`.dll`). |
| [`scripts/`](scripts/) | Scripts auxiliares específicos deste mod. |
| [`mod.json`](mod.json) | Metadados machine-readable (inventário de mods do workspace). |

---

## Workflow de desenvolvimento

Ciclo completo de backlog/specs/reviews/código/memória/grafos: ver [WORKFLOW.md](../../WORKFLOW.md).

## Mapa de código

Grafo de arquitetura e dependências deste mod (graphify): [`GRAPH_REPORT.md`](../../references/graphs/mods/SPT-MagCheckInterrupt/GRAPH_REPORT.md).  
Regenerar após mudanças: `/update-mod-graph SPT-MagCheckInterrupt` (ou `bash scripts/update-graphs.sh SPT-MagCheckInterrupt`).

## Comparar modificações com o original

```bash
diff -r mods/SPT-MagCheckInterrupt/original/ mods/SPT-MagCheckInterrupt/modded/
```

---

_Adicionado em 2026-09-05T06:01:31Z_
