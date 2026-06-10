# 001 — skin-tagilla-academia · Code Review 01

**Mod:** AutoGym
**Spec funcional:** [001-skin-tagilla-academia-01-spec.md](001-skin-tagilla-academia-01-spec.md)
**Spec técnica:** [001-skin-tagilla-academia-02-spec-tech.md](001-skin-tagilla-academia-02-spec-tech.md)
**Asbuild:** [001-skin-tagilla-academia-05-asbuild.md](001-skin-tagilla-academia-05-asbuild.md)
**Data:** 2026-06-10

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 4 · Total: 4

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | B — Bug latente | 🟡 | Leak do handle de bundle se `SetSkin` lançar após o retain | ✅ Aplicado |
| CR-01-02 | B — Bug latente | 🟡 | `Apply`/`Restore` têm trechos fora de try/catch dentro de patch Harmony | ✅ Aplicado |
| CR-01-03 | E — Manutenção | 🟢 | `Restore` loga warning espúrio quando o corpo já foi destruído | ✅ Aplicado |
| CR-01-04 | F — Melhoria | 🟢 | Id malformado no config cai no catch genérico em vez de mensagem específica | ✅ Aplicado |

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

## Conformidade verificada

- ✅ Critérios da spec funcional cobertos: troca no início (`Apply` via Prefix), restauração no fim (`Restore` via Finalizer, cobre exceções), toggle F12 lido só no início do ciclo, persistência zero (nenhuma escrita em `Profile.Customization`), fallback no-op + warning com AllTheClothes ausente, idempotência quando o jogador já usa a skin (`skinId == originalId`).
- ✅ Todos os PA-01-* da review técnica implementados conforme resolução (saneamento de estado órfão, log de suite id, `HasIntergratedArmor` recalculado).
- ✅ Código bate com as refs do Assembly citadas (conferido `SetSkin` :747, `BodyCustomization` :514, `GetBundle` :348, `Retain` :125, `LoadBundles` :173, `GetSuite` :391, `HasIntegratedArmor` :367).
- ✅ Sandbox respeitado (`original/` intocado); padrão do mod seguido (helper estático ao lado de `WorkoutGearVisibility`); `PROPRIEDADES.md` atualizado; compilação 0 warnings / 0 erros.
- ✅ Sem hot path: código roda apenas em eventos de início/fim de treino.

---

## Pontos

### CR-01-01 · B — Bug latente · 🟡 Médio · ✅ Aplicado em 2026-06-10

**Leak do handle de bundle se `SetSkin` lançar após o retain**

