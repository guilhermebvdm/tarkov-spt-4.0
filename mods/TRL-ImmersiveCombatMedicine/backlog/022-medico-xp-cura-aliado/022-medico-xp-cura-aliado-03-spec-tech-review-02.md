# 022 — Médico ganha XP ao curar aliado · Review Técnica 02

**Mod:** TRL-ImmersiveCombatMedicine
**Spec técnica revisada:** [022-medico-xp-cura-aliado-02-spec-tech.md](022-medico-xp-cura-aliado-02-spec-tech.md)
**Data:** 2026-09-09

> Segunda rodada, após a expansão de escopo decidida em PA-01-01 (rodada 01) — inclusão do mecanismo 2 (`HealthChangedEvent`/`ExpForHeal`, XP por HP restaurado) e aplicação das correções PA-01-02 a PA-01-05. Foco desta rodada: validar que a expansão foi implementada de forma tecnicamente correta e que nenhuma correção da rodada 01 foi perdida.

**Memória consultada:** snapshot de 2026-09-05 (Sessão 10) · pendências que afetam: nenhuma.

**Docs técnicos conferidos:** `spt-antipatterns.md`, `fika-packet-desync-prevention-plan.md` — sem contradição.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 1 · Total: 1

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-02-01 | B — Edge case | 🟢 Menor | `OnPatientHealthChanged` não filtra `EBodyPart` — inclui HP restaurado em membros fora do tratamento ativo | ✅ Resolvido 2026-09-09 |

## Verificação da rodada 01

- ✅ **PA-01-01 (expansão de escopo):** mecanismo 2 documentado em §1.1, com formula (`ExpForHeal`, `globals.json:36324`=1), evento (`HealthChangedEvent`, `IHealthController.cs:50`) e ponto de disparo (`ChangeHealth`, `ActiveHealthController.cs:3933-3954`) — todos reconfirmados nesta rodada, linhas batem.
- ✅ **PA-01-02 (dupla reflection):** `FindEffectForRead` (§5.4) substitui `HasEffect` + `ReadHealExperience` por uma única chamada por efeito, reaproveitada para existência E leitura de XP.
- ✅ **PA-01-03 (campo morto):** `_subscribedPatientHcForXp` não aparece mais em nenhum stub (grep no arquivo: 0 ocorrências).
- ✅ **PA-01-04 (tipos de leitura vs remoção):** `FindEffectForRead(activeHc, target, _heavyBleedType)` etc. — usa os tipos "de leitura" (`_heavyBleedType`/`_lightBleedType`/`_fractureType`), reservando os "concretos" para `RemoveEffectNative`. Confere com a convenção documentada em `BandAidNetworkHandler.cs:23-26`.
- ✅ **PA-01-05 (helper compartilhado):** `Helpers/HealXpCredit.cs` (§5.1) é chamado pelos dois handlers do Caminho A (`OnPatientHealerDone`, flush de `_pendingHealthXp` em `CleanupPatientSubscription`) e pelo Caminho B (`OnTreatmentReportReceived`) — único ponto de escrita nos 4 pontos de crédito.
- ✅ **Correção de truncamento (achado da própria spec, não da review 01):** o acumulador `_pendingHealthXp` (Caminho A) e o cálculo `(int)(ExpForHeal × heal)` feito uma única vez por aplicação (Caminho B) evitam a perda de XP por arredondamento repetido em curas graduais pequenas — verificado contra `GClass2266.method_1` (`GClass2266.cs:256-264`), que usa exatamente esse padrão (acumula em `float`, trunca 1x).

## Categorias

- **A — Gaps de Especificação:** informações ausentes que ambiguam a implementação
- **B — Edge Cases:** cenários válidos não cobertos
- **C — Erros de Lógica:** pressupostos errados, contradições, código incompatível com SPT 4.0+

## Impacto

- 🔴 **Bloqueador** — impede implementar ou causa bug/crash garantido
- 🟡 **Importante** — pode causar comportamento errado em cenário relevante
- 🟢 **Menor** — qualidade/clareza, não bloqueia

---

## Pontos

### PA-02-01 · B — Edge case · 🟢 Menor · ✅ Resolvido em 2026-09-09

**`OnPatientHealthChanged` não filtra `EBodyPart` — inclui HP restaurado em membros fora do tratamento ativo**

**Problema:** O handler proposto em §5.2 (`OnPatientHealthChanged(EBodyPart bodyPart, float diff, DamageInfoStruct damageInfo)`) credita `expForHeal * diff` para QUALQUER `diff > 0` recebido enquanto a assinatura estiver ativa — sem checar se `bodyPart` é o membro que o médico está de fato tratando. Isso é fiel ao vanilla (`GClass2266.OnHealthChanged` também não filtra por membro, `GClass2266.cs:169`), então não é um erro de implementação — mas no ICM a JANELA de assinatura é mais longa que um único `ChangeHealth`: fica aberta do início ao fim de toda a operação de cura (bridge via `EffectRemovedEvent`/`GInterface376`). Se, durante essa janela, o PACIENTE regenerar HP passivamente em OUTRO membro por qualquer motivo alheio ao tratamento do médico (ex.: regeneração natural/skill do próprio jogador, se aplicável a bots/peers), esse HP também seria contado como XP do médico.

**Por que importa:** Cenário de baixa probabilidade (a janela de assinatura dura só a duração de uma cura, tipicamente poucos segundos) e o impacto é sempre a FAVOR do médico (XP extra, nunca comportamento quebrado) — por isso 🟢, não 🟡. Ainda assim, é um edge case que a spec funcional (corner cases) não cobre explicitamente.

**Sugestão:** Duas opções, ambas simples: (a) aceitar o comportamento como está (é literalmente o que o vanilla faz — não filtra por origem do heal, só por sinal positivo) e documentar a decisão na spec funcional como corner case aceito; ou (b) filtrar `OnPatientHealthChanged` para só contar `diff` quando `bodyPart == checkPart` (a parte-alvo do tratamento, já resolvida em `MedicHealPatch.cs:420-422` como `checkPart`). Dado que a mecânica de regeneração passiva de HP fora de tratamento médico é rara/inexistente pra bots e pacientes tratados pelo ICM (a maioria do HP só muda por dano ou por este próprio tratamento), a opção (a) é suficiente — mas precisa ser uma decisão registrada, não um "não pensamos nisso".

**Decisão:**
- `[x]` Aceitar sugestão (a) — comportamento igual ao vanilla, documentar como corner case aceito

**Resolução:** Consistente com o princípio geral desta spec ("replicar o vanilla, nunca inventar comportamento novo") — o vanilla também não filtra por membro em `OnHealthChanged`. Corner case documentado na spec funcional `022-01`. Sem mudança de código necessária.
