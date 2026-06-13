---
name: spt-mod-best-practices
description: Technical best practices for SPT 4.0 / EFT 0.16.x client and server mod development. Use during /create-technical-spec, /review-spec, /review-technical-spec and /code-mod to validate lifecycle, memory, patching and raid-flow correctness. Covers BepInEx plugins, Harmony patches, raid lifecycle (GameWorld, AbstractGame, BaseLocalGame), hideout flow, and common pitfalls in this repo's `mods/<mod>/modded/` sandbox.
---

# SPT Mod Best Practices (SPT 4.0 / EFT 0.16.x)

Apply this skill whenever the task touches code in `mods/<mod>/modded/` or its specs/reviews. Pair with `csharp-mod-best-practices` for language-level rules.

> **📦 Itens / inventário / equipamento / contêineres / armas compostas / presets / munição / hideout:** antes de escrever ou revisar **qualquer** código que toque essas estruturas, **leia `docs/technical/inventario-itens-spt4.md`** — fonte de verdade canônica para `_id`/`_tpl`/`parentId`/`slotId`, `location {x,y,r}` em grades, slots de equipamento, carregador/câmara, presets, re-id ao clonar árvores, e hideout (`Areas`/`HideoutAreas`). Não montar árvore de item "de cabeça" — confira lá.

Authoritative references (in this repo) — full evidence hierarchy in `.agents/resources.md` → "Hierarquia de evidência":
- **Itens/inventário/hideout:** `docs/technical/inventario-itens-spt4.md` (estrutura de árvore de item, `location`, presets, hideout)
- Decompiled client assembly: `references/eft-decompiled/Assembly-CSharp/`
- SPT server source: `references/spt-source/` (server-side logic; gitignored — see `references/README.md`)
- FIKA (coop): `references/fika-server/`, `references/fika-plugin/` (`Fika.Core`), `references/fika-headless/`
- Wiki snapshot: `wiki/spt/` (read-only)
- Mod conventions: `AGENTS.md`, `.agents/conventions.md`
- **Erros recorrentes já cometidos neste repo:** `docs/technical/spt-antipatterns.md` (AP-01..AP-08) — ler antes de escrever ou revisar spec técnica.

## 1. Plugin lifecycle

### `BepInEx.BaseUnityPlugin.Awake()`
- Runs **once at game boot**, before any profile/raid exists. Do **not** access `Singleton<GameWorld>.Instance`, `ClientApplication`, profile data or any in-raid component here — they are null.
- Awake is the only place to: register Harmony patches, read config (`Config.Bind`), subscribe to long-lived static events.
- Never do file I/O, network calls or heavy reflection on the Awake path. It blocks game startup.

### Harmony patches
- One `HarmonyInstance` per plugin, created in `Awake()` with a unique GUID. Never call `PatchAll()` blindly — register patches explicitly so removals are predictable.
- Prefer `[HarmonyPostfix]` for read-only observation, `[HarmonyPrefix]` only when you need to short-circuit or mutate `__args`. Use `[HarmonyTranspiler]` only when no other option works — they break on every EFT update.
- For obfuscated targets (`GClass####`, `Class####`), resolve the `MethodBase` in a static helper using a **stable signature/predicate** (return type + parameter list + name fragment). Do **not** hardcode `GClassNNNN` — those numbers shift between EFT builds.
- Patches must be **idempotent and side-effect-light**. Heavy work inside a patch runs on the game's hot path.

### Configuration
- Use `Config.Bind<T>()` with `ConfigDescription` and `AcceptableValueRange` so values appear correctly in the F12 menu.
- New `ConfigEntry`s require an update to `mods/<mod>/PROPRIEDADES.md` (per `AGENTS.md`). Document units, defaults and side effects.
- Read config via the `ConfigEntry<T>.Value` accessor each time you need it; do **not** cache a stale snapshot at Awake unless the value is intentionally start-only (document it).

## 2. Raid lifecycle (most important section)

This is where 80% of mod bugs live. Anything you allocate, register or subscribe **must be released when the raid ends**, regardless of how the raid ends (extract, death, MIA, alt-F4 to menu).

### Key types (Assembly-CSharp, with line refs from `references/eft-decompiled/`)
- **`Comfort.Common.Singleton<T>`** — the *only* correct Singleton (there is also a `RootMotion.Singleton` — do not import that one). Use `Singleton<GameWorld>.Instance` / `.Instantiated`.
- `EFT/GameWorld.cs` — exists only during a raid. Hosts players, bots, world state. Key members:
  - `MainPlayer` (`:572`) — the local human `Player`.
  - `OnGameStarted()` (`:2584`) — virtual, called once when the raid is fully ready. **This is the correct start hook**, not anything on `BaseLocalGame`.
  - `AfterGameStarted` event (`:961`) — alternative subscription-based start signal.
  - `OnDestroy()` (`:2111`) — virtual, fires when the raid scene is torn down.
  - `OnPersonAdd` event (`:991`, takes `IPlayer`) — bots/humans entering the world.
