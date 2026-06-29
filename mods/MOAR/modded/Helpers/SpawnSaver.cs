using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using MOARServer.Globals;
using MOARServer.Models;

namespace MOARServer.Helpers;

public static class SpawnSaver
{
    private static string GetConfigPath(string type)
    {
        var modPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
        var fileName = type.ToLower() switch
        {
            "pmc" => "pmcSpawns.json",
            "scav" => "scavSpawns.json",
            "player" => "playerSpawns.json",
            "sniper" => "sniperSpawns.json",
            _ => "pmcSpawns.json"
        };
        return Path.Combine(modPath, "config", "Spawns", fileName);
    }

    private static SpawnLocations GetGlobalDict(string type)
    {
        return type.ToLower() switch
        {
            "pmc" => ModConfig.PmcSpawns,
            "scav" => ModConfig.ScavSpawns,
            "player" => ModConfig.PlayerSpawns,
            "sniper" => ModConfig.SniperSpawns,
            _ => ModConfig.PmcSpawns
        };
    }

    public static void SaveSpawn(string type, double x, double y, double z, string map)
    {
        var dict = GetGlobalDict(type);
        if (!dict.ContainsKey(map))
        {
            dict[map] = new List<SpawnPosition>();
        }
        
        dict[map].Add(new SpawnPosition { X = x, Y = y, Z = z });
        
        var json = JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(GetConfigPath(type), json);
        
        Console.WriteLine($"[MOAR] Saved new {type} spawn to {map} at ({x}, {y}, {z})");
    }
    
    public static void DeleteSpawn(string type, double x, double y, double z, string map)
    {
        var dict = GetGlobalDict(type);
        if (dict.TryGetValue(map, out var spawns))
        {
            // Find closest spawn
            double minDist = double.MaxValue;
            SpawnPosition closest = null!;
            
            foreach (var spawn in spawns)
            {
                double dist = Math.Sqrt(Math.Pow(spawn.X - x, 2) + Math.Pow(spawn.Y - y, 2) + Math.Pow(spawn.Z - z, 2));
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = spawn;
                }
            }
            
            // Delete if within 10 meters
            if (closest != null && minDist < 10.0)
            {
                spawns.Remove(closest);
                var json = JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(GetConfigPath(type), json);
                
                Console.WriteLine($"[MOAR] Deleted {type} spawn from {map} at ({closest.X}, {closest.Y}, {closest.Z})");
            }
        }
    }
}
