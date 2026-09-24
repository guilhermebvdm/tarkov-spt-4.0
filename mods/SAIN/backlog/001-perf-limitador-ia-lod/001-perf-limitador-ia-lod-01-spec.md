# 001 — Otimização do Limitador de IA por Distância (AI Limiter + LOD)

**Mod:** SAIN (`modded-multithread`)
**Status:** Backlog
**Criado:** 2026-09-19
**Perfil:** Não-regressão (`/optimize-mod-performance`, Fase 2) — origem: [relatorio-auditoria-codigo-01.md](../../docs/modded-multithread/relatorio-auditoria-codigo-01.md) (achados `AUD-01-01` a `AUD-01-04`, todos aceitos)

## Visão geral

O fork `modded-multithread` do SAIN tem dois mecanismos independentes de "economizar processamento de IA para bots longe do jogador": o `SAINAILimit` original (degrada alcance de visão/audição entre bots) e um sistema de LOD adaptativo novo, introduzido só neste fork (`BotComponent.cs`, throttla a *taxa de atualização* do bot: 60Hz perto, 25Hz médio, 8Hz longe). Os dois foram adicionados em momentos diferentes e nunca foram integrados. Isso gera trabalho redundante (cada um pergunta "onde está o humano mais próximo?" por conta própria) e uma fronteira de distância sem margem de segurança que faz a taxa de atualização "piscar" para bots parados perto do limiar. Este item corrige as duas coisas **sem mudar o comportamento que o jogador percebe** — é otimização pura, não feature nova.

## Comportamento atual

- `SAINAILimit.CheckAILimit()` (`SAIN/Classes/Bot/SAINAILimit.cs:36-46`) busca o humano mais próximo a cada ~3s, por bot, via `PlayerSpawnTracker.FindClosestHumanPlayer`.
- `BotComponent.GetMinDistanceToHumanPlayer()` (`SAIN/Components/BotComponent.cs:227-248`) busca o mesmo dado a cada 0.5s, por bot, via o mesmo método — resultado não compartilhado com o `SAINAILimit`.
- `BotComponent.cs:272` decide "bot está perto do jogador?" com um único limiar (`distToHuman <= LOD_CLOSE_DIST`, 50m), sem margem — um bot parado perto dos 50m pode alternar Tier 0 (60Hz) ⇄ Tier 1 (25Hz) a cada checagem de 0.5s.
- Os limiares do LOD (`LOD_CLOSE_DIST`, `LOD_MID_DIST`, `LOD_MID_INTERVAL`, `LOD_FAR_INTERVAL`) são `const` privadas em `BotComponent.cs:195-198` — não aparecem no editor F6, diferente de `AILimitSettings.AILimitRanges` (já editável).
- A relação entre os dois sistemas não está documentada em `docs/modded-multithread/01-arquitetura-multithread-e-lod.md`.

## Comportamento desejado

- `SAINAILimit` reusa a distância já calculada pelo LOD (`Bot.DistanceToClosestHuman`) em vez de buscar de novo — elimina a busca redundante, sem perda de precisão (a cadência do LOD, 0.5s, é mais fina que a do limiter, 3s).
- A fronteira "perto/longe" do LOD ganha uma margem de segurança (dead-band) — um bot parado perto dos 50m para de oscilar de tier a cada 0.5s.
- Os 4 limiares do LOD passam a ser editáveis no F6, no mesmo padrão de `AILimitRanges`.
- A documentação de arquitetura passa a explicar que os dois sistemas coexistem e como se relacionam.
- **Em nenhum cenário** um bot perto de um jogador humano (host, convidado Fika, ou em raid Headless) deve receber tratamento diferente do que recebe hoje — a correção é só de eficiência/estabilidade interna.

## Critérios de aceite

