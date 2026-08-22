using EFT;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace tarkin.ladders.bep
{
    public class LaddersLoader : IDisposable
    {
        private readonly string pathDirSceneBundles;

        AssetBundle sceneBundle;
        Scene scene; 
        
        private static readonly Dictionary<string, string> LocationIdBundleMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "factory4_day", "Factory_Rework_Ladders" },
            { "factory4_night", "Factory_Rework_Ladders" },

            { "Sandbox", "Sandbox_Ladders" },
            { "Sandbox_high", "Sandbox_Ladders" },

            { "TarkovStreets", "City_Ladders" },
            { "bigmap", "Custom_Ladders" },
            { "RezervBase", "Reserve_Base_Ladders" },

            { "Woods", "Woods_Ladders" },
            { "Shoreline", "Shoreline_Ladders" },
            { "Interchange", "Interchange_Ladders" },
            { "Lighthouse", "Lighthouse_Ladders" }
        };

        private string GetBundleFileName(string locationId)
        {
            if (LocationIdBundleMapping.TryGetValue(locationId, out string bundleName))
            {
                return bundleName.ToLowerInvariant();
            }

            return locationId.ToLowerInvariant();
        }

        public LaddersLoader(string pathDirSceneBundles) 
        {
            this.pathDirSceneBundles = pathDirSceneBundles;
        }

        public void Load(GameWorld gameWorld)
        {
            string bundleFileName = GetBundleFileName(gameWorld.LocationId);
            string bundlePath = Path.Combine(pathDirSceneBundles, bundleFileName);
            if (!File.Exists(bundlePath))
            {
                Plugin.Logger.LogWarning($"ladders scene bundle not found at: {bundlePath}");
                return;
            }

            sceneBundle = AssetBundle.LoadFromFile(bundlePath);
            scene = SceneManager.LoadScene(sceneBundle.GetAllScenePaths()[0],
                new LoadSceneParameters() { loadSceneMode = LoadSceneMode.Additive, localPhysicsMode = LocalPhysicsMode.None });

            Plugin.Logger.LogInfo($"Successfully loaded ladders scene {scene.name}");
        }

        public void Unload()
        {
            if (scene.isLoaded)
            {
#if DEBUG
                SceneManager.UnloadScene(scene); // unsafe apparently (unity docs say so), but required for hot reload
#elif RELEASE
                SceneManager.UnloadSceneAsync(scene);
#endif
            }
            if (sceneBundle != null)
                sceneBundle.Unload(false);
        }

        public void Dispose()
        {
            Unload();
        }
    }
}
