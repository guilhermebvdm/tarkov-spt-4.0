using System.Text.Json;
using System.Text.Json.Nodes;

namespace TRLItemsManagement;

/// <summary>
///     Writes <c>data/items.json</c> back to disk after a controller has mutated a tpl's cached
///     <c>spt.*</c> view in memory. Shared ONLY for the write-back mechanics (serialize + tmp+rename) —
///     each caller does its own read + field patch, since what gets patched differs per feature
///     (flea price fields vs. ban fields) and some callers (<c>FleaPriceController</c>) already have
///     the parsed <c>JsonObject</c> in memory from their own validation reads, so forcing a shared
///     "read+patch+write" method would mean a wasted second parse of a ~17 MB file for them.
///     <para>
///     Best-effort by design: every caller wraps this in try/catch (same reasoning as
///     <c>AuditLogService.Append</c>/<c>FleaLevelController.MirrorIntoMeta</c>) — a failure to refresh
///     this DISPLAY cache must never turn an already-successful game-facing write (ragfair.json /
///     SPT_Data items.json / mod-item-bans.json) into a 500 for the caller. Worst case on failure: the
///     viewer's list shows a stale price/ban state for that one tpl until the next manual rescan —
///     exactly the pre-existing "known interim gap," not a new failure mode.
///     </para>
/// </summary>
internal static class ItemCatalogPatcher
{
    internal static void WriteBack(string itemsCatalogPath, JsonObject itemsRoot)
    {
        var serialized = itemsRoot.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n";
        var tmp = itemsCatalogPath + ".tmp";
        File.WriteAllText(tmp, serialized);
        File.Move(tmp, itemsCatalogPath, overwrite: true);
    }
}
