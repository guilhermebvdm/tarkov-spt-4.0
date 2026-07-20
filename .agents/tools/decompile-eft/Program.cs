using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;

namespace DecompileEft
{
    /// <summary>
    /// Descompila TODOS os tipos top-level de um assembly, um a um.
    ///
    /// Diferença central para o `ilspycmd -p`: ali um único método indecompilável aborta o processo
    /// e descarta namespaces inteiros SEM AVISAR (causa dos 102 namespaces vazios do dump anterior).
    /// Aqui cada tipo é independente: o que falha vira um stub `// DECOMPILE-ERROR` visível e nomeado,
    /// e o índice registra o status — buraco silencioso vira buraco auditável.
    /// </summary>
    internal static class Program
    {
        private const string AliasPrefix = "// [SPT 4.1 alias — RÓTULO (mapping comunitário), não verificado] ";

        private static int Main(string[] args)
        {
            string dll = null, outDir = null, mappings = null, only = null;
            string sptVersion = null, eftVersion = null, buildGuid = null, dllSha = null;
            int dumpVersion = 1;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--mappings":    mappings = Next(args, ref i); break;
                    case "--only":        only = Next(args, ref i); break;
                    case "--dump-version": dumpVersion = int.Parse(Next(args, ref i), CultureInfo.InvariantCulture); break;
                    case "--spt-version": sptVersion = Next(args, ref i); break;
                    case "--eft-version": eftVersion = Next(args, ref i); break;
                    case "--build-guid":  buildGuid = Next(args, ref i); break;
                    case "--dll-sha256":  dllSha = Next(args, ref i); break;
                    default:
                        if (dll == null) dll = args[i];
                        else if (outDir == null) outDir = args[i];
                        break;
                }
            }

            if (dll == null || (outDir == null && only == null))
            {
                Console.Error.WriteLine("uso: DecompileEft <assembly.dll> <outdir> [--mappings <consolidated-mappings.txt>]");
                Console.Error.WriteLine("                 [--dump-version N] [--spt-version X] [--eft-version X] [--build-guid G] [--dll-sha256 S]");
                Console.Error.WriteLine("     DecompileEft <assembly.dll> --only <FQN>     # smoke test: 1 tipo no stdout");
                return 2;
            }
            if (!File.Exists(dll)) { Console.Error.WriteLine($"❌ assembly não encontrado: {dll}"); return 2; }

            string managedDir = Path.GetDirectoryName(Path.GetFullPath(dll));
            var resolver = new UniversalAssemblyResolver(Path.GetFullPath(dll), throwOnError: false, targetFramework: null);
            resolver.AddSearchDirectory(managedDir);

            var settings = new DecompilerSettings { ThrowOnAssemblyResolveErrors = false };
            var decompiler = new CSharpDecompiler(Path.GetFullPath(dll), resolver, settings);

            // ── smoke test: um tipo, direto no stdout (comparável com `ilspycmd -t <FQN>`) ──
            if (only != null)
            {
                var target = decompiler.TypeSystem.MainModule.TopLevelTypeDefinitions
                    .FirstOrDefault(t => t.ReflectionName == only || t.FullName == only || t.Name == only);
                if (target == null) { Console.Error.WriteLine($"❌ tipo não encontrado: {only}"); return 1; }
                Console.Out.Write(decompiler.DecompileTypeAsString(target.FullTypeName));
                return 0;
            }

            var alias = LoadMappings(mappings);
            Directory.CreateDirectory(outDir);

            var entries = new List<IndexEntry>();
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int ok = 0, failed = 0, n = 0;
            var sw = Stopwatch.StartNew();

            var allTypes = decompiler.TypeSystem.MainModule.TopLevelTypeDefinitions.ToList();
            Console.WriteLine($"[*] {allTypes.Count} tipos top-level em {Path.GetFileName(dll)}");

