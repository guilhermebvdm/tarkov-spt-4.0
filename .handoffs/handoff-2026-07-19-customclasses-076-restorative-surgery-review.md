# Handoff — Review completo do 076 (Restorative Surgery: cirurgia do Médico sem cortar HP máx)

> **Data:** 2026-07-19<br>
> **Autor da sessão:** Guilherme (+ agente)<br>
> **Mods:** `mods/CustomClasses/` **v0.5.1** + `mods/TRL-ImmersiveCombatMedicine/` (ICM) **v1.2.3** — feature CROSS-MOD<br>
> **Commits:** `946c7a74` (feature) · `9eaaae5b` (grafo ICM) · branch `main`, **NADA pushado**<br>

---

## ⚠️ Missão #1 desta sessão — REVIEW COMPLETO do 076

O item 076 (perk **Restorative Surgery** do Médico de Combate) foi **implementado, buildado (0/0), deployado em D:\SPT, e passou por 1 rodada de review adversarial** que achou e corrigiu 1 bloqueador. **Falta: (a) um review independente e mais fundo (é feature coop cross-mod, alto risco), e (b) validação in-game** (nada foi jogado — agente não joga).

Faça o review com o rigor de uma feature coop: **o gate é a classe do OPERADOR, não do paciente**, e o valor viaja entre processos (Fika). O antipadrão mais provável de bug aqui é **confundir operador com paciente** e **timing de sincronização**.

---

## 1. O que a feature DEVE fazer (critérios de aceite — a bússola do review)

O perk faz a cirurgia do **Médico de Combate** (CMS/Surv12 restaurando um membro *blacked*) **NÃO reduzir o HP MÁXIMO** daquele membro pelo resto da raid. Vanilla deixa uma "cicatriz" permanente; o perk restaura o membro ao máximo cheio.

**F12:** `2 · Combat Medic` → `Restorative Surgery — Enabled` (default `true`) + `Restorative Surgery — Max HP penalty mult` (float 0..1, **default 0**). Semântica do mult: `0 = sem cicatriz (retém 100%)` · `1 = penalidade vanilla` · `0.5 = metade da cicatriz`.

**Gate = classe do OPERADOR** (quem usa o kit), resolvida por `ClassIdentities.ClassNameEnOf(doctor)` (local via `SkillMultipliers`, peer via a rota 057; retorna `null` para `IsAI` → bots barrados).

### Critérios de aceite (cada um é um teste in-game)
| # | Cenário | Comportamento esperado |
|---|---|---|
| **AC-1** | Médico (solo) opera o PRÓPRIO membro blacked | membro volta com **HP máx cheio** (com default mult=0). Antes do perk: volta reduzido. |
| **AC-2** | NÃO-Médico (solo) opera o próprio membro | penalidade **vanilla** (membro reduzido). |
| **AC-3 (coop)** | Médico **host** opera membro de um **client** | o membro do client volta **cheio** (autoritativo no cliente do client). |
| **AC-4 (coop)** | Médico **client** opera membro do **host** | o membro do host volta **cheio**. |
| **AC-5 (coop)** | Médico **client A** opera membro do **client B** | o membro do B volta **cheio**. |
| **AC-6 (edge CRÍTICO)** | NÃO-Médico opera um paciente que POR ACASO é Médico | penalidade **vanilla** (o gate é o OPERADOR; o paciente-Médico NÃO deve se beneficiar). |
| **AC-7 (bots)** | Bot faz cirurgia / é operado | **sem** efeito do perk (penalidade vanilla). Nenhum bot afetado. |
| **AC-8** | Slider `Max HP penalty mult` = 1.0 no F12 | comportamento **vanilla** (perk efetivamente off). 0.5 = metade da cicatriz. |
| **AC-9** | CustomClasses instalado, ICM ausente | auto-cirurgia funciona; ally-surgery não existe (sem ICM). Sem crash. |
| **AC-10** | ICM instalado, CustomClasses ausente | cirurgia **vanilla** em tudo (bridge fail-open). Sem crash. |
| **AC-11 (UI)** | Abrir a aba CLASS como Médico | **Restorative Surgery** aparece na lista de perks (grupo Médico), no tooltip e na notificação de raid-start. *(v0.5.1: registrado no `PerksCatalog` como `Flag`; a v0.5.0 tinha o mecanismo mas NÃO exibia o perk.)* |

⚠️ **Como medir AC-1..AC-6 in-game:** operar um membro *blacked* e conferir o **HP máximo** daquele membro na tela de Health (a barra do membro). Cheio = perk funcionou. O `Perk Diagnostics` do F12 NÃO cobre cirurgia (não há linha), então é observação direta da barra de HP do membro.

---

## 2. Design da implementação (arquitetura opção B — decisão do usuário)

