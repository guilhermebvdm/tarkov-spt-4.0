# Spec Técnica: Item 007 - Inércia e Velocidade Máxima

## Arquivos Afetados
- `modded/Plugin.cs`
- `modded/Patches/RaidLifecyclePatches.cs` (ou onde for injetada a aplicação das velocidades de inicio de raid)

## Propriedades do F12 a Adicionar
```csharp
// ConfigEntrys em Plugin.cs
public static ConfigEntry<float> _InertiaMultiplier;
public static ConfigEntry<float> _WalkSpeedMultiplier;
public static ConfigEntry<float> _SprintSpeedMultiplier;
public static ConfigEntry<float> _TurnPenaltyMultiplier;
```

## Como a Velocidade Máxima e Inércia funcionam no SPT
As variáveis do jogador e da movimentação derivam de `MovementContext` (`Player.MovementContext` no `Player`).
Entretanto, a forma mais resiliente e global de alterar Inércia base e velocidade é intervir no `BackendConfigSettingsClass` lido por todos os contextos de movimento e no `EFTHardSettings.Instance` durante a raid.

## Plano de Execução
1. **Interferência na Inicialização (`RaidLifecyclePatches.cs` / `GameWorldOnGameStartedPatch.cs`)**:
   - Assim como foi feito com a velocidade de crouch e lean (Item 005), os parâmetros de locomoção normal e sprint podem ser escalados.
   - O `EFTHardSettings.Instance.Inertia` e curvas relacionadas de aceleração e desaceleração devem sofrer override usando o `_InertiaMultiplier.Value`.
2. **Substituição Dinâmica no `MovementContext`**:
   - Para Walking/Sprint e Turn Penalty, acessar o `MainPlayer.MovementContext` se possível, ou as variáveis bases de penalidade de rotação que constam na configuração de armaduras/pesos da instância física (`Player.Physical`).
   - Se for o caso, podemos modificar os coeficientes em `Singleton<BackendConfigSettingsClass>.Instance.Inertia` multiplicando os vetores da curva (ex: `SprintSpeedInertiaCurveMin/Max`) ou apenas interceptar o `mc.MaxSpeed` adicionando limites via state (como em `mc.AddStateSpeedLimit()`).

## Possível Estratégia Alternativa
Para garantir que o código afete todos os métodos de caminhada, interceptar o getter do `WalkInertia` e do próprio Speed Limit via Harmony pode ser necessário se não houver um meio estático. Contudo, manipular via `BackendConfigSettingsClass` de forma passiva no `Awake` ou `OnGameStarted` é ideal em custo/benefício.

> [!NOTE]
> Essa parte precisará de alguma validação (tentativa e erro) para verificar a propriedade exata que a engine usa para velocidade do WSAD. Geralmente o SPT altera o `WalkSpeed` lido em `Player.MovementContext.MaxSpeed` ou afins.
