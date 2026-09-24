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
    /// <summary>Aplica o que chega via SetWeatherForce. Nunca transmite.</summary>
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
            // ThunderEventTrigger deliberadamente não setado (fica false) — ref: 06-fix-01.
        };

        WeatherSyncNetworkHandler.Broadcast(packet);
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

        // ref: 06-fix-01 — removido o antigo `if (packet.ThunderEventTrigger) Class443.Controller?.HandleReconnect(
        // ESeasonStatus.Storm, ...)`. Causa raiz confirmada em RainController.cs:250-280: o estado "Storm" do
        // RainController (Class670.vmethod_7() → Class678) SEMPRE desliga a chuva e liga neve/nevasca
        // (_snowWetRenderer.WinterShow=true, _snowFlakes ativado), não importa a estação atual — não existe
        // "tempestade de verão" nesse state machine, só o efeito de nevasca do Winter Event. Rain/Cloudness/Wind
        // altos via SetWeatherForce (linha acima) já produzem trovão/raio nativamente quando a curva pede,
        // sem precisar dessa troca de estado.
    }
}
