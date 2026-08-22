# ORBIT

**Versão base:** 1.2.1 · **Licença:** MIT  
**Upstream:** https://github.com/Chazut/ORBIT @ `2b31b22536ec557d609c2d09c0db9e2f6f6e6c11` (tag `1.2.1`)  
**Forge:** https://forge.sp-tarkov.com/mod/1479/orbit  

---

## O que é

**ORBIT** (*Objective-driven Raid Bot Intelligence Tactics*) é um mod BepInEx para SPT que transforma o comportamento tático e estratégico da IA em raid. Ele atribui objetivos reais aos bots (saquear áreas valiosas sala por sala, caçar em áreas de conflito PvP, visitar pontos de missão EFT e extrair de forma orgânica).

O mod foi construído integrando conceitos avançados de movimentação em esquadrão, campo de advecção de mapa (Phobos), camada customizada de saque (looting) baseada em APIs do próprio jogo e roteamento de missões, tudo respeitando as personalidades do SAIN (Rats, Chads, Timmys, etc.).

## Estrutura desta pasta

| Pasta / Arquivo | Conteúdo |
|---|---|
| `original/` | Clone do repositório oficial, sem `.git`. **Não modificar.** Referência intocada usada para diff e atualizações. |
| `modded/` | Cópia de trabalho. Modificações e novos desenvolvimentos vão aqui. |
| `assets/` | Imagens, prints, documentação externa. |
| `backlog/` | Ideias, bugs, tarefas e próximos passos. |
| `builds/` | Builds geradas para distribuição (`.dll`). |
| `scripts/` | Scripts auxiliares específicos deste mod. |
| `mod.json` | Metadados machine-readable do mod. |
| `PROPRIEDADES.md` | Catálogo completo de configurações F12 (BepInEx ConfigurationManager). |

## Configurações e Parâmetros (F12)

Para ver todas as propriedades e descrições detalhadas em português das seções de configuração do mod, consulte o documento:
- [PROPRIEDADES.md](PROPRIEDADES.md)

## Workflow de desenvolvimento

Ciclo completo de backlog/specs/reviews/código/memória/grafos: ver [WORKFLOW.md](../../WORKFLOW.md).

## Mapa de código

Grafo de análise estática do código deste mod (graphify): [`GRAPH_REPORT.md`](../../references/graphs/mods/ORBIT/GRAPH_REPORT.md).  
Regenerar após mudanças no código:
```bash
bash scripts/update-graphs.sh ORBIT
```

## Comparar modificações com o original

```bash
diff -r mods/ORBIT/original/ mods/ORBIT/modded/
```

## Build

Para compilar o mod para Release dentro das diretrizes de isolamento do repositório:
```bash
dotnet build mods/ORBIT/modded/Orbit/Orbit.csproj -c Release
```

---

_Adicionado em 2026-08-22T16:14:00Z_
