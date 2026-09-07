# Graph Report - modded  (2026-09-05)

## Corpus Check
- 25 files · ~440,198 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 260 nodes · 384 edges · 13 communities
- Extraction: 98% EXTRACTED · 2% INFERRED · 0% AMBIGUOUS · INFERRED: 7 edges (avg confidence: 0.84)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `5ddac638`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- ConfigUtil
- MagCheckInterrupt.Utils
- .Debug
- AnimationUtil
- MagCheckReloadOperation
- AmmoDetailsPatch
- PlayerStateDebug
- ConfigPacket
- KeybindsUtil
- MagCheckInterrupt.csproj
- MagCheckInterrupt
- ConfigurationManagerAttributes

## God Nodes (most connected - your core abstractions)
1. `SwapReloadOperation` - 25 edges
2. `MagCheckReloadOperation` - 21 edges
3. `ConfigUtil` - 21 edges
4. `MagCheckInterrupt.Utils` - 13 edges
5. `Fika` - 11 edges
6. `PlayerStateDebug` - 10 edges
7. `MagCheckInterrupt.Patches` - 10 edges
8. `MagCheckInterrupt.Components` - 9 edges
9. `AmmoDetailsPatch` - 9 edges
10. `ConfigPacket` - 7 edges

## Surprising Connections (you probably didn't know these)
- `MagCheckReloadOperation` --references--> `PlayerStateDebug`  [EXTRACTED]
  mods/SPT-MagCheckInterrupt/modded/MagCheckInterrupt/Components/MagCheckReloadOperation.cs → mods/SPT-MagCheckInterrupt/modded/MagCheckInterrupt/Components/PlayerStateDebug.cs

## Import Cycles
- None detected.

## Communities (13 total, 0 thin omitted)

### Community 0 - "ConfigUtil"
Cohesion: 0.06
Nodes (25): ConfigEntry, ConfigFile, EReloadMode, EventHandler, FikaGameEndedEvent, FikaNetworkManagerCreatedEvent, FikaRaidStartedEvent, GUIStyle (+17 more)

### Community 1 - "MagCheckInterrupt.Utils"
Cohesion: 0.07
Nodes (21): MagCheckInterrupt.Patches, MagCheckInterrupt.External, MagCheckInterrupt.Components, MagCheckInterrupt.Utils, FirearmController, IInventoryOperation, MethodBase, PatchPostfix (+13 more)

### Community 2 - ".Debug"
Cohesion: 0.13
Nodes (14): Action, AttachModResult, Conditional, FirearmLightStateStruct, FirearmOperation, Item, Callback, FieldRef (+6 more)

### Community 3 - "AnimationUtil"
Cohesion: 0.10
Nodes (13): AnimatorWrapper, FirearmsAnimator, MethodBase, PatchPrefix, ReloadAnimationPatch, FirearmsAnimator, MethodBase, PatchPrefix (+5 more)

### Community 4 - "MagCheckReloadOperation"
Cohesion: 0.10
Nodes (14): Dictionary, FirearmController, MagCheckReloadOperation, SpeedState, Normal, Restored, Slowed, FirearmController (+6 more)

### Community 5 - "AmmoDetailsPatch"
Cohesion: 0.10
Nodes (13): AmmoDetails, GamePlayerOwner, FieldRef, MethodBase, PatchPrefix, AmmoDetails, AmmoDetailsPatch, EUtilityType (+5 more)

### Community 6 - "PlayerStateDebug"
Cohesion: 0.15
Nodes (11): ItemAddress, Callback, EUtilityType, IdlingOperation, IInventoryOperation, MagazineItemClass, PlayerStateDebug, MonoBehaviour (+3 more)

### Community 7 - "ConfigPacket"
Cohesion: 0.12
Nodes (8): MagCheckInterrupt.Net, INetSerializable, NetDataReader, NetDataWriter, ConfigPacket, NetDataReader, NetDataWriter, ReloadCalledPacket

### Community 8 - "KeybindsUtil"
Cohesion: 0.14
Nodes (10): KeyBindingClass, InputBindingsDataClass, MethodBase, PatchPostfix, UpdateBindingsPatch, InputBindingsDataClass, EReloadMode, Press (+2 more)

### Community 9 - "MagCheckInterrupt.csproj"
Cohesion: 0.18
Nodes (8): netstandard2.1, Microsoft.NET.Sdk, netstandard2.1, Microsoft.NET.Sdk, Microsoft.CodeAnalysis.NetAnalyzers (10.0.*-*), Microsoft.Unity.Analyzers (1.25.*-*), Microsoft.VisualStudio.Threading.Analyzers (17.14.*-*), RoR2.BepInEx.Analyzers (1.0.*-*)

### Community 10 - "MagCheckInterrupt"
Cohesion: 0.29
Nodes (5): BaseUnityPlugin, MagCheckInterrupt, MagCheckInterrupt, LogSource, ManualLogSource

### Community 11 - "ConfigurationManagerAttributes"
Cohesion: 0.40
Nodes (4): ConfigEntryBase&gt;, CustomHotkeyDrawerFunc, Func, ConfigurationManagerAttributes

## Knowledge Gaps
- **24 isolated node(s):** `netstandard2.1`, `Microsoft.NET.Sdk`, `Normal`, `Slowed`, `Restored` (+19 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 109 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `MagCheckInterrupt.Utils` connect `MagCheckInterrupt.Utils` to `ConfigUtil`, `KeybindsUtil`, `MagCheckInterrupt`?**
  _High betweenness centrality (0.262) - this node is a cross-community bridge._
- **Why does `MagCheckReloadOperation` connect `MagCheckReloadOperation` to `MagCheckInterrupt.Utils`, `AmmoDetailsPatch`, `PlayerStateDebug`, `ConfigPacket`?**
  _High betweenness centrality (0.190) - this node is a cross-community bridge._
- **What connects `netstandard2.1`, `Microsoft.NET.Sdk`, `Normal` to the rest of the system?**
  _24 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `ConfigUtil` be split into smaller, more focused modules?**
  _Cohesion score 0.059800664451827246 - nodes in this community are weakly interconnected._
- **Should `MagCheckInterrupt.Utils` be split into smaller, more focused modules?**
  _Cohesion score 0.06736353077816493 - nodes in this community are weakly interconnected._
- **Should `.Debug` be split into smaller, more focused modules?**
  _Cohesion score 0.13118279569892474 - nodes in this community are weakly interconnected._
- **Should `AnimationUtil` be split into smaller, more focused modules?**
  _Cohesion score 0.09782608695652174 - nodes in this community are weakly interconnected._