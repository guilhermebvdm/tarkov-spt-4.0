# Benchmarking protocol (dev builds)

Everything here is dev-only: release packages contain **no** recorder, sampler, or
per-frame code. Bench instrumentation compiles in with `-p:Bench=true` on both projects.

## Build + deploy a bench pair

```
dotnet build CompoundingPerf.csproj -c Release -p:SkipDeploy=true -p:Bench=true
dotnet build client/CompoundingPerf.Client.csproj -c Release -p:SkipDeploy=true -p:Bench=true
# copy server DLL  -> <install>/SPT/user/mods/CompoundingPerf/
# copy client DLL  -> <install>/BepInEx/plugins/CompoundingPerf.Client/
```

Primary test environment is the vanilla+FIKA sandbox (`C:\SPT-vanilla-sandbox`), never
the modded daily install.

## What gets recorded

- **Client** (`user/logs/CompoundingPerf-framestats.jsonl`) — one line per raid:
  map, peak/total population, frames, avg FPS, 1% / 0.1% lows, hitch counts (>50ms,
  >100ms), worst frame + when it happened (`maxFrameAtSec` near the end of the raid =
  teardown artifact, mid-raid = real stall). First `WarmupSkipSeconds` (default 20s)
  are trimmed — loading frames say nothing about gameplay.
- **Server** (`user/logs/CompoundingPerf-serverstats.jsonl`) — one line per 30s:
  GC pause ms/% (the stalls S8/S11 target), gen0/1/2 counts, heap MB, and feature
  counters (saves executed vs skipped, cache hits, dispatch hit rate).

Both tag every line with `masterEnabled`, so a single config flip separates the sides.

## Run protocol

1. Pick ONE map per comparison (Factory = small-map control; Customs/Woods = where
   the server-side features actually get exercised). Same time-of-day setting.
2. **Side A (mod on)**: `MasterEnabled: true` → start server → start launcher → 2-3
   raids of similar length/playstyle. Avoid alt-tabbing during raids.
3. **Side B (baseline)**: close game, stop server, flip `MasterEnabled: false`,
   restart BOTH (the client reads masterEnabled at launch for row tagging) → mirror
   the same raids.
4. Report: `python scripts/bench-report.py [logsDir]` — groups by map and side,
   excludes non-comparable rows (no warmup trim / older modVersion) and says so.

## Rules learned the hard way

- **Never change the build between sides.** A new feature or fix invalidates the
  open data set — archive the JSONLs (`*-archive-<date>.jsonl`) and start over.
- Boot-only smoke tests exercise neither saves nor MVC request scopes. A bench run
  needs a loaded profile; FIKA API health needs a `curl` against
  `/fika/api/players` with the Bearer key from fika.jsonc.
- 2-3 raids per side is directional, not proof — claim accordingly. Expected shape:
  avg FPS ~flat (the mod is server-side), differences live in 1% lows, hitch
  rates, and server GC pause totals.
- Worst-frame spikes at the very end of a raid (extract/teardown) are not stalls;
  `maxFrameAtSec` exists precisely to tell them apart.
