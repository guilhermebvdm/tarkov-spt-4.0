using System;
using System.Collections.Generic;
using EFT.InventoryLogic;
using UnityEngine;

namespace Manimal.LoadAmmoAnim.CustomEFTData
{
    public struct OffsetData
    {
        public Vector3 MagPosition;
        public Quaternion MagRotation;
        public Vector3 BulletPosition;
        public Quaternion BulletRotation;

        public static readonly OffsetData Default = new OffsetData
        {
            MagPosition = MagOffsetRegistry.BaseMagPos,
            MagRotation = MagOffsetRegistry.BaseMagRot,
            BulletPosition = MagOffsetRegistry.BaseBulletPos,
            BulletRotation = MagOffsetRegistry.BaseBulletRot
        };
    }

    /// <summary>
    /// Registry of local translation and rotation offsets mapped by caliber families and magazine form-factor
    /// to ensure consistent alignment in first-person view across all weapons.
    /// </summary>
    public static class MagOffsetRegistry
    {
        // 5.56x45mm NATO Golden Baseline calibrated in-game by hand (0.0 F12 Marco Zero)
        public static readonly Vector3 BaseMagPos = new Vector3(0.003f, 0.091f, 0.030f);
        public static readonly Quaternion BaseMagRot = Quaternion.Euler(-5.0f, 0.0f, -7.0f);
        public static readonly Vector3 BaseBulletPos = new Vector3(-0.005f, 0.030f, 0.000f);
        public static readonly Quaternion BaseBulletRot = Quaternion.Euler(90.0f, 10.0f, 0.0f);

