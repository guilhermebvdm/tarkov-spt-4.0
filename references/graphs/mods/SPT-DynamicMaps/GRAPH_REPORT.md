# Graph Report - mods\SPT-DynamicMaps\modded  (2026-06-12)

## Corpus Check
- 43 files · ~466,413 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 688 nodes · 983 edges · 32 communities
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS · INFERRED: 2 edges (avg confidence: 0.8)
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
- [[_COMMUNITY_Community 20|Community 20]]
- [[_COMMUNITY_Community 21|Community 21]]
- [[_COMMUNITY_Community 22|Community 22]]
- [[_COMMUNITY_Community 23|Community 23]]
- [[_COMMUNITY_Community 25|Community 25]]
- [[_COMMUNITY_Community 26|Community 26]]
- [[_COMMUNITY_Community 27|Community 27]]
- [[_COMMUNITY_Community 29|Community 29]]
- [[_COMMUNITY_Community 30|Community 30]]
- [[_COMMUNITY_Community 34|Community 34]]
- [[_COMMUNITY_Community 35|Community 35]]
- [[_COMMUNITY_Community 36|Community 36]]

## God Nodes (most connected - your core abstractions)
1. `ModdedMapScreen` - 42 edges
2. `MapView` - 29 edges
3. `OtherPlayersMarkerProvider` - 23 edges
4. `CorpseMarkerProvider` - 22 edges
5. `QuestUtils` - 22 edges
6. `BackpackMarkerProvider` - 21 edges
7. `MapMarker` - 20 edges
8. `MapSelectDropdown` - 20 edges
9. `GameUtils` - 19 edges
10. `ExtractMarkerProvider` - 18 edges

## Surprising Connections (you probably didn't know these)
- `AirdropMarkerProvider` --implements--> `IDynamicMarkerProvider`  [EXTRACTED]
  DynamicMarkers/AirdropMarkerProvider.cs → DynamicMarkers/IDynamicMarkerProvider.cs
- `BTRMarkerProvider` --implements--> `IDynamicMarkerProvider`  [EXTRACTED]
  DynamicMarkers/BTRMarkerProvider.cs → DynamicMarkers/IDynamicMarkerProvider.cs
- `BackpackMarkerProvider` --implements--> `IDynamicMarkerProvider`  [EXTRACTED]
  DynamicMarkers/BackpackMarkerProvider.cs → DynamicMarkers/IDynamicMarkerProvider.cs
- `CorpseMarkerProvider` --implements--> `IDynamicMarkerProvider`  [EXTRACTED]
  DynamicMarkers/CorpseMarkerProvider.cs → DynamicMarkers/IDynamicMarkerProvider.cs
- `ExtractMarkerProvider` --implements--> `IDynamicMarkerProvider`  [EXTRACTED]
  DynamicMarkers/ExtractMarkerProvider.cs → DynamicMarkers/IDynamicMarkerProvider.cs

## Import Cycles
- None detected.

## Communities (32 total, 0 thin omitted)

### Community 0 - "Community 0"
Cohesion: 0.06
Nodes (22): CursorPositionText, IEnumerator, LevelSelectSlider, MapPeekComponent, MapSelectDropdown, Mask, PlayerPositionText, ScrollRect (+14 more)

### Community 1 - "Community 1"
Cohesion: 0.06
Nodes (30): DynamicMaps.UI.Components, MapMarker, DynamicMaps.UI.Components, PlayerMapMarker, DynamicMaps.UI.Components, TransformMapMarker, IPointerEnterHandler, IPointerExitHandler (+22 more)

### Community 2 - "Community 2"
Cohesion: 0.08
Nodes (21): DynamicMaps.UI.Components, MapView, MapLabel, MapLayer, TransformMapMarker, Color, float, GameObject (+13 more)

### Community 3 - "Community 3"
Cohesion: 0.11
Nodes (12): DynamicMaps.UI.Controls, LevelSelectSlider, Scrollbar, bool, float, GameObject, int, List (+4 more)

### Community 4 - "Community 4"
Cohesion: 0.13
Nodes (18): Condition, ConditionCounterCreator, MethodInfo, QuestDataClass, TriggerWithId, Color, FieldInfo, IEnumerable (+10 more)

### Community 5 - "Community 5"
Cohesion: 0.07
Nodes (23): Attribute, BaseUnityPlugin, ManualLogSource, MapScreen, ModdedMapScreen, EftBattleUIScreen, int, string (+15 more)

### Community 6 - "Community 6"
Cohesion: 0.14
Nodes (10): bool, Color, Dictionary, IPlayer, MapDef, MapView, Player, string (+2 more)

### Community 7 - "Community 7"
Cohesion: 0.14
Nodes (10): CorpseMarkerProvider, bool, Color, Dictionary, IPlayer, MapDef, MapView, Player (+2 more)

### Community 8 - "Community 8"
Cohesion: 0.12
Nodes (10): BTRView, Profile, FieldInfo, HashSet, IPlayer, Player, PropertyInfo, Type (+2 more)

### Community 9 - "Community 9"
Cohesion: 0.12
Nodes (11): DynamicMaps.UI.Controls, MapSelectDropdown, DropDownBox, Dictionary, GameObject, HashSet, IEnumerable, List (+3 more)

### Community 10 - "Community 10"
Cohesion: 0.13
Nodes (9): MapDef, MapView, DynamicMaps.DynamicMarkers, IDynamicMarkerProvider, MapDef, MapView, string, DynamicMaps.DynamicMarkers (+1 more)

