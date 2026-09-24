using System.Collections.Generic;

namespace TRLImmersiveCombatMedicine.Helpers
{
    public class ItemStats
    {
        public string Name;
        public float MaxHP;
        public float HealAmount; // SE FOR 0 = ITEM DE USO (Gasta 1.0)
        public float UseTime;

        // Custos (Apenas para MedKits)
        public float LightBleedCost;
        public float HeavyBleedCost;
        public float FractureCost;

        // Capacidades
        public bool StopsLightBleed;
        public bool StopsHeavyBleed;
        public bool FixesFracture;
        public bool RemovesPain;
        public bool IsSurgery;
        public float SurgeryPenaltyMin;
        public float SurgeryPenaltyMax;
        public bool StopsAllBleeds; // Zagustin
        public bool IsTourniquet;    // Torniquete realista
    }

    public static class ItemDatabase
    {
        private static Dictionary<string, ItemStats> _database = new Dictionary<string, ItemStats>();

        public static void Initialize()
        {
            // === MEDKITS (HealAmount > 0) ===
            // AI-2 (Cheese) — MaxHP:100, HealRate:50, UseTime:2s, RadExposure(cost:0)
            _database["5755356824597772cb798962"] = new ItemStats { Name = "AI-2", MaxHP = 100, HealAmount = 50, UseTime = 2f };
            // Car — MaxHP:220, HealRate:70, UseTime:3s, LightBleed(cost:50)
            _database["590c661e86f7741e566b646a"] = new ItemStats { Name = "Car", MaxHP = 220, HealAmount = 70, UseTime = 3f, StopsLightBleed = true, LightBleedCost = 50f };
            // Salewa — MaxHP:400, HealRate:85, UseTime:3s, LightBleed(cost:45), HeavyBleed(cost:175)
            _database["544fb45d4bdc2dee738b4568"] = new ItemStats { Name = "Salewa", MaxHP = 400, HealAmount = 85, UseTime = 3f, StopsLightBleed = true, LightBleedCost = 45f, StopsHeavyBleed = true, HeavyBleedCost = 175f };
            // IFAK — MaxHP:300, HealRate:50, UseTime:3s, LightBleed(cost:30), HeavyBleed(cost:210), RadExposure(cost:0)
            _database["590c678286f77426c9660122"] = new ItemStats { Name = "IFAK", MaxHP = 300, HealAmount = 50, UseTime = 3f, StopsLightBleed = true, LightBleedCost = 30f, StopsHeavyBleed = true, HeavyBleedCost = 210f };
            // AFAK — MaxHP:400, HealRate:60, UseTime:3s, LightBleed(cost:30), HeavyBleed(cost:170), RadExposure(cost:0)
            _database["60098ad7c2240c0fe85c570a"] = new ItemStats { Name = "AFAK", MaxHP = 400, HealAmount = 60, UseTime = 3f, StopsLightBleed = true, LightBleedCost = 30f, StopsHeavyBleed = true, HeavyBleedCost = 170f };
            // Grizzly — MaxHP:1800, HealRate:175, UseTime:5s, LightBleed(cost:40), HeavyBleed(cost:130), Fracture(cost:50), Contusion(cost:0), RadExposure(cost:0)
            _database["590c657e86f77412b013051d"] = new ItemStats { Name = "Grizzly", MaxHP = 1800, HealAmount = 175, UseTime = 5f, StopsLightBleed = true, LightBleedCost = 40f, StopsHeavyBleed = true, HeavyBleedCost = 130f, FixesFracture = true, FractureCost = 50f };

            // === ITENS DE USO (HealAmount = 0) ===

            // Bandagens (Light Bleed)
            _database["544fb25a4bdc2dfb738b4567"] = new ItemStats { Name = "Bandage", HealAmount = 0, UseTime = 2f, StopsLightBleed = true };
            _database["5751a25924597722c463c472"] = new ItemStats { Name = "Army Bandage", MaxHP = 2, HealAmount = 0, UseTime = 2f, StopsLightBleed = true };

            // Torniquetes & Hemostáticos (Heavy Bleed)
            _database["5e831507ea0a7c419c2f9bd9"] = new ItemStats { Name = "Esmarch", HealAmount = 0, UseTime = 5f, StopsHeavyBleed = true, IsTourniquet = true };
            _database["60098af40accd37ef2175f27"] = new ItemStats { Name = "CAT", HealAmount = 0, UseTime = 3f, StopsHeavyBleed = true, IsTourniquet = true };
            _database["5e8488fa988a8701445df1e4"] = new ItemStats { Name = "CALOK-B", MaxHP = 3, HealAmount = 0, UseTime = 3f, StopsHeavyBleed = true };

            // Talas (Fracture)
            _database["544fb3364bdc2d34748b456a"] = new ItemStats { Name = "Splint", HealAmount = 0, UseTime = 5f, FixesFracture = true };
            _database["5af0454c86f7746bf20992e8"] = new ItemStats { Name = "Alu Splint", MaxHP = 5, HealAmount = 0, UseTime = 3f, FixesFracture = true };

            // Cirurgia — healthPenaltyMin/Max são percentuais de HP máximo que serão mantidos
            _database["5d02778e86f774203e7dedbe"] = new ItemStats { Name = "CMS", IsSurgery = true, UseTime = 16f, SurgeryPenaltyMin = 0.25f, SurgeryPenaltyMax = 0.45f };
            _database["5d02797c86f774203f38e30a"] = new ItemStats { Name = "Surv12", IsSurgery = true, UseTime = 20f, SurgeryPenaltyMin = 0.60f, SurgeryPenaltyMax = 0.72f, FixesFracture = true };

            // Injetores
            _database["5c0e530286f7747fa1419862"] = new ItemStats { Name = "Propital", UseTime = 2f, RemovesPain = true, HealAmount = 0 };
            _database["5c0e533786f7747fa23f4d47"] = new ItemStats { Name = "Zagustin", UseTime = 2f, StopsAllBleeds = true, HealAmount = 0 };
        }

        public static ItemStats GetStats(string templateId)
        {
            if (_database.TryGetValue(templateId, out var stats)) return stats;
            // Fallback Genérico
            return new ItemStats { Name = "Unknown", HealAmount = 50f, UseTime = 4f };
        }
    }
}
