# 022 — Médico ganha XP ao curar aliado · As-Built

**Mod:** TRL-ImmersiveCombatMedicine
**Spec funcional:** [022-medico-xp-cura-aliado-01-spec.md](022-medico-xp-cura-aliado-01-spec.md)
**Spec técnica:** [022-medico-xp-cura-aliado-02-spec-tech.md](022-medico-xp-cura-aliado-02-spec-tech.md)
**Última review técnica:** [022-medico-xp-cura-aliado-03-spec-tech-review-02.md](022-medico-xp-cura-aliado-03-spec-tech-review-02.md)
**Build inicial:** 2026-09-09

> Documentação pós-implementação. Workspace ativo: `mods/TRL-ImmersiveCombatMedicine/modded-V3(review)/`. Versão: `1.13.6` → `1.14.0`.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `modded-V3(review)/Helpers/HealXpCredit.cs` | Helper único `CreditHealXp(Player doctor, int amount)` — replica as 3 chamadas do vanilla (`ExperienceGained`+`AddInt(ExpHeal)`+`ShowStatNotification`), usado pelos 2 mecanismos × 2 caminhos. |
| MODIFICADO | `modded-V3(review)/Patches/Medical/MedicHealPatch.cs` | Caminho A: novo campo `_pendingHealthXp`; handlers `OnPatientHealerDone` (XP de efeito, `HealerDoneEvent`) e `OnPatientHealthChanged` (acumula XP de HP, `HealthChangedEvent`); subscribe/unsubscribe dos 2 eventos junto com `EffectRemovedEvent` já existente; flush truncado 1x em `CleanupPatientSubscription`. |
| MODIFICADO | `modded-V3(review)/Patches/Medical/BandAidTreatmentReportPacket.cs` | Renomeado `BandAidTreatmentReportPacketV2` → `V3`, campo novo `XpAwarded` (envelope de comprimento mantido). |
| MODIFICADO | `modded-V3(review)/Patches/Medical/LegacyPackets.cs` | Novo stub `BandAidTreatmentReportPacketV2` (descarta payload com envelope, formato aposentado) registrado em `LegacyPacketCompat.Register` — evita `ParseException` de peer desatualizado (AP-11). |
| MODIFICADO | `modded-V3(review)/Patches/Medical/BandAidNetworkHandler.cs` | Registro do pacote atualizado para `V3`; novo helper `FindEffectForRead` (substitui a dupla-reflection de `HasEffect`+leitura); `ApplyFullTreatmentLocally` acumula `xpAwarded` (efeito via `GInterface326.HealExperience` + HP via `ExpForHeal`); `SendTreatmentReport`/`OnTreatmentReportReceived` propagam `XpAwarded`; crédito via `HealXpCredit.CreditHealXp` no cliente do médico. |
| MODIFICADO | `TRL-ImmersiveCombatMedicine.csproj` | `<Version>1.13.6</Version>` → `1.14.0` (mudança de wire format lockstep). |
| MODIFICADO | `TRLImmersiveCombatMedicinePlugin.cs` | `BepInPlugin` versão `1.13.6` → `1.14.0`. |

## PA-NN-MM resolvidos durante o build

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | A — Gap · 🟡 | Escopo expandido para incluir XP de HP restaurado (`ExpForHeal`), além de `HealExperience` por efeito — decisão do usuário. |
| PA-01-02 | A — Gap · 🟡 | `FindEffectForRead` unificou a leitura — 1 chamada de reflection por efeito (existência + `HealExperience`), não 2. |
| PA-01-03 | C — Erro de lógica · 🟢 | Campo morto `_subscribedPatientHcForXp` não existe no código final — `_subscribedPatientHc` reusado para os 2 eventos novos. |
| PA-01-04 | C — Erro de lógica · 🟢 | `FindEffectForRead` usa os tipos "de leitura" (`_heavyBleedType`/`_lightBleedType`/`_fractureType`), preservando os "concretos" só para `RemoveEffectNative`. |
| PA-01-05 | A — Gap · 🟢 | `Helpers/HealXpCredit.cs` — único ponto de crédito para os 2 mecanismos × 2 caminhos. |
| PA-02-01 | B — Edge case · 🟢 | Aceito como fiel ao vanilla (não filtra HP por origem/membro) — corner case documentado na spec funcional, sem mudança de código. |

## Validação de build

- `dotnet build "TRL-ImmersiveCombatMedicine.csproj" -c Release -o ../builds/client` — **Build succeeded, 0 Warning(s), 0 Error(s)**. Saída confinada a `mods/TRL-ImmersiveCombatMedicine/builds/client/` — **não instalado** em `D:/SPT` (restrição explícita do usuário).
- `node scripts/check-packet-hashes.js` — **✓ Nenhuma colisão de hash CRC-16** após o rename `V2` → `V3` (os 4 avisos reportados são pré-existentes, sobre `modded-V2(channel)`, não relacionados a este item).

## Mudanças posteriores

### `/apply-code-review` rodada 01 (2026-09-09)

Achados aplicados: CR-01-01 (🟠), CR-01-02 (🟢). Nenhum rejeitado/pulado.

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `modded-V3(review)/Patches/Medical/MedicalLogic.cs` | CR-01-01 — `ResolvePendingConsumeFromReport` passa a retornar `bool` (achou/consumiu a entrada pendente ou não). |
| MODIFICADO | `modded-V3(review)/Patches/Medical/BandAidNetworkHandler.cs` | CR-01-01 — `OnTreatmentReportReceived` só chama `HealXpCredit.CreditHealXp` quando `consumed == true`, fechando o corner case de report duplicado/reenviado creditando XP 2x. CR-01-02 — nova sobrecarga `LogVersionMismatch(packetName, extraNote)`. |
| MODIFICADO | `modded-V3(review)/Patches/Medical/LegacyPackets.cs` | CR-01-02 — `WarnOnce` ganhou sobrecarga com nota; registro do stub `BandAidTreatmentReportPacketV2` usa a nota "(formato retirado na 1.14.0 — não confundir com o legado ≤1.10.0...)". |

Rebuild após aplicação: `dotnet build -c Release -o ../builds/client` — **0 Warning(s), 0 Error(s)**.

## Pendências de validação in-game (não bloqueiam o build, bloqueiam "entregue-validado")

- [ ] Curar aliado local (bot) com sangramento/fratura/intoxicação/dor: médico ganha o mesmo XP que o self-heal do mesmo efeito.
- [ ] MedKit sem nenhum efeito ativo credita XP de HP (`ExpForHeal`) mesmo sem efeito resolvido.
- [ ] Mesmos cenários via rede (Caminho B) — peer remoto.
- [ ] Self-heal do médico não duplica XP.
- [ ] Paciente (aliado) não ganha XP (nem efeito, nem HP).
- [ ] Kit resolvendo múltiplos efeitos + HP na mesma aplicação soma XP corretamente.
- [ ] Cura gradual longa (MedKit ao longo do tempo) não perde XP de HP por truncamento por tick.
- [ ] Peer com build anterior a 1.14.0 na mesma raid: stub `BandAidTreatmentReportPacketV2` descarta o pacote sem travar a fila de eventos.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-09 | Build concluído via `/code-mod` — 0 erros/0 warnings; validação in-game pendente (usuário instala manualmente). |
| 2026-09-09 | Aplicação de 2 achados de code-review 01 via `/apply-code-review` — IDs: CR-01-01, CR-01-02. Rebuild 0 erros/0 warnings. |
