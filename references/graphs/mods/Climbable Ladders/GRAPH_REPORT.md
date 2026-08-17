# Graph Report - modded  (2026-08-15)

## Corpus Check
- 32 files · ~7,222 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 336 nodes · 472 edges · 19 communities (17 shown, 2 thin omitted)
- Extraction: 98% EXTRACTED · 2% INFERRED · 0% AMBIGUOUS · INFERRED: 10 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `63aababb`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- GameObjectDisablerByPathEditorWindow
- ModulePatch
- ProceduralLadderBody
- PlayerLadderController
- ProceduralLadderLimb
- ObservedPlayerLadderController
- ProceduralGrip
- LadderStatePacket
- LaddersLoader
- tarkin.ladders.fika
- tarkin.ladders.bep
- FikaHandler
- Ladder.cs
- Patch_MoveInputTranslator_TranslateAxes
- Patch_Physical_CanClimb
- LadderEditor
- ladders.shared.editor/package.json
- ladders.shared/package.json

## God Nodes (most connected - your core abstractions)
1. `PlayerLadderController` - 25 edges
2. `ProceduralLadderBody` - 21 edges
3. `tarkin.ladders.bep` - 17 edges
4. `ProceduralGrip` - 17 edges
5. `GameObjectDisablerByPathEditorWindow` - 17 edges
6. `ProceduralLadderLimb` - 15 edges
7. `GameObjectDisablerByPath` - 13 edges
8. `FikaHandler` - 12 edges
9. `LaddersLoader` - 11 edges
10. `ProceduralLadderLeg` - 11 edges

## Surprising Connections (you probably didn't know these)
- `PlayerLadderController` --references--> `ProceduralLadderBody`  [EXTRACTED]
  mods/Climbable Ladders/modded/ladders.bep/PlayerLadderController.cs → mods/Climbable Ladders/modded/ladders.bep/ProceduralLadderBody.cs
- `MainPlayerLadderControllerTracker` --references--> `PlayerLadderController`  [EXTRACTED]
  mods/Climbable Ladders/modded/ladders.fika/MainPlayerTracker.cs → mods/Climbable Ladders/modded/ladders.bep/PlayerLadderController.cs
- `ProceduralLadderBody` --references--> `ProceduralGrip`  [EXTRACTED]
  mods/Climbable Ladders/modded/ladders.bep/ProceduralLadderBody.cs → mods/Climbable Ladders/modded/ladders.bep/ProceduralGrip.cs
- `ProceduralLadderBody` --references--> `ProceduralLadderArm`  [EXTRACTED]
  mods/Climbable Ladders/modded/ladders.bep/ProceduralLadderBody.cs → mods/Climbable Ladders/modded/ladders.bep/ProceduralLadderArm.cs
- `ObservedPlayerLadderController` --references--> `ProceduralLadderBody`  [EXTRACTED]
  mods/Climbable Ladders/modded/ladders.fika/ObservedPlayerLadderController.cs → mods/Climbable Ladders/modded/ladders.bep/ProceduralLadderBody.cs

## Import Cycles
- None detected.

## Communities (19 total, 2 thin omitted)

### Community 0 - "GameObjectDisablerByPathEditorWindow"
Cohesion: 0.10
Nodes (17): EditorWindow, IEnumerator, GameObject, int, SerializedProperty, Vector2, GameObjectDisablerByPathEditorWindow, Dictionary (+9 more)

### Community 1 - "ModulePatch"
Cohesion: 0.08
Nodes (16): GameWorld, MethodBase, PatchPostfix, Patch_GameWorld_Dispose, GameWorld, MethodBase, PatchPostfix, Patch_GameWorld_OnGameStarted (+8 more)

### Community 2 - "ProceduralLadderBody"
Cohesion: 0.07
Nodes (16): AnimationCurve, FieldInfo, IDisposable, float, Ladder, Player, Quaternion, Vector3 (+8 more)

### Community 3 - "PlayerLadderController"
Cohesion: 0.13
Nodes (13): CancellationToken, CancellationTokenSource, bool, DamageInfo, EBodyPart, float, IPlayer, Ladder (+5 more)

### Community 4 - "ProceduralLadderLimb"
Cohesion: 0.13
Nodes (11): Transform, Vector3, ProceduralLadderArm, bool, float, int, Ladder, LimbIK (+3 more)

### Community 5 - "ObservedPlayerLadderController"
Cohesion: 0.09
Nodes (15): DamageInfo, EBodyPart, float, IPlayer, Ladder, Player, ObservedPlayerLadderController, Dictionary (+7 more)

