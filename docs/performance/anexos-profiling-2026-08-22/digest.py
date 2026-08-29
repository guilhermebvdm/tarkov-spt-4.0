"""Digest SPT Runtime Profiler captures into comparable summaries.

Captures:
  vanilla  (SPT_2, Mode=ModAttribution): 2026-08-22_203734, 2026-08-22_203938
  modded   (SPT,   Mode=UpdateOnly):     2026-08-22_205500 (== 205433 == 205508), 2026-08-22_205604
"""
import csv, json, os, statistics, math
from collections import defaultdict

OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "digests")
os.makedirs(OUT, exist_ok=True)

CAPTURES = [
    ("vanilla-A", r"D:\SPT_2\BepInEx\profiling\2026-08-22_203734"),
    ("vanilla-B", r"D:\SPT_2\BepInEx\profiling\2026-08-22_203938"),
    ("modded-A", r"D:\SPT\BepInEx\profiling\2026-08-22_205500"),
    ("modded-B", r"D:\SPT\BepInEx\profiling\2026-08-22_205604"),
]

def read_csv(path):
    with open(path, newline="", encoding="utf-8-sig") as f:
        return list(csv.DictReader(f))

def fl(x):
    try:
        return float(x)
    except (ValueError, TypeError):
        return 0.0

def pct(sorted_vals, p):
    if not sorted_vals:
        return 0.0
    k = (len(sorted_vals) - 1) * p / 100.0
    lo, hi = int(math.floor(k)), int(math.ceil(k))
    if lo == hi:
        return sorted_vals[lo]
    return sorted_vals[lo] + (sorted_vals[hi] - sorted_vals[lo]) * (k - lo)

