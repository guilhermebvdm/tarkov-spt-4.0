namespace CustomClasses.Web;

/// <summary>One editable skill → starting level row (item 025).</summary>
public sealed class SkillLevelRow
{
    public string Skill { get; set; } = string.Empty;
    public int Level { get; set; }
}

/// <summary>One editable skill → XP multiplier row (item 025).</summary>
public sealed class SkillFactorRow
{
    public string Skill { get; set; } = string.Empty;
    public double Factor { get; set; } = 1.0;
}

/// <summary>One editable hideout station → starting level row (item 025).</summary>
public sealed class HideoutRow
{
    public string Station { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
}

/// <summary>One editable equipped slot (EquipmentSlots name → item). (Item 026)</summary>
public sealed class EquippedSlotRow
{
    public string Slot { get; set; } = string.Empty;
    public ItemSpecModel Item { get; set; } = new();
}

/// <summary>
///     Item 026: mutable view-model for one <see cref="ItemSpec"/> (the record is init-only).
///     Edited in place by ItemSpecEditor (Equipped tab; reused by item 028 for the stash) and
///     rebuilt into the immutable spec on save via <see cref="ToSpec"/>.
/// </summary>
public sealed class ItemSpecModel
{
    public string? Tpl { get; set; }

    /// <summary>Preset id OR weapon tpl (resolution semantics in docs/class-schema.md §3.1).</summary>
    public string? Preset { get; set; }

    public bool Premium { get; set; }

    /// <summary>Honored in stash/contents only — ignored on equipped slots (PA-02-05).</summary>
    public int Count { get; set; } = 1;

    /// <summary>Cartridge tpl — REQUIRED when LoadedMag/Chambered is true (PA-01-03).</summary>
    public string? Ammo { get; set; }

    public bool LoadedMag { get; set; }
    public bool Chambered { get; set; }

    public List<ItemSpecModel> Contents { get; } = [];
    public List<ModSpecModel> Mods { get; } = [];

    public static ItemSpecModel FromSpec(ItemSpec spec)
    {
        var model = new ItemSpecModel
        {
            Tpl = NullIfBlank(spec.Tpl),
            Preset = NullIfBlank(spec.Preset),
            Premium = spec.Premium,
            Count = spec.Count < 1 ? 1 : spec.Count,
            Ammo = NullIfBlank(spec.Ammo),
            LoadedMag = spec.LoadedMag,
            Chambered = spec.Chambered,
        };

        foreach (var content in spec.Contents ?? [])
        {
            model.Contents.Add(FromSpec(content));
        }

        foreach (var mod in spec.Mods ?? [])
        {
            model.Mods.Add(ModSpecModel.FromSpec(mod));
        }

        return model;
    }

    /// <summary>Empty strings/lists become null so the saved file stays close to a hand-written one.</summary>
    public ItemSpec ToSpec() => new()
    {
        Tpl = NullIfBlank(Tpl),
        Preset = NullIfBlank(Preset),
        Premium = Premium,
        Count = Count < 1 ? 1 : Count,
        Ammo = NullIfBlank(Ammo),
        LoadedMag = LoadedMag,
        Chambered = Chambered,
        Contents = Contents.Count > 0 ? Contents.Select(c => c.ToSpec()).ToList() : null,
        Mods = Mods.Count > 0 ? Mods.Select(m => m.ToSpec()).ToList() : null,
    };

    private static string? NullIfBlank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}

/// <summary>Item 026: mutable view-model for one <see cref="ModSpec"/> node (manual mod tree).</summary>
public sealed class ModSpecModel
{
    public string SlotId { get; set; } = string.Empty;
    public string? Tpl { get; set; }
    public List<ModSpecModel> Mods { get; } = [];

    public static ModSpecModel FromSpec(ModSpec spec)
    {
        var model = new ModSpecModel
        {
            SlotId = spec.SlotId?.Trim() ?? string.Empty,
            Tpl = string.IsNullOrWhiteSpace(spec.Tpl) ? null : spec.Tpl,
        };

        foreach (var sub in spec.Mods ?? [])
        {
            model.Mods.Add(FromSpec(sub));
        }

        return model;
    }

