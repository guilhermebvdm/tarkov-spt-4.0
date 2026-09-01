# Documentação Técnica e Funcional — Climbable Ladders

Este diretório reúne a documentação técnica, arquitetural e funcional completa do mod **Climbable Ladders** (v1.0.3) para Escape From Tarkov / SPT 4.0.

---

## Sumário de Documentos

| Documento | Descrição | Status |
|---|---|---|
| [01. Visão Geral e Arquitetura](01-visao-geral-e-arquitetura.md) | Princípios de design, arquitetura em 4 módulos C#, ciclo de vida de raid (`GameWorld`) e injeção aditiva de AssetBundles de cenas por mapa. | 🟢 Vivo |
| [02. Controlador de Jogador e Máquina de Estados](02-controlador-de-jogador-e-maquina-de-estados.md) | Máquina de estados (`PlayerLadderController`), fluxo de transição com `ApproachState`, física de subida, consumo de estamina por peso, dano por fratura e modo barra fixa. | 🟢 Vivo |
| [03. Cinemática Inversa e Animação Procedural](03-cinematica-inversa-e-animacao-procedural.md) | Pipeline de animação corporal (`ProceduralLadderBody`), cinemática de membros (`FinalIK`), trajetórias em arco (`InArc`), rig de dedos (`ProceduralGrip`) e áudio balístico contextual. | 🟢 Vivo |
| [04. Infraestrutura de Cenas e Ferramentas de Edição](04-infraestrutura-de-cenas-e-ferramentas-de-edicao.md) | Componente `Ladder` e registro por `NetId`, modificadores de geometria (`GameObjectDisablerByPath`, `ProxyTransformModifierByPath`) e extensões do Unity Editor. | 🟢 Vivo |
| [05. Patches Harmony e Integração com EFT](05-patches-harmony-e-integracao-com-eft.md) | Patches Harmony (`GetAvailableActions`, `TranslateAxes`, `CanClimb`, `CanVault`, `DistanceToMainObstacle`), integração com Vaulting e tabela de remapeamento de obfuscação. | 🟢 Vivo |
| [06. Suporte Multiplayer Coop (Fika)](06-suporte-multiplayer-coop-fika.md) | Arquitetura de replicação de rede no Fika Core, pacotes (`LadderStatePacket`, `BarAnglePacket`), rastreador local e controlador observador remoto (`ObservedPlayerLadderController`). | 🟢 Vivo |
| [Relatório de Auditoria Técnica de Código (Review 01)](relatorio-auditoria-codigo-01.md) | Auditoria estática detalhada, memory leaks de trackers, efeitos colaterais de flags estáticas, raycasts de física e comparação com TRL-FikaSync. | 🟢 Vivo |

---

## Relação de Arquivos de Código-Fonte do Mod

### 1. Cliente BepInEx (`ladders.bep`)
- **Ponto de Entrada e Carregamento:**
  - [Plugin.cs](../modded/ladders.bep/Plugin.cs) — Plugin BepInEx client-side e gerenciador de ciclo de vida.
  - [LaddersLoader.cs](../modded/ladders.bep/LaddersLoader.cs) — Carregador dinâmico de cenas aditivas por `LocationId`.
- **Controlador e Máquina de Estados:**
  - [PlayerLadderController.cs](../modded/ladders.bep/PlayerLadderController.cs) — Controlador de física, estamina, fraturas e transições do jogador.
- **Cinemática Inversa e Rigging Procedural:**
  - [ProceduralLadderBody.cs](../modded/ladders.bep/ProceduralLadderBody.cs) — Orquestrador de animação do corpo e alinhamento de rig.
  - [ProceduralLadderLimb.cs](../modded/ladders.bep/ProceduralLadderLimb.cs) — Classe base para cinemática de membros e cálculo de arcos.
  - [ProceduralLadderArm.cs](../modded/ladders.bep/ProceduralLadderArm.cs) — Cinemática e offsets dos braços com FinalIK.
  - [ProceduralLadderLeg.cs](../modded/ladders.bep/ProceduralLadderLeg.cs) — Cinemática e offsets das pernas com FinalIK.
  - [ProceduralGrip.cs](../modded/ladders.bep/ProceduralGrip.cs) — Implementação procedural de `IWeaponGripPose` para deformação de dedos.
