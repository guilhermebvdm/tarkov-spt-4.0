# 007 — Desmaio 2.0: gatilhos percentuais · Code Review 01

**Mod:** TRL-ImmersiveCombatMedicine
**Spec funcional:** [007-desmaio-percentual-01-spec.md](007-desmaio-percentual-01-spec.md)
**Spec técnica:** [007-desmaio-percentual-02-spec-tech.md](007-desmaio-percentual-02-spec-tech.md)
**Asbuild:** [007-desmaio-percentual-05-asbuild.md](007-desmaio-percentual-05-asbuild.md)
**Data:** 2026-07-19

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.
>
> `Memória consultada: snapshot de 2026-07-19 (Sessão 4, mods/TRL-ImmersiveCombatMedicine/memory/sessions.md) · pendências que afetam esta review: [P-3.7 — item 007 é o de MAIOR RISCO do overhaul restante, plano explícito de 2 rodadas de code-review; esta é a rodada 1], [P-2.13/P-2.14/P-2.15 — bugs HISTÓRICOS do pipeline de desmaio (recálculo de duração ao vivo deslocando o wake, guard de re-entrada quebrado, sync Fika duplicada), todos já corrigidos]. Nenhuma das 3 pendências históricas reaparece no diff: git diff confirma que TODAS as linhas que essas pendências protegem (`TraumaState.BlackoutTimers`/`BlackoutStartTimes`, guard `ContainsKey`/`FaintedPlayerIds`, `FikaBridge.SyncFaintStatus`, `TraumaState.BotFaintCooldowns`) permanecem byte-idênticas — só a CONDIÇÃO de entrada (`shouldFaint`) foi trocada, exatamente como a spec técnica prometeu.`

Revisão adversarial de contexto limpo da implementação **v1.8.0** (working tree, ainda não commitado). Escopo: os 5 arquivos do diff real (`TraumaBlackoutTrigger.cs` criado; `HealthPatches.cs`, `TRLImmersiveCombatMedicinePlugin.cs`, `TRL-ImmersiveCombatMedicine.csproj`, `PROPRIEDADES.md` modificados — confirmados via `git diff` contra HEAD, batendo linha a linha com a lista do asbuild) contra a spec técnica pós-2-rodadas (PA-01-01/02/03 e PA-02-01/02/03/04/05 já resolvidos na spec ANTES do build) e o Assembly real (`references/eft-decompiled/Assembly-CSharp/EFT/Player.cs`).

**Nota de escopo:** `mod-backlog.md` também aparece modificado (status 007 ⚪→🟢, esperado do `/code-mod`). `docs/spt4-items-inventory-hideout.md` e outros arquivos fora de `mods/TRL-ImmersiveCombatMedicine/` presentes no `git status` do repo (`TRL-ItemsManagement`, `launcher/`) são de sessões paralelas — fora de escopo, não avaliados.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 2 · Total: 2

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | E — Legibilidade/manutenção | 🟠 Forte | Evidência "reforçada" da spec técnica (§7, PA-02-01) sobre prioridade Harmony está factualmente invertida | ✅ Aplicado |
| CR-01-02 | F — Melhoria opcional | 🟢 Menor | Captura de `__state` roda mesmo com o motor/gatilho totalmente desligado | ✅ Aplicado |

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

## Verificação de evidências

