# CustomizationPersistenceFix

Mod **server-side** (SPT 4.0) que corrige um **bug do SPT core**: roupas/skins (aparência do PMC) **não persistem** entre sessões — ao recarregar o jogo o personagem volta sempre para o uniforme **default** da facção.

## Causa

`ProfileFixerService.CheckForAndFixPmcProfileIssues` (chamado em `/client/game/start`) tem a checagem de `Customization.Body/Hands/Feet` com a **lógica invertida**:

```csharp
// Head — CORRETO (reseta só se a peça for inválida)
if (!customizationDb.ContainsKey(...Head...)) { ...Head = default; }

// Body / Hands / Feet — BUG (falta o "!": reseta toda peça VÁLIDA)
if (customizationDb.ContainsKey(...Body...)) { ...Body = DefaultUsecBody; }
```

Ou seja, qualquer roupa **válida** equipada é trocada pelo default a cada load. (Afeta qualquer perfil/skin, não é específico de nenhum mod.)

## Fix

Patch Harmony em `CheckForAndFixPmcProfileIssues`: captura `Body/Hands/Feet` antes (Prefix) e, se ainda forem válidos na DB de customização, restaura depois (Postfix) — desfazendo apenas o reset indevido. Peças realmente inválidas continuam sendo tratadas pelo método original.

- `CustomizationPersistenceFixMod.cs` — entry `IOnLoad`: aplica o `Harmony.PatchAll` no boot e guarda o `DatabaseService`.
- `ProfileFixerCustomizationPatch.cs` — o patch Prefix/Postfix.

## Build

`/compile-mod CustomizationPersistenceFix` → instala em `D:/SPT/SPT/user/mods/CustomizationPersistenceFix/`. Requer **reiniciar o servidor**.

> Bug do SPT 4.0.13 (`compatibleTarkovVersion 0.16.9`). Candidato a report upstream — se uma versão futura do SPT corrigir a lógica, este mod pode ser removido.