- `EFT/BaseLocalGame.cs` — controls match flow. Key members:
  - `Stop(string profileId, ExitStatus exitStatus, string exitName, float delay = 0f)` (`:1018`) — the canonical raid-stop entry. Called for `Left` (`:806`, `:1252`), `Killed` (`:896`), `MissingInAction` (`:982`).
  - `vmethod_2()` (abstract, `:735`) — start coroutine, awaited inside the start sequence.
- `EFT/AbstractGame.cs` — base of `BaseLocalGame`. Patch here only if you genuinely need to cover all derived game types (rare).
- `EFT/Player.cs` (human + bot common type), `EFT/NetworkPlayer.cs`, `EFT/HideoutPlayer.cs`. Distinguish via `IsYourPlayer` (`Player`), or `MainPlayer is HideoutPlayer` for hideout context.
- `EFT/IPlayer.cs` — interface used by world events (`OnPersonAdd`, etc.) covering both `Player` and `NetworkPlayer`. Prefer this when iterating "everything that acts in the world".

### Start hook
Subscribe to game-start once (in `Awake`) and re-resolve raid singletons each start:

```csharp
// Patch GameWorld.OnGameStarted (EFT/GameWorld.cs:2584) and call into your manager.
// Inside the postfix:
var gameWorld = Singleton<GameWorld>.Instance; // Comfort.Common
if (gameWorld == null || gameWorld.MainPlayer == null) return; // not in raid, abort
if (gameWorld.MainPlayer is HideoutPlayer) return;             // hideout, abort if raid-only
RaidSession.Begin(gameWorld);
```

Equivalent alternative: subscribe to `gameWorld.AfterGameStarted` (`EFT/GameWorld.cs:961`) inside a one-shot postfix on `GameWorld.Awake`/equivalent. Pick one path and stick to it.

### Stop hook (the leak point)
You **must** hook the raid-stop path. The robust pattern:
- Patch `GameWorld.OnDestroy` (`EFT/GameWorld.cs:2111`) **and** `BaseLocalGame.Stop(...)` (`EFT/BaseLocalGame.cs:1018`). Either may fire first depending on extract type (`Left`/`Killed`/`MIA`).
- Make `RaidSession.End()` idempotent — guard with a `bool _ended` so double-fire is a no-op.
- Inside `End()`: unsubscribe events, dispose `CancellationTokenSource`s, stop `Coroutine`s, destroy spawned `GameObject`s, clear collections that reference `Player`/`Profile` (those references prevent GC of the entire raid).

### Things that leak across raids if you forget
- Static `List<Player>` / `Dictionary<string, BotOwner>` populated during raid and never cleared.
- `MonoBehaviour` instances attached to `gameWorld.transform` or `mainPlayer.gameObject` (destroyed automatically) **vs.** a `GameObject` you parented to `null` or to a persistent root (survives the raid — bug).
- Harmony patches are global and persist across raids — do not register/unregister them per raid; instead make the patch logic check `Singleton<GameWorld>.Instantiated` and bail when false.
- `Action`/`event` subscriptions on long-lived singletons (`Camera.Instance`, `GameUI`, `MonoBehaviourSingleton<Preloader>`) — every raid adds one more handler unless you `-=` it on stop.
- Unity `Coroutine`s started on a `MonoBehaviour` whose owner is the plugin (not destroyed) — use a `CancellationTokenSource` per raid or anchor the coroutine on a `GameWorld`-parented helper.

### Hideout vs. raid vs. menu
The mod can be active in three contexts. Always check which one before acting:
- **Menu / matchmaker:** no `GameWorld`. Most patches should early-return.
- **Hideout:** `GameWorld` may exist with limited features; `MainPlayer` is a `HideoutPlayer` (`EFT/HideoutPlayer.cs`). Robust guard: `if (gameWorld.MainPlayer is HideoutPlayer) return;`. Avoid hardcoded `LocationId == "hideout"` string checks — they shift between EFT builds.
- **Raid:** full `GameWorld`, AI, loot, extracts. The default target for most mods.

If the spec talks about a "raid mod" that should not run in hideout, this guard is mandatory.

## 3. Memory & performance

- Profile bot-bound code: SPT is single-threaded CPU-bound on bot AI (see `wiki/spt/Performance_Tuning.md`). Anything you add inside a per-frame or per-bot-tick patch multiplies by N bots.
- Avoid allocations in hot paths: no `string.Format`, no LINQ chains, no `new List<T>()` per frame inside `Update`/`FixedUpdate`/per-tick AI patches. Reuse buffers.
- Prefer `Span<T>`/array pooling (`ArrayPool<T>.Shared`) over short-lived `List<T>` for tight loops.
- Cache reflection: `MethodInfo`, `FieldInfo`, `PropertyInfo`, `AccessTools.Field`, `AccessTools.Method` — resolve once in a static initializer, never per call.
- Cache the result of `Singleton<T>.Instance` inside a method when used multiple times; do not cache it across raids.
- Textures, `AudioClip`s, `AssetBundle`s loaded by the mod must be `Destroy`/`Unload(true)`-ed on raid end or plugin teardown to free VRAM/RAM.
- Do not `Resources.UnloadUnusedAssets()` or `GC.Collect()` from a mod — both cause hitches and break other mods relying on those assets.

