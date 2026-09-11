using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using TRLWeatherSync.Networking;
using TRLWeatherSync.Patches;

namespace TRLWeatherSync;

[BepInPlugin("trl.weathersync", "TRL-WeatherSync", "1.1.1")]
[BepInDependency("com.fika.core", BepInDependency.DependencyFlags.HardDependency)]
public class TRLWeatherSyncPlugin : BaseUnityPlugin
{
    internal static ManualLogSource Log = null!;

    public static ConfigEntry<bool> EnableWeatherSync { get; private set; } = null!;
    public static ConfigEntry<float> SyncIntervalSeconds { get; private set; } = null!;
    public static ConfigEntry<float> StormCheckCooldownSeconds { get; private set; } = null!;

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

        StormCheckCooldownSeconds = Config.Bind(
            "Storm",
            "Storm Check Cooldown Seconds",
            300f,
            new ConfigDescription(
                "Intervalo mínimo, em segundos, entre tentativas de iniciar uma tempestade sincronizada. A cada ciclo, " +
                "a autoridade de clima da raid sorteia contra a probabilidade nativa de raio/trovão do jogo (baseada na " +
                "nebulosidade); se o sorteio ganhar, a tempestade começa sincronizada para todos. O FIM da tempestade " +
                "ainda não é sincronizado nesta versão — cada jogador sai dela pelo tempo nativo do próprio jogo (CR-01-02 " +
                "/ pendência P-2.1: sincronizar o fim é trabalho futuro, ainda não resolvido com segurança).",
                new AcceptableValueRange<float>(60f, 1800f)));

        new GameWorldOnGameStartedPatch().Enable();
        new GameWorldOnDestroyPatch().Enable();
        new CoopGameStopPatch().Enable();

        WeatherSyncNetworkHandler.SubscribeManagerCreatedEvent();

        Log.LogInfo("[TRL-WeatherSync] Plugin inicializado.");
    }
}
