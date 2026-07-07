using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Utils;   // JsonUtil

namespace TRLItemsManagement;

/// <summary>
///     Recomputes SPT's own <c>SPT_Data/checks.dat</c> integrity manifest after this mod writes to a
///     file SPT tracks (<c>configs/ragfair.json</c>, <c>database/globals.json</c>,
///     <c>database/templates/items.json</c>) — without this, SPT logs a file-validation failure on every
///     boot after such an edit. Ported from <c>serve.js</c>'s <c>updateSptChecks()</c>.
///     <para>
///     <c>checks.dat</c> is base64(JSON array of <c>{Path, Hash}</c>, 2-space indent) + a trailing
///     newline; <c>Path</c> is forward-slash, relative to <c>SPT_Data/</c> (e.g. <c>configs/ragfair.json</c>).
///     <c>Hash</c> is an uppercase-hex MD5 of the file's raw bytes.
///     </para>
/// </summary>
[Injectable(InjectionType.Singleton)]
public class SptChecksService(SptDataPathsService sptPaths, JsonUtil jsonUtil)
{
    private sealed record ChecksEntry
    {
        [JsonPropertyName("Path")] public string Path { get; set; } = "";
        [JsonPropertyName("Hash")] public string Hash { get; set; } = "";
    }

    public readonly record struct UpdateResult(bool Ok, string? Error, IReadOnlyList<string> Updated);

    /// <summary>
    ///     <paramref name="relativePaths"/>: SPT_Data-relative, forward-slash paths (e.g.
    ///     <c>"configs/ragfair.json"</c>) — the exact strings <c>checks.dat</c> itself uses as keys.
    ///     A path absent from the manifest is silently skipped (mirrors the Node original — not every
    ///     file SPT reads is necessarily integrity-checked).
    /// </summary>
    public UpdateResult Update(params string[] relativePaths)
    {
        if (!System.IO.File.Exists(sptPaths.ChecksDatPath))
        {
            return new UpdateResult(false, "checks.dat not found", Array.Empty<string>());
        }

        var raw = System.IO.File.ReadAllText(sptPaths.ChecksDatPath);
        var manifestJson = Encoding.UTF8.GetString(Convert.FromBase64String(raw.Trim()));
        var manifest = jsonUtil.Deserialize<List<ChecksEntry>>(manifestJson);
        if (manifest is null)
        {
            return new UpdateResult(false, "checks.dat manifest failed to parse", Array.Empty<string>());
        }

        var updated = new List<string>();
        foreach (var relPath in relativePaths)
        {
            var absPath = Path.Combine(sptPaths.SptDataDir, relPath.Replace('/', Path.DirectorySeparatorChar));
            if (!System.IO.File.Exists(absPath))
            {
                continue;
            }

            var entry = manifest.Find(e => e.Path == relPath);
            if (entry is null)
            {
                continue; // not tracked by the manifest — nothing to update
            }

            var bytes = System.IO.File.ReadAllBytes(absPath);
            var hash = Convert.ToHexString(MD5.HashData(bytes)); // Convert.ToHexString is already uppercase
            if (entry.Hash != hash)
            {
                entry.Hash = hash;
                updated.Add(relPath);
            }
        }

        var reencoded = jsonUtil.Serialize(manifest, indented: true) ?? "[]";
        var reencodedBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(reencoded)) + "\n";
        System.IO.File.WriteAllText(sptPaths.ChecksDatPath, reencodedBase64, new UTF8Encoding(false));

        return new UpdateResult(true, null, updated);
    }
}
