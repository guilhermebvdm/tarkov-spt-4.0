# Graph Report - mods\stancesAndCameraPositionSPT4.0.11\modded  (2026-06-12)

## Corpus Check
- 23 files · ~64,191 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 417 nodes · 618 edges · 22 communities
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `c3e8df24`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- [[_COMMUNITY_Community 0|Community 0]]
- [[_COMMUNITY_Community 1|Community 1]]
- [[_COMMUNITY_Community 2|Community 2]]
- [[_COMMUNITY_Community 3|Community 3]]
- [[_COMMUNITY_Community 4|Community 4]]
- [[_COMMUNITY_Community 6|Community 6]]
- [[_COMMUNITY_Community 7|Community 7]]
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
- [[_COMMUNITY_Community 24|Community 24]]

## God Nodes (most connected - your core abstractions)
1. `StanceManager` - 60 edges
2. `ModulePatch` - 35 edges
3. `Plugin` - 28 edges
4. `SpringGetPatch` - 18 edges
5. `PlayerStanceController` - 14 edges
6. `MountingManager` - 13 edges
7. `MountingCollisionPatch` - 13 edges
8. `Stance` - 13 edges
9. `MountingUI` - 9 edges
10. `ConfigurationManagerAttributes` - 9 edges

## Surprising Connections (you probably didn't know these)
- `BattleUIScreenPatch` --inherits--> `ModulePatch`  [EXTRACTED]
  MountingUI.cs → Plugin.cs
- `FOVClampPatch` --inherits--> `ModulePatch`  [EXTRACTED]
  Patches/FOVClampPatch.cs → Plugin.cs
- `FOVSliderPatch` --inherits--> `ModulePatch`  [EXTRACTED]
  Patches/FOVSliderPatch.cs → Plugin.cs
- `StartEquipWeapPatch` --inherits--> `ModulePatch`  [EXTRACTED]
  Patches/ManualChamberingPatches.cs → Plugin.cs
- `StartReloadResetPatch` --inherits--> `ModulePatch`  [EXTRACTED]
  Patches/ManualChamberingPatches.cs → Plugin.cs

## Import Cycles
- None detected.

## Communities (22 total, 0 thin omitted)

### Community 0 - "Community 0"
Cohesion: 0.06
Nodes (17): GameWorld, KeyCode, List, CameraRotationMod, bool, ConfigEntry, FieldInfo, FirearmController (+9 more)

### Community 1 - "Community 1"
Cohesion: 0.08
Nodes (22): Action, ChamberWeaponClass, FirearmsAnimator, MonoBehaviour, CameraRotationMod.Patches, bool, ECommand, FirearmController (+14 more)

### Community 2 - "Community 2"
Cohesion: 0.09
Nodes (20): Callback, Func, GClass2015, GClass2050, MagazineItemClass, ActionStanceCheckChamberPatch, ActionStanceCheckFireModePatch, ActionStanceExamineWeaponPatch (+12 more)

### Community 3 - "Community 3"
Cohesion: 0.07
Nodes (22): ApplyProne, BaseUnityPlugin, ConfigurationManagerAttributes, Dictionary, EventArgs, ManualLogSource, MethodInfo, ModSpeed (+14 more)

### Community 4 - "Community 4"
Cohesion: 0.10
Nodes (15): NewRecoilShotEffect, AddRecoilForceMountPatch, CameraRotationMod.Patches, bool, FieldInfo, FirearmController, float, MethodBase (+7 more)

### Community 6 - "Community 6"
Cohesion: 0.11
Nodes (13): Dictionary<Spring, PlayerStanceController>, CameraRotationMod.Patches, bool, float, int, MethodBase, PatchPostfix, Player (+5 more)

### Community 7 - "Community 7"
Cohesion: 0.16
Nodes (11): CameraRotationMod.FikaSync, Stance, Deserialize(), FikaNetworkSync, Serialize(), netstandard2.1, MethodImpl, NetDataReader (+3 more)

### Community 9 - "Community 9"
Cohesion: 0.15
Nodes (10): HarmonyPriority, CameraRotationMod.Patches, GClass774, MethodBase, PatchPostfix, PatchPrefix, HandsStaminaConsumePatch, HandsStaminaProcessPatch (+2 more)

