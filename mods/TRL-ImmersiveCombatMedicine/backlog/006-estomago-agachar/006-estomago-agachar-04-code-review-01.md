# 006 — Estômago: agachar probabilístico · Code Review 01

**Mod:** TRL-ImmersiveCombatMedicine
**Spec funcional:** [006-estomago-agachar-01-spec.md](006-estomago-agachar-01-spec.md)
**Spec técnica:** [006-estomago-agachar-02-spec-tech.md](006-estomago-agachar-02-spec-tech.md)
**Asbuild:** [006-estomago-agachar-05-asbuild.md](006-estomago-agachar-05-asbuild.md)
**Data:** 2026-07-19

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.
>
> `Memória consultada: snapshot de 2026-07-19 (Sessão 3, mods/TRL-ImmersiveCombatMedicine/memory/sessions.md) · pendências que afetam esta review: [P-3.5 — item 003 v1.4.1 entregue, reusa exatamente a primitiva TraumaPose/fila/absorção D2 tocada aqui], [P-3.6 — item 004 v1.5.2 entregue, reusa AbsorbIfCycleEngaged/IsCycleEngaged], [P-3.7 — pausa de custo do overhaul; diretiva de retomada citava explicitamente "review técnica do 006 ×2 → impl v1.7.0 → 2 code-reviews"; esta é a rodada 1 das 2] · nenhum bloqueador 🔴 conhecido específico do item 006.` Nenhum bug/lição da memória reaparece no código revisado.

Revisão adversarial de contexto limpo da implementação **v1.7.0** (working tree, ainda não commitado). Escopo: os 6 arquivos do diff real (`TraumaStomachConsumer.cs` criado; `TraumaPose.cs`, `TraumaLegsConsumer.cs`, `HealthPatches.cs`, `TRLImmersiveCombatMedicinePlugin.cs`, `PROPRIEDADES.md`, `TRL-ImmersiveCombatMedicine.csproj` modificados — confirmados via `git diff` contra HEAD, batendo linha a linha com a lista do asbuild) contra a spec técnica pós-review (PA-01-01/PA-01-02 já resolvidos na spec ANTES do build), os code-reviews do 003/004 (padrões de rigor já estabelecidos em `TraumaPose.cs`) e o motor 002 (`TraumaEngine.cs`) como contrato.

**Nota de escopo:** `TraumaArmsConsumer.cs` e `TraumaFallCycleConsumer.cs` também aparecem modificados no working tree (`git status`), mas o diff desses dois arquivos referencia exclusivamente `CR-01-01`/`CR-01-02` do **code-review do item 005** (extração do helper `IsPauseCondition` para `internal`, log de voz suprimida) — nada relacionado ao 006. Corretamente EXCLUÍDOS da lista de arquivos do asbuild do 006; não avaliados nesta rodada (fora de escopo).

**Contadores:** 🔴 0 · 🟠 0 · 🟡 0 · 🟢 2 — total 2 achados.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 1 · Deferido: 1 · Total: 2

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | B — Bug latente | 🟢 Menor | Branch defensivo `!p.IsYourPlayer` vaza a reserva do cooldown sem refund | ✅ Aplicado |
| CR-01-02 | D — Arquitetura | 🟢 Menor | Boilerplate de `Update()` (world-swap/toggle) duplicado pela 4ª vez sem helper compartilhado | Deferido (009/011) |

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

