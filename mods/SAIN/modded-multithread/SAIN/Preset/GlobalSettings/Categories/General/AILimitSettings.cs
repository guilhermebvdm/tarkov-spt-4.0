using System.Collections.Generic;
using SAIN.Attributes;

namespace SAIN.Preset.GlobalSettings;

public class AILimitSettings : SAINSettingsBase<AILimitSettings>, ISAINSettings
{
    [Name("Limit SAIN Function in AI vs AI - Global Toggle")]
    [Description(
        "Disables certains functions when ai are fighting other ai, and they aren't close to a human player. Turn off if you are spectating ai in free-cam."
    )]
    public bool LimitAIvsAIGlobal = true;

    [Description("How often (in seconds) to check distances to all human players.")]
    [MinMax(1f, 5f, 10f)]
    public float AILimitUpdateFrequency = 3f;

    [Description(
        "Defines the ranges that different tiers of AI limit are set. "
            + "If a bot is further than this number (in meters) from the closest Human Player, "
            + "they will be assigned this AI Limit setting."
    )]
    [MinMax(150f, 600f, 1f)]
    public Dictionary<AILimitSetting, float> AILimitRanges = new()
    {
        { AILimitSetting.Far, 150f },
        { AILimitSetting.VeryFar, 250f },
        { AILimitSetting.Narnia, 400f },
    };

    [Name("Limit AI vs AI Vision")]
    [Description("Reduces visible range for bots vs other bots if they are bot far from a human player.")]
    public bool LimitAIvsAIVision = true;

    [MinMax(10f, 200f, 1f)]
    public Dictionary<AILimitSetting, float> MaxVisionRanges = new()
    {
        { AILimitSetting.Far, 200f },
        { AILimitSetting.VeryFar, 100f },
        { AILimitSetting.Narnia, 50f },
    };

    [Name("Limit AI vs AI Hearing")]
    [Description("Reduces hearing distance for bots vs other bots if they are bot far from a human player.")]
    public bool LimitAIvsAIHearing = true;

    [MinMax(10f, 200f, 1f)]
    public Dictionary<AILimitSetting, float> MaxHearingRanges = new()
    {
        { AILimitSetting.Far, 100f },
        { AILimitSetting.VeryFar, 60f },
        { AILimitSetting.Narnia, 25f },
    };

    // ref: AUD-01-03 - Limiares do LOD adaptativo de tick-rate (modded-multithread), antes
    // hardcoded como `const` privadas em BotComponent.cs e não editáveis pelo F6.
    [Name("LOD Close Distance")]
    [Description("Distância (m) abaixo da qual o bot recebe atualização de IA em taxa máxima.")]
    [MinMax(20f, 100f, 1f)]
    public float LODCloseDistance = 50f;

    [Name("LOD Mid Distance")]
    [Description("Distância (m) abaixo da qual o bot recebe atualização em taxa média (~25Hz); acima, taxa mínima (~8Hz).")]
    [MinMax(50f, 300f, 1f)]
    public float LODMidDistance = 150f;

    [Name("LOD Close Distance Margin")]
    [Description("Margem de segurança (m) para sair da faixa \"perto\" - evita oscilação de taxa de atualização para bots parados perto do limiar.")]
    [MinMax(0f, 30f, 1f)]
    [Advanced]
    public float LODCloseDistanceMargin = 10f;

    [Name("LOD Mid Interval")]
    [Description("Intervalo (s) entre atualizações de IA para bots na faixa média de distância (~25Hz no padrão).")]
    [MinMax(0.02f, 0.1f, 100f)]
    [Advanced]
    public float LODMidIntervalSeconds = 0.04f;

    [Name("LOD Far Interval")]
    [Description("Intervalo (s) entre atualizações de IA para bots na faixa distante (~8Hz no padrão).")]
    [MinMax(0.05f, 0.3f, 100f)]
    [Advanced]
    public float LODFarIntervalSeconds = 0.12f;
}
