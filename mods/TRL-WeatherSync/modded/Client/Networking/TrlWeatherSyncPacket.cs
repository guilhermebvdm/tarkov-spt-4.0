using System;
using Fika.Core.Networking.LiteNetLib.Utils;

namespace TRLWeatherSync.Networking;

/// <summary>
/// Pacote broadcast periódico do papel Source com o alvo de clima corrente da raid.
/// Campos espelham o subconjunto necessário de WeatherClass (Assembly-CSharp/WeatherClass.cs:22-56)
/// para reconstruir um WeatherClass e chamar WeatherController.Instance.SetWeatherForce(...).
/// </summary>
public struct TrlWeatherSyncPacket : INetSerializable
{
    /// <summary>Ticks (long) do momento-alvo da transição — vira WeatherClass.Time.</summary>
    public long TargetTime;

    public float Cloudness;
    public float Wind;
    public int WindDirection;
    public float Rain;
    public float ScaterringFogDensity;
    public float Temperature;

    /// <summary>Vestigial (06-fix-01) — sempre false a partir da v1.1.2. Mantido no layout do pacote de
    /// propósito, pra não mudar o formato serializado (evita reintroduzir o antipattern AP-11 de layout
    /// mudando sem renomear o tipo). Antes disparava um HandleReconnect(ESeasonStatus.Storm) que causava
    /// nevasca de inverno incorreta em vez de tempestade de verão — ver 06-fix-01.md do item 001.</summary>
    public bool ThunderEventTrigger;

    /// <summary>NÃO serializado. Falso quando o corpo veio truncado — não processar nem retransmitir.</summary>
    internal bool Valid;

    [ThreadStatic] private static NetDataWriter? _inner;

    public void Serialize(NetDataWriter writer)
    {
        var inner = _inner ??= new NetDataWriter(true, 64);
        inner.Reset();

        inner.Put(TargetTime);
        inner.Put(Cloudness);
        inner.Put(Wind);
        inner.Put(WindDirection);
        inner.Put(Rain);
        inner.Put(ScaterringFogDensity);
        inner.Put(Temperature);
        inner.Put(ThunderEventTrigger);

        // overload de 3 args — o de 1 arg escreve o buffer inteiro com padding (docs/technical/fika-packet-desync-prevention-plan.md §5.1).
        writer.PutBytesWithLength(inner.Data, 0, checked((ushort)inner.Length));
    }

    public void Deserialize(NetDataReader reader)
    {
        // reset total: a instância é reutilizada entre recepções (NetPacketProcessor.cs:387-397).
        TargetTime = 0;
        Cloudness = 0f;
        Wind = 0f;
        WindDirection = 0;
        Rain = 0f;
        ScaterringFogDensity = 0f;
        Temperature = 0f;
        ThunderEventTrigger = false;
        Valid = false;

        if (!reader.TryGetBytesWithLength(out var payload) || payload == null) return;

        var inner = new NetDataReader(payload);

        if (!inner.TryGetLong(out TargetTime)) return;
        if (!inner.TryGetFloat(out Cloudness)) return;
        if (!inner.TryGetFloat(out Wind)) return;
        if (!inner.TryGetInt(out WindDirection)) return;
        if (!inner.TryGetFloat(out Rain)) return;
        if (!inner.TryGetFloat(out ScaterringFogDensity)) return;
        if (!inner.TryGetFloat(out Temperature)) return;
        if (!inner.TryGetBool(out ThunderEventTrigger)) return;

        Valid = true;
    }
}
