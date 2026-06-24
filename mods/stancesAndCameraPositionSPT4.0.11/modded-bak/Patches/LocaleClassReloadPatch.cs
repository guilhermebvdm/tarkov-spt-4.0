using System.Reflection;
using System.Threading.Tasks;
using SPT.Reflection.Patching;
using Comfort.Common;

namespace CameraRotationMod.Patches
{
    public class LocaleClassReloadPatch : ModulePatch
    {
        private static bool _hasBeenSet = false;

        protected override MethodBase GetTargetMethod()
        {
            return typeof(LocaleClass).GetMethod(nameof(LocaleClass.ReloadBackendLocale));
        }

        [PatchPostfix]
        public static void Postfix(Task __result)
        {
            if (!_hasBeenSet)
            {
                _ = Task.Run(async () => 
                {
                    await __result;
                    
                    // Delay for a short time to let EFT finish loading the locale strings completely
                    await Task.Delay(1000);
                    
                    // Fetch the current language from EFT SharedGameSettings
                    if (Singleton<SharedGameSettingsClass>.Instance != null && 
                        Singleton<SharedGameSettingsClass>.Instance.Game?.Settings?.Language?.Value != null)
                    {
                        LocaleUtils.SetLanguageFromEFTLocale(Singleton<SharedGameSettingsClass>.Instance.Game.Settings.Language.Value);
                    }
                    
                    // Update BepInEx DispName dynamically
                    TranslationUpdater.Update(Plugin.Instance.Config);
                    
                    Plugin.Logger.LogInfo($"F12 Menu Translated to: {LocaleUtils.CurrentLanguage}");
                });
                
                _hasBeenSet = true;
            }
        }
    }
}
