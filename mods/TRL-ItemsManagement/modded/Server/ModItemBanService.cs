using System.Text.Json;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;   // MongoId
using SPTarkov.Server.Core.Services;        // DatabaseService

namespace TRLItemsManagement;

/// <summary>
///     Ban/unban for MOD items — a gap <see cref="Api.BanController"/>'s file-based approach cannot
///     cover: mod items (added by each mod's own <c>db/</c> folder) are injected into the live database
///     during <c>PostDBModLoader</c>, AFTER <c>SPT_Data/database/templates/items.json</c> is already
///     loaded from disk — they never exist in that file, so writing to it can only ever ban/unban
///     VANILLA items (confirmed: the same limitation exists in <c>serve.js</c>'s original
///     <c>handleBanToggle</c>, ported faithfully — not a regression introduced here).
///     <para>
///     Persists to <c>config/mod-item-bans.json</c> (<c>{ "&lt;tpl&gt;": true, ... }</c> — only banned
///     tpls are stored, absence = not banned) — the mod's own data, same pattern as
///     <c>overrides.json</c>/<c>buy-overrides.json</c>. <see cref="ModItemBanOnLoad"/> applies it at
///     boot (mutates <see cref="DatabaseService.GetItems"/> live, same "boot-time mutation" pattern as
///     <see cref="Pricing.SellPriceApplier"/>); <see cref="Api.BanController"/> ALSO mutates the live
///     object immediately on write, so a ban/unban of a mod item takes effect in the current session too,
///     not just after a restart.
///     </para>
/// </summary>
[Injectable(InjectionType.Singleton)]
public class ModItemBanService(ModPathsService modPaths)
{
    private string ConfigPath => Path.Combine(modPaths.ConfigDir, "mod-item-bans.json");

    internal Dictionary<string, bool> ReadAll()
    {
        if (!File.Exists(ConfigPath))
        {
            return new Dictionary<string, bool>();
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, bool>>(File.ReadAllText(ConfigPath))
                ?? new Dictionary<string, bool>();
        }
        catch
        {
            return new Dictionary<string, bool>();
        }
    }

    internal void SetBanned(string tpl, bool banned)
    {
        var all = ReadAll();
        if (banned)
        {
            all[tpl] = true;
        }
        else
        {
            all.Remove(tpl);
        }

        Directory.CreateDirectory(modPaths.ConfigDir);
        var serialized = JsonSerializer.Serialize(all, new JsonSerializerOptions { WriteIndented = true }) + "\n";
        var tmp = ConfigPath + ".tmp";
        File.WriteAllText(tmp, serialized);
        File.Move(tmp, ConfigPath, overwrite: true);
    }

    /// <summary>Applies every configured mod-item ban to the LIVE database — used at boot (<see cref="ModItemBanOnLoad"/>) and, for the tpl just written, immediately by <see cref="Api.BanController"/> so the current session reflects the change too.</summary>
    internal static int Apply(DatabaseService databaseService, IReadOnlyDictionary<string, bool> bans)
    {
        var applied = 0;
        var liveItems = databaseService.GetItems();
        foreach (var (tplStr, banned) in bans)
        {
            if (!banned)
            {
                continue;
            }

            if (liveItems.TryGetValue(new MongoId(tplStr), out var template) && template.Properties is not null)
            {
                template.Properties.CanSellOnRagfair = false;
                applied++;
            }
        }

        return applied;
    }
}