## 4. Server vs. client mods

- **Server mods (C# under `[game]/SPT/user/mods`):** run in the SPT server process. Patch via SPT's DI/router system. Cannot reach Unity types — never reference `UnityEngine` from server code.
- **Client mods (`BepInEx/plugins`):** Unity-side; full access to EFT types. Cannot directly access SPT server internals — communicate via the SPT HTTP routes registered by a paired server mod.
- A **combination mod** must keep the two sides clearly separated in this repo: `mods/<mod>/modded/Server/` and `mods/<mod>/modded/Client/` (or equivalent split). Do not import across the boundary.

## 5. Compatibility & defensive coding

- SPT 3.x and SPT 4.0 are architecturally incompatible — never copy patterns from a 3.x mod without verifying. (See `AGENTS.md`.)
- Verify every Assembly reference cited in a spec by opening `references/eft-decompiled/Assembly-CSharp/<file>.cs:<line>` before writing code that depends on it. Class names and signatures change between EFT 0.16.x patches.
- Wrap every Harmony patch body in `try/catch` and log via `BepInEx.Logging.ManualLogSource`. An unhandled exception inside a prefix can prevent the original method from running and brick the raid.
- Never assume another mod is or is not present. If interop is required, resolve the foreign type via reflection and degrade gracefully when missing.
- Do not write to or read from arbitrary files under `[game]/` from a client mod — restrict I/O to `BepInEx/plugins/<modname>/`.

## 6. Logging

- One `ManualLogSource` per plugin, named after the plugin GUID.
- `LogInfo` for one-time lifecycle events (Awake done, raid start/end). `LogDebug` (gated by config) for per-frame detail. **Never** log per-frame at `LogInfo` — it floods the BepInEx console and tanks FPS.
- Include the raid id (`gameWorld.MainPlayer.Profile.Id` + a session counter) in raid-scoped logs to make multi-raid debugging tractable.

## 7. Sandbox rules in this repo

- All edits go in `mods/<mod>/modded/`. Never modify `mods/<mod>/original/`. (See `/code-mod`.)
- If you need an upstream type or helper, copy the minimum into `modded/` and document the source in a comment: `// ref: original/<file>:<line>`.
- Cite Assembly evidence as `arquivo.cs:linha` (the convention in `AGENTS.md` §"Hierarquia de referências").

## 8. Canonical API vs direct state mutation

- When changing player/weapon/world state, prefer the EFT-canonical entry point (public controller method, `ECommand` via `TranslateCommand`, operation API) over writing internal fields directly. Canonical paths fire the side-effects the rest of the game expects: HUD updates, sounds, animation/state-machine transitions, network sync in Fika. Real cases: stamina drain must go through `Consume()`/`UpdateStamina()` — writing `HandsStamina.Current` directly skips HUD, low-stamina sounds and `HandsExhausted` (stances 001, PA-02-01); weapon mounting must go through `ECommand.WeaponMounting (140)` (stances 004, 06-fix-01).
- Before mutating any field directly, grep the Assembly for the setter/command/operation the game itself uses for that transition and list its side-effects in the technical spec. If you still bypass it, document why and which side-effects you intentionally skip.
- See `docs/technical/spt-antipatterns.md` AP-04 for the full case history.

## Review checklist (use during `/review-technical-spec` and `/code-mod`)

1. **Lifecycle:** Is there a clear Awake / raid-start / raid-end story? Are stop hooks idempotent and covering both `GameWorld.OnDestroy` and `BaseLocalGame.Stop` (patch `AbstractGame.Stop` only to cover all derived game types — rare)?
2. **Leaks:** Every `+= handler`, `new GameObject`, `StartCoroutine`, `CancellationTokenSource`, static collection — is its release point identified?
3. **Hot path:** Any allocations or LINQ in per-frame / per-bot-tick code? Reflection cached?
4. **Context guards:** Does code that assumes a raid early-return in menu/hideout? **Multiplayer/Fika:** does every player-reactive patch distinguish the local player (`IsYourPlayer` / `__instance == MainPlayer.HandsController`) from bots and other Fika players? (AP-02)
5. **Patches:** Targets resolved by signature, not `GClassNNNN`? Bodies wrapped in try/catch and logged?
6. **Compatibility:** Assembly refs verified at `arquivo:linha`? No SPT-3.x patterns? No cross-side imports (server↔client)?
7. **Config:** New entries documented in `PROPRIEDADES.md` with units and defaults?
8. **Sandbox:** All changes in `modded/`? `original/` untouched?
9. **Canonical API:** state changes go through the EFT command/API path (§8)? Side-effects of any bypass documented? (AP-04)
10. **Virtual dispatch:** when patching a virtual/abstract method, were ALL overrides audited for base-call, and is the patch on a routing point that covers every path? (AP-03)

If any item is unanswered, flag as 🔴 in the review or stop the build and request a `/review-technical-spec` pass.
