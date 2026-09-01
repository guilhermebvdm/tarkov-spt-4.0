"""frames-baseline: distribuicoes de FrameMs/ManagedProfiledMs, gap managed, variancia, serie temporal."""
import csv, json, math, os, statistics
from collections import defaultdict

CAPTURES = [
    ("vanilla-A", r"D:\SPT_2\BepInEx\profiling\2026-08-22_203734"),
    ("vanilla-B", r"D:\SPT_2\BepInEx\profiling\2026-08-22_203938"),
    ("modded-A", r"D:\SPT\BepInEx\profiling\2026-08-22_205500"),
    ("modded-B", r"D:\SPT\BepInEx\profiling\2026-08-22_205604"),
]
OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "frames_baseline_results.json")

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

def dist(vals):
    s = sorted(vals)
    return {
        "n": len(s),
        "avg": statistics.mean(s),
        "p50": pct(s, 50), "p90": pct(s, 90), "p95": pct(s, 95),
        "p99": pct(s, 99), "p999": pct(s, 99.9), "max": s[-1],
        "stdev": statistics.pstdev(s),
    }

BANDS = [(0, 8), (8, 16.7), (16.7, 25), (25, 33), (33, 50), (50, 100), (100, float("inf"))]
BAND_LABELS = ["0-8", "8-16.7", "16.7-25", "25-33", "33-50", "50-100", ">100"]

def histogram(vals):
    total_time = sum(vals)
    n = len(vals)
    rows = []
    for (lo, hi), lab in zip(BANDS, BAND_LABELS):
        sel = [v for v in vals if lo <= v < hi] if hi != float("inf") else [v for v in vals if v >= lo]
        rows.append({
            "band": lab, "frames": len(sel),
            "pctFrames": 100 * len(sel) / n,
            "timeMs": sum(sel),
            "pctTime": 100 * sum(sel) / total_time if total_time else 0,
        })
    return rows, total_time

results = {}
frames_data = {}   # name -> list of (t, frameMs, managedMs)
methods_main = {}  # name -> dict key->row (main thread)

for name, root in CAPTURES:
    frames = read_csv(os.path.join(root, "frames.csv"))
    rows = [(fl(r["TimestampSeconds"]), fl(r["FrameMs"]), fl(r["ManagedProfiledMs"])) for r in frames]
    frames_data[name] = rows
    meths = read_csv(os.path.join(root, "methods.csv"))
    md = {}
    for m in meths:
        if m["ThreadId"] != "1":
            continue
        key = f'{m["DeclaringType"]}.{m["Method"]}'
        if key in md:  # same type.method twice (overload) -> merge self
            md[key]["SelfTotalMs"] = str(fl(md[key]["SelfTotalMs"]) + fl(m["SelfTotalMs"]))
            md[key]["Calls"] = str(fl(md[key]["Calls"]) + fl(m["Calls"]))
        else:
            md[key] = dict(m)
    methods_main[name] = md

