using BepInEx;
using BepInEx.Logging;
using TRLCoreSight.Configuration;
using TRLCoreSight.Patches;

namespace TRLCoreSight
{
    [BepInPlugin("com.trl.coresight", "TRL-CoreSight", "0.4.19")]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGuid = "com.trl.coresight";
        public const string ModName = "TRL-CoreSight";
        public const string ModVersion = "0.4.19";

        public static Plugin Instance { get; private set; }
        public static ManualLogSource LogSource { get; private set; }

        private void Awake()
        {
            Instance = this;
            LogSource = Logger;

            ModConfig.Init(Config);

            // Inicialização da detecção de topologia de CPU (leitura passiva de hardware)
            Core.CpuTopologyManager.InitializeTopology();

            // Registro de patches de ciclo de vida e renderização
            new RaidStartPatch().Enable();
            new GameWorldOnDestroyPatch().Enable();

            // Otimização de física de cartuchos (Item 006)
            new ShellSpawnCullingPatch().Enable();
            new ShellSpawnAllCullingPatch().Enable();
            new ShellSpawnJamCullingPatch().Enable();

            // Otimização de loose loot (Item 002)
            new LootRegisterPatch().Enable();

            // Anti-Stutter de GC em combates (Item 004)
            new WeaponShotGCPatch().Enable();

            // Camuflagem Zonal & Percepção de IA (Item 008)
            new BotVisionSpeedPatch().Enable();
            new BotAimOffsetPatch().Enable();

            LogSource.LogInfo($"[{ModName}] v{ModVersion} carregado com sucesso. Otimizações prontas para raid.");
        }
    }
}
