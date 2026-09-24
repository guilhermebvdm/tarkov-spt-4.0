# 001 — Fix 01 · Tempestade sincronizada (CR-01-02) causava nevasca de inverno em vez de tempestade de verão

**Mod:** TRL-WeatherSync
**Item raiz:** [001-sincronizacao-continua-clima-raid-01-spec.md](001-sincronizacao-continua-clima-raid-01-spec.md)
**Asbuild:** [001-sincronizacao-continua-clima-raid-05-asbuild.md](001-sincronizacao-continua-clima-raid-05-asbuild.md)
**Criado:** 2026-09-12
**Disparado por:** feedback in-raid do usuário — o convidado (Receiver) numa raid hospedada pelo usuário (Host/Source) viu a estação mudar de Verão para Inverno no meio da raid, sem nenhuma ação sua. Usuário levou o caso pra uma segunda IA (Gemini) investigar; o diagnóstico dela foi trazido de volta e verificado linha por linha contra o Assembly nesta sessão.

## Contexto

O item 001 já tinha uma feature de "tempestade sincronizada" (`RollForStorm`, aplicada no code-review CR-01-02 da rodada 01): a cada ciclo de broadcast, o papel `Source` sorteava contra `IWeatherCurve.LightningThunderProbability`; se ganhasse, mandava `ThunderEventTrigger = true` no pacote, e o `Receiver` chamava `Class443.Controller?.HandleReconnect(ESeasonStatus.Storm, SeasonsSettingsClass.Default)`.

Em raid real (Host no Ground Zero, convidado no Shoreline), o convidado relatou a estação mudando de Verão pra Inverno no meio da partida — comportamento nunca intencionado (a feature deveria produzir uma tempestade de verão, não uma troca de estação).

## Causa raiz

Confirmado lendo `Assembly-CSharp/Class444.cs` e `Assembly-CSharp/RainController.cs` diretamente (não recon — `arquivo.cs:linha` de cada ponto):

