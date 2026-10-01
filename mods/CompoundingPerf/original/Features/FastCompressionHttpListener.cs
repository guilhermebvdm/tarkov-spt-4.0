using System.IO.Compression;
using System.Text;
using HarmonyLib;
using Microsoft.AspNetCore.Http;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Servers.Http;
using SPTarkov.Server.Core.Utils;
using CompoundingPerf.Telemetry;

namespace CompoundingPerf.Features;

/// <summary>
/// S9 — Subclass of <see cref="SptHttpListener"/> registered via SPT.DI's
/// <see cref="Injectable.TypeOverride"/>. Vanilla compresses every JSON response with
/// <c>CompressionLevel.SmallestSize</c> — zlib's slowest setting — which is a poor
/// trade for SPT's localhost (or LAN, under FIKA) connections: for multi-megabyte
/// payloads like the item DB, SmallestSize costs several times the CPU of Fastest to
/// save single-digit percent of bytes that never cross a real network.
///
/// <para>This override reproduces vanilla's <c>SendZlibJson</c> exactly, with the
/// compression level taken from config (default <c>Fastest</c>). Everything else —
/// status code, content type, session cookie, zlib framing — is unchanged; the EFT
/// client's inflater doesn't care what level the deflater used.</para>
/// </summary>
[Injectable(TypeOverride = typeof(SptHttpListener), TypePriority = 100)]
public class FastCompressionHttpListener(
    HttpRouter                      httpRouter,
    IEnumerable<ISerializer>        serializers,
    ISptLogger<SptHttpListener>     logger,
    ISptLogger<RequestLogger>       requestsLogger,
    JsonUtil                        jsonUtil,
    HttpResponseUtil                httpResponseUtil)
    : SptHttpListener(httpRouter, serializers, logger, requestsLogger, jsonUtil, httpResponseUtil)
{
    /// <summary>Kill-switch. While false, defers to vanilla (SmallestSize).</summary>
    public static volatile bool IsEnabled;

    /// <summary>Level applied when enabled. Set from config at OnLoad.</summary>
    public static volatile CompressionLevel Level = CompressionLevel.Fastest;

    public override async Task SendZlibJson(HttpResponse resp, string output, MongoId sessionID)
    {
        if (!IsEnabled)
        {
            await base.SendZlibJson(resp, output, sessionID);
            return;
        }

        TelemetryHub.Increment("s9.compression.responses");

        // Mirror of vanilla SendZlibJson, with the configurable level.
        resp.StatusCode = 200;
        resp.ContentType = "application/json";
        resp.Headers.Append("Set-Cookie", $"PHPSESSID={sessionID.ToString()}");

        await using var deflateStream = new ZLibStream(resp.Body, Level);
        await deflateStream.WriteAsync(Encoding.UTF8.GetBytes(output));
    }

    public static CompressionLevel ParseLevel(string? s) => s?.ToLowerInvariant() switch
    {
        "optimal"       => CompressionLevel.Optimal,
        "nocompression" => CompressionLevel.NoCompression,
        "smallestsize"  => CompressionLevel.SmallestSize,
        _               => CompressionLevel.Fastest,
    };
}

/// <summary>
/// The load-bearing half of S9. <c>SendZlibJson</c>'s ONLY caller is
/// <c>SptHttpListener.SendResponse</c>, and that call site is a non-virtual
/// <c>call</c> in the shipped IL — the DI override above never runs through it.
/// A Harmony prefix detours the method body itself, replacing the implementation
/// (vanilla's exact sequence, configurable compression level) for every call site.
/// Skipping an async-kickoff original by supplying <c>__result</c> is the standard
/// Harmony pattern — distinct from the ReversePatch-on-async approach that caused
/// the V1.0 save incident.
/// </summary>
public static class FastCompressionPatch
{
    public static void Apply(Harmony harmony, ISptLogger<CompoundingPerfMod> logger)
    {
        var target = AccessTools.Method(typeof(SptHttpListener), nameof(SptHttpListener.SendZlibJson));
        if (target is null)
        {
            logger.Warning("[CompoundingPerf/S9] SptHttpListener.SendZlibJson not found — compression patch NOT applied");
            return;
        }
        harmony.Patch(target, prefix: new HarmonyMethod(typeof(FastCompressionPatch), nameof(Prefix)));
    }

    public static bool Prefix(HttpResponse resp, string output, MongoId sessionID, ref Task __result)
    {
        if (!FastCompressionHttpListener.IsEnabled)
        {
            return true;
        }

        __result = SendFast(resp, output, sessionID);
        return false;
    }

    private static async Task SendFast(HttpResponse resp, string output, MongoId sessionID)
    {
        TelemetryHub.Increment("s9.compression.responses");

        // Vanilla SendZlibJson, line for line, except the compression level.
        resp.StatusCode = 200;
        resp.ContentType = "application/json";
        resp.Headers.Append("Set-Cookie", $"PHPSESSID={sessionID.ToString()}");

        await using var deflateStream = new ZLibStream(resp.Body, FastCompressionHttpListener.Level);
        await deflateStream.WriteAsync(Encoding.UTF8.GetBytes(output));
    }
}
