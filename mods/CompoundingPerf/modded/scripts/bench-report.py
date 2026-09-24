#!/usr/bin/env python3
"""A/B benchmark report for CompoundingPerf dev (BENCH) builds.

Reads the two JSONL files the BENCH builds write:
  - CompoundingPerf-framestats.jsonl   (client: one line per raid)
  - CompoundingPerf-serverstats.jsonl  (server: one line per 30s interval)

Groups client rows by (map, masterEnabled) and server rows by masterEnabled, prints
side-by-side means. Rows are excluded (and listed) when they aren't comparable:
missing warmup trim, or a modVersion different from the newest seen.

Usage:
  python bench-report.py [logsDir]
  # default logsDir: C:/SPT-vanilla-sandbox/SPT/user/logs
"""

import json
import sys
from collections import defaultdict
from pathlib import Path

LOGS = Path(sys.argv[1] if len(sys.argv) > 1 else r"C:/SPT-vanilla-sandbox/SPT/user/logs")


def read_jsonl(name):
    p = LOGS / name
    if not p.exists():
        return []
    rows = []
    for line in p.read_text(encoding="utf-8").splitlines():
        line = line.strip()
        if line:
            try:
                rows.append(json.loads(line))
            except json.JSONDecodeError:
                print(f"  ! unparseable line skipped in {name}")
    return rows


def mean(xs):
    xs = list(xs)
    return sum(xs) / len(xs) if xs else 0.0


def fmt_side(rows):
    if not rows:
        return "  (no raids)"
    per_min = lambda r, k: r[k] / (r["durationSec"] / 60.0)
    return (
        f"  raids={len(rows)}  "
        f"avgFps={mean(r['avgFps'] for r in rows):.1f}  "
        f"1%low={mean(r['onePercentLowFps'] for r in rows):.1f}  "
        f"0.1%low={mean(r['pointOnePercentLowFps'] for r in rows):.1f}  "
        f"hitches>50ms/min={mean(per_min(r, 'hitchesOver50Ms') for r in rows):.1f}  "
        f">100ms/min={mean(per_min(r, 'hitchesOver100Ms') for r in rows):.1f}  "
        f"worst={max(r['maxFrameMs'] for r in rows):.0f}ms"
    )


def main():
    client = read_jsonl("CompoundingPerf-framestats.jsonl")
    server = read_jsonl("CompoundingPerf-serverstats.jsonl")

    # --- client: filter to comparable rows ---
    newest = max((r.get("modVersion", "") for r in client), default="")
    excluded = [r for r in client if "warmupSkippedSec" not in r or r.get("modVersion") != newest]
    usable = [r for r in client if r not in excluded]

    print(f"=== CompoundingPerf A/B report  (logs: {LOGS})")
    if excluded:
        print(f"--- excluded {len(excluded)} non-comparable raid(s): "
              + ", ".join(f"{r['ts'][:16]} ({'no-trim' if 'warmupSkippedSec' not in r else 'v' + r.get('modVersion', '?')})" for r in excluded))

    by_map = defaultdict(lambda: {"on": [], "off": []})
    for r in usable:
        by_map[r.get("map", "unknown")]["on" if r["masterEnabled"] else "off"].append(r)

    print("\n--- CLIENT (per-raid frame stats, warmup-trimmed) ---")
    for m, sides in sorted(by_map.items()):
        print(f"\n[{m}]")
        print(f"  MOD ON :{fmt_side(sides['on'])}")
        print(f"  MOD OFF:{fmt_side(sides['off'])}")
        pop = [f"{r.get('peakAlivePlayers', '?')}/{r.get('uniquePlayersSeen', '?')}"
               for r in sides["on"] + sides["off"] if "peakAlivePlayers" in r]
        if pop:
            print(f"  population (peak/total per raid): {', '.join(pop)}")

    # --- server: split intervals by side, ignore idle intervals (no requests at all) ---
    print("\n--- SERVER (30s GC/counter intervals) ---")
    for side, label in ((True, "MOD ON "), (False, "MOD OFF")):
        rows = [r for r in server if r["masterEnabled"] == side]
        if not rows:
            print(f"  {label}: (no samples)")
            continue
        print(
            f"  {label}: n={len(rows)}  "
            f"gcPause={mean(r['gcPauseMs'] for r in rows):.0f}ms/interval ({mean(r['gcPausePct'] for r in rows):.2f}%)  "
            f"gen2/interval={mean(r['gen2'] for r in rows):.2f}  "
            f"heap={mean(r['heapMb'] for r in rows):.0f}MB  "
            f"savesExec={sum(r['savesExecuted'] for r in rows)}  "
            f"savesSkipped={sum(r['savesSkippedClean'] for r in rows)}  "
            f"cacheHits={sum(r['cacheHits'] for r in rows)}  "
            f"sanitized={sum(r.get('sanitizerCalls', 0) for r in rows)}  "
            f"compressed={sum(r.get('compressedResponses', 0) for r in rows)}  "
            f"gcSkipped(S8)={sum(r.get('forcedGcSkipped', 0) for r in rows)}  "
            f"wsSends(S13)={sum(r.get('wsSends', 0) for r in rows)}  "
            f"wsBroadcasts(S13)={sum(r.get('wsBroadcasts', 0) for r in rows)}  "
            f"notifierPolls(S13)={sum(r.get('notifierPolls', 0) for r in rows)}"
        )

    print("\nNote: 2-3 raids per side per map is directional, not proof. Same map, similar")
    print("raid length, server restarted between sides (client too — it reads masterEnabled at launch).")


if __name__ == "__main__":
    main()
