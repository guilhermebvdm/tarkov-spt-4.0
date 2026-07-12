# Memória de Sessões — TRL-ImmersiveCombatMedicine

## Estado atual
- CAUSA-RAIZ do "prompt F nunca aparece" encontrada (2026-07-11): o DLL implantado em D:\SPT era uma build ANTIGA (pré-commits, ainda com `Camera.main` + early-return silencioso) — nenhum fix das rodadas anteriores chegou ao jogo. A fonte não compilava do repo (csproj referenciava `References\*.dll` inexistente) e o mod usava `client/` em vez de `modded/`, ficando fora do tooling (compile-mod.sh e update-graphs.sh).
- Estrutura alinhada à convenção do repo: `client/` → `modded/` (git mv). Build+install via `bash .agents/scripts/compile-mod.sh TRL-ImmersiveCombatMedicine` funciona (References auto-resolvidas de D:\SPT; `ItemComponent.Types.dll` adicionado ao mapa do script). DLL novo implantado e verificado por forense binária (contém SphereCastAll/WeaponRoot/LookDirection, sem Camera.main).
- Validação estática (workflow 12 agentes, verificação adversarial): TODAS as APIs do scan existem e são válidas no EFT 0.16.x — LookDirection nunca é zero; WeaponRoot é campo serializado válido; GetPlayerByCollider só registra a cápsula do CharacterController (BodyPartColliders resolvem via fallback GetComponentInParent<Player>, que funciona pois hitboxes são filhas do GO do Player e, sob Fika, todo player/bot de rede é subclasse de Player).
- Padrão canônico de interação do vanilla (referência p/ melhoria futura): GamePlayerOwner.LateUpdate → Player.InteractionRaycast (origem PlayerBones.LootRaycastOrigin, dist ~2.8, mask Interactive|Deadbody|Player|Loot) → GameWorld.FindInteractable → ActionPanel.
- HUD médico implementado (BandAidUI) com ECG plano em cadáveres; aplicação de medicina trava (CheckManualInputs) se o paciente estiver morto; consumo usa DiscardItemNetworked pré-verificado (anti slot fantasma); bots desmaiados entram em prone (BotRagdollSimulator descartado).

## Pendências
- [P-2.9] (aberta 2026-07-12) VALIDAR IN-GAME o lote da sessão autônoma /g-autodev (commits e89e6100..e7683f19 + anotações): 15 achados CR-01 aplicados (incl. bloqueadores CR-01-01 client→bot, CR-01-02 faint sync, CR-01-04 defib) + G-2 (médico vê bleeds/fraturas de paciente REMOTO). Testes-chave: (a) solo-host: curar bot 2× (regressão mão travada), desmaiar e morrer desmaiado → menu com áudio normal e raid seguinte sem prone fantasma; (b) 2 PCs: client cura bot (agora deve funcionar!), client desmaia → bots do host param de atirar, ícones de efeito do paciente remoto no HUD; (c) revive com desfibrilador consome o item sem quebrar o revive. [DEBUG-ICM] mantido de propósito para esses testes.
- [P-2.1] (aberta 2026-07-11, atualizada 2026-07-12) Validar em raid (HITL): mirar em bot/player vivo a ~1,3 m → ações "Examinar (Médico)" e "Tocar no ombro" devem aparecer no ActionPanel NATIVO (como loot). CAUSA #2 encontrada pelas sondas: o host GameObject era destruído pela limpeza de boot do EFT logo após "Chainloader startup complete" (DontDestroyOnLoad não protege de Destroy explícito); no 3.11 funcionava porque tudo vivia no GO do plugin. Fix: componentes no GO do plugin + interação migrada para o pipeline nativo (MedicInteractable : InteractableObject + postfix em GetActionsClass.GetAvailableActions, padrão Fika 2.3.4). ATENÇÃO: launcher com Dev Mode OFF pode reverter o DLL local no sync.
- [P-2.2] (aberta 2026-07-11, atualizada 2026-07-12) Após validar, remover TODAS as sondas [DEBUG-ICM] (grep pelo tag) e os campos/configs mortos do shoulder-tap por tecla (warnings CS0414); ScanForPatient e o canvas custom já foram removidos no refactor da interação nativa.
- [P-2.3] (aberta 2026-07-11) Coop-sync: FikaBridge.SyncFaintStatus só atualiza lista local — o FikaPacketManager.cs do TrueTrauma 3.11 não foi migrado; desmaio não é propagado aos peers.
- [P-2.6] (aberta 2026-07-12) Validar in-game o fix do redirect de cura (commit 25be6540): bot com bleed/HP faltando → log deve mostrar "method_5 CHAMADO" → "Cura redirecionada", HP do bot subindo e HpResource da Salewa caindo UMA vez só. CAUSA era nomes camelCase (medsController_0/queue_0/float_0) vs campos reais PascalCase (MedsController_0/Queue_0/Float_0, Player.cs:19444/19453/19456) — AccessTools.Field case-sensitive → vanilla aplicava no MÉDICO saudável → FailedToApply instantâneo.
- [P-2.8] (aberta 2026-07-12) Validar fix da mão travada (commit 44390b2f): 2ª cura no MESMO bot ferido enquanto ele se auto-medica → animação do médico deve completar normalmente. CAUSA: bots usam o mesmo ObservedMedsControllerClass; o redirect sequestrava a operação do BOT (Surv12 do estômago), sobrescrevia _currentObservedMedsControllerClass e o ForceFinishAnimation finalizava a operação errada — a do médico ficava órfã (mão presa, só HANB End resolvia). LIÇÃO COOP: patch em controller de item SEMPRE precisa de guard de ownership (owner == MainPlayer) — bots/peers passam pelo mesmo código.
- [P-2.7] (aberta 2026-07-12) Bug latente (achado do verificador, não corrigido — 1 variável por vez): OnPatientEffectRemoved filtra `effect is GInterface350`, mas MedEffect nativo implementa GInterface376 (GInterface350 = marcador tipo painkiller, só Berserk) — bridge method_8 nunca dispara; finalização hoje depende 100% do timer ForceFinishAnimation. Avaliar trocar filtro para GInterface376 após P-2.6 validado.
- [P-2.4] (aberta 2026-07-11) (Opcional) Alinhar o scan ao padrão canônico (origem PlayerBones.LootRaycastOrigin; WeaponRoot+LookDirection diverge da câmera em freelook).
- [P-2.5] (aberta 2026-07-11, fechada 2026-07-12) RESOLVIDA com correção de diagnóstico: o pin do manifest (6ccdd2b) JÁ ERA a v2.3.4 — o que estava velho era o CHECKOUT LOCAL (02c0de7a, era 2.2.6) e os grafos gerados dele. Rematerializado fika-plugin e fika-server nos pins (= tag v2.3.4 = Fika instalado), grafos regenerados, manifest com fikaVersion+vendoredAt. Referência p/ P-2.4: Fika 2.3.4 pluga o prompt de revive via patch em GetActionsClass.GetAvailableActions (Fika.Core/Main/Patches/Revival/GetActionsClass_GetAvailableActions_Patch.cs) — padrão canônico a imitar no prompt F.

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
