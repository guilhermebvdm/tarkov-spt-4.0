# 022 — Médico ganha XP ao curar aliado · Code Review 01

**Mod:** TRL-ImmersiveCombatMedicine
**Spec funcional:** [022-medico-xp-cura-aliado-01-spec.md](022-medico-xp-cura-aliado-01-spec.md)
**Spec técnica:** [022-medico-xp-cura-aliado-02-spec-tech.md](022-medico-xp-cura-aliado-02-spec-tech.md)
**Asbuild:** [022-medico-xp-cura-aliado-05-asbuild.md](022-medico-xp-cura-aliado-05-asbuild.md)
**Data:** 2026-09-09

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

**Memória consultada:** snapshot de 2026-09-05 (Sessão 10) · pendências que afetam: nenhuma.

**Docs técnicos conferidos:** `spt-antipatterns.md` (sempre), `fika-packet-desync-prevention-plan.md` — código confere com o padrão de envelope/rename/stub descrito.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 2 · Total: 2

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | B — Bug latente | 🟠 Forte | Crédito de XP no Caminho B não é idempotente contra report duplicado/reenviado | ✅ Aplicado 2026-09-09 |
| CR-01-02 | E — Legibilidade | 🟢 Menor | `LogVersionMismatch` afirma "≤1.10.0" também para o novo stub `BandAidTreatmentReportPacketV2` (que é ≥1.11.0) | ✅ Aplicado 2026-09-09 |

## Categorias

- **A — Crítico** — bug grave, crash garantido, corrupção de estado, security issue.
- **B — Bug latente** — comportamento errado em cenário plausível, não acionado pelo caminho golden.
- **C — Gap vs. spec** — código não implementa critério de aceite, corner case, ou AC da spec.
- **D — Arquitetura** — viola padrões do repo, duplica código, leak de estado, abuso de reflection.
- **E — Legibilidade/manutenção** — nomes ruins, comentário "porquê" ausente, código morto, complexidade desnecessária.
- **F — Melhoria opcional** — refactor de qualidade, micro-otimização, simplificação.

## Impacto

- 🔴 **Bloqueador** — fix obrigatório antes de fechar o item.
- 🟠 **Forte** — fix recomendado; pode ser deferido para `06-fix-NN.md` futuro.
- 🟡 **Médio** — anotar, decidir caso a caso.
- 🟢 **Menor** — opcional.

---

## Pontos

### CR-01-01 · B — Bug latente · 🟠 Forte · ✅ Aplicado em 2026-09-09

**Crédito de XP no Caminho B não é idempotente contra report duplicado/reenviado**

