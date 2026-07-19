# 077 — Médico: perks de tempo/movimento valem na cura de aliado do ICM · Code Review 01

**Mod:** CustomClasses (+ TRL-ImmersiveCombatMedicine)
**As-Built:** [077-medic-perks-ally-heal-icm-05-asbuild.md](077-medic-perks-ally-heal-icm-05-asbuild.md)
**Data:** 2026-07-19

> Análise crítica do código implementado. IDs `CR-01-MM`. Resolver 🔴 via `/apply-code-review` antes de fechar.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 2 · 🟢 Menores: 1 · Total: 3
> Memória consultada: snapshot Sessão 16 (2026-07-15) · pendência que afeta: **P-16.1** (🔴 072 não validado in-game — pré-condição do 077).

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| CR-01-01 | B — Bug latente | 🟡 | Descompasso movimento×efeito no bot local | ✅ Aceito como dívida (documentado) |
| CR-01-02 | C — Gap vs. spec | 🟡 | Sincronia da animação acelerada em coop = validação in-game | ✅ Aceito (pendência de validação) |
| CR-01-03 | E — Legibilidade | 🟢 | `1f * AllyAnimSpeedMult` redundante | ✅ Rejeitado (mantém valor-base visível) |

## Verificações positivas

- **Semântica de movimento correta:** o operador tem `UsingMeds=true` (não sprint/jump) + `HealingLegs` decide o andar. `CanWalk` (MovementContext.cs:1292) só checa `HealingLegs` → Médico+Mobile Surgery anda (sem correr/pular); os demais ficam imobilizados. Bate com o AC refinado.
- **Fail-safe assimétrico correto:** `AllyHealTimeMult` fail-open (1f), `AllyMobileSurgeon` fail-safe (false) — sem CustomClasses, o ICM imobiliza todos e usa tempo padrão (BandAidController.cs:566-574).
- **Lock pareado em todos os cleanups:** `ReleaseSurgeryImmobilize` chamado nos 5 pontos que soltam `UsingMeds` (EmergencyDrop:537, paciente-morto:621, fim-normal:639, CancelHeal:739, Deactivate:910) + reset do mult em `CleanupHealState` (médico-morto) e `ResetAllState` (raid). Todo caminho de `StopCoroutine` (Deactivate/EmergencyDrop/CancelHeal) também solta — sem lock órfão.
- **Sem conflito com 072:** o guard `BandAidIsRedirecting` mantém o escopo do 072 desarmado no redirect → `MedAnimSpeedPatch.Prefix` não modifica o `speed`, então `base * AllyAnimSpeedMult` passa intacto. Sem dupla aceleração no path remoto.
- **Invariante do mult:** setado no início de cada `HealRoutine` antes de `SetInHands` (BandAidController.cs:574) — auto-cura não é afetada (o Prefix do `MedicHealPatch` só usa o mult no path de redirect).

## Categorias

A — Crítico · B — Bug latente · C — Gap vs. spec · D — Arquitetura · E — Legibilidade · F — Melhoria

## Impacto

🔴 Bloqueador · 🟠 Forte · 🟡 Médio · 🟢 Menor

---

## Pontos

### CR-01-01 · B — Bug latente · 🟡 Médio — ✅ Aceito como dívida

**Descompasso movimento×efeito no caminho de bot local**

**Local:** [`BandAidController.cs:566-574`](../../modded/Patches/Medical/BandAidController.cs#L566) + [`MedicHealPatch.cs:424`](../../../TRL-ImmersiveCombatMedicine/modded/Patches/Medical/MedicHealPatch.cs)

**Problema:** para **bot local** (paciente com `ActiveHealthController`), o `MedicHealPatch` cria um MedEffect nativo cuja duração é o `UseTimeFor` vanilla — que o guard do 072 não deixa acelerar. O `totalUseTime` do `HealRoutine` (que rege o `HealingLegs` e a espera) é encurtado pelo `allyTimeMult`, mas o efeito nativo não. Resultado: um Médico **sem** Mobile Surgery mas **com** Swift Surgeon operando um bot fica imobilizado por menos tempo que o efeito nativo dura.

**Por que importa:** cosmético e edge (operar bot é raro; e o Médico com Mobile Surgery — o caso comum — nem é imobilizado). O caminho de coop real (aliado humano remoto) funciona limpo (o `totalUseTime` rege o efeito via packet).

**Sugestão:** aceitar como limitação já registrada em PA-01-01 (spec técnica §7). Não vale complicar o código por um edge.

**Decisão:**
- `[x]` Rejeitar (aceitar como dívida): limitação documentada em PA-01-01; caminho principal (coop humano) correto.

### CR-01-02 · C — Gap vs. spec · 🟡 Médio — ✅ Aceito (pendência de validação)

**Sincronia da animação acelerada em coop é validação in-game**

**Local:** [`MedicHealPatch.cs:318/371/424`](../../../TRL-ImmersiveCombatMedicine/modded/Patches/Medical/MedicHealPatch.cs) (3 pontos de `SetUseTimeMultiplier`)

**Problema:** o corner case da spec funcional ("sincronia da duração acelerada em coop") exige que o gesto acelerado não termine em tempos diferentes entre o processo do operador e os peers. O código acelera a animação **localmente** via `AllyAnimSpeedMult`; se o Fika replica o `SetUseTimeMultiplier` aos observers segue o mesmo mecanismo do 072 na auto-cirurgia, mas **isso não é verificável em código**.

**Por que importa:** se a velocidade da animação não replicar, os peers veem o gesto no tempo cheio enquanto o efeito já aplicou — divergência visual (não afeta o resultado do tratamento, que é regido pelo `totalUseTime`/packet).

**Sugestão:** manter como está — é um AC de **validação in-game** (mesmo mecanismo do 072, que também aguarda P-16.1). Adicionar ao roteiro de teste: operar aliado como Médico com Swift Surgeon e um 3º peer observando o gesto.

**Decisão:**
- `[x]` Aceitar (pendência de validação in-game — sem mudança de código).

### CR-01-03 · E — Legibilidade · 🟢 Menor — ✅ Rejeitado

**`1f * AllyAnimSpeedMult` é redundante**

**Local:** [`MedicHealPatch.cs:318/371`](../../../TRL-ImmersiveCombatMedicine/modded/Patches/Medical/MedicHealPatch.cs)

**Problema:** `1f * AllyAnimSpeedMult` poderia ser `AllyAnimSpeedMult`.

**Sugestão:** manter o `1f *` explícito — documenta que o valor-base vanilla daquele path é `1f` (simétrico ao 3º ponto `(1f + num) *`, onde o base não é 1). Torna o diff auditável.

**Decisão:**
- `[x]` Rejeitar (clareza > brevidade; mantém o valor-base visível).

---

## Conclusão

**Sem bloqueadores.** O código implementa fielmente a spec (funcional + técnica), com fail-safe correto e lock de movimento pareado em todos os cleanups. Os 3 pontos são dívida documentada / validação in-game / estilo — nenhum exige mudança de código. **Item pronto para build (`/compile-mod`) e validação in-game.**

Pré-condição de validação: **P-16.1** (os perks 072 na auto-cirurgia) precede a validação do 077.