        // Baseline caliber-family offsets
        private static readonly Dictionary<string, OffsetData> _caliberOffsets =
            new Dictionary<string, OffsetData>(StringComparer.OrdinalIgnoreCase)
            {
                // 5.56x45mm NATO (Standard STANAG curve / straight - Golden Baseline)
                {
                    "Caliber556x45NATO",
                    new OffsetData
                    {
                        MagPosition = BaseMagPos,
                        MagRotation = BaseMagRot,
                        BulletPosition = BaseBulletPos,
                        BulletRotation = BaseBulletRot
                    }
                },
                // 7.62x39mm (AK curve - angled slightly forward)
                {
                    "Caliber762x39",
                    new OffsetData
                    {
                        MagPosition = BaseMagPos + new Vector3(0.002f, 0.002f, 0.005f),
                        MagRotation = BaseMagRot * Quaternion.Euler(3f, -2f, 0f),
                        BulletPosition = BaseBulletPos,
                        BulletRotation = BaseBulletRot
                    }
                },
                // 5.45x39mm (AK-74 curve)
                {
                    "Caliber545x39",
                    new OffsetData
                    {
                        MagPosition = BaseMagPos + new Vector3(0.001f, 0.001f, 0.002f),
                        MagRotation = BaseMagRot * Quaternion.Euler(1.5f, -1f, 0f),
                        BulletPosition = BaseBulletPos,
                        BulletRotation = BaseBulletRot
                    }
                },
                // 7.62x51mm NATO (.308 Win / FAL / SR-25 / M1A / SCAR-H / MDR 762 / HK417 / G28)
                {
                    "Caliber762x51",
                    new OffsetData
                    {
                        MagPosition = BaseMagPos + new Vector3(0.065f, 0.000f, -0.030f),
                        MagRotation = BaseMagRot * Quaternion.Euler(-10f, 0f, 0f),
                        BulletPosition = BaseBulletPos + new Vector3(0f, 0.020f, 0f),
                        BulletRotation = BaseBulletRot
                    }
                },
                // 7.62x54mmR (SVD / SV-98 rimmed curved mag)
                {
                    "Caliber762x54R",
                    new OffsetData
                    {
                        MagPosition = BaseMagPos + new Vector3(0f, -0.008f, 0.002f),
                        MagRotation = BaseMagRot * Quaternion.Euler(3.5f, 0f, 0f),
                        BulletPosition = BaseBulletPos,
                        BulletRotation = BaseBulletRot
                    }
                },
                // 9x19mm Parabellum (Glock / P226 / M9A3 / MP5 / MPX)
                {
                    "Caliber9x19Para",
                    new OffsetData
                    {
                        MagPosition = BaseMagPos + new Vector3(0.005f, 0.015f, -0.006f),
                        MagRotation = BaseMagRot * Quaternion.Euler(12f, 0f, 0f),
                        BulletPosition = BaseBulletPos + new Vector3(0f, 0.005f, 0f),
                        BulletRotation = BaseBulletRot * Quaternion.Euler(10f, 0f, 0f)
                    }
                },
                // .45 ACP (1911 / Vector 45 / UMP 45)
                {
                    "Caliber1143x23ACP",
                    new OffsetData
                    {
                        MagPosition = BaseMagPos + new Vector3(0.005f, 0.015f, -0.006f),
                        MagRotation = BaseMagRot * Quaternion.Euler(12f, 0f, 0f),
                        BulletPosition = BaseBulletPos + new Vector3(0f, 0.005f, 0f),
                        BulletRotation = BaseBulletRot * Quaternion.Euler(10f, 0f, 0f)
                    }
                },
                // 9x18mm PM (Makarov / PB / Kedr / Klin)
                {
                    "Caliber9x18PM",
                    new OffsetData
                    {
                        MagPosition = BaseMagPos + new Vector3(0.005f, 0.018f, -0.006f),
                        MagRotation = BaseMagRot * Quaternion.Euler(14f, 0f, 0f),
                        BulletPosition = BaseBulletPos + new Vector3(0f, 0.006f, 0f),
                        BulletRotation = BaseBulletRot * Quaternion.Euler(12f, 0f, 0f)
                    }
                },
                // 9x21mm (SR-1MP / Gyurza / Veresk)
                {
                    "Caliber9x21",
                    new OffsetData
                    {
                        MagPosition = BaseMagPos + new Vector3(0.005f, 0.015f, -0.006f),
                        MagRotation = BaseMagRot * Quaternion.Euler(12f, 0f, 0f),
                        BulletPosition = BaseBulletPos + new Vector3(0f, 0.005f, 0f),
                        BulletRotation = BaseBulletRot * Quaternion.Euler(10f, 0f, 0f)
                    }
                },
                // 7.62x25mm TT (TT-33 / PPSH-41 stick)
                {
                    "Caliber762x25TT",
                    new OffsetData
                    {
                        MagPosition = BaseMagPos + new Vector3(0.005f, 0.015f, -0.006f),
                        MagRotation = BaseMagRot * Quaternion.Euler(12f, 0f, 0f),
                        BulletPosition = BaseBulletPos + new Vector3(0f, 0.005f, 0f),
                        BulletRotation = BaseBulletRot * Quaternion.Euler(10f, 0f, 0f)
                    }
                },
                // 5.7x28mm (FN Five-seveN)
                {
                    "Caliber57x28",
                    new OffsetData
                    {
                        MagPosition = BaseMagPos + new Vector3(0.003f, 0.012f, -0.004f),
                        MagRotation = BaseMagRot * Quaternion.Euler(10f, 0f, 0f),
                        BulletPosition = BaseBulletPos + new Vector3(0f, 0.004f, 0f),
                        BulletRotation = BaseBulletRot * Quaternion.Euler(8f, 0f, 0f)
                    }
                },
                // 4.6x30mm (MP7)
                {
                    "Caliber46x30",
                    new OffsetData
                    {
                        MagPosition = BaseMagPos + new Vector3(0.004f, 0.012f, -0.004f),
                        MagRotation = BaseMagRot * Quaternion.Euler(10f, 0f, 0f),
                        BulletPosition = BaseBulletPos + new Vector3(0f, 0.004f, 0f),
                        BulletRotation = BaseBulletRot * Quaternion.Euler(8f, 0f, 0f)
                    }
                },
                // 9x39mm (VSS Vintorez / AS VAL)
                {
                    "Caliber9x39",
                    new OffsetData
                    {
                        MagPosition = BaseMagPos + new Vector3(0.001f, -0.004f, 0.002f),
                        MagRotation = BaseMagRot * Quaternion.Euler(2f, 0f, 0f),
                        BulletPosition = BaseBulletPos,
                        BulletRotation = BaseBulletRot
                    }
                },
                // .366 TKM (VPO-209 / VPO-215)
                {
                    "Caliber366TKM",
                    new OffsetData
                    {
                        MagPosition = BaseMagPos + new Vector3(0.002f, -0.005f, 0.004f),
                        MagRotation = BaseMagRot * Quaternion.Euler(3f, -2f, 0f),
                        BulletPosition = BaseBulletPos,
                        BulletRotation = BaseBulletRot
                    }
                },
                // 12/70 Shotgun (Saiga-12 / MP-155 detach box)
                {
                    "Caliber12g",
                    new OffsetData
                    {
                        MagPosition = BaseMagPos + new Vector3(0f, -0.020f, 0.010f),
                        MagRotation = BaseMagRot * Quaternion.Euler(2f, 0f, 0f),
                        BulletPosition = BaseBulletPos + new Vector3(0f, -0.005f, 0f),
                        BulletRotation = BaseBulletRot
                    }
                },
                // 20/70 Shotgun (TOZ-106)
                {
                    "Caliber20g",
                    new OffsetData
                    {
                        MagPosition = BaseMagPos + new Vector3(0f, -0.010f, 0.005f),
                        MagRotation = BaseMagRot * Quaternion.Euler(2f, 0f, 0f),
                        BulletPosition = BaseBulletPos + new Vector3(0f, -0.003f, 0f),
                        BulletRotation = BaseBulletRot
                    }
                },
                // .338 Lapua Magnum (AXMC / Mk-18)
                {
                    "Caliber86x70",
                    new OffsetData
                    {
                        MagPosition = BaseMagPos + new Vector3(0f, -0.015f, 0f),
                        MagRotation = BaseMagRot,
                        BulletPosition = BaseBulletPos + new Vector3(0f, -0.003f, 0f),
                        BulletRotation = BaseBulletRot
                    }
                },
                // 6.8x51mm (.277 Fury / Spear - AR-10 magazine profile)
                {
                    "Caliber68x51",
                    new OffsetData
                    {
                        MagPosition = BaseMagPos + new Vector3(0.065f, 0.000f, -0.030f),
                        MagRotation = BaseMagRot * Quaternion.Euler(-10f, 0f, 0f),
                        BulletPosition = BaseBulletPos + new Vector3(0f, 0.020f, 0f),
                        BulletRotation = BaseBulletRot
                    }
                },
                // .50 AE (Desert Eagle)
                {
                    "Caliber50AE",
                    new OffsetData
                    {
                        MagPosition = BaseMagPos + new Vector3(0.005f, 0.015f, -0.006f),
                        MagRotation = BaseMagRot * Quaternion.Euler(14f, 0f, 0f),
                        BulletPosition = BaseBulletPos + new Vector3(0f, 0.008f, 0f),
                        BulletRotation = BaseBulletRot * Quaternion.Euler(12f, 0f, 0f)
                    }
                }
            };

