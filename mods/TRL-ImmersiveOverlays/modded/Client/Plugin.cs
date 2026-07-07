using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;

namespace TRL_ImmersiveOverlays
{
    [BepInPlugin("com.trl.immersiveoverlays", "TRL-ImmersiveOverlays", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource LogSource;
        public static ConfigEntry<bool> EnableMod;

        private void Awake()
        {
            LogSource = Logger;
            
            EnableMod = Config.Bind("Geral", "Ativar Mod", true, "Ativa ou desativa a sobreposição visual dos óculos.");

            Logger.LogInfo("TRL-ImmersiveOverlays carregado. Registrando componentes...");

            // Anexa o OverlayController ao próprio GameObject do plugin, 
            // que possui DontDestroyOnLoad por padrão no BepInEx.
            gameObject.AddComponent<OverlayController>();
        }
    }
}