Todas as âncoras `arquivo.cs:linha` citadas na spec técnica (§1, §2, §5) e no asbuild foram conferidas contra o código real via `Read` + `git diff` linha a linha: 100% batem, incluindo os ranges completos citados (`TraumaEngine.cs:117-122`/`127-134`/`137-143`/`583-599`, `TraumaPose.cs:94-140`/`193-234`/`342-362`/`379-419`). Nenhum achado de Categoria A (âncora quebrada). Zero `GClassNNNN`/`GStructNNNN` novo no diff — regra de deofuscação 4.1 (AP-09) não se aplica (nenhuma referência EFT ofuscada tocada; todo o item é código C# interno do próprio mod). Grafo do mod (827 nós/1370 arestas, já regenerado incluindo `TraumaStomachConsumer`) usado só como confirmação cruzada — o `Grep` exaustivo por `CancelKind|KindWord|AbsorbIfCycleEngaged|BotCrouchDip|TryInvoluntaryCrouch|TryInvoluntaryFall` em `modded/` inteiro (não só nos arquivos listados pela spec) não encontrou nenhum call site esquecido.

---

## Verificações adicionais (limpo em)

- **Regressão 003/004 em `TraumaPose.cs` (foco #1 do escopo):** grep exaustivo de todos os call sites de `CancelKind`/`KindWord`/`AbsorbIfCycleEngaged`/`BotCrouchDip`/`TryInvoluntaryCrouch`/`TryInvoluntaryFall` em `modded/` inteiro confirma que os ÚNICOS call sites fora de `TraumaPose.cs`/`TraumaStomachConsumer.cs` são: `TraumaLegsConsumer.cs:134` (`BotCrouchDip(p)` — region default `Legs`, assinatura idêntica à v1.6.0), `TraumaLegsConsumer.cs:137` (`TryInvoluntaryCrouch(p, TraumaRegion.Legs, kind)` — já explícito antes do 006, intocado), `TraumaLegsConsumer.cs:213` (único call site que precisou de edição — `CancelKind` ganhou o parâmetro `region` obrigatório; o `git diff` mostra exatamente 1 linha alterada nesse arquivo) e `TraumaFallCycleConsumer.cs:145`/`:345` (`TryInvoluntaryFall`, assinatura que já tinha `region` desde o 004 — inalterada, e o arquivo não chama nenhum dos métodos que ganharam `region` nesta entrega). `KindWord(kind, region)` devolve `"fall"` para `InvoluntaryFall` **independente da região** (`TraumaPose.cs:65`) — os logs `fall ...` do 004 continuam bit-idênticos. Nenhuma regressão encontrada.
- **Dedup por região aditivo (foco #2):** `Defer` (`TraumaPose.cs:193-235`) casa por `(player, kind, region)` (`:205`); `PumpDeferred` (`:242-283`) re-valida cada entrada pela PRÓPRIA região via `TraumaEngine.GetLine(p, e.Region) != e.RequiredLine` (`:251`). Um adiado de pernas (`Region=Legs`) e um adiado de estômago (`Region=Stomach`) do MESMO jogador coexistem como duas entradas da lista `_deferred`, cada uma cancelada/executada só pela cura/execução da SUA região — a cura de uma nunca cancela a intenção da outra. Confirmado por leitura direta, corner "pernas adiado + estômago adiado" funciona como a spec promete.
- **Reserva atômica do cooldown / PA-01-01 (foco #3):** `TraumaStomachConsumer.cs:94` chama `TraumaEngine.ReportOneShotExecuted(p, InvoluntaryCrouch)` IMEDIATAMENTE após o pré-check de cooldown passar (`:81-85`) e ANTES de `BotCrouchDip`/`TryInvoluntaryCrouch` (`:99-105`) — exatamente como a spec corrigida (e não a versão ingênua) descreve. Os caminhos que NÃO executam desfazem essa reserva corretamente: `AbsorbIfCycleEngaged` (`TraumaPose.cs:98-105`) e o NOOP de pose-baixa (`:121-126`, `:402-409`) fazem `TryGetOneShotDeadline` + `ReportOneShotCanceled` lendo de volta o EXATO valor que o consumidor acabou de gravar (mesmo `Time.time + OneShotCooldownSeconds()`, casado por `Mathf.Approximately` em `TraumaEngine.cs:132`) — sem essa correção teria havido vazamento; com ela, ABSORB/NOOP realmente devolvem o cooldown. `Defer` (`TraumaPose.cs:196-197`) captura essa reserva fresca como `PublishDeadline` ANTES de enfileirar, preservando-a durante toda a espera do D7 — nenhuma regressão "cooldown vazando ativo sem efeito" nos caminhos já existentes (o único caminho novo que NÃO refunda é o achado CR-01-01 abaixo).
- **Bots inclusos sem gate de headless (foco #4):** `TraumaStomachConsumer.IsActive()` (`:35-40`) não tem `!FikaBackendUtils.IsHeadless` — confirmado por comparação direta com `TraumaArmsConsumer.IsActive()` (`TraumaArmsConsumer.cs:51-56`), que TEM esse gate (`!FikaBackendUtils.IsHeadless && ...`). A ausência no 006 é deliberada e documentada inline (`TraumaStomachConsumer.cs:34`, "SEM gate de headless (≠ 005)"), consistente com a decisão 11 (bots inclusos; headless precisa rolar pelos próprios bots).
- **Legado removido (foco #5):** `git diff` de `HealthPatches.cs` mostra a remoção completa do bloco `if (TRLImmersiveCombatMedicinePlugin.ConfigStomachEnabled.Value) { ... }` (stamina zerada, `SetPoseLevel(0f, true)`, `VoiceHelper.TriggerTraumaVoice(__instance, "Gut")`, guard `IsCycleEngaged` do PA-01-09) substituído por comentário-lápide. Os blocos de desmaio (linhas 50-95, acima) e o restante do Postfix ficam bit-idênticos no diff — nenhum código de pernas/braços/desmaio tocado. `grep` por `ConfigStomachEnabled`/`"Gut"`/`Sistema de Estomago` em `modded/` inteiro mostra: o bind (Plugin.cs, tooltip INERTE), o comentário-lápide (HealthPatches.cs) e o `case "Gut"` remanescente em `VoiceAndHealthUtils.cs:53` — este último órfão (nenhum call site restante o invoca), exatamente como a spec previu ("remoção é do 010, não deste item"). Nenhuma voz/drenagem órfã ativa.
- **Versão (foco #6):** `TRL-ImmersiveCombatMedicine.csproj:7` → `<Version>1.7.0</Version>` (`git diff` mostra `1.6.0` → `1.7.0`); `TRLImmersiveCombatMedicinePlugin.cs:17` → `[BepInPlugin(..., "1.7.0")]`; log de boot (`:77`) também `"v1.7.0"`. Os três pontos sincronizados.
- **Config/entrega:** `PROPRIEDADES.md` com seção 10 nova (2 entries), tooltip INERTE na seção 2, tooltip real na seção 6, linha na tabela Renomeadas e Histórico — todos conferidos via `git diff`, batendo literalmente com os `Config.Bind` do Plugin. Rename-at-delivery do placeholder `Stomach Effects (item 006)` → `Stomach Effects` segue o padrão CR-03-01 (delete sem copiar valor + `Config.Save`, `Plugin.cs:407-428`). `mod-backlog.md` com status 006 🟢.
- **Toast independente do roll:** `TraumaObservability.MaybeToastFirstOccurrence` (motor, intocado) gateia só por `TraumaConsumerRegistry.AnyActiveFor(Stomach)` — que fica true assim que `TraumaStomachConsumer.Awake()` registra (`:28`), independente do resultado de qualquer roll individual. Confirmado por leitura direta do motor.

---

## Pontos

### CR-01-01 · B — Bug latente · 🟢 Menor · ✅ Aplicado em 2026-07-19

**Branch defensivo `!p.IsYourPlayer` vaza a reserva do cooldown sem refund**

**Local:** [`mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/TraumaStomachConsumer.cs:99-105`](../../modded/Patches/Trauma/TraumaStomachConsumer.cs#L99)

**Problema:**

```csharp
if (p.IsAI)
{
    TraumaPose.BotCrouchDip(p, TraumaRegion.Stomach); // dip fire-and-forget — ref: TraumaPose.cs:383
    return;
}
if (!p.IsYourPlayer) return; // defesa extra — motor só publica donos (D16); espelho nunca chega
TraumaPose.TryInvoluntaryCrouch(p, TraumaRegion.Stomach, TraumaOneShotKind.InvoluntaryCrouch);
```

A reserva atômica do cooldown (`TraumaEngine.ReportOneShotExecuted`, linha 94) já rodou ANTES deste trecho. Se o branch `!p.IsYourPlayer` for alcançado, o método retorna sem chamar `TryInvoluntaryCrouch` e sem devolver a reserva — nenhum `TraumaEngine.TryGetOneShotDeadline` + `ReportOneShotCanceled` acontece aqui, ao contrário de TODOS os outros caminhos que não executam (`AbsorbIfCycleEngaged`, NOOP pose-baixa, `MovementContext` nulo, os 3 early-returns de `BotCrouchDip`), que seguem religiosamente o padrão "toda saída sem efeito devolve o cooldown" estabelecido pelos achados CR-01-04/CR-02-02 do 003. Hoje este branch é estruturalmente inalcançável: `TrackPlayer` (`TraumaEngine.cs:341-344`) só cria records para `IsOwnedHere(p)` (via `ActiveHealthController`), e o único caminho para `OnTransitionCore` receber um `Player` é através de uma transição desse record — logo todo humano que chega aqui já é `IsYourPlayer==true` (o próprio comentário confirma: "espelho nunca chega"). Mas se essa garantia do motor mudar no futuro (nova versão do Fika/SPT, ou um bug de regressão em `IsOwnedHere`), o cooldown compartilhado (pernas+estômago) ficaria reservado por 3-5s sem NENHUM efeito físico ter acontecido — suprimindo silenciosamente o próximo agachar legítimo (de pernas OU estômago) nessa janela.

**Por que importa:** Quebra, num caminho novo introduzido pelo 006, o invariante "todo caminho que reserva o cooldown executa OU refunda" que os próprios code-reviews do 003 (CR-01-04, CR-02-02) estabeleceram como contrato da primitiva compartilhada. Hoje é dead code sem consequência observável — mas é justamente o tipo de guard defensivo que existe PARA o dia em que a premissa que o torna "inalcançável" deixar de valer, e nesse dia falharia silenciosamente (sem log, sem erro) em vez de degradar com segurança.

**Sugestão:** Espelhar o refund já usado nos outros 5 caminhos sem efeito da mesma classe:

```csharp
if (!p.IsYourPlayer)
{
    if (TraumaEngine.TryGetOneShotDeadline(p, TraumaOneShotKind.InvoluntaryCrouch, out float dMirror))
        TraumaEngine.ReportOneShotCanceled(p, TraumaOneShotKind.InvoluntaryCrouch, dMirror);
    return; // defesa extra — motor só publica donos (D16); espelho nunca chega
}
```

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Branch `!p.IsYourPlayer` agora chama `TryGetOneShotDeadline`/`ReportOneShotCanceled` antes do `return`, espelhando o refund dos demais caminhos sem efeito.

**Aplicação:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/TraumaStomachConsumer.cs` (linhas ~104-111). Recompilado junto do CR-01-02 (deferido) — ver histórico.

---

### CR-01-02 · D — Arquitetura · 🟢 Menor · Deferido em 2026-07-19

**Boilerplate de `Update()` (world-swap/toggle) duplicado pela 4ª vez sem helper compartilhado**

**Local:** [`mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/TraumaStomachConsumer.cs:108-141`](../../modded/Patches/Trauma/TraumaStomachConsumer.cs#L108) × [`TraumaLegsConsumer.cs:178-199`](../../modded/Patches/Trauma/TraumaLegsConsumer.cs#L178)

**Problema:** O `Update()` do novo `TraumaStomachConsumer` repete, quase linha por linha, a mesma sequência já presente em `TraumaLegsConsumer` (e, por herança de padrão, em `TraumaFallCycleConsumer`/`TraumaArmsConsumer`): detectar `Singleton<GameWorld>.Instance == null` → limpar bookkeeping próprio + `_trackedWorld = null`; detectar world-swap via `!ReferenceEquals(gw, _trackedWorld)` → limpar + adotar; computar `bool active = IsActive()` e comparar com `_wasActive` para os dois edges (ON→OFF cancela, OFF→ON não estabelece nada). Esta é a 4ª cópia quase-idêntica desse esqueleto no mod (003/004/005/006), cada uma com pequenas variações no que limpa. O próprio 004 já reconheceu esse tipo de duplicação como risco em `CR-02-02` (predicado de pausa duplicado) e extraiu um helper (`IsPauseCondition`) — o mesmo racional se aplica aqui, num escopo maior.

**Por que importa:** Nenhum bug hoje — cada cópia está correta para seu próprio consumidor. O risco é o mesmo que motivou `CR-02-02` do 004: numa futura mudança da fronteira de raid/world-swap (ex.: item 007/009), só alguns dos 4 `Update()` seriam atualizados, divergindo silenciosamente. Puramente uma questão de manutenção futura, sem sintoma hoje.

**Sugestão:** Fora do escopo obrigatório do 006 (refactor tocaria os 4 consumidores, não só o item corrente). Se aceito, extrair um helper estático em `TraumaPose` ou numa classe utilitária nova, ex. `TraumaConsumerLifecycle.HandleWorldAndToggle(ref GameWorld tracked, ref bool wasActive, Func<bool> isActive, Action onWorldReset, Action onToggleOff)`, e migrar os 4 consumidores num item futuro (009 — hardening coop — é o candidato natural, já que ele varre os 4 consumidores de qualquer forma). Registrar como premissa para o item 011/009 em vez de bloquear o 006.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[x]` Rejeitar (deferir / aceitar como dívida): registrado como premissa para o item 009 (hardening coop, já varre os 4 consumidores) ou 011 (matriz de comportamento) — refactor tocaria 003/004/005 já entregues sem necessidade imediata.

---

## Veredito

Implementação **fiel à spec técnica corrigida** — os dois achados da review técnica (`PA-01-01` reserva atômica do cooldown, `PA-01-02` citação de RNG) estão implementados exatamente como a spec resolvida descreve, não a versão ingênua original. Zero regressão encontrada nos call sites existentes do 003/004 em `TraumaPose.cs` (grep exaustivo, não só os listados pela spec). Dedup por região funciona aditivamente. Bots sem gate de headless confirmado deliberado. Legado removido por inteiro, sem resíduo ativo. Versão sincronizada nos 3 pontos. Os 2 achados desta rodada são 🟢 opcionais — **nenhum bloqueador**; o item pode ser fechado sem `/apply-code-review`, mas ambos são de baixo custo caso o usuário prefira aplicá-los.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-19 | Code review 01 criada via `/code-review` (revisor adversarial de contexto limpo): 0 🔴 · 0 🟠 · 0 🟡 · 2 🟢. Verificação linha-a-linha de todo o diff real (`git diff` vs HEAD) contra a spec técnica corrigida, o asbuild e os padrões de rigor dos code-reviews do 003/004. Achados: CR-01-01 (branch defensivo sem refund do cooldown — dead code hoje) e CR-01-02 (boilerplate de `Update()` duplicado pela 4ª vez, sem helper — mesma classe do CR-02-02 do 004). |
| 2026-07-19 | CR-01-01 aplicado (refund do cooldown no branch defensivo). CR-01-02 deferido — registrado como premissa para 009/011. Sem 2ª rodada (0 🔴/🟠/🟡 restantes). Item 006 FECHADO 🟢. Build v1.7.0 recompilado (0 erros), implantado em D:\SPT. |
