# 077 — Médico: perks de tempo/movimento valem na cura de aliado do ICM · Review Técnica 01

**Mod:** CustomClasses (+ TRL-ImmersiveCombatMedicine)
**Spec técnica revisada:** [077-medic-perks-ally-heal-icm-02-spec-tech.md](077-medic-perks-ally-heal-icm-02-spec-tech.md)
**Data:** 2026-07-19

> Análise crítica da spec técnica. Cada ponto recebe `PA-01-MM`. Resolver bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 1 · 🟢 Menores: 2 · ✅ Resolvidos: 3 · Total: 3
> Memória consultada: snapshot Sessão 16 (2026-07-15) · pendência que afeta: **P-16.1** (🔴 072 não validado in-game — pré-condição, já anotada na spec).

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | B — Edge | 🟡 | Aceleração de tempo no caminho de bot local é parcial | ✅ Resolvido |
| PA-01-02 | A — Gap | 🟢 | Falta ref de linha do `SetUseTimeMultiplier` | ✅ Resolvido |
| PA-01-03 | B — Edge | 🟢 | Robustez do campo estático `AllyAnimSpeedMult` | ✅ Resolvido |

## Verificação positiva (premissa crítica confirmada)

O design de movimento depende de `HealingLegs` bloquear o andar **enquanto** `UsingMeds` (já setado pelo `HealRoutine`) não bloqueia. Confirmado no Assembly: [`MovementContext.CanWalk` (MovementContext.cs:1292-1306)](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L1292) checa **apenas** `HealingLegs` (:1296) + colisão física — **não** `UsingMeds`. Por isso o operador anda hoje na cura de aliado (com `UsingMeds` ativo), e ligar `HealingLegs` é o lock correto e suficiente. Design de movimento validado.

## Categorias

- **A — Gaps de Especificação** · **B — Edge Cases** · **C — Erros de Lógica**

## Impacto

- 🔴 Bloqueador · 🟡 Importante · 🟢 Menor

---

## Pontos

### PA-01-01 · B — Edge Case · 🟡 Importante — ✅ Resolvido em 2026-07-19

**Aceleração de tempo no caminho de bot local é parcial**

**Problema:** o `totalUseTime = stats.UseTime * timeMult + 2f` controla o momento do efeito **apenas no caminho de paciente REMOTO (humano)** — ali o tratamento é aplicado via `BandAidHealPacket` depois da espera, então encurtar a espera cura o paciente mais cedo. No caminho de **bot local**, o `MedicHealPatch.Prefix` cria um **MedEffect nativo** no bot ([MedicHealPatch.cs:327](../../../modded/../../TRL-ImmersiveCombatMedicine/modded/Patches/Medical/MedicHealPatch.cs)) cuja duração é o `UseTimeFor` vanilla — e o guard `BandAidIsRedirecting` do 072 (por design, mantido) impede acelerar esse `UseTimeFor`. Encurtar só o `totalUseTime` do `HealRoutine` cria descompasso: a espera/animação terminam antes do efeito nativo.

**Por que importa:** um Médico com Swift Surgeon operando um **bot** teria animação e espera aceleradas, mas o efeito da cirurgia no tempo cheio — corte visual e, no pior caso, `ForceFinishAnimation` antes de o efeito nativo aplicar.

**Sugestão:** aceitar que a **aceleração de tempo** vale plena no caminho de **aliado humano remoto** (o cenário de coop real que motivou o item) e é **parcial/ignorada no bot local**. A **imobilização** (`HealingLegs`) segue valendo nos dois. Documentar a limitação na §7 da spec — não vale complicar o código por um edge (operar bot é raro). Não é bloqueador porque o caminho principal (coop humano) funciona limpo.

**Decisão:**
- `[x]` Aceitar sugestão
**Resolução:** adicionada nota de limitação na §7 (Riscos) da spec técnica — aceleração de tempo plena no path remoto, parcial no bot local; imobilização vale nos dois.

### PA-01-02 · A — Gap · 🟢 Menor — ✅ Resolvido em 2026-07-19

**Falta ref de linha do `SetUseTimeMultiplier`**

**Problema:** a §2 e a §5 citam `FirearmsAnimator.SetUseTimeMultiplier(float)` sem `arquivo.cs:linha`.

**Sugestão:** ancorar em [`FirearmsAnimator.cs:465`](../../../../references/eft-decompiled/Assembly-CSharp/FirearmsAnimator.cs#L465). Bônus: o vanilla usa exatamente o padrão `SetUseTimeMultiplier(1f + num)` na cirurgia ([Player.cs:19568](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L19568) e :19907) — confirma que multiplicar o valor-base é a operação certa.

**Decisão:**
- `[x]` Aceitar sugestão
**Resolução:** ref `FirearmsAnimator.cs:465` adicionada à §2 da spec técnica.

### PA-01-03 · B — Edge Case · 🟢 Menor — ✅ Resolvido em 2026-07-19

**Robustez do campo estático `AllyAnimSpeedMult`**

**Problema:** `MedicHealPatch.AllyAnimSpeedMult` é estático global. Se algum caminho iniciasse a animação de cura sem passar pelo set do `HealRoutine`, herdaria o valor anterior.

**Por que importa:** baixo — o `HealRoutine` é o **único** iniciador da cura de aliado e **seta o campo no início de cada execução** (idempotente), antes de `SetInHands`; além disso os cleanups resetam para `1f`. O valor nunca sobrevive a uma cura sem ser reescrito.

**Sugestão:** manter o set no **início de cada** `HealRoutine` (não confiar só no reset do cleanup anterior) — já é o design. Reforçar isso no checklist de implementação como invariante explícita.

**Decisão:**
- `[x]` Aceitar sugestão
**Resolução:** invariante "setar `AllyAnimSpeedMult` no início de cada `HealRoutine`, antes de `SetInHands`" reforçada no checklist §8 da spec técnica.

---

## Conclusão

**Sem bloqueadores.** A premissa crítica (movimento) foi verificada no Assembly. Os 3 pontos foram aceitos e aplicados na spec técnica. **Pronto para `/code-mod`.**
