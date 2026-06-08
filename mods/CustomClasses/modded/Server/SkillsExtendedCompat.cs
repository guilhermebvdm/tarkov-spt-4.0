using SPTarkov.Server.Core.Models.Spt.Mod;   // SptMod

namespace CustomClasses;

/// <summary>
///     Item 006: soft-detect do Skills-Extended (sem dependência hard) + conjunto de skills que ele "revive".
///     Essas skills são membros de SkillTypes/ESkillId vanilla (ref: SkillTypes.cs:42,43,66,70), mas só
///     ganham XP / aparecem na tela com o SE instalado. Sem o SE, um multiplicador pra elas é inócuo.
/// </summary>
public static class SkillsExtendedCompat
{
    public const string ModGuid = "com.cj.SkillsExtended";   // ref: SE Metadata.cs:16

    /// <summary>Skills revividas pelo SE (nomes de SkillTypes).</summary>
    public static readonly HashSet<string> Skills =
        new(StringComparer.OrdinalIgnoreCase) { "FirstAid", "FieldMedicine", "BearRawpower", "UsecNegotiations" };

    /// <summary>True se o SE está nos mods carregados. Padrão idêntico ao SE p/ Fika (ConfigController.cs:29).</summary>
    public static bool IsPresent(IReadOnlyList<SptMod> loadedMods) =>
        loadedMods.Any(m => string.Equals(m.ModMetadata?.ModGuid, ModGuid, StringComparison.Ordinal));
}
