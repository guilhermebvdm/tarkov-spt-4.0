// RemzDNB - 2026

using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace RZEssentials._Shared;

public static class ItemUtils
{
    public static HashSet<string> BuildCategoryTplSet(string categoryId, Dictionary<MongoId, TemplateItem> items)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (mongoId, item) in items)
        {
            if (item.Properties is null) continue;

            var current = item.Parent.ToString();
            while (!string.IsNullOrEmpty(current))
            {
                if (current == categoryId)
                {
                    result.Add(mongoId.ToString());
                    break;
                }
                if (!items.TryGetValue(current, out var parent)) break;
                current = parent.Parent.ToString();
            }
        }

        return result;
    }

    public static HashSet<string> BuildCategoryTplSet(IEnumerable<string> categoryIds, Dictionary<MongoId, TemplateItem> items)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var categoryId in categoryIds)
            result.UnionWith(BuildCategoryTplSet(categoryId, items));
        return result;
    }
}