### Community 6 - "ProceduralGrip"
Cohesion: 0.16
Nodes (13): IEnumerable, IWeaponGripPose, bool, float, int, List, Quaternion, Transform (+5 more)

### Community 7 - "LadderStatePacket"
Cohesion: 0.10
Nodes (13): EStateType, INetSerializable, float, int, NetDataReader, NetDataWriter, BarAnglePacket, int (+5 more)

### Community 8 - "LaddersLoader"
Cohesion: 0.12
Nodes (9): AssetBundle, Dictionary, GameWorld, string, LaddersLoader, ManualLogSource, Plugin, PatchManager (+1 more)

### Community 9 - "tarkin.ladders.fika"
Cohesion: 0.14
Nodes (16): tarkin.ladders.bep, netstandard2.1, Microsoft.Unity.Analyzers (1.23.0), UnityEngine.Modules (2022.3.43), Microsoft.NET.Sdk, tarkin.ladders.fika, netstandard2.1, Microsoft.Unity.Analyzers (1.23.0) (+8 more)

### Community 10 - "tarkin.ladders.bep"
Cohesion: 0.16
Nodes (9): AvailableInteractionState, tarkin.ladders.bep, tarkin.ladders.shared, tarkin.ladders.fika, GamePlayerOwner, IInteractive, MethodBase, PatchPrefix (+1 more)

### Community 11 - "FikaHandler"
Cohesion: 0.14
Nodes (8): BaseUnityPlugin, FikaNetworkManagerCreatedEvent, IFikaNetworkManager, List, FikaHandler, ManualLogSource, Plugin, NetPacketProcessor

### Community 12 - "Ladder.cs"
Cohesion: 0.15
Nodes (11): ContextMenu, BuildHierarchyPrefix(), CheckUniqueId(), GameObject, Ladder, Vector3, GenerateUniqueName(), OnValidate() (+3 more)

### Community 13 - "Patch_MoveInputTranslator_TranslateAxes"
Cohesion: 0.19
Nodes (7): Dictionary, MethodBase, PatchPrefix, Player, Patch_MoveInputTranslator_TranslateAxes, MoveInputTranslator, PlayerInputAxesDelegate

### Community 14 - "Patch_Physical_CanClimb"
Cohesion: 0.25
Nodes (6): bool, MethodBase, PatchPostfix, Patch_Physical_CanClimb, Patch_Physical_CanVault, Physical

### Community 15 - "LadderEditor"
Cohesion: 0.25
Nodes (4): tarkin.ladders.shared.editor, Editor, SerializedProperty, LadderEditor

## Knowledge Gaps
- **20 isolated node(s):** `netstandard2.1`, `Microsoft.Unity.Analyzers (1.23.0)`, `UnityEngine.Modules (2022.3.43)`, `Microsoft.NET.Sdk`, `EStateType` (+15 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **2 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `tarkin.ladders.bep` connect `tarkin.ladders.bep` to `ModulePatch`, `ProceduralLadderBody`, `ProceduralLadderLimb`, `ProceduralGrip`, `LaddersLoader`, `Patch_MoveInputTranslator_TranslateAxes`, `Patch_Physical_CanClimb`?**
  _High betweenness centrality (0.266) - this node is a cross-community bridge._
- **Why does `tarkin.ladders.shared` connect `tarkin.ladders.bep` to `GameObjectDisablerByPathEditorWindow`, `Ladder.cs`, `ObservedPlayerLadderController`, `LadderEditor`?**
  _High betweenness centrality (0.229) - this node is a cross-community bridge._
- **Why does `ProceduralLadderBody` connect `ProceduralLadderBody` to `PlayerLadderController`, `ProceduralLadderLimb`, `ObservedPlayerLadderController`, `ProceduralGrip`, `tarkin.ladders.bep`?**
  _High betweenness centrality (0.202) - this node is a cross-community bridge._
- **What connects `netstandard2.1`, `Microsoft.Unity.Analyzers (1.23.0)`, `UnityEngine.Modules (2022.3.43)` to the rest of the system?**
  _20 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `GameObjectDisablerByPathEditorWindow` be split into smaller, more focused modules?**
  _Cohesion score 0.09659090909090909 - nodes in this community are weakly interconnected._
- **Should `ModulePatch` be split into smaller, more focused modules?**
  _Cohesion score 0.08262108262108261 - nodes in this community are weakly interconnected._
- **Should `ProceduralLadderBody` be split into smaller, more focused modules?**
  _Cohesion score 0.07007575757575757 - nodes in this community are weakly interconnected._