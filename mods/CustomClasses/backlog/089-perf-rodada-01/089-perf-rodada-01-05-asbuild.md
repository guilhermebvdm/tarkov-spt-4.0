# 089 — perf — Rodada 01 de otimização · As-Build

**Mod:** CustomClasses
**Spec funcional:** [089-perf-rodada-01-01-spec.md](089-perf-rodada-01-01-spec.md)
**Spec técnica:** [089-perf-rodada-01-02-spec-tech.md](089-perf-rodada-01-02-spec-tech.md)
**Data:** 2026-09-01
**Lado:** **client apenas** — nenhum arquivo de `modded/Server/` alterado (só o bump de versão).

---

## 1. O que foi entregue

Os **8 achados** do [relatório de auditoria 01](../../docs/relatorio-auditoria-codigo-01.md) (`--perf`), mais os **32 pontos** das quatro reviews técnicas e os **6** do code review 01.

| Achado | Entrega | Arquivo principal |
|---|---|---|
| `AUD-01-01` | Menu: bail sem Menu-Overhaul, transform cacheado com `activeInHierarchy`, poll 3-em-3, espera em tempo real | `Patches/MenuClassIdentityPatch.cs` |
| `AUD-01-02` | `EClassId` + `Parse`/`NameOf`; 44 `IsLocalClass` + 6 `IsClass` migrados; overloads de string **removidos**; `ClassNameEnOf` **removido** | `SkillMultipliers.cs`, `ClassIdentities.cs` |
| `AUD-01-03` | 4 alvos consolidados (ver §2) | `Patches/ClassWeaponPatches.cs`, `Patches/ClassCombatHealthPatches.cs` |
| `AUD-01-04` | Accessor do emissor compilado 1× (`Expression.Lambda`) | `Patches/SilentKnifePatch.cs` |
| `AUD-01-05` | Logs `[053-tab*]` gateados por `PerkDiag.Enabled` | `Patches/SkillsClassTabPatch.cs` |
| `AUD-01-06` | `p is HideoutPlayer` | `Patches/WeaponMasteryPatches.cs` |
| ~~`AUD-01-07b`~~ | **NÃO ENTREGUE — rejeitado** (PA-01-07). Ver §5 | — |
| `AUD-01-07a/c/d` | (a) resolvido por consequência do `AUD-01-02` · (c) cache de tooltip · (d) cache de grupos | `Patches/SkillPanelPatch.cs`, `PerkDiagnostics.cs` |
| `AUD-01-08` | `TintedCache` limitado: quantização de cor + LRU move-to-end + cap 4/ícone + guard de mesmo-frame | `UI/ClassIconCache.cs` |

## 2. Inventário de patches (o delta do `AUD-01-03`)

**13 classes de patch REMOVIDAS** (o compilador acusa qualquer `Enable()` órfão):

`ShootRecoilPatch` · `RecoilFloorCapturePatch` · `RecoilFloorApplyPatch` · `WeaponMasteryRecoilPatch` · `WeaponMasteryErgoPatch` · `HeavyWeaponErgoPatch` · `BulwarkPatch` · `ExecutionMeleePatch` · `AdrenalineTriggerPatch` · `LocalHitTypePatch` · `ReloadSpeedPatch` · `ShotgunReloadPatch` · `HolsterDrawResetPatch`

**5 classes de patch CRIADAS** (⚠️ **nada acusa se um `Enable()` faltar** — PA-03-05):

| Classe | Alvo | Prioridade |
|---|---|---|
| `ShootCapturePatch` | `PWA.Shoot` | **`Priority.First`** |
| `ShootApplyPatch` | `PWA.Shoot` | **`Priority.Last`** |
| `ClassDamagePatch` | `Player.ApplyDamageInfo` | (nenhuma) |
| `FirearmSyncPatch` | `FirearmController.SetAnimatorAndProceduralValues` | (nenhuma) |
| `TotalErgoPatch` | `FirearmController.TotalErgonomics` | (nenhuma) |

**Helpers criados:** `BranchFailLog` · `RecoilBranches` · `ReloadBranches` · `ErgoBranches` · `DamageBranches` · `ShootRecoilState` · `LocalHitState` · `BulwarkArmor` (era `BulwarkPatch`) · `PerfCount` · `SyncState`.

**2 arquivos deletados:** `Patches/RecoilFloorPatch.cs` (PA-03-07 — o XMLdoc do B15 migrou para `RecoilBranches.ApplyFloor`) e `Patches/AdrenalineTriggerPatch.cs`.

