# Graph Report - modded  (2026-08-15)

## Corpus Check
- 7 files · ~1,126 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 82 nodes · 105 edges · 7 communities
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `63aababb`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- LadderNetworkHandler
- ObservedPlayerLadderController
- LadderNetworkHandler.cs
- LadderStatePacket
- MainPlayerLadderTracker
- BarAnglePacket
- Plugin

## God Nodes (most connected - your core abstractions)
1. `LadderNetworkHandler` - 14 edges
2. `MainPlayerLadderTracker` - 12 edges
3. `ObservedPlayerLadderController` - 10 edges
4. `LadderStatePacket` - 9 edges
5. `BarAnglePacket` - 7 edges
6. `Plugin` - 7 edges
7. `TRL.FikaSync.ClimbableLadders.Networking.Packets` - 4 edges
8. `TRL.FikaSync.ClimbableLadders.Controllers` - 3 edges
9. `TRL.FikaSync.ClimbableLadders.Networking` - 3 edges
10. `EStateType` - 1 edges

## Surprising Connections (you probably didn't know these)
- `Plugin` --references--> `LadderNetworkHandler`  [EXTRACTED]
  mods/TRL-FikaSync-ClimbableLadders/modded/Plugin.cs → mods/TRL-FikaSync-ClimbableLadders/modded/Networking/LadderNetworkHandler.cs

## Import Cycles
- None detected.

## Communities (7 total, 0 thin omitted)

### Community 0 - "LadderNetworkHandler"
Cohesion: 0.18
Nodes (7): FikaNetworkManagerCreatedEvent, IFikaNetworkManager, List, NetPacketProcessor, Player, PlayerLadderController, LadderNetworkHandler

### Community 1 - "ObservedPlayerLadderController"
Cohesion: 0.15
Nodes (9): float, Player, ObservedPlayerLadderController, DamageInfo, EBodyPart, IPlayer, Ladder, MonoBehaviour (+1 more)

### Community 2 - "LadderNetworkHandler.cs"
Cohesion: 0.16
Nodes (8): TRL.FikaSync.ClimbableLadders, TRL.FikaSync.ClimbableLadders.Controllers, TRL.FikaSync.ClimbableLadders.Networking.Packets, TRL.FikaSync.ClimbableLadders.Networking, netstandard2.1, Microsoft.Unity.Analyzers (1.23.0), UnityEngine.Modules (2022.3.43), Microsoft.NET.Sdk

### Community 3 - "LadderStatePacket"
Cohesion: 0.22
Nodes (7): EStateType, int, NetDataReader, NetDataWriter, EStateType, LadderStatePacket, string

### Community 4 - "MainPlayerLadderTracker"
Cohesion: 0.23
Nodes (6): Action, bool, IDisposable, float, PlayerLadderController, MainPlayerLadderTracker

### Community 5 - "BarAnglePacket"
Cohesion: 0.25
Nodes (6): INetSerializable, float, int, NetDataReader, NetDataWriter, BarAnglePacket

### Community 6 - "Plugin"
Cohesion: 0.33
Nodes (3): BaseUnityPlugin, ManualLogSource, Plugin

## Knowledge Gaps
- **6 isolated node(s):** `EStateType`, `TRL.FikaSync.ClimbableLadders`, `netstandard2.1`, `Microsoft.Unity.Analyzers (1.23.0)`, `UnityEngine.Modules (2022.3.43)` (+1 more)
  These have ≤1 connection - possible missing edges or undocumented components.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `LadderNetworkHandler` connect `LadderNetworkHandler` to `LadderNetworkHandler.cs`, `MainPlayerLadderTracker`, `Plugin`?**
  _High betweenness centrality (0.512) - this node is a cross-community bridge._
- **Why does `ObservedPlayerLadderController` connect `ObservedPlayerLadderController` to `LadderNetworkHandler.cs`?**
  _High betweenness centrality (0.294) - this node is a cross-community bridge._
- **Why does `MainPlayerLadderTracker` connect `MainPlayerLadderTracker` to `LadderNetworkHandler.cs`?**
  _High betweenness centrality (0.217) - this node is a cross-community bridge._
- **What connects `EStateType`, `TRL.FikaSync.ClimbableLadders`, `netstandard2.1` to the rest of the system?**
  _6 weakly-connected nodes found - possible documentation gaps or missing edges._