**Local:** [`mods/TRL-ImmersiveCombatMedicine/modded-V3(review)/Patches/Medical/BandAidNetworkHandler.cs:956-964`](../../modded-V3%28review%29/Patches/Medical/BandAidNetworkHandler.cs#L956)

**Problema:** Dentro de `OnTreatmentReportReceived`, o crédito de XP roda incondicionalmente a cada vez que o pacote é processado:

```csharp
// linha 959
MedicalLogic.ResolvePendingConsumeFromReport(packet.PatientProfileId, packet.ItemTemplateId, packet.CostAmount);

// linha 964
HealXpCredit.CreditHealXp(mainPlayer, packet.XpAwarded);
```

`MedicalLogic.ResolvePendingConsumeFromReport` (`MedicalLogic.cs:474-488`) já resolve exatamente este problema para o CUSTO do item — ela remove a entrada de `_pendingConsumes` ANTES de consumir (`RemoveAt(i)` na linha 481, `ConsumeSafe(...)` na 483) e, se o report chegar de novo (duplicado/reenviado), o loop não encontra mais a entrada e simplesmente loga "sem consumo pendente correspondente" sem debitar o item de novo — é uma guarda de idempotência real. `HealXpCredit.CreditHealXp`, cinco linhas abaixo, não tem NENHUMA guarda equivalente: se o mesmo pacote `BandAidTreatmentReportPacketV3` for entregue duas vezes (reenvio de rede, replay, ou qualquer caminho que já dispare esse handler mais de uma vez), o médico recebe o XP duas vezes.

**Por que importa:** A spec funcional 022-01 lista explicitamente este corner case: *"Caminho B (rede): pacote de conclusão de tratamento chega duplicado, fora de ordem ou é reenviado — XP não é creditado mais de uma vez pelo mesmo tratamento."* O código como está viola esse critério diretamente — na prática o risco é baixo (canais `Reliable*` do LiteNetLib não duplicam entrega em condições normais), mas não há proteção alguma caso aconteça (reconexão, relay do host, retry manual futuro).

**Sugestão:** Fazer `ResolvePendingConsumeFromReport` retornar `bool` (`true` quando encontrou e consumiu a entrada pendente, `false` no caminho "já resolvido/sem correspondência" — troca os dois `return;` por `return true;`/`return false;`) e gatear o crédito de XP no mesmo `if`:

```csharp
bool consumed = MedicalLogic.ResolvePendingConsumeFromReport(packet.PatientProfileId, packet.ItemTemplateId, packet.CostAmount);

if (consumed)
{
    HealXpCredit.CreditHealXp(mainPlayer, packet.XpAwarded);
}
```

Isso reusa a MESMA fonte de verdade de "este report já foi processado?" que o mod já tem pra custo, em vez de criar um segundo mecanismo de dedup paralelo — e cobre o corner case da spec funcional exatamente como descrito.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada conforme proposto.
**Aplicação:** `MedicalLogic.ResolvePendingConsumeFromReport` (`MedicalLogic.cs:478-491`) agora retorna `bool` (`true` = achou e consumiu a entrada pendente). `BandAidNetworkHandler.OnTreatmentReportReceived` (`BandAidNetworkHandler.cs:959-969`) guarda o retorno em `consumed` e só chama `HealXpCredit.CreditHealXp(...)` quando `consumed == true` — mesma guarda de idempotência do consumo do item, agora cobrindo também o crédito de XP. `// ref: CR-01-01` nos dois pontos.

---

### CR-01-02 · E — Legibilidade · 🟢 Menor · ✅ Aplicado em 2026-09-09

**`LogVersionMismatch` afirma "≤1.10.0" também para o novo stub `BandAidTreatmentReportPacketV2` (que é ≥1.11.0)**

**Local:** [`mods/TRL-ImmersiveCombatMedicine/modded-V3(review)/Patches/Medical/BandAidNetworkHandler.cs:118-128`](../../modded-V3%28review%29/Patches/Medical/BandAidNetworkHandler.cs#L118), consumido por [`LegacyPackets.cs:39`](../../modded-V3%28review%29/Patches/Medical/LegacyPackets.cs#L39)

**Problema:** `LogVersionMismatch(string packetName)` escreve uma mensagem fixa: *"Recebido '{packetName}' no formato ≤1.10.0: há peer com versão anterior... (o formato de pacote mudou na 1.11.0)."* O novo stub `BandAidTreatmentReportPacketV2` (`LegacyPackets.cs:146-157`) reusa essa mesma função (`WarnOnce("BandAidTreatmentReportPacketV2")`, linha 39), mas o formato que ele descarta é o **V2 (≥1.11.0, com envelope)** — não o formato bruto ≤1.10.0 que os outros 6 stubs da lista tratam. Um dev lendo o log veria "formato ≤1.10.0" para um peer que na verdade está só uma versão (1.13.x) atrás da atual (1.14.0), o que é factualmente impreciso.

**Por que importa:** Puramente cosmético — não afeta o mecanismo (o pacote ainda é descartado com segurança, sem `ParseException`) — mas confunde quem for diagnosticar uma raid mista de versões pelo log.

**Sugestão:** Adicionar uma sobrecarga simples, ex. `LogVersionMismatch(string packetName, string extraNote = null)`, e chamar `WarnOnce` (ou diretamente `LogVersionMismatch`) para este caso específico com uma nota tipo `"(retirado na 1.14.0 — não confundir com o formato ≤1.10.0)"`. Alternativa mais simples: apenas ajustar o texto de log deste stub específico para não herdar a frase "≤1.10.0" genérica.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada conforme proposto.
**Aplicação:** nova sobrecarga `BandAidNetworkHandler.LogVersionMismatch(string packetName, string extraNote)` (`BandAidNetworkHandler.cs:131-138`) com texto genérico ("formato aposentado", sem afirmar "≤1.10.0"); `LegacyPacketCompat.Register`/`WarnOnce` (`LegacyPackets.cs`) ganharam a sobrecarga correspondente e o registro de `BandAidTreatmentReportPacketV2` passou a usá-la com a nota "(formato retirado na 1.14.0 — não confundir com o legado ≤1.10.0 acima...)". `// ref: CR-01-02` nos dois pontos.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-09 | Code review 01 criada via `/code-review` |
| 2026-09-09 | Aplicação automática de 2 achados via `/apply-code-review` — IDs aplicados: CR-01-01, CR-01-02 |
