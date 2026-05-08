---
name: csharp-mod-best-practices
description: C# language and runtime best practices applied to SPT 4.0 / EFT 0.16.x mod development (BepInEx plugins and SPT C# server mods). Use during /create-technical-spec, /review-spec, /review-technical-spec and /code-mod to validate memory ownership, async/threading, reflection, Harmony patch shape and Unity-specific C# pitfalls. Pair with `spt-mod-best-practices` for lifecycle/raid-flow rules.
---

# C# Best Practices for SPT Mods

Target framework: .NET (Unity Mono runtime ≥ 6.0 for client mods; matching SPT server runtime for server mods). EFT/SPT mod code lives in `mods/<mod>/modded/`.

This skill covers the **C# / runtime** concerns. Lifecycle, raid hooks and SPT-specific rules are in `spt-mod-best-practices`.

## 1. Memory ownership

### Disposal
- Anything implementing `IDisposable` (`CancellationTokenSource`, `HttpClient`, `Stream`, `Bitmap`, `SemaphoreSlim`, custom wrappers) must be disposed deterministically. Prefer `using var x = ...;` for method-scoped resources.
- For raid-scoped resources, store them on a session object and dispose them in the raid-end hook (see `spt-mod-best-practices` §2). Do not rely on finalizers — Unity's GC is not aggressive.
- Never share a single `HttpClient` per request; create one at plugin scope and reuse it. Conversely, never share a `CancellationTokenSource` across raids — create a fresh one each raid.

### References that pin objects
- Static collections (`static List<Player> _bots`) hold strong references and prevent GC of the whole raid graph. Clear them on raid end.
- Subscribed events are strong references too: `someStatic.SomeEvent += OnFoo;` keeps `this` alive until you `-= OnFoo`. For long-lived static publishers, use weak-event patterns or strict subscribe/unsubscribe pairing.
- Closures capture `this` (or surrounding locals) implicitly. Be explicit: assign to a local first if you only need a field, so the closure does not capture the whole instance.

### Allocations
- In hot paths (Harmony postfix on `Update`, AI tick patches, HUD draw), avoid:
  - `string` concatenation/`string.Format` → use `StringBuilder` (cached) or skip the log.
  - LINQ (`Where`, `Select`, `ToList`) → manual `for` loops; LINQ allocates iterators and lambda captures.
  - `new List<T>()` / `new Dictionary<,>()` per call → reuse instance fields or `ArrayPool<T>.Shared`.
  - Boxing of value types (passing `int`/`enum` to `object`-typed APIs, including `string.Format`).
- Prefer `struct` for small immutable data passed by `in`/`ref` to avoid heap traffic. Mark them `readonly struct` to prevent defensive copies.

## 2. Async, threading and Unity

- Unity APIs (`GameObject`, `Transform`, `Singleton<T>.Instance`, `MonoBehaviour`, anything in `UnityEngine.*`) are **main-thread only**. Touching them from a `Task.Run`, a thread-pool callback, or a background `Thread` will silently corrupt state or throw `UnityException`.
- For background work that must update the game, marshal back to the main thread via a `MonoBehaviour` queue (a singleton GameObject draining an `Action` queue in `Update`) or via `UniTask`/coroutines.
- `async void` is forbidden except for top-level event handlers — exceptions vanish. Use `async Task` and `await` it, or wrap in a `try/catch` that logs.
- Always pass a `CancellationToken` through async chains and check `ct.ThrowIfCancellationRequested()` at suspension points. Tie the token to raid lifetime so awaiting code unwinds cleanly on raid end.
- Coroutines: prefer `IEnumerator` started on a `MonoBehaviour` whose lifetime matches the work's scope. Never start a coroutine on a `MonoBehaviour` that may be destroyed before the coroutine completes — it will silently stop mid-step and skip cleanup `finally` blocks.

## 3. Harmony / reflection

- Cache every `MethodInfo`, `FieldInfo`, `PropertyInfo`, `AccessTools.*` lookup in a `static readonly` field initialized once. Reflection lookup is orders of magnitude slower than the call itself.
- Resolve obfuscated targets (`GClass####`, `Class####`, `Struct####`) by **signature predicate**, not by name. Example pattern:

  ```csharp
  static readonly MethodBase Target = AccessTools
      .GetTypesFromAssembly(typeof(GameWorld).Assembly)
      .First(t => t.Name.StartsWith("GClass") && t.GetMethod("Foo", Flags) != null)
      .GetMethod("Foo", Flags);
  ```

  This survives EFT renumbering across patches.
- Never throw out of a Harmony prefix/postfix unless you intend to skip the original. Wrap the body in `try/catch (Exception ex) { Log.LogError(ex); }`. An unhandled prefix exception cancels the original call.
- Prefer `__instance`, `__result`, `__args`, `___privateField` injection over manual reflection inside the patch.
- Do not call back into the patched method from the patch (infinite recursion). Use `[HarmonyReversePatch]` or hold a delegate to the original.

## 4. Nullability and defensive code

