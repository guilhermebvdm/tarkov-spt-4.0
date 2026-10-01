using System;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using Fika.Core.Networking.LiteNetLib;

namespace TRLWeatherSync.Networking;

public static class WeatherSyncNetworkHandler
{
    private static IFikaNetworkManager? _lastRegisteredManager;
    private static WeatherRole _lastRegisteredRole = WeatherRole.Unset;
    private static ManualLogSource Log => TRLWeatherSyncPlugin.Log;

    /// <summary>Assinar UMA vez no Awake do plugin — FikaEventDispatcher não suporta unsubscribe real
    /// (ver ressalva 2, docs/technical/fika-packet-desync-prevention-plan.md §4.1).</summary>
    public static void SubscribeManagerCreatedEvent()
    {
        FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerCreatedEvent>(OnManagerCreated);
    }

    private static void OnManagerCreated(FikaNetworkManagerCreatedEvent e)
    {
        try
        {
            // Este evento pode disparar antes de GameWorld.OnGameStarted (que cria WeatherSyncSession.Instance
            // via Begin()) — nesse caso Role resolve pra Unset e EnsurePacketsRegistered não faz nada aqui de
            // propósito. O polling em WeatherSyncSession.Update() cobre essa janela assim que Instance/Role
            // existir — é o padrão híbrido evento+polling do doc canônico (§4.1).
            EnsurePacketsRegistered(WeatherSyncSession.Instance?.Role ?? WeatherRole.Unset);
        }
        catch (Exception ex)
        {
            // airbag: um handler que lança aqui impede os handlers de OUTROS mods de rodar (DispatchEvent sem try/catch).
            Log.LogError($"[TRL-WeatherSync] Falha no registro via evento: {ex}");
        }
    }

    /// <summary>Chamar no Update() da sessão. `role` vem de WeatherSyncSession — Source E Receiver
    /// registram o mesmo handler de recepção (em raid Headless o Relay ecoa o pacote de volta pro
    /// Source original, que precisa de handler registrado ou sofre ParseException na própria máquina);
    /// Relay (só Headless) registra a variante com NetPeer, pra saber de quem veio e reencaminhar.</summary>
    public static void EnsurePacketsRegistered(WeatherRole role)
    {
        if (role == WeatherRole.Unset) return; // ainda não estamos em raid / sessão não resolvida
        if (!Singleton<IFikaNetworkManager>.Instantiated) return;

        var currentManager = Singleton<IFikaNetworkManager>.Instance;
        if (_lastRegisteredManager == currentManager && _lastRegisteredRole == role) return;

        try
        {
            switch (role)
            {
                case WeatherRole.Source:
                case WeatherRole.Receiver:
                    currentManager.RegisterPacket<TrlWeatherSyncPacket>(OnWeatherSyncPacketReceived);
                    break;
                case WeatherRole.Relay:
                    currentManager.RegisterPacket<TrlWeatherSyncPacket, NetPeer>(OnWeatherSyncPacketReceivedForRelay);
                    break;
            }

            _lastRegisteredManager = currentManager;
            _lastRegisteredRole = role;
            Log.LogInfo($"[TRL-WeatherSync] Pacote registrado (papel: {role}) na instância ativa do NetworkManager.");
        }
        catch (Exception ex)
        {
            Log.LogError($"[TRL-WeatherSync] Falha ao registrar pacote: {ex.Message}");
        }
    }

    /// <summary>Chamado só pelo papel Source (autoridade de clima) e pelo Relay (retransmissão) —
    /// envia pro(s) peer(s) visível(is). Num Cliente isso alcança só o servidor (FikaClient.cs:388);
    /// num Host normal (ou no Relay), todos os peers. Sempre a partir da main thread.</summary>
    public static void Broadcast(TrlWeatherSyncPacket packet)
    {
        if (!Singleton<IFikaNetworkManager>.Instantiated) return;

        try
        {
            Singleton<IFikaNetworkManager>.Instance.SendData(ref packet, DeliveryMethod.Unreliable, broadcast: true);
        }
        catch (Exception ex)
        {
            Log.LogWarning($"[TRL-WeatherSync] Erro ao transmitir pacote: {ex.Message}");
        }
    }

    private static void OnWeatherSyncPacketReceived(TrlWeatherSyncPacket packet)
    {
        try
        {
            if (!Singleton<GameWorld>.Instantiated) return;   // fora de raid, ignora
            if (!packet.Valid) return;                        // corpo truncado — não processa nem retransmite

            WeatherSyncSession.Instance?.ApplyReceivedWeather(packet);
        }
        catch (Exception ex)
        {
            // protege a fila de eventos do frame inteiro (AP-11, causa 4) — nunca deixar escapar.
            Log.LogError($"[TRL-WeatherSync] Exceção no handler de rede: {ex}");
        }
    }

    /// <summary>Só roda no processo Headless (papel Relay). NUNCA toca em WeatherController —
    /// só reencaminha o pacote recebido do Source pra todos os peers (o eco de volta pro próprio
    /// Source é inofensivo: ele só reaplicaria a si mesmo um clima muito próximo do que já tem).</summary>
    private static void OnWeatherSyncPacketReceivedForRelay(TrlWeatherSyncPacket packet, NetPeer sender)
    {
        try
        {
            if (!packet.Valid) return;

            Broadcast(packet);
        }
        catch (Exception ex)
        {
            Log.LogError($"[TRL-WeatherSync] Exceção ao retransmitir pacote (papel Relay): {ex}");
        }
    }
}
