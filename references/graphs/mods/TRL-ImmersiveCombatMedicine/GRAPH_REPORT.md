# Graph Report - mods\TRL-ImmersiveCombatMedicine\modded  (2026-07-13)

## Corpus Check
- 28 files · ~25,263 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 456 nodes · 718 edges · 28 communities
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `4936e8f2`
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
- [[_COMMUNITY_Community 16|Community 16]]
- [[_COMMUNITY_Community 17|Community 17]]
- [[_COMMUNITY_Community 18|Community 18]]
- [[_COMMUNITY_Community 19|Community 19]]
- [[_COMMUNITY_Community 20|Community 20]]
- [[_COMMUNITY_Community 21|Community 21]]
- [[_COMMUNITY_Community 22|Community 22]]
- [[_COMMUNITY_Community 24|Community 24]]
- [[_COMMUNITY_Community 25|Community 25]]
- [[_COMMUNITY_Community 26|Community 26]]
- [[_COMMUNITY_Community 27|Community 27]]
- [[_COMMUNITY_Community 28|Community 28]]

## God Nodes (most connected - your core abstractions)
1. `BandAidUI` - 58 edges
2. `BandAidController` - 44 edges
3. `BandAidNetworkHandler` - 26 edges
4. `MedicalLogic` - 23 edges
5. `MedicHealPatch` - 22 edges
6. `TRLImmersiveCombatMedicinePlugin` - 17 edges
7. `TourniquetManager` - 16 edges
8. `AggroHelper` - 14 edges
9. `Player` - 11 edges
10. `DebugBotInvisibility` - 9 edges

## Surprising Connections (you probably didn't know these)
- `BandAidController` --inherits--> `MonoBehaviour`  [EXTRACTED]
  Patches/Medical/BandAidController.cs →   _Bridges community 1 → community 5_
- `BandAidUI` --inherits--> `MonoBehaviour`  [EXTRACTED]
  Patches/Medical/BandAidUI.cs →   _Bridges community 5 → community 0_

## Import Cycles
- None detected.

## Communities (28 total, 0 thin omitted)

### Community 0 - "Community 0"
Cohesion: 0.07
Nodes (30): Color, Color32, Font, FontStyle, Image, int, TRLImmersiveCombatMedicine, Band_Aid (+22 more)

### Community 1 - "Community 1"
Cohesion: 0.08
Nodes (19): Coroutine, EBandAidPressMode, EBoundItem, IEnumerator, KeyboardShortcut, KeyGroup, BandAidController, ActionsReturnClass (+11 more)

### Community 2 - "Community 2"
Cohesion: 0.11
Nodes (16): ActiveHealthController, BandAidHealCheckPacket, BandAidHealPacket, BandAidShoulderTapPacket, BandAidTreatmentReportPacket, Band_Aid, BandAidNetworkHandler, BandAidHealCheckResponsePacket (+8 more)

### Community 3 - "Community 3"
Cohesion: 0.17
Nodes (14): Band_Aid, MedicalLogic, PendingConsume, bool, EBodyPart, float, IHealthController, Item (+6 more)

### Community 4 - "Community 4"
Cohesion: 0.11
Nodes (16): IEffect, AnimCleanupPatch, Band_Aid, MedicHealPatch, object, bool, EBodyPart, FieldInfo (+8 more)

### Community 5 - "Community 5"
Cohesion: 0.15
Nodes (12): Band_Aid, TourniquetData, TourniquetManager, MonoBehaviour, Dictionary, EBodyPart, float, Item (+4 more)

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
Cohesion: 0.12
Nodes (11): BandAidController, BaseUnityPlugin, Harmony, BandAidHealCheckResponsePacket, ConfigEntry, float, GameObject, GameWorld (+3 more)

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

### Community 16 - "Community 16"
Cohesion: 0.20
Nodes (8): HashSet, bool, Dictionary, FieldInfo, float, ManualLogSource, TraumaState, TrueTrauma

### Community 17 - "Community 17"
Cohesion: 0.33
Nodes (5): netstandard2.1, BepInEx.Analyzers (1.*), BepInEx.Core (5.*), BepInEx.PluginInfoProps (1.*), Microsoft.NET.Sdk

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

### Community 24 - "Community 24"
Cohesion: 0.18
Nodes (6): InteractableObject, MedicInteractable, TRLImmersiveCombatMedicine, ActionsReturnClass, GamePlayerOwner, Player

### Community 25 - "Community 25"
Cohesion: 0.18
Nodes (8): GInterface177, MedicActionsPatch, TRLImmersiveCombatMedicine, ActionsReturnClass, float, GamePlayerOwner, HarmonyPrefix, MethodBase

### Community 26 - "Community 26"
Cohesion: 0.21
Nodes (6): ConfigFile, bool, ConfigEntry, float, DebugBotInvisibility, TRLImmersiveCombatMedicine

### Community 27 - "Community 27"
Cohesion: 0.33
Nodes (5): NetDataReader, NetDataWriter, Band_Aid, Deserialize(), Serialize()

### Community 28 - "Community 28"
Cohesion: 0.33
Nodes (5): Band_Aid, Deserialize(), Serialize(), NetDataReader, NetDataWriter

## Knowledge Gaps
- **144 isolated node(s):** `TRLImmersiveCombatMedicine`, `ConfigEntry`, `bool`, `float`, `ConfigFile` (+139 more)
  These have ≤1 connection - possible missing edges or undocumented components.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `BandAidUI` connect `Community 0` to `Community 5`?**
  _High betweenness centrality (0.057) - this node is a cross-community bridge._
- **Why does `BandAidController` connect `Community 1` to `Community 0`, `Community 5`?**
  _High betweenness centrality (0.048) - this node is a cross-community bridge._
- **What connects `TRLImmersiveCombatMedicine`, `ConfigEntry`, `bool` to the rest of the system?**
  _144 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Community 0` be split into smaller, more focused modules?**
  _Cohesion score 0.06874717322478517 - nodes in this community are weakly interconnected._
- **Should `Community 1` be split into smaller, more focused modules?**
  _Cohesion score 0.07922705314009662 - nodes in this community are weakly interconnected._
- **Should `Community 2` be split into smaller, more focused modules?**
  _Cohesion score 0.10661268556005399 - nodes in this community are weakly interconnected._
- **Should `Community 4` be split into smaller, more focused modules?**
  _Cohesion score 0.10591133004926108 - nodes in this community are weakly interconnected._