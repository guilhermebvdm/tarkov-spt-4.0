using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace SPT.Launcher.Helpers
{
    /// <summary>
    /// Item 030: catálogo dos itens da tela "Mods e Configs", parseado do manifesto do servidor
    /// (optionalMods[] / optionalConfigs[] + as categorias de cada eixo) e compartilhado entre o
    /// ProfileViewModel (que sincroniza e alimenta o catálogo + o resumo) e a ModsConfigsViewModel (que
    /// lista, agrupada por categoria). Name/description/categoria bilíngues (o servidor passa `string` OU
    /// `{ pt, en }`), resolvidos pelo idioma ativo no consumo.
    /// </summary>
    public static class ModsConfigCatalog
    {
        public sealed class Item
        {
            public string Id { get; init; }
            public bool IsOptionalConfig { get; init; }
            public string Category { get; init; } // id da categoria (pode ser null → grupo default)
            public string NamePt { get; init; }
            public string NameEn { get; init; }
            public string DescPt { get; init; }
            public string DescEn { get; init; }

            public string ResolveName(bool preferPt) => Pick(preferPt, NamePt, NameEn) ?? Id;
            public string ResolveDescription(bool preferPt) => Pick(preferPt, DescPt, DescEn) ?? "";

            internal static string Pick(bool preferPt, string pt, string en) =>
                preferPt ? (string.IsNullOrEmpty(pt) ? en : pt) : (string.IsNullOrEmpty(en) ? pt : en);
        }

        public sealed class Category
        {
            public string Id { get; init; }
            public int Order { get; init; }
            public string NamePt { get; init; }
            public string NameEn { get; init; }

            public string ResolveName(bool preferPt) => Item.Pick(preferPt, NamePt, NameEn) ?? Id;
        }

        public static IReadOnlyList<Item> OptionalMods { get; private set; } = new List<Item>();
        public static IReadOnlyList<Item> OptionalConfigs { get; private set; } = new List<Item>();
        public static IReadOnlyList<Category> OptionalModCategories { get; private set; } = new List<Category>();
        public static IReadOnlyList<Category> OptionalConfigCategories { get; private set; } = new List<Category>();

        /// <summary>Atualiza o catálogo a partir do manifesto (tolerante a ausência/shape).</summary>
        public static void UpdateFromManifest(JToken optionalModsToken, JToken optionalConfigsToken,
            JToken optionalModCategoriesToken, JToken optionalConfigCategoriesToken)
        {
            OptionalMods = ParseItems(optionalModsToken, isOptionalConfig: false);
            OptionalConfigs = ParseItems(optionalConfigsToken, isOptionalConfig: true);
            OptionalModCategories = ParseCategories(optionalModCategoriesToken);
            OptionalConfigCategories = ParseCategories(optionalConfigCategoriesToken);
        }

        private static List<Item> ParseItems(JToken token, bool isOptionalConfig)
        {
            var result = new List<Item>();
            if (token is not JArray array) return result;

            foreach (var entry in array.OfType<JObject>())
            {
                string id = entry.Value<string>("id");
                if (string.IsNullOrWhiteSpace(id)) continue;

                var (namePt, nameEn) = ReadLocalized(entry["name"]);
                var (descPt, descEn) = ReadLocalized(entry["description"]);

                result.Add(new Item
                {
                    Id = id,
                    IsOptionalConfig = isOptionalConfig,
                    Category = entry.Value<string>("category"),
                    NamePt = namePt,
                    NameEn = nameEn,
                    DescPt = descPt,
                    DescEn = descEn,
                });
            }

            return result;
        }

        private static List<Category> ParseCategories(JToken token)
        {
            var result = new List<Category>();
            if (token is not JArray array) return result;

            foreach (var entry in array.OfType<JObject>())
            {
                string id = entry.Value<string>("id");
                if (string.IsNullOrWhiteSpace(id)) continue;

                var (namePt, nameEn) = ReadLocalized(entry["name"]);
                result.Add(new Category
                {
                    Id = id,
                    Order = entry.Value<int?>("order") ?? 0,
                    NamePt = namePt,
                    NameEn = nameEn,
                });
            }

            return result;
        }

        /// <summary>Lê um campo que pode ser string simples OU objeto { pt, en }.</summary>
        private static (string pt, string en) ReadLocalized(JToken token)
        {
            if (token == null) return (null, null);
            if (token.Type == JTokenType.String)
            {
                string s = token.Value<string>();
                return (s, s); // string única serve os dois idiomas
            }
            if (token is JObject obj)
            {
                return (obj.Value<string>("pt"), obj.Value<string>("en"));
            }
            return (null, null);
        }
    }
}
