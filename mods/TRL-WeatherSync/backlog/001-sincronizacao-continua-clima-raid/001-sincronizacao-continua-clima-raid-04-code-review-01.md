# 001 — Sincronização Contínua de Clima em Raid · Code Review 01

**Mod:** TRL-WeatherSync
**Spec funcional:** [001-sincronizacao-continua-clima-raid-01-spec.md](001-sincronizacao-continua-clima-raid-01-spec.md)
**Spec técnica:** [001-sincronizacao-continua-clima-raid-02-spec-tech.md](001-sincronizacao-continua-clima-raid-02-spec-tech.md)
**Asbuild:** [001-sincronizacao-continua-clima-raid-05-asbuild.md](001-sincronizacao-continua-clima-raid-05-asbuild.md)
**Data:** 2026-09-10

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.
>
> **Memória consultada:** snapshot de 2026-09-10 (Sessão 2) · pendências que afetam: P-2.1 (fim de tempestade forçada, sem implementação — já documentado, não é achado novo) / nenhuma outra.
>
> **Compilação verificada:** o build atual (`mods/TRL-WeatherSync/builds/client/TRL-WeatherSync.dll`, 0 avisos/0 erros) compila com sucesso — os achados abaixo são de **lógica em runtime**, não de erro de compilação.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 3 · Total: 3

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | A | 🔴 | Descompasso de escala em `Rain`/`Wind` faz o clima recebido sempre aplicar ~zero chuva e vento | ✅ Aplicado em 2026-09-10 |
| CR-01-02 | C | 🟠 | Sincronização de tempestade não ativa nunca — `ThunderEventTrigger` é sempre `false` | ✅ Aplicado em 2026-09-10 |
| CR-01-03 | F | 🟢 | `WeatherClass.WindDirections[i].normalized` recalculado a cada chamada em vez de precomputado | ✅ Aplicado em 2026-09-10 |

## Categorias

- **A — Crítico** — bug grave, crash garantido, corrupção de estado, security issue.
- **B — Bug latente** — comportamento errado em cenário plausível, não acionado pelo caminho golden.
- **C — Gap vs. spec** — código não implementa critério de aceite, corner case, ou AC da spec.
- **D — Arquitetura** — viola padrões do repo, duplica código, leak de estado, abuso de reflection.
- **E — Legibilidade/manutenção** — nomes ruins, comentário "porquê" ausente, código morto, complexidade desnecessária.
- **F — Melhoria opcional** — refactor de qualidade, micro-otimização, simplificação.

## Impacto

- 🔴 **Bloqueador** — fix obrigatório antes de fechar o item.
- 🟠 **Forte** — fix recomendado; pode ser deferido para `06-fix-NN.md` futuro.
- 🟡 **Médio** — anotar, decidir caso a caso.
- 🟢 **Menor** — opcional.

---

## Pontos

### CR-01-01 · A — Crítico · 🔴 Bloqueador · ✅ Aplicado em 2026-09-10

**Descompasso de escala em `Rain`/`Wind`: o clima recebido pelo `Receiver` sempre acaba com ~zero de chuva e vento, independente do que o `Source` está transmitindo**

