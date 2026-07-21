using System;
using System.Linq;
using System.Reflection;
using BepInEx.Bootstrap;

namespace DiscordRaidMap.RaidMap
{
    internal static class HostCheck
    {
        private const string FikaPluginGuid = "com.fika.core";
        private static readonly Lazy<PropertyInfo> FikaIsServerProperty = new(FindFikaIsServerProperty);

        internal static bool CanBroadcast()
        {
            if (!Chainloader.PluginInfos.ContainsKey(FikaPluginGuid))
            {
                return true;
            }

            var isServerProperty = FikaIsServerProperty.Value;
            if (isServerProperty == null)
            {
                return false;
            }

            return isServerProperty.GetValue(null) is true;
        }

        private static Assembly FindFikaCoreAssembly()
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(assembly => assembly.GetName().Name == "Fika.Core");
        }

        private static PropertyInfo FindFikaIsServerProperty()
        {
            var backendUtils = FindFikaCoreAssembly()?.GetType("Fika.Core.Main.Utils.FikaBackendUtils");
            return backendUtils?.GetProperty("IsServer", BindingFlags.Public | BindingFlags.Static);
        }
    }
}
