# 007 — Desmaio 2.0: gatilhos percentuais · Review Técnica 02

**Mod:** TRL-ImmersiveCombatMedicine
**Spec técnica revisada:** [007-desmaio-percentual-02-spec-tech.md](007-desmaio-percentual-02-spec-tech.md)
**Data:** 2026-07-19

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-02-MM` (review 02, ponto MM). Resolver até zerar bloqueadores antes de `/code-mod`.
>
> Memória consultada: snapshot de 2026-07-19 (Sessão 4, `mods/TRL-ImmersiveCombatMedicine/memory/sessions.md`). Pendências P-2.13/P-2.14/P-2.15 revisadas como barra de rigor (bugs históricos de timing/sincronização do pipeline de desmaio) — nenhuma reintroduzida; a spec não toca `BlackoutTimers`/`BlackoutStartTimes`/`FikaBridge.SyncFaintStatus`/`ConfigBlackoutDuration` fora do ponto único já existente. P-3.7 confirma este é o item de maior risco do overhaul restante — esta é a **rodada 2, planejada como a ÚLTIMA**. Todos os 3 achados da rodada 1 (PA-01-01/02/03) foram conferidos contra a spec técnica ATUAL (não só contra a resolução relatada): os três estão de fato corrigidos no texto (§2 cita `docs/trauma-primitives.md §P7` em vez de `GClass921`; §6 passo [D] tem a nota sobre `Evaluate` nunca ser invocado quando o escudo bloqueia; §7 tem o parágrafo do corner "hit simultâneo tórax+cabeça", e o corner correspondente está `[x]` na `01-spec.md`) — nenhum reaberto.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 5 · Total: 5

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-02-01 | A — Gap | 🟡 Importante | Evidência de segurança entre mods (BBC/VC) verifica a garantia errada — a ordem/mutação real de prefixes não foi auditada por decompile | ✅ Resolvido |
| PA-02-02 | A — Gap | 🟡 Importante | Nomes de variável C# das 4 `ConfigEntry` novas não aparecem em nenhuma tabela — só dedutíveis lendo o stub de `TraumaBlackoutTrigger.cs` | ✅ Resolvido |
| PA-02-03 | A — Gap | 🟢 Menor | Bloco novo de `MigrateOrphanedConfigKeys` sem citação de linha do template a replicar (rigor abaixo do 005/006) | ✅ Resolvido |
| PA-02-04 | A — Gap | 🟢 Menor | `PROPRIEDADES.md` §4 não menciona a atualização da linha existente "Blackout 2.0 (item 007)" na seção 6 | ✅ Resolvido |
| PA-02-05 | A — Gap | 🟢 Menor | Postfix chama `Evaluate` para QUALQUER `bodyPartType`; o filtro de domínio (tórax/cabeça) dentro de `Evaluate` é código morto na prática | ✅ Resolvido |

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

### PA-02-01 · A — Gap · 🟡 Importante

**Evidência de segurança entre mods (BBC/VC) verifica a garantia errada, não a que a spec precisa**

**Problema:** A "decisão técnica central" da spec (§1) depende inteiramente de uma premissa: **o Prefix deste mod é o PRIMEIRO código a ler `GetBodyPartHealth` — nada mutou o HP da parte ainda quando `__state` é capturado.** A spec (§7) e a review 01 ("Pontos verificados sem achado") tratam essa premissa como garantida citando que "Harmony gera o local de `__state` por método de patch (não é campo compartilhado entre patches de mods diferentes no mesmo alvo)". Essa é uma garantia REAL, mas responde a uma pergunta diferente (isolamento da variável `__state` entre os Prefixes de mods distintos) da que a estratégia central precisa (se o CORPO de um Prefix de terceiro, rodando ANTES do nosso por prioridade, muta o HP da parte antes de nós lermos). Nem a spec nem a review 01 auditaram por decompile o corpo real dos dois mods citados (`BringBackConcussion.dll`, `VisceralCombat.dll`) para essa pergunta específica — a citação de `docs/trauma-primitives.md §P7` que a spec usa em outros pontos foi produzida para uma pergunta diferente (domínio de tremor, item 005), não para mutação de HP em `ApplyDamageInfo`.
Fiz essa auditoria agora (decompile via `ilspycmd` dos DLLs instalados, `launcher/Launcher4.0-v2/.../BepInEx/plugins/{BringBackConcussion,VisceralCombat}.dll`, mesmos binários já usados como evidência no P7):
- `BringBackConcussion.Patches.ConcussionPatch` usa `[PatchPrefix] public static void PatchPrefix(...)` (framework `ModulePatch` do SPT-Aki) — **sem** `[HarmonyPriority]` declarado, portanto registrado com a prioridade padrão do Harmony (`Priority.Normal` = 400). O corpo inteiro (~55 linhas) só chama `activeHealthController.DoContusion(...)`, `.DoStun(...)` e emissão de som/efeito visual — **zero** referência a `GetBodyPartHealth`/`ChangeHealth`/qualquer setter de HP por parte. Como nosso Prefix é `Priority.High` (200 — número MENOR = prioridade MAIOR no Harmony), o nosso roda ANTES do de BBC de qualquer forma; e mesmo que rodasse depois, BBC não muta o HP da parte.
- `VisceralCombat` tem **dois** patches em `Player.ApplyDamageInfo` (`GetTargetMethod` retorna o mesmo `MethodBase` duas vezes, ambos com `[PatchPostfix]` — nenhum `[PatchPrefix]` nesse alvo). Postfixes sempre rodam DEPOIS do corpo original (que já rodou depois de TODOS os Prefixes) — não podem interferir na captura de `__state`, que acontece antes de qualquer Postfix.
Conclusão: a premissa da spec SE MANTÉM correta — mas nem a spec nem a review 01 escreveram essa prova; escreveram uma prova de uma alegação diferente e a apresentaram como resposta a esta.

**Por que importa:** Este é o item de maior risco do overhaul (P-3.7) e a "decisão técnica central" (§1) é o pilar que sustenta toda a estratégia do patch. Se um item futuro (008, 011) ou uma instalação com um mod de combate adicional reabrir esta pergunta, o leitor não encontra na spec nem na review 01 a evidência real (só a isolação de `__state`, irrelevante para mutação de HP) — precisaria redescobrir por decompile do zero, exatamente como eu fiz agora. Também é o tipo de lacuna que, se um novo mod de terceiros for adicionado à instalação com um Prefix `bool`-retornante de prioridade MAIOR que `Priority.High` que de fato mute HP da parte (cenário hipotético, não o caso hoje), quebraria silenciosamente a premissa sem que ninguém tivesse uma seção da spec apontando o que checar.

**Sugestão:** Substituir, em §7, a frase que hoje só afirma "nenhum cancela o original" (evidência de SKIP, não de mutação) por um parágrafo citando a prova real: "`BringBackConcussion.Patches.ConcussionPatch.PatchPrefix` (decompile do DLL instalado) é `void`, sem `[HarmonyPriority]` (prioridade padrão `Normal`=400, MENOR que o `High`=200 do nosso Prefix — o nosso sempre roda primeiro) e seu corpo só chama `DoContusion`/`DoStun`/emissão de som — nunca toca `GetBodyPartHealth`/HP de parte, mesmo que rodasse depois. `VisceralCombat` tem 2 Postfixes em `ApplyDamageInfo` e ZERO Prefixes nesse alvo — não pode interferir na captura de `__state` (que ocorre antes de qualquer Postfix)." Isso troca uma citação que prova a garantia ERRADA por uma que prova a garantia CERTA, e blinda specs futuras (008/011) que reusarem este trecho.

**Decisão:** `[x]` Aceitar sugestão

**Resolução:** §7 atualizado com o parágrafo de prova real (decompile BBC/VC).

---

### PA-02-02 · A — Gap · 🟡 Importante

**Nomes de variável C# das 4 `ConfigEntry` novas não aparecem em nenhuma tabela — só dedutíveis lendo o stub de `TraumaBlackoutTrigger.cs`**

**Problema:** A tabela §3 lista as 4 entradas novas só pelo **Nome (EN)** exibido no F12 (`Chest Faint Percent Threshold`, `Head Faint Percent Threshold`, `Chest Faint Absolute Damage Floor`, `Head Faint Absolute Damage Floor`) — a coluna que, nos itens 003-006, sempre corresponde ao 2º argumento de `Config.Bind(section, key, ...)`. Em NENHUM lugar da spec (§3, §4, §8 checklist) aparece o nome do CAMPO C# (`ConfigEntry<float>`) que deve ser declarado em `TRLImmersiveCombatMedicinePlugin.cs` para cada uma. Os nomes de campo só aparecem, de passagem, dentro do stub de `TraumaBlackoutTrigger.cs` em §5 (`ConfigBlackoutChestPercent`, `ConfigBlackoutHeadPercent`, `ConfigBlackoutChestAbsoluteFloor`, `ConfigBlackoutHeadAbsoluteFloor`) — e esses nomes **não seguem** o padrão "CamelCase literal do Nome (EN)" que o resto do arquivo usa (ex.: item 006, `Stomach Crouch Chance Percent` → `ConfigStomachCrouchChancePercent`, correspondência 1:1 óbvia). Aqui, `Chest Faint Percent Threshold` → `ConfigBlackoutChestPercent` NÃO é uma tradução literal (some "Faint", some "Threshold", aparece "Blackout" que não está no nome exibido) — só é descobrível cruzando §3 com o corpo de §5, e nenhuma tabela declara o bind (`Config.Bind("11. Trauma 2.0 (Desmaio)", "Chest Faint Percent Threshold", 50f, ...)`) com o nome do campo ao lado, ao contrário de TODOS os itens anteriores (ex.: 006 §5: `ConfigStomachCrouchChancePercent = Config.Bind("10. Trauma 2.0 (Estômago)", "Stomach Crouch Chance Percent", 75f, ...)` — par nome-de-campo/nome-exibido no MESMO lugar).

**Por que importa:** É exatamente o tipo de erro de implementação que a diretiva do usuário nomeou como "fonte comum de erro" (Passo 3, item 3 desta rodada). Um implementador que siga só §3 (a tabela "oficial" de configs) e §8 (checklist: "Adicionar as 4 `ConfigEntry<float>` novas") sem notar os nomes usados dentro do stub de `TraumaBlackoutTrigger.cs` pode nomear os campos de forma "óbvia" mas DIFERENTE (ex.: `ConfigChestFaintPercentThreshold`), o que quebra a compilação assim que `TraumaBlackoutTrigger.cs` (copiado literalmente do stub §5) referenciar `TRLImmersiveCombatMedicinePlugin.ConfigBlackoutChestPercent` — um campo que não existe. Não é um bloqueador (o erro apareceria na hora de compilar, não silenciosamente), mas é retrabalho evitável e uma inconsistência que os itens 003-006 nunca tiveram.

**Sugestão:** Adicionar à tabela §3 (ou a uma nova linha logo abaixo dela) a coluna/mapeamento explícito campo↔chave, no formato usado implicitamente pelos itens anteriores — por exemplo, uma coluna "Campo C#" com `ConfigBlackoutChestPercent`, `ConfigBlackoutHeadPercent`, `ConfigBlackoutChestAbsoluteFloor`, `ConfigBlackoutHeadAbsoluteFloor` ao lado de cada "Nome (EN)" respectivo — os MESMOS 4 nomes já usados no stub §5, só tornando a correspondência explícita em vez de implícita.

**Decisão:** `[x]` Aceitar sugestão

**Resolução:** Coluna "Campo C#" adicionada à tabela §3, com os 4 nomes de campo ao lado dos respectivos Nome (EN).

---

### PA-02-03 · A — Gap · 🟢 Menor

**Bloco novo de `MigrateOrphanedConfigKeys` sem citação de linha do template a replicar**

**Problema:** A spec (§4, linha do arquivo `TRLImmersiveCombatMedicinePlugin.cs`; §8, item do checklist) descreve o 5º bloco de `MigrateOrphanedConfigKeys` só em prosa: *"bloco novo em `MigrateOrphanedConfigKeys` deletando o órfão 'Blackout 2.0 (item 007)'"* / *"padrão CR-03-01/003-006"*. Comparado ao rigor real dos itens anteriores: a spec técnica do 005 cita explicitamente `Plugin:290-312` (bloco do 003) e `Plugin:314-334` (template "mais novo", bloco do 004) como o literal a replicar; a spec técnica do 006 vai além e escreve o pseudo-stub completo do bloco (comentário com a condição `section == "..." && key == "..."`, `orphans.Remove` + `Config.Save()` + a mensagem exata do `LogWarning`, citando `Plugin:361-382` como o padrão literal do 005). A spec do 007 não cita nenhuma linha do bloco mais recente (o de "Stomach Effects (item 006)", hoje em `TRLImmersiveCombatMedicinePlugin.cs:407-428`, conferido nesta rodada) nem escreve o pseudo-stub — só o nome da key órfã e a referência genérica "padrão 003-006".

**Por que importa:** O padrão é mecanicamente simples e já existe 4 vezes no arquivo real (confirmado nesta rodada, linhas 338-428) — o risco de erro é baixo. Mas é uma queda de rigor real e mensurável em relação ao 005/006 (que citam linha exata do bloco-molde), na única seção de código deste item que NÃO tem stub explícito em §5 (ao contrário do Prefix/Postfix/`TraumaBlackoutTrigger.cs`, que têm blocos completos). Um implementador apressado poderia copiar o bloco errado (ex.: usar a key errada "Blackout 2.0 (item 007)" só parcialmente, ou esquecer o `Config.Save()` antes do `Remove`, invertendo a ordem que a lição CR-03-01 já corrigiu uma vez).

**Sugestão:** Adicionar a §4 (ou a uma nota em §8) a citação da linha exata do bloco mais recente a replicar: `TRLImmersiveCombatMedicinePlugin.cs:407-428` (bloco "Stomach Effects (item 006)", o "template literal mais novo"), com a troca mecânica de `section`/`key`/mensagens de log para `"Blackout 2.0 (item 007)"` → `"Blackout 2.0"` — no mesmo formato que a spec do 005 usou para apontar para o bloco do 004.

**Decisão:** `[x]` Aceitar sugestão

**Resolução:** §4 e §8 atualizados com a citação `Plugin.cs:407-428` (bloco "Stomach Effects" como template literal).

---

### PA-02-04 · A — Gap · 🟢 Menor

**`PROPRIEDADES.md` §4 não menciona a atualização da linha existente "Blackout 2.0 (item 007)" na seção 6**

**Problema:** A tabela §4 da spec técnica descreve a mudança em `PROPRIEDADES.md` como: *"Seção 11 nova (4 entries); linha nova na tabela 'Renomeadas' (`Blackout 2.0 (item 007)` → `Blackout 2.0`)"* — e o checklist §8 repete a mesma dupla ação. Comparando com o padrão real do item 006 (§4 da spec técnica do 006, conferido nesta rodada): *"Seção 10 nova; INERTE na seção 2; **tooltip real do `Stomach Effects` na seção 6**; linha na tabela Renomeadas"* — o 006 lista explicitamente a atualização da linha JÁ EXISTENTE na seção 6 (`Stomach Effects (item 006)` → default `false`/tooltip placeholder vira `Stomach Effects` → default `true`/tooltip real), que de fato foi aplicada em `PROPRIEDADES.md:70` (linha real, conferida nesta rodada: `| Stomach Effects | bool | true (rename do placeholder, era false) | ... |`). A spec do 007 não lista essa mesma ação — hoje `PROPRIEDADES.md:71` tem a linha `| Blackout 2.0 (item 007) | bool | false | — | — | Placeholder — desmaio percentual. Sem função até o item 007... |`, que precisa da MESMA transformação (nome, default, tooltip) e não está mencionada em nenhuma lista de ações da spec técnica.

**Por que importa:** É puramente documentação (não afeta o código/compilação), mas é o mesmo tipo de omissão que o padrão do repo (`repo-workflow-best-practices` §7) trata como gate de entrega — `PROPRIEDADES.md` é a fonte única de verdade das `ConfigEntry`. Sem a instrução explícita, a linha da seção 6 pode ficar desatualizada (nome antigo + tooltip placeholder) mesmo depois da seção 11 nova e da tabela Renomeadas serem escritas corretamente — um leitor do F12 veria dois textos conflitantes: o tooltip real no jogo (do `Config.Bind` novo) e o tooltip placeholder ainda em `PROPRIEDADES.md`.

**Sugestão:** Adicionar à célula "Resumo" da linha `PROPRIEDADES.md` em §4 (e ao item correspondente do checklist §8): "...; linha existente `Blackout 2.0 (item 007)` na seção 6 atualizada (nome → `Blackout 2.0`, padrão `false`→`true`, tooltip placeholder → tooltip real do `Config.Bind`) — mesmo padrão do `Stomach Effects` no item 006."

**Decisão:** `[x]` Aceitar sugestão

**Resolução:** §4 e §8 atualizados mencionando a atualização da linha existente da seção 6 do `PROPRIEDADES.md`.

---

### PA-02-05 · A — Gap · 🟢 Menor

**Postfix chama `Evaluate` para QUALQUER `bodyPartType`; o filtro de domínio (tórax/cabeça) dentro de `Evaluate` é código morto na prática**

**Problema:** O stub do Postfix (§5) chama `TraumaBlackoutTrigger.Evaluate(__instance, bodyPartType, __state)` gated só por `isValidTraumaType` (filtro de TIPO de dano) e `ConfigConsumerBlackout2.Value` — sem nenhum filtro por `bodyPartType` (Chest/Head) antes da chamada. O filtro de domínio existe DENTRO de `Evaluate` (o `if (Chest) ... else if (Head) ... else return false;` no fim do stub, comentado "domínio do desmaio é só tórax/cabeça"). Só que, para QUALQUER hit em Legs/Arms/Stomach/Common, o Prefix nunca populou `__state` (só o faz para Chest/Head — §5, `if (bodyPartType == Chest || Head) { ... }`), então `__state` chega em `Evaluate` como o sentinel `-1f`; e o PRIMEIRO check dentro de `Evaluate` (`if (preHitHp <= 0f) return false;`, pensado para o corner "parte já destruída") intercepta e retorna `false` ANTES de a execução alcançar o `if/else if/else` de domínio. Ou seja: o `else { return false; }` de domínio nunca executa na prática — o comportamento correto (nenhum roll fora de tórax/cabeça) sai do sentinel `-1f` coincidir com o guard do corner case, não do filtro de domínio explícito.

**Por que importa:** Não é um bug hoje (o resultado final está correto — nenhum roll fora de tórax/cabeça, por um caminho diferente do que o comentário do código sugere). Mas é uma armadilha de manutenção: se um dev futuro alterar o corner case "vida pré-tiro ≤0" (ex.: trocar o sentinel do Prefix de `-1f` para `0f`, ou tratar `preHitHp <= 0f` como "avaliar mesmo assim" em vez de "abortar", por qualquer motivo ligado ao item 008/011), o filtro de domínio (Legs/Arms/Stomach nunca deveriam rolar desmaio) deixa de ser garantido pelo sentinel e passa a depender SÓ do `else` no fim — que, sendo hoje código morto, é o tipo de branch que ninguém testa porque "nunca executou". `Evaluate` também é chamado (e descartado) em TODO hit de tipo válido em QUALQUER parte do corpo — sem custo de performance real (chamada trivial, sem alocação), mas sem necessidade.

**Sugestão:** Adicionar, no Postfix (§5), um filtro explícito por `bodyPartType` ANTES de chamar `Evaluate` — trocar `bool shouldFaint = isValidTraumaType && ConfigConsumerBlackout2.Value && TraumaBlackoutTrigger.Evaluate(...)` por `bool shouldFaint = isValidTraumaType && (bodyPartType == EBodyPart.Chest || bodyPartType == EBodyPart.Head) && ConfigConsumerBlackout2.Value && TraumaBlackoutTrigger.Evaluate(...)`. Isso torna o filtro de domínio explícito no ponto de chamada (paridade com a legibilidade do bloco antigo, que também testava `bodyPartType == Chest`/`Head` antes de qualquer outra coisa) e libera o `else` dentro de `Evaluate` para ficar puramente defensivo (nunca mais dead code por coincidência de sentinel).

**Decisão:** `[x]` Aceitar sugestão

**Resolução:** Filtro explícito `bodyPartType == Chest || Head` adicionado ao `shouldFaint` no stub do Postfix (§5), antes da chamada a `Evaluate`.

---

## Pontos verificados sem achado (auditoria rigorosa, sem contra-evidência)

- **Lista de tipos de dano válidos (`isValidTraumaType` — Bullet/Explosion/Sniper/Landmine/GrenadeFragment):** confirmado suficiente e correta. `EDamageType.Fall` existe como valor distinto (`references/eft-decompiled/Assembly-CSharp/EFT/Player.cs:28343`, `EffectsController.cs:1486`) e NÃO está na lista — dano de queda corretamente não dispara o gatilho de desmaio, como esperado pela spec funcional. `EDamageType.Melee` (`BotMemoryClass.cs:967`, `Player.cs:18392`) também fica de fora, consistente com o domínio do item (arma de fogo/explosão). Esta lista é HERDADA sem alteração do código atual (`HealthPatches.cs:44-48`) — o item 007 não a toca, então não é uma decisão nova a justificar aqui; verificação feita só para confirmar que nada no comportamento desejado do item exige revisá-la.
- **Auditoria independente de AP-02/AP-03 (Fika):** re-verificado por leitura direta (não só confiando na citação da spec/review 01) — `ObservedPlayer.ApplyDamageInfo` (`references/fika-plugin/Fika.Core/Main/Players/ObservedPlayer.cs:570-577`) só seta campos `Last*`, não chama `base` — patch inerte no espelho. `FikaPlayer.ApplyDamageInfo` (`FikaPlayer.cs:673-687`) e `FikaBot.ApplyDamageInfo` (`FikaBot.cs:246-252`) chamam `base.ApplyDamageInfo(...)` no fim do método — patch dispara no dono. Bate exatamente com o texto da spec §9 checks 2/3.
- **Rename-at-delivery — mecânica de fundo (delete-antes-do-save, sem copiar valor):** o padrão descrito em prosa pela spec (§4/§8) bate com os 4 blocos reais em `TRLImmersiveCombatMedicinePlugin.cs:338-428` (conferidos nesta rodada) — cada bloco busca por `section`+`key` exatos nos `OrphanedEntries`, remove sem copiar valor, salva, loga. O ÚNICO gap é a falta de citação de linha (PA-02-03), não a descrição do mecanismo em si.
- **Config `ConfigConsumerBlackout2` — estado hoje vs. spec:** confirmado que a key ATUAL no código (`TRLImmersiveCombatMedicinePlugin.cs:147-148`) é `"Blackout 2.0 (item 007)"` com default `false` — exatamente o placeholder que a spec descreve como origem do rename-at-delivery. Sem discrepância.

---

## Status

Nenhum bloqueador 🔴. 2 achados 🟡 (evidência de segurança entre mods verificando a garantia errada — a garantia CERTA existe e foi confirmada nesta rodada por decompile, só falta entrar na spec; ambiguidade nome-de-campo↔nome-exibido nas 4 configs novas) + 3 achados 🟢 (rigor de citação do bloco de migração; linha da seção 6 do `PROPRIEDADES.md` não mencionada; filtro de domínio redundante/código morto em `Evaluate`) — todos de documentação/citação, nenhum exige redesenho de estratégia, ponto de patch ou stub de código além de acréscimos pontuais.

**Pronto para `/code-mod`:** sim. Os 5 achados foram aplicados na spec técnica (PA-02-01/02/03/04 são texto/citação; PA-02-05 é um filtro explícito de 1 linha no stub). Sem 3ª rodada — o plano previa 2 rodadas para este item de maior risco, ambas concluídas sem bloqueador.