Todas as âncoras `arquivo.cs:linha` citadas pela spec técnica (§1, §2, §7) foram reconferidas contra o Assembly real (não só contra o texto reportado como "já verificado"): `Player.cs:30463` (assinatura de `ApplyDamageInfo`, `virtual`), `:30475` (`DoWoundRelapse`, sem mutação de HP), `:30480` (`ActiveHealthController.ApplyDamage` — mutação real do HP), `:25291` (`ActiveHealthController` property) — todas batem exatamente. `GetBodyPartHealth(EBodyPart, bool rounded = false)` (assinatura usada por `TraumaBlackoutTrigger.cs:29` e `HealthPatches.cs:26`) foi cross-checada contra usos REAIS no dump (`ActorDataStruct.cs:772-785`, `Player.cs:19563`, `:25060`, `BotFirstAidClass.cs:287/312/416`) — evidência independente e mais forte do que a já citada pela spec (`docs/trauma-primitives.md §P7`, protótipo compilado), confirmando o padrão de chamada como genuíno. Nenhum achado de Categoria A (âncora quebrada). Zero `GClassNNNN`/`GStructNNNN` novo no diff (regra AP-09 não se aplica — `Player`/`EBodyPart`/`ActiveHealthController` são nomes estáveis, sem entrada no mapa de deofuscação por não serem ofuscados). Grafo do mod regenerado conferido: `graphify-out/graph.json` tem exatamente **835 nós / 1381 links**, batendo com o valor reportado no as-built.

Adicionalmente (fora do escopo normal de "citações da spec", mas motivado pela criticidade do item): decompilei via `ilspycmd` o `0Harmony.dll` (HarmonyX 2.7.0, `~/.nuget/packages/harmonyx/2.7.0/lib/netstandard2.0/`, a mesma lib que este BepInEx/SPT 4.0 usa) e os DLLs reais de `BringBackConcussion`/`VisceralCombat` (`launcher/Launcher4.0-v2/project/SPT.Launcher/bin/Release/net9.0/win-x64/publish/BepInEx/plugins/`) para verificar de forma independente a prova de segurança entre mods que a spec técnica (§7, PA-02-01) alega ter "endurecido" na rodada 2 — resultado no achado CR-01-01 abaixo.

---

## Verificações adicionais (limpo em)

