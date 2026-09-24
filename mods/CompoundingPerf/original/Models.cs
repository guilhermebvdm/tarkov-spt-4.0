// Compile-included by both the server project (net9.0) and the client project (net471).
// Avoid `required` keyword for net471 compatibility — use defaults instead.

namespace CompoundingPerf;

public record CompoundingPerfConfig
{
    /// <summary>One-switch A/B toggle: when false, EVERY optimization is disabled (server
    /// and client) regardless of individual flags — but the frame-stats recorder keeps
    /// running, so with/without benchmark runs are a single config flip apart.</summary>
    public bool MasterEnabled { get; set; } = true;

    public ServerToggles Server { get; set; } = new();
    public ClientToggles Client { get; set; } = new();
    public TelemetryOptions Telemetry { get; set; } = new();
    public CompatOptions Compat { get; set; } = new();
}

public record ServerToggles
{
    public ProfileSaveDebouncerOptions ProfileSaveDebouncer { get; set; } = new();
    public ResponseCacheOptions        ResponseCache        { get; set; } = new();
    public ThreadSafeRandomOptions     ThreadSafeRandom     { get; set; } = new();
    public ResponseSanitizerOptions    ResponseSanitizer    { get; set; } = new();
    public RagfairCalmUpdatesOptions   RagfairCalmUpdates   { get; set; } = new();
    public FastCompressionOptions      FastCompression      { get; set; } = new();
    public ThreadSafeCachesOptions          ThreadSafeCaches        { get; set; } = new();
    public SaveDirtyTrackingOptions         SaveDirtyTracking       { get; set; } = new();
    public IsolatedBotRandomisationOptions  IsolatedBotRandomisation { get; set; } = new();
    public CalmNotifierOptions              CalmNotifier             { get; set; } = new();
    // S14 (FastRouteDispatch) was REMOVED before release: memoizing url→router
    // resolution changed the /launcher/server/connect response under FIKA (raw
    // 0.0.0.0 backendUrl → game cannot connect). Root cause not fully explained,
    // which by itself disqualifies a behavior-neutrality-critical feature.
}

public record CalmNotifierOptions
{
    // S13: (a) websocket sends no longer hold the global socket lock during network
    // I/O, payloads serialize once per message instead of once per socket, and the
    // socket snapshot is taken safely under the lock (vanilla returns a lazy iterator
    // that races); (b) the /notify long-poll releases its thread between checks
    // instead of pinning one thread-pool thread per connected client.
    public bool Enabled { get; set; } = true;
}

public record IsolatedBotRandomisationOptions
{
    // S12: fixes a verified vanilla bug — night-raid equipment modifiers are written
    // into SHARED bot config: they compound per generated bot, persist across raids
    // until restart, and race across vanilla's parallel bot generation. The fix gives
    // every caller a private clone, so the modifier applies exactly once per bot.
    public bool Enabled { get; set; } = true;
}

public record ThreadSafeCachesOptions
{
    // S10: serializes access to three SPT caches whose plain collections are written
    // at runtime while other threads read them (HandbookHelper price cache,
    // ItemBaseClassService base-class cache, ItemFilterService blacklists). Same
    // hazard family S6 fixed in RandomUtil; no behavior change.
    public bool Enabled { get; set; } = true;
}

public record SaveDirtyTrackingOptions
{
    // S11: skips the periodic profile save entirely when the session is provably clean.
    // Vanilla serializes + MD5-hashes the FULL profile every tick just to discover
    // nothing changed; with this on, an idle session costs nothing. Any request that
    // isn't in a small known-pure whitelist marks the session dirty, so player-driven
    // changes can never be skipped.
    public bool Enabled { get; set; } = true;

    /// <summary>A clean session still gets a real save this often, to persist
    /// server-internal changes that bypass HTTP (hideout production progress).
    /// This is the worst-case persistence window for purely passive changes.</summary>
    public int ForceSaveIntervalSeconds { get; set; } = 300;
}

public record ResponseSanitizerOptions
{
    // S7: replaces vanilla's five-regex-passes-per-response ClearString with a single
    // scan that returns the original string allocation-free when (as is almost always
    // the case for serialized JSON) there are no raw control characters to strip.
    // Output is identical to vanilla for every input.
    public bool Enabled { get; set; } = true;
}