    public ModSpec ToSpec() => new()
    {
        SlotId = string.IsNullOrWhiteSpace(SlotId) ? null : SlotId,
        Tpl = string.IsNullOrWhiteSpace(Tpl) ? null : Tpl,
        Mods = Mods.Count > 0 ? Mods.Select(m => m.ToSpec()).ToList() : null,
    };
}

/// <summary>
///     Item 025: mutable view-model for the class edit form. <see cref="ClassDefinition"/> is an
///     immutable record (init-only setters), so the form binds to this model and rebuilds the
///     definition on save via <see cref="ToDefinition"/>. Dictionaries become row lists so rows can
///     be added/removed/bound in place. Item 026: <c>loadout.equipped</c> opens into editable
///     <see cref="EquippedSlotRow"/>s; item 028: <c>loadout.stash</c> opens into an editable list of
///     <see cref="ItemSpecModel"/>s (same FromSpec/ToSpec round-trip as equipped).
/// </summary>
public sealed class ClassEditModel
{
    // ── General ──────────────────────────────────────────────────────────────
    /// <summary>Edition key — READ-ONLY in the UI (rename orphans existing profiles; duplicate instead, item 027).</summary>
    public string Name { get; set; } = string.Empty;

    public string? DisplayNameEn { get; set; }
    public string? DisplayNamePt { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionPt { get; set; }
    public bool Enabled { get; set; } = true;

    /// <summary>null/blank = mod default ("SPT Zero to hero").</summary>
    public string? BaseEdition { get; set; }

    public string? IconFile { get; set; }
    public string? NameColor { get; set; }

    // ── Tables ───────────────────────────────────────────────────────────────
    public List<SkillLevelRow> Skills { get; } = [];
    public List<SkillFactorRow> Multipliers { get; } = [];
    public List<HideoutRow> Hideout { get; } = [];

    // ── Outfit (customization ids; null = template default) ─────────────────
    public string? UsecUpper { get; set; }
    public string? UsecLower { get; set; }
    public string? BearUpper { get; set; }
    public string? BearLower { get; set; }

    // ── Loadout (item 026: equipped; item 028: stash) ────────────────────────
    /// <summary>Editable equipped slots (EquipmentSlots name → mutable ItemSpec model). (Item 026)</summary>
    public List<EquippedSlotRow> Equipped { get; } = [];

    /// <summary>Editable stash lines (flat list — GridPacker positions at runtime). (Item 028)</summary>
    public List<ItemSpecModel> Stash { get; } = [];

    public static ClassEditModel FromDefinition(ClassDefinition def)
    {
        var model = new ClassEditModel
        {
            Name = def.Name?.Trim() ?? string.Empty,
            DisplayNameEn = def.DisplayName?.En,
            DisplayNamePt = def.DisplayName?.Pt,
            DescriptionEn = def.Description?.En,
            DescriptionPt = def.Description?.Pt,
            Enabled = def.Enabled,
            BaseEdition = NullIfBlank(def.BaseEdition),
            IconFile = NullIfBlank(def.IconFile),
            NameColor = NullIfBlank(def.NameColor),
            UsecUpper = NullIfBlank(def.Outfit?.Usec?.Upper),
            UsecLower = NullIfBlank(def.Outfit?.Usec?.Lower),
            BearUpper = NullIfBlank(def.Outfit?.Bear?.Upper),
            BearLower = NullIfBlank(def.Outfit?.Bear?.Lower),
        };

        foreach (var (slot, spec) in def.Loadout?.Equipped ?? [])
        {
            model.Equipped.Add(new EquippedSlotRow { Slot = slot, Item = ItemSpecModel.FromSpec(spec) });
        }

        foreach (var spec in def.Loadout?.Stash ?? [])
        {
            model.Stash.Add(ItemSpecModel.FromSpec(spec));
        }

        foreach (var (skill, level) in def.Skills ?? [])
        {
            model.Skills.Add(new SkillLevelRow { Skill = skill, Level = level });
        }

        foreach (var (skill, factor) in def.SkillMultipliers ?? [])
        {
            model.Multipliers.Add(new SkillFactorRow { Skill = skill, Factor = factor });
        }

        foreach (var (station, level) in def.Hideout ?? [])
        {
            model.Hideout.Add(new HideoutRow { Station = station, Level = level });
        }

        return model;
    }

    /// <summary>
    ///     Rebuilds an immutable <see cref="ClassDefinition"/> from the form state. Empty tables/strings
    ///     become null (omitted on serialize) so a round-trip stays close to a hand-written file.
    ///     Duplicate row keys keep the FIRST occurrence (the UI only offers unused names on add).
    /// </summary>
    public ClassDefinition ToDefinition() => new()
    {
        Name = NullIfBlank(Name)?.Trim(),
        DisplayName = BuildLocalized(DisplayNameEn, DisplayNamePt),
        Enabled = Enabled,
        BaseEdition = NullIfBlank(BaseEdition),
        Description = BuildLocalized(DescriptionEn, DescriptionPt),
        IconFile = NullIfBlank(IconFile),
        NameColor = NullIfBlank(NameColor),
        Skills = ToDict(Skills, r => r.Skill, r => r.Level),
        SkillMultipliers = ToDict(Multipliers, r => r.Skill, r => r.Factor),
        Hideout = ToDict(Hideout, r => r.Station, r => r.Level),
        Loadout = BuildLoadout(),
        Outfit = BuildOutfit(),
    };

    /// <summary>
    ///     Equipped rows → dict (first occurrence wins — the UI only offers unused slots on add);
    ///     stash rows → immutable spec list (item 028). Both empty → loadout omitted on serialize.
    /// </summary>
    private Loadout? BuildLoadout()
    {
        Dictionary<string, ItemSpec>? equipped = null;
        if (Equipped.Count > 0)
        {
            equipped = new Dictionary<string, ItemSpec>(StringComparer.Ordinal);
            foreach (var row in Equipped)
            {
                var slot = row.Slot?.Trim();
                if (!string.IsNullOrWhiteSpace(slot))
                {
                    equipped.TryAdd(slot!, row.Item.ToSpec());
                }
            }

            if (equipped.Count == 0)
            {
                equipped = null;
            }
        }

        var stash = Stash.Count > 0 ? Stash.Select(s => s.ToSpec()).ToList() : null;

        return equipped is null && stash is null
            ? null
            : new Loadout { Equipped = equipped, Stash = stash };
    }

    private Outfit? BuildOutfit()
    {
        var usec = BuildSide(UsecUpper, UsecLower);
        var bear = BuildSide(BearUpper, BearLower);
        return usec is null && bear is null ? null : new Outfit { Usec = usec, Bear = bear };
    }

    private static OutfitSide? BuildSide(string? upper, string? lower)
    {
        upper = NullIfBlank(upper);
        lower = NullIfBlank(lower);
        return upper is null && lower is null ? null : new OutfitSide { Upper = upper, Lower = lower };
    }

    private static LocalizedText? BuildLocalized(string? en, string? pt)
    {
        en = NullIfBlank(en);
        pt = NullIfBlank(pt);
        return en is null && pt is null ? null : new LocalizedText { En = en, Pt = pt };
    }

    private static Dictionary<string, TValue>? ToDict<TRow, TValue>(
        List<TRow> rows, Func<TRow, string> key, Func<TRow, TValue> value)
    {
        var dict = new Dictionary<string, TValue>(StringComparer.Ordinal);
        foreach (var row in rows)
        {
            var k = key(row)?.Trim();
            if (!string.IsNullOrWhiteSpace(k))
            {
                dict.TryAdd(k!, value(row));   // first occurrence wins — duplicates blocked in the UI
            }
        }

        return dict.Count > 0 ? dict : null;
    }

    private static string? NullIfBlank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}
