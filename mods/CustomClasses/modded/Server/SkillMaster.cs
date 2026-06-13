using SPTarkov.Server.Core.Models.Enums;   // SkillTypes

namespace CustomClasses;

/// <summary>The four EFT UI category tabs plus the Skills-Extended "Special Elite" section.</summary>
public enum SkillCategory
{
    Physical,
    Mental,
    Combat,
    Practical,
    SpecialElite,
}

/// <summary>One skill in canonical position. Color/label derive from <see cref="Category"/>.</summary>
public sealed record SkillMasterEntry(SkillTypes Skill, string Name, SkillCategory Category);

/// <summary>
///     Item 031: canonical skill order for the editor UI (port of the RZ viewer's SKILL_MASTER,
///     tools/tarkov-itemdb/viewer/profiles.js:10-45). Every live skill always renders in the SAME
///     fixed position — Physical → Mental → Combat → Practical, then a final "Special Elite" section
///     for the four Skills-Extended skills — so switching classes keeps each skill visually pinned.
///
///     NO MAGIC NUMBERS: the list and its count are DERIVED from <see cref="SkillWeights.Explicit"/>
///     (the 31 live skills ported from RZ) plus <see cref="SkillsExtendedCompat.Skills"/> (the 4 SE
///     skills). The category bucketing of each skill comes from <see cref="SkillWeights.Categories"/>.
///     The only thing hardcoded here is the ORDER of the four main categories and the order of the
///     four Special Elite skills — premissa P5: <see cref="Dictionary{TKey,TValue}"/> enumeration
///     preserves insertion order in practice but is NOT a contract, so the per-category order is taken
///     from a stable explicit sequence copied from the Explicit/SKILL_MASTER declaration order. The
///     total count stays derived (<c>Entries.Count</c>), never written.
/// </summary>
public static class SkillMaster
{
    // Category colors ported from the RZ viewer (profiles.css:170-205). Combat reuses the mod's
    // accent (#c8a35a — the fallback used across the rest of the editor). Special Elite gets a
    // neutral purple distinct from the four existing colors (premissa P6 — cosmetic, adjustable).
    private const string ColorPhysical = "#c87c50";
    private const string ColorMental = "#7090c8";
    private const string ColorCombat = "#c8a35a";
    private const string ColorPractical = "#6e9a3f";
    private const string ColorSpecialElite = "#9a6ec8";

    /// <summary>
    ///     Fixed order of the four main categories. The per-category skill order is taken from the
    ///     declaration order of <see cref="SkillWeights.Explicit"/> (already Ph→M→C→P, 1:1 with the
    ///     viewer's SKILL_MASTER), filtered by category and excluding the Special Elite set.
    /// </summary>
    private static readonly SkillCategory[] MainCategoryOrder =
        [SkillCategory.Physical, SkillCategory.Mental, SkillCategory.Combat, SkillCategory.Practical];

    /// <summary>
    ///     Special Elite skills (the SE-revived four), in the fixed order shown in the kickoff's
    ///     "Skills-Extended" section. Membership is decided by <see cref="SkillsExtendedCompat.Skills"/>
    ///     — independent of how <see cref="SkillWeights.Categories"/> happens to bucket them (premissa P4:
    ///     FirstAid/FieldMedicine sit under "P", the faction passives under "S"; that does not matter here).
    /// </summary>
    private static readonly SkillTypes[] SpecialEliteOrder =
        [SkillTypes.FirstAid, SkillTypes.FieldMedicine, SkillTypes.UsecNegotiations, SkillTypes.BearRawpower];

    /// <summary>
    ///     All canonical skills in fixed order (Ph→M→C→P, then Special Elite). Count/list are derived
    ///     from <see cref="SkillWeights"/> + <see cref="SkillsExtendedCompat"/> — no magic numbers.
    /// </summary>
    public static IReadOnlyList<SkillMasterEntry> Entries { get; } = BuildEntries();

    /// <summary>Hex color per category (see the const block; mirrors the RZ viewer palette).</summary>
    public static string ColorOf(SkillCategory category) => category switch
    {
        SkillCategory.Physical => ColorPhysical,
        SkillCategory.Mental => ColorMental,
        SkillCategory.Combat => ColorCombat,
        SkillCategory.Practical => ColorPractical,
        SkillCategory.SpecialElite => ColorSpecialElite,
        _ => ColorCombat,
    };

    /// <summary>Section label shown above each category block in the canonical list.</summary>
    public static string LabelOf(SkillCategory category) => category switch
    {
        SkillCategory.Physical => "Physical (Ph)",
        SkillCategory.Mental => "Mental (M)",
        SkillCategory.Combat => "Combat (C)",
        SkillCategory.Practical => "Practical (P)",
        SkillCategory.SpecialElite => "Special Elite",
        _ => category.ToString(),
    };

    private static List<SkillMasterEntry> BuildEntries()
    {
        // The Special Elite set is the SE-revived skills, matched by SkillTypes name (case-insensitive).
        var specialElite = SpecialEliteOrder
            .Where(s => SkillsExtendedCompat.Skills.Contains(s.ToString()))
            .ToList();
        var specialEliteSet = new HashSet<SkillTypes>(specialElite);

        var entries = new List<SkillMasterEntry>();

        // Main categories in fixed order. Within each, walk SkillWeights.Explicit in declaration
        // order (Ph→M→C→P, 1:1 with the viewer), keep skills bucketed into that category by
        // SkillWeights.Categories, and exclude anything claimed by the Special Elite section.
        foreach (var category in MainCategoryOrder)
        {
            var tag = CategoryTag(category);
            foreach (var skill in SkillWeights.Explicit.Keys)
            {
                if (specialEliteSet.Contains(skill))
                {
                    continue;
                }

                if (SkillWeights.Categories.TryGetValue(skill, out var skillTag)
                    && string.Equals(skillTag, tag, StringComparison.Ordinal))
                {
                    entries.Add(new SkillMasterEntry(skill, skill.ToString(), category));
                }
            }
        }

        // Special Elite last, in the fixed declared order.
        foreach (var skill in specialElite)
        {
            entries.Add(new SkillMasterEntry(skill, skill.ToString(), SkillCategory.SpecialElite));
        }

        return entries;
    }

    /// <summary>Maps a <see cref="SkillCategory"/> to the short tag used in <see cref="SkillWeights.Categories"/>.</summary>
    private static string CategoryTag(SkillCategory category) => category switch
    {
        SkillCategory.Physical => "Ph",
        SkillCategory.Mental => "M",
        SkillCategory.Combat => "C",
        SkillCategory.Practical => "P",
        _ => string.Empty,
    };
}
