# Memória de Sessões — TRL-WeatherSync

## Estado atual

- **Item 001 (Sincronização Contínua de Clima em Raid) fechado no papel e compilado, com Fix 01 aplicado (2026-09-12):** spec, review, code review (3/3 aplicados), Fix 01 e as-built prontos. Código em `modded/Client/`, versão `1.1.2`, 0 avisos/erros. **Tempestade sincronizada (CR-01-02) removida por completo** — causava nevasca de inverno incorreta em qualquer estação (achado via feedback in-raid + Assembly, ver P-2.1 abaixo). Sincronização contínua de Rain/Cloudness/Wind continua normal. Falta recompilar via `/compile-mod` e reteste in-raid (P-2.4).
- **Item 002 (Gerenciador Ciclo Natural Estações) codado, revisado e compilado (2026-09-11):** spec, review técnica 01 (5/5 resolvidos), code review 01 (3/3 aplicados) e as-built prontos. Código 100% server-side em `modded/Server/` (6 arquivos, DI `IOnLoad`/`IOnUpdate`, sem Harmony) — escreve direto em `WeatherConfig.OverrideSeason`/`WeatherPresetWeight`, versão `1.0.2`, 0 erros/0 avisos. Config instalado fica em `SPT/user/mods/TRL-WeatherSync/Config/season-cycle.json` (path simplificado a pedido do usuário — sem `Resources/`, pasta singular `Config/` não `Configs/`). **Bug real achado e corrigido no code review (CR-01-01):** `SeasonCycleUpdater` sem `TypePriority` explícita podia rodar DEPOIS de `GameCallbacks` (`OnLoadOrder.GameCallbacks=300000`), que dispara `PostDbLoadService` pré-gerando e cacheando a previsão de clima no boot — sem a prioridade certa, a(s) primeira(s) raid(s) após cada restart do SPT Server usavam a estação real do calendário, não a do ciclo do mod. Corrigido com `TypePriority = OnLoadOrder.Database` (200000). Build corrigido está só em `mods/TRL-WeatherSync/builds/` — falta copiar manualmente pro install real (P-2.4) antes de validar em raid.
- **`modded/` reestruturado** em `modded/Client/` (item 001, BepInEx) + `modded/Server/` (item 002, C# puro) — segue a convenção já usada em Skills-Extended/CustomClasses.
- **`/compile-mod` já suporta `server-csharp`** (descoberto 2026-09-11 — a doc `docs/technical/spt4-mod-creation.md:142` que dizia "ainda não suportado" está desatualizada). O script instala AMBOS os lados automaticamente quando `.spt-path` é válido — sem flag pra pular.
- **Modelo de 3 papéis** (`Source`/`Relay`/`Receiver`, item 001) desacopla autoridade de clima de `FikaBackendUtils.IsServer`, resolvendo o caso Fika-Headless (P-2.2, resolvida).
- **Tempestade sincronizada (item 001) REMOVIDA (Fix 01, 2026-09-12):** `RollForStorm`/`ThunderEventTrigger`/`HandleReconnect(ESeasonStatus.Storm)` nunca produziram tempestade de verão — sempre disparavam o efeito de nevasca do Winter Event (`RainController.cs:275-280`, `Class678`), incondicionalmente, porque o `RainController` só tem UM "modo tempestade" no jogo inteiro (o de neve) — não existe variante de verão. Descoberto via feedback in-raid real (convidado viu Verão virar Inverno). Mecanismo removido; sync contínuo de Rain/Cloudness/Wind já basta pra trovão/raio nativo.
- **Estação fixa não trava o clima** (item 002, decisão do usuário 2026-09-10): quando `fixedSeason` está ativo, a estética (folhagem/neve) trava, mas os pesos de sol/chuva/neve continuam seguindo o ciclo natural — `SeasonCycleUpdater.Apply()` sobrescreve a entrada da estação fixa com os pesos da estação cíclica corrente a cada tick.
- **Preferência permanente do usuário:** nenhum compile deste mod (client ou server) deve instalar automaticamente na pasta real do jogo/servidor — ver `feedback_compile_mod_install.md` (memória pessoal) pra técnica exata (`--spt-path` inválido cobre os dois lados). **Vacilo real 2026-09-11:** esqueci de aplicar a técnica numa compilação e o script instalou os dois `.dll` direto em `E:/Tarkov Red Line`; usuário, avisado, deixou como estava dessa vez. **Corrigido na recompilação seguinte, mesma sessão:** `--spt-path <inválido> --allow-same-version` (o `--allow-same-version` porque só o server mudou de versão nessa rodada — client ficou em 1.1.1) confirmado via timestamp que não escreveu nada novo no install real. Técnica validada, P-2.5 fechada.
- Ainda sem `original/` — mod não é port, é criação original do repo.

---

## Pendências

- [P-2.8] (aberta 2026-09-12) **Validação in-game do item 004 (Gotas na Lente Reativas ao Ângulo da Câmera) + `/compile-mod` real.** Item 004 criado e implementado (patch `CameraLensRainDropsPatch` em `GClass986.Update(float dt)`, 5 novas `ConfigEntry` na seção `Lens Drops`, versão `1.3.0`, `dotnet build` local com 0 erros/0 avisos). Pendente de teste in-game (olhar para o céu, horizonte e chão para validar a taxa de spawn e a secagem rápida).
- [P-2.7] (aberta 2026-09-12, **validada in-game pelo usuário 2026-09-12**) **Validação in-game do item 003 (Chuva Fraca Mais Visível) + `/compile-mod` real.** ✅ Confirmado pelo usuário em raid: tamanho da gota de chuva ajustável ao vivo via F12 funcionou perfeitamente e resolveu a invisibilidade frontal em chuva fraca/garoa. Falta compilação/distribuição formal via `/compile-mod`.
- [P-2.4] (aberta 2026-09-11, **primeira validação positiva 2026-09-11**) **Validação in-game dos itens 001 e 002.** ✅ Confirmado pelo usuário: raid de ~5min com Fika-Headless como host, mod com os fixes copiado manualmente pro install real — 0 erros, carregou normal, árvores com aparência de Outono (folhas secas) batendo com a estação calculada pelo ciclo (`season-cycle.json` padrão tem Outono como fase corrente nessa data) — confirma visualmente que `SeasonCycleUpdater`/`OverrideSeason` está funcionando de ponta a ponta em raid real, incluindo o cenário Headless. **Ainda não testado:** chuva sincronizada entre múltiplos jogadores reais (item 001) — usuário vai testar com amigos depois, fica pra uma sessão futura. Também pendentes: reconexão em tempestade, raid1→raid2, modo estação fixa, consistência entre peers com o mesmo `.json`, e observar se o bug do `CR-01-01` (season errada logo após restart) realmente não se manifesta mais. 🟢 Reduzido de bloqueador pra acompanhamento — primeira evidência real é positiva.
- ~~[P-2.5]~~ **Resolvida 2026-09-11:** `--spt-path <inválido> --allow-same-version` confirmado (via timestamp dos arquivos no install real) que não escreve nada em `E:/Tarkov Red Line`. Continua valendo pra sempre usar essa técnica em `/compile-mod TRL-WeatherSync` — registrado em `feedback_compile_mod_install.md` (memória pessoal), não é mais pendência aberta, é procedimento padrão.
- ~~[P-2.1]~~ **Superada pelo Fix 01 (2026-09-12):** a pendência original era "como sincronizar o FIM de uma tempestade forçada" — deixou de fazer sentido porque o próprio INÍCIO forçado (`RollForStorm`/CR-01-02) foi removido de vez, por produzir o efeito ERRADO desde o início (nevasca de inverno, não tempestade de verão — ver Fix 01, `001-...-06-fix-01.md`). Causa raiz real: `RainController` (`RainController.cs:248-280`) só tem UM state machine de "tempestade" no jogo inteiro (`Class670.vmethod_7()` sempre cria `Class678`/`WinterStormReconnect`, incondicional) — a tag `ESeason.Summer` em `Class444.Class451`/`Class452` é só bookkeeping do OUTRO state machine e não influencia o efeito visual real. **Lição:** hipótese "tempestade de verão via HandleReconnect(Storm)" nunca foi confirmada por leitura completa da cadeia de efeitos (só a entrada em Storm foi confirmada, não o resultado visual) — só apareceu como bug real em feedback in-raid, meses depois. Reforça a regra de graph-code-navigation/AP-09: confirmar o EFEITO final, não só que a chamada "funciona" sem erro.
- [P-2.6] (aberta 2026-09-12) **Segunda IA (Gemini) usada como investigação externa, verificada antes de agir.** Usuário trouxe um diagnóstico do Gemini sobre o bug da tempestade; a resposta tinha o diagnóstico final CORRETO mas um detalhe técnico impreciso (framing de "Storm = evento de Inverno" no `Class444`, quando na verdade `Class444` tagueia esse storm como `ESeason.Summer` — o problema real está só no `RainController`, um state machine separado). Toda alegação técnica externa foi reconfirmada linha por linha no Assembly antes de qualquer código ser tocado, e a imprecisão foi corrigida na explicação ao usuário em vez de repassada sem revisão. 🟢 Prática validada — continuar tratando output de outras IAs como candidato a verificar, nunca como fonte pinada (mesmo espírito do AP-09).
- [P-1.4] (aberta 2026-09-02) **Implementação dos Patches de Otimização de Neve e Chuva** — Harmony patches em `SnowFlakes.cs` (redução de malhas de partículas) e `SnowWetRenderer.cs` (culling em miras óticas e bypass de CommandBuffer HDR). Citações validadas (`SnowFlakes.cs:24` `int_1 = 16383` × 4 materiais ≈ 65.532; `SnowFlakes.cs:190`; `SnowWetRenderer.cs:322` e `:350`). 🟢 Performance, ainda não iniciada.

---

## 2026-09-11/12 (GMT-3) — Sessão 5: Validação in-game + investigação FIKA custom-weather/time + Fix 01 (bug real da tempestade)

**Tema central:** Primeira validação in-game real dos itens 001/002, duas investigações de interação com telas nativas do FIKA (clima customizado da raid, time-of-day/flow — nenhuma delas exige mudança nossa), pesquisa de viabilidade pro P-1.4 (ainda não iniciado), e o achado mais importante da sessão: um bug real e confirmado na feature de tempestade sincronizada do item 001, reportado pelo usuário depois de testar com um amigo.

**Decisões-chave:**
- [Primeiro teste in-game positivo]: raid de ~5min com Fika-Headless como host — 0 erros, estação (Outono) bateu com o cálculo do `season-cycle.json`. Confirma item 002 funcionando ponta a ponta, incluindo o cenário Headless.
- [`referenceEpochUtc` explicado e recalculado à pedido]: usuário queria alinhar a virada de estação pra domingo 00:01 (horário de Brasília) — calculado o timestamp exato (`1789268460`) e explicado que aplicar isso IMEDIATAMENTE faz "hoje" regredir pra Verão (fim do ciclo anterior) até domingo — usuário aceitou de propósito.
- [Path do config simplificado a pedido do usuário]: `Resources/Configs/season-cycle.json` → `Config/season-cycle.json` (sem "Resources/", pasta singular). Aplicado em `SeasonCycleConfigController.cs`, `Server.csproj`, doc. Validado com `dotnet build` limpo duas vezes (usuário corrigiu "Configs" pra "Config" no meio do processo).
- [FIKA "Use custom weather" investigado]: quando o host liga esse toggle nativo do FIKA e escolhe clima manual nos dropdowns, `SetupCustomWeather()` aplica o clima escolhido e `GenerateWeathers()` não sobrescreve — MAS a estação (item 002, `Season`) continua chegando certa, porque `WeatherRequest()` sempre roda antes independente do toggle. Só os PESOS de clima do item 002 ficam sem efeito nessa raid específica. Decisão: ignorar/respeitar, mesmo padrão do mapa Laboratory — não vale a pena brigar por cima da escolha manual do host.
- [Time of day / time flow confirmados como não tocados por nós]: são tratados 100% pelo FIKA (`CoopGame.cs:100-129`), nenhum item do mod lê ou escreve isso.
- [P-1.4 (redução de partículas de neve/chuva) — pesquisa de viabilidade, ainda não implementado]: `SnowFlakes.cs` cria a malha de partículas UMA VEZ no `Start()` (não dá pra trocar contagem ao vivo sem reconstruir a malha — precisa pelo menos reentrar na raid); já `SnowWetRenderer.cs`'s culling de mira ótica/CommandBuffer roda por frame (esse SIM dá pra ligar/desligar ao vivo com um simples `if`). Registrado como referência pra quando o P-1.4 for codado de verdade.
- **[Achado principal] Bug real na tempestade sincronizada (CR-01-02), corrigido no Fix 01**: usuário reportou que um convidado (Receiver) viu Verão virar Inverno no meio de uma raid hospedada por ele. Usuário consultou o Gemini, que diagnosticou (com uma imprecisão de framing) que `HandleReconnect(ESeasonStatus.Storm)` na verdade aciona o efeito de nevasca do Winter Event, não uma tempestade de verão. Reconfirmado linha por linha no Assembly (`Class444.cs`, `RainController.cs`) antes de agir: `RainController` só tem UM state machine de "tempestade" (`Class670.vmethod_7()` sempre cria `Class678`/`WinterStormReconnect`, incondicional) — não existe variante de verão, apesar de `Class444.Class451`/`Class452` (outro state machine, separado) tagueá-lo como `ESeason.Summer`. Mecanismo removido por completo (`06-fix-01.md`, item 001) — sync contínuo de Rain/Cloudness/Wind já basta.

**Lições / hipóteses descartadas:**
- Uma segunda IA (Gemini) trazida pelo usuário deu um diagnóstico majoritariamente correto, mas com um detalhe impreciso — tratado como candidato a verificar, não fonte pinada, e a imprecisão foi corrigida na explicação ao usuário em vez de repassada (P-2.6).
- A hipótese original de que `HandleReconnect(ESeasonStatus.Storm)` produziria "só" uma tempestade de verão nunca foi confirmada por leitura completa da cadeia de efeitos até o `RainController` — só a ENTRADA no estado tinha sido confirmada (Sessão 3), não o resultado visual final. Só apareceu como bug real em feedback in-raid, dias depois de "entregue". Lição: confirmar o efeito final ponta a ponta, não só que a chamada não lança exceção.

**Atividade cronológica (resumo — detalhe nos artefatos):**
1. Validação in-game (Headless, Ground Zero, Outono confirmado) → P-2.4 avançada.
2. Explicação + cálculo de `referenceEpochUtc` pro domingo 00:01 BRT, a pedido do usuário.
3. Simplificação de path `Resources/Configs/` → `Config/` (item 002), com correção de "Configs" pra "Config" no meio do processo.
4. Investigação FIKA "Use custom weather" (screenshots do usuário) + confirmação de que time-of-day/flow não são tocados pelo mod.
5. Pesquisa de viabilidade pro P-1.4 (partículas de neve/chuva on/off ao vivo) — sem código ainda.
6. Relato de bug real (tempestade→nevasca) → segunda opinião do Gemini → verificação própria no Assembly → `06-fix-01` criado e aplicado no item 001.

**Pendências abertas nesta sessão:** ver bloco "Pendências" no topo (P-2.1 superada, P-2.6 nova — lição sobre verificar IA externa; P-2.4/P-1.4 seguem, sem mudança de conteúdo além do já registrado).

---

## 2026-09-11 02:00 (GMT-3) — Sessão 4: Item 002 completo (spec técnica → review → código, 100% server-side)

**Tema central:** Levar o item 002 (Gerenciador Ciclo Natural Estações) da spec funcional já pronta (criada no fim da Sessão 3) até implementado e revisado — descoberta central: o item inteiro cabe no servidor, sem nenhum código de cliente.

**Decisões-chave:**
- [Item é 100% server-side]: `SeasonalEventService.GetActiveWeatherSeason()` (`SeasonalEventService.cs:307-312`) já checa `WeatherConfig.OverrideSeason` antes do calendário real — bastou escrever nesse campo (+ `WeatherPresetWeight`) via um componente `IOnLoad`/`IOnUpdate` injetado com `ConfigServer`. A estação chega ao client pelo handshake nativo que o item 001 já documentou (`_backendSession.WeatherRequest()`), sem nada pra sincronizar em raid.
- [Precedente real usado como referência]: `mods/Skills-Extended/modded/Server/` (padrão `IOnLoad`+`ConfigController` lendo `.json` de `Resources/Configs/`) e `mods/CustomClasses/modded/Server/` confirmaram que a convenção `modded/Client/`+`modded/Server/` já funciona no repo — usada como base do design em vez de inventar um padrão novo.
- [Estação fixa não trava o clima]: correção do usuário sobre um achado de review (PA-01-04) — "estação fixa" trava só a estética/sub-fase nativa (folhagem, neve acumulada), NÃO a chance de sol/chuva/neve. `SeasonCycleUpdater.Apply()` foi redesenhado pra sempre calcular a estação cíclica em paralelo e sobrescrever os pesos da estação fixa com os da estação cíclica corrente a cada tick.
- [Risco crítico achado na review, não na spec]: `App.cs:68-71` (boot do servidor SPT) não embrulha `IOnLoad.OnLoad()` em try/catch — uma exceção não tratada em qualquer mod (inclusive o nosso) pode abortar o boot do servidor inteiro. PA-01-01 (🔴 bloqueador) corrigido com try/catch + null-guard nos dois componentes novos.
- [Reestruturação `modded/` aprovada]: usuário aprovou mover os arquivos client do item 001 pra `modded/Client/`, liberando `modded/Server/` pro item novo — confirmado via `AskUserQuestion`, `dotnet build` validou que nada quebrou.
- [Build manual até o `/compile-mod` suportar server-csharp]: usuário optou por build manual (`dotnet build`) em vez de pedir extensão do script — decisão explícita, não pedir a extensão sem o usuário voltar a pedir (P-2.5).

**Lições / hipóteses descartadas:**
- **Suposição de ordem de `IOnLoad` não provada**: o stub original assumia que `TypePriority = OnLoadOrder.PreSptModLoader` garantia que o `SeasonCycleConfigController` carregasse antes do `SeasonCycleUpdater` — mas `App.cs:68` não expõe, no código vendorizado deste repo, nenhuma ordenação por prioridade (ela viveria dentro do pacote `SPTarkov.DI`, não vendorizado). Resolvido com defensividade (null-guard) em vez de confiar na suposição.
- **`AbstractModMetadata` tem mais membros abstratos do que o stub assumia**: `Contributors`/`Incompatibilities`/`ModDependencies`/`Url`/`IsBundleMod` são abstratos apesar de nullable — só descoberto rodando `dotnet build` e lendo o erro `CS0534`. Lição: mesmo copiando um precedente real (Skills-Extended), só a compilação prova a superfície completa de uma classe externa não vendorizada.
- **`JsonUtil` do servidor rejeita chaves JSON desconhecidas**: `UnmappedMemberHandling.Disallow` (`JsonUtil.cs:24`) — um campo `_comment` de documentação no `.json` teria quebrado o deserializer silenciosamente (capturado pelo try/catch da PA-01-01, mas o mod inteiro pararia de funcionar). `ReadCommentHandling.Skip` (`JsonUtil.cs:20`) confirma que comentários `//` de linha são o jeito correto de documentar o `.json`.

**Atividade cronológica (resumo — detalhe nos artefatos):**
1. `/create-technical-spec` — pesquisa confirmou o design 100% server-side; `02-spec-tech.md` criada citando `WeatherConfig.cs`, `SeasonalEventService.cs`, `WeatherGenerator.cs`, `ConfigServer.cs`, `WeatherController.cs`, `App.cs`, `HostGameController.cs`/`ClientGameController.cs` (laboratory/labyrinth).
2. `/review-technical-spec` — 5 achados (1🔴/2🟡/2🟢); usuário resolveu todos numa resposta só, incluindo a correção de design da PA-01-04.
3. `AskUserQuestion` — duas decisões: reestruturar `modded/` (sim) e build manual vs. estender `/compile-mod` (manual).
4. `/code-mod` — `modded/` reestruturado, 6 arquivos novos em `modded/Server/`, `dotnet build` validado nos dois lados (client após mover, server novo), 3 ajustes descobertos durante o build documentados no as-built.

**Pendências abertas nesta sessão:** ver bloco "Pendências" no topo (P-2.4 nova — validação in-game dos dois itens; P-2.5 nova — tooling `/compile-mod`; P-2.1/P-1.4 já existiam, sem mudança de conteúdo).

**Cross-refs:** resolve P-2.3 (aberta na Sessão 3) e P-1.3 (aberta na Sessão 1) — feature implementada, detalhe nos artefatos de `backlog/002-gerenciador-ciclo-natural-estacoes/`, não duplicado aqui.

---

## 2026-09-10 (GMT-3) — Sessão 3: Item 001 completo (backlog → spec → código → review → fix → compile)

**Tema central:** Levar o item 001 (Sincronização Contínua de Clima em Raid) do zero até implementado, revisado e compilado — ciclo completo de backlog do repo, mais duas investigações extras pedidas pelo usuário (papel de clima em raid Headless; se "tempestade" é distinto de chuva forte).

**Decisões-chave:**
- [Modelo de 3 papéis resolve o caso Headless]: `FikaBackendUtils.IsHeadless`/`IsHeadlessGame`/`IsHeadlessRequester` já existem no FIKA e bastam pra desacoplar "autoridade de clima" de "Host de rede" — não precisou inventar handshake de eleição. Ideia do usuário ("host-convidado"). Ref: `001-...-02-spec-tech.md` §1.1.
- [Bug crítico achado e corrigido: escala Rain/Wind]: `IWeatherCurve.Rain`/`Wind` (lido no Host) vêm normalizados 0-1; `WeatherClass.Rain`/`Wind` (o que `SetWeatherForce` espera) precisa estar em 1-5 — sem `Mathf.Lerp(1f, 5f, ...)` antes de transmitir, todo clima sincronizado caía pra zero silenciosamente. Achado em `/code-review` (CR-01-01), não em spec — lição: mesmo com toda a spec técnica revisada 2x, um bug de unidade só apareceu lendo o código implementado contra o Assembly de novo.
- [Tempestade sincroniza só o início, não o fim]: decisão consciente do usuário depois de descobrir que forçar o fim exige reverter DOIS state machines (`Class444` + `RainController`), nenhum caminho de saída confirmado ainda (P-2.1). Política de início: sorteio contra `IWeatherCurve.LightningThunderProbability` + cooldown configurável (`WeatherSyncSession.RollForStorm`).
- [Preferência de instalação confirmada de novo]: `/compile-mod` neste mod nunca instala automaticamente — usuário reforçou isso preventivamente antes mesmo de eu rodar o comando. Memória pessoal do usuário (`feedback_compile_mod_install.md`) atualizada com a técnica usada (referências pré-populadas manualmente + `--spt-path` inválido de propósito).

**Lições / hipóteses descartadas:**
- **Autocorreção:** a alegação da Sessão 2 de que `ERainControllerStatus` não existe estava errada — existe aninhado dentro de `RainController` (`RainController.cs:19-29`), não como tipo de topo de arquivo. A nevasca de inverno (`WinterStorm`) é real, e se conecta ao `ESeasonStatus.Storm` de `Class444` via `RainController.method_8()`/`method_9()`. Motivada por pergunta do usuário ("será que tempestade é chuva nível 5?") — hipótese em si não confirmada (não há checagem direta de limiar no código lido), mas a investigação valeu a pena mesmo assim.
- Hipótese "chuva nível 5 = tempestade direto" — não confirmada por código; tratada como correlação plausível (via `LightningThunderProbability`, que deriva de `Cloudiness`), não uma regra direta encontrada.

**Atividade cronológica (resumo — detalhe nos artefatos):**
1. `/add-backlog-item` → `/create-spec` → `/review-spec` → `/create-technical-spec` → `/review-technical-spec` (2 rodadas, 8 pontos, todos resolvidos) → `/code-mod` (6 arquivos criados).
2. Investigação extra: papel de clima em raid Headless (pergunta do usuário) → modelo de 3 papéis adicionado à spec técnica, autorrevisão achou e corrigiu um bug próprio (eco do Relay causaria `ParseException` no Source).
3. `/compile-mod` (1ª vez, v1.0.0, sem instalar no jogo) → `/code-review` (achou CR-01-01 crítico + CR-01-02/03) → `/apply-code-review` (CR-01-01) → recompile (v1.0.1).
4. Investigação extra: autocorreção do `ERainControllerStatus` + dados reais de `weather.json` do servidor (pesos de clima por estação, datas de calendário).
5. Decisão do usuário sobre CR-01-02 (só início, não fim) → implementado `RollForStorm` + CR-01-03 (precompute) → recompile (v1.1.0). Rodada 01 de code-review fechada (3/3).

**Pendências abertas nesta sessão:** ver bloco "Pendências" no topo (P-2.3, P-2.1 ampliada aqui; P-1.3/P-1.4 já existiam, sem mudança de conteúdo).

**Cross-refs:** resolve P-2.2 (aberta na Sessão 2) e P-1.1/P-1.2/P-1.2.1/P-1.2.2/P-1.2.3 (abertas nas Sessões 1/2) — todas ✅, detalhe nos commits/artefatos de `backlog/001-sincronizacao-continua-clima-raid/`, não duplicado aqui.

---

## 2026-09-10 (GMT-3) — Sessão 2: Pesquisa de Handshake FIKA e API Real de Clima/Estações (correção do Roadmap)

**Tema central:** Antes de começar a codar, verificar se o `ROADMAP.md` tinha base suficiente em código real — resultado: a parte de otimização de neve/chuva (§4) já tinha citações reais e conferiu; a parte de rede/handshake do FIKA (§5/§6) e o mapeamento de sub-fases de estação (§2) tinham lacunas ou alegações não verificadas. Sessão de leitura pura (sem código escrito), relatório completo em [`docs/investigacao-fika-eft-2026-09-10.md`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-WeatherSync/docs/investigacao-fika-eft-2026-09-10.md).

**Decisões-chave:**
- [Correção — depois refutada na Sessão 3]: `ERainControllerStatus` não existe no assembly decompilado. **Isso estava errado — ver autocorreção na Sessão 3.** Mantido aqui por imutabilidade (§8 memory-curation); não editar retroativamente.
- [Storm é evento de servidor, não fase agendada]: `ESeasonStatus.Storm` (`Class451`/`Class452`) só é alcançado via `StormStartedEvent : SyncEventFromServer` (payload vazio), escutado por `Class444.method_1` e propagado pelo estado atual (`StormStarted(visual)`). Isso não entra no `cycleOrder` do `server/config.json` — é candidato a ser sincronizado como evento de rede (`ThunderEventTrigger`), não como fase do calendário.
- [Handshake de clima do FIKA confirmado]: `ClientGameController.GetWeather()` (`ClientGameController.cs:147-165`) envia `RequestPacket{Type=Weather}` uma única vez no loading screen e faz polling até `WeatherReady`; `WeatherController.Instance.method_0(WeatherClasses)` é o ponto real de aplicação, usado tanto pelo Host (`HostGameController.cs:384/542`) quanto pelo Client (`ClientGameController.cs:139`). Confirma o diagnóstico original do Roadmap §1 sem alterações.
- [Padrão de broadcast periódico já existe no FIKA]: `FikaServer.cs` já implementa exatamente o padrão necessário para `TrlWeatherSyncPacket` — accumulator `_sendThreshold` em `Update()` disparando `SendData(ref packet, DeliveryMethod.Unreliable)` em broadcast (usado hoje para FPS via `StatisticsPacket`). Virou o template de implementação (usado na Sessão 3).
- [Chuva normal não tem o problema da tempestade]: `WeatherCurve.RainCurve` é dado contínuo interpolado, sem decisão local — diferente de `Storm`. Premissa do Roadmap original sobre "RainRandomness = geradores de ruído da Unity" não se sustentou: é só um nome de legado pro campo `rain_intensity` do servidor, não consumido pela curva.
- [Servidor SPT não tem conceito de "Storm"]: só 3 presets (`SUNNY`/`RAINY`/`CLOUDY`). `RaidWeatherService` é singleton por processo — cada FIKA player gera sua própria previsão com RNG isolado, motivo do handshake único existir.
- [Arquitetura de registro de pacote sem alterar o FIKA]: `RegisterPacket<T>` é público, parte do contrato `IFikaNetworkManager` — consumível direto do plugin externo, sem Harmony patch nos internals nem fork.
- [`SetWeatherForce` faz a interpolação suave nativa]: usado na Sessão 3 em vez de Lerp manual.

**Lições / hipóteses descartadas:**
- Ver autocorreção do `ERainControllerStatus` na Sessão 3 — a lição desta sessão (`ERainControllerStatus` não existe) foi refutada por evidência nova.

---

## 2026-09-02 14:10 (GMT-3) — Sessão 1: Concepção, Investigação de Sub-Fases de Estações, Otimização de Neve/Chuva e Roadmap

**Tema central:** Diagnóstico aprofundado dos gargalos de sincronização de clima do EFT/FIKA, investigação das sub-fases sazonais nativas do Tarkov, arquitetura de controle de 1 semana real por estação via servidor e soluções de otimização de FPS na neve/chuva.

**Decisões-chave:**
- [Sub-Fases Sazonais Nativas]: Mapeamento de todas as variantes internas do EFT no `ESeason`, `ESeasonStatus` e `Class444`:
  - Primavera: `SpringEarly` (degelo de neve residual) e `Spring` (plena e florida).
  - Verão: `Summer` (ensolarado) e `Storm` (tempestades tropicais).
  - Outono: `Autumn` (folhas douradas) e `AutumnLate` (árvores secas sem folhas, solo gélido).
  - Inverno: `Winter` (neve estável) e `WinterStorm` (nevasca violenta com vento horizontal) — **confirmado real na Sessão 3**, depois de ter sido descartado por engano na Sessão 2.
- [Controle de Tempo via Servidor]: Definição de controle determinístico por timestamp UTC no `server/config.json`, com adoção oficial da **Opção A** (1 semana real de 7 dias por estação macro, totalizando 4 semanas / 28 dias reais para o ciclo anual completo, com 3,5 dias em sub-fases de transição na Primavera e Outono). Imune ao `TimeFactor` (~7x) da partida.
- [Otimização de Neve e Chuva]:
  - Constatado que a chuva/neve segue a câmera do jogador a 10m (`RainFollow.cs`).
  - Identificado o gargalo das 65.532 partículas do `SnowFlakes.cs` e a dupla renderização pesada nas miras telescópicas (`OpticCameraManager.Camera`).
  - Estabelecido o módulo de otimização com presets de partículas (4k/8k), culling de neve em miras e desativação do passe HDR 10-bit de glitters.