- **Patches Harmony e Obfuscação:**
  - [Patch_InteractionContextHelper_GetAvailableActions.cs](../modded/ladders.bep/Patch_InteractionContextHelper_GetAvailableActions.cs) — Injeção da ação "Climb" no menu de interação.
  - [Patch_MoveInputTranslator_TranslateAxes.cs](../modded/ladders.bep/Patch_MoveInputTranslator_TranslateAxes.cs) — Captura de eixos de input de movimento e rotação.
  - [Patch_Physical.cs](../modded/ladders.bep/Patch_Physical.cs) — Sobrescrita de permissões de escalada e salto.
  - [Patch_VaultingComponent.cs](../modded/ladders.bep/Patch_VaultingComponent.cs) — Ajuste da distância de obstáculos para transição em vaulting.
  - [Patch_GameWorld_OnGameStarted.cs](../modded/ladders.bep/Patch_GameWorld_OnGameStarted.cs) — Hook de início de raid para carga de cenas.
  - [Patch_GameWorld_Dispose.cs](../modded/ladders.bep/Patch_GameWorld_Dispose.cs) — Hook de encerramento de raid para descarte de cenas.
  - [GlobalUsings_EFTDeobfuscationRemap.cs](../modded/ladders.bep/GlobalUsings_EFTDeobfuscationRemap.cs) — Mapeamento global de tipos ofuscados do EFT 0.16.9.

### 2. Componentes Compartilhados (`ladders.shared`)
- [Ladder.cs](../modded/ladders.shared/Ladder.cs) — Componente de entidade física de escada e registro global por `NetId`.
- [GameObjectDisablerByPath.cs](../modded/ladders.shared/GameObjectDisablerByPath.cs) — Desativador/destruidor de colisores e geometrias de mapa por caminho de hierarquia.
- [ProxyTransformModifierByPath.cs](../modded/ladders.shared/ProxyTransformModifierByPath.cs) — Modificador de transformações espaciais de objetos originais da cena.

### 3. Ferramentas do Unity Editor (`ladders.shared.editor`)
- [LadderEditor.cs](../modded/ladders.shared.editor/LadderEditor.cs) — Handles 3D no Scene View para edição visual de largura, espaçamento e altura.
- [GameObjectDisablerByPathEditorWindow.cs](../modded/ladders.shared.editor/GameObjectDisablerByPathEditorWindow.cs) — Janela de ferramentas para seleção, teste de visibilidade e ping de nós na hierarquia.

### 4. Sincronização Multiplayer Fika (`ladders.fika`)
- [Plugin.cs](../modded/ladders.fika/Plugin.cs) — Plugin de extensão Fika com dependências `com.fika.core` e `com.tarkin.ladders`.
- [FikaHandler.cs](../modded/ladders.fika/FikaHandler.cs) — Registro de pacotes e manipulador de eventos de rede Fika.
- [MainPlayerTracker.cs](../modded/ladders.fika/MainPlayerTracker.cs) — Rastreador do jogador local e transmissor de pacotes de estado e ângulo.
- [ObservedPlayerLadderController.cs](../modded/ladders.fika/ObservedPlayerLadderController.cs) — Réplica remota com interpolação suave para jogadores observados.
- [LadderStatePacket.cs](../modded/ladders.fika/LadderStatePacket.cs) — Pacote serializável de entrada/saída de escada.
- [BarAnglePacket.cs](../modded/ladders.fika/BarAnglePacket.cs) — Pacote serializável de ângulo em barra fixa.
