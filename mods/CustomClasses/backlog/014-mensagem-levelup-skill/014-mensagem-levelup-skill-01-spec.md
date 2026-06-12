# 014 — Mensagem de level-up de skill (buff/debuff)

**Mod:** CustomClasses
**Status:** Backlog
**Criado:** 2026-06-08

## Visão geral

Customizar a **notificação de level-up de skill** (o popup "⚠ X skill leveled up to Y" que aparece no jogo) para as skills que têm **multiplicador da classe**, dando um toque temático: skills com **buff** sobem "fácil", skills com **debuff** sobem "finalmente". Puramente cosmético (não altera XP nem nível).

## Comportamento atual

Ao subir uma skill, o jogo mostra a notificação vanilla: **"⚠ <skill> skill leveled up to <lvl>"** (ex.: "Endurance skill leveled up to 9"), igual para qualquer skill.

## Comportamento desejado

Quando a skill que subiu tem **multiplicador da classe** (`fator ≠ 1`), a mensagem é reescrita:

- **Buff** (`fator > 1`): **"⚠ <skill> skill `EASILY` leveled up to <lvl> ;)"** — a palavra **`EASILY`** em **verde** (mesma cor de buff do mod); o `;)` no fim.
- **Debuff** (`fator < 1`): **"⚠ <skill> skill `FINALLY` leveled up to <lvl>"** — a palavra **`FINALLY`** em **vermelho** (mesma cor de debuff do mod).
- **Sem multiplicador** (ou edition vanilla): mensagem **vanilla** intacta.

Só a palavra-chave (`EASILY`/`FINALLY`) é colorida; o resto do texto segue o estilo da notificação. Textos em **inglês** (a notificação do jogo é em inglês).

## Critérios de aceite

- [ ] Subir uma skill com **buff** da classe → notificação "… skill **EASILY** leveled up to N ;)" com **EASILY em verde**.
- [ ] Subir uma skill com **debuff** da classe → notificação "… skill **FINALLY** leveled up to N" com **FINALLY em vermelho**.
- [ ] Subir uma skill **sem multiplicador** (ou perfil de edition vanilla) → notificação **vanilla** inalterada.
- [ ] O `<skill>` e o `<lvl>` continuam corretos (nome real da skill + nível alcançado).
- [ ] A cor verde/vermelha é a **mesma** usada no resto do mod (consistência com o marcador/selo).

## Corner cases

- [ ] **Skill bloqueada/"beta"** (Locked): não sobe de nível → não dispara a notificação (sem caso).
- [ ] **Level-up fora de raid** (hideout/menu — ex.: Crafting, HideoutManagement): a customização também vale (a notificação é a mesma).
- [ ] **Múltiplos level-ups seguidos** (ex.: com multiplicador alto subindo vários níveis): cada notificação é reescrita corretamente.
- [ ] **i18n** (item 008): a notificação do jogo é em inglês; manter inglês por ora. Se o seletor de idioma do mod for aplicar aqui no futuro, centralizar as strings (evitar hardcode espalhado).
- [ ] **Cache da classe não carregado** ainda: garantir o mesmo `EnsureLoaded()` usado nos outros patches (a sessão existe quando uma skill sobe).

## Fora de escopo

- Alterar o ganho de XP ou o nível (só o texto da notificação).
- Customizar outras notificações (mastering, achievements, etc.).
- Tradução das palavras-chave (inglês por ora; i18n futuro).

## Referências

- Cores/consistência: `modded/Client/MultiplierFormat.cs` (verde/vermelho do mod).
- Dados da classe no client: `modded/Client/SkillMultipliers.cs` (fator por skill).
- Escala de XP correlata: item 005; UI dos multiplicadores: item 010.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-08 | Item criado (pedido do usuário) + decisão de texto/cor travada: buff=`EASILY` verde +`;)`, debuff=`FINALLY` vermelho (semântico). |
