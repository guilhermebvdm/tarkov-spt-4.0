# 006 — Item Physics Sem Expirar (Manter Rigidbody Adormecido em Vez de Destruir) · As-Built

**Mod:** VisceralCombat
**Spec funcional:** [006-item-physics-sem-expirar-01-spec.md](006-item-physics-sem-expirar-01-spec.md)
**Spec técnica:** [006-item-physics-sem-expirar-02-spec-tech.md](006-item-physics-sem-expirar-02-spec-tech.md)
**Última review técnica:** [006-item-physics-sem-expirar-03-spec-tech-review-01.md](006-item-physics-sem-expirar-03-spec-tech-review-01.md)
**Build inicial:** 2026-09-21

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod` e atualizado por `/apply-code-review`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LootItemPersistPhysicsPatch.cs` | Dois Prefixes Harmony: `LootItemStopPhysicsPatch` (pula a destruição do Rigidbody em `LootItem.StopPhysics()` quando "Item Physics" está ativo, só para a corrotina de assentamento via reflexão em `ienumerator_0`) e `LootItemKillCleanupPatch` (destrói qualquer Rigidbody preservado em `LootItem.Kill()`, antes do `GameObject` voltar pro pool). |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat/VisceralEntry.cs` | Registrados `LootItemStopPhysicsPatch`/`LootItemKillCleanupPatch` em `Awake()` (logo após `PhysicalItemsPatch`); versão do `[BepInPlugin]` `3.11.9` → `3.11.10`. |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.csproj` | `<Version>` `3.11.9` → `3.11.10` (acompanha o bump do `[BepInPlugin]`). |

**Desvio pontual do stub da spec técnica:** o stub de `LootItemPersistPhysicsPatch.cs` (§5 da spec técnica) não listava `using Fika.Core.Main.Utils;`, necessário para `QuickLogger`/`ELogType` (usados nos `catch` de ambos os patches, mesmo padrão já usado em `WeaponDropOnDeathPatch.cs`). Adicionado o `using` faltante — reuso de utilitário já existente no mod, não API nova.

## PA-NN-MM resolvidos durante o build

> Pontos da última review técnica que foram **aplicados como parte da implementação** (não como /apply-code-review posterior).

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | C — Erro de Lógica · 🟢 Menor | Já resolvido na spec técnica antes do `/code-mod` (cast único `(MonoBehaviour)__instance`); o código implementado em `LootItemStopPhysicsPatch.Prefix` segue exatamente esse stub corrigido. |
| PA-01-02 | A — Gap · 🟡 Importante | Documental — comportamento de toggle desligado no meio da raid já estava descrito em §7 da spec técnica antes do build; nenhuma mudança de código adicional necessária (o comportamento decorre naturalmente da ausência de re-checagem, não de uma lógica a implementar). |
| PA-01-03 | A — Gap · 🟢 Menor | Documental — não-conflito com `PhysicalItemsPatch` já confirmado em §7 da spec técnica antes do build; confirmado nesta implementação que os dois patches (`IsRigidbodyDone`/`StopPhysics`) seguem operando em pontos distintos, sem alteração necessária em `PhysicalItemsPatch.cs`. |

## Mudanças posteriores

> Atualizado por `/apply-code-review` a cada rodada. Cada entrada lista os achados aplicados/rejeitados/pulados naquela rodada e os arquivos tocados.

- **2026-09-21 — [06-fix-01](006-item-physics-sem-expirar-06-fix-01.md):** `LootItemKillCleanupPatch` zerava o Rigidbody dentro de `Kill()` sem considerar que `Fika.Core.Main.Components.ItemPositionSyncer` (componente do FIKA) continuava vigiando o item — gerava `NullReferenceException` repetida no log ao lootar um item com física preservada. Corrigido destruindo o `ItemPositionSyncer` (se presente) antes de zerar o Rigidbody. `LootItemPersistPhysicsPatch.cs` tocado. Build 3.12.2.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-21 | Build concluído via `/code-mod` — 0 erros, mesmos 20 warnings pré-existentes do mod. Instalado automaticamente em `E:/Tarkov Red Line/BepInEx/plugins/VisceralCombat` (padrão do `/compile-mod`); cópia para `E:\Tarkov Red Line - SERVER TEST` pendente de confirmação do usuário. |
| 2026-09-21 | [06-fix-01](006-item-physics-sem-expirar-06-fix-01.md) aplicado — destrói `ItemPositionSyncer` (FIKA) antes de zerar o Rigidbody em `Kill()`, evitando `NullReferenceException` repetida. Build 3.12.2, 0 erros. |
