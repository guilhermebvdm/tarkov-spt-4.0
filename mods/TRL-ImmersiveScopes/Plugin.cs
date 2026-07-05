using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace TRL_ImmersiveScopes
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log;

        private void Awake()
        {
            // Inicializando o Logger para podermos mandar mensagens pro console do BepInEx
            Log = Logger;
            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded! Iniciando mod Fisheye Scopes.");

            // É aqui que vamos iniciar o Harmony para injetar nosso código nas câmeras do Tarkov
            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            
            // Exemplo de como chamaremos o Patch assim que você tiver o AssetBundle:
            // harmony.PatchAll(typeof(OpticCameraPatch));
        }
    }

    public static class PluginInfo
    {
        public const string PLUGIN_GUID = "com.trl.immersivescopes";
        public const string PLUGIN_NAME = "TRL-ImmersiveScopes";
        public const string PLUGIN_VERSION = "1.0.0";
    }
}
