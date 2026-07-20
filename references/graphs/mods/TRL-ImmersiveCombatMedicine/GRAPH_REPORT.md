# Graph Report - mods\TRL-ImmersiveCombatMedicine\modded  (2026-07-19)

## Corpus Check
- 47 files · ~51,480 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 835 nodes · 1381 edges · 47 communities (46 shown, 1 thin omitted)
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `6d212d2e`
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
- [[_COMMUNITY_Community 35|Community 35]]
- [[_COMMUNITY_Community 36|Community 36]]
- [[_COMMUNITY_Community 37|Community 37]]
- [[_COMMUNITY_Community 38|Community 38]]
- [[_COMMUNITY_Community 39|Community 39]]
- [[_COMMUNITY_Community 40|Community 40]]
- [[_COMMUNITY_Community 41|Community 41]]
- [[_COMMUNITY_Community 42|Community 42]]
- [[_COMMUNITY_Community 43|Community 43]]
- [[_COMMUNITY_Community 44|Community 44]]
- [[_COMMUNITY_Community 45|Community 45]]
- [[_COMMUNITY_Community 46|Community 46]]

## God Nodes (most connected - your core abstractions)
1. `BandAidUI` - 58 edges
2. `BandAidController` - 48 edges
3. `TraumaEngine` - 44 edges
4. `TraumaArmsConsumer` - 37 edges
5. `TraumaPose` - 30 edges
6. `TraumaFallCycleConsumer` - 28 edges
7. `BandAidNetworkHandler` - 26 edges
8. `MedicalLogic` - 24 edges
9. `MedicHealPatch` - 23 edges
10. `TraumaBotFall` - 23 edges

## Surprising Connections (you probably didn't know these)
- `BandAidController` --inherits--> `MonoBehaviour`  [EXTRACTED]
  Patches/Medical/BandAidController.cs →   _Bridges community 1 → community 45_
- `BandAidUI` --inherits--> `MonoBehaviour`  [EXTRACTED]
  Patches/Medical/BandAidUI.cs →   _Bridges community 45 → community 0_
- `TourniquetManager` --inherits--> `MonoBehaviour`  [EXTRACTED]
  Patches/Medical/TourniquetManager.cs →   _Bridges community 45 → community 5_
- `TraumaArmsConsumer` --inherits--> `MonoBehaviour`  [EXTRACTED]
  Patches/Trauma/TraumaArmsConsumer.cs →   _Bridges community 45 → community 42_
- `TraumaEngine` --inherits--> `MonoBehaviour`  [EXTRACTED]
  Patches/Trauma/TraumaEngine.cs →   _Bridges community 45 → community 15_

## Import Cycles
- None detected.

## Communities (47 total, 1 thin omitted)

### Community 0 - "Community 0"
Cohesion: 0.07
Nodes (30): Color, Color32, Font, FontStyle, Image, TRLImmersiveCombatMedicine, Band_Aid, BandAidUI (+22 more)

### Community 1 - "Community 1"
Cohesion: 0.08
Nodes (21): Coroutine, EBandAidPressMode, EBoundItem, IEnumerator, KeyboardShortcut, KeyGroup, BandAidController, ActionsReturnClass (+13 more)

### Community 2 - "Community 2"
Cohesion: 0.11
Nodes (16): ActiveHealthController, BandAidHealCheckPacket, BandAidHealPacket, BandAidShoulderTapPacket, BandAidTreatmentReportPacket, Band_Aid, BandAidNetworkHandler, BandAidHealCheckResponsePacket (+8 more)

### Community 3 - "Community 3"
Cohesion: 0.16
Nodes (16): Band_Aid, DiscardWatch, MedicalLogic, PendingConsume, bool, EBodyPart, float, IHealthController (+8 more)

### Community 4 - "Community 4"
Cohesion: 0.10
Nodes (16): AnimCleanupPatch, Band_Aid, MedicHealPatch, object, bool, EBodyPart, FieldInfo, float (+8 more)

### Community 5 - "Community 5"
Cohesion: 0.16
Nodes (11): Band_Aid, TourniquetData, TourniquetManager, Dictionary, EBodyPart, float, Item, List (+3 more)

### Community 6 - "Community 6"
Cohesion: 0.15
Nodes (12): BotOwner, HarmonyPostfix, HarmonyPrefix, IPlayer, BotAddEnemyPatch, BotCalcGoalPatch, BotCheckAndAddEnemyPatch, BotIsEnemyPatch (+4 more)

### Community 7 - "Community 7"
Cohesion: 0.16
Nodes (10): BotMemoryClass, AggroHelper, bool, BotOwner, FieldInfo, MethodInfo, Player, PropertyInfo (+2 more)

### Community 8 - "Community 8"
Cohesion: 0.16
Nodes (9): ActionsReturnClass, GamePlayerOwner, HarmonyPostfix, HarmonyPrefix, MethodBase, Player, Band_Aid, FikaReviveGetActionsPatch (+1 more)

### Community 9 - "Community 9"
Cohesion: 0.12
Nodes (11): BandAidController, BaseUnityPlugin, Harmony, BandAidHealCheckResponsePacket, ConfigEntry, float, GameObject, GameWorld (+3 more)

