# Inventário de Funcionalidades — TRL-ImmersiveCombatMedicine

Este documento cataloga todas as funcionalidades e subsistemas do mod **TRL-ImmersiveCombatMedicine** no workspace [`modded-testchannel`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel) para a rodada de **Revisão Minuciosa** com base no [`.claude/commands/code-review.md`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/.claude/commands/code-review.md).

---

## Estrutura do Roteiro de Revisão

O mod é composto por **16 funcionalidades** divididas em dois grandes domínios:
1. **Módulo Médico (`BandAid` / Medicina de Combate e Coop)**: Itens 01 a 08.
2. **Módulo Trauma 2.0 (Motor Físico, Queda, Desmaio e Punições)**: Itens 09 a 16.

---

## Módulo 1: Medicina em Combate & Coop (`BandAid`)

### Item 01 · Interação Nativa e Ativação do Modo Médico
- **Arquivos:** [`Patches/Medical/MedicInteractable.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/MedicInteractable.cs), [`Patches/Medical/MedicActionsPatch.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/MedicActionsPatch.cs), [`Patches/Medical/BandAidController.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/BandAidController.cs)
- **Mecânica:** Injeção de `MedicInteractable` em jogadores/bots vivos via `ActionPanel` nativo do EFT; opções "Examinar (Médico)" e "Tocar no Ombro"; gate de proximidade (~1,3m) e ativação/desativação limpa do modo médico.

### Item 02 · HUD Médico do Operador e Monitor Cardíaco (ECG)
- **Arquivos:** [`Patches/Medical/BandAidUI.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/BandAidUI.cs), [`Patches/Medical/MedicLocale.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/MedicLocale.cs), [`Helpers/ImageLoader.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Helpers/ImageLoader.cs)
- **Mecânica:** Interface visual (Canvas Unity) exibindo a silhueta do corpo do paciente em tempo real, status de HP por membro, ícones de ferimento (sangramento leve/pesado, fratura, membro destruído), pulso cardíaco dinâmico baseado na saúde, linha reta (flatline) em caso de morte e feedback em texto das ações em andamento.

### Item 03 · Lógica de Tratamento e Seleção Inteligente de Ferimentos (`SmartTarget`)
- **Arquivos:** [`Patches/Medical/MedicalLogic.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/MedicalLogic.cs), [`Helpers/ItemDatabase.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Helpers/ItemDatabase.cs)
- **Mecânica:** Identificação prioritária de partes feridas compatíveis com o item selecionado (`FindSmartTarget`); aplicação direta de curativos, analgésicos, hemostáticos e kits médicos em pacientes locais (bots/self); cálculo de débitos e descarte ao zerar.

### Item 04 · Animação em Primeira Pessoa, Perks de Classe e Redirecionamento
- **Arquivos:** [`Patches/Medical/MedicHealPatch.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/MedicHealPatch.cs), [`Patches/Medical/BandAidController.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/BandAidController.cs), [`Patches/Medical/CustomClassesBridge.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/CustomClassesBridge.cs)
- **Mecânica:** Execução da animação visual nas mãos do médico (`SetInHands`); aplicação de modificadores de velocidade de animação baseados em classes/perks do jogador; supressão de corte prematuro via `MedsMethod8Patch`; transição de mãos ao concluir.

### Item 05 · Cancelamento de Cura com Regra de Desesterilização (Punição Canônica)
- **Arquivos:** [`Patches/Medical/BandAidController.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/BandAidController.cs), [`Patches/Medical/MedicLocale.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/MedicLocale.cs)
- **Mecânica:** Interrupção voluntária pelo médico clicando com o Mouse0; tolerância de 1.0s (`ItemRemoveAfterInterruptionTime`); desconto de 1 carga para kits de cirurgia (CMS/Surv12) e consumíveis de uso único sem aplicar o efeito no paciente; feedback visual dedicado.

### Item 06 · Protocolo de Rede Cooperativo FIKA (Handshake & Tratamento Remoto)
- **Arquivos:** [`Patches/Medical/BandAidNetworkHandler.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/BandAidNetworkHandler.cs), [`Patches/Medical/PacketEnvelope.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/PacketEnvelope.cs), [`Patches/Medical/BandAidHealPacket.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/BandAidHealPacket.cs), [`Patches/Medical/BandAidHealCheckPacket.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/BandAidHealCheckPacket.cs), [`Patches/Medical/BandAidTreatmentReportPacket.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/BandAidTreatmentReportPacket.cs), [`Patches/Medical/BandAidShoulderTapPacket.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/BandAidShoulderTapPacket.cs)
- **Mecânica:** Handshake tripartite de rede (Check -> Response -> Heal -> Report); envelope binário com magic header `TRLM` no canal 3 isolado; sincronização de saldo real do médico e cálculo autoritativo de custo no paciente com fallback de timeout.

### Item 07 · Sistema Realista de Torniquetes e Necrose por Tempo
- **Arquivos:** [`Patches/Medical/TourniquetManager.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/TourniquetManager.cs), [`Patches/Medical/BandAidController.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/BandAidController.cs)
- **Mecânica:** Aplicação de torniquetes (Esmarch, CAT) em membros com sangramento pesado; estancamento contínuo; contador de necrose por tempo de permanência; danos progressivos ao membro se não for removido a tempo; devolução ou destruição do item.

### Item 08 · Ressuscitação com Desfibrilador e Integração com Coma/Downed
- **Arquivos:** [`Patches/Trauma/FikaRevivePatch.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Trauma/FikaRevivePatch.cs), [`Patches/Medical/MedicalLogic.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/MedicalLogic.cs)
- **Mecânica:** Patching no hook de revive do Fika; consumo do desfibrilador via `DiscardItemNetworked`; restauração do estado vital do aliado sem corrupção de slots de inventário nem congelamento de mãos.

---

## Módulo 2: Motor Físico, Queda, Desmaio & Punições (`Trauma 2.0`)

### Item 09 · Trauma Engine & Motor de Estados Reativos
- **Arquivos:** [`Patches/Trauma/TraumaEngine.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Trauma/TraumaEngine.cs), [`Patches/Trauma/TraumaEngineState.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Trauma/TraumaEngineState.cs), [`Patches/Trauma/TraumaMatrixResolver.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Trauma/TraumaMatrixResolver.cs), [`Patches/Trauma/TraumaState.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Trauma/TraumaState.cs)
- **Mecânica:** Leitura contínua da saúde corporal do jogador; resolução reativa de severidade por parte do corpo; reconciliação de analgésicos e cura; disparo de eventos para consumidores de efeito.

### Item 10 · Sistema de Pernas, Mancar N1/N2 e Bloqueio de Sprint
- **Arquivos:** [`Patches/Trauma/TraumaLegsConsumer.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Trauma/TraumaLegsConsumer.cs), [`Patches/Trauma/SpeedLimitPatches.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Trauma/SpeedLimitPatches.cs), [`Patches/Trauma/MovementPatches.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Trauma/MovementPatches.cs)
- **Mecânica:** Penalidade de mancar escalonada (N1 = 80%, N2 = 55%); bloqueio estrito de sprint com pernas comprometidas sem analgésico; agachamento involuntário one-shot (`TraumaPose`).

