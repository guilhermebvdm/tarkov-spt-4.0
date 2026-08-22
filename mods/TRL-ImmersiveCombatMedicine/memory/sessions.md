# Memória de Sessões — TRL-ImmersiveCombatMedicine

## Estado atual

> **Delta 2026-08-17 (Sessão 9):** ICM em **v1.13.5** no workspace [`modded-V3(review)`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-V3%28review%29). Concluída a auditoria técnica exaustiva de 16 de 16 funcionalidades do mod (100% aprovadas e consolidadas em `revisao-geral-consolidada.md`), com formalização do [`reviews/code-review-05.md`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/reviews/code-review-05.md) (6 achados resolvidos, 0 bloqueadores). Principais entregas: (1) unificação arquitetural de namespaces sob `TRLImmersiveCombatMedicine.*` (`.Medical`, `.Trauma`, `.Helpers`, `.Fika`) e desambiguação de `global::Fika.Core`; (2) otimização estrita de GC (GC Alloc = 0) em loops de HUD (`BandAidUI.cs`) e validação corporal (`MedicalLogic.cs`); (3) governança de logs diagnósticos por toggles de debug no menu F12 do BepInEx (`ConfigDebugMedicLogs` e `ConfigDebugPhysicsLogs`); (4) eliminação de 10 warnings de analisador `Harmony003` com `ref DamageInfoStruct` em `HealthPatches.cs`; (5) verificação defensiva de mãos em `HandsStateGuard.cs` contra comidas/bebidas; (6) compilação limpa com 0 Erros e 0 Warnings com isolamento estrito de build.

- Auditoria integral concluída: 16 relatórios detalhados (`revisao-item-01` a `revisao-item-16`) validados contra fontes canônicas (`references/eft-decompiled` EFT 0.16.9, `references/fika-plugin` FIKA 2.3.4, `references/spt-source` SPT 4.0.13).
- Arquitetura de rede isolada em canal confiável (`ReliableUnordered` + magic header `TRLM`), eliminando conflitos de inventário no canal 0 e crashes de `ParseException`.
- Cura e consumo com cálculo autoritativo 1:1 respeitando o saldo real do kit e regra canônica de desesterilização/cancelamento com perda de item após 1.0s.
- GC zero-alloc garantido no loop quente de HUD/física e auditoria de memória em 2 fases via `TraumaPurge`.
- Item 07 (Torniquetes e Necrose por Tempo) documentado e reservado para discussão/implementação dedicada posterior.

## Pendências

- [P-9.1] (aberta 2026-08-17) **VALIDAR IN-GAME a build consolidada v1.13.5 no `modded-V3(review)`** — Cenários a testar: **(1)** Curar aliado até esgotar item e validar descarte síncrono sem slot fantasma nem travamento de mãos; **(2)** Validar duração total da animação médica sem corte prematuro no 1º segundo; **(3)** Testar cancelamento de cirurgia CMS após 1.0s com consumo de 1 carga e toast de desesterilização; **(4)** Verificar menu F12 (seção "12. Debug (Dev)") com toggles `Debug Medic Logs` e `Debug Physics Logs` desligados (console limpo) e ligados (logs diagnósticos emitidos); **(5)** Verificar ausência de micro-stutters/alocações no HUD médico. 🔴 Bloqueador.
- [P-9.2] (aberta 2026-08-17) **Definir especificação e ativação do Item 07 (Torniquetes e Necrose por Tempo)** — Estruturar chave composta `(player, bodyPart)` no `TourniquetManager` e calibrar mecânica de dano progressivo por isquemia. 🟢 Ideia.
- [P-5.1] (aberta 2026-07-26) **VALIDAR IN-GAME a Leva 1 + os itens independentes do Trauma 2.0** — Roteiro: `docs/happy-flow-test-plan.md`. Cenários C1 (desfibrilador sem piscar), C2 (hitbox pós-revive com TRL-Fixes 002), H2 (2 pernas zeradas + analgésico), H8 (log de purga na entrada da raid). 🟡 Débito técnico.
- [P-5.2] (aberta 2026-07-26) **Coleta de `LogOutput.log` das duas máquinas** para destravar TTL do agachar adiado (item 018) e calibragem de clamped legs. 🟡 Débito técnico.
- [P-4.4] (aberta 2026-07-25, PARCIALMENTE FECHADA) Itens residuais do Trauma 2.0: 015 (desmaio desacoplado do Fika), 016 (ação "Acordar" x "Reviver"), 018 (TTL agachamento). 🟡 Débito técnico.

---

## 2026-08-17 01:43 (GMT-3) — Sessão 9: Auditoria Integral dos 16 Itens, Code Review 05, Otimização de GC Zero-Alloc e Unificação de Namespaces (v1.13.4 → v1.13.5)

**Tema central:** Conclusão da auditoria técnica minuciosa de 16/16 itens do mod contra fontes canônicas (`references/eft-decompiled`, `references/fika-plugin`, `references/spt-source`), formalização do Code Review 05 e consolidação do workspace `modded-V3(review)` em v1.13.5 com GC zero-alloc e isolamento de logs diagnósticos no menu F12 do BepInEx.

