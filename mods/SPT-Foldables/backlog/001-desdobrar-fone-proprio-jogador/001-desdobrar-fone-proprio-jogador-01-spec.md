# 001 — Auto-desdobrar fone só no próprio jogador

**Mod:** SPT-Foldables
**Status:** Backlog
**Criado:** 2026-09-24

## Visão geral

O mod tenta desdobrar automaticamente o fone de ouvido do próprio jogador sempre que a aba de equipamento é mostrada durante uma raid — **independente de haver ou não um container externo aberto ao lado** (corpo, bot, outro jogador, baú, mochila no chão). A ação de desdobrar não é instantânea — há uma espera real (o tempo de animação de dobra do item) antes de agir — e nessa janela o jogador pode já ter fechado aquele container externo ou trocado pra outro. Quando a ação atrasada finalmente executa, ela reconstrói a tela de inventário por cima do que está sendo mostrado no momento — e é justamente a reconstrução do painel do **container externo** que duplica/corrompe a lista de itens dele. Reproduzido em raid real: erro de "item duplicado" no log (originado especificamente no processamento do equipamento do corpo sendo lootado, não no do próprio jogador), painel do corpo mostrando só o colete, e o colete "fantasma" persistindo até no inventário do próprio jogador (tecla de inventário).

## Comportamento atual

- A rotina de auto-desdobrar do fone roda toda vez que a aba de equipamento é mostrada durante uma raid, **independente de haver ou não um container externo sendo mostrado junto** (corpo, bot, outro jogador, baú, mochila no chão).
- Se o fone do jogador estiver dobrado, a rotina inicia uma espera real (o tempo de animação de dobra do item) antes de agir — não é instantâneo.
- Não há nenhuma checagem de que a tela ainda mostra o mesmo container externo (ou nenhum) quando essa ação atrasada finalmente dispara.

## Comportamento desejado

- A rotina de auto-desdobrar do fone só executa quando **nenhum container externo** está sendo mostrado ao lado do próprio equipamento — ou seja, só quando o jogador está olhando exclusivamente o próprio inventário.
- Assim que houver qualquer container externo aberto (corpo, bot, outro jogador, baú, mochila no chão), a rotina não faz nada — não inicia a espera nem agenda nenhuma ação atrasada.
- O comportamento de auto-desdobrar continua idêntico ao atual quando não há nenhum container externo envolvido (tecla de inventário sozinha, ou tela de equipamento no hideout/menu, se aplicável).

## Critérios de aceite

- [ ] Abrir o painel de loot de um corpo/bot com o fone de ouvido do jogador dobrado **não** dispara a rotina de auto-desdobrar (nem agenda a espera de dobra).
- [ ] Abrir só o próprio inventário (sem nenhum container externo aberto) com o fone dobrado continua desdobrando automaticamente — comportamento original preservado.
- [ ] Abrir e fechar o loot do **mesmo** corpo várias vezes seguidas não duplica nem corrompe o painel (sem `ArgumentException: same key already added` nos logs).
- [ ] Após lootear um corpo cujo fone do jogador estava dobrado, fechar o loot e olhar só o próprio inventário não mostra nenhum rig/painel "fantasma" residual daquele corpo.
- [ ] **Fika/multiplayer:** validado tanto pelo host quanto por um convidado, com qualquer container externo aberto (corpo/bot/outro jogador humano vivo/baú/mochila no chão) — em nenhum desses casos a rotina de auto-desdobrar dispara.
- [ ] **Estado entre raids:** N/A — a decisão é tomada a cada chamada com base no que está sendo mostrado naquele momento, sem nenhum estado guardado entre raids.

## Corner cases

- [ ] Jogador abre o inventário de outro **jogador humano vivo** (não bot/corpo) — por exemplo durante uma revive — conta como container externo aberto igual a um corpo; mesma regra se aplica.
- [ ] Container externo que não é um conjunto completo de equipamento (ex.: baú, mesa de carregamento, mochila solta no chão) — ainda deve suprimir a rotina, mesmo sem ter um slot de fone de ouvido relevante, porque é, ainda assim, um container externo sendo mostrado.
- [ ] Modo singleplayer/hideout sem nenhum container externo aberto — comportamento deve permanecer idêntico ao original.
- [ ] Fone de ouvido ausente (slot vazio) — já tratado hoje sem erro; a mudança não pode alterar esse comportamento.
- [ ] Quando não for possível determinar com segurança se há um container externo aberto (ex.: contexto inesperado, fora dos casos previstos) — a rotina não deve executar; prefere não desdobrar automaticamente a arriscar reabrir a reentrância que causa o bug.

## Fora de escopo

- [ ] A definir

## Referências

- [Foldables (upstream, v1.0.3)](https://github.com/ozen-m/SPT-Foldables/releases/tag/v1.0.3)
- [mod-backlog.md](../mod-backlog.md)
- Investigação original: raid real em coop (FIKA `modded-V2`) durante invasão de raid via headless, log do jogador convidado mostrando `ArgumentException: An item with the same key has already been added. Key: TacticalVest` e `NullReferenceException` em `EFT.UI.DragAndDrop.GridItemView.UpdateInfo`, ambos originados em `Foldables.Patches.Operations.InRaid.InventoryScreenShowPatch.Postfix`.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-24 | Item criado via `/add-backlog-item`, a partir de investigação de bug de rig acumulando/corrompendo em corpos lootados em coop — causa raiz identificada no código-fonte do Foldables (`async void` reentrante em `ItemsPanel.Show`, sem filtro de dono do inventário) |
| 2026-09-24 | Revisão `/review-spec` — 2 gaps corrigidos (Visão geral e Comportamento atual reescritos sem nomes de classe/método do EFT, que são problema da spec técnica; um critério de aceite reescrito pra remover termo de código "coroutine") + 2 corner cases adicionados (reconexão à raid; fallback seguro quando o dono do inventário não pode ser determinado) |
| 2026-09-24 | Revisão `/review-technical-spec` 01 (PA-01-01, 🔴) encontrou erro de lógica: o critério "dono do inventário" nunca distingue jogador de corpo (o parâmetro que representa o jogador é sempre o do próprio cliente); reencaixada pra "há ou não container externo aberto junto" — Visão geral, Comportamento atual/desejado, critérios e corner cases reescritos; corner case de reconexão removido (deixou de ser relevante, a nova checagem não depende de identidade de jogador) |