- Treat every `Singleton<T>.Instance`, `gameWorld.MainPlayer`, `Profile.Inventory` as **possibly null** at the moment your patch fires — patches run in unexpected contexts (hideout, menu, dying frame). Check before dereferencing.
- Enable `<Nullable>enable</Nullable>` in `.csproj` (Unity Mono with `LangVersion ≥ 9` supports it for nullable annotations only — runtime null-checking is unchanged). Treat the resulting warnings as guidance, not hard errors, since Unity-side reference types are routinely non-annotated.
- Validate **at boundaries only** (config load, HTTP/IPC entry points). Do not re-validate internal invariants — trust your own code.
- Do not catch-and-swallow `Exception` to "be safe". Catch the specific type you can handle; let the rest crash and log. Silent failures in patches mask real bugs for weeks.

## 5. API surface and code shape

- Internal helpers `internal` / `private`. Only what other mods or the server side genuinely need is `public`.
- `sealed` by default for classes that aren't designed for inheritance — prevents accidental extension and helps the JIT.
- `readonly` for fields that are assigned only in the constructor. `const` only for true compile-time constants (don't use `const` for IDs that may change between EFT patches — use `static readonly`).
- Match SPT/EFT naming: `PascalCase` for types/methods/properties, `camelCase` for locals/parameters, `_camelCase` for private fields. Match the surrounding file's existing style if it differs.
- One responsibility per class. A patch class should usually contain a single patch + its helpers; a session manager should not also be doing config parsing.

## 6. Collections and concurrency

- Default to the simplest non-thread-safe collection (`List<T>`, `Dictionary<,>`). Add concurrency only when proven necessary.
- If multiple threads (e.g., HTTP handler + main thread) touch a collection, use `ConcurrentDictionary<,>` or a lock — not `lock` on `this`/`typeof(...)`. Use a private `static readonly object _gate = new();`.
- Iterating a collection while mutating it throws `InvalidOperationException`. Snapshot via `ToArray()` (acceptable when not on a hot path) or use a deferred-removal list.

## 7. Strings, IDs and serialization

- Tarkov item/trader/quest IDs are MongoDB-style `string` 24-char hex. Treat as opaque — never parse digits or assume length is stable across versions.
- Compare with `string.Equals(a, b, StringComparison.Ordinal)`. Avoid culture-sensitive comparisons (`==` is ordinal in C# for strings, but `ToLower()` is culture-sensitive — use `.ToLowerInvariant()`).
- For JSON config and SPT server interop, use the JSON library already chosen by the host (Newtonsoft on the server side; `BepInEx`/Unity may bring its own). Do not pull `System.Text.Json` into a Unity Mono target without verifying compatibility.
- File paths: build with `Path.Combine`, not string concatenation. Use forward slashes when dealing with SPT server-side configs.

## 8. Logging discipline

- One `ManualLogSource` per plugin (client) or one DI-injected logger (server). Never `Console.WriteLine` or `Debug.Log` — those bypass the SPT log infrastructure.
- Levels: `Error` only for unexpected exceptions (with stack trace). `Warning` for recoverable issues. `Info` for one-time lifecycle. `Debug`/`Message` gated by a config toggle for verbose output.
- Never log secrets, full file dumps, or per-frame state at `Info` or higher.

## 9. Build and project hygiene

- `LangVersion`: pin in the `.csproj`. Match what the Unity Mono / SPT server runtime supports (typically C# 9–11).
- Reference EFT/SPT assemblies via `<Reference>` with `Private="false"` so the DLL isn't redistributed inside the mod.
- Output target: `BepInEx/plugins/<ModName>/` for client mods, `[game]/SPT/user/mods/<ModName>/` for server mods. Keep the final folder structure mirrored under `mods/<mod>/modded/` for review.
- Nuget: minimize. Each added dependency multiplies version-clash risk with other mods (Unity's flat plugin folder).

## C# review checklist (use during `/review-technical-spec` and `/code-mod`)

1. **Disposal:** every `IDisposable` paired with `using` or an explicit Dispose in the appropriate end hook?
2. **Static state:** every static collection / event subscription has a documented clear/unsubscribe point?
3. **Hot paths:** no LINQ, no `string.Format`, no per-call `new` in patches that run per frame / per bot tick?
4. **Reflection:** all `MethodInfo`/`FieldInfo` cached in `static readonly`? Obfuscated types resolved by signature, not by `GClassNNNN` name?
5. **Threading:** Unity APIs touched only from main thread? Async paths flow `CancellationToken`? No `async void` outside event handlers?
6. **Patches:** prefix/postfix bodies wrapped in try/catch + log? No `throw` that would skip the original unintentionally?
7. **Nullability:** `Singleton<T>.Instance`, `MainPlayer`, `Profile` checked before dereference?
8. **Naming/visibility:** internals are `internal`/`private`? Public surface matches what the mod actually exposes?
9. **Strings/IDs:** ordinal comparisons? IDs treated as opaque?
10. **Logging:** levels appropriate? No per-frame `LogInfo`?

If any item fails, flag in the review or stop the build and request a `/review-technical-spec` pass.
