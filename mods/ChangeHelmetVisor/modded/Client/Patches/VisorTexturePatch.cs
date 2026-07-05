using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;
using static VisorEffect;

namespace ChangeHelmetVisor.Patches
{
    internal class VisorTexturePatch() : ModulePatch
    {
        private static readonly string ImageFolderPath = Path.Combine(BepInEx.Paths.PluginPath, "ChangeHelmetVisorClient", "textures");
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(VisorEffect), nameof(VisorEffect.SetMask));
        }

        [PatchPostfix]
        public static void Postfix(
            ref Dictionary<EMask, Texture> ___dictionary_0,
            VisorEffect __instance
            )
        {
            try
            {
                float zoomWide = ChangeHelmetVisor.zoomScaleWide.Value;
                float zoomNarrow = ChangeHelmetVisor.zoomScaleNarrow.Value;

                Material material = __instance.method_4();

                if (Directory.Exists(ImageFolderPath))
                {
                    var ImagePathNarrow = Path.Combine(ImageFolderPath, "VisorMask_maska_sch.png");
                    var ImagePathWide = Path.Combine(ImageFolderPath, "VisorMask.png");

                    var narrowTexture = LoadTexture(ImagePathNarrow);
                    var wideTexture = LoadTexture(ImagePathWide);

                    if (___dictionary_0.ContainsKey(EMask.Narrow))
                    {
                        Texture original = ___dictionary_0[EMask.Narrow];
                        Texture2D croppedTexture = CropTexture(narrowTexture, zoomNarrow);
                        ___dictionary_0[EMask.Narrow] = croppedTexture;
                        if (material.GetTexture("_Mask") == original)
                        {
                            material.SetTexture("_Mask", croppedTexture);
                        }
                    }

                    if (___dictionary_0.ContainsKey(EMask.Wide))
                    {
                        Texture original = ___dictionary_0[EMask.Wide];
                        Texture2D croppedTexture = CropTexture(wideTexture, zoomWide);
                        ___dictionary_0[EMask.Wide] = croppedTexture;
                        if (material.GetTexture("_Mask") == original)
                        {
                            material.SetTexture("_Mask", croppedTexture);
                        }
                    }

                    ChangeHelmetVisor.LogSource.LogDebug($"Change Helmet Visor: Successfully loaded visor textures");
                }
                else
                {
                    ChangeHelmetVisor.LogSource.LogError($"Change Helmet Visor: Could not find the directory {ImageFolderPath}");
                    return;
                }
            }
            catch (Exception ex)
            {
                ChangeHelmetVisor.LogSource.LogError($"ChangeVisorTexture: Error applying visor texture - {ex.Message}\n{ex.StackTrace}");
            }
        }

        public static Texture2D LoadTexture(string path)
        {
            if (!File.Exists(path)) return null;

            try
            {
                byte[] fileData = File.ReadAllBytes(path);
                Texture2D tex = new Texture2D(2, 2);
                if (ImageConversion.LoadImage(tex, fileData))
                {
                    tex.wrapMode = TextureWrapMode.Repeat;
                    tex.filterMode = FilterMode.Bilinear;
                    return tex;
                }
            }
            catch (Exception ex)
            {
                ChangeHelmetVisor.LogSource.LogError($"ChangeHelmetVisor: Could not load the texture at {path}. {ex.Message}");
            }
            return null;
        }

        public static Texture2D CropTexture(Texture2D source, float zoom)
        {
            int croppedWidth = Mathf.RoundToInt(source.width / zoom);
            int croppedHeight = Mathf.RoundToInt(source.height / zoom);

            int offsetX = (source.width - croppedWidth) / 2;
            int offsetY = (source.height - croppedHeight) / 2;

            Color[] pixels = source.GetPixels(offsetX, offsetY, croppedWidth, croppedHeight);
            Texture2D croppedTexture = new Texture2D(croppedWidth, croppedHeight, source.format, true);

            croppedTexture.SetPixels(pixels);
            croppedTexture.Apply();

            return croppedTexture;
        }
    }
}
