using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;                          // IOnLoad, OnLoadOrder
using SPTarkov.Server.Core.Helpers;                     // ModHelper
using SPTarkov.Server.Core.Services;                    // DatabaseService
using SPTarkov.Server.Core.Utils;                       // FileUtil, JsonUtil, TimeUtil
using SPTarkov.Server.Core.Utils.Cloners;               // ICloner
using SPTarkov.Server.Core.Models.Utils;                // ISptLogger
using SPTarkov.Server.Core.Models.Enums;                // SkillTypes
using SPTarkov.Server.Core.Models.Eft.Common;           // PmcData
using SPTarkov.Server.Core.Models.Eft.Common.Tables;    // ProfileSides, CommonSkill
using SPTarkov.Server.Core.Models.Spt.Mod;              // SptMod (item 006: soft-detect Skills-Extended)

namespace CustomClasses;

/// <summary>
///     Registers character "classes" as selectable launcher editions by loading one JSON file
///     per class from <c>config/classes/</c> and injecting each into the profiles database after
///     it is loaded (PostDBModLoader). Adding a class = dropping a <c>.json</c>/<c>.jsonc</c> file
///     (no recompile). Invalid files are skipped with a clear log; the others still load.
/// </summary>
[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]   // ref: OnLoadOrder.cs:9
public class CustomClassesMod(
    ModHelper modHelper,
    FileUtil fileUtil,
    JsonUtil jsonUtil,
    TimeUtil timeUtil,
    DatabaseService databaseService,
    LocaleService localeService,   // item 008: língua do servidor p/ resolver a descrição
    ICloner cloner,
    InventoryBuilder inventoryBuilder,
    HideoutBuilder hideoutBuilder,
    OutfitBuilder outfitBuilder,
    SkillMultiplierRegistry skillMultiplierRegistry,
    IReadOnlyList<SptMod> loadedMods,   // item 006: soft-detect do Skills-Extended
    ISptLogger<CustomClassesMod> logger
) : IOnLoad
{
    private const string DefaultBaseEdition = "SPT Zero to hero";   // empty-stash base — class controls its own items

    private bool _seInstalled;   // item 006: Skills-Extended presente nos mods carregados

    public Task OnLoad()
    {
        var classesPath = System.IO.Path.Combine(   // System.IO.Path: evita ambiguidade com ...Tables.Path
            modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly()),   // ref: ModHelper.cs:10
            "config", "classes");

        if (!fileUtil.DirectoryExists(classesPath))   // ref: FileUtil.cs:48
        {
            logger.Info($"[CustomClasses] No classes folder at '{classesPath}' — no custom classes registered.");
            return Task.CompletedTask;
        }

        // Item 006: detecta o Skills-Extended (soft) p/ avisar sobre multiplicadores de skills que dependem dele.
        _seInstalled = SkillsExtendedCompat.IsPresent(loadedMods);
        logger.Info($"[CustomClasses] Skills-Extended detectado: {(_seInstalled ? "sim" : "não")}.");

        // Non-recursive: only the top of config/classes/ (subfolders ignored — handy for drafts). PA-01-03
        var files = fileUtil.GetFiles(classesPath, false, "*.json")   // ref: FileUtil.cs:11
            .Concat(fileUtil.GetFiles(classesPath, false, "*.jsonc"))
            .Distinct()   // CR-01-02: dedupe — defesa contra overlap de glob *.json/*.jsonc em alguns sistemas
            .ToList();

        var templates = databaseService.GetProfileTemplates();   // ref: DatabaseService.cs:141
        int loaded = 0, skipped = 0;

        foreach (var file in files)
        {
            var fileName = fileUtil.GetFileNameAndExtension(file);   // ref: FileUtil.cs:33
            try
            {
                var def = jsonUtil.Deserialize<ClassDefinition>(fileUtil.ReadFile(file));   // ref: ModHelper.cs:28
                if (def is null || string.IsNullOrWhiteSpace(def.Name))
                {
                    logger.Error($"[CustomClasses] '{fileName}': missing required 'name' — skipped.");
                    skipped++;
                    continue;
                }

                if (!def.Enabled)
                {
                    logger.Info($"[CustomClasses] '{def.Name}' is disabled in '{fileName}' — skipped.");
                    skipped++;
                    continue;
                }

                if (RegisterClass(templates, def, fileName))
                {
                    loaded++;
                }
                else
                {
                    skipped++;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"[CustomClasses] '{fileName}': failed to parse/register — skipped. {ex.Message}");
                skipped++;
            }
        }

        logger.Info($"[CustomClasses] Loaded {loaded} class(es), skipped {skipped}, from '{classesPath}'.");
        return Task.CompletedTask;
    }

    /// <summary>Registers a single validated class as a launcher edition. Returns false (and logs) on skip.</summary>
    private bool RegisterClass(IDictionary<string, ProfileSides> templates, ClassDefinition def, string fileName)
    {
        // CR-01-03: trim para evitar chave/edition com espaço acidental vindo do JSON editado à mão.
        var name = def.Name!.Trim();

        // Collision guard: never overwrite a vanilla / other-mod / duplicate edition.
        if (templates.ContainsKey(name))
        {
            logger.Warning($"[CustomClasses] Edition '{name}' already exists — '{fileName}' skipped (no overwrite).");
            return false;
        }

        var baseKey = (string.IsNullOrWhiteSpace(def.BaseEdition) ? DefaultBaseEdition : def.BaseEdition!).Trim();
        if (!templates.TryGetValue(baseKey, out var baseSides) || baseSides is null)
        {
            logger.Error(
                $"[CustomClasses] '{fileName}': base edition '{baseKey}' not found — skipped. " +
                $"Available: {string.Join(", ", templates.Keys)}");
            return false;
        }

        // Deep clone (ICloner.Clone is deep) — mutating nested Skills.Common is safe and does
        // NOT affect the vanilla base template. Clone returns T? — guard it.  // ref: ICloner.cs:5, CR-01-03
        var sides = cloner.Clone(baseSides);
        if (sides is null)
        {
            logger.Error($"[CustomClasses] '{fileName}': clone of base '{baseKey}' returned null — skipped.");
            return false;
        }

        // Item 008: resolve a descrição pela locale do servidor (pt* → pt, senão en; vazio → nome).
        // GetText devolve a key verbatim quando não registrada (ServerLocalisationService.cs:92), então
        // gravamos o texto literal já resolvido. Não há AddText público p/ registrar key de locale.
        var desc = def.Description?.Resolve(localeService.GetDesiredServerLocale());   // ref: LocaleService.cs:74
        sides.DescriptionLocaleKey = string.IsNullOrWhiteSpace(desc) ? name : desc;

        // PA-01-02: per-side counts + warn if a side applied nothing despite configured skills.
        var usecApplied = ApplySkills(sides.Usec?.Character, def);
        var bearApplied = ApplySkills(sides.Bear?.Character, def);
        if (def.Skills is { Count: > 0 } && (usecApplied == 0 || bearApplied == 0))
        {
            logger.Warning(
                $"[CustomClasses] '{def.Name}': a side applied 0 skills " +
                $"(usec={usecApplied}, bear={bearApplied}) — check base template '{baseKey}'.");
        }

        // Item 003: itens (equipado/composto) + hideout, nos dois lados. (CR-01-04: capturar p/ log)
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
        var mults = 0;
        if (def.SkillMultipliers is { Count: > 0 })
        {
            var clean = new Dictionary<string, double>(StringComparer.Ordinal);
            foreach (var (skillName, factor) in def.SkillMultipliers)
            {
                if (!Enum.TryParse<SkillTypes>(skillName, ignoreCase: true, out var st) || !Enum.IsDefined(typeof(SkillTypes), st))
                {
                    logger.Warning($"[CustomClasses] '{name}': skillMultipliers — skill desconhecida '{skillName}' — ignorada.");
                    continue;
                }

                // Item 006: skill do Skills-Extended sem o SE instalado → registra mesmo assim (inócuo), mas avisa.
                if (!_seInstalled && SkillsExtendedCompat.Skills.Contains(st.ToString()))
                {
                    logger.Warning(
                        $"[CustomClasses] '{name}': skill '{st}' depende do Skills-Extended (não detectado) — " +
                        $"multiplicador registrado, mas sem efeito até instalar o SE.");
                }

                clean[st.ToString()] = factor < 0 ? 0 : factor;   // clamp ≥ 0; normaliza o nome via enum
            }

            if (clean.Count > 0)
            {
                skillMultiplierRegistry.Set(name, clean);   // edition (= name) → fatores
                mults = clean.Count;
            }
        }

        templates[name] = sides;
        logger.Info(
            $"[CustomClasses] Registered '{name}' (base '{baseKey}', skills usec={usecApplied}/bear={bearApplied}, " +
            $"items usec={itemsUsec}/bear={itemsBear}, hideout={stations}, outfit usec={outfitsUsec}/bear={outfitsBear}, skillMults={mults}) from '{fileName}'.");
        return true;
    }

    /// <summary>
    ///     Applies the class skills to one side's common skills. Returns the count applied
    ///     (0 if the side has no skills container or the class defines none).
    /// </summary>
    private int ApplySkills(PmcData? character, ClassDefinition def)
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
