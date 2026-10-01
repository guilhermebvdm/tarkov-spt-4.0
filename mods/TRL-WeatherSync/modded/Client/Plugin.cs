using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using TRLWeatherSync.Networking;
using TRLWeatherSync.Patches;

namespace TRLWeatherSync;

[BepInPlugin("trl.weathersync", "TRL-WeatherSync", "1.3.0")]
[BepInDependency("com.fika.core", BepInDependency.DependencyFlags.HardDependency)]
public class TRLWeatherSyncPlugin : BaseUnityPlugin
{
    internal static ManualLogSource Log = null!;

    public static ConfigEntry<bool> EnableWeatherSync { get; private set; } = null!;
    public static ConfigEntry<float> SyncIntervalSeconds { get; private set; } = null!;
    public static ConfigEntry<float> MinRainDropSize { get; private set; } = null!;

    // Configurações de gotas na lente (item 004)
    public static ConfigEntry<bool> EnableLensDropsTuning { get; private set; } = null!;
    public static ConfigEntry<float> LensRainDropsLookUpMultiplier { get; private set; } = null!;
    public static ConfigEntry<float> LensRainDropsForwardRate { get; private set; } = null!;
    public static ConfigEntry<bool> LensRainDropsLookDownDrain { get; private set; } = null!;
    public static ConfigEntry<float> LensRainDropsMaxLifetime { get; private set; } = null!;

    private void Awake()
    {
        Log = Logger;

        EnableWeatherSync = Config.Bind(
            "Networking",
            "Enable Weather Sync",
            true,
            new ConfigDescription(
                "Ativa a sincronização contínua de clima entre Host e Clientes em raids FIKA. Sem efeito fora de raids coop. " +
                "IMPORTANTE: este mod precisa estar instalado em TODOS os participantes da raid (Host e Clientes) — um peer " +
                "sem o mod pode causar problemas de rede para todo mundo na raid."));

        SyncIntervalSeconds = Config.Bind(
            "Networking",
            "Sync Interval Seconds",
            10f,
            new ConfigDescription(
                "Intervalo, em segundos, entre cada pacote de sincronização de clima enviado pela autoridade de clima da raid. " +
                "Valores menores deixam o clima mais preciso, mas aumentam o tráfego de rede.",
                new AcceptableValueRange<float>(5f, 30f)));

        MinRainDropSize = Config.Bind(
            "Rain",
            "Min Rain Drop Size",
            0.08f,
            new ConfigDescription(
                "Define o tamanho mínimo visível da gota de chuva (útil para enxergar chuvas fracas de frente). " +
                "Ajusta ao vivo sem precisar reiniciar a raid. Não altera chuvas fortes caso já ultrapassem este tamanho. Defina 0 para desativar.",
                new AcceptableValueRange<float>(0f, 0.30f)));

        EnableLensDropsTuning = Config.Bind(
            "Lens Drops",
            "Enable Lens Drops Tuning",
            true,
            new ConfigDescription(
                "Ativa a calibragem dinâmica de gotas de chuva na lente da câmera reativas ao ângulo de visão. " +
                "Se desativado, o comportamento retorna 100% ao original do jogo."));

        LensRainDropsLookUpMultiplier = Config.Bind(
            "Lens Drops",
            "Look Up Multiplier",
            2.5f,
            new ConfigDescription(
                "Multiplicador de quantidade e frequência de gotas ao olhar para cima (para o céu).",
                new AcceptableValueRange<float>(1.0f, 5.0f)));

        LensRainDropsForwardRate = Config.Bind(
            "Lens Drops",
            "Forward Rate Multiplier",
            1.5f,
            new ConfigDescription(
                "Multiplicador base da taxa de gotas ao olhar para a frente/horizonte (reforça a presença de pingos em chuvas fracas).",
                new AcceptableValueRange<float>(0.5f, 3.0f)));

        LensRainDropsLookDownDrain = Config.Bind(
            "Lens Drops",
            "Look Down Drain Effect",
            true,
            new ConfigDescription(
                "Faz as gotas da lente secarem/escorrerem rapidamente ao inclinar a cabeça para o chão (simula a aba do capacete/boné protegendo o rosto)."));

        LensRainDropsMaxLifetime = Config.Bind(
            "Lens Drops",
            "Max Drop Lifetime Seconds",
            8.0f,
            new ConfigDescription(
                "Tempo máximo de vida de cada gota na lente em segundos. Valores menores deixam o ciclo de gotas mais contínuo e orgânico (o padrão do jogo é 25s).",
                new AcceptableValueRange<float>(2.0f, 25.0f)));

        // ref: 06-fix-01 — removida a ConfigEntry "Storm Check Cooldown Seconds" (seção "Storm") junto com a
        // feature de tempestade sincronizada (CR-01-02), que causava nevasca incorreta em vez de tempestade de
        // verão. Um config antigo do usuário pode manter a chave órfã "Storm.Storm Check Cooldown Seconds" no
        // .cfg salvo — BepInEx ignora chaves sem ConfigEntry correspondente, sem erro nem efeito.

        new GameWorldOnGameStartedPatch().Enable();
        new GameWorldOnDestroyPatch().Enable();
        new CoopGameStopPatch().Enable();
        new RainDropVisibilityPatch().Enable();
        new CameraLensRainDropsPatch().Enable();

        WeatherSyncNetworkHandler.SubscribeManagerCreatedEvent();

        Log.LogInfo("[TRL-WeatherSync] Plugin inicializado.");
    }
}
