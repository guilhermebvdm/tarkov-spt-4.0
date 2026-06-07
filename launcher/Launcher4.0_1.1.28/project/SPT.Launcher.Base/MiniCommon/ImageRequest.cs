/* ImageRequest.cs
 * License: NCSA Open Source License
 * 
 * Copyright: SPT
 * AUTHORS:
 * waffle.lord
 */

using SPT.Launcher.Controllers;
using SPT.Launcher.Helpers;
using System;
using System.Collections.Generic;
using System.IO;

namespace SPT.Launcher.MiniCommon
{

    public static class ImageRequest
    {
        public static string ImageCacheFolder = Path.Join(LauncherSettingsProvider.Instance.GamePath, "SPT", "SPT_Data", "Launcher", "Image_Cache");

        private static List<string> CachedRoutes = new List<string>();

        private static string LauncherRoute = "/files/launcher/";
        public static void CacheBackgroundImage() => CacheImage($"{LauncherRoute}bg.png", Path.Combine(ImageCacheFolder, "bg.png"));
        public static void CacheSideImage(string Side)
        {
            if (Side == null || string.IsNullOrWhiteSpace(Side) || Side.ToLower() == "unknown") return;

            string SideImagePath = Path.Combine(ImageCacheFolder, $"side_{Side.ToLower()}.png");

            CacheImage($"{LauncherRoute}side_{Side.ToLower()}.png", SideImagePath);
        }

        private static void CacheImage(string route, string filePath)
        {
            try
            {
                Directory.CreateDirectory(ImageCacheFolder);

                if (String.IsNullOrWhiteSpace(route) || CachedRoutes.Contains(route)) //Don't want to request the image if it was already cached this session.
                {
                    return;
                }

                using Stream s = new Request(null, LauncherSettingsProvider.Instance.Server.Url).Send(route, "GET", null, false);

                using MemoryStream ms = new MemoryStream();

                s.CopyTo(ms);

                if (ms.Length == 0) return;

                using FileStream fs = File.Create(filePath);
                ms.Seek(0, SeekOrigin.Begin);
                ms.CopyTo(fs);

                CachedRoutes.Add(route);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Exception(ex);
            }
        }
    }
}
