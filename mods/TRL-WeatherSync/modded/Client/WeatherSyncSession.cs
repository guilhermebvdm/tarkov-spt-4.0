using System;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.Weather;
using Fika.Core.Main.Utils;
using TRLWeatherSync.Networking;
using UnityEngine;

namespace TRLWeatherSync;

/// <summary>Papel de autoridade de clima desta sessão. Independente de FikaBackendUtils.IsServer
/// quando a raid é hospedada por Fika-Headless — ver mods/FIKA/modded/Fika-Headless/.../DestroyGraphics
/// e Class444_Run_Patch.cs, que desativam o sistema de clima do próprio processo Headless.</summary>
public enum WeatherRole
{
    Unset,
    /// <summary>Lê a própria WeatherCurve e transmite periodicamente.</summary>
    Source,
    /// <summary>Só o processo Headless — retransmite o pacote do Source, nunca toca em WeatherController.</summary>
    Relay,
    /// <summary>Aplica o que chega via SetWeatherForce/HandleReconnect. Nunca transmite.</summary>
    Receiver,
}

public class WeatherSyncSession : MonoBehaviour
{
    public static WeatherSyncSession? Instance { get; private set; }
    private static ManualLogSource Log => TRLWeatherSyncPlugin.Log;

    // ref: CR-01-03 — WeatherClass.WindDirections é fixo (static readonly); precomputar os 9
    // vetores normalizados uma vez em vez de recalcular a cada chamada de NearestWindDirectionIndex.
    private static readonly Vector2[] NormalizedWindDirections = BuildNormalizedWindDirections();

    private static Vector2[] BuildNormalizedWindDirections()
    {
        var result = new Vector2[WeatherClass.WindDirections.Length];
        for (var i = 0; i < result.Length; i++)
        {
            result[i] = WeatherClass.WindDirections[i].normalized;
        }
        return result;
    }

    public WeatherRole Role { get; private set; } = WeatherRole.Unset;
    private float _accumulator;
    private float _stormCooldownRemaining;
    private bool _ended;

    public static void Begin()
    {
        if (Instance != null) return; // idempotente — Begin pode disparar mais de uma vez por engano
        var gameWorld = Singleton<GameWorld>.Instance;
        if (gameWorld == null || gameWorld.MainPlayer == null) return;
        if (gameWorld.MainPlayer is HideoutPlayer) return; // nunca em hideout

        var go = new GameObject(nameof(WeatherSyncSession));
        var session = go.AddComponent<WeatherSyncSession>();
        session.Role = ResolveRole();
        Instance = session;
        Log.LogInfo($"[TRL-WeatherSync] Sessão de sync iniciada — papel: {session.Role}.");
    }

    /// <summary>Regra de resolução de papel — desacopla "autoridade de clima" de "Host de rede"
    /// (necessário porque em raid Headless o Host de rede tem o sistema de clima desativado).
    /// Calculada uma vez por raid.</summary>
    private static WeatherRole ResolveRole()
    {
        // ref: mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/Utils/FikaBackendUtils.cs:59 —
        // true só no processo Headless em si.
        if (FikaBackendUtils.IsHeadless) return WeatherRole.Relay;

        // ref: FikaBackendUtils.cs:67 — true em todo cliente conectado a uma raid hospedada por Headless.
        if (FikaBackendUtils.IsHeadlessGame)
        {
            // ref: FikaBackendUtils.cs:71 — true no cliente que pediu a sessão Headless.
            return FikaBackendUtils.IsHeadlessRequester ? WeatherRole.Source : WeatherRole.Receiver;
        }

        // Raid normal (Host humano) — Host de rede é a autoridade de clima, como antes.
        // ref: mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/Packets/World/RequestSubPackets.cs:45
        return FikaBackendUtils.IsServer ? WeatherRole.Source : WeatherRole.Receiver;
    }

    public static void End()
    {
        if (Instance == null || Instance._ended) return; // idempotente
        Instance._ended = true;
        Destroy(Instance.gameObject);
        Instance = null;
    }

    private void Update()
    {
        WeatherSyncNetworkHandler.EnsurePacketsRegistered(Role);
        if (Role != WeatherRole.Source) return; // Relay e Receiver não enviam nada — só reagem a pacotes.
        if (!TRLWeatherSyncPlugin.EnableWeatherSync.Value) return;

        _accumulator += Time.unscaledDeltaTime;
        if (_accumulator < TRLWeatherSyncPlugin.SyncIntervalSeconds.Value) return;
        _accumulator = 0f;

        try
        {
            BroadcastCurrentWeather();
        }
        catch (Exception ex)
        {
            Log.LogError($"[TRL-WeatherSync] Falha ao montar/enviar broadcast de clima: {ex}");
        }
    }

