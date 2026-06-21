using SPTarkov.Server.Core.Models.Enums;   // SkillTypes

namespace CustomClasses;

/// <summary>How a skill's cost weight was obtained (never a silent 0 — see <see cref="SkillWeights.ResolveWeight"/>).</summary>
public enum SkillWeightOrigin
{
    /// <summary>1:1 port of RZCustomProfiles SKILL_MULTS (31 live skills, reference char lvl 43).</summary>
    Explicit,

    /// <summary>Derived from the Skills-Extended XP mechanics (4 skills revived by SE).</summary>
    Derived,

    /// <summary>Skill has a known category but no weight — median of that category's explicit weights.</summary>
    CategoryFallback,

    /// <summary>Skill outside every map — neutral 1.00, flagged so the caller can warn.</summary>
    UnmappedFallback,
}

/// <summary>
///     Item 022: weighted-cost table for class skills. Port of the RZCustomProfiles model
///     (mods/RZCustomProfiles/scripts/build-profile-jsons.js SKILL_MULTS + the formula doc
///     002-custom-profiles-00-multiplicadores.md): weight = BASELINE(15) / expected level on the
///     lvl-43 reference character, clamped to [0.25, 5.00], 2 decimals. Cost = Σ level × weight.
///     The 4 Skills-Extended skills get weights derived from the SE source (see <see cref="Derived"/>).
///     Any other skill falls back to its category median — NEVER a silent 0.
/// </summary>
public static class SkillWeights
{
    /// <summary>Median level of neutral skills on the reference char (Assault/Endurance = 15). RZ doc §Fórmula.</summary>
    public const double Baseline = 15.0;

    public const double MinWeight = 0.25;
    public const double MaxWeight = 5.00;

    /// <summary>Weighted-cost budget target per class (RZ validateProfile: [28, 32]).</summary>
    public const double BudgetMin = 28.0;
    public const double BudgetMax = 32.0;

    /// <summary>Informative design rules ported from RZ (non-blocking warnings in CostService).</summary>
    public const int MaxSkillsWithPoints = 6;
    public const int SuggestedLevelCap = 10;

    /// <summary>
    ///     1:1 port of RZ SKILL_MULTS (31 live skills in SPT 4.0.13). Keep in sync with
    ///     mods/CustomClasses/scripts/check-skill-costs.mjs (parity script copies these values).
    /// </summary>
    public static readonly IReadOnlyDictionary<SkillTypes, double> Explicit = new Dictionary<SkillTypes, double>
    {
        // Ph — Physical
        { SkillTypes.Endurance, 1.00 },
        { SkillTypes.Strength, 0.47 },
        { SkillTypes.Vitality, 1.67 },
        { SkillTypes.Health, 1.67 },
        { SkillTypes.StressResistance, 0.88 },
        { SkillTypes.Metabolism, 0.29 },
        { SkillTypes.Immunity, 3.75 },
        // M — Mental
        { SkillTypes.Perception, 0.88 },
        { SkillTypes.Intellect, 0.68 },
        { SkillTypes.Attention, 0.60 },
        { SkillTypes.Charisma, 0.40 },
        { SkillTypes.Memory, 0.50 },
        // C — Combat
        { SkillTypes.Pistol, 3.00 },
        { SkillTypes.Revolver, 0.71 },
        { SkillTypes.Assault, 1.00 },
        { SkillTypes.Shotgun, 2.50 },
        { SkillTypes.Sniper, 1.50 },
        { SkillTypes.DMR, 3.75 },
        { SkillTypes.Throwing, 0.83 },
        { SkillTypes.Melee, 1.88 },
        { SkillTypes.RecoilControl, 1.00 },
        { SkillTypes.AimDrills, 1.15 },
        { SkillTypes.TroubleShooting, 2.50 },
        // P — Practical
        { SkillTypes.Surgery, 1.25 },
        { SkillTypes.CovertMovement, 0.94 },
        { SkillTypes.Search, 0.43 },
        { SkillTypes.MagDrills, 0.94 },
        { SkillTypes.LightVests, 3.75 },
        { SkillTypes.HeavyVests, 3.75 },
        { SkillTypes.WeaponTreatment, 1.25 },
        { SkillTypes.Crafting, 0.33 },
        { SkillTypes.HideoutManagement, 0.39 },
    };