- **Captura de HP pré-hit (foco #1 do Passo 4):** `HealthPatches.cs:22-27` — `__state = -1f;` seguido de `if (bodyPartType == Chest || Head) { ahc.GetBodyPartHealth(bodyPartType).Current }` roda no **Prefix**, ANTES de qualquer outro código do patch e, estruturalmente, antes do corpo original de `ApplyDamageInfo` (`Player.cs:30480`, onde `ActiveHealthController.ApplyDamage` muta o HP). Não é aritmética reversa — é exatamente a captura direta que a spec técnica decidiu (e não a alternativa descartada `postHp + damageInfo.Damage`). Confirmado.
- **Filtro `bodyPartType` explícito no Postfix (PA-02-05, foco #2):** `HealthPatches.cs:79-82` — `bool shouldFaint = isValidTraumaType && (bodyPartType == EBodyPart.Chest || bodyPartType == EBodyPart.Head) && ConfigConsumerBlackout2.Value && TraumaBlackoutTrigger.Evaluate(...)`. O filtro de domínio está ANTES da chamada a `Evaluate`, exatamente como a review técnica 02 exigiu — o `else { return false; }` dentro de `Evaluate` (`TraumaBlackoutTrigger.cs:52-55`) agora é puramente defensivo, não o único mecanismo. Confirmado.
- **Nomes de campo C# exatos (foco #3):** `ConfigBlackoutChestPercent`/`ConfigBlackoutHeadPercent`/`ConfigBlackoutChestAbsoluteFloor`/`ConfigBlackoutHeadAbsoluteFloor` usados literalmente, sem variação, tanto na declaração (`TRLImmersiveCombatMedicinePlugin.cs:74-77` campos + `:219-228` `Config.Bind`) quanto no consumo (`TraumaBlackoutTrigger.cs:42-43`, `:48-49`). `grep` cruzado confirma zero ocorrência de um nome alternativo (`ConfigChestFaintPercentThreshold` etc., a armadilha que PA-02-02 preveniu). Confirmado.
- **Zero regressão no pipeline pós-gatilho (foco #4):** `git diff` de `HealthPatches.cs` isolado ao bloco `if (shouldFaint) { ... }` mostra que `TraumaState.BlackoutTimers[id]`, `TraumaState.BlackoutStartTimes[id]`, `Physical.Stamina.Current = 0f`, `MovementContext.IsInPronePose = true`, `firearm.SetAim(false)` e `FikaBridge.SyncFaintStatus(__instance, true)` são as MESMAS linhas, byte-idênticas, só reindentadas (a única mudança estrutural foi achatar `if (isValidTraumaType) { if (isChestTrauma||isHeadTrauma) {...} }` em `if (shouldFaint) {...}`, semanticamente equivalente por short-circuit). O guard de re-entrada (`TraumaState.BlackoutTimers.ContainsKey(id) || TraumaState.FaintedPlayerIds.Contains(id)) return;` e o cooldown de bot (`BotFaintCooldowns.TryGetValue(...)`) também são idênticos, ANTES do cálculo de `shouldFaint` — nenhuma das 3 pendências históricas (P-2.13 relógio/grace, P-2.14 consumo, P-2.15 guard/sync) tem qualquer linha tocada. Confirmado.
- **Migração de config órfã (foco #5):** `TRLImmersiveCombatMedicinePlugin.cs:457-478` (bloco novo) replica literalmente o padrão de `:434-455` (bloco "Stomach Effects" do item 006, citado pela spec como template): mesma sequência `foreach (DictionaryEntry entry in orphans) { AccessTools.Property(...) → match section+key → orphans.Remove(def) → Config.Save() → LogWarning }`, delete-antes-do-save, sem copiar valor (lição CR-03-01). Confirmado idêntico em estrutura, só os literais (`"Blackout 2.0 (item 007)"`/`"Blackout 2.0"`) trocados.
- **`PROPRIEDADES.md` (foco #6):** `git diff` confirma as DUAS mudanças exigidas pela spec: (a) seção nova `## Seção 11. Trauma 2.0 (Desmaio)` com as 4 entries; (b) a linha JÁ EXISTENTE `Blackout 2.0 (item 007)` na seção 6 atualizada (nome → `Blackout 2.0`, default `false`→`true`, tooltip placeholder → tooltip real), mais a linha na tabela "Renomeadas" e a entrada no Histórico de Alterações. Confirmado — não só uma das duas.
- **Versão (foco #7):** `TRL-ImmersiveCombatMedicine.csproj:7` → `<Version>1.8.0</Version>` (era 1.7.0); `TRLImmersiveCombatMedicinePlugin.cs:17` → `[BepInPlugin(..., "1.8.0")]`; log do `Awake` (`:83`) também `"v1.8.0"`. Os três pontos sincronizados. Confirmado.
- **Namespace do arquivo novo:** `TraumaBlackoutTrigger.cs` usa `namespace TRLImmersiveCombatMedicine.Trauma` — consistente com a convenção adotada pelos consumidores "Trauma*" mais recentes (`TraumaStomachConsumer`, `TraumaArmsConsumer`, `TraumaEngine`), não o namespace legado `TrueTrauma` de `HealthPatches.cs`/`TraumaState.cs`. Correto, sem achado.
- **Csproj/compilação:** projeto é SDK-style sem `<Compile Include>` explícito — glob automático inclui `TraumaBlackoutTrigger.cs` sem edição manual do `.csproj` necessária. Consistente com o relato de "0 erros, mesmos 10 warnings pré-existentes".
- **Fika/AP-02/AP-03:** não re-auditado nesta rodada além do que a spec já documentou (§9 checks 2/3) — nenhuma mudança de patch target ou de guard de ownership neste diff (só a condição interna do Postfix mudou), portanto a auditoria de overrides da spec técnica permanece válida sem necessidade de nova verificação.

---

## Pontos

### CR-01-01 · E — Legibilidade/manutenção · 🟠 Forte · ✅ Aplicado em 2026-07-19

**Evidência "reforçada" da spec técnica (§7, PA-02-01) sobre prioridade Harmony está factualmente invertida**

**Local:** [`mods/TRL-ImmersiveCombatMedicine/backlog/007-desmaio-percentual/007-desmaio-percentual-02-spec-tech.md` §7](../007-desmaio-percentual-02-spec-tech.md), parágrafo PA-02-01 — evidência que sustenta a segurança da captura de `__state` em [`mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/HealthPatches.cs:14`](../../modded/Patches/Trauma/HealthPatches.cs#L14) (`[HarmonyPriority(Priority.High)]`).

**Problema:** A spec técnica (§7), na rodada 2, reforçou deliberadamente a "decisão técnica central" do item (que nosso Prefix é o PRIMEIRO código a ler o HP da parte) com uma "prova real por decompile":

> *"`BringBackConcussion.Patches.ConcussionPatch.PatchPrefix` é `void`, **sem** `[HarmonyPriority]` (prioridade padrão `Normal`=400, número MAIOR que o `High`=200 do nosso Prefix, logo o NOSSO sempre roda primeiro)"*

Decompilei o `0Harmony.dll` real (HarmonyX 2.7.0, `~/.nuget/packages/harmonyx/2.7.0/lib/netstandard2.0/0Harmony.dll` — a mesma lib usada por este BepInEx/SPT 4.0) via `ilspycmd -t HarmonyLib.Priority`:

```csharp
public static class Priority
{
    public const int Last = 0;
    public const int VeryLow = 100;
    public const int Low = 200;              // <- NÃO é High=200 como a spec afirma
    public const int LowerThanNormal = 300;
    public const int Normal = 400;
    public const int HigherThanNormal = 500;
    public const int High = 600;             // <- valor REAL de Priority.High
    public const int VeryHigh = 700;
    public const int First = 800;
}
```

`Priority.High` é **600**, não 200 — o valor 200 citado pela spec é na verdade `Priority.Low`. Recompilei a mesma classe a partir do `0Harmony.dll` REALMENTE implantado (`D:\SPT\BepInEx\core\0Harmony.dll`, não só o pacote NuGet de compilação) — valores idênticos, confirmando que é a mesma versão em compile-time e runtime. Além disso, a REGRA de ordenação que a spec descreve ("número MENOR = prioridade MAIOR") está **invertida**: decompilei `HarmonyLib.PatchInfoSerialization.PriorityComparer` (usado por `PatchSorter.CompareTo`) e o comparador é `return -priority.CompareTo(value);` — isto é, quanto MAIOR o valor numérico de prioridade de um patch, mais CEDO ele é ordenado entre os Prefixes do mesmo alvo (`Priority.First = 800` = "Patch first"; `Priority.Last = 0` = "Patch last", conforme os próprios doc-comments do enum). A regra real do HarmonyX é o OPOSTO do que a spec técnica documenta.

Também decompilei o DLL real de `BringBackConcussion` (`launcher/Launcher4.0-v2/project/SPT.Launcher/bin/Release/net9.0/win-x64/publish/BepInEx/plugins/BringBackConcussion.dll`) para conferir a outra metade da alegação: `ConcussionPatch : ModulePatch` (framework `SPT.Reflection.Patching`, não `HarmonyLib` diretamente) com `[PatchPrefix] public static void PatchPrefix(ref EBodyPart bodyPartType, ref DamageInfoStruct damageInfo, ref Player __instance)`, sem prioridade explícita — plausivelmente resolvida para `Priority.Normal` (400) pelo `HarmonyMethod` default quando não especificada, então essa metade da alegação (BBC roda em `Normal`=400) está correta.

**Coincidência que salva a conclusão prática:** usando os valores REAIS (nosso `High`=600 vs. `Normal`=400 do BBC) com a regra REAL (maior valor executa primeiro entre Prefixes), o nosso Prefix (600) AINDA roda antes do de BBC (400) — a conclusão final da spec ("nosso sempre roda primeiro") permanece verdadeira. Mas chegou lá por dois erros que se cancelam (constante errada + regra invertida), não pela prova que o texto alega ter feito.

**Por que importa:** Este é o item de MAIOR RISCO do overhaul (P-3.7) e esta é exatamente a "prova real" que a rodada 2 de review técnica (PA-02-01) foi desenhada para produzir, substituindo uma garantia genérica por uma garantia específica e verificada — mas a verificação, como escrita, está factualmente errada em dois pontos independentes. Se um item futuro (008, 011) ou uma auditoria de compatibilidade com um NOVO mod copiar este trecho como referência de "como o HarmonyX ordena prioridades", um mantenedor que confie na regra invertida ("número menor = prioridade maior") poderia atribuir a um patch futuro uma prioridade BAIXA pensando estar tornando-o "alto", e esse patch rodaria DEPOIS do que deveria — silenciosamente reabrindo exatamente a classe de bug (mutação de HP antes da nossa captura) que este item inteiro foi desenhado para descartar. É a mesma categoria de risco de timing/ordenação que as pendências históricas P-2.13/14/15 (memória do mod) alertam a vigiar neste pipeline específico — não uma reincidência literal delas, mas o mesmo padrão de risco (premissa de ordenação de execução não verificada corretamente).

**Sugestão:** Corrigir o parágrafo §7/PA-02-01 da spec técnica: trocar "`Priority.High`=200" por **600**, "`Priority.Normal`=400, MENOR que" por "**400, menor que os 600 do nosso Prefix**", e a frase-regra por: *"a regra real do HarmonyX (`PatchInfoSerialization.PriorityComparer`, decompilado de `0Harmony.dll` 2.7.0) é que o MAIOR valor numérico de prioridade executa PRIMEIRO entre Prefixes do mesmo alvo — nosso Prefix (`Priority.High`=600) roda antes do de BBC (`Priority.Normal`=400 por default) por essa razão."* Nenhuma mudança de código é necessária — `HealthPatches.cs:14` já usa a constante nomeada `Priority.High` corretamente (sem literal numérico incorreto no `.cs`, só na prosa da spec). Opcionalmente, adicionar um comentário curto no próprio Prefix (`HealthPatches.cs`) lembrando "maior valor numérico = executa primeiro entre Prefixes" para blindar contra o mesmo erro de leitura em revisões futuras deste patch.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Spec técnica §7 corrigida — `Priority.High` = 600 (não 200), regra real do HarmonyX (maior valor = executa primeiro entre Prefixes) documentada com a fonte do decompile.

**Aplicação:** `mods/TRL-ImmersiveCombatMedicine/backlog/007-desmaio-percentual/007-desmaio-percentual-02-spec-tech.md` §7. Nenhuma mudança de código (a constante nomeada `Priority.High` já estava correta no `.cs`).

---

### CR-01-02 · F — Melhoria opcional · 🟢 Menor · ✅ Aplicado em 2026-07-19

**Captura de `__state` roda mesmo com o motor/gatilho totalmente desligado**

**Local:** [`mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/HealthPatches.cs:22-29`](../../modded/Patches/Trauma/HealthPatches.cs#L22)

**Problema:**

```csharp
__state = -1f;
if (bodyPartType == EBodyPart.Chest || bodyPartType == EBodyPart.Head)
{
    var ahc = __instance?.ActiveHealthController; // ref: Player.cs:25291
    if (ahc != null) __state = ahc.GetBodyPartHealth(bodyPartType).Current; // ref: docs/trauma-primitives.md §P7 ...
}

if (!TRLImmersiveCombatMedicinePlugin.ConfigMasterEnabled.Value) return true;
```

A captura de HP pré-hit roda incondicionalmente para todo hit em Chest/Head, ANTES de qualquer checagem de `ConfigMasterEnabled`/`ConfigBlackoutEnabled`/`ConfigConsumerBlackout2`. Mesmo com o mod inteiro desligado (master OFF) ou com o sub-toggle "Blackout 2.0" OFF, todo hit em tórax/cabeça paga uma chamada a `GetBodyPartHealth` (property lookup + acesso a `ActiveHealthController`) cujo resultado nunca será consumido (o Postfix bail-out em `ConfigMasterEnabled.Value` acontece antes de qualquer uso de `__state`).

**Por que importa:** Custo desprezível na prática — `ApplyDamageInfo` roda por-hit (evento de dano), não por-frame, e a chamada em si é trivial (getter, sem alocação) — não é um caminho quente no sentido de `spt-mod-best-practices` §3. Não há sintoma observável hoje; é puramente uma oportunidade de simplificação.

**Sugestão:** Se desejado, mover a leitura de `__state` para depois do check `if (!ConfigMasterEnabled.Value) return true;` (ainda dentro do Prefix, antes do corpo original rodar — a garantia de "captura antes da mutação" não depende de ONDE dentro do Prefix a leitura acontece, só de acontecer antes do retorno `true` que deixa o corpo original executar). Não bloqueia nada — puramente uma melhoria de clareza/eficiência opcional.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Captura de `__state` movida para depois do gate `ConfigMasterEnabled`/`IsAlive` no Prefix; `__instance.ActiveHealthController` sem `?.` (já garantido não-null nesse ponto).

**Aplicação:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/HealthPatches.cs` (Prefix reordenado).

---

## Veredito

Implementação **fiel à spec técnica pós-2-rodadas**: todos os 7 focos do Passo 4 (captura de HP pré-hit correta, filtro `bodyPartType` explícito no Postfix, nomes de campo C# exatos, zero regressão no pipeline pós-gatilho, migração de config órfã fiel ao padrão, `PROPRIEDADES.md` com as duas mudanças, versão sincronizada em csproj+`BepInPlugin`) foram confirmados via `git diff` linha a linha e verificação direta do Assembly. Nenhuma das 3 pendências históricas do pipeline de desmaio (P-2.13/P-2.14/P-2.15) foi reintroduzida — o diff toca exclusivamente a condição de entrada (`shouldFaint`), preservando byte-a-byte tudo que vem depois (`BlackoutTimers`, `BlackoutStartTimes`, `FikaBridge.SyncFaintStatus`, guards de re-entrada, `BotFaintCooldowns`).

O único achado de peso (CR-01-01, 🟠) não é um bug de comportamento — é um erro factual na EVIDÊNCIA que a spec técnica usa para justificar a decisão mais crítica do item, que por coincidência (dois erros que se cancelam) ainda chega à conclusão prática correta. Dado que este é o item de maior risco do overhaul e que a rodada 2 de review técnica foi desenhada especificamente para "endurecer" essa prova, recomendo corrigir antes de fechar — é uma edição de texto, sem risco de regressão de código. CR-01-02 é puramente opcional.

**Sem bloqueadores 🔴** — o item PODE ser fechado tecnicamente, mas dado o perfil de risco declarado (P-3.7, 2 rodadas de code-review planejadas), recomendo aplicar CR-01-01 e prosseguir para a rodada 2 do code-review conforme planejado, em vez de fechar após a rodada 1.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-19 | Code review 01 criada via `/code-review` (revisor adversarial de contexto limpo): 0 🔴 · 1 🟠 · 0 🟡 · 1 🟢. Verificação linha-a-linha do diff real (`git diff` vs HEAD) contra a spec técnica pós-2-rodadas e o Assembly real (`Player.cs`), incluindo decompile independente do `0Harmony.dll` (HarmonyX) e dos DLLs reais de `BringBackConcussion`/`VisceralCombat` para reauditar a prova de segurança entre mods citada em PA-02-01. Achados: CR-01-01 (valores/regra de `HarmonyLib.Priority` citados na spec estão invertidos — conclusão prática ainda correta por coincidência) e CR-01-02 (captura de `__state` roda mesmo com toggles desligados — otimização opcional). |
