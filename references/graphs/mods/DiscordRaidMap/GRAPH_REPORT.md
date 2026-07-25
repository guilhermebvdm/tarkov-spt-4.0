# Graph Report - mods\DiscordRaidMap\modded  (2026-07-25)

## Corpus Check
- 13 files · ~218,840 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 192 nodes · 304 edges · 14 communities (12 shown, 2 thin omitted)
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `56252cb4`
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

## God Nodes (most connected - your core abstractions)
1. `Renderer` - 40 edges
2. `RaidStateCollector` - 23 edges
3. `Color32` - 15 edges
4. `RaidBroadcaster` - 13 edges
5. `RaidMapLifecycle` - 11 edges
6. `DiscordWebhookClient` - 10 edges
7. `ConfigurationManagerAttributes` - 9 edges
8. `Player` - 8 edges
9. `Settings` - 8 edges
10. `IPlayer` - 7 edges

## Surprising Connections (you probably didn't know these)
- `RaidStateCollector` --implements--> `IDisposable`  [EXTRACTED]
  RaidMap/RaidStateCollector.cs → RaidMap/DiscordWebhookClient.cs
- `Renderer` --implements--> `IDisposable`  [EXTRACTED]
  RaidMap/Renderer.cs → RaidMap/DiscordWebhookClient.cs
- `RaidMapLifecycle` --implements--> `IDisposable`  [EXTRACTED]
  RaidMap/RaidMapLifecycle.cs → RaidMap/DiscordWebhookClient.cs

## Import Cycles
- None detected.

## Communities (14 total, 2 thin omitted)

### Community 0 - "Community 0"
Cohesion: 0.10
Nodes (21): Bitmap, byte, Color32, CpuImage, Font, Graphics, ImageCodecInfo, MapDefinition (+13 more)

### Community 1 - "Community 1"
Cohesion: 0.16
Nodes (12): FieldInfo, HashSet, IEnumerable, IPlayer, Player, GameWorld, List, RaidMarkerType (+4 more)

### Community 2 - "Community 2"
Cohesion: 0.18
Nodes (8): MethodBase, ModulePatch, GameWorld, DiscordRaidMap.Patches, GameStartedPatch, GameWorldOnDestroyPatch, PatchPostfix, PatchPrefix

### Community 3 - "Community 3"
Cohesion: 0.13
Nodes (10): DiscordWebhookClient, float, bool, object, RaidSnapshot, Renderer, Task, DiscordRaidMap.RaidMap (+2 more)

### Community 4 - "Community 4"
Cohesion: 0.17
Nodes (7): EventArgs, RaidBroadcaster, IDisposable, GameWorld, Renderer, DiscordRaidMap.Discord, RaidMapLifecycle

### Community 5 - "Community 5"
Cohesion: 0.22
Nodes (8): HttpClient, MultipartFormDataContent, int, string, Task, DiscordRaidMap.Discord, DiscordWebhookClient, WebhookMessage

### Community 6 - "Community 6"
Cohesion: 0.22
Nodes (6): Assembly, Lazy, PropertyInfo, string, DiscordRaidMap.RaidMap, HostCheck

### Community 7 - "Community 7"
Cohesion: 0.27
Nodes (6): ConfigEntry, ConfigFile, List, string, DiscordRaidMap, Settings

### Community 8 - "Community 8"
Cohesion: 0.20
Nodes (9): ConfigEntryBase>, ConfigurationManagerAttributes, bool, int, object, string, CustomHotkeyDrawerFunc, Func<object, string> (+1 more)

### Community 9 - "Community 9"
Cohesion: 0.25
Nodes (4): BaseUnityPlugin, DiscordRaidMap, Plugin, RaidMapLifecycle

### Community 10 - "Community 10"
Cohesion: 0.40
Nodes (4): Dictionary, DiscordRaidMap.RaidMap, MapDefinition, MapRegistry

### Community 11 - "Community 11"
Cohesion: 0.50
Nodes (3): DiscordRaidMap.RaidMap, RaidMarker, RaidSnapshot

## Knowledge Gaps
- **68 isolated node(s):** `bool`, `ConfigEntryBase>`, `CustomHotkeyDrawerFunc`, `string`, `object` (+63 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **2 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `IDisposable` connect `Community 4` to `Community 0`, `Community 1`, `Community 5`?**
  _High betweenness centrality (0.226) - this node is a cross-community bridge._
- **Why does `Renderer` connect `Community 0` to `Community 4`, `Community 13`?**
  _High betweenness centrality (0.219) - this node is a cross-community bridge._
- **Why does `RaidStateCollector` connect `Community 1` to `Community 4`?**
  _High betweenness centrality (0.140) - this node is a cross-community bridge._
- **What connects `bool`, `ConfigEntryBase>`, `CustomHotkeyDrawerFunc` to the rest of the system?**
  _68 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Community 0` be split into smaller, more focused modules?**
  _Cohesion score 0.09663120567375887 - nodes in this community are weakly interconnected._
- **Should `Community 3` be split into smaller, more focused modules?**
  _Cohesion score 0.1323529411764706 - nodes in this community are weakly interconnected._