using System.Collections.Generic;

namespace TRLDynamicSpawnServer.Models
{
    public static class BotZones
    {
        // Zone lists extracted from ABPS for the dropdowns
        
        public static readonly List<string> CustomsZones = new()
        {
            "ZoneBrige", "ZoneCrossRoad", "ZoneDormitory", "ZoneGasStation",
            "ZoneFactoryCenter", "ZoneFactorySide", "ZoneOldAZS", "ZoneBlockPost",
            "ZoneTankSquare", "ZoneWade", "ZoneCustoms", "ZoneScavBase"
        };

        public static readonly List<string> FactoryZones = new()
        {
            "BotZone"
        };

        public static readonly List<string> InterchangeZones = new()
        {
            "ZoneCenter", "ZoneCenterBot", "ZoneGoshan", "ZoneIdea", 
            "ZoneOli", "ZonePowerStation", "ZoneRamp", "ZoneRoad", "ZoneIDEAMall"
        };

        public static readonly List<string> LaboratoryZones = new()
        {
            "BotZoneFloor1", "BotZoneFloor2", "BotZoneBasement"
        };

        public static readonly List<string> LighthouseZones = new()
        {
            "Zone_TreatmentContainers", "Zone_TreatmentBeach", "Zone_TreatmentRocks", "Zone_Helicopter",
            "Zone_Bunker", "Zone_Chalet", "Zone_Village", "Zone_LongRoad", "Zone_SniperPeak", "Zone_RuinedHouse",
            "Zone_Island", "Zone_Blockpost"
        };

        public static readonly List<string> ReserveZones = new()
        {
            "ZoneSubCommand", "ZoneSubStation", "ZoneBunker", "ZonePTOR1", "ZonePTOR2", 
            "ZoneBarracks", "ZoneRailBunker", "ZoneGluharBase" // Note: Some are custom names used by SPT
        };

        public static readonly List<string> ShorelineZones = new()
        {
            "ZoneSanatorium1", "ZoneSanatorium2", "ZonePassFar", "ZonePassClose", 
            "ZoneTunnel", "ZonePowerStation", "ZoneGasStation", "ZonePort", "ZoneVillage", "ZoneBunker"
        };

        public static readonly List<string> StreetsZones = new()
        {
            "ZoneMVD", "ZoneHotel", "ZoneCardi", "ZoneFactory", "ZoneConcordia_1", "ZoneConcordia_2", 
            "ZonePinewood", "ZoneCinema", "ZoneSniper1", "ZoneSniper2", "ZoneSniper3"
        };

        public static readonly List<string> WoodsZones = new()
        {
            "ZoneRedHouse", "ZoneWoodCutter", "ZoneHouse", "ZoneBigCutter", "ZoneMiniHouse", "ZoneClear", 
            "ZoneScavBase", "ZoneVillage", "ZoneBrokenVill", "ZoneHighRocks"
        };
        
        public static readonly List<string> GroundZeroZones = new()
        {
            "ZoneSandbox"
        };
    }
}
