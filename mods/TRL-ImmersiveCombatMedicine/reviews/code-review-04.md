# TRL-ImmersiveCombatMedicine — Code Review 04 (delta pós-CR-03)

> **Data:** 2026-07-13<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme (workflow 4 dimensões × verificação adversarial, 8 agentes)<br>
> **Referências:** [code-review-03.md](./code-review-03.md), [../docs/coop-heal-matrix.md](../docs/coop-heal-matrix.md)<br>

---

**Escopo:** commits `2233cd82` (CR-03 fixes), `e723a949` (membro pré-animação), `6be9892d`/`04ed4b26` (CR-04 descarte+desmaio), `575932a4` (range-ready), `17fb4ea6` (CR-05 consumo autoritativo). Pré-condição formal (spec/backlog) indisponível — review ad-hoc do delta, mesmo caveat das rodadas 01-03.

**Contadores:** 🔴 1 · 🟠 2 · 🟡 7 · 🟢 12

## Tabela-resumo

| Prio | # | Achado | Dim | Esforço | Status |
|---|---|---|---|---|---|
| 🔴 | CR-04-01 | CR-05 consumo autoritativo é código morto: RegisterPendingConsume nunca é chamado — médico continua  | consumo | trivial | ✅ resolvido na criação |
| 🟠 | CR-04-02 | TryDiscardOnce assume rejeição síncrona, mas no Fika 2.3.4 só o guard CanExecute do vanilla é síncro | consumo | medio | [ ] pendente |
| 🟠 | CR-04-03 | Item simples no caminho LOCAL continua sem descarte (gate isRemotePatient) — bandagem/tala/esmarch/C | consumo | trivial | [ ] pendente |
| 🟡 | CR-04-04 | DeferredDiscardRoutine sobrevive ao fim da raid (GO do plugin, sessão inteira) — ResetAllState não a | consumo | pequeno | [ ] pendente |
| 🟡 | CR-04-05 | Desligar 'Sistema de Desmaio' no F12 durante um desmaio deixa o jogador preso em Downed (imóvel, inv | desmaio | pequeno | [ ] pendente |
| 🟡 | CR-04-06 | DoStun(2f,1f) do entry é pausado pelo ToggleDowned(true) e RETOMA no wake — ~2-4s de 'tela suja' pós | desmaio | trivial | [ ] pendente |
| 🟡 | CR-04-07 | Conclusão normal da cura não limpa _currentPatientEffect nem NativeMedEffectApplied — referência est | correcao | trivial | [ ] pendente |
| 🟡 | CR-04-08 | coop-heal-matrix não reflete nada do delta: G-3/G-4 sem anotação pós-CR-05, célula (e) obsoleta, not | consistencia | pequeno | [ ] pendente |
| 🟡 | CR-04-09 | PROPRIEDADES.md: 'Duracao do Desmaio' sem a faixa 5-120 e sem o tooltip novo do CR-04 | consistencia | trivial | [ ] pendente |
| 🟡 | CR-04-10 | sessions.md P-2.13(a) valida o comportamento de cura do CR-04 que o CR-05 provou quebrado e substitu | consistencia | trivial | [ ] pendente |
| 🟢 | CR-04-11 | Matching de PendingConsume por (PatientId, TemplateId) FIFO sem nonce: report perdido faz a cura seg | consumo | pequeno | [ ] pendente |
| 🟢 | CR-04-12 | effectCost é cobrado mesmo quando RemoveEffectNative falha silenciosamente (método void, exceção eng | consumo | trivial | [ ] pendente |
| 🟢 | CR-04-13 | ScheduleNetworkedDiscard sem dedup por item: dois agendamentos do mesmo item podem enviar RemoveOper | consumo | pequeno | [ ] pendente |
| 🟢 | CR-04-14 | Loop de renovação DoContusion é no-op para o jogador local desmaiado (DamageCoeff=0 durante o downed | desmaio | pequeno | [ ] pendente |
| 🟢 | CR-04-15 | Bot acorda sem nenhum cooldown de re-desmaio — o flap/juggling que o guard CR-04 eliminou para human | desmaio | pequeno | [ ] pendente |
| 🟢 | CR-04-16 | _expectedTreatmentPart setado antes dos guards da resposta e nunca resetado fora do HealRoutine — hi | correcao | trivial | [ ] pendente |
| 🟢 | CR-04-17 | NOTA: highlight pré-animação pode divergir do membro realmente tratado durante o UseTime (3-16s) — f | correcao | trivial | [ ] pendente |
| 🟢 | CR-04-18 | Footer dinâmico mostra só MainKey — modifiers configurados (ex.: Shift+F) ficam fora da dica de fech | correcao | trivial | [ ] pendente |
| 🟢 | CR-04-19 | code-review-03: deferidos CR-03-19 (revalidação approve→apply) e CR-03-20 (versionamento de pacotes) | consistencia | trivial | [ ] pendente |
| 🟢 | CR-04-20 | CR-01-18 resolvido de fato pelo CR-04 (FaintController.cs deletado) mas segue '[ ] Pendente' no code | consistencia | trivial | [ ] pendente |
| 🟢 | CR-04-21 | sessions.md 'Estado atual' descreve o consumo pré-CR-04 ('DiscardItemNetworked pré-verificado') — pa | consistencia | trivial | [ ] pendente |
| 🟢 | CR-04-22 | Resolução do CR-03-16 ('micro-textos alinhados') não corresponde ao estado atual — tooltips doc↔códi | consistencia | trivial | [ ] pendente |

## Achados

### CR-04-01 · A · 🔴 blocker

**CR-05 consumo autoritativo é código morto: RegisterPendingConsume nunca é chamado — médico continua debitando a estimativa defasada**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/MedicalLogic.cs:78` (dim: consumo) · **Veredito:** CONFIRMED

_Achado independentemente por 2 dimensões (consumo + consistência) — fundido._

**Problema:** O branch de paciente remoto de ApplyTreatment (MedicalLogic.cs:78) ainda chama ConsumeSafe imediatamente com a estimativa local (consumeCost calculado da saúde OBSERVADA, sem custos por efeito). RegisterPendingConsume (MedicalLogic.cs:412) é private e não tem NENHUM call site no repo (grep: única ocorrência é a definição). Consequência em cadeia: _pendingConsumes fica sempre vazio; ResolvePendingConsumeFromReport (chamado em BandAidNetworkHandler.cs:748) sempre cai no log 'Report sem consumo pendente correspondente' e NÃO debita nada; TickPendingConsumes (BandAidController.cs:176) e ClearPendingConsumes (BandAidController.cs:983) são no-ops.

**Por que importa:** É exatamente o bug que o commit 17fb4ea6 declara corrigir (G-3/G-4 'delivered'): paciente cura +27 HP e o kit do médico perde ~1.2; Salewa nunca cobra os 175 do heavy bleed. Todo heal remoto (médico client→qualquer, médico host→client) reproduz o sintoma do teste 2-PCs. O CostAmount do report chega correto ao médico e é ignorado. Não há dupla cobrança (o resolve retorna sem consumir), mas a feature-título do commit está inerte.

**Sugestão:** Trocar a linha 78 por RegisterPendingConsume(doctor, item, patient.ProfileId, consumeCost) — a estimativa atual vira o FallbackCost do timeout de 4s, como o design descreve. Validar em 2 PCs que o log '[CR-05] Consumo pelo report: custo real X' aparece no médico.

**Decisão:**
- [x] Aceitar sugestão

**Resolução:** ✅ Aplicado na criação da review (2026-07-13, build 1.1.0): era regressão de aplicação de edit da própria sessão (par de edits falhou por arquivo-não-lido e só metade foi reaplicada) — RegisterPendingConsume religado no branch remoto; consumo autoritativo ATIVO.

---

### CR-04-02 · B · 🟠 strong

**TryDiscardOnce assume rejeição síncrona, mas no Fika 2.3.4 só o guard CanExecute do vanilla é síncrono — rejeições de vmethod_0/host chegam 1+ frame depois e o retry nunca dispara**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/MedicalLogic.cs:560` (dim: consumo) · **Veredito:** CONFIRMED

**Problema:** Só a falha 'Can't execute operationResult.Value.CanExecute()' invoca o callback SINCRONAMENTE (TraderControllerClass.TryRunNetworkTransaction, scratchpad TraderControllerClass.cs:1545-1556 — o else com FailedResult roda inline antes do return). Tudo que passa do CanExecute vai para vmethod_1, que no Fika é async: ClientInventoryController.HandleOperation e HostInventoryController.HandleOperation fazem 'await Task.Yield()' quando o player está vivo (ClientInventoryController.cs:80-87; HostInventoryController.cs:79-86) — as falhas de RunClientOperation ('LOCAL: hands controller can't perform this operation', :112), de RunHostOperation ('Can't execute {operation}', :173) e a negação do host para operação de client (ServerStatusDelegate) invocam o callback no frame seguinte ou depois. Nesses casos rejected ainda é false no return → TryDiscardOnce retorna true → DeferredDiscardRoutine encerra

**Por que importa:** O mecanismo de retry (4×0.75s) foi a resposta ao log 2-PCs onde 100% dos descartes falhavam; ele só cobre a única falha que é síncrona (item nas mãos), que o defer de mãos-livres já evita. Qualquer outra rejeição real (mãos ocupadas com arma no vmethod_0, host negando a operação do client, dessinc de espelho) volta a deixar o item zerado no inventário silenciosamente — o cenário 'AI-2/Salewa eternos' que o CR-05 quis matar.

**Sugestão:** Usar o Task<IResult> retornado em vez da flag: guardar a Task e, na coroutine, aguardar task.IsCompleted (com cap de ~1s) e tratar task falha OU callback Failed como retry. Alternativa mínima: aguardar 2-3 frames após TryRunNetworkTransaction antes de decidir, capturando o resultado do callback num holder.

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-04-03 · B · 🟠 strong

**Item simples no caminho LOCAL continua sem descarte (gate isRemotePatient) — bandagem/tala/esmarch/CAT/Zagustin/Propital infinitos para host→bot e solo**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/MedicalLogic.cs:500` (dim: consumo) · **Veredito:** CONFIRMED

**Problema:** O commit 6be9892d ('discard-on-depletion now also applies to the LOCAL programmatic path') removeu o gate só dos branches MedKitComponent/ResourceComponent; o branch final de item simples manteve 'if (isRemotePatient)'. Itens sem MedKitComponent — confirmado: MedsItemClass só adiciona MedKitComponent quando template.MaxHpResource > 0 (decompilado do Assembly real), e no SPT DB Bandage/Esmarch/CAT/Splint/Propital/Zagustin têm MaxHpResource=0 — caem nesse branch. No caminho LOCAL (paciente com ActiveHealthController: host médico curando BOT, ou solo), ConsumeSafe é chamado com isRemotePatient=false (linhas 211/343/368/386) → nenhum descarte, nenhum log. O comentário da linha 499 ('sempre descarta') contradiz o código.

**Por que importa:** Host médico curando bots é caminho golden do coop PvE (e solo=host mascara ainda mais): esmarch/bandagem/tala/Zagustin nunca somem — meds de 1 uso infinitos, quebra de economia idêntica à reportada no teste 2-PCs, só que no processo do host. Itens multi-uso (Army Bandage=2, CALOK-B=3, CMS=3, Surv12=9, Alu Splint=5) estão OK porque têm MedKitComponent.

**Sugestão:** Remover o gate: descartar incondicionalmente no branch de item simples (o descarte agora é diferido/networked e seguro no host — HostInventoryController executa e propaga). Ajustar o log para não mencionar 'remoto'.

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-04-04 · B · 🟡 medium

**DeferredDiscardRoutine sobrevive ao fim da raid (GO do plugin, sessão inteira) — ResetAllState não a para e não há guard de GameWorld**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidController.cs:936` (dim: consumo)

**Problema:** O controller vive no GameObject do plugin BepInEx (TRLImmersiveCombatMedicinePlugin.cs:84-85, escolhido justamente por sobreviver à sessão inteira) e ResetAllState (BandAidController.cs:971-1013) só para _activeHealCoroutine — coroutines de descarte agendadas seguem vivas após o fim da raid. A proteção efetiva é 'doctor == null' (fake-null do Unity quando o Player é destruído), que cobre o caso menu: com o Player destruído a rotina dá yield break e TryDiscardOnce nem roda. O risco residual real é a JANELA de teardown (extract/morte → tela de fim → destroy do Player, vários segundos): a rotina pode disparar Discard+TryRunNetworkTransaction contra um mundo em desmontagem — no client, envia RemoveOperation para um host que pode já ter desregistrado o peer; no host, muta o inventário possivelmente DEPOIS do snapshot de save do profile (mutação perdida → kit zerado 'volta' no stash). Exceções

**Por que importa:** O caso capa (raid termina com descarte pendente — comum, já que o descarte espera até 6s+3s de mãos/retries) hoje depende exclusivamente do timing de destruição do Player para não operar sobre estado morto; item de raid antiga nunca é tocado no menu (Item é C# puro mas doctor fake-null bloqueia antes), porém a janela de teardown é alcançável a cada extract durante cura recém-terminada.

**Sugestão:** Capturar Singleton<GameWorld>.Instance no agendamento e abortar (yield break) se Instance mudou ou é null antes de cada tentativa; adicionalmente, guardar as Coroutines agendadas numa lista e StopCoroutine nelas em ResetAllState (mesmo padrão do _activeHealCoroutine).

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-04-05 · B · 🟡 medium

**Desligar 'Sistema de Desmaio' no F12 durante um desmaio deixa o jogador preso em Downed (imóvel, invulnerável, tela preta) — o fallback WakeLocalPlayer do CR-04 é inalcançável nesse caminho**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/TRLImmersiveCombatMedicinePlugin.cs:227` (dim: desmaio)

**Problema:** Com blackout config OFF mid-faint, o MainLoopPatch (MovementPatches.cs:103-107) remove BlackoutTimers/StartTimes SEM acordar; mas o early-return do Plugin.Update ('if (!ConfigMasterEnabled.Value || !ConfigBlackoutEnabled.Value) { ...; return; }', linha 227-232) vem ANTES do fallback novo do CR-04 ('else if (TraumaState.IsFainted) → WakeLocalPlayer', linha 278-282). ToggleDowned(false) só existe em WakeLocalPlayer, logo nunca roda enquanto o toggle estiver off: o jogador fica com IsMoveIgnored/IsAxesIgnored=true e DamageCoeff=0 (FikaPlayer.cs:522/527-528) até re-habilitar a config. O comentário do fallback promete exatamente cobrir 'deadline sumiu por outro caminho — acordar limpo', mas o caminho que o próprio MainLoopPatch cria (limpeza por config off) é inalcançável para ele.

**Por que importa:** Cenário plausível: o jogador desabilita o sistema via F12 no meio de um desmaio bugado justamente para se libertar — e obtém o oposto: softlock em ragdoll Fika (imóvel, imune, tela preta) sem nenhuma saída além de re-ligar o toggle (aí sim o else-if dispara e acorda). Em coop, o host o vê deitado indefinidamente.

**Sugestão:** Mover a checagem de wake para antes do early-return: no início do Update (após obter gameWorld/MainPlayer), se TraumaState.IsFainted && (!ConfigBlackoutEnabled.Value || !ConfigMasterEnabled.Value) → WakeLocalPlayer(gameWorld, localId) e então return. Alternativa equivalente: fazer o branch de limpeza do MainLoopPatch (MovementPatches.cs:103-107) também tratar o humano local via um flag que o Plugin.Update consuma.

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-04-06 · B · 🟡 medium

**DoStun(2f,1f) do entry é pausado pelo ToggleDowned(true) e RETOMA no wake — ~2-4s de 'tela suja' pós-consciência em TODO desmaio, contradizendo o objetivo CR-04 de tela limpa**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/HealthPatches.cs:80` (dim: desmaio)

**Problema:** Sequência confirmada nas fontes: DoStun(2f,1f) roda no ApplyDamageInfo Postfix (DamageCoeff ainda 1 → efeito aplicado; AHC_real.cs:4118-4123: duração = Stun.StunBuildUpTime 0.2s + 2×Player.BlindnessDuration ≈ 2.2s de work + DefaultResidueTime 2f de residue, AHC_real.cs:3092-3096 → timeline total ≈ 4.2s). ≤1 frame depois, Plugin.Update chama ToggleDowned(true) → PauseAllEffects (FikaPlayer.cs:523) congela o stun com ~100% restante; no wake, ToggleDowned(false) → UnpauseAllEffects (FikaPlayer.cs:556) o retoma do zero. Resultado: praticamente toda a timeline do stun (~2.2s cheios + 2s de fade) toca DEPOIS do wake — o mesmo mecanismo que o próprio audit CR-04 mediu com o DoStun(60f) ('conscious but stunned'). Reduzir 60→2s encolheu o residual, mas não o eliminou. Nota: mover o DoStun 'para antes do pause' NÃO resolve — PauseAllEffects snapshota TODOS os efeitos ativos no momento do downed, i

**Por que importa:** Golden path: acontece em todo desmaio com wake. O jogador acorda, recupera controle (fix CR-04 ok), mas passa ~2-4s com blur/zumbido de stun + ~1.6s de contusion — exatamente a 'tela suja no wake' que o CR-04 prometeu limpar. Em coop, isso é a janela em que o grace de 5s deveria dar reação ao jogador.

**Sugestão:** Remover o DoStun do entry (feedback de impacto é imperceptível: no frame seguinte o ToggleDowned corta para DeathFade/FastBlur.Die e o AudioListener cai a 5% — o stun não adiciona nada durante o blackout). Se quiser grogginess deliberada, aplicar o stun em WakeLocalPlayer APÓS ToggleDowned(false) (DamageCoeff já voltou a 1, o guard 'DamageCoeff > 0f' passa) com duração curta configurável — aí ele roda de fato no pós-wake por escolha, não por acidente do pause/unpause. Idem para a 1ª contusion: n

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-04-07 · D · 🟡 medium

**Conclusão normal da cura não limpa _currentPatientEffect nem NativeMedEffectApplied — referência estática órfã atravessa raids e ResetAllState dispara ForceResidue espúrio**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/MedicHealPatch.cs:140` (dim: correcao)

**Problema:** Quando a cura nativa COMPLETA normalmente, nem `NativeMedEffectApplied` nem `_currentPatientEffect` são limpos (o HealRoutine só os reseta no INÍCIO da cura seguinte, linha 574, e o reset da flag não limpa a referência). Duas consequências: (a) se a cura seguinte não cria MedEffect (paciente remoto), a flag fica false com a referência ainda setada — o early-return da linha 140 então NUNCA mais limpa `_currentPatientEffect`, que é campo estático e mantém referência forte ao MedEffect→ActiveHealthController→Player da raid antiga pelo resto da sessão (até outra cura nativa sobrescrever); (b) se nenhuma cura seguinte rodou, `ResetAllState` na troca de raid encontra flag true + efeito JÁ CONCLUÍDO e invoca ForceResidue nele — benigno em efeito (base ForceResidue no-op com State Residued/Removed, AHC_real.cs:550-561; MedEffect.ForceResidue só seta Bool_2, linha 1980) mas loga 'MedEffect DO red

**Por que importa:** Leak de estado entre raids via campo estático (pina o grafo inteiro do Player/AHC do paciente da raid anterior — exatamente o antipattern que a categoria D do repo lista) e log de cancel falso em cada teardown de raid, que suja o diagnóstico de aborts reais durante os testes coop.

**Sugestão:** No caminho de sucesso do HealRoutine (branch NativeMedEffectApplied, BandAidController.cs:634-642) zerar `MedicHealPatch.NativeMedEffectApplied = false` e limpar a referência (expor um `MedicHealPatch.ClearCurrentPatientEffect()` ou mover `_currentPatientEffect = null` para antes do early-return na linha 140, executando a limpeza mesmo quando a flag já está false). Alternativa mínima: limpar ambos em CleanupPatientSubscription, que já roda em todos os fins de cura.

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-04-08 · C · 🟡 medium

**coop-heal-matrix não reflete nada do delta: G-3/G-4 sem anotação pós-CR-05, célula (e) obsoleta, nota MESMA BUILD desatualizada, Histórico parado no CR-03**

**Local:** `mods/TRL-ImmersiveCombatMedicine/docs/coop-heal-matrix.md:44` (dim: consistencia)

**Problema:** (a) O CR-05 implementou exatamente o fix proposto para G-3/G-4 (report do paciente com custo real, cobrança só por efeito DE FATO removido — BandAidNetworkHandler.cs:345-366/398-401) e a matriz segue listando ambos como gaps abertos sem nota — dado o achado do débito não-ligado, o estado correto é "mecanismo entregue, fiação do débito no médico pendente"; (b) célula (e) não menciona consumo por report nem o descarte diferido networked (CR-04/CR-05); "parcial=local-only" continua verdadeiro, mas o CR-04 re-classificou isso como benigno/vanilla-like (mirror de RemainingResource deferido no commit 6be9892d) — a referência a CR-01-23 ficou sem esse contexto; (c) a nota MESMA BUILD cita só as mudanças do CR-03, mas e723a949 (+ExpectedBodyPart no BandAidHealCheckResponsePacket) e 17fb4ea6 (+CostAmount no BandAidTreatmentReportPacket) mudaram o wire de novo — ambos os commits dizem "SAME-BUILD 

**Por que importa:** É o doc canônico de critérios de aceite coop; quem planejar o teste 3-players/headless por ele vai testar contra um estado de código de 2 gerações atrás e re-derivar gaps já endereçados (ou confiar num 'entregue' que ainda não debita).

**Sugestão:** Atualizar células (b)/(e)/(f); anotar G-3/G-4 com o estado real pós-CR-05 (mecanismo entregue via BandAidTreatmentReportPacket.CostAmount; débito no médico pendente do fix da fiação); reescrever a nota de deploy enumerando as mudanças de wire mais recentes (HealCheckResponse/TreatmentReport, 2026-07-13); linhas novas no Histórico.

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-04-09 · C · 🟡 medium

**PROPRIEDADES.md: 'Duracao do Desmaio' sem a faixa 5-120 e sem o tooltip novo do CR-04**

**Local:** `mods/TRL-ImmersiveCombatMedicine/PROPRIEDADES.md:31` (dim: consistencia)

**Problema:** O CR-04 (04ed4b26) adicionou AcceptableValueRange 5-120 (piso anti-flap medido no log do teste 2-PCs) e o aviso "ALINHAR ENTRE TODOS OS PEERS" ao tooltip; o doc — que se declara "fonte única de verdade" e cuja regra do repo é "toda entry atualiza este arquivo" — mantém Faixa "—" e o tooltip antigo, e o Histórico de Alterações não ganhou linha do CR-04. Conferido o restante: nenhuma entry nova ausente (12 entries no doc = 12 Config.Bind no código, incl. '5. Debug').

**Por que importa:** A faixa e o aviso de alinhamento entre peers são exatamente a informação operacional que o admin do servidor coop precisa (config divergente foi a causa-raiz do flap de desmaio no teste); o doc canônico esconde ambos.

**Sugestão:** Faixa `5–120`, tooltip novo verbatim, e linha no Histórico de Alterações (CR-04: piso anti-flap + alinhamento entre peers).

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-04-10 · C · 🟡 medium

**sessions.md P-2.13(a) valida o comportamento de cura do CR-04 que o CR-05 provou quebrado e substituiu**

**Local:** `mods/TRL-ImmersiveCombatMedicine/memory/sessions.md:13` (dim: consistencia)

**Problema:** O CR-05 (17fb4ea6) constatou que a regra de cura do CR-04 nunca funcionou (descarte no fim forçado da animação com item nas mãos → CanExecute rejeitava 100%; regra descartar-sem-subtrair deixava o recurso INTACTO) e a substituiu (sempre subtrai min(custo,restante); descarte diferido por coroutine). O item (a) da P-2.13 continua instruindo validar as builds 23:20/23:28 com o comportamento antigo — duplica e contradiz a P-2.14, que já descreve os testes corretos. Só o item (b) DESMAIO da P-2.13 segue válido. P-2.2 (limpeza [DEBUG-ICM] pendente) conferida: coerente com o código (sondas ainda presentes no Plugin e no Controller).

**Por que importa:** Duas pendências abertas com checklists de cura conflitantes → risco real de a próxima sessão de validação rodar o roteiro obsoleto e tirar conclusão errada (regra de imutabilidade da memória permite anotação, não exige reescrita).

**Sugestão:** Anotar P-2.13: "(a) SUBSTITUÍDA por P-2.14/CR-05 (a mecânica de descarte/consumo do CR-04 nunca funcionou — validar cura SÓ pela P-2.14); (b) desmaio segue válido".

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-04-11 · B · 🟢 minor

**Matching de PendingConsume por (PatientId, TemplateId) FIFO sem nonce: report perdido faz a cura seguinte debitar a instância/custo errados + fallback duplo**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/MedicalLogic.cs:431` (dim: consumo)

**Problema:** No happy path o ReliableOrdered preserva a ordem e o FIFO casa certo (report 1→pending 1). Mas existem caminhos SEM report: TryApplyFullTreatmentOnLocalBot retorna false silenciosamente (bot morreu/virou observado entre o envio e a chegada — BandAidNetworkHandler.cs:781), e ApplyFullTreatmentLocally early-returns em stats==null ou HC não-Active (:301-311). Nesse caso o pending 1 fica vivo; duas curas rápidas no MESMO paciente com o MESMO template (2× AI-2, instâncias A e B) fazem o report da cura 2 resolver o pending 1 → custo real da cura 2 debitado na instância A (errada), e o pending 2 expira aos 4s debitando o fallback na instância B → débito duplo para um único tratamento aplicado. (Hoje inatingível por causa do bloqueador do RegisterPendingConsume — vira real assim que ele for ligado.)

**Por que importa:** Com custos por efeito grandes (Salewa 175) o cruzamento de custo entre kits distintos do mesmo template é visível; o débito duplo (real+fallback) pune o médico. É uma fragilidade de correlação, não de transporte — o versionamento de pacotes deferido não a cobre.

**Sugestão:** Adicionar um OpId (ushort incremental) em BandAidHealPacket e ecoá-lo no BandAidTreatmentReportPacket; casar pending por OpId (wire change — regra MESMA BUILD já aceita no projeto). Alternativa sem wire: casar também por referência de Item e expirar pendentes órfãos mais cedo.

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-04-12 · B · 🟢 minor

**effectCost é cobrado mesmo quando RemoveEffectNative falha silenciosamente (método void, exceção engolida)**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidNetworkHandler.cs:353` (dim: consumo)

**Problema:** HasEffect é checado ANTES no mesmo target (mesmo frame — race desprezível), mas RemoveEffectNative é void e engole falhas: method_15 null (rename em update do EFT), exceção de reflection, ou result null são apenas logados (:505-535) e effectCost soma o custo mesmo assim. O paciente continua sangrando e o médico paga (Salewa: 175). Zagustin validado OK: HealAmount=0 → totalCost=1f independente de effectCost (:386-388).

**Por que importa:** Cobrança sem efeito é o inverso do objetivo do CR-05 ('debitar o que o PACIENTE efetivamente aplicou'). Baixa probabilidade dentro da mesma build (reflection estável), mas quando quebrar, quebra 100% das curas com efeito e o custo mascara o sintoma.

**Sugestão:** RemoveEffectNative retornar bool (method_15 result != null / ForceResidue executado) e condicionar o effectCost += ao retorno true — espelha o padrão já usado no RemoveEffect do caminho local (MedicalLogic.cs:124-165, que só cobra dentro do if).

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-04-13 · B · 🟢 minor

**ScheduleNetworkedDiscard sem dedup por item: dois agendamentos do mesmo item podem enviar RemoveOperation duplicada se a 1ª ainda aguarda aprovação do host**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidController.cs:931` (dim: consumo)

**Problema:** ConsumeSafe agenda descarte sempre que HpResource <= 0.005f (MedicalLogic.cs:483-484) — um segundo uso/débito do mesmo kit já zerado (charge=min(cost,0)=0 → continua <=0.005f) agenda uma SEGUNDA coroutine para o mesmo item. O guard CurrentAddress==null só salva se a 1ª operação já EXECUTOU; no médico client a operação fica pendente de aprovação do host (item ainda com endereço até o Started) — a 2ª TryDiscardOnce passa o guard e o CanExecute e envia RemoveOperation duplicada; o host executa a 1ª e falha a 2ª (item já removido), com risco de rollback/dessinc no pipeline de inventário Fika.

**Por que importa:** Janela pequena (precisa de novo débito no kit zerado antes do descarte aterrissar — ex.: report real + uso encadeado), mas a consequência é justamente a família de sintomas que o CR-04 caçou (espelho fantasma/slot morto). Barato de fechar agora que o descarte é diferido.

**Sugestão:** Manter um HashSet<string> (item.Id) de descartes em voo no BandAidController: ScheduleNetworkedDiscard ignora se já contém; remover do set quando a rotina termina (sucesso, esgota tentativas ou aborta).

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-04-14 · E · 🟢 minor

**Loop de renovação DoContusion é no-op para o jogador local desmaiado (DamageCoeff=0 durante o downed) — o cap CR-04 'para 2s antes do wake' não tem efeito prático para humanos**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/MovementPatches.cs:47` (dim: desmaio)

**Problema:** ToggleDowned(true) seta SetDamageCoeff(0f) (FikaPlayer.cs:522) e DoContusion guarda 'if (base.IsAlive && base.DamageCoeff > 0f)' (AHC_real.cs:4174-4176) — logo TODAS as renovações de 2s do CASO 1 durante o downed são descartadas pelo AHC para o humano local; o visual do blackout vem do DeathFade/FastBlur do Fika, não da contusion. O cap novo do CR-04 ('&& now + 2f <= TraumaState.BlackoutTimers[id]') portanto só governa chamadas que já não faziam nada (humano downed) ou que ninguém vê (bots, DamageCoeff=1 mas sem câmera). A única contusion que DE FATO aplica é a do frame de entrada (LateUpdate do mesmo frame do hit, antes do ToggleDowned do Update seguinte) — e essa é pausada e retoma ~1.6s no wake, o que o cap não previne. Detalhe conexo: WakeLocalPlayer não remove ContusionRenewTimers e o CASO 2 humano (MovementPatches.cs:72) é inalcançável no fluxo normal (Plugin.Update remove Blackout

**Por que importa:** A justificativa do fix CR-04 ('sem o cap, acordava com contusion residual') atribui a 'tela suja' ao renovador errado — o residual real é o stun + a 1ª contusion pré-downed (achado B do stun). Manter o bloco dá falsa sensação de que o visual do blackout depende dele, e o custo/complexidade (dict ContusionRenewTimers + cap) não compra nada.

**Sugestão:** Remover o bloco de renovação DoContusion e o dict ContusionRenewTimers (ou, mínimo: pular DoContusion quando '__instance.IsYourPlayer' — cobrindo também a 1ª chamada pré-downed do frame de entrada, complemento do achado do stun). Se remover o dict, tirar também do ResetAll e do CASO 2.

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-04-15 · F · 🟢 minor

**Bot acorda sem nenhum cooldown de re-desmaio — o flap/juggling que o guard CR-04 eliminou para humanos persiste para bots**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/MovementPatches.cs:85` (dim: desmaio)

**Problema:** No CASO 2 IsAI, o wake do bot remove atomicamente os dois escudos que o guard de re-entrada (HealthPatches.cs:55) consulta: SyncFaintStatus(__instance, false) tira o id de FaintedPlayerIds e GraceTimers.Remove(id) — coerente com CR-01-19 (bot não tem grace). Resultado: no mesmo frame do wake, o próximo hit de chest ≥35 / head ≥10 re-desmaia o bot instantaneamente. Um jogador com arma de dano alto e cadência lenta pode manter um bot em loop desmaio→wake→desmaio indefinidamente (PauseBot/UnpauseBot + prone flap), reproduzindo para bots o ciclo 'alvo preso em ragdoll' que o commit CR-04 declara ter encerrado.

**Por que importa:** Assimetria de design não documentada: humanos ganharam guard blackout+grace, bots ganharam zero janela. Como bots desmaiados continuam levando dano integral (o escudo do Prefix é '!__instance.IsAI'), o abuso é limitado (normalmente o alvo morre), mas o flap visual e a supressão infinita são observáveis em coop.

**Sugestão:** Decidir e registrar: se supressão encadeada de bot é feature, documentar no PROPRIEDADES.md/sessions.md; senão, dar cooldown curto de re-desmaio a bots (ex.: dict BotFaintCooldown[id]=now+8f setado no wake do CASO 2 IsAI e checado no guard do HealthPatches junto com FaintedPlayerIds).

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-04-16 · B · 🟢 minor

**_expectedTreatmentPart setado antes dos guards da resposta e nunca resetado fora do HealRoutine — highlight pré-animação errado na cura seguinte se a coroutine aprovada não iniciar**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidController.cs:107` (dim: correcao)

**Problema:** A atribuição na linha 107 roda em toda resposta Approved, mas o StartCoroutine é condicional (mainPlayer null no fim de raid, ou _pendingHealStats null). Se a HealRoutine não inicia, o membro esperado fica stale no campo — e como ResetAllState não o reseta, sobrevive inclusive à troca de raid. A PRÓXIMA cura via caminho LOCAL (ProcessHeal→HealRoutine direto, sem handshake que sobrescreva o campo) mostra no HUD o membro do paciente remoto ANTIGO como alvo pré-animação, até o method_5 corrigir com a parte real (~1-2s depois, quando o DoMedEffect roda).

**Por que importa:** Feedback visual errado exatamente na feature nova do e723a949 (mostrar o membro ANTES da animação): o médico pode ler 'PERNA ESQ.' de uma cura de outra raid/paciente ao iniciar uma cura local. Janela estreita, mas a correção é de uma linha.

**Sugestão:** Mover a atribuição de `_expectedTreatmentPart` para DENTRO do bloco `if (mainPlayer != null && ...)` que inicia a HealRoutine (ela só tem consumidor ali), e adicionar `_expectedTreatmentPart = EBodyPart.Common;` no ResetAllState junto do reset dos _pendingHeal*.

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-04-17 · F · 🟢 minor

**NOTA: highlight pré-animação pode divergir do membro realmente tratado durante o UseTime (3-16s) — facete visual da dívida aceita approve→apply (família G-3/G-4)**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidNetworkHandler.cs:681` (dim: correcao)

**Problema:** O paciente calcula o alvo esperado no APPROVE, mas a aplicação recalcula FindSmartTarget/GetBlackedPart no fim do UseTime — se o estado mudou no meio (bleed do membro destacado estancou sozinho/por outro médico, novo heavy bleed surgiu em membro de prioridade maior), o médico vê o membro errado pulsando por até UseTime segundos. O TreatmentReport final corrige o HUD (OnTreatmentReportReceived:761) e o CONSUMO usa o custo real do apply (CR-05), então não há efeito mecânico — só o highlight transitório.

**Por que importa:** É a mesma família da revalidação approve→apply explicitamente DEFERIDA/aceita nos reviews 01-03; registrar aqui evita que uma rodada futura reporte como bug novo. Sem gap mecânico: cura, custo e report final são autoritativos do lado do paciente.

**Sugestão:** Nenhuma ação de código. Registrar a aceitação (comentário curto no call site ou linha no coop-heal-matrix: 'expected part é dica de UI, best-effort; report final é a verdade') para ancorar a decisão.

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-04-18 · F · 🟢 minor

**Footer dinâmico mostra só MainKey — modifiers configurados (ex.: Shift+F) ficam fora da dica de fechar o examinador**

**Local:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Medical/BandAidUI.cs:653` (dim: correcao)

**Problema:** O fix CR-03 tornou o footer fiel à keybind/modo reais, mas lê apenas `MainKey`: se o usuário configurar `Shift+F` no F12, o footer exibe '[Segure F]' — e o CheckPressMode (BandAidController.cs:394-402) EXIGE o modifier nesse caso (`hasModifiers → shortcut.IsPressed()`), então a dica exibida não fecha o examinador.

**Por que importa:** Contradiz o espírito do próprio fix (footer refletir a config real); usuário com modifier configurado recebe instrução que não funciona.

**Sugestão:** Compor a string com os modifiers: `string keyLabel = shortcut.Modifiers.Any() ? string.Join("+", shortcut.Modifiers) + "+" + shortcut.MainKey : shortcut.MainKey.ToString();` (mesma semântica do CheckPressMode).

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-04-19 · C · 🟢 minor

**code-review-03: deferidos CR-03-19 (revalidação approve→apply) e CR-03-20 (versionamento de pacotes) sem anotação — o CR-05 mudou o quadro dos dois**

**Local:** `mods/TRL-ImmersiveCombatMedicine/reviews/code-review-03.md:363` (dim: consistencia)

**Problema:** (a) CR-03-19: o CR-05 entregou a metade 'ACK de custo' da família G-3/G-4 — o paciente agora cobra apenas efeitos que existiam e foram removidos (hadHeavy/hadLight/hadFracture antes do RemoveEffectNative, BandAidNetworkHandler.cs:345-366) e o custo real viaja no report; o residual do deferido encolheu para o no-op visual (ferida some no intervalo → fallback Chest + report mascarando) — e a metade 'débito no médico' depende do fix da fiação (achado do RegisterPendingConsume). (b) CR-03-20: o wire mudou DUAS vezes depois do deferimento (e723a949 +ExpectedBodyPart; 17fb4ea6 +CostAmount), sempre sem versionamento — o risco deferido cresceu e a mitigação-por-doc citada aponta para uma nota da matriz que não enumera os pacotes novos.

**Por que importa:** Os deferidos são o registro do que se decidiu NÃO fazer; sem a anotação, a próxima rodada re-analisa do zero ou assume que o escopo deferido não mudou.

**Sugestão:** Nota datada nos dois achados: em CR-03-19, registrar a entrega parcial via CR-05 (+ dependência do fix da fiação do débito); em CR-03-20, registrar as 2 mudanças de wire pós-deferimento e apontar a atualização da nota MESMA BUILD na matriz.

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-04-20 · C · 🟢 minor

**CR-01-18 resolvido de fato pelo CR-04 (FaintController.cs deletado) mas segue '[ ] Pendente' no code-review-01**

**Local:** `mods/TRL-ImmersiveCombatMedicine/reviews/code-review-01.md:539` (dim: consistencia)

**Problema:** Mesmo padrão 'vice-versa' (código reflete o achado, artefato diz pendente) que o CR-03-10 corrigiu para o CR-01-20: a parte principal da sugestão do CR-01-18 foi executada pelo CR-04, mas nem a decisão nem a linha do índice (linha 35) foram marcadas. O residual da sugestão (blocos comentados de torniquete via rede em BandAidNetworkHandler/BandAidUI) pode seguir existindo — anotar como resto, não como bloqueio.

**Por que importa:** O review-01 é o backlog vivo dos itens médios não aplicados; um item já entregue aparecendo como pendente infla o passivo e pode gerar retrabalho.

**Sugestão:** Marcar CR-01-18 ✅ 'resolvido fora do fluxo (CR-04/04ed4b26: FaintController.cs deletado)' + nota do residual (blocos comentados) + ✅ na linha 35 do índice.

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-04-21 · C · 🟢 minor

**sessions.md 'Estado atual' descreve o consumo pré-CR-04 ('DiscardItemNetworked pré-verificado') — padrão que o próprio CR-04 identificou como causa do slot fantasma**

**Local:** `mods/TRL-ImmersiveCombatMedicine/memory/sessions.md:8` (dim: consistencia)

**Problema:** O snapshot 'Estado atual' (que a convenção memory-curation manda manter como delta do estado corrente, não histórico) ainda vende como proteção o padrão Discard pré-verificado — exatamente o que o CR-04 (6be9892d) diagnosticou como CAUSA do slot fantasma/CMS exception (simulate:false detach silencioso) e o CR-05 substituiu por subtração-sempre + descarte diferido networked. A seção também não menciona nada de CR-04/CR-05 (relógio único do desmaio, grace no wake).

**Por que importa:** É a primeira coisa que uma sessão nova lê para calibrar o estado do mod; afirma como 'lição correta' um design já provado errado em teste.

**Sugestão:** Atualizar a linha para o estado CR-05 (ConsumeSafe sempre subtrai; descarte diferido via coroutine networked; consumo por report do paciente quando a fiação for ligada) — ou remover a menção e deixar as pendências P-2.13/P-2.14 contarem a história.

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-04-22 · E · 🟢 minor

**Resolução do CR-03-16 ('micro-textos alinhados') não corresponde ao estado atual — tooltips doc↔código seguem divergentes**

**Local:** `mods/TRL-ImmersiveCombatMedicine/PROPRIEDADES.md:41` (dim: consistencia)

**Problema:** A divergência literal apontada pelo CR-03-16 ('regra única' vs 'mesma regra'; doc acentuado vs código sem acentos nas keys Medic Interact Key/Distance) persiste no par doc/código, mas o review registra o item como aplicado. Pode ser intencional (doc canônico com acentos, código ASCII para o F12) — nesse caso falta a anotação de canonicidade que o próprio achado sugeria.

**Por que importa:** Um ✅ que não bate com o código corrói a confiança no ledger de reviews — exatamente o tipo de deriva que esta lente existe para pegar.

**Sugestão:** Ou alinhar verbatim (aproveitando a edição do achado da Duracao do Desmaio), ou anotar no cabeçalho do PROPRIEDADES.md qual lado é canônico e corrigir a nota de resolução do CR-03-16 para refletir a decisão real.

**Decisão:**
- [ ] Pendente
- [ ] Aceitar sugestão
- [ ] Aceitar com modificação: _________________
- [ ] Rejeitar (deferir / aceitar como dívida): _________________

---

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-13 | Guilherme | Criação (rodada 04). Bloqueador CR-04-01 resolvido na criação (build 1.1.0). Demais achados aguardam decisão. |