# Changelog

## 1.3.0 — 2026-06-20

The big one. Four new optimizations, two removals, the FIKA crash fix, and a pass to
make sure every feature actually does what it says — verified with live counters, not
just compiled-and-shipped.

**New**
- **S8 CalmRagfairServer** — vanilla forces a blocking, compacting full GC every time
  enough flea offers expire, a recurring multi-hundred-ms-to-multi-second stall. This
  runs the exact same expiry sequence minus the forced collect. In a measured session
  it skipped 54 forced GCs and the worst frame-pause dropped from ~8s (mod off) to ~1.7s.
- **S9 FastCompressionHttpListener** — responses compressed at zlib `Fastest` instead of
  vanilla's slowest `SmallestSize`, several-fold less CPU for a few percent more bytes
  that only cross localhost/LAN. Output is byte-compatible; the client doesn't care.
- **S10 ThreadSafeCaches** — serializes access to three SPT caches (handbook prices, item
  base classes, item blacklists) that vanilla writes at runtime while parallel ragfair
  tasks read them. Pure correctness; prevents an intermittent concurrent-collection crash.
- **S7 FastHttpResponseUtil** — replaces five regex passes per response with one
  zero-allocation scan. Identical output.
- **S11 SaveDirtyTracking** — skips the periodic profile save when the session is provably
  clean (vanilla serializes + hashes the full profile every tick even when idle).
- **S12 IsolatedBotRandomisation** — fixes a vanilla bug where night-raid equipment
  modifiers were written into shared bot config, compounding per bot and persisting across
  raids until restart.
- **S13 CalmNotifier** — websocket sends no longer hold the global socket lock during
  network I/O, payloads serialize once per message, and the `/notify` long-poll releases
  its thread between checks instead of pinning one per connected client. Most valuable for
  FIKA hosts.

**Removed**
- **S3 LogLevelFilter and C4 HotPathLogSuppressor** — the log-filtering pair is gone. On
  default setups the filters dropped close to nothing, while suppressing other mods'
  in-raid logs made everyone else's bug reports worse and kept a Harmony patch on SPT's
  central logging queue. Community feedback was right. **The mod is now server-only — no
  BepInEx plugin ships, which also resolves the client/server GUID-mismatch warning some
  saw in check-mods.** (Upgrading from 1.2.x? Delete the old
  `BepInEx/plugins/CompoundingPerf.Client` folder.)
- A route-dispatch memoization tried during development was pulled before release: it
  changed a launcher response under FIKA in a way that wasn't fully explained, and a perf
  feature that can't prove it's behavior-neutral doesn't ship.

**Fixed**
- **FIKA 2.3.x API endpoints crashing** (`/fika/api/players`: `no profiles found in
  saveServer` — thanks to Fiodor for the report): DI overrides registered without an
  explicit `InjectionType` were resolved as Scoped instead of Singleton, so FIKA's API
  controllers got a fresh empty `SaveServer` per request. Every override now declares the
  exact lifetime of the class it replaces, and the mod warns at boot if another mod
  displaces one of its overrides.
- **S11 was throttling itself to nothing** — its force-save interval defaulted to 60s,
  equal to SPT's save tick, so a clean session was always "due" and never actually
  skipped. Raised to 300s so it skips ~4 of every 5 idle saves.

**Internals** — every server feature uses SPT's DI `TypeOverride` so other mods' Harmony
patches keep working; three small Harmony patches (S6, S7, S9) backstop the cases where
the C# compiler dispatches an internal call non-virtually. All overrides match their
vanilla DI lifetime. FIKA 2.3.x compatible.

## 1.2.1 — 2026-06-09

Fixes from an automated multi-lens code audit of our own source:

- **Fixed**: `CachingHttpRouter`'s cache used a case-sensitive comparer while its path whitelist was case-insensitive — variant-case requests could double-cache the same payload.
- **Fixed**: `LogSuppressorPatch` initialized its allowlist lazily from a Harmony Prefix with no memory barrier (double-checked-locking race). Under heavy multi-threaded logging this could observe a half-published state and throw. Initialization is now eager, from `Plugin.Awake()`, before any patch is applied.
- **Removed**: the three disabled experimental features (RaidLootPrewarmer, ShaderWarmup, PostRaidGC) are gone from the codebase rather than shipping as off-by-default toggles. Their history below stands; if a future version brings one back, it will be as a redesigned implementation that meets the bounded-cost bar.

## 1.2.0 — 2026-06-01

- **Added — S6 ThreadSafeRandomUtil**: DI override of SPT's `RandomUtil`. Four virtual methods that still touch the shared non-thread-safe `System.Random` (`GetDouble`, `GetBool`, `RandInt`, `RandNum`) are now lock-wrapped; the private `GetSecureRandomNumber` is Harmony-patched to `Random.Shared`. Pure correctness fix; unblocks future concurrency work (S5 re-enable, parallel bot generation).
- S5 RaidLootPrewarmer remains disabled pending re-validation on top of S6.

## 1.1.0 — 2026-05-26

- **Added — S2 ResponseCache**: DI override of `HttpRouter` caching fifteen static-after-load endpoints. First request per path runs the normal chain (other mods' modifications are captured); repeats serve from memory.
- **Added, then disabled — S5 RaidLootPrewarmer**: background pre-generation of next raid's loot. Crashed in play: the background generation raced on SPT's non-thread-safe `RandomUtil`. Disabled by default; root cause later fixed by S6.
- **Added, then disabled — C5 ShaderWarmup**: `Shader.WarmupAllShaders` during first raid load took 85 seconds on a ~50-plugin install. Disabled by default.
- Build: `-p:SkipDeploy=true` escape hatch for building while SPT is running.
- Fixed SAIN detection GUID (`me.sol.sain`).

## 1.0.0 — 2026-04-28

Initial release.

- **S1 ProfileSaveDebouncer**: trailing-edge save coalescer. Originally implemented via `HarmonyReversePatch` on the async `SaveProfileAsync` — which failed silently and broke saves (async kickoff IL can't be safely copied cross-assembly). Reimplemented the same day as a DI `TypeOverride` subclass of `SaveServer` using plain virtual dispatch; verified across multiple raid sessions since.
- **S3 LogLevelFilter**: Harmony Prefix on `SptLoggerQueueManager.EnqueueMessage` dropping blocklisted-namespace lines below a configurable level.
- **C4 HotPathLogSuppressor** + **RaidStateTracker** (client): in-raid BepInEx log suppression with a plugin allowlist.
- **C2 PostRaidGC** (client): post-raid forced GC. Caused a ~974 ms hitch on a 2 GB heap even with the gentlest collector flags; disabled by default.