**Gates por evento:** `ApplyDamageInfo` 4→2 · `PWA.Shoot` 4→2 · `SetAnimatorAndProceduralValues` 3→1 par · `TotalErgonomics` 2→1.

> ⚠️ **`PWA.Shoot` é 4→2, não 4→1** (PA-01-01). `Priority.First`/`Last` ordenam contra prefixos de **outros mods** (RealRecoil), não só contra os nossos. Um patch único `Normal` capturaria um `str` já multiplicado por terceiros e clamparia o piso B15 antes deles — em silêncio, e o overlay 052 não pegaria.

## 3. Desvios da spec técnica (deliberados, com razão)

| # | Spec dizia | Implementado | Por quê |
|---|---|---|---|
| 1 | Chave do cache de tooltip `(ESkillId, float, string?)` | `(float, string?)` | `MultiplierFormat.TooltipText(float, string?)` **não recebe** o skill id (`MultiplierFormat.cs:55`) — incluí-lo criaria N entradas idênticas. O requisito real do PA-01-03 (o `className` na chave) está cumprido. `CR-01-06` |
| 2 | Branches de reload recebendo o `BuffInfo` | Recebem o `FirearmController` e leem `BuffInfo` internamente | O tipo do `BuffInfo` é **ofuscado** (`GClass2250`); nomeá-lo numa assinatura viola AP-09 (os números mudam entre builds do EFT). O compilador barrou, e o código original também nunca o nomeava |
| 3 | Helper de armadura chamado `Bulwark` | `BulwarkArmor` | Colidia com o método `DamageBranches.Bulwark`. O nome novo descreve melhor (detecção de armadura de tronco) |

## 4. Instrumentação temporária (remover na Fase 4)

**17 blocos `PERF-INSTR`**, todos gateados por `Perk Diagnostics` (`PerksConfig.DiagnosticsEnabled`, default `false`).

| Instrumento | Onde | Responde |
|---|---|---|
| INSTR-1 | `MenuClassIdentityPatch.ApplyToMenu` | `finds=` por abertura de menu (`AUD-01-01`) |
| INSTR-2 | `PerfCount` + `Plugin.PerfDumpLoop` (dump a cada 60 s) | censo das superfícies quentes (`AUD-01-02`/`03`) |
| INSTR-3 | `ClassIconCache.GetTinted` | `tintedCache=` por inserção (`AUD-01-08`) |

**Como ler o dump do INSTR-2:**
- ⚠️ **O primeiro dump após ligar o diagnóstico é PARCIAL — descartar** (PA-02-05). Usar do segundo em diante.
- `*Calls` é incrementado **antes** do gate e `*Passed` **depois**; a razão deve ficar ~1/N. Se subir, o gate afrouxou.
- ⚠️ `shoot=N gates=M`: **M ≈ 2N** para tiros do player local e **≈ N** para tiros de bot, porque o `ShootCapturePatch` resolve o gate para todos e o `ShootApplyPatch` só para o local (`CR-01-04`).

**Remoção:** `grep -rn 'PERF-INSTR' modded/Client/` tem de voltar **vazio** (a corrotina `PerfDumpLoop` inteira é um dos blocos, não só as linhas de log).

## 5. O que ficou de fora

- **`AUD-01-07b`** (Adrenalina: `WaitForSeconds` em vez de `yield return null`) — **rejeitado** na review técnica 01 (`PA-01-07`) e registrado como ❌ no relatório de auditoria. Ganho de ~30 µs por janela de 25 s contra até 50 ms de atraso no re-sync do reload, alocação nova e divergência sob `timeScale`. `AdrenalineState.cs` **não foi tocado**.
- **`modded/Server/`** — só o bump de versão em 2 arquivos.
- **`PROPRIEDADES.md` / `PROPERTIES.md`** — inalterados, corretamente: **zero `ConfigEntry` nova** (a instrumentação reusa o toggle existente).

## 6. Versão

`0.16.8` → **`0.16.9`** nos **quatro** arquivos que a carregam (PA-02-02):

| Arquivo | Quem lê |
|---|---|
| `modded/Client/CustomClasses.Client.csproj:9` | build |
| **`modded/Client/Plugin.cs:13`** (`BepInPlugin`) | **log de boot do BepInEx** |
| `modded/Server/CustomClasses.Server.csproj:10` | build |
| **`modded/Server/CustomClassesMetadata.cs:19`** | **log de boot do SPT.Server** |

