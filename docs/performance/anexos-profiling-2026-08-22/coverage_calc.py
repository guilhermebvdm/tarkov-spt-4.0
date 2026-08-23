"""Coverage-gaps analysis: plugin inventory vs observed, Harmony baseline, Update-family gap, profiler overhead."""
import csv, json, os, glob, hashlib
from collections import defaultdict

SCRATCH = os.path.dirname(os.path.abspath(__file__))
CAPS = {
    "vanilla-A": r"D:\SPT_2\BepInEx\profiling\2026-08-22_203734",
    "vanilla-B": r"D:\SPT_2\BepInEx\profiling\2026-08-22_203938",
    "modded-A": r"D:\SPT\BepInEx\profiling\2026-08-22_205500",
    "modded-B": r"D:\SPT\BepInEx\profiling\2026-08-22_205604",
}

def read_csv(path):
    with open(path, newline="", encoding="utf-8-sig") as f:
        return list(csv.DictReader(f))

def fl(x):
    try: return float(x)
    except (ValueError, TypeError): return 0.0

out = {}

# frames per capture
nframes = {}
for name, root in CAPS.items():
    with open(os.path.join(root, "frames.csv"), encoding="utf-8-sig") as f:
        nframes[name] = sum(1 for _ in f) - 1
out["nframes"] = nframes

# ---------- 1. plugin inventory vs observed ----------
plugroot = r"D:\SPT\BepInEx\plugins"
entries = sorted(os.listdir(plugroot), key=str.lower)
entry_dlls = {}
for e in entries:
    p = os.path.join(plugroot, e)
    if os.path.isdir(p):
        dlls = sorted({os.path.splitext(os.path.basename(f))[0]
                       for f in glob.glob(os.path.join(p, "**", "*.dll"), recursive=True)})
    elif e.lower().endswith(".dll"):
        dlls = [os.path.splitext(e)[0]]
    else:
        dlls = []
    entry_dlls[e] = dlls
out["inventoryCount"] = len(entries)
out["inventoryEntries"] = entries

# observed assemblies+plugins in modded captures (any thread)
observed_asm = set()
observed_plugins = {}   # assembly -> (pluginName, selfTotalMs mainthread A+B, methods)
for name in ("modded-A", "modded-B"):
    for r in read_csv(os.path.join(CAPS[name], "mod-summary.csv")):
        observed_asm.add(r["Assembly"].lower())
        key = r["Assembly"]
        d = observed_plugins.setdefault(key, {"plugin": r["PluginName"], "selfTotalMs": 0.0, "methods": 0, "caps": set()})
        if r["ThreadId"] == "1":
            d["selfTotalMs"] += fl(r["SelfTotalMs"])
            d["methods"] = max(d["methods"], int(fl(r["MethodsObserved"])))
        d["caps"].add(name)
# also methods.csv PluginName/Assembly (in case mod-summary misses)
for name in ("modded-A", "modded-B"):
    for r in read_csv(os.path.join(CAPS[name], "methods.csv")):
        if r.get("PluginName"):
            observed_asm.add(r["Assembly"].lower())
for k in observed_plugins:
    observed_plugins[k]["caps"] = sorted(observed_plugins[k]["caps"])
out["observedPluginAssemblies"] = {k: v for k, v in sorted(observed_plugins.items(), key=lambda kv: -kv[1]["selfTotalMs"])}

visible, invisible, no_dll = [], [], []
for e in entries:
    dlls = entry_dlls[e]
    if not dlls:
        no_dll.append(e)
        continue
    hit = [d for d in dlls if d.lower() in observed_asm]
    if hit:
        visible.append({"entry": e, "matchedAssemblies": hit})
    else:
        invisible.append({"entry": e, "dlls": dlls[:6], "nDlls": len(dlls)})
out["visibleEntries"] = visible
out["invisibleEntries"] = invisible
out["noDllEntries"] = no_dll
out["counts"] = {"visible": len(visible), "invisible": len(invisible), "noDll": len(no_dll)}

