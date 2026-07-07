using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace TRLItemsManagement.Api;

public sealed record SetBanRequest(string? Tpl, bool? Banned);

/// <summary>
///     Flea ban/unban toggle. Rewrites <c>_props.CanSellOnRagfair</c> directly on SPT's own
///     <c>SPT_Data/database/templates/items.json</c> (~19 MB) — NOT <c>ragfair.json</c>: SPT 4.0 dropped
///     the per-tpl custom blacklist list from its config model (only <c>EnableBsgList</c>/
///     <c>EnableQuestList</c>/<c>CustomItemCategoryList</c> survive there), so <c>CanSellOnRagfair</c> on
///     the item template itself is the only mechanism SPT actually honors for a per-tpl ban. Ported from
///     <c>serve.js</c>'s <c>handleBanToggle</c>.
///     <para>
///     Precondition: <c>ragfair.json:dynamic.blacklist.enableBsgList</c> must be <c>true</c> (the
///     default — missing/absent counts as enabled, only an explicit <c>false</c> disables it), or SPT
///     ignores <c>CanSellOnRagfair</c> entirely and the toggle would silently no-op in-game. Checked and
///     REFUSED (409), never applied as a silent no-op.
///     </para>
///     <para>
///     This file uses CRLF + 2-space indent (confirmed on-disk, unlike <c>ragfair.json</c>'s tabs) —
///     hardcoded here rather than run through <see cref="StyleSensitiveJsonWriter"/>'s sniffing, mirroring
///     <c>serve.js</c>'s own hardcoded <c>.replace(/\n/g, '\r\n')</c> for this specific file.
///     </para>
/// </summary>
[ApiController]
[Route("TRLItemsManagement-Server/api")]
public sealed class BanController(
    SptDataPathsService sptPaths,
    WriteLockService writeLock,
    SptChecksService checksService) : ControllerBase
{
    private static readonly Regex TplPattern = new("^[a-f0-9]{24}$", RegexOptions.IgnoreCase);

    [HttpPost("ban")]
    public Task<IActionResult> SetBan([FromBody] SetBanRequest body)
    {
        if (body.Tpl is not { } tpl || !TplPattern.IsMatch(tpl))
        {
            return Task.FromResult<IActionResult>(BadRequest(new { error = "invalid tpl (expected 24-char hex BSG id)" }));
        }

        var banned = body.Banned ?? false;

        return writeLock.RunAsync(() =>
        {
            var ragfairRoot = JsonNode.Parse(System.IO.File.ReadAllText(sptPaths.RagfairConfigPath)) as JsonObject;
            var enableBsgList = ragfairRoot?["dynamic"]?["blacklist"]?["enableBsgList"]?.GetValue<bool?>();
            if (enableBsgList == false)
            {
                return Task.FromResult<IActionResult>(Conflict(new
                {
                    error = "enableBsgList is false in ragfair.json — CanSellOnRagfair toggles would be ignored by SPT",
                }));
            }

            var sptItemsRoot = JsonNode.Parse(System.IO.File.ReadAllText(sptPaths.ItemsJsonPath)) as JsonObject;
            if (sptItemsRoot?[tpl] is not JsonObject sptEntry || sptEntry["_props"] is not JsonObject props)
            {
                return Task.FromResult<IActionResult>(NotFound(new { error = "tpl not in SPT items.json" }));
            }

            var wasBanned = props["CanSellOnRagfair"]?.GetValue<bool?>() == false;
            props["CanSellOnRagfair"] = !banned;

            WriteSptItemsJson(sptItemsRoot);
            var checksResult = checksService.Update("database/templates/items.json");

            return Task.FromResult<IActionResult>(Ok(new
            {
                ok = true,
                tpl,
                banned,
                wasBanned,
                checks = new { ok = checksResult.Ok, updated = checksResult.Updated },
            }));
        });
    }

    /// <summary>Atomic (tmp+rename) write, forcing CRLF + 2-space indent — see class doc.</summary>
    private void WriteSptItemsJson(JsonNode root)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
        {
            Indented = true,
            IndentSize = 2,
            IndentCharacter = ' ',
            // See StyleSensitiveJsonWriter's doc-comment — same non-ASCII-escaping avoidance, doubly
            // important here since this file is full of literal Cyrillic/other-locale item descriptions.
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        }))
        {
            root.WriteTo(writer);
        }

        // Normalize to bare \n first (in case the writer already emitted CRLF on this platform — see
        // StyleSensitiveJsonWriter's doc-comment for why that assumption can't be trusted), THEN convert
        // every \n to \r\n in one deterministic pass — avoids a \r\r\n double-CR artifact either way.
        var normalized = Encoding.UTF8.GetString(stream.ToArray()).Replace("\r\n", "\n");
        var crlf = normalized.Replace("\n", "\r\n");

        var tmp = sptPaths.ItemsJsonPath + ".tmp";
        System.IO.File.WriteAllText(tmp, crlf, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        System.IO.File.Move(tmp, sptPaths.ItemsJsonPath, overwrite: true);
    }
}
