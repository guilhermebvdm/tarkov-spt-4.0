# 003 — Bloquear Desmembramento de Perna em Boss/Escolta Vivos

**Mod:** VisceralCombat
**Status:** Backlog
**Criado:** 2026-09-10

## Visão geral

O mecanismo de desmembramento de perna em bots vivos (rastejar/agonia, item 001) hoje se aplica a **qualquer** bot vivo, sem distinguir um Scav raso de um Boss (Killa, Tagilla, Glukhar, etc.) ou da escolta dele. Este item restringe essa feature: Boss e escolta vivos nunca perdem a perna em combate — a mecânica de rastejo/agonia fica reservada a bots comuns. O desmembramento **pós-morte** (cadáver já morto sendo atingido depois) continua liberado geral, sem essa restrição — Boss morto pode ser desmembrado normalmente.

## Comportamento atual

- Ao atingir a perna de um bot vivo, há 30% de chance por disparo de desmembrá-la, prendendo o bot no chão em agonia/rastejo (`LimbKillPatch`, ramo "vivo"). A única checagem existente antes disso é: o alvo é um bot (`IsAI`) e todos os jogadores humanos do raid têm o mod instalado (`AllPlayersHaveVisceralCombat`). Não há nenhuma distinção por papel/tipo de bot — Boss e escolta são afetados igual a um Scav comum.
- O desmembramento pós-morte (cadáver já morto) não tem essa checagem de "vivo" — já roda para qualquer corpo, Boss incluso, e este item não altera esse caminho.

## Comportamento desejado

- Um Boss (ex.: Killa, Tagilla, Reshala, Glukhar, Shturman, Sanitar, Kaban, Kollontay, Partizan, Zryachiy, Knight/BigPipe/BirdEye, sectários) ou um membro da escolta dele, **enquanto estiver vivo**, nunca entra no estado de desmembramento de perna/rastejo — o tiro na perna causa dano normal, sem acionar a mecânica de agonia forçada do mod.
- Assim que esse mesmo Boss/escolta morrer, o desmembramento pós-morte volta a valer normalmente (pernas, braços, cabeça — sem restrição adicional).
- Bots comuns (Scav, PMC-bot, Raider, infectado, etc.) continuam funcionando exatamente como hoje — sem nenhuma mudança de comportamento pra eles.

## Critérios de aceite

- [ ] Atirar na perna de um Boss vivo (ex.: Killa) não aciona o rastejo/agonia forçada — o Boss continua se movendo/lutando normalmente após levar o tiro.
- [ ] Atirar na perna de um membro da escolta de um Boss vivo tem o mesmo resultado — sem desmembramento/agonia forçada.
- [ ] Atirar na perna de um bot comum (Scav/PMC-bot/Raider) continua acionando o desmembramento em vivos exatamente como antes (30% de chance, sem regressão).
- [ ] Depois que o Boss ou a escolta morre, um tiro subsequente na perna (ou cabeça/braço) do cadáver desmembra normalmente — sem nenhuma restrição adicional pós-morte.
- [ ] **Fika/multiplayer:** o bloqueio é decidido localmente a partir do papel do bot (`WildSpawnType`, dado sincronizado do profile), então o resultado é o mesmo em todos os peers — não depende de round-trip de rede nem de qual peer detectou o tiro primeiro.
- [ ] **Estado entre raids:** a checagem é feita a cada tiro, sem guardar nenhum estado; não há nada para persistir ou limpar entre raids.

## Corner cases

- [ ] Boss cuja função (`WildSpawnType`) o EFT ainda não classifica como boss numa atualização futura do jogo — o bloqueio depende da tabela interna do jogo (`WildSpawnType.IsBossOrFollower()`), não de uma lista hardcoded no mod; se o jogo já não classificar como boss, o mod naturalmente segue essa classificação (sem lista própria pra manter atualizada).
- [ ] Bot que muda de "vivo" pra "downed" (caído mas não morto — ex.: mecanismo de "fica no chão mas não morreu") — a checagem de boss/escolta deve valer nesse estado também, já que o gate atual de "vivo" (`isDead`) já engloba `RagdollHelperClass.IsPlayerDowned`.
- [ ] Escolta que perde o vínculo com o Boss (Boss já morreu, escolta continua viva) — `IsBossOrFollower()` é uma propriedade do papel (`WildSpawnType`) do próprio bot, não depende do Boss estar vivo, então a escolta continua protegida mesmo depois do Boss cair.
- [ ] Jogador humano (não bot) — já é coberto pelo gate existente (`!player.IsAI` já bloqueia qualquer humano); este item não muda esse comportamento, só adiciona a checagem de boss/escolta **dentro** do universo que já é bot.
- [ ] `player.Profile`/`Profile.Info`/`Profile.Info.Settings` nulo no momento do tiro (ex.: bot em transição de spawn) — a checagem de boss/escolta deve falhar de forma segura (não travar, não crashar) e cair no comportamento padrão de bot comum (não tratar como boss por engano nem quebrar o desmembramento normal por uma referência nula).

<!-- review: revisado — sem gaps adicionais que exijam decisão humana antes da spec técnica. -->

## Fora de escopo

- [ ] A definir

## Referências

- `LimbKillPatch.cs:68-71` (ponto exato do gate atual, "vivo + IsAI + AllPlayersHaveVisceralCombat")
- `BotSettingsRepoClass.cs:555-559` (`WildSpawnType.IsBossOrFollower()`, API canônica do EFT já confirmada no Assembly)
- Item de backlog anterior: `001-alive-leg-dismemberment` (feature original que este item restringe)

## Histórico

| Data | Evento |
|---|---|
| 2026-09-10 | Item criado via `/add-backlog-item` |
| 2026-09-10 | Revisão `/review-spec` — 1 corner case adicionado (Profile/Settings nulo, defesa contra crash) |
