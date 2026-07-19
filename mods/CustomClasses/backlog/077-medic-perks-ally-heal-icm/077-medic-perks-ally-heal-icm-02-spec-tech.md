# 077 — Médico: perks de tempo/movimento valem na cura de aliado do ICM · Spec Técnica

**Mod:** CustomClasses (+ TRL-ImmersiveCombatMedicine)
**Spec funcional:** [077-medic-perks-ally-heal-icm-01-spec.md](077-medic-perks-ally-heal-icm-01-spec.md)
**Criado:** 2026-07-19

> Fonte primária: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda ref ao EFT cita `arquivo.cs:linha`.

## 1. Estratégia

**Nenhum Harmony patch novo no EFT.** É integração **cross-mod** no mesmo molde do item 076 (arquitetura "opção B"): o **CustomClasses** expõe uma API pública nova (`CombatMedicAllyPerks`) e o **ICM** (dono da cura de aliado) a consome por **reflection** (bridge existente `CustomClassesBridge`, soft-dep fail-safe) e ajusta o **seu próprio `HealRoutine`**.

**Simplificação-chave vs. 076:** o operador é **sempre o `MainPlayer` local** — o `HealRoutine` roda no processo de quem tem o item na mão, e não se opera remotamente. Logo o gate é a **classe LOCAL** (`SkillMultipliers.IsLocalClass`), sem mapa 057 nem packet; movimento e velocidade da animação já **replicam** aos peers pelo Fika nativo (mesmo caminho do 072 na auto-cirurgia).

Dois eixos:
- **Movimento** — o ICM liga `EPhysicalCondition.HealingLegs` no `MovementContext` do operador durante a **cirurgia** de aliado (bloqueia andar — a mesma condição que o vanilla usa na auto-cirurgia, Player.cs:28968), **exceto** se o operador for Médico com Mobile Surgery. Libera em todos os cleanups.
- **Tempo** — o ICM multiplica sua duração (`stats.UseTime`) e a velocidade da animação (`SetUseTimeMultiplier`) pelo fator do operador-Médico (Swift Surgeon na cirurgia / Rapid Care nos demais). O guard `BandAidIsRedirecting` do 072 **permanece** (o 072 não deve encurtar em paralelo — o ICM é quem controla o tempo aqui).

## 2. Pontos de patch

Sem patch novo. APIs canônicas do EFT invocadas pelo ICM (já usadas pelo mod):

