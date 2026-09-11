# 001 — Sincronização Contínua de Clima em Raid · Review Técnica 01

**Mod:** TRL-WeatherSync
**Spec técnica revisada:** [001-sincronizacao-continua-clima-raid-02-spec-tech.md](001-sincronizacao-continua-clima-raid-02-spec-tech.md)
**Data:** 2026-09-10

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM` (review 01, ponto MM). Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 6 · Total: 6

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C | 🔴 | Patch em `BaseLocalGame<T>.Stop` nunca dispara em raid FIKA real (`CoopGame` override não chama base) | ✅ Resolvido em 2026-09-10 |
| PA-01-02 | C | 🔴 | Instalação assimétrica do mod entre Host/Client derruba a fila de eventos de rede de toda a raid | ✅ Resolvido em 2026-09-10 |
| PA-01-03 | A | 🟡 | Postfixes de lifecycle sem `try/catch` | ✅ Resolvido em 2026-09-10 |
| PA-01-04 | A | 🟡 | `AtmospherePressure` no pacote não é consumido pela curva de clima que o mod pretende sincronizar | ✅ Resolvido em 2026-09-10 |
| PA-01-05 | A | 🟢 | Conversão `Vector2` (curva) → índice `WindDirection` sem algoritmo, só `TODO confirmar` | ✅ Resolvido em 2026-09-10 |
| PA-01-06 | B | 🟢 | Corner case de drift de relógio (spec funcional) não citado explicitamente no fluxo de dados | ✅ Resolvido em 2026-09-10 |

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

### PA-01-01 · C — Erro de Lógica · 🔴 Bloqueador · ✅ Resolvido em 2026-09-10

**Patch em `BaseLocalGame<T>.Stop` nunca dispara em raid FIKA real — `CoopGame` (a classe de jogo real do FIKA) sobrescreve `Stop` sem chamar `base.Stop(...)`**

**Problema:** A spec técnica (§1, §2, §5.3) propõe um `Postfix` em `BaseLocalGame<T>.Stop` ([`BaseLocalGame-1.cs:1018`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/BaseLocalGame-1.cs#L1018), confirmado `public virtual void Stop(...)`) como um dos 3 hooks de fim de sessão, e a §9 marca o check 3 (AP-03 — auditoria de overrides de alvo virtual) como **N/A** com a justificativa "nenhum alvo virtual/ofuscado é patcheado". Essa justificativa está **errada**: `Stop` é `virtual`, e a classe de jogo real usada em toda raid FIKA — `Fika.Core.Main.GameMode.CoopGame : BaseLocalGame<EftGamePlayerOwner>` ([`mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/GameMode/CoopGame.cs:42`](../../../../mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/GameMode/CoopGame.cs#L42)) — **sobrescreve** `Stop` em [`CoopGame.cs:718`](../../../../mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/GameMode/CoopGame.cs#L718) e **nunca chama `base.Stop(...)`** (confirmado por grep no arquivo inteiro — zero ocorrências de `base.Stop`). O override faz sua própria sequência de teardown (dispatch de `FikaGameEndedEvent`, reset de dados de transição, remoção de dogtag, etc.) sem delegar à base.

**Por que importa:** É exatamente o padrão do AP-03 já documentado neste repo (`docs/technical/spt-antipatterns.md` AP-03, caso real do F4 no stances — 13 de 14 overrides não chamavam base). Harmony intercepta o IL do método que efetivamente roda; como `CoopGame` é a classe usada em **toda** raid FIKA (o público-alvo inteiro deste mod, que é FIKA-only), o patch em `BaseLocalGame<T>.Stop` **nunca vai disparar** em nenhuma raid real. Na prática, `WeatherSyncSession.End()` só seria chamado pelo patch de `GameWorld.OnDestroy` (esse sim confirmado seguro — `ClientGameWorld.OnDestroy()` chama `base.OnDestroy()` em [`ClientGameWorld.cs:222`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/ClientGameWorld.cs#L222)), o que reduz a robustez do "duplo hook" que o §2/§9 check 1 e check 5 afirmam ter — a spec fica com só 1 hook de saída funcional em vez de 2, sem que ninguém tivesse percebido.

**Sugestão:** Trocar o alvo do terceiro patch de `BaseLocalGame<T>.Stop` (`BaseLocalGame-1.cs:1018`) para `CoopGame.Stop` (`mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/GameMode/CoopGame.cs:718`) diretamente — já que o mod é FIKA-only por natureza (guard `Singleton<IFikaNetworkManager>.Instantiated` já existe em toda a cadeia), não há motivo para mirar a base genérica. Atualizar §1, §2 e §5.3 (o `GetTargetMethod()` fica `AccessTools.Method(typeof(CoopGame), "Stop")`, sem o `TODO confirmar` genérico atual — o tipo fechado real já está confirmado). Atualizar §9 check 3 de N/A para ✅ com esta evidência (é, sim, um alvo virtual, e agora está auditado corretamente).

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Alvo do 3º patch trocado para `CoopGame.Stop` (`mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/GameMode/CoopGame.cs:718`) em `001-...-02-spec-tech.md` §1, §2, §5.3, §9 check 3. `GameWorld.OnDestroy`/`OnGameStarted` confirmados seguros (sem override problemático na cadeia FIKA).

---

### PA-01-02 · C — Erro de Lógica (contradiz doc canônico) · 🔴 Bloqueador · ✅ Resolvido em 2026-09-10

**Instalação assimétrica do mod (Host com TRL-WeatherSync, Cliente sem — ou vice-versa) derruba a fila de eventos de rede de TODA a raid, não só do clima**

**Problema:** A spec funcional (`001-...-01-spec.md`, corner case "Host tem o mod instalado mas algum Cliente não") exige explicitamente definir o comportamento desse cenário. A spec técnica não aborda isso em nenhuma seção — §7 "Riscos e dependências" fala de dependência de FIKA instalado, mas não do caso em que só PARTE dos peers tem o mod TRL-WeatherSync. O doc canônico obrigatório para este item ([`fika-packet-desync-prevention-plan.md` §2](../../../../docs/technical/fika-packet-desync-prevention-plan.md#2-causas-raiz-de-desincronização--parseexception), causa 1/2/4) é explícito: um pacote com hash sem handler registrado **lança `ParseException`** em [`NetPacketProcessor.cs:88`](../../../../mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/LiteNetLib/Utils/NetPacketProcessor.cs#L88) (confirmado nesta revisão — linha bate exatamente), e essa exceção sobe sem proteção até `LiteNetManager.PollEvents` ([`LiteNetManager.cs:1439`](../../../../mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/LiteNetLib/LiteNetManager.cs#L1439), confirmado nesta revisão), derrubando **todos os eventos pendentes daquele frame — de todos os peers, inclusive `PlayerState`** (posição/movimento). Também confirmado nesta revisão: `mods/FIKA/modded/Fika-Plugin/Fika.Core` **não tem nenhum mecanismo de validação de lista de mods entre peers** (grep por `ModList`/`RequiredMods`/`PluginList` não achou nada) — não existe rede de segurança do próprio FIKA para esse cenário.

**Por que importa:** Como o Host (se tiver o mod) transmite `TrlWeatherSyncPacket` em broadcast a cada `SyncIntervalSeconds` (§5.4), o primeiro pacote enviado a um Cliente **sem o mod instalado** vai gerar `ParseException` no Cliente e travar/pinar o movimento de **todos os jogadores daquela raid** por um frame, repetidamente, a cada ciclo de envio — não é um bug cosmético do clima, é o mesmo sintoma de "jogadores patinando" que o doc canônico usa como exemplo motivador. Isso é praticamente garantido de acontecer na primeira vez que alguém jogar com um amigo que ainda não instalou o mod, e não deixa rastro óbvio de causa (o log de erro aparece na máquina do Cliente sem o mod, não na do Host).

**Sugestão:** Adicionar uma seção explícita em §1 ou §7 definindo a política. Duas rotas possíveis, escolher uma:
1. **Handshake de capacidade:** antes do primeiro broadcast, o Host confirma (via um pacote de "ping"/capacidade próprio, ou reaproveitando algum sinal já existente do FIKA) que cada peer tem o handler registrado; só broadcasta pra quem confirmou. Mais robusto, mais trabalho.
2. **Aceitar como restrição documentada:** declarar que TRL-WeatherSync exige instalação em **todos** os peers da raid (like um mod "hard-required" em coop), documentar isso com destaque no `README.md`/tooltip do F12, e mitigar o pior efeito com uma janela de atraso no primeiro broadcast (ex: só começar a enviar 5-10s depois do raid iniciar, dando tempo de qualquer erro de registro tardio (causa 1 do doc canônico) se resolver) — não elimina o risco de instalação assimétrica, só reduz o de timing. Mais simples, mas exige aviso claro ao usuário.

Registrar a decisão nesta spec (§1/§7) antes de prosseguir — este ponto bloqueia porque muda o desenho do handler de recepção e possivelmente exige um pacote adicional.

**Decisão:**
- `[x]` Caminho alternativo: usuário escolheu a rota 2 (aceitar como restrição documentada) — mod exigido em todos os peers da raid, sem handshake de capacidade.

**Resolução:** Documentado em `001-...-02-spec-tech.md` §1 (nova subseção "Pré-requisito de instalação") e §7. Checklist §8 ganhou item explícito para documentar isso em `README.md`/tooltip F12. A mitigação de timing (atraso no primeiro broadcast) não foi adotada — fica como possível melhoria futura, não bloqueia.

---

### PA-01-03 · A — Gap · 🟡 Importante · ✅ Resolvido em 2026-09-10

**Postfixes de lifecycle (§5.3) sem `try/catch`, violando checklist explícito das skills carregadas**

**Problema:** Os 3 `Postfix` em `RaidLifecyclePatches.cs` (§5.3) chamam `WeatherSyncSession.Begin()`/`End()` diretamente, sem nenhum `try/catch`. A skill `spt-mod-best-practices` §5 e o checklist §"Review checklist" item 5 exigem "Bodies wrapped in try/catch and logged" para todo Harmony patch; `csharp-mod-best-practices` checklist item 6 pede o mesmo. Comparar com `WeatherSyncNetworkHandler.cs` (§5.2), que segue a regra corretamente em todos os seus métodos.

**Por que importa:** Se `WeatherSyncSession.Begin()` lançar (ex.: `gameWorld.MainPlayer` nulo num frame de transição, ou falha ao instanciar o `GameObject`), a exceção sobe sem proteção a partir de um Postfix do Harmony — na melhor hipótese só polui o log; na pior, dependendo de outros Postfixes registrados no mesmo método por outros mods, pode interromper a cadeia de patches daquele método (comportamento de Harmony com múltiplos patches no mesmo alvo não é garantidamente isolado por exceção).

**Sugestão:** Envolver o corpo de cada um dos 3 `Postfix` em `try { ... } catch (Exception ex) { Log.LogError($"[TRL-WeatherSync] ..."); }`, seguindo o mesmo padrão já usado em `WeatherSyncNetworkHandler`. Ajuste pequeno, direto — atualizar o stub §5.3.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Os 3 `Postfix` em `001-...-02-spec-tech.md` §5.3 agora têm `try/catch` com log individual. `WeatherSyncSession.Update()` (§5.4) também ganhou `try/catch` ao redor de `BroadcastCurrentWeather()` por extensão do mesmo princípio (achado relacionado ao risco novo de Headless, ver §7).

---

### PA-01-04 · A — Gap · 🟡 Importante · ✅ Resolvido em 2026-09-10

**`AtmospherePressure` incluído no pacote, mas não é lido pela curva de clima que o mod sincroniza**

**Problema:** O stub §5.1/§5.4 inclui `AtmospherePressure` no `TrlWeatherSyncPacket` e no `WeatherClass` alvo montado em `ApplyReceivedWeather`. Porém `IWeatherCurve` (a interface que `WeatherController.WeatherCurve` expõe, [`IWeatherCurve.cs`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Weather/IWeatherCurve.cs)) **não tem propriedade de pressão** — só `Wind`, `TopWind`, `Rain`, `Cloudiness`, `Fog`, `Temperature`, `LightningThunderProbability`, `CurrentNormalizedTimeValue`. A própria investigação registrada em `docs/investigacao-fika-eft-2026-09-10.md` (seção G, sobre `RainRandomness`) já estabeleceu o mesmo padrão para outro campo de `WeatherClass`: campos que existem na struct de transporte mas não são lidos por `WeatherCurve.method_4()` (o método que constrói as curvas interpoladas) não têm efeito visual algum quando sincronizados via `SetWeatherForce`.

**Por que importa:** O stub em §5.4 hoje só preenche `AtmospherePressure` com um valor fixo `760f` no lado do Host (com `TODO confirmar` já anotado) — ou seja, mesmo sem o problema de "não é lido pela curva", o campo já nasce sempre igual, nunca refletindo o clima real. Isso é inconsistente com a própria evidência que este mesmo item de pesquisa já levantou sobre `RainRandomness`, e há risco de o implementador gastar tempo tentando "consertar" a origem do valor de pressão sem perceber que, mesmo corrigido, não teria efeito visível.

**Sugestão:** Remover `AtmospherePressure` do `TrlWeatherSyncPacket` e do `WeatherClass` alvo, a menos que se confirme (fora desta spec, por leitura adicional de quem consome `WeatherClass.AtmospherePressure` além de `WeatherCurve`) que ele afeta algo relevante — nesse caso, documentar a fonte real do valor em vez de um placeholder fixo. Reduz o tamanho do pacote e remove um `TODO confirmar` inteiro.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Campo removido do `struct`, `Serialize`, `Deserialize`, montagem do pacote no Host e montagem do `WeatherClass` alvo no Cliente (`001-...-02-spec-tech.md` §5.1/§5.4).

---

### PA-01-05 · A — Gap · 🟢 Menor · ✅ Resolvido em 2026-09-10

**Conversão `Vector2` (curva) → índice `WindDirection` sem algoritmo definido**

**Problema:** §5.4 marca `WindDirection = 0` com um `TODO confirmar` apontando que `IWeatherCurve.Wind` é `Vector2` (direção contínua) enquanto `WeatherClass.WindDirection` é um índice `int` numa tabela fixa de 9 vetores (`WeatherClass.WindDirections[]`, [`WeatherClass.cs:9-20`](../../../../references/eft-decompiled/Assembly-CSharp/WeatherClass.cs#L9)). A spec já identifica corretamente o problema, mas não propõe nenhum algoritmo (nem aproximado) — o implementador começa do zero.

**Por que importa:** Sem direção de vento correta, o campo fica sempre em `WindDirection = 0` (`new Vector2(0.1f, 0.1f)`, quase nulo) mesmo que o Host tenha vento forte em outra direção — degrada silenciosamente a qualidade da sincronização de vento, sem quebrar nada (é um item 🟢, não 🔴/🟡).

**Sugestão:** Adicionar ao §5.4 um esboço de conversão: iterar `WeatherClass.WindDirections` calculando o produto escalar (`Vector2.Dot`) normalizado contra `curve.Wind.normalized`, escolher o índice de maior produto escalar (direção mais próxima). Não precisa ser exato nesta spec — só dar um ponto de partida concreto em vez de um `TODO` vazio.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** `NearestWindDirectionIndex(Vector2)` adicionado a `001-...-02-spec-tech.md` §5.4, exatamente com o algoritmo sugerido (maior `Vector2.Dot` contra `WeatherClass.WindDirections[]`, com guard para vento ~zero retornando índice 0).

---

### PA-01-06 · B — Edge Case · 🟢 Menor · ✅ Resolvido em 2026-09-10

**Corner case de drift de relógio Host/Cliente (spec funcional) não citado explicitamente no fluxo de dados da spec técnica**

**Problema:** A spec funcional lista como corner case: *"Ao longo de uma raid longa, pequenas diferenças de relógio interno entre Host e Cliente não devem causar divergência perceptível de clima"*. A spec técnica cobre isso **implicitamente** (o broadcast periódico de `WeatherSyncSession.Update()`, §5.4, naturalmente re-corrige qualquer drift a cada ciclo), mas não menciona esse corner case em nenhum lugar — nem em §6 (Fluxo de dados) nem em §7 (Riscos).

**Por que importa:** Não é um erro funcional (o mecanismo já cobre o caso), mas um revisor ou implementador futuro lendo só a spec técnica não tem como saber que esse corner case foi considerado e resolvido — parece uma lacuna quando na verdade é coberto por design.

**Sugestão:** Adicionar uma frase em §7 confirmando explicitamente: "O broadcast periódico (§5.1/§5.4) também é o mecanismo de correção de drift de relógio entre Host e Cliente — cada ciclo reaplica os valores atuais do Host via `SetWeatherForce`, o que limita qualquer divergência acumulada à janela de `SyncIntervalSeconds`." Cosmético, não bloqueia.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Frase adicionada a `001-...-02-spec-tech.md` §7 ("Drift de relógio Host/Cliente... já coberto por design").
