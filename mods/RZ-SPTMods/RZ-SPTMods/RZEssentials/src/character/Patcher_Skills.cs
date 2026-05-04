// RemzDNB - 2026

using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Services;
using RZEssentials._Shared;

namespace RZEssentials.Character;

[Injectable(TypePriority = OnLoadOrder.PostSptModLoader)]
public class Patcher_Skills(
    DatabaseService databaseService,
    ConfigLoader configLoader,
    RzeLogger log
) : IOnLoad
{
    public Task OnLoad()
    {
        var config = configLoader.Load<SkillsConfig>();
        if (!config.Enabled || config.Settings.Count == 0)
            return Task.CompletedTask;

        var skillsSettings = databaseService.GetGlobals().Configuration.SkillsSettings;
        var skillBlockProps = skillsSettings.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.PropertyType.IsClass && p.PropertyType != typeof(string))
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        var patched = 0;
        var warnings = 0;

        foreach (var (blockName, values) in config.Settings)
        {
            if (!skillBlockProps.TryGetValue(blockName, out var blockProp))
            {
                log.Warning(LogChannel.Character, $"Skill block '{blockName}' not found on SkillsSettings : skipping.");
                warnings++;
                continue;
            }

            var block = blockProp.GetValue(skillsSettings);
            if (block is null)
            {
                log.Warning(LogChannel.Character, $"Skill block '{blockName}' is null at runtime : skipping.");
                warnings++;
                continue;
            }

            var valueProps = block.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite)
                .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var (propName, value) in values)
            {
                if (!valueProps.TryGetValue(propName, out var valueProp))
                {
                    log.Warning(LogChannel.Character, $"'{blockName}.{propName}' not found : skipping.");
                    warnings++;
                    continue;
                }

                try
                {
                    var underlying = Nullable.GetUnderlyingType(valueProp.PropertyType) ?? valueProp.PropertyType;
                    object? result = underlying switch
                    {
                        _ when underlying == typeof(double) => value,
                        _ when underlying == typeof(float) => (float)value,
                        _ when underlying == typeof(int) => (int)value,
                        _ when underlying == typeof(long) => (long)value,
                        _ when underlying == typeof(bool) => value != 0,
                        _ => null,
                    };

                    if (result is null)
                    {
                        log.Warning(LogChannel.Character, $"'{blockName}.{propName}': unsupported type '{underlying.Name}' : skipping.");
                        warnings++;
                        continue;
                    }

                    valueProp.SetValue(block, result);
                    patched++;
                }
                catch (Exception ex)
                {
                    log.Warning(LogChannel.Character, $"'{blockName}.{propName}': exception : {ex.Message}");
                    warnings++;
                }
            }
        }

        log.Info(LogChannel.Character, $"SkillsSettings: {patched} value(s) patched, {warnings} warning(s).");
        return Task.CompletedTask;
    }
}