        /// <summary>
        /// Retrieves the calibrated offset data for a given caliber and magazine form factor.
        /// Lookup priority:
        ///   1. <see cref="OffsetFileStore"/> by <paramref name="templateId"/> (absolute, user-calibrated)
        ///   2. Hardcoded caliber-family offsets (relative to Golden Baseline)
        ///   3. <see cref="OffsetData.Default"/> (5.56x45mm Golden Baseline)
        /// </summary>
        public static OffsetData GetOffset(string caliber, string templateId = null, MagazineItemClass mag = null)
        {
            // Priority 1 — per-magazine file-store entry (absolute offset, highest priority).
            if (!string.IsNullOrEmpty(templateId) && OffsetFileStore.TryGet(templateId, out var fileStoreOffset))
                return fileStoreOffset;

            OffsetData baseOffset = OffsetData.Default;

            if (!string.IsNullOrEmpty(caliber) && _caliberOffsets.TryGetValue(caliber, out var specific))
            {
                baseOffset = specific;
            }

            // Handle special form factors
            if (mag != null)
            {
                // 1. Drum Magazine Detection (2x2 grid size or drum name)
                bool isDrum = (mag.Width >= 2 && mag.Height >= 2)
                    || (mag.Template != null && mag.Template.Name.IndexOf("drum", StringComparison.OrdinalIgnoreCase) >= 0);

                if (isDrum)
                {
                    // Shift the hand slightly outward to wrap around the drum body
                    baseOffset.MagPosition += new Vector3(-0.025f, -0.010f, 0.015f);
                    baseOffset.MagRotation *= Quaternion.Euler(0f, -5f, 0f);
                }

                // 2. SMG stick magazines in 9x19 or .45 (e.g. MP5, MPX, Vector) vs Pistol in-grip magazines
                bool isSmgStick = mag.Height >= 2 && (mag.Template != null &&
                    (mag.Template.Name.IndexOf("mp5", StringComparison.OrdinalIgnoreCase) >= 0
                    || mag.Template.Name.IndexOf("mpx", StringComparison.OrdinalIgnoreCase) >= 0
                    || mag.Template.Name.IndexOf("vector", StringComparison.OrdinalIgnoreCase) >= 0
                    || mag.Template.Name.IndexOf("pp19", StringComparison.OrdinalIgnoreCase) >= 0));

                if (isSmgStick)
                {
                    // Straighten SMG stick magazines compared to tilted pistol grips
                    baseOffset.MagPosition = BaseMagPos + new Vector3(0f, -0.005f, 0f);
                    baseOffset.MagRotation = BaseMagRot * Quaternion.Euler(2f, 0f, 0f);
                    baseOffset.BulletPosition = BaseBulletPos;
                    baseOffset.BulletRotation = BaseBulletRot;
                }

                // 3. FN P90 horizontal top magazine
                if (mag.Template != null && mag.Template.Name.IndexOf("p90", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    baseOffset.MagPosition = BaseMagPos + new Vector3(-0.020f, 0.030f, -0.010f);
                    baseOffset.MagRotation = BaseMagRot * Quaternion.Euler(0f, 90f, 0f);
                    baseOffset.BulletPosition = BaseBulletPos;
                    baseOffset.BulletRotation = BaseBulletRot;
                }
            }

            return baseOffset;
        }
    }
}
