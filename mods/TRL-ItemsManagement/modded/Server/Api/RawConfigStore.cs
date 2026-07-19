using System.Text.Json;
using System.Text.Json.Nodes;

namespace TRLItemsManagement.Api;

/// <summary>
///     Shared raw-JSON I/O for the mod-owned override config files (trader sell/buy price, stock, ...).
///     Each config is a plain <c>traderId → tpl → { ... }</c> object the mod owns entirely — no SPT_Data
///     "original style" to preserve, so always 2-space indented + trailing <c>\n</c>. Tolerates a missing
///     file (empty object) and self-heals a corrupt (unparseable) one — backed up to a <c>.corrupt.bak</c>
///     sibling and treated as empty so the viewer never 500s on a hand-edit gone wrong; the next write
///     repairs it. Extracted from the identical helpers in <see cref="TraderPriceController"/> and
///     <see cref="StockController"/> (code-review CR-03).
/// </summary>
internal static class RawConfigStore
{
    /// <summary>Read into a response DTO: <c>traderId → tpl → JsonElement</c> (deep, detached copy).</summary>
    internal static Dictionary<string, Dictionary<string, JsonElement>> ReadForResponse(string path)
    {
        var root = ReadForWrite(path);
        var result = new Dictionary<string, Dictionary<string, JsonElement>>();
        foreach (var (traderId, tplMapNode) in root)
        {
            if (tplMapNode is not JsonObject tplMap)
            {
                continue;
            }

            var inner = new Dictionary<string, JsonElement>();
            foreach (var (tpl, ovr) in tplMap)
            {
                if (ovr is not null)
                {
                    inner[tpl] = JsonSerializer.Deserialize<JsonElement>(ovr.ToJsonString());
                }
            }

            result[traderId] = inner;
        }

        return result;
    }

    /// <summary>
    ///     Read for mutation. Missing file → empty object. Corrupt (unparseable) file → backed up to a
    ///     <c>.corrupt.bak</c> sibling and treated as empty, so the next write self-heals the config and
    ///     the viewer never 500s over a file it can't read either way.
    /// </summary>
    internal static JsonObject ReadForWrite(string path)
    {
        if (!System.IO.File.Exists(path))
        {
            return new JsonObject();
        }

        try
        {
            return JsonNode.Parse(System.IO.File.ReadAllText(path)) as JsonObject ?? new JsonObject();
        }
        catch
        {
            try
            {
                var backup = path + ".corrupt.bak";
                if (System.IO.File.Exists(backup))
                {
                    System.IO.File.Delete(backup);
                }

                System.IO.File.Move(path, backup);
            }
            catch
            {
                // Best-effort backup — even if it fails, still fall through and serve {} below.
            }

            return new JsonObject();
        }
    }

    /// <summary>Atomic (tmp+rename) write, 2-space indented + trailing newline.</summary>
    internal static void Write(string path, JsonObject overrides)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var serialized = overrides.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n";
        var tmp = path + ".tmp";
        System.IO.File.WriteAllText(tmp, serialized);
        System.IO.File.Move(tmp, path, overwrite: true);
    }
}
