# Backlog — Project FIKA

> Índice canônico de itens de backlog do mod FIKA (SPT 4.0). Cada linha aponta para uma pasta `NNN-<slug>/` com spec funcional, técnica, revisões e asbuilds.

| # | Título | Resumo | Pasta | Status |
|---|---|---|---|---|
| 001 | Fix descarte de mochila ("ZZ" / DropBackpack) no Headless/Host | Correção do erro `hands controller can't perform this operation` ao dropar mochila com duplo toque rápido ("ZZ") em raids multiplayer via Headless/Host. Patch Harmony `ObservedPlayer_DropBackpackSafety_Patch` em `Player.TryRemoveFromHands`. | [001-drop-backpack-sync-fix/](./001-drop-backpack-sync-fix/) | 🟢 |
| 002 | Watchdog de Timeout de Inventário e Desync em Coop/Headless | Mecanismo de auto-recuperação e timeout de 5 segundos para callbacks não respondidos de rede em `FikaPlayer.WaitingForCallback`, prevenindo bloqueio de disparos e granadas após perdas de pacote em recargas. | [002-inventory-desync-watchdog/](./002-inventory-desync-watchdog/) | 🟢 |
| 003 | Fix rejeição de swap de magazine in-place no Headless (GClass1561) | Correção da rejeição `GClass1561`/`inOutHandsProcess` ao trocar magazine 1-para-1 em arma empunhada via FIKA Headless, sem enfraquecer a proteção de concorrência herdada do EFT. | [003-magazine-swap-inplace-fix/](./003-magazine-swap-inplace-fix/) | 🟢 |
| 004 | Swap de carregador rejeitado após curar aliado (colisão com transição de mãos não-magazine) | `IsSelfReferentialMagazineSwap` só tolera colisão magazine-vs-magazine; arma reequipada após curar um aliado (TRL-ImmersiveCombatMedicine) abre um Begin não-magazine e o swap de carregador em seguida é rejeitado pelo Host (`GClass1561`), travando a mão do jogador. | [004-colisao-cura-swap-magazine/](./004-colisao-cura-swap-magazine/) | ⚪ |
