---
name: spt-performance-analysis
description: Performance engineering for SPT 4.0 / EFT 0.16.x / Fika mods (client BepInEx plugins and C# server mods). Use during /audit-mod-code (mandatory in --perf mode), /optimize-mod-performance, and during /review-technical-spec, /code-mod and /code-review whenever the mod patches frequently-called EFT methods, runs per-frame/per-bot work, or keeps timers/coroutines/polling loops alive. Provides the cost model (unit cost × frequency × entity count × lifetime + growth), an execution-surface taxonomy with grep recipes, a method to estimate the call frequency of a Harmony target, execution-lifecycle auditing, config-surface auditing, low-overhead temporary instrumentation patterns, and a measured before/after validation plan. Points to `spt-memory-leak-analysis` (allocation churn / HOT), `spt-mod-best-practices` §2/§3 and `csharp-mod-best-practices` §1–§3 for the underlying rules instead of repeating them.
---

# SPT / EFT / Fika Performance Analysis

Corpo de conhecimento para **investigar e otimizar custo de execução** (CPU, alocações, recursos) em mods deste repo. Alvo: SPT 4.0 / EFT 0.16.x, client (BepInEx/Harmony/Unity) e server (C# `[Injectable]` ou TS legado).

> **Esta skill NÃO reescreve as regras de hot path e lifecycle** — elas já vivem em:
> - `spt-mod-best-practices` §2 (raid lifecycle), §3 (memory & performance: nada de LINQ/alloc/reflection não cacheada em hot path), §6 (logging).
> - `csharp-mod-best-practices` §1 (allocations), §2 (async/threading/coroutines), §3 (Harmony/reflection cacheada).
> - `spt-memory-leak-analysis` §3 (mecanismo **HOT** — churn de alocação por frame) e §6 (medição de memória in-game).
> - `docs/technical/spt-antipatterns.md`: **AP-01** (teardown ausente), **AP-03** (virtual dispatch), **AP-04** (bypass de API canônica), **AP-06** (compilar ≠ funcionar).
>
> O que **esta** skill adiciona: (1) o **modelo de custo** que prioriza achados (frequência × entidades × duração × acúmulo), (2) uma **taxonomia de superfícies de execução** com greps de detecção, (3) o método para **estimar a frequência de um alvo Harmony**, (4) a auditoria de **ciclo de vida de execução** (trabalho que continua quando deveria parar), (5) a auditoria de **configuração** como alavanca de performance, (6) padrões de **instrumentação temporária de baixo overhead**, e (7) o **plano de validação medida** antes/depois.

## 0. As duas perguntas-guia

Toda a investigação se organiza em torno de:

1. **"O que este mod está fazendo mais vezes, por mais tempo, para mais entidades ou por mais ciclos de vida do que realmente precisa?"**
2. **"Quando esse processamento deveria deixar de existir — e ele realmente deixa?"**

Não se procura "código lento" em abstrato: procura-se **trabalho multiplicado** (por frame, por bot, por evento) e **trabalho zumbi** (que sobrevive à razão de existir). Contexto do repo que dá peso a isso: o SPT é CPU-bound single-thread na IA dos bots (`wiki/spt/Performance_Tuning.md`), e todo mod client roda também no **Fika headless** — um processo que fica de pé por horas hospedando raid após raid (`spt-memory-leak-analysis` §1).

---

## 1. Modelo de custo (o que define a prioridade)

Custo efetivo de um trecho ≈ **custo unitário × frequência × nº de entidades × duração de vida**, ajustado pelo **potencial de acúmulo** (o custo cresce ao longo da raid?). A fórmula não é para calcular literalmente — é para obrigar cada achado a responder os cinco eixos:

### 1.1 Classes de frequência (severidade base)

| Classe | Gatilho típico | Ordem de grandeza | Severidade base |
|---|---|---|---|
| **per-frame** | `Update`/`LateUpdate`/`FixedUpdate`/`OnGUI`, patch em método por-frame | 60–144×/s | 🔴 |
| **per-tick-IA / per-bot periódico** | lógica por bot em tick de IA ou timer curto | dezenas/s × N bots | 🔴/🟠 |
| **per-event frequente** | tiro, hit, movimento, pacote de rede | rajadas; escala com atividade | 🟠 |
| **per-event raro** | spawn, morte, porta, extração, troca de arma | unidades/min | 🟡 |
| **per-raid** | raid-start/raid-end | 1× por raid | 🟢 |
| **one-shot/boot** | `Awake`, load de config/asset | 1× por processo | 🟢 |

### 1.2 Multiplicador de entidades

O mesmo código muda de classe conforme **para quantas entidades roda**: ×1 (só o player local) · ×N bots (20–50 num mapa cheio, mais com mods de spawn) · ×M objetos (cadáveres, itens, contêineres, projéteis) · **×N×M** (padrão "para cada bot, varrer todos os outros" — O(N²), o pior caso). Um método barato ×1 vira ofensor real ×50; sempre registrar o multiplicador.

### 1.3 Duração de vida da execução

Até quando o trabalho roda? **para sempre** (componente/coroutine nunca parado) · **até o fim da raid** · **até despawn/morte da entidade** · **escopo curto** (dentro de um evento). Trabalho com duração maior que a necessidade é achado mesmo quando barato — ver §4.

### 1.4 Potencial de acúmulo

O custo **de hoje** é o mesmo de daqui a 40 minutos? Sinais de não: coleção varrida que só cresce, handlers duplicados a cada onda/raid (trabalho 2×, 3×…), cadáveres/entidades mortas ainda processadas, segunda onda custando mais que a primeira. Acúmulo promove a severidade — é o análogo de CPU do leak per-raid.

### 1.5 Ajuste pelo custo unitário

Sobe: raycast/física, busca global de objeto, reflection não cacheada, I/O, serialização JSON, alocação que dispara GC, log com formatação. Desce: comparação de campo, early-return, leitura de bool. Severidade final = classe de frequência × entidades × duração × acúmulo, **ajustada** pelo custo unitário e pela **certeza** (lido e provado vs. hipótese).

---

## 2. Taxonomia de superfícies de execução (com grep de detecção)

Cada mecanismo tem um código usado nos achados `AUD-NN-MM` do relatório de auditoria. Os greps **apontam candidatos**; a confirmação é sempre a leitura do `arquivo.cs:linha` com estimativa de frequência (§3) e lifecycle (§4).

### FREQ — execução periódica com frequência acima da necessidade
- **Sintoma:** trabalho em `Update`/`FixedUpdate`/`LateUpdate`/`OnGUI`, `InvokeRepeating` com intervalo curto, coroutine em loop com `WaitForSeconds` pequeno, `while (true)` async com `Task.Delay` curto, polling de estado que tem evento nativo (AP-03 de polling — ver `/audit-mod-code` Dimensão 2 e `spt-mod-best-practices` checklist 3).
- **Grep:** `void (Update|FixedUpdate|LateUpdate|OnGUI)\(` · `InvokeRepeating|WaitForSeconds|WaitForEndOfFrame|Task\.Delay|while \(true\)|while\(true\)` · `Time\.(deltaTime|time|frameCount)`.
- **Pergunta-chave:** o resultado muda a cada frame, ou só quando um evento acontece? Se há evento nativo/ponto de patch para a transição, polling é desperdício por construção.

### PATCH — Harmony em método quente do EFT
- **Sintoma:** `[HarmonyPatch]` (ou `GetTargetMethod`) num método que o jogo chama por frame, por tick de IA ou por entidade. O corpo do patch multiplica pelo tráfego do alvo — um patch "pequeno" num método chamado milhares de vezes é um ofensor grande. Agrava: alocação, log, reflection não cacheada ou chamada adicional dentro do patch.
- **Grep:** `\[HarmonyPatch|HarmonyPostfix|HarmonyPrefix|GetTargetMethod` — e para cada alvo, estimar a frequência pelo §3.
- **Pergunta-chave:** quantas vezes por segundo o **alvo** roda, e para quantas instâncias? O patch tem early-return barato no topo para os contextos em que não interessa (menu/hideout/bot não relevante — `spt-mod-best-practices` §2)?

### ENT — trabalho multiplicado por entidades
- **Sintoma:** loop sobre a lista de players/bots/objetos do mundo em superfície frequente; operação global executada por entidade em vez de 1× compartilhada; padrão O(N²) ("para cada bot, olhar todos os outros"); processamento de entidades irrelevantes (mortas, distantes, fora do interesse do mod).
- **Grep:** `foreach|for \(` cruzado com `AllAlivePlayers|RegisteredPlayers|Players|Bots|allObservedPlayers|GetComponentsInChildren|FindObjectsOfType` — confirmar no decompile o nome real da coleção usada.
- **Pergunta-chave:** dá para filtrar antes de iterar (só vivos, só no raio, só os N relevantes), cachear o subconjunto e atualizá-lo por evento (spawn/morte), ou inverter o loop (1 varredura global compartilhada em vez de 1 por entidade)?

### LIFE — ciclo de vida de execução (trabalho zumbi ou duplicado)
- **Sintoma:** o trabalho continua quando a razão dele já morreu — ver §4. Inclui **duplicação**: componente/coroutine/subscription criada de novo a cada raid/onda/evento sem destruir a anterior → execução 2×, 3×… crescente (mesma raiz do leak EVT/DISP da `spt-memory-leak-analysis` §3, mas o sintoma aqui é **CPU crescente**, não só retenção).
- **Grep:** os mesmos da skill de leak (`+= |StartCoroutine|AddComponent|new GameObject|InvokeRepeating|Task\.Run|new Timer`) — mas a pergunta muda: não "quem libera a memória?", e sim "**quem para a execução**, e o que acontece se o start rodar duas vezes?".

### GROW — custo que cresce com o tempo de raid
- **Sintoma:** operação frequente sobre estrutura que só cresce (lista de cadáveres, histórico de eventos, registry sem remoção) — o custo por chamada aumenta ao longo da raid mesmo sem "bug" pontual. Segunda onda mais cara que a primeira; raid de 40 min mais pesada que a de 5.
- **Grep:** `\.Add\(|\.Enqueue\(|\.Push\(` em coleções de instância/static, cruzado com quem **itera** essas coleções em superfície FREQ/PATCH — e o grep negativo do `.Remove/.Clear/.Dequeue` no ponto certo.
- **Pergunta-chave:** quem remove, quando, e existe limite? (Complementa STAT da skill de leak — lá o eixo é retenção; aqui é o custo de varrer o que cresceu.)

### UNITY — operação Unity cara em superfície frequente
- **Sintoma:** em contexto FREQ/PATCH/ENT: `GameObject.Find`/`FindObjectOfType`/`FindObjectsOfType` (varrem a cena), `GetComponent*` repetido no mesmo objeto, `Camera.main` (busca por tag), raycasts/`Physics.Overlap*`/queries de física, `Instantiate`/`Destroy` em volume, `.material`/`.materials` (clona), criação dinâmica de `Texture`/`Mesh`, APIs que retornam array novo a cada acesso (ex.: `.transforms`, `GetComponents` sem buffer).
- **Grep:** `GameObject\.Find|FindObjectOfType|FindObjectsOfType|GetComponent|Camera\.main|Physics\.|Raycast|OverlapSphere|OverlapBox|Instantiate\(|\.material\b`.
- **Fix típico:** resolver 1× e cachear (invalidando por evento), buffers `NonAlloc` quando existirem, `sharedMaterial`/`MaterialPropertyBlock`, pooling (`spt-memory-leak-analysis` §8.3).

### ALLOC — churn de alocação em hot path
- **Delegado:** é o mecanismo **HOT** da `spt-memory-leak-analysis` §3 (greps e fixes lá; regras em `csharp-mod-best-practices` §1). Aqui só entra o enquadramento de custo: alocação por frame = pressão de GC = frametime spike no cliente e hitch no headless (que roda GC incremental em raid). Classificar como ALLOC no relatório de performance e referenciar a skill de leak, sem duplicar.

### LOG — logging em superfície frequente
- **Sintoma:** `LogInfo`/`LogWarning`/`Debug.Log`/console em Update, em loop, por bot ou por evento frequente; string interpolada/formatada **antes** do check de nível (o custo existe mesmo com log desligado); dumps grandes; mesma informação repetida.
- **Grep:** `LogInfo|LogWarning|LogError|LogDebug|Debug\.Log|Console\.Write` cruzado com superfícies FREQ/PATCH/ENT.
- **Fix típico:** nível certo (`LogDebug` gated por config — `spt-mod-best-practices` §6), checar o gate **antes** de montar a string, rate-limit/agregação, logar só na mudança de estado.

### IO — I/O, serialização e trabalho externo em superfície frequente
- **Sintoma:** leitura/escrita de arquivo, `JsonConvert`/serialização, reflection não cacheada (`csharp-mod-best-practices` §3), chamada HTTP/rede, releitura de config em hot path; no server: query/reconstrução repetida do que é imutável (ver `reference_spt_localedb_per_call_cost` — caso real: `GetLocaleDb()` re-materializava um dict a cada chamada, 3.8s→40ms com cache).
- **Grep:** `File\.|Directory\.|StreamReader|StreamWriter|JsonConvert|JObject|Deserialize|Serialize|GetMethod\(|GetField\(|GetProperty\(|AccessTools\.` em superfícies frequentes · `HttpClient|WebRequest|UnityWebRequest` fora de fluxo one-shot.

### CFG — configuração agressiva (a alavanca sem código)
- **Sintoma:** o mecanismo está correto, mas os **parâmetros** o tornam caro: intervalo curto demais, raio/distância grande demais, cap de entidades alto, scan global habilitado, debug logging ligado por default, limpeza desabilitada.
- **Onde olhar:** `Config.Bind` no `Plugin.cs` + `PROPRIEDADES.md` (client) · `config/*.json*`/`.cfg`/`.ini` e presets (server/pacote) — mapear **quais chaves alimentam frequência, raio, quantidade, logging e limpeza**, o default de cada uma e onde o valor entra no código.
- **Pergunta-chave:** existe um default mais são que entrega ~a mesma experiência? Timers periódicos de vários sistemas estão sincronizados (mesmo intervalo, mesma fase) causando spike no mesmo frame? (Dessinronizar com offset/jitter é fix barato.)

---

## 3. Estimar a frequência de um alvo Harmony (método)

Um patch herda o tráfego do alvo — estimar esse tráfego é obrigatório antes de classificar o achado:

1. **Ler o alvo no decompile** (`references/eft-decompiled/`, confirmar existência via `types-index.json` — AP-09): o próprio corpo costuma revelar a classe de frequência (chamado de um `Update`? de um evento? tem `deltaTime`?).
2. **Grafo aponta os callers** (`graph-code-navigation`): `get_neighbors`/`graphify explain` no método alvo → quem chama; subir a cadeia até achar a origem (frame loop, tick de IA, evento discreto). Cada hop se **prova** lendo o `arquivo.cs:linha`.
3. **Classificar:** origem em `Update`/`LateUpdate`/frame loop → per-frame; em tick de IA/`BotOwner` → per-tick × N bots; em evento de input/combate → per-event frequente; em fluxo de menu/loading → frio.
4. **Contar instâncias:** o alvo é método de instância? Quantas instâncias vivas o chamam (1 player? todo `Player` incluindo bots? todo item?)? É o multiplicador do §1.2 — cuidado com patch que roda para bots/observed players sem necessidade (gate por `IsYourPlayer`/identidade — AP-02, `reference_customclasses_perk_gating` como caso real).
5. **Na dúvida, medir:** contador temporário (§6) responde em uma raid o que a estática não fecha.

---

## 4. Ciclo de vida de execução (criação → ativação → uso → desativação → cleanup)

Para **cada** sistema que executa periodicamente (componente, coroutine, timer, task, subscription, worker), preencher as cinco etapas e apontar a que falta:

| Etapa | Pergunta | Falha típica |
|---|---|---|
| Criação | Quem cria, quantas vezes? | criado de novo a cada raid/onda sem destruir o anterior → execução duplicada crescente |
| Ativação | O que liga? Pode ligar 2×? | start não-idempotente; `OnEnable → +=` sem `-=` no `OnDisable` |
| Uso | Roda no contexto certo? | ativo em menu/hideout/loading; roda para entidade morta/despawnada/distante |
| Desativação | O que desliga? A condição é alcançável? | coroutine sem condição de saída; timer nunca cancelado; componente de cadáver ainda ativo |
| Cleanup | Quem destrói no raid-end (todos os caminhos)? | teardown ausente/não-idempotente — AP-01; matriz extract/morte/MIA/alt-F4 |

Perguntas de estado de raid/IA (para mods de gameplay): há processamento sem IA ativa? Lógica de bot continua após morte/despawn? Managers percorrem listas vazias ou obsoletas? Nova raid herda estado (e custo) da anterior? — O lado **retenção** disso é a `spt-memory-leak-analysis` (LIFE/EVT/STAT); aqui o achado é o **trabalho** que continua.

---

## 5. Procedimento de investigação estática

1. **Classificar o mod** (client/server/combo — mesmos critérios da `spt-memory-leak-analysis` §5.1) e resolver a raiz do código (`modded/`, ou raiz/`src/` em mod próprio — `.agents/conventions.md`).
2. **Mapear o Panorama de execução:** toda superfície periódica/frequente do mod numa tabela — superfície → classe de frequência → multiplicador de entidades → gate de contexto → quem para/quando. Patches Harmony entram com a frequência **estimada do alvo** (§3). É o mapa que a priorização usa.
3. **Varrer com os greps do §2**, um mecanismo por vez, no escopo definido. Para cada candidato: provar a frequência real, o multiplicador, o gate e o lifecycle **lendo o código** (grafo aponta, leitura prova).
4. **Auditar a configuração** (§2 CFG) — defaults e onde cada chave entra no código.
5. **Cruzar com a memória do mod** (`sessions.md` — `memory-curation` §14) e com relatórios anteriores (`relatorio-auditoria-codigo-*`, `MEMORY-LEAK-review-*`): achado já registrado é **referenciado por ID**, não duplicado; resolvido não volta.
6. **Classificar cada achado:** mecanismo (§2) × modelo de custo (§1) → severidade; e **nível de evidência**:
   - **Evidência forte** — frequência, multiplicador e mecanismo comprovados por leitura (e/ou medição); há razão concreta para ser hot path relevante.
   - **Suspeita** — padrão preocupante com eixo não provado (frequência do alvo incerta, N desconhecido); **precisa de instrumentação/medição** antes de virar plano de refactor.
   - **Melhoria preventiva** — boa prática que não explica problema atual. Entra agregada e curta, nunca inflando o relatório (o objetivo é achar os maiores ofensores, não 80 sugestões cosméticas).

---

## 6. Instrumentação temporária de baixo overhead

Quando a estática não decide (Suspeitas), medir com instrumentação **direcionada, temporária e barata** — nunca um profiler caseiro permanente:

- **Contador agregado:** `static int`/`long` incrementado no ponto de interesse; dump **agregado** no raid-end ou a cada N segundos (nunca por chamada). Responde "quantas vezes / para quantas entidades / ainda roda depois do despawn?".
- **Stopwatch amostrado:** medir 1 chamada a cada K (`if ((_n++ & 0x3FF) != 0) return;`) com `System.Diagnostics.Stopwatch` reutilizado; acumular ticks e reportar média/máximo no dump. Responde "quanto custa cada execução?".
- **Census de vida:** contagem de instâncias/coroutines/handlers vivos logada no raid-start, no raid-end e após eventos-chave (morte/despawn/onda). Responde "duplicou? parou quando devia?" — é a versão CPU do "log de objetos vivos" da `spt-memory-leak-analysis` §8.8.
- **Tamanho de coleção:** logar `count` das coleções GROW nos mesmos pontos.

Regras invioláveis:
- **Gate por `ConfigEntry<bool>`** (ex.: `Debug.PerfInstrumentation`, default `false`), checado **antes** de qualquer formatação de string — desligada, o custo é um branch.
- **Zero alocação no caminho quente** mesmo ligada: contadores primitivos, sem LINQ, string só no dump agregado.
- **Marcada para remoção:** todo bloco leva `// PERF-INSTR AUD-NN-MM — temporary, remove after validation` e é removido (ou no mínimo mantido desligado por default e documentado em `PROPRIEDADES.md`) depois que a medição fechar o achado.
- Instrumentação também **compila e valida in-game** como qualquer mudança (AP-06) — e num mod Fika, lembrar que ela roda no headless também.

---

## 7. Validação medida (antes/depois — nada de "parece mais eficiente")

Otimização sem medição é fé. Para cada mudança aplicada, o plano de validação nomeia **métrica, cenário e critério**:

- **Cenário pareado:** mesma medição antes e depois em condição comparável — mesmo mapa, ponto de spawn próximo, contagem de bots semelhante, mesma duração. Sem par, a comparação não vale.
- **Métricas por tipo de fix:** contagem de chamadas/s e custo médio (contadores/Stopwatch do §6) · census de instâncias/coroutines/handlers (duplicação e lifecycle) · tamanho de coleções ao longo da raid (GROW) · volume de linhas de log · RSS/heap quando ALLOC/retenção (medição da `spt-memory-leak-analysis` §6) · FPS/frametime externos quando disponíveis (são a métrica mais ruidosa — usar como confirmação, não como única evidência).
- **Cenários de lifecycle:** morte/despawn de bots (o trabalho parou?), múltiplas ondas (2ª onda custa como a 1ª?), raid longa (>20 min — custo estável?), **raid1→exit→raid2** e alt-F4/morte/MIA (nova raid não herda custo — matriz do `fix.md.tmpl`), headless real quando o mod roda nele.
- **Regressão funcional:** cada fix nomeia o comportamento que deve permanecer idêntico e como conferi-lo in-game. Otimizar removendo funcionalidade sem trade-off declarado é bug, não fix.

---

## 8. Falsos positivos e o que não fazer

- **Código frio:** custo teórico em fluxo raro (menu, boot, raid-end) não é achado — LINQ no `Awake` é estilo, não performance. Frequência primeiro, elegância depois.
- **Trabalho já gated:** superfície frequente cujo topo é um early-return barato (`if (!Instantiated) return;`) já está resolvida — o achado seria a **ausência** do gate.
- **Cache intencional / retenção deliberada com limite** não é GROW (ver `spt-memory-leak-analysis` §7).
- **Baseline do EFT/SPT/Fika:** custo que existe sem o mod não é do mod — na dúvida, comparar com/sem.
- **Polling ocasionalmente é o design certo** — quando não existe evento nativo confiável para a transição (provar a ausência antes de aceitar), o fix é **cadência controlada** (throttle a 0.2–0.5s + offset), não a caça infinita ao evento.
- **Micro-otimização de código frio ilegível não entra** — o custo de manutenção supera o ganho inexistente.
- **Não** propor `GC.Collect`/`Resources.UnloadUnusedAssets`/RAM cleaner (o headless bane — skill de leak §2), threading de Unity API (`csharp-mod-best-practices` §2), nem registrar/desregistrar patch por raid (patches são globais; o gate vai no corpo).
- **Não concluir "otimizado" de compilação limpa** — AP-06: só a medição do §7 fecha um achado.

## Checklist de auditoria (usar em /optimize-mod-performance e nos reviews)

1. **Panorama existe?** Toda superfície periódica/frequente mapeada com frequência × entidades × gate × lifecycle? (§5.2)
2. **Patches dimensionados?** Todo alvo Harmony tem frequência estimada pelo §3 e early-return de contexto no topo? (PATCH)
3. **Frequência justificada?** Cada Update/timer/coroutine responde "por que essa cadência?" — e polling só onde não há evento? (FREQ)
4. **Entidades filtradas?** Loops sobre players/bots/objetos filtram para o subconjunto relevante, sem O(N²) escondido? (ENT)
5. **Lifecycle completo?** Cada sistema tem as 5 etapas do §4 — sem trabalho zumbi, sem start duplicável? (LIFE)
6. **Nada cresce sem limite?** Coleções varridas frequentemente têm remoção/limite? 2ª onda custa como a 1ª? (GROW)
7. **Unity/alloc/log/IO fora do hot path?** Operações caras cacheadas ou por evento; ALLOC via skill de leak; log gated antes de formatar? (UNITY/ALLOC/LOG/IO)
8. **Config auditada?** Chaves de frequência/raio/quantidade/logging com default são, timers dessincronizados? (CFG)
9. **Evidência nomeada:** cada achado tem os eixos do §1 + nível (Forte/Suspeita/Preventiva), Suspeita tem plano de instrumentação? (§5.6/§6)
10. **Validação medida:** cada fix tem métrica, cenário pareado e critério de regressão funcional? (§7)

Se um item falha, é achado `AUD-NN-MM` no relatório (ou 🔴 no review técnico/code-review). Confirmação final é sempre medição in-game (§7; AP-06).
