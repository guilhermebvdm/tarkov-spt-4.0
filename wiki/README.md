---
title: Wiki — Base de Conhecimento Local
date: 2026-05-03
status: 🟢 Vivo
authors: [guilhermebvdm]
---

# Wiki — Base de Conhecimento Local

Pasta agregadora de bases de conhecimento externas mantidas como **snapshot somente-leitura** dentro deste repo. Cada subpasta é uma fonte distinta, com sua própria origem, licença e procedimento de sincronização.

## Fontes disponíveis

| Fonte | Pasta | Origem | Licença |
|-------|-------|--------|---------|
| Wiki oficial do SPT | [spt/](spt/) | [github.com/sp-tarkov/wiki](https://github.com/sp-tarkov/wiki) → [wiki.sp-tarkov.com](https://wiki.sp-tarkov.com/) | CC BY-NC-ND 4.0 |

## Regras gerais

- **Não edite** arquivos dentro destas subpastas — são snapshots fiéis ao upstream.
- Para anotar, comentar ou estender o conteúdo, escreva em [docs/](../docs/) com link de volta para o arquivo da wiki.
- Sempre cite a fonte ao reproduzir trechos: ver instruções no README de cada subpasta.

## Adicionar nova fonte

1. Criar `wiki/<nome-da-fonte>/`.
2. Adicionar `README.md` na subpasta com: origem, licença, SHA/data do snapshot, comando de sync.
3. Adicionar/estender script em `.agents/hooks/sync-wiki.sh` (ou criar novo) cobrindo o fetch.
4. Registrar a fonte na tabela acima.
