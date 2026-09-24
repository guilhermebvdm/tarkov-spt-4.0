# CompoundingPerf

A performance mod for [SPT](https://www.sp-tarkov.com/) 4.0.13 that stacks several small, independent server-side optimizations under one config. Each feature is individually toggleable, designed to coexist with other mods, and held to one rule: **a feature ships only if its cost is bounded by construction** — nothing that could blow up on a heavily-modded install.

Since 1.3 the mod is **server-only**: it ships no BepInEx plugin and never touches your game client.

## What it does

| Feature | Default | What you get |
|---|---|---|
| **ProfileSaveDebouncer** (S1) | on | Coalesces rapid-fire profile saves. At most one save in flight plus one trailing per profile; the trailing save always captures the latest state, so nothing is ever lost — duplicate work just gets merged. |
| **ResponseCache** (S2) | on | Caches fifteen static-after-load HTTP endpoints (item DB, handbook, hideout recipes, globals…). First request runs the full router chain; repeats serve from memory. |
| **ThreadSafeRandomUtil** (S6) | on | Fixes a latent thread-safety hazard in SPT's `RandomUtil` (shared `System.Random` accessed without synchronization). Pure correctness — no behavior change, prevents RNG-state corruption under concurrent load. |
| **ResponseSanitizer** (S7) | on | Replaces the five-regex-passes-per-HTTP-response `ClearString` with a single scan. Identical output, zero allocations on the common path. |
| **RagfairCalmUpdates** (S8) | on | Vanilla forces a blocking, compacting full GC every time enough flea offers expire — a recurring stall. This reproduces the expiry sequence exactly, minus the forced collect. |
| **FastCompression** (S9) | on | Vanilla zlib-compresses every response at `SmallestSize` (slowest). `Fastest` is several times cheaper in CPU for a few percent larger payloads that only cross localhost/LAN. |
| **ThreadSafeCaches** (S10) | on | Serializes access to three SPT caches whose plain collections are written at runtime while other threads read them (handbook prices, item base classes, item blacklists). Pure correctness fix. |
| **SaveDirtyTracking** (S11) | on | Skips the periodic profile save entirely when the session is provably clean — vanilla serializes and hashes the full profile every tick even when idle. Any non-pure request marks the session dirty, so player-driven changes can never be skipped. |
| **IsolatedBotRandomisation** (S12) | on | Fixes a vanilla bug: night-raid equipment modifiers are written into shared bot config — they compound per generated bot, persist across raids until restart, and race across parallel bot generation. Every bot now gets a private copy. |
| **CalmNotifier** (S13) | on | Websocket sends stop holding the global socket lock during network writes, payloads serialize once per message, and the `/notify` long-poll releases its thread between checks. Most valuable for FIKA hosts. |

Earlier features that didn't survive honest testing were removed rather than shipped disabled: background loot pre-generation, shader pre-warming, post-raid GC — and, in 1.3, the log-filtering pair (S3/C4: dropped close to nothing on default setups, and suppressing other mods' in-raid logs makes everyone else's bug reports worse — community feedback was right) plus a route-dispatch memoization (S14) that changed a launcher response under FIKA in a way we couldn't fully explain. A perf feature that can't prove it's behavior-neutral doesn't ship.

## How it works

Server features avoid Harmony where possible in favor of SPT 4.0's DI `TypeOverride` mechanism — subclasses like `CoalescingSaveServer : SaveServer` and `CachingHttpRouter : HttpRouter` replace the built-ins at container resolution, which means normal C# virtual dispatch, no IL surgery, and other mods' Harmony patches on those classes keep working (our subclass *is* the target they patch). Every override declares the same DI lifetime as the vanilla class it replaces. Three small Harmony patches remain where a DI override alone isn't enough: S6 prefixes `RandomUtil`'s private RNG helper, and S7/S9 use body detours to backstop their overrides at internal call sites the C# compiler dispatches non-virtually (so the optimization applies even when SPT calls the method from inside its own class).

Compatibility is explicit: per-session endpoints (profile, quests, flea search) are never cached, and the mod logs a warning at boot if another mod displaced one of its overrides instead of silently degrading.

## Compatibility

- Works on unmodded SPT 4.0.13.
- Works with FIKA 2.3.x (including the 2.3 web API endpoints).
- Designed to coexist with other mods — DI overrides instead of method replacement, per-session endpoints never cached.

## Install

Drop the release archive onto your SPT folder, or manually:

- `CompoundingPerf.dll` + `config.json` → `SPT/user/mods/CompoundingPerf/`

Every feature has an `Enabled` flag — flip any of them without touching the rest. `MasterEnabled: false` disables every optimization at once for A/B comparisons.

## Building from source

Server targets `net9.0` against the `SPTarkov.Server.Core` 4.0.13 NuGet packages; the client project exists only for dev benchmark builds (`-p:Bench=true` compiles an in-raid frame-stats recorder; release builds contain nothing and are not shipped). Set `-p:SptRoot=<path>` if SPT isn't at `C:\SPT`; pass `-p:SkipDeploy=true` to build without deploying to a live install.

```
dotnet build CompoundingPerf.csproj -c Release
dotnet test tests/CompoundingPerf.Tests.csproj -c Release
```

The unit-test suite covers the save-coalescer state machine (including its trailing-edge no-state-loss guarantee), the dirty-tracking save-skip rules, the cache whitelist, the response sanitizer against vanilla's regex behavior, and the thread-safety of the RandomUtil override under 16-thread hammering.

## License

MIT — see [LICENSE](LICENSE).