            foreach (var type in allTypes)
            {
                n++;
                if (n % 1000 == 0) Console.WriteLine($"    {n}/{allTypes.Count} ({sw.Elapsed.TotalSeconds:F0}s)");

                string fqn = type.ReflectionName;
                string ns = type.Namespace ?? string.Empty;
                string alias41 = Lookup(alias, type.Name);

                // Layout do dump: pasta = namespace DOT-SEPARATED (ex.: "EFT.HealthSystem/"), sem
                // namespace = raiz. Mantido igual ao dump anterior para não quebrar links/docs.
                string baseName = SanitizeSegment(type.Name);
                if (type.TypeParameterCount > 0) baseName += "-" + type.TypeParameterCount.ToString(CultureInfo.InvariantCulture);

                string rel = Path.Combine(ns.Length == 0 ? "" : SanitizeSegment(ns), baseName + ".cs");
                if (!used.Add(rel))                                  // colisão após sanitizar → desambigua
                {
                    int k = 2;
                    string cand;
                    do { cand = Path.Combine(ns.Length == 0 ? "" : SanitizeSegment(ns), $"{baseName}-{k}.cs"); k++; }
                    while (!used.Add(cand));
                    rel = cand;
                }

                string full = Path.Combine(outDir, rel);
                Directory.CreateDirectory(Path.GetDirectoryName(full));

                string status;
                var body = new StringBuilder();
                if (alias41 != null) body.Append(AliasPrefix).Append(alias41).Append('\n');

                try
                {
                    body.Append(decompiler.DecompileTypeAsString(type.FullTypeName));
                    status = "ok"; ok++;
                }
                catch (Exception ex)
                {
                    // O tipo EXISTE no assembly — só o decompiler não deu conta. Deixar explícito
                    // (o modo -p engolia isto silenciosamente, junto com o resto do namespace).
                    body.Append("// DECOMPILE-ERROR: ").Append(fqn).Append(" — ")
                        .Append(ex.GetType().Name).Append(": ").Append(ex.Message.Replace('\n', ' ')).Append('\n')
                        .Append("// Este tipo EXISTE no assembly; a descompilação automática falhou.\n")
                        .Append("// Fallback: ilspycmd -t \"").Append(fqn).Append("\" \"<Assembly-CSharp.dll>\"\n");
                    status = "stub-error"; failed++;
                }

                File.WriteAllText(full, body.ToString(), new UTF8Encoding(false));
                entries.Add(new IndexEntry
                {
                    fqn = fqn,
                    status = status,
                    alias41 = alias41,
                    // path só quando NÃO é derivável de (namespace, nome) — economiza espaço no índice
                    path = IsDerivable(ns, type.Name, type.TypeParameterCount, rel) ? null : rel.Replace('\\', '/')
                });
            }

            sw.Stop();
            WriteIndex(outDir, entries, dumpVersion, sptVersion, eftVersion, buildGuid, dllSha, ok, failed, sw.Elapsed);
            WriteScaffold(outDir, dumpVersion, buildGuid);