**Contexto que o reviewer PRECISA saber:**
- Cirurgia **vanilla é sempre AUTO-cirurgia**: `DoMedEffect` é chamado no `_player.ActiveHealthController` do próprio operador (Player.cs:19553). O `MedEffect` chama `ActiveHealthController.RestoreBodyPart(EBodyPart, float healthPenalty)` (ActiveHealthController:1907) no health controller do paciente.
- **Cura de ALIADO é feature do mod ICM** (`TRL-ImmersiveCombatMedicine`, namespace misto `Band_Aid`/`TRLImmersiveCombatMedicine`). **⚠️ O mod `mods/Band-Aid/` NÃO fica ativo** — foi só a base pra criar o ICM. Use o ICM como fonte de verdade.
- **Health é autoritativo no cliente do PACIENTE** (Fika). O ICM sincroniza por packet (`BandAidHealPacket`, carrega `DoctorProfileId` + `SurgeryPenalty`).

> 🔑 **SEMÂNTICA que quase quebrou a feature (leia com atenção):** o `healthPenalty` do `RestoreBodyPart` **é a FRAÇÃO de HP máximo RETIDA, NÃO a penalidade**: `novo Maximum = Maximum × healthPenalty` (ActiveHealthController:3903). `1.0` = sem cicatriz; `0.0` = membro volta com **1** de máximo. A skill Surgery vanilla empurra esse valor **em direção a 1**. A 1ª versão fazia `penalty × mult` (mult 0 → membro em 1 HP = PIOR caso, o inverso do perk); o review pegou e a fórmula virou **`penalty + (1 - penalty) × (1 - mult)` = `Lerp(1, penalty, mult)`**. **Reconfirme essa fórmula in-game** — é o cerne.

**Arquitetura (opção B):** CustomClasses é dono do "operador é Médico?"; o ICM é dono da cirurgia de aliado e chama a API do CustomClasses por **reflection** (soft-dep, fail-open).

### Os 3 pontos onde a penalidade é aplicada + como cada um é tratado
| # | Ponto | Processo | Como o 076 age |
|---|---|---|---|
| **#1 nativo** | `ActiveHealthController.RestoreBodyPart` (auto-cirurgia) | cliente do paciente (=operador) | `SurgeryPenaltyPatch` (Prefix): se `ExternalHandling` → pula; senão `healthPenalty = Adjust(__instance.Player, healthPenalty)`. Operador = paciente. |
| **#2 ICM envio** | `MedicalLogic.ApplySurgery(doctor, patient, …)` | cliente do MÉDICO | `penalty = CustomClassesBridge.AdjustSurgeryPenalty(doctor, penalty)` → o valor ajustado vai p/ a aplicação local E p/ o `SendHealPacket`. Aplicação envolta em `SetExternalHandling(true/false)`. |
| **#3 ICM recepção** | `ApplySurgeryFromNetwork` / `ApplyFullTreatmentLocally` | cliente do PACIENTE (autoritativo) | corpo envolto em `SetExternalHandling(true/false)` pra o patch nativo #1 **pular** (senão re-ajustaria pela classe do PACIENTE). No full-treatment, resolve o `doctor` do `packet.DoctorProfileId` e re-ajusta (penalty fresco ali). |

**Por que o `SetExternalHandling`:** o #3 aplica `RestoreBodyPart` no `ActiveHealthController` do paciente, o que dispara o patch nativo #1. Sem o flag, o #1 re-ajustaria pela classe do PACIENTE — quebrando o **AC-6** (paciente-Médico operado por não-Médico se beneficiaria por engano). O flag faz o #1 pular; o valor já veio gateado pelo OPERADOR no #2. `_externalHandling` é **bool** (não contador) — ver "Riscos".

---

## 3. Arquivos tocados (mapa para o review)

**CustomClasses** (`mods/CustomClasses/modded/Client/`):
- **`CombatMedicSurgery.cs`** (NOVO) — a API pública (`Adjust`, `SetExternalHandling`) + o patch nativo `SurgeryPenaltyPatch`. **A fórmula do `Adjust` é o ponto #1 do review.**
- `PerksConfig.cs` — `RestorativeSurgeryEnabled` + `RestorativeSurgeryPenalty` (seção Médico).
- `Plugin.cs` — `new SurgeryPenaltyPatch().Enable();`.