### Community 11 - "Community 11"
Cohesion: 0.16
Nodes (9): BackpackMarkerProvider, Color, Dictionary, LootItem, MapDef, MapView, string, Vector2 (+1 more)

### Community 12 - "Community 12"
Cohesion: 0.16
Nodes (10): bool, Color, Dictionary, MapDef, MapView, string, DynamicMaps.DynamicMarkers, ExtractMarkerProvider (+2 more)

### Community 13 - "Community 13"
Cohesion: 0.10
Nodes (12): AbstractTextControl, DynamicMaps.UI.Controls, CursorPositionText, DynamicMaps.UI.Controls, DynamicMaps.UI.Controls, PlayerPositionText, bool, GameObject (+4 more)

### Community 14 - "Community 14"
Cohesion: 0.17
Nodes (9): AirdropMarkerProvider, AirdropBox, Color, Dictionary, MapDef, MapView, string, Vector2 (+1 more)

### Community 15 - "Community 15"
Cohesion: 0.12
Nodes (12): Item, bool, Dictionary, FieldInfo, LootItem, MethodBase, PatchPostfix, Player (+4 more)

### Community 16 - "Community 16"
Cohesion: 0.14
Nodes (10): Component, Image, Tween, Color, GameObject, KeyboardShortcut, RectTransform, TextMeshProUGUI (+2 more)

### Community 17 - "Community 17"
Cohesion: 0.19
Nodes (8): BTRMarkerProvider, Color, MapDef, MapMarker, MapView, string, Vector2, DynamicMaps.DynamicMarkers

### Community 18 - "Community 18"
Cohesion: 0.21
Nodes (7): Color, MapDef, MapView, PlayerMapMarker, string, DynamicMaps.DynamicMarkers, PlayerMarkerProvider

### Community 20 - "Community 20"
Cohesion: 0.26
Nodes (5): List, MapDef, MapView, DynamicMaps, QuestMarkerProvider

### Community 21 - "Community 21"
Cohesion: 0.16
Nodes (8): DynamicMaps.UI.Components, ILayerBound, MapLayer, MapLayerDef, float, GameObject, LayerStatus, Vector3

### Community 22 - "Community 22"
Cohesion: 0.08
Nodes (16): DynamicMaps.UI.Components, MapLabel, DynamicMaps.UI.Components, MapPeekComponent, ILayerBound, MonoBehaviour, bool, Color (+8 more)

### Community 23 - "Community 23"
Cohesion: 0.19
Nodes (6): RotationAxis, Transform, Vector2, Vector3, DynamicMaps.Utils, MathUtils

### Community 25 - "Community 25"
Cohesion: 0.18
Nodes (7): AirdropBoxOnBoxLandPatch, AirdropBox, bool, List, MethodBase, PatchPostfix, DynamicMaps.Patches

### Community 26 - "Community 26"
Cohesion: 0.05
Nodes (27): CommonUI, ModulePatch, BattleUIScreenShowPatch, EftBattleUIScreen, MethodBase, PatchPostfix, DynamicMaps.Patches, CommonUIAwakePatch (+19 more)

### Community 27 - "Community 27"
Cohesion: 0.20
Nodes (9): ConfigurationManagerAttributes, bool, ConfigEntryBase>, CustomHotkeyDrawerFunc, Func<object, string>, Func<string, object>, int, object (+1 more)

### Community 29 - "Community 29"
Cohesion: 0.28
Nodes (6): ConfigFile, List, string, DynamicMaps.Config, Settings, ConfigEntry

### Community 30 - "Community 30"
Cohesion: 0.22
Nodes (7): BoundingRectangle, BoundingRectangularSolid, DynamicMaps.Data, MapDef, MapLabelDef, MapLayerDef, MapMarkerDef

### Community 34 - "Community 34"
Cohesion: 0.29
Nodes (5): Sprite, Texture2D, Dictionary, DynamicMaps.Utils, TextureUtils

### Community 35 - "Community 35"
Cohesion: 0.25
Nodes (4): Color, string, DumpUtils, DynamicMaps.Utils

### Community 36 - "Community 36"
Cohesion: 0.67
Nodes (3): DynamicMaps, net471, Microsoft.NET.Sdk

## Knowledge Gaps
- **247 isolated node(s):** `bool`, `ConfigEntryBase>`, `CustomHotkeyDrawerFunc`, `string`, `object` (+242 more)
  These have ≤1 connection - possible missing edges or undocumented components.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `IDynamicMarkerProvider` connect `Community 10` to `Community 6`, `Community 7`, `Community 11`, `Community 12`, `Community 14`, `Community 17`, `Community 18`, `Community 20`?**
  _High betweenness centrality (0.071) - this node is a cross-community bridge._
- **Why does `ModdedMapScreen` connect `Community 0` to `Community 22`?**
  _High betweenness centrality (0.045) - this node is a cross-community bridge._
- **Why does `MapMarker` connect `Community 1` to `Community 22`?**
  _High betweenness centrality (0.043) - this node is a cross-community bridge._
- **What connects `bool`, `ConfigEntryBase>`, `CustomHotkeyDrawerFunc` to the rest of the system?**
  _247 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Community 0` be split into smaller, more focused modules?**
  _Cohesion score 0.058673469387755105 - nodes in this community are weakly interconnected._
- **Should `Community 1` be split into smaller, more focused modules?**
  _Cohesion score 0.05550416281221091 - nodes in this community are weakly interconnected._
- **Should `Community 2` be split into smaller, more focused modules?**
  _Cohesion score 0.0782608695652174 - nodes in this community are weakly interconnected._