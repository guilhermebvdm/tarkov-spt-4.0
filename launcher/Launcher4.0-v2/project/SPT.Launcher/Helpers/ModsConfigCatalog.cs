using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace SPT.Launcher.Helpers
{
    /// <summary>
    /// Item 030: catálogo dos itens da tela "Mods e Configs", parseado do manifesto do servidor
    /// (optionalMods[] / performanceItems[]) e compartilhado entre o ProfileViewModel (que sincroniza e
    /// alimenta o catálogo + o resumo) e a ModsConfigsViewModel (que lista). Cada item tem name/description
    /// bilíngue (o servidor passa `string` OU `{ pt, en }`), resolvido pelo idioma ativo no consumo.
    /// </summary>
    public static class ModsConfigCatalog
    {
        public sealed class Item
        {
            public string Id { get; init; }
            public bool IsPerformance { get; init; }
            public string NamePt { get; init; }
            public string NameEn { get; init; }
            public string DescPt { get; init; }
            public string DescEn { get; init; }

            public string ResolveName(bool preferPt) => Pick(preferPt, NamePt, NameEn) ?? Id;
            public string ResolveDescription(bool preferPt) => Pick(preferPt, DescPt, DescEn) ?? "";

            private static string Pick(bool preferPt, string pt, string en) =>
                preferPt ? (string.IsNullOrEmpty(pt) ? en : pt) : (string.IsNullOrEmpty(en) ? pt : en);
        }

        public static IReadOnlyList<Item> OptionalMods { get; private set; } = new List<Item>();
        public static IReadOnlyList<Item> PerformanceItems { get; private set; } = new List<Item>();

        /// <summary>Atualiza o catálogo a partir dos dois arrays do manifesto (tolerante a ausência/shape).</summary>
        public static void UpdateFromManifest(JToken optionalModsToken, JToken performanceItemsToken)
        {
            OptionalMods = Parse(optionalModsToken, isPerformance: false);
            PerformanceItems = Parse(performanceItemsToken, isPerformance: true);
        }

        private static List<Item> Parse(JToken token, bool isPerformance)
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
                    IsPerformance = isPerformance,
                    NamePt = namePt,
                    NameEn = nameEn,
                    DescPt = descPt,
                    DescEn = descEn,
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
