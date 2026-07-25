# 001 — portabilidade-spt-4

**Mod:** TRL-DynamicSpawn
**Status:** Entregue
**Criado:** 2026-07-17T22:13:00-03:00

## Visão geral

Portar e corrigir a inicialização do mod TRL-DynamicSpawn para a versão SPT 4.0.

## Comportamento atual

O mod não inicializa o seu loop de spawn dinâmico no cliente e não spawna nenhum bot, pois os patches de inicialização do DynamicSpawnManager estão desabilitados ou com dependências nulas.

## Comportamento desejado

O loop de spawn do DynamicSpawnManager deve ser ativado com sucesso e receber instâncias reais de IBotCreator e BotsController.

## Critérios de aceite

- [x] O patch DynamicSpawnManagerPatch é registrado e habilitado no Plugin.cs
- [x] O patch obtém referências não nulas de IBotCreator e BotsController usando os Singletons do SPT 4.0
- [x] A compilação é bem sucedida com 0 erros e 0 avisos

## Corner cases

- [x] Garantir compatibilidade com as assinaturas de construtor do SPT 4.0.

## Histórico

| Data | Evento |
|---|---|
| 2026-07-17 | Item criado e corrigido |
