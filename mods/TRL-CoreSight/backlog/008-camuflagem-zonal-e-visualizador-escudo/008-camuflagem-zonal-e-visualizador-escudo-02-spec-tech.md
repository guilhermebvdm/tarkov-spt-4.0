---
title: "008 — Especificação Técnica: Camuflagem Zonal Anatômica e Visualizador Debug"
date: "2026-09-16"
status: "🔵 Em andamento"
authors: ["TRL Team", "Antigravity"]
---

# 008 — Especificação Técnica: Camuflagem Zonal Anatômica e Visualizador Debug

## 1. Mapeamento de Classes e APIs do EFT

### 1.1. Esqueleto do Jogador (`PlayerBones`)
- `Player.PlayerBones.Head`: Transform da cabeça.
- `Player.PlayerBones.Ribcage`: Transform do tórax/peito/mochila.
- `Player.PlayerBones.Pelvis`: Transform do quadril (pivô central ao deitar).
- `Player.PlayerBones.LeftThigh1` e `LeftFoot`: Membro inferior esquerdo.
- `Player.PlayerBones.RightThigh1` e `RightFoot`: Membro inferior direito.

### 1.2. Visão e Mira da IA (`Assembly-CSharp`)
- `EnemyInfo.method_9`: Método de cálculo do multiplicador de velocidade de spotting do bot (`visibilitySpeedMultiplier`). Patch postfix multiplica o resultado pela fração de exposição do jogador (`1.0f - (CoverageRatio * 0.75f)`).
- `BotAimingClass.method_13`: Método que calcula `EndTargetPoint` do tiro do bot. Patch postfix adiciona offset/dispersão vetorial na mira se o jogador estiver parcialmente coberto (`CoverageRatio > 0.2f && CoverageRatio < 0.95f`).

## 2. Arquitetura de Componentes

### 2.1. `NaturalConcealmentManager` (Refatorado)
- Mantém 5 zonas (`ConcealmentZone`):
  ```csharp
  public class ConcealmentZone
  {
      public string Name;
      public Transform BoneTransform;
      public CapsuleCollider PhysicsCollider;
      public bool IsCovered;
      public Vector3 Offset;
      public float Radius;
      public float Height;
  }
  ```
- **Envelope de 30 cm:**
  - `Radius = BaseRadius + ModConfig.ConcealmentOffsetMeters.Value;` (padrão `+0.30m`).
- **Ciclo de Sensoriamento (0,2s):**
  - Executa `Physics.OverlapSphereNonAlloc` no layer `Foliage` (Layer 15) no centro de cada zona.
  - Verifica proximidade de `TreeInteractive` nativos (`player.AIData.IsInTree`).
  - Atualiza `IsCovered` de cada zona individualmente.
  - `CoverageRatio = coveredCount / 5.0f;`.

### 2.2. `ConcealmentVisualizer` (Novo)
- Anexado ao GameObject do gerenciador.
- Cria 5 primitivas visuais sem colisão (`MeshRenderer` + `MeshFilter`).
- Material em runtime com shader translúcido (`Transparent/Diffuse` / `Sprites/Default`) e cor `new Color(1f, 0f, 0f, 0.5f)`.
- Ativado via toggle F12 ou tecla de atalho configurável (padrão **F9**).
- No `LateUpdate`, sincroniza as posições, rotações e ativações dos visualizadores com as zonas cobertas.

## 3. Revisão Crítica e Riscos Técnicos

1. **Risco de Stutter:** A amostragem volumétrica por esferas roda a cada 0,2s com arrays pré-alocados (`NonAlloc`), gerando zero alocação de GC no hot path.
2. **Risco de Visibilidade em Câmeras:** O material translúcido do visualizador debug só deve renderizar se o usuário explicitamente ativar no F12 / F9, permanecendo 100% desligado durante gameplay padrão.
