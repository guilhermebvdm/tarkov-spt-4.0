using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;                                 // JsonDocument (item 027: cheap profile scan)
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;                     // ModHelper
using SPTarkov.Server.Core.Services;                    // DatabaseService
using SPTarkov.Server.Core.Utils;                       // FileUtil, JsonUtil
using SPTarkov.Server.Core.Models.Utils;                // ISptLogger

namespace CustomClasses;

/// <summary>One class file as seen by the editor: parsed definition + dry-run diagnostics + state flags. (Item 021)</summary>
/// <param name="FileName">Bare file name inside config/classes/ (e.g. "cacador.jsonc").</param>
/// <param name="Definition">Parsed definition, or null when the file could not be parsed.</param>
/// <param name="Enabled">def.enabled (false when unparseable).</param>
/// <param name="Registered">True when the class's edition key is currently in the profile templates.</param>
/// <param name="Diagnostics">Parse + dry-run (ValidateAndBuild, allowReplace=true) diagnostics.</param>
public sealed record ClassFileEntry(
    string FileName,
    ClassDefinition? Definition,
    bool Enabled,
    bool Registered,
    List<ClassDiagnostic> Diagnostics);

/// <summary>Outcome of <see cref="ClassEditorService.Save"/>. (Item 021)</summary>
/// <param name="Success">True when the file was written (warnings allowed; any Error blocks the write).</param>
/// <param name="Diagnostics">Validation/IO diagnostics.</param>
public sealed record SaveResult(bool Success, List<ClassDiagnostic> Diagnostics);

/// <summary>Outcome of <see cref="ClassEditorService.Create"/> / <see cref="ClassEditorService.Duplicate"/>. (Item 027)</summary>
/// <param name="Success">True when the new class file was written (and hot-applied).</param>
/// <param name="FileName">Bare file name of the new class file (null on failure).</param>
/// <param name="Diagnostics">Name-validation + save diagnostics.</param>
public sealed record CreateResult(bool Success, string? FileName, List<ClassDiagnostic> Diagnostics);