    /// <summary>
    ///     Weights derived from the Skills-Extended XP mechanics (vendored source, default SkillsConfig.json),
    ///     applying the same RZ formula: weight = 15 / expected level at the lvl-43 reference char.
    ///     Skill level is linear in XP (100 progress per level — ref: ProfileHelper.cs:460, CustomClassesMod.ApplySkills),
    ///     so expected level ≈ (actions over the career × XP per action × action factor) / 100.
    ///     Career assumption: ~400 raids to reach lvl 43 (same reference character as the RZ table).
    ///
    ///     FirstAid     — 6.25 XP per healing-item effect (MedEffect: medkits/bandages/splints —
    ///                    SE OnGameStarted.ApplyMedicalXp) × action factor 0.35 (SkillClassCtorPatch)
    ///                    ≈ 2.19 XP/use → ~46 uses/level. Healing is very frequent (~2/raid → ~800 uses)
    ///                    → expected level ≈ 16 (CovertMovement-like cadence) → 15/16 = 0.94.
    ///     FieldMedicine— 5.50 XP per stim/painkiller effect (SE ApplyMedicalXp, Stimulator/PainKiller)
    ///                    × factor 0.35 ≈ 1.93 XP/use → ~52 uses/level. Stims/painkillers are clearly
    ///                    less frequent than heals (~1/raid → ~400 uses) → expected level ≈ 8 → 15/8 = 1.88.
    ///     UsecNegotiations — passive faction skill (USEC only, FactionLocked): 2.25 XP per BEAR PMC kill
    ///                    (SE OnEnemyKillPatch) × factor 1.0 → ~44 kills/level. Opposite-faction PMC kills
    ///                    are rare (~300 over the career) → expected level ≈ 6 → 15/6 = 2.50.
    ///     BearRawpower — symmetric passive (BEAR only, 2.25 XP per USEC PMC kill, factor 1.0) —
    ///                    same cadence as UsecNegotiations → expected level ≈ 6 → 15/6 = 2.50.
    /// </summary>
    public static readonly IReadOnlyDictionary<SkillTypes, double> Derived = new Dictionary<SkillTypes, double>
    {
        { SkillTypes.FirstAid, 0.94 },
        { SkillTypes.FieldMedicine, 1.88 },
        { SkillTypes.UsecNegotiations, 2.50 },
        { SkillTypes.BearRawpower, 2.50 },
    };

