# 001 — Sincronização Contínua de Clima em Raid · Review Técnica 02

**Mod:** TRL-WeatherSync
**Spec técnica revisada:** [001-sincronizacao-continua-clima-raid-02-spec-tech.md](001-sincronizacao-continua-clima-raid-02-spec-tech.md)
**Data:** 2026-09-10

> Análise crítica focada na adição do §1.1 (modelo de 3 papéis: `Source`/`Relay`/`Receiver`, resolve P-2.2 da memória) — mudança de desenho feita após a review 01 já ter zerado os 6 pontos anteriores, a pedido do usuário ("virar host-convidado" para clima em raid Headless). Cada ponto recebe um ID `PA-02-MM`.

## Resumo

> 🔴 Bloqueadores: 1 · 🟡 Importantes: 0 · 🟢 Menores: 1 · ✅ Resolvidos: 2 · Total: 2

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-02-01 | C | 🔴 | Eco do relay pro `Source` original causa `ParseException` — `Source` não registra handler de recepção | ✅ Resolvido em 2026-09-10 |
| PA-02-02 | B | 🟢 | Sincronização do evento `FikaNetworkManagerCreatedEvent` com `WeatherSyncSession.Instance` ainda não instanciado | ✅ Resolvido em 2026-09-10 |

## Categorias

- **A — Gaps de Especificação:** informações ausentes que ambiguam a implementação
- **B — Edge Cases:** cenários válidos não cobertos
- **C — Erros de Lógica:** pressupostos errados, contradições, código incompatível com SPT 4.0+

## Impacto

- 🔴 **Bloqueador** — impede implementar ou causa bug/crash garantido
- 🟡 **Importante** — pode causar comportamento errado em cenário relevante
- 🟢 **Menor** — qualidade/clareza, não bloqueia

---

## Pontos

### PA-02-01 · C — Erro de Lógica · 🔴 Bloqueador · ✅ Resolvido em 2026-09-10

**Eco do relay pro `Source` original causa `ParseException` na própria máquina do jogador que iniciou a raid**

**Problema:** No desenho original do §1.1/§5.2 (antes desta review), `Role = Source` **não registra nenhum handler de recepção** para `TrlWeatherSyncPacket` — só envia (`EnsurePacketsRegistered` tinha `case WeatherRole.Source: break;`, sem registrar nada). Mas no Caso B (§6, raid Headless), o `Relay` reencaminha o pacote via `Broadcast()` = `SendData(..., broadcast: true)` — e, do lado do **servidor**, "enviar pra todos" alcança **todos os peers conectados, incluindo o peer do Cliente que originalmente mandou o pacote** (confirmado: um servidor não tem a si mesmo como peer, mas o Cliente `Source` É um peer normal do servidor, então recebe o eco de volta). Como esse Cliente nunca registrou handler nenhum para `TrlWeatherSyncPacket` (papel `Source` não registrava), `NetPacketProcessor.GetCallbackFromData` não encontra a hash no dicionário `_callbacks` e **lança `ParseException`** (confirmado lendo [`NetPacketProcessor.cs:83-91`](../../../../mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/LiteNetLib/Utils/NetPacketProcessor.cs#L83) nesta review) — exatamente o mesmo mecanismo do PA-01-02 (review 01), agora reintroduzido pelo próprio desenho do relay.

**Por que importa:** Isso quebraria a rede pro jogador que **iniciou a raid** — ironicamente, a pessoa cuja ideia motivou essa mudança de desenho seria a primeira a sofrer o bug. Aconteceria a cada ciclo de broadcast (a cada `SyncIntervalSeconds`), derrubando a fila de eventos de rede daquele frame pra esse jogador repetidamente.

**Sugestão:** `Role = Source` deve registrar o **mesmo handler** que `Role = Receiver` (`RegisterPacket<TrlWeatherSyncPacket>(OnWeatherSyncPacketReceived)`), além de continuar enviando pelo seu próprio accumulator. O eco recebido de volta faz `Source` reaplicar em si mesmo um clima quase idêntico ao que já tem (via `SetWeatherForce`) — redundante, mas inofensivo (é literalmente o mesmo padrão já aceito na descrição do fluxo de dados do Caso B). Simplifica o `switch` em `EnsurePacketsRegistered` pra: `Source` e `Receiver` registram igual; só `Relay` é diferente.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** `EnsurePacketsRegistered` (`001-...-02-spec-tech.md` §5.2) alterado — `case WeatherRole.Source:` e `case WeatherRole.Receiver:` agora caem no mesmo `RegisterPacket<TrlWeatherSyncPacket>(OnWeatherSyncPacketReceived)`. `WeatherSyncSession.Update()` (§5.4) mantido como estava — o `if (Role != WeatherRole.Source) return;` antes do envio já garante que só `Source` tenta transmitir, independente de também ter um handler de recepção registrado.

---

### PA-02-02 · B — Edge Case · 🟢 Menor · ✅ Resolvido em 2026-09-10

**`FikaNetworkManagerCreatedEvent` pode disparar antes de `WeatherSyncSession.Instance` existir**

**Problema:** `OnManagerCreated` (§5.2) chama `EnsurePacketsRegistered(WeatherSyncSession.Instance?.Role ?? WeatherRole.Unset)`. O evento `FikaNetworkManagerCreatedEvent` dispara durante a inicialização de rede do FIKA ([`fika-packet-desync-prevention-plan.md` §4.1](../../../../docs/technical/fika-packet-desync-prevention-plan.md#41-padrão-híbrido--a-api-de-eventos-do-fika--polling)), que pode acontecer **antes** de `GameWorld.OnGameStarted()` (o hook que cria `WeatherSyncSession.Instance` via `Begin()`, §2). Se isso ocorrer, `Instance` é `null`, o papel resolve pra `Unset`, e `EnsurePacketsRegistered` retorna sem registrar nada nesse disparo do evento.

**Por que importa:** Não é um bug por si só — o próprio doc canônico já avisa que o padrão híbrido é "evento + polling como rede de segurança", exatamente pra esse tipo de janela. O polling no `Update()` (§5.4, chamado todo frame) reconcilia isso assim que `Instance`/`Role` existir. Mas a spec não deixava essa relação explícita, o que pode confundir um implementador lendo só o `OnManagerCreated` isolado achando que é a única via de registro.

**Sugestão:** Adicionar um comentário em `OnManagerCreated` (§5.2) explicando que `Unset` é esperado nesse caso e que o polling em `WeatherSyncSession.Update()` cobre a lacuna — sem mudança de lógica, só documentação inline pra não parecer um bug.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Comentário adicionado ao método `OnManagerCreated` em `001-...-02-spec-tech.md` §5.2.
