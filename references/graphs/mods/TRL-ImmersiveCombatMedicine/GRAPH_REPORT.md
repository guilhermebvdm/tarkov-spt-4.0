# Graph Report - mods\TRL-ImmersiveCombatMedicine\modded  (2026-07-11)

## Corpus Check
- 24 files · ~18,433 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 377 nodes · 586 edges · 24 communities
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `5f4c6f7f`
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

## God Nodes (most connected - your core abstractions)
1. `BandAidUI` - 51 edges
2. `BandAidController` - 39 edges
3. `BandAidNetworkHandler` - 20 edges
4. `MedicalLogic` - 16 edges
5. `TourniquetManager` - 16 edges
6. `AggroHelper` - 14 edges
7. `MedicHealPatch` - 14 edges
8. `TRLImmersiveCombatMedicinePlugin` - 12 edges
9. `ImageLoader` - 9 edges
10. `LimbUI` - 9 edges

## Surprising Connections (you probably didn't know these)
- `BandAidController` --inherits--> `MonoBehaviour`  [EXTRACTED]
  Patches/Medical/BandAidController.cs →   _Bridges community 1 → community 15_
- `BandAidUI` --inherits--> `MonoBehaviour`  [EXTRACTED]
  Patches/Medical/BandAidUI.cs →   _Bridges community 15 → community 0_
- `TourniquetManager` --inherits--> `MonoBehaviour`  [EXTRACTED]
  Patches/Medical/TourniquetManager.cs →   _Bridges community 15 → community 5_

## Import Cycles
- None detected.

## Communities (24 total, 0 thin omitted)

### Community 0 - "Community 0"
Cohesion: 0.08
Nodes (28): Color, Color32, Font, FontStyle, int, Band_Aid, BandAidUI, LimbUI (+20 more)

### Community 1 - "Community 1"
Cohesion: 0.08
Nodes (20): Coroutine, EBandAidPressMode, EBoundItem, GameWorld, IEnumerator, KeyboardShortcut, KeyGroup, BandAidController (+12 more)

### Community 2 - "Community 2"
Cohesion: 0.12
Nodes (14): ActiveHealthController, BandAidHealCheckPacket, BandAidHealPacket, BandAidShoulderTapPacket, Band_Aid, BandAidNetworkHandler, BandAidHealCheckResponsePacket, bool (+6 more)

### Community 3 - "Community 3"
Cohesion: 0.25
Nodes (10): Band_Aid, MedicalLogic, bool, EBodyPart, IHealthController, Item, ItemStats, ManualLogSource (+2 more)

### Community 4 - "Community 4"
Cohesion: 0.11
Nodes (14): IEffect, AnimCleanupPatch, Band_Aid, MedicHealPatch, object, bool, EBodyPart, float (+6 more)

### Community 5 - "Community 5"
Cohesion: 0.16
Nodes (11): List, Band_Aid, TourniquetData, TourniquetManager, Dictionary, EBodyPart, float, Item (+3 more)

### Community 6 - "Community 6"
Cohesion: 0.15
Nodes (12): IPlayer, BotOwner, HarmonyPostfix, HarmonyPrefix, BotAddEnemyPatch, BotCalcGoalPatch, BotCheckAndAddEnemyPatch, BotIsEnemyPatch (+4 more)

### Community 7 - "Community 7"
Cohesion: 0.16
Nodes (10): BotMemoryClass, AggroHelper, bool, BotOwner, FieldInfo, MethodInfo, Player, Type (+2 more)

### Community 8 - "Community 8"
Cohesion: 0.16
Nodes (9): ActionsReturnClass, GamePlayerOwner, HarmonyPostfix, HarmonyPrefix, MethodBase, Player, Band_Aid, FikaReviveGetActionsPatch (+1 more)