1. **`Class443.Controller?.HandleReconnect(ESeasonStatus.Storm, ...)` só tem algum efeito se o `Class443.Controller` do cliente que recebe estiver correntemente em `Class446` (SummerState) — [`Class444.cs:94-99`](../../../../references/eft-decompiled/Assembly-CSharp/Class444.cs#L94) mostra que a implementação BASE de `HandleReconnect` (herdada por toda estação que não seja `Class446`) é um no-op puro (só loga um trace e retorna). Só `Class446.HandleReconnect` ([`Class444.cs:181-224`](../../../../references/eft-decompiled/Assembly-CSharp/Class444.cs#L181)) realmente processa o `case ESeasonStatus.Storm`, criando `Class452` ([`Class444.cs:459-465`](../../../../references/eft-decompiled/Assembly-CSharp/Class444.cs#L459)).
2. **`Class452` chama `RainController.method_9()`** no construtor — que despacha pra `Class668_0.vmethod_7()`. O estado corrente do `RainController` por padrão é `Class670` (`ERainControllerStatus.Summer`, [`RainController.cs:248-254`](../../../../references/eft-decompiled/Assembly-CSharp/RainController.cs#L248)), cujo `vmethod_7()` **sempre** cria `Class678` (`ERainControllerStatus.WinterStormReconnect`, [`RainController.cs:275-280`](../../../../references/eft-decompiled/Assembly-CSharp/RainController.cs#L275)) — não existe nenhum branch condicional por estação real; o mapeamento é fixo.
3. **`Class678` herda de `Class675`**, cujo construtor ([`RainController.cs:354-367`](../../../../references/eft-decompiled/Assembly-CSharp/RainController.cs#L354)) desliga TODA a renderização de chuva (`_wetRenderer.enabled=false`, `_rainFallDrops.SetActive(false)`, `_rainSplashController.SetActive(false)`) e liga TODA a renderização de neve (`_snowWetRenderer.WinterShow=true`, `_snowWetRenderer.enabled=true`, `_snowFlakes.gameObject.SetActive(true)`) — incondicionalmente. **O `RainController` só tem UM "modo tempestade" no jogo inteiro, e é o efeito de nevasca do Winter Event — não existe um "modo tempestade de verão" separado.** `Class451`/`Class452` (o lado de `Class444`) são de fato marcados com `ESeason.Summer` no construtor, mas essa tag não influencia em nada o efeito visual real, que vem do `RainController` e é sempre o de neve.
4. **Assimetria Host/Convidado confirmada em código próprio:** [`WeatherSyncNetworkHandler.cs:46-75`](../../modded/Client/Networking/WeatherSyncNetworkHandler.cs#L46) — o broadcast do `Source` não volta pra ele mesmo pela rede num Host normal (sem Fika-Headless); só quem *recebe* o pacote roda `OnWeatherSyncPacketReceived` → `ApplyReceivedWeather`. O Host que sorteou `ThunderEventTrigger=true` nunca aplica o efeito em si mesmo — só o(s) Receiver(s) veem a troca.

Isso **refuta** a premissa original do `04-code-review-01.md` (CR-01-02, aceita e aplicada na rodada 01) de que `HandleReconnect(ESeasonStatus.Storm, ...)` produziria uma tempestade de verão sincronizada. O documento de review original não é editado (imutabilidade, `repo-workflow-best-practices` §5) — esta é a correção, registrada aqui e citando o achado original.

## Mudanças aplicadas

| Arquivo | Mudança |
|---|---|
| `modded/Client/WeatherSyncSession.cs` | Removido `RollForStorm()`, o campo `_stormCooldownRemaining`, e a atribuição `ThunderEventTrigger = RollForStorm(curve)` em `BroadcastCurrentWeather()`. Removido o bloco `if (packet.ThunderEventTrigger) { Class443.Controller?.HandleReconnect(...) }` em `ApplyReceivedWeather()`. |
| `modded/Client/Networking/TrlWeatherSyncPacket.cs` | Campo `ThunderEventTrigger` mantido no layout serializado (evita mudar o formato do pacote — AP-11), mas agora sempre `false`; comentário XML atualizado explicando que é vestigial. |
| `modded/Client/Plugin.cs` | Removida a `ConfigEntry<float> StormCheckCooldownSeconds` (seção "Storm" inteira). Versão `1.1.1 → 1.1.2`. |
| `TRL-WeatherSync.csproj` | `<Version>1.1.1 → 1.1.2</Version>`. |
| `PROPRIEDADES.md` | Seção "Storm" substituída por nota explicando a remoção. |

A sincronização contínua de `Rain`/`Cloudness`/`Wind`/`ScaterringFogDensity` (via `SetWeatherForce`, inalterada) continua funcionando normalmente — quando a curva de clima do Host manda chuva/nuvem altos, o próprio jogo já produz trovão/raio nativamente nos clientes, sem precisar de nenhuma troca de estado especial. Nenhuma feature de "tempestade" foi perdida de fato — só o mecanismo quebrado que tentava fazer uma transição de estado explícita foi removido.

## Checklist de validação (obrigatório antes de marcar o fix como entregue)

- [x] Compila via `dotnet build` sem erros (0 erros, 0 avisos) — `/compile-mod` ainda pendente (ver Histórico)
- [ ] **In-raid:** comportamento corrigido observado em raid real — clima de chuva/nuvem alta sincroniza normalmente e NUNCA mais dispara troca de estação/nevasca
- [ ] **Fika/multiplayer:** testar especificamente com convidado (Receiver) numa raid de Verão com nebulosidade/chuva alta — confirmar que a estação nunca muda
- [ ] **raid1 → exit → raid2:** sem estado vazado entre raids (N/A relevante aqui — `_stormCooldownRemaining` removido, não há mais estado de cooldown pra vazar)
- [ ] **alt-F4 / morte / MIA:** teardown idempotente, sem exceção no LogOutput.log
- [x] Memória do mod atualizada (`/update-memory` equivalente feito manualmente) com a lição do fix — ver `memory/sessions.md`

## Histórico

| Data | Evento |
|---|---|
| 2026-09-12 | Fix criado e aplicado — causa raiz investigada e confirmada linha por linha no Assembly (`Class444.cs`, `RainController.cs`), removendo `RollForStorm`/`ThunderEventTrigger`/`HandleReconnect(ESeasonStatus.Storm)`. Compilação local (`dotnet build`) validada, 0 erros/0 avisos. Recompilação via `/compile-mod` (com `--spt-path` inválido, técnica já estabelecida) e validação in-raid real ainda pendentes. |