    private void BroadcastCurrentWeather()
    {
        if (WeatherController.Instance == null) return;

        var curve = WeatherController.Instance.WeatherCurve; // ref: WeatherController.cs:86-96
        // ref: CR-01-01 — IWeatherCurve.Rain/Wind já vêm normalizados 0-1 (WeatherCurve.cs:48), mas
        // WeatherClass.Rain/Wind (o que SetWeatherForce espera) precisa estar na escala 1-5, porque
        // é isso que WeatherCurve.method_4() desnormaliza internamente via Mathf.InverseLerp(1f, 5f, ...)
        // (WeatherCurve.cs:247-248). Sem essa conversão de volta, o valor sempre cai pra 0 do lado do
        // Receiver. Mathf.Lerp(1f, 5f, x) é a inversa exata do InverseLerp(1f, 5f, x) do jogo.
        var packet = new TrlWeatherSyncPacket
        {
            TargetTime = DateTime.UtcNow.AddSeconds(TRLWeatherSyncPlugin.SyncIntervalSeconds.Value).Ticks,
            Cloudness = curve.Cloudiness,
            Wind = Mathf.Lerp(1f, 5f, curve.Wind.magnitude),
            WindDirection = NearestWindDirectionIndex(curve.Wind),
            Rain = Mathf.Lerp(1f, 5f, curve.Rain),
            ScaterringFogDensity = curve.Fog,
            Temperature = curve.Temperature,
            ThunderEventTrigger = RollForStorm(curve), // ref: CR-01-02
        };

        WeatherSyncNetworkHandler.Broadcast(packet);
    }

    /// <summary>
    /// Política simples de início de tempestade (CR-01-02): sorteia contra a probabilidade nativa
    /// de raio/trovão do jogo (IWeatherCurve.LightningThunderProbability, derivada da nebulosidade —
    /// WeatherCurve.cs:56) a cada ciclo de broadcast, respeitando um cooldown mínimo pra não disparar
    /// toda hora. Só cobre o INÍCIO — o fim da tempestade não é forçado (nunca mandamos "false" depois
    /// de "true"); cada jogador sai dela pelo tempo nativo do próprio jogo. Sincronizar o fim é trabalho
    /// futuro (pendência P-2.1 da memória do mod — precisa reverter dois state machines, Class444 E
    /// RainController, e ainda não sabemos como fazer isso com segurança).
    /// </summary>
    private bool RollForStorm(IWeatherCurve curve)
    {
        if (_stormCooldownRemaining > 0f)
        {
            _stormCooldownRemaining -= TRLWeatherSyncPlugin.SyncIntervalSeconds.Value;
            return false;
        }

        if (UnityEngine.Random.value >= curve.LightningThunderProbability) return false;

        _stormCooldownRemaining = TRLWeatherSyncPlugin.StormCheckCooldownSeconds.Value;
        return true;
    }

    /// <summary>
    /// Converte a direção contínua da curva (Vector2) para o índice discreto que WeatherClass
    /// espera — WeatherClass.WindDirections[] tem 9 vetores fixos (WeatherClass.cs:9-20);
    /// escolhe o de maior produto escalar com a direção normalizada.
    /// </summary>
    private static int NearestWindDirectionIndex(Vector2 wind)
    {
        if (wind.sqrMagnitude < 0.0001f) return 0;

        var normalized = wind.normalized;
        var bestIndex = 0;
        var bestDot = float.NegativeInfinity;

        for (var i = 0; i < NormalizedWindDirections.Length; i++)
        {
            var dot = Vector2.Dot(normalized, NormalizedWindDirections[i]);
            if (dot > bestDot)
            {
                bestDot = dot;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    public void ApplyReceivedWeather(TrlWeatherSyncPacket packet)
    {
        if (WeatherController.Instance == null) return;

        var target = new WeatherClass
        {
            Time = packet.TargetTime,
            Cloudness = packet.Cloudness,
            Wind = packet.Wind,
            WindDirection = packet.WindDirection,
            Rain = packet.Rain,
            ScaterringFogDensity = packet.ScaterringFogDensity,
            Temperature = packet.Temperature,
        };

        // ref: WeatherController.cs:120 — interpola nativamente via AnimationCurve (WeatherCurve.cs:92-110).
        WeatherController.Instance.SetWeatherForce(target);

        if (packet.ThunderEventTrigger)
        {
            // ref: Class443.cs:28 (Controller) + GInterface29.cs:17 (HandleReconnect)
            // Confirmado que entrar em Storm a partir de estado normal funciona (Class444.cs:181-224).
            // Encerrar uma tempestade chamando isso de novo a partir de dentro dela pode ser no-op
            // (Class451/Class452 não sobrescrevem HandleReconnect — herdam o no-op de Class445,
            // Class444.cs:94-98) — não resolvido nesta versão (pendência P-2.1 da memória do mod).
            Class443.Controller?.HandleReconnect(ESeasonStatus.Storm, SeasonsSettingsClass.Default);
        }
    }
}