**Decisões-chave:**
- [CR-05-01 / Unificação de Namespaces]: Unificação estrutural de todos os arquivos sob `TRLImmersiveCombatMedicine.*` (`.Medical`, `.Trauma`, `.Helpers`, `.Fika`), eliminando legados `Band_Aid` e `TrueTrauma`, e desambiguação de tipos com `global::Fika.Core.Main.Players.FikaPlayer`. Ref: [`reviews/code-review-05.md:54`](reviews/code-review-05.md#L54).
- [CR-05-02 / Otimização de GC Zero-Alloc]: Eliminação de alocações na Heap por frame em loops de HUD (`BandAidUI.cs:257` com dicionário estático de reflexão antecipada `_genericFindMethods` e cache de argumentos `_bodyPartArgsCache`) e em verificações corporais (`MedicalLogic.cs:19` com array estático constante `AllBodyParts`). Ref: [`reviews/code-review-05.md:73`](reviews/code-review-05.md#L73).
- [CR-05-03 / Governança de Logs Diagnósticos F12]: Criação da categoria `"12. Debug (Dev)"` no arquivo de configuração do BepInEx (`TRLImmersiveCombatMedicinePlugin.cs:255`) com os toggles `ConfigDebugMedicLogs` e `ConfigDebugPhysicsLogs` (default `false`), eliminando spam de console em gameplay regular e permitindo depuração pontual sob demanda. Ref: [`reviews/code-review-05.md:94`](reviews/code-review-05.md#L94).
- [CR-05-04 / Resolução de Warning Harmony003]: Correção de assinatura em `HealthPatches.cs:15,51` para `ref DamageInfoStruct damageInfo`, eliminando 10 avisos do analisador do BepInEx sobre modificação de struct por valor. Ref: [`reviews/code-review-05.md:115`](reviews/code-review-05.md#L115).
- [CR-05-05 / Verificação Defensiva de Mãos]: Reforço em `HandsStateGuard.cs:13` validando `player.HandsController.Item` contra `MedsItemClass` e `FoodDrinkItemClass` para impedir início concorrente de tratamentos médicos durante consumo e evitar descompasso no cliente Fika. Ref: [`reviews/code-review-05.md:134`](reviews/code-review-05.md#L134).
- [CR-05-06 / Versionamento SemVer 1.13.5 e Isolamento de Build]: Bump de versão para `1.13.5` e compilação do binário `TRLImmersiveCombatMedicine.dll` mantida estritamente dentro de `modded-V3(review)/bin/Release/netstandard2.1/`. Ref: [`reviews/code-review-05.md:153`](reviews/code-review-05.md#L153).

**Lições / hipóteses descartadas:**
- *Reflexão genérica em rotinas de Update de UI:* Executar `method.MakeGenericMethod()` e `new object[] { bodyPart }` a cada 250ms em loops de UI gera dezenas de alocações na Heap por segundo no Unity Mono, provocando micro-stutters periódicos de GC. O padrão correto é pré-instanciar delegates/métodos especializados no `CacheTypes()` e reutilizar arrays pré-alocados por membro (`_bodyPartArgsCache`). Ref: [`reviews/code-review-05.md:73`](reviews/code-review-05.md#L73).
- *Desacoplamento de logs diagnósticos:* Emissão incondicional de `LogInfo` em patches de alta frequência (recomputes de física, checagem de animação `method_5`) polui a saída e degrada o desempenho. Todo log diagnóstico de desenvolvimento deve ser gateado por `ConfigEntry<bool>` dedicado no menu F12.

**Atividade cronológica:**
1. Auditoria exaustiva de 16 funcionalidades do mod com geração dos relatórios técnicos dedicados `revisao-item-01` a `revisao-item-16` validados contra o decompile do EFT 0.16.9 e Fika 2.3.4.
2. Consolidação geral dos resultados em `revisao-geral-consolidada.md` e estruturação do plano em `scripts/plano-implementacao-correcoes-e-debug-f12.md`.
3. Criação do workspace de revisão e refatoração `modded-V3(review)` aplicando as correções dos achados CR-05-01 a CR-05-06.
4. Formalização do Code Review 05 em `reviews/code-review-05.md` (6 achados identificados e 100% resolvidos, 0 bloqueadores).
5. Compilação do projeto com `dotnet build` em `modded-V3(review)` resultando em build verde com 0 Erros e 0 Warnings.

**Pendências abertas nesta sessão:**
- [P-9.1] (aberta 2026-08-17) **VALIDAR IN-GAME a build consolidada v1.13.5 no `modded-V3(review)`** — Cenários a testar: **(1)** Curar aliado até esgotar item e validar descarte síncrono sem slot fantasma nem travamento de mãos; **(2)** Validar duração total da animação médica sem corte prematuro no 1º segundo; **(3)** Testar cancelamento de cirurgia CMS após 1.0s com consumo de 1 carga e toast de desesterilização; **(4)** Verificar menu F12 (seção "12. Debug (Dev)") com toggles `Debug Medic Logs` e `Debug Physics Logs` desligados (console limpo) e ligados (logs diagnósticos emitidos); **(5)** Verificar ausência de micro-stutters/alocações no HUD médico. Categoria: 🔴 bloqueador.
- [P-9.2] (aberta 2026-08-17) **Definir especificação e ativação do Item 07 (Torniquetes e Necrose por Tempo)** — Estruturar chave composta `(player, bodyPart)` no `TourniquetManager` e calibrar mecânica de dano progressivo por isquemia. Categoria: 🟢 ideia.

**Cross-refs:**
- Resolve [P-8.1] de 2026-08-15 através da consolidação na build v1.13.5 com Code Review 05.

---

## 2026-08-15 (GMT-3) — Sessão 8: Eliminação de Colisão de Grid Fika, Animação Integral, Anti-Spam de Cura, Consumo 1:1 e Cancelamento com Punição (v1.13.3 → v1.13.4)

**Tema central:** Diagnóstico aprofundado e resolução de 5 problemas mecânicos/arquiteturais no módulo médico (`BandAid`) no canal de testes [`modded-testchannel`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel).

**Implementações e Descobertas Técnicas:**

1. **Eliminação do Bug de Colisão de Grid e Slots Fantasmas no FIKA (v1.13.3):**
   - **Fast-Path Síncrono Imediato:** Em `MedicalLogic.DiscardItemNetworked`, quando o médico esgota um kit com mãos desocupadas e inventário livre, executa `StartDiscardAttempt` no mesmo frame sem instanciar coroutines desnecessárias nem esperar ticks atrasados.
   - **Eliminação de Timers Bloqueantes:** Em `BandAidController.DeferredDiscardRoutine`, todos os `WaitForSeconds(0.25f/0.2f/0.75f)` foram eliminados e substituídos por checagens a cada frame (`yield return null` ~16ms).
   - **Estabilidade de Mãos em `EmergencyDrop`:** Removida a reflexão instável em `SpawnController`, adotando a transição canônica `doctor.TrySetLastEquippedWeapon(true)`.

2. **Garantia de Animação Completa Sem Interrupção Precoce (v1.13.4):**
   - **Causa Raiz:** No EFT nativo (`Player.cs:19615`), `ObservedMedsControllerClass.method_8` é o callback do `EffectRemovedEvent`. Em autocura, `method_8` verifica a fila de partes do médico (`method_4()`); ao curar um aliado com efeito pontual rápido (Light Bleed de 1s ou cura rápida de 5 HP), o evento disparava, via `method_4() == false` e invocava imediatamente `method_9()`, cancelando a animação no 1º segundo.
   - **Fix:** Criado o patch `MedsMethod8Patch` em `MedicHealPatch.cs` que intercepta `method_8` com guard de ownership (`operationOwner == localMainPlayer && BandAidHealActive`) e suprime a chamada precoce, permitindo que a animação dure o `totalUseTime` completo até a finalização suave via `ForceFinishAnimation`.

3. **Bloqueador de Cura Concorrente e Anti-Spam (v1.13.4):**
   - **Causa Raiz:** Durante a janela de 1 a 3 segundos de espera pela resposta do handshake de rede (`_pendingHealTimeout > 0f`), `CheckManualInputs()` continuava rodando todos os frames (`!_isHealingInProgress`), permitindo spam de hotkeys e sobreposição de coroutines.
   - **Fix:** Inseridos guards fail-early em `CheckManualInputs()` e `ProcessHeal()` em `BandAidController.cs` bloqueando requisições quando `_isHealingInProgress`, `_pendingHealTimeout > 0f`, `HandsController is Player.MedsController` ou `InventoryController.IsInventoryBlocked()`. Limpeza imediata de `_pendingHealTimeout = -1f` no `OnHealCheckResponseHandler`.

4. **Consumo Rigoroso 1:1 de HP e Sincronização de Saldo no Coop (v1.13.4):**
   - **Causa Raiz:** No EFT vanilla (`ActiveHealthController.cs:1825-1841`), 1 ponto de recurso do MedKit (`HpResource`) restaura exatamente 1 ponto de HP do membro, respeitando o `HpResourceRate` (`stats.HealAmount`). No coop remoto, o paciente não recebia o saldo disponível do médico e curava dano total mesmo se o kit tivesse pouco recurso.
   - **Fix:** Transmitido o saldo disponível real do kit do médico (`AvailableResource`) dentro do campo existente `HealAmount` em `BandAidHealPacketV2` (sem alterar o layout binário do envelope, mantendo 100% de compatibilidade FIKA). No paciente remoto (`ApplyFullTreatmentLocally`), deduz-se primeiro o custo fixo de efeitos (sangramentos/fraturas) e limita-se a cura de HP estritamente ao saldo restante: $\text{heal} = \min(\text{availableForHp}, \text{stats.HealAmount}, \text{hpNeeded})$.

5. **Regra Canônica de Cancelamento com Punição (v1.13.4):**
   - **Regra EFT:** No EFT (`ActiveHealthController.cs:1945`), `ItemRemoveAfterInterruptionTime = 1.0s`.
   - **Fix:** Registrado `_healStartTime = Time.time` no início de `HealRoutine`. Em `CancelHealInProgress()`, se o cancelamento ocorrer com `elapsed >= 1.0s`, consome 1 carga de kits cirúrgicos (CMS/Surv12), talas/bandagens ou consumíveis via `MedicalLogic.ConsumeSafe(doctor, savedItem, 1.0f)` por desesterilização/abertura sem aplicar o efeito no paciente, exibindo a notificação localizada `TreatmentCancelledWithItemLoss` (`MedicLocale.cs`). Se cancelado antes de 1s, o cancelamento é limpo.

6. **Versionamento e Build:**
   - Versão SemVer atualizada para **1.13.4** em `TRL-ImmersiveCombatMedicine.csproj` e `TRLImmersiveCombatMedicinePlugin.cs`.
   - Compilação validada com `dotnet build` (**0 erros**).

---

## 2026-08-02 (GMT-3) — Sessão 7: Isolamento no Canal 3 FIKA, Cópia modded-testchannel e Guard de Mãos

- **Análise Forense de Log (`LogOutput.log`)**:
  - Correlacionados os erros de `hands controller can't perform this operation` (Linha 3066) e `item Default Inventory is currently being modified` (Linhas 3129, 3474) aos travamentos em jogo ao apertar "R" e ao tentar pegar o colete Tarbank.
- **Cópia de Testes**:
  - Criada a cópia `mods/TRL-ImmersiveCombatMedicine/modded-testchannel/` preservando o `modded/` original intacto.
- **Implementações na cópia `modded-testchannel/`**:
  - **`HandsStateGuard.cs`** (`Helpers/HandsStateGuard.cs`): Adicionada checagem defensiva `CanPerformInteraction(player)` para validar se `player.HandsController.Item` não é consumo/medicina antes de iniciar ações do mod.
  - **Canal 3 Isolado no FIKA** (`Patches/Medical/BandAidNetworkHandler.cs`): Atualizado o envio de pacotes `SendPacket<T>` para usar `DeliveryMethod.ReliableUnordered` (desvinculando os pacotes customizados da fila do Channel 0 de inventário) e configurada a constante de Magic Header `TRLM` (`0x4D4C5254`).
- **Validação de Build**:
  - Compilado `TRL-ImmersiveCombatMedicine.csproj` em `modded-testchannel/` com **0 Erros**.

---

## 2026-07-26 (GMT-3) — Sessão 6: 1º TESTE IN-GAME de fato + Leva 1 de correções (v1.12.0)

**Tema central:** O overhaul Trauma 2.0 foi finalmente jogado em coop Fika (Guilherme como **client**, Umbigo como **host**) — fecha parcialmente a P-4.4, aberta desde 2026-07-19. O teste parou no meio (44+12 cenários é impraticável) e produziu 13 achados, que viraram os itens 013-021 + `TRL-Fixes` 002. Nesta sessão foram entregues 013, 014, 017, 020, 021 e o fix do Fika; 015/016 (desmaio desacoplado) e 018/019 aguardam o próximo teste e o log.

**Descobertas técnicas (as mais valiosas da sessão):**
- **A coluna "sem analgésico" da matriz de pernas é, na prática, VANILLA.** `Player.UpdateSpeedLimitByHealth` põe todo o castigo de perna dentro de `if (!OnPainkillers)`: bloqueia sprint e aplica cap mais severo que o alvo do mod, e a composição do dicionário de limites é por **mínimo** — o vanilla sempre ganha. Os defaults N1=80%/N2=55% só se manifestam **com** analgésico, onde o vanilla não aplica nada e libera o sprint. Registrado na §1.2 da matriz de comportamento; leitura de `clamped=` no log virou verificação obrigatória. **Consequência para calibrar qualquer coisa de velocidade: medir em qual coluna o número vive antes de mexer nele.**
- **S1.1 ("liberar correr com a perna doendo") é INVIÁVEL** sem o mod forçar `CanSprint = true` acima do vanilla (`MovementContext.CanSprint` retorna false para perna zerada **ou** quebrada; só `OnPainkillers` escapa, com `return true` antes dos checks de perna). Recusado pelo usuário — decisão 18 preservada. Não reabrir como bug.
- **Bug do Fika, não nosso: hitbox perdida no revive.** `ReviveInteractable.RemoveRagdoll` devolve a hierarquia à layer `Player` e nunca repromove os `BodyPartCollider` para `HitCollider`; `HitMask` contém `Deadbody` e `HitCollider`, **não** `Player`. O `Player.Init` do vanilla nunca separa esse par (layer na linha 28646, `SetupHitColliders()` na 28647). Corrigido em `TRL-Fixes` 002, patchando `RemoveRagdoll` (não `RevivePlayer`) porque o defeito é **por observador** e há dois caminhos: quem revive passa por `ReviveInteractable.cs:240`, os demais peers por `ObservedPlayer.cs:1327`.
- **Hipótese REFUTADA, registrar para não voltar:** troca de equipamento **não** restaura a hitbox. `RecalculateEquipmentParams` reativa as placas de armadura mas nunca chama `SetupHitColliders` — não existe auto-recuperação, a hitbox fica quebrada até o fim da raid. Isso deixa a divergência relatada (bots acertavam, jogador não) **sem explicação no código**; o cenário C2 mede as duas fontes separadamente.
- **O item piscando no inventário tem nome:** operação de inventário parada em `CommandStatus.Begin` sem `Succeed`/`Failed` → `ItemView` liga `IsBeingRemoved` e nunca desliga (`ItemView.cs:578,596` / `SlotView.cs:555-573`), e `IsInteractive` exclui essa flag — daí "pisca **e** não pode usar **e** o slot fica morto". Era o padrão que o CR-04/CR-05 já havia abandonado no sistema de cura; o revive era o último caller nele.
- **As falas de dor do mod antigo, em boa parte, NUNCA tocaram.** `VoiceHelper.SafePlayVoice` resolvia o trigger por `Enum.Parse(typeof(EPhraseTrigger), nome)` dentro de um `catch {}` VAZIO, e 3 dos 4 nomes usados **não existem** no enum: `OnLegBroken`, `OnHandBroken`, `OnPain`. Resultado: metade das falas de perna/braço (o sorteio 50/50) e **todas** as de estômago eram silêncio absoluto, sem nada no log. O "my arm is fucked" que o usuário lembra era o `OnAgony` genérico. **Nomes corretos: `LegBroken` (49) e `HandBroken` (48)** — e o jogo os usa para FRATURA (`BotMemoryClass.cs:648,652`, reagindo ao efeito de fratura), não para membro zerado; os dois estão no menu de voz do jogador em "HEALTH STATUS" (`GesturesMenu.cs:469`), o que prova que há clipe. **Lição transversal: nunca resolver enum do EFT por string** — gatilho tipado transforma nome errado em erro de compilação em vez de silêncio.
- **O jogo já pune usar a perna ferida, e o mod antigo punia a coisa errada.** `InflictSelfDamage` (`Player.cs:31140-31159`): 2 HP por perna comprometida a cada 1-1,5s **enquanto sprinta** — e como o vanilla bloqueia sprint sem analgésico, esse dreno só ocorre **com** analgésico. Pousar de pulo: 3 HP por perna (`:31228-31243`), e o jogo **já grita** ali com o gate `!OnPainkillers && !IsAI`. **Levantar não custa nada no vanilla** — era exatamente o que o mod antigo punia (30% fratura / 70% dano). Decisão do usuário: não duplicar; decisão 21 preservada.
- **Os dois tremores coexistem por design** (§5.5 da matriz): o do mod ancora no braço ferido de propósito, para não colidir com o tremor de estimulante; o nativo vem do efeito `Pain`, que chama `AddEffect<Tremor>(EBodyPart.Head)` **sempre**, independente do membro ferido. Analgésico oculta o ícone de dor mas não remove o Tremor já criado. Não é o UnderFire.

**Decisões-chave do usuário:**
- Desmaio: **desacoplar 100% do Fika** (sem `ToggleDowned`), **vulnerabilidade total**, e desmaiado que zera o HP **cai no coma do Fika** em vez de morrer — o desmaio nunca mata sozinho.
- **Duas ações distintas** sobre o caído: desmaio → **"Acordar"/"Reanimar"** sem item; coma → **"Reviver"** com desfibrilador. Fecha o CR-01-21 pela raiz e reusa o pipeline de ações que o mod já tem (`MedicInteractable` + postfix em `GetAvailableActions`), sem item novo nem pipeline novo.
- **A camada de invisibilidade à IA no desmaio está CORRETA** — confirmado em jogo pelo usuário, depois de eu ter diagnosticado errado que estava inerte. Não tocar em `BotPatches`/`AggroHelper`/`FaintedPlayerIds`.
- `Z2 + analgésico` → Manqueira Severa com sprint bloqueado (era Leve); o analgésico passa a só suprimir o agachar.
- Falas de dor: mapa completo com triggers nativos dedicados, **sem** falas em bots.
- Persistência entre raids: **não mexer no vanilla**; só tornar a purga do mod verificável.

**Achado de processo grave — trabalho pronto e nunca implantado:** o fonte do ICM estava em **1.11.0** (commit `98733981`, envelope de todos os 6 pacotes Fika, tipos com sufixo `V2`) mas `.last-compile-versions` registrava **1.10.0** — ou seja, **o teste do usuário rodou 1.10.0, sem esse trabalho**. Lição: `git log` do fonte não prova o que está no jogo; conferir `.last-compile-versions` **antes** de interpretar resultado de teste. E como a 1.11.0 mudou wire format, os dois peers têm de atualizar juntos.

**Lições / hipóteses descartadas:**
- **Diagnóstico com o usuário é iterativo, não one-shot.** Duas premissas minhas caíram por informação que só ele tinha: (1) que a IA ignorava o desmaiado (ela vê e atira depois do revive, e o comportamento durante o desmaio está certo); (2) que o problema de dano no desmaio era só o escudo do mod. Perguntar "o que exatamente você observou" antes de propor arquitetura economizou refazer o item 015.
- **Um code-review no próprio código pegou o achado mais sério da sessão:** o patch novo resolvia tipo do Fika por nome sem `[BepInDependency("com.fika.core")]` — a ordem de carga do BepInEx é indeterminada e a falha seria *silenciosa*, logando "Fika nao encontrado" e se disfarçando de "Fika não instalado". O ICM tinha o mesmo buraco latente e funcionava por acidente de ordenação. Padrão do repo (DiscordRaidMap, MOAR-Client) já declarava; nós não.
- **Auditoria de estado precisa distinguir "vazamento" de "estado legítimo".** A primeira versão do item 020 media tudo nas duas fases e teria acusado erro em cima do comportamento **correto** de spawn ferido, porque o sweep estabelecedor repovoa records/caps entre as duas medições. Separar estado transitório de estado derivado de ferimento foi descoberto implementando, não planejando.
- **Auditar no fim da raid é falso positivo garantido** — a limpeza é cascata assíncrona por consumidor, sem ordem garantida no frame. O resíduo real aparece na entrada da raid seguinte. Desvio consciente do plano.

**Pendências abertas nesta sessão:** ver P-5.1, P-5.2 e P-5.3 no topo.

---

## 2026-07-25 (GMT-3) — Sessão 5 (continuação): plano de teste mestre criado + preparação do card Trello

**Tema central:** Consolidar um plano de teste único e sequenciado para todo o overhaul Trauma 2.0 (002-010, v1.10.0), separando explicitamente SOLO/COOP, e preparar a criação de um card filho no Trello (tag "teste") para rastrear a execução.

**Decisões-chave:**
- Documento novo: `docs/master-test-plan.md` — substitui a necessidade de navegar entre `trauma-behavior-matrix.md` §5 (44 cenários) e `trauma-coop-test-protocol.md` (B1/B2) como 2 listas separadas; renumera tudo numa sequência única por fase (`S<fase>.<n>` solo, `C<fase>.<n>` coop).
- **Padrão de processo estabelecido** (repetir para próximas versões/módulos): 1ª rodada do plano de teste sempre passa por uma revisão adversarial (agente cético relendo TODAS as specs funcionais + a matriz de comportamento, fontes primárias, não confiando no resumo do próprio plano) antes de considerar pronto. Achados desta rodada: 2 cenários da matriz original tinham sido dropados SILENCIOSAMENTE na consolidação (fila de adiados multi-região do 006; roll de estômago durante desmaio) + 1 cenário de risco real JÁ DOCUMENTADO EM MEMÓRIA (migração fracionária pt-BR do item 008, achado da review técnica de 2026-07-19) também tinha sumido — lição: consolidar documentos de teste sem comparar item-a-item com a fonte original perde cobertura silenciosamente, mesmo quando a intenção é só reorganizar.
- Gaps estruturais fechados pela revisão: nenhum cenário testava "spawn ferido" (persistência de dano entre raids) em 5 sistemas simultaneamente (nova fase S1b); Braços (005) era o único dos 4 consumidores de estado contínuo sem teste de toggle-OFF dedicado (S3.7, relevante porque o item 009/A4 pediu explicitamente essa regressão); faltavam instruções de "curar antes de avançar de fase" (sem isso, testar o ciclo de queda antes do desmaio percentual contamina o setup, já que o personagem fica mancando/prone periodicamente); 4 cenários coop exigem 2 peers e nunca tinham sido escritos em nenhum documento (reconexão Fika, cura remota revertendo efeito visível a ambos, DOWNED durante o ciclo de queda, médico desconectando mid-cura).
- **Trello:** usuário forneceu o card principal do mod — `https://trello.com/c/8dR5yd1s/27-trl-combatmedicine` (salvo em memória `reference_icm_trello_card.md`). Combinado: a cada versão substancial entregue, criar um card FILHO com a tag "teste" (já existente no board), linkado ao card principal, com o plano de teste daquela versão como checklist — vira prática recorrente para qualquer módulo/mod com card principal equivalente.
- **Achado de infraestrutura:** existe um MCP Trello configurado globalmente em `~/.claude.json` (`@delorenj/mcp-server-trello`, API key+token já preenchidos) mas ele não estava ativo nesta sessão (ToolSearch não encontrou nenhuma ferramenta "trello") — precisa reiniciar a sessão do Claude Code para conectar. Card não foi criado ainda nesta sessão por esse motivo.

**Lições / hipóteses descartadas:**
- Confirmada a lição de custo já registrada: delegar a revisão adversarial a um agente que releia TUDO da fonte primária (não confiar no resumo do documento sendo revisado) pega gaps que uma releitura rápida do próprio autor não pegaria — a subagente achou 3 cenários dropados silenciosamente que eu não tinha notado ao escrever a 1ª versão.

**Pendências abertas nesta sessão:**
- Card do Trello ainda não criado — aguardando restart da sessão para o MCP Trello conectar. Conteúdo do card (título/descrição/checklist) deve ser gerado a partir de `docs/master-test-plan.md` quando a ferramenta estiver disponível.
- `docs/master-test-plan.md` em si ainda não foi executado (mesma natureza de P-4.4 — é roteiro pronto, não validação).

**Cross-refs:**
- Consome [P-4.4] (validação in-game do overhaul) — este documento é o roteiro completo que a pendência pede para "consumir".

---

## 2026-07-25 (GMT-3) — Sessão 5 (continuação): item 010 entregue — OVERHAUL TRAUMA 2.0 CONCLUÍDO (002→010)

**Tema central:** Fechar o item 010 (migração de configs + release), o último do backlog formal do overhaul Trauma 2.0 — do zero (spec funcional) até entregue, incluindo uma mudança de wire format de rede Fika (o handshake de recusa de cura `DenyReason` string → `DenyReasonId` enum/byte).

**Decisões-chave:**
- Distância de interação final (`Medic Interact Distance`) resolvida em **3,5 m** (era 5, "valor alto para testes") — sem decisão prévia registrada; escolhida com evidência (vanilla nativo ~2,5 m + margem para a cura em coop, que exige ficar parado por vários segundos, ao contrário do loot instantâneo). Ajuste de 1 número, fácil de recalibrar se o usuário quiser outro valor após validar in-game.
- As 3 keys legadas inertes ("Sistema de Pernas/Braços/Estomago") foram removidas por **remoção simples** (padrão Shoulder Tap), não por migração — nenhuma delas era lida funcionalmente. Isso corrigiu uma expectativa registrada no defer do item 008 (CR-01-01: "o item 010 vai adicionar uma 7ª/8ª/9ª cópia do bloco de resgate de órfã") — não era verdade, porque não há valor a migrar.
- **Achado crítico da spec técnica** (pego ANTES do `/code-mod`, não depois): remover `ConfigArmsEnabled` sem remover também o bloco de migração histórica do mojibake "Sistema de Braços" em `MigrateOrphanedConfigKeys()` (que escrevia `ConfigArmsEnabled.Value`) quebraria a compilação. O bloco inteiro foi removido junto (já tinha cumprido seu papel one-time desde 2026-07-12).
- Textos legados (Band-Aid/torniquete/ActionPanel/HUD médico, ~25 pontos) migrados para i18n EN/PT via `MedicLocale.cs` (classe nova, espelha `TraumaLocale.cs` do item 002, reusa `IsGamePortuguese()` sem duplicar a leitura de idioma) — fecha a decisão 22 por completo.
- **Mudança de wire format Fika:** `BandAidHealCheckResponsePacket.DenyReason` (string) → `DenyReasonId` (enum `MedicDenyReasonId`, byte). A tradução do motivo de recusa passou a acontecer no MÉDICO (ponto de exibição), não no paciente (ponto de geração) — cada peer vê a recusa no PRÓPRIO idioma, reusando o `ItemTemplateId` já existente no pacote (sem campo novo). Mesma classe de mudança já feita antes pelo mod (CR-02/CR-05) — todos os peers de uma sessão coop precisam da MESMA build pós-item-010.
- **Rigor do gate:** 2 rodadas de review técnica ANTES do `/code-mod` (13 achados totais, incluindo 3 bloqueadores de compilação reais no wire format — `using` faltantes + um 3º ponto de leitura do campo não mapeado) + só 1 rodada de code-review DEPOIS (1 achado opcional, aplicado). Decisão deliberada de não rodar uma 2ª rodada de code-review: o risco real do item (wire format) já tinha sido extensivamente mitigado na fase de spec técnica, e a única rodada de code-review (com verificação via `git diff` linha a linha em todos os 12 arquivos + build isolado) não achou nenhum problema real — só uma nota informativa. Retorno marginal de uma 2ª rodada seria baixo.

**Lições / hipóteses descartadas:**
- Confirmada a lição de custo já registrada: investir rigor na fase de SPEC TÉCNICA (antes do código existir) é mais barato que investir o mesmo rigor depois — os 3 bloqueadores de compilação do wire format foram pegos e corrigidos ANTES de qualquer linha de código real ser escrita, então o `/code-mod` implementou uma spec já "pré-testada" e o `/code-review` subsequente só precisou CONFIRMAR, não caçar.
- Um achado de review que a própria sugestão do revisor marca como "não recomendo ação agora" (ex.: CR-01-01, `BandAidUI.ShowTreatment` fora do escopo original de i18n) ainda deve ser aplicado por padrão (diretiva do usuário) — a leitura correta de "sugestão de baixa prioridade" não é "pular", é "aplicar, já que é barato e fecha completamente o objetivo do item".

**Atividade cronológica:**
1. Escopo real mapeado por pesquisa dedicada (Agent Explore) ANTES de escrever a spec — 6 sub-itens com evidência de código real (configs, sondas debug, PROPRIEDADES.md, distância, i18n, release).
2. Spec funcional escrita + revisada (decisão de distância resolvida, inventário completo de ~25 textos i18n anexado).
3. Spec técnica escrita (achou sozinha 2 problemas: o bloco do mojibake e o congelamento de idioma no `BuildUI()`) + 2 rodadas de review técnica (13 achados, incl. 3 bloqueadores de compilação no wire format — todos aplicados).
4. `/code-mod` — implementação completa dos 4 blocos, 1 desvio da spec (CS0052 em `DenyReasonId`, campo precisou ser `internal`) documentado e corrigido. Build v1.10.0, 0 erros.
5. Code-review rodada 1 — 1 achado 🟢 opcional (texto residual de `BandAidUI.ShowTreatment`), aplicado. Item fechado sem 2ª rodada (rigor já mitigado na fase de spec técnica).

**Pendências abertas nesta sessão:**
- [P-4.4] (herdada) segue aberta e agora cobre também o item 010 — nada do overhaul completo (002-010) foi validado IN-GAME. Testes específicos do 010: distância 3,5 m percebida como razoável; textos em inglês aparecem corretamente com o jogo em EN; torniquete/cura aplicado por um peer aparece traduzido no idioma de CADA peer (não do originador) — o teste mais importante, já que é o único ponto que muda wire format.
- **TODOS os peers Fika de uma sessão coop precisam rodar a build pós-item-010 simultaneamente** — mudança de wire format incompatível com builds anteriores (mesmo aviso já dado em CR-02/CR-05 anteriores do mod).

**Cross-refs:**
- Resolve [P-4.5] (aberta na Sessão 4, atualizada no início desta Sessão 5) — item 010 concluído, fecha o ciclo do backlog formal 002→010 (011 já entregue antes, na Sessão 4).
- Fecha o backlog formal completo do overhaul Trauma 2.0 (`mods/TRL-ImmersiveCombatMedicine/backlog/mod-backlog.md`, itens 001-011, todos 🟢).

---

## 2026-07-25 (GMT-3) — Sessão 5: item 009 (hardening coop) entregue — P-4.1 fechada; iniciando item 010

**Tema central:** Fechar o item 009 (hardening coop/bots do Trauma 2.0 — helper compartilhado `TraumaConsumerLifecycle` + decisão A3 de voz dupla-fonte), que já vinha com Bloco A implementado e 1ª rodada de code-review sem bloqueadores de uma sessão anterior; rodar a 2ª rodada de code-review (plano 2× dado o risco da refatoração A4 tocar 4 consumidores já entregues) e então avançar para o item 010.

**Decisões-chave:**
- Decisão A3 (item 009, documentação pura — sem mudança de código): colisão de voz dupla-fonte entre `TraumaVoice.PlayStrong` (item 004, queda/negação de levantar) e `TraumaVoice.TryPlayStrong` (item 005, lockout de re-ADS) — ambos competem pelo mesmo `Player.Speaker.Play(OnAgony, importance:100)`. ACEITA SEM ARBITRAGEM: o motor vanilla `PhraseSpeakerClass.Play` já arbitra "primeiro chega, leva"; a precondição de colisão (queda E lockout de ADS no mesmo frame) é estreita; o item 005 já tolera a perda com retry 0,3s + log. Registrada como comentário XML acima de `PlayStrong` em `TraumaVoice.cs` — zero mudança de assinatura/lógica, confirmado por 2 rodadas de code-review.
- CR-01-01 (rodada 1, 🟢 opcional): campo `_onToggleOn` do `TraumaStomachConsumer` (sempre `null`, warning `CS0649`) removido — `null` literal passado direto no call site de `Tick()`. Aplicado e recompilado (0 erros, warning eliminado).
- CR-02-01 (rodada 2, 🟢 opcional, categoria D): cada consumidor criava 2 delegates independentes para o mesmo `IsActive` (um dentro de `TraumaConsumerRegistry.Register`, outro em `_isActiveDelegate` para o helper A4). Aplicado com **modificação** (não a sugestão literal do achado, que propunha expor um getter `TryGetIsActive` no registry — rejeitada por trocar uma alocação desprezível por acoplamento novo motor↔helper): o delegate agora é criado 1× no início do `Awake()` e a MESMA referência de campo é passada tanto para `Register(...)` quanto usada pelo `Tick()` — zero acoplamento novo, mesma economia de alocação. Aplicado nos 4 consumidores, recompilado (0 erros).
- Diretiva de processo confirmada nesta sessão: mesmo achados 🟢 puramente cosméticos, cuja própria review recomenda "não mudar nada", são aplicados por padrão (ver [[feedback-apply-all-review-findings]]) — a exceção é achado factualmente ERRADO, não achado de baixo valor. Quando a sugestão literal do achado tem um trade-off pior que uma alternativa óbvia, aplicar a alternativa e documentar como "aceito com modificação", não pular o achado.

**Lições / hipóteses descartadas:**
- Nenhuma lição nova de arquitetura — a rodada 2 confirmou independentemente (via `git diff HEAD` real, já que o working tree seguia não commitado) que não há regressão em nenhum dos 5 arquivos tocados por A4/A3. `graphify affected` teve granularidade insuficiente para provar ausência de callers (grafo "aponta", não "prova" — leitura direta do código foi a evidência real usada).

**Atividade cronológica:**
1. Retomada: CR-01-01 (rodada 1) aplicado no código (3 edições em `TraumaStomachConsumer.cs`), recompilado, marcado `✅ Aplicado` no artefato `009-coop-hardening-04-code-review-01.md`.
2. Agent (code-review rodada 2) lançado em background — 0🔴/🟠/🟡, 1🟢 (CR-02-01). Item 009 confirmado pronto para fechar em ambas as rodadas.
3. Em paralelo, Agent (Explore) mapeou o escopo real do item 010 (6 sub-itens: configs legados, sondas `[DEBUG-ICM]`, `PROPRIEDADES.md`, distância de interação final, i18n EN/PT dos textos legados, zip de release) — relatório completo com paths/linhas, sem escrever spec ainda.
4. CR-02-01 aplicado com modificação nos 4 consumidores (`TraumaLegsConsumer`/`TraumaFallCycleConsumer`/`TraumaArmsConsumer`/`TraumaStomachConsumer`), recompilado, marcado `✅ Aplicado` no artefato `009-coop-hardening-04-code-review-02.md`.
5. Asbuild (`009-coop-hardening-05-asbuild.md`) atualizado com a seção "Mudanças posteriores" (CR-01-01 + CR-02-01) e a nota sobre o Bloco B corrigida (o doc `docs/trauma-coop-test-protocol.md` já existe — a nota antiga dizia que não existia ainda, desatualizada).
6. P-4.1 fechada nesta memória (resolvida pelo item 009/A4); P-4.5 atualizada (009 entregue, 010 em andamento).

**Pendências abertas nesta sessão:**
- Item 010 (migração de configs + release) iniciado logo em seguida — ver próxima entrada quando fechado.
- P-4.4 (validação in-game do overhaul completo) segue aberta — nenhum item 002-011 foi validado em raid real ainda; item 009 também soma a essa pendência (bloco B do protocolo de teste coop é roteiro pronto, execução real pendente).

**Cross-refs:**
- Fecha [P-4.1] (aberta na Sessão 4, 2026-07-19).
- Atualiza [P-4.5] (aberta na Sessão 4, 2026-07-19).

---

## 2026-07-19 23:50 (GMT-3) — Sessão 4 (continuação): itens 008 e 011 entregues — overhaul 003-011 CONCLUÍDO

**Tema central:** Fechar o overhaul Trauma 2.0 com o item 008 (duração aleatória do desmaio) e o item 011 (matriz de comportamento total), completando o ciclo 003→011 que o usuário pediu para levar "até o final".

**Decisões-chave:**
- Item 008: migração do valor legado (`ConfigBlackoutDuration`) feita por **CÓPIA**, não pelo padrão "rename-at-delivery com descarte" usado em 003-007 — porque o campo era um valor REAL ajustado pelo usuário (histórico de tuning documentado em P-2.13/P-2.15), não um placeholder nunca escolhido. Ref: [008-desmaio-duracao-aleatoria-02-spec-tech.md §1](../backlog/008-desmaio-duracao-aleatoria/008-desmaio-duracao-aleatoria-02-spec-tech.md).
- Item 008, achado real da review técnica: parse do valor legado sem `CultureInfo.InvariantCulture` corromperia a migração em máquinas com cultura pt-BR/de-DE (ponto decimal lido como separador de milhar, inflando o valor 10× e sendo clampado ao teto da faixa) — primeiro parse de `float` numa migração de config neste mod; risco de classe nova documentado para o item 010. Ref: [008-desmaio-duracao-aleatoria-03-spec-tech-review-01.md PA-01-01](../backlog/008-desmaio-duracao-aleatoria/008-desmaio-duracao-aleatoria-03-spec-tech-review-01.md).
- Item 011: em vez de eu mesmo ler todas as specs/reviews/memória dos 7 itens sequencialmente (caro), paralelizei a EXTRAÇÃO — 1 agente Explore por item (002-008), cada um retornando um relatório estruturado (comportamento final, config F12, decisões novas, interims, contradições vs. matriz original) sem escrever arquivo — e fiz a SÍNTESE final eu mesmo em [docs/trauma-behavior-matrix.md](../docs/trauma-behavior-matrix.md). Depois rodei um agente de VERIFICAÇÃO DE COMPLETUDE independente (releitura cética das fontes primárias, não confiando na síntese) antes de considerar o documento pronto.
- A verificação de completude achou 9 premissas de prioridade alta ausentes na síntese inicial — quase todas do item 004 (ciclo de queda), que tinha o maior volume de "premissas p/ item 011" marcadas explicitamente nas 2 rodadas de review técnica dele. Todas incorporadas antes de fechar o item.

**Lições / hipóteses descartadas:**
- Para documento de síntese que varre muitos artefatos, o padrão "extrair em paralelo por unidade (item/sessão/módulo) + sintetizar + verificar completude com um agente cético independente" funcionou bem e deve ser repetido — a verificação pegou gaps reais que a síntese sozinha teria deixado passar silenciosamente.
- Confirmada a lição já registrada na P-3.7: itens pequenos com reuso extensivo (008) toleram 1 rodada de review em cada fase sem perder achados reais — a rodada única do 008 pegou o achado de cultura de parse, que é genuinamente sério.

**Atividade cronológica:**
1. Spec funcional do 008 escrita + revisada (rápido, item pequeno e bem contido).
2. Agent (spec técnica 008) + Agent (review técnica 008, 1 rodada) — achado PA-01-01 (cultura de parse) aplicado.
3. Agent (`/code-mod` 008) — build v1.9.0, 0 erros. Agent (code-review 008, 1 rodada) — 1🟢 deferido pro item 010.
4. Commit do item 008.
5. Spec funcional do 011 escrita (define escopo/estrutura do documento de síntese, não é spec técnica — item é 100% documentação).
6. 7 Agents Explore em paralelo (itens 002-008), cada um extraindo comportamento/config/premissas/interims/contradições em relatório estruturado.
7. Síntese do relatório único (`docs/trauma-behavior-matrix.md`) a partir dos 7 relatórios.
8. Agent de verificação de completude independente — achou 9 gaps de prioridade alta (maioria do item 004) + 1 divergência de config (tag `avançado` faltante) + 1 imprecisão na spec funcional (15 linhas da matriz, não 16).
9. Todos os gaps corrigidos diretamente no documento; item 011 fechado 🟢.

**Pendências abertas nesta sessão:**
- [P-4.4] (aberta 2026-07-19, ATUALIZADA 2026-07-25) Nenhum item do overhaul completo (002-010, backlog fechado) foi validado IN-GAME. Ver as notas "VALIDAR IN-GAME" específicas de cada item nas pendências P-3.2 a P-3.7. Candidato natural para consumir o plano de teste de `docs/trauma-behavior-matrix.md` §5 + `docs/trauma-coop-test-protocol.md` (item 009). Item 010 soma 3 testes específicos: distância 3,5 m, textos EN corretos com jogo em inglês, e — o mais importante, único ponto com mudança de wire format — torniquete/cura aplicado por um peer aparece traduzido no idioma de CADA peer que observa, não no idioma de quem originou o evento.
- [P-4.5] (aberta 2026-07-19, FECHADA 2026-07-25 — ver Sessão 5) Item 009 (hardening coop) ENTREGUE 🟢 v1.9.1; item 010 (migração de configs + release) ENTREGUE 🟢 v1.10.0 — overhaul Trauma 2.0 CONCLUÍDO (backlog formal 001-011, todos 🟢).

**Cross-refs:**
- Resolve [P-3.7] (aberta nesta mesma sessão, ver bloco de Pendências no topo — FECHADA).

---

## 2026-07-19 22:00 (GMT-3) — Sessão 4 (continuação): item 007 entregue — maior risco do overhaul, gate 2×2 completo

**Tema central:** Levar o item 007 (Desmaio 2.0: gatilhos percentuais) do zero (spec funcional) até entregue, com rigor 2×2 (2 rodadas de review técnica + 2 de code-review) dado ser o item de maior risco declarado do overhaul restante.

**Decisões-chave:**
- Decisão técnica central: capturar o HP pré-hit da parte (tórax/cabeça) no **Prefix** de `Player.ApplyDamageInfo` via parâmetro especial `__state` do Harmony, NUNCA por aritmética reversa (`postHp + damageInfo.Damage`) — `ActiveHealthController.ApplyDamage` já muta o HP antes do Postfix rodar, e reconstruir por aritmética distorce em overkill. Ref: [007-desmaio-percentual-02-spec-tech.md §1](../backlog/007-desmaio-percentual/007-desmaio-percentual-02-spec-tech.md).
- Decisão de design tomada autonomamente (diretiva do usuário de seguir até o fim): `ConfigBlackoutEnabled` ("Sistema de Desmaio") CONTINUA sendo o master de todo o pipeline (timers/wake/grace/sync) — diferente dos itens 003-006, cujo legado vira 100% inerte. O novo toggle "Blackout 2.0" é um SUB-toggle que só decide a lógica de ENTRADA (percentual vs. nenhuma). Razão: preservar o pipeline com histórico de bugs de timing (P-2.13/14/15) sem redesenhar seu controle mestre.
- Rodada 2 do code-review (CR-01-01, 🟠) achou um erro factual real: a spec técnica citava `Priority.High=200` e "número menor = prioridade maior" — ambos ERRADOS (`Priority.High` real = 600; a regra do HarmonyX é o OPOSTO: maior valor executa primeiro entre Prefixes). A conclusão prática ("nosso Prefix sempre roda antes de BringBackConcussion") continuava correta por coincidência de dois erros que se cancelavam — mas a "prova" que a review técnica 2 tinha "endurecido" especificamente para blindar essa decisão estava ela mesma errada. Corrigido via decompile real do `0Harmony.dll` (compile-time E runtime). Lição: quando uma prova é adicionada especificamente para "endurecer" uma decisão crítica, vale re-verificar a prova em si na rodada seguinte, não só a conclusão.

**Lições / hipóteses descartadas:**
- A citação `GClass921.cs:1143` (achado PA-01-01 da review técnica 1) parecia uma evidência válida (assinatura bate) mas era da classe ERRADA (`ObservedPlayerHealthController`, controller do espelho, método stub que lança exceção) — lição: bater a assinatura não prova que a classe citada é a certa; conferir também QUAL classe implementa o método de fato.
- Nenhuma lição descartada sobre o pipeline de desmaio em si — as pendências históricas (P-2.13/14/15) não foram tocadas nem reabertas (verificado por `git diff` em 2 rodadas de code-review).

**Atividade cronológica:**
1. Spec funcional criada + revisada (1 decisão de design marcada `<!-- review: -->` e resolvida autonomamente).
2. Agent (spec técnica) — decisão central pesquisada e documentada com evidência do Assembly real.
3. Agent (review técnica r1) — 3 achados (1🟡+2🟢, todos de citação/documentação) — aplicados.
4. Agent (review técnica r2) — 5 achados (2🟡+3🟢) incluindo a citação errada de `GClass921` corrigida DE NOVO no contexto do Harmony (achado diferente da r1) e um filtro de domínio (`bodyPartType`) tornado explícito — aplicados.
5. Agent (`/code-mod`) — implementação completa, v1.8.0, 0 erros.
6. Agent (code-review r1) — 1🟠 (valores/regra de `Priority` invertidos) + 1🟢 — ambos aplicados; recompilado.
7. Agent (code-review r2) — 0🔴/🟠/🟡, 1🟢 (asbuild desatualizado) — aplicado. Item fechado 🟢.

**Pendências abertas nesta sessão:** nenhuma nova além das já registradas em P-4.1/P-4.2/P-4.3 (ver bloco anterior desta mesma sessão).

---

## 2026-07-19 19:50 (GMT-3) — Sessão 4: Retomada da P-3.7 — 005 fechado + 006 entregue (spec fix + code-mod + 2 code-reviews)

**Tema central:** Retomar o overhaul Trauma 2.0 de onde a sessão anterior parou por custo (P-3.7): fechar o item 005 (código já implementado, sem code-review) e levar o item 006 do zero até entregue.

**Decisões-chave:**
- Code-review r1 do 005 **refeito do zero** em vez de retomado — o transcript do revisor da sessão anterior não existe mais nesta sessão nova (agentes não persistem entre invocações separadas do Claude Code); a "retomada barata" que a P-3.7 original previa só valeria dentro da MESMA sessão viva. Resultado: 0🔴, 1🟡 (CR-01-01 — predicado de incapacidade duplicado pela 3ª vez em vez de reusar `TraumaFallCycleConsumer.IsPauseCondition`, já `internal` agora) + 2🟢 (log de voz suprimida por incapacidade; grafo do mod desatualizado desde o commit do 004) — todos aplicados, v1.6.1. Ref: [005-bracos-tremor-ads-04-code-review-01.md](../backlog/005-bracos-tremor-ads/005-bracos-tremor-ads-04-code-review-01.md).
- Review técnica do 006 rodada com **1 revisor só** (não 2) — item pequeno (ZERO patch Harmony novo, ZERO mudança no motor, reuso extensivo de `TraumaPose` já validado 2x pelos 003/004). Achado real e não-trivial: PA-01-01 (🟡) — a reserva do cooldown compartilhado (pernas↔estômago) era um "pré-check depois chama", não atômica como o motor faz (`TryPublishOneShot` stampa NA DECISÃO de publicar); corrigido na spec técnica ANTES do `/code-mod` (`ReportOneShotExecuted` movido para logo após o pré-check, antes de chamar a primitiva) — o próprio revisor recomendou aplicar e seguir sem 2ª rodada, dado o custo/risco do fix (1 linha, corner raro mas real). Ref: [006-estomago-agachar-03-spec-tech-review-01.md](../backlog/006-estomago-agachar/006-estomago-agachar-03-spec-tech-review-01.md).
- `/code-mod` do 006 delegado a um Agent com o mesmo nível de detalhe de um `/code-mod` normal (spec técnica já corrigida, checklist §8, referências a todos os arquivos tocados por 003/004/005 para evitar regressão em `TraumaPose.cs`, código COMPARTILHADO). Zero regressão encontrada no code-review seguinte (grep exaustivo de todos os call sites de `CancelKind`/`KindWord`/`AbsorbIfCycleEngaged`/`BotCrouchDip` fora do 006). Ref: [006-estomago-agachar-05-asbuild.md](../backlog/006-estomago-agachar/006-estomago-agachar-05-asbuild.md).
- Code-review do 006 também 1 rodada só — 0🔴, 2🟢 (CR-01-01 branch defensivo `!IsYourPlayer` vazava o cooldown sem refund, dead code hoje mas invariante quebrado — aplicado; CR-01-02 boilerplate de `Update()` world-swap/toggle duplicado pela 4ª vez entre 003/004/005/006, sem helper compartilhado — **deferido para o item 009 ou 011**, refactor tocaria os 4 consumidores sem necessidade imediata). Ref: [006-estomago-agachar-04-code-review-01.md](../backlog/006-estomago-agachar/006-estomago-agachar-04-code-review-01.md).
- Padrão operacional adotado nesta sessão (a manter): cada fase do gate (code-review, review técnica, code-mod) delegada a um `Agent` em background com prompt auto-contido (paths exatos, o que ler, o que NÃO tocar) — o orquestrador só sintetiza o resumo retornado, nunca lê o código inteiro nem os artefatos completos no próprio contexto. Manteve o custo de tokens do orquestrador baixo mesmo cobrindo 2 itens completos numa sessão.

**Lições / hipóteses descartadas:**
- Descartada a suposição de que "retomar code-review morto" seria mais barato que refazer — só é mais barato se o AGENTE ainda estiver vivo (mesma sessão). Entre sessões, sempre refazer do zero.
- Confirmada na prática a lição de custo da P-3.7 original: 1 rodada de review basta para specs/código pequenos com reuso extensivo já validado — não houve nenhum achado 🔴/🟠 perdido por pular a 2ª rodada em nenhum dos dois casos (spec técnica e code-review do 006).

**Atividade cronológica:**
1. Leitura da P-3.7 (memória) + confirmação do estado dos artefatos em disco (005 sem `04-code-review`, 006 sem `03-spec-tech-review`) — bateu com o relato da memória.
2. Agent (code-review r1 do 005) + Agent (review técnica r1 do 006) lançados em paralelo (background).
3. Fixes do CR-01-01/02/03 do 005 aplicados diretamente (mecânicos: `internal`, log, `update-graphs.sh`) — build v1.6.1, 0 erros.
4. Fix do PA-01-01/02 do 006 aplicado na spec técnica (reserva atômica do cooldown + citação de RNG corrigida).
5. Agent (`/code-mod` do 006) lançado — implementação completa (`TraumaStomachConsumer.cs` novo + 3 arquivos modificados + config), build v1.7.0, 0 erros (csproj precisou de sync manual pós-agente — divergia do `BepInPlugin`).
6. Grafo do mod regenerado 2× (pós-005, pós-006) — 827 nós/1370 arestas final.
7. Agent (code-review r1 do 006) lançado — 0🔴, 2🟢; CR-01-01 aplicado, CR-01-02 deferido. Build final v1.7.0 recompilado.

**Pendências abertas nesta sessão:**
- [P-4.1] (aberta 2026-07-19, FECHADA 2026-07-25 — ver Sessão 5) CR-01-02 do 006 deferido: extrair helper compartilhado para o boilerplate de `Update()` (world-swap/toggle) duplicado 4× entre `TraumaLegsConsumer`/`TraumaFallCycleConsumer`/`TraumaArmsConsumer`/`TraumaStomachConsumer`. Categoria: débito técnico 🟡. RESOLVIDA pelo item 009 (A4) — `TraumaConsumerLifecycle` extraído, 4 consumidores migrados, 2 rodadas de code-review sem regressão.
- [P-4.2] (aberta 2026-07-19) Nenhum dos itens 005/006 foi validado IN-GAME ainda (mesma situação herdada de P-3.5/P-3.6 para 003/004) — 006 reusa a MESMA fila/primitiva/absorção de 003/004, então um bug estrutural na validação deles é retrabalho herdado aqui também.
- [P-4.3] (aberta 2026-07-19) Nenhuma mudança desta sessão foi commitada ainda — working tree com 005+006 juntos (arquivos compartilhados como `csproj`/`Plugin.cs`/`graph.json` foram tocados por AMBOS em sequência, não dá pra separar em 2 commits sem reverter). Fazer commit único abrangendo só os arquivos do ICM tocados nesta sessão (excluir mudanças de outra sessão presentes no mesmo working tree: `TRL-ItemsManagement`, `launcher/`, `items.json`).

**Cross-refs:**
- Resolve parcialmente [P-3.7] (Sessão 3, sem entrada cronológica própria no arquivo — só o bloco de pendências do topo foi mantido pela sessão anterior antes da interrupção por custo).

---

## 2026-07-11 23:55 (GMT-3) — Sessão 2: Diagnóstico do prompt F ausente — build velha implantada (não era código)

**Tema central:** Descobrir por que o prompt F nunca aparecia apesar das rodadas de fix no ScanForPatient.

**Decisões-chave:**
- Diagnóstico por eliminação com validação estática massiva (workflow: vanilla decompilado, diff 3.11, Fika, lifecycle/JIT) já que o BepInEx sobrescreve LogOutput.log a cada boot (sem log de raid das rodadas anteriores).
- Forense binária no DLL implantado (strings ASCII/UTF-16) provou que era build pré-fix: continha `Camera.main`+`SphereCast` simples+máscara `HighPolyCollider`, sem `WeaponRoot`/`SphereCastAll`.
- Reestruturação `client/`→`modded/` em vez de build manual: entra no tooling canônico do repo (compile, graphs) e evita recorrência.

**Lições / hipóteses descartadas:**
- LIÇÃO PRINCIPAL: antes de debugar comportamento de mod client, confirmar que o DLL implantado corresponde à fonte (data + forense de strings). As rodadas da Sessão 1 debugaram um fantasma.
- Descartada: "Update morre por exceção/JIT por frame em raid" — todos os membros externos do caminho quente existem no decompilado (verificado adversarialmente).
- Descartada: "APIs do scan inválidas no 4.0" — LookDirection/WeaponRoot/GetPlayerByCollider/layers todos válidos; o scan atual detectaria bots.
- Descartada: "mismatch Fika compile×runtime" — DLL referencia Fika.Core 2.3.4.0, idêntico ao instalado.
- Unity do EFT 0.16.x é 2022.3.43f1; `Resources.GetBuiltinResource<Font>("Arial.ttf")` NÃO lança no player runtime desta build (verificado no Player.log da sessão de boot).

**Atividade cronológica:**
1. Leitura da memória da Sessão 1 + código do BandAidController (cadeia Update→ScanForPatient→_potentialTarget→LateUpdate).
2. LogOutput.log do boot: plugin carrega limpo; nenhuma linha de raid; sem spam de exceção em menu.
3. Grafo do mod gerado via graphify (377 nós) — cadeia F→prompt mapeada.
4. Workflow de validação (12 agentes, 4 frentes + verificação adversarial): causa-raiz no lifecycle-jit finder (build velha), confirmada por verificador independente.
5. `git mv client modded`, `ItemComponent.Types.dll` no mapa do compile-mod.sh, build OK (0 erros), install em D:\SPT, forense binária confirma código novo, grafo regenerado pelo script canônico.

## 2026-07-11 21:00 (GMT-3) — Sessão 1: Correções de UI Médica, Consumo e Compatibilidade com FIKA/SPT 4.0

**Tema central:** Refinamento do BandAid HUD para cadáveres, conserto de detecção de pacientes (SPT 4.0 / FIKA) e análise da lógica de consumo.

**Decisões-chave:**
- HUD com Cadáveres ativado: o raycast aceita a layer Deadbody e a função de atualização do ECG congela num bip contínuo se HP <= 0. Ref: BandAidUI.cs e BandAidController.cs.
- Bot Ragdoll removido: reverting da injeção de ragdoll físico para o bot, pois quebrava a animação, voltando para SetPoseLevel(0f) (prone). Ref: MovementPatches.cs.
- Detecção Raycast fixa para SPT 4.0 e FIKA: mudança de Camera.main para WeaponRoot e uso de AllLayers + GetComponentInParent para encontrar o jogador. Ref: BandAidController.cs.

**Lições / hipóteses descartadas:**
- A hipótese do slot fantasma por causa de consumo: o código já possuía a lição correta ConsumeSafe, onde itens perto do 0 são descartados pela rede antes de modificar o HP localmente para o server aprovar.
- A hipótese do GetPlayerByCollider falhar no FIKA: GameWorld.GetPlayerByCollider falha com partes (ossos) customizadas da layer Fika. Sempre fazer hit.collider.GetComponentInParent<Player>().
- Em SPT 4.0, Camera.main retorna nulo sob certas circunstâncias do Fika, portanto atirar rays do WeaponRoot ou LookDirection do player local é preferível.

**Atividade cronológica:**
1. Alteração do BandAidUI e Controller para aceitar e renderizar Flatline em jogadores com HP 0.
2. Análise do MedicalLogic.cs provando que o bug de Slot Fantasma não ocorrerá devido ao design do TryRunNetworkTransaction.
3. Remoção do BotRagdollSimulator.cs e reversão para Prone.
4. Identificação de bug crítico na mira do scanner (ScanForPatient não detectava).
5. Code review levantando bloqueador sobre Layers e Fallback do collider.
6. Refactor do scanner para contornar nulos no Camera.main e no GetPlayerByCollider.

---

## 2026-07-29 02:40 (GMT-3) — Sessão 6: Arquitetura do Canal 3 Compartilhado TRL & Validação da Autoridade de Inventário

**Tema central:** Definição da diretriz de isolamento de rede do mod no `ROADMAP.md` e análise da coexistência entre a autoridade de inventário do Tarkov/FIKA e as mensagens customizadas do mod.

**Decisões-chave:**
- **Diretriz de Rede (ROADMAP.md):** Especificada no [ROADMAP.md](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/ROADMAP.md) a migração dos pacotes de dados médicos (sinais de socorro, desmaio, cura remota e animação do socorrista) para o **Channel 3 Compartilhado TRL** com a assinatura binária `TRLM` (`0x54 0x52 0x4C 0x4D`).
- **Isolamento e Autoridade do Inventário:** Validado que mover as mensagens médicas para o Canal 3 **não afeta nem desincroniza o inventário do FIKA**. As mensagens do mod trafegam no Canal 3 notificando as intenções entre médicos e pacientes, enquanto o desconto real da durabilidade dos itens do inventário (`ApplyTreatment` / `ExecuteOperation`) permanece 100% sob a gestão autoritativa do FIKA no `Channel 0`.

**Lições / hipóteses descartadas:**
- A dúvida sobre a necessidade de manter as mensagens do mod no `Channel 0` do FIKA para garantir a baixa de durabilidade dos itens foi descartada: a baixa de durabilidade é nativa do Tarkov via `Channel 0`, dispensando o mod de usar o leitor do FIKA para tráfego secundário.

**Atividade cronológica:**
1. Análise técnica da interação entre a baixa de itens e as mensagens de cura em rede coop.
2. Criação do `ROADMAP.md` formalizando o Canal 3 Compartilhado TRL com Magic Header `TRLM`.

---

## 2026-08-12 — Sessão 7: Trava de Pose de 1 Segundo para Agachamento Involuntário por 1 Perna (v1.13.2)

**Tema central:** Implementação do cooldown de trava de postura por 1,0 segundo (1f) quando o jogador ou a IA sofrem agachamento involuntário por 1 perna fraturada/zerada.

**Alterações Realizadas:**
1. **`TraumaPose.cs`**:
   - Adicionada a gestão de tempo `SetCrouchLock` e `IsCrouchLocked` baseada em `ProfileId` e `Time.time`.
   - Injetada a trava de 1.0s ao executar `TryInvoluntaryCrouch` (humano) e em `BotCrouchDip` (bots com no mínimo 1.0s de dip).
2. **`InputPatches.cs` (`CantStandUpPatch`)**:
   - Atualizado o patch em `MovementContext.CanStandAt` para checar `TraumaPose.IsCrouchLocked(player)`.
   - Impede o jogador de subir a postura (`h > PoseLevel + 0.05f`) durante 1 segundo após o agachamento involuntário, liberando total movimento em seguida.
3. **`TRLImmersiveCombatMedicinePlugin.cs` & `.csproj`**:
   - Bump de versão SemVer para `1.13.2`.
4. **Validação de Build**:
   - Compilado `TRL-ImmersiveCombatMedicine.csproj` com **0 Erros**.

