using HarmonyLib;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using CompoundingPerf.Telemetry;

namespace CompoundingPerf.Features;

/// <summary>
/// S7 — Subclass of <see cref="HttpResponseUtil"/> registered via SPT.DI's
/// <see cref="Injectable.TypeOverride"/>. Replaces <c>ClearString</c> — which vanilla
/// implements as five sequential <c>Regex.Replace</c> passes over every HTTP response
/// body — with a single scan.
///
/// <para>The five regexes each remove one control character (backspace, form-feed,
/// newline, carriage-return, tab). Because <c>System.Text.Json</c> escapes control
/// characters inside string values (a newline in data becomes the two characters
/// <c>\n</c>, not a raw 0x0A), serialized responses almost never contain raw control
/// characters at all — so the overwhelmingly common case is "nothing to remove".
/// Vanilla still pays five full regex traversals (plus five intermediate string
/// allocations when anything matches) to discover that.</para>
///
/// <para>Our version scans once; if the response is clean (the ~always case) it returns
/// the original string with zero allocations. When control characters are present the
/// behavior is byte-for-byte identical to vanilla: exactly those five characters are
/// removed, everything else passes through.</para>
/// </summary>
[Injectable(TypeOverride = typeof(HttpResponseUtil), TypePriority = 100)]
public class FastHttpResponseUtil(JsonUtil jsonUtil, ServerLocalisationService serverLocalisationService)
    : HttpResponseUtil(jsonUtil, serverLocalisationService)
{
    /// <summary>Kill-switch. False until OnLoad reads the config; while false we defer
    /// to the vanilla regex implementation.</summary>
    public static volatile bool IsEnabled;

    protected override string ClearString(string? s)
    {
        if (!IsEnabled)
        {
            return base.ClearString(s);
        }

        TelemetryHub.Increment("s7.sanitizer.calls");
        return FastSanitize(s);
    }

    /// <summary>
    /// Pure, testable core: removes exactly the characters vanilla's regex list removes
    /// (\b \f \n \r \t). Returns the ORIGINAL string instance when nothing needs removing.
    /// </summary>
    public static string FastSanitize(string? s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return "";
        }

        // Fast path: count offending chars. Serialized JSON rarely has any.
        var removed = 0;
        for (var i = 0; i < s.Length; i++)
        {
            if (IsStripped(s[i]))
            {
                removed++;
            }
        }

        if (removed == 0)
        {
            return s; // zero allocations
        }

        // Slow path: single exact-size allocation, copy everything except stripped chars.
        return string.Create(s.Length - removed, s, static (dest, src) =>
        {
            var w = 0;
            foreach (var c in src)
            {
                if (!IsStripped(c))
                {
                    dest[w++] = c;
                }
            }
        });
    }

    private static bool IsStripped(char c) =>
        c is '\b' or '\f' or '\n' or '\r' or '\t';
}

/// <summary>
/// The load-bearing half of S7. The DI override above only intercepts EXTERNAL virtual
/// call sites — but every real caller of <c>ClearString</c> (<c>GetBody</c>,
/// <c>NoBody</c>, <c>NullResponse</c>) is INSIDE HttpResponseUtil, and Roslyn emitted
/// those as non-virtual <c>call</c> instructions (the method wasn't virtual in source;
/// SPT's virtualizer flips the flag but does not rewrite call sites). A Harmony prefix
/// detours the method body itself, so every call site lands here regardless.
/// </summary>
public static class ResponseSanitizerPatch
{
    public static void Apply(Harmony harmony, ISptLogger<CompoundingPerfMod> logger)
    {
        var target = AccessTools.Method(typeof(HttpResponseUtil), "ClearString");
        if (target is null)
        {
            logger.Warning("[CompoundingPerf/S7] HttpResponseUtil.ClearString not found — sanitizer patch NOT applied");
            return;
        }
        harmony.Patch(target, prefix: new HarmonyMethod(typeof(ResponseSanitizerPatch), nameof(Prefix)));
    }

    public static bool Prefix(string? s, ref string __result)
    {
        if (!FastHttpResponseUtil.IsEnabled)
        {
            return true;
        }

        TelemetryHub.Increment("s7.sanitizer.calls");
        __result = FastHttpResponseUtil.FastSanitize(s);
        return false;
    }
}
