# 005 — Toggle Mestre (F12) com Núcleo de Compatibilidade Sempre Ativo em Coop · As-Built

**Mod:** VisceralCombat
**Spec funcional:** [005-toggle-mestre-compat-coop-01-spec.md](005-toggle-mestre-compat-coop-01-spec.md)
**Spec técnica:** [005-toggle-mestre-compat-coop-02-spec-tech.md](005-toggle-mestre-compat-coop-02-spec-tech.md)
**Última review técnica:** [005-toggle-mestre-compat-coop-03-spec-tech-review-01.md](005-toggle-mestre-compat-coop-03-spec-tech-review-01.md)
**Build inicial:** 2026-09-20

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod` e atualizado por `/apply-code-review`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat/VisceralEntry.cs` | Adicionado `VisceralCombatEnabled` (ConfigEntry, seção "General", primeiro bind) + helper `IsCategoryActive(...)` |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs` | 5 gates convertidos pra `IsCategoryActive` (`EnableDismemberment`:358, `UseActiveRagdolls`:319/:409, `EnableBloodEffects`:821/:968) |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs` | `EnableDismemberment.Value` → `IsCategoryActive(EnableDismemberment)` |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Dismemberment.Patches/ProneLockPatch.cs` | Gate novo no topo do `Prefix` de `ProneLockPatch` e `ProneMoverDoPronePatch` (mestre desligado = comportamento vanilla) |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Dismemberment.Patches/BleedPatch.cs` | 4 gates convertidos pra `IsCategoryActive` (`EnableBloodEffects`:38/:142/:201, `EnableArmorSparks`:80) |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Dismemberment.Patches/GameStartedPatch.cs` | Bloco de decal de sangue: `EnableBloodEffects.Value` → `IsCategoryActive(EnableBloodEffects)` |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Classes/RagdollHelperClass.cs` | `EnableImpactBloodCloud.Value` → `IsCategoryActive(EnableImpactBloodCloud)` (:582) |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/GameStartedPatch.cs` | Setup de colisão de camada (`BodyCollision`/`UseActiveRagdolls`, :48/:52/:62) convertido pra `IsCategoryActive` |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/BodiesImpulsePatch.cs` | `ItemForce.Value` → `IsCategoryActive(ItemForce)`; gate novo (`VisceralCombatEnabled.Value` bruto) inserido antes do impulso/wake de cadáver, que não tinha config nenhum |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/CreateCorpsePatch.cs` | Gate novo no topo do `Postfix` (:32) |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/GrenadeDeadBodiesPatch.cs` | Gate novo logo após o `VisceralEntry.Instance == null` existente (:20) |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/AttachWeaponPatch.cs` | Gate novo no topo do `Postfix` (:18) |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/PlayerInitPatch.cs` | Gate novo ANTES de agendar o `ContinueWith` (:19); achado extra fora da tabela §2: `UseActiveRagdolls.Value` dentro do `ContinueWith` também convertido pra `IsCategoryActive` |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/ShootOffHelmetPatch.cs` | `ShootHelmetOff.Value` → `IsCategoryActive(ShootHelmetOff)` (:29) |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/CreateBSGRagdollPatch.cs` | Fallback `return true;` inserido como primeira linha do `Prefix` (:27) — inverso dos demais gates: mestre desligado deixa o ragdoll vanilla do EFT rodar |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/RagdollClassPatch.cs` | Fallback `return true;` inserido como primeira linha do `Prefix` (:23), mesmo padrão inverso |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combat.Patches/ShellCasingPatch.cs` | `NeverDeleteShells.Value` → `IsCategoryActive(NeverDeleteShells)` (:28) |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/GrenadeItemsPatch.cs` | `ItemForce.Value` → `IsCategoryActive(ItemForce)` (:21) |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/PhysicalItemsPatch.cs` | `ItemForce.Value` → `IsCategoryActive(ItemForce)` (:23) |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combined.Patches/DeathInventoryDropPatch.cs` | Removida a linha que acoplava a queda de capacete/óculos ao `EnableDismemberment` (:156) — agora só depende do toggle próprio `DropHeadEquipmentOnDismemberment`, por motivo de sincronização de dados em coop |
| MODIFICADO | `mods/VisceralCombat/PROPRIEDADES.md` | Nova seção "General" documentando `Visceral Combat Enabled` + nota explicando a exceção de sincronização arma/capacete |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat/VisceralCombat.csproj` | `<Version>` `3.9.21` → `3.10.0` |
| MODIFICADO (plugin version) | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat/VisceralEntry.cs` | `[BepInPlugin]` `3.9.22` → `3.10.0` |

## PA-NN-MM resolvidos durante o build

> Pontos da última review técnica que foram **aplicados como parte da implementação** (não como /apply-code-review posterior — já haviam sido resolvidos na spec técnica antes do `/code-mod` rodar; listados aqui pela rastreabilidade final no código).

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | A — Gap · 🔴 Bloqueador | Tabela §2 corrigida (7 call sites faltantes + contagem de `BleedPatch.cs`); todos os gates aplicados no código conforme checklist §8 |
| PA-01-02 | B — Edge Case · 🟡 Importante | `DeathInventoryDropPatch.cs:156` desacoplado de `EnableDismemberment` — capacete/óculos caem por motivo de sincronização de dados em coop, não UX |
| PA-01-03 | B — Edge Case · 🟢 Menor | `GameStartedPatch.cs` (Ragdolls, setup de colisão de camada) gateado — ressalva documentada inline de que só afeta a próxima raid |

**Achado extra descoberto durante a verificação exaustiva final (fora da tabela §2 original):** `PlayerInitPatch.cs:23` também usava `UseActiveRagdolls.Value` bruto dentro do `ContinueWith` — convertido pra `IsCategoryActive` além do gate novo de topo já previsto na tabela.

## Mudanças posteriores

> Atualizado por `/apply-code-review` a cada rodada. Cada entrada lista os achados aplicados/rejeitados/pulados naquela rodada e os arquivos tocados.

(vazio inicialmente — preenchido por `/apply-code-review`)

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-20 | Build concluído via `/code-mod` — 0 erros de compilação (20 warnings pré-existentes, nenhum novo). Compilação instalou automaticamente em `E:/Tarkov Red Line/BepInEx/plugins/VisceralCombat` (comportamento padrão do `compile-mod.sh` — ver ressalva no relatório do `/code-mod`). Validação em jogo/coop ainda pendente pelo usuário. |
