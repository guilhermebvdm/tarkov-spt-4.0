using BepInEx;
using BepInEx.Logging;

namespace TarkovRedLine.PvpMode
{
    /// <summary>
    /// TRL-PvpMode — regras de modo de jogo para o servidor Tarkov Red Line.
    /// Primeiro modo: vidas por raid com renascimento em outro ponto de spawn,
    /// construído sobre o estado "downed" do Fika.
    /// </summary>
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("com.fika.core", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.trl.pvpmode";
        public const string PluginName = "TRL-PvpMode";
        public const string PluginVersion = "0.1.0";

        public static Plugin Instance { get; private set; }
        public static ManualLogSource Log { get; private set; }

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            Log.LogInfo($"[{PluginName}] {PluginVersion} carregado.");
        }
    }
}
