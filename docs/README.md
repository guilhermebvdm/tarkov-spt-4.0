# Documentação — tarkov-spt-4.0

Índice da documentação técnica e arquitetural.

## Estrutura

- `technical/` — guias técnicos: criação de mod client/server, build C#, migração 3.x → 4.0, antipatterns, deofuscação de GClass
- `migration/` — inventário e tracking da migração de mods 3.x → 4.0 (ver [migration/README.md](migration/README.md))
- `discord-mods-topics/` — transcrições e análises de threads do Discord sobre mods (ex.: ORBIT, Realism)
- `ideas/` — rascunhos e propostas de features

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
