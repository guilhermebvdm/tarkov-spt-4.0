# 060 — Weapon Mastery: transparência in-game do efeito por nível

**Mod:** CustomClasses
**Status:** Backlog
**Criado:** 2026-07-04 (origem: RN-04 da review de regras de negócio do 058)

## Visão geral

As 4 maestrias que o 058 ressuscitou têm efeito por nível (−0.4% recuo / +0.2% ergo), mas a tela SKILLS não mostra
buff nenhum (os quadradinhos vêm de config do jogo que segue vazia) — a linguagem visual do vanilla "ensina" que a
skill não faz nada. Menor mecanismo de transparência: **rodapé informativo no painel da aba CLASS** (superfície
própria do mod, item 059), com os valores VIVOS do F12.

## Comportamento desejado

- No painel de classe (aba CLASS; o popover do deploy herda), um rodapé compacto e esmaecido abaixo das colunas:
  *"WEAPON MASTERY — SMG · LMG · Lançador · Underbarrel: −0.4% recuo · +0.2% ergo por nível"* (en/pt pelo idioma
  do EFT; percentuais calculados do F12 na hora — mudar o F12 muda o texto no próximo refresh).
- Só aparece com `Weapon Mastery — Enabled` ligado; some com ele desligado.
- Não interfere no layout dos cards (linha própria, fora das colunas).

## Critérios de aceite

- [ ] Rodapé visível na aba CLASS com os valores corretos do F12; some com `Weapon Mastery — Enabled` off.
- [ ] i18n: texto pt com o EFT em português; en caso contrário.
- [ ] **Fika/multiplayer:** N/A — texto local de UI, sem estado.
- [ ] **Estado entre raids:** N/A — reconstruído a cada Refresh do painel.

## Corner cases

- [ ] F12 alterado com a tela aberta → próximo Refresh reflete (não precisa live por frame).
- [ ] Popover do deploy (mesmo painel): rodapé aparece também — aceitável (informativo, 1 linha).
- [ ] Classe vanilla (painel de mensagem): rodapé aparece mesmo assim (a maestria é classless — correto).

## Histórico

| Data | Evento |
|---|---|
| 2026-07-04 | Item criado (RN-04) + spec mínima + implementação direta (item pequeno, infra do 059) |
