# 008 — Opções customizadas: configs performance · Kickoff

**Launcher:** Launcher4.0beta · **Data:** 2026-07-03 · **Origem:** Trello MTav8H5f itens 4.2 e 4.2.1 · **Deps:** 007 (motor de sync)

> Brief de kickoff — insumo para `/create-spec`. Não é a spec.

## Objetivo

Opção "Usar configs performance" (checkbox + descrição) na tela logada: ao habilitar, sobrepor na pasta `config` do usuário as configs presentes na pasta `config-performance` do server, **mantendo** os arquivos divergentes (customizados) do usuário.

## Perguntas p/ a spec

- Comportamento ao **desabilitar** (reverter para as configs padrão do server? de onde?).
- Interação com a regra de `config` do item 007 (mesmo motor, fonte diferente).
- Persistência da escolha (settings do launcher por conta ou por máquina).
