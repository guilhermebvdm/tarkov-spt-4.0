using BepInEx.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Reflection;

namespace ZGFueDkx.ZGCLib.Helpers
{
    internal class JsonSettingsFactory
    {
        public static JsonSerializerSettings GetJsonSerializerSettings(ManualLogSource? logger)
        {
            return new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Include,
                Error = (sender, args) => HandleError(args, logger)
            };
        }

        private static void HandleError(ErrorEventArgs args, ManualLogSource? logger)
        {
            logger?.LogDebug($"JSON Error at {args.ErrorContext.Path}: {args.ErrorContext.Error.Message}");

            args.ErrorContext.Handled = true;

            var currentObject = args.CurrentObject;
            if (currentObject is null)
            {
                return;
            }

            PropertyInfo? property = FindProperty(currentObject.GetType(), args.ErrorContext.Path);
            if (property is not null)
            {
                JsonUtils.JsonIgnoreErrorAttribute? ignoreAttr = property.GetCustomAttribute<JsonUtils.JsonIgnoreErrorAttribute>(true);
                if (ignoreAttr is not null)
                {
                    return;
                }
            }

            logger?.LogError($"JSON Error at {args.ErrorContext.Path}: {args.ErrorContext.Error.Message}");
        }

        private static PropertyInfo? FindProperty(Type type, string path)
        {
            string raw = path.Split('.').Last();
            string clean = raw.Split('[', ']')[0];

            return type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(prop =>
                {
                    var attr = prop.GetCustomAttribute<JsonPropertyAttribute>();
                    return attr?.PropertyName == clean || prop.Name == clean;
                });
        }
    }
}