### Community 10 - "Community 10"
Cohesion: 0.22
Nodes (8): EBracingDirection, EMountState, CameraRotationMod, FirearmController, float, Player, Vector3, MountingManager

### Community 11 - "Community 11"
Cohesion: 0.13
Nodes (11): EftBattleUIScreen, GameObject, Image, BattleUIScreenPatch, CameraRotationMod, MethodBase, PatchPostfix, MountingUI (+3 more)

### Community 12 - "Community 12"
Cohesion: 0.17
Nodes (7): CameraRotationMod, float, Player, Spring, Stance, Vector3, PlayerStanceController

### Community 13 - "Community 13"
Cohesion: 0.16
Nodes (8): CameraRotationMod.Patches, MethodBase, PatchPostfix, PatchPrefix, Player, MovementContextSpeedPatch, MovementContextSprintSpeedPatch, PlayerChangeSpeedPatch

### Community 14 - "Community 14"
Cohesion: 0.22
Nodes (7): CameraRotationMod.Patches, bool, FirearmController, MethodBase, object, PatchPrefix, SnapFireTriggerPatch

### Community 15 - "Community 15"
Cohesion: 0.20
Nodes (9): ConfigEntryBase>, CustomHotkeyDrawerFunc, Func<object, string>, Func<string, object>, ConfigurationManagerAttributes, bool, int, object (+1 more)

### Community 16 - "Community 16"
Cohesion: 0.24
Nodes (5): CameraRotationMod.Patches, MethodBase, PatchPostfix, GameWorldOnDestroyPatch, GameWorldOnGameStartedPatch

### Community 17 - "Community 17"
Cohesion: 0.22
Nodes (6): GClass1085, NumberSlider, CameraRotationMod.Patches, MethodBase, PatchPostfix, FOVSliderPatch

### Community 18 - "Community 18"
Cohesion: 0.22
Nodes (6): CameraRotationMod.Patches, ECommand, GamePlayerOwner, MethodBase, PatchPrefix, MountingInputPatch

### Community 19 - "Community 19"
Cohesion: 0.22
Nodes (6): CameraRotationMod.Patches, FieldInfo, MethodBase, PatchPostfix, PlayerSpringPatch, PlayerSpring

### Community 20 - "Community 20"
Cohesion: 0.25
Nodes (5): BasePhysicalClass, CameraRotationMod.Patches, MethodBase, PatchPostfix, PhysicalInertiaPatch

### Community 21 - "Community 21"
Cohesion: 0.29
Nodes (4): CameraRotationMod.Patches, MethodBase, PatchPostfix, FOVClampPatch

### Community 22 - "Community 22"
Cohesion: 0.33
Nodes (4): CameraRotationMod, bool, float, StanceStaminaState

### Community 24 - "Community 24"
Cohesion: 0.50
Nodes (3): CameraRotationMod, ConfigEntry, StanceConfig

## Knowledge Gaps
- **121 isolated node(s):** `netstandard2.1`, `Microsoft.NET.Sdk`, `CameraRotationMod.FikaSync`, `NetDataWriter`, `NetDataReader` (+116 more)
  These have ≤1 connection - possible missing edges or undocumented components.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `ModulePatch` connect `Community 2` to `Community 1`, `Community 4`, `Community 6`, `Community 9`, `Community 11`, `Community 13`, `Community 14`, `Community 16`, `Community 17`, `Community 18`, `Community 19`, `Community 20`, `Community 21`?**
  _High betweenness centrality (0.489) - this node is a cross-community bridge._
- **Why does `Plugin` connect `Community 3` to `Community 2`?**
  _High betweenness centrality (0.093) - this node is a cross-community bridge._
- **What connects `netstandard2.1`, `Microsoft.NET.Sdk`, `CameraRotationMod.FikaSync` to the rest of the system?**
  _121 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Community 0` be split into smaller, more focused modules?**
  _Cohesion score 0.06467661691542288 - nodes in this community are weakly interconnected._
- **Should `Community 1` be split into smaller, more focused modules?**
  _Cohesion score 0.07681365576102418 - nodes in this community are weakly interconnected._
- **Should `Community 2` be split into smaller, more focused modules?**
  _Cohesion score 0.08819345661450925 - nodes in this community are weakly interconnected._
- **Should `Community 3` be split into smaller, more focused modules?**
  _Cohesion score 0.07057057057057058 - nodes in this community are weakly interconnected._