public record RagfairCalmUpdatesOptions
{
    // S8: vanilla's flea-offer expiry pass ends with a forced, blocking, compacting
    // full GC — a recurring multi-hundred-ms stall on large heaps. This feature
    // reproduces the expiry sequence exactly and omits only the forced collect;
    // the runtime's server GC reclaims the memory on its own schedule.
    public bool Enabled { get; set; } = true;
}

public record FastCompressionOptions
{
    // S9: vanilla compresses every JSON response at CompressionLevel.SmallestSize —
    // zlib's slowest setting — over what is almost always a localhost connection.
    // Fastest cuts response-compression CPU several-fold for a few percent more bytes.
    public bool Enabled { get; set; } = true;

    /// <summary>One of: Fastest, Optimal, SmallestSize, NoCompression. Unrecognized
    /// values fall back to Fastest.</summary>
    public string Level { get; set; } = "Fastest";
}

public record ClientToggles
{
    // The in-raid log suppressor (C4) was removed in 1.3: its benefit never survived
    // measurement on real setups, and suppressing other mods' in-raid logs breaks the
    // ecosystem's debugging currency. FrameStats is BENCH-dev-build-only.
    public FrameStatsOptions FrameStats { get; set; } = new();
}

public record FrameStatsOptions
{
    // C6: per-raid frame-time benchmark recorder. One float write per frame while in
    // raid (no allocation, no measurable cost); on raid end, appends a stats line
    // (avg FPS, 1%/0.1% lows, hitch counts, worst spike) to
    // SPT/user/logs/CompoundingPerf-framestats.jsonl. Deliberately NOT gated by
    // MasterEnabled so with/without comparisons measure both sides.
    public bool Enabled { get; set; } = true;

    /// <summary>Leading seconds of each raid discarded from the stats — the spawn-in /
    /// asset-streaming window produces multi-second frames that say nothing about
    /// gameplay performance and would dominate the lows.</summary>
    public double WarmupSkipSeconds { get; set; } = 20;
}

public record ProfileSaveDebouncerOptions
{
    // V1.0: enabled by default — implemented via SPT.DI TypeOverride of SaveServer
    // (see CoalescingSaveServer). Trailing-edge semantics preserve durability:
    // at most one save in flight + one trailing per profile, the trailing save
    // captures the latest in-memory state, no mutations are dropped.
    public bool Enabled { get; set; } = true;
}

public record ResponseCacheOptions
{
    // V1.1: enabled by default — caches a conservative whitelist of static-after-load
    // endpoints (item DB, handbook, hideout recipes, globals, etc.). First request per
    // path runs through the normal router chain (so other mods' modifications are
    // captured); subsequent requests return the cached JSON directly.
    public bool Enabled { get; set; } = true;

    /// <summary>Extra paths to cache beyond the built-in conservative whitelist.
    /// Use only for paths whose responses don't change at runtime.</summary>
    public List<string> AdditionalPaths { get; set; } = new();
}

public record ThreadSafeRandomOptions
{
    // S6: replace the unsafe instance Random in SPT's RandomUtil with a lock-protected
    // path (for the four virtual methods that touch it: GetDouble, GetBool, RandInt,
    // RandNum) and a Harmony-patched Random.Shared path (for the non-virtual
    // GetSecureRandomNumber). Pure correctness fix — no behavior change, just lets
    // concurrent callers stop corrupting RNG state.
    public bool Enabled { get; set; } = true;
}

public record TelemetryOptions
{
    public bool Enabled { get; set; } = false;
    public int DumpEveryNRaids { get; set; } = 10;
    public bool DumpOnRaidEnd { get; set; } = true;
    public bool DumpOnServerShutdown { get; set; } = true;

    /// <summary>Stopwatch timing on hot paths is opt-in because the measurement itself
    /// has overhead. Counters are always cheap.</summary>
    public bool TimingEnabled { get; set; } = false;
}

public record CompatOptions
{
    public bool AutoDisableOnConflict { get; set; } = true;
    public bool Verbose { get; set; } = true;
}
