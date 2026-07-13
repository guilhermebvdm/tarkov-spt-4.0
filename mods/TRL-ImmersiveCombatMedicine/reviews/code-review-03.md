# TRL-ImmersiveCombatMedicine — Code Review 03 (delta + lentes g-review-content)

> **Data:** 2026-07-12<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme (workflow 3 dimensões × verificação adversarial, 6 agentes; lentes /g-review-content)<br>
> **Referências:** [code-review-02.md](./code-review-02.md), [../docs/coop-heal-matrix.md](../docs/coop-heal-matrix.md)<br>

---

**Escopo:** delta `f5b7f931` (fixes do CR-02) + `a6fb9939` (feature membro-alvo), mais consistência código↔artefatos↔memória (lente g-review-content). **Autorização do usuário: aplicar tudo** — 23 aplicados, 2 deferidos com justificativa.

**Contadores:** 🟠 3 · 🟡 11 · 🟢 11 (sem bloqueadores)

## Tabela-resumo (formato g-review-content)

| Prio | # | Achado | Lente | Esforço | Status |
|---|---|---|---|---|---|
| 🟠 | CR-03-01 | MigrateOrphanedConfigKeys não é one-time: o órfão nunca é removido e o BepInEx persiste Orphane | 🔴 | ? | ✅ |
| 🟠 | CR-03-02 | Requisito de deploy da coop-heal-matrix perdeu a nuance nova: pacotes mudaram de formato — agor | 🟢 | pequeno | ✅ |
| 🟠 | CR-03-03 | P-2.3 aberta em sessions.md afirma que 'desmaio não é propagado aos peers' — contradiz o código | 🟡 | trivial | ✅ |
| 🟡 | CR-03-04 | CancelNativePatientEffect usa CancelApplyingItem, que é RemoveMedEffect 'blanket' (Common, todo | 🔴 | ? | ✅ |
| 🟡 | CR-03-05 | TraumaFaintPacket mudou o formato de wire (2 floats novos) sem versionamento — DLL antiga e nov | 🟢 | ? | ✅ |
| 🟡 | CR-03-06 | OnTreatmentReportReceived não confere PatientProfileId contra o paciente atualmente exibido no  | 🟢 | ? | ✅ |
| 🟡 | CR-03-07 | coop-heal-matrix: células (d)/(f) e a linha de 'relacionados CR-01' ainda descrevem G-1/G-2/CR- | 🟡 | pequeno | ✅ |
| 🟡 | CR-03-08 | P-2.7 aberta pede 'avaliar trocar filtro para GInterface376' — o código já usa GInterface376 de | 🟡 | trivial | ✅ |
| 🟡 | CR-03-09 | P-2.1, P-2.6 e P-2.8 seguem abertas embora a validação parcial registrada em P-2.9 (itens 1-7,  | 🟡 | pequeno | ✅ |
| 🟡 | CR-03-10 | CR-01-20 sem nenhuma decisão marcada, mas o código implementa exatamente a sugestão (flag Nativ | 🟡 | trivial | ✅ |
| 🟡 | CR-03-11 | PROPRIEDADES.md não documenta a mudança de identidade da key 'Sistema de Braços' nem a migração | 🟢 | pequeno | ✅ |
| 🟡 | CR-03-12 | TreatmentReport não confere paciente/item da cura corrente — report atrasado do paciente A pint | 🟢 | pequeno | ✅ |
| 🟡 | CR-03-13 | Report que chega DEPOIS do HideUI re-popula o status num canvas inativo — próxima ShowUI exibe  | 🟢 | pequeno | ✅ |
| 🟡 | CR-03-14 | Briga de escrita no BarOutline: UpdateLimb zera effectColor a 4Hz por cima do pulso âmbar — fli | 🟢 | pequeno | ✅ |
| 🟢 | CR-03-15 | ShowTreatment(Common) com destaque ativo deixa pulso órfão no membro do heal anterior; UpdateLi | 🔴 | ? | ✅ |
| 🟢 | CR-03-16 | Tooltips do PROPRIEDADES.md divergem em micro-texto dos Config.Bind reais ('regra única' vs 'me | 🔵 | pequeno | ✅ |
| 🟢 | CR-03-17 | Footer do HUD hardcoda '[Pressione F] Fechar Examinador' — contradiz o default Hold do MedicInt | 🟡 | pequeno | ✅ |
| 🟢 | CR-03-18 | Linha ClearTreatment mal indentada no caminho DeactivateMedicMode (in-delta a6fb9939) | 🔵 | trivial | ✅ |
| 🟢 | CR-03-19 | Aprovação (CanUseItem no handshake) e aplicação (FindSmartTarget após UseTime) avaliadas em mom | 🟢 | pequeno | ⏸️ |
| 🟢 | CR-03-20 | Wire-format do TraumaFaintPacket mudou (+2 floats) e um pacote novo foi adicionado sem nenhum v | 🟢 | medio | ⏸️ |
| 🟢 | CR-03-21 | Nome do item troca no meio da cura remota: status inicial usa ShortName.Localized(), o report u | 🔵 | pequeno | ✅ |
| 🟢 | CR-03-22 | Reflection GetProperty("BodyPart") é desnecessária — result já é tipado IEffect e a interface e | 🔵 | trivial | ✅ |
| 🟢 | CR-03-23 | Notificação 'Tratamento Completo.' não expõe a parte tratada nem o HP curado — informação que a | 🔵 | trivial | ✅ |
| 🟢 | CR-03-24 | PartLabelPt duplica os labels literais de CreateLimbBlock — duas fontes de verdade para os nome | 🔵 | pequeno | ✅ |
| 🟢 | CR-03-25 | Indentação quebrada do ClearTreatment no abort do timeout — única das 5 inserções fora do nível | 🔵 | trivial | ✅ |

## Achados

### CR-03-01 · 🔴 Erro factual · 🟠 strong

**MigrateOrphanedConfigKeys não é one-time: o órfão nunca é removido e o BepInEx persiste OrphanedEntries no Save — a migração roda TODO boot e re-clobbera 'Sistema de Braços' com o valor antigo**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/TRLImmersiveCombatMedicinePlugin.cs:173` (dim: correcao) · **Veredito:** CONFIRMED

**Problema:** A migração copia o valor da key mojibake ('Sistema de BraÃ§os') mas não remove a entrada de OrphanedEntries. No BepInEx real do jogo (D:/SPT/BepInEx/core/BepInEx.dll, decompilado): ConfigFile.Save() escreve as OrphanedEntries de volta no .cfg (`.Concat(OrphanedEntries.Select(...))`) e ConfigFile.Reload() as repopula a cada startup. Resultado: a key quebrada fica no arquivo para sempre e MigrateOrphanedConfigKeys reexecuta em TODO Awake, sobrescrevendo ConfigArmsEnabled com o valor antigo (false, segundo o commit). Se o usuário reabilitar 'Sistema de Braços' via F12 e reiniciar, o valor volta a false silenciosamente — exatamente a classe de sintoma que o CR-02 (review 02, achado da migração de encoding) queria consertar.

**Sugestão:** Após copiar o valor, remover o órfão antes do Save: guardar a chave e chamar `orphans.Remove(def)` (IDictionary.Remove funciona com a ConfigDefinition como chave) — isso também purga a linha mojibake do .cfg no Save() seguinte, tornando a migração de fato one-time. Ajustar o comentário/log. Mecânica de reflection em si está correta (property privada achada por AccessTools, DictionaryEntry OK, Section/Key públicos) e a ordem antes de DebugBotInvis

**Decisão:**
- [x] Aceitar sugestão (autorização global do usuário: "tudo")

**Resolução:** ✅ Aplicado em 2026-07-12. orphans.Remove(def) antes do Save — migração de fato one-time; key mojibake purgada do .cfg.

---

### CR-03-02 · 🟢 Gap · 🟠 strong

**Requisito de deploy da coop-heal-matrix perdeu a nuance nova: pacotes mudaram de formato — agora TODAS as máquinas precisam da MESMA build, não só 'ter o mod'**

**Local:** `mods/TRL-ImmersiveCombatMedicine/docs/coop-heal-matrix.md:33` (dim: consistencia) · **Veredito:** CONFIRMED

**Problema:** f5b7f931 adicionou DurationSeconds/GraceSeconds ao TraumaFaintPacket (TraumaFaintPacket.cs:15-16, Serialize/Deserialize com 2 floats novos) e a6fb9939 criou o BandAidTreatmentReportPacket (registrado em BandAidNetworkHandler.CheckInit:63). A seção '❗ Requisito de deploy' da matriz só exige o mod instalado em todas as máquinas — não exige a mesma build. Com builds mistas: máquina com build nova recebendo TraumaFaintPacket antigo → reader.GetFloat() estoura (buffer underrun) no deserialize; máquina com build antiga recebendo BandAidTreatmentReportPacket → pacote não registrado → exatamente a ParseException 'que descarta o resto do batch de eventos de rede daquele frame' descrita pela própria matriz (linha 35). E o plugin segue [BepInPlugin ... "1.0.0")] (TRLImmersiveCombatMedicinePlugin.cs:1

**Sugestão:** Atualizar a seção de deploy da matriz: 'todas as máquinas com a MESMA build do mod' + nota de que a partir de 2026-07-12 o formato do TraumaFaintPacket mudou e existe pacote novo (BandAidTreatmentReportPacket) — build antiga em qualquer ponta quebra faint sync e report. Registrar linha no Histórico de Alterações. Opcional (fora do doc): bump de versão do plugin ao empacotar, conforme feedback_version_increment_on_release.

**Decisão:**
- [x] Aceitar sugestão (autorização global do usuário: "tudo")

**Resolução:** ✅ Aplicado em 2026-07-12. Matriz atualizada: MESMA BUILD obrigatória (wire-format).

---

### CR-03-03 · 🟡 Inconsistência · 🟠 strong

**P-2.3 aberta em sessions.md afirma que 'desmaio não é propagado aos peers' — contradiz o código, que propaga desde a wave 5 e foi refinado pelo próprio delta**

**Local:** `mods/TRL-ImmersiveCombatMedicine/memory/sessions.md:16` (dim: consistencia) · **Veredito:** CONFIRMED

**Problema:** FikaBridge.SyncFaintStatus envia TraumaFaintPacket aos peers desde CR-01-02 (wave 5, a37b82d7) e o delta f5b7f931 ainda refinou o mecanismo (duração viaja no pacote + guard de autoridade IsYourPlayer||IsAI — FikaBridge.cs:19-28). A pendência P-2.3 descreve o estado de 2026-07-11 como se fosse atual. f5b7f931/a6fb9939 editaram sessions.md (P-2.10, P-2.11, P-2.9) sem fechar P-2.3.

**Sugestão:** Fechar P-2.3 com nota de resolução: 'RESOLVIDA por CR-01-02 (wave 5 a37b82d7) + CR-02-01/02 (f5b7f931: duração no pacote, guard de autoridade, bot wake emite false); validação in-game coberta por P-2.9(b)'.

**Decisão:**
- [x] Aceitar sugestão (autorização global do usuário: "tudo")

**Resolução:** ✅ Aplicado em 2026-07-12. P-2.3 fechada com nota (resolvida por CR-01-02+CR-02).

---

### CR-03-04 · 🔴 Erro factual · 🟡 medium

**CancelNativePatientEffect usa CancelApplyingItem, que é RemoveMedEffect 'blanket' (Common, todos os MedEffects) — o abort cancela também a automedicação em curso do próprio bot paciente**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/MedicHealPatch.cs:133` (dim: correcao)

**Problema:** No AHC real (ilspycmd, scratchpad): GClass3010.CancelApplyingItem() → RemoveMedEffect() → method_19(EBodyPart.Common, effect is MedEffect) → ForceResidue() em TODOS os MedEffects existentes do paciente, em qualquer body part (AHC_real.cs:3642-3671; GClass3010.cs:113-116). Bots usam o MESMO pipeline DoMedEffect/MedEffect para os meds deles (comentário do próprio patch, MedicHealPatch.cs:204-208; Player.cs:19553). Cenário realista: médico (host) cura um bot ferido que está se automedicando no mesmo instante — qualquer um dos 5 caminhos de abort força-residua TAMBÉM o MedEffect próprio do bot (ForceResidue seta Bool_2=true → Residue() pula as curas e aplica a lógica de interrupção/consumo parcial do item do bot). Colateral secundário: heal #2 abortado no mesmo paciente mata o MedEffect ainda 

**Sugestão:** Cancel direcionado em vez de blanket: guardar o IEffect retornado por DoMedEffect (mesmo campo _currentPatientEffect que o CR-02-10 deferido já sugere para o bridge) e no abort chamar ForceResidue() só nessa instância via reflection (método público na base GClass3008). Fallback para CancelApplyingItem apenas se a referência se perdeu.

**Decisão:**
- [x] Aceitar sugestão (autorização global do usuário: "tudo")

**Resolução:** ✅ Aplicado em 2026-07-12. Cancel direcionado: _currentPatientEffect guardado no redirect e ForceResidue() só nessa instância; blanket vira fallback logado.

---

### CR-03-05 · 🟢 Gap · 🟡 medium

**TraumaFaintPacket mudou o formato de wire (2 floats novos) sem versionamento — DLL antiga e nova na mesma raid geram ParseException e perda dos demais eventos de rede do frame (nota de deploy obrigatória)**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/TraumaFaintPacket.cs:23` (dim: correcao)

**Problema:** O hash de roteamento do NetPacketProcessor é o nome do tipo (FNV-1 de 'Band_Aid.TraumaFaintPacket') — idêntico entre versões. Peer com DLL ANTIGA recebendo pacote novo: Deserialize lê string+bool e deixa 8 bytes; ReadAllPackets loopa `while (reader.AvailableBytes > 0)` (Fika.Core/Networking/LiteNetLib/Utils/NetPacketProcessor.cs:135-141) e o segundo ReadPacket lê um hash de lixo → ParseException (linha 88). Peer NOVO recebendo pacote antigo: GetFloat estoura os bounds do reader. Em ambos os casos a exceção não é capturada em lugar nenhum da cadeia ProcessEvent→PollEvents (LiteNetManager.cs:1428-1441 sem try/catch): ela escapa do FikaClient/FikaServer.Update e os eventos pendentes RESTANTES daquele frame são descartados — desync além do próprio faint.

**Sugestão:** Registrar como nota de deploy: TODOS os peers (incluindo headless) precisam atualizar o mod na mesma janela — nenhuma raid com DLL mista. Estrutural (opcional): renomear o tipo do pacote a cada mudança de layout (muda o hash → versão velha só loga 'Undefined packet' em vez de corromper o reader) ou incluir um byte de versão no início do payload.

**Decisão:**
- [x] Aceitar sugestão (autorização global do usuário: "tudo")

**Resolução:** ✅ Aplicado em 2026-07-12. Aplicado via doc: requisito MESMA BUILD em todas as máquinas gravado na matriz coop (versionamento de pacote real = dívida).

---

### CR-03-06 · 🟢 Gap · 🟡 medium

**OnTreatmentReportReceived não confere PatientProfileId contra o paciente atualmente exibido no HUD — report atrasado pinta membro/status no HUD do paciente errado e sobrevive à troca de alvo**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidNetworkHandler.cs:710` (dim: correcao)

**Problema:** O handler valida só o DoctorProfileId. O pacote carrega PatientProfileId mas ele é ignorado ao chamar ShowTreatment. Como o report chega no FIM do tratamento remoto (após ApplyFullTreatmentLocally no paciente), há janela real para: (a) o médico já ter trocado o HUD para o paciente B → a linha '► ITEM → MEMBRO' e o pulso âmbar aparecem sobre os blocos de membro de B com o membro de A; (b) o HUD já ter sido fechado (HideUI → ClearTreatment) → ShowTreatment re-arma _treatmentActive/_treatmentText num canvas inativo e o estado stale reaparece na PRÓXIMA ShowUI de qualquer paciente. Como o destaque é mantido no sucesso por design, o visual errado persiste até o próximo Clear.

**Sugestão:** Antes do ShowTreatment, validar o alvo do HUD: expor o ProfileId do _targetPlayer no BandAidUI (ex.: `public string CurrentTargetProfileId`) e retornar se `packet.PatientProfileId` não bater ou se o canvas estiver inativo. Alternativa: passar o patientId para ShowTreatment e deixar a própria UI decidir.

**Decisão:**
- [x] Aceitar sugestão (autorização global do usuário: "tudo")

**Resolução:** ✅ Aplicado em 2026-07-12. Identidade do report: só pinta o HUD se PatientProfileId == paciente examinado (ActivePatientProfileId); pós-HideUI descarta.

---

### CR-03-07 · 🟡 Inconsistência · 🟡 medium

**coop-heal-matrix: células (d)/(f) e a linha de 'relacionados CR-01' ainda descrevem G-1/G-2/CR-01-01 como problemas ativos, contradizendo a própria tabela de gaps (✅) e o código**

**Local:** `mods/TRL-ImmersiveCombatMedicine/docs/coop-heal-matrix.md:27` (dim: consistencia)

**Problema:** A tabela de gaps marca G-1 ✅, G-2 ✅, G-5 ✅ (linhas 42-46) e o Histórico registra CR-01-01/02 aplicados (linha 64), mas o corpo da matriz não foi atualizado: célula (d) do cenário 3 ainda diz 'host-player sofre fallthrough de cirurgia (G-1)' (linha 25), a linha (f) inteira ainda diz 'ícones de bleed/fratura ❌ (G-2)' (linha 27), e a linha 49 apresenta CR-01-01 no presente ('nunca funciona') sem o ✅ que os demais receberam. A linha (f) também ignora o feedback de membro-alvo novo (a6fb9939), que mudou o que o médico vê no HUD.

**Sugestão:** Passar ✅/nota nas células afetadas: (d) cenário 3 → 'G-1 aplicado — receptor não-paciente retorna cedo'; (f) → 'HP/ECG ✅ · ícones de efeito ✅ (G-2 aplicado) · feedback do membro-alvo via BandAidTreatmentReportPacket (a6fb9939), validação P-2.11'; linha 49 → marcar CR-01-01/02/04 como aplicados (validação pendente). Adicionar linha no Histórico.

**Decisão:**
- [x] Aceitar sugestão (autorização global do usuário: "tudo")

**Resolução:** ✅ Aplicado em 2026-07-12. Células (d)/(f) e linha de relacionados atualizadas ao código atual.

---

### CR-03-08 · 🟡 Inconsistência · 🟡 medium

**P-2.7 aberta pede 'avaliar trocar filtro para GInterface376' — o código já usa GInterface376 desde CR-01-08; o residual real já virou CR-02-07 deferido**

**Local:** `mods/TRL-ImmersiveCombatMedicine/memory/sessions.md:19` (dim: consistencia)

**Problema:** MedicHealPatch.cs:99 já filtra `if (!(effect is GInterface376))` com comentário 'ref: CR-01-08' — a ação proposta pela P-2.7 foi executada na sessão autônoma (CR-01-08 ✅). O problema residual do bridge (efetivamente morto para a família MedKit por timing MedKitStartDelay+useTime vs WaitForSeconds) já está catalogado como CR-02-07 deferido em reviews/code-review-02.md. A pendência descreve código que não existe mais e propõe trabalho já feito.

**Sugestão:** Fechar P-2.7 com nota: 'filtro corrigido para GInterface376 por CR-01-08 (sessão autônoma); residual de timing do bridge registrado como CR-02-07 (deferido — observar logs no teste in-game)'.

**Decisão:**
- [x] Aceitar sugestão (autorização global do usuário: "tudo")

**Resolução:** ✅ Aplicado em 2026-07-12. P-2.7 fechada (CR-01-08 já aplicado; resíduo → deferidos CR-02/03).

---

### CR-03-09 · 🟡 Inconsistência · 🟡 medium

**P-2.1, P-2.6 e P-2.8 seguem abertas embora a validação parcial registrada em P-2.9 (itens 1-7, editada pelo delta a6fb9939) cubra exatamente seus critérios**

**Local:** `mods/TRL-ImmersiveCombatMedicine/memory/sessions.md:13` (dim: consistencia)

**Problema:** a6fb9939 registrou em P-2.9 que o usuário aprovou 'prompt/distância' (critério da P-2.1 — prompt no ActionPanel nativo), 'cura 1×' (critério da P-2.6 — HP do bot subindo e HpResource caindo UMA vez) e '2ª cura sem mão travada' (critério da P-2.8), mas as três pendências (linhas 14, 17 e 18) permanecem abertas sem cross-reference nem atualização — o mesmo edit que gravou a validação não as tocou.

**Sugestão:** Fechar P-2.1/P-2.6/P-2.8 referenciando a validação parcial de P-2.9 (itens 1-7), ou — se a intenção era mantê-las até a Parte 2 com 2º PC — anotar em cada uma 'critério solo-host aprovado em P-2.9 (2026-07-12); resta cenário coop'. Verificar se manter aberto foi intencional.

**Decisão:**
- [x] Aceitar sugestão (autorização global do usuário: "tudo")

**Resolução:** ✅ Aplicado em 2026-07-12. P-2.1/P-2.6/P-2.8 anotadas como VALIDADAS (aprovação dos itens 1-7 pelo usuário).

---

### CR-03-10 · 🟡 Inconsistência · 🟡 medium

**CR-01-20 sem nenhuma decisão marcada, mas o código implementa exatamente a sugestão (flag NativeMedEffectApplied + skip no HealRoutine) — e o delta ainda a reforçou**

**Local:** `mods/TRL-ImmersiveCombatMedicine/reviews/code-review-01.md:578` (dim: consistencia)

**Problema:** A sugestão do CR-01-20 ('setar uma flag ex.: MedicHealPatch.NativeEffectApplied e o HealRoutine pular ApplyTreatment/consumo') está implementada literalmente: MedicHealPatch.NativeMedEffectApplied (MedicHealPatch.cs:39, setada na linha 346) e o skip no HealRoutine (BandAidController.cs ~625: 'MedEffect nativo aplicado no paciente — ApplyTreatment programático pulado'), introduzidos pelo fix Salewa (25be6540, ANTERIOR ao commit do review). O delta f5b7f931 ainda estendeu o mecanismo (CancelNativePatientEffect reseta a flag nos aborts — CR-02-03). É o caso 'vice-versa': código reflete o achado, artefato diz pendente.

**Sugestão:** Marcar CR-01-20 como aplicado com nota: '✅ Já implementado pré-review pelo fix Salewa (25be6540: NativeMedEffectApplied + skip no HealRoutine); caminho de abort coberto por CR-02-03 (CancelNativePatientEffect). Validação in-game via P-2.9 (cura 1× aprovada).'

**Decisão:**
- [x] Aceitar sugestão (autorização global do usuário: "tudo")

**Resolução:** ✅ Aplicado em 2026-07-12. CR-01-20 marcado ✅ resolvido fora do fluxo (flag NativeMedEffectApplied).

---

### CR-03-11 · 🟢 Gap · 🟡 medium

**PROPRIEDADES.md não documenta a mudança de identidade da key 'Sistema de Braços' nem a migração one-time (CR-02-04) — o ledger só cobre ShoulderTap**

**Local:** `mods/TRL-ImmersiveCombatMedicine/PROPRIEDADES.md:53` (dim: consistencia)

**Problema:** f5b7f931 adicionou MigrateOrphanedConfigKeys() (TRLImmersiveCombatMedicinePlugin.cs:157-184): a key 'Sistema de BraÃ§os' (bytes mojibake) mudou de identidade para 'Sistema de Braços' no CR-01-06 e o valor órfão do usuário é migrado uma vez no Awake. PROPRIEDADES.md — que se declara 'fonte única de verdade' das configs e cuja sugestão do próprio CR-02-04 pedia 'registrar a quebra em PROPRIEDADES.md' — não tem linha sobre a key renomeada, a entrada órfã antiga que fica no .cfg, nem a migração automática; o Histórico de Alterações não ganhou linha nova (convenção do repo: toda edição de config documenta).

**Sugestão:** Adicionar linha num ledger 'Renomeadas/Migradas' (ou na tabela Removidas): key antiga 'Sistema de BraÃ§os' (encoding quebrado) → 'Sistema de Braços' em 2026-07-12 (CR-01-06/CR-02-04), com migração one-time do valor órfão no Awake; a entrada antiga permanece órfã no .cfg (inofensiva). Registrar no Histórico de Alterações.

**Decisão:**
- [x] Aceitar sugestão (autorização global do usuário: "tudo")

**Resolução:** ✅ Aplicado em 2026-07-12. Seção 'Renomeadas (migração automática)' criada no PROPRIEDADES.md.

---

### CR-03-12 · 🟢 Gap · 🟡 medium

**TreatmentReport não confere paciente/item da cura corrente — report atrasado do paciente A pinta membro errado durante a cura do paciente B (mesmo padrão do G-5 do handshake)**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidNetworkHandler.cs:706` (dim: gaps)

**Problema:** OnTreatmentReportReceived valida apenas DoctorProfileId == mainPlayer.ProfileId. PacketProfileId do paciente e ItemTemplateId não são conferidos contra o estado corrente do médico (HUD alvo / _targetPatient / item em uso). O G-5 da coop-heal-matrix (resposta do handshake sem conferir ItemTemplateId) foi corrigido com exatamente essa checagem em BandAidController.cs:83 — o pacote novo reintroduz o mesmo padrão sem o guard.

**Sugestão:** Passar a identidade na chamada: ShowTreatment(string patientProfileId, EBodyPart part, string itemName) com guard interno `if (_targetPlayer == null || _targetPlayer.ProfileId != patientProfileId) return;` — o packet já carrega PatientProfileId. Opcionalmente conferir também ItemTemplateId contra o item em uso no BandAidController (mesma forma do fix G-5). Esse mesmo guard resolve também o finding do report pós-HideUI (HUD fechado → _targetPlayer

**Decisão:**
- [x] Aceitar sugestão (autorização global do usuário: "tudo")

**Resolução:** ✅ Aplicado em 2026-07-12. Coberto pela identidade do report (mesmo fix do achado da dimensão correção).

---

### CR-03-13 · 🟢 Gap · 🟡 medium

**Report que chega DEPOIS do HideUI re-popula o status num canvas inativo — próxima ShowUI exibe tratamento stale de outra cura**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidUI.cs:688` (dim: gaps)

**Problema:** ClearTreatment é chamado em HideUI (BandAidUI.cs:655) e nos 5 aborts — mas ShowTreatment não verifica se o HUD está ativo, e ShowUI (BandAidUI.cs:641-650) não limpa o estado. Se o médico fecha o HUD antes do report voltar (latência do relay paciente→host→médico), o ShowTreatment tardio escreve _treatmentText, seta _treatmentActive=true e captura _treatmentOutlineOriginal num canvas desativado. Na próxima ShowUI (qualquer paciente), o texto stale ('► SALEWA → PERNA ESQ.') reaparece e UpdateTreatmentHighlight volta a pulsar o membro reportado — sem nenhuma cura em andamento — até o próximo HideUI/abort.

**Sugestão:** Duas linhas: (a) guard de HUD ativo/identidade no ShowTreatment (mesma mudança do finding do PatientProfileId — `_targetPlayer == null → return`); (b) ClearTreatment() no início de ShowUI, já que cada abertura é uma sessão nova de exame (o 'kept on success' vale só enquanto o HUD segue aberto).

**Decisão:**
- [x] Aceitar sugestão (autorização global do usuário: "tudo")

**Resolução:** ✅ Aplicado em 2026-07-12. Coberto: ActivePatientProfileId é null fora do modo médico → report descartado.

---

### CR-03-14 · 🟢 Gap · 🟡 medium

**Briga de escrita no BarOutline: UpdateLimb zera effectColor a 4Hz por cima do pulso âmbar — flicker periódico e máscara do outline de membro destruído**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidUI.cs:1052` (dim: gaps)

**Problema:** Update() chama UpdateTreatmentHighlight() (linha 741) todo frame e, nos frames em que o tick de 250ms dispara (linha 743+), UpdateLimb roda DEPOIS e sobrescreve BarOutline.effectColor do membro em destaque com Color.clear (ou COL_DESTROYED). A última escrita vence na renderização: o pulso apaga por 1 frame a cada 250ms (blink perceptível a 4Hz). Inverso também: enquanto o pulso está ativo, o vermelho de membro destruído (0 HP) fica mascarado pelo âmbar nos outros frames, e RestoreLimbOutline devolve uma cor capturada que pode estar defasada (membro destruído durante o tratamento restaura Color.clear em vez de COL_DESTROYED).

**Sugestão:** Fazer UpdateLimb ceder o campo enquanto há destaque: `if (!(_treatmentActive && part == _treatmentPart)) limb.BarOutline.effectColor = destroyed ? COL_DESTROYED : Color.clear;`. Com isso _treatmentOutlineOriginal pode ser eliminado — ClearTreatment não precisa restaurar nada, o próximo tick de UpdateLimb recalcula o valor correto (destroyed ou clear) sozinho. Bônus: o pulso pode usar `Color.Lerp(destroyed ? COL_DESTROYED : Color.clear, COL_TREAT,

**Decisão:**
- [x] Aceitar sugestão (autorização global do usuário: "tudo")

**Resolução:** ✅ Aplicado em 2026-07-12. UpdateLimb pula o membro em destaque (e mantém a cor-base p/ restauração).

---

### CR-03-15 · 🔴 Erro factual · 🟢 minor

**ShowTreatment(Common) com destaque ativo deixa pulso órfão no membro do heal anterior; UpdateLimbBlock disputa o BarOutline com o pulso (write de estado a 4 Hz)**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidUI.cs:693` (dim: correcao)

**Problema:** Sequência: heal #1 termina com sucesso (destaque mantido por design) → HealRoutine #2 chama ShowTreatment(Common, item2). Com part==Common o bloco de ativação é pulado, mas _treatmentActive continua true e _treatmentPart continua no membro do heal #1 — RestoreLimbOutline restaura a cor por um frame e UpdateTreatmentHighlight volta a pulsar o membro ANTIGO enquanto o texto mostra o item novo com '...'. Só se corrige quando o part real resolve. Adicional: UpdateLimbBlock (BandAidUI.cs:1052) escreve `BarOutline.effectColor = destroyed ? COL_DESTROYED : Color.clear` a cada 0.25s DEPOIS do pulso no mesmo Update → frame de flicker 4×/s durante o tratamento, e `_treatmentOutlineOriginal` capturado 1× pode restaurar cor obsoleta se o estado destroyed do membro mudar durante o heal (auto-corrige em

**Sugestão:** Em ShowTreatment com part==Common e _treatmentActive: RestoreLimbOutline() + _treatmentActive=false (placeholder não deve manter destaque). Em UpdateLimbBlock, pular o write do BarOutline quando `_treatmentActive && part == _treatmentPart` — e usar essa cor de estado (destroyed/clear) como novo `_treatmentOutlineOriginal` em vez da captura única.

**Decisão:**
- [x] Aceitar sugestão (autorização global do usuário: "tudo")

**Resolução:** ✅ Aplicado em 2026-07-12. ShowTreatment(Common) restaura o outline anterior e desativa o highlight; UpdateLimb não sobrescreve mais o pulso (atualiza a cor-base).

---

### CR-03-16 · 🔵 Melhoria · 🟢 minor

**Tooltips do PROPRIEDADES.md divergem em micro-texto dos Config.Bind reais ('regra única' vs 'mesma regra'; '≤2 s' inexistente no código; acentos que o F12 real não mostra)**

**Local:** `mods/TRL-ImmersiveCombatMedicine/PROPRIEDADES.md:41` (dim: consistencia)

**Problema:** O doc se declara fonte única de verdade, mas 3 tooltips não batem literalmente com o código: 'Medic Interact Distance' — doc '(regra única)' vs código '(mesma regra)' e doc acentuado vs código sem acentos (Plugin.cs:61); 'Medic Interact Key' — doc acentuado vs código sem acentos (Plugin.cs:53); 'Invisivel para Bots' — doc diz 'Atirar num bot re-agroa por ≤2 s' mas o código diz só 'Atirar num bot re-agroa' e inclui parêntese '(remove do registro de players da IA + apaga memoria de inimigo)' que o doc omite (DebugBotInvisibility.cs:38). O usuário que compara o F12 com o doc vê textos diferentes.

**Sugestão:** Alinhar: ou copiar as tooltips verbatim do código para o doc (e corrigir acentos no código, que rendem bem no F12 em UTF-8), ou anotar no cabeçalho do doc que as tooltips são a versão 'canônica' e o código deve convergir. Verificar se a paráfrase foi intencional.

**Decisão:**
- [x] Aceitar sugestão (autorização global do usuário: "tudo")

**Resolução:** ✅ Aplicado em 2026-07-12. Micro-textos alinhados na seção Renomeadas/histórico (tooltips conferidos).

---

### CR-03-17 · 🟡 Inconsistência · 🟢 minor

**Footer do HUD hardcoda '[Pressione F] Fechar Examinador' — contradiz o default Hold do MedicInteractMode e a tecla configurável documentados em PROPRIEDADES.md**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidUI.cs:366` (dim: consistencia)

**Problema:** O texto do rodapé afirma 'Pressione F', mas MedicInteractMode default é Hold (segurar — Plugin.cs:55 e PROPRIEDADES.md:38) e a tecla é rebindável (MedicInteractKey). Após CR-01-15 atualizar a tooltip da key para o fluxo novo, o footer ficou como último texto de UI com instrução divergente (pré-delta, mas visível na mesma tela que o delta a6fb9939 alterou).

**Sugestão:** Montar o texto do footer dinamicamente a partir de MedicInteractKey/MedicInteractMode ('[Segure F] Fechar' quando Hold), ou no mínimo trocar o literal para refletir o default Hold. Verificar se intencional (pode haver plano de trocar o default para Press).

**Decisão:**
- [x] Aceitar sugestão (autorização global do usuário: "tudo")

**Resolução:** ✅ Aplicado em 2026-07-12. Footer dinâmico no ShowUI: tecla+modo reais da config (Segure/Duplo/Pressione).

---

### CR-03-18 · 🔵 Melhoria · 🟢 minor

**Linha ClearTreatment mal indentada no caminho DeactivateMedicMode (in-delta a6fb9939)**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidController.cs:883` (dim: consistencia)

**Problema:** Das 5 inserções de BandAidUI.Instance?.ClearTreatment() do commit a6fb9939, a do bloco `if (_isHealingInProgress)` dentro de DeactivateMedicMode ficou com 12 espaços num bloco de 16 (linhas vizinhas 882/884) — as outras 4 estão corretas. Sem efeito funcional (C#), mas destoa visualmente e sugere linha fora do bloco.

**Sugestão:** Reindentar a linha 883 para 16 espaços, alinhada às vizinhas.

**Decisão:**
- [x] Aceitar sugestão (autorização global do usuário: "tudo")

**Resolução:** ✅ Aplicado em 2026-07-12. Indentação corrigida.

---

### CR-03-19 · 🟢 Gap · 🟢 minor

**Aprovação (CanUseItem no handshake) e aplicação (FindSmartTarget após UseTime) avaliadas em momentos diferentes — ferida resolvida no intervalo degrada para fallback Chest: item consumido sem efeito e report mascarando o no-op**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidNetworkHandler.cs:343` (dim: gaps)

**Problema:** TryAnswerForLocalBot aprova via MedicalLogic.CanUseItem no instante do handshake; ApplyFullTreatmentLocally só roda após UseTime (3-16s) + latência, e FindSmartTarget re-decide o alvo sem re-validar. As duas funções usam a MESMA ordem de prioridade (heavy→light→fracture→pior HP, mesma iteração de Enum.GetValues), então 'aprova pelo braço, trata a perna' não ocorre num estado congelado — a divergência é temporal: se a ferida que motivou a aprovação sumir no intervalo (bot se automedicou, outro médico, bleed expirou), FindSmartTarget cai no fallback Chest (linhas 423/426) e o item é aplicado num tórax possivelmente cheio: RemoveEffectNative loga 'sem efeito', heal=0, e o SendTreatmentReport novo ainda mostra '► SALEWA → TÓRAX' para o médico.

**Sugestão:** Aceitável como comportamento base (a prioridade espelhada entre os dois lados é correta), mas fechar a aresta do no-op: em ApplyFullTreatmentLocally, re-rodar CanUseItem(patient, stats) antes de aplicar; se falhar, pular a aplicação e enviar o report com BodyPart=Common e HealedAmount=0 (a infra do pacote já existe) para o médico ver '...'/'sem efeito' em vez de um membro inventado pelo fallback Chest.

**Decisão:**
- [x] Rejeitar (deferir): Deferido por design: é a família G-3/G-4 (ACK estrutural com revalidação) — item próprio no backlog.

---

### CR-03-20 · 🟢 Gap · 🟢 minor

**Wire-format do TraumaFaintPacket mudou (+2 floats) e um pacote novo foi adicionado sem nenhum versionamento — raid com versões mistas do mod quebra a deserialização do faint-sync**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/TraumaFaintPacket.cs:15` (dim: gaps)

**Problema:** f5b7f931 acrescenta 8 bytes ao TraumaFaintPacket e a6fb9939 registra um tipo de pacote novo (BandAidTreatmentReportPacket). Um peer rodando a DLL anterior deserializa o TraumaFaintPacket com o layout antigo (ProfileId+bool) e deixa 8 bytes sobrando — ou, no sentido inverso, o Deserialize novo chama GetFloat() além do fim do buffer de um pacote antigo e lança dentro do handler de rede do Fika. O BandAidTreatmentReportPacket não registrado no peer antigo é caso mais benigno (hash de tipo desconhecido), mas o faint-sync misto é quebra silenciosa de uma feature de segurança (neutralização de aggro do desmaiado).

**Sugestão:** Mínimo barato: byte de versão de protocolo no início de cada pacote do mod (const PROTOCOL_VERSION), com log de erro explícito ('peer com versão X, esperado Y — atualize o mod') no mismatch. Alternativa zero-código: registrar no UPDATE/release notes que 4.x.y muda protocolo de rede e exige atualização simultânea de todos os peers.

**Decisão:**
- [x] Rejeitar (deferir): Deferido (versionamento real de pacotes); mitigado por doc (MESMA BUILD).

---

### CR-03-21 · 🔵 Melhoria · 🟢 minor

**Nome do item troca no meio da cura remota: status inicial usa ShortName.Localized(), o report usa ItemDatabase.Name hardcoded (fallback 'Unknown')**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidNetworkHandler.cs:710` (dim: gaps)

**Problema:** O placeholder inicial ('► SALEWA → ...') vem do ShortName localizado do jogo (depende do locale do cliente — ex.: 'Bandagem' em PT); quando o report chega, o texto é reescrito com o Name do ItemDatabase do mod ('Bandage', 'Army Bandage', 'Car'...), e GetStats nunca retorna null — item fora do dicionário vira o fallback 'Unknown' (ItemDatabase.cs:77), exibindo '► UNKNOWN → PERNA ESQ.'. O usuário vê o rótulo do MESMO item mudar no meio da mesma cura.

**Sugestão:** Guardar o nome exibido em campo (_treatmentItemName) no primeiro ShowTreatment e, no update vindo do report, atualizar SÓ a parte (ex.: método UpdateTreatmentPart(part) ou ShowTreatment(part, null) preservando o nome anterior quando itemName é null). O ItemTemplateId do packet fica apenas para a checagem de identidade do outro finding.

**Decisão:**
- [x] Aceitar sugestão (autorização global do usuário: "tudo")

**Resolução:** ✅ Aplicado em 2026-07-12. Report envia itemName=null → ShowTreatment preserva o nome mostrado desde o início.

---

### CR-03-22 · 🔵 Melhoria · 🟢 minor

**Reflection GetProperty("BodyPart") é desnecessária — result já é tipado IEffect e a interface expõe BodyPart publicamente (verificado na DLL real)**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/MedicHealPatch.cs:354` (dim: gaps)

**Problema:** A variável é declarada `IEffect result = patientHc.DoMedEffect(...)` (MedicHealPatch.cs:294) e a interface global IEffect da Assembly-CSharp real (D:/SPT/EscapeFromTarkov_Data/Managed, via ilspycmd) declara `EBodyPart BodyPart { get; }`; a base concreta dos efeitos (ActiveHealthController.GClass3008) implementa como `public EBodyPart BodyPart` — ou seja, `appliedPart = result.BodyPart;` compila e funciona direto. A reflection funciona hoje, mas depende de a implementação seguir pública: se um update do EFT mudar para implementação explícita de interface, GetProperty no tipo concreto retorna null e o catch{} silencioso deixa o HUD em '...' permanente nas curas locais com Common — regressão invisível.

**Sugestão:** Substituir o bloco try/catch inteiro por `if (appliedPart == EBodyPart.Common && result != null) appliedPart = result.BodyPart;`.

**Decisão:**
- [x] Aceitar sugestão (autorização global do usuário: "tudo")

**Resolução:** ✅ Aplicado em 2026-07-12. Leitura tipada result.BodyPart.

---

### CR-03-23 · 🔵 Melhoria · 🟢 minor

**Notificação 'Tratamento Completo.' não expõe a parte tratada nem o HP curado — informação que a feature nova já resolve e descarta**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidController.cs:627` (dim: gaps)

**Problema:** Os dois toasts de conclusão (linhas 627 e 637) são genéricos, enquanto o delta passou a conhecer a parte tratada (appliedPart no MedicHealPatch, report remoto com BodyPart+HealedAmount). O HealedAmount do report, inclusive, hoje só aparece no log (BandAidNetworkHandler.cs:709) — nunca na UI. Nota: o item 'log do report no médico' da checklist já existe nessa mesma linha 709, não é gap.

**Sugestão:** Enriquecer o toast com o estado que a UI já tem: 'Tratamento Completo ({PartLabelPt[part]}, +{healed:F0} HP).' — a parte pode vir de um campo estático LastAppliedPart no MedicHealPatch (local) e do packet no caminho remoto; quando desconhecida, manter o texto atual.

**Decisão:**
- [x] Aceitar sugestão (autorização global do usuário: "tudo")

**Resolução:** ✅ Aplicado em 2026-07-12. Toast inclui a parte quando conhecida: 'Tratamento Completo (PERNA ESQ.).'

---

### CR-03-24 · 🔵 Melhoria · 🟢 minor

**PartLabelPt duplica os labels literais de CreateLimbBlock — duas fontes de verdade para os nomes PT dos membros**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidUI.cs:668` (dim: gaps)

**Problema:** a6fb9939 introduziu PartLabelPt com exatamente as mesmas 7 strings que já existem hardcoded nas chamadas de CreateLimbBlock (BandAidUI.cs:346-352). Um ajuste futuro de rótulo (ex.: encurtar 'ESTÔMAGO') precisa ser feito em dois lugares no mesmo arquivo — e o histórico recente do mod inclui exatamente uma rodada de correção de strings PT (mojibake CR-01-06/CR-02).

**Sugestão:** CreateLimbBlock passa a buscar o label em PartLabelPt (remover o parâmetro `string label` e usar `PartLabelPt[part]`), tornando o dicionário a fonte única — as posições continuam nas chamadas.

**Decisão:**
- [x] Aceitar sugestão (autorização global do usuário: "tudo")

**Resolução:** ✅ Aplicado em 2026-07-12. PartLabel() é fonte única — CreateLimbBlock consome o mesmo mapa.

---

### CR-03-25 · 🔵 Melhoria · 🟢 minor

**Indentação quebrada do ClearTreatment no abort do timeout — única das 5 inserções fora do nível do bloco**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidController.cs:883` (dim: gaps)

**Problema:** Das 5 chamadas de ClearTreatment inseridas por a6fb9939 nos caminhos de abort, a da linha 883 ficou com 12 espaços dentro de um bloco de 16 — visualmente parece fora do if/escopo em que de fato está.

**Sugestão:** Re-indentar a linha 883 para 16 espaços, alinhada às vizinhas.

**Decisão:**
- [x] Aceitar sugestão (autorização global do usuário: "tudo")

**Resolução:** ✅ Aplicado em 2026-07-12. Indentação corrigida.

---

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-12 | Guilherme | Criação + aplicação integral autorizada ("tudo"): 23 aplicados, 2 deferidos. |
| 2026-07-13 | Guilherme | Anotação (rodada 04): os 2 deferidos ganharam mitigação parcial pelo CR-05 — a revalidação approve→apply agora tem o CONSUMO pós-aplicação (resta só a faceta visual do membro pré-anim, aceita); o versionamento de pacotes segue dívida (mitigado por MESMA BUILD na matriz). |