**ICM** (`mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/`):
- **`CustomClassesBridge.cs`** (NOVO, namespace `Band_Aid`) — reflection p/ `CombatMedicSurgery` (fail-open).
- `MedicalLogic.cs` — `ApplySurgery`: adjust + `SetExternalHandling` wrap (#2).
- `BandAidNetworkHandler.cs` — `ApplySurgeryFromNetwork`: `SetExternalHandling` wrap (#3); `ApplyFullTreatmentLocally`: resolve doctor do packet + adjust.

---

## 4. Review já feito (NÃO repetir — ir mais fundo)

1 rodada adversarial (agente independente). Resultado: **arquitetura de roteamento CONFIRMADA sólida**; achados aplicados:
- **[BLOQUEADOR] fórmula invertida** (`penalty × mult` → membro em 1 HP). **CORRIGIDO** para `Lerp(1, penalty, mult)`. *(reconfirmar in-game — AC-1)*
- **[FORTE] comentário "RestoreBodyPart(2 args) é só cirurgia" era falso** — há uma regeneração periódica de membro (ActiveHealthController:2256, `healthPenalty = 1f`). Pós-`Lerp`, `Lerp(1,1,mult)=1` → **inócuo**. Comentário corrigido.
- **[MENOR]** coop pode falhar SILENCIOSO se `ClassNameEnOf(doctor)` não resolver (mapa 057 não prefetchado) → aplica vanilla. Mitigado: o `Prefetch` roda no `GameWorld.OnGameStarted` (raid-start), antes de qualquer cirurgia. **Confirmar in-game no coop.**

**O que o novo review deve atacar (mais fundo):**
- **Reentrância do `_externalHandling` (bool, não contador):** existe algum caminho em que `ApplySurgery`/`ApplySurgeryFromNetwork` seja aninhado (uma cirurgia dentro de outra, ou um subscriber de evento de restore que dispare outra cirurgia)? Um `false` interno zeraria o flag cedo. Hoje o agente disse "não ocorre" — **verificar** (ex.: fila de body parts, Band_Aid re-invocando).
- **AC-6 de verdade in-game** (não-Médico opera paciente-Médico) — é o teste que valida o `SetExternalHandling`.
- **Timing coop:** em qual ordem exata o packet chega e o `ApplyFullTreatment` vs `ApplySurgeryFromNetwork` roda? O `MedicalLogic.ApplyTreatment` tem um branch (`ApplyFullTreatment=true`, MedicalLogic.cs:86) — confirmar que os 3 cenários caem no caminho esperado e o doctor resolve no cliente do paciente.
- **Consumo de item / double-apply:** o ICM tem lógica de `ConsumeSafe`/`PendingConsume` e um guard "MedEffect nativo aplicado → ApplyTreatment pulado" (BandAidController:634). Confirmar que o 076 não introduz double-apply nem consumo errado.
- **`RestoreBodyPart` overload:** confirmar que o patch mira só `(EBodyPart, float)` e que não há OUTRO caller relevante além de cirurgia + regen.

---

## 5. Riscos conhecidos deixados no código
- **`_externalHandling` é bool.** Assume que não há cirurgia aninhada/reentrante. Se o review achar um caminho reentrante, virar contador (como o `_depth` do `MedicTiming` no 072, ClassMedicPatches.cs).
- **Acoplamento por reflection** ICM→CustomClasses: se um dos dois renomear `CombatMedicSurgery.Adjust`/`SetExternalHandling` ou o GUID, o bridge degrada silencioso (fail-open → penalty vanilla). Sem crash, mas o perk some sem aviso.
- **Coop silent-fail** se o mapa 057 não resolver o doctor (ver §4 MENOR).

## 6. Achado LATERAL (fora do 076, mas anotado) — bug potencial no 072
O `MedicTiming.BandAidIsRedirecting()` (CustomClasses `ClassMedicPatches.cs`) referencia `Band_Aid.MedicHealPatch` por reflection. O `MedicalLogic` do mod oficial está em `namespace Band_Aid`, mas **confirmar se o `MedicHealPatch` também está** (o `BandAidController` está em `TRLImmersiveCombatMedicine`). Se estiver no namespace errado, o guard do 072 (que impede o Swift Surgeon/Rapid Care de encurtar a cura de um ALIADO) **nunca dispara** → os perks de TEMPO do Médico vazariam para a cura de aliado. **Vale um item de backlog separado se confirmado.**

## 7. Referências (não duplicar)
- Backlog: `mods/CustomClasses/backlog/mod-backlog.md` (item **076**, com o detalhe da entrega).
- Regra de gating de instância (bots): `mods/CustomClasses/docs/class-design.md` (seção Implementação) + memória `reference_customclasses_perk_gating` (auditoria 075).
- Memória de coop: `feedback_coop_multiplayer_sync` · efeitos de peer client-side: `reference_fika_peer_effects_client_side`.
- Forense de DLL implantado (validar build certa antes de debugar): `feedback_verify_deployed_dll_before_debugging` (⚠️ literais de log = UTF-16LE; attr/nomes = UTF-8).
- Decompile EFT curado no scratchpad desta sessão: `ActiveHealthController.cs` (RestoreBodyPart:3891, semântica:3903), `Player.cs` (DoMedEffect:19553).

## 8. Estado geral do backlog (contexto)
Entregues nas sessões recentes, **todos pendentes de validação in-game**: 067 (cor F12), 068 (description/Peladão), 071 (mastery leak), 072 (perks médico), 074 (5 perks inertes/clampados), 075 (auditoria de leak — só doc), **076 (este)** + o balanceamento B5–B18 (deployado). Abertos: 069 (bilinguismo), 070 (review-mod-properties), 073 (perk→buff), + 🟡 antigos (051/054/055/057/058/059) e o bug 013 (botão SKILLS com Menu-Overhaul). **Branch: ~111 commits à frente do remote, nada pushado** (decisão de push pendente).
