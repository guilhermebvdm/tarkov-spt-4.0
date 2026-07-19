# Graph Report - mods\stancesAndCameraPositionSPT4.0.11\modded  (2026-07-19)

## Corpus Check
- 39 files · ~73,072 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 542 nodes · 740 edges · 36 communities (35 shown, 1 thin omitted)
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `7329a573`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- [[_COMMUNITY_Community 0|Community 0]]
- [[_COMMUNITY_Community 1|Community 1]]
- [[_COMMUNITY_Community 2|Community 2]]
- [[_COMMUNITY_Community 3|Community 3]]
- [[_COMMUNITY_Community 4|Community 4]]
- [[_COMMUNITY_Community 5|Community 5]]
- [[_COMMUNITY_Community 6|Community 6]]
- [[_COMMUNITY_Community 7|Community 7]]
- [[_COMMUNITY_Community 8|Community 8]]
- [[_COMMUNITY_Community 9|Community 9]]
- [[_COMMUNITY_Community 10|Community 10]]
- [[_COMMUNITY_Community 11|Community 11]]
- [[_COMMUNITY_Community 12|Community 12]]
- [[_COMMUNITY_Community 13|Community 13]]
- [[_COMMUNITY_Community 14|Community 14]]
- [[_COMMUNITY_Community 15|Community 15]]
- [[_COMMUNITY_Community 16|Community 16]]
- [[_COMMUNITY_Community 17|Community 17]]
- [[_COMMUNITY_Community 18|Community 18]]
- [[_COMMUNITY_Community 19|Community 19]]
- [[_COMMUNITY_Community 20|Community 20]]
- [[_COMMUNITY_Community 21|Community 21]]
- [[_COMMUNITY_Community 22|Community 22]]
- [[_COMMUNITY_Community 23|Community 23]]
- [[_COMMUNITY_Community 24|Community 24]]
- [[_COMMUNITY_Community 25|Community 25]]
- [[_COMMUNITY_Community 26|Community 26]]
- [[_COMMUNITY_Community 27|Community 27]]
- [[_COMMUNITY_Community 28|Community 28]]
- [[_COMMUNITY_Community 29|Community 29]]
- [[_COMMUNITY_Community 30|Community 30]]
- [[_COMMUNITY_Community 31|Community 31]]
- [[_COMMUNITY_Community 32|Community 32]]
- [[_COMMUNITY_Community 33|Community 33]]
- [[_COMMUNITY_Community 34|Community 34]]

## God Nodes (most connected - your core abstractions)
1. `StanceManager` - 59 edges
2. `ModulePatch` - 35 edges
3. `Plugin` - 34 edges
4. `ApplyComplexRotationPatch` - 24 edges
5. `TransitionMetrics` - 16 edges
6. `HoldBreathPatch` - 15 edges
7. `ApplySimpleRotationPatch` - 14 edges
8. `Stance` - 13 edges
9. `ObservedStanceAnimator` - 12 edges
10. `PassiveMountUI` - 11 edges

## Surprising Connections (you probably didn't know these)
- `BattleUIScreenPatch` --inherits--> `ModulePatch`  [EXTRACTED]
  PassiveMountUI.cs → Plugin.cs
- `ApplyComplexRotationPatch` --inherits--> `ModulePatch`  [EXTRACTED]
  Patches/ApplyComplexRotationPatch.cs → Plugin.cs
- `ApplySimpleRotationPatch` --inherits--> `ModulePatch`  [EXTRACTED]
  Patches/ApplySimpleRotationPatch.cs → Plugin.cs
- `BlockActiveMountPatch` --inherits--> `ModulePatch`  [EXTRACTED]
  Patches/BlockActiveMountPatch.cs → Plugin.cs
- `HoldBreathPatch` --inherits--> `ModulePatch`  [EXTRACTED]
  Patches/HoldBreathPatch.cs → Plugin.cs

## Import Cycles
- None detected.

## Communities (36 total, 1 thin omitted)

### Community 0 - "Community 0"
Cohesion: 0.07
Nodes (17): GameWorld, KeyCode, List, CameraRotationMod, bool, ConfigEntry, FieldInfo, FirearmController (+9 more)

