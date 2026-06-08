using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Utils;            // ISptLogger
using SPTarkov.Server.Core.Models.Enums.Hideout;    // HideoutAreas
using SPTarkov.Server.Core.Models.Eft.Common;       // PmcData

namespace CustomClasses;

/// <summary>
///     Sets a class's starting hideout station levels on a character.
///     See docs/technical/inventario-itens-spt4.md §10. (Item 003)
/// </summary>
[Injectable]
public class HideoutBuilder(ISptLogger<HideoutBuilder> logger)
{
    /// <summary>Returns the number of stations set.</summary>
    public int Apply(PmcData? character, Dictionary<string, int> hideout, string className)
    {
        var areas = character?.Hideout?.Areas;   // ref: BotBase.cs:710
        if (areas is null)
        {
            logger.Warning($"[CustomClasses] '{className}': base has no Hideout.Areas — hideout skipped.");
            return 0;
        }

        var set = 0;
        foreach (var (stationName, level) in hideout)
        {
            if (level <= 0)
            {
                continue;
            }

            if (!Enum.TryParse<HideoutAreas>(stationName, ignoreCase: true, out var type)
                || !Enum.IsDefined(typeof(HideoutAreas), type))
            {
                logger.Warning($"[CustomClasses] '{className}': unknown hideout station '{stationName}' — ignored.");
                continue;
            }

            var area = areas.FirstOrDefault(a => a.Type == type);   // ref: BotBase.cs:828
            if (area is null)
            {
                logger.Warning($"[CustomClasses] '{className}': hideout station '{stationName}' not in base — ignored.");
                continue;
            }

            area.Level = Math.Max(area.Level ?? 0, level);   // Level is int? (PA-01-06)
            // PA-02-03: mark the station as built/active, not "in construction".
            area.Active = true;
            area.Constructing = false;
            area.CompleteTime = 0;
            set++;
        }

        return set;
    }
}
