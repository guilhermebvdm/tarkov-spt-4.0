using BepInEx;

namespace ModoPVP
{
    [BepInPlugin("com.umbigopreto.modopvp", "Modo PVP", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; }

        private void Awake()
        {
            Instance = this;

            // Inicia e carrega os novos interceptadores
            new Patches.PlayerDeadPatch().Enable();
            new Patches.DisableF8ExtractPatch().Enable();
            new Patches.DisableSpectatorPatch().Enable();

            Logger.LogInfo("Modo PVP (Respawn Mod) por Umbigo Preto foi carregado com sucesso!");
            
            Logger.LogInfo("Patches aplicados.");
        }
    }
}