for name, root in CAPTURES:
    rows = frames_data[name]
    fms = [r[1] for r in rows]
    mms = [r[2] for r in rows]
    gaps = [f - m for _, f, m in rows]
    hist, total_time = histogram(fms)
    d = {
        "root": root,
        "frameMs": dist(fms),
        "managedMs": dist(mms),
        "gapMs": dist(gaps),
        "histogram": hist,
        "totalTimeMs": total_time,
        "durationSeconds": rows[-1][0] - rows[0][0],
        "pctTimeOver25": 100 * sum(v for v in fms if v > 25) / total_time,
        "pctTimeOver33": 100 * sum(v for v in fms if v > 33.3) / total_time,
        "pctFramesOver25": 100 * sum(1 for v in fms if v > 25) / len(fms),
        "pctFramesOver33": 100 * sum(1 for v in fms if v > 33.3) / len(fms),
        "managedShareAvg": 100 * statistics.mean(mms) / statistics.mean(fms),
    }
    # negative gap frames (managed > frame => atribuicao atravessando fronteira de frame?)
    neg = [g for g in gaps if g < 0]
    d["negativeGapFrames"] = {"count": len(neg), "minGap": min(neg) if neg else 0}

    # ---- serie temporal: media por segundo ----
    per_sec = defaultdict(list)
    for t, f, m in rows:
        per_sec[int(t)].append((f, m))
    secs = sorted(per_sec)
    series = []
    for s in secs:
        vs = per_sec[s]
        series.append({
            "sec": s, "n": len(vs),
            "avgFrameMs": statistics.mean(v[0] for v in vs),
            "maxFrameMs": max(v[0] for v in vs),
            "avgManagedMs": statistics.mean(v[1] for v in vs),
        })
    d["perSecond"] = series
    # regressao linear FrameMs ~ t (por frame, nao por segundo, p/ nao pesar segundos c/ poucos frames)
    ts = [r[0] for r in rows]
    n = len(ts)
    mt, mf = statistics.mean(ts), statistics.mean(fms)
    cov = sum((t - mt) * (f - mf) for t, f in zip(ts, fms)) / n
    var = sum((t - mt) ** 2 for t in ts) / n
    slope = cov / var if var else 0
    d["trend"] = {"slopeMsPerSecond": slope, "deltaOver30s": slope * (rows[-1][0] - rows[0][0])}
    # metades
    half = rows[len(rows) // 2][0]
    h1 = [f for t, f, _ in rows if t < half]
    h2 = [f for t, f, _ in rows if t >= half]
    d["halves"] = {"h1Avg": statistics.mean(h1), "h2Avg": statistics.mean(h2),
                   "h1p95": pct(sorted(h1), 95), "h2p95": pct(sorted(h2), 95)}
    # trend tambem no managed
    mm_mean = statistics.mean(mms)
    covm = sum((t - mt) * (m - mm_mean) for t, m in zip(ts, mms)) / n
    d["trendManaged"] = {"slopeMsPerSecond": covm / var if var else 0}
    results[name] = d

# ---- item 2: estimar quanto do gap modded seria managed nao-instrumentado ----
# metodos main-thread do vanilla (ModAttribution) que NAO existem no methods.csv do modded (UpdateOnly)
# => rodam no modded tambem (codigo comum: EFT+SPT+Fika+plugins comuns) mas nao sao medidos la.
adj = {}
for van in ("vanilla-A", "vanilla-B"):
    vrows = methods_main[van]
    nframes_v = len(frames_data[van])
    for mod in ("modded-A", "modded-B"):
        mkeys = set(methods_main[mod].keys())
        missing = []
        for key, m in vrows.items():
            if key not in mkeys:
                missing.append((key, m))
        tot_self = sum(fl(m["SelfTotalMs"]) for _, m in missing)
        plugin_self = sum(fl(m["SelfTotalMs"]) for _, m in missing if m["PluginName"])
        game_self = tot_self - plugin_self
        top = sorted(missing, key=lambda km: -fl(km[1]["SelfTotalMs"]))[:25]
        adj[f"{van}_vs_{mod}"] = {
            "missingMethodCount": len(missing),
            "missingSelfPerFrameMs": tot_self / nframes_v,
            "missingPluginSelfPerFrameMs": plugin_self / nframes_v,
            "missingGameSelfPerFrameMs": game_self / nframes_v,
            "topMissing": [{"m": k, "plugin": m["PluginName"],
                            "selfPerFrameMs": fl(m["SelfTotalMs"]) / nframes_v,
                            "callsPerFrame": fl(m["Calls"]) / nframes_v} for k, m in top],
        }

# principais individuais no vanilla (p/ citar): Player.LateUpdate etc.
key_methods = {}
for van in ("vanilla-A", "vanilla-B"):
    nf = len(frames_data[van])
    km = {}
    for key in ("Player.LateUpdate", "Player.VisualPass", "Player.UpdateTick"):
        if key in methods_main[van]:
            m = methods_main[van][key]
            km[key] = {"selfPerFrameMs": fl(m["SelfTotalMs"]) / nf, "callsPerFrame": fl(m["Calls"]) / nf}
    key_methods[van] = km

# ---- item 3: variancia run-to-run ----
def pair_delta(a, b, field):
    return results[b]["frameMs"][field] - results[a]["frameMs"][field]

noise = {
    "withinVanilla": {f: pair_delta("vanilla-A", "vanilla-B", f) for f in ("avg", "p50", "p90", "p95", "p99", "p999", "max")},
    "withinModded": {f: pair_delta("modded-A", "modded-B", f) for f in ("avg", "p50", "p90", "p95", "p99", "p999", "max")},
}
# cross-env: comparar medias dos pares
cross = {}
for f in ("avg", "p50", "p90", "p95", "p99", "p999", "max"):
    v = (results["vanilla-A"]["frameMs"][f] + results["vanilla-B"]["frameMs"][f]) / 2
    m = (results["modded-A"]["frameMs"][f] + results["modded-B"]["frameMs"][f]) / 2
    noise_ruler = max(abs(noise["withinVanilla"][f]), abs(noise["withinModded"][f]))
    cross[f] = {"vanillaMean": v, "moddedMean": m, "delta": m - v, "noiseRuler": noise_ruler,
                "exceedsNoise": abs(m - v) > noise_ruler}
noise["crossEnv"] = cross

out = {"captures": results, "gapAdjustment": adj, "vanillaKeyMethods": key_methods, "noise": noise}
with open(OUT, "w", encoding="utf-8") as f:
    json.dump(out, f, indent=1)

# ---- print resumo ----
print("=== DISTRIBUICOES FrameMs ===")
for name in results:
    r = results[name]["frameMs"]
    print(f'{name:10s} n={r["n"]:5d} avg={r["avg"]:6.2f} p50={r["p50"]:6.2f} p90={r["p90"]:6.2f} p95={r["p95"]:6.2f} p99={r["p99"]:6.2f} p999={r["p999"]:7.2f} max={r["max"]:7.2f} sd={r["stdev"]:5.2f}')
print("\n=== DISTRIBUICOES ManagedProfiledMs ===")
for name in results:
    r = results[name]["managedMs"]
    print(f'{name:10s} avg={r["avg"]:6.2f} p50={r["p50"]:6.2f} p90={r["p90"]:6.2f} p95={r["p95"]:6.2f} p99={r["p99"]:6.2f} max={r["max"]:7.2f}')
print("\n=== GAP (FrameMs - ManagedMs) ===")
for name in results:
    r = results[name]["gapMs"]
    d = results[name]
    print(f'{name:10s} avg={r["avg"]:6.2f} p50={r["p50"]:6.2f} p95={r["p95"]:6.2f} p99={r["p99"]:6.2f} max={r["max"]:7.2f} | managedShare={d["managedShareAvg"]:5.1f}% negGaps={d["negativeGapFrames"]["count"]}')
print("\n=== HISTOGRAMA (% frames / % tempo) ===")
for name in results:
    print(name)
    for h in results[name]["histogram"]:
        print(f'  {h["band"]:>8s}: {h["frames"]:5d} f ({h["pctFrames"]:5.1f}%)  {h["timeMs"]:8.0f} ms ({h["pctTime"]:5.1f}% tempo)')
    print(f'  tempo>25ms: {results[name]["pctTimeOver25"]:.1f}%  tempo>33ms: {results[name]["pctTimeOver33"]:.1f}%  (frames>25: {results[name]["pctFramesOver25"]:.1f}%, >33: {results[name]["pctFramesOver33"]:.1f}%)')
print("\n=== TREND ===")
for name in results:
    t = results[name]["trend"]
    h = results[name]["halves"]
    print(f'{name:10s} slope={t["slopeMsPerSecond"]:+.4f} ms/s (delta30s={t["deltaOver30s"]:+.2f}ms) | h1avg={h["h1Avg"]:.2f} h2avg={h["h2Avg"]:.2f} | h1p95={h["h1p95"]:.2f} h2p95={h["h2p95"]:.2f}')
print("\n=== AJUSTE DO GAP (metodos vanilla ausentes no modded) ===")
for k, a in adj.items():
    print(f'{k}: {a["missingMethodCount"]} metodos, self total {a["missingSelfPerFrameMs"]:.3f} ms/f (plugins {a["missingPluginSelfPerFrameMs"]:.3f}, jogo {a["missingGameSelfPerFrameMs"]:.3f})')
print("\n=== KEY METHODS VANILLA ===")
print(json.dumps(key_methods, indent=1))
print("\n=== NOISE / CROSS ===")
print(json.dumps(noise, indent=1, default=float))
print("\nsaved ->", OUT)
