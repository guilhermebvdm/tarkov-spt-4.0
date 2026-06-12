# 001 — skin-tagilla-academia · Code Review 02

**Mod:** AutoGym
**Spec funcional:** [001-skin-tagilla-academia-01-spec.md](001-skin-tagilla-academia-01-spec.md)
**Spec técnica:** [001-skin-tagilla-academia-02-spec-tech.md](001-skin-tagilla-academia-02-spec-tech.md)
**Asbuild:** [001-skin-tagilla-academia-05-asbuild.md](001-skin-tagilla-academia-05-asbuild.md)
**Data:** 2026-06-10

> Rodada 02 — revisão pós-aplicação da rodada 01 (CR-01-01..04 ✅ Aplicados, não reavaliados). Foco: checklists das skills `spt-mod-best-practices`, `csharp-mod-best-practices`, `repo-workflow-best-practices` + verificação critério-a-critério da spec funcional contra o código em `modded/`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 3 · Total: 3

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-02-01 | E — Manutenção | 🟢 | Parâmetro `owner` de `Restore` não é usado nem validado | ✅ Aplicado |
| CR-02-02 | F — Melhoria | 🟢 | Id do config sem `.Trim()` — espaços colados geram "id inválido" | ✅ Aplicado |
| CR-02-03 | F — Melhoria | 🟢 | Nenhum log de sucesso (nem Debug) — validação in-game às cegas | ✅ Aplicado |

---

## 1. Critérios de aceite (spec funcional) × código