**Local:** [`mods/TRL-WeatherSync/modded/WeatherSyncSession.cs:106-111`](../../modded/WeatherSyncSession.cs#L106) (leitura no `Source`) e [`mods/TRL-WeatherSync/modded/WeatherSyncSession.cs:148-157`](../../modded/WeatherSyncSession.cs#L148) (aplicação no `Receiver`)

**Problema:** `BroadcastCurrentWeather()` lê `curve.Rain` e `curve.Wind.magnitude` diretamente de `IWeatherCurve` e manda no pacote sem conversão:

```csharp
Wind = curve.Wind.magnitude,
...
Rain = curve.Rain,
```

`ApplyReceivedWeather()` pega esses mesmos valores e joga direto num `WeatherClass`:

```csharp
Wind = packet.Wind,
...
Rain = packet.Rain,
```

Só que **os dois lados da conversão usam escalas diferentes**, confirmado lendo `EFT.Weather.WeatherCurve` (Assembly-CSharp) inteiro nesta revisão:

- `IWeatherCurve.Rain` ([`WeatherCurve.cs:48`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Weather/WeatherCurve.cs#L48)) = `Mathf.Clamp01(RainCurve.Evaluate(...))` — **já normalizado 0.0 a 1.0**. É esse valor que `curve.Rain` devolve no Host.
- Só que a curva é **construída** a partir de `WeatherClass.Rain` em [`WeatherCurve.cs:247`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Weather/WeatherCurve.cs#L247): `float num4 = Mathf.InverseLerp(1f, 5f, weatherClass.Rain);` — ou seja, `WeatherClass.Rain` (o campo que `SetWeatherForce`/`ApplyReceivedWeather` espera) precisa estar na **escala 1 a 5** (bate com o comentário do próprio servidor SPT, `WeatherData.cs`: *"1-3 light rain, 3+ 'rain'"*), não 0-1.
- O mesmíssimo padrão vale pra `Wind`: `WeatherClass.Wind` também passa por `Mathf.InverseLerp(1f, 5f, weatherClass.Wind)` em [`WeatherCurve.cs:248`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Weather/WeatherCurve.cs#L248), enquanto `curve.Wind.magnitude` (o que lemos no Host) já é o resultado pós-normalização.

Como `Mathf.InverseLerp` clampa o resultado em `[0,1]` e retorna `0` pra qualquer entrada `≤ 1`, mandar um `Rain`/`Wind` já normalizado (sempre em `[0,1]`) faz `InverseLerp(1, 5, valorEntre0e1)` avaliar **sempre para `0`** do lado de quem recebe — inclusive no caso extremo de `curve.Rain == 1.0` (`InverseLerp(1,5,1.0) = 0`, porque `1.0` é exatamente o início do range, não o fim).

**Por que importa:** Isso derruba a funcionalidade central do item — chuva e vento **nunca** sincronizam de verdade entre os jogadores; todo `Receiver` (inclusive o próprio `Source`, que também aplica via eco/self-apply) acaba sempre recebendo chuva e vento em ~zero, não importa o clima real da raid. É o critério de aceite #1/#2 da spec funcional (chuva/vento/neblina consistentes entre jogadores) falhando silenciosamente — sem exceção, sem log de erro, o pacote "funciona" (chega, é aplicado), só que com o valor errado.

**Sugestão:** Converter de volta pra escala `[1,5]` antes de montar o pacote, revertendo o `InverseLerp` com um `Lerp` (matematicamente é a inversa exata da mesma fórmula, então o round-trip fecha). Em `WeatherSyncSession.BroadcastCurrentWeather()` (linha ~107-109):

```csharp
Wind = Mathf.Lerp(1f, 5f, curve.Wind.magnitude),
...
Rain = Mathf.Lerp(1f, 5f, curve.Rain),
```

Vale testar em raid real depois do fix — a fórmula bate na leitura do Assembly, mas nenhum teste in-game foi feito neste ciclo (ver checklist §8 da spec técnica, ainda pendente).

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada conforme proposto.
**Aplicação:** `mods/TRL-WeatherSync/modded/WeatherSyncSession.cs:106-113` — `Wind`/`Rain` agora passam por `Mathf.Lerp(1f, 5f, ...)` antes de entrar no `TrlWeatherSyncPacket`, revertendo a normalização de `IWeatherCurve` de volta pra escala `[1,5]` que `WeatherClass`/`WeatherCurve.method_4()` espera. `NearestWindDirectionIndex(curve.Wind)` não mudou (usa só a direção normalizada, não a magnitude). Comentário `// ref: CR-01-01` adicionado no bloco. Teste in-game da correção ainda pendente (fora do escopo deste comando).

---

### CR-01-02 · C — Gap vs. spec · 🟠 Forte · ✅ Aplicado em 2026-09-10

**Sincronização de tempestade nunca ativa — `ThunderEventTrigger` é hardcoded `false`, então os critérios de aceite #3/#4 da spec funcional (tempestade sincronizada) não são atendidos por este build**

**Local:** [`mods/TRL-WeatherSync/modded/WeatherSyncSession.cs:112`](../../modded/WeatherSyncSession.cs#L112)

```csharp
ThunderEventTrigger = false, // política de decisão de tempestade fica fora do escopo deste item.
```

**Problema:** A spec funcional (`001-...-01-spec.md`) lista como critério de aceite: *"Quando uma tempestade começa durante a raid, todos os jogadores da mesma raid entram na tempestade dentro de poucos segundos entre si"* e o critério simétrico para o fim da tempestade. O transporte pro evento de tempestade existe (`TrlWeatherSyncPacket.ThunderEventTrigger`, `ApplyReceivedWeather` já sabe reagir a ele), mas **nada no código decide quando `ThunderEventTrigger` deve virar `true`** — está permanentemente fixo em `false`. Isso já estava documentado como decisão consciente em §7 da spec técnica ("Política de decisão de tempestade fora de escopo") e no as-built como limitação conhecida — não é uma descoberta nova, mas cabe registrar formalmente aqui porque é um Gap vs. um critério de aceite explícito da spec funcional, não só um detalhe técnico.

**Por que importa:** Do jeito que está, a metade "tempestade sincronizada" do item simplesmente não existe em runtime — só a parte de clima contínuo (chuva/vento/neblina/nuvem/temperatura) funciona (uma vez que CR-01-01 for corrigido). Se alguém testar o mod achando que tempestades vão vir sincronizadas, vai encontrar um mod que nunca dispara tempestade nenhuma via rede.

**Sugestão:** Duas rotas, nenhuma delas precisa ser feita agora:
1. Aceitar como dívida explícita — o item fecha como "sincronização de clima contínuo" funcionando, e abre um novo item de backlog dedicado só à política de decisão de tempestade (ex.: Host decide via `IWeatherCurve.LightningThunderProbability` + um cooldown, ou um controle manual no F12).
2. Se preferir fechar tudo de uma vez, implementar uma primeira versão simples agora: no Host, sortear contra `curve.LightningThunderProbability` a cada ciclo de broadcast (com um cooldown mínimo pra não disparar toda hora) e setar `ThunderEventTrigger = true` por um ciclo quando o sorteio "ganhar".

Ambas as rotas são aceitáveis — a decisão é do usuário, não uma correção técnica única.

**Decisão:**
- `[x]` Aceitar sugestão (rota 2 — implementar versão simples agora, só o início)

**Resolução:** Implementado `WeatherSyncSession.RollForStorm(IWeatherCurve)` — sorteia contra `curve.LightningThunderProbability` a cada ciclo de broadcast, com cooldown (`ConfigEntry` nova `Storm Check Cooldown Seconds`, default 300s, faixa 60-1800s). Decisão explícita do usuário (confirmada em sessão): **só o início é sincronizado — o fim continua não sendo forçado**, cada jogador sai da tempestade pelo tempo nativo do próprio jogo. Isso evita depender da pendência P-2.1 (ainda não resolvida — reverter os dois state machines de tempestade, `Class444` e `RainController`, com segurança) para entregar pelo menos metade do critério de aceite da spec funcional.
**Aplicação:** `mods/TRL-WeatherSync/modded/WeatherSyncSession.cs` (`RollForStorm`, campo `_stormCooldownRemaining`, `ThunderEventTrigger = RollForStorm(curve)`) + `mods/TRL-WeatherSync/modded/Plugin.cs` (`ConfigEntry<float> StormCheckCooldownSeconds`, seção "Storm").

---

### CR-01-03 · F — Melhoria opcional · 🟢 Menor · ✅ Aplicado em 2026-09-10

**`WeatherClass.WindDirections[i].normalized` recalculado a cada chamada de `NearestWindDirectionIndex`, quando a tabela é fixa**

**Local:** [`mods/TRL-WeatherSync/modded/WeatherSyncSession.cs:131-138`](../../modded/WeatherSyncSession.cs#L131)

```csharp
for (var i = 0; i < WeatherClass.WindDirections.Length; i++)
{
    var dot = Vector2.Dot(normalized, WeatherClass.WindDirections[i].normalized);
    ...
```

**Problema:** `WeatherClass.WindDirections` é `public static readonly Vector2[]` — não muda em runtime — mas `.normalized` dos 9 vetores é recalculado toda vez que `NearestWindDirectionIndex` roda.

**Por que importa:** Overhead irrelevante na prática (só roda uma vez a cada `SyncIntervalSeconds`, não por frame — não é hot path), mas é trabalho redundante fácil de eliminar.

**Sugestão:** Precomputar um array estático `private static readonly Vector2[] _normalizedWindDirections = WeatherClass.WindDirections.Select(v => v.normalized).ToArray();` (ou um loop no static constructor) uma única vez, e usar esse array no lugar de `WeatherClass.WindDirections[i].normalized` dentro do loop. Puramente cosmético — não bloqueia nada.

**Decisão:**
- `[x]` Aceitar com modificação: loop explícito num método `BuildNormalizedWindDirections()` chamado pela inicialização do campo estático, em vez de LINQ (`Select`/`ToArray`) — consistente com a convenção do repo de evitar LINQ (mesmo fora de hot path, por padrão de estilo do `csharp-mod-best-practices`).

**Resolução:** `NormalizedWindDirections` (array estático precomputado) + `BuildNormalizedWindDirections()` adicionados a `WeatherSyncSession.cs`; `NearestWindDirectionIndex` agora usa o array precomputado em vez de `WeatherClass.WindDirections[i].normalized`.
**Aplicação:** `mods/TRL-WeatherSync/modded/WeatherSyncSession.cs`.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-10 | Code review 01 criada via `/code-review` |
| 2026-09-10 | Aplicação automática de 1 achado via `/apply-code-review` — IDs aplicados: CR-01-01. CR-01-02 e CR-01-03 seguem pendentes (não decididos pelo usuário nesta rodada). |
| 2026-09-10 | Aplicação de 2 achados via `/apply-code-review` (rodada manual, decisão do usuário em conversa) — IDs aplicados: CR-01-02 (política simples de início de tempestade, `RollForStorm`), CR-01-03 (precompute de `NormalizedWindDirections`). Rodada 01 fechada — 3/3 achados aplicados, 0 pendentes. |
