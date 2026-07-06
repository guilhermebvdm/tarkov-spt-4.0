using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;

namespace TRL_Immersive_Overlays
{
    [BepInPlugin("com.trl.immersiveoverlays", "TRL-Immersive-Overlays", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource LogSource;
        public static ConfigEntry<bool> EnableMod;

        private void Awake()
        {
            LogSource = Logger;
            
            EnableMod = Config.Bind("Geral", "Ativar Mod", true, "Ativa ou desativa a sobreposição visual dos óculos.");

            Logger.LogInfo("TRL-Immersive-Overlays carregado. Registrando componentes...");

            // Anexa o OverlayController ao próprio GameObject do plugin, 
            // que possui DontDestroyOnLoad por padrão no BepInEx.
            gameObject.AddComponent<OverlayController>();
        }
    }
}