### Community 1 - "Community 1"
Cohesion: 0.06
Nodes (23): ApplyProne, BaseUnityPlugin, ConfigurationManagerAttributes, EventArgs, MethodImpl, MethodInfo, ModSpeed, Multiplier (+15 more)

### Community 2 - "Community 2"
Cohesion: 0.07
Nodes (24): AmmoItemClass, ChamberWeaponClass, ECommand, FirearmsAnimator, GamePlayerOwner, CameraRotationMod.Patches, Action, bool (+16 more)

### Community 3 - "Community 3"
Cohesion: 0.09
Nodes (20): Callback, GClass2015, GClass2050, MagazineItemClass, ActionStanceCheckChamberPatch, ActionStanceCheckFireModePatch, ActionStanceExamineWeaponPatch, ActionStanceOnIdlePatch (+12 more)

### Community 4 - "Community 4"
Cohesion: 0.06
Nodes (21): CameraBobbingScript, CameraRotationMod, MonoBehaviour, CameraRotationMod.Networking, AdsWaypoint, bool, int, PlayerBones (+13 more)

### Community 5 - "Community 5"
Cohesion: 0.10
Nodes (16): ApplyComplexRotationPatch, CameraRotationMod.Patches, AdsWaypoint, bool, FieldInfo, float, int, MethodBase (+8 more)

### Community 6 - "Community 6"
Cohesion: 0.15
Nodes (10): AudioClip, AudioSource, IEnumerator, CameraRotationMod.Patches, Action, bool, MethodBase, PatchPostfix (+2 more)

### Community 7 - "Community 7"
Cohesion: 0.22
Nodes (9): Channel, CameraRotationMod, bool, float, int, Stance, string, Vector3 (+1 more)

### Community 8 - "Community 8"
Cohesion: 0.12
Nodes (12): EftBattleUIScreen, GameObject, Image, BattleUIScreenPatch, CameraRotationMod, float, MethodBase, PatchPostfix (+4 more)

### Community 9 - "Community 9"
Cohesion: 0.15
Nodes (12): ApplySimpleRotationPatch, CameraRotationMod.Patches, bool, FieldInfo, float, int, MethodBase, PatchPostfix (+4 more)

### Community 10 - "Community 10"
Cohesion: 0.16
Nodes (8): CameraRotationMod.Patches, MethodBase, PatchPostfix, PatchPrefix, Player, MovementContextSpeedPatch, MovementContextSprintSpeedPatch, PlayerChangeSpeedPatch

### Community 11 - "Community 11"
Cohesion: 0.21
Nodes (8): CameraRotationMod, bool, ConfigEntry, FieldInfo, Func, Player, StaminaController, StaminaScenario

### Community 12 - "Community 12"
Cohesion: 0.19
Nodes (9): CameraRotationMod.Patches, EBracingDir, FieldInfo, FirearmController, float, MethodBase, PatchPostfix, Vector3 (+1 more)

### Community 13 - "Community 13"
Cohesion: 0.18
Nodes (7): FikaNetworkManagerCreatedEvent, IFikaNetworkManager, CameraRotationMod.Networking, bool, ManualLogSource, FikaSyncManager, StanceSyncPacket

### Community 14 - "Community 14"
Cohesion: 0.18
Nodes (7): CameraRotationMod.Patches, MethodBase, PatchPostfix, PatchPrefix, ProceduralWeaponAnimation, PassiveRecoilPatch, PassiveSwayPatch

### Community 15 - "Community 15"
Cohesion: 0.22
Nodes (7): CameraRotationMod.Patches, bool, FirearmController, MethodBase, object, PatchPrefix, SnapFireTriggerPatch

### Community 16 - "Community 16"
Cohesion: 0.24
Nodes (6): CameraRotationMod.Patches, GClass774, MethodBase, PatchPrefix, HandsConsumeNeutralizePatch, HandsStaminaNeutralizePatch

### Community 17 - "Community 17"
Cohesion: 0.20
Nodes (9): ConfigEntryBase>, CustomHotkeyDrawerFunc, Func<object, string>, Func<string, object>, ConfigurationManagerAttributes, bool, int, object (+1 more)