            Console.WriteLine();
            Console.WriteLine($"[ok] {ok} ok | {failed} stub-error | {entries.Count} total | {sw.Elapsed.TotalSeconds:F0}s");
            double pct = entries.Count == 0 ? 0 : 100.0 * failed / entries.Count;
            Console.WriteLine($"  stub-error = {pct:F2}% (aceite do plano: <1% e <100 absolutos)");
            if (failed > 100 || pct > 1.0)
                Console.WriteLine("[!] ACIMA DO THRESHOLD — investigar o harness antes de publicar (GO/NO-GO do plano).");
            return 0;
        }

        private static string Next(string[] a, ref int i) => (++i < a.Length) ? a[i] : null;

        // ── mapping 4.1 ────────────────────────────────────────────────────────────────────────
        // Formato: "GClass680 -> ABotProfileCreator". Só interessam as chaves top-level (sem '+');
        // as aninhadas descrevem tipos que aqui saem inline no arquivo do pai.
        private static Dictionary<string, string> LoadMappings(string path)
        {
            var map = new Dictionary<string, string>(StringComparer.Ordinal);
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return map;
            foreach (var line in File.ReadLines(path))
            {
                if (line.Length == 0 || line[0] == '#') continue;
                int sep = line.IndexOf(" -> ", StringComparison.Ordinal);
                if (sep <= 0) continue;
                string left = line.Substring(0, sep).Trim();
                string right = line.Substring(sep + 4).Trim();
                if (left.Length == 0 || right.Length == 0 || left.IndexOf('+') >= 0) continue;
                map[left] = right;
            }
            Console.WriteLine($"[*] mapping 4.1: {map.Count} aliases top-level carregados");
            return map;
        }

        private static string Lookup(Dictionary<string, string> map, string name)
            => (name != null && map.TryGetValue(name, out var v)) ? v : null;

        // ── filename ───────────────────────────────────────────────────────────────────────────
        private static readonly char[] Invalid = Path.GetInvalidFileNameChars();
        private static readonly HashSet<string> Reserved = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { "CON","PRN","AUX","NUL","COM1","COM2","COM3","COM4","COM5","COM6","COM7","COM8","COM9",
          "LPT1","LPT2","LPT3","LPT4","LPT5","LPT6","LPT7","LPT8","LPT9" };

        /// <summary>Sanitiza um segmento de caminho: tipos gerados pelo compilador trazem `&lt;&gt;`,
        /// genéricos trazem backtick, e o NTFS recusa vários desses.</summary>
        private static string SanitizeSegment(string s)
        {
            var sb = new StringBuilder(s.Length);
            foreach (char c in s)
                sb.Append(Array.IndexOf(Invalid, c) >= 0 || c == '`' || c < 32 ? '_' : c);
            string outp = sb.ToString().TrimEnd('.', ' ');
            if (outp.Length == 0) outp = "_";
            if (Reserved.Contains(outp)) outp = "_" + outp;
            return outp.Length > 180 ? outp.Substring(0, 180) : outp;   // margem para MAX_PATH
        }

        private static bool IsDerivable(string ns, string name, int arity, string rel)
        {
            string expect = Path.Combine(ns.Length == 0 ? "" : SanitizeSegment(ns),
                SanitizeSegment(name) + (arity > 0 ? "-" + arity.ToString(CultureInfo.InvariantCulture) : "") + ".cs");
            return string.Equals(expect, rel, StringComparison.Ordinal);
        }

        // ── artefatos ──────────────────────────────────────────────────────────────────────────
        private sealed class IndexEntry
        {
            public string fqn { get; set; }
            public string status { get; set; }
            public string alias41 { get; set; }
            public string path { get; set; }
        }

        private static void WriteIndex(string outDir, List<IndexEntry> entries, int dumpVersion,
            string spt, string eft, string guid, string sha, int ok, int failed, TimeSpan elapsed)
        {
            // O índice é VERSIONADO no git (o dump não é). É ele que responde "esse tipo existe?"
            // mesmo em uma máquina sem o dump baixado — sem isso, um grep vazio viraria "não existe".
            string dest = Path.Combine(Path.GetDirectoryName(outDir.TrimEnd('\\', '/')) ?? outDir, "types-index.json");
            var doc = new Dictionary<string, object>
            {
                ["_readme"] = "Indice de TODOS os tipos do Assembly-CSharp. Ausencia AQUI = o tipo nao existe no assembly. " +
                              "Ausencia do .cs em disco NAO significa isso (o dump e gitignored; pode nao estar baixado). " +
                              "alias41 = rotulo do mapping comunitario SPT 4.1: aponta o conceito, NAO prova assinatura; " +
                              "cobre tipos, nao membros; ausencia de alias nao significa que o tipo nao exista.",
                ["dumpVersion"] = dumpVersion,
                ["buildGuid"] = guid,
                ["eftVersion"] = eft,
                ["sptVersion"] = spt,
                ["dllSha256"] = sha,
                ["generatedAtUtc"] = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture),
                ["elapsedSeconds"] = Math.Round(elapsed.TotalSeconds, 1),
                ["counts"] = new Dictionary<string, object>
                {
                    ["total"] = entries.Count,
                    ["ok"] = ok,
                    ["stubError"] = failed,
                    ["withAlias41"] = entries.Count(e => e.alias41 != null)
                },
                ["types"] = entries
            };
            File.WriteAllText(dest, JsonSerializer.Serialize(doc, new JsonSerializerOptions
            {
                WriteIndented = false,   // compacto: são ~15 mil entradas versionadas
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }), new UTF8Encoding(false));
            Console.WriteLine($"[ok] types-index.json → {dest} ({new FileInfo(dest).Length / 1024} KB)");
        }

        /// <summary>Re-cria os marcadores que o runner apaga junto com o dump antigo.</summary>
        private static void WriteScaffold(string outDir, int dumpVersion, string guid)
        {
            File.WriteAllText(Path.Combine(outDir, ".graphify_root"), "", new UTF8Encoding(false));
            File.WriteAllText(Path.Combine(outDir, ".dump-stamp.json"),
                JsonSerializer.Serialize(new Dictionary<string, object>
                {
                    ["dumpVersion"] = dumpVersion,
                    ["buildGuid"] = guid,
                    ["generatedAtUtc"] = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture)
                }), new UTF8Encoding(false));
            File.WriteAllText(Path.Combine(outDir, "Assembly-CSharp.csproj"),
                "<Project Sdk=\"Microsoft.NET.Sdk\">\n" +
                "  <!-- Projeto de NAVEGACAO (IDE), nao compilavel: referencia de leitura do cliente EFT. -->\n" +
                "  <PropertyGroup>\n" +
                "    <TargetFramework>netstandard2.1</TargetFramework>\n" +
                "    <AssemblyName>Assembly-CSharp</AssemblyName>\n" +
                "    <NoWarn>$(NoWarn);CS0169;CS0649;CS8600;CS8602</NoWarn>\n" +
                "    <EnableDefaultCompileItems>true</EnableDefaultCompileItems>\n" +
                "  </PropertyGroup>\n" +
                "</Project>\n", new UTF8Encoding(false));
        }
    }
}
