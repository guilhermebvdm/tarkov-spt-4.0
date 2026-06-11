# Spec Técnica: Apoiar Arma em Superfícies (Weapon Mounting)

## 1. Contexto Técnico
Precisamos introduzir uma mecânica de "Mounting" ou "Weapon Resting" para estabilizar a arma. O jogo base (Tarkov) na versão mais recente possui sua própria mecânica nativa (`Player.MovementContext.EnterMountedState()`), no entanto, precisaremos interceptá-la ou replicar nossa própria lógica para controlar as variáveis de recoil, estamina e sway, similar ao que o Realism Mod faz.

## 2. Pontos de Interceptação (Harmony Patches)

### 2.1 Controle de Input
Devemos monitorar a ação de montar (seja interceptando `ECommand.WeaponMounting` no `InputOverrides` equivalente, ou lendo teclas).
- **Patch alvo sugerido**: `EFT.Player.Update()` ou o componente de Input (`EFT.InputSystem`).
- Se utilizarmos o mounting nativo, o estado já estará em `Player.MovementContext._inMountedState`. O Realism Mod faz uso do `player.MovementContext.EnterMountedState()` injetando dados falsos ou customizados de `MountPointData` caso não exista superfície válida do jogo base, mas para nós, pode ser mais seguro depender do sistema da BSG ou criar um "Raycast" da câmera para frente.

### 2.2 Redução de Estamina de Braço
A estamina de braço no Tarkov é gerenciada pelo `Player.Stamina` e afetada no `Player.Update` ou durante o *Aiming*.
- Precisaremos aplicar um multiplicador zero para o dreno de estamina do braço enquanto `IsMounting == true`.

### 2.3 Redução de Recuo e Sway
O Recuo e Sway (Balanço) são gerenciados pelas classes de animação de arma, como `ProceduralWeaponAnimation` (PWA).
- **Patch alvo sugerido**: `ProceduralWeaponAnimation.Update()` ou `ProceduralWeaponAnimation.ApplyMounting()`.
- Variáveis para interceptar e sobrescrever multiplicando pelos valores configurados no F12:
  - `pwa.Breath.Intensity` *= F12_SwayMultiplier
  - `pwa.HandsContainer.HandsRotation.InputIntensity` *= F12_SwayMultiplier
  - `pwa.Shootingg.RecoilStrength` *= F12_RecoilMultiplier

## 3. Configurações BepInEx (PluginConfig)
Criar uma seção no F12 em `PluginConfig.cs`:
```csharp
public static ConfigEntry<bool> EnableWeaponMounting;
public static ConfigEntry<float> MountingRecoilMultiplier;
public static ConfigEntry<float> MountingSwayMultiplier;

// Inicialização:
EnableWeaponMounting = Config.Bind("4. Weapon Mounting", "Enable Weapon Mounting", true, "Ativa a funcionalidade de apoiar arma em superfícies.");
MountingRecoilMultiplier = Config.Bind("4. Weapon Mounting", "Recoil Multiplier", 0.5f, new ConfigDescription("Multiplicador de recuo ao apoiar a arma.", new AcceptableValueRange<float>(0.1f, 1.0f)));
MountingSwayMultiplier = Config.Bind("4. Weapon Mounting", "Sway Multiplier", 0.2f, new ConfigDescription("Multiplicador de balanço (sway) ao apoiar a arma.", new AcceptableValueRange<float>(0.1f, 1.0f)));
```

## 4. Implementação
1. Criar `WeaponMountingPatch.cs` na pasta `modded/`.
2. Adicionar os patches para ler a ação de montar e manter o estado em uma variável global ou no controle de postura atual.
3. Se o jogador estiver montado, interceptar os cálculos de estamina (ex: `PlayerPatches.StaminaDrainPatch`) e retornar `false` ou modificar o `amount` para 0.
4. Aplicar a redução na `ProceduralWeaponAnimation`.

## 5. Passos Seguintes
- Implementar e testar `WeaponMountingPatch.cs`.
- Verificar conflitos com outras instâncias de Mod de Recoil ou Camera Position.