    /// <summary>
    ///     Category per skill (EFT UI tabs: Ph/M/C/P; "S" = faction/special — outside the 4-category
    ///     coverage rule). Includes the RZ live-skill map, the 4 SE skills, and the dead skills the RZ
    ///     doc categorizes (so a future revival lands on a sane CategoryFallback instead of 1.00).
    /// </summary>
    public static readonly IReadOnlyDictionary<SkillTypes, string> Categories = new Dictionary<SkillTypes, string>
    {
        // Ph — Physical (RZ map)
        { SkillTypes.Endurance, "Ph" }, { SkillTypes.Strength, "Ph" }, { SkillTypes.Vitality, "Ph" },
        { SkillTypes.Health, "Ph" }, { SkillTypes.StressResistance, "Ph" }, { SkillTypes.Metabolism, "Ph" },
        { SkillTypes.Immunity, "Ph" },
        // M — Mental (RZ map)
        { SkillTypes.Perception, "M" }, { SkillTypes.Intellect, "M" }, { SkillTypes.Attention, "M" },
        { SkillTypes.Charisma, "M" }, { SkillTypes.Memory, "M" },
        // C — Combat (RZ map + dead combat skills from the RZ doc)
        { SkillTypes.Pistol, "C" }, { SkillTypes.Revolver, "C" }, { SkillTypes.Assault, "C" },
        { SkillTypes.Shotgun, "C" }, { SkillTypes.Sniper, "C" }, { SkillTypes.DMR, "C" },
        { SkillTypes.Throwing, "C" }, { SkillTypes.Melee, "C" }, { SkillTypes.RecoilControl, "C" },
        { SkillTypes.AimDrills, "C" }, { SkillTypes.TroubleShooting, "C" },
        { SkillTypes.SMG, "C" }, { SkillTypes.LMG, "C" }, { SkillTypes.HMG, "C" },
        { SkillTypes.Launcher, "C" }, { SkillTypes.AttachedLauncher, "C" },
        // Item 047: weapon-system gems (NATO/Eastern) — Combat. Mirror em scripts/skill-weights.mjs.
        { SkillTypes.UsecArsystems, "C" }, { SkillTypes.BearAksystems, "C" },
        // P — Practical (RZ map + dead practical skills from the RZ doc; FirstAid/FieldMedicine revived by SE)
        { SkillTypes.Surgery, "P" }, { SkillTypes.CovertMovement, "P" }, { SkillTypes.Search, "P" },
        { SkillTypes.MagDrills, "P" }, { SkillTypes.LightVests, "P" }, { SkillTypes.HeavyVests, "P" },
        { SkillTypes.WeaponTreatment, "P" }, { SkillTypes.Crafting, "P" }, { SkillTypes.HideoutManagement, "P" },
        { SkillTypes.FirstAid, "P" }, { SkillTypes.FieldMedicine, "P" },
        { SkillTypes.Sniping, "P" }, { SkillTypes.ProneMovement, "P" }, { SkillTypes.WeaponModding, "P" },
        { SkillTypes.AdvancedModding, "P" }, { SkillTypes.NightOps, "P" }, { SkillTypes.SilentOps, "P" },
        { SkillTypes.Lockpicking, "P" },
        // Item 047: ShadowConnections gem (scav cooldown / cultists) — Practical. Note: enum é "Shadowconnections" (c minúsculo).
        { SkillTypes.Shadowconnections, "P" },
        // S — faction/special (SE faction passives; not part of the Ph/M/C/P coverage rule)
        { SkillTypes.UsecNegotiations, "S" }, { SkillTypes.BearRawpower, "S" },
    };

    /// <summary>Median of the explicit weights per category — fallback for categorized skills without a weight.</summary>
    private static readonly IReadOnlyDictionary<string, double> CategoryMedians = ComputeCategoryMedians();

    /// <summary>
    ///     Resolves a skill's cost weight. Order: explicit (RZ) → derived (SE) → category median →
    ///     neutral 1.00 flagged as <see cref="SkillWeightOrigin.UnmappedFallback"/>. Never returns 0.
    /// </summary>
    public static (double Weight, SkillWeightOrigin Origin) ResolveWeight(SkillTypes skill)
    {
        if (Explicit.TryGetValue(skill, out var explicitWeight))
        {
            return (explicitWeight, SkillWeightOrigin.Explicit);
        }

        if (Derived.TryGetValue(skill, out var derivedWeight))
        {
            return (derivedWeight, SkillWeightOrigin.Derived);
        }

        if (Categories.TryGetValue(skill, out var category)
            && CategoryMedians.TryGetValue(category, out var median))
        {
            return (median, SkillWeightOrigin.CategoryFallback);
        }

        return (1.00, SkillWeightOrigin.UnmappedFallback);
    }

    private static Dictionary<string, double> ComputeCategoryMedians()
    {
        var byCategory = new Dictionary<string, List<double>>(StringComparer.Ordinal);
        foreach (var (skill, weight) in Explicit)
        {
            // every explicit skill is categorized by construction (RZ map)
            var category = Categories[skill];
            if (!byCategory.TryGetValue(category, out var list))
            {
                byCategory[category] = list = [];
            }

            list.Add(weight);
        }

        var medians = new Dictionary<string, double>(StringComparer.Ordinal);
        foreach (var (category, weights) in byCategory)
        {
            weights.Sort();
            var n = weights.Count;
            medians[category] = n % 2 == 1
                ? weights[n / 2]
                : Math.Round((weights[n / 2 - 1] + weights[n / 2]) / 2.0, 2);
        }

        // resulting medians: Ph=1.00, M=0.60, C=1.50, P=0.94 (documented in 022-...-02-spec-tech.md)
        return medians;
    }
}
