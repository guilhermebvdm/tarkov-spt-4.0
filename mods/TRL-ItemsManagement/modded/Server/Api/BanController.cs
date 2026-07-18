using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using SPTarkov.Server.Core.Models.Common;   // MongoId
using SPTarkov.Server.Core.Services;        // DatabaseService

namespace TRLItemsManagement.Api;

public sealed record SetBanRequest(string? Tpl, bool? Banned);

/// <summary>
///     Flea ban/unban toggle — two paths, gated on whether the tpl exists in SPT's own
///     <c>SPT_Data/database/templates/items.json</c> (~19 MB):
///     <list type="bullet">
///     <item>Vanilla item (present in that file): rewrite its <c>_props.CanSellOnRagfair</c> there —
///     NOT <c>ragfair.json</c> (SPT 4.0 dropped the per-tpl custom blacklist from its config model; the
///     item template itself is the only mechanism SPT honors). Ported from <c>serve.js</c>'s
///     <c>handleBanToggle</c>.</item>
///     <item>Mod item (absent from that file — mod items are injected into the live database during
///     <c>PostDBModLoader</c>, AFTER this file is already loaded from disk, so they can NEVER be banned
///     via a file rewrite): delegate to <see cref="ModItemBanService"/>, which persists to its own
///     <c>config/mod-item-bans.json</c> and mutates the LIVE database immediately, so the ban applies to
///     the current session too, not just after a restart.</item>
///     </list>
///     <para>
///     Precondition (checked UNCONDITIONALLY, before branching into vanilla vs. mod-item — applies to
///     BOTH paths, since <c>enableBsgList:false</c> makes SPT ignore <c>CanSellOnRagfair</c> regardless of
///     which item it lives on): must be <c>true</c> (default — missing/absent counts as enabled, only an
///     explicit <c>false</c> disables it), or the toggle would silently no-op in-game. Checked and
///     REFUSED (409) for either path, never applied as a silent no-op.
///     </para>
/// </summary>
[ApiController]
[Route("TRLItemsManagement-Server/api")]
public sealed class BanController(
    SptDataPathsService sptPaths,
    ModPathsService modPaths,
    WriteLockService writeLock,
    SptChecksService checksService,
    ModItemBanService modItemBanService,
    DatabaseService databaseService,
    AuditLogService auditLog) : ControllerBase
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
                    error = "enableBsgList is false in ragfair.json - CanSellOnRagfair toggles would be ignored by SPT",
                }));
            }

            var sptItemsRoot = JsonNode.Parse(System.IO.File.ReadAllText(sptPaths.ItemsJsonPath)) as JsonObject;
            if (sptItemsRoot?[tpl] is JsonObject sptEntry && sptEntry["_props"] is JsonObject props)
            {
                var wasBannedVanilla = props["CanSellOnRagfair"]?.GetValue<bool?>() == false;
                if (wasBannedVanilla == banned)
                {
                    // Already in the requested state — no-op. Don't rewrite the ~19 MB items.json
                    // + ~17 MB cache + checks.dat, and don't log a spurious "banned: X→X" entry
                    // carrying a live Undo button. "Only touch disk on a real mutation."
                    return Task.FromResult<IActionResult>(Ok(new
                    {
                        ok = true, tpl, banned, wasBanned = wasBannedVanilla, modItem = false, changed = false,
                    }));
                }

                props["CanSellOnRagfair"] = !banned;

                WriteSptItemsJson(sptItemsRoot);
                var checksResult = checksService.Update("database/templates/items.json");

                PatchCatalogBan(tpl, banned, modItem: false);
                auditLog.Append("ban", "set", tpl, before: new { banned = wasBannedVanilla }, after: new { banned }, extra: new { modItem = false });

                return Task.FromResult<IActionResult>(Ok(new
                {
                    ok = true,
                    tpl,
                    banned,
                    wasBanned = wasBannedVanilla,
                    modItem = false,
                    checks = new { ok = checksResult.Ok, updated = checksResult.Updated },
                }));
            }

            // Not in the vanilla file — check the LIVE database (includes mod items, injected after
            // this file was loaded). If it's not there either, the tpl is genuinely unknown.
            if (!databaseService.GetItems().TryGetValue(new MongoId(tpl), out var liveTemplate) || liveTemplate.Properties is null)
            {
                return Task.FromResult<IActionResult>(NotFound(new { error = "tpl not found in SPT items.json nor in the live database" }));
            }

            var wasBannedMod = liveTemplate.Properties.CanSellOnRagfair == false;
            if (wasBannedMod == banned)
            {
                // Already in the requested state — no-op (see the vanilla branch above).
                return Task.FromResult<IActionResult>(Ok(new
                {
                    ok = true, tpl, banned, wasBanned = wasBannedMod, modItem = true, changed = false,
                }));
            }

            modItemBanService.SetBanned(tpl, banned);
            liveTemplate.Properties.CanSellOnRagfair = !banned; // apply to the CURRENT session immediately too

            PatchCatalogBan(tpl, banned, modItem: true);
            auditLog.Append("ban", "set", tpl, before: new { banned = wasBannedMod }, after: new { banned }, extra: new { modItem = true });

            return Task.FromResult<IActionResult>(Ok(new
            {
                ok = true,
                tpl,
                banned,
                wasBanned = wasBannedMod,
                modItem = true,
            }));
        });
    }

    /// <summary>
    ///     Keeps the mod's own <c>data/items.json</c> cache (<c>spt.fleaBanned</c>/<c>fleaBanReasons</c>,
    ///     read by the viewer's ban icon/filter) in sync with the ban just applied above — otherwise the
    ///     viewer shows a stale (not-banned) state for this tpl until the next manual rescan, the same
    ///     "known interim gap" <see cref="FleaPriceController"/> had for price edits. Reasons mirror
    ///     <c>load-spt.js</c>'s derivation: a vanilla ban flips <c>CanSellOnRagfair</c> → <c>'bsg'</c>; a
    ///     mod-item ban goes through <see cref="ModItemBanService"/>'s custom-ban list → <c>'custom'</c>.
    ///     Best-effort (try/catch): a cache-write failure here must never turn the already-successful ban
    ///     write above into an error response.
    /// </summary>
    private void PatchCatalogBan(string tpl, bool banned, bool modItem)
    {
        try
        {
            var itemsCatalogPath = Path.Combine(modPaths.DataDir, "items.json");
            if (!System.IO.File.Exists(itemsCatalogPath))
            {
                return;
            }

            var itemsRoot = JsonNode.Parse(System.IO.File.ReadAllText(itemsCatalogPath)) as JsonObject;
            // Not "?? new JsonObject()" — a genuinely missing spt block would be a catalog anomaly
            // (every real item has one), and fabricating an empty one here would silently wipe every
            // OTHER cached spt.* field (fleaPrice, fleaFloor, basePrice, ...) the next time this gets
            // serialized. Bail out instead, same as "tpl not in the catalog at all".
            if (itemsRoot?[tpl] is not JsonObject item || item["spt"] is not JsonObject sptBlock)
            {
                return;
            }

            sptBlock["fleaBanned"] = banned;
            var reasons = new JsonArray();
            if (banned)
            {
                reasons.Add(modItem ? "custom" : "bsg");
            }

            sptBlock["fleaBanReasons"] = reasons;

            ItemCatalogPatcher.WriteBack(itemsCatalogPath, itemsRoot);
        }
        catch
        {
            // best-effort — see ItemCatalogPatcher's class doc
        }
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
