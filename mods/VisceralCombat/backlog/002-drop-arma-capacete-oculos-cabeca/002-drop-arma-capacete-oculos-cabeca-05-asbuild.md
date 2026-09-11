# 002 — Drop de Arma na Morte + Capacete/Óculos no Desmembramento de Cabeça · As-Built

**Mod:** VisceralCombat
**Spec funcional:** [002-drop-arma-capacete-oculos-cabeca-01-spec.md](002-drop-arma-capacete-oculos-cabeca-01-spec.md)
**Spec técnica:** [002-drop-arma-capacete-oculos-cabeca-02-spec-tech.md](002-drop-arma-capacete-oculos-cabeca-02-spec-tech.md)
**Última review técnica:** [002-drop-arma-capacete-oculos-cabeca-03-spec-tech-review-01.md](002-drop-arma-capacete-oculos-cabeca-03-spec-tech-review-01.md)
**Build inicial:** 2026-09-09

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/WeaponDropOnDeathPatch.cs` | Prefix em `Player.DropItemDead` — dropa a arma em mãos (exceto faca) como item solto via `ThrowItem`, gated por `FikaBackendUtils.IsServer \|\| IsSinglePlayer` |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs` | Novo método privado `DropHeadEquipment(Player)` + chamada em `DismemberLimb` quando `bodyPartType == Head`, fora do `foreach` de transforms (correção PA-01-01) |
| MODIFICADO | `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat/VisceralEntry.cs` | 2 novas `ConfigEntry<bool>` (`DropWeaponOnDeath`, `DropHeadEquipmentOnDismemberment`), registro de `WeaponDropOnDeathPatch` no `Awake()`, bump de versão do plugin 3.9.10 → 3.9.11 |
| MODIFICADO | `mods/VisceralCombat/PROPRIEDADES.md` | Documentadas as 2 novas propriedades nas seções `Ragdolls \| Character Properties` e `Dismemberment` |

## PA-NN-MM resolvidos durante o build

> Pontos da última review técnica que foram **aplicados como parte da implementação** (não como `/apply-code-review` posterior — todos os 4 pontos da review 01 foram resolvidos na própria spec técnica antes do `/code-mod`, ver histórico de `002-...-02-spec-tech.md`).

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | C — Erro de Lógica · 🔴 | `DropHeadEquipment` implementado fora do `foreach` de transforms em `DismemberLimb` (uma chamada por evento, não por transform casado) |
| PA-01-02 | A — Gap · 🟡 | Resolvido por investigação (`Corpse.cs`) antes do código — sem alteração de implementação necessária |
| PA-01-03 | B — Edge Case · 🟡 | Resolvido por investigação (FIKA `Player_OnDead_Patch`/`ObservedPlayer`) — gate de autoridade mantido como especificado; validação coop decisiva ainda pendente (ver Pendências) |
| PA-01-04 | A — Gap · 🟢 | Citação de evidência incorporada na spec técnica, não no código |

## Mudanças posteriores

**2026-09-09 — `/apply-code-review` (rodada 01):** aplicados CR-01-01 (🟡, bug latente — `WeaponDropOnDeathPatch` podia descartar a arma silenciosamente se `InventoryController` não fosse `TraderControllerClass`; corrigido para só pular o vanilla quando o `ThrowItem` de fato rodou, com log de aviso no caso contrário) e CR-01-02 (🟢, indentação). Arquivo tocado: `WeaponDropOnDeathPatch.cs:51-58`. Rebuild confirmado.

## Achado durante a compilação (AP-09)

A checagem de faca originalmente especificada (`item.GetItemComponent<KnifeComponent>()`, mesma usada pelo vanilla em `Player.cs:26848`) **falhou no `dotnet build`** com `CS0311`/`CS0012`: `IItemComponent` vive num assembly (`ItemComponent.Types`) não referenciado por `VisceralCombat.csproj`. Corrigido para `item is KnifeItemClass` (`KnifeItemClass.cs:7` — `class KnifeItemClass : Item`, já disponível via `Assembly-CSharp`), sem mudar o resultado funcional. Caso real de AP-09 — o `.cs` do dump confirmava o método, mas só a compilação revelou a dependência de assembly ausente.

## Pendências antes de marcar 🟢 Entregue

- [x] Build local (`dotnet build ... -c Release -o mods/VisceralCombat/builds/002-drop-arma-capacete-oculos-cabeca/`) — **sem instalar automaticamente no jogo** (restrição explícita do usuário; instalação manual fica a cargo dele). `Build succeeded`, 0 erros.
- [ ] Validação solo (bot com arma/faca em mãos; desmembramento de cabeça na morte e pós-morte; hit de cabeça sem desmembrar não duplica).
- [ ] Validação coop 2 máquinas — em especial o teste **decisivo** do item 1 (client morre, host observa) descrito na spec técnica §8, que decide se o gate de autoridade escolhido está correto ou se o item precisa voltar para `/create-technical-spec`.
- [ ] `/code-review` do código implementado.
- [ ] `/update-memory VisceralCombat` registrando o resultado da validação coop.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-09 | Build concluído via `/code-mod` — código implementado, aguardando compilação e validação in-game/coop antes de fechar o item |
| 2026-09-09 | `/code-review` rodada 01: 0 🔴 + 0 🟠 + 1 🟡 + 1 🟢 |
| 2026-09-09 | Aplicação de 2 achados de code-review 01 via `/apply-code-review` — IDs: CR-01-01, CR-01-02. Rebuild confirmado |