# ---------- 2. Harmony baseline (vanilla) ----------
harm = {}
for name in ("vanilla-A", "vanilla-B"):
    rows = read_csv(os.path.join(CAPS[name], "harmony-patches.csv"))
    executed = [r for r in rows if fl(r["Calls"]) > 0]
    nf = nframes[name]
    per_owner = defaultdict(lambda: [0.0, 0.0, 0, 0])  # self, incl, calls, patches
    for r in executed:
        o = per_owner[(r["Owner"], r["PluginName"])]
        o[0] += fl(r["SelfTotalMs"]); o[1] += fl(r["InclusiveTotalMs"])
        o[2] += int(fl(r["Calls"])); o[3] += 1
    harm[name] = {
        "rowsTotal": len(rows), "executed": len(executed),
        "selfTotalMs": round(sum(fl(r["SelfTotalMs"]) for r in executed), 2),
        "inclTotalMs": round(sum(fl(r["InclusiveTotalMs"]) for r in executed), 2),
        "selfPerFrameMs": round(sum(fl(r["SelfTotalMs"]) for r in executed) / nf, 4),
        "inclPerFrameMs": round(sum(fl(r["InclusiveTotalMs"]) for r in executed) / nf, 4),
        "byOwner": sorted(
            [{"owner": o, "plugin": p, "selfTotalMs": round(v[0], 2), "selfPerFrameMs": round(v[0]/nf, 4),
              "inclTotalMs": round(v[1], 2), "calls": v[2], "callsPerFrame": round(v[2]/nf, 2), "patches": v[3]}
             for (o, p), v in per_owner.items()], key=lambda x: -x["selfTotalMs"]),
        "topPatches": sorted(
            [{"owner": r["Owner"], "kind": r["Kind"], "target": f'{r["OriginalType"]}.{r["OriginalMethod"]}',
              "calls": int(fl(r["Calls"])), "callsPerFrame": round(int(fl(r["Calls"]))/nf, 2),
              "selfTotalMs": round(fl(r["SelfTotalMs"]), 2), "selfPerFrameMs": round(fl(r["SelfTotalMs"])/nf, 5),
              "inclTotalMs": round(fl(r["InclusiveTotalMs"]), 2), "maxInclMs": round(fl(r["MaxInclusiveMs"]), 3),
              "coverage": r.get("TimingCoverage", "")}
             for r in executed], key=lambda x: -x["selfTotalMs"])[:15],
        "distinctOwners": len(per_owner),
    }
# modded harmony csv emptiness
for name in ("modded-A", "modded-B"):
    rows = read_csv(os.path.join(CAPS[name], "harmony-patches.csv"))
    harm[name] = {"rowsTotal": len(rows)}
out["harmony"] = harm

# ---------- 3. Update-family gap ----------
UPD = ("Update", "LateUpdate", "FixedUpdate")
meth = {}
methodname_hist = {}
rowcounts = {}
for name, root in CAPS.items():
    rows = read_csv(os.path.join(root, "methods.csv"))
    rowcounts[name] = {"total": len(rows), "main": sum(1 for r in rows if r["ThreadId"] == "1"),
                       "distinctMethodIds": len({r["MethodId"] for r in rows})}
    main = [r for r in rows if r["ThreadId"] == "1"]
    hist = defaultdict(int)
    for r in main:
        hist[r["Method"]] += 1
    methodname_hist[name] = dict(sorted(hist.items(), key=lambda kv: -kv[1])[:15])
    m = {}
    for r in main:
        key = (r["Assembly"], r["DeclaringType"], r["Method"])
        m[key] = r
    meth[name] = m
out["methodRowCounts"] = rowcounts
out["methodNameHistogram"] = methodname_hist

def updset(name):
    return {k for k in meth[name] if k[2] in UPD}

van_upd = updset("vanilla-A") | updset("vanilla-B")
mod_upd = updset("modded-A") | updset("modded-B")
out["updFamilyCounts"] = {n: len(updset(n)) for n in CAPS}
out["updFamilyUnion"] = {"vanilla": len(van_upd), "modded": len(mod_upd)}

# game-code only (PluginName empty in vanilla row) present in vanilla but absent in modded
missing = []
for key in van_upd - mod_upd:
    # cost in vanilla (take max across A/B by selfPerFrame)
    best = None
    for n in ("vanilla-A", "vanilla-B"):
        r = meth[n].get(key)
        if r is None: continue
        row = {"cap": n, "selfPerFrameMs": fl(r["SelfPerFrameMs"]), "inclPerFrameMs": fl(r["InclusivePerFrameMs"]),
               "callsPerFrame": fl(r["CallsPerFrame"]), "selfTotalMs": fl(r["SelfTotalMs"]),
               "plugin": r["PluginName"]}
        if best is None or row["selfPerFrameMs"] > best["selfPerFrameMs"]:
            best = row
    missing.append({"assembly": key[0], "type": key[1], "method": key[2], **best})
missing.sort(key=lambda x: -x["selfPerFrameMs"])
out["updMissingInModded"] = missing[:40]
out["updMissingInModdedTotals"] = {
    "count": len(missing),
    "gameCodeCount": sum(1 for m in missing if not m["plugin"]),
    "selfPerFrameMsSum": round(sum(m["selfPerFrameMs"] for m in missing), 4),
    "gameCodeSelfPerFrameMsSum": round(sum(m["selfPerFrameMs"] for m in missing if not m["plugin"]), 4),
    "inclPerFrameMsSum_gameCode": round(sum(m["inclPerFrameMs"] for m in missing if not m["plugin"]), 4),
}
# reverse: in modded but not vanilla (expected: TRL mods' MonoBehaviours)
extra = []
for key in mod_upd - van_upd:
    best = None
    for n in ("modded-A", "modded-B"):
        r = meth[n].get(key)
        if r is None: continue
        row = {"cap": n, "selfPerFrameMs": fl(r["SelfPerFrameMs"]), "plugin": r["PluginName"]}
        if best is None or row["selfPerFrameMs"] > best["selfPerFrameMs"]:
            best = row
    extra.append({"assembly": key[0], "type": key[1], "method": key[2], **best})
