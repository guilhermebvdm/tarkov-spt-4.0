# 001 — skin-tagilla-academia · Review Técnica 01

**Mod:** AutoGym
**Spec técnica revisada:** [001-skin-tagilla-academia-02-spec-tech.md](001-skin-tagilla-academia-02-spec-tech.md)
**Data:** 2026-06-10

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM` (review 01, ponto MM). Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 5 · Total: 5

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | B — Edge Case | 🟡 | Estado estático órfão se o corpo for destruído sem `StopWorkout` | ✅ Resolvido |
| PA-01-02 | B — Edge Case | 🟡 | `Restore` deve invalidar geração ANTES de ler `_swappedBody` (ordem ok no stub, mas falta cobrir `Apply` pós-destruição) — consolidado em PA-01-01 | ✅ Resolvido |
| PA-01-03 | C — Lógica | 🟢 | `CustomizationClipping` acumula flags da skin de treino e nunca limpa | ✅ Resolvido |
| PA-01-04 | A — Gap | 🟢 | `HasIntergratedArmor` fica obsoleto durante o swap | ✅ Resolvido |
| PA-01-05 | A — Gap | 🟢 | Mensagem de log não distingue id inválido / suite id em vez de body id | ✅ Resolvido |

## Categorias

- **A — Gaps de Especificação:** informações ausentes que ambiguam a implementação
- **B — Edge Cases:** cenários válidos não cobertos
- **C — Erros de Lógica:** pressupostos errados, contradições, código incompatível com SPT 4.0+

## Impacto

- 🔴 **Bloqueador** — impede implementar ou causa bug/crash garantido
- 🟡 **Importante** — pode causar comportamento errado em cenário relevante
- 🟢 **Menor** — qualidade/clareza, não bloqueia

---

## Verificação de evidências

Todas as refs de Assembly citadas na spec foram conferidas contra os arquivos em `references/eft-decompiled/Assembly-CSharp/`:

- ✅ `PlayerBody.SetSkin` em [PlayerBody.cs:747](../../../../references/eft-decompiled/Assembly-CSharp/EFT/PlayerBody.cs#L747); destruição da skin anterior em :758-762; `BodyCustomization` em :514; `SkeletonRootJoint` em :510.
- ✅ `CustomizationSolverClass.GetBundle` em [CustomizationSolverClass.cs:348-351](../../../../references/eft-decompiled/Assembly-CSharp/CustomizationSolverClass.cs#L348-L351) — `[CanBeNull]`, delega a `GetItem` (Dictionary_1 clothing + Dictionary_5 heads).
- ✅ `GClass1857.Retain` (:125) / `LoadBundles` (:173); padrão de retain/release em [GClass1041.cs:112-116, 184-198](../../../../references/eft-decompiled/Assembly-CSharp/GClass1041.cs#L112-L116).
- ✅ `HideoutPlayerOwner.PrepareWorkout` (:753) / `StopWorkout` (:769); `HideoutPlayer` property (:120).
- ✅ `MongoID(string)` ctor (:59) e `operator ==` ([MongoID.cs:312](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MongoID.cs#L312)) — comparação `skinId == originalId` do stub compila.
- ✅ `ResourceKey` é classe (comparações `!= null` em GClass1041.cs:186) — anotação `ResourceKey?` do stub é válida.
- ✅ Hideout usa o `Player.Init` completo ([Player.cs:28629](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L28629)), que popula `BodyCustomization` — o overload curto de `Init` ([PlayerBody.cs:665](../../../../references/eft-decompiled/Assembly-CSharp/EFT/PlayerBody.cs#L665)) que NÃO popula `BodyCustomization` não é usado para o hideout player.
- ✅ ID `66a25a3af12f29d8a2599527` confirmado como `body` do kit `top_boss_tagilla_nohead` no `config.jsonc` do AllTheClothes.

---

## Pontos

### PA-01-01 · B — Edge Case · 🟡 Importante · ✅ Resolvido em 2026-06-10

**Estado estático órfão se o corpo for destruído sem `StopWorkout` parear**

**Problema:** `Apply` faz early-return quando `_swappedBody != null`. Não há evidência no Assembly de que `StopWorkout` ([HideoutPlayerOwner.cs:769](../../../../references/eft-decompiled/Assembly-CSharp/EFT/HideoutPlayerOwner.cs#L769)) seja chamado no teardown do hideout — se o jogador sair do hideout no meio do treino e o `HideoutPlayer` for destruído sem `StopWorkout`, `_swappedBody` continua apontando para um `PlayerBody` Unity-destruído e `_retainedBundles` permanece retido.

**Por que importa:** na próxima sessão de hideout, todo `Apply` early-returna (`_swappedBody != null`) — a feature para de funcionar silenciosamente até um `StopWorkout` rodar e limpar via exceção capturada. Além do leak do handle de bundle e da referência morta (viola a regra de static-state da skill `spt-mod-best-practices` §3).

**Sugestão:** no início de `Apply`, sanear estado órfão usando o lifetime check do Unity (`MonoBehaviour` destruído compara `== null` via overload):

```csharp
if (_swappedBody is not null && !_swappedBody) // destruído sem StopWorkout
{
    _swappedBody = null;
    _retainedBundles?.Release();
    _retainedBundles = null;
}
```

Atualizar o stub §5 da spec técnica e adicionar uma linha no §7 (riscos) documentando que o par Prepare/Stop não é garantido pelo jogo.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** Saneamento de estado órfão adicionado ao início de `Apply` no stub §5 da spec técnica; risco documentado no §7.

### PA-01-02 · B — Edge Case · 🟡 Importante · ✅ Resolvido em 2026-06-10

**Consolidado em PA-01-01** — mesmo cenário-raiz (destruição do corpo sem `StopWorkout`); mantido como ID separado apenas para registrar que o caminho `ApplyAsync` pós-await já cobre o caso via `playerBody == null` (Unity check), mas o caminho `Apply`/early-return não. A resolução de PA-01-01 fecha este ponto.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (fechar junto com PA-01-01)
- `[ ]` Caminho alternativo: _________________

**Resolução:** Fechado junto com PA-01-01 (mesma resolução).

### PA-01-03 · C — Lógica · 🟢 Menor · ✅ Resolvido em 2026-06-10

**`CustomizationClipping` acumula flags da skin de treino e nunca limpa**

**Problema:** `SetSkin` faz `CustomizationClipping |= component.GetClippingCustoms` ([PlayerBody.cs:754-757](../../../../references/eft-decompiled/Assembly-CSharp/EFT/PlayerBody.cs#L754-L757)) — só adiciona flags, nunca remove. Se a skin de treino tiver `ClippingRuleChanger`, as flags persistem no corpo após o restore.

**Por que importa:** regras de clipping extras podem ocultar meshes de roupas/equipamento indevidamente até o corpo ser recriado (troca de cena). Para a Tagilla's Chest (torso nu) o risco é baixíssimo, mas a propriedade `Workout Body Skin Id` aceita qualquer template.

**Sugestão:** adicionar ao §7 da spec técnica como risco aceito (mesma classe do `_bodyRenderers` obsoleto): "flags de `CustomizationClipping` adicionadas pela skin de treino persistem até o corpo ser recriado; risco cosmético, zero para a skin padrão". Nenhuma mudança de código.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** Risco documentado no §7 da spec técnica. Sem mudança de código.

### PA-01-04 · A — Gap · 🟢 Menor · ✅ Resolvido em 2026-06-10

**`HasIntergratedArmor` fica obsoleto durante o swap**

**Problema:** `Init` calcula `HasIntergratedArmor` a partir do Body do perfil ([PlayerBody.cs:606](../../../../references/eft-decompiled/Assembly-CSharp/EFT/PlayerBody.cs#L606)); `SetSkin` não recalcula. Durante o treino, o valor reflete a skin do perfil, não a Tagilla.

**Por que importa:** com `Hide Workout Gear` desligado e colete equipado, o visual do colete sobre o torso trocado pode clipar (a flag participa das regras de clipping de `CustomItem.cs:99`). Cosmético e restrito ao treino.

**Sugestão:** documentar no §7 como risco aceito; opcionalmente mencionar que setar/restaurar `playerBody.HasIntergratedArmor = solver.HasIntegratedArmor(skinId)` ([CustomizationSolverClass.cs:367-374](../../../../references/eft-decompiled/Assembly-CSharp/CustomizationSolverClass.cs#L367-L374)) no Apply/Restore eliminaria o gap (propriedade pública com setter, [PlayerBody.cs:571](../../../../references/eft-decompiled/Assembly-CSharp/EFT/PlayerBody.cs#L571)). Implementar só se trivial durante o `/code-mod`.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** Risco documentado no §7 da spec técnica; correção via setter fica opcional no `/code-mod`.

### PA-01-05 · A — Gap · 🟢 Menor · ✅ Resolvido em 2026-06-10

**Mensagem de log não distingue id inválido / suite id em vez de body id**

**Problema:** dois erros de configuração distintos caem no mesmo aviso: (a) string que não é MongoID válido → `new MongoID(...)` lança → `"failed to swap"`; (b) usuário cola o **suite id** (`66a258e3...`) em vez do **body id** — `GetBundle` resolve só clothing/heads (Dictionary_1/_5, [CustomizationSolverClass.cs:377-388](../../../../references/eft-decompiled/Assembly-CSharp/CustomizationSolverClass.cs#L377-L388)), retorna `null` → warning de "not found" que sugere AllTheClothes ausente.

**Por que importa:** o erro de configuração mais provável (copiar o id errado do config do AllTheClothes) gera diagnóstico enganoso.

**Sugestão:** no caminho `bundle == null`, tentar `solver.GetSuite(skinId)` ([CustomizationSolverClass.cs:391-394](../../../../references/eft-decompiled/Assembly-CSharp/CustomizationSolverClass.cs#L391-L394)); se retornar não-nulo, logar "id configurado é um SUITE id; use o body template id". Atualizar stub §5.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** Stub §5 atualizado: caminho `bundle == null` consulta `GetSuite` e loga mensagem específica para suite id.
