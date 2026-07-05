using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using ChangeHelmetVisorServer.Models;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace ChangeHelmetVisorServer;

public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.trinagan.changehelmetvisorserver";
    public override string Name { get; init; } = "Change Helmet Visor";
    public override string Author { get; init; } = "Trinagan";
    public override List<string>? Contributors { get; init; }
    public override SemanticVersioning.Version Version { get; init; } = new("2.0.0");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.11");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url { get; init; }
    public override bool? IsBundleMod { get; init; }
    public override string License { get; init; } = "MIT";
}

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class ChangeHelmetVisorServer(
    ISptLogger<ChangeHelmetVisorServer> logger,
    DatabaseService databaseService,
    ModHelper modHelper)
    : IOnLoad
{
    public Task OnLoad()
    {
        var pathToMod = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var config = modHelper.GetJsonDataFromFile<ModConfig>(pathToMod, "config.jsonc");
        var faceShields = FindAllFaceShieldHelmets();
        var items = databaseService.GetItems();

        logger.Info($"[Change Helmet Visor] Initialising changes...");

        if (config.AllFaceShields == true)
        {
            ChangeAllHelmetsVisorProperties(config, faceShields, items);
        }
        else
        {
            ChangeSpecificHelmetsVisors(config, faceShields, items);
        }

        return Task.CompletedTask;
    }

    private void ChangeAllHelmetsVisorProperties(ModConfig config, List<String> faceShields, Dictionary<MongoId, TemplateItem> items)
    {
        try
        {
            if (string.Equals(config.MaskType, "Wide", StringComparison.InvariantCultureIgnoreCase) || string.Equals(config.MaskType, "Narrow", StringComparison.InvariantCultureIgnoreCase) || string.Equals(config.MaskType, "NoMask", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var faceShield in faceShields)
                {
                    var faceShieldProperties = items.TryGetValue(faceShield, out var faceShieldProperty);
                    
                    if (faceShieldProperty?.Properties == null)
                    {
                        return;
                    }

                    faceShieldProperty.Properties.FaceShieldMask = config.MaskType;
                }
                logger.Info($"[Change Helmet Visor] Changed the visor of all face shields to '{config.MaskType}'");
            }
            else
            {
                logger.Warning($"[Change Helmet Visor] Incorrect 'MaskType' property '{config.MaskType}' in config.jsonc. Skipping the changes");
            }
        }
        catch (Exception ex)
        {
            logger.Error($"[Change Helmet Visor] Could not load the helmet properties {ex.Message}");
        }
    }

    private void ChangeSpecificHelmetsVisors(ModConfig config, List<String> faceShields, Dictionary<MongoId, TemplateItem> items)
    {
        try
        {
            foreach (var faceShield in config.SpecificFaceShields)
            {
                var faceShieldId = faceShield.Key;
                var faceShieldMaskaType = faceShield.Value;
                if (faceShields.Contains(faceShieldId))
                {
                    var faceShieldProperties = items.TryGetValue(faceShieldId, out var faceShieldProperty);
                    if (faceShieldProperty?.Properties == null)
                    {
                        return;
                    }
                    else
                    {
                        if (string.Equals(faceShieldMaskaType, "Wide", StringComparison.InvariantCultureIgnoreCase) || string.Equals(faceShieldMaskaType, "Narrow", StringComparison.InvariantCultureIgnoreCase) || string.Equals(faceShieldMaskaType, "NoMask", StringComparison.InvariantCultureIgnoreCase))
                        {
                            faceShieldProperty.Properties.FaceShieldMask = faceShieldMaskaType;
                            logger.Info($"[Change Helmet Visor] Changed the visor for '{faceShieldProperty.Name}' to '{faceShieldMaskaType}'");
                        }
                        else
                        {
                            logger.Warning($"[Change Helmet Visor] Incorrect entry '{faceShieldMaskaType}' inside the config.jsonc under 'SpecificFaceShields' for '{faceShieldProperty.Name}' with id '{faceShieldId}'. Face shield will be skipped");
                        }
                    }
                }
                else
                {
                    logger.Warning($"[Change Helmet Visor] Provided ID '{faceShieldId}' is not a face shield, or it does not use a visor! Please provide an id for a valid face shield with a visor");
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error($"[Change Helmet Visor] Could not load the helmet properties {ex.Message}");
        }
    }

    private List<String> FindAllFaceShieldHelmets()
    {
        var items = databaseService.GetItems();
        var faceShieldHelmets = new List<String>();

        foreach (var item in items.Values)
        {
            if ((item.Parent == "57bef4c42459772e8d35a53b" || item.Parent == "5a341c4686f77469e155819e") && item.Properties?.FaceShieldComponent == true)
            {
                faceShieldHelmets.Add(item.Id);
            }
        }
        return faceShieldHelmets;
    }
}