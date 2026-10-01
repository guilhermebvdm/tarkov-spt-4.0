using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace PatchDryRun
{
    /// <summary>
    /// Runs the ORBIT patch start-up against the real SPT 4.0 game assemblies, outside the game.
    ///
    /// The mod's own code runs unmodified: every ModulePatch.Enable(), the manual patch sets and the
    /// reflection bindings. The single thing replaced is Harmony.Patch itself: Unity's native calls cannot be
    /// compiled outside the game process, so instead of detouring the game method the harness checks what
    /// Harmony would check when it builds the detour:
    ///   - the target method was resolved (GetTargetMethod / AccessTools did not return null or throw);
    ///   - every prefix / postfix / finalizer parameter binds: __instance, __result, ___field and the
    ///     parameters matched by name (same rules as HarmonyManipulator.EmitCallParameter in HarmonyX 2.9);
    ///   - every transpiler runs over the target's real IL without throwing (ORBIT's transpilers throw when
    ///     the call they guard is not found exactly once).
    /// What it cannot prove: behaviour in a raid.
    ///
    /// usage: PatchDryRun.exe &lt;spt root&gt; &lt;build dir with ORBIT.dll [and Orbit.Fika.dll]&gt;
    /// exit code 0 = no failure, 1 = at least one failure, 2 = the harness itself could not run.
    /// </summary>
    internal static class Program
    {
        private static readonly List<string> Dirs = new List<string>();
        private static readonly List<string> Captured = new List<string>();
        private static readonly List<string> Intercepted = new List<string>();
        private static readonly List<string> Problems = new List<string>();
        private static int _failures;
        private static int _patchCalls;

        private static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine("usage: PatchDryRun <spt root> <build dir>");
                return 2;
            }
            var spt = Path.GetFullPath(args[0]);
            var build = Path.GetFullPath(args[1]);
            var stage = Path.Combine(Path.GetTempPath(), "orbit-dryrun-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(stage);
            Dirs.Add(stage);
            Dirs.Add(Path.Combine(spt, "EscapeFromTarkov_Data", "Managed"));
            Dirs.Add(Path.Combine(spt, "BepInEx", "core"));
            Dirs.Add(Path.Combine(spt, "BepInEx", "plugins", "spt"));
            Dirs.Add(Path.Combine(spt, "BepInEx", "plugins"));
            foreach (var sub in new[] { "Fika", "SAIN", "DrakiaXYZ-Waypoints", "MoreBotsAPI" })
                Dirs.Add(Path.Combine(spt, "BepInEx", "plugins", sub));
            AppDomain.CurrentDomain.AssemblyResolve += Resolve;

            try
            {
                return Run(spt, build, stage);
            }
            catch (Exception e)
            {
                Console.WriteLine("HARNESS ERROR: " + Flatten(e));
                return 2;
            }
            finally
            {
                try { Directory.Delete(stage, true); } catch { /* files stay locked until exit */ }
            }
        }

        private static Assembly Resolve(object sender, ResolveEventArgs e)
        {
            var name = new AssemblyName(e.Name).Name;
            foreach (var dir in Dirs)
            {
                var path = Path.Combine(dir, name + ".dll");
                if (File.Exists(path)) return Assembly.LoadFrom(path);
            }
            return null;
        }

        private static int Run(string spt, string build, string stage)
        {
            var staged = Stage(build, stage);
            var orbit = Assembly.LoadFrom(staged);
            Console.WriteLine($"ORBIT {orbit.GetName().Version} from {build}");
            Console.WriteLine($"SPT install: {spt}");
            foreach (var probe in new[] { "Assembly-CSharp", "spt-reflection", "0Harmony", "DrakiaXYZ-BigBrain", "SAIN", "Fika.Core" })
            {
                var asm = TryLoad(probe);
                Console.WriteLine($"  {probe,-20} {(asm == null ? "NOT FOUND" : asm.GetName().Version + "  " + asm.Location)}");
            }

            ReportUnloadableGameTypes();

            SetupLogging(orbit);
            var own = new Harmony("orbit.dryrun.interceptor");
            // In the game (Mono) Assembly-CSharp.GetTypes() returns every type. On the desktop CLR a few types
            // cannot load (listed above), so GetTypes() throws; hand the mod the types that did load instead.
            own.Patch(typeof(object).Assembly.GetType("System.Reflection.RuntimeAssembly", true).GetMethod("GetTypes", Type.EmptyTypes),
                finalizer: new HarmonyMethod(typeof(Program).GetMethod(nameof(TolerateTypeLoad), BindingFlags.Static | BindingFlags.NonPublic)));
            own.Patch(
                typeof(Harmony).GetMethod("Patch", new[] { typeof(MethodBase), typeof(HarmonyMethod), typeof(HarmonyMethod), typeof(HarmonyMethod), typeof(HarmonyMethod), typeof(HarmonyMethod) }),
                new HarmonyMethod(typeof(Program).GetMethod(nameof(InterceptPatch), BindingFlags.Static | BindingFlags.NonPublic)));

            Section("1. ModulePatch classes (ORBIT.dll) — the patches Plugin.DelayedLoad enables with EnableSafe");
            var modulePatch = TryLoad("spt-reflection").GetType("SPT.Reflection.Patching.ModulePatch", true);
            var enable = modulePatch.GetMethod("Enable", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
            var target = modulePatch.GetProperty("TargetMethod");
            var patches = SafeTypes(orbit).Where(t => !t.IsAbstract && modulePatch.IsAssignableFrom(t)).OrderBy(t => t.FullName).ToList();
            var notEnabled = 0;
            foreach (var type in patches)
            {
                Problems.Clear();
                var before = _patchCalls;
                // A patch class can say it does not apply to this game version; the plugin then never enables it.
                var applies = type.GetProperty("Applies", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (applies != null && applies.PropertyType == typeof(bool) && !(bool)applies.GetValue(null, null))
                {
                    notEnabled++;
                    Console.WriteLine($"  N/A   {type.Name,-36} Applies == false on this install: the plugin does not enable it");
                    continue;
                }
                try
                {
                    var instance = Activator.CreateInstance(type, true);
                    enable.Invoke(instance, null);
                    var resolved = (MethodBase)target.GetValue(instance);
                    if (Problems.Count > 0) Fail(type.Name, Describe(resolved) + "  ::  " + string.Join(" | ", Problems));
                    else if (_patchCalls == before) Fail(type.Name, "Enable() returned without patching anything");
                    else Ok(type.Name, Describe(resolved));
                }
                catch (Exception e)
                {
                    Fail(type.Name, Flatten(e));
                }
            }
            Console.WriteLine($"  -- {patches.Count} ModulePatch classes, {notEnabled} not enabled on this install");

            Section("2. Manual Harmony patch sets (static Enable(), each reports through its Ready flag)");
            foreach (var name in new[]
                     {
                         "Orbit.Patches.NativeGhostBodyPatches",
                         "Orbit.Patches.SanitarPatrolMedicinePatch",
                         "Orbit.Patches.LootPatrolResumePatch",
                     })
                RunReady(orbit, name, "Enable");

            Section("3. Reflection bindings resolved once at start-up");
            Probe(orbit, "Orbit.Sain.SainPersonality", "InitIfNeeded");

            Section("3a. Bindings into optional mods (checked when the mod is installed, listed as absent otherwise)");
            LoadOptionalPlugins(spt);
            ProbeOptionalMods(orbit);

            var fikaPath = Path.Combine(stage, "Orbit.Fika.dll");
            var fika = File.Exists(fikaPath) ? Assembly.LoadFrom(fikaPath) : null;

            Section("3b. Reflection handles kept in static readonly fields (FieldInfo, MethodInfo, compiled readers)");
            Console.WriteLine("  A null handle means the member name was not found: the mod then takes its fallback path for that feature.");
            ScanStaticHandles(orbit);
            if (fika != null) ScanStaticHandles(fika);

            if (fika != null)
            {
                Section("4. Orbit.Fika — door synchronisation (FikaPlayer / ObservedPlayer)");
                try
                {
                    var receiver = fika.GetType("Orbit.Fika.DoorStateReceiver", true);
                    var supported = (bool)receiver.GetProperty("Supported", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).GetValue(null);
                    if (supported) Ok("DoorStateReceiver.Supported", "WorldInteractiveObject._interaction, Break and ResultState found");
                    else Fail("DoorStateReceiver.Supported", "false: the door interaction fields were not found");
                }
                catch (Exception e)
                {
                    Fail("DoorStateReceiver.Supported", Flatten(e));
                }
                Problems.Clear();
                var before = _patchCalls;
                try
                {
                    var bridge = fika.GetType("Orbit.Fika.DoorSyncBridge", true);
                    var ctor = bridge.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).First();
                    ctor.Invoke(new object[] { Logger.CreateLogSource("Orbit.Fika-dryrun") });
                    if (Problems.Count > 0) Fail("DoorSyncBridge", string.Join(" | ", Problems));
                    else Ok("DoorSyncBridge", $"{_patchCalls - before} postfixes bound");
                }
                catch (Exception e)
                {
                    Fail("DoorSyncBridge", Flatten(e));
                }
            }

            Section("3c. Fika accessors compiled by the client plugin (Helpers/GhostSpectatorPlayers)");
            ProbeFikaAccessors(orbit);

            // The mod reports a binding it could not make through its own log. Any warning or error it wrote
            // during this run is a feature it turned off, so it counts as a failure here.
            if (Captured.Count > 0) Section("Warnings and errors the mod itself logged during the run");
            foreach (var line in Captured.Distinct())
                Fail("mod log", line);

            Section("Every Harmony.Patch call the mod made");
            foreach (var line in Intercepted) Console.WriteLine("  " + line);
            Console.WriteLine($"  -- {_patchCalls} calls");

            Console.WriteLine();
            Console.WriteLine(_failures == 0 ? "RESULT: OK — no failure" : $"RESULT: {_failures} FAILURE(S)");
            return _failures == 0 ? 0 : 1;
        }

        // ---------------------------------------------------------------- staging

        // Orbit.Log prefixes each line with UnityEngine.Time.frameCount, a native call the CLR refuses to
        // compile outside the game ("ECall methods must be packaged into a system module"). The staged copy
        // replaces that single call with the constant 0, in class Orbit.Log only; nothing else is changed.
        private static string Stage(string build, string stage)
        {
            var resolver = new DefaultAssemblyResolver();
            foreach (var dir in Dirs.Skip(1)) if (Directory.Exists(dir)) resolver.AddSearchDirectory(dir);
            resolver.AddSearchDirectory(build);
            var output = Path.Combine(stage, "ORBIT.dll");
            using (var module = ModuleDefinition.ReadModule(Path.Combine(build, "ORBIT.dll"), new ReaderParameters { AssemblyResolver = resolver }))
            {
                var log = module.GetType("Orbit.Log") ?? throw new InvalidOperationException("Orbit.Log not found in ORBIT.dll");
                var replaced = 0;
                foreach (var method in log.Methods.Where(m => m.HasBody))
                    foreach (var ins in method.Body.Instructions)
                        if (ins.OpCode == OpCodes.Call && ins.Operand is MethodReference mr
                            && mr.DeclaringType.FullName == "UnityEngine.Time" && mr.Name == "get_frameCount")
                        {
                            ins.OpCode = OpCodes.Ldc_I4_0;
                            ins.Operand = null;
                            replaced++;
                        }
                module.Write(output);
                Console.WriteLine($"staged ORBIT.dll: {replaced} Time.frameCount call(s) in Orbit.Log replaced by 0");
            }
            StageGameAssembly(stage, resolver);
            // Keep the JIT from inlining across the staged assemblies so each mod method is compiled on its own.
            const string ini = "[.NET Framework Debugging Control]\r\nGenerateTrackingInfo=1\r\nAllowOptimize=0\r\n";
            File.WriteAllText(Path.Combine(stage, "ORBIT.ini"), ini);
            var fika = Path.Combine(build, "Orbit.Fika.dll");
            if (File.Exists(fika))
            {
                File.Copy(fika, Path.Combine(stage, "Orbit.Fika.dll"), true);
                File.WriteAllText(Path.Combine(stage, "Orbit.Fika.ini"), ini);
            }
            return output;
        }

        // The obfuscated game assembly declares delegate types that are not sealed. Mono (the game's runtime)
        // loads them; the desktop CLR refuses ("delegate classes must be sealed"), which makes
        // Assembly.GetTypes() throw — and ORBIT calls it. The staged copy only sets the sealed flag on those
        // delegate types; no method, field or name is touched.
        private static void StageGameAssembly(string stage, DefaultAssemblyResolver resolver)
        {
            var source = Dirs.Skip(1).Select(d => Path.Combine(d, "Assembly-CSharp.dll")).First(File.Exists);
            using (var module = ModuleDefinition.ReadModule(source, new ReaderParameters { AssemblyResolver = resolver }))
            {
                var fixedCount = 0;
                foreach (var type in module.GetTypes())
                    if (!type.IsSealed && type.BaseType?.FullName == "System.MulticastDelegate")
                    {
                        type.IsSealed = true;
                        fixedCount++;
                    }
                module.Write(Path.Combine(stage, "Assembly-CSharp.dll"));
                Console.WriteLine($"staged Assembly-CSharp.dll: {fixedCount} delegate type(s) marked sealed so the desktop CLR can load them");
            }
        }

        private static void SetupLogging(Assembly orbit)
        {
            Logger.Listeners.Add(new Listener());
            var plugin = orbit.GetType("Orbit.Plugin", true);
            plugin.GetField("LogSource").SetValue(null, Logger.CreateLogSource("ORBIT"));
            var cfg = new ConfigFile(Path.Combine(Path.GetTempPath(), "orbit-dryrun-" + Guid.NewGuid().ToString("N") + ".cfg"), false);
            plugin.GetField("QuietLogging").SetValue(null, cfg.Bind("dryrun", "Quiet logging", false));
            plugin.GetField("PerfLogging").SetValue(null, cfg.Bind("dryrun", "Performance logging", false));
        }

        private sealed class Listener : ILogListener
        {
            public void LogEvent(object sender, LogEventArgs e)
            {
                if (e.Source.SourceName != "ORBIT" && !e.Source.SourceName.StartsWith("Orbit", StringComparison.Ordinal)) return;
                var loud = (e.Level & (LogLevel.Warning | LogLevel.Error | LogLevel.Fatal)) != 0;
                Console.WriteLine($"      [mod {e.Level}] {e.Data}");
                if (loud) Captured.Add($"[{e.Level}] {e.Data}");
            }

            public void Dispose() { }
        }

        private static Exception TolerateTypeLoad(Exception __exception, ref Type[] __result)
        {
            if (!(__exception is ReflectionTypeLoadException e)) return __exception;
            __result = e.Types.Where(t => t != null).ToArray();
            return null;
        }

        private static void ReportUnloadableGameTypes()
        {
            try
            {
                Console.WriteLine($"  Assembly-CSharp.GetTypes(): all {TryLoad("Assembly-CSharp").GetTypes().Length} types load");
            }
            catch (ReflectionTypeLoadException e)
            {
                var names = e.LoaderExceptions.OfType<TypeLoadException>().Select(x => x.TypeName).Where(n => !string.IsNullOrEmpty(n)).Distinct().OrderBy(n => n).ToList();
                Console.WriteLine($"  Assembly-CSharp.GetTypes(): {e.Types.Count(t => t != null)} types load, {e.Types.Count(t => t == null)} do not on the desktop CLR " +
                                  "(explicit-layout structs, default interface methods, the game's re-signed Newtonsoft.Json). They are skipped; in the game all load.");
                Console.WriteLine("      not loadable here: " + string.Join(", ", names.Take(60)) + (names.Count > 60 ? ", ..." : ""));
            }
        }

        // ---------------------------------------------------------------- Harmony.Patch interception

        private static bool InterceptPatch(Harmony __instance, MethodBase original, HarmonyMethod prefix, HarmonyMethod postfix,
            HarmonyMethod transpiler, HarmonyMethod finalizer, HarmonyMethod ilmanipulator, ref MethodInfo __result)
        {
            if (__instance.Id == "orbit.dryrun.interceptor") return true;
            _patchCalls++;
            __result = null;
            if (original == null)
            {
                Problem($"[{__instance.Id}] Harmony.Patch received a null target (the AccessTools lookup found nothing)");
                Intercepted.Add($"NULL TARGET  via {__instance.Id}");
                // Harmony itself throws here; keep that behaviour so the mod's own error handling runs.
                throw new NullReferenceException("Null method for " + __instance.Id);
            }
            var kinds = new List<string>();
            Check("prefix", original, prefix, kinds);
            Check("postfix", original, postfix, kinds);
            Check("finalizer", original, finalizer, kinds);
            if (transpiler?.method != null)
            {
                kinds.Add("transpiler " + transpiler.method.DeclaringType?.Name + "." + transpiler.method.Name);
                var error = RunTranspiler(original, transpiler.method);
                if (error != null)
                {
                    Problem($"transpiler {transpiler.method.Name} on {Describe(original)}: {error}");
                    Intercepted.Add($"{Describe(original)}  <=  {string.Join(", ", kinds)}");
                    throw new InvalidOperationException(error);
                }
            }
            if (ilmanipulator?.method != null) kinds.Add("ilmanipulator (not checked)");
            Intercepted.Add($"{Describe(original)}  <=  {string.Join(", ", kinds)}");
            return false;
        }

        private static void Check(string kind, MethodBase original, HarmonyMethod patch, List<string> kinds)
        {
            if (patch?.method == null) return;
            kinds.Add(kind + " " + patch.method.DeclaringType?.Name + "." + patch.method.Name);
            foreach (var problem in Bind(original, patch.method))
                Problem($"{kind} {patch.method.DeclaringType?.Name}.{patch.method.Name} on {Describe(original)}: {problem}");
        }

        // Mirrors HarmonyManipulator.EmitCallParameter (HarmonyX 2.9): the conditions under which Harmony
        // throws while building the detour, in the same order.
        private static IEnumerable<string> Bind(MethodBase original, MethodInfo patch)
        {
            var parameters = original.GetParameters();
            var names = parameters.Select(p => p.Name).ToArray();
            var returned = original is MethodInfo mi ? mi.ReturnType : typeof(void);
            foreach (var p in patch.GetParameters())
            {
                switch (p.Name)
                {
                    case "__originalMethod":
                    case "__runOriginal":
                    case "__state":
                    case "__exception":
                        continue;
                    case "__args":
                        if (p.ParameterType != typeof(object[])) yield return "__args must be object[]";
                        continue;
                    case "__instance":
                        if (original.IsStatic) yield return "__instance used on a static target (Harmony passes null)";
                        else if (!Compatible(p.ParameterType, original.DeclaringType))
                            yield return $"__instance is {p.ParameterType.Name} but the target is declared on {original.DeclaringType?.Name}";
                        continue;
                    case "__result":
                        if (returned == typeof(void)) yield return "Cannot get result from void method";
                        else
                        {
                            var type = p.ParameterType.IsByRef ? p.ParameterType.GetElementType() : p.ParameterType;
                            if (!type.IsAssignableFrom(returned))
                                yield return $"Cannot assign method return type {returned.FullName} to __result type {type.FullName}";
                        }
                        continue;
                }
                if (p.Name.StartsWith("___", StringComparison.Ordinal))
                {
                    var field = p.Name.Substring(3);
                    if (field.All(char.IsDigit)) continue;
                    if (AccessTools.Field(original.DeclaringType, field) == null)
                        yield return $"No such field defined in class {original.DeclaringType?.FullName}: {field}";
                    continue;
                }
                if (p.Name.StartsWith("__", StringComparison.Ordinal) && int.TryParse(p.Name.Substring(2), out var index))
                {
                    if (index < 0 || index >= parameters.Length) yield return $"No parameter found at index {index}";
                    continue;
                }
                var mapped = p.GetCustomAttributes(typeof(HarmonyArgument), false).Cast<HarmonyArgument>().FirstOrDefault();
                var wanted = mapped?.OriginalName ?? p.Name;
                var at = mapped != null && mapped.Index >= 0 ? mapped.Index : Array.IndexOf(names, wanted);
                if (at < 0 || at >= parameters.Length)
                {
                    yield return $"Parameter \"{p.Name}\" not found in method (target parameters: {string.Join(", ", names)})";
                    continue;
                }
                var want = p.ParameterType.IsByRef ? p.ParameterType.GetElementType() : p.ParameterType;
                var have = parameters[at].ParameterType.IsByRef ? parameters[at].ParameterType.GetElementType() : parameters[at].ParameterType;
                if (!Compatible(want, have))
                    yield return $"Parameter \"{p.Name}\" is {want.Name} in the patch but {have.Name} in the target";
            }
        }

        private static bool Compatible(Type patchSide, Type targetSide)
            => patchSide == typeof(object) || targetSide == null || patchSide.IsAssignableFrom(targetSide) || targetSide.IsAssignableFrom(patchSide);

        private static string RunTranspiler(MethodBase original, MethodInfo transpiler)
        {
            try
            {
                var instructions = PatchProcessor.GetOriginalInstructions(original, out var generator);
                var args = transpiler.GetParameters().Select(p =>
                    p.ParameterType.IsAssignableFrom(typeof(List<CodeInstruction>)) ? instructions
                    : p.ParameterType == typeof(ILGenerator) ? generator
                    : typeof(MethodBase).IsAssignableFrom(p.ParameterType) ? (object)original
                    : null).ToArray();
                var result = (IEnumerable<CodeInstruction>)transpiler.Invoke(null, args);
                var count = result.Count();
                return count == 0 ? "transpiler returned no instructions" : null;
            }
            catch (Exception e)
            {
                return Flatten(e);
            }
        }

        // ---------------------------------------------------------------- sections 2 and 3

        private static void RunReady(Assembly asm, string typeName, string method)
        {
            var type = asm.GetType(typeName);
            if (type == null) { Fail(typeName, "type not found in ORBIT.dll"); return; }
            Problems.Clear();
            var before = _patchCalls;
            try
            {
                type.GetMethod(method, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).Invoke(null, null);
                var ready = type.GetProperty("Ready", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if (ready != null && !(bool)ready.GetValue(null))
                    Fail(type.Name, "Ready == false after Enable(): the mod disabled this feature itself — see its [mod Warning] line");
                else if (Problems.Count > 0) Fail(type.Name, string.Join(" | ", Problems));
                else Ok(type.Name, $"Ready, {_patchCalls - before} Harmony.Patch calls bound");
            }
            catch (Exception e)
            {
                Fail(type.Name, Flatten(e));
            }
        }

        private static void Probe(Assembly asm, string typeName, string method)
        {
            var type = asm.GetType(typeName);
            if (type == null) { Fail(typeName, "type not found in ORBIT.dll"); return; }
            try
            {
                var m = type.GetMethod(method, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if (m == null) { Fail(type.Name, method + " not found"); return; }
                m.Invoke(null, null);
                Console.WriteLine($"  RAN   {type.Name}.{method}() — static fields it filled:");
                foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).OrderBy(f => f.Name))
                {
                    if (field.IsLiteral) continue;
                    var kind = field.FieldType;
                    if (!typeof(MemberInfo).IsAssignableFrom(kind) && !typeof(Delegate).IsAssignableFrom(kind) && kind != typeof(bool)) continue;
                    var value = field.GetValue(null);
                    var text = value == null ? "NULL" : value is bool b ? b.ToString() : value is MemberInfo mi ? (mi.DeclaringType != null ? mi.DeclaringType.FullName + "." : "") + mi.Name : "bound";
                    Console.WriteLine($"        {field.Name,-34} {text}");
                }
            }
            catch (Exception e)
            {
                Fail(type.Name + "." + method, Flatten(e));
            }
        }

        // Null handles that are expected on this install, with the reason. Anything else that is null fails.
        private static readonly Dictionary<string, string> ExpectedNull = new Dictionary<string, string>();

        // Runs the static initialiser of every mod type that keeps a reflection handle in a static readonly
        // field and reports the handles left null. Types whose initialiser needs the Unity runtime are listed
        // as not checkable instead of failing.
        private static void ScanStaticHandles(Assembly asm)
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            int ok = 0, bad = 0, skipped = 0;
            foreach (var type in SafeTypes(asm).OrderBy(t => t.FullName))
            {
                if (type.IsGenericTypeDefinition || type.Name.StartsWith("<", StringComparison.Ordinal)) continue;
                var handles = type.GetFields(flags).Where(f => f.IsInitOnly && !f.IsLiteral && IsHandle(f.FieldType)).ToList();
                if (handles.Count == 0) continue;
                try
                {
                    System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);
                }
                catch (Exception e)
                {
                    skipped++;
                    Console.WriteLine($"  SKIP  {type.Name,-36} static initialiser cannot run outside the game: {Flatten(e)}");
                    continue;
                }
                foreach (var field in handles)
                {
                    object value;
                    try { value = field.GetValue(null); }
                    catch (Exception e) { Console.WriteLine($"  SKIP  {type.Name}.{field.Name}: {Flatten(e)}"); skipped++; continue; }
                    var key = type.Name + "." + field.Name;
                    if (value != null) { ok++; continue; }
                    if (ExpectedNull.TryGetValue(key, out var why)) { Console.WriteLine($"  NULL  {key,-52} expected: {why}"); continue; }
                    bad++;
                    Fail(key, "null: the member this handle looks up by name does not exist on this install");
                }
            }
            Console.WriteLine($"  -- {asm.GetName().Name}: {ok} handles resolved, {bad} null, {skipped} not checkable here");
        }

        private static bool IsHandle(Type t)
            => typeof(MemberInfo).IsAssignableFrom(t) || typeof(Delegate).IsAssignableFrom(t);

        // ---------------------------------------------------------------- optional mods

        // Mods ORBIT integrates with through reflection. They are optional; when one is installed its DLL is
        // loaded so the type lookups (AccessTools.TypeByName) can find it, as they would in the game.
        private static readonly Regex OptionalPlugin = new Regex(
            "UNTARGH|MoreBots|RUAF|ISB|BlackDiv|RoguesVRaiders|InterchangeRework|Manimal[.](Interchange|Lighthouse)|CombineSoldiers", RegexOptions.IgnoreCase);

        private static void LoadOptionalPlugins(string spt)
        {
            var plugins = Path.Combine(spt, "BepInEx", "plugins");
            foreach (var file in Directory.GetFiles(plugins, "*.dll", SearchOption.AllDirectories).OrderBy(f => f))
            {
                if (!OptionalPlugin.IsMatch(Path.GetFileName(file))) continue;
                try
                {
                    var asm = Assembly.LoadFrom(file);
                    Dirs.Add(Path.GetDirectoryName(file));
                    Console.WriteLine($"  loaded optional mod {asm.GetName().Name} {asm.GetName().Version}  ({file.Substring(plugins.Length + 1)})");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"  could not load {Path.GetFileName(file)}: {Flatten(e)}");
                }
            }
        }

        private static void ProbeOptionalMods(Assembly orbit)
        {
            // Checkpoint adapters (UNTAR / RUAF / ISB): one binding object per mod, five members each.
            var adapters = orbit.GetType("Orbit.Systems.NativeGhostAdapters", true);
            RunQuiet(adapters, "ResolveBindings");
            foreach (var pair in new[] { new[] { "_untar", "UNTAR" }, new[] { "_ruaf", "RUAF Come Home" }, new[] { "_isb", "ISB" } })
            {
                var binding = StaticField(adapters, pair[0]);
                var what = "NativeGhostAdapters " + pair[1];
                if (binding == null) { Fail(what, "binding object was not created"); continue; }
                if (InstanceField(binding, "Type") == null) { Absent(what); continue; }
                var missing = new[] { "Available", "Point", "Choose", "Set" }.Where(n => InstanceField(binding, n) == null).ToList();
                if (missing.Count > 0) Fail(what, "mod installed but these members did not bind: " + string.Join(", ", missing));
                else Ok(what, "CanDoCheckpointActions, guardPoint, GetCheckpointCoverPoint, SetGuardPoint, guardPointDirty bound");
            }
            if (AccessTools.TypeByName("RoguesVRaiders.SquadRegistry") == null) Absent("NativeGhostAdapters RoguesVRaiders");
            else CheckStatics(adapters, "NativeGhostAdapters RoguesVRaiders", "_rvrMember", "_rvrBoard", "_rvrOrders");

            // MoreBotsAPI hunt manager: regroup recovery and the per-tick hunt update.
            var regroup = orbit.GetType("Orbit.Systems.NativeGhostRegroup", true);
            RunQuiet(regroup, "Resolve", new object[] { null });
            if (StaticField(regroup, "_type") == null) Absent("NativeGhostRegroup MoreBotsAPI");
            else CheckStatics(regroup, "NativeGhostRegroup MoreBotsAPI", "_owner", "_active", "_regroup", "_regrouping", "_ignore", "_dirty", "_point", "_choose", "_setPoint");

            var system = orbit.GetType("Orbit.Systems.NativeGhostSystem", true);
            RunQuiet(system, "HuntUpdater", new object[] { null });
            if (StaticField(system, "_huntType") == null) Absent("NativeGhostSystem MoreBotsAPI hunt");
            else CheckStatics(system, "NativeGhostSystem MoreBotsAPI hunt", "_huntActive", "_huntUpdate");
        }

        // GhostSpectatorPlayers compiles its Fika accessors in its constructor, from the plugin instance BepInEx
        // registered. Outside the game there is no Chainloader, so an empty FikaPlugin object is registered first.
        private static void ProbeFikaAccessors(Assembly orbit)
        {
            const string what = "GhostSpectatorPlayers";
            var fikaCore = TryLoad("Fika.Core");
            var pluginType = fikaCore?.GetType("Fika.Core.FikaPlugin");
            if (pluginType == null) { Absent(what + " (Fika.Core)"); return; }
            try
            {
                var info = (PluginInfo)Activator.CreateInstance(typeof(PluginInfo), true);
                typeof(PluginInfo).GetProperty("Instance").SetValue(info, FormatterServices.GetUninitializedObject(pluginType), null);
                Chainloader.PluginInfos["com.fika.core"] = info;
                var type = orbit.GetType("Orbit.Helpers.GhostSpectatorPlayers", true);
                var instance = Activator.CreateInstance(type, true);
                var missing = new[] { "_handler", "_humans", "_extracted", "_netId", "_headless" }.Where(n => InstanceField(instance, n) == null).ToList();
                if (missing.Count > 0) Fail(what, "accessors not compiled: " + string.Join(", ", missing));
                else Ok(what, "IFikaNetworkManager.CoopHandler, HumanPlayers, ExtractedPlayers, FikaPlayer.NetId, FikaBackendUtils.IsHeadless bound");
            }
            catch (Exception e)
            {
                Console.WriteLine($"  SKIP  {what,-36} cannot be exercised outside the game: {Flatten(e)}");
            }
        }

        private const BindingFlags Any = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static object StaticField(Type type, string name) => type.GetField(name, Any)?.GetValue(null);

        private static object InstanceField(object target, string name) => target.GetType().GetField(name, Any)?.GetValue(target);

        // Resolvers take a bot and go on to use it; only the binding they do first matters here.
        private static void RunQuiet(Type type, string method, object[] args = null)
        {
            var m = type.GetMethod(method, Any);
            if (m == null) { Fail(type.Name + "." + method, "method not found in ORBIT.dll"); return; }
            try { m.Invoke(null, args); }
            catch (TargetInvocationException e) when (e.InnerException is NullReferenceException) { /* no bot outside the game */ }
        }

        private static void CheckStatics(Type type, string what, params string[] fields)
        {
            var missing = fields.Where(f => StaticField(type, f) == null).ToList();
            if (missing.Count > 0) Fail(what, "mod installed but these members did not bind: " + string.Join(", ", missing));
            else Ok(what, fields.Length + " members bound");
        }

        private static void Absent(string what) => Console.WriteLine($"  ABSENT {what,-35} mod not installed here: binding not checkable");

        // ---------------------------------------------------------------- helpers

        private static IEnumerable<Type> SafeTypes(Assembly asm)
        {
            try { return asm.GetTypes(); }
            catch (ReflectionTypeLoadException e)
            {
                foreach (var le in e.LoaderExceptions.Select(x => x.Message).Distinct().Take(10))
                    Console.WriteLine("  type load problem: " + le);
                return e.Types.Where(t => t != null);
            }
        }

        private static Assembly TryLoad(string name)
        {
            try { return Assembly.Load(name); }
            catch { return null; }
        }

        private static string Describe(MethodBase m)
        {
            if (m == null) return "null";
            var ps = string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name));
            return $"{m.DeclaringType?.FullName}.{m.Name}({ps})";
        }

        private static string Flatten(Exception e)
        {
            var parts = new List<string>();
            for (var x = e; x != null; x = x.InnerException)
            {
                if (x is TargetInvocationException) continue;
                parts.Add(x.GetType().Name + ": " + x.Message.Replace("\r", " ").Replace("\n", " "));
            }
            return string.Join("  <=  ", parts.Distinct());
        }

        private static void Problem(string text) => Problems.Add(text);

        private static void Section(string title)
        {
            Console.WriteLine();
            Console.WriteLine("== " + title);
        }

        private static void Ok(string what, string detail) => Console.WriteLine($"  OK    {what,-36} {detail}");

        private static void Fail(string what, string detail)
        {
            _failures++;
            Console.WriteLine($"  FAIL  {what,-36} {detail}");
        }
    }
}