def digest_capture(name, root):
    d = {"name": name, "root": root}

    # --- capture.json header (first bytes only) ---
    with open(os.path.join(root, "capture.json"), encoding="utf-8") as f:
        head = f.read(2500)
    for key in ("mode", "startUtc", "durationNs"):
        idx = head.find(f'"{key}"')
        if idx >= 0:
            seg = head[idx:idx + 120].split(":", 1)[1].split(",")[0].strip().strip('"')
            d[key] = seg
    # diagnostics block
    for key in ("droppedEntries", "droppedFramePackets", "lateFramePackets", "droppedDeepFrames", "stackMismatches"):
        idx = head.find(f'"{key}"')
        if idx >= 0:
            seg = head[idx:idx + 80].split(":", 1)[1].split(",")[0].strip()
            d[key] = seg

    # --- frames.csv ---
    frames = read_csv(os.path.join(root, "frames.csv"))
    fms = sorted(fl(r["FrameMs"]) for r in frames)
    mms = sorted(fl(r["ManagedProfiledMs"]) for r in frames)
    dur = fl(frames[-1]["TimestampSeconds"]) - fl(frames[0]["TimestampSeconds"]) if frames else 0
    d["frames"] = {
        "count": len(frames),
        "durationSeconds": round(dur, 2),
        "avgFps": round(len(frames) / dur, 1) if dur else 0,
        "frameMs": {
            "avg": round(statistics.mean(fms), 3), "p50": round(pct(fms, 50), 3),
            "p90": round(pct(fms, 90), 3), "p95": round(pct(fms, 95), 3),
            "p99": round(pct(fms, 99), 3), "max": round(fms[-1], 3),
        },
        "managedProfiledMs": {
            "avg": round(statistics.mean(mms), 3), "p50": round(pct(mms, 50), 3),
            "p95": round(pct(mms, 95), 3), "p99": round(pct(mms, 99), 3), "max": round(mms[-1], 3),
        },
        "pctFramesOver": {
            ">16.7ms": round(100 * sum(1 for v in fms if v > 16.7) / len(fms), 1),
            ">25ms": round(100 * sum(1 for v in fms if v > 25) / len(fms), 1),
            ">33.3ms": round(100 * sum(1 for v in fms if v > 33.3) / len(fms), 1),
            ">50ms": round(100 * sum(1 for v in fms if v > 50) / len(fms), 1),
            ">100ms": round(100 * sum(1 for v in fms if v > 100) / len(fms), 1),
        },
    }
    # top-self-method histogram: which method tops the frame most often
    hist = defaultdict(lambda: [0, 0.0])
    for r in frames:
        h = hist[r["TopSelfMethod"]]
        h[0] += 1
        h[1] += fl(r["TopSelfMs"])
    d["topSelfMethodHistogram"] = sorted(
        [{"method": k, "framesTopped": v[0], "sumTopSelfMs": round(v[1], 1)} for k, v in hist.items()],
        key=lambda x: -x["framesTopped"])[:25]
    # worst 15 frames by FrameMs with their top method
    worst = sorted(frames, key=lambda r: -fl(r["FrameMs"]))[:15]
    d["worstFramesList"] = [
        {"frame": r["FrameNumber"], "t": round(fl(r["TimestampSeconds"]), 2), "frameMs": fl(r["FrameMs"]),
         "managedMs": fl(r["ManagedProfiledMs"]), "top": r["TopSelfMethod"], "topMs": fl(r["TopSelfMs"])}
        for r in worst]

    nframes = len(frames)

    # --- methods.csv ---
    methods = read_csv(os.path.join(root, "methods.csv"))
    d["methodCount"] = len(methods)
    main = [m for m in methods if m["ThreadId"] == "1"]

    def mrow(m):
        return {
            "asm": m["Assembly"], "type": m["DeclaringType"], "method": m["Method"],
            "plugin": m["PluginName"], "calls": int(fl(m["Calls"])),
            "callsPerFrame": round(fl(m["CallsPerFrame"]), 2),
            "selfTotalMs": round(fl(m["SelfTotalMs"]), 1),
            "selfPerFrameMs": round(fl(m["SelfPerFrameMs"]), 4),
            "inclTotalMs": round(fl(m["InclusiveTotalMs"]), 1),
            "inclPerFrameMs": round(fl(m["InclusivePerFrameMs"]), 4),
            "avgSelfMs": round(fl(m["AverageSelfMs"]), 4),
            "maxInclMs": round(fl(m["MaxInclusiveMs"]), 2),
            "p99InclMs": round(fl(m["P99InclusiveMs"]), 4),
        }

    d["topMethodsBySelf"] = [mrow(m) for m in sorted(main, key=lambda m: -fl(m["SelfTotalMs"]))[:50]]
    d["topMethodsByCalls"] = [mrow(m) for m in sorted(main, key=lambda m: -fl(m["Calls"]))[:40]]
    d["topMethodsByMaxIncl"] = [mrow(m) for m in sorted(main, key=lambda m: -fl(m["MaxInclusiveMs"]))[:25]]

    # aggregate by assembly (main thread)
    asm = defaultdict(lambda: [0.0, 0, 0])
    for m in main:
        a = asm[m["Assembly"]]
        a[0] += fl(m["SelfTotalMs"])
        a[1] += int(fl(m["Calls"]))
        a[2] += 1
    d["byAssembly"] = sorted(
        [{"assembly": k, "selfTotalMs": round(v[0], 1), "selfPerFrameMs": round(v[0] / nframes, 4),
          "calls": v[1], "methods": v[2]} for k, v in asm.items()],
        key=lambda x: -x["selfTotalMs"])[:40]

    # bot-count proxy: per-instance game methods
    proxies = {}
    for m in main:
        key = f'{m["DeclaringType"]}.{m["Method"]}'
        if key in ("Player.LateUpdate", "Player.UpdateTick", "FikaPlayer.ManualUpdate",
                   "ObservedPlayer.ManualUpdate", "BotOwner.LateUpdate", "BotOwner.UpdateManual",
                   "GameWorld.Update", "GameWorld.LateUpdate"):
            proxies[key] = round(fl(m["CallsPerFrame"]), 2)
    d["instanceProxies"] = proxies

    # --- mod-summary.csv ---
    mods = read_csv(os.path.join(root, "mod-summary.csv"))
    d["modSummaryMain"] = [
        {"assembly": r["Assembly"], "plugin": r["PluginName"], "version": r["PluginVersion"],
         "methods": int(fl(r["MethodsObserved"])), "harmonyHandlers": int(fl(r["HarmonyHandlersObserved"])),
         "calls": int(fl(r["Calls"])), "selfTotalMs": round(fl(r["SelfTotalMs"]), 1),
         "selfPerFrameMs": round(fl(r["SelfPerFrameMs"]), 4), "maxMethodInclMs": round(fl(r["MaxMethodInclusiveMs"]), 2)}
        for r in mods if r["ThreadId"] == "1"]
    d["modSummaryMain"].sort(key=lambda x: -x["selfTotalMs"])
    d["modTotalSelfPerFrameMs_mainThread"] = round(sum(x["selfPerFrameMs"] for x in d["modSummaryMain"]), 3)

    # --- edges.csv ---
    edges = read_csv(os.path.join(root, "edges.csv"))
    d["topEdges"] = [
        {"caller": r["Caller"], "callee": r["Callee"], "calls": int(fl(r["Calls"])),
         "inclTotalMs": round(fl(r["InclusiveTotalMs"]), 1), "avgInclMs": round(fl(r["AverageInclusiveMs"]), 4),
         "maxInclMs": round(fl(r["MaxInclusiveMs"]), 2)}
        for r in sorted(edges, key=lambda r: -fl(r["InclusiveTotalMs"]))[:35] if r["ThreadId"] == "1"]

    # --- harmony-patches.csv ---
    hp = read_csv(os.path.join(root, "harmony-patches.csv"))
    executed = [r for r in hp if fl(r["Calls"]) > 0]
    d["harmonyPatchRows"] = len(hp)
    d["harmonyPatchesExecuted"] = len(executed)
    d["topHarmonyBySelf"] = [
        {"owner": r["Owner"], "kind": r["Kind"], "target": f'{r["OriginalType"]}.{r["OriginalMethod"]}',
         "patch": f'{r["PatchType"]}.{r["PatchMethod"]}', "plugin": r["PluginName"],
         "calls": int(fl(r["Calls"])), "selfTotalMs": round(fl(r["SelfTotalMs"]), 2),
         "inclTotalMs": round(fl(r["InclusiveTotalMs"]), 2), "maxInclMs": round(fl(r["MaxInclusiveMs"]), 3)}
        for r in sorted(executed, key=lambda r: -fl(r["SelfTotalMs"]))[:30]]
    d["topHarmonyByCalls"] = [
        {"owner": r["Owner"], "kind": r["Kind"], "target": f'{r["OriginalType"]}.{r["OriginalMethod"]}',
         "plugin": r["PluginName"], "calls": int(fl(r["Calls"])),
         "selfTotalMs": round(fl(r["SelfTotalMs"]), 2)}
        for r in sorted(executed, key=lambda r: -fl(r["Calls"]))[:20]]

    # --- timeline.csv: bucket totals + spike/periodicity detection ---
    tl = read_csv(os.path.join(root, "timeline.csv"))
    bucket_self = defaultdict(float)          # bucket -> total self ms (main thread)
    method_bucket = defaultdict(dict)         # method -> {bucket: selfMs}
    for r in tl:
        if r["ThreadId"] != "1":
            continue
        b = int(r["BucketIndex"])
        s = fl(r["SelfMs"])
        bucket_self[b] += s
        method_bucket[r["Method"]][b] = method_bucket[r["Method"]].get(b, 0.0) + s
    nb = max(bucket_self) + 1 if bucket_self else 0
    series = [round(bucket_self.get(i, 0.0), 2) for i in range(nb)]
    d["bucketSeries250ms"] = series
    med = statistics.median(series) if series else 0
    d["bucketStats"] = {"median": round(med, 2), "max": round(max(series), 2) if series else 0,
                        "spikeBuckets(>2x med)": [i for i, v in enumerate(series) if med > 0 and v > 2 * med]}
    # spiky methods: high max-bucket / median-bucket ratio with meaningful cost
    spiky = []
    for meth, bm in method_bucket.items():
        vals = [bm.get(i, 0.0) for i in range(nb)]
        tot = sum(vals)
        if tot < 20:  # ignore trivial
            continue
        mx = max(vals)
        mmed = statistics.median(vals)
        ratio = mx / mmed if mmed > 0.01 else float("inf")
        peaks = [i for i, v in enumerate(vals) if v > 0.5 * mx and v > mmed * 3]
        gaps = [round((peaks[i + 1] - peaks[i]) * 0.25, 2) for i in range(len(peaks) - 1)] if len(peaks) > 1 else []
        spiky.append({"method": meth, "totalSelfMs": round(tot, 1), "maxBucketMs": round(mx, 2),
                      "medianBucketMs": round(mmed, 2), "ratio": round(ratio, 1) if ratio != float("inf") else "inf",
                      "peakBuckets": peaks[:20], "peakGapsSeconds": gaps[:19]})
    spiky.sort(key=lambda x: -x["maxBucketMs"])
    d["spikyMethods"] = spiky[:25]

    # --- worst-frames deep captures ---
    wf_dir = os.path.join(root, "worst-frames")
    id2name = {m["MethodId"]: f'{m["DeclaringType"]}.{m["Method"]}' for m in methods}
    id2asm = {m["MethodId"]: m["Assembly"] for m in methods}
    wf_files = sorted(os.listdir(wf_dir)) if os.path.isdir(wf_dir) else []
    agg = defaultdict(lambda: [0, 0.0, 0])  # method -> [appearances, total selfMs, total calls]
    per_frame = []
    for fn in wf_files:
        with open(os.path.join(wf_dir, fn), encoding="utf-8") as f:
            w = json.load(f)
        rows = [r for r in w.get("methods", []) if r.get("threadId") == 1]
        rows.sort(key=lambda r: -r["selfNs"])
        top = [{"m": id2name.get(str(r["methodId"]), f'#{r["methodId"]}'),
                "selfMs": round(r["selfNs"] / 1e6, 3), "calls": r["callCount"]} for r in rows[:12]]
        for r in rows[:30]:
            k = id2name.get(str(r["methodId"]), f'#{r["methodId"]}')
            a = agg[k]
            a[0] += 1
            a[1] += r["selfNs"] / 1e6
            a[2] += r["callCount"]
        per_frame.append({"file": fn, "frame": w["frameNumber"],
                          "frameMs": round(w["frameDurationNs"] / 1e6, 2),
                          "managedMs": round(w["managedProfiledNs"] / 1e6, 2),
                          "top": top[:8]})
    per_frame.sort(key=lambda x: -x["frameMs"])
    d["deepWorstFrames"] = {"count": len(wf_files), "frames": per_frame[:20],
                            "aggregatedTopMethods": sorted(
                                [{"method": k, "appearances": v[0], "totalSelfMs": round(v[1], 1), "calls": v[2]}
                                 for k, v in agg.items()], key=lambda x: -x["totalSelfMs"])[:30]}
    return d

all_digests = {}
for name, root in CAPTURES:
    print("processing", name)
    all_digests[name] = digest_capture(name, root)

with open(os.path.join(OUT, "digests.json"), "w", encoding="utf-8") as f:
    json.dump(all_digests, f, indent=1)
print("done ->", os.path.join(OUT, "digests.json"))
