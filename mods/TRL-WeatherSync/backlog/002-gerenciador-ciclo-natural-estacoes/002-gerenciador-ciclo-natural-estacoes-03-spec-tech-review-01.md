# 002 — Gerenciador Ciclo Natural Estações · Review Técnica 01

**Mod:** TRL-WeatherSync
**Spec técnica revisada:** [002-gerenciador-ciclo-natural-estacoes-02-spec-tech.md](002-gerenciador-ciclo-natural-estacoes-02-spec-tech.md)
**Data:** 2026-09-10

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM` (review 01, ponto MM). Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 4 · ⏭️ Rejeitados: 1 · Total: 5

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Erro de Lógica | 🔴 Bloqueador | `OnLoad` sem try/catch pode derrubar o boot do servidor inteiro | ✅ Resolvido |
| PA-01-02 | A — Gap | 🟡 Importante | Config só é lida uma vez no boot — contradiz "aplica na próxima raid" | ✅ Resolvido (caminho alternativo) |
| PA-01-03 | B — Edge Case | 🟡 Importante | Corner case "relógio alterado" só parcialmente resolvido | ⏭️ Rejeitado |
| PA-01-04 | A — Gap | 🟢 Menor | Corner case "estação fixa trava pesos?" nunca respondido no texto | ✅ Resolvido (com modificação) |
| PA-01-05 | C — Erro de Lógica | 🟢 Menor | `Enum.TryParse<Season>` case-sensitive — typo silencioso no `.json` | ✅ Resolvido |

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

**`OnLoad` sem try/catch pode derrubar o boot do servidor inteiro (não só deste mod)**

**Problema:** [`App.cs:68-71`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Utils/App.cs#L68) (`foreach (var onLoad in onLoadComponents) { await onLoad.OnLoad(); }`) e [`SptServerStartupService.cs:25`](../../../../references/spt-source/SPTarkov.Server/Services/SptServerStartupService.cs#L25) (`await app.InitializeAsync();`) **não têm try/catch** — contraste direto com `Update()` ([`App.cs:107-117`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Utils/App.cs#L107)), que embrulha cada `IOnUpdate.OnUpdate()` individualmente e só loga o erro sem propagar. O stub de `SeasonCycleUpdater.OnLoad()` (§5 da spec técnica) chama `Apply()`, que desreferencia `configController.Config` sem checar null; e `SeasonCycleConfigController.OnLoad()` (§5) chama `fileUtil.ReadFileAsync(path)` sem tratar arquivo ausente/malformado. Além disso, a spec **assume sem confirmar** que `TypePriority`/`OnLoadOrder` garante que `SeasonCycleConfigController` roda antes de `SeasonCycleUpdater` — `App.cs:68` não ordena `onLoadComponents` por prioridade nenhuma visível no código vendorizado deste repo; a ordenação real (se existir) acontece dentro do container `SPTarkov.DI`, que **não está vendorizado** em `references/spt-source/` (é um pacote NuGet externo) — não dá pra confirmar o comportamento a partir do que este repo tem em disco.

**Por que importa:** uma exceção não tratada em **qualquer** `IOnLoad.OnLoad()` de **qualquer** mod (incluindo o nosso) propaga por essa cadeia sem nenhuma barreira e pode abortar o boot do servidor inteiro — todos os mods, todos os jogadores, não é um bug isolado deste item. Se a suposição de ordenação estiver errada (não dá pra provar que está certa com o código disponível), `SeasonCycleUpdater.OnLoad()` lançaria `NullReferenceException` em `configController.Config` logo no primeiro boot depois de instalado.

**Sugestão:** embrulhar o corpo de `Apply()` e o corpo de `SeasonCycleConfigController.OnLoad()` em `try/catch` com log de erro (mesmo princípio "airbag" que a skill `spt-mod-best-practices` §5 exige pra patches Harmony, adaptado pro lado servidor). Em `Apply()`, checar `configController.Config == null` no topo e retornar cedo com log de warning — o próximo `OnUpdate()` (~5s depois, `App.cs:92-122`) tenta de novo, então a auto-cura é gratuita. Adicionar uma frase em §7 (Riscos) documentando explicitamente que a ordem de `IOnLoad` entre componentes/mods diferentes **não é garantida pelo código vendorizado deste repo**, então o design não pode depender dela sem essa rede de segurança.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada conforme proposto. `SeasonCycleConfigController.OnLoad()` e `SeasonCycleUpdater.Apply()` embrulhados em `try/catch` com log de erro; `Apply()` checa `configController.Config == null` no topo e retorna cedo com log de warning (auto-cura no próximo tick). §7 (Riscos) ganhou duas entradas novas documentando explicitamente a falta de garantia de ordem entre `IOnLoad` e a ausência de try/catch no harness nativo (`App.cs:68-71`, `SptServerStartupService.cs:25`).
**Aplicação:** `002-...-02-spec-tech.md` §5 (stubs `SeasonCycleConfigController.OnLoad()` e `SeasonCycleUpdater.Apply()`), §7 (2 novos itens de risco).

---

### PA-01-02 · A — Gap · 🟡 Importante · ✅ Resolvido em 2026-09-10

**Config só é lida uma vez no boot — contradiz "aplica na próxima raid" da spec funcional**

**Problema:** `SeasonCycleConfigController` implementa só `IOnLoad` (§5 da spec técnica) — `season-cycle.json` é lido **uma única vez**, no boot do processo do servidor SPT. A spec funcional ([`002-...-01-spec.md`](002-gerenciador-ciclo-natural-estacoes-01-spec.md), corner case "Modo 'estação fixa' ativado durante uma raid em andamento") diz: *"Só na próxima raid, mas isso vai ser definido via server"* — o que implica que editar o `.json` e simplesmente entrar numa raid nova (sem reiniciar o processo do servidor) já deveria bastar pra pegar a mudança. Com o design atual isso só funciona se o servidor SPT for **reiniciado** entre as raids — o que não é garantido (uma sessão de jogo longa pode rodar várias raids seguidas sem reiniciar o servidor local de cada peer FIKA).

**Por que importa:** o usuário edita `season-cycle.json` esperando que a próxima raid já reflita a mudança (conforme a spec funcional promete), mas continua vendo o valor antigo até reiniciar o SPT — comportamento observável errado, sem nenhum log ou aviso que explique o motivo.

**Sugestão:** mover a releitura do `season-cycle.json` pra dentro do ciclo de `SeasonCycleUpdater.OnUpdate()` (a cada ~5s, `App.cs:92-122`) em vez de só no `OnLoad` — por exemplo, `SeasonCycleConfigController` também implementa `IOnUpdate` e recarrega o arquivo a cada tick (opcionalmente comparando `File.GetLastWriteTimeUtc` pra evitar I/O redundante todo tick). Isso faz a config funcionar como hot-reload de fato, batendo com a expectativa literal da spec funcional.

**Decisão:**
- `[x]` Caminho alternativo: usuário esclareceu o mal-entendido — a expectativa de "aplica na próxima raid" era sobre a **progressão da estação já configurada** (que já funciona sem reload, recalculada a cada `OnUpdate` a partir do tempo real), não sobre **editar o `.json`**. O usuário reinicia o SPT Server manualmente após editar `.json`, mesmo fluxo já usado pros outros mods do repo — hot-reload não é necessário.

**Resolução:** Sem mudança de código. `SeasonCycleConfigController` continua só `IOnLoad`. Adicionada nota em §5 (bloco de "Reload") e §4 (linha do `README.md`) deixando explícito que edição do `.json` exige reiniciar o SPT Server, e que isso é distinto da progressão automática da estação (que não depende de reload).
**Aplicação:** `002-...-02-spec-tech.md` §5 (nota de Reload após o stub do `SeasonCycleConfigController`), §4 (linha `README.md`).

---

### PA-01-03 · B — Edge Case · 🟡 Importante · ⏭️ Rejeitado em 2026-09-10

**Corner case "relógio do jogador alterado" só parcialmente resolvido**

**Problema:** o stub de `ResolveSeasonFromCycle` (§5) usa `DateTimeOffset.UtcNow.ToUnixTimeSeconds()` como "agora" — isso lê o relógio do sistema operacional da máquina que roda o SPT local. O `referenceEpochUtc` fixo no `.json` resolve o problema de "de onde a contagem começa" (não depende de quando o mod foi instalado nem de estado salvo), mas **não protege contra o jogador adiantar/atrasar o relógio do Windows em tempo real** pra forçar uma estação diferente — a spec funcional lista esse corner case explicitamente, e a spec técnica cita a mesma frase da spec funcional (§1) sem deixar claro que só metade do problema foi endereçada.

**Por que importa:** um leitor da spec técnica pode concluir que o corner case está 100% fechado (o termo `referenceEpochUtc` é citado como a resposta), quando na prática a manipulação ao vivo do relógio continua possível — é uma limitação estrutural (o SPT local não tem fonte de tempo externa confiável pra cross-checar; o próprio vanilla usa `timeUtil.GetDateTimeNow()` sem proteção nenhuma, [`SeasonalEventService.cs:314`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/SeasonalEventService.cs#L314)), não um bug deste item — mas isso precisa estar escrito, não implícito.

**Sugestão:** adicionar uma frase em §1 ou §7 deixando explícito que `referenceEpochUtc` resolve o problema do "ponto de partida" da contagem, mas que manipulação ao vivo do relógio do SO continua possível (mesma limitação do vanilla, não uma regressão) — documentar como limite conhecido, não deixar como lacuna silenciosa.

**Decisão:**
- `[x]` Rejeitar: usuário decidiu que manipulação de relógio está fora de escopo — não é um cenário esperado, e se acontecer o resultado incorreto é aceitável ("se acontecer ele que se dane, não mandei alterar o relógio").

**Resolução:** Rejeitado — usuário não considera esse risco relevante o suficiente pra documentar como limitação formal. Mesmo assim, uma nota curta foi mantida em §7 (Riscos) registrando a decisão explícita (não como lacuna, como decisão consciente rastreável), para não perder o contexto caso o tema volte no futuro.
**Aplicação:** `002-...-02-spec-tech.md` §7 (1 linha registrando a decisão do usuário, sem alteração de comportamento).

---

### PA-01-04 · A — Gap · 🟢 Menor · ✅ Resolvido (com modificação) em 2026-09-10

**Corner case "estação fixa trava também os pesos de clima?" nunca é respondido explicitamente no texto**

**Problema:** a spec funcional lista esse corner case sem marcador `<!-- review -->` (ou seja, esperava resposta técnica, não decisão do usuário). O design atual (`Apply()`, §5) já resolve isso "de graça" — `GetWeatherPresetWeightsBySeason` sempre busca pesos pela `Season` ativa no momento ([`WeatherGenerator.cs:77-82`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Generators/WeatherGenerator.cs#L77)), então travar em Inverno automaticamente trava os pesos de Inverno também — mas nenhuma seção da spec técnica afirma isso explicitamente.

**Por que importa:** quem ler a spec técnica sem re-derivar a lógica sozinho não consegue confirmar que esse corner case foi endereçado — parece um buraco na spec, mesmo não sendo um buraco no design.

**Sugestão:** adicionar uma frase curta em §5 (perto do stub de `Apply()`) ou §6 confirmando: "quando `fixedSeason` está setado, os pesos de clima usados são automaticamente os da estação travada, porque `GetWeatherPresetWeightsBySeason` sempre resolve pela `Season` ativa no momento — não existe caminho separado onde o peso varia independente da estação travada."

**Decisão:**
- `[x]` Aceitar com modificação: o usuário corrigiu a premissa da sugestão — o comportamento **desejado** é o oposto do que o design (não documentado) produzia. "Estação fixa" deve travar só a estética/sub-fase nativa; a chance de sol/chuva/neve **não** pode travar — precisa continuar variando ao longo do ano mesmo com a estação fixa ativa.

**Resolução:** Interpretação adotada — `Apply()` foi redesenhado (não é só documentação): `ResolveSeasonFromCycle()` (a estação cíclica "natural") agora é **sempre** calculada, mesmo com `fixedSeason` ativo. `OverrideSeason` usa a estação fixa (trava a estética). Mas a entrada do dicionário `WeatherPresetWeight` correspondente à estação fixa é **sobrescrita a cada tick** com os pesos da estação cíclica corrente — assim `GetWeatherPresetWeightsBySeason` (que sempre busca pela chave da estação fixa) lê pesos que continuam mudando ao longo do ano, mesmo a estética ficando parada.
**Aplicação:** `002-...-02-spec-tech.md` §5 (`Apply()` reescrito: `cyclicSeasonName`/`activeSeasonName` separados, bloco final de sobrescrita condicional a `cfg.FixedSeason != null`).

---

### PA-01-05 · C — Erro de Lógica · 🟢 Menor · ✅ Resolvido em 2026-09-10

**`Enum.TryParse<Season>` é case-sensitive por padrão — typo silencioso no `.json`**

**Problema:** no stub de `Apply()` (§5), `Enum.TryParse<Season>(seasonName, out var season)` não passa `ignoreCase: true`. Um editor humano do `season-cycle.json` que digitar `"winter"` em vez de `"WINTER"` faz o parse falhar silenciosamente — cai no branch de warning e log, sem crashar, mas sem aplicar o override esperado naquele tick.

**Por que importa:** é um erro de configuração fácil de cometer (o `.json` é editado à mão por cada admin de instalação FIKA, conforme a spec funcional exige consistência entre peers) e o sintoma (estação não muda) é sutil o suficiente pra não ser óbvio sem checar o log do servidor.

**Sugestão:** usar `Enum.TryParse<Season>(seasonName, ignoreCase: true, out var season)` no stub.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada conforme proposto, e estendida para os dois outros `Enum.Parse`/`TryParse` do mesmo stub (nome de estação e nome de `WeatherPreset`), pela mesma razão.
**Aplicação:** `002-...-02-spec-tech.md` §5 (`Apply()`: `Enum.TryParse(activeSeasonName, ignoreCase: true, ...)` e ambos os `Enum.Parse<WeatherPreset>(kv.Key, ignoreCase: true)`).
