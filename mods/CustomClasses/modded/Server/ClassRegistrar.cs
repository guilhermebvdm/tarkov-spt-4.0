using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Services;                    // DatabaseService, LocaleService
using SPTarkov.Server.Core.Utils;                       // TimeUtil
using SPTarkov.Server.Core.Utils.Cloners;               // ICloner
using SPTarkov.Server.Core.Models.Utils;                // ISptLogger
using SPTarkov.Server.Core.Models.Enums;                // SkillTypes
using SPTarkov.Server.Core.Models.Eft.Common;           // PmcData
using SPTarkov.Server.Core.Models.Eft.Common.Tables;    // ProfileSides, CommonSkill
using SPTarkov.Server.Core.Models.Spt.Mod;              // SptMod (item 006: soft-detect Skills-Extended)

namespace CustomClasses;

/// <summary>
///     Item 021: everything needed to register one class, fully built but NOT yet applied.
///     Produced by <see cref="ClassRegistrar.ValidateAndBuild"/> (pure / dry-run); applied by
///     <see cref="ClassRegistrar.Commit"/> in one build-then-swap step.
/// </summary>
public sealed record RegistrationPlan
{
    /// <summary>Edition key (trimmed <c>def.name</c>).</summary>
    public required string Name { get; init; }

    /// <summary>Resolved (trimmed/defaulted) base edition key that was cloned.</summary>
    public required string BaseEdition { get; init; }

    /// <summary>The source definition (visual identity fields are read from here at commit time).</summary>
    public required ClassDefinition Definition { get; init; }

    /// <summary>Fully built sides (clone of base + skills + loadout + hideout + outfit + description).</summary>
    public required ProfileSides Sides { get; init; }

    /// <summary>Normalized (enum-cased) and clamped (≥ 0) skill XP multipliers. Empty = none.</summary>
    public Dictionary<string, double> SkillMultipliers { get; init; } = [];

    /// <summary>File the definition came from (log/audit context only).</summary>
    public string? SourceFileName { get; init; }

    /// <summary>Per-side counts, used for the "Registered ..." summary log at commit.</summary>
    public required RegistrationCounts Counts { get; init; }
}

/// <summary>Counts gathered while building a <see cref="RegistrationPlan"/> (for the commit summary log). (Item 021)</summary>
public sealed record RegistrationCounts(
    int SkillsUsec,
    int SkillsBear,
    int ItemsUsec,
    int ItemsBear,
    int HideoutStations,
    int OutfitsUsec,
    int OutfitsBear,
    int SkillMultipliers);

