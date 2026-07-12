# Memória de Sessões — TRL-ImmersiveCombatMedicine

## Estado atual
- HUD médico implementado (BandAidUI) e configurado para ignorar checagem de "IsAlive" no raycast para habilitar ECG plano em cadáveres.
- A aplicação de medicina trava (CheckManualInputs) se o paciente estiver morto.
- O consumo de recursos no MedicalLogic usa um sistema DiscardItemNetworked pré-verificando se o uso será total para não dessincronizar o HP da resource com o FIKA (evitando o slot fantasma).
- A verificação de paciente pelo laser (ScanForPatient) foi corrigida para SPT 4.0: usa Physics.AllLayers para suportar FikaObservedPlayer, o fallback GetComponentInParent<Player>() se GetPlayerByCollider falhar, e calcula a direção pelo cano da arma em vez do Camera.main que ficou instável no 4.0.
- O BotRagdollSimulator forçado em bots foi descartado por instabilidade/bug e eles agora apenas entram em prone quando desmaiam.

## Pendências
- Nenhuma pendência em aberto.

---

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