### Community 10 - "Community 10"
Cohesion: 0.18
Nodes (9): ECommand, ETranslateResult, GamePlayerOwner, MovementContext, CantStandUpPatch, FallAttemptCommandPatch, FreezeAxesPatch, FreezeCommandPatch (+1 more)

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
Cohesion: 0.33
Nodes (6): DamageInfoStruct, HarmonyPriority, EBodyPart, Player, DamageTriggerPatch, TrueTrauma

### Community 15 - "Community 15"
Cohesion: 0.09
Nodes (19): Action, bool, Dictionary, EBodyPart, float, GameWorld, IEffect, IPlayer (+11 more)

### Community 16 - "Community 16"
Cohesion: 0.20
Nodes (8): bool, Dictionary, FieldInfo, float, HashSet, ManualLogSource, TraumaState, TrueTrauma

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

### Community 29 - "Community 29"
Cohesion: 0.12
Nodes (16): EDamageType, Func, Action, bool, Dictionary, float, IHealthController, Player (+8 more)

### Community 30 - "Community 30"
Cohesion: 0.17
Nodes (8): HashSet, Player, TraumaChangeReason, TraumaOneShotKind, TraumaRegion, TraumaTransition, TraumaObservability, TRLImmersiveCombatMedicine.Trauma

### Community 31 - "Community 31"
Cohesion: 0.28
Nodes (5): string, TraumaLine, TraumaLocale, TRLImmersiveCombatMedicine.Trauma, TraumaTextId

### Community 32 - "Community 32"
Cohesion: 0.48
Nodes (3): TraumaLine, TraumaMatrixResolver, TRLImmersiveCombatMedicine.Trauma

### Community 34 - "Community 34"
Cohesion: 0.23
Nodes (6): Band_Aid, CustomClassesBridge, bool, MethodInfo, Player, string

### Community 35 - "Community 35"
Cohesion: 0.13
Nodes (13): Action, bool, Dictionary, int, List, MethodInfo, Player, PropertyInfo (+5 more)

### Community 36 - "Community 36"
Cohesion: 0.17
Nodes (12): bool, Dictionary, ESpeedLimit, GameWorld, List, Player, TraumaLine, TraumaOneShotKind (+4 more)

### Community 37 - "Community 37"
Cohesion: 0.25
Nodes (5): MovementContext, Player, CanSprintPatch, TRLImmersiveCombatMedicine.Trauma, UpdateSpeedLimitByHealthPatch

### Community 38 - "Community 38"
Cohesion: 0.07
Nodes (23): ActionData, CustomLayer, CustomLogic, LayerProbe, MethodImpl, Action, bool, BotOwner (+15 more)

### Community 39 - "Community 39"
Cohesion: 0.14
Nodes (11): FallPhase, bool, float, GameWorld, Player, TraumaLine, TraumaOneShotKind, TraumaRegion (+3 more)

### Community 40 - "Community 40"
Cohesion: 0.24
Nodes (5): ESpeedLimit, HashSet, Player, TraumaSpeedCap, TRLImmersiveCombatMedicine.Trauma

### Community 41 - "Community 41"
Cohesion: 0.27
Nodes (5): Dictionary, float, Player, TraumaVoice, TRLImmersiveCombatMedicine.Trauma

### Community 42 - "Community 42"
Cohesion: 0.10
Nodes (17): IHandsController, Action, bool, EBodyPart, FirearmController, float, GameWorld, IEffect (+9 more)

### Community 43 - "Community 43"
Cohesion: 0.19
Nodes (8): GClass3008, bool, EBodyPart, IEffect, MethodInfo, Player, TraumaTremor, TRLImmersiveCombatMedicine.Trauma

### Community 44 - "Community 44"
Cohesion: 0.20
Nodes (7): FieldInfo, FirearmController, MethodBase, ProceduralWeaponAnimation, SetAimLockoutPatch, TremorVisualReassertPatch, TRLImmersiveCombatMedicine.Trauma

### Community 45 - "Community 45"
Cohesion: 0.22
Nodes (7): MonoBehaviour, bool, GameWorld, TraumaRegion, TraumaTransition, TraumaStomachConsumer, TRLImmersiveCombatMedicine.Trauma

### Community 46 - "Community 46"
Cohesion: 0.36
Nodes (5): EBodyPart, float, Player, TraumaBlackoutTrigger, TRLImmersiveCombatMedicine.Trauma

## Knowledge Gaps
- **249 isolated node(s):** `bool`, `TRLImmersiveCombatMedicine`, `ConfigEntry`, `bool`, `float` (+244 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **1 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `BandAidUI` connect `Community 0` to `Community 45`?**
  _High betweenness centrality (0.047) - this node is a cross-community bridge._
- **Why does `TraumaEngine` connect `Community 15` to `Community 45`?**
  _High betweenness centrality (0.039) - this node is a cross-community bridge._
- **Why does `BandAidController` connect `Community 1` to `Community 0`, `Community 45`?**
  _High betweenness centrality (0.039) - this node is a cross-community bridge._
- **What connects `bool`, `TRLImmersiveCombatMedicine`, `ConfigEntry` to the rest of the system?**
  _249 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Community 0` be split into smaller, more focused modules?**
  _Cohesion score 0.06874717322478517 - nodes in this community are weakly interconnected._
- **Should `Community 1` be split into smaller, more focused modules?**
  _Cohesion score 0.07510204081632653 - nodes in this community are weakly interconnected._
- **Should `Community 2` be split into smaller, more focused modules?**
  _Cohesion score 0.10796221322537113 - nodes in this community are weakly interconnected._