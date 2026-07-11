namespace TRLItemsManagement;

/// <summary>
///     Copies the Node pipeline's output (<c>&lt;toolPath&gt;/data/*.json</c>) into the mod's own
///     install data folder (<c>user/mods/TRL-ItemsManagement/data/</c>) after a refresh/rescan script
///     finishes — the pipeline always writes to its OWN repo-relative <c>data/</c> (see
///     <c>scripts/refresh-item.js</c>'s <c>ROOT</c>), never directly to the install, so this copy step is
///     what makes a refresh/rescan actually reach <see cref="Api.DataController"/>'s served files.
/// </summary>
internal static class CatalogSync
{
    private static readonly string[] CatalogFiles = ["items.json", "categories.json", "traders.json", "meta.json", "hideout-crafts.json"];

    internal static void SyncFromPipeline(string toolPath, string installDataDir)
    {
        Directory.CreateDirectory(installDataDir);
        foreach (var fileName in CatalogFiles)
        {
            var src = Path.Combine(toolPath, "data", fileName);
            if (!File.Exists(src))
            {
                continue; // e.g. hideout-crafts.json may not exist on an older pipeline checkout
            }

            File.Copy(src, Path.Combine(installDataDir, fileName), overwrite: true);
        }
    }
}