**Local:** [`mods/AutoGym/modded/WorkoutBodySkinSwap.cs:79-87`](../../modded/WorkoutBodySkinSwap.cs#L79-L87)

**Problema:** `_retainedBundles = handle` é atribuído **antes** de `SetSkin`. Se `SetSkin` (ou `HasIntegratedArmor`) lançar, o catch loga mas `_swappedBody` nunca é setado — enquanto `_retainedBundles` fica preenchido. No próximo `Apply`, o saneamento de estado órfão não dispara (`_swappedBody` é `null`) e um novo ciclo sobrescreve `_retainedBundles`, vazando o handle anterior (bundle retido para sempre).

**Por que importa:** leak de refcount de bundle a cada falha consecutiva de `SetSkin`; o `Restore` subsequente também faria `Release` de um handle que não corresponde a nenhuma skin aplicada.

**Sugestão:** atribuir `_retainedBundles`/`_swappedBody` somente após o `SetSkin` bem-sucedido e liberar o handle no caminho de exceção:

```csharp
var handle = GClass1857.Retain(Singleton<IEasyAssets>.Instance, new[] { bundle.path });
try
{
    await GClass1857.LoadBundles(handle);
    if (generation != _generation || playerBody == null) { handle.Release(); return; }
    playerBody.SetSkin(...);
    playerBody.HasIntergratedArmor = solver.HasIntegratedArmor(skinId);
    _retainedBundles = handle;
    _swappedBody = playerBody;
}
catch
{
    handle.Release();
    throw; // re-lançado para o catch externo logar
}
```

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada conforme proposto.
**Aplicação:** `_retainedBundles`/`_swappedBody` atribuídos somente após `SetSkin` bem-sucedido; try interno em volta de load+apply com `handle.Release()` no catch e re-throw para o log externo. (`modded/WorkoutBodySkinSwap.cs`)

### CR-01-02 · B — Bug latente · 🟡 Médio · ✅ Aplicado em 2026-06-10

**`Apply`/`Restore` têm trechos fora de try/catch dentro de patch Harmony**

**Local:** [`mods/AutoGym/modded/WorkoutBodySkinSwap.cs:15-36`](../../modded/WorkoutBodySkinSwap.cs#L15-L36) e [:122-126](../../modded/WorkoutBodySkinSwap.cs#L122-L126)

**Problema:** o corpo síncrono de `Apply` (saneamento com `_retainedBundles?.Release()`, acesso a `owner?.HideoutPlayer?.PlayerBody`) roda direto no Prefix de `PrepareWorkout` sem try/catch. O mesmo vale para o `Release()` no `finally` de `Restore`, que se lançar propaga para o Finalizer Harmony. A skill `csharp-mod-best-practices` §3 exige corpo de patch protegido: exceção não tratada num Prefix cancela o método original do jogo (o treino não iniciaria).

**Por que importa:** uma exceção em `Release()` (estado interno corrompido do DependencyGraph) ou em propriedade do owner quebraria `PrepareWorkout`/`StopWorkout` inteiros — bug do mod viraria treino quebrado. Probabilidade baixa, consequência alta.

**Sugestão:** envolver o corpo inteiro de `Apply` e de `Restore` em `try/catch (Exception ex) { Plugin.Log?.LogWarning(...); }` (o `finally` interno continua). Observação: os patches upstream (`WorkoutGearVisibility.Hide/Restore`) têm a mesma exposição — fora do escopo deste item, não corrigir aqui.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada conforme proposto.
**Aplicação:** Corpo síncrono de `Apply` envolvido em try/catch com log; `Release()` do `finally` de `Restore` protegido por try/catch próprio. (`modded/WorkoutBodySkinSwap.cs`)

### CR-01-03 · E — Manutenção · 🟢 Menor · ✅ Aplicado em 2026-06-10

**`Restore` loga warning espúrio quando o corpo já foi destruído**

**Local:** [`mods/AutoGym/modded/WorkoutBodySkinSwap.cs:102-115`](../../modded/WorkoutBodySkinSwap.cs#L102-L115)

**Problema:** `if (playerBody != null)` usa o overload Unity de `==`, então um corpo destruído passa como `null`... **exceto** que `playerBody` foi tipado como `PlayerBody?` e a comparação com `!= null` em referência managed viva mas Unity-destruída retorna `false` pelo overload — correto. Porém entre o check e o `SetSkin` não há proteção contra destruição no mesmo frame de teardown; nesse caso `SetSkin` lança `MissingReferenceException` e o catch loga `"failed to restore"` num cenário que é teardown normal, não falha.

**Por que importa:** warning enganoso no log do BepInEx durante saída do hideout; custo de diagnóstico futuro.

**Sugestão:** capturar `MissingReferenceException` separadamente com `LogDebug` (ou checar `if (playerBody is not null && playerBody)` e aceitar o residual como risco irrelevante). Mudança de 2 linhas.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada conforme proposto.
**Aplicação:** `Restore` captura `MissingReferenceException` separadamente e loga em `LogDebug` como teardown normal. (`modded/WorkoutBodySkinSwap.cs`)

### CR-01-04 · F — Melhoria opcional · 🟢 Menor · ✅ Aplicado em 2026-06-10

**Id malformado no config cai no catch genérico em vez de mensagem específica**

**Local:** [`mods/AutoGym/modded/WorkoutBodySkinSwap.cs:43`](../../modded/WorkoutBodySkinSwap.cs#L43)

**Problema:** `new MongoID(valor)` com string que não é 24-hex lança e cai no catch genérico (`"failed to swap workout body skin: <stack>"`), enquanto o erro real é configuração inválida.

**Por que importa:** o usuário que digitou um id errado no F12 recebe um stack trace em vez de "id inválido".

**Sugestão:** envolver o ctor num try/catch dedicado (ou validar `length == 24` + hex antes) e logar `"AutoGym: '{value}' is not a valid customization id."`. Opcional.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada conforme proposto.
**Aplicação:** Ctor `new MongoID(...)` em try/catch dedicado; id inválido loga mensagem específica sem stack trace. (`modded/WorkoutBodySkinSwap.cs`)

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-10 | Code review 01 criada via `/code-review` |
| 2026-06-10 | Aplicação automática de 4 achados via `/apply-code-review` — IDs aplicados: CR-01-01, CR-01-02, CR-01-03, CR-01-04 |