/// <summary>
///     Item 021: the single class-registration pipeline, shared by the boot loader
///     (<see cref="CustomClassesMod.OnLoad"/>) and the web editor (<see cref="ClassEditorService"/>)
///     — same validation by construction.
///     <para>
///     Phases: <see cref="ValidateAndBuild"/> is a PURE dry-run — it runs every validation and builds the
///     complete <see cref="ProfileSides"/> without touching the profile-templates dict or the registries
///     (builders may log warnings; that log parity with the pre-021 loader is intentional).
///     <see cref="Commit"/> then applies the plan atomically-ish (build-then-swap: the fully built sides
///     object is assigned to the templates dict in a single reference write). <see cref="Remove"/> undoes
///     a registration (templates + registries).
///     </para>
///     <para>
///     Concurrency: the templates dict and the registries are plain <c>Dictionary</c>s also read by SPT
///     (profile creation) and by the mod's routers on HTTP threads. Hot-apply mutation races are accepted
///     (local single-user server — see item 021 kickoff); build-then-swap keeps the exposed state always
///     fully built, never half-mutated.
///     </para>
///     SINGLETON: holds no per-request state; injecting scoped builders into a singleton follows SPT's own
///     pattern (e.g. ItemHelper [Singleton] injects ICloner [Scoped] — ref: ItemHelper.cs:19).
/// </summary>
[Injectable(InjectionType.Singleton)]
public class ClassRegistrar(
    DatabaseService databaseService,
    LocaleService localeService,
    ICloner cloner,
    TimeUtil timeUtil,
    InventoryBuilder inventoryBuilder,
    HideoutBuilder hideoutBuilder,
    OutfitBuilder outfitBuilder,
    SkillMultiplierRegistry skillMultiplierRegistry,
    ClassVisualRegistry classVisualRegistry,
    ClassEditionKeyRegistry classEditionKeyRegistry,   // item 058: fileName → chave efetiva registrada
    LauncherLanguageConfig launcherLanguageConfig,     // item 058 (CR-01-01): língua da chave de edition
    IReadOnlyList<SptMod> loadedMods,   // item 006: soft-detect do Skills-Extended
    ISptLogger<ClassRegistrar> logger
)
{
    // Item 027: public — ClassEditorService.Create writes the default base explicitly into new class files.
    public const string DefaultBaseEdition = "SPT Zero to hero";   // empty-stash base — class controls its own items

    private bool? _seInstalled;

    /// <summary>Item 006: Skills-Extended present among loaded mods (lazy — resolved on first use, post mod load).</summary>
    public bool SkillsExtendedInstalled => _seInstalled ??= SkillsExtendedCompat.IsPresent(loadedMods);

    /// <summary>
    ///     Item 058 (ref: CR-01-01/CR-01-02): chave de edition EFETIVA de uma definição — a mesma que
    ///     <see cref="ValidateAndBuild"/> usa em <c>plan.Name</c> (language do settings.jsonc aplicada:
    ///     pt/en → displayName[lang], fallback name; trimmed). Para callers que precisam da chave SEM
    ///     montar um plan (ex.: ClassEditorService.Delete → hot-remove, BuildEntry → flag Registered).
    /// </summary>
    public string ResolveEditionKey(ClassDefinition def) => launcherLanguageConfig.ResolveEditionKey(def);

    /// <summary>
    ///     Phase 1 — PURE dry-run. Runs every validation the pre-021 loader ran (blank name, edition
    ///     collision, missing base edition, null clone) and builds the complete sides (skills + loadout +
    ///     hideout + outfit + resolved description) WITHOUT mutating templates or registries.
    ///     Returns null when a blocking (Error) problem is found; <paramref name="diagnostics"/> always
    ///     carries the structured outcome. Log parity: the same messages the loader logged before 021 are
    ///     still logged here (and the builders keep logging their own warnings directly — those are NOT
    ///     captured as diagnostics yet, by design).
    /// </summary>
    /// <param name="def">Parsed class definition.</param>
    /// <param name="fileName">Source file name, for log/diagnostic context.</param>
    /// <param name="allowReplace">
    ///     When true (editor re-saving an existing class) a collision with an edition REGISTERED BY THIS MOD
    ///     is not an error. Collisions with vanilla/other-mod editions are always errors. Boot uses false.
    /// </param>
    /// <param name="diagnostics">Structured validation outcome (always set).</param>
    public RegistrationPlan? ValidateAndBuild(
        ClassDefinition def,
        string fileName,
        bool allowReplace,
        out List<ClassDiagnostic> diagnostics)
    {
        diagnostics = [];

        // Boot pre-checks these two before calling (with its own logs); kept here too so editor dry-runs
        // get the same outcome without relying on the caller.
        if (string.IsNullOrWhiteSpace(def.Name))
        {
            diagnostics.Add(new ClassDiagnostic(DiagnosticSeverity.Error, DiagnosticCodes.NameMissing,
                $"'{fileName}': missing required 'name'."));
            return null;
        }

        if (!def.Enabled)
        {
            diagnostics.Add(new ClassDiagnostic(DiagnosticSeverity.Info, DiagnosticCodes.ClassDisabled,
                $"'{def.Name}' is disabled in '{fileName}' — it will not register."));
            // Not blocking: still validate/build so the editor can preview a disabled class.
        }

        // ref: CR-01-01 (item 058, fix estrutural): a chave EFETIVA é resolvida AQUI, no pipeline
        // compartilhado — boot, Save/hotApply e Delete produzem a MESMA editionKey em qualquer
        // language do settings.jsonc (antes o transform era privado do boot e um Save+hotApply sob
        // language=pt registrava a edition EN transitória → perfis com edition fantasma pós-restart).
        // ResolveEditionKey já faz o trim (CR-01-03 do item 021: chave sem espaço acidental).
        // O def/arquivo mantém o name cru — re-chaveamento é preocupação de registro, não de disco.
        var name = launcherLanguageConfig.ResolveEditionKey(def);
        var templates = databaseService.GetProfileTemplates();   // ref: DatabaseService.cs:141

        // Collision guard: never overwrite a vanilla / other-mod / duplicate edition. With allowReplace the
        // editor may re-save over an edition THIS MOD registered (ClassVisualRegistry tracks ownership).
        if (templates.ContainsKey(name) && !(allowReplace && classVisualRegistry.Contains(name)))
        {
            logger.Warning($"[CustomClasses] Edition '{name}' already exists — '{fileName}' skipped (no overwrite).");
            diagnostics.Add(new ClassDiagnostic(DiagnosticSeverity.Error, DiagnosticCodes.EditionCollision,
                $"Edition '{name}' already exists — '{fileName}' skipped (no overwrite)."));
            return null;
        }

        var baseKey = (string.IsNullOrWhiteSpace(def.BaseEdition) ? DefaultBaseEdition : def.BaseEdition!).Trim();
        if (!templates.TryGetValue(baseKey, out var baseSides) || baseSides is null)
        {
            var available = string.Join(", ", templates.Keys);
            logger.Error(
                $"[CustomClasses] '{fileName}': base edition '{baseKey}' not found — skipped. " +
                $"Available: {available}");
            diagnostics.Add(new ClassDiagnostic(DiagnosticSeverity.Error, DiagnosticCodes.BaseEditionNotFound,
                $"Base edition '{baseKey}' not found. Available: {available}"));
            return null;
        }

        // Deep clone (ICloner.Clone is deep) — mutating nested Skills.Common is safe and does
        // NOT affect the vanilla base template. Clone returns T? — guard it.  // ref: ICloner.cs:5, CR-01-03
        var sides = cloner.Clone(baseSides);
        if (sides is null)
        {
            logger.Error($"[CustomClasses] '{fileName}': clone of base '{baseKey}' returned null — skipped.");
            diagnostics.Add(new ClassDiagnostic(DiagnosticSeverity.Error, DiagnosticCodes.CloneFailed,
                $"Clone of base edition '{baseKey}' returned null."));
            return null;
        }

        // Item 008: resolve a descrição pela locale do servidor (pt* → pt, senão en; vazio → nome).
        // GetText devolve a key verbatim quando não registrada (ServerLocalisationService.cs:92), então
        // gravamos o texto literal já resolvido. Não há AddText público p/ registrar key de locale.
        var desc = def.Description?.Resolve(localeService.GetDesiredServerLocale());   // ref: LocaleService.cs:74
        sides.DescriptionLocaleKey = string.IsNullOrWhiteSpace(desc) ? name : desc;

        // PA-01-02: per-side counts + warn if a side applied nothing despite configured skills.
        var usecApplied = ApplySkills(sides.Usec?.Character, def, diagnostics);
        var bearApplied = ApplySkills(sides.Bear?.Character, def, diagnostics);
        if (def.Skills is { Count: > 0 } && (usecApplied == 0 || bearApplied == 0))
        {
            logger.Warning(
                $"[CustomClasses] '{def.Name}': a side applied 0 skills " +
                $"(usec={usecApplied}, bear={bearApplied}) — check base template '{baseKey}'.");
            diagnostics.Add(new ClassDiagnostic(DiagnosticSeverity.Warning, DiagnosticCodes.SideSkillsNotApplied,
                $"A side applied 0 skills (usec={usecApplied}, bear={bearApplied}) — check base template '{baseKey}'."));
        }

        // Item 003: itens (equipado/composto) + hideout, nos dois lados. (CR-01-04: capturar p/ log)
        // Builders log their own warnings directly (not captured as diagnostics — item 021 decision).
        int itemsUsec = 0, itemsBear = 0, stations = 0;
        if (def.Loadout is not null)
        {
            itemsUsec = inventoryBuilder.Apply(sides.Usec?.Character, def.Loadout, name);
            itemsBear = inventoryBuilder.Apply(sides.Bear?.Character, def.Loadout, name);
        }
        if (def.Hideout is { Count: > 0 })
        {
            stations = hideoutBuilder.Apply(sides.Usec?.Character, def.Hideout, name);
            hideoutBuilder.Apply(sides.Bear?.Character, def.Hideout, name);
        }

        // Item 004: outfit (roupas + aparência) por lado.
        int outfitsUsec = 0, outfitsBear = 0;
        if (def.Outfit is not null)
        {
            outfitsUsec = outfitBuilder.Apply(sides.Usec, "Usec", def.Outfit.Usec, name);
            outfitsBear = outfitBuilder.Apply(sides.Bear, "Bear", def.Outfit.Bear, name);
        }

        // Item 005: multiplicadores de XP de skill (servidos ao client por edition = nome da classe).
        var clean = new Dictionary<string, double>(StringComparer.Ordinal);
        if (def.SkillMultipliers is { Count: > 0 })
        {
            foreach (var (skillName, factor) in def.SkillMultipliers)
            {
                if (!Enum.TryParse<SkillTypes>(skillName, ignoreCase: true, out var st) || !Enum.IsDefined(typeof(SkillTypes), st))
                {
                    logger.Warning($"[CustomClasses] '{name}': skillMultipliers — skill desconhecida '{skillName}' — ignorada.");
                    diagnostics.Add(new ClassDiagnostic(DiagnosticSeverity.Warning, DiagnosticCodes.UnknownMultiplierSkill,
                        $"skillMultipliers: unknown skill '{skillName}' — ignored."));
                    continue;
                }

                // Item 006: skill do Skills-Extended sem o SE instalado → registra mesmo assim (inócuo), mas avisa.
                if (!SkillsExtendedInstalled && SkillsExtendedCompat.Skills.Contains(st.ToString()))
                {
                    logger.Warning(
                        $"[CustomClasses] '{name}': skill '{st}' depende do Skills-Extended (não detectado) — " +
                        $"multiplicador registrado, mas sem efeito até instalar o SE.");
                    diagnostics.Add(new ClassDiagnostic(DiagnosticSeverity.Warning, DiagnosticCodes.SkillsExtendedMissing,
                        $"Skill '{st}' depends on Skills-Extended (not detected) — multiplier registered but inert."));
                }

                clean[st.ToString()] = factor < 0 ? 0 : factor;   // clamp ≥ 0; normaliza o nome via enum
            }
        }

        return new RegistrationPlan
        {
            Name = name,
            BaseEdition = baseKey,
            Definition = def,
            Sides = sides,
            SkillMultipliers = clean,
            SourceFileName = fileName,
            Counts = new RegistrationCounts(
                usecApplied, bearApplied, itemsUsec, itemsBear, stations, outfitsUsec, outfitsBear, clean.Count),
        };
    }

    /// <summary>
    ///     Phase 2 — applies a built plan: registries first (keys not yet visible to readers), then the
    ///     templates entry in a single reference assignment (build-then-swap). Logs the same
    ///     "Registered '...'" summary the pre-021 loader logged.
    ///     On hot-apply over an existing class, an empty multiplier set CLEARS the previous entry
    ///     (no-op at boot, where the registry starts empty).
    /// </summary>
    public void Commit(RegistrationPlan plan)
    {
        if (plan.SkillMultipliers.Count > 0)
        {
            skillMultiplierRegistry.Set(plan.Name, plan.SkillMultipliers);   // edition (= name) → fatores
        }
        else
        {
            skillMultiplierRegistry.Remove(plan.Name);   // hot-apply: clear stale multipliers from a previous version
        }

        // Item 011 (PA-01-01): registra a identidade visual só no registro EFETIVO (após todas as validações),
        // junto de templates[name] = sides — assim o router só expõe identidade de classes realmente registradas.
        // Item 008 (i18n): nome localizado en/pt (fallback ao próprio name) p/ o client resolver pelo idioma do EFT.
        var def = plan.Definition;
        // Item 068: a description en/pt viaja junto p/ o client mostrar na aba CLASS + tooltip (não só no launcher).
        classVisualRegistry.Set(plan.Name, def.IconFile, def.NameColor, def.DisplayName?.En, def.DisplayName?.Pt,
            def.Description?.En, def.Description?.Pt);

        // Item 058: grava a chave EFETIVA registrada a partir deste arquivo (com language=pt/en o boot
        // re-chaveia p/ displayName[lang]) — consumida pelo ClassListRouter (editionKey do contrato SP0).
        if (!string.IsNullOrWhiteSpace(plan.SourceFileName))
        {
            classEditionKeyRegistry.Set(plan.SourceFileName!, plan.Name);
        }

        // Build-then-swap: single reference write — readers (CreateProfileService → ProfileHelper.
        // GetProfileTemplateForSide, ref: ProfileHelper.cs:804/806) always see either the old or the new
        // fully built sides, never a half-mutated one.
        databaseService.GetProfileTemplates()[plan.Name] = plan.Sides;

        var c = plan.Counts;
        logger.Info(
            $"[CustomClasses] Registered '{plan.Name}' (base '{plan.BaseEdition}', skills usec={c.SkillsUsec}/bear={c.SkillsBear}, " +
            $"items usec={c.ItemsUsec}/bear={c.ItemsBear}, hideout={c.HideoutStations}, outfit usec={c.OutfitsUsec}/bear={c.OutfitsBear}, skillMults={c.SkillMultipliers}) from '{plan.SourceFileName}'.");
    }

    /// <summary>
    ///     Unregisters a class: removes the launcher edition (templates) AND the registry entries.
    ///     Safety: refuses to touch editions NOT registered by this mod (ClassVisualRegistry tracks
    ///     ownership) — so a stray name can never delete a vanilla/other-mod edition.
    ///     Existing profiles created from the edition are not affected (edition only disappears from
    ///     the launcher's new-profile list).
    /// </summary>
    public bool Remove(string name)
    {
        if (!classVisualRegistry.Contains(name))
        {
            logger.Warning($"[CustomClasses] Remove('{name}'): not a class registered by this mod — nothing removed.");
            return false;
        }

        var removed = databaseService.GetProfileTemplates().Remove(name);
        skillMultiplierRegistry.Remove(name);
        classVisualRegistry.Remove(name);
        classEditionKeyRegistry.RemoveByEdition(name);   // item 058: limpa fileName → editionKey
        logger.Info($"[CustomClasses] Removed class '{name}' (edition unregistered{(removed ? string.Empty : "; template entry was already absent")}).");
        return removed;
    }

    /// <summary>
    ///     Applies the class skills to one side's common skills. Returns the count applied
    ///     (0 if the side has no skills container or the class defines none).
    /// </summary>
    private int ApplySkills(PmcData? character, ClassDefinition def, List<ClassDiagnostic> diagnostics)
    {
        if (character?.Skills?.Common is null || def.Skills is null || def.Skills.Count == 0)
        {
            return 0;
        }

        // Skills.Common is IEnumerable<CommonSkill> (ref: BotBase.cs:412) — materialize to mutate.
        var common = character.Skills.Common.ToList();
        var applied = 0;
        foreach (var (skillName, level) in def.Skills)
        {
            // PA-01-01: Enum.TryParse aceita numérico/indefinido — exigir IsDefined p/ rejeitar skill fantasma.
            if (!Enum.TryParse<SkillTypes>(skillName, ignoreCase: true, out var skill)
                || !Enum.IsDefined(typeof(SkillTypes), skill))
            {
                logger.Warning($"[CustomClasses] '{def.Name}': unknown skill '{skillName}' — ignored.");
                // Both sides run through here — dedupe so the diagnostic appears once per skill name.
                if (!diagnostics.Any(d =>
                        d.Code == DiagnosticCodes.UnknownSkill
                        && d.Message.Contains($"'{skillName}'", StringComparison.Ordinal)))
                {
                    diagnostics.Add(new ClassDiagnostic(DiagnosticSeverity.Warning, DiagnosticCodes.UnknownSkill,
                        $"skills: unknown skill '{skillName}' — ignored."));
                }
                continue;
            }

            var progress = Math.Clamp(level, 0, 51) * 100d;          // ref: ProfileHelper.cs:460/543 (5100 == level 51)
            var entry = common.FirstOrDefault(s => s.Id == skill);   // ref: ProfileHelper.cs:509
            if (entry is null)
            {
                // CR-01-01: new skill not in base — set LastAccess to now so fatigue/decay math is sane.
                common.Add(new CommonSkill { Id = skill, Progress = progress, LastAccess = timeUtil.GetTimeStamp() });
            }
            else
            {
                entry.Progress = progress;
            }

            applied++;
        }

        character.Skills.Common = common;
        return applied;
    }
}