- [ ] `SAINAILimit` não chama mais `PlayerSpawnTracker.FindClosestHumanPlayer` diretamente — lê `Bot.DistanceToClosestHuman`.
- [ ] Bot parado a ~50m de um jogador, por 2 minutos, não troca de `CurrentLodTier` mais que 1 vez (a transição inicial ao chegar na distância é esperada; oscilação repetida não).
- [ ] Os 4 limiares do LOD aparecem no editor F6 e alteram o comportamento em tempo real quando ajustados.
- [ ] Comportamento sensorial do `SAINAILimit` (alcance de visão/audição degradado por distância) permanece idêntico ao atual — os thresholds de `AILimitRanges` (150/250/400m) não mudam.
- [ ] Comportamento do LOD (quem entra em Tier 0 por combate/dano recente/lockout) permanece idêntico — só a fronteira de distância parada ganha margem.
- [ ] **Nenhum** membro `public`/`protected` existente de `SAINAILimit`, `AILimitSettings`, `BotComponent`, `PlayerSpawnTracker` ou `PlayerComponent` foi renomeado, removido ou teve assinatura alterada (checar diff completo). Só adição de membros novos é permitida.
- [ ] **Fika/multiplayer:** um bot perto de um jogador **convidado** (não-host) continua recebendo Tier 0/atualização plena — a busca de distância (`FindClosestHumanPlayer`, filtra por `PlayerComponent.IsAI`) não muda para depender de `MainPlayer`/`IsYourPlayer`.
- [ ] **Headless:** o mesmo vale em raid hospedada por instância Headless (sem jogador local) — a fonte de "quem é humano" continua sendo `PlayerComponent.IsAI` (via `BotOwner` presente), nunca conceito de jogador local.
- [ ] **Estado entre raids:** raid1 → exit → raid2 não herda estado/contadores de tier do bot anterior (cada `BotComponent` é recriado por raid, comportamento já correto hoje — não deve regredir).

## Corner cases

- [ ] Raid sem nenhum jogador humano vivo por perto (cenário raro, ex.: todos mortos/desconectados) — `SAINAILimit` deve continuar recebendo `float.MaxValue`/fallback igual ao `BotComponent` já trata hoje (`BotComponent.cs:271-272`, `CR-01-01` já resolvido), não reintroduzir o bug de bot preso em Tier 0 permanente.
- [ ] Troca de valor dos limiares do LOD pelo F6 **durante** uma raid em andamento — não deve quebrar bots já instanciados (novos valores devem valer no próximo ciclo de checagem, sem exceção).
- [ ] Bot exatamente na borda da margem (dead-band) — comportamento determinístico, sem flutuação por ponto flutuante (usar comparação com folga, não igualdade exata).

## Fora de escopo

- [ ] Incorporar contagem de bots ativos/em combate simultâneos na decisão de tier (mencionado no relatório como melhoria futura, não incluído nesta rodada).
- [ ] Aplicar o padrão `TryGetValue` defensivo em outros consumidores de dicionário do `AILimitSettings` além do já corrigido em `EnemyVisionClass.cs`.
- [ ] Qualquer mudança no raycast de linha de visão (`VisionRaycastJob.cs`) — já auditado nesta sessão, oclusão confirmada intacta, fora do escopo de performance deste item.

## Referências

- [relatorio-auditoria-codigo-01.md](../../docs/modded-multithread/relatorio-auditoria-codigo-01.md) — achados `AUD-01-01` a `AUD-01-04`, evidência completa e plano de validação.
- [docs/modded-multithread/02-code-review-etapa-1.md](../../docs/modded-multithread/02-code-review-etapa-1.md) — `CR-01-01` a `CR-01-04`, contexto dos bugs já corrigidos no LOD.
- [docs/modded-multithread/01-arquitetura-multithread-e-lod.md](../../docs/modded-multithread/01-arquitetura-multithread-e-lod.md) — arquitetura atual do LOD, a ser atualizada por este item (`AUD-01-04`).

## Histórico

| Data | Evento |
|---|---|
| 2026-09-19 | Item criado via `/optimize-mod-performance SAIN --fase 2`, a partir do relatório de auditoria 01 |