| Alvo (Assembly) | Uso | Motivo |
|---|---|---|
| [`MovementContext.cs:1578`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L1578) `SetPhysicalCondition(EPhysicalCondition, bool)` | chamada | liga/desliga `HealingLegs` no operador (imobiliza) |
| [`EPhysicalCondition.cs:17`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/EPhysicalCondition.cs#L17) `HealingLegs = 0x100` | enum | condição que bloqueia andar ([gate em `MovementContext.cs:1296`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L1296), sob `CanWalk` :1292) |
| [`Player.cs:28968`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L28968) | ref (prova) | o vanilla liga `HealingLegs` na cirurgia (auto) via `OnHealthEffectAdded` — comportamento que replicamos p/ aliado |
| [`FirearmsAnimator.cs:465`](../../../../references/eft-decompiled/Assembly-CSharp/FirearmsAnimator.cs#L465) `SetUseTimeMultiplier(float)` | chamada | velocidade da animação de uso (já invocada em `MedicHealPatch`; vanilla usa `SetUseTimeMultiplier(1f + num)` na cirurgia — Player.cs:19568/19907) |

## 3. Novas propriedades F12 (BepInEx)

Nenhuma. Reusa os toggles/valores do 072 (`Swift Surgeon`, `Rapid Care`, `Mobile Surgery` na seção `2 · Combat Medic`) — o 077 apenas estende o **alcance** desses perks para a cura de aliado.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `mods/CustomClasses/modded/Client/CombatMedicAllyPerks.cs` | CRIAR | Fachada pública bridge-friendly: `AllyHealTimeMult(bool)` + `AllyMobileSurgeon()` (gate classe local, fail-safe) |
| `mods/CustomClasses/modded/Client/Patches/ClassMedicPatches.cs` | MODIFICAR | `MedicTiming`: extrair overload `FactorFor(bool isSurgery)` (respeita `_disabled`) reusado pela fachada |
| `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/CustomClassesBridge.cs` | MODIFICAR | Resolver `CombatMedicAllyPerks` + 2 wrappers reflection (`AllyHealTimeMult`, `AllyMobileSurgeon`), fail-open |
| `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidController.cs` | MODIFICAR | `HealRoutine`: computar fator+imobilização; aplicar ao `totalUseTime`; helper `ReleaseSurgeryImmobilize` em todos os cleanups |
| `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/MedicHealPatch.cs` | MODIFICAR | Campo `AllyAnimSpeedMult` + multiplicá-lo nos 3 `SetUseTimeMultiplier` (animação acompanha o tempo) |

## 5. Stubs de código

```csharp
// mods/CustomClasses/modded/Client/CombatMedicAllyPerks.cs  (CRIAR)
using System;

namespace CustomClasses.Client;

/// 077 — fachada PÚBLICA (bridge-friendly) dos perks de TEMPO/MOVIMENTO do Médico p/ a cura de ALIADO do ICM.
/// Gate = classe LOCAL (o operador é sempre o MainPlayer; dispensa mapa 057/packet). Fail-safe: 1f / false.
public static class CombatMedicAllyPerks
{
    /// Fator de tempo do procedimento p/ operador Médico: Swift Surgeon (cirurgia) / Rapid Care (demais). 1 = sem efeito.
    public static float AllyHealTimeMult(bool isSurgery)
    {
        try
        {
            if (!SkillMultipliers.IsLocalClass("Combat Medic")) return 1f;   // ref: mod ClassMedicPatches.cs:186
            return MedicTiming.FactorFor(isSurgery);                          // reusa 072 (respeita _disabled)
        }
        catch (Exception ex) { Plugin.Log?.LogError($"[CustomClasses] (077) AllyHealTimeMult: {ex.Message}"); return 1f; }
    }

    /// True se o operador local é Médico c/ Mobile Surgery ligado → pode ANDAR na cirurgia de aliado.
    public static bool AllyMobileSurgeon()
    {
        try
        {
            return PerksConfig.MobileSurgeryEnabled?.Value == true
                   && SkillMultipliers.IsLocalClass("Combat Medic");          // ref: mod ClassMedicPatches.cs:374-376
        }
        catch (Exception ex) { Plugin.Log?.LogError($"[CustomClasses] (077) AllyMobileSurgeon: {ex.Message}"); return false; }
    }
}
```

```csharp
// mods/CustomClasses/modded/Client/Patches/ClassMedicPatches.cs  (MODIFICAR — dentro de MedicTiming)
/// 077 — overload por bool (o ICM já sabe se é cirurgia via ItemStats). Fonte única da lógica de tempo do 072.
internal static float FactorFor(bool isSurgery)
{
    if (_disabled) return 1f;
    if (isSurgery)
        return PerksConfig.SwiftSurgeonEnabled?.Value == true ? (PerksConfig.SwiftSurgeonTime?.Value ?? 0.5f) : 1f;
    return PerksConfig.RapidCareEnabled?.Value == true ? (PerksConfig.RapidCareUseTime?.Value ?? 0.7f) : 1f;
}
// e o existente vira: internal static float FactorFor(Item? item) => FactorFor(IsSurgery(item));
```

```csharp
// mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/CustomClassesBridge.cs  (MODIFICAR)
private static MethodInfo _allyTimeMult;   // public static float CombatMedicAllyPerks.AllyHealTimeMult(bool)
private static MethodInfo _allyMobile;     // public static bool  CombatMedicAllyPerks.AllyMobileSurgeon()

// dentro de Resolve(), após resolver CombatMedicSurgery:
var allyType = asm.GetType("CustomClasses.Client.CombatMedicAllyPerks");
_allyTimeMult = allyType?.GetMethod("AllyHealTimeMult", new[] { typeof(bool) });
_allyMobile   = allyType?.GetMethod("AllyMobileSurgeon", Type.EmptyTypes);

/// Fator de tempo do operador-Médico (1 = padrão / CustomClasses ausente). Fail-OPEN: tempo normal.
public static float AllyHealTimeMult(bool isSurgery)
{
    Resolve();
    try { if (_allyTimeMult != null) return (float)_allyTimeMult.Invoke(null, new object[] { isSurgery }); }
    catch { }
    return 1f;
}

/// True só se o operador local é Médico c/ Mobile Surgery. Fail-SAFE: false (imobiliza) se CustomClasses ausente.
public static bool AllyMobileSurgeon()
{
    Resolve();
    try { if (_allyMobile != null) return (bool)_allyMobile.Invoke(null, new object[0]); }
    catch { }
    return false;
}
```

```csharp
// mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/MedicHealPatch.cs  (MODIFICAR)
/// 077 — velocidade extra da animação de cura de aliado (setada pelo HealRoutine; 1 = sem efeito). 1/timeMult.
public static float AllyAnimSpeedMult = 1f;
// nos 3 SetUseTimeMultiplier do Prefix, multiplicar o valor-base por AllyAnimSpeedMult:
//   L313:  setMult?.Invoke(anim, new object[] { 1f * AllyAnimSpeedMult });
//   L365:  setMultMethod2?.Invoke(animator2, new object[] { 1f * AllyAnimSpeedMult });
//   L418:  setMultMethod?.Invoke(animator, new object[] { (1f + num) * AllyAnimSpeedMult });
```

```csharp
// mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidController.cs  (MODIFICAR)
// (A) dentro de HealRoutine, logo após SetPhysicalCondition(UsingMeds, true) — BandAidController.cs:561
float timeMult = CustomClassesBridge.AllyHealTimeMult(stats.IsSurgery);              // 077 — 1 se não-Médico/sem CC
if (stats.IsSurgery && !CustomClassesBridge.AllyMobileSurgeon())                      // 077 — imobiliza não-Médico
    doctor.MovementContext.SetPhysicalCondition(EPhysicalCondition.HealingLegs, true); // ref: MovementContext.cs:1578
MedicHealPatch.AllyAnimSpeedMult = timeMult > 0f ? 1f / timeMult : 1f;                // 077 — animação acompanha

// (B) o tempo de espera (era: float totalUseTime = stats.UseTime + 2f;) — BandAidController.cs:590
float totalUseTime = stats.UseTime * timeMult + 2f;                                   // 077 — Swift/Rapid encurtam

// (C) helper novo, chamado em TODO cleanup junto do UsingMeds=false (L536/607/624/723/894 + ResetAllState)
private void ReleaseSurgeryImmobilize(Player doctor)
{
    try { doctor?.MovementContext.SetPhysicalCondition(EPhysicalCondition.HealingLegs, false); } catch { } // ref: MovementContext.cs:1578
    MedicHealPatch.AllyAnimSpeedMult = 1f;   // reset incondicional (setar false já era false = no-op seguro)
}
```

## 6. Fluxo de dados

```
[operador Médico usa item médico em ALIADO]
  → BandAidController.HealRoutine (BandAidController.cs:549)  [roda no processo do OPERADOR = MainPlayer local]
      timeMult   = CustomClassesBridge.AllyHealTimeMult(stats.IsSurgery)
                     → CombatMedicAllyPerks.AllyHealTimeMult → IsLocalClass("Combat Medic") + MedicTiming.FactorFor
      immobilize = stats.IsSurgery && !CustomClassesBridge.AllyMobileSurgeon()
  → se immobilize: MovementContext.SetPhysicalCondition(HealingLegs, true)   [EFT: CanWalk=false, MovementContext.cs:1296]
  → totalUseTime = stats.UseTime * timeMult + 2f                            [procedimento encurtado]
  → doctor.SetInHands → MedicHealPatch.Prefix → SetUseTimeMultiplier(base * (1/timeMult))  [animação acelera]
  → WaitForSeconds(totalUseTime) → aplica tratamento / envia BandAidHealPacket
  → cleanup (fim normal / EmergencyDrop / CancelHeal / Deactivate / morte / ResetAllState)
      → ReleaseSurgeryImmobilize(doctor) → SetPhysicalCondition(HealingLegs, false) + AllyAnimSpeedMult=1
```

Peers observam movimento e velocidade de animação do operador **replicados via Fika** (ObservedPlayer roda no cliente de quem observa — mesmo caminho do 072 na auto-cirurgia).

## 7. Riscos e dependências

- **Guard `BandAidIsRedirecting` do 072 (ClassMedicPatches.cs:193) — MANTER.** Ele impede o 072 de encurtar o `UseTimeFor` vanilla durante o redirect. O 077 encurta por caminho próprio (o `totalUseTime` do ICM). Se o guard fosse removido, o caminho de **bot local** (que cria MedEffect nativo) sofreria dupla aceleração (072 no `UseTimeFor` + 077 no `totalUseTime`). Confirmar que o guard segue ativo após a mudança.
- **`HealingLegs` vs. auto-cirurgia.** O vanilla liga `HealingLegs` na auto-cirurgia (Player.cs:28968); na cura de aliado não há MedEffect no operador, então quem liga é o 077. Contextos **disjuntos** (o `HealRoutine` só roda na cura de aliado). O `ReleaseSurgeryImmobilize` só é chamado nos cleanups do `HealRoutine`, nunca desligando um `HealingLegs` legítimo de uma auto-cirurgia.
- **Lock preso (o maior risco).** Se um caminho de encerramento não chamar `ReleaseSurgeryImmobilize`, o operador fica sem andar. Mitigação: parear com **todos** os pontos que já setam `UsingMeds=false` (5 sites) + `ResetAllState` (raid change). Reset incondicional é seguro (desligar já-desligado é no-op).
- **Fail-safe.** CustomClasses ausente → `AllyHealTimeMult`=1 (tempo padrão) + `AllyMobileSurgeon`=false (imobiliza todos) = comportamento correto do ICM standalone (que hoje nem imobiliza — é melhoria).
- **Sincronia coop (validar in-game).** A animação acelerada é local ao operador; a replicação da velocidade aos peers segue o mesmo mecanismo do 072 — validar que os peers veem sem dessincronia.
- **Limitação — bot local (review PA-01-01):** a **aceleração de tempo** vale plena no caminho de **aliado humano remoto** (o `totalUseTime` do ICM controla o efeito via packet). No caminho de **bot local**, o efeito vem de um MedEffect nativo criado no bot (`UseTimeFor` vanilla, que o guard do 072 não deixa acelerar) — encurtar só a espera/animação cria descompasso, então a aceleração é **parcial/ignorada** para bot. A **imobilização** (`HealingLegs`) vale nos dois. Aceito como limitação (operar bot é edge).
- **Pré-condição P-16.1 (🔴):** os perks 072 ainda não foram validados in-game; o 077 assume que Swift Surgeon/Rapid Care/Mobile Surgery funcionam na auto-cirurgia.

## 8. Checklist de implementação

- [ ] CustomClasses: extrair `MedicTiming.FactorFor(bool isSurgery)` e redirecionar `FactorFor(Item)` para ele.
- [ ] CustomClasses: criar `CombatMedicAllyPerks.cs` (2 métodos públicos, gate `IsLocalClass`, fail-safe).
- [ ] ICM `CustomClassesBridge`: resolver `CombatMedicAllyPerks` + `AllyHealTimeMult`/`AllyMobileSurgeon` (fail-open).
- [ ] ICM `MedicHealPatch`: campo `AllyAnimSpeedMult` + multiplicá-lo nos 3 `SetUseTimeMultiplier`.
- [ ] ICM `BandAidController.HealRoutine`: computar `timeMult`/imobilização; ligar `HealingLegs`; aplicar `timeMult` ao `totalUseTime`; **setar `AllyAnimSpeedMult` no início de CADA `HealRoutine`, antes de `SetInHands`** (invariante — não depende do reset anterior; review PA-01-03).
- [ ] ICM `BandAidController`: helper `ReleaseSurgeryImmobilize` + chamar em L536/607/624/723/894 e `ResetAllState`.
- [ ] Build (`/compile-mod` para os dois mods) e validar AC-1..AC-7 in-game (coop).

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start + stop idempotentes — AP-01 | ✅ | `HealingLegs` liberado no `ResetAllState` (mudança de raid, BandAidController.cs:1029) + todos os cleanups; reset incondicional |
| 2 | Filtro MainPlayer/Fika em todo ponto que reage a ação de player — AP-02 | ✅ | `HealRoutine` roda só no processo do operador (MainPlayer); gate `IsLocalClass` na fachada (§5) |
| 3 | Alvos ofuscados/virtuais por assinatura; overrides auditados — AP-03 | ✅ | Sem patch novo; APIs canônicas não-virtuais (`SetPhysicalCondition` MovementContext.cs:1578; `SetUseTimeMultiplier`) já usadas pelo mod |
| 4 | Mudança de estado via API canônica; side-effects mapeados — AP-04 | ✅ | `SetPhysicalCondition` (não seta campo cru); side-effect = bloqueio de andar, mapeado (MovementContext.cs:1296) |
| 5 | Estado entre raids: raid1→exit→raid2, alt-F4/morte/MIA — | ✅ | `ReleaseSurgeryImmobilize` nos cleanups + `ResetAllState` (roda quando o GameWorld morre — cobre morte/extract/alt-F4) |
| 6 | ConfigEntry sem ambiguidade — AP-05 | N/A | Não introduz ConfigEntry; reusa os toggles do 072 |
| 7 | Reentry-guard em método patcheado — AP-07 | N/A | Sem patch recursivo; `HealRoutine` é single-flight (`_isHealingInProgress`, BandAidController.cs:551) |
| 8 | Flags/caches validados após troca — AP-08 | ✅ | `AllyAnimSpeedMult` resetado em todo cleanup; `timeMult` recomputado por procedimento (não cacheado) |

## Histórico

| Data | Evento |
|---|---|
| 2026-07-19 | Spec técnica criada via `/create-technical-spec` (10 refs ao Assembly/mod verificadas; 5 stubs compiláveis) |
