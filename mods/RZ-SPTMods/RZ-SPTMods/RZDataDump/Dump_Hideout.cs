// RemzDNB - 2026

using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Enums.Hideout;
using SPTarkov.Server.Core.Services;

namespace RZDataDump;

// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
// HideoutDumper
//
// Reads every hideout area from the live database and dumps a ready-to-use
// hideoutConfig.json skeleton following the RZCustomEconomy format.
//
// Output : user/mods/RZDataDump/dev/hideout_dump.json
//
// Defaults : RemoveFromDb = false, Enabled = true, DisplayLevel = true,
//            all UseCustom* = true, all requirement dicts populated with vanilla values.
// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

[Injectable(TypePriority = OnLoadOrder.PostSptModLoader)]
public class HideoutDumper(
    ILogger<HideoutDumper> logger,
    DatabaseService databaseService,
    ConfigLoader configLoader
) : IOnLoad
{
    private static readonly string _devDir = Path.Combine(
        AppContext.BaseDirectory, "user", "mods", "RZDataDump", "dev"
    );

    public Task OnLoad()
    {
        var config = configLoader.Load<MasterConfig>(MasterConfig.FileName, Assembly.GetExecutingAssembly());

        if (!config.DumpHideoutEnabled)
            return Task.CompletedTask;

        DumpHideout();

        return Task.CompletedTask;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DumpHideout
    // ─────────────────────────────────────────────────────────────────────────

    private void DumpHideout()
    {
        var areas = databaseService.GetTables().Hideout?.Areas;
        if (areas is null || areas.Count == 0)
        {
            logger.LogWarning("[RZDataDump] No hideout areas found in database — cannot dump hideout.");
            return;
        }

        var traderNicknames = BuildTraderNicknameMap();

        var areaTypeNames = Enum.GetValues<HideoutAreas>()
            .ToDictionary(e => (int)e, e => e.ToString());

        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine("  \"Areas\": {");
        sb.AppendLine();

        var sortedAreas = areas
            .Where(a => a.Type.HasValue)
            .OrderBy(a => (int)a.Type!.Value)
            .ToList();

        for (var i = 0; i < sortedAreas.Count; i++)
        {
            WriteAreaBlock(sb, sortedAreas[i], sortedAreas[i].Type!.Value.ToString(),
                traderNicknames, areaTypeNames, isLast: i == sortedAreas.Count - 1);
        }

        sb.AppendLine("  }");
        sb.Append("}");

        var outputPath = Path.Combine(_devDir, "hideout_dump.json");
        try
        {
            Directory.CreateDirectory(_devDir);
            File.WriteAllText(outputPath, sb.ToString());
            logger.LogInformation(
                "\e[1;32m[RZDataDump] {Count} hideout area(s) dumped to dev/hideout_dump.json.\e[0m",
                sortedAreas.Count
            );
        }
        catch (Exception ex)
        {
            logger.LogWarning("[RZDataDump] Failed to write hideout_dump.json: {Err}", ex.Message);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // WriteAreaBlock
    // ─────────────────────────────────────────────────────────────────────────

    private static void WriteAreaBlock(
        StringBuilder sb,
        HideoutArea area,
        string areaName,
        Dictionary<string, string> traderNicknames,
        Dictionary<int, string> areaTypeNames,
        bool isLast)
    {
        const string I  = "    ";   // inside "Areas": {}
        const string II = "      "; // inside area block

        sb.AppendLine($"{I}// ── {areaName} ──────────────────────────────────────────────────────────────");
        sb.AppendLine($"{I}\"{areaName}\": {{");

        sb.AppendLine($"{II}\"RemoveFromDb\": false,");
        sb.AppendLine($"{II}\"Enabled\": true,");
        sb.AppendLine($"{II}\"DisplayLevel\": true,");

        var stages = area.Stages;

        // ── Construction time ──────────────────────────────────────────────
        sb.AppendLine($"{II}\"UseCustomConstructionTime\": true,");
        WriteConstructionTime(sb, stages, II);

        // ── Item requirements ──────────────────────────────────────────────
        sb.AppendLine($"{II}\"UseCustomItemRequirements\": true,");
        WriteRequirementsBlock(sb, stages, "ItemRequirements", II, trailingComma: true, req =>
            req.Type == "Item"
                ? $"{{ \"ItemTpl\": \"{req.TemplateId}\", \"ItemCount\": {req.Count ?? 1} }}"
                : null
        );

        // ── Area requirements ──────────────────────────────────────────────
        sb.AppendLine($"{II}\"UseCustomAreaRequirements\": true,");
        WriteRequirementsBlock(sb, stages, "AreaRequirements", II, trailingComma: true, req =>
        {
            if (req.Type != "Area" || !req.AreaType.HasValue) return null;
            var depName = areaTypeNames.TryGetValue(req.AreaType.Value, out var n) ? n : req.AreaType.Value.ToString();
            return $"{{ \"AreaName\": \"{depName}\", \"AreaLevel\": {req.RequiredLevel ?? 0} }}";
        });

        // ── Skill requirements ─────────────────────────────────────────────
        sb.AppendLine($"{II}\"UseCustomSkillRequirements\": true,");
        WriteRequirementsBlock(sb, stages, "SkillRequirements", II, trailingComma: true, req =>
            req.Type == "Skill" && !string.IsNullOrEmpty(req.SkillName)
                ? $"{{ \"SkillName\": \"{req.SkillName}\", \"SkillLevel\": {req.SkillLevel ?? 0} }}"
                : null
        );

        // ── Trader requirements — last field, no trailing comma on block ───
        sb.AppendLine($"{II}\"UseCustomTraderRequirements\": true,");
        WriteTraderRequirementsBlock(sb, stages, II, traderNicknames);

        sb.AppendLine($"{I}}}{(isLast ? "" : ",")}");
        sb.AppendLine();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // WriteConstructionTime
    // ─────────────────────────────────────────────────────────────────────────

    private static void WriteConstructionTime(StringBuilder sb, Dictionary<string, Stage>? stages, string indent)
    {
        // Stage "0" is the unbuilt state — construction time is irrelevant there.
        var relevant = stages?
            .Where(kvp => kvp.Key != "0" && (kvp.Value.ConstructionTime ?? 0) > 0)
            .OrderBy(kvp => int.TryParse(kvp.Key, out var n) ? n : 999)
            .ToList() ?? [];

        if (relevant.Count == 0)
        {
            sb.AppendLine($"{indent}\"ConstructionTime\": {{}},");
            return;
        }

        sb.AppendLine($"{indent}\"ConstructionTime\": {{");
        for (var i = 0; i < relevant.Count; i++)
        {
            var (level, stage) = relevant[i];
            var comma = i < relevant.Count - 1 ? "," : "";
            sb.AppendLine($"{indent}  \"{level}\": {(int)(stage.ConstructionTime ?? 0)}{comma}");
        }
        sb.AppendLine($"{indent}}},");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // WriteRequirementsBlock  (Item / Area / Skill)
    // ─────────────────────────────────────────────────────────────────────────

    private static void WriteRequirementsBlock(
        StringBuilder sb,
        Dictionary<string, Stage>? stages,
        string blockName,
        string indent,
        bool trailingComma,
        Func<StageRequirement, string?> formatter)
    {
        var tc = trailingComma ? "," : "";

        var populated = stages?
            .Where(kvp => kvp.Key != "0")
            .Select(kvp => (
                Level:   kvp.Key,
                Entries: (kvp.Value.Requirements ?? [])
                             .Select(formatter)
                             .Where(s => s is not null)
                             .Cast<string>()
                             .ToList()
            ))
            .Where(x => x.Entries.Count > 0)
            .OrderBy(x => int.TryParse(x.Level, out var n) ? n : 999)
            .ToList() ?? [];

        if (populated.Count == 0)
        {
            sb.AppendLine($"{indent}\"{blockName}\": {{}}{tc}");
            return;
        }

        sb.AppendLine($"{indent}\"{blockName}\": {{");
        for (var i = 0; i < populated.Count; i++)
        {
            var (level, entries) = populated[i];
            var levelComma = i < populated.Count - 1 ? "," : "";
            sb.AppendLine($"{indent}  \"{level}\": [");
            for (var j = 0; j < entries.Count; j++)
            {
                var entryComma = j < entries.Count - 1 ? "," : "";
                sb.AppendLine($"{indent}    {entries[j]}{entryComma}");
            }
            sb.AppendLine($"{indent}  ]{levelComma}");
        }
        sb.AppendLine($"{indent}}}{tc}");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // WriteTraderRequirementsBlock
    // Resolves traderId → nickname so the output matches RZCustomEconomy's format.
    // Last field in every area block — no trailing comma on the closing brace.
    // ─────────────────────────────────────────────────────────────────────────

    private static void WriteTraderRequirementsBlock(
        StringBuilder sb,
        Dictionary<string, Stage>? stages,
        string indent,
        Dictionary<string, string> traderNicknames)
    {
        var populated = stages?
            .Where(kvp => kvp.Key != "0")
            .Select(kvp => (
                Level:   kvp.Key,
                Entries: (kvp.Value.Requirements ?? [])
                             .Where(r => r.Type == "TraderLoyalty" && r.TraderId != null)
                             .Select(r =>
                             {
                                 var id = r.TraderId.ToString();
                                 var nickname = traderNicknames.TryGetValue(id, out var n) ? n : id;
                                 return $"{{ \"TraderName\": \"{nickname}\", \"TraderLoyalty\": {r.LoyaltyLevel ?? 1} }}";
                             })
                             .ToList()
            ))
            .Where(x => x.Entries.Count > 0)
            .OrderBy(x => int.TryParse(x.Level, out var n) ? n : 999)
            .ToList() ?? [];

        if (populated.Count == 0)
        {
            sb.AppendLine($"{indent}\"TraderRequirements\": {{}}");
            return;
        }

        sb.AppendLine($"{indent}\"TraderRequirements\": {{");
        for (var i = 0; i < populated.Count; i++)
        {
            var (level, entries) = populated[i];
            var levelComma = i < populated.Count - 1 ? "," : "";
            sb.AppendLine($"{indent}  \"{level}\": [");
            for (var j = 0; j < entries.Count; j++)
            {
                var entryComma = j < entries.Count - 1 ? "," : "";
                sb.AppendLine($"{indent}    {entries[j]}{entryComma}");
            }
            sb.AppendLine($"{indent}  ]{levelComma}");
        }
        sb.AppendLine($"{indent}}}");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BuildTraderNicknameMap  traderId → nickname
    // ─────────────────────────────────────────────────────────────────────────

    private Dictionary<string, string> BuildTraderNicknameMap()
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            foreach (var (id, trader) in databaseService.GetTraders())
            {
                var nickname = trader.Base?.Nickname;
                if (!string.IsNullOrEmpty(nickname))
                    map[id.ToString()] = nickname;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning("[RZDataDump] Could not build trader nickname map: {Err}", ex.Message);
        }
        return map;
    }
}
