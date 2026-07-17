# Graph Report - mods\TRL-ItemsManagement\modded  (2026-07-15)

## Corpus Check
- 36 files · ~61,902 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 269 nodes · 335 edges · 32 communities (24 shown, 8 thin omitted)
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `ef101906`
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

## God Nodes (most connected - your core abstractions)
1. `TraderPriceController` - 18 edges
2. `IActionResult` - 11 edges
3. `FleaPriceController` - 10 edges
4. `DebugController` - 9 edges
5. `Task` - 9 edges
6. `TraderPriceOnLoad` - 7 edges
7. `BuyPriceOverrides` - 6 edges
8. `BanController` - 6 edges
9. `CatalogRebuildController` - 6 edges
10. `ItemRefreshController` - 6 edges

## Surprising Connections (you probably didn't know these)
- `BanController` --inherits--> `ControllerBase`  [EXTRACTED]
  Server/Api/BanController.cs →   _Bridges community 2 → community 6_
- `CatalogRebuildController` --inherits--> `ControllerBase`  [EXTRACTED]
  Server/Api/CatalogRebuildController.cs →   _Bridges community 2 → community 7_
- `DebugController` --inherits--> `ControllerBase`  [EXTRACTED]
  Server/Api/DebugController.cs →   _Bridges community 2 → community 4_
- `FleaCapController` --inherits--> `ControllerBase`  [EXTRACTED]
  Server/Api/FleaCapController.cs →   _Bridges community 2 → community 9_
- `FleaLevelController` --inherits--> `ControllerBase`  [EXTRACTED]
  Server/Api/FleaLevelController.cs →   _Bridges community 2 → community 12_

## Import Cycles
- None detected.

## Communities (32 total, 8 thin omitted)

### Community 0 - "Community 0"
Cohesion: 0.19
Nodes (13): TraderPriceController, DeleteTraderPriceRequest, HttpPatch, JsonElement, PatchTraderPriceRequest, Dictionary, HashSet, HttpDelete (+5 more)

### Community 1 - "Community 1"
Cohesion: 0.10
Nodes (15): GStruct300, int, Item, MethodBase, ModulePatch, GetUserItemPricePatch, PatchPostfix, PaymentHelper (+7 more)

### Community 2 - "Community 2"
Cohesion: 0.11
Nodes (13): AuditLogController, DataController, ImagesController, ControllerBase, DateTimeOffset, HttpGet, IActionResult, HashSet (+5 more)

### Community 3 - "Community 3"
Cohesion: 0.15
Nodes (12): FleaPriceController, Computed, DeleteFleaPriceRequest, PreviousCached, HttpDelete, HttpGet, HttpPost, IActionResult (+4 more)

### Community 4 - "Community 4"
Cohesion: 0.26
Nodes (7): DebugController, HttpGet, IActionResult, JsonObject, List, MongoId, Regex

### Community 5 - "Community 5"
Cohesion: 0.15
Nodes (9): IOnLoad, TraderPriceOnLoad, Task, ModItemBanOnLoad, bool, DatabaseService, Dictionary, ISptLogger (+1 more)

### Community 6 - "Community 6"
Cohesion: 0.22
Nodes (7): BanController, HttpPost, IActionResult, JsonNode, Regex, Task, SetBanRequest

### Community 7 - "Community 7"
Cohesion: 0.33
Nodes (5): CatalogRebuildController, RefreshAllRequest, HttpPost, IActionResult, Task

### Community 8 - "Community 8"
Cohesion: 0.40
Nodes (6): ItemRefreshController, RefreshItemRequest, HttpPost, IActionResult, Regex, Task

### Community 9 - "Community 9"
Cohesion: 0.25
Nodes (6): FleaCapController, HttpGet, HttpPost, IActionResult, Task, SetFleaCapRequest

### Community 10 - "Community 10"
Cohesion: 0.22
Nodes (7): OnEntry, ParseSummary, TraderOverrideConfigParser, JsonUtil, MongoId, TCtx, TryResolveTrader

### Community 11 - "Community 11"
Cohesion: 0.22
Nodes (7): BuyPriceLoader, Dictionary, ISptLogger, JsonUtil, MongoId, TraderPriceOnLoad, TraderOverride

### Community 12 - "Community 12"
Cohesion: 0.29
Nodes (5): FleaLevelController, HttpPost, IActionResult, Task, SetFleaMinLevelRequest

### Community 13 - "Community 13"
Cohesion: 0.36
Nodes (4): BuyPriceOverrides, bool, Dictionary, Entry

### Community 14 - "Community 14"
Cohesion: 0.29
Nodes (4): IReadOnlyDictionary, DatabaseService, Dictionary, ModItemBanService

### Community 15 - "Community 15"
Cohesion: 0.29
Nodes (6): net9.0, SPTarkov.Common (4.0.2), SPTarkov.DI (4.0.2), SPTarkov.Server.Core (4.0.2), SPTarkov.Server.Web (4.0.2), Microsoft.NET.Sdk.Web

### Community 16 - "Community 16"
Cohesion: 0.29
Nodes (5): Func, SemaphoreSlim, Task, WriteLockService, T

### Community 17 - "Community 17"
Cohesion: 0.29
Nodes (5): RouteAction, JsonUtil, List, StaticRouter, TraderBuyOverridesRouter

### Community 18 - "Community 18"
Cohesion: 0.33
Nodes (4): HarmonyPrefix, PmcData, SellItemPatch, ProcessSellTradeRequestData

### Community 19 - "Community 19"
Cohesion: 0.40
Nodes (3): BaseUnityPlugin, Plugin, ManualLogSource

### Community 20 - "Community 20"
Cohesion: 0.40
Nodes (3): RunResult, Task, NodeScriptRunner

### Community 22 - "Community 22"
Cohesion: 0.40
Nodes (3): JsonNode, Regex, StyleSensitiveJsonWriter

## Knowledge Gaps
- **103 isolated node(s):** `bool`, `Entry`, `MethodBase`, `TraderClass`, `GStruct300` (+98 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **8 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `TraderPriceController` connect `Community 0` to `Community 2`?**
  _High betweenness centrality (0.082) - this node is a cross-community bridge._
- **Why does `FleaPriceController` connect `Community 3` to `Community 2`?**
  _High betweenness centrality (0.061) - this node is a cross-community bridge._
- **Why does `DebugController` connect `Community 4` to `Community 2`?**
  _High betweenness centrality (0.043) - this node is a cross-community bridge._
- **What connects `bool`, `Entry`, `MethodBase` to the rest of the system?**
  _103 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Community 1` be split into smaller, more focused modules?**
  _Cohesion score 0.1 - nodes in this community are weakly interconnected._
- **Should `Community 2` be split into smaller, more focused modules?**
  _Cohesion score 0.10526315789473684 - nodes in this community are weakly interconnected._