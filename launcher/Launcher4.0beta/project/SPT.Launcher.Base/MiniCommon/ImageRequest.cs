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

        /// <summary>
        /// Caches an image served by the CONNECTED backend (RequestHandler endpoint) under
        /// Image_Cache/<paramref name="fileName"/> and returns the local path, or null on failure.
        /// Used for CustomClasses class icons (item 004). Static files come raw (no zlib).
        /// </summary>
        public static string CacheServerImage(string route, string fileName)
        {
            if (string.IsNullOrWhiteSpace(route) || string.IsNullOrWhiteSpace(fileName)) return null;

            try
            {
                Directory.CreateDirectory(ImageCacheFolder);

                string filePath = Path.Combine(ImageCacheFolder, fileName);

                if (CachedRoutes.Contains(route) && File.Exists(filePath))
                {
                    return filePath;
                }

                using Stream s = new Request(null, RequestHandler.GetBackendUrl()).Send(route, "GET", null, false);

                if (s == null) return null;

                using MemoryStream ms = new MemoryStream();

                s.CopyTo(ms);

                if (ms.Length == 0) return null;

                using (FileStream fs = File.Create(filePath))
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.CopyTo(fs);
                }

                CachedRoutes.Add(route);
                return filePath;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Exception(ex);
                return null;
            }
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