### Item 11 · Ciclo de Queda (Fall Cycle), FSM e Hold de Bots
- **Arquivos:** [`Patches/Trauma/TraumaFallCycleConsumer.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Trauma/TraumaFallCycleConsumer.cs), [`Patches/Trauma/TraumaBotFall.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Trauma/TraumaBotFall.cs), [`Patches/Trauma/TraumaPose.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Trauma/TraumaPose.cs)
- **Mecânica:** Queda forçada em prone ao sofrer trauma grave nas 2 pernas; máquina de estados finitos (FallPending -> Blocked -> Released -> Rising -> Window); negação de levantar na origem via `CanStandAt`; integração com BigBrain para controle e hold de bots.

### Item 12 · Sistema de Braços, Fadiga de Mira e Lockout de ADS
- **Arquivos:** [`Patches/Trauma/TraumaArmsConsumer.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Trauma/TraumaArmsConsumer.cs), [`Patches/Trauma/ArmsAimPatches.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Trauma/ArmsAimPatches.cs), [`Patches/Trauma/InputPatches.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Trauma/InputPatches.cs), [`Patches/Trauma/TraumaTremor.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Trauma/TraumaTremor.cs)
- **Mecânica:** Instabilidade e balanço de mira em braços fraturados ou zerados; dreno acelerado de estamina de braço; cancelamento automático de visada (ADS) ao tomar dano e bloqueio temporário de re-ADS.

### Item 13 · Sistema de Estômago e Efeitos Metabólicos
- **Arquivos:** [`Patches/Trauma/TraumaStomachConsumer.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Trauma/TraumaStomachConsumer.cs)
- **Mecânica:** Reações físicas ao dano no estômago (tosse/engasgo); aceleração do consumo de energia e hidratação do jogador; roll dinâmico de reavaliação a cada novo ferimento.

### Item 14 · Sistema de Desmaio (Blackout / Faint) e Aggro de IA
- **Arquivos:** [`Patches/Trauma/TraumaBlackoutTrigger.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Trauma/TraumaBlackoutTrigger.cs), [`Patches/Trauma/TraumaFaintPacket.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Trauma/TraumaFaintPacket.cs), [`Helpers/AggroHelper.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Helpers/AggroHelper.cs), [`Debugging/DebugBotInvisibility.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Debugging/DebugBotInvisibility.cs)
- **Mecânica:** Perda súbita de consciência por trauma massivo (dano percentual da vida atual com analgésico como gate); supressão de aggro de IA durante o desmaio; sincronização de blackout na rede; recuperação temporal limpa.

### Item 15 · Sistema de Voz Diegética e Expressões de Dor
- **Arquivos:** [`Patches/Trauma/TraumaVoice.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Trauma/TraumaVoice.cs), [`Patches/Trauma/TraumaPainVoice.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Trauma/TraumaPainVoice.cs), [`Helpers/VoiceAndHealthUtils.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Helpers/VoiceAndHealthUtils.cs)
- **Mecânica:** Reprodução de falas de dor nativas do EFT (`LegBroken`, `HandBroken`, `OnAgony`) mapeadas aos eventos de trauma; controle de repetição e anti-spam.

### Item 16 · Purga de Estado, Observabilidade e Reset de Ciclo de Vida entre Raids
- **Arquivos:** [`Patches/Trauma/TraumaPurge.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Trauma/TraumaPurge.cs), [`Patches/Trauma/TraumaObservability.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Trauma/TraumaObservability.cs), [`Patches/Trauma/TraumaConsumerLifecycle.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Trauma/TraumaConsumerLifecycle.cs), [`TRLImmersiveCombatMedicinePlugin.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/TRLImmersiveCombatMedicinePlugin.cs)
- **Mecânica:** Auditoria estática e dinâmica de estados ao sair/entrar em raids; limpeza incondicional de campos estáticos, coroutines e timers; prevenção de memory leaks de GC e telemetria de diagnóstico.