extra.sort(key=lambda x: -x["selfPerFrameMs"])
out["updOnlyInModdedTop"] = extra[:15]
out["updOnlyInModdedCount"] = len(extra)
out["updOnlyInModdedGameCode"] = [e for e in extra if not e["plugin"]][:25]
out["updOnlyInModdedGameCodeCount"] = sum(1 for e in extra if not e["plugin"])

# is EFT Player.LateUpdate anywhere in modded (any thread / any variant)?
plu = {}
for name in ("modded-A", "modded-B", "vanilla-A", "vanilla-B"):
    hits = []
    for r in read_csv(os.path.join(CAPS[name], "methods.csv")):
        if r["Method"] == "LateUpdate" and "Player" in r["DeclaringType"]:
            hits.append({"thread": r["ThreadId"], "asm": r["Assembly"], "type": r["DeclaringType"],
                         "calls": int(fl(r["Calls"])), "selfPerFrameMs": round(fl(r["SelfPerFrameMs"]), 4),
                         "inclPerFrameMs": round(fl(r["InclusivePerFrameMs"]), 4)})
    plu[name] = hits
out["playerLateUpdatePresence"] = plu

# managed profiled avg per capture (frames.csv)
mng = {}
for name, root in CAPS.items():
    rows = read_csv(os.path.join(root, "frames.csv"))
    vals = [fl(r["ManagedProfiledMs"]) for r in rows]
    fms = [fl(r["FrameMs"]) for r in rows]
    mng[name] = {"managedAvg": round(sum(vals)/len(vals), 3), "frameAvg": round(sum(fms)/len(fms), 3),
                 "managedShareOfFrame": round(100*sum(vals)/sum(fms), 1)}
out["managedVsFrame"] = mng

# ---------- 4. profiler self-overhead ----------
prof = {}
for name, root in CAPS.items():
    rows = [r for r in read_csv(os.path.join(root, "methods.csv"))
            if "profiler" in r["Assembly"].lower() or "profiler" in (r["PluginName"] or "").lower()]
    nf = nframes[name]
    tot_self = sum(fl(r["SelfTotalMs"]) for r in rows)
    prof[name] = {
        "rows": len(rows),
        "selfTotalMs": round(tot_self, 2),
        "selfPerFrameMs": round(tot_self / nf, 4),
        "top": sorted([{"thread": r["ThreadId"], "type": r["DeclaringType"], "method": r["Method"],
                        "calls": int(fl(r["Calls"])), "selfTotalMs": round(fl(r["SelfTotalMs"]), 2),
                        "selfPerFrameMs": round(fl(r["SelfTotalMs"])/nf, 4),
                        "maxInclMs": round(fl(r["MaxInclusiveMs"]), 3)} for r in rows],
                      key=lambda x: -x["selfTotalMs"])[:10],
    }
out["profilerOverhead"] = prof

# ---------- 5. structural checks ----------
# duplicate captures md5
dup = {}
for d in ("2026-08-22_205433", "2026-08-22_205500", "2026-08-22_205508"):
    p = os.path.join(r"D:\SPT\BepInEx\profiling", d, "frames.csv")
    if os.path.exists(p):
        h = hashlib.md5(open(p, "rb").read()).hexdigest()
        dup[d] = h
out["duplicateFramesMd5"] = dup
# worst-frames counts
wf = {}
for name, root in CAPS.items():
    p = os.path.join(root, "worst-frames")
    wf[name] = len(os.listdir(p)) if os.path.isdir(p) else 0
out["worstFramesFileCount"] = wf
# capture durations
durs = {}
for name, root in CAPS.items():
    rows = read_csv(os.path.join(root, "frames.csv"))
    durs[name] = round(fl(rows[-1]["TimestampSeconds"]) - fl(rows[0]["TimestampSeconds"]), 2)
out["captureDurations"] = durs
# frames.csv columns (to confirm: no GC / GPU columns)
with open(os.path.join(CAPS["modded-A"], "frames.csv"), encoding="utf-8-sig") as f:
    out["framesCsvColumns"] = f.readline().strip().split(",")
# ManualDeepCapture / DeepRetained usage
mdc = {}
for name, root in CAPS.items():
    rows = read_csv(os.path.join(root, "frames.csv"))
    mdc[name] = {"manualDeep": sum(1 for r in rows if r["ManualDeepCapture"].strip().lower() in ("true", "1")),
                 "deepRetained": sum(1 for r in rows if r["DeepRetained"].strip().lower() in ("true", "1"))}
out["deepCaptureUsage"] = mdc

with open(os.path.join(SCRATCH, "coverage_calc_out.json"), "w", encoding="utf-8") as f:
    json.dump(out, f, indent=1)
print("done")
print(json.dumps(out["counts"]))
print("updMissing totals:", json.dumps(out["updMissingInModdedTotals"]))