### Community 9 - "Community 9"
Cohesion: 0.17
Nodes (7): BaseUnityPlugin, TRLImmersiveCombatMedicine, TRLImmersiveCombatMedicinePlugin, ConfigEntry, BandAidHealCheckResponsePacket, Harmony, ManualLogSource

### Community 10 - "Community 10"
Cohesion: 0.18
Nodes (8): ECommand, ETranslateResult, MovementContext, GamePlayerOwner, CantStandUpPatch, FreezeAxesPatch, FreezeCommandPatch, TrueTrauma

### Community 11 - "Community 11"
Cohesion: 0.23
Nodes (7): EPhraseTrigger, EBodyPart, Player, HealthUtils, SilenceVoicePatch, TrueTrauma, VoiceHelper

### Community 12 - "Community 12"
Cohesion: 0.20
Nodes (7): Band_Aid, bool, Dictionary, ManualLogSource, Sprite, string, ImageLoader

### Community 13 - "Community 13"
Cohesion: 0.22
Nodes (7): Band_Aid, bool, Dictionary, float, string, ItemDatabase, ItemStats

### Community 14 - "Community 14"
Cohesion: 0.31
Nodes (6): DamageInfoStruct, HarmonyPriority, EBodyPart, Player, DamageTriggerPatch, TrueTrauma

### Community 15 - "Community 15"
Cohesion: 0.25
Nodes (6): FikaPlayer, MonoBehaviour, bool, Player, FaintController, TrueTrauma

### Community 16 - "Community 16"
Cohesion: 0.22
Nodes (8): HashSet, bool, Dictionary, FieldInfo, float, ManualLogSource, TraumaState, TrueTrauma

### Community 17 - "Community 17"
Cohesion: 0.29
Nodes (6): netstandard2.1, BepInEx.Analyzers (1.*), BepInEx.Core (5.*), BepInEx.PluginInfoProps (1.*), UnityEngine.Modules (2019.4.39), Microsoft.NET.Sdk

### Community 18 - "Community 18"
Cohesion: 0.40
Nodes (3): Player, FikaBridge, TrueTrauma

### Community 19 - "Community 19"
Cohesion: 0.33
Nodes (5): Band_Aid, Deserialize(), Serialize(), NetDataReader, NetDataWriter

### Community 20 - "Community 20"
Cohesion: 0.33
Nodes (5): Band_Aid, Deserialize(), Serialize(), NetDataReader, NetDataWriter

### Community 21 - "Community 21"
Cohesion: 0.33
Nodes (5): Band_Aid, Deserialize(), Serialize(), NetDataReader, NetDataWriter

### Community 22 - "Community 22"
Cohesion: 0.40
Nodes (3): Player, MainLoopPatch, TrueTrauma

## Knowledge Gaps
- **120 isolated node(s):** `TrueTrauma`, `Player`, `TrueTrauma`, `MethodInfo`, `PropertyInfo` (+115 more)
  These have ≤1 connection - possible missing edges or undocumented components.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `BandAidUI` connect `Community 0` to `Community 15`?**
  _High betweenness centrality (0.079) - this node is a cross-community bridge._
- **Why does `BandAidController` connect `Community 1` to `Community 15`?**
  _High betweenness centrality (0.064) - this node is a cross-community bridge._
- **Why does `TourniquetManager` connect `Community 5` to `Community 15`?**
  _High betweenness centrality (0.033) - this node is a cross-community bridge._
- **What connects `TrueTrauma`, `Player`, `TrueTrauma` to the rest of the system?**
  _120 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Community 0` be split into smaller, more focused modules?**
  _Cohesion score 0.07890122735242548 - nodes in this community are weakly interconnected._
- **Should `Community 1` be split into smaller, more focused modules?**
  _Cohesion score 0.07641196013289037 - nodes in this community are weakly interconnected._
- **Should `Community 2` be split into smaller, more focused modules?**
  _Cohesion score 0.12258064516129032 - nodes in this community are weakly interconnected._