Teste que pega os quatro: `grep -rn '0\.16\.8' modded/` volta vazio (fora de `obj/` e de um comentário histórico).

## 7. Build

```
dotnet build --no-incremental → 0 erros · 1 warning
  CS8602 em ClassMovementPatches.cs:108 — PRÉ-EXISTENTE (era :95 antes das inserções de instrumentação)
```

**Artefato:** `mods/CustomClasses/modded/Client/bin/Debug/CustomClasses-Client.dll` — **não instalado**. O `/compile-mod` instalaria e sobrescreveria a DLL de linha de base.

**Ambiente:** este worktree não tinha `.spt-path` nem `References/`; ambos foram criados a partir de `D:/SPT` para permitir compilar sem instalar.

## 8. Linha de base pré-089 (PENDENTE — gate humano)

> ⚠️ **Esta seção está vazia de propósito.** A memória do mod tem duas pendências 🔴 — **P-10.1** e **P-16.1** — dizendo que ~21 efeitos dos itens 050/072 **nunca foram validados in-game**. Sem uma linha de base, todo AC do tipo "o perk X continua funcionando" é indecidível: se X falhar depois, não há como saber se esta rodada quebrou ou se já estava quebrado.

**Passo 0a — ✅ FEITO (2026-08-23).** A DLL instalada foi preservada em [`builds/pre-089-2026-08-23/`](../../builds/pre-089-2026-08-23/) (`CustomClasses-Client.dll`, 180.224 bytes, `AssemblyVersion 0.16.8.0`, instalada em 2026-08-22 17:40) com README de reinstalação. **Isso tornou a linha de base recuperável a qualquer momento** — ela deixou de ser um gate irreversível e virou tarefa agendável.

**Passo 0b — ⬜ PENDENTE (só o usuário pode).** Reinstalar a DLL do backup, confirmar `0.16.8` no log de boot e percorrer a matriz de perks das 6 classes + Peladão + um perfil vanilla com `Perk Diagnostics` ligado, preenchendo a tabela abaixo com **o que funciona hoje**:

| Classe | Perks a conferir | Funciona hoje? |
|---|---|---|
| Tanque | Couraça · Pack Mule · Bunker (recuo+ergo) · Heavy Frame · Tireless Arms · Recarga de escopeta | |
| Fuzileiro | Adrenalina (recuo/recarga/ADS) · Cool Under Fire · Loud Operator · Saque Rápido · Saque Barulhento | |
| Caçador | Rooted · Sharpshooter · Iron Lungs · Calm Sights · Steady Arms · Stalker · Light Frame | |
| Furtivo | Execution (melee+velocidade) · Rattled · Ghost Step · Morte Silenciosa · Light Frame | |
| Saqueador | Lebre · Quick Hands · Pack Mule · Silent Looter · Medroso · Falta de habilidade | |
| Médico | Rapid Care · Swift Surgeon · Restorative Surgery · Efficient Metabolism · Shaky Hands · Rattled | |
| Peladão / vanilla | Nenhum perk dispara · identidade visual correta | |

Marcar no `05-asbuild` quais ACs da Fase 4 foram verificados contra base **conhecida** e quais contra base **desconhecida**.

## 9. Pendências de Fase 4

- [ ] **Passo 0b** — raid de linha de base (§8)
- [ ] Validação in-game de todos os ACs da [01-spec](089-perf-rodada-01-01-spec.md) — **nada aqui rodou no jogo** (AP-06)
- [ ] Medições dos critérios B com `Perk Diagnostics` ligado (INSTR-1/2/3)
- [ ] `/update-mod-graph CustomClasses` — 13 classes removidas, 5 criadas, `IsLocalClass` com assinatura nova
- [ ] Remover a instrumentação (`grep PERF-INSTR` vazio)
- [ ] Anotar cada `AUD-01-*` no relatório de auditoria (✅ Aplicado + números, ou ❌ sem ganho)
- [ ] `/update-memory CustomClasses`

## Histórico

| Data | Evento |
|---|---|
| 2026-08-23 | Passo 0a — backup da DLL de baseline |
| 2026-08-24 | Implementação dos 8 achados (`ecce1ee5`, `d79b7847`, `6f97e82b`) + code review 01 |
| 2026-09-01 | Code review 01 aplicado (6 achados) · as-build criado · rodada fechada para validação humana |