### Community 18 - "Community 18"
Cohesion: 0.24
Nodes (5): CameraRotationMod.Patches, MethodBase, PatchPostfix, GameWorldOnDestroyPatch, GameWorldOnGameStartedPatch

### Community 19 - "Community 19"
Cohesion: 0.22
Nodes (6): EPointOfView, CameraRotationMod.Patches, MethodBase, PatchPostfix, PlayerBones, ObservedStanceShiftPatch

### Community 20 - "Community 20"
Cohesion: 0.22
Nodes (6): CameraRotationMod.Patches, bool, MethodBase, PatchPostfix, LocaleClassReloadPatch, Task

### Community 21 - "Community 21"
Cohesion: 0.22
Nodes (6): CameraRotationMod.Patches, FieldInfo, MethodBase, PatchPostfix, PlayerSpringPatch, PlayerSpring

### Community 22 - "Community 22"
Cohesion: 0.25
Nodes (5): AdsWaypoint, CameraRotationMod, bool, float, Stance

### Community 23 - "Community 23"
Cohesion: 0.25
Nodes (5): BasePhysicalClass, CameraRotationMod.Patches, MethodBase, PatchPostfix, PhysicalInertiaPatch

### Community 24 - "Community 24"
Cohesion: 0.25
Nodes (4): CameraRotationMod, EBracingDir, float, PassiveMountState

### Community 25 - "Community 25"
Cohesion: 0.25
Nodes (5): BlockActiveMountPatch, CameraRotationMod.Patches, MethodBase, PatchPrefix, Player

### Community 26 - "Community 26"
Cohesion: 0.29
Nodes (4): CameraRotationMod, Dictionary, string, LocaleUtils

### Community 27 - "Community 27"
Cohesion: 0.29
Nodes (4): CameraRotationMod, bool, int, TransitionSpeedTracker

### Community 28 - "Community 28"
Cohesion: 0.33
Nodes (4): GInterface424, GStruct154, Player, TestCompile

### Community 29 - "Community 29"
Cohesion: 0.33
Nodes (5): NetDataReader, NetDataWriter, CameraRotationMod.Networking, Deserialize(), Serialize()

### Community 30 - "Community 30"
Cohesion: 0.33
Nodes (3): CameraRotationMod, Vector3, SpringMath

### Community 31 - "Community 31"
Cohesion: 0.40
Nodes (3): ConfigFile, CameraRotationMod, TranslationUpdater

### Community 32 - "Community 32"
Cohesion: 0.40
Nodes (3): CameraRotationMod, bool, StanceStaminaState

### Community 33 - "Community 33"
Cohesion: 0.50
Nodes (3): CameraRotationMod, ConfigEntry, StanceConfig

## Knowledge Gaps
- **193 isolated node(s):** `CameraRotationMod`, `bool`, `float`, `Stance`, `CameraRotationMod` (+188 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **1 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `ModulePatch` connect `Community 3` to `Community 2`, `Community 5`, `Community 6`, `Community 8`, `Community 9`, `Community 10`, `Community 12`, `Community 14`, `Community 15`, `Community 16`, `Community 18`, `Community 19`, `Community 20`, `Community 21`, `Community 23`, `Community 25`?**
  _High betweenness centrality (0.383) - this node is a cross-community bridge._
- **Why does `Plugin` connect `Community 1` to `Community 3`?**
  _High betweenness centrality (0.090) - this node is a cross-community bridge._
- **Why does `ManualChamberingComponent` connect `Community 2` to `Community 4`?**
  _High betweenness centrality (0.073) - this node is a cross-community bridge._
- **What connects `CameraRotationMod`, `bool`, `float` to the rest of the system?**
  _193 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Community 0` be split into smaller, more focused modules?**
  _Cohesion score 0.06526806526806526 - nodes in this community are weakly interconnected._
- **Should `Community 1` be split into smaller, more focused modules?**
  _Cohesion score 0.06342494714587738 - nodes in this community are weakly interconnected._
- **Should `Community 2` be split into smaller, more focused modules?**
  _Cohesion score 0.07179487179487179 - nodes in this community are weakly interconnected._