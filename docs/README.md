# Documentação — tarkov-spt-4.0

Índice da documentação técnica e arquitetural.

## Estrutura

- `technical/` — **camada canônica do harness**, lida durante o ciclo de desenvolvimento. O roteamento (qual doc ler em qual situação) vive em [technical/README.md](technical/README.md) — não duplicar a lista aqui.
- `migration/` — inventário e tracking da migração de mods 3.x → 4.0 (ver [migration/README.md](migration/README.md))
- `performance/` — relatórios da investigação de performance cross-stack (profiling in-game, comparativos vanilla × TRL); relatórios por mod continuam em `mods/<mod>/docs/`
- `discord-mods-topics/` — transcrições e análises de threads do Discord sobre mods (ex.: ORBIT, Realism)
- `ideas/` — rascunhos e propostas de features
- `files-from-4.1/` — dado bruto: tabelas de deofuscação SPT 4.0 → 4.1 (consulta por `grep`, ver [.agents/resources.md](../.agents/resources.md))

Só `technical/` participa do roteamento do ciclo de desenvolvimento.

## Convenção

Todo `.md` neste diretório (exceto este README) precisa de frontmatter:

```yaml
---
title: ...
date: YYYY-MM-DD
status: 🟢 Vivo
authors: ...
---
```

Ver `.agents/conventions.md` para detalhes. **Status e data de cada doc vivem no próprio frontmatter** (fonte de verdade) — não há tabela de status manual aqui, para não desatualizar.
