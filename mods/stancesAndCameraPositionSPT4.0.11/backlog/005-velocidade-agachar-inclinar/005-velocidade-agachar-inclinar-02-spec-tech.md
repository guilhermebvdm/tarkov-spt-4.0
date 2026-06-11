# Spec Técnica: Velocidade Agachar/Inclinar

## 1. Contexto Técnico
As velocidades de transição de pose e inclinação no Tarkov são geralmente controladas pelas propriedades de aceleração dentro do `Player.MovementContext` ou pelo multiplicador de velocidade da animação em `PlayerAnimator`.

## 2. Pontos de Interceptação (Harmony Patches)
- **Inclinação (Lean)**: Variáveis relacionadas ao tilt/lean speed (ex: `MovementContext.TiltSpeed` ou `MovementContext.UpdateTilt`).
- **Agachar/Deitar**: O `MovementContext.UpdatePose` ou `PoseLevel`.
- Vamos monitorar e injetar multiplicadores nesses getters ou nos métodos de atualização (Updates).

## 3. Configurações F12
```csharp
public static ConfigEntry<bool> EnableFasterTransitions;
public static ConfigEntry<float> CrouchProneSpeedMultiplier;
public static ConfigEntry<float> LeanSpeedMultiplier;

// Em PluginConfig
EnableFasterTransitions = Config.Bind("5. Movement Speed", "Enable Faster Transitions", true);
CrouchProneSpeedMultiplier = Config.Bind("5. Movement Speed", "Crouch/Prone Speed Multiplier", 1.5f);
LeanSpeedMultiplier = Config.Bind("5. Movement Speed", "Lean Speed Multiplier", 1.5f);
```