/// <summary>
///     Item 021: load/save service for the class files in the mod's install folder
///     (<c>user/mods/CustomClasses/config/classes/</c>, resolved via ModHelper.GetAbsolutePathToModFolder).
///     Every save runs the SAME validation pipeline as boot (<see cref="ClassRegistrar.ValidateAndBuild"/>,
///     dry-run, allowReplace=true) — an Error blocks the write entirely.
///     <para>
///     Save flow: validate/build → rotating backup (<c>&lt;file&gt;.bak1..bak3</c>, shifted) → write indented
///     JSON → optional hot-apply (<see cref="ClassRegistrar.Commit"/>; <c>enabled:false</c> hot-applies as
///     <see cref="ClassRegistrar.Remove"/>) → audit line in <c>config/classes/_audit.log</c> (not *.json, so
///     the boot loader never picks it up).
///     </para>
///     <para>
///     KNOWN LOSSES / CAVEATS:
///     (1) JSONC comments are LOST on save — the definition is re-serialized from the DTO; <c>.bak1</c>
///     preserves the last manual state. Formatting/key order is normalized too (defaults like
///     <c>"premium": false</c> may appear explicitly).
///     (2) Rename: if the saved definition's <c>name</c> differs from the name previously stored in the same
///     file, the OLD edition stays registered — the caller must call <see cref="ClassRegistrar.Remove"/> for
///     the old name (the service cannot guess intent: rename vs. duplicate).
///     (3) Concurrency: no file locking; concurrent saves from two editor tabs are accepted as a non-goal
///     (local single-user server — item 021 kickoff).
///     </para>
///     Driver: exercised by item 024 (class viewer) via ListClassFiles; until then validated through the
///     020 smoke page / manual call (see item 021 as-built).
/// </summary>
[Injectable(InjectionType.Singleton)]
public class ClassEditorService(
    ModHelper modHelper,
    FileUtil fileUtil,
    JsonUtil jsonUtil,
    DatabaseService databaseService,
    ClassRegistrar classRegistrar,
    ISptLogger<ClassEditorService> logger
)
{
    private const int MaxBackups = 3;
    private const string AuditFileName = "_audit.log";   // NOT *.json/*.jsonc — invisible to the boot loader

    private string? _classesPath;

    /// <summary>config/classes/ inside the installed mod folder (same resolution as CustomClassesMod.OnLoad).</summary>
    private string ClassesPath => _classesPath ??= System.IO.Path.Combine(   // System.IO.Path: evita ambiguidade com ...Tables.Path
        modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly()),   // ref: ModHelper.cs:10
        "config", "classes");

    /// <summary>
    ///     Scans config/classes/ (non-recursive, *.json + *.jsonc — same globs as boot) and returns one
    ///     entry per file with parse + dry-run diagnostics. The dry-run uses allowReplace=true so a class
    ///     that is already registered (by its own file) does not produce a false collision Error.
    ///     Note: the dry-run logs the same warnings boot would (registrar + builders log directly).
    /// </summary>
    public List<ClassFileEntry> ListClassFiles()
    {
        var entries = new List<ClassFileEntry>();
        if (!fileUtil.DirectoryExists(ClassesPath))   // ref: FileUtil.cs:48
        {
            return entries;
        }

        var files = fileUtil.GetFiles(ClassesPath, false, "*.json")   // ref: FileUtil.cs:11
            .Concat(fileUtil.GetFiles(ClassesPath, false, "*.jsonc"))
            .Distinct()
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)   // deterministic UI order
            .ToList();

        var templates = databaseService.GetProfileTemplates();   // ref: DatabaseService.cs:141

        foreach (var file in files)
        {
            var fileName = fileUtil.GetFileNameAndExtension(file);   // ref: FileUtil.cs:33
            var diagnostics = new List<ClassDiagnostic>();
            var def = ParseFile(file, fileName, diagnostics);

            var registered = false;
            if (def is not null)
            {
                registered = !string.IsNullOrWhiteSpace(def.Name) && templates.ContainsKey(def.Name!.Trim());
                _ = classRegistrar.ValidateAndBuild(def, fileName, allowReplace: true, out var dryRun);
                diagnostics.AddRange(dryRun);
            }

            entries.Add(new ClassFileEntry(fileName, def, def?.Enabled ?? false, registered, diagnostics));
        }

        // CR-EP-06: two files claiming the same `name` — the per-file dry-run (allowReplace=true) cannot
        // see the sibling file, so it passes clean while boot registers only the FIRST (alphabetical) and
        // skips the rest with EditionCollision. Surface the collision as an Error on EVERY involved file.
        var collisions = entries
            .Where(e => !string.IsNullOrWhiteSpace(e.Definition?.Name))
            .GroupBy(e => e.Definition!.Name!.Trim(), StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1);
        foreach (var group in collisions)
        {
            var involved = string.Join(", ", group.Select(e => e.FileName));
            foreach (var entry in group)
            {
                entry.Diagnostics.Add(new ClassDiagnostic(DiagnosticSeverity.Error, DiagnosticCodes.DuplicateClassName,
                    $"'{entry.FileName}': class name '{group.Key}' is declared by {group.Count()} files ({involved}) — "
                    + "at boot only the first (alphabetical) registers; the others are skipped (EditionCollision). "
                    + "Rename or delete the duplicates."));
            }
        }

        return entries;
    }

    /// <summary>
    ///     Loads and parses one class file. Returns null (with diagnostics) on bad file name, missing file
    ///     or parse failure. Parse-only: run ValidateAndBuild / ListClassFiles for full validation.
    /// </summary>
    public ClassDefinition? Load(string fileName, out List<ClassDiagnostic> diagnostics)
    {
        diagnostics = [];
        if (!TryResolveClassFile(fileName, diagnostics, out var fullPath))
        {
            return null;
        }

        if (!fileUtil.FileExists(fullPath))   // ref: FileUtil.cs:58
        {
            diagnostics.Add(new ClassDiagnostic(DiagnosticSeverity.Error, DiagnosticCodes.ParseError,
                $"'{fileName}': file not found."));
            return null;
        }

        return ParseFile(fullPath, fileName, diagnostics);
    }

    /// <summary>
    ///     Validates, backs up, writes and (optionally) hot-applies one class definition.
    ///     Any Error diagnostic aborts BEFORE anything is written. With <paramref name="hotApply"/>:
    ///     enabled class → <see cref="ClassRegistrar.Commit"/> (replaces the live edition, build-then-swap;
    ///     new profiles pick it up immediately — ref: ProfileHelper.cs:806 reads the live dict);
    ///     <c>enabled:false</c> → <see cref="ClassRegistrar.Remove"/>. See class doc for the rename caveat.
    /// </summary>
    public SaveResult Save(string fileName, ClassDefinition def, bool hotApply)
    {
        var diagnostics = new List<ClassDiagnostic>();
        if (!TryResolveClassFile(fileName, diagnostics, out var fullPath))
        {
            return new SaveResult(false, diagnostics);
        }

        // Dry-run first — nothing is written when a blocking problem exists.
        var plan = classRegistrar.ValidateAndBuild(def, fileName, allowReplace: true, out var validation);
        diagnostics.AddRange(validation);
        if (plan is null)
        {
            return new SaveResult(false, diagnostics);
        }

        // Serialize before touching the disk so a serializer failure leaves everything intact.
        // NOTE: comments from hand-edited .jsonc are lost here (re-serialization) — .bak1 keeps them.
        var json = jsonUtil.Serialize(def, indented: true);   // ref: JsonUtil.cs:174
        if (json is null)
        {
            diagnostics.Add(new ClassDiagnostic(DiagnosticSeverity.Error, DiagnosticCodes.SerializeFailed,
                $"'{fileName}': serializing the class definition returned null."));
            return new SaveResult(false, diagnostics);
        }

        RotateBackups(fullPath);
        fileUtil.WriteFile(fullPath, json + "\n");   // ref: FileUtil.cs:78 (trailing newline: POSIX-friendly diffs)

        if (hotApply)
        {
            if (def.Enabled)
            {
                classRegistrar.Commit(plan);
            }
            else
            {
                classRegistrar.Remove(plan.Name);   // kickoff: enabled:false hot-applies as removal
            }
        }

        Audit(fileName, "save", $"name='{plan.Name}', enabled={def.Enabled}, hotApply={hotApply}");
        return new SaveResult(true, diagnostics);
    }

    /// <summary>
    ///     Backs up and deletes one class file. With <paramref name="hotRemove"/> the class's edition is
    ///     also unregistered (parsed from the file before deletion; an unparseable file deletes without
    ///     hot-remove). Returns false when the file name is invalid or the file does not exist.
    /// </summary>
    public bool Delete(string fileName, bool hotRemove)
    {
        var diagnostics = new List<ClassDiagnostic>();
        if (!TryResolveClassFile(fileName, diagnostics, out var fullPath) || !fileUtil.FileExists(fullPath))
        {
            return false;
        }

        // Best-effort: grab the edition name before the file disappears.
        string? name = null;
        try
        {
            name = jsonUtil.Deserialize<ClassDefinition>(fileUtil.ReadFile(fullPath))?.Name?.Trim();
        }
        catch (Exception ex)
        {
            logger.Warning($"[CustomClasses] Delete('{fileName}'): unparseable file — deleting without hot-remove. {ex.Message}");
        }

        RotateBackups(fullPath);   // .bak1 = last content, recoverable
        fileUtil.DeleteFile(fullPath);   // ref: FileUtil.cs:160

        var editionRemoved = false;
        if (hotRemove && !string.IsNullOrWhiteSpace(name))
        {
            editionRemoved = classRegistrar.Remove(name!);
        }

        Audit(fileName, "delete", $"name='{name ?? "?"}', hotRemove={hotRemove}, editionRemoved={editionRemoved}");
        return true;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Lifecycle (item 027) — create / duplicate / profile-usage scan
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Item 027: every name a NEW class must not collide with — live profile-template keys (vanilla +
    ///     hidden + already-registered classes) UNION the <c>name</c> of every class FILE on disk (a disabled
    ///     or invalid-but-parseable class is not in the templates, yet creating a second file with its name
    ///     would blow up at the next boot). Case-INSENSITIVE on purpose — stricter than the registrar's
    ///     ordinal collision check, because two editions differing only by case are a footgun, not a feature.
    /// </summary>
    public HashSet<string> ExistingEditionNames()
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var key in databaseService.GetProfileTemplates().Keys)   // ref: DatabaseService.cs:141
        {
            names.Add(key);
        }

        if (!fileUtil.DirectoryExists(ClassesPath))
        {
            return names;
        }

        var files = fileUtil.GetFiles(ClassesPath, false, "*.json")
            .Concat(fileUtil.GetFiles(ClassesPath, false, "*.jsonc"))
            .Distinct();
        foreach (var file in files)
        {
            try
            {
                var name = jsonUtil.Deserialize<ClassDefinition>(fileUtil.ReadFile(file))?.Name?.Trim();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    names.Add(name!);
                }
            }
            catch
            {
                // Unparseable file cannot claim a name — skip (it still shows as Invalid in the list).
            }
        }

        return names;
    }

    /// <summary>
    ///     Item 027: validates a candidate name for a NEW class. Returns a human-readable error or null when
    ///     the name is usable. Pass a snapshot from <see cref="ExistingEditionNames"/> for cheap live
    ///     (per-keystroke) validation in dialogs; without it the snapshot is taken here.
    ///     Any character is allowed in the name itself (editions like "Caçador" exist) — file-name safety is
    ///     handled by the slug in <see cref="Create"/>/<see cref="Duplicate"/>, never by restricting the name.
    /// </summary>
    public string? ValidateNewClassName(string? name, IReadOnlySet<string>? existingNames = null)
    {
        var trimmed = name?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return "Name is required.";
        }

        var existing = existingNames ?? ExistingEditionNames();
        if (existing.Contains(trimmed!))
        {
            return $"'{trimmed}' collides with an existing edition or class (vanilla editions included).";
        }

        return null;
    }

    /// <summary>
    ///     Item 027: creates a new class file from the minimal valid template (name + explicit default
    ///     baseEdition + enabled + displayName/description {en,pt} = name) and hot-applies it — the edition
    ///     shows up in the launcher without a restart. The file name is a collision-free ASCII slug of the
    ///     name (<c>&lt;slug&gt;.jsonc</c>). Returns the new file name or the blocking diagnostics.
    /// </summary>
    public CreateResult Create(string name)
    {
        var diagnostics = new List<ClassDiagnostic>();
        var error = ValidateNewClassName(name);
        if (error is not null)
        {
            diagnostics.Add(new ClassDiagnostic(DiagnosticSeverity.Error, DiagnosticCodes.InvalidClassName, error));
            return new CreateResult(false, null, diagnostics);
        }

        var trimmed = name.Trim();
        var def = new ClassDefinition
        {
            Name = trimmed,
            DisplayName = new LocalizedText { En = trimmed, Pt = trimmed },
            Description = new LocalizedText { En = trimmed, Pt = trimmed },
            Enabled = true,
            BaseEdition = ClassRegistrar.DefaultBaseEdition,   // explicit in the file — self-documenting
        };

        var fileName = UniqueClassFileName(Slugify(trimmed));
        var save = Save(fileName, def, hotApply: true);
        diagnostics.AddRange(save.Diagnostics);
        if (!save.Success)
        {
            return new CreateResult(false, null, diagnostics);
        }

        Audit(fileName, "create", $"name='{trimmed}'");
        return new CreateResult(true, fileName, diagnostics);
    }

    /// <summary>
    ///     Item 027: copies an existing class file under a new name — the official "rename" path (renaming
    ///     in place would orphan profiles created from the old edition). Everything except <c>name</c> and
    ///     <c>displayName</c> (set to the new name, en+pt) is carried over verbatim: skills, multipliers,
    ///     loadout, outfit, hideout, description, iconFile, nameColor, enabled. Hot-applied on save.
    /// </summary>
    public CreateResult Duplicate(string sourceFileName, string newName)
    {
        var diagnostics = new List<ClassDiagnostic>();
        var source = Load(sourceFileName, out var loadDiagnostics);
        diagnostics.AddRange(loadDiagnostics);
        if (source is null)
        {
            return new CreateResult(false, null, diagnostics);
        }

        var error = ValidateNewClassName(newName);
        if (error is not null)
        {
            diagnostics.Add(new ClassDiagnostic(DiagnosticSeverity.Error, DiagnosticCodes.InvalidClassName, error));
            return new CreateResult(false, null, diagnostics);
        }

        var trimmed = newName.Trim();
        var def = source with
        {
            Name = trimmed,
            DisplayName = new LocalizedText { En = trimmed, Pt = trimmed },
        };

        var fileName = UniqueClassFileName(Slugify(trimmed));
        var save = Save(fileName, def, hotApply: true);
        diagnostics.AddRange(save.Diagnostics);
        if (!save.Success)
        {
            return new CreateResult(false, null, diagnostics);
        }

        Audit(fileName, "duplicate", $"source='{sourceFileName}', name='{trimmed}'");
        return new CreateResult(true, fileName, diagnostics);
    }

    /// <summary>
    ///     Item 027: labels ("username (file.json)") of the user profiles created from the given edition —
    ///     fed to the delete-confirmation dialog. Scans <c>user/profiles/*.json</c> (resolved from the mod's
    ///     install folder: <c>user/mods/CustomClasses → ../../profiles</c>) reading only <c>info.edition</c> /
    ///     <c>info.username</c> via JsonDocument (profiles are MB-sized — no full deserialization).
    ///     Unreadable/unparseable profiles are skipped with a log warning. Edition match is ORDINAL — the
    ///     launcher writes the edition key verbatim (evidence: D:/SPT/SPT/user/profiles, field info.edition).
    /// </summary>
    public List<string> ProfilesUsingEdition(string editionName)
    {
        var result = new List<string>();
        var profilesPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(
            modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly()),   // ref: ModHelper.cs:10
            "..", "..", "profiles"));
        if (!fileUtil.DirectoryExists(profilesPath))
        {
            logger.Warning($"[CustomClasses] ProfilesUsingEdition: profile folder not found at '{profilesPath}'.");
            return result;
        }

        foreach (var file in fileUtil.GetFiles(profilesPath, false, "*.json"))   // ref: FileUtil.cs:11
        {
            try
            {
                using var stream = System.IO.File.OpenRead(file);
                using var doc = JsonDocument.Parse(stream);
                if (!doc.RootElement.TryGetProperty("info", out var info)
                    || !info.TryGetProperty("edition", out var edition)
                    || edition.ValueKind != JsonValueKind.String
                    || !string.Equals(edition.GetString(), editionName, StringComparison.Ordinal))
                {
                    continue;
                }

                var fileName = fileUtil.GetFileNameAndExtension(file);   // ref: FileUtil.cs:33
                var username = info.TryGetProperty("username", out var u) && u.ValueKind == JsonValueKind.String
                    ? u.GetString()
                    : null;
                result.Add(string.IsNullOrWhiteSpace(username) ? fileName : $"{username} ({fileName})");
            }
            catch (Exception ex)
            {
                logger.Warning($"[CustomClasses] ProfilesUsingEdition: skipping unreadable profile '{file}': {ex.Message}");
            }
        }

        return result;
    }

    /// <summary>
    ///     ASCII slug for a new file name: diacritics folded (FormD, marks dropped), lowercased,
    ///     anything outside [a-z0-9] collapsed to single '-', trimmed. Empty result → "class".
    /// </summary>
    private static string Slugify(string name)
    {
        var normalized = name.Trim().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
            {
                continue;   // drop the combining accent ("ç" → "c", "ã" → "a")
            }

            var lower = char.ToLowerInvariant(ch);
            if (lower is >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                sb.Append(lower);
            }
            else if (sb.Length > 0 && sb[^1] != '-')
            {
                sb.Append('-');
            }
        }

        var slug = sb.ToString().Trim('-');
        return slug.Length == 0 ? "class" : slug;
    }

    /// <summary>
    ///     First free "&lt;slug&gt;.jsonc" in config/classes/ — "-2", "-3", … appended on collision.
    ///     Checks BOTH extensions so "x.jsonc" is never created next to an existing "x.json".
    /// </summary>
    private string UniqueClassFileName(string slug)
    {
        for (var i = 1; ; i++)
        {
            var stem = i == 1 ? slug : $"{slug}-{i}";
            if (!fileUtil.FileExists(System.IO.Path.Combine(ClassesPath, stem + ".jsonc"))
                && !fileUtil.FileExists(System.IO.Path.Combine(ClassesPath, stem + ".json")))
            {
                return stem + ".jsonc";
            }
        }
    }

    /// <summary>Reads + deserializes one file; failures become a ParseError diagnostic.</summary>
    private ClassDefinition? ParseFile(string fullPath, string fileName, List<ClassDiagnostic> diagnostics)
    {
        try
        {
            var def = jsonUtil.Deserialize<ClassDefinition>(fileUtil.ReadFile(fullPath));   // ref: JsonUtil.cs:47
            if (def is null)
            {
                diagnostics.Add(new ClassDiagnostic(DiagnosticSeverity.Error, DiagnosticCodes.ParseError,
                    $"'{fileName}': file is empty or deserialized to null."));
            }

            return def;
        }
        catch (Exception ex)
        {
            diagnostics.Add(new ClassDiagnostic(DiagnosticSeverity.Error, DiagnosticCodes.ParseError,
                $"'{fileName}': failed to parse — {ex.Message}"));
            return null;
        }
    }

    /// <summary>
    ///     Accepts only a bare *.json/*.jsonc file name (no path separators / traversal) — this service will
    ///     be fed from HTTP routes (item 024+), so the file-name boundary is validated here.
    /// </summary>
    private bool TryResolveClassFile(string fileName, List<ClassDiagnostic> diagnostics, out string fullPath)
    {
        fullPath = string.Empty;

        var isBareName = !string.IsNullOrWhiteSpace(fileName)
            && string.Equals(fileName, System.IO.Path.GetFileName(fileName), StringComparison.Ordinal);
        var ext = isBareName ? System.IO.Path.GetExtension(fileName) : string.Empty;
        var isClassExt = string.Equals(ext, ".json", StringComparison.OrdinalIgnoreCase)
            || string.Equals(ext, ".jsonc", StringComparison.OrdinalIgnoreCase);

        if (!isBareName || !isClassExt)
        {
            diagnostics.Add(new ClassDiagnostic(DiagnosticSeverity.Error, DiagnosticCodes.InvalidFileName,
                $"'{fileName}': expected a bare *.json/*.jsonc file name inside config/classes/."));
            return false;
        }

        fullPath = System.IO.Path.Combine(ClassesPath, fileName);
        return true;
    }

    /// <summary>Shifts &lt;file&gt;.bak1→bak2→bak3 (oldest dropped) and snapshots the current file as .bak1.</summary>
    private void RotateBackups(string fullPath)
    {
        if (!fileUtil.FileExists(fullPath))
        {
            return;   // new file — nothing to back up
        }

        var oldest = $"{fullPath}.bak{MaxBackups}";
        if (fileUtil.FileExists(oldest))
        {
            fileUtil.DeleteFile(oldest);
        }

        for (var i = MaxBackups - 1; i >= 1; i--)
        {
            var src = $"{fullPath}.bak{i}";
            if (fileUtil.FileExists(src))
            {
                System.IO.File.Move(src, $"{fullPath}.bak{i + 1}");   // FileUtil has no Move — direct BCL call
            }
        }

        fileUtil.CopyFile(fullPath, $"{fullPath}.bak1", overwrite: true);   // ref: FileUtil.cs:177
    }

    /// <summary>
    ///     Appends one tab-separated audit line (UTC). Audit failures never fail the operation.
    ///     CR2-EP-11: user-controlled values (class names inside <paramref name="summary"/>, bare file
    ///     names) are sanitized so embedded newlines/tabs can never forge audit lines or break the TSV.
    /// </summary>
    private void Audit(string fileName, string action, string summary)
    {
        static string Sanitize(string value) =>
            value.Replace("\r", " ").Replace("\n", " ").Replace("\t", " ");

        try
        {
            var line = $"{DateTime.UtcNow:yyyy-MM-dd'T'HH:mm:ss'Z'}\t{Sanitize(fileName)}\t{action}\t{Sanitize(summary)}\n";
            System.IO.File.AppendAllText(System.IO.Path.Combine(ClassesPath, AuditFileName), line);
        }
        catch (Exception ex)
        {
            logger.Warning($"[CustomClasses] audit log write failed ({AuditFileName}): {ex.Message}");
        }
    }
}
