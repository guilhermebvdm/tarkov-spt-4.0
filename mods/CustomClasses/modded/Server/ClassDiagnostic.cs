namespace CustomClasses;

/// <summary>Severity of a <see cref="ClassDiagnostic"/>. Error = blocks registration; Warning/Info = informational. (Item 021)</summary>
public enum DiagnosticSeverity
{
    Error,
    Warning,
    Info,
}

/// <summary>
///     Item 021: structured diagnostic produced by <see cref="ClassRegistrar.ValidateAndBuild"/> (and by
///     <see cref="ClassEditorService"/> for file-level problems). Mirrors what the boot loader logs, so the
///     editor UI can show the SAME validation outcome the server would produce, without parsing log text.
/// </summary>
/// <param name="Severity">Error blocks registration; Warning/Info do not.</param>
/// <param name="Code">Stable machine-readable code — see <see cref="DiagnosticCodes"/>.</param>
/// <param name="Message">Human-readable message (same wording as the server log where one exists).</param>
public sealed record ClassDiagnostic(DiagnosticSeverity Severity, string Code, string Message);

/// <summary>Stable codes for <see cref="ClassDiagnostic.Code"/>. (Item 021)</summary>
public static class DiagnosticCodes
{
    /// <summary>File could not be read/deserialized (or deserialized to null).</summary>
    public const string ParseError = "ParseError";

    /// <summary>File name is not a bare *.json / *.jsonc name inside config/classes/.</summary>
    public const string InvalidFileName = "InvalidFileName";

    /// <summary>Required 'name' is missing or blank.</summary>
    public const string NameMissing = "NameMissing";

    /// <summary>'enabled' is false — class will not register (Info).</summary>
    public const string ClassDisabled = "ClassDisabled";

    /// <summary>Edition key already exists (vanilla, other mod, duplicate file — or replace not allowed).</summary>
    public const string EditionCollision = "EditionCollision";

    /// <summary>'baseEdition' not found among the profile templates.</summary>
    public const string BaseEditionNotFound = "BaseEditionNotFound";

    /// <summary>Deep clone of the base edition returned null.</summary>
    public const string CloneFailed = "CloneFailed";

    /// <summary>A side (USEC/BEAR) applied 0 skills although the class configures skills.</summary>
    public const string SideSkillsNotApplied = "SideSkillsNotApplied";

    /// <summary>Unknown skill name in 'skills' — ignored.</summary>
    public const string UnknownSkill = "UnknownSkill";

    /// <summary>Unknown skill name in 'skillMultipliers' — ignored.</summary>
    public const string UnknownMultiplierSkill = "UnknownMultiplierSkill";

    /// <summary>Multiplier targets a Skills-Extended skill but SE is not installed (registered anyway, inert).</summary>
    public const string SkillsExtendedMissing = "SkillsExtendedMissing";

    /// <summary>Serializing the class definition back to JSON failed.</summary>
    public const string SerializeFailed = "SerializeFailed";

    /// <summary>New class name rejected (empty, or colliding with an existing edition/class file name). (Item 027)</summary>
    public const string InvalidClassName = "InvalidClassName";

    /// <summary>Two or more class FILES declare the same 'name' (case-insensitive, trimmed) — at boot only the first (alphabetical) registers. (CR-EP-06)</summary>
    public const string DuplicateClassName = "DuplicateClassName";
}
