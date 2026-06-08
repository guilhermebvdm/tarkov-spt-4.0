# 009 — Ocultar edições vanilla no launcher

**Mod:** CustomClasses
**Status:** Backlog
**Criado:** 2026-06-07

> Numerado 009 (não 004) porque os números 004-008 já estão reservados no roadmap; este item é independente e entrou depois.

## Visão geral

Permitir **esconder edições da tela de criação de perfil** do launcher, configurável via JSON (sem recompilar). Objetivo imediato: deixar a lista enxuta — ocultar as edições vanilla que não queremos como ponto de partida e manter só as úteis + as classes do mod.

## Comportamento atual

A tela "criar perfil" do launcher lista **todas** as edições registradas: as vanilla (Standard, Left Behind, Prepare To Escape, Edge Of Darkness, Unheard, Tournament, SPT Developer, SPT Easy start, SPT Zero to hero) **mais** as classes do CustomClasses. A lista fica poluída com edições que não queremos oferecer.

## Comportamento desejado

Uma **lista configurável (JSON)** define quais edições ficam **ocultas** na criação de perfil. O launcher passa a listar apenas as não-ocultas (+ as classes do mod). Por **default**, o mod já oculta as 7 vanilla indesejadas, mantendo só `SPT Developer` e `SPT Zero to hero` (além das classes do mod). Editar a lista e reiniciar o servidor muda o que aparece.

## Critérios de aceite

- [ ] Existe uma config (JSON) que lista as edições a **ocultar** na criação de perfil.
- [ ] Por default, ficam **ocultas**: Standard, Left Behind, Prepare To Escape, Edge Of Darkness, Unheard, Tournament, SPT Easy start.
- [ ] Permanecem **visíveis**: SPT Developer, SPT Zero to hero, e **todas as classes do CustomClasses**.
- [ ] Editar a lista (sem recompilar) e reiniciar o servidor muda o que aparece no launcher.
- [ ] Ocultar uma edição **não** impede perfis já existentes criados com ela de carregar/jogar (só some da criação de **novos**).

## Corner cases

- [ ] **Chave de edição inexistente na lista:** ignorada (log), sem erro.
- [ ] **Lista vazia / ausente:** nada é ocultado (todas aparecem) — comportamento atual.
- [ ] **Ocultar acidentalmente uma classe do mod:** só ocultar o que está na lista; as classes do mod nunca são ocultadas por default.
- [ ] **Outros mods que adicionam edições:** só ocultamos as que estiverem na lista; edições de outros mods não são afetadas a menos que listadas.
- [ ] **Ordem de carregamento:** a ocultação precisa ser aplicada antes do launcher consultar a lista de edições.

## Fora de escopo

- Itens/skills/outfits/multiplicadores/i18n — outros itens.
- Renomear/reordenar edições no launcher — só ocultar.

## Referências

- Mecanismo do SPT: o launcher filtra as edições por uma blacklist (`CoreConfig.Features.CreateNewProfileTypesBlacklist`) — confirmado em `LauncherController`/`LauncherV2Controller` (a confirmar no tech spec).
- Item 002 (loader/config do mod): [002-class-schema-loader-01-spec.md](../002-class-schema-loader/002-class-schema-loader-01-spec.md)

<!-- review: blacklist (listar o que OCULTAR) vs whitelist (listar o que MOSTRAR)? Proposta: blacklist — alinha com o CreateNewProfileTypesBlacklist do SPT e evita sumir com as classes do mod por engano. Onde fica a config: arquivo próprio (config/launcher.jsonc) ou dentro de um masterConfig do mod? -->

## Histórico

| Data | Evento |
|---|---|
| 2026-06-07 | Item criado via `/add-backlog-item` |
| 2026-06-07 | Spec funcional criada via `/create-spec` |
