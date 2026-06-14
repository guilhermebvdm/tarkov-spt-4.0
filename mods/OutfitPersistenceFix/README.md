# OutfitPersistenceFix

Mod **server-side** (SPT 4.0) que corrige um **bug do SPT core**: roupas/skins (aparência do PMC) **não persistem** entre sessões — ao recarregar o jogo o personagem volta sempre para o uniforme **default** da facção.

## Causa

`ProfileFixerService.FixProfileBreakingInventoryItemIssues` (chamado em `/client/game/start` via `GameController`, **quando** `core.json → fixes.fixProfileBreakingInventoryItemIssues == true`) tem a checagem de `Customization.Body/Hands/Feet` com a **lógica invertida**:

```csharp
// Head — CORRETO (reseta só se a peça for inválida)
if (!customizationDb.ContainsKey(...Head...)) { ...Head = default; }

// Body / Hands / Feet — BUG (falta o "!": reseta toda peça VÁLIDA)
if (customizationDb.ContainsKey(...Body...)) { ...Body = DefaultUsecBody; }
```

Ou seja, qualquer roupa **válida** equipada é trocada pelo default a cada load. (Afeta qualquer perfil/skin, não é específico de nenhum mod.) O default do SPT para a flag é `false`; instalações que a ligam disparam o bug.

> ⚠️ O método chamado pelo SPT é **`FixProfileBreakingInventoryItemIssues`**, não `CheckForAndFixPmcProfileIssues` (este último não toca em customização). Versões anteriores deste mod patcheavam o método errado e eram um no-op.

## Fix

Patch Harmony em `FixProfileBreakingInventoryItemIssues`: o Prefix captura `Body/Hands/Feet` antes do método rodar; o Postfix reescreve cada peça com a lógica **correta** — peça válida → preservada (desfaz o reset indevido); peça inválida/ausente → default da facção (o que o SPT pretendia). Os demais reparos do método (dupes, tags, `StackObjectsCount`) são preservados. `Head` não é tocado (já está correto no SPT).

- `OutfitPersistenceFixMod.cs` — entry `IOnLoad`: aplica o `Harmony.PatchAll` no boot e guarda o `DatabaseService`.
- `ProfileFixerCustomizationPatch.cs` — o patch Prefix/Postfix.

## Build

`/compile-mod OutfitPersistenceFix` → instala em `D:/SPT/SPT/user/mods/OutfitPersistenceFix/`. Requer **reiniciar o servidor**.

> Bug do SPT 4.0.13 (`compatibleTarkovVersion 0.16.9`). Candidato a report upstream — se uma versão futura do SPT corrigir a lógica, este mod pode ser removido.

---

**Workflow de desenvolvimento:** ver [WORKFLOW.md](../../WORKFLOW.md).
