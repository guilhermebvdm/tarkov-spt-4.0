using System;
using System.Collections.Generic;
using UnityEngine;

namespace DiscordRaidMap.RaidMap
{
    internal sealed class MapDefinition
    {
        public string ImageFile { get; set; }
        public Vector2 Min { get; set; }
        public Vector2 Max { get; set; }
        public float Rotation { get; set; }
    }

    internal static class MapRegistry
    {
        private static readonly Dictionary<string, MapDefinition> Maps = new(StringComparer.OrdinalIgnoreCase)
        {
            ["bigmap"] = new() { ImageFile = "customs.png", Min = new Vector2(-372f, -306f), Max = new Vector2(698f, 235f), Rotation = 180f },
            ["factory4_day"] = new() { ImageFile = "factory.png", Min = new Vector2(-65f, -64.5f), Max = new Vector2(77.6f, 67.2f), Rotation = 90f },
            ["factory4_night"] = new() { ImageFile = "factory.png", Min = new Vector2(-65f, -64.5f), Max = new Vector2(77.6f, 67.2f), Rotation = 90f },
            ["Sandbox"] = new() { ImageFile = "groundzero.png", Min = new Vector2(-99f, -124f), Max = new Vector2(249f, 364f), Rotation = 180f },
            ["Sandbox_high"] = new() { ImageFile = "groundzero.png", Min = new Vector2(-99f, -124f), Max = new Vector2(249f, 364f), Rotation = 180f },
            ["Interchange"] = new() { ImageFile = "interchange.png", Min = new Vector2(-364f, -443f), Max = new Vector2(534f, 452f), Rotation = 180f },
            ["laboratory"] = new() { ImageFile = "labs.png", Min = new Vector2(-292f, -441f), Max = new Vector2(-96f, -223f), Rotation = 270f },
            ["Labyrinth"] = new() { ImageFile = "labyrinth.png", Min = new Vector2(-52.5f, -36.5f), Max = new Vector2(50.7f, 75.3f), Rotation = 90f },
            ["Lighthouse"] = new() { ImageFile = "lighthouse.png", Min = new Vector2(-545f, -998f), Max = new Vector2(512f, 721f), Rotation = 180f },
            ["RezervBase"] = new() { ImageFile = "reserve.png", Min = new Vector2(-303.5f, -275f), Max = new Vector2(292f, 271.5f), Rotation = 180f },
            ["Shoreline"] = new() { ImageFile = "shoreline.png", Min = new Vector2(-1060f, -415f), Max = new Vector2(508f, 622f), Rotation = 180f },
            ["TarkovStreets"] = new() { ImageFile = "streets.png", Min = new Vector2(-279f, -299f), Max = new Vector2(324f, 533f), Rotation = 180f },
            ["Woods"] = new() { ImageFile = "woods.png", Min = new Vector2(-756f, -915f), Max = new Vector2(647f, 443f), Rotation = 180f }
        };

        public static bool TryGet(string location, out MapDefinition map)
        {
            return Maps.TryGetValue(location ?? "", out map);
        }
    }
}