| Critério de aceite | Implementação | Veredito |
| --- | --- | --- |
| Iniciar treino com a feature ligada troca o torso para "Tagilla's Chest" | Prefix de `PrepareWorkout` → [`Apply`](../../modded/WorkoutBodySkinSwap.cs#L16) → resolve body id → carrega bundle → [`SetSkin(Body, bundle, SkeletonRootJoint)`](../../modded/WorkoutBodySkinSwap.cs#L100-L102) (mecanismo idêntico ao `PlayerBody.Init`, [PlayerBody.cs:607-616](../../../../references/eft-decompiled/Assembly-CSharp/EFT/PlayerBody.cs#L607-L616)) | ✅ no código · ⚠️ exige validação in-game |
| Encerrar o treino restaura o torso anterior, sem resíduo visual | Finalizer de `StopWorkout` → [`Restore`](../../modded/WorkoutBodySkinSwap.cs#L121) re-aplica `BodyCustomization[Body]` do perfil; `SetSkin` destrói a skin de treino ([PlayerBody.cs:758-762](../../../../references/eft-decompiled/Assembly-CSharp/EFT/PlayerBody.cs#L758-L762)); Finalizer cobre o caminho de exceção | ✅ · ⚠️ validação in-game |
| Fechar e reabrir o jogo mostra a skin original (persistência zero) | Por construção: nenhuma escrita em `Profile.Customization` nem em `PlayerBody.BodyCustomization`/`BodyCustomizationId` — grep no diff confirma que só `SetSkin`/`HasIntergratedArmor` são mutados | ✅ |
| Feature desligada no F12 → nenhuma troca | [`Apply:28-31`](../../modded/WorkoutBodySkinSwap.cs#L28-L31) early-return se `SwapWorkoutBodySkin != true`; `Restore` independe do toggle (ciclo corrente sempre fecha) | ✅ |
| AllTheClothes ausente → treino normal, sem erro, warning no log | [`ApplyAsync:75-88`](../../modded/WorkoutBodySkinSwap.cs#L75-L88) — `GetBundle` null → `LogWarning` + no-op; nunca lança para o jogo | ✅ |
| Idêntico com `Hide Workout Gear` on/off | `WorkoutGearVisibility` opera sobre `SlotViews` (equipamento); swap opera sobre `BodySkins` — conjuntos disjuntos; chamadas em sequência no mesmo Prefix/Finalizer sem dependência | ✅ |

### Corner cases (spec funcional) × código

| Corner case | Implementação | Veredito |
| --- | --- | --- |
| Jogador já usa Tagilla's Chest | [`ApplyAsync:64-67`](../../modded/WorkoutBodySkinSwap.cs#L64-L67) `skinId == originalId` → no-op; `Restore` sem swap ativo é no-op | ✅ |
| Treinos consecutivos sem vazar estado | `Restore` zera `_swappedBody`/`_retainedBundles`; "skin anterior" é sempre `BodyCustomization` (imutável pelo mod), nunca a Tagilla | ✅ |
| Encerramento abrupto (exceção no fluxo) | Patch de `StopWorkout` é **Finalizer** Harmony (roda mesmo com exceção no original); `Restore` inteiro protegido | ✅ |
| Bundle ausente/corrompido | Falha de `LoadBundles`/`SetSkin` → catch interno faz `handle.Release()` (CR-01-01) e loga; treino prossegue | ✅ |
| Corpo feminino / skin não aplicável | Template inexistente → caminho "not found"; template existente mas mesh incompatível degrada cosmético — risco documentado §7 da spec técnica | ✅ |
| Início duplo sem Stop | [`Apply:34-37`](../../modded/WorkoutBodySkinSwap.cs#L34-L37) `_swappedBody != null` → mantém primeiro estado; gen token invalida load em voo duplicado | ✅ |
| Toggle off durante treino em andamento | `Restore` não lê config — restauração do ciclo corrente sempre ocorre | ✅ |
| FIKA: troca local, sem rede | `SetSkin` é puramente visual/local; nenhum pacote, nenhum acesso a `Profile` | ✅ |

## 2. Checklists das skills × código

**spt-mod-best-practices (§8):**
1. Lifecycle: hooks pareados Prefix/Finalizer; estado órfão saneado (PA-01-01); idempotente ✅
2. Leaks: `_swappedBody`/`_retainedBundles` com pontos de release identificados (Restore, saneamento, catch de falha) ✅
3. Hot path: código roda só em eventos de treino; reflection zero; sem LINQ em loop ✅
4. Context guards: patch só dispara em `HideoutPlayerOwner` (contexto hideout por definição); null-checks em `owner?.HideoutPlayer?.PlayerBody` ✅
5. Patches: alvos por `nameof` em tipo público (sem GClassNNNN); corpos protegidos por try/catch (CR-01-02) ✅
6. Compatibilidade: refs do Assembly conferidas; interop AllTheClothes degrada graciosamente; sem padrão SPT 3.x ✅
7. Config: 2 entries documentadas em `PROPRIEDADES.md` com defaults e tooltips pt-BR ✅
8. Sandbox: tudo em `modded/`; `original/` intocado (`git diff` vazio em `original/`) ✅

**csharp-mod-best-practices (checklist):** disposal pareado (handle Release nos 4 caminhos) ✅ · static state com clear points ✅ · sem alocação em hot path ✅ · sem reflection ✅ · Unity APIs só no main thread (continuations no contexto Unity, padrão já provado pelo QTE patch do upstream) ✅ · sem `async void` (Task com discard + try/catch interno) ✅ · null-safety em `Singleton<T>.Instance`/`MainPlayer`-equivalentes ✅ · `internal`/`sealed` adequados ✅ · IDs opacos, comparação via `MongoID.operator==` ✅ · logging com níveis corretos, zero per-frame ✅

**repo-workflow-best-practices (checklist):** nomenclatura de artefatos ✅ · sandbox ✅ · refs `arquivo.cs:linha` conferidas ✅ · rastreabilidade `// ref: PA/CR-NN-MM` inline ✅ · reviews imutáveis (rodada 01 só anotada) ✅ · status 🟢 coerente ✅ · PROPRIEDADES.md ✅ · sem rename de seção no `Config.Bind` ✅

**Pendência conhecida (não é achado de código):** o AC de validação manual in-game segue aberto no checklist da spec técnica — treinar na academia e observar troca/restauração. Nenhuma análise estática substitui isso (cf. memória do repo sobre validação SPT).

---

## Pontos

### CR-02-01 · E — Manutenção · 🟢 Menor · ✅ Aplicado em 2026-06-11

**Parâmetro `owner` de `Restore` não é usado nem validado**

**Local:** [`mods/AutoGym/modded/WorkoutBodySkinSwap.cs:121`](../../modded/WorkoutBodySkinSwap.cs#L121)

**Problema:** `Restore(HideoutPlayerOwner owner)` ignora `owner` por completo — restaura a partir de `_swappedBody` global. Funciona porque o hideout tem um único player local, mas o parâmetro sugere uma validação que não existe.

**Por que importa:** leitor futuro assume que o restore é per-owner; se um dia houver dois owners (variação FIKA), o `Restore` de um limparia o swap do outro silenciosamente.

**Sugestão:** ou remover o parâmetro (`Restore()`) deixando o acoplamento global explícito, ou adicionar guard documentando a premissa: `if (owner?.HideoutPlayer?.PlayerBody != _swappedBody && _swappedBody is not null) return;` — preferência pela remoção (YAGNI) + comentário de premissa single-player.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[x]` Aceitar com modificação: remover o parâmetro (opção preferida da sugestão)
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada.
**Aplicação:** Parâmetro `owner` removido de `Restore()`; premissa single-player documentada em comentário; call-site do Finalizer em `Plugin.cs` atualizado. (`modded/WorkoutBodySkinSwap.cs`, `modded/Plugin.cs`)

### CR-02-02 · F — Melhoria opcional · 🟢 Menor · ✅ Aplicado em 2026-06-11

**Id do config sem `.Trim()` — espaços colados geram "id inválido"**

**Local:** [`mods/AutoGym/modded/WorkoutBodySkinSwap.cs:55`](../../modded/WorkoutBodySkinSwap.cs#L55)

**Problema:** `new MongoID(Plugin.WorkoutBodySkinId.Value)` falha se o usuário colar o id com espaço/quebra de linha do config do AllTheClothes — o erro de cópia mais provável no F12.

**Por que importa:** atrito de configuração evitável com 1 chamada.

**Sugestão:** `skinId = new MongoID(Plugin.WorkoutBodySkinId.Value.Trim());`

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada.
**Aplicação:** `.Trim()` aplicado ao valor do config antes do ctor `MongoID`. (`modded/WorkoutBodySkinSwap.cs`)

### CR-02-03 · F — Melhoria opcional · 🟢 Menor · ✅ Aplicado em 2026-06-11

**Nenhum log de sucesso (nem Debug) — validação in-game às cegas**

**Local:** [`mods/AutoGym/modded/WorkoutBodySkinSwap.cs:107`](../../modded/WorkoutBodySkinSwap.cs#L107) e [:141](../../modded/WorkoutBodySkinSwap.cs#L141)

**Problema:** swap e restore bem-sucedidos não emitem nada. Na validação in-game pendente, o usuário não consegue confirmar pelo log do BepInEx que `Apply`/`Restore` rodaram (vs. early-return silencioso por id igual, solver nulo etc.).

**Por que importa:** o AC de validação manual fica mais caro de diagnosticar se a troca não aparecer visualmente.

**Sugestão:** `Plugin.Log?.LogDebug($"AutoGym: workout body skin applied ({skinId}).")` após a linha 107 e `LogDebug("AutoGym: body skin restored.")` após a 141. `LogDebug` não polui o console default.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada.
**Aplicação:** `LogDebug` adicionado após swap aplicado e após restore bem-sucedido. (`modded/WorkoutBodySkinSwap.cs`)

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-10 | Code review 02 criada via `/code-review` (skills + critérios de aceite) |
| 2026-06-11 | Aplicação automática de 3 achados via `/apply-code-review` — IDs aplicados: CR-02-01, CR-02-02, CR-02-